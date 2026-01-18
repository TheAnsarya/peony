namespace Peony.Core.Tests;

using Xunit;
using Peony.Core;
using Peony.Platform.NES;

public class TextExtractorTests {
	#region TableFile Tests

	[Fact]
	public void TableFile_LoadFromTbl_ParsesBasicEntries() {
		var content = """
			00=A
			01=B
			02=C
			""";

		var table = TableFile.LoadFromTbl(content);

		Assert.Equal("A", table.DecodeByte(0x00));
		Assert.Equal("B", table.DecodeByte(0x01));
		Assert.Equal("C", table.DecodeByte(0x02));
	}

	[Fact]
	public void TableFile_LoadFromTbl_IgnoresComments() {
		var content = """
			; This is a comment
			# This is also a comment
			00=A
			; Another comment
			01=B
			""";

		var table = TableFile.LoadFromTbl(content);

		Assert.Equal("A", table.DecodeByte(0x00));
		Assert.Equal("B", table.DecodeByte(0x01));
	}

	[Fact]
	public void TableFile_LoadFromTbl_ParsesDirectives() {
		var content = """
			@name=Test Table
			@end=FF
			00=A
			""";

		var table = TableFile.LoadFromTbl(content);

		Assert.Equal("Test Table", table.Name);
		Assert.Equal((byte)0xff, table.EndByte);
	}

	[Fact]
	public void TableFile_LoadFromTbl_ParsesWordEntries() {
		var content = """
			00=A
			0100=Alpha
			0200=Beta
			""";

		var table = TableFile.LoadFromTbl(content);

		Assert.True(table.HasWordEntries);
		Assert.Equal("Alpha", table.DecodeWord(0x0100));
		Assert.Equal("Beta", table.DecodeWord(0x0200));
	}

	[Fact]
	public void TableFile_LoadFromTbl_HandlesEscapeSequences() {
		var content = """
			00=\n
			01=\t
			02=\\
			""";

		var table = TableFile.LoadFromTbl(content);

		Assert.Equal("\n", table.DecodeByte(0x00));
		Assert.Equal("\t", table.DecodeByte(0x01));
		Assert.Equal("\\", table.DecodeByte(0x02));
	}

	[Fact]
	public void TableFile_CreateAsciiTable_ContainsPrintableChars() {
		var table = TableFile.CreateAsciiTable();

		Assert.Equal(" ", table.DecodeByte(0x20));
		Assert.Equal("A", table.DecodeByte(0x41));
		Assert.Equal("Z", table.DecodeByte(0x5a));
		Assert.Equal("a", table.DecodeByte(0x61));
		Assert.Equal("z", table.DecodeByte(0x7a));
		Assert.Equal("0", table.DecodeByte(0x30));
		Assert.Equal("9", table.DecodeByte(0x39));
	}

	[Fact]
	public void TableFile_EncodeByte_ReversesDecodeByte() {
		var table = TableFile.CreateAsciiTable();

		Assert.Equal((byte)0x41, table.EncodeByte("A"));
		Assert.Equal((byte)0x61, table.EncodeByte("a"));
		Assert.Equal((byte)0x30, table.EncodeByte("0"));
	}

	[Fact]
	public void TableFile_ControlCodes_AreTracked() {
		var table = TableFile.CreateAsciiTable();
		table.AddControlCode(0xfe, "NEWLINE");
		table.AddControlCode(0xff, "END");

		Assert.True(table.IsControlCode(0xfe));
		Assert.True(table.IsControlCode(0xff));
		Assert.False(table.IsControlCode(0x41));
		Assert.Equal("NEWLINE", table.GetControlCode(0xfe));
	}

	#endregion

	#region TextExtraction Tests

	[Fact]
	public void ExtractText_DecodesSimpleString() {
		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x00]; // "Hello\0"

		var text = TextExtraction.ExtractText(data, 0, data.Length, table);

