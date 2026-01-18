namespace Peony.Core.Tests;

using Xunit;

public class DictionaryTextCompressorTests {
	[Fact]
	public void AddEntry_AddsNewEntry_ReturnsTrue() {
		var compressor = new DictionaryTextCompressor();
		Assert.True(compressor.AddEntry("th"));
		Assert.Equal(1, compressor.EntryCount);
	}

	[Fact]
	public void AddEntry_DuplicateEntry_ReturnsFalse() {
		var compressor = new DictionaryTextCompressor();
		compressor.AddEntry("th");
		Assert.False(compressor.AddEntry("th"));
	}

	[Fact]
	public void AddEntry_WithSpecificValue_UsesValue() {
		var compressor = new DictionaryTextCompressor();
		compressor.AddEntry("th", 0x80);

		Assert.True(compressor.Dictionary.ContainsKey("th"));
		Assert.Equal((byte)0x80, compressor.Dictionary["th"]);
	}

	[Fact]
	public void AddEntry_AutoAssignsValues() {
		var compressor = new DictionaryTextCompressor(0x00, 0xff);
		compressor.AddEntry("aa");
		compressor.AddEntry("bb");
		compressor.AddEntry("cc");

		Assert.Equal(3, compressor.EntryCount);
		Assert.NotEqual(compressor.Dictionary["aa"], compressor.Dictionary["bb"]);
	}

	[Fact]
	public void AddEntry_RespectsByteRange() {
		var compressor = new DictionaryTextCompressor(0x80, 0x82);
		Assert.True(compressor.AddEntry("aa"));
		Assert.True(compressor.AddEntry("bb"));
		Assert.True(compressor.AddEntry("cc"));
		Assert.False(compressor.AddEntry("dd")); // Out of range

		Assert.Equal(3, compressor.EntryCount);
	}

	[Fact]
	public void LoadFromDte_ParsesEntries() {
		var content = """
			; Test DTE
			80=th
			81=he
			82=in
			""";

		var compressor = DictionaryTextCompressor.LoadFromDte(content);

		Assert.Equal(3, compressor.EntryCount);
		Assert.Equal("th", compressor.ReverseDictionary[0x80]);
		Assert.Equal("he", compressor.ReverseDictionary[0x81]);
	}

	[Fact]
	public void LoadFromDte_SkipsComments() {
		var content = """
			; Comment line
			# Another comment
			80=th
			; Inline comment ignored
			81=he
			""";

		var compressor = DictionaryTextCompressor.LoadFromDte(content);
		Assert.Equal(2, compressor.EntryCount);
	}

	[Fact]
	public void SaveToDte_ProducesValidFormat() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);
		compressor.AddEntry("he", 0x81);

		var output = compressor.SaveToDte();

		Assert.Contains("80=th", output);
		Assert.Contains("81=he", output);
	}

	[Fact]
	public void Compress_UsesDictionaryEntries() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);
		compressor.AddEntry("he", 0x81);

		var table = TableFile.CreateAsciiTable();
		var result = compressor.Compress("the", table);

		// "th" should be 0x80, "e" should be ASCII 'e' (0x65)
		Assert.Equal(2, result.Length);
		Assert.Equal(0x80, result[0]);
		Assert.Equal(0x65, result[1]);
	}

	[Fact]
	public void Compress_FallsBackToBaseTable() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		// No dictionary entries

		var table = TableFile.CreateAsciiTable();
		var result = compressor.Compress("abc", table);

		Assert.Equal(3, result.Length);
		Assert.Equal(0x61, result[0]); // 'a'
		Assert.Equal(0x62, result[1]); // 'b'
		Assert.Equal(0x63, result[2]); // 'c'
	}

	[Fact]
	public void Decompress_UsesDictionary() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);
		compressor.AddEntry("he", 0x81);

		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x80, 0x65]; // "th" + "e"

		var result = compressor.Decompress(data, table);

		Assert.Equal("the", result);
	}

	[Fact]
	public void Decompress_StopsAtEndByte() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);

		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x80, 0x00, 0x65]; // "th" + end + "e"

		var result = compressor.Decompress(data, table, endByte: 0x00);

		Assert.Equal("th", result);
	}

	[Fact]
	public void RoundTrip_PreservesText() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);
		compressor.AddEntry("he", 0x81);
		compressor.AddEntry("in", 0x82);

		var table = TableFile.CreateAsciiTable();
		var original = "the thing";

		var compressed = compressor.Compress(original, table);
		var decompressed = compressor.Decompress(compressed, table);

		Assert.Equal(original, decompressed);
	}

	[Fact]
	public void CreateMergedTable_IncludesBothTableAndDictionary() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);

		var baseTable = TableFile.LoadFromTbl("41=A\n42=B");

		var merged = compressor.CreateMergedTable(baseTable);

		Assert.Equal("A", merged.DecodeByte(0x41));
		Assert.Equal("th", merged.DecodeByte(0x80));
	}

	[Fact]
	public void MaxEntries_ReflectsByteRange() {
		var compressor = new DictionaryTextCompressor(0x80, 0x8f);
		Assert.Equal(16, compressor.MaxEntries);
	}
}

