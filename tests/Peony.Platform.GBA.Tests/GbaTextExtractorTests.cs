namespace Peony.Platform.GBA.Tests;

using Peony.Core;
using Xunit;

public class GbaTextExtractorTests {
	private readonly GbaTextExtractor _extractor = new();

	#region Basic Text Extraction Tests

	[Fact]
	public void ExtractText_WithSimpleAsciiTable_ExtractsCorrectly() {
		// Create simple ASCII table
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C");
		table.EndByte = 0x00;
		byte[] data = [0x41, 0x42, 0x43, 0x00]; // "ABC" + end

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("ABC", result);
	}

	[Fact]
	public void ExtractText_WithOffset_StartsAtCorrectPosition() {
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C");
		table.EndByte = 0x00;
		byte[] data = [0xFF, 0xFF, 0x41, 0x42, 0x43, 0x00];

		var result = _extractor.ExtractText(data, 2, 100, table);

		Assert.Equal("ABC", result);
	}

	[Fact]
	public void ExtractText_WithMaxLength_TruncatesCorrectly() {
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C");
		table.EndByte = 0x00;
		byte[] data = [0x41, 0x42, 0x43, 0x00];

		var result = _extractor.ExtractText(data, 0, 2, table);

		Assert.Equal("AB", result);
	}

	#endregion

	#region 32-bit Pointer Table Tests

	[Fact]
	public void ExtractFromPointerTable32_WithValidPointers_ExtractsAllStrings() {
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C");
		table.EndByte = 0x00;

		// Build test data:
		// Offset 0x00: Pointer table (2 x 32-bit pointers)
		// Offset 0x08: First string "ABC"
		// Offset 0x0C: Second string "BA"
		byte[] data = [
			// Pointer table (offset 0x00)
			0x08, 0x00, 0x00, 0x08, // 0x08000008 -> offset 0x08
			0x0C, 0x00, 0x00, 0x08, // 0x0800000C -> offset 0x0C
			// String 1 at 0x08
			0x41, 0x42, 0x43, 0x00, // "ABC"
			// String 2 at 0x0C
			0x42, 0x41, 0x00, 0xFF  // "BA"
		];

		var results = _extractor.ExtractFromPointerTable32(data, 0x00, 2, table, "Test");

		Assert.Equal(2, results.Count);
		Assert.Equal("ABC", results[0].Text);
		Assert.Equal("BA", results[1].Text);
		Assert.Equal(0x08, results[0].Offset);
		Assert.Equal(0x0C, results[1].Offset);
	}

	[Fact]
	public void ExtractFromPointerTable32_WithInvalidPointer_SkipsEntry() {
		var table = TableFile.LoadFromTbl("41=A");
		table.EndByte = 0x00;

		byte[] data = [
			// Pointer 1: Invalid (WRAM address)
			0x00, 0x00, 0x00, 0x02, // 0x02000000 - WRAM
			// Pointer 2: Valid ROM address
			0x08, 0x00, 0x00, 0x08, // 0x08000008 -> offset 0x08
			// String at 0x08
			0x41, 0x00
		];

		var results = _extractor.ExtractFromPointerTable32(data, 0x00, 2, table);

		Assert.Single(results);
		Assert.Equal("A", results[0].Text);
	}

	[Fact]
	public void ExtractFromPointerTable32_WithWaitState1Address_ConvertsCorrectly() {
		var table = TableFile.LoadFromTbl("41=A");
		table.EndByte = 0x00;

		byte[] data = [
			// Pointer: Wait State 1 address (0x0A000008)
			0x08, 0x00, 0x00, 0x0A,
			0xFF, 0xFF, 0xFF, 0xFF,
			// String at offset 0x08
			0x41, 0x00
		];

		var results = _extractor.ExtractFromPointerTable32(data, 0x00, 1, table);

		Assert.Single(results);
		Assert.Equal(0x08, results[0].Offset);
	}

