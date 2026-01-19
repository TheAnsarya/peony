namespace Peony.Platform.GBA.Tests;

using Xunit;

public class GbaChrExtractorTests {
	#region 4bpp Linear Tile Tests

	[Fact]
	public void Decode4bppLinear_SingleTile_DecodesCorrectly() {
		// 4bpp linear: each byte = 2 pixels (low nibble, high nibble)
		// First 4 bytes = row 0: pixels 0-7
		byte[] data = new byte[32]; // One 8x8 tile
		data[0] = 0x10; // pixels 0=0, 1=1
		data[1] = 0x32; // pixels 2=2, 3=3
		data[2] = 0x54; // pixels 4=4, 5=5
		data[3] = 0x76; // pixels 6=6, 7=7

		var pixels = GbaChrExtractor.Decode4bppLinear(data, 1);

		// Check first row
		Assert.Equal((byte)0, pixels[0]);
		Assert.Equal((byte)1, pixels[1]);
		Assert.Equal((byte)2, pixels[2]);
		Assert.Equal((byte)3, pixels[3]);
		Assert.Equal((byte)4, pixels[4]);
		Assert.Equal((byte)5, pixels[5]);
		Assert.Equal((byte)6, pixels[6]);
		Assert.Equal((byte)7, pixels[7]);
	}

	[Fact]
	public void Decode4bppLinear_AllZeros_ReturnsZeroPixels() {
		byte[] data = new byte[32];

		var pixels = GbaChrExtractor.Decode4bppLinear(data, 1);

		Assert.All(pixels, p => Assert.Equal((byte)0, p));
	}

	[Fact]
	public void Decode4bppLinear_AllOnes_Returns15ForAllPixels() {
		byte[] data = new byte[32];
		Array.Fill(data, (byte)0xff);

		var pixels = GbaChrExtractor.Decode4bppLinear(data, 1);

		Assert.All(pixels, p => Assert.Equal((byte)0xf, p));
	}

	[Fact]
	public void Decode4bppLinear_MultipleTiles_DecodesAll() {
		byte[] data = new byte[64]; // 2 tiles
		data[0] = 0x21; // Tile 0: pixel 0=1, pixel 1=2
		data[32] = 0x43; // Tile 1: pixel 0=3, pixel 1=4

		var pixels = GbaChrExtractor.Decode4bppLinear(data, 2);

		Assert.Equal(128, pixels.Length); // 2 tiles * 64 pixels
		Assert.Equal((byte)1, pixels[0]);
		Assert.Equal((byte)2, pixels[1]);
		Assert.Equal((byte)3, pixels[64]); // Start of tile 1
		Assert.Equal((byte)4, pixels[65]);
	}

	[Fact]
	public void ExtractTiles4bpp_ReturnsTileData() {
		byte[] data = new byte[32];

		var tileData = GbaChrExtractor.ExtractTiles4bpp(data);

		Assert.Equal(1, tileData.TileCount);
		Assert.Equal(4, tileData.BitDepth);
		Assert.Equal(64, tileData.Pixels.Length);
	}

	#endregion

	#region 8bpp Linear Tile Tests

	[Fact]
	public void Decode8bppLinear_SingleTile_DecodesCorrectly() {
		// 8bpp linear: each byte = 1 pixel
		byte[] data = new byte[64]; // One 8x8 tile
		for (int i = 0; i < 64; i++) {
			data[i] = (byte)i;
		}

		var pixels = GbaChrExtractor.Decode8bppLinear(data, 1);

		for (int i = 0; i < 64; i++) {
			Assert.Equal((byte)i, pixels[i]);
		}
	}

	[Fact]
	public void Decode8bppLinear_AllZeros_ReturnsZeroPixels() {
		byte[] data = new byte[64];

		var pixels = GbaChrExtractor.Decode8bppLinear(data, 1);

		Assert.All(pixels, p => Assert.Equal((byte)0, p));
	}

	[Fact]
	public void ExtractTiles8bpp_ReturnsTileData() {
		byte[] data = new byte[64];

		var tileData = GbaChrExtractor.ExtractTiles8bpp(data);

		Assert.Equal(1, tileData.TileCount);
		Assert.Equal(8, tileData.BitDepth);
		Assert.Equal(64, tileData.Pixels.Length);
	}

	#endregion

	#region BGR555 Color Conversion Tests

	[Fact]
	public void Bgr555ToArgb_Black_ReturnsBlack() {
		ushort black = 0x0000;

		uint argb = GbaChrExtractor.Bgr555ToArgb(black);

		Assert.Equal(0xff000000u, argb);
	}

	[Fact]
	public void Bgr555ToArgb_White_ReturnsWhite() {
		ushort white = 0x7fff; // All bits set in BGR555

		uint argb = GbaChrExtractor.Bgr555ToArgb(white);

		Assert.Equal(0xffffffffu, argb);
	}

