namespace Peony.Core;

using System.Text;
using System.Text.Json;

/// <summary>
/// Interface for text extraction from ROMs
/// </summary>
public interface ITextExtractor {
	/// <summary>
	/// Extract text from ROM using a table file
	/// </summary>
	string ExtractText(ReadOnlySpan<byte> data, int offset, int maxLength, TableFile table);

	/// <summary>
	/// Extract all text blocks from ROM
	/// </summary>
	List<TextBlock> ExtractAllText(ReadOnlySpan<byte> data, TableFile table, TextExtractionOptions options);
}

/// <summary>
/// A block of extracted text
/// </summary>
public record TextBlock {
	/// <summary>ROM offset where text starts</summary>
	public required int Offset { get; init; }

	/// <summary>Length in bytes</summary>
	public required int Length { get; init; }

	/// <summary>Decoded text content</summary>
	public required string Text { get; init; }

	/// <summary>Raw bytes</summary>
	public required byte[] RawBytes { get; init; }

	/// <summary>Optional label/identifier</summary>
	public string? Label { get; init; }

	/// <summary>Category (dialog, menu, item name, etc.)</summary>
	public string? Category { get; init; }
}

/// <summary>
/// Options for text extraction
/// </summary>
public record TextExtractionOptions {
	/// <summary>Start offset in ROM</summary>
	public int StartOffset { get; init; }

	/// <summary>End offset in ROM (exclusive)</summary>
	public int EndOffset { get; init; }

	/// <summary>Minimum text length to extract</summary>
	public int MinLength { get; init; } = 3;

	/// <summary>Maximum text length</summary>
	public int MaxLength { get; init; } = 1000;

	/// <summary>Include control codes in output</summary>
	public bool IncludeControlCodes { get; init; } = true;

	/// <summary>Stop at null terminator (0x00)</summary>
	public bool NullTerminated { get; init; } = true;

	/// <summary>Stop at end byte if specified</summary>
	public byte? EndByte { get; init; }

	/// <summary>Category label for extracted text</summary>
	public string? Category { get; init; }
}

/// <summary>
/// Table file (.tbl) for character encoding
/// </summary>
public class TableFile {
	private readonly Dictionary<byte, string> _byteToChar = [];
	private readonly Dictionary<string, byte> _charToByte = [];
	private readonly Dictionary<ushort, string> _wordToChar = [];
	private readonly Dictionary<string, ushort> _charToWord = [];
	private readonly Dictionary<byte, string> _controlCodes = [];

	/// <summary>Table name/description</summary>
	public string Name { get; set; } = "Unnamed";

	/// <summary>Whether table uses 16-bit entries</summary>
	public bool HasWordEntries => _wordToChar.Count > 0;

	/// <summary>End-of-string byte value</summary>
	public byte? EndByte { get; set; }