	[Fact]
	public void ExtractFromPointerTable32_WithWaitState2Address_ConvertsCorrectly() {
		var table = TableFile.LoadFromTbl("41=A");
		table.EndByte = 0x00;

		byte[] data = [
			// Pointer: Wait State 2 address (0x0C000008)
			0x08, 0x00, 0x00, 0x0C,
			0xFF, 0xFF, 0xFF, 0xFF,
			// String at offset 0x08
			0x41, 0x00
		];

		var results = _extractor.ExtractFromPointerTable32(data, 0x00, 1, table);

		Assert.Single(results);
		Assert.Equal(0x08, results[0].Offset);
	}

	#endregion

	#region 16-bit Pointer Table Tests

	[Fact]
	public void ExtractFromPointerTable16_WithBaseOffset_ExtractsCorrectly() {
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C");
		table.EndByte = 0x00;

		// Pointers at 0x00, strings at 0x04
		byte[] data = [
			// Pointer table (16-bit offsets from base 0x04)
			0x00, 0x00, // 0x0000 + base 0x04 = 0x04
			0x04, 0x00, // 0x0004 + base 0x04 = 0x08
			// String 1 at 0x04 (need at least 3 chars for MinLength)
			0x41, 0x42, 0x43, 0x00, // "ABC"
			// String 2 at 0x08
			0x42, 0x43, 0x41, 0x00  // "BCA"
		];

		var results = _extractor.ExtractFromPointerTable16(data, 0x00, 2, 0x04, table, "Test");

		Assert.Equal(2, results.Count);
		Assert.Equal("ABC", results[0].Text);
		Assert.Equal("BCA", results[1].Text);
	}

	#endregion

	#region Address Conversion Tests

	[Theory]
	[InlineData(0x08000000u, 0)]         // Start of ROM WS0
	[InlineData(0x08001000u, 0x1000)]    // Middle of ROM
	[InlineData(0x09FFFFFFu, 0x01FFFFFF)] // End of ROM WS0
	[InlineData(0x0A000000u, 0)]         // Start of ROM WS1
	[InlineData(0x0A001000u, 0x1000)]    // Middle of ROM WS1
	[InlineData(0x0C000000u, 0)]         // Start of ROM WS2
	[InlineData(0x0C001000u, 0x1000)]    // Middle of ROM WS2
	public void GbaAddressToOffset_WithRomAddress_ReturnsCorrectOffset(uint address, int expectedOffset) {
		var result = GbaTextExtractor.GbaAddressToOffset(address);
		Assert.Equal(expectedOffset, result);
	}

	[Theory]
	[InlineData(0x00000000u)] // BIOS
	[InlineData(0x02000000u)] // WRAM
	[InlineData(0x03000000u)] // IWRAM
	[InlineData(0x04000000u)] // I/O
	[InlineData(0x05000000u)] // Palette RAM
	[InlineData(0x06000000u)] // VRAM
	[InlineData(0x07000000u)] // OAM
	[InlineData(0x0E000000u)] // SRAM
	public void GbaAddressToOffset_WithNonRomAddress_ReturnsNegativeOne(uint address) {
		var result = GbaTextExtractor.GbaAddressToOffset(address);
		Assert.Equal(-1, result);
	}

	[Theory]
	[InlineData(0, 0x08000000u)]
	[InlineData(0x1000, 0x08001000u)]
	[InlineData(0x01FFFFFF, 0x09FFFFFFu)]
	public void OffsetToGbaAddress_ReturnsWaitState0Address(int offset, uint expectedAddress) {
		var result = GbaTextExtractor.OffsetToGbaAddress(offset);
		Assert.Equal(expectedAddress, result);
	}

	#endregion

	#region Pokemon GBA Table Tests

	[Fact]
	public void CreatePokemonGbaTable_HasCorrectEndByte() {
		var table = GbaTextExtractor.CreatePokemonGbaTable();
		Assert.Equal((byte)0xFF, table.EndByte);
	}

	[Fact]
	public void CreatePokemonGbaTable_DecodesUppercaseLetters() {
		var table = GbaTextExtractor.CreatePokemonGbaTable();
		byte[] data = [0xBB, 0xBC, 0xBD, 0xFF]; // ABC

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("ABC", result);
	}

	[Fact]
	public void CreatePokemonGbaTable_DecodesLowercaseLetters() {
		var table = GbaTextExtractor.CreatePokemonGbaTable();
		byte[] data = [0xD5, 0xD6, 0xD7, 0xFF]; // abc

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("abc", result);
	}

