using Peony.Core;
using Peony.Platform.GameBoy;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for Game Boy platform analyzer including MBC detection and register labeling
/// </summary>
public class GameBoyAnalyzerTests {
	private readonly GameBoyAnalyzer _analyzer = new();

	/// <summary>
	/// Creates a minimal valid Game Boy ROM header for testing
	/// </summary>
	private static byte[] CreateGameBoyRom(
		byte mbcType = 0x00,
		byte romSize = 0,
		byte ramSize = 0,
		string title = "TEST",
		bool cgb = false) {
		// Minimum size: 0x150 bytes for complete header
		// Use enough for at least 2 banks to test banking
		var size = Math.Max(0x8000, (2 << romSize) * 0x4000);
		var rom = new byte[size];

		// Nintendo logo (required at $0104-$0133)
		// Just fill with placeholder for tests
		for (int i = 0x104; i < 0x134; i++) {
			rom[i] = 0xce;  // Placeholder
		}

		// Title at $0134-$0143
		var titleBytes = System.Text.Encoding.ASCII.GetBytes(title);
		for (int i = 0; i < Math.Min(titleBytes.Length, 16); i++) {
			rom[0x134 + i] = titleBytes[i];
		}

		// CGB flag at $0143
		rom[0x143] = cgb ? (byte)0x80 : (byte)0x00;

		// Cartridge type at $0147
		rom[0x147] = mbcType;

		// ROM size at $0148
		rom[0x148] = romSize;

		// RAM size at $0149
		rom[0x149] = ramSize;

		// Entry point at $0100
		rom[0x100] = 0x00;  // NOP
		rom[0x101] = 0xc3;  // JP
		rom[0x102] = 0x50;  // $0150
		rom[0x103] = 0x01;

		return rom;
	}

	[Fact]
	public void Analyze_ValidGameBoyRom_IdentifiesPlatform() {
		var rom = CreateGameBoyRom();

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Game Boy", info.Platform);
	}

