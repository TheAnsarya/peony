namespace Peony.Platform.GameBoy;

using Peony.Core;

/// <summary>
/// Game Boy graphics extractor supporting 2bpp tile format
/// </summary>
public class GameBoyChrExtractor : IGraphicsExtractor {
	public string Platform => "GameBoy";

	private const int HeaderOffset = 0x100;
	private const int TitleOffset = 0x134;
	private const int BytesPerTile = 16; // 2bpp, 8x8 = 16 bytes

	/// <summary>
	/// Extract all graphics from a Game Boy ROM
	/// </summary>
	public GraphicsExtractionResult ExtractAll(ReadOnlySpan<byte> rom, GraphicsExtractionOptions options) {
		var tilesets = new List<TileSetInfo>();
		var palettes = new List<PaletteInfo>();
		var outputFiles = new List<string>();

		// Game Boy graphics are typically scattered throughout the ROM
		// A comprehensive extraction would scan for tile patterns

		return new GraphicsExtractionResult {
			TileSets = tilesets,
			Palettes = palettes,
			OutputFiles = outputFiles
		};
	}

	/// <summary>
	/// Extract tiles from specific offset
	/// </summary>
	public TileData ExtractTiles(ReadOnlySpan<byte> rom, int offset, int size, int bitDepth) {
		if (bitDepth != 2) {
			throw new ArgumentException("Game Boy only supports 2bpp tiles", nameof(bitDepth));
		}

		if (offset + size > rom.Length) {
			size = rom.Length - offset;
		}

		var data = rom.Slice(offset, size);
		return ExtractTiles2bpp(data);
	}

	/// <summary>
	/// Extract 2bpp tiles (Game Boy format - same as NES)
	/// Each tile is 16 bytes: 2 bytes per row (interleaved bitplanes)
	/// </summary>
	public static TileData ExtractTiles2bpp(ReadOnlySpan<byte> data) {
		int tileCount = data.Length / BytesPerTile;
		var pixels = Decode2bppGameBoy(data, tileCount);

		int tilesPerRow = 16;
		int tilesPerCol = (tileCount + tilesPerRow - 1) / tilesPerRow;

		return new TileData {
			Pixels = pixels,
			Width = tilesPerRow * 8,
			Height = tilesPerCol * 8,
			TileCount = tileCount,
			BitDepth = 2
		};
	}

	/// <summary>
	/// Decode 2bpp Game Boy tiles
	/// Format: 2 bytes per row, bitplanes interleaved (low byte, high byte)
	/// This is the same format as NES 2bpp
	/// </summary>
	public static byte[] Decode2bppGameBoy(ReadOnlySpan<byte> data, int tileCount) {
		const int pixelsPerTile = 64;
		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * BytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + BytesPerTile > data.Length)
				break;

