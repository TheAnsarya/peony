using Peony.Core;
using Peony.Platform.GBA;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for Game Boy Advance platform analyzer including register labeling
/// </summary>
public class GbaAnalyzerTests {
	private readonly GbaAnalyzer _analyzer = new();

	/// <summary>
	/// Creates a minimal valid GBA ROM header for testing
	/// </summary>
	private static byte[] CreateGbaRom(
		string title = "TEST",
		string gameCode = "AXXX",
		string makerCode = "01",
		byte version = 0) {
		// Minimum ROM size for testing
		var rom = new byte[0x200];

		// Entry point (ARM branch instruction to $080000c0)
		// EA00002E = b $080000c0
		rom[0] = 0x2e;
		rom[1] = 0x00;
		rom[2] = 0x00;
		rom[3] = 0xea;

		// Nintendo logo at $04-$9f (just fill with placeholder)
		for (int i = 0x04; i < 0xa0; i++) {
			rom[i] = 0x00;
		}

		// Title at $a0-$ab (12 bytes)
		var titleBytes = System.Text.Encoding.ASCII.GetBytes(title.PadRight(12));
		for (int i = 0; i < 12; i++) {
			rom[0xa0 + i] = titleBytes[i];
		}

		// Game code at $ac-$af (4 bytes)
		var codeBytes = System.Text.Encoding.ASCII.GetBytes(gameCode.PadRight(4));
		for (int i = 0; i < 4; i++) {
			rom[0xac + i] = codeBytes[i];
		}

		// Maker code at $b0-$b1 (2 bytes)
		var makerBytes = System.Text.Encoding.ASCII.GetBytes(makerCode.PadRight(2));
		for (int i = 0; i < 2; i++) {
			rom[0xb0 + i] = makerBytes[i];
		}

		// Fixed value $96 at $b2
		rom[0xb2] = 0x96;

		// Main unit code at $b3
		rom[0xb3] = 0x00;

		// Device type at $b4
		rom[0xb4] = 0x00;

		// Reserved area $b5-$bb
		for (int i = 0xb5; i <= 0xbb; i++) {
			rom[i] = 0x00;
		}

		// Software version at $bc
		rom[0xbc] = version;

		// Header checksum at $bd
		// Calculate checksum
		int checksum = 0;
		for (int i = 0xa0; i <= 0xbc; i++) {
			checksum -= rom[i];
		}
		rom[0xbd] = (byte)((checksum - 0x19) & 0xff);

		return rom;
	}

	[Fact]
	public void Analyze_ValidGbaRom_IdentifiesPlatform() {
		var rom = CreateGbaRom();

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Game Boy Advance", info.Platform);
	}

	[Fact]
	public void Analyze_ReadsTitle() {
		var rom = CreateGbaRom(title: "POKEMON");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("POKEMON", info.Metadata["Title"]);
	}

	[Fact]
	public void Analyze_ReadsGameCode() {
		var rom = CreateGbaRom(gameCode: "AXVE");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("AXVE", info.Metadata["GameCode"]);
	}

	[Fact]
	public void Analyze_ReadsMakerCode() {
		var rom = CreateGbaRom(makerCode: "01");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("01", info.Metadata["MakerCode"]);
	}

	[Fact]
	public void Analyze_ReadsVersion() {
		var rom = CreateGbaRom(version: 2);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("1.2", info.Metadata["Version"]);
	}

	[Fact]
	public void Analyze_CalculatesRomSize() {
		var rom = CreateGbaRom();

		var info = _analyzer.Analyze(rom);

		Assert.Equal("512 B", info.Metadata["RomSize"]);
	}

