using Peony.Core;
using Xunit;

namespace Peony.Platform.Atari2600.Tests;

/// <summary>
/// Tests for <see cref="Atari2600Analyzer"/>.
/// </summary>
public class Atari2600AnalyzerTests {
	private readonly Atari2600Analyzer _analyzer = new();

	#region Properties

	[Fact]
	public void Platform_ReturnsAtari2600() {
		Assert.Equal("Atari 2600", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_Is6502() {
		Assert.Equal("6502", _analyzer.CpuDecoder.Architecture);
	}

	[Fact]
	public void RomDataOffset_IsZero() {
		Assert.Equal(0, _analyzer.RomDataOffset);
	}

	#endregion

	#region TIA Write Register Tests

	[Theory]
	[InlineData(0x00u, "VSYNC")]
	[InlineData(0x01u, "VBLANK")]
	[InlineData(0x02u, "WSYNC")]
	[InlineData(0x06u, "COLUP0")]
	[InlineData(0x07u, "COLUP1")]
	[InlineData(0x08u, "COLUPF")]
	[InlineData(0x09u, "COLUBK")]
	[InlineData(0x0du, "PF0")]
	[InlineData(0x0eu, "PF1")]
	[InlineData(0x0fu, "PF2")]
	[InlineData(0x10u, "RESP0")]
	[InlineData(0x11u, "RESP1")]
	[InlineData(0x15u, "AUDC0")]
	[InlineData(0x19u, "AUDV0")]
	[InlineData(0x1bu, "GRP0")]
	[InlineData(0x1cu, "GRP1")]
	[InlineData(0x2au, "HMOVE")]
	[InlineData(0x2cu, "CXCLR")]
	public void GetRegisterLabel_TiaWriteRegisters_ReturnsCorrectLabel(uint address, string expected) {
		var result = _analyzer.GetRegisterLabel(address);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(0x0040u, "VSYNC")]   // Mirrored: bit 6 set, but $1080 mask => TIA
	[InlineData(0x0100u, "VSYNC")]   // Another mirror
	public void GetRegisterLabel_TiaMirrored_ReturnsLabel(uint address, string expected) {
		var result = _analyzer.GetRegisterLabel(address);

		Assert.Equal(expected, result);
	}

	#endregion

	#region RIOT Register Tests

	[Theory]
	[InlineData(0x280u, "SWCHA")]
	[InlineData(0x281u, "SWACNT")]
	[InlineData(0x282u, "SWCHB")]
	[InlineData(0x283u, "SWBCNT")]
	[InlineData(0x284u, "INTIM")]
	[InlineData(0x285u, "TIMINT")]
	[InlineData(0x294u, "TIM1T")]
	[InlineData(0x295u, "TIM8T")]
	[InlineData(0x296u, "TIM64T")]
	[InlineData(0x297u, "T1024T")]
	public void GetRegisterLabel_RiotRegisters_ReturnsCorrectLabel(uint address, string expected) {
		var result = _analyzer.GetRegisterLabel(address);

		Assert.Equal(expected, result);
	}

	#endregion

	#region Memory Region Tests

	[Theory]
	[InlineData(0x00u)]
	[InlineData(0x01u)]
	[InlineData(0x0du)]
	[InlineData(0x2cu)]
	public void GetMemoryRegion_TiaAddresses_ReturnsHardware(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Hardware, result);
	}

	[Theory]
	[InlineData(0x80u)]
	[InlineData(0xffu)]
	public void GetMemoryRegion_RamAddresses_ReturnsRam(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Ram, result);
	}