			for (int row = 0; row < 8; row++) {
				// Game Boy: low byte first, then high byte (same row)
				byte lowPlane = data[tileOffset + row * 2];
				byte highPlane = data[tileOffset + row * 2 + 1];

				for (int col = 0; col < 8; col++) {
					int bit = 7 - col;
					int colorIndex = ((lowPlane >> bit) & 1) | (((highPlane >> bit) & 1) << 1);
					pixels[pixelOffset + row * 8 + col] = (byte)colorIndex;
				}
			}
		}

		return pixels;
	}

	/// <summary>
	/// Extract tile data from VRAM-style arrangement (background map)
	/// </summary>
	public static byte[] ExtractTilemap(ReadOnlySpan<byte> data, int offset, int width = 32, int height = 32) {
		int size = width * height;
		var tilemap = new byte[size];

		if (offset + size <= data.Length) {
			data.Slice(offset, size).CopyTo(tilemap);
		}

		return tilemap;
	}

	/// <summary>
	/// Extract OAM sprite attributes
	/// Each sprite entry is 4 bytes: Y, X, Tile, Attributes
	/// </summary>
	public static List<SpriteAttribute> ExtractOamSprites(ReadOnlySpan<byte> data, int offset, int count = 40) {
		var sprites = new List<SpriteAttribute>();

		for (int i = 0; i < count; i++) {
			int entryOffset = offset + i * 4;
			if (entryOffset + 4 > data.Length) break;

			var sprite = new SpriteAttribute {
				Y = data[entryOffset],
				X = data[entryOffset + 1],
				TileIndex = data[entryOffset + 2],
				Attributes = data[entryOffset + 3]
			};

			sprites.Add(sprite);
		}

		return sprites;
	}

	/// <summary>
	/// Convert Game Boy Color 15-bit BGR to ARGB
	/// Format: 0bbbbbgggggrrrrr (little-endian)
	/// </summary>
	public static uint Bgr555ToArgb(ushort bgr555) {
		int r = (bgr555 & 0x1f) << 3;
		int g = ((bgr555 >> 5) & 0x1f) << 3;
		int b = ((bgr555 >> 10) & 0x1f) << 3;

		// Expand 5-bit to 8-bit
		r |= r >> 5;
		g |= g >> 5;
		b |= b >> 5;

		return 0xff000000 | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
	}

	/// <summary>
	/// Extract GBC palette (4 colors, 8 bytes)
	/// </summary>
	public static uint[] ExtractGbcPalette(ReadOnlySpan<byte> data, int offset) {
		var colors = new uint[4];

		for (int i = 0; i < 4 && offset + i * 2 + 1 < data.Length; i++) {
			ushort bgr555 = (ushort)(data[offset + i * 2] | (data[offset + i * 2 + 1] << 8));
			colors[i] = Bgr555ToArgb(bgr555);
		}

		return colors;
	}

	/// <summary>
	/// Save tiles as BMP image
	/// </summary>
	public static void SaveAsBmp(TileData tiles, string path, uint[]? palette = null) {
		palette ??= DmgGreenPalette;

		// Arrange tiles into image
		var imageData = TileGraphics.ArrangeTiles(tiles.Pixels, tiles.TileCount, 16, palette);

		// Save as BMP
		SaveImage(imageData, tiles.Width, tiles.Height, path);
	}

	/// <summary>
	/// Save ARGB image data to BMP file
	/// </summary>
	private static void SaveImage(uint[] pixels, int width, int height, string path) {
		using var fs = File.Create(path);
		using var writer = new BinaryWriter(fs);

		int rowSize = ((width * 3 + 3) / 4) * 4;
		int imageSize = rowSize * height;
		int fileSize = 54 + imageSize;

		// File header
		writer.Write((byte)'B');
		writer.Write((byte)'M');
		writer.Write(fileSize);
		writer.Write((short)0);
		writer.Write((short)0);
		writer.Write(54);

		// DIB header
		writer.Write(40);
		writer.Write(width);
		writer.Write(height);
		writer.Write((short)1);
		writer.Write((short)24);
		writer.Write(0);
		writer.Write(imageSize);
		writer.Write(2835);
		writer.Write(2835);
		writer.Write(0);
		writer.Write(0);

		// Pixel data (bottom-up, BGR)
		var rowBuffer = new byte[rowSize];
		for (int y = height - 1; y >= 0; y--) {
			for (int x = 0; x < width; x++) {
				uint argb = pixels[y * width + x];
				rowBuffer[x * 3 + 0] = (byte)(argb & 0xff);
				rowBuffer[x * 3 + 1] = (byte)((argb >> 8) & 0xff);
				rowBuffer[x * 3 + 2] = (byte)((argb >> 16) & 0xff);
			}
			writer.Write(rowBuffer);
		}
	}

	/// <summary>
	/// Classic DMG green palette
	/// </summary>
	public static readonly uint[] DmgGreenPalette = [
		0xff0f380f, // Darkest green
		0xff306230, // Dark green
		0xff8bac0f, // Light green
		0xff9bbc0f  // Lightest green
	];

	/// <summary>
	/// Classic DMG grayscale palette
	/// </summary>
	public static readonly uint[] DmgGrayscalePalette = [
		0xff000000, // Black
		0xff555555, // Dark gray
		0xffaaaaaa, // Light gray
		0xffffffff  // White
	];

	/// <summary>
	/// Game Boy Pocket palette
	/// </summary>
	public static readonly uint[] GbPocketPalette = [
		0xff000000,
		0xff646464,
		0xffa5a5a5,
		0xffffffff
	];
}

/// <summary>
/// Game Boy sprite attribute record
/// </summary>
public record SpriteAttribute {
	/// <summary>Y position (minus 16 for actual screen position)</summary>
	public byte Y { get; init; }

	/// <summary>X position (minus 8 for actual screen position)</summary>
	public byte X { get; init; }

	/// <summary>Tile number from pattern table</summary>
	public byte TileIndex { get; init; }

	/// <summary>Attribute flags</summary>
	public byte Attributes { get; init; }

	/// <summary>Priority: true = behind background</summary>
	public bool BehindBackground => (Attributes & 0x80) != 0;

	/// <summary>Y flip</summary>
	public bool FlipY => (Attributes & 0x40) != 0;

	/// <summary>X flip</summary>
	public bool FlipX => (Attributes & 0x20) != 0;

	/// <summary>DMG palette (0 or 1)</summary>
	public int DmgPalette => (Attributes >> 4) & 1;

	/// <summary>GBC VRAM bank</summary>
	public int VramBank => (Attributes >> 3) & 1;

	/// <summary>GBC palette number (0-7)</summary>
	public int GbcPalette => Attributes & 0x07;

	/// <summary>Actual screen X position</summary>
	public int ScreenX => X - 8;

	/// <summary>Actual screen Y position</summary>
	public int ScreenY => Y - 16;
}
