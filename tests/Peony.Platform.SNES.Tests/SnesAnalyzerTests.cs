namespace Peony.Platform.SNES.Tests;

using Xunit;

/// <summary>
/// Tests for SNES address mapping, bank detection, and CDL/Pansy integration.
/// Covers fixes from Epic #179.
/// </summary>
public class SnesAnalyzerTests {
	/// <summary>
	/// Creates a minimal LoROM analyzer for testing address mapping.
	/// The header at offset 0x7fc0 is set up for a 512KB LoROM game.
	/// </summary>
	private static SnesAnalyzer CreateLoRomAnalyzer(int romSize = 524288) {
		// Build a minimal ROM with a valid LoROM header at 0x7fc0
		var rom = new byte[romSize];

		// LoROM header at 0x7fc0
		// Map mode byte at 0x7fd5: 0x20 = LoROM
		rom[0x7fd5] = 0x20;
		// ROM size byte at 0x7fd7: 0x0a = 1MB (just needs to be valid)
		rom[0x7fd7] = 0x0a;
		// Checksum complement at 0x7fdc-0x7fdd
		rom[0x7fdc] = 0xff;
		rom[0x7fdd] = 0xff;
		// Checksum at 0x7fde-0x7fdf
		rom[0x7fde] = 0x00;
		rom[0x7fdf] = 0x00;
		// Reset vector at 0x7ffc-0x7ffd: $8000
		rom[0x7ffc] = 0x00;
		rom[0x7ffd] = 0x80;

		var analyzer = new SnesAnalyzer();
		analyzer.Analyze(rom);
		return analyzer;
	}

	// ==========================================================
	// GetTargetBank tests (#184, bank 0 fix)
	// ==========================================================

	[Fact]
	public void GetTargetBank_24BitAddress_ExtractsBank() {
		var analyzer = CreateLoRomAnalyzer();
		// 24-bit address 0x079030 → bank 7
		Assert.Equal(7, analyzer.GetTargetBank(0x079030, 15));
	}

	[Fact]
	public void GetTargetBank_Bank0_24BitAddress_ReturnsZero() {
		var analyzer = CreateLoRomAnalyzer();
		// 24-bit address 0x008000 → bank 0
		Assert.Equal(0, analyzer.GetTargetBank(0x008000, 15));
	}

	[Fact]
	public void GetTargetBank_16BitLoRomRange_ReturnsBank0() {
		var analyzer = CreateLoRomAnalyzer();
		// 16-bit address 0x8000 (LoROM code space) → bank 0
		Assert.Equal(0, analyzer.GetTargetBank(0x8000, 15));
		Assert.Equal(0, analyzer.GetTargetBank(0x9030, 15));
		Assert.Equal(0, analyzer.GetTargetBank(0xffff, 15));
	}

	[Fact]
	public void GetTargetBank_16BitBelowLoRom_ReturnsCurrent() {
		var analyzer = CreateLoRomAnalyzer();
		// 16-bit address below $8000 → returns currentBank
		Assert.Equal(15, analyzer.GetTargetBank(0x7fff, 15));
		Assert.Equal(3, analyzer.GetTargetBank(0x0000, 3));
		Assert.Equal(15, analyzer.GetTargetBank(0x2100, 15));
	}

	[Fact]
	public void GetTargetBank_HighBanks_ExtractsCorrectly() {
		var analyzer = CreateLoRomAnalyzer();
		Assert.Equal(0x0f, analyzer.GetTargetBank(0x0f8000, 0));
		Assert.Equal(0x01, analyzer.GetTargetBank(0x018000, 0));
		Assert.Equal(0x02, analyzer.GetTargetBank(0x02c000, 0));
	}

	// ==========================================================
	// AddressToOffset tests (#180, bank 0 fix)
	// ==========================================================

	[Fact]
	public void AddressToOffset_Bank0_LoRom_ReturnsZero() {
		var analyzer = CreateLoRomAnalyzer();
		// Bank 0, address $8000 → offset 0
		var offset = analyzer.AddressToOffset(0x8000, 524288, 0);
		Assert.Equal(0, offset);
	}

