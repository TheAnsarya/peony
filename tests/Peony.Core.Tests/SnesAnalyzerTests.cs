namespace Peony.Tests;

using Peony.Core;
using Peony.Platform.SNES;
using Xunit;

/// <summary>
/// Tests for the SNES platform analyzer
/// </summary>
public class SnesAnalyzerTests {
	[Fact]
	public void Platform_ReturnsSnes() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("SNES", analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_Returns65816() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("65816", analyzer.CpuDecoder.Architecture);
	}

	[Fact]
	public void GetRegisterLabel_PpuRegister_ReturnsLabel() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("INIDISP", analyzer.GetRegisterLabel(0x2100));
		Assert.Equal("BGMODE", analyzer.GetRegisterLabel(0x2105));
		Assert.Equal("VMDATAL", analyzer.GetRegisterLabel(0x2118));
	}

	[Fact]
	public void GetRegisterLabel_ApuRegister_ReturnsLabel() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("APUIO0", analyzer.GetRegisterLabel(0x2140));
		Assert.Equal("APUIO3", analyzer.GetRegisterLabel(0x2143));
	}

	[Fact]
	public void GetRegisterLabel_CpuRegister_ReturnsLabel() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("NMITIMEN", analyzer.GetRegisterLabel(0x4200));
		Assert.Equal("RDNMI", analyzer.GetRegisterLabel(0x4210));
		Assert.Equal("JOY1L", analyzer.GetRegisterLabel(0x4218));
	}

	[Fact]
	public void GetRegisterLabel_DmaRegister_ReturnsLabel() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal("DMAP0", analyzer.GetRegisterLabel(0x4300));
		Assert.Equal("BBAD0", analyzer.GetRegisterLabel(0x4301));
		Assert.Equal("DMAP7", analyzer.GetRegisterLabel(0x4370));
	}

	[Fact]
	public void GetMemoryRegion_Wram_ReturnsRam() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal(MemoryRegion.Ram, analyzer.GetMemoryRegion(0x0000));
		Assert.Equal(MemoryRegion.Ram, analyzer.GetMemoryRegion(0x1fff));
		Assert.Equal(MemoryRegion.Ram, analyzer.GetMemoryRegion(0x7e0000));
		Assert.Equal(MemoryRegion.Ram, analyzer.GetMemoryRegion(0x7f0000));
	}

	[Fact]
	public void GetMemoryRegion_Hardware_ReturnsHardware() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal(MemoryRegion.Hardware, analyzer.GetMemoryRegion(0x2100));
		Assert.Equal(MemoryRegion.Hardware, analyzer.GetMemoryRegion(0x4200));
	}

	[Fact]
	public void GetMemoryRegion_Rom_ReturnsRom() {
		var analyzer = new SnesAnalyzer();
		Assert.Equal(MemoryRegion.Rom, analyzer.GetMemoryRegion(0x8000));
		Assert.Equal(MemoryRegion.Rom, analyzer.GetMemoryRegion(0xc00000));
	}

	[Fact]
	public void LoRom_AddressToOffset_CorrectMapping() {
		var analyzer = new SnesAnalyzer();

		// Create a minimal LoRom header at $7fc0
		var rom = new byte[0x8000];
		// Set checksum/complement to validate header
		rom[0x7fdc] = 0xff; // Complement low
		rom[0x7fdd] = 0xff; // Complement high
		rom[0x7fde] = 0x00; // Checksum low
		rom[0x7fdf] = 0x00; // Checksum high
		rom[0x7fd5] = 0x20; // LoRom flag

		analyzer.Analyze(rom);

		// Bank 0, offset $8000 -> file offset 0
		Assert.Equal(0, analyzer.AddressToOffset(0x008000, rom.Length));

		// Bank 0, offset $FFFF -> file offset $7FFF
		Assert.Equal(0x7fff, analyzer.AddressToOffset(0x00ffff, rom.Length));
	}

	[Fact]
	public void Analyze_DetectsCopierHeader() {
		var analyzer = new SnesAnalyzer();

		// ROM with 512-byte copier header + minimal LoRom
		var rom = new byte[512 + 0x8000];
		// Set checksum at copier-adjusted position
		rom[512 + 0x7fdc] = 0xff;
		rom[512 + 0x7fdd] = 0xff;
		rom[512 + 0x7fde] = 0x00;
		rom[512 + 0x7fdf] = 0x00;
		rom[512 + 0x7fd5] = 0x20;

		var info = analyzer.Analyze(rom);

		Assert.True(analyzer.HasCopierHeader);
		Assert.Equal(SnesMapMode.LoRom, analyzer.MapMode);
	}

	[Fact]
	public void IsInSwitchableRegion_AlwaysReturnsFalse() {
		var analyzer = new SnesAnalyzer();
		// SNES uses direct addressing, not bankswitching like NES
		Assert.False(analyzer.IsInSwitchableRegion(0x8000));
		Assert.False(analyzer.IsInSwitchableRegion(0xc000));
	}

	[Fact]
	public void DetectBankSwitch_ReturnsNull() {
		var analyzer = new SnesAnalyzer();
		var rom = new byte[0x8000];

		// SNES doesn't use BRK-based bank switching
		Assert.Null(analyzer.DetectBankSwitch(rom, 0x8000, 0));
	}

	[Fact]
	public void GetEntryPoints_ReturnsResetVector() {
		var analyzer = new SnesAnalyzer();

		// Create ROM with reset vector
		var rom = new byte[0x8000];
		rom[0x7fdc] = 0xff;
		rom[0x7fdd] = 0xff;
		rom[0x7fde] = 0x00;
		rom[0x7fdf] = 0x00;
		rom[0x7fd5] = 0x20;

		// Set reset vector at $7ffc-$7ffd (header + $3c)
		rom[0x7ffc] = 0x00;
		rom[0x7ffd] = 0x80; // Reset = $8000

		var entries = analyzer.GetEntryPoints(rom);
		Assert.Contains(0x8000u, entries);
	}
}
