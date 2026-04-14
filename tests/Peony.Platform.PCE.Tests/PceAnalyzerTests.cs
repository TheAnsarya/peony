namespace Peony.Platform.PCE.Tests;

using Peony.Core;
using Xunit;

public class PceAnalyzerTests {
	private readonly PceAnalyzer _analyzer = new();

	[Fact]
	public void Platform_ReturnsPCEngine() {
		Assert.Equal("PC Engine", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_IsHuC6280() {
		Assert.Equal("HuC6280", _analyzer.CpuDecoder.Architecture);
	}

	// --- ROM analysis ---

	[Fact]
	public void Analyze_256KRom_ReturnsCorrectSize() {
		var rom = new byte[256 * 1024]; // 256KB raw HuCard

		var info = _analyzer.Analyze(rom);

		Assert.Equal("PC Engine", info.Platform);
		Assert.Equal(256 * 1024, info.Size);
	}

	[Fact]
	public void Analyze_RawFormat_NoHeader() {
		var rom = new byte[32768]; // 32KB, aligned to 8KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Raw", info.Metadata["Format"]);
	}

	[Fact]
	public void Analyze_HeaderedFormat_512ByteHeader() {
		// 32KB + 512 byte header
		var rom = new byte[32768 + 512];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Headered", info.Metadata["Format"]);
	}

	[Fact]
	public void Analyze_CalculatesBanks() {
		var rom = new byte[256 * 1024]; // 256KB = 32 banks of 8KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("32", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_DetectsHuCard() {
		var rom = new byte[256 * 1024]; // Standard HuCard

		var info = _analyzer.Analyze(rom);

		Assert.Equal("HuCard", info.Metadata["Type"]);
	}

	[Fact]
	public void Analyze_DetectsSF2Mapper() {
		var rom = new byte[512 * 1024]; // > 384KB → SF2 Mapper

		var info = _analyzer.Analyze(rom);

		Assert.Equal("SF2 Mapper", info.Metadata["Type"]);
	}

	// --- Memory regions ---

	[Fact]
	public void GetMemoryRegion_IoArea_ReturnsHardware() {
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x0000));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x1fff));
	}

	[Fact]
	public void GetMemoryRegion_WorkRam_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x2000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x3fff));
	}

	[Fact]
	public void GetMemoryRegion_RomArea_ReturnsRom() {
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x4000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xe000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xffff));
	}

	// --- Register labels ---

	[Fact]
	public void GetRegisterLabel_VdcStatus() {
		Assert.Equal("VDC_STATUS", _analyzer.GetRegisterLabel(0x0000));
	}

	[Fact]
	public void GetRegisterLabel_PsgSelect() {
		Assert.Equal("PSG_SELECT", _analyzer.GetRegisterLabel(0x0800));
	}

	[Fact]
	public void GetRegisterLabel_TimerCount() {
		Assert.Equal("TIMER_COUNT", _analyzer.GetRegisterLabel(0x0c00));
	}

	[Fact]
	public void GetRegisterLabel_IoPort() {
		Assert.Equal("IO_PORT", _analyzer.GetRegisterLabel(0x1000));
	}

	[Fact]
	public void GetRegisterLabel_IrqDisable() {
		Assert.Equal("IRQ_DISABLE", _analyzer.GetRegisterLabel(0x1402));
	}

	[Fact]
	public void GetRegisterLabel_Unknown_ReturnsNull() {
		Assert.Null(_analyzer.GetRegisterLabel(0x5000));
	}

	// --- Entry points ---

	[Fact]
	public void GetEntryPoints_ReadsResetVector() {
		var rom = new byte[8192]; // 1 page
		// Set RESET vector at end of page: $FFFE/$FFFF
		rom[8190] = 0x00; // low byte
		rom[8191] = 0xe0; // high byte → $e000

		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0xe000u, entryPoints);
	}

	// --- Address translation ---

	[Fact]
	public void AddressToOffset_IoRamArea_ReturnsMinusOne() {
		Assert.Equal(-1, _analyzer.AddressToOffset(0x0000, 32768));
		Assert.Equal(-1, _analyzer.AddressToOffset(0x2000, 32768));
	}

	[Fact]
	public void IsInSwitchableRegion_RomPages_ReturnsTrue() {
		Assert.True(_analyzer.IsInSwitchableRegion(0x4000));
		Assert.True(_analyzer.IsInSwitchableRegion(0xe000));
	}

	[Fact]
	public void IsInSwitchableRegion_LowPages_ReturnsFalse() {
		Assert.False(_analyzer.IsInSwitchableRegion(0x0000));
		Assert.False(_analyzer.IsInSwitchableRegion(0x2000));
	}
}
