namespace Peony.Platform.SMS.Tests;

using Peony.Core;
using Xunit;

public class SmsAnalyzerTests {
	private readonly SmsAnalyzer _analyzer = new();

	[Fact]
	public void Platform_ReturnsSegaMasterSystem() {
		Assert.Equal("Sega Master System", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_IsZ80() {
		Assert.Equal("Z80", _analyzer.CpuDecoder.Architecture);
	}

	// --- ROM analysis ---

	[Fact]
	public void Analyze_32KRom_ReturnsCorrectSize() {
		var rom = new byte[32768]; // 32KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Sega Master System", info.Platform);
		Assert.Equal(32768, info.Size);
	}

	[Fact]
	public void Analyze_256KRom_CalculatesBanks() {
		var rom = new byte[256 * 1024]; // 256KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal(256 * 1024, info.Size);
		Assert.True(info.Metadata.ContainsKey("Banks"));
		Assert.Equal("16", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_512KRom_CalculatesBanks() {
		var rom = new byte[512 * 1024]; // 512KB

		var info = _analyzer.Analyze(rom);

		Assert.Equal("32", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_RomSize_InMetadata() {
		var rom = new byte[32768];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("32KB", info.Metadata["RomSize"]);
	}

	[Fact]
	public void Analyze_HasMapperMetadata() {
		var rom = new byte[32768];

		var info = _analyzer.Analyze(rom);

		Assert.True(info.Metadata.ContainsKey("Mapper"));
	}

	// --- Memory regions ---

	[Fact]
	public void GetMemoryRegion_RomArea_ReturnsRom() {
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x0000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x4000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0x8000));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xbfff));
	}

	[Fact]
	public void GetMemoryRegion_RamArea_ReturnsRam() {
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xc000));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xdfff));
	}

	[Fact]
	public void GetMemoryRegion_MapperRegs_ReturnsHardware() {
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xfffc));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0xffff));
	}

	// --- Register labels ---

	[Fact]
	public void GetRegisterLabel_MapperCtrl() {
		Assert.Equal("MAPPER_CTRL", _analyzer.GetRegisterLabel(0xfffc));
	}

	[Fact]
	public void GetRegisterLabel_MapperSlot0() {
		Assert.Equal("MAPPER_SLOT0", _analyzer.GetRegisterLabel(0xfffd));
	}

	[Fact]
	public void GetRegisterLabel_MapperSlot1() {
		Assert.Equal("MAPPER_SLOT1", _analyzer.GetRegisterLabel(0xfffe));
	}

	[Fact]
	public void GetRegisterLabel_MapperSlot2() {
		Assert.Equal("MAPPER_SLOT2", _analyzer.GetRegisterLabel(0xffff));
	}

	[Fact]
	public void GetRegisterLabel_Unknown_ReturnsNull() {
		Assert.Null(_analyzer.GetRegisterLabel(0x0000));
	}

	// --- Entry points ---

	[Fact]
	public void GetEntryPoints_IncludesResetVector() {
		var rom = new byte[32768];
		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0x0000u, entryPoints);
	}

	[Fact]
	public void GetEntryPoints_IncludesInterruptVector() {
		var rom = new byte[32768];
		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0x0038u, entryPoints); // IM 1 maskable interrupt
	}

	[Fact]
	public void GetEntryPoints_IncludesNMI() {
		var rom = new byte[32768];
		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0x0066u, entryPoints); // NMI (pause button)
	}

	// --- Address translation ---

	[Fact]
	public void AddressToOffset_RamArea_ReturnsMinusOne() {
		Assert.Equal(-1, _analyzer.AddressToOffset(0xc000, 32768));
	}

	[Fact]
	public void AddressToOffset_FirstKB_DirectMapping() {
		Assert.Equal(0, _analyzer.AddressToOffset(0x0000, 32768));
		Assert.Equal(0x100, _analyzer.AddressToOffset(0x0100, 32768));
		Assert.Equal(0x3ff, _analyzer.AddressToOffset(0x03ff, 32768));
	}
}
