namespace Peony.Core;

using System.Text;

/// <summary>
/// Dictionary-based text compression (DTE/MTE) support
/// DTE = Dual Tile Encoding (2 characters per byte)
/// MTE = Multiple Tile Encoding (2+ characters per byte)
/// </summary>
public class DictionaryTextCompressor {
	private readonly Dictionary<string, byte> _dictionary = [];
	private readonly Dictionary<byte, string> _reverseDictionary = [];
	private byte _nextDictByte = 0x00;
	private readonly byte _dictStart;
	private readonly byte _dictEnd;

	/// <summary>
	/// Create a new dictionary compressor with the specified byte range
	/// </summary>
	/// <param name="dictStart">First byte value to use for dictionary entries</param>
	/// <param name="dictEnd">Last byte value to use for dictionary entries</param>
	public DictionaryTextCompressor(byte dictStart = 0x00, byte dictEnd = 0x7f) {
		_dictStart = dictStart;
		_dictEnd = dictEnd;
		_nextDictByte = dictStart;
	}

	/// <summary>
	/// Dictionary entries (string -> byte mapping)
	/// </summary>
	public IReadOnlyDictionary<string, byte> Dictionary => _dictionary;

	/// <summary>
	/// Reverse dictionary (byte -> string mapping)
	/// </summary>
	public IReadOnlyDictionary<byte, string> ReverseDictionary => _reverseDictionary;

	/// <summary>
	/// Number of dictionary entries
	/// </summary>
	public int EntryCount => _dictionary.Count;

	/// <summary>
	/// Maximum dictionary size based on byte range
	/// </summary>
	public int MaxEntries => _dictEnd - _dictStart + 1;

	/// <summary>
	/// Add a dictionary entry
	/// </summary>
	public bool AddEntry(string text, byte? value = null) {
		if (_dictionary.ContainsKey(text)) {
			return false;
		}

		byte dictByte;
		if (value.HasValue) {
			dictByte = value.Value;
			if (_reverseDictionary.ContainsKey(dictByte)) {
				return false;
			}
		} else {
			if (_nextDictByte > _dictEnd) {
				return false;
			}
			// Find next available byte
			while (_reverseDictionary.ContainsKey(_nextDictByte) && _nextDictByte <= _dictEnd) {
				_nextDictByte++;
			}
			if (_nextDictByte > _dictEnd) {
				return false;
			}
			dictByte = _nextDictByte++;
		}

		_dictionary[text] = dictByte;
		_reverseDictionary[dictByte] = text;
		return true;
	}

