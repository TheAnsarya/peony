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
	/// Create a Pokemon Red/Blue style table (Game Boy)
	/// </summary>
	public static TableFile CreatePokemonTable() {
		var table = new TableFile { Name = "Pokemon" };
		table.EndByte = 0x50; // End marker

		// Uppercase letters A-Z at 0x80-0x99
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x80 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x80 + i);
		}

		// Lowercase letters a-z at 0xa0-0xb9
		for (int i = 0; i < 26; i++) {
			var c = (char)('a' + i);
			table._byteToChar[(byte)(0xa0 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0xa0 + i);
		}

		// Numbers 0-9 at 0xf6-0xff
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0xf6 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0xf6 + i);
		}

		// Common punctuation
		table._byteToChar[0x7f] = " ";
		table._charToByte[" "] = 0x7f;
		table._byteToChar[0xe7] = "!";
		table._byteToChar[0xe8] = ".";
		table._byteToChar[0xf4] = ",";
		table._byteToChar[0xf2] = "-";
		table._byteToChar[0xf3] = "'";

		// Control codes
		table._controlCodes[0x4f] = "<line>";
		table._controlCodes[0x50] = "<end>";
		table._controlCodes[0x51] = "<para>";
		table._controlCodes[0x55] = "<cont>";
		table._controlCodes[0x57] = "<done>";

		return table;
	}

	/// <summary>
	/// Create a Dragon Quest/Warrior style table (NES/SNES)
	/// </summary>
	public static TableFile CreateDragonQuestTable() {
		var table = new TableFile { Name = "Dragon Quest" };
		table.EndByte = 0xff;

		// Uppercase A-Z starting at 0x80
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x80 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x80 + i);
		}

		// Lowercase a-z starting at 0x9a
		for (int i = 0; i < 26; i++) {
			var c = (char)('a' + i);
			table._byteToChar[(byte)(0x9a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x9a + i);
		}

		// Numbers 0-9 starting at 0xb4
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0xb4 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0xb4 + i);
		}

		// Common characters
		table._byteToChar[0xbe] = " ";
		table._charToByte[" "] = 0xbe;
		table._byteToChar[0xbf] = ".";
		table._byteToChar[0xc0] = ",";
		table._byteToChar[0xc1] = "-";
		table._byteToChar[0xc2] = "!";
		table._byteToChar[0xc3] = "?";
		table._byteToChar[0xc4] = "'";
		table._byteToChar[0xc5] = "\"";
		table._byteToChar[0xc6] = ":";

		// Control codes
		table._controlCodes[0xf0] = "<name>";
		table._controlCodes[0xf1] = "<item>";
		table._controlCodes[0xf2] = "<number>";
		table._controlCodes[0xfc] = "<line>";
		table._controlCodes[0xfd] = "<para>";
		table._controlCodes[0xfe] = "<wait>";
		table._controlCodes[0xff] = "<end>";

		return table;
	}

	/// <summary>
	/// Create a Final Fantasy style table (NES/SNES)
	/// </summary>
	public static TableFile CreateFinalFantasyTable() {
		var table = new TableFile { Name = "Final Fantasy" };
		table.EndByte = 0x00;

		// Uppercase A-Z starting at 0x8a
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x8a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x8a + i);
		}

		// Lowercase a-z starting at 0xa4
		for (int i = 0; i < 26; i++) {
			var c = (char)('a' + i);
			table._byteToChar[(byte)(0xa4 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0xa4 + i);
		}

		// Numbers 0-9 starting at 0x80
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0x80 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x80 + i);
		}

		// Common punctuation
		table._byteToChar[0xff] = " ";
		table._charToByte[" "] = 0xff;
		table._byteToChar[0xc0] = ".";
		table._byteToChar[0xc1] = ",";
		table._byteToChar[0xc2] = "-";
		table._byteToChar[0xc3] = "'";
		table._byteToChar[0xc4] = "!";
		table._byteToChar[0xc5] = "?";
		table._byteToChar[0xbe] = ":";

		// Control codes
		table._controlCodes[0x00] = "<end>";
		table._controlCodes[0x01] = "<line>";
		table._controlCodes[0x02] = "<name>";
		table._controlCodes[0x03] = "<item>";

		return table;
	}

	/// <summary>
	/// Create a basic Shift-JIS compatible table for Japanese text
	/// </summary>
	public static TableFile CreateShiftJisTable() {
		var table = new TableFile { Name = "Shift-JIS" };

		// ASCII range (single byte)
		for (int i = 0x20; i <= 0x7e; i++) {
			table._byteToChar[(byte)i] = ((char)i).ToString();
			table._charToByte[((char)i).ToString()] = (byte)i;
		}

		// Half-width katakana (0xa1-0xdf)
		var katakana = "。「」、・ヲァィゥェォャュョッーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン゛゜";
		for (int i = 0; i < katakana.Length && i < 63; i++) {
			table._byteToChar[(byte)(0xa1 + i)] = katakana[i].ToString();
			table._charToByte[katakana[i].ToString()] = (byte)(0xa1 + i);
		}

		// Common two-byte hiragana (82xx range)
		var hiragana = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをん";
		for (int i = 0; i < hiragana.Length && i < 83; i++) {
			ushort code = (ushort)(0x829f + i);
			// Skip gaps in Shift-JIS encoding
			if (i >= 31) code++;
			table._wordToChar[code] = hiragana[i].ToString();
			table._charToWord[hiragana[i].ToString()] = code;
		}

		return table;
	}

	/// <summary>
	/// Create a Mother/EarthBound style table (SNES)
	/// </summary>
	public static TableFile CreateEarthBoundTable() {
		var table = new TableFile { Name = "EarthBound" };
		table.EndByte = 0x02;

		// Uppercase A-Z at 0x30-0x49
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x30 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x30 + i);
		}

		// Lowercase a-z at 0x4a-0x63
		for (int i = 0; i < 26; i++) {
			var c = (char)('a' + i);
			table._byteToChar[(byte)(0x4a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x4a + i);
		}

		// Numbers 0-9 at 0x60-0x69
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0x60 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x60 + i);
		}

		// Space and punctuation
		table._byteToChar[0x00] = " ";
		table._charToByte[" "] = 0x00;
		table._byteToChar[0x1e] = "-";
		table._byteToChar[0x1f] = "'";
		table._byteToChar[0x6a] = "!";
		table._byteToChar[0x6b] = "?";
		table._byteToChar[0x6c] = ".";
		table._byteToChar[0x6d] = ",";

		// Control codes
		table._controlCodes[0x02] = "<end>";
		table._controlCodes[0x03] = "<line>";
		table._controlCodes[0x04] = "<page>";
		table._controlCodes[0x1c] = "<name>";
		table._controlCodes[0x1d] = "<item>";

		return table;
	}

	/// <summary>
	/// Create a Legend of Zelda style table (NES)
	/// </summary>
	public static TableFile CreateZeldaTable() {
		var table = new TableFile { Name = "Zelda" };
		table.EndByte = 0xff;

		// Uppercase letters A-Z at 0x0a-0x23
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x0a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x0a + i);
		}

		// Numbers 0-9 at 0x00-0x09
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)i] = c.ToString();
			table._charToByte[c.ToString()] = (byte)i;
		}

		// Space and punctuation
		table._byteToChar[0x24] = " ";
		table._charToByte[" "] = 0x24;
		table._byteToChar[0x28] = ",";
		table._byteToChar[0x29] = "!";
		table._byteToChar[0x2a] = "'";
		table._byteToChar[0x2b] = "&";
		table._byteToChar[0x2c] = ".";
		table._byteToChar[0x2d] = "\"";
		table._byteToChar[0x2e] = "?";
		table._byteToChar[0x2f] = "-";

		// Control codes
		table._controlCodes[0xe0] = "<line>";
		table._controlCodes[0xe1] = "<item>";
		table._controlCodes[0xe2] = "<rupee>";
		table._controlCodes[0xff] = "<end>";

		return table;
	}

	/// <summary>
	/// Create a Metroid style table (NES)
	/// </summary>
	public static TableFile CreateMetroidTable() {
		var table = new TableFile { Name = "Metroid" };
		table.EndByte = 0xff;

		// Uppercase letters A-Z at 0x0a-0x23
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)(0x0a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x0a + i);
		}

		// Numbers 0-9 at 0x00-0x09
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)i] = c.ToString();
			table._charToByte[c.ToString()] = (byte)i;
		}

		// Space
		table._byteToChar[0x24] = " ";
		table._charToByte[" "] = 0x24;
		table._byteToChar[0xfe] = ".";

		// Control codes
		table._controlCodes[0xff] = "<end>";

		return table;
	}

	/// <summary>
	/// Create a Castlevania style table (NES)
	/// </summary>
	public static TableFile CreateCastlevaniaTable() {
		var table = new TableFile { Name = "Castlevania" };
		table.EndByte = 0x40;

		// Uppercase letters A-Z at 0x00-0x19
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)i] = c.ToString();
			table._charToByte[c.ToString()] = (byte)i;
		}

		// Numbers 0-9 at 0x1a-0x23
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0x1a + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x1a + i);
		}

		// Space and punctuation
		table._byteToChar[0x24] = " ";
		table._charToByte[" "] = 0x24;
		table._byteToChar[0x28] = ".";
		table._byteToChar[0x2c] = "-";
		table._byteToChar[0x2d] = "!";
		table._byteToChar[0x2e] = "'";

		// Control codes
		table._controlCodes[0x40] = "<end>";
		table._controlCodes[0x41] = "<line>";

		return table;
	}

	/// <summary>
	/// Create a Mega Man style table (NES)
	/// </summary>
	public static TableFile CreateMegaManTable() {
		var table = new TableFile { Name = "Mega Man" };
		table.EndByte = 0xff;

		// Uppercase letters A-Z at 0x00-0x19
		for (int i = 0; i < 26; i++) {
			var c = (char)('A' + i);
			table._byteToChar[(byte)i] = c.ToString();
			table._charToByte[c.ToString()] = (byte)i;
		}

		// Numbers 0-9 at 0x50-0x59
		for (int i = 0; i < 10; i++) {
			var c = (char)('0' + i);
			table._byteToChar[(byte)(0x50 + i)] = c.ToString();
			table._charToByte[c.ToString()] = (byte)(0x50 + i);
		}

		// Space and punctuation
		table._byteToChar[0x40] = " ";
		table._charToByte[" "] = 0x40;
		table._byteToChar[0x1a] = ".";
		table._byteToChar[0x1b] = "-";
		table._byteToChar[0x1c] = "!";

		// Control codes
		table._controlCodes[0xff] = "<end>";

		return table;
	}

	/// <summary>
	/// Get a table by template name
	/// </summary>
	public static TableFile GetTemplate(string name) {
		return name.ToLowerInvariant() switch {
			"ascii" => CreateAsciiTable(),
			"pokemon" or "pkmn" => CreatePokemonTable(),
			"dw" or "dq" or "dragonquest" or "dragonwarrior" => CreateDragonQuestTable(),
			"ff" or "finalfantasy" => CreateFinalFantasyTable(),
			"sjis" or "shiftjis" or "shift-jis" => CreateShiftJisTable(),
			"eb" or "earthbound" or "mother" => CreateEarthBoundTable(),
			"zelda" or "loz" => CreateZeldaTable(),
			"metroid" => CreateMetroidTable(),
			"castlevania" or "cv" => CreateCastlevaniaTable(),
			"megaman" or "mm" or "rockman" => CreateMegaManTable(),
			_ => CreateAsciiTable()
		};
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