	[Fact]
	public void Bgr555ToArgb_Red_ReturnsRed() {
		ushort red = 0x001f; // R=31, G=0, B=0

		uint argb = GbaChrExtractor.Bgr555ToArgb(red);

		Assert.Equal(0xffu, (argb >> 16) & 0xff); // R = 255
		Assert.Equal(0x00u, (argb >> 8) & 0xff);  // G = 0
		Assert.Equal(0x00u, argb & 0xff);          // B = 0
	}

	[Fact]
	public void Bgr555ToArgb_Green_ReturnsGreen() {
		ushort green = 0x03e0; // R=0, G=31, B=0

		uint argb = GbaChrExtractor.Bgr555ToArgb(green);

		Assert.Equal(0x00u, (argb >> 16) & 0xff); // R = 0
		Assert.Equal(0xffu, (argb >> 8) & 0xff);  // G = 255
		Assert.Equal(0x00u, argb & 0xff);          // B = 0
	}

	[Fact]
	public void Bgr555ToArgb_Blue_ReturnsBlue() {
		ushort blue = 0x7c00; // R=0, G=0, B=31

		uint argb = GbaChrExtractor.Bgr555ToArgb(blue);

		Assert.Equal(0x00u, (argb >> 16) & 0xff); // R = 0
		Assert.Equal(0x00u, (argb >> 8) & 0xff);  // G = 0
		Assert.Equal(0xffu, argb & 0xff);          // B = 255
	}

	#endregion

	#region Palette Extraction Tests

	[Fact]
	public void ExtractPalette_ExtractsColors() {
		byte[] data = [
			0x00, 0x00, // Black
			0x1f, 0x00, // Red
			0xe0, 0x03, // Green
			0x00, 0x7c  // Blue
		];

		var palette = GbaChrExtractor.ExtractPalette(data, 0, 4);

		Assert.Equal(4, palette.Length);
		Assert.Equal(0xff000000u, palette[0]); // Black
		Assert.Equal(0xffff0000u, palette[1]); // Red (approximately)
		Assert.Equal(0xff00ff00u, palette[2]); // Green (approximately)
		Assert.Equal(0xff0000ffu, palette[3]); // Blue (approximately)
	}

	[Fact]
	public void ExtractPalette_WithOffset_StartsAtOffset() {
		byte[] data = [
			0xff, 0xff, // Padding
			0x00, 0x00, // Black
			0xff, 0x7f  // White
		];

		var palette = GbaChrExtractor.ExtractPalette(data, 2, 2);

		Assert.Equal(2, palette.Length);
		Assert.Equal(0xff000000u, palette[0]);
		Assert.Equal(0xffffffffu, palette[1]);
	}

	#endregion

	#region Grayscale Palette Tests

	[Fact]
	public void GetGrayscalePalette_4Colors_ReturnsCorrectGradient() {
		var palette = GbaChrExtractor.GetGrayscalePalette(4);

		Assert.Equal(4, palette.Length);
		Assert.Equal(0xff000000u, palette[0]); // Black
		Assert.Equal(0xff555555u, palette[1]); // Dark gray
		Assert.Equal(0xffaaaaaau, palette[2]); // Light gray
		Assert.Equal(0xffffffffu, palette[3]); // White
	}

	[Fact]
	public void GetGrayscalePalette_16Colors_Returns16Colors() {
		var palette = GbaChrExtractor.GetGrayscalePalette(16);

		Assert.Equal(16, palette.Length);
		Assert.Equal(0xff000000u, palette[0]);  // Black
		Assert.Equal(0xffffffffu, palette[15]); // White
	}

	#endregion

	#region OAM Parsing Tests

	[Fact]
	public void ParseOam_SingleEntry_ParsesCorrectly() {
		// Create OAM entry: Y=10, X=20, Tile=5, 8x8 square
		byte[] oamData = new byte[8];
		oamData[0] = 10; // Y
		oamData[1] = 0x00; // Y high + flags (no affine, no disable)
		oamData[2] = 20; // X low
		oamData[3] = 0x00; // X high + flags (shape=0, size=0)
		oamData[4] = 5; // Tile index low
		oamData[5] = 0x00; // Tile index high + priority + palette

		var entries = GbaOamParser.ParseOam(oamData, 1);

		Assert.Single(entries);
		Assert.Equal(10, entries[0].Y);
		Assert.Equal(20, entries[0].X);
		Assert.Equal(5, entries[0].TileIndex);
	}

