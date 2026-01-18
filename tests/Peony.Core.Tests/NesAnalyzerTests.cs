using Peony.Core;
using Peony.Platform.NES;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for NES platform analyzer including mapper detection and register labeling
/// </summary>
public class NesAnalyzerTests {
	private readonly NesAnalyzer _analyzer = new();

	/// <summary>
	/// Creates a minimal valid iNES header for testing
	/// </summary>
	private static byte[] CreateNesRom(int prgBanks = 2, int chrBanks = 1, int mapper = 0) {
		// Calculate total ROM size
		var prgSize = prgBanks * 16384;
		var chrSize = chrBanks * 8192;
		var totalSize = 16 + prgSize + chrSize;  // 16-byte header + PRG + CHR

		var rom = new byte[totalSize];

		// iNES header magic: "NES" + 0x1a
		rom[0] = 0x4e;  // 'N'
		rom[1] = 0x45;  // 'E'
		rom[2] = 0x53;  // 'S'
		rom[3] = 0x1a;

		// PRG ROM banks (16KB each)
		rom[4] = (byte)prgBanks;

		// CHR ROM banks (8KB each)
		rom[5] = (byte)chrBanks;

		// Flags 6: mapper low nibble in high nibble
		rom[6] = (byte)((mapper & 0x0f) << 4);

		// Flags 7: mapper high nibble in high nibble
		rom[7] = (byte)(mapper & 0xf0);

		// Add reset vector pointing to $8000
		var vectorOffset = 16 + prgSize - 6;
		rom[vectorOffset] = 0x00;      // NMI low
		rom[vectorOffset + 1] = 0x80;  // NMI high
		rom[vectorOffset + 2] = 0x00;  // Reset low
		rom[vectorOffset + 3] = 0x80;  // Reset high
		rom[vectorOffset + 4] = 0x00;  // IRQ low
		rom[vectorOffset + 5] = 0x80;  // IRQ high

		return rom;
	}

	[Fact]
	public void Analyze_ValidNesRom_IdentifiesPlatform() {
		var rom = CreateNesRom();

		var info = _analyzer.Analyze(rom);

		Assert.Equal("NES", info.Platform);
	}

	[Fact]
	public void Analyze_NromMapper0_DetectsCorrectly() {
		var rom = CreateNesRom(prgBanks: 2, chrBanks: 1, mapper: 0);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("0", info.Metadata["Mapper"]);
		Assert.Equal("NROM", info.Metadata["MapperName"]);
	}

	[Fact]
	public void Analyze_Mmc1Mapper1_DetectsCorrectly() {
		var rom = CreateNesRom(prgBanks: 8, chrBanks: 0, mapper: 1);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("1", info.Metadata["Mapper"]);
		Assert.Equal("MMC1", info.Metadata["MapperName"]);
	}

	[Fact]
	public void Analyze_Mmc3Mapper4_DetectsCorrectly() {
		var rom = CreateNesRom(prgBanks: 16, chrBanks: 16, mapper: 4);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("4", info.Metadata["Mapper"]);
		Assert.Equal("MMC3", info.Metadata["MapperName"]);
	}

	[Fact]
	public void Analyze_CalculatesPrgSize() {
		var rom = CreateNesRom(prgBanks: 4);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("64K", info.Metadata["PRG"]);  // 4 banks * 16KB
	}

	[Fact]
	public void Analyze_CalculatesChrSize() {
		var rom = CreateNesRom(chrBanks: 4);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("32K", info.Metadata["CHR"]);  // 4 banks * 8KB
	}

	[Fact]
	public void Analyze_NoChr_ShowsRam() {
		var rom = CreateNesRom(chrBanks: 0);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("RAM", info.Metadata["CHR"]);
	}