	[Fact]
	public void Analyze_NoMbc_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x00);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("None", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_Mbc1_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x01);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("MBC1", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_Mbc1Ram_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x02);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("MBC1+RAM", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_Mbc1RamBattery_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x03);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("MBC1+RAM+BATTERY", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_Mbc3_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x11);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("MBC3", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_Mbc5_DetectsCorrectly() {
		var rom = CreateGameBoyRom(mbcType: 0x19);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("MBC5", info.Metadata["MBC"]);
	}

	[Fact]
	public void Analyze_ReadsTitle() {
		var rom = CreateGameBoyRom(title: "POKEMON");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("POKEMON", info.Metadata["Title"]);
	}

	[Fact]
	public void Analyze_DetectsCgbMode() {
		var rom = CreateGameBoyRom(cgb: true);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["CGB"]);
	}

	[Fact]
	public void Analyze_DetectsDmgMode() {
		var rom = CreateGameBoyRom(cgb: false);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("No", info.Metadata["CGB"]);
	}

	[Fact]
	public void Analyze_CalculatesRomBanks_Size0() {
		var rom = CreateGameBoyRom(romSize: 0);  // 32KB = 2 banks

		var info = _analyzer.Analyze(rom);

		Assert.Equal("2", info.Metadata["RomBanks"]);
	}

	[Fact]
	public void Analyze_CalculatesRomBanks_Size1() {
		var rom = CreateGameBoyRom(romSize: 1);  // 64KB = 4 banks

		var info = _analyzer.Analyze(rom);

		Assert.Equal("4", info.Metadata["RomBanks"]);
	}

	[Fact]
	public void Analyze_CalculatesRamBanks_NoRam() {
		var rom = CreateGameBoyRom(ramSize: 0);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("0", info.Metadata["RamBanks"]);
	}

	[Fact]
	public void Analyze_CalculatesRamBanks_8KB() {
		var rom = CreateGameBoyRom(ramSize: 2);  // 8KB = 1 bank

		var info = _analyzer.Analyze(rom);

		Assert.Equal("1", info.Metadata["RamBanks"]);
	}

	[Fact]
	public void Analyze_CalculatesRamBanks_32KB() {
		var rom = CreateGameBoyRom(ramSize: 3);  // 32KB = 4 banks

		var info = _analyzer.Analyze(rom);

		Assert.Equal("4", info.Metadata["RamBanks"]);
	}

	[Fact]
	public void GetRegisterLabel_JoypadRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff00);

		Assert.Equal("P1", label);
	}

	[Fact]
	public void GetRegisterLabel_LcdcRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff40);

		Assert.Equal("LCDC", label);
	}

	[Fact]
	public void GetRegisterLabel_StatRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff41);

		Assert.Equal("STAT", label);
	}

	[Fact]
	public void GetRegisterLabel_LyRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff44);

		Assert.Equal("LY", label);
	}

	[Fact]
	public void GetRegisterLabel_DmaRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff46);

		Assert.Equal("DMA", label);
	}

	[Fact]
	public void GetRegisterLabel_IeRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xffff);

		Assert.Equal("IE", label);
	}

	[Fact]
	public void GetRegisterLabel_IfRegister_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0xff0f);

		Assert.Equal("IF", label);
	}

	[Fact]
	public void GetRegisterLabel_TimerRegisters_ReturnCorrectLabels() {
		Assert.Equal("DIV", _analyzer.GetRegisterLabel(0xff04));
		Assert.Equal("TIMA", _analyzer.GetRegisterLabel(0xff05));
		Assert.Equal("TMA", _analyzer.GetRegisterLabel(0xff06));
		Assert.Equal("TAC", _analyzer.GetRegisterLabel(0xff07));
	}

	[Fact]
	public void GetRegisterLabel_SoundRegisters_ReturnCorrectLabels() {
		Assert.Equal("NR10", _analyzer.GetRegisterLabel(0xff10));
		Assert.Equal("NR52", _analyzer.GetRegisterLabel(0xff26));
	}

	[Fact]
	public void GetRegisterLabel_WaveRam_ReturnsWaveRamLabel() {
		var label = _analyzer.GetRegisterLabel(0xff30);

		Assert.Equal("WAVE_RAM", label);  // First address just returns WAVE_RAM
	}

	[Fact]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull() {
		var label = _analyzer.GetRegisterLabel(0x5000);

		Assert.Null(label);
	}

	[Fact]
	public void GetMemoryRegion_RomBank0_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x0000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetMemoryRegion_RomBankN_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x4000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetMemoryRegion_Vram_ReturnsGraphics() {
		var region = _analyzer.GetMemoryRegion(0x8000);

		Assert.Equal(MemoryRegion.Graphics, region);
	}

	[Fact]
	public void GetMemoryRegion_ExternalRam_ReturnsRom() {
		// External RAM is cartridge RAM, treated as ROM space in terms of type
		var region = _analyzer.GetMemoryRegion(0xa000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetMemoryRegion_WorkRam_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0xc000);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_Oam_ReturnsGraphics() {
		var region = _analyzer.GetMemoryRegion(0xfe00);

		Assert.Equal(MemoryRegion.Graphics, region);
	}

	[Fact]
	public void GetMemoryRegion_IoRegisters_ReturnsHardware() {
		var region = _analyzer.GetMemoryRegion(0xff00);

		Assert.Equal(MemoryRegion.Hardware, region);
	}

	[Fact]
	public void GetMemoryRegion_HighRam_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0xff80);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetEntryPoints_IncludesStandardEntries() {
		var rom = CreateGameBoyRom();

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Contains((uint)0x0100, entries);  // Start
		Assert.Contains((uint)0x0040, entries);  // VBlank
		Assert.Contains((uint)0x0048, entries);  // LCD STAT
		Assert.Contains((uint)0x0050, entries);  // Timer
		Assert.Contains((uint)0x0058, entries);  // Serial
		Assert.Contains((uint)0x0060, entries);  // Joypad
	}

	[Fact]
	public void IsInSwitchableRegion_Bank0_False() {
		var rom = CreateGameBoyRom(mbcType: 0x01);  // MBC1
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0x0000));
		Assert.False(_analyzer.IsInSwitchableRegion(0x3fff));
	}

	[Fact]
	public void IsInSwitchableRegion_Bank1Plus_True() {
		var rom = CreateGameBoyRom(mbcType: 0x01);  // MBC1
		_analyzer.Analyze(rom);

		Assert.True(_analyzer.IsInSwitchableRegion(0x4000));
		Assert.True(_analyzer.IsInSwitchableRegion(0x7fff));
	}

	[Fact]
	public void AddressToOffset_Bank0_DirectMapping() {
		var rom = CreateGameBoyRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x0100, rom.Length);

		Assert.Equal(0x0100, offset);
	}

	[Fact]
	public void AddressToOffset_Bank1_MapsToSecondBank() {
		var rom = CreateGameBoyRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x4000, rom.Length, 1);

		Assert.Equal(0x4000, offset);  // Bank 1 starts at 0x4000 in ROM
	}

	[Fact]
	public void AddressToOffset_Bank2_MapsCorrectly() {
		var rom = CreateGameBoyRom(romSize: 1);  // 64KB = 4 banks
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x4000, rom.Length, 2);

		Assert.Equal(0x8000, offset);  // Bank 2 starts at 0x8000 in ROM
	}

	[Fact]
	public void AddressToOffset_OutsideRom_ReturnsNegative() {
		var rom = CreateGameBoyRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x8000, rom.Length);  // VRAM

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void BankCount_ReturnsCorrectCount() {
		var rom = CreateGameBoyRom(romSize: 2);  // 128KB = 8 banks
		_analyzer.Analyze(rom);

		Assert.Equal(8, _analyzer.BankCount);
	}

	[Fact]
	public void DetectBankSwitch_ReturnsNull() {
		// Game Boy doesn't use BRK-based bank switching
		var rom = CreateGameBoyRom();
		_analyzer.Analyze(rom);

		var result = _analyzer.DetectBankSwitch(rom, 0x4000, 1);

		Assert.Null(result);
	}
}