	[Fact]
	public void ParseOam_GetDimensions_8x8Square_ReturnsCorrect() {
		byte[] oamData = new byte[8];
		// Shape=0, Size=0 = 8x8
		oamData[1] = 0x00; // shape in bits 14-15 (0)
		oamData[3] = 0x00; // size in bits 14-15 (0)

		var entries = GbaOamParser.ParseOam(oamData, 1);

		Assert.Equal((8, 8), entries[0].GetDimensions());
	}

	[Fact]
	public void ParseOam_GetDimensions_16x16Square_ReturnsCorrect() {
		byte[] oamData = new byte[8];
		// Shape=0, Size=1 = 16x16
		oamData[1] = 0x00;
		oamData[3] = 0x40; // size=1 in bits 14-15

		var entries = GbaOamParser.ParseOam(oamData, 1);

		Assert.Equal((16, 16), entries[0].GetDimensions());
	}

	[Fact]
	public void ParseOam_GetDimensions_32x8Wide_ReturnsCorrect() {
		byte[] oamData = new byte[8];
		// Shape=1 (wide), Size=1 = 32x8
		oamData[1] = 0x40; // shape=1 in bits 14-15
		oamData[3] = 0x40; // size=1 in bits 14-15

		var entries = GbaOamParser.ParseOam(oamData, 1);

		Assert.Equal((32, 8), entries[0].GetDimensions());
	}

	[Fact]
	public void ParseOam_GetTileCount_16x16_Returns4() {
		byte[] oamData = new byte[8];
		oamData[1] = 0x00;
		oamData[3] = 0x40; // 16x16

		var entries = GbaOamParser.ParseOam(oamData, 1);

		Assert.Equal(4, entries[0].GetTileCount()); // 16/8 * 16/8 = 2*2 = 4
	}

	#endregion

	#region LZ77 Tests

	[Fact]
	public void IsLz77Compressed_ValidHeader_ReturnsTrue() {
		byte[] data = [0x10, 0x20, 0x00, 0x00]; // LZ77 header

		Assert.True(GbaLz77.IsLz77Compressed(data));
	}

	[Fact]
	public void IsLz77Compressed_InvalidHeader_ReturnsFalse() {
		byte[] data = [0x00, 0x20, 0x00, 0x00];

		Assert.False(GbaLz77.IsLz77Compressed(data));
	}

	[Fact]
	public void IsLz77Compressed_TooShort_ReturnsFalse() {
		byte[] data = [0x10, 0x20];

		Assert.False(GbaLz77.IsLz77Compressed(data));
	}

	[Fact]
	public void Decompress_UncompressedData_DecompressesCorrectly() {
		// LZ77 format: 0x10, size (24-bit LE), flags, data
		// flags=0x00 means all 8 following bytes are uncompressed
		byte[] compressed = [
			0x10,       // LZ77 marker
			0x04, 0x00, 0x00, // Decompressed size = 4
			0x00,       // Flags (all literal)
			0x41, 0x42, 0x43, 0x44 // "ABCD"
		];

		var result = GbaLz77.Decompress(compressed);

		Assert.Equal(4, result.Length);
		Assert.Equal((byte)'A', result[0]);
		Assert.Equal((byte)'B', result[1]);
		Assert.Equal((byte)'C', result[2]);
		Assert.Equal((byte)'D', result[3]);
	}

	[Fact]
	public void TryDecompress_InvalidData_ReturnsNull() {
		byte[] data = [0x00, 0x20, 0x00, 0x00];

		var result = GbaLz77.TryDecompress(data);

		Assert.Null(result);
	}

	#endregion

	#region Integration Tests

	[Fact]
	public void ExtractTiles_4bpp_ThroughExtractor() {
		var extractor = new GbaChrExtractor();
		byte[] rom = new byte[64];
		rom[0] = 0x21; // Some pixel data

		var tileData = extractor.ExtractTiles(rom, 0, 32, 4);

		Assert.Equal(1, tileData.TileCount);
		Assert.Equal(4, tileData.BitDepth);
	}

	[Fact]
	public void ExtractTiles_8bpp_ThroughExtractor() {
		var extractor = new GbaChrExtractor();
		byte[] rom = new byte[128];
		for (int i = 0; i < 64; i++) rom[i] = (byte)i;

		var tileData = extractor.ExtractTiles(rom, 0, 64, 8);

		Assert.Equal(1, tileData.TileCount);
		Assert.Equal(8, tileData.BitDepth);
		Assert.Equal((byte)0, tileData.Pixels[0]);
		Assert.Equal((byte)63, tileData.Pixels[63]);
	}

	[Fact]
	public void ExtractTiles_InvalidBitDepth_Throws() {
		var extractor = new GbaChrExtractor();
		byte[] rom = new byte[32];

		Assert.Throws<ArgumentException>(() =>
			extractor.ExtractTiles(rom, 0, 16, 2) // 2bpp not supported on GBA
		);
	}

	#endregion
}
