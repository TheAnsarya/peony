using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for PlatformMemoryMap — address classification, register names, and vectors.
/// </summary>
[Collection("PlatformResolver")]
public class PlatformMemoryMapTests {
	// ========================================================================
	// NES Classification
	// ========================================================================

	[Theory]
	[InlineData(0x0000u, ByteClassification.Data)]
	[InlineData(0x07ffu, ByteClassification.Data)]
	[InlineData(0x0800u, ByteClassification.Data)]
	[InlineData(0x1fffu, ByteClassification.Data)]
	[InlineData(0x6000u, ByteClassification.Data)]
	[InlineData(0x7fffu, ByteClassification.Data)]
	public void Nes_RamAddresses_ClassifiedAsData(uint address, ByteClassification expected) {
		Assert.Equal(expected, PlatformMemoryMap.GetKnownClassification("NES", address));
	}

	[Theory]
	[InlineData(0x2000u)]
	[InlineData(0x2007u)]
	[InlineData(0x2008u)]
	[InlineData(0x3fffu)]
	[InlineData(0x4000u)]
	[InlineData(0x4017u)]
	public void Nes_HardwareRegisters_ClassifiedAsHardware(uint address) {
		Assert.Equal(ByteClassification.Hardware, PlatformMemoryMap.GetKnownClassification("NES", address));
	}

	[Theory]
	[InlineData(0x8000u)]
	[InlineData(0xffffu)]
	public void Nes_PrgRom_ReturnsNull(uint address) {
		Assert.Null(PlatformMemoryMap.GetKnownClassification("NES", address));
	}

	// ========================================================================
	// NES Register Names
	// ========================================================================

	[Theory]
	[InlineData(0x2000u, "PPUCTRL")]
	[InlineData(0x2001u, "PPUMASK")]
	[InlineData(0x2002u, "PPUSTATUS")]
	[InlineData(0x4014u, "OAMDMA")]
	[InlineData(0x4015u, "SND_CHN")]
	[InlineData(0x4016u, "JOY1")]
	[InlineData(0x4017u, "JOY2")]
	public void Nes_GetHardwareRegisterName_ReturnsCorrectName(uint address, string expected) {
		Assert.Equal(expected, PlatformMemoryMap.GetHardwareRegisterName("NES", address));
	}

	[Fact]
	public void Nes_UnknownAddress_ReturnsNullRegisterName() {
		Assert.Null(PlatformMemoryMap.GetHardwareRegisterName("NES", 0x8000));
	}

	// ========================================================================
	// NES Vectors
	// ========================================================================

	[Fact]
	public void Nes_GetVectors_ReturnsThreeVectors() {
		var vectors = PlatformMemoryMap.GetVectors("NES");
		Assert.Equal(3, vectors.Count);
		Assert.Contains(vectors, v => v.Name == "NMI" && v.Address == 0xfffa);
		Assert.Contains(vectors, v => v.Name == "RESET" && v.Address == 0xfffc);
		Assert.Contains(vectors, v => v.Name == "IRQ" && v.Address == 0xfffe);
	}

	// ========================================================================
	// SNES Classification
	// ========================================================================

	[Theory]
	[InlineData(0x0000u)]
	[InlineData(0x1fffu)]
	public void Snes_Wram_ClassifiedAsData(uint address) {
		Assert.Equal(ByteClassification.Data, PlatformMemoryMap.GetKnownClassification("SNES", address));
	}

	[Theory]
	[InlineData(0x2100u)]
	[InlineData(0x213fu)]
	[InlineData(0x2140u)]
	[InlineData(0x2143u)]
	[InlineData(0x4200u)]
	[InlineData(0x421fu)]
	public void Snes_HardwareRegisters_ClassifiedAsHardware(uint address) {
		Assert.Equal(ByteClassification.Hardware, PlatformMemoryMap.GetKnownClassification("SNES", address));
	}

	[Fact]
	public void Snes_GetVectors_ContainsResetAndNmi() {
		var vectors = PlatformMemoryMap.GetVectors("SNES");
		Assert.True(vectors.Count >= 4);
		Assert.Contains(vectors, v => v.Name == "RESET" && v.Address == 0xfffc);
		Assert.Contains(vectors, v => v.Name == "NMI" && v.Address == 0xffea);
		Assert.Contains(vectors, v => v.Name == "IRQ" && v.Address == 0xffee);
	}

	// ========================================================================
	// Game Boy Classification
	// ========================================================================

	[Theory]
	[InlineData(0x8000u)]
	[InlineData(0x9fffu)]
	public void GameBoy_Vram_ClassifiedAsGraphics(uint address) {
		Assert.Equal(ByteClassification.Graphics, PlatformMemoryMap.GetKnownClassification("GameBoy", address));
	}

	[Theory]
	[InlineData(0xc000u)]
	[InlineData(0xdfffu)]
	[InlineData(0xa000u)]
	public void GameBoy_Ram_ClassifiedAsData(uint address) {
		Assert.Equal(ByteClassification.Data, PlatformMemoryMap.GetKnownClassification("GameBoy", address));
	}

