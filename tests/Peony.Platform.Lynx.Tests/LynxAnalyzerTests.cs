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

	#region Memory Mapping Tests

	[Fact]
	public void OffsetToAddress_WithoutHeader_MapsToRamLoadAddress() {
		// Raw ROM without LNX header - offset 0 maps to $0200
		var romData = new byte[1024];
		_analyzer.Analyze(romData);

		var address = _analyzer.OffsetToAddress(0);

		Assert.Equal(0x0200u, address);
	}

	[Fact]
	public void OffsetToAddress_WithLnxHeader_MapsAfterHeader() {
		// LNX header is 64 bytes, so ROM data starts at offset 64
		var romData = new byte[128];
		// LNX magic
		romData[0] = 0x4c; // L
		romData[1] = 0x59; // Y
		romData[2] = 0x4e; // N
		romData[3] = 0x58; // X
		// Bank0 size (little-endian)
		romData[4] = 0x40; // 64 bytes
		romData[5] = 0x00;

		_analyzer.Analyze(romData);

		// Offset 64 (start of ROM data after header) → $0200
		var address = _analyzer.OffsetToAddress(64);

		Assert.Equal(0x0200u, address);
	}

	[Theory]
	[InlineData(0, 0x0200u)]      // First byte of ROM → RAM position
	[InlineData(100, 0x0264u)]    // 100 bytes in → $0200 + 100
	[InlineData(0x1000, 0x1200u)] // 4KB in → $0200 + 0x1000
	public void OffsetToAddress_WithoutHeader_CalculatesCorrectly(int offset, uint expectedAddress) {
		var romData = new byte[0x10000]; // 64KB bare ROM
		_analyzer.Analyze(romData);

		var address = _analyzer.OffsetToAddress(offset);

		Assert.Equal(expectedAddress, address);
	}

	[Theory]
	[InlineData(0x0200u, 0)]      // Start of loaded ROM
	[InlineData(0x0264u, 100)]    // $0200 + 100
	[InlineData(0x1200u, 0x1000)] // $0200 + 0x1000
	public void AddressToOffset_WithoutHeader_CalculatesCorrectly(uint address, int expectedOffset) {
		var romData = new byte[0x10000]; // 64KB bare ROM
		_analyzer.Analyze(romData);

		var offset = _analyzer.AddressToOffset(address, romData.Length);

		Assert.Equal(expectedOffset, offset);
	}

	[Fact]
	public void AddressToOffset_BelowLoadAddress_ReturnsMinusOne() {
		var romData = new byte[1024];
		_analyzer.Analyze(romData);

		var offset = _analyzer.AddressToOffset(0x01ff, romData.Length);

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void AddressToOffset_InHardwareRegion_ReturnsMinusOne() {
		var romData = new byte[1024];
		_analyzer.Analyze(romData);

		var offset = _analyzer.AddressToOffset(0xfc00, romData.Length);

		Assert.Equal(-1, offset);
	}

	[Fact]
	public void RomDataOffset_WithoutHeader_ReturnsZero() {
		var romData = new byte[1024]; // No LNX header
		_analyzer.Analyze(romData);

		Assert.Equal(0, _analyzer.RomDataOffset);
	}

	[Fact]
	public void RomDataOffset_WithLnxHeader_Returns64() {
		var romData = new byte[128];
		// LNX magic
		romData[0] = 0x4c; // L
		romData[1] = 0x59; // Y
		romData[2] = 0x4e; // N
		romData[3] = 0x58; // X
		romData[4] = 0x40; // Bank0 size: 64 bytes
		romData[5] = 0x00;

		_analyzer.Analyze(romData);

		Assert.Equal(64, _analyzer.RomDataOffset);
	}

	#endregion
}
