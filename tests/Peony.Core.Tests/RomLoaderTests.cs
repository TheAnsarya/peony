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
	[InlineData("game.md", "genesis")]
	[InlineData("game.gen", "genesis")]
	[InlineData("game.smd", "genesis")]
	public void DetectPlatform_ByExtension_ReturnsCorrectPlatform(string path, string expected) {
		var rom = new byte[1024]; // Minimal data

		var result = RomLoader.DetectPlatform(rom, path);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("GAME.NES", "nes")]
	[InlineData("GAME.SFC", "snes")]
	[InlineData("game.GBA", "gba")]
	[InlineData("GAME.A26", "atari2600")]
	public void DetectPlatform_ByExtension_CaseInsensitive(string path, string expected) {
		var rom = new byte[1024];

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
	public void DetectPlatform_GbaLogo_ReturnsGba() {
		// GBA ROMs have 0x96 at offset 0xb2
		var rom = new byte[256];
		rom[0xb2] = 0x96;

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("gba", result);
	}

	[Fact]
	public void DetectPlatform_SmallRom_ReturnsAtari2600() {
		var rom = new byte[4096]; // 4KB typical Atari 2600 size

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("atari2600", result);
	}

	[Theory]
	[InlineData(2048)]
	[InlineData(4096)]
	public void DetectPlatform_Atari2600Sizes_ReturnsAtari2600(int size) {
		var rom = new byte[size];

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("atari2600", result);
	}

	[Fact]
	public void DetectPlatform_GenesisHeader_ReturnsGenesis() {
		var rom = new byte[0x400];
		rom[0x100] = (byte)'S';
		rom[0x101] = (byte)'E';
		rom[0x102] = (byte)'G';
		rom[0x103] = (byte)'A';

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("genesis", result);
	}

	[Fact]
	public void DetectPlatform_UnknownFormat_ReturnsUnknown() {
		var rom = new byte[65536]; // 64KB generic

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		Assert.Equal("unknown", result);
	}

	[Fact]
	public void DetectPlatform_EmptyPath_ReturnsUnknown() {
		var rom = new byte[65536];

		var result = RomLoader.DetectPlatform(rom, "");

		Assert.Equal("unknown", result);
	}

	[Fact]
	public void DetectPlatform_NoExtension_FallsBackToMagic() {
		var rom = new byte[128];
		rom[0] = 0x4e; rom[1] = 0x45; rom[2] = 0x53; rom[3] = 0x1a;

		var result = RomLoader.DetectPlatform(rom, "romfile");

		Assert.Equal("nes", result);
	}

	[Fact]
	public void DetectPlatform_TooSmallForGbaLogo_DoesNotDetectGba() {
		// ROM is too small to have the GBA logo check at 0xb2
		var rom = new byte[64];
		// Even if somehow 0x96 appears, the bounds check should prevent detection

		var result = RomLoader.DetectPlatform(rom, "unknown.bin");

		// Should be unknown, not GBA
		Assert.NotEqual("gba", result);
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

	[Fact]
	public void DetectPlatform_GbaExtensionOverridesNesHeader() {
		var rom = new byte[128];
		rom[0] = 0x4e; rom[1] = 0x45; rom[2] = 0x53; rom[3] = 0x1a;

		var result = RomLoader.DetectPlatform(rom, "game.gba");

		Assert.Equal("gba", result);
	}

	#endregion

	#region Load Tests

	[Fact]
	public void Load_MissingFile_ThrowsFileNotFoundException() {
		Assert.Throws<FileNotFoundException>(() => RomLoader.Load("nonexistent_rom.nes"));
	}

	[Fact]
	public void Load_ValidFile_ReturnsBytes() {
		var tempPath = Path.Combine(Path.GetTempPath(), "test_rom.nes");
		try {
			var data = new byte[32];
			data[0] = 0x4e; data[1] = 0x45; data[2] = 0x53; data[3] = 0x1a;
			File.WriteAllBytes(tempPath, data);

			var result = RomLoader.Load(tempPath);

			Assert.Equal(32, result.Length);
			Assert.Equal(0x4e, result[0]); // NES header preserved
		} finally {
			File.Delete(tempPath);
		}
	}

	[Fact]
	public void Load_SnesWithCopierHeader_StripsHeader() {
		var tempPath = Path.Combine(Path.GetTempPath(), "test_rom.sfc");
		try {
			// Create ROM with 512-byte copier header (total size % 1024 == 512)
			var data = new byte[512 + 1024]; // 512 copier + 1024 ROM
			data[0] = 0xAA; // Copier header marker
			data[512] = 0xBB; // First actual ROM byte
			File.WriteAllBytes(tempPath, data);

			var result = RomLoader.Load(tempPath);

			Assert.Equal(1024, result.Length);
			Assert.Equal(0xBB, result[0]); // Copier header stripped
		} finally {
			File.Delete(tempPath);
		}
	}

	[Fact]
	public void Load_SnesWithoutCopierHeader_KeepsData() {
		var tempPath = Path.Combine(Path.GetTempPath(), "test_rom.sfc");
		try {
			// Size % 1024 == 0 means no copier header
			var data = new byte[1024];
			data[0] = 0xCC;
			File.WriteAllBytes(tempPath, data);

			var result = RomLoader.Load(tempPath);

			Assert.Equal(1024, result.Length);
			Assert.Equal(0xCC, result[0]);
		} finally {
			File.Delete(tempPath);
		}
	}

	[Fact]
	public void Load_NesRom_PreservesInesHeader() {
		var tempPath = Path.Combine(Path.GetTempPath(), "test_header.nes");
		try {
			var data = new byte[16 + 16384]; // iNES header + 1 PRG bank
			data[0] = 0x4e; data[1] = 0x45; data[2] = 0x53; data[3] = 0x1a;
			data[4] = 1; // 1 PRG bank
			File.WriteAllBytes(tempPath, data);

			var result = RomLoader.Load(tempPath);

			// NES iNES header should be preserved
			Assert.Equal(data.Length, result.Length);
			Assert.Equal(0x4e, result[0]);
		} finally {
			File.Delete(tempPath);
		}
	}

	#endregion
}
