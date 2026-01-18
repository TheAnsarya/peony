namespace Peony.Platform.SNES.Tests;

using Peony.Core;
using Xunit;

public class SnesChrExtractorTests {
	[Fact]
	public void Decode4bppSnesPlanar_SingleTile_DecodesCorrectly() {
		// 4bpp SNES tile: 32 bytes
		// First 16 bytes: planes 0,1 interleaved by row
		// Last 16 bytes: planes 2,3 interleaved by row
		var data = new byte[32];

		// Row 0: plane0=0xFF, plane1=0x00, plane2=0x00, plane3=0x00
		// Should give all pixels = 0001 (binary) = 1
		data[0] = 0xff;  // plane 0, row 0
		data[1] = 0x00;  // plane 1, row 0
		data[16] = 0x00; // plane 2, row 0
		data[17] = 0x00; // plane 3, row 0

		var pixels = SnesChrExtractor.Decode4bppSnesPlanar(data, 1);

		// First row should all be 1
		for (int i = 0; i < 8; i++) {
			Assert.Equal(1, pixels[i]);
		}
	}

	[Fact]
	public void Decode4bppSnesPlanar_Color15_DecodesCorrectly() {
		// All planes set = color 15 (0b1111)
		var data = new byte[32];

		// Row 0: all planes set
		data[0] = 0xff;  // plane 0
		data[1] = 0xff;  // plane 1
		data[16] = 0xff; // plane 2
		data[17] = 0xff; // plane 3

		var pixels = SnesChrExtractor.Decode4bppSnesPlanar(data, 1);

		// First row should all be 15
		for (int i = 0; i < 8; i++) {
			Assert.Equal(15, pixels[i]);
		}
	}

	[Fact]
	public void Decode4bppSnesPlanar_Checkerboard_DecodesCorrectly() {
		// Alternating pattern: 0xAA = 10101010
		var data = new byte[32];
		data[0] = 0xaa;  // plane 0 = 10101010
		data[1] = 0x00;  // plane 1
		data[16] = 0x00; // plane 2
		data[17] = 0x00; // plane 3

		var pixels = SnesChrExtractor.Decode4bppSnesPlanar(data, 1);

		// Pattern should be 1,0,1,0,1,0,1,0
		Assert.Equal(1, pixels[0]);
		Assert.Equal(0, pixels[1]);
		Assert.Equal(1, pixels[2]);
		Assert.Equal(0, pixels[3]);
	}

	[Fact]
	public void Decode8bppSnesPlanar_SingleTile_DecodesCorrectly() {
		// 8bpp SNES tile: 64 bytes (4 pairs of 2bpp planes)
		var data = new byte[64];

		// Set plane 0 only = color 1
		data[0] = 0xff;

		var pixels = SnesChrExtractor.Decode8bppSnesPlanar(data, 1);

		// First row should all be 1
		for (int i = 0; i < 8; i++) {
			Assert.Equal(1, pixels[i]);
		}
	}

	[Fact]
	public void Decode8bppSnesPlanar_Color255_DecodesCorrectly() {
		// All 8 planes set = color 255
		var data = new byte[64];

		for (int row = 0; row < 8; row++) {
			data[row * 2] = 0xff;      // plane 0
			data[row * 2 + 1] = 0xff;  // plane 1
			data[16 + row * 2] = 0xff; // plane 2
			data[16 + row * 2 + 1] = 0xff; // plane 3
			data[32 + row * 2] = 0xff; // plane 4
			data[32 + row * 2 + 1] = 0xff; // plane 5
			data[48 + row * 2] = 0xff; // plane 6
			data[48 + row * 2 + 1] = 0xff; // plane 7
		}

		var pixels = SnesChrExtractor.Decode8bppSnesPlanar(data, 1);

		// All pixels should be 255
		for (int i = 0; i < 64; i++) {
			Assert.Equal(255, pixels[i]);
		}
	}

	[Fact]
	public void ExtractTiles2bpp_ReturnsCorrectTileCount() {
		// 5 tiles worth of data
		var data = new byte[16 * 5];
		var result = SnesChrExtractor.ExtractTiles2bpp(data);

		Assert.Equal(5, result.TileCount);
		Assert.Equal(2, result.BitDepth);
	}

	[Fact]
	public void ExtractTiles4bpp_ReturnsCorrectTileCount() {
		// 5 tiles worth of data
		var data = new byte[32 * 5];
		var result = SnesChrExtractor.ExtractTiles4bpp(data);

		Assert.Equal(5, result.TileCount);
		Assert.Equal(4, result.BitDepth);
	}

	[Fact]
	public void ExtractTiles8bpp_ReturnsCorrectTileCount() {
		// 5 tiles worth of data
		var data = new byte[64 * 5];
		var result = SnesChrExtractor.ExtractTiles8bpp(data);

		Assert.Equal(5, result.TileCount);
		Assert.Equal(8, result.BitDepth);
	}

	[Fact]
	public void ExtractPalette_Bgr555ToArgb_Black() {
		var data = new byte[] { 0x00, 0x00 }; // BGR555 black
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 1);