	/// <summary>
	/// Load a table file from TBL format
	/// </summary>
	public static TableFile LoadFromTbl(string content) {
		var table = new TableFile();
		var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

		foreach (var line in lines) {
			var trimmed = line.Trim();

			// Skip comments and empty lines
			if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#')) {
				continue;
			}

			// Handle special directives
			if (trimmed.StartsWith('@')) {
				ParseDirective(table, trimmed);
				continue;
			}

			// Parse hex=char format
			var eqIndex = trimmed.IndexOf('=');
			if (eqIndex < 2) continue;

			var hexPart = trimmed[..eqIndex];
			var charPart = trimmed[(eqIndex + 1)..];

			// Handle escape sequences
			charPart = UnescapeString(charPart);

			if (hexPart.Length == 2) {
				// Single byte entry
				if (byte.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out byte b)) {
					table._byteToChar[b] = charPart;
					if (!table._charToByte.ContainsKey(charPart)) {
						table._charToByte[charPart] = b;
					}
				}
			} else if (hexPart.Length == 4) {
				// Two-byte entry
				if (ushort.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ushort w)) {
					table._wordToChar[w] = charPart;
					if (!table._charToWord.ContainsKey(charPart)) {
						table._charToWord[charPart] = w;
					}
				}
			}
		}

		return table;
	}

	/// <summary>
	/// Load a table file from a file path
	/// </summary>
	public static TableFile LoadFromFile(string path) {
		var content = File.ReadAllText(path);
		var table = LoadFromTbl(content);
		table.Name = Path.GetFileNameWithoutExtension(path);
		return table;
	}

	/// <summary>
	/// Create a simple ASCII table (0x20-0x7E)
	/// </summary>
	public static TableFile CreateAsciiTable() {
		var table = new TableFile { Name = "ASCII" };
		for (int i = 0x20; i <= 0x7e; i++) {
			table._byteToChar[(byte)i] = ((char)i).ToString();
			table._charToByte[((char)i).ToString()] = (byte)i;
		}
		return table;
	}

	/// <summary>
	/// Decode a single byte to its character representation
	/// </summary>
	public string? DecodeByte(byte b) {
		return _byteToChar.TryGetValue(b, out var c) ? c : null;
	}

	/// <summary>
	/// Decode a word (2 bytes) to its character representation
	/// </summary>
	public string? DecodeWord(ushort w) {
		return _wordToChar.TryGetValue(w, out var c) ? c : null;
	}

	/// <summary>
	/// Encode a character to its byte value
	/// </summary>
	public byte? EncodeByte(string c) {
		return _charToByte.TryGetValue(c, out var b) ? b : null;
	}

	/// <summary>
	/// Encode a character to its word value
	/// </summary>
	public ushort? EncodeWord(string c) {
		return _charToWord.TryGetValue(c, out var w) ? w : null;
	}

	/// <summary>
	/// Check if a byte is a control code
	/// </summary>
	public bool IsControlCode(byte b) {
		return _controlCodes.ContainsKey(b);
	}

	/// <summary>
	/// Get control code description
	/// </summary>
	public string? GetControlCode(byte b) {
		return _controlCodes.TryGetValue(b, out var desc) ? desc : null;
	}

	/// <summary>
	/// Add a control code
	/// </summary>
	public void AddControlCode(byte b, string description) {
		_controlCodes[b] = description;
	}

	/// <summary>
	/// Check if byte can be decoded
	/// </summary>
	public bool CanDecode(byte b) {
		return _byteToChar.ContainsKey(b) || _controlCodes.ContainsKey(b);
	}

	/// <summary>
	/// Get all byte mappings
	/// </summary>
	public IReadOnlyDictionary<byte, string> ByteMappings => _byteToChar;

	/// <summary>
	/// Get all word mappings
	/// </summary>
	public IReadOnlyDictionary<ushort, string> WordMappings => _wordToChar;

	/// <summary>
	/// Get all control codes
	/// </summary>
	public IReadOnlyDictionary<byte, string> ControlCodes => _controlCodes;

	private static void ParseDirective(TableFile table, string line) {
		var parts = line.Split('=', 2);
		if (parts.Length < 2) return;

		var directive = parts[0].ToLowerInvariant();
		var value = parts[1].Trim();

		switch (directive) {
			case "@name":
				table.Name = value;
				break;
			case "@end":
				if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out byte end)) {
					table.EndByte = end;
				}
				break;
		}
	}

	private static string UnescapeString(string s) {
		if (string.IsNullOrEmpty(s)) return s;

		return s
			.Replace("<space>", " ")
			.Replace("<SPACE>", " ")
			.Replace("\\n", "\n")
			.Replace("\\r", "\r")
			.Replace("\\t", "\t")
			.Replace("\\\\", "\\");
	}
}

/// <summary>
/// Text extraction utilities
/// </summary>
public static class TextExtraction {
	/// <summary>
	/// Extract text from ROM data using a table file
	/// </summary>
	public static string ExtractText(ReadOnlySpan<byte> data, int offset, int maxLength, TableFile table, TextExtractionOptions? options = null) {
		options ??= new TextExtractionOptions();
		var sb = new StringBuilder();
		int pos = offset;
		int endPos = Math.Min(offset + maxLength, data.Length);
		byte? endByte = options.EndByte ?? table.EndByte;

		while (pos < endPos) {
			byte b = data[pos];

			// Check for end byte
			if (options.NullTerminated && b == 0x00) break;
			if (endByte.HasValue && b == endByte.Value) break;

			// Try word decode first if table has word entries
			if (table.HasWordEntries && pos + 1 < endPos) {
				ushort word = (ushort)(data[pos] | (data[pos + 1] << 8));
				var wordChar = table.DecodeWord(word);
				if (wordChar != null) {
					sb.Append(wordChar);
					pos += 2;
					continue;
				}
			}

			// Try byte decode
			var byteChar = table.DecodeByte(b);
			if (byteChar != null) {
				sb.Append(byteChar);
			} else if (table.IsControlCode(b)) {
				if (options.IncludeControlCodes) {
					var code = table.GetControlCode(b);
					sb.Append($"[{code ?? $"${b:x2}"}]");
				}
			} else {
				// Unknown byte - show as hex
				sb.Append($"[${b:x2}]");
			}

			pos++;
		}

		return sb.ToString();
	}