	[Theory]
	[InlineData(0x280u)]
	[InlineData(0x297u)]
	public void GetMemoryRegion_RiotAddresses_ReturnsHardware(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Hardware, result);
	}

	[Theory]
	[InlineData(0xf000u)]
	[InlineData(0xf800u)]
	[InlineData(0xfffcu)]
	[InlineData(0xffffu)]
	public void GetMemoryRegion_RomAddresses_ReturnsRom(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Rom, result);
	}

	#endregion

	#region Bank Switching Detection Tests

	[Fact]
	public void Analyze_2kRom_NoScheme() {
		var rom = new byte[2048];
		FillResetVector(rom, 0xf800);

		var info = _analyzer.Analyze(rom);

		Assert.Null(_analyzer.DetectedScheme);
		Assert.Equal(1, _analyzer.BankCount);
	}

	[Fact]
	public void Analyze_4kRom_NoScheme() {
		var rom = new byte[4096];
		FillResetVector(rom, 0xf000);

		var info = _analyzer.Analyze(rom);

		Assert.Null(_analyzer.DetectedScheme);
		Assert.Equal(1, _analyzer.BankCount);
	}

	[Fact]
	public void Analyze_8kRom_DetectsF8() {
		var rom = new byte[8192];
		FillResetVector(rom, 0xf000);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F8", _analyzer.DetectedScheme);
		Assert.Equal(2, _analyzer.BankCount);
	}

	[Fact]
	public void Analyze_16kRom_DetectsF6() {
		var rom = new byte[16384];
		FillResetVector(rom, 0xf000);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F6", _analyzer.DetectedScheme);
		Assert.Equal(4, _analyzer.BankCount);
	}

	[Fact]
	public void Analyze_32kRom_DetectsF4() {
		var rom = new byte[32768];
		FillResetVector(rom, 0xf000);

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F4", _analyzer.DetectedScheme);
		Assert.Equal(8, _analyzer.BankCount);
	}

	[Fact]
	public void Analyze_8kRomWith3FSignature_Detects3F() {
		var rom = new byte[8192];
		FillResetVector(rom, 0xf800);
		// STA $3F pattern (Tigervision)
		rom[100] = 0x85; // STA zpg
		rom[101] = 0x3f;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("3F", _analyzer.DetectedScheme);
	}

	[Fact]
	public void Analyze_8kRomWithE0Signature_DetectsE0() {
		var rom = new byte[8192];
		FillResetVector(rom, 0xf000);
		// STA $1FE0 pattern (Parker Bros)
		rom[100] = 0x8d; // STA abs
		rom[101] = 0xe0;
		rom[102] = 0x1f;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("E0", _analyzer.DetectedScheme);
	}

	[Fact]
	public void Analyze_8kRomWithFESignature_DetectsFE() {
		var rom = new byte[8192];
		FillResetVector(rom, 0xf000);
		// JSR $01FE pattern (Activision)
		rom[100] = 0x20; // JSR abs
		rom[101] = 0xfe;
		rom[102] = 0x01;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("FE", _analyzer.DetectedScheme);
	}

	#endregion

	#region Entry Point Tests

	[Fact]
	public void GetEntryPoints_4kRom_ExtractsResetVector() {
		var rom = new byte[4096];
		FillResetVector(rom, 0xf000);

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Contains(0xf000u, entries);
	}

	[Fact]
	public void GetEntryPoints_SameResetAndIrq_ReturnsSingleEntry() {
		var rom = new byte[4096];
		// Reset vector: $F100
		rom[^4] = 0x00;
		rom[^3] = 0xf1;
		// IRQ vector: $F100 (same)
		rom[^2] = 0x00;
		rom[^1] = 0xf1;

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Single(entries);
		Assert.Equal(0xf100u, entries[0]);
	}

	[Fact]
	public void GetEntryPoints_DifferentResetAndIrq_ReturnsBoth() {
		var rom = new byte[4096];
		// Reset vector: $F000
		rom[^4] = 0x00;
		rom[^3] = 0xf0;
		// IRQ vector: $F100
		rom[^2] = 0x00;
		rom[^1] = 0xf1;

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Equal(2, entries.Length);
		Assert.Equal(0xf000u, entries[0]);
		Assert.Equal(0xf100u, entries[1]);
	}

	[Fact]
	public void GetEntryPoints_TinyRom_ReturnsFallback() {
		var rom = new byte[2]; // Too small for vectors

		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Single(entries);
		Assert.Equal(0xf000u, entries[0]);
	}

	#endregion

	#region Address Mapping Tests

	[Fact]
	public void AddressToOffset_4kRom_MapsCorrectly() {
		var rom = new byte[4096];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf000, 4096));
		Assert.Equal(0xfff, _analyzer.AddressToOffset(0xffff, 4096));
	}

	[Fact]
	public void AddressToOffset_2kRom_MapsCorrectly() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf800, 2048));
		Assert.Equal(0x7ff, _analyzer.AddressToOffset(0xffff, 2048));
	}

	[Fact]
	public void AddressToOffset_F8Bank0_MapsCorrectly() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf000, 8192, 0));
		Assert.Equal(0xfff, _analyzer.AddressToOffset(0xffff, 8192, 0));
	}

	[Fact]
	public void AddressToOffset_F8Bank1_MapsCorrectly() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom);

		Assert.Equal(4096, _analyzer.AddressToOffset(0xf000, 8192, 1));
	}

	[Fact]
	public void AddressToOffset_BelowRom_ReturnsNegative() {
		var rom = new byte[4096];
		_analyzer.Analyze(rom);

		Assert.Equal(-1, _analyzer.AddressToOffset(0x0080, 4096));
	}

	[Fact]
	public void OffsetToAddress_ReturnsF000Plus() {
		Assert.Equal(0xf000u, _analyzer.OffsetToAddress(0));
		Assert.Equal(0xf100u, _analyzer.OffsetToAddress(0x100));
		Assert.Equal(0xffffu, _analyzer.OffsetToAddress(0xfff));
	}

	#endregion

	#region IsInSwitchableRegion Tests

	[Fact]
	public void IsInSwitchableRegion_NoScheme_ReturnsFalse() {
		var rom = new byte[4096]; // No banking
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0xf000));
	}

	[Fact]
	public void IsInSwitchableRegion_F8Scheme_RomRegion_ReturnsTrue() {
		var rom = new byte[8192]; // F8 banking
		_analyzer.Analyze(rom);

		Assert.True(_analyzer.IsInSwitchableRegion(0xf000));
		Assert.True(_analyzer.IsInSwitchableRegion(0xffff));
	}

	[Fact]
	public void IsInSwitchableRegion_F8Scheme_BelowRom_ReturnsFalse() {
		var rom = new byte[8192]; // F8 banking
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0x0080));
	}

	#endregion

	#region DetectBankSwitch Tests

	[Fact]
	public void DetectBankSwitch_ReturnsNull() {
		// Atari 2600 doesn't use BRK-based bank switching
		var rom = new byte[4096];
		var result = _analyzer.DetectBankSwitch(rom, 0xf000, 0);

		Assert.Null(result);
	}

	#endregion

	#region Analyze RomInfo Tests

	[Fact]
	public void Analyze_ReturnsCorrectRomInfo() {
		var rom = new byte[8192];
		var info = _analyzer.Analyze(rom);

		Assert.Equal("Atari 2600", info.Platform);
		Assert.Equal(8192, info.Size);
		Assert.Equal("F8", info.Mapper);
		Assert.Equal("F8", info.Metadata["BankScheme"]);
		Assert.Equal("2", info.Metadata["Banks"]);
		Assert.Equal("4096", info.Metadata["BankSize"]);
	}

	#endregion

	#region Helpers

	/// <summary>Writes a reset vector pointing to the given address at the end of the ROM.</summary>
	private static void FillResetVector(byte[] rom, ushort address) {
		rom[^4] = (byte)(address & 0xff);
		rom[^3] = (byte)(address >> 8);
		rom[^2] = (byte)(address & 0xff);
		rom[^1] = (byte)(address >> 8);
	}

	#endregion
}