	/// <summary>
	/// Load dictionary from DTE format file content
	/// Format: XX=text (where XX is hex byte value)
	/// </summary>
	public static DictionaryTextCompressor LoadFromDte(string content, byte dictStart = 0x00, byte dictEnd = 0xff) {
		var compressor = new DictionaryTextCompressor(dictStart, dictEnd);
		var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

		foreach (var line in lines) {
			var trimmed = line.Trim();

			// Skip comments
			if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#')) {
				continue;
			}

			// Parse XX=text format
			var eqIndex = trimmed.IndexOf('=');
			if (eqIndex < 2) continue;

			var hexPart = trimmed[..eqIndex];
			var textPart = trimmed[(eqIndex + 1)..];

			if (byte.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out byte b)) {
				compressor.AddEntry(textPart, b);
			}
		}

		return compressor;
	}

	/// <summary>
	/// Generate a DTE format file
	/// </summary>
	public string SaveToDte() {
		var sb = new StringBuilder();
		sb.AppendLine("; Dictionary Text Encoding (DTE) file");
		sb.AppendLine("; Format: XX=text");
		sb.AppendLine();

		foreach (var kvp in _dictionary.OrderBy(x => x.Value)) {
			sb.AppendLine($"{kvp.Value:X2}={kvp.Key}");
		}

		return sb.ToString();
	}

	/// <summary>
	/// Compress text using the dictionary
	/// Returns bytes and a table file with the dictionary entries merged
	/// </summary>
	public byte[] Compress(string text, TableFile baseTable) {
		var result = new List<byte>();
		int pos = 0;

		while (pos < text.Length) {
			bool found = false;

			// Try longest matches first (up to 8 characters for MTE)
			for (int len = Math.Min(8, text.Length - pos); len >= 2; len--) {
				var substr = text.Substring(pos, len);
				if (_dictionary.TryGetValue(substr, out byte dictByte)) {
					result.Add(dictByte);
					pos += len;
					found = true;
					break;
				}
			}

			if (!found) {
				// Single character - use base table encoding
				var c = text[pos].ToString();
				var encoded = baseTable.EncodeByte(c);
				if (encoded.HasValue) {
					result.Add(encoded.Value);
				} else {
					// Unknown character - use placeholder
					result.Add(0xff);
				}
				pos++;
			}
		}

		return [.. result];
	}

	/// <summary>
	/// Decompress bytes using the dictionary and base table
	/// </summary>
	public string Decompress(ReadOnlySpan<byte> data, TableFile baseTable, byte? endByte = null) {
		var sb = new StringBuilder();

		foreach (byte b in data) {
			if (endByte.HasValue && b == endByte.Value) {
				break;
			}

			if (_reverseDictionary.TryGetValue(b, out string? dictEntry)) {
				sb.Append(dictEntry);
			} else {
				var decoded = baseTable.DecodeByte(b);
				if (decoded != null) {
					sb.Append(decoded);
				} else {
					sb.Append($"[${b:x2}]");
				}
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Create a merged table file that includes the dictionary entries
	/// </summary>
	public TableFile CreateMergedTable(TableFile baseTable) {
		var content = new StringBuilder();

		// Add base table entries
		foreach (var kvp in baseTable.ByteMappings) {
			content.AppendLine($"{kvp.Key:X2}={EscapeForTbl(kvp.Value)}");
		}

		// Add dictionary entries (will override if conflicts)
		foreach (var kvp in _dictionary) {
			content.AppendLine($"{kvp.Value:X2}={EscapeForTbl(kvp.Key)}");
		}

		if (baseTable.EndByte.HasValue) {
			content.AppendLine($"@end={baseTable.EndByte.Value:X2}");
		}

		return TableFile.LoadFromTbl(content.ToString());
	}

	private static string EscapeForTbl(string s) {
		return s.Replace("\\", "\\\\")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t");
	}
}

/// <summary>
/// Automatic DTE dictionary builder from text corpus
/// </summary>
public static class DteDictionaryBuilder {
	/// <summary>
	/// Build a DTE dictionary from a collection of text strings
	/// Analyzes frequency of 2-character pairs and selects best candidates
	/// </summary>
	public static DictionaryTextCompressor BuildFromCorpus(
		IEnumerable<string> texts,
		int maxEntries,
		byte dictStart = 0x00,
		byte dictEnd = 0xff,
		int minPairLength = 2,
		int maxPairLength = 2) {

		// Count all n-gram frequencies
		var frequencies = new Dictionary<string, int>();

		foreach (var text in texts) {
			for (int len = minPairLength; len <= maxPairLength; len++) {
				for (int i = 0; i <= text.Length - len; i++) {
					var ngram = text.Substring(i, len);
					frequencies.TryGetValue(ngram, out int count);
					frequencies[ngram] = count + 1;
				}
			}
		}

		// Score entries by space savings: (frequency * (length - 1))
		// Each entry saves (length - 1) bytes per occurrence
		var scored = frequencies
			.Select(kvp => new {
				Text = kvp.Key,
				Frequency = kvp.Value,
				Score = kvp.Value * (kvp.Key.Length - 1)
			})
			.Where(x => x.Frequency >= 2) // Must appear at least twice
			.OrderByDescending(x => x.Score)
			.Take(maxEntries)
			.ToList();

		var compressor = new DictionaryTextCompressor(dictStart, dictEnd);

		foreach (var entry in scored) {
			if (!compressor.AddEntry(entry.Text)) {
				break; // Dictionary full
			}
		}

		return compressor;
	}

	/// <summary>
	/// Build a DTE dictionary optimized for common English pairs
	/// </summary>
	public static DictionaryTextCompressor BuildEnglishDte(byte dictStart = 0x00, byte dictEnd = 0x7f) {
		var compressor = new DictionaryTextCompressor(dictStart, dictEnd);

		// Most common English digraphs
		var commonPairs = new[] {
			"th", "he", "in", "er", "an", "re", "on", "at", "en", "nd",
			"ti", "es", "or", "te", "of", "ed", "is", "it", "al", "ar",
			"st", "to", "nt", "ng", "se", "ha", "as", "ou", "io", "le",
			"ve", "co", "me", "de", "hi", "ri", "ro", "ic", "ne", "ea",
			"ra", "ce", "li", "ch", "ll", "be", "ma", "si", "om", "ur",
			"ca", "el", "ta", "la", "ns", "di", "fo", "ho", "pe", "ec",
			"pr", "no", "ct", "us", "ac", "ot", "il", "tr", "ly", "nc",
			"et", "ut", "ss", "so", "rs", "un", "lo", "wa", "ge", "ie",
			"wh", "ee", "wi", "em", "ad", "ol", "rt", "po", "we", "na",
			"ul", "ni", "ts", "mo", "ow", "pa", "im", "mi", "ai", "sh",
			"ir", "su", "id", "os", "iv", "ia", "am", "fi", "ci", "vi",
			"pl", "ig", "tu", "ev", "ld", "ry", "mp", "fe", "bl", "ab",
			"gh", "ty", "op", "wo", "sa", "ay", "ex", "ke", "fr", "oo",
			"av", "ag", "if", "ap", "gr", "od", "bo", "sp", "rd", "do",
			"uc", "bu", "ei", "ov", "by", "rm", "ep", "tt", "oc", "fa"
		};

		foreach (var pair in commonPairs) {
			compressor.AddEntry(pair);
		}

		return compressor;
	}

	/// <summary>
	/// Build a DTE dictionary optimized for common Japanese pairs (romanized)
	/// </summary>
	public static DictionaryTextCompressor BuildJapaneseDte(byte dictStart = 0x00, byte dictEnd = 0x7f) {
		var compressor = new DictionaryTextCompressor(dictStart, dictEnd);

		// Common Japanese syllables (hiragana romanized)
		var commonPairs = new[] {
			"no", "to", "ka", "ta", "ni", "na", "wa", "ga", "de", "mo",
			"ha", "ru", "wo", "shi", "te", "ma", "ra", "ko", "re", "yo",
			"da", "ku", "ki", "su", "sa", "so", "ne", "se", "mi", "ro",
			"me", "mu", "chi", "tsu", "ya", "yu", "he", "ho", "fu", "hi",
			"nu", "ri", "ke", "a", "i", "u", "e", "o", "n"
		};

		foreach (var pair in commonPairs) {
			compressor.AddEntry(pair);
		}

		return compressor;
	}

	/// <summary>
	/// Analyze space savings from using a dictionary
	/// </summary>
	public static DteSavingsReport AnalyzeSavings(
		IEnumerable<string> texts,
		DictionaryTextCompressor compressor,
		TableFile baseTable) {

		long originalBytes = 0;
		long compressedBytes = 0;
		var entryUsage = new Dictionary<string, int>();

		foreach (var text in texts) {
			// Original: 1 byte per character
			originalBytes += text.Length;

			// Compressed
			var compressed = compressor.Compress(text, baseTable);
			compressedBytes += compressed.Length;
		}

		return new DteSavingsReport {
			OriginalBytes = originalBytes,
			CompressedBytes = compressedBytes,
			BytesSaved = originalBytes - compressedBytes,
			CompressionRatio = originalBytes > 0 ? (double)compressedBytes / originalBytes : 1.0,
			DictionarySize = compressor.EntryCount
		};
	}
}

/// <summary>
/// Report on DTE compression savings
/// </summary>
public record DteSavingsReport {
	/// <summary>Total original text bytes</summary>
	public required long OriginalBytes { get; init; }

	/// <summary>Total compressed bytes</summary>
	public required long CompressedBytes { get; init; }

	/// <summary>Bytes saved</summary>
	public required long BytesSaved { get; init; }

	/// <summary>Compression ratio (lower is better)</summary>
	public required double CompressionRatio { get; init; }

	/// <summary>Number of dictionary entries used</summary>
	public required int DictionarySize { get; init; }

	/// <summary>Percentage of space saved</summary>
	public double SavingsPercent => OriginalBytes > 0 ? (double)BytesSaved / OriginalBytes * 100 : 0;
}
