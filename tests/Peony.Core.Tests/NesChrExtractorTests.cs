namespace Peony.Core.Tests;

using Peony.Platform.NES;
using Xunit;

/// <summary>
/// Tests for NES CHR graphics extraction
/// </summary>
public class NesChrExtractorTests {
	private readonly NesChrExtractor _extractor = new();

	[Fact]
	public void Platform_ReturnsNES() {
		Assert.Equal("NES", _extractor.Platform);
	}

	[Fact]
	public void Decode2bppPlanar_DecodesSimpleTile() {
		// Single tile: 16 bytes
		// Low plane: all 0s except bit 0 set for each row
		// High plane: all 0s
		// Result: rightmost pixel should be color 1, rest color 0
		var tile = new byte[16];
		for (int i = 0; i < 8; i++) {
			tile[i] = 0x01; // Low plane: rightmost bit set
			tile[i + 8] = 0x00; // High plane: all clear
		}

		var pixels = TileGraphics.Decode2bppPlanar(tile, 1);

		Assert.Equal(64, pixels.Length);
		// Each row: 7 zeros, then a 1
		for (int row = 0; row < 8; row++) {
			for (int col = 0; col < 7; col++) {
				Assert.Equal(0, pixels[row * 8 + col]);
			}
			Assert.Equal(1, pixels[row * 8 + 7]);
		}
	}

	[Fact]
	public void Decode2bppPlanar_DecodesAllColors() {
		// Create tile with all 4 colors in first row
		// Col 0-1: color 0 (00)
		// Col 2-3: color 1 (01)
		// Col 4-5: color 2 (10)
		// Col 6-7: color 3 (11)
		var tile = new byte[16];
		tile[0] = 0b00110011; // Low plane row 0
		tile[8] = 0b00001111; // High plane row 0

		var pixels = TileGraphics.Decode2bppPlanar(tile, 1);

		// Check first row
		Assert.Equal(0, pixels[0]); // Col 0: 0|0 = 0
		Assert.Equal(0, pixels[1]); // Col 1: 0|0 = 0
		Assert.Equal(1, pixels[2]); // Col 2: 1|0 = 1
		Assert.Equal(1, pixels[3]); // Col 3: 1|0 = 1
		Assert.Equal(2, pixels[4]); // Col 4: 0|1 = 2
		Assert.Equal(2, pixels[5]); // Col 5: 0|1 = 2
		Assert.Equal(3, pixels[6]); // Col 6: 1|1 = 3
		Assert.Equal(3, pixels[7]); // Col 7: 1|1 = 3
	}

	[Fact]
	public void Decode2bppPlanar_HandlesMultipleTiles() {
		// Two tiles, 32 bytes
		var tiles = new byte[32];
		// First tile: all color 1
		for (int i = 0; i < 8; i++) {
			tiles[i] = 0xff;
			tiles[i + 8] = 0x00;
		}
		// Second tile: all color 2
		for (int i = 0; i < 8; i++) {
			tiles[16 + i] = 0x00;
			tiles[16 + i + 8] = 0xff;
		}

		var pixels = TileGraphics.Decode2bppPlanar(tiles, 2);

		Assert.Equal(128, pixels.Length);
		// First tile: all 1s
		for (int i = 0; i < 64; i++) {
			Assert.Equal(1, pixels[i]);
		}
		// Second tile: all 2s
		for (int i = 64; i < 128; i++) {
			Assert.Equal(2, pixels[i]);
		}
	}

	[Fact]
	public void ExtractTiles_Returns2bppData() {
		// Create minimal ROM with header and CHR data
		var rom = CreateTestRom(prgBanks: 1, chrBanks: 1);

		var tileData = _extractor.ExtractTiles(rom, 16 + 16384, 16, 2);

		Assert.Equal(1, tileData.TileCount);
		Assert.Equal(2, tileData.BitDepth);
		Assert.Equal(64, tileData.Pixels.Length);
	}

