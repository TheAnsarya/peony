using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for WonderSwanAnalyzer — ROM footer, I/O registers, bank switching, address mapping.
/// </summary>
[Collection("PlatformResolver")]
public class WonderSwanAnalyzerTests {
	private static IPlatformAnalyzer GetAnalyzer() {
		var profile = PlatformResolver.Resolve("wonderswan")!;
		return profile.Analyzer;
	}

	// ========================================================================
	// Analyze
	// ========================================================================

	[Fact]
	public void Analyze_ValidRom_ReturnsRomInfo() {
		var rom = CreateWonderSwanRom();
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("WonderSwan", info.Platform);
		Assert.Equal(rom.Length, info.Size);
	}

	[Fact]
	public void Analyze_ReadsFooterPublisherId() {
		var rom = CreateWonderSwanRom(publisherId: 0x01);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("$01", info.Metadata["PublisherID"]);
	}

	[Fact]
	public void Analyze_DetectsColorMode() {
		var rom = CreateWonderSwanRom(isColor: true);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["Color"]);
		Assert.Equal("WonderSwan Color", info.Metadata["Platform"]);
	}

	[Fact]
	public void Analyze_DetectsMonochrome() {
		var rom = CreateWonderSwanRom(isColor: false);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("No", info.Metadata["Color"]);
		Assert.Equal("WonderSwan", info.Metadata["Platform"]);
	}

	[Fact]
	public void Analyze_ReadsOrientation() {
		var rom = CreateWonderSwanRom(vertical: true);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("Vertical", info.Metadata["Orientation"]);
	}

	[Fact]
	public void Analyze_ReadsSramSize() {
		var rom = CreateWonderSwanRom(sramByte: 0x02);
		var analyzer = GetAnalyzer();
		var info = analyzer.Analyze(rom);

		Assert.Equal("256Kbit (32KB)", info.Metadata["SramSize"]);
	}

	// ========================================================================
	// GetEntryPoints
	// ========================================================================

	[Fact]
	public void GetEntryPoints_ReturnsResetVector() {
		var rom = CreateWonderSwanRom();
		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var entries = analyzer.GetEntryPoints(rom);

		Assert.Contains(0xffff0u, entries);
	}

	// ========================================================================
	// GetRegisterLabel
	// ========================================================================

	[Theory]
	[InlineData(0x00u, "DISP_CTRL")]
	[InlineData(0x01u, "BACK_COLOR")]
	[InlineData(0x40u, "DMA_SRC_L")]
	[InlineData(0x80u, "SND_CH1_PITCH_L")]
	[InlineData(0x90u, "SND_CTRL")]
	[InlineData(0xa0u, "HWTYPE")]
	[InlineData(0xb0u, "INT_BASE")]
	[InlineData(0xb2u, "INT_ENABLE")]
	[InlineData(0xb5u, "KEYPAD")]
	[InlineData(0xc0u, "BANK_ROM2")]
	[InlineData(0xc1u, "BANK_SRAM")]
	[InlineData(0xc2u, "BANK_ROM0")]
	[InlineData(0xc3u, "BANK_ROM1")]
	public void GetRegisterLabel_KnownRegisters_ReturnsName(uint address, string expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetRegisterLabel(address));
	}

	[Fact]
	public void GetRegisterLabel_UnknownPort_ReturnsNull() {
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.GetRegisterLabel(0xf0));
	}

	// ========================================================================
	// GetMemoryRegion
	// ========================================================================

	[Theory]
	[InlineData(0x00000u, MemoryRegion.Ram)]
	[InlineData(0x03fffu, MemoryRegion.Ram)]
	[InlineData(0x10000u, MemoryRegion.Ram)]   // SRAM
	[InlineData(0x1ffffu, MemoryRegion.Ram)]
	public void GetMemoryRegion_RamArea_ReturnsRam(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x20000u, MemoryRegion.Rom)]
	[InlineData(0x40000u, MemoryRegion.Rom)]
	[InlineData(0xfffffu, MemoryRegion.Rom)]
	public void GetMemoryRegion_RomArea_ReturnsRom(uint address, MemoryRegion expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.GetMemoryRegion(address));
	}

	// ========================================================================
	// AddressToOffset
	// ========================================================================

	[Fact]
	public void AddressToOffset_LinearRom_MapsToEndOfRom() {
		var romLength = 0x80000; // 512KB
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[romLength]);

		// $FFFFF should map to last byte of ROM
		var offset = analyzer.AddressToOffset(0xfffff, romLength);
		Assert.Equal(romLength - 1, offset);
	}

	[Fact]
	public void AddressToOffset_LinearRom_HighAddress() {
		var romLength = 0x80000; // 512KB
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[romLength]);

		// $FFFF0 (reset vector) should map to near-end of ROM
		var offset = analyzer.AddressToOffset(0xffff0, romLength);
		Assert.True(offset >= 0 && offset < romLength);
	}

	[Fact]
	public void AddressToOffset_RamArea_ReturnsNegative() {
		var analyzer = GetAnalyzer();
		analyzer.Analyze(new byte[0x80000]);
		Assert.Equal(-1, analyzer.AddressToOffset(0x0000, 0x80000));
	}

	// ========================================================================
	// IsInSwitchableRegion
	// ========================================================================

	[Theory]
	[InlineData(0x20000u, true)]
	[InlineData(0x2ffffu, true)]
	[InlineData(0x30000u, true)]
	[InlineData(0x3ffffu, true)]
	public void IsInSwitchableRegion_SwitchableBanks_True(uint address, bool expected) {
		var analyzer = GetAnalyzer();
		Assert.Equal(expected, analyzer.IsInSwitchableRegion(address));
	}

	[Theory]
	[InlineData(0x00000u)]
	[InlineData(0x10000u)]
	[InlineData(0x40000u)]
	[InlineData(0xfffffu)]
	public void IsInSwitchableRegion_FixedAreas_False(uint address) {
		var analyzer = GetAnalyzer();
		Assert.False(analyzer.IsInSwitchableRegion(address));
	}

	// ========================================================================
	// DetectBankSwitch
	// ========================================================================

	[Fact]
	public void DetectBankSwitch_MovOutPattern_Detected() {
		// mov al, $05 (0xB0 0x05); out $C0, al (0xE6 0xC0)
		var rom = new byte[0x80000]; // 512KB maps to $80000-$FFFFF

		// Address $F0000 → offset = 0x80000 - (0x100000 - 0xF0000) = 0x70000
		var offset = 0x70000;
		rom[offset] = 0xb0;     // mov al, imm8
		rom[offset + 1] = 0x05; // bank 5
		rom[offset + 2] = 0xe6; // out port, al
		rom[offset + 3] = 0xc0; // port $C0 (BANK_ROM2)

		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0xf0000, 0);

		Assert.NotNull(result);
		Assert.Equal(5, result.TargetBank);
	}

	[Fact]
	public void DetectBankSwitch_NoPattern_ReturnsNull() {
		var rom = new byte[0x80000];
		var analyzer = GetAnalyzer();
		analyzer.Analyze(rom);
		var result = analyzer.DetectBankSwitch(rom, 0x40000, 0);

		Assert.Null(result);
	}

	// ========================================================================
	// OffsetToAddress
	// ========================================================================

	[Fact]
	public void OffsetToAddress_ReturnsNull() {
		// WonderSwan needs ROM length to compute, so returns null
		var analyzer = GetAnalyzer();
		Assert.Null(analyzer.OffsetToAddress(0));
	}

	// ========================================================================
	// Helpers
	// ========================================================================

	private static byte[] CreateWonderSwanRom(
		byte publisherId = 0x00,
		bool isColor = false,
		byte sramByte = 0x00,
		bool vertical = false) {
		var rom = new byte[0x20000]; // 128KB

		// Footer is last 16 bytes
		var footerStart = rom.Length - 16;
		rom[footerStart + 0] = publisherId;
		rom[footerStart + 1] = isColor ? (byte)1 : (byte)0;
		rom[footerStart + 2] = 0x01; // Game ID
		rom[footerStart + 3] = 0x00; // Version
		rom[footerStart + 4] = 0x02; // ROM size code
		rom[footerStart + 5] = sramByte;
		rom[footerStart + 6] = (byte)(vertical ? 1 : 0); // Flags: orientation

		return rom;
	}
}
