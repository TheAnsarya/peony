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

	#region BMP Export Tests

	[Fact]
	public void SaveToBmp_CreatesValidBmpHeader() {
		// Create 2x2 image
		uint[] pixels = [0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFFFFFFFF];

		var bmp = TileGraphics.SaveToBmp(pixels, 2, 2);

		// BMP header checks
		Assert.Equal((byte)'B', bmp[0]);
		Assert.Equal((byte)'M', bmp[1]);

		// DIB header size at offset 14
		int dibSize = BitConverter.ToInt32(bmp, 14);
		Assert.Equal(40, dibSize);

		// Width at offset 18
		int width = BitConverter.ToInt32(bmp, 18);
		Assert.Equal(2, width);

		// Height at offset 22
		int height = BitConverter.ToInt32(bmp, 22);
		Assert.Equal(2, height);

		// Bits per pixel at offset 28
		short bpp = BitConverter.ToInt16(bmp, 28);
		Assert.Equal(24, bpp);
	}

	[Fact]
	public void SaveToBmp_CorrectFileSize() {
		// 8x8 image = 64 pixels at 24bpp = 192 bytes data + 54 header
		// Row size = 8 * 3 = 24 bytes (already 4-byte aligned)
		// Total = 54 + (24 * 8) = 246 bytes
		uint[] pixels = new uint[64];
		Array.Fill(pixels, 0xFF000000u);

		var bmp = TileGraphics.SaveToBmp(pixels, 8, 8);

		Assert.Equal(246, bmp.Length);

		// File size in header
		int fileSize = BitConverter.ToInt32(bmp, 2);
		Assert.Equal(246, fileSize);
	}

	[Fact]
	public void SaveToBmp_CorrectPixelDataOffset() {
		uint[] pixels = [0xFF000000];

		var bmp = TileGraphics.SaveToBmp(pixels, 1, 1);

		int offset = BitConverter.ToInt32(bmp, 10);
		Assert.Equal(54, offset); // Standard BMP header size
	}

	[Fact]
	public void ExportTilesToBmp_CreatesBmpFromIndexedPixels() {
		// 1 tile = 64 indexed pixels
		var pixels = new byte[64];
		Array.Fill(pixels, (byte)1); // All color index 1

		var bmp = TileGraphics.ExportTilesToBmp(pixels, 1, 1, TileGraphics.NesGrayscale);

		// Verify BMP header
		Assert.Equal((byte)'B', bmp[0]);
		Assert.Equal((byte)'M', bmp[1]);

		// Should be 8x8 image
		int width = BitConverter.ToInt32(bmp, 18);
		int height = BitConverter.ToInt32(bmp, 22);
		Assert.Equal(8, width);
		Assert.Equal(8, height);
	}

	[Fact]
	public void ExportTilesToBmp_MultipleRows() {
		// 4 tiles arranged as 2x2 grid
		var pixels = new byte[256]; // 4 tiles * 64 pixels
		Array.Fill(pixels, (byte)2);

		var bmp = TileGraphics.ExportTilesToBmp(pixels, 4, 2, TileGraphics.NesGrayscale);

		int width = BitConverter.ToInt32(bmp, 18);
		int height = BitConverter.ToInt32(bmp, 22);
		Assert.Equal(16, width);  // 2 tiles * 8 pixels
		Assert.Equal(16, height); // 2 rows * 8 pixels
	}

	[Fact]
	public void SaveToBmp_RowPaddingIsCorrect() {
		// 5-pixel wide image: 5 * 3 = 15 bytes per row, needs 1 byte padding
		uint[] pixels = new uint[5];
		Array.Fill(pixels, 0xFFFF0000u);

		var bmp = TileGraphics.SaveToBmp(pixels, 5, 1);

		// Row size: 5*3 = 15, padding = (4 - 15%4) % 4 = 1
		// Pixel data size: 16 * 1 = 16
		// Total size: 54 + 16 = 70
		Assert.Equal(70, bmp.Length);
	}

	#endregion

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