	[Theory]
	[InlineData(0xff00u)]
	[InlineData(0xff7fu)]
	[InlineData(0xffffu)]
	public void GameBoy_IoRegisters_ClassifiedAsHardware(uint address) {
		Assert.Equal(ByteClassification.Hardware, PlatformMemoryMap.GetKnownClassification("GameBoy", address));
	}

	[Theory]
	[InlineData(0xff00u, "JOYP")]
	[InlineData(0xff40u, "LCDC")]
	[InlineData(0xff44u, "LY")]
	[InlineData(0xff46u, "DMA")]
	[InlineData(0xffffu, "IE")]
	public void GameBoy_GetHardwareRegisterName_ReturnsCorrectName(uint address, string expected) {
		Assert.Equal(expected, PlatformMemoryMap.GetHardwareRegisterName("GameBoy", address));
	}

	[Fact]
	public void GameBoy_GetVectors_ContainsEntryAndInterrupts() {
		var vectors = PlatformMemoryMap.GetVectors("GameBoy");
		Assert.True(vectors.Count >= 5);
		Assert.Contains(vectors, v => v.Name == "ENTRY_POINT" && v.Address == 0x0100);
		Assert.Contains(vectors, v => v.Name == "VBLANK_ISR" && v.Address == 0x0040);
	}

	// ========================================================================
	// GBA Classification
	// ========================================================================

	[Theory]
	[InlineData(0x00000000u)]
	[InlineData(0x00003fffu)]
	public void Gba_Bios_ClassifiedAsCode(uint address) {
		Assert.Equal(ByteClassification.Code, PlatformMemoryMap.GetKnownClassification("GBA", address));
	}

	[Theory]
	[InlineData(0x02000000u)]
	[InlineData(0x03000000u)]
	public void Gba_Ram_ClassifiedAsData(uint address) {
		Assert.Equal(ByteClassification.Data, PlatformMemoryMap.GetKnownClassification("GBA", address));
	}

	[Fact]
	public void Gba_IoRegisters_ClassifiedAsHardware() {
		Assert.Equal(ByteClassification.Hardware, PlatformMemoryMap.GetKnownClassification("GBA", 0x04000000));
	}

	[Fact]
	public void Gba_Vram_ClassifiedAsGraphics() {
		Assert.Equal(ByteClassification.Graphics, PlatformMemoryMap.GetKnownClassification("GBA", 0x06000000));
	}

	// ========================================================================
	// Atari 2600 Classification
	// ========================================================================

	[Theory]
	[InlineData(0x0000u)]
	[InlineData(0x002cu)]
	[InlineData(0x0030u)]
	[InlineData(0x003du)]
	[InlineData(0x0280u)]
	public void Atari2600_TiaAndRiot_ClassifiedAsHardware(uint address) {
		Assert.Equal(ByteClassification.Hardware, PlatformMemoryMap.GetKnownClassification("Atari2600", address));
	}

	[Fact]
	public void Atari2600_RiotRam_ClassifiedAsData() {
		Assert.Equal(ByteClassification.Data, PlatformMemoryMap.GetKnownClassification("Atari2600", 0x0080));
	}

	[Theory]
	[InlineData(0x0000u, "VSYNC")]
	[InlineData(0x0002u, "WSYNC")]
	[InlineData(0x000du, "PF0")]
	[InlineData(0x001bu, "GRP0")]
	public void Atari2600_GetHardwareRegisterName_ReturnsCorrectName(uint address, string expected) {
		Assert.Equal(expected, PlatformMemoryMap.GetHardwareRegisterName("Atari2600", address));
	}

	[Fact]
	public void Atari2600_GetVectors_ReturnsThreeVectors() {
		var vectors = PlatformMemoryMap.GetVectors("Atari2600");
		Assert.Equal(3, vectors.Count);
		Assert.Contains(vectors, v => v.Name == "RESET");
	}

	// ========================================================================
	// Edge Cases
	// ========================================================================

	[Fact]
	public void UnknownPlatform_ReturnsNull() {
		Assert.Null(PlatformMemoryMap.GetKnownClassification("UnknownSystem", 0x0000));
	}

	[Fact]
	public void UnknownPlatform_ReturnsNullRegisterName() {
		Assert.Null(PlatformMemoryMap.GetHardwareRegisterName("UnknownSystem", 0x2000));
	}

	[Fact]
	public void UnknownPlatform_ReturnsEmptyVectors() {
		var vectors = PlatformMemoryMap.GetVectors("UnknownSystem");
		Assert.Empty(vectors);
	}

	[Fact]
	public void CaseInsensitive_PlatformResolution() {
		// Should resolve via PlatformResolver (case-insensitive)
		Assert.NotNull(PlatformMemoryMap.GetKnownClassification("nes", 0x0000));
		Assert.NotNull(PlatformMemoryMap.GetKnownClassification("NES", 0x0000));
	}
}
