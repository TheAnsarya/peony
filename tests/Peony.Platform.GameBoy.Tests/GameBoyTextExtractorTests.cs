namespace Peony.Platform.GameBoy.Tests;

using Peony.Core;
using Xunit;

public class GameBoyTextExtractorTests {
	[Fact]
	public void ExtractText_WithPokemonTable_DecodesCorrectly() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// "HELLO" in Pokemon style: H=87, E=84, L=8B, L=8B, O=8E, end=50
		byte[] data = [0x87, 0x84, 0x8b, 0x8b, 0x8e, 0x50];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("HELLO", result);
	}

	[Fact]
	public void ExtractText_WithDragonWarriorTable_DecodesCorrectly() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreateDragonWarriorGbTable();

		// "HERO" in DW style: H=07, E=04, R=11, O=0E, end=FF
		byte[] data = [0x07, 0x04, 0x11, 0x0e, 0xff];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("HERO", result);
	}

	[Fact]
	public void ExtractText_WithZeldaTable_DecodesAsciiLike() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreateZeldaTable();

		// ASCII-like: "Link" = L=4C, i=69, n=6E, k=6B
		byte[] data = [0x4c, 0x69, 0x6e, 0x6b, 0xff];

		var result = extractor.ExtractText(data, 0, 10, table);

		Assert.Equal("Link", result);
	}

	[Fact]
	public void CreatePokemonTable_HasCorrectMappings() {
		var table = GameBoyTextExtractor.CreatePokemonTable();

		Assert.Equal("A", table.DecodeByte(0x80));
		Assert.Equal("Z", table.DecodeByte(0x99));
		Assert.Equal("a", table.DecodeByte(0xa0));
		Assert.Equal("0", table.DecodeByte(0xf6));
		Assert.Equal((byte)0x50, table.EndByte);
	}

	[Fact]
	public void CreateDragonWarriorTable_HasCorrectEndByte() {
		var table = GameBoyTextExtractor.CreateDragonWarriorGbTable();

		Assert.Equal((byte)0xff, table.EndByte);
		Assert.Equal("A", table.DecodeByte(0x00));
	}

	[Fact]
	public void CreateZeldaTable_HasAsciiMapping() {
		var table = GameBoyTextExtractor.CreateZeldaTable();

		Assert.Equal("A", table.DecodeByte(0x41));
		Assert.Equal("Z", table.DecodeByte(0x5a));
		Assert.Equal("a", table.DecodeByte(0x61));
	}

	[Fact]
	public void GameBoyAddressToOffset_Bank0_ReturnsAddress() {
		// Bank 0: $0000-$3FFF is direct
		int result = GameBoyTextExtractor.GameBoyAddressToOffset(0x1000, 0);
		Assert.Equal(0x1000, result);

		result = GameBoyTextExtractor.GameBoyAddressToOffset(0x0000, 0);
		Assert.Equal(0, result);

		result = GameBoyTextExtractor.GameBoyAddressToOffset(0x3fff, 0);
		Assert.Equal(0x3fff, result);
	}

	[Fact]
	public void GameBoyAddressToOffset_Bank1_CalculatesCorrectly() {
		// Bank 1: $4000-$7FFF maps to file offset $4000
		int result = GameBoyTextExtractor.GameBoyAddressToOffset(0x4000, 1);
		Assert.Equal(0x4000, result);

		result = GameBoyTextExtractor.GameBoyAddressToOffset(0x7fff, 1);
		Assert.Equal(0x7fff, result);
	}

	[Fact]
	public void GameBoyAddressToOffset_Bank2_CalculatesCorrectly() {
		// Bank 2: $4000-$7FFF maps to file offset $8000
		int result = GameBoyTextExtractor.GameBoyAddressToOffset(0x4000, 2);
		Assert.Equal(0x8000, result);

		result = GameBoyTextExtractor.GameBoyAddressToOffset(0x5000, 2);
		Assert.Equal(0x9000, result);
	}

	[Fact]
	public void GameBoyAddressToOffset_HigherBanks_CalculatesCorrectly() {
		// Bank 10: $4000 maps to file offset $28000
		int result = GameBoyTextExtractor.GameBoyAddressToOffset(0x4000, 10);
		Assert.Equal(0x28000, result);

		// Bank 127: $4000 maps to file offset $1FC000
		result = GameBoyTextExtractor.GameBoyAddressToOffset(0x4000, 127);
		Assert.Equal(0x1fc000, result);
	}

	[Fact]
	public void GameBoyAddressToOffset_InvalidAddress_ReturnsNegative() {
		// Address above $7FFF (RAM/IO area)
		int result = GameBoyTextExtractor.GameBoyAddressToOffset(0x8000, 1);
		Assert.Equal(-1, result);
	}

	[Fact]
	public void OffsetToGameBoyAddress_Bank0_ReturnsCorrectly() {
		var (address, bank) = GameBoyTextExtractor.OffsetToGameBoyAddress(0x1000);
		Assert.Equal(0x1000, address);
		Assert.Equal(0, bank);
	}

	[Fact]
	public void OffsetToGameBoyAddress_Bank1_ReturnsCorrectly() {
		var (address, bank) = GameBoyTextExtractor.OffsetToGameBoyAddress(0x5000);
		Assert.Equal(0x5000, address);
		Assert.Equal(1, bank);
	}

	[Fact]
	public void OffsetToGameBoyAddress_HigherBank_ReturnsCorrectly() {
		// Offset $28000 = bank 10, address $4000
		var (address, bank) = GameBoyTextExtractor.OffsetToGameBoyAddress(0x28000);
		Assert.Equal(0x4000, address);
		Assert.Equal(10, bank);
	}

	[Fact]
	public void ExtractFromPointerTable_Extracts16BitPointers() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// Create test data with pointer table at offset 0
		// Pointers point to text at offset 0x10 and 0x16
		// Pokemon: H=87, E=84, L=8B, L=8B, O=8E, W=96, O=8E, R=91, L=8B, D=83
		byte[] data = [
			0x10, 0x00,  // Pointer 0: points to $0010
			0x16, 0x00,  // Pointer 1: points to $0016
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // padding
			0x87, 0x84, 0x8b, 0x8b, 0x8e, 0x50,  // "HELLO" at $0010
			0x96, 0x8e, 0x91, 0x8b, 0x83, 0x50   // "WORLD" at $0016
		];

		var results = extractor.ExtractFromPointerTable(data, 0, 2, 0, table);

		Assert.Equal(2, results.Count);
		Assert.Equal("HELLO", results[0].Text);
		Assert.Equal("WORLD", results[1].Text);
	}

	[Fact]
	public void ExtractFromBankedPointers_ExtractsFromCorrectBank() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// Create test data - simulating bank 0 with pointer table
		// Pointer at $4000 in bank 1 = file offset $4000
		byte[] data = new byte[0x4010];

		// Pointer table at offset 0
		data[0] = 0x00;  // Low byte
		data[1] = 0x40;  // High byte ($4000)

		// Text at file offset $4000 (bank 1, address $4000)
		data[0x4000] = 0x87;  // H
		data[0x4001] = 0x88;  // I
		data[0x4002] = 0x50;  // END

		var results = extractor.ExtractFromBankedPointers(data, 0, 1, 1, table);

		Assert.Single(results);
		Assert.Equal("HI", results[0].Text);
		Assert.Equal(0x4000, results[0].Offset);
	}

	[Fact]
	public void ExtractAllText_ScansRomForTextBlocks() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// Text block surrounded by non-text bytes
		byte[] data = [
			0x00, 0x00, 0x00, 0x00,  // Non-text padding
			0x87, 0x84, 0x8b, 0x8b, 0x8e, 0x50,  // "HELLO" + end
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
	public void PokemonTable_HasSpecialControls() {
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// Check Pokemon-specific control codes are mapped
		Assert.Equal("[LINE]", table.DecodeByte(0x4f));
		Assert.Equal("[PLAYER]", table.DecodeByte(0x52));
		Assert.Equal("[RIVAL]", table.DecodeByte(0x53));
	}

	[Fact]
	public void PokemonTable_MapsNumbersCorrectly() {
		var table = GameBoyTextExtractor.CreatePokemonTable();

		Assert.Equal("0", table.DecodeByte(0xf6));
		Assert.Equal("1", table.DecodeByte(0xf7));
		Assert.Equal("9", table.DecodeByte(0xff));
	}

	[Fact]
	public void ExtractText_StopsAtMaxLength() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		// Very long text (all A's)
		byte[] data = Enumerable.Repeat((byte)0x80, 100).ToArray();

		var result = extractor.ExtractText(data, 0, 5, table);

		Assert.Equal("AAAAA", result);
	}

	[Fact]
	public void ExtractText_FromMiddleOffset_WorksCorrectly() {
		var extractor = new GameBoyTextExtractor();
		var table = GameBoyTextExtractor.CreatePokemonTable();

		byte[] data = [0x00, 0x00, 0x00, 0x87, 0x88, 0x50]; // padding + "HI"

		var result = extractor.ExtractText(data, 3, 10, table);

		Assert.Equal("HI", result);
	}

	[Fact]
	public void ZeldaTable_HasControlCodes() {
		var table = GameBoyTextExtractor.CreateZeldaTable();

		Assert.Equal("[LINE]", table.DecodeByte(0xf0));
		Assert.Equal("[END]", table.DecodeByte(0xff));
	}
}