	[Fact]
	public void GetRegisterLabel_DispCnt_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000000);

		Assert.Equal("DISPCNT", label);
	}

	[Fact]
	public void GetRegisterLabel_DispStat_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000002);

		Assert.Equal("DISPSTAT", label);
	}

	[Fact]
	public void GetRegisterLabel_VCount_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000004);

		Assert.Equal("VCOUNT", label);
	}

	[Fact]
	public void GetRegisterLabel_KeyInput_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000130);

		Assert.Equal("KEYINPUT", label);
	}

	[Fact]
	public void GetRegisterLabel_InterruptEnable_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000200);

		Assert.Equal("IE", label);
	}

	[Fact]
	public void GetRegisterLabel_InterruptFlag_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000202);

		Assert.Equal("IF", label);
	}

	[Fact]
	public void GetRegisterLabel_Ime_ReturnsCorrectLabel() {
		var label = _analyzer.GetRegisterLabel(0x04000208);

		Assert.Equal("IME", label);
	}

	[Fact]
	public void GetRegisterLabel_BgCnt_ReturnsCorrectLabels() {
		Assert.Equal("BG0CNT", _analyzer.GetRegisterLabel(0x04000008));
		Assert.Equal("BG1CNT", _analyzer.GetRegisterLabel(0x0400000a));
		Assert.Equal("BG2CNT", _analyzer.GetRegisterLabel(0x0400000c));
		Assert.Equal("BG3CNT", _analyzer.GetRegisterLabel(0x0400000e));
	}

	[Fact]
	public void GetRegisterLabel_DmaRegisters_ReturnCorrectLabels() {
		Assert.Equal("DMA0SAD", _analyzer.GetRegisterLabel(0x040000b0));
		Assert.Equal("DMA0DAD", _analyzer.GetRegisterLabel(0x040000b4));
		Assert.Equal("DMA0CNT_L", _analyzer.GetRegisterLabel(0x040000b8));
		Assert.Equal("DMA0CNT_H", _analyzer.GetRegisterLabel(0x040000ba));
	}

	[Fact]
	public void GetRegisterLabel_TimerRegisters_ReturnCorrectLabels() {
		Assert.Equal("TM0CNT_L", _analyzer.GetRegisterLabel(0x04000100));
		Assert.Equal("TM0CNT_H", _analyzer.GetRegisterLabel(0x04000102));
		Assert.Equal("TM1CNT_L", _analyzer.GetRegisterLabel(0x04000104));
	}

	[Fact]
	public void GetRegisterLabel_UnknownAddress_ReturnsNull() {
		var label = _analyzer.GetRegisterLabel(0x05000000);

		Assert.Null(label);
	}

	[Fact]
	public void GetMemoryRegion_Bios_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x00000000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetMemoryRegion_EWram_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x02000000);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_IWram_ReturnsRam() {
		var region = _analyzer.GetMemoryRegion(0x03000000);

		Assert.Equal(MemoryRegion.Ram, region);
	}

	[Fact]
	public void GetMemoryRegion_IoRegisters_ReturnsHardware() {
		var region = _analyzer.GetMemoryRegion(0x04000000);

		Assert.Equal(MemoryRegion.Hardware, region);
	}

	[Fact]
	public void GetMemoryRegion_PaletteRam_ReturnsGraphics() {
		var region = _analyzer.GetMemoryRegion(0x05000000);

		Assert.Equal(MemoryRegion.Graphics, region);
	}

	[Fact]
	public void GetMemoryRegion_Vram_ReturnsGraphics() {
		var region = _analyzer.GetMemoryRegion(0x06000000);

		Assert.Equal(MemoryRegion.Graphics, region);
	}

	[Fact]
	public void GetMemoryRegion_Oam_ReturnsGraphics() {
		var region = _analyzer.GetMemoryRegion(0x07000000);

		Assert.Equal(MemoryRegion.Graphics, region);
	}

	[Fact]
	public void GetMemoryRegion_GamePakRom_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x08000000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetMemoryRegion_GamePakSram_ReturnsRom() {
		var region = _analyzer.GetMemoryRegion(0x0e000000);

		Assert.Equal(MemoryRegion.Rom, region);
	}

	[Fact]
	public void GetEntryPoints_IncludesRomStart() {
		var rom = CreateGbaRom();

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Contains((uint)0x08000000, entries);
	}

	[Fact]
	public void GetEntryPoints_DecodesArmBranchInstruction() {
		var rom = CreateGbaRom();
		// Entry point is EA00002E = b $080000c0

		var entries = _analyzer.GetEntryPoints(rom);

		// Should include the branch target
		Assert.Contains((uint)0x080000c0, entries);
	}

	[Fact]
	public void IsInSwitchableRegion_AlwaysFalse() {
		// GBA doesn't use traditional banking
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0x08000000));
		Assert.False(_analyzer.IsInSwitchableRegion(0x0a000000));
	}

	[Fact]
	public void AddressToOffset_RomAddress_MapsCorrectly() {
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x08000000, rom.Length);

		Assert.Equal(0, offset);  // ROM start
	}

	[Fact]
	public void AddressToOffset_RomAddressOffset_MapsCorrectly() {
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x08000100, rom.Length);

		Assert.Equal(0x100, offset);
	}

	[Fact]
	public void AddressToOffset_OutsideRom_ReturnsNegative() {
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x02000000, rom.Length);  // EWRAM

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void AddressToOffset_BeyondRomSize_ReturnsNegative() {
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		var offset = _analyzer.AddressToOffset(0x08100000, rom.Length);  // Beyond our small test ROM

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void BankCount_AlwaysOne() {
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		Assert.Equal(1, _analyzer.BankCount);
	}

	[Fact]
	public void DetectBankSwitch_ReturnsNull() {
		// GBA doesn't use bank switching
		var rom = CreateGbaRom();
		_analyzer.Analyze(rom);

		var result = _analyzer.DetectBankSwitch(rom, 0x08000000, 0);

		Assert.Null(result);
	}
}
