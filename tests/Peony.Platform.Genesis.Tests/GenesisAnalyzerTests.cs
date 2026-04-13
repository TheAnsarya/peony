namespace Peony.Platform.Genesis.Tests;

using System.Text;

using Peony.Core;
using Xunit;

public class GenesisAnalyzerTests {
	private readonly GenesisAnalyzer _analyzer = new();

	[Fact]
	public void Platform_ReturnsSegaGenesis() {
		Assert.Equal("Sega Genesis", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_IsM68000() {
		Assert.Equal("M68000", _analyzer.CpuDecoder.Architecture);
	}

	// --- ROM analysis ---

	[Fact]
	public void Analyze_ReturnsCorrectPlatform() {
		var rom = CreateValidRom(512 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Sega Genesis", info.Platform);
	}

	[Fact]
	public void Analyze_ReturnsCorrectSize() {
		var rom = CreateValidRom(512 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal(512 * 1024, info.Size);
	}

	[Fact]
	public void Analyze_RomSize_InMetadata() {
		var rom = CreateValidRom(1024 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("1024KB", info.Metadata["RomSize"]);
	}

	[Fact]
	public void Analyze_CalculatesBanks_512K() {
		var rom = CreateValidRom(512 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("1", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_CalculatesBanks_2MB() {
		var rom = CreateValidRom(2 * 1024 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("4", info.Metadata["Banks"]);
	}

	// --- Header parsing ---

	[Fact]
	public void Analyze_ReadsConsoleName() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x100, "SEGA GENESIS    ");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("SEGA GENESIS", info.Metadata["Console"]);
	}

	[Fact]
	public void Analyze_ReadsDomesticName() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x120, "TEST GAME DOMESTIC");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("TEST GAME DOMESTIC", info.Metadata["DomesticName"]);
	}

	[Fact]
	public void Analyze_ReadsOverseasName() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x150, "TEST GAME OVERSEAS");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("TEST GAME OVERSEAS", info.Metadata["OverseasName"]);
	}

	[Fact]
	public void Analyze_ReadsSerial() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x180, "GM 00000000-00");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("GM 00000000-00", info.Metadata["Serial"]);
	}

	[Fact]
	public void Analyze_ReadsChecksum() {
		var rom = CreateValidRom(512 * 1024);
		rom[0x18e] = 0xab;
		rom[0x18f] = 0xcd;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("$abcd", info.Metadata["Checksum"]);
	}

	[Fact]
	public void Analyze_ValidHeader_WithSega() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x100, "SEGA GENESIS    ");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["ValidHeader"]);
	}

	[Fact]
	public void Analyze_InvalidHeader_NoSega() {
		var rom = CreateValidRom(512 * 1024);
		WriteAscii(rom, 0x100, "XXXX GENESIS    ");

		var info = _analyzer.Analyze(rom);

		Assert.Equal("No", info.Metadata["ValidHeader"]);
	}

	[Fact]
	public void Analyze_Mapper_NoneForSmallRom() {
		var rom = CreateValidRom(512 * 1024);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("None", info.Mapper);
	}

	[Fact]
	public void Analyze_Mapper_SSFIIForLargeRom() {
		var rom = CreateValidRom(5 * 1024 * 1024); // >4MB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("SSFII", info.Mapper);
	}

	// --- Memory regions ---

	[Fact]
	public void GetMemoryRegion_CartridgeRom_ReturnsRom() {
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x000000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x200000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x3fffff));
	}

	[Fact]
	public void GetMemoryRegion_Z80Space_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xa00000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xa0ffff));
	}

	[Fact]
	public void GetMemoryRegion_IoRegisters_ReturnsHardware() {
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xa10000));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xa1001f));
	}

	[Fact]
	public void GetMemoryRegion_Vdp_ReturnsHardware() {
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xc00000));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xc00004));
	}

	[Fact]
	public void GetMemoryRegion_WorkRam_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xe00000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xffffff));
	}

	// --- Register labels ---

	[Fact]
	public void GetRegisterLabel_VdpData() {
		Assert.Equal("VDP_DATA", _analyzer.GetRegisterLabel(0xc00000));
	}

	[Fact]
	public void GetRegisterLabel_VdpCtrl() {
		Assert.Equal("VDP_CTRL", _analyzer.GetRegisterLabel(0xc00004));
	}

	[Fact]
	public void GetRegisterLabel_Z80BusReq() {
		Assert.Equal("Z80_BUSREQ", _analyzer.GetRegisterLabel(0xa11100));
	}

	[Fact]
	public void GetRegisterLabel_Ctrl1Data() {
		Assert.Equal("CTRL1_DATA", _analyzer.GetRegisterLabel(0xa10003));
	}

	[Fact]
	public void GetRegisterLabel_Psg() {
		Assert.Equal("PSG", _analyzer.GetRegisterLabel(0xc00011));
	}

	[Fact]
	public void GetRegisterLabel_Unknown_ReturnsNull() {
		Assert.Null(_analyzer.GetRegisterLabel(0x123456));
	}

	// --- Entry points ---

	[Fact]
	public void GetEntryPoints_ReadsInitialPC() {
		var rom = CreateValidRom(512 * 1024);
		// Initial PC at $000004 (big-endian 32-bit)
		rom[4] = 0x00;
		rom[5] = 0x00;
		rom[6] = 0x02;
		rom[7] = 0x00; // $000200

		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0x200u, entryPoints);
	}

	[Fact]
	public void GetEntryPoints_IncludesExceptionVectors() {
		var rom = CreateValidRom(512 * 1024);
		// Initial PC
		rom[4] = 0x00; rom[5] = 0x00; rom[6] = 0x02; rom[7] = 0x00;
		// V-blank handler at $70 (Level 6 interrupt)
		rom[0x78] = 0x00; rom[0x79] = 0x00; rom[0x7a] = 0x04; rom[0x7b] = 0x00;

		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0x200u, entryPoints);
		Assert.Contains(0x400u, entryPoints);
	}

	// --- Address translation ---

	[Fact]
	public void AddressToOffset_RomAddress_MapsDirectly() {
		Assert.Equal(0, _analyzer.AddressToOffset(0x000000, 512 * 1024));
		Assert.Equal(0x1000, _analyzer.AddressToOffset(0x001000, 512 * 1024));
	}

	[Fact]
	public void AddressToOffset_BeyondRom_ReturnsMinusOne() {
		Assert.Equal(-1, _analyzer.AddressToOffset(0x100000, 512 * 1024));
	}

	[Fact]
	public void AddressToOffset_NonRomAddress_ReturnsMinusOne() {
		Assert.Equal(-1, _analyzer.AddressToOffset(0xe00000, 512 * 1024));
	}

	// --- Helpers ---

	private static byte[] CreateValidRom(int size) {
		var rom = new byte[size];
		// Set initial SSP at $000000
		rom[0] = 0x00; rom[1] = 0xff; rom[2] = 0xff; rom[3] = 0x00;
		return rom;
	}

	private static void WriteAscii(byte[] rom, int offset, string text) {
		var bytes = Encoding.ASCII.GetBytes(text);
		Array.Copy(bytes, 0, rom, offset, Math.Min(bytes.Length, rom.Length - offset));
	}
}
