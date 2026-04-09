using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for SmsAnalyzer — mapper detection, bank switching, memory regions, registers.
/// </summary>
[Collection("PlatformResolver")]
public class SmsAnalyzerTests {
	private static IPlatformAnalyzer GetAnalyzer() {
		var profile = PlatformResolver.Resolve("sms")!;
		return profile.Analyzer;
	}

	// ========================================================================
	// Analyze
	// ========================================================================

	[Fact]
	public void Analyze_ValidRom_ReturnsRomInfo() {
		var rom = new byte[0x8000]; // 32KB
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Sega Master System", info.Platform);
		Assert.Equal(rom.Length, info.Size);
	}

	[Fact]
	public void Analyze_CalculatesBankCount() {
		var rom = new byte[0x20000]; // 128KB = 8 banks
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("8", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_DetectsRomSize() {
		var rom = new byte[0x8000]; // 32KB
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("32KB", info.Metadata["RomSize"]);
	}

	// ========================================================================
	// GetEntryPoints
	// ========================================================================

	[Fact]
	public void GetEntryPoints_IncludesResetAndInterrupts() {
		var rom = new byte[0x8000];
		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0x0000u, entries); // Z80 reset
		Assert.Contains(0x0038u, entries); // Maskable interrupt (IM 1)
		Assert.Contains(0x0066u, entries); // NMI (pause button)
	}

	// ========================================================================
	// GetRegisterLabel
	// ========================================================================

	[Theory]
	[InlineData(0xfffcu, "MAPPER_CTRL")]
	[InlineData(0xfffdu, "MAPPER_SLOT0")]
	[InlineData(0xfffeu, "MAPPER_SLOT1")]
	[InlineData(0xffffu, "MAPPER_SLOT2")]
	public void GetRegisterLabel_MapperRegisters_ReturnsName(uint address, string expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetRegisterLabel(address));
	}

	[Fact]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull() {
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.GetRegisterLabel(0x0000));
	}

	// ========================================================================
	// GetMemoryRegion
	// ========================================================================

	[Theory]
	[InlineData(0x0000u, MemoryRegion.Rom)]
	[InlineData(0x3fffu, MemoryRegion.Rom)]
	[InlineData(0x4000u, MemoryRegion.Rom)]
	[InlineData(0xbfffu, MemoryRegion.Rom)]
	public void GetMemoryRegion_RomArea_ReturnsRom(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0xc000u, MemoryRegion.Ram)]
	[InlineData(0xdfffu, MemoryRegion.Ram)]
	[InlineData(0xe000u, MemoryRegion.Ram)]
	public void GetMemoryRegion_RamArea_ReturnsRam(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0xfffcu)]
	[InlineData(0xffffu)]
	public void GetMemoryRegion_MapperRegs_ReturnsHardware(uint address) {
		var analyzer = GetAnalyzer();
		Assert.Equal(MemoryRegion.Hardware, analyzer.GetMemoryRegion(address));
	}

	// ========================================================================
	// AddressToOffset
	// ========================================================================

	[Fact]
	public void AddressToOffset_FirstKb_AlwaysMapsToBank0() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x8000]);

		Assert.Equal(0x000, analyzer.AddressToOffset(0x0000, 0x8000));
		Assert.Equal(0x3ff, analyzer.AddressToOffset(0x03ff, 0x8000));
	}

	[Fact]
	public void AddressToOffset_RamArea_ReturnsNegative() {
		var analyzer = GetAnalyzer();
		Assert.Equal(-1, analyzer.AddressToOffset(0xc000, 0x8000));
	}

	[Fact]
	public void AddressToOffset_Page0_WithBank0_MapsDirect() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x8000]);

		// Page 0 ($0400-$3FFF) with bank 0
		Assert.Equal(0x0400, analyzer.AddressToOffset(0x0400, 0x8000, 0));
	}

	// ========================================================================
	// OffsetToAddress
	// ========================================================================

	[Fact]
	public void OffsetToAddress_FirstBank_MapsDirectly() {
		var analyzer = GetAnalyzer();
		Assert.Equal(0x0000u, analyzer.OffsetToAddress(0));
		Assert.Equal(0x1000u, analyzer.OffsetToAddress(0x1000));
	}

	[Fact]
	public void OffsetToAddress_OtherBank_MapsToPage1() {
		var analyzer = GetAnalyzer();
		// Offset in bank 1 maps to $4000+ range
		var addr = analyzer.OffsetToAddress(0x4000);
		Assert.NotNull(addr);
		Assert.Equal(0x4000u, addr);
	}

	// ========================================================================
	// IsInSwitchableRegion
	// ========================================================================

	[Fact]
	public void IsInSwitchableRegion_First1Kb_Fixed() {
		var analyzer = GetAnalyzer();
		Assert.False(analyzer.IsInSwitchableRegion(0x0000));
		Assert.False(analyzer.IsInSwitchableRegion(0x03ff));
	}

	[Theory]
	[InlineData(0x0400u)]
	[InlineData(0x4000u)]
	[InlineData(0x8000u)]
	[InlineData(0xbfffu)]
	public void IsInSwitchableRegion_RomBanks_Switchable(uint address) {
		var analyzer = GetAnalyzer();
		Assert.True(analyzer.IsInSwitchableRegion(address));
	}

	[Fact]
	public void IsInSwitchableRegion_RamArea_NotSwitchable() {
		var analyzer = GetAnalyzer();
		Assert.False(analyzer.IsInSwitchableRegion(0xc000));
	}

	// ========================================================================
	// DetectBankSwitch
	// ========================================================================

	[Fact]
	public void DetectBankSwitch_LdMappingPattern_Detected() {
		// ld a, 3 (0x3E 0x03); ld ($fffe), a (0x32 0xFE 0xFF)
		var rom = new byte[0x8000];
		rom[0x100] = 0x3e; // ld a, imm8
		rom[0x101] = 0x03; // bank 3
		rom[0x102] = 0x32; // ld (nn), a
		rom[0x103] = 0xfe; // addr low
		rom[0x104] = 0xff; // addr high

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0x100, 0);

		Assert.NotNull(result);
		Assert.Equal(3, result.TargetBank);
	}

	[Fact]
	public void DetectBankSwitch_NoPattern_ReturnsNull() {
		var rom = new byte[0x8000];
		rom[0x100] = 0x00; // NOP
		rom[0x101] = 0x00;

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0x100, 0);

		Assert.Null(result);
	}
}