		Assert.Equal(0xff000000u, colors[0]);
	}

	[Fact]
	public void ExtractPalette_Bgr555ToArgb_White() {
		var data = new byte[] { 0xff, 0x7f }; // BGR555 white (0x7FFF)
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 1);

		// Should be close to white
		Assert.Equal(0xffffffffu, colors[0]);
	}

	[Fact]
	public void ExtractPalette_Bgr555ToArgb_Red() {
		var data = new byte[] { 0x1f, 0x00 }; // BGR555 red (R=31, G=0, B=0)
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 1);

		// Red channel should be max, others 0
		uint red = (colors[0] >> 16) & 0xff;
		uint green = (colors[0] >> 8) & 0xff;
		uint blue = colors[0] & 0xff;

		Assert.Equal(255u, red);
		Assert.Equal(0u, green);
		Assert.Equal(0u, blue);
	}

	[Fact]
	public void ExtractPalette_Bgr555ToArgb_Green() {
		var data = new byte[] { 0xe0, 0x03 }; // BGR555 green (R=0, G=31, B=0) = 0x03E0
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 1);

		uint red = (colors[0] >> 16) & 0xff;
		uint green = (colors[0] >> 8) & 0xff;
		uint blue = colors[0] & 0xff;

		Assert.Equal(0u, red);
		Assert.Equal(255u, green);
		Assert.Equal(0u, blue);
	}

	[Fact]
	public void ExtractPalette_Bgr555ToArgb_Blue() {
		var data = new byte[] { 0x00, 0x7c }; // BGR555 blue (R=0, G=0, B=31) = 0x7C00
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 1);

		uint red = (colors[0] >> 16) & 0xff;
		uint green = (colors[0] >> 8) & 0xff;
		uint blue = colors[0] & 0xff;

		Assert.Equal(0u, red);
		Assert.Equal(0u, green);
		Assert.Equal(255u, blue);
	}

	[Fact]
	public void ExtractPalette_MultipleColors() {
		var data = new byte[] {
			0x00, 0x00, // Black
			0x1f, 0x00, // Red
			0xe0, 0x03, // Green
			0x00, 0x7c  // Blue
		};
		var colors = SnesChrExtractor.ExtractPalette(data, 0, 4);

		Assert.Equal(4, colors.Length);
	}

	[Fact]
	public void Bgr555ToArgb_MidGray() {
		// Mid gray: R=15, G=15, B=15 = 0x3DEF
		ushort gray = 0x3def;
		var argb = SnesChrExtractor.Bgr555ToArgb(gray);

		uint red = (argb >> 16) & 0xff;
		uint green = (argb >> 8) & 0xff;
		uint blue = argb & 0xff;

		// Should be around 123 (15 * 8 + 15/4 ≈ 123)
		Assert.True(red >= 120 && red <= 130);
		Assert.True(green >= 120 && green <= 130);
		Assert.True(blue >= 120 && blue <= 130);
	}

	[Fact]
	public void GetGrayscalePalette_4Colors() {
		var palette = SnesChrExtractor.GetGrayscalePalette(4);

		Assert.Equal(4, palette.Length);
		Assert.Equal(0xff000000u, palette[0]); // Black
		Assert.Equal(0xffffffffu, palette[3]); // White
	}

	[Fact]
	public void GetGrayscalePalette_16Colors() {
		var palette = SnesChrExtractor.GetGrayscalePalette(16);

		Assert.Equal(16, palette.Length);
		Assert.Equal(0xff000000u, palette[0]);  // Black
		Assert.Equal(0xffffffffu, palette[15]); // White
	}

	[Fact]
	public void ExtractTiles_WithBitDepthParameter() {
		var extractor = new SnesChrExtractor();
		var data = new byte[128]; // 4 4bpp tiles

		var result = extractor.ExtractTiles(data, 0, 128, 4);

		Assert.Equal(4, result.TileCount);
		Assert.Equal(4, result.BitDepth);
	}

	[Fact]
	public void ExtractTiles_ClampsSizeToRomLength() {
		var extractor = new SnesChrExtractor();
		var data = new byte[64]; // Only 2 4bpp tiles

		// Request more than available
		var result = extractor.ExtractTiles(data, 0, 256, 4);

		Assert.Equal(2, result.TileCount);
	}

	[Fact]
	public void Mode7_ExtractTiles_LinearFormat() {
		// Mode 7 tiles are linear 8bpp (1 byte per pixel)
		var data = new byte[64]; // One 8x8 tile
		for (int i = 0; i < 64; i++) {
			data[i] = (byte)(i % 256);
		}

		var result = SnesMode7Extractor.ExtractMode7Tiles(data, 0, 1);

		Assert.Equal(1, result.TileCount);
		Assert.Equal(8, result.BitDepth);
	}

	[Fact]
	public void Mode7_ExtractTilemap_CorrectSize() {
		var data = new byte[128 * 128 + 100];
		var tilemap = SnesMode7Extractor.ExtractMode7Tilemap(data, 0);

		Assert.Equal(128 * 128, tilemap.Length);
	}

	[Fact]
	public void Sprite_Extract16x16_CombinesTiles() {
		// 4 8x8 tiles = 128 bytes
		var data = new byte[128];

		// Fill each tile with different value to verify arrangement
		for (int t = 0; t < 4; t++) {
			var tilePixels = new byte[32];
			// Set plane 0 to create visible pattern
			for (int r = 0; r < 8; r++) {
				tilePixels[r * 2] = (byte)((t + 1) * 0x11); // Different pattern per tile
			}
			Array.Copy(tilePixels, 0, data, t * 32, 32);
		}

		var pixels = SnesSpriteExtractor.Extract16x16Sprite(data, 0);

		Assert.Equal(256, pixels.Length); // 16x16
	}

	[Fact]
	public void Sprite_ExtractTiles_Uses4bpp() {
		var data = new byte[128]; // 4 tiles
		var result = SnesSpriteExtractor.ExtractSpriteTiles(data, 0, 128);

		Assert.Equal(4, result.BitDepth);
	}
}