public class DteDictionaryBuilderTests {
	[Fact]
	public void BuildFromCorpus_FindsFrequentPairs() {
		var texts = new[] {
			"the thing",
			"the other",
			"with the"
		};

		var compressor = DteDictionaryBuilder.BuildFromCorpus(texts, maxEntries: 10);

		// "th" appears 4 times, should be included
		Assert.True(compressor.Dictionary.ContainsKey("th"));
	}

	[Fact]
	public void BuildFromCorpus_RespectsMaxEntries() {
		var texts = new[] {
			"the thing that is there then"
		};

		var compressor = DteDictionaryBuilder.BuildFromCorpus(texts, maxEntries: 3);

		Assert.True(compressor.EntryCount <= 3);
	}

	[Fact]
	public void BuildFromCorpus_PrioritizesByScore() {
		var texts = new[] {
			"aaaaaa", // "aa" appears 5 times
			"bbbb",   // "bb" appears 3 times
			"cc"      // "cc" appears 1 time
		};

		var compressor = DteDictionaryBuilder.BuildFromCorpus(texts, maxEntries: 2);

		// "aa" should be included (highest score)
		Assert.True(compressor.Dictionary.ContainsKey("aa"));
	}

	[Fact]
	public void BuildEnglishDte_IncludesCommonPairs() {
		var compressor = DteDictionaryBuilder.BuildEnglishDte();

		Assert.True(compressor.Dictionary.ContainsKey("th"));
		Assert.True(compressor.Dictionary.ContainsKey("he"));
		Assert.True(compressor.Dictionary.ContainsKey("in"));
		Assert.True(compressor.Dictionary.ContainsKey("er"));
	}

	[Fact]
	public void BuildJapaneseDte_IncludesCommonSyllables() {
		var compressor = DteDictionaryBuilder.BuildJapaneseDte();

		Assert.True(compressor.Dictionary.ContainsKey("no"));
		Assert.True(compressor.Dictionary.ContainsKey("to"));
		Assert.True(compressor.Dictionary.ContainsKey("ka"));
	}

	[Fact]
	public void AnalyzeSavings_CalculatesCorrectly() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		compressor.AddEntry("th", 0x80);
		compressor.AddEntry("he", 0x81);

		var table = TableFile.CreateAsciiTable();
		var texts = new[] { "the the the" }; // 11 chars

		var report = DteDictionaryBuilder.AnalyzeSavings(texts, compressor, table);

		Assert.Equal(11, report.OriginalBytes);
		Assert.True(report.CompressedBytes < report.OriginalBytes);
		Assert.True(report.BytesSaved > 0);
		Assert.True(report.SavingsPercent > 0);
	}

	[Fact]
	public void AnalyzeSavings_NoCompression_Returns100Percent() {
		var compressor = new DictionaryTextCompressor(0x80, 0xff);
		// No dictionary entries

		var table = TableFile.CreateAsciiTable();
		var texts = new[] { "xyz" }; // No common pairs

		var report = DteDictionaryBuilder.AnalyzeSavings(texts, compressor, table);

		Assert.Equal(1.0, report.CompressionRatio);
	}
}

public class DteSavingsReportTests {
	[Fact]
	public void SavingsPercent_CalculatesCorrectly() {
		var report = new DteSavingsReport {
			OriginalBytes = 100,
			CompressedBytes = 80,
			BytesSaved = 20,
			CompressionRatio = 0.8,
			DictionarySize = 10
		};

		Assert.Equal(20.0, report.SavingsPercent);
	}

	[Fact]
	public void SavingsPercent_ZeroOriginal_ReturnsZero() {
		var report = new DteSavingsReport {
			OriginalBytes = 0,
			CompressedBytes = 0,
			BytesSaved = 0,
			CompressionRatio = 1.0,
			DictionarySize = 0
		};

		Assert.Equal(0.0, report.SavingsPercent);
	}
}