	[Fact]
	public void ExtractTiles_ThrowsForNon2bpp() {
		var rom = CreateTestRom(prgBanks: 1, chrBanks: 1);

		Assert.Throws<ArgumentException>(() =>
			_extractor.ExtractTiles(rom, 16, 16, 4));
	}

	[Fact]
	public void ExtractAll_NoGraphicsForChrRam() {
		// ROM with CHR-RAM (0 CHR banks)
		var rom = CreateTestRom(prgBanks: 1, chrBanks: 0);

		var result = _extractor.ExtractAll(rom, new GraphicsExtractionOptions {
			OutputDirectory = Path.Combine(Path.GetTempPath(), "peony_test_chr"),
			GenerateMetadata = false
		});

		Assert.Empty(result.TileSets);
		Assert.Equal(0, result.TotalTiles);
	}

	[Fact]
	public void ExtractAll_ExtractsChrBanks() {
		// ROM with 2 CHR banks
		var rom = CreateTestRom(prgBanks: 1, chrBanks: 2);
		var outputDir = Path.Combine(Path.GetTempPath(), $"peony_test_chr_{Guid.NewGuid():N}");

		try {
			var result = _extractor.ExtractAll(rom, new GraphicsExtractionOptions {
				OutputDirectory = outputDir,
				ImageFormat = "bmp",
				GenerateMetadata = true
			});

			Assert.Equal(2, result.TileSets.Count);
			Assert.Equal(512, result.TileSets[0].TileCount); // 8KB / 16 bytes per tile
			Assert.Equal(512, result.TileSets[1].TileCount);
			Assert.True(result.OutputFiles.Count >= 2); // At least 2 images
		} finally {
			if (Directory.Exists(outputDir)) {
				Directory.Delete(outputDir, true);
			}
		}
	}

	[Fact]
	public void ArrangeTiles_CreatesCorrectDimensions() {
		// 4 tiles arranged 2x2
		var pixels = new byte[256]; // 4 tiles * 64 pixels
		var palette = TileGraphics.NesGrayscale;

		var image = TileGraphics.ArrangeTiles(pixels, 4, 2, palette);

		// 2 tiles wide (16 pixels), 2 tiles tall (16 pixels)
		Assert.Equal(16 * 16, image.Length);
	}

	[Fact]
	public void ApplyPalette_MapsColors() {
		var pixels = new byte[] { 0, 1, 2, 3 };
		var palette = new uint[] { 0xff000000, 0xff555555, 0xffaaaaaa, 0xffffffff };

		var result = TileGraphics.ApplyPalette(pixels, palette);

		Assert.Equal(0xff000000u, result[0]);
		Assert.Equal(0xff555555u, result[1]);
		Assert.Equal(0xffaaaaaau, result[2]);
		Assert.Equal(0xffffffffu, result[3]);
	}

	[Fact]
	public void NesPalette_Has64Colors() {
		Assert.Equal(64, TileGraphics.NesPalette.Length);
	}

	[Fact]
	public void NesGrayscale_Has4Colors() {
		Assert.Equal(4, TileGraphics.NesGrayscale.Length);
	}

	/// <summary>
	/// Create a test iNES ROM
	/// </summary>
	private static byte[] CreateTestRom(int prgBanks, int chrBanks) {
		int prgSize = prgBanks * 16384;
		int chrSize = chrBanks * 8192;
		var rom = new byte[16 + prgSize + chrSize];

		// iNES header
		rom[0] = 0x4e; // 'N'
		rom[1] = 0x45; // 'E'
		rom[2] = 0x53; // 'S'
		rom[3] = 0x1a; // EOF
		rom[4] = (byte)prgBanks;
		rom[5] = (byte)chrBanks;

		// Fill CHR with recognizable pattern
		for (int i = 0; i < chrSize; i++) {
			rom[16 + prgSize + i] = (byte)(i % 256);
		}

		return rom;
	}
}
