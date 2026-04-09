using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for GenesisAnalyzer — header parsing, vectors, memory regions, registers.
/// </summary>
[Collection("PlatformResolver")]
public class GenesisAnalyzerTests {
	private static IPlatformAnalyzer GetAnalyzer() {
		var profile = PlatformResolver.Resolve("genesis")!;
		return profile.Analyzer;
	}

	// ========================================================================
	// Analyze
	// ========================================================================

	[Fact]
	public void Analyze_ValidRom_ReturnsRomInfo() {
		var rom = CreateGenesisRom();
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Sega Genesis", info.Platform);
		Assert.Equal(rom.Length, info.Size);
	}

	[Fact]
	public void Analyze_WithSegaHeader_DetectsValidHeader() {
		var rom = CreateGenesisRom(withSegaHeader: true);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["ValidHeader"]);
	}

	[Fact]
	public void Analyze_WithoutSegaHeader_DetectsInvalidHeader() {
		var rom = new byte[0x200];
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("No", info.Metadata["ValidHeader"]);
	}

	[Fact]
	public void Analyze_LargeRom_DetectsSsfiiMapper() {
		var rom = new byte[0x500000]; // 5MB > 4MB threshold
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("SSFII", info.Metadata["Mapper"]);
	}

	// ========================================================================
	// GetEntryPoints
	// ========================================================================

	[Fact]
	public void GetEntryPoints_ReadsInitialPcFromVector() {
		var rom = CreateGenesisRom();

		// Set initial PC (vector 1) at offset $04
		rom[4] = 0x00;
		rom[5] = 0x00;
		rom[6] = 0x02;
		rom[7] = 0x00; // $000200

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0x000200u, entries);
	}

	[Fact]
	public void GetEntryPoints_IncludesExceptionVectors() {
		var rom = CreateGenesisRom();

		// Set initial PC at $04
		rom[4] = 0x00; rom[5] = 0x00; rom[6] = 0x02; rom[7] = 0x00;

		// Set V-blank vector ($78) to $001000
		rom[0x78] = 0x00; rom[0x79] = 0x00; rom[0x7a] = 0x10; rom[0x7b] = 0x00;

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0x000200u, entries);
		Assert.Contains(0x001000u, entries);
	}

	// ========================================================================
	// GetRegisterLabel
	// ========================================================================

	[Theory]
	[InlineData(0xa10001u, "VERSION")]
	[InlineData(0xa10003u, "CTRL1_DATA")]
	[InlineData(0xa11100u, "Z80_BUSREQ")]
	[InlineData(0xa11200u, "Z80_RESET")]
	[InlineData(0xc00000u, "VDP_DATA")]
	[InlineData(0xc00004u, "VDP_CTRL")]
	[InlineData(0xc00011u, "PSG")]
	public void GetRegisterLabel_KnownRegisters_ReturnsName(uint address, string expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetRegisterLabel(address));
	}

	[Theory]
	[InlineData(0x000000u)]
	[InlineData(0x100000u)]
	[InlineData(0xe00000u)]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull(uint address) {
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.GetRegisterLabel(address));
	}

	// ========================================================================
	// GetMemoryRegion
	// ========================================================================

	[Theory]
	[InlineData(0x000000u, MemoryRegion.Rom)]
	[InlineData(0x200000u, MemoryRegion.Rom)]
	[InlineData(0x3fffffu, MemoryRegion.Rom)]
	public void GetMemoryRegion_RomArea_ReturnsRom(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0xa10001u)]
	[InlineData(0xc00000u)]
	[InlineData(0xc00004u)]
	public void GetMemoryRegion_HardwareArea_ReturnsHardware(uint address) {
		var analyzer = GetAnalyzer();
		Assert.Equal(MemoryRegion.Hardware, analyzer.GetMemoryRegion(address));
	}

	[Fact]
	public void GetMemoryRegion_WorkRam_ReturnsRam() {
		var analyzer = GetAnalyzer();
		Assert.Equal(MemoryRegion.Ram, analyzer.GetMemoryRegion(0xe00000));
	}

	// ========================================================================
	// AddressToOffset
	// ========================================================================

	[Theory]
	[InlineData(0x000000u, 0)]
	[InlineData(0x000100u, 0x100)]
	[InlineData(0x100000u, 0x100000)]
	public void AddressToOffset_RomRange_ReturnsDirect(uint address, int expected) {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x200000]);
		Assert.Equal(expected, analyzer.AddressToOffset(address, 0x200000));
	}

	[Fact]
	public void AddressToOffset_OutsideRom_ReturnsNegative() {
		var analyzer = GetAnalyzer();
		Assert.Equal(-1, analyzer.AddressToOffset(0x400000, 0x200000));
	}

	// ========================================================================
	// OffsetToAddress
	// ========================================================================

	[Theory]
	[InlineData(0, 0x000000u)]
	[InlineData(0x100, 0x000100u)]
	public void OffsetToAddress_DirectMapping(int offset, uint expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.OffsetToAddress(offset));
	}

	// ========================================================================
	// IsInSwitchableRegion / DetectBankSwitch
	// ========================================================================

	[Fact]
	public void IsInSwitchableRegion_AlwaysFalse() {
		var analyzer = GetAnalyzer();
		Assert.False(analyzer.IsInSwitchableRegion(0x000000));
		Assert.False(analyzer.IsInSwitchableRegion(0x200000));
	}

	[Fact]
	public void DetectBankSwitch_ReturnsNull() {
		var rom = new byte[0x200];
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.DetectBankSwitch(rom, 0, 0));
	}

	// ========================================================================
	// Helpers
	// ========================================================================

	private static byte[] CreateGenesisRom(bool withSegaHeader = false) {
		var rom = new byte[0x80000]; // 512KB

		if (withSegaHeader) {
			// "SEGA" at $100
			rom[0x100] = (byte)'S';
			rom[0x101] = (byte)'E';
			rom[0x102] = (byte)'G';
			rom[0x103] = (byte)'A';
		}

		// Initial SSP at $000000 = $00FFFE
		rom[0] = 0x00; rom[1] = 0x00; rom[2] = 0xff; rom[3] = 0xfe;
		// Initial PC at $000004 = $000200
		rom[4] = 0x00; rom[5] = 0x00; rom[6] = 0x02; rom[7] = 0x00;

		return rom;
	}
}
