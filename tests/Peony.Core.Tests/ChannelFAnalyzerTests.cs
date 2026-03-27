using Peony.Core;
using Peony.Platform.ChannelF;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for the Channel F platform analyzer
/// </summary>
public class ChannelFAnalyzerTests {
	private readonly ChannelFAnalyzer _analyzer = new();

	// ========== Platform Properties ==========

	[Fact]
	public void Platform_ReturnsChannelF() {
		Assert.Equal("Channel F", _analyzer.Platform);
	}

	[Fact]
	public void CpuDecoder_IsF8() {
		Assert.Equal("F8", _analyzer.CpuDecoder.Architecture);
	}

	[Fact]
	public void BankCount_Is1() {
		Assert.Equal(1, _analyzer.BankCount);
	}

	[Fact]
	public void RomDataOffset_Is0() {
		Assert.Equal(0, _analyzer.RomDataOffset);
	}

	// ========== Analyze ==========

	[Fact]
	public void Analyze_2KCart_IdentifiesCorrectly() {
		var rom = new byte[2048];
		var info = _analyzer.Analyze(rom);

		Assert.Equal("Channel F", info.Platform);
		Assert.Equal(2048, info.Size);
		Assert.Null(info.Mapper);
		Assert.Contains("2K Cartridge", info.Metadata["RomType"]);
	}

	[Fact]
	public void Analyze_6KCart_IdentifiesCorrectly() {
		var rom = new byte[6144]; // 6K
		var info = _analyzer.Analyze(rom);

		Assert.Contains("6K Cartridge", info.Metadata["RomType"]);
	}

	[Fact]
	public void Analyze_BiosRom_IdentifiesCorrectly() {
		var rom = new byte[1024]; // 1K BIOS
		var info = _analyzer.Analyze(rom);

		Assert.Contains("BIOS ROM 1", info.Metadata["RomType"]);
	}

	[Fact]
	public void Analyze_FullBios_IdentifiesCorrectly() {
		var rom = new byte[2048]; // 2K BIOS set
		var info = _analyzer.Analyze(rom);

		Assert.Equal("Channel F", info.Platform);
	}

	// ========== Memory Regions ==========

	[Theory]
	[InlineData(0x0000u, MemoryRegion.Rom)]      // BIOS ROM 1 start
	[InlineData(0x03ffu, MemoryRegion.Rom)]      // BIOS ROM 1 end
	[InlineData(0x0400u, MemoryRegion.Rom)]      // BIOS ROM 2 start
	[InlineData(0x07ffu, MemoryRegion.Rom)]      // BIOS ROM 2 end
	[InlineData(0x0800u, MemoryRegion.Rom)]      // Cart ROM start
	[InlineData(0x17ffu, MemoryRegion.Rom)]      // Cart ROM end
	public void GetMemoryRegion_RomAddresses(uint address, MemoryRegion expected) {
		Assert.Equal(expected, _analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x2800u, MemoryRegion.Ram)]
	[InlineData(0x2fffu, MemoryRegion.Ram)]
	public void GetMemoryRegion_RamAddresses(uint address, MemoryRegion expected) {
		Assert.Equal(expected, _analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x3000u, MemoryRegion.Graphics)] // VRAM start
	[InlineData(0x37ffu, MemoryRegion.Graphics)] // VRAM end
	public void GetMemoryRegion_VramAddresses(uint address, MemoryRegion expected) {
		Assert.Equal(expected, _analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x3800u, MemoryRegion.Hardware)] // I/O start
	[InlineData(0x38ffu, MemoryRegion.Hardware)] // I/O end
	public void GetMemoryRegion_IoAddresses(uint address, MemoryRegion expected) {
		Assert.Equal(expected, _analyzer.GetMemoryRegion(address));
	}

	[Theory]
	[InlineData(0x1800u)]  // Gap between cart ROM and RAM
	[InlineData(0x27ffu)]  // Just before RAM
	[InlineData(0x4000u)]  // Above I/O
	public void GetMemoryRegion_UnmappedAddresses(uint address) {
		Assert.Equal(MemoryRegion.Unknown, _analyzer.GetMemoryRegion(address));
	}

	// ========== Register Labels ==========

	[Theory]
	[InlineData(0x00u, "PORT0_CONSOLE")]
	[InlineData(0x01u, "PORT1_RIGHT")]
	[InlineData(0x04u, "PORT4_LEFT")]
	[InlineData(0x05u, "PORT5_SOUND")]
	public void GetRegisterLabel_KnownPorts(uint port, string expected) {
		Assert.Equal(expected, _analyzer.GetRegisterLabel(port));
	}

	[Fact]
	public void GetRegisterLabel_UnknownPort_ReturnsNull() {
		Assert.Null(_analyzer.GetRegisterLabel(0x10));
	}

	// ========== Entry Points ==========

	[Fact]
	public void GetEntryPoints_CartridgeRom_Returns0x0800() {
		var rom = new byte[4096]; // Cart ROM (includes BIOS + cart area)
		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Single(entries);
		Assert.Equal(0x0800u, entries[0]);
	}

	[Fact]
	public void GetEntryPoints_BiosOnly_Returns0x0000() {
		var rom = new byte[1024]; // BIOS only, no cart
		var entries = _analyzer.GetEntryPoints(rom);

		Assert.Single(entries);
		Assert.Equal(0x0000u, entries[0]);
	}

	// ========== Address Mapping ==========

	[Fact]
	public void AddressToOffset_FullRom_DirectMapping() {
		Assert.Equal(0x0800, _analyzer.AddressToOffset(0x0800, 0x1800));
	}

	[Fact]
	public void AddressToOffset_CartOnly_SubtractsBase() {
		// 2K cart-only ROM, address $0900 maps to offset $0100
		Assert.Equal(0x0100, _analyzer.AddressToOffset(0x0900, 0x0800));
	}

	[Fact]
	public void AddressToOffset_OutOfRange_ReturnsNeg1() {
		Assert.Equal(-1, _analyzer.AddressToOffset(0x5000, 0x1800));
	}

	[Fact]
	public void OffsetToAddress_DirectMapping() {
		Assert.Equal(0x0800u, _analyzer.OffsetToAddress(0x0800));
	}

	// ========== Bank Switching ==========

	[Fact]
	public void IsInSwitchableRegion_AlwaysFalse() {
		Assert.False(_analyzer.IsInSwitchableRegion(0x0800));
		Assert.False(_analyzer.IsInSwitchableRegion(0x0000));
	}

	[Fact]
	public void DetectBankSwitch_AlwaysNull() {
		var rom = new byte[2048];
		Assert.Null(_analyzer.DetectBankSwitch(rom, 0x0800, 0));
	}
}
