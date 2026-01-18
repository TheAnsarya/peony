namespace Peony.Platform.SNES.Tests;

using Peony.Core;
using Xunit;

public class SnesTextExtractorTests {
	[Fact]
	public void ExtractText_WithFinalFantasyTable_DecodesCorrectly() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// "HELLO" in FF style encoding: H=89, E=86, L=8D, L=8D, O=90
		byte[] data = [0x89, 0x86, 0x8d, 0x8d, 0x90, 0x00];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("HELLO", result);
	}

	[Fact]
	public void ExtractText_WithChronoTriggerTable_DecodesAsciiStyle() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateChronoTriggerTable();

		// ASCII-like: "Test" = 0x54, 0x65, 0x73, 0x74
		byte[] data = [0x54, 0x65, 0x73, 0x74, 0x00];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("Test", result);
	}

	[Fact]
	public void ExtractText_WithDragonQuestTable_DecodesCorrectly() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateDragonQuestTable();

		// "HERO" in DQ style: H=07, E=04, R=11, O=0E
		byte[] data = [0x07, 0x04, 0x11, 0x0e, 0xff];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("HERO", result);
	}

	[Fact]
	public void CreateFinalFantasyTable_HasCorrectMappings() {
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		Assert.Equal("A", table.DecodeByte(0x82));
		Assert.Equal("Z", table.DecodeByte(0x9b));
		Assert.Equal("a", table.DecodeByte(0x9c));
		Assert.Equal("0", table.DecodeByte(0xb6));
		Assert.Equal((byte)0x00, table.EndByte);
	}

	[Fact]
	public void CreateChronoTriggerTable_HasAsciiMapping() {
		var table = SnesTextExtractor.CreateChronoTriggerTable();

		Assert.Equal("A", table.DecodeByte(0x41));
		Assert.Equal("Z", table.DecodeByte(0x5a));
		Assert.Equal("a", table.DecodeByte(0x61));
		Assert.Equal("z", table.DecodeByte(0x7a));
		Assert.Equal("0", table.DecodeByte(0x30));
	}

	[Fact]
	public void CreateDragonQuestTable_HasCorrectEndByte() {
		var table = SnesTextExtractor.CreateDragonQuestTable();

		Assert.Equal((byte)0xff, table.EndByte);
		Assert.Equal("A", table.DecodeByte(0x00));
	}

	[Fact]
	public void SnesAddressToOffset_LoRom_ConvertsBankCorrectly() {
		// Bank $00, offset $8000 = file offset $0000
		int result = SnesTextExtractor.SnesAddressToOffset(0x008000, SnesMapMode.LoRom);
		Assert.Equal(0, result);

		// Bank $01, offset $8000 = file offset $8000
		result = SnesTextExtractor.SnesAddressToOffset(0x018000, SnesMapMode.LoRom);
		Assert.Equal(0x8000, result);

		// Bank $02, offset $FFFF = file offset $17FFF
		result = SnesTextExtractor.SnesAddressToOffset(0x02ffff, SnesMapMode.LoRom);
		Assert.Equal(0x17fff, result);
	}

	[Fact]
	public void SnesAddressToOffset_LoRom_RejectsLowAddresses() {
		// LoROM: addresses below $8000 in bank are not ROM
		int result = SnesTextExtractor.SnesAddressToOffset(0x007fff, SnesMapMode.LoRom);
		Assert.Equal(-1, result);
	}

	[Fact]
	public void SnesAddressToOffset_LoRom_HandlesMirrorBanks() {
		// Bank $80 mirrors bank $00
		int result = SnesTextExtractor.SnesAddressToOffset(0x808000, SnesMapMode.LoRom);
		Assert.Equal(0, result);

		// Bank $81 mirrors bank $01
		result = SnesTextExtractor.SnesAddressToOffset(0x818000, SnesMapMode.LoRom);
		Assert.Equal(0x8000, result);
	}

	[Fact]
	public void SnesAddressToOffset_HiRom_ConvertsBankCorrectly() {
		// Bank $C0, offset $0000 = file offset $0000
		int result = SnesTextExtractor.SnesAddressToOffset(0xc00000, SnesMapMode.HiRom);
		Assert.Equal(0, result);

		// Bank $C1, offset $0000 = file offset $10000
		result = SnesTextExtractor.SnesAddressToOffset(0xc10000, SnesMapMode.HiRom);
		Assert.Equal(0x10000, result);

		// Bank $C0, offset $FFFF = file offset $FFFF
		result = SnesTextExtractor.SnesAddressToOffset(0xc0ffff, SnesMapMode.HiRom);
		Assert.Equal(0xffff, result);
	}

	[Fact]
	public void SnesAddressToOffset_HiRom_HandlesMirrorBanks() {
		// Bank $40-$7D mirror the ROM in HiROM
		int result = SnesTextExtractor.SnesAddressToOffset(0x400000, SnesMapMode.HiRom);
		Assert.Equal(0, result);

		result = SnesTextExtractor.SnesAddressToOffset(0x410000, SnesMapMode.HiRom);
		Assert.Equal(0x10000, result);
	}

	[Fact]
	public void ExtractFromPointerTable_Extracts16BitPointers() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// Create test data with pointer table and text
		// Pointer table at offset 0, pointing to text at offset 0x10
		// Layout: 2 pointers (4 bytes) + 12 bytes padding = 16 bytes, then text at 0x10
		// FF table: A=82, B=83, C=84, D=85, E=86, F=87, G=88, H=89, I=8A, J=8B, K=8C, L=8D, M=8E
		//           N=8F, O=90, P=91, Q=92, R=93, S=94, T=95, U=96, V=97, W=98, X=99, Y=9A, Z=9B
		byte[] data = [
			0x10, 0x00,  // Pointer 0: points to $0010 (offset 16)
			0x16, 0x00,  // Pointer 1: points to $0016 (offset 22)
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 12 bytes padding
			0x89, 0x86, 0x8d, 0x8d, 0x90, 0x00,  // "HELLO" at $0010 (H=89, E=86, L=8D, L=8D, O=90)
			0x98, 0x90, 0x93, 0x8d, 0x85, 0x00   // "WORLD" at $0016 (W=98, O=90, R=93, L=8D, D=85)
		];

		var results = extractor.ExtractFromPointerTable(data, 0, 2, 0, table);

		Assert.Equal(2, results.Count);
		Assert.Equal("HELLO", results[0].Text);
		Assert.Equal("WORLD", results[1].Text);
	}

	[Fact]
	public void ExtractFromLongPointerTable_Extracts24BitPointers() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// Create test data simulating a LoROM setup
		// 24-bit pointer at $8010 bank $00 = file offset $0010
		byte[] data = new byte[0x20];

		// Set up pointer table at offset 0 (3 bytes per pointer)
		data[0] = 0x10;  // Low byte
		data[1] = 0x80;  // Mid byte ($8010)
		data[2] = 0x00;  // Bank ($00)

		// Text at file offset $10
		data[0x10] = 0x89; // H
		data[0x11] = 0x8e; // N (actually 'N' is 8F)
		data[0x12] = 0x00; // End

		var results = extractor.ExtractFromLongPointerTable(data, 0, 1, table, SnesMapMode.LoRom);

		Assert.Single(results);
	}

	[Fact]
	public void ExtractAllText_ScansRomForTextBlocks() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// Text block surrounded by non-text bytes
		byte[] data = [
			0x00, 0x00, 0x00, 0x00,  // Non-text padding
			0x89, 0x86, 0x8d, 0x8d, 0x90, 0x00,  // "HELLO" + terminator
			0x00, 0x00, 0x00, 0x00   // Non-text padding
		];

		var options = new TextExtractionOptions {
			StartOffset = 0,
			EndOffset = data.Length,
			MinLength = 3
		};

		var results = extractor.ExtractAllText(data, table, options);

		Assert.True(results.Count > 0);
	}

	[Fact]
	public void ExtractText_WithControlCodes_IncludesInOutput() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// "A" + [LINE] control code + "B"
		byte[] data = [0x82, 0x01, 0x83, 0x00];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Contains("[LINE]", result);
	}

	[Fact]
	public void ExtractText_WithWaitControl_HandlesCorrectly() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// "HI" + WAIT + END
		byte[] data = [0x89, 0x8a, 0x03, 0x02, 0x00];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Contains("HI", result);
		Assert.Contains("[WAIT]", result);
		Assert.Contains("[END]", result);
	}

	[Fact]
	public void ChronoTriggerTable_HasCharacterMappings() {
		var table = SnesTextExtractor.CreateChronoTriggerTable();

		// Check character name mappings are defined (they're mapped as text, not control codes)
		Assert.Equal("[CRONO]", table.DecodeByte(0x03));
		Assert.Equal("[MARLE]", table.DecodeByte(0x04));
		Assert.Equal("[LUCCA]", table.DecodeByte(0x05));
	}

	[Fact]
	public void DragonQuestTable_HasNameMapping() {
		var table = SnesTextExtractor.CreateDragonQuestTable();

		// Check NAME mapping exists (mapped as text, not control code)
		Assert.Equal("[NAME]", table.DecodeByte(0xf0));
	}

	[Fact]
	public void ExtractText_StopsAtMaxLength() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		// Very long text
		byte[] data = Enumerable.Repeat((byte)0x82, 100).ToArray();

		var result = extractor.ExtractText(data, 0, 5, table);

		// Should stop at maxLength (5 characters)
		Assert.Equal("AAAAA", result);
	}

	[Fact]
	public void ExtractText_FromMiddleOffset_WorksCorrectly() {
		var extractor = new SnesTextExtractor();
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		byte[] data = [0x00, 0x00, 0x00, 0x89, 0x8e, 0x00]; // padding + "HI"

		var result = extractor.ExtractText(data, 3, 10, table);

		Assert.StartsWith("H", result);
	}

	[Fact]
	public void FinalFantasyTable_MapsNumbersCorrectly() {
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		Assert.Equal("0", table.DecodeByte(0xb6));
		Assert.Equal("1", table.DecodeByte(0xb7));
		Assert.Equal("9", table.DecodeByte(0xbf));
	}

	[Fact]
	public void FinalFantasyTable_MapsPunctuationCorrectly() {
		var table = SnesTextExtractor.CreateFinalFantasyTable();

		Assert.Equal("!", table.DecodeByte(0xc0));
		Assert.Equal("?", table.DecodeByte(0xc1));
		Assert.Equal(".", table.DecodeByte(0xc7));
	}
}
