using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for PceAnalyzer — copier header detection, vectors, MPR mapping, bank switching.
/// </summary>
[Collection("PlatformResolver")]
public class PceAnalyzerTests {
	private static IPlatformAnalyzer GetAnalyzer() {
		var profile = PlatformResolver.Resolve("pce")!;
		return profile.Analyzer;
	}

	// ========================================================================
	// Analyze
	// ========================================================================

	[Fact]
	public void Analyze_RawRom_NoHeaderOffset() {
		// Raw ROM: size is multiple of 8KB (no 512-byte header)
		var rom = new byte[0x40000]; // 256KB
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("PC Engine", info.Platform);
		Assert.Equal("Raw", info.Metadata["Format"]);
		Assert.Equal(0, analyzer.RomDataOffset);
	}

	[Fact]
	public void Analyze_HeaderedRom_DetectsHeader() {
		// Headered ROM: size % 8KB == 512
		var rom = new byte[0x40000 + 512]; // 256KB + 512 byte header
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Headered", info.Metadata["Format"]);
		Assert.Equal(512, analyzer.RomDataOffset);
	}

	[Fact]
	public void Analyze_SmallRom_HuCard() {
		var rom = new byte[0x40000]; // 256KB
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("HuCard", info.Metadata["Type"]);
	}

	[Fact]
	public void Analyze_LargeRom_CdRom() {
		var rom = new byte[0x200000]; // 2MB
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("CD-ROM", info.Metadata["Type"]);
	}

	// ========================================================================
	// GetEntryPoints
	// ========================================================================

	[Fact]
	public void GetEntryPoints_ReadsVectorsFromLastPage() {
		var rom = new byte[0x2000]; // 8KB = 1 page
		// Set RESET vector at $FFFE-$FFFF (last 2 bytes of page)
		rom[0x1ffe] = 0x00; // low byte
		rom[0x1fff] = 0xe0; // high byte → $e000

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0xe000u, entries);
	}

	[Fact]
	public void GetEntryPoints_EmptyRom_FallbackToDefault() {
		var rom = new byte[4]; // Too small for vectors
		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0xe000u, entries); // Default reset vector
	}

	// ========================================================================
	// GetRegisterLabel
	// ========================================================================

	[Theory]
	[InlineData(0x0000u, "VDC_STATUS")]
	[InlineData(0x0400u, "VCE_CTRL")]
	[InlineData(0x0800u, "PSG_SELECT")]
	[InlineData(0x0c00u, "TIMER_COUNT")]
	[InlineData(0x0c01u, "TIMER_CTRL")]
	[InlineData(0x1000u, "IO_PORT")]
	public void GetRegisterLabel_KnownRegisters_ReturnsName(uint address, string expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetRegisterLabel(address));
	}

	[Fact]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull() {
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.GetRegisterLabel(0x5000));
	}

	// ========================================================================
	// GetMemoryRegion
	// ========================================================================

	[Theory]
	[InlineData(0x0000u, MemoryRegion.Hardware)]
	[InlineData(0x1fffu, MemoryRegion.Hardware)]
	public void GetMemoryRegion_IoArea_ReturnsHardware(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x2000u, MemoryRegion.Ram)]
	[InlineData(0x3fffu, MemoryRegion.Ram)]
	public void GetMemoryRegion_WorkRam_ReturnsRam(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x4000u, MemoryRegion.Rom)]
	[InlineData(0xe000u, MemoryRegion.Rom)]
	[InlineData(0xffffu, MemoryRegion.Rom)]
	public void GetMemoryRegion_RomPages_ReturnsRom(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	// ========================================================================
	// AddressToOffset
	// ========================================================================

	[Fact]
	public void AddressToOffset_IoArea_ReturnsNegative() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x8000]);
		Assert.Equal(-1, analyzer.AddressToOffset(0x2000, 0x8000));
	}

	[Fact]
	public void AddressToOffset_RomArea_ReturnsValid() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x40000]);
		var offset = analyzer.AddressToOffset(0x4000, 0x40000, 0);
		Assert.True(offset >= 0);
	}

	// ========================================================================
	// OffsetToAddress
	// ========================================================================

	[Fact]
	public void OffsetToAddress_MapsToLastPage() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x2000]);
		var addr = analyzer.OffsetToAddress(0);
		Assert.NotNull(addr);
		// Should map to $E000-$FFFF range
		Assert.True(addr >= 0xe000u);
	}

	// ========================================================================
	// IsInSwitchableRegion
	// ========================================================================

	[Fact]
	public void IsInSwitchableRegion_IoAndRam_NotSwitchable() {
		var analyzer = GetAnalyzer();
		Assert.False(analyzer.IsInSwitchableRegion(0x0000));
		Assert.False(analyzer.IsInSwitchableRegion(0x2000));
		Assert.False(analyzer.IsInSwitchableRegion(0x3fff));
	}

	[Theory]
	[InlineData(0x4000u)]
	[InlineData(0x8000u)]
	[InlineData(0xe000u)]
	[InlineData(0xffffu)]
	public void IsInSwitchableRegion_RomPages_Switchable(uint address) {
		var analyzer = GetAnalyzer();
		Assert.True(analyzer.IsInSwitchableRegion(address));
	}

	// ========================================================================
	// DetectBankSwitch
	// ========================================================================

	[Fact]
	public void DetectBankSwitch_TamPattern_Detected() {
		// lda #$05 (0xA9 0x05); tam #$04 (0x53 0x04)
		// Address $4000 with bank 0 maps to offset = 0 * 0x2000 + ($4000 % $2000) + 0 = 0
		var rom = new byte[0x8000];
		rom[0] = 0xa9; // lda #imm8
		rom[1] = 0x05; // bank 5
		rom[2] = 0x53; // tam
		rom[3] = 0x04; // bit mask

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0x4000, 0);

		Assert.NotNull(result);
		Assert.Equal(5, result.TargetBank);
	}

	[Fact]
	public void DetectBankSwitch_NoPattern_ReturnsNull() {
		var rom = new byte[0x8000];
		rom[0x4000] = 0xea; // NOP (6502)

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0x4000, 0);

		Assert.Null(result);
	}
}