	[Fact]
	public void GetRegisterLabel_PpuCtrl_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x2000);

		Assert.Equal("PPUCTRL", label);
	}

	[Fact]
	public void GetRegisterLabel_PpuMask_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x2001);

		Assert.Equal("PPUMASK", label);
	}

	[Fact]
	public void GetRegisterLabel_PpuStatus_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x2002);

		Assert.Equal("PPUSTATUS", label);
	}

	[Fact]
	public void GetRegisterLabel_OamDma_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x4014);

		Assert.Equal("OAMDMA", label);
	}

	[Fact]
	public void GetRegisterLabel_PpuMirror_ReturnsCorrectLabel() {
		// PPU registers mirror every 8 bytes from $2000-$3FFF
		var label = _analyzer.GetRegisterLabel(0x2008);  // Mirror of $2000

		Assert.Equal("PPUCTRL", label);
	}

	[Fact]
	public void GetRegisterLabel_ApuPulse1_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x4000);

		Assert.Equal("SQ1_VOL", label);
	}

	[Fact]
	public void GetRegisterLabel_JoyPad_ReturnsCorrectLabel() {
		Assert.Equal("JOY1", _analyzer.GetRegisterLabel(0x4016));
		Assert.Equal("JOY2", _analyzer.GetRegisterLabel(0x4017));
	}

	[Fact]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull() {
		var label = _analyzer.GetRegisterLabel(0x5000);

		Assert.Null(label);
	}

	[Fact]
	public void GetMemoryRegion_ZeroPage_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x0000);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_Stack_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x0100);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_RamMirror_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x0800);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_PpuRegisters_ReturnsHardware() {
		var region = _analyzer.GetMemoryRegion(0x2000);

		Assert.Equal(MemoryRegion.Hardware, region);
	}

	[Fact]
	public void GetMemoryRegion_ApuRegisters_ReturnsHardware() {
		var region = _analyzer.GetMemoryRegion(0x4000);

		Assert.Equal(MemoryRegion.Hardware, region);
	}

	[Fact]
	public void GetMemoryRegion_SRam_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x6000);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_PrgRom_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x8000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetEntryPoints_ReturnsResetVector() {
		var rom = CreateNesRom();

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Contains((uint)0x8000, entries);  // Our test ROM has reset at $8000
	}

	[Fact]
	public void GetEntryPoints_IncludesNmiVector() {
		var rom = CreateNesRom();
		// Set NMI to different address than Reset
		// In our CreateNesRom, vectors are at offset 16 + prgSize - 6
		// For 2 PRG banks (32K), that's 16 + 32768 - 6 = 32778
		// We need to modify the NMI vector (first 2 bytes of the 6)
		var prgSize = 2 * 16384;  // 2 banks
		var vectorOffset = 16 + prgSize - 6;
		rom[vectorOffset] = 0x50;  // NMI low = $50
		rom[vectorOffset + 1] = 0x80;  // NMI high = $80, so $8050

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Contains((uint)0x8050, entries);
	}

	[Fact]
	public void IsInSwitchableRegion_Nrom_AlwaysFalse() {
		var rom = CreateNesRom(mapper: 0);
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0x8000));
		Assert.False(_analyzer.IsInSwitchableRegion(0xc000));
	}

	[Fact]
	public void IsInSwitchableRegion_Mmc1_SwitchableIn8000Range() {
		var rom = CreateNesRom(mapper: 1);
		_analyzer.Analyze(rom);

		Assert.True(_analyzer.IsInSwitchableRegion(0x8000));
		Assert.True(_analyzer.IsInSwitchableRegion(0xbfff));
		Assert.False(_analyzer.IsInSwitchableRegion(0xc000));  // Fixed bank
	}

	[Fact]
	public void AddressToOffset_NromSmall_MapsCorrectly() {
		var rom = CreateNesRom(prgBanks: 1, mapper: 0);  // 16K NROM
		_analyzer.Analyze(rom);

		// For 16K NROM, $8000-$BFFF and $C000-$FFFF both map to the same 16K
		var offset1 = _analyzer.AddressToOffset(0x8000, rom.Length);
		var offset2 = _analyzer.AddressToOffset(0xc000, rom.Length);

		Assert.Equal(16, offset1);  // First PRG byte after header
	}

	[Fact]
	public void AddressToOffset_Nrom32K_MapsCorrectly() {
		var rom = CreateNesRom(prgBanks: 2, mapper: 0);  // 32K NROM
		_analyzer.Analyze(rom);

		var offset8000 = _analyzer.AddressToOffset(0x8000, rom.Length);
		var offsetC000 = _analyzer.AddressToOffset(0xc000, rom.Length);

		Assert.Equal(16, offset8000);         // Start of PRG
		Assert.Equal(16 + 16384, offsetC000); // Second 16K bank
	}

	[Fact]
	public void AddressToOffset_Mmc1_FixedBankMapsToLast() {
		var rom = CreateNesRom(prgBanks: 8, mapper: 1);  // 128K MMC1
		_analyzer.Analyze(rom);

		// $C000-$FFFF should map to the last bank
		var offsetC000 = _analyzer.AddressToOffset(0xc000, rom.Length);
		var expectedOffset = 16 + (7 * 16384);  // Last bank (bank 7)

		Assert.Equal(expectedOffset, offsetC000);
	}

	[Fact]
	public void AddressToOffset_BelowRom_ReturnsNegative() {
		var rom = CreateNesRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x7fff, rom.Length);

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void BankCount_Mmc1_ReturnsCorrectCount() {
		var rom = CreateNesRom(prgBanks: 8, mapper: 1);
		_analyzer.Analyze(rom);

		Assert.Equal(8, _analyzer.BankCount);
	}
}