	[Fact]
	public void AddressToOffset_24BitBank0_LoRom_ReturnsZero() {
		var analyzer = CreateLoRomAnalyzer();
		// 24-bit address $00:8000 → offset 0
		var offset = analyzer.AddressToOffset(0x008000, 524288, 0);
		Assert.Equal(0, offset);
	}

	[Fact]
	public void AddressToOffset_Bank7_LoRom() {
		var analyzer = CreateLoRomAnalyzer();
		// Bank 7, address $8000 → offset 0x38000
		var offset = analyzer.AddressToOffset(0x078000, 524288, 7);
		Assert.Equal(0x38000, offset);
	}

	[Fact]
	public void AddressToOffset_24BitBank7_LoRom() {
		var analyzer = CreateLoRomAnalyzer();
		// 24-bit $07:9030 → offset 0x39030
		var offset = analyzer.AddressToOffset(0x079030, 524288, 7);
		Assert.Equal(0x39030, offset);
	}

	[Fact]
	public void AddressToOffset_Bank15_LoRom() {
		var analyzer = CreateLoRomAnalyzer();
		// Bank 15, $0F:8000 → offset 0x78000
		var offset = analyzer.AddressToOffset(0x0f8000, 524288, 15);
		Assert.Equal(0x78000, offset);
	}

	// ==========================================================
	// OffsetToAddress tests (roundtrip)
	// ==========================================================

	[Fact]
	public void OffsetToAddress_Offset0_LoRom_ReturnsBank0() {
		var analyzer = CreateLoRomAnalyzer();
		// Offset 0 → $00:8000
		var addr = analyzer.OffsetToAddress(0);
		Assert.NotNull(addr);
		Assert.Equal(0x008000u, addr!.Value);
	}

	[Fact]
	public void OffsetToAddress_Bank7_LoRom() {
		var analyzer = CreateLoRomAnalyzer();
		// Offset 0x39030 → $07:9030
		var addr = analyzer.OffsetToAddress(0x39030);
		Assert.NotNull(addr);
		Assert.Equal(0x079030u, addr!.Value);
	}

	[Fact]
	public void OffsetToAddress_Bank15_LoRom() {
		var analyzer = CreateLoRomAnalyzer();
		// Offset 0x78000 → $0F:8000
		var addr = analyzer.OffsetToAddress(0x78000);
		Assert.NotNull(addr);
		Assert.Equal(0x0f8000u, addr!.Value);
	}

	// ==========================================================
	// Roundtrip: offset → address → offset
	// ==========================================================

	[Theory]
	[InlineData(0x00000)]  // Bank 0 start
	[InlineData(0x01030)]  // Bank 0 interior
	[InlineData(0x08000)]  // Bank 1 start
	[InlineData(0x38000)]  // Bank 7 start
	[InlineData(0x39030)]  // Bank 7 interior
	[InlineData(0x58000)]  // Bank 11 start
	[InlineData(0x78000)]  // Bank 15 start
	[InlineData(0x7ffff)]  // Last byte
	public void OffsetToAddress_Then_AddressToOffset_Roundtrips(int originalOffset) {
		var analyzer = CreateLoRomAnalyzer();
		var address = analyzer.OffsetToAddress(originalOffset);
		Assert.NotNull(address);

		var bank = (int)(address!.Value >> 16);
		var recoveredOffset = analyzer.AddressToOffset(address.Value, 524288, bank);
		Assert.Equal(originalOffset, recoveredOffset);
	}

	// ==========================================================
	// GetTargetBank + OffsetToAddress consistency
	// ==========================================================

	[Theory]
	[InlineData(0, 0)]      // Bank 0
	[InlineData(0x08000, 1)]  // Bank 1
	[InlineData(0x38000, 7)]  // Bank 7
	[InlineData(0x78000, 15)] // Bank 15
	public void GetTargetBank_Consistent_With_OffsetToAddress(int offset, int expectedBank) {
		var analyzer = CreateLoRomAnalyzer();
		var address = analyzer.OffsetToAddress(offset);
		Assert.NotNull(address);

		var bank = analyzer.GetTargetBank(address!.Value, 99);
		Assert.Equal(expectedBank, bank);
	}
}
