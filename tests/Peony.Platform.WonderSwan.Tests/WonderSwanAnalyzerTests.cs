namespace Peony.Platform.WonderSwan.Tests;

using Peony.Core;
using Xunit;

public class WonderSwanAnalyzerTests {
	private readonly WonderSwanAnalyzer _analyzer = new();

	[Fact]
	public void Platform_ReturnsWonderSwan() {
		Assert.Equal("WonderSwan", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_IsV30MZ() {
		Assert.Equal("V30MZ", _analyzer.CpuDecoder.Architecture);
	}

	// --- ROM analysis ---

	[Fact]
	public void Analyze_512KRom_ReturnsCorrectSize() {
		var rom = new byte[512 * 1024];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("WonderSwan", info.Platform);
		Assert.Equal(512 * 1024, info.Size);
	}

	[Fact]
	public void Analyze_CalculatesBanks() {
		var rom = new byte[256 * 1024]; // 4 banks of 64KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("4", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_RomSize_InMetadata() {
		var rom = new byte[512 * 1024];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("512KB", info.Metadata["RomSize"]);
	}

	// --- Footer parsing ---

	[Fact]
	public void Analyze_ReadsColorFlag_Mono() {
		var rom = new byte[64 * 1024];
		// Footer at last 16 bytes: byte[1] = color flag
		rom[^15] = 0x00; // Not color

		var info = _analyzer.Analyze(rom);

		Assert.Equal("No", info.Metadata["Color"]);
		Assert.Equal("WonderSwan", info.Metadata["Platform"]);
	}

	[Fact]
	public void Analyze_ReadsColorFlag_Color() {
		var rom = new byte[64 * 1024];
		rom[^15] = 0x01; // Color

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["Color"]);
		Assert.Equal("WonderSwan Color", info.Metadata["Platform"]);
	}

	[Fact]
	public void Analyze_ReadsOrientation_Horizontal() {
		var rom = new byte[64 * 1024];
		rom[^10] = 0x00; // Horizontal

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Horizontal", info.Metadata["Orientation"]);
	}

	[Fact]
	public void Analyze_ReadsOrientation_Vertical() {
		var rom = new byte[64 * 1024];
		rom[^10] = 0x01; // Vertical flag set

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Vertical", info.Metadata["Orientation"]);
	}

	[Fact]
	public void Analyze_ReadsSramSize() {
		var rom = new byte[64 * 1024];
		rom[^11] = 0x01; // 64Kbit / 8KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("64Kbit (8KB)", info.Metadata["SramSize"]);
	}

	[Fact]
	public void Analyze_ReadsRtcFlag() {
		var rom = new byte[64 * 1024];
		rom[^10] = 0x02; // RTC flag

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Yes", info.Metadata["RTC"]);
	}

	// --- Memory regions ---

	[Fact]
	public void GetMemoryRegion_InternalRam_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x0000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x3fff));
	}

	[Fact]
	public void GetMemoryRegion_ExtendedRam_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x4000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xffff));
	}

	[Fact]
	public void GetMemoryRegion_Sram_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x10000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x1ffff));
	}

	[Fact]
	public void GetMemoryRegion_Rom_ReturnsRom() {
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x20000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x40000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xfffff));
	}

	// --- Register labels ---

	[Fact]
	public void GetRegisterLabel_DispCtrl() {
		Assert.Equal("DISP_CTRL", _analyzer.GetRegisterLabel(0x00));
	}

	[Fact]
	public void GetRegisterLabel_BankRom2() {
		Assert.Equal("BANK_ROM2", _analyzer.GetRegisterLabel(0xc0));
	}

	[Fact]
	public void GetRegisterLabel_IntEnable() {
		Assert.Equal("INT_ENABLE", _analyzer.GetRegisterLabel(0xb2));
	}

	[Fact]
	public void GetRegisterLabel_DmaCtrl() {
		Assert.Equal("DMA_CTRL", _analyzer.GetRegisterLabel(0x48));
	}

	[Fact]
	public void GetRegisterLabel_SndCtrl() {
		Assert.Equal("SND_CTRL", _analyzer.GetRegisterLabel(0x90));
	}

	// --- Entry points ---

	[Fact]
	public void GetEntryPoints_IncludesResetVector() {
		var rom = new byte[64 * 1024];
		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0xffff0u, entryPoints); // V30MZ reset vector
	}

	// --- Address translation ---

	[Fact]
	public void AddressToOffset_LinearRom_MapsToEndOfRom() {
		var romLength = 256 * 1024;
		// $FFFF0 should map to romLength - 16
		var offset = _analyzer.AddressToOffset(0xffff0, romLength);

		Assert.Equal(romLength - 16, offset);
	}

	[Fact]
	public void AddressToOffset_LinearRom_LastByte() {
		var romLength = 256 * 1024;
		// $FFFFF should map to romLength - 1
		var offset = _analyzer.AddressToOffset(0xfffff, romLength);

		Assert.Equal(romLength - 1, offset);
	}
}
