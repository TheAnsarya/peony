using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for <see cref="RomLoader"/>.
/// </summary>
public class RomLoaderTests {
	#region DetectPlatform Extension Tests

	[Theory]
	[InlineData("game.a26", "atari2600")]
	[InlineData("game.lnx", "lynx")]
	[InlineData("game.lyx", "lynx")]
	[InlineData("game.nes", "nes")]
	[InlineData("game.sfc", "snes")]
	[InlineData("game.smc", "snes")]
	[InlineData("game.gb", "gb")]
	[InlineData("game.gbc", "gb")]
	[InlineData("game.gba", "gba")]
	public void DetectPlatform_ByExtension_ReturnsCorrectPlatform(string path, string expected) {
		var rom = new byte[1024]; // Minimal data

		var result = RomLoader.DetectPlatform(rom, path);

		Assert.Equal(expected, result);
	}

	#endregion

	#region DetectPlatform Magic Bytes Tests

	[Fact]
	public void DetectPlatform_LynxMagicBytes_ReturnsLynx() {
		var rom = new byte[128];
		rom[0] = (byte)'L';
		rom[1] = (byte)'Y';
		rom[2] = (byte)'N';
		rom[3] = (byte)'X';

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("lynx", result);
	}

	[Fact]
	public void DetectPlatform_NesHeader_ReturnsNes() {
		var rom = new byte[128];
		rom[0] = 0x4e; // N
		rom[1] = 0x45; // E
		rom[2] = 0x53; // S
		rom[3] = 0x1a; // EOF

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("nes", result);
	}

	[Fact]
	public void DetectPlatform_SmallRom_ReturnsAtari2600() {
		var rom = new byte[4096]; // 4KB typical Atari 2600 size

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("atari2600", result);
	}

	[Fact]
	public void DetectPlatform_UnknownFormat_ReturnsUnknown() {
		var rom = new byte[65536]; // 64KB generic

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("unknown", result);
	}

	#endregion

	#region Extension Takes Priority

	[Fact]
	public void DetectPlatform_ExtensionOverridesMagic_WhenExtensionMatches() {
		// ROM has NES magic but .lnx extension
		var rom = new byte[128];
		rom[0] = 0x4e;
		rom[1] = 0x45;
		rom[2] = 0x53;
		rom[3] = 0x1a;

		var result = RomLoader.DetectPlatform(rom, "game.lnx");

		// Extension takes priority
		Assert.Equal("lynx", result);
	}

	#endregion
}