	[Fact]
	public void CreatePokemonGbaTable_DecodesNumbers() {
		var table = GbaTextExtractor.CreatePokemonGbaTable();
		byte[] data = [0xA1, 0xA2, 0xA3, 0xFF]; // 012

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("012", result);
	}

	[Fact]
	public void CreatePokemonGbaTable_DecodesControlCodes() {
		var table = GbaTextExtractor.CreatePokemonGbaTable();
		byte[] data = [0xFA, 0xFE, 0xFF]; // [PLAYER][LINE] + end

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("[PLAYER][LINE]", result);
	}

	#endregion

	#region Golden Sun Table Tests

	[Fact]
	public void CreateGoldenSunTable_HasCorrectEndByte() {
		var table = GbaTextExtractor.CreateGoldenSunTable();
		Assert.Equal((byte)0x00, table.EndByte);
	}

	[Fact]
	public void CreateGoldenSunTable_DecodesAsciiText() {
		var table = GbaTextExtractor.CreateGoldenSunTable();
		byte[] data = [0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00]; // "Hello"

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("Hello", result);
	}

	[Fact]
	public void CreateGoldenSunTable_DecodesControlCodes() {
		var table = GbaTextExtractor.CreateGoldenSunTable();
		byte[] data = [0x48, 0x69, 0x01, 0x00]; // "Hi" + LINE

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Contains("[LINE]", result);
	}

	#endregion

	#region Fire Emblem Table Tests

	[Fact]
	public void CreateFireEmblemTable_HasCorrectEndByte() {
		var table = GbaTextExtractor.CreateFireEmblemTable();
		Assert.Equal((byte)0x00, table.EndByte);
	}

	[Fact]
	public void CreateFireEmblemTable_DecodesAsciiText() {
		var table = GbaTextExtractor.CreateFireEmblemTable();
		byte[] data = [0x4D, 0x61, 0x72, 0x74, 0x68, 0x00]; // "Marth"

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("Marth", result);
	}

	[Fact]
	public void CreateFireEmblemTable_DecodesControlCodes() {
		var table = GbaTextExtractor.CreateFireEmblemTable();
		byte[] data = [0x05, 0x0A, 0x00]; // [FACE][TACTICIAN]

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Contains("[FACE]", result);
		Assert.Contains("[TACTICIAN]", result);
	}

	#endregion

	#region Final Fantasy Advance Table Tests

	[Fact]
	public void CreateFinalFantasyAdvanceTable_HasCorrectEndByte() {
		var table = GbaTextExtractor.CreateFinalFantasyAdvanceTable();
		Assert.Equal((byte)0x00, table.EndByte);
	}

	[Fact]
	public void CreateFinalFantasyAdvanceTable_DecodesAsciiText() {
		var table = GbaTextExtractor.CreateFinalFantasyAdvanceTable();
		byte[] data = [0x43, 0x65, 0x63, 0x69, 0x6C, 0x00]; // "Cecil"

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Equal("Cecil", result);
	}

	[Fact]
	public void CreateFinalFantasyAdvanceTable_DecodesControlCodes() {
		var table = GbaTextExtractor.CreateFinalFantasyAdvanceTable();
		byte[] data = [0x04, 0x05, 0x06, 0x00]; // [NAME][ITEM][NUM]

		var result = _extractor.ExtractText(data, 0, 100, table);

		Assert.Contains("[NAME]", result);
		Assert.Contains("[ITEM]", result);
		Assert.Contains("[NUM]", result);
	}

	#endregion

	#region Text Scanning Tests

	[Fact]
	public void ExtractAllText_FindsMultipleTextBlocks() {
		var table = TableFile.LoadFromTbl("41=A\n42=B\n43=C\n00=[END]");
		table.EndByte = 0x00;

		byte[] data = [
			0x41, 0x42, 0x43, 0x00, // ABC
			0xFF, 0xFF,             // Padding
			0x42, 0x43, 0x41, 0x00  // BCA
		];

		var options = new TextExtractionOptions {
			MinLength = 2,
			NullTerminated = true
		};

		var results = _extractor.ExtractAllText(data, table, options);

		Assert.True(results.Count >= 2);
	}

	#endregion
}