		Assert.Equal("Hello", text);
	}

	[Fact]
	public void ExtractText_StopsAtNullTerminator() {
		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x48, 0x69, 0x00, 0x58, 0x58]; // "Hi\0XX"

		var text = TextExtraction.ExtractText(data, 0, data.Length, table);

		Assert.Equal("Hi", text);
	}

	[Fact]
	public void ExtractText_StopsAtCustomEndByte() {
		var content = """
			@end=FC
			41=A
			42=B
			43=C
			""";
		var table = TableFile.LoadFromTbl(content);
		byte[] data = [0x41, 0x42, 0x43, 0xfc, 0x41]; // "ABC[END]A"

		var options = new TextExtractionOptions { NullTerminated = false };
		var text = TextExtraction.ExtractText(data, 0, data.Length, table, options);

		Assert.Equal("ABC", text);
	}

	[Fact]
	public void ExtractText_IncludesControlCodes() {
		var table = TableFile.CreateAsciiTable();
		table.AddControlCode(0xfe, "NL");
		byte[] data = [0x48, 0x69, 0xfe, 0x00]; // "Hi[NL]\0"

		var options = new TextExtractionOptions { IncludeControlCodes = true };
		var text = TextExtraction.ExtractText(data, 0, data.Length, table, options);

		Assert.Equal("Hi[NL]", text);
	}

	[Fact]
	public void ExtractText_ShowsUnknownBytesAsHex() {
		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x48, 0x69, 0x80, 0x00]; // "Hi[0x80]\0"

		var text = TextExtraction.ExtractText(data, 0, data.Length, table);

		Assert.Contains("[$80]", text);
	}

	[Fact]
	public void ExtractText_HandlesOffset() {
		var table = TableFile.CreateAsciiTable();
		byte[] data = [0x00, 0x00, 0x48, 0x69, 0x00]; // "\0\0Hi\0"

		var text = TextExtraction.ExtractText(data, 2, 3, table);

		Assert.Equal("Hi", text);
	}

	#endregion

	#region Pointer Table Extraction Tests

	[Fact]
	public void ExtractFromPointerTable_ExtractsMultipleStrings() {
		var table = TableFile.CreateAsciiTable();

		// Pointer table at 0x00: [0x0004, 0x000a] pointing to strings
		// Strings at 0x04: "Hello\0World\0"
		byte[] data = [
			0x04, 0x00, // Pointer 0 -> 0x0004
			0x0a, 0x00, // Pointer 1 -> 0x000a
			0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x00, // "Hello\0"
			0x57, 0x6f, 0x72, 0x6c, 0x64, 0x00  // "World\0"
		];

		var blocks = TextExtraction.ExtractFromPointerTable(
			data,
			pointerTableOffset: 0,
			pointerCount: 2,
			textBankOffset: 0,
			table);

		Assert.Equal(2, blocks.Count);
		Assert.Equal("Hello", blocks[0].Text);
		Assert.Equal("World", blocks[1].Text);
		Assert.Equal(0x04, blocks[0].Offset);
		Assert.Equal(0x0a, blocks[1].Offset);
	}

	[Fact]
	public void ExtractFromPointerTable_WithBankOffset() {
		var table = TableFile.CreateAsciiTable();

		// Pointer at 0x00: [0x0000] but actual text at bank offset 0x8000
		byte[] data = new byte[0x8010];
		data[0] = 0x00; // Pointer low
		data[1] = 0x00; // Pointer high
		data[0x8000] = 0x48; // H
		data[0x8001] = 0x65; // e
		data[0x8002] = 0x6c; // l
		data[0x8003] = 0x6c; // l
		data[0x8004] = 0x6f; // o
		data[0x8005] = 0x00; // null

		var blocks = TextExtraction.ExtractFromPointerTable(
			data,
			pointerTableOffset: 0,
			pointerCount: 1,
			textBankOffset: 0x8000,
			table);

		Assert.Single(blocks);
		Assert.Equal("Hello", blocks[0].Text);
		Assert.Equal(0x8000, blocks[0].Offset);
	}

	#endregion

	#region Text Scanning Tests

	[Fact]
	public void ScanForText_FindsTextBlocks() {
		var table = TableFile.CreateAsciiTable();

		// Data with embedded ASCII text
		byte[] data = [
			0x00, 0x00, 0x00, // garbage
			0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x00, // "Hello\0"
			0xff, 0xff, // garbage
			0x57, 0x6f, 0x72, 0x6c, 0x64, 0x00  // "World\0"
		];

		var options = new TextExtractionOptions {
			StartOffset = 0,
			EndOffset = data.Length,
			MinLength = 3
		};

		var blocks = TextExtraction.ScanForText(data, table, options);

		Assert.Equal(2, blocks.Count);
		Assert.Equal("Hello", blocks[0].Text);
		Assert.Equal("World", blocks[1].Text);
	}

	[Fact]
	public void ScanForText_RespectsMinLength() {
		var table = TableFile.CreateAsciiTable();

		byte[] data = [
			0x41, 0x42, 0x00, // "AB\0" - too short
			0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x00 // "Hello\0"
		];

		var options = new TextExtractionOptions {
			StartOffset = 0,
			EndOffset = data.Length,
			MinLength = 4
		};

		var blocks = TextExtraction.ScanForText(data, table, options);

		Assert.Single(blocks);
		Assert.Equal("Hello", blocks[0].Text);
	}

	#endregion

	#region NES Text Extractor Tests

	[Fact]
	public void NesTextExtractor_ExtractFixedLengthStrings_Works() {
		var table = TableFile.CreateAsciiTable();
		var extractor = new NesTextExtractor();

		// 3 strings of 4 bytes each, padded with 0x00
		byte[] data = [
			0x41, 0x42, 0x43, 0x00, // "ABC\0"
			0x44, 0x45, 0x00, 0x00, // "DE\0\0"
			0x46, 0x00, 0x00, 0x00  // "F\0\0\0"
		];

		var blocks = extractor.ExtractFixedLengthStrings(data, 0, 3, 4, table, "test");

		Assert.Equal(3, blocks.Count);
		Assert.Equal("ABC", blocks[0].Text);
		Assert.Equal("DE", blocks[1].Text);
		Assert.Equal("F", blocks[2].Text);
		Assert.Equal("test_00", blocks[0].Label);
		Assert.Equal("test", blocks[0].Category);
	}

	[Fact]
	public void NesTextExtractor_CreateDragonQuestTable_HasCorrectMappings() {
		var table = NesTextExtractor.CreateDragonQuestTable();

		Assert.Equal("Dragon Quest", table.Name);
		Assert.Equal((byte)0xfc, table.EndByte);
		Assert.Equal("A", table.DecodeByte(0x80));
		Assert.Equal("Z", table.DecodeByte(0x99));
		Assert.Equal("a", table.DecodeByte(0x9a));
		Assert.Equal("0", table.DecodeByte(0xb4));
		Assert.Equal(" ", table.DecodeByte(0xc6));
	}

	[Fact]
	public void NesTextExtractor_CreateFinalFantasyTable_HasCorrectMappings() {
		var table = NesTextExtractor.CreateFinalFantasyTable();

		Assert.Equal("Final Fantasy", table.Name);
		Assert.Equal((byte)0x00, table.EndByte);
		Assert.Equal("A", table.DecodeByte(0x82));
		Assert.Equal("a", table.DecodeByte(0x9c));
		Assert.Equal(" ", table.DecodeByte(0xff));
	}

	#endregion

	#region JSON/Script Output Tests

	[Fact]
	public void SaveAsJson_CreatesValidJson() {
		var blocks = new List<TextBlock> {
			new() {
				Offset = 0x1000,
				Length = 5,
				Text = "Hello",
				RawBytes = [0x48, 0x65, 0x6c, 0x6c, 0x6f],
				Label = "msg_0",
				Category = "dialog"
			}
		};

		var path = Path.Combine(Path.GetTempPath(), $"text_test_{Guid.NewGuid()}.json");
		try {
			TextExtraction.SaveAsJson(blocks, path);

			Assert.True(File.Exists(path));
			var content = File.ReadAllText(path);
			Assert.Contains("\"count\": 1", content);
			Assert.Contains("\"offset\": \"0x001000\"", content);
			Assert.Contains("\"text\": \"Hello\"", content);
			Assert.Contains("\"label\": \"msg_0\"", content);
			Assert.Contains("\"rawHex\": \"48656c6c6f\"", content);
		} finally {
			if (File.Exists(path)) File.Delete(path);
		}
	}

	[Fact]
	public void SaveAsScript_CreatesReadableScript() {
		var blocks = new List<TextBlock> {
			new() {
				Offset = 0x1000,
				Length = 5,
				Text = "Hello, world!",
				RawBytes = [],
				Label = "greeting"
			}
		};

		var path = Path.Combine(Path.GetTempPath(), $"script_test_{Guid.NewGuid()}.txt");
		try {
			TextExtraction.SaveAsScript(blocks, path);

			Assert.True(File.Exists(path));
			var content = File.ReadAllText(path);
			Assert.Contains("; Offset: $001000", content);
			Assert.Contains("; Label: greeting", content);
			Assert.Contains("Hello, world!", content);
		} finally {
			if (File.Exists(path)) File.Delete(path);
		}
	}

	[Fact]
	public void GenerateAsciiTblTemplate_CreatesValidTbl() {
		var template = TextExtraction.GenerateAsciiTblTemplate();

		Assert.Contains("@name=ASCII", template);
		Assert.Contains("@end=00", template);
		Assert.Contains("20= ", template); // Space
		Assert.Contains("41=A", template);
		Assert.Contains("61=a", template);
		Assert.Contains("30=0", template);
	}

	#endregion
}
