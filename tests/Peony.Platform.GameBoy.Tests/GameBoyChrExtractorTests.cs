namespace Peony.Platform.GameBoy.Tests;

using Peony.Core;
using Xunit;

public class GameBoyChrExtractorTests {
	[Fact]
	public void Platform_ReturnsGameBoy() {
		var extractor = new GameBoyChrExtractor();
		Assert.Equal("GameBoy", extractor.Platform);
	}

	[Fact]
	public void Decode2bppGameBoy_SingleTile_DecodesCorrectly() {
		// 2bpp tile: low byte, high byte per row
		var data = new byte[16];

		// Row 0: low=0xFF, high=0x00 -> all color 1
		data[0] = 0xff;
		data[1] = 0x00;

		var pixels = GameBoyChrExtractor.Decode2bppGameBoy(data, 1);

		// First row should all be 1
		for (int i = 0; i < 8; i++) {
			Assert.Equal(1, pixels[i]);
		}
	}

	[Fact]
	public void Decode2bppGameBoy_Color3_DecodesCorrectly() {
		// Both planes set = color 3
		var data = new byte[16];
		data[0] = 0xff;
		data[1] = 0xff;

		var pixels = GameBoyChrExtractor.Decode2bppGameBoy(data, 1);

		// First row should all be 3
		for (int i = 0; i < 8; i++) {
			Assert.Equal(3, pixels[i]);
		}
	}

	[Fact]
	public void Decode2bppGameBoy_Checkerboard_DecodesCorrectly() {
		var data = new byte[16];
		data[0] = 0xaa; // 10101010
		data[1] = 0x00;

		var pixels = GameBoyChrExtractor.Decode2bppGameBoy(data, 1);

		// Pattern should be 1,0,1,0,1,0,1,0
		Assert.Equal(1, pixels[0]);
		Assert.Equal(0, pixels[1]);
		Assert.Equal(1, pixels[2]);
		Assert.Equal(0, pixels[3]);
	}

	[Fact]
	public void ExtractTiles2bpp_ReturnsCorrectTileCount() {
		var data = new byte[16 * 5]; // 5 tiles
		var result = GameBoyChrExtractor.ExtractTiles2bpp(data);

		Assert.Equal(5, result.TileCount);
		Assert.Equal(2, result.BitDepth);
	}

	[Fact]
	public void ExtractTiles_ThrowsForNon2bpp() {
		var extractor = new GameBoyChrExtractor();
		var data = new byte[32];

		Assert.Throws<ArgumentException>(() => extractor.ExtractTiles(data, 0, 32, 4));
	}

	[Fact]
	public void ExtractTiles_ClampsSizeToRomLength() {
		var extractor = new GameBoyChrExtractor();
		var data = new byte[32]; // 2 tiles

		var result = extractor.ExtractTiles(data, 0, 128, 2);

		Assert.Equal(2, result.TileCount);
	}

	[Fact]
	public void ExtractTilemap_ReturnsCorrectSize() {
		var data = new byte[2048];
		var tilemap = GameBoyChrExtractor.ExtractTilemap(data, 0, 32, 32);

		Assert.Equal(1024, tilemap.Length);
	}

	[Fact]
	public void ExtractTilemap_CopiesData() {
		var data = new byte[1024];
		for (int i = 0; i < 1024; i++) {
			data[i] = (byte)(i & 0xff);
		}

		var tilemap = GameBoyChrExtractor.ExtractTilemap(data, 0, 32, 32);

		Assert.Equal(data[0], tilemap[0]);
		Assert.Equal(data[255], tilemap[255]);
	}

	[Fact]
	public void ExtractOamSprites_ReturnsCorrectCount() {
		var data = new byte[160]; // 40 sprites * 4 bytes
		var sprites = GameBoyChrExtractor.ExtractOamSprites(data, 0, 40);

		Assert.Equal(40, sprites.Count);
	}

	[Fact]
	public void ExtractOamSprites_ParsesAttributes() {
		var data = new byte[] {
			100, 50, 0x42, 0x80, // Y=100, X=50, Tile=0x42, Behind BG
			0, 0, 0, 0x40        // Y=0, X=0, Tile=0, FlipY
		};

		var sprites = GameBoyChrExtractor.ExtractOamSprites(data, 0, 2);

		Assert.Equal(100, sprites[0].Y);
		Assert.Equal(50, sprites[0].X);
		Assert.Equal(0x42, sprites[0].TileIndex);
		Assert.True(sprites[0].BehindBackground);
		Assert.False(sprites[0].FlipY);

		Assert.True(sprites[1].FlipY);
		Assert.False(sprites[1].FlipX);
	}

	[Fact]
	public void SpriteAttribute_ScreenPosition_CalculatesCorrectly() {
		var sprite = new SpriteAttribute {
			X = 20,
			Y = 32
		};

		Assert.Equal(12, sprite.ScreenX);  // 20 - 8
		Assert.Equal(16, sprite.ScreenY);  // 32 - 16
	}

	[Fact]
	public void SpriteAttribute_GbcAttributes_ParseCorrectly() {
		var sprite = new SpriteAttribute {
			Attributes = 0x2f // FlipX, VramBank 1, Palette 7
		};

		Assert.True(sprite.FlipX);
		Assert.Equal(1, sprite.VramBank);
		Assert.Equal(7, sprite.GbcPalette);
	}

	[Fact]
	public void Bgr555ToArgb_Black() {
		ushort black = 0x0000;
		var argb = GameBoyChrExtractor.Bgr555ToArgb(black);

		Assert.Equal(0xff000000u, argb);
	}

	[Fact]
	public void Bgr555ToArgb_White() {
		ushort white = 0x7fff;
		var argb = GameBoyChrExtractor.Bgr555ToArgb(white);

		Assert.Equal(0xffffffffu, argb);
	}

	[Fact]
	public void Bgr555ToArgb_Red() {
		ushort red = 0x001f; // R=31, G=0, B=0
		var argb = GameBoyChrExtractor.Bgr555ToArgb(red);

		uint r = (argb >> 16) & 0xff;
		uint g = (argb >> 8) & 0xff;
		uint b = argb & 0xff;

		Assert.Equal(255u, r);
		Assert.Equal(0u, g);
		Assert.Equal(0u, b);
	}

	[Fact]
	public void ExtractGbcPalette_Returns4Colors() {
		var data = new byte[] {
			0x00, 0x00, // Black
			0x1f, 0x00, // Red
			0xe0, 0x03, // Green
			0x00, 0x7c  // Blue
		};

		var palette = GameBoyChrExtractor.ExtractGbcPalette(data, 0);

		Assert.Equal(4, palette.Length);
	}

	[Fact]
	public void DmgGreenPalette_Has4Colors() {
		Assert.Equal(4, GameBoyChrExtractor.DmgGreenPalette.Length);
	}

	[Fact]
	public void DmgGrayscalePalette_Has4Colors() {
		Assert.Equal(4, GameBoyChrExtractor.DmgGrayscalePalette.Length);
		Assert.Equal(0xff000000u, GameBoyChrExtractor.DmgGrayscalePalette[0]); // Black
		Assert.Equal(0xffffffffu, GameBoyChrExtractor.DmgGrayscalePalette[3]); // White
	}
}
