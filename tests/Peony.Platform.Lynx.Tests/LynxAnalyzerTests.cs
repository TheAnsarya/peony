using Peony.Core;
using Xunit;

namespace Peony.Platform.Lynx.Tests;

/// <summary>
/// Tests for <see cref="LynxAnalyzer"/>.
/// </summary>
public class LynxAnalyzerTests {
	private readonly LynxAnalyzer _analyzer = new();

	#region GetRegisterLabel Tests

	[Theory]
	[InlineData(0xfc00, "TMPADR_L")]
	[InlineData(0xfc01, "TMPADR_H")]
	[InlineData(0xfc02, "TILTACUM_L")]
	[InlineData(0xfc52, "MATH_A")]
	[InlineData(0xfc60, "MATH_G")]
	[InlineData(0xfc6c, "MATH_M")]
	public void GetRegisterLabel_SuzyRegisters_ReturnsCorrectLabel(uint address, string expected) {
		var result = _analyzer.GetRegisterLabel(address);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(0xfd00, "TIM0BKUP")]
	[InlineData(0xfd03, "TIM0CTLB")]
	[InlineData(0xfd20, "AUD0VOL")]
	[InlineData(0xfd81, "INTRST")]
	[InlineData(0xfd87, "SYSCTL1")]
	[InlineData(0xfd90, "SDONEACK")]
	[InlineData(0xfd94, "DISPADRL")]
	[InlineData(0xfda0, "GREEN0")]
	[InlineData(0xfdaf, "GREENF")]
	public void GetRegisterLabel_MikeyRegisters_ReturnsCorrectLabel(uint address, string expected) {
		var result = _analyzer.GetRegisterLabel(address);

		Assert.Equal(expected, result);
	}

	[Fact]
	public void GetRegisterLabel_NonRegisterAddress_ReturnsNull() {
		var result = _analyzer.GetRegisterLabel(0x0200); // RAM address

		Assert.Null(result);
	}

	#endregion

	#region GetMemoryRegion Tests

	[Theory]
	[InlineData(0x0000u)]
	[InlineData(0x00ffu)]
	[InlineData(0x0100u)]
	[InlineData(0x01ffu)]
	[InlineData(0x0200u)]
	[InlineData(0xfbffu)]
	public void GetMemoryRegion_RAM_ReturnsRam(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Ram, result);
	}

	[Theory]
	[InlineData(0xfc00u)]
	[InlineData(0xfcffu)]
	[InlineData(0xfd00u)]
	[InlineData(0xfdffu)]
	public void GetMemoryRegion_Hardware_ReturnsHardware(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Hardware, result);
	}

	[Theory]
	[InlineData(0xfe00u)]
	[InlineData(0xffffu)]
	public void GetMemoryRegion_BootROM_ReturnsRom(uint address) {
		var result = _analyzer.GetMemoryRegion(address);

		Assert.Equal(MemoryRegion.Rom, result);
	}

	#endregion

	#region GetEntryPoints Tests

	[Fact]
	public void GetEntryPoints_IncludesStandardEntryPoint() {
		var romData = new byte[1024];

		var entryPoints = _analyzer.GetEntryPoints(romData);

		Assert.Contains(0x0200u, entryPoints);
	}

	#endregion

	#region Platform Properties Tests

	[Fact]
	public void Platform_ReturnsAtariLynx() {
		Assert.Equal("Atari Lynx", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_Is65SC02() {
		Assert.NotNull(_analyzer.CpuDecoder);
		// The decoder should be a 65SC02 decoder
		Assert.Contains("65SC02", _analyzer.CpuDecoder.GetType().Name);
	}

	#endregion
}