	/// <summary>
	/// Extract text from a pointer table
	/// </summary>
	public static List<TextBlock> ExtractFromPointerTable(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		int textBankOffset,
		TableFile table,
		TextExtractionOptions? options = null) {

		options ??= new TextExtractionOptions();
		var blocks = new List<TextBlock>();

		for (int i = 0; i < pointerCount; i++) {
			int ptrOffset = pointerTableOffset + (i * 2);
			if (ptrOffset + 1 >= data.Length) break;

			// Read 16-bit pointer (little-endian)
			int ptr = data[ptrOffset] | (data[ptrOffset + 1] << 8);
			int textOffset = textBankOffset + ptr;

			if (textOffset >= data.Length) continue;

			// Find text length (scan to end byte or null)
			int length = 0;
			byte? endByte = options.EndByte ?? table.EndByte;
			while (textOffset + length < data.Length && length < options.MaxLength) {
				byte b = data[textOffset + length];
				if (options.NullTerminated && b == 0x00) break;
				if (endByte.HasValue && b == endByte.Value) break;
				length++;
			}

			if (length < options.MinLength) continue;

			var text = ExtractText(data, textOffset, length + 1, table, options);
			var rawBytes = data.Slice(textOffset, length).ToArray();

			blocks.Add(new TextBlock {
				Offset = textOffset,
				Length = length,
				Text = text,
				RawBytes = rawBytes,
				Label = $"text_{i:x4}",
				Category = options.Category
			});
		}

		return blocks;
	}

	/// <summary>
	/// Scan ROM for text blocks
	/// </summary>
	public static List<TextBlock> ScanForText(ReadOnlySpan<byte> data, TableFile table, TextExtractionOptions options) {
		var blocks = new List<TextBlock>();
		int pos = options.StartOffset;
		int endPos = options.EndOffset > 0 ? Math.Min(options.EndOffset, data.Length) : data.Length;

		while (pos < endPos) {
			// Skip bytes that aren't in the table
			if (!table.CanDecode(data[pos])) {
				pos++;
				continue;
			}

			// Found potential text start - scan for block
			int start = pos;
			int consecutive = 0;
			int unknownCount = 0;

			while (pos < endPos) {
				byte b = data[pos];

				if (options.NullTerminated && b == 0x00) {
					pos++;
					break;
				}

				if (table.CanDecode(b)) {
					consecutive++;
					unknownCount = 0;
				} else {
					unknownCount++;
					if (unknownCount > 2) break; // Too many unknown bytes
				}

				pos++;

				if (pos - start >= options.MaxLength) break;
			}

			int length = pos - start;

			// Only add if long enough and mostly decodable
			if (length >= options.MinLength && consecutive >= options.MinLength) {
				var text = ExtractText(data, start, length, table, options);
				var rawBytes = data.Slice(start, length).ToArray();

				blocks.Add(new TextBlock {
					Offset = start,
					Length = length,
					Text = text,
					RawBytes = rawBytes,
					Category = options.Category
				});
			}
		}

		return blocks;
	}

	/// <summary>
	/// Save extracted text blocks to JSON
	/// </summary>
	public static void SaveAsJson(List<TextBlock> blocks, string path, bool indented = true) {
		var options = new JsonSerializerOptions {
			WriteIndented = indented,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var output = new {
			count = blocks.Count,
			blocks = blocks.Select(b => new {
				offset = $"0x{b.Offset:x6}",
				length = b.Length,
				label = b.Label,
				category = b.Category,
				text = b.Text,
				rawHex = Convert.ToHexString(b.RawBytes).ToLowerInvariant()
			})
		};

		var json = JsonSerializer.Serialize(output, options);
		Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
		File.WriteAllText(path, json);
	}

	/// <summary>
	/// Save extracted text as a script file
	/// </summary>
	public static void SaveAsScript(List<TextBlock> blocks, string path) {
		var sb = new StringBuilder();
		sb.AppendLine("; Text extraction script");
		sb.AppendLine($"; Blocks: {blocks.Count}");
		sb.AppendLine();

		foreach (var block in blocks) {
			sb.AppendLine($"; Offset: ${block.Offset:x6}, Length: {block.Length}");
			if (!string.IsNullOrEmpty(block.Label)) {
				sb.AppendLine($"; Label: {block.Label}");
			}
			sb.AppendLine(block.Text);
			sb.AppendLine();
		}

		Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
		File.WriteAllText(path, sb.ToString());
	}

	/// <summary>
	/// Generate a table file template from ASCII range
	/// </summary>
	public static string GenerateAsciiTblTemplate() {
		var sb = new StringBuilder();
		sb.AppendLine("; ASCII Table Template");
		sb.AppendLine("; Format: XX=char where XX is hex byte value");
		sb.AppendLine();
		sb.AppendLine("@name=ASCII");
		sb.AppendLine("@end=00");
		sb.AppendLine();

		// Printable ASCII range
		for (int i = 0x20; i <= 0x7e; i++) {
			char c = (char)i;
			sb.AppendLine($"{i:X2}={c}");
		}

		return sb.ToString();
	}
}
