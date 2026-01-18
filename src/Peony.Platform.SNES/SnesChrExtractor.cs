namespace Peony.Platform.SNES;

using Peony.Core;

/// <summary>
/// SNES graphics extractor supporting 2bpp, 4bpp, and 8bpp tile formats
/// </summary>
public class SnesChrExtractor : IGraphicsExtractor {
	public string Platform => "SNES";

	/// <summary>
	/// Extract all graphics from a SNES ROM
	/// </summary>
	public GraphicsExtractionResult ExtractAll(ReadOnlySpan<byte> rom, GraphicsExtractionOptions options) {
		var tilesets = new List<TileSetInfo>();
		var palettes = new List<PaletteInfo>();
		var outputFiles = new List<string>();

		// SNES ROMs typically have graphics scattered throughout
		// We'll scan for common patterns and extract based on known offsets

		// For a generic extractor, we scan the entire ROM for tile data
		// Real usage would specify known offsets

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
		if (offset + size > rom.Length) {
			size = rom.Length - offset;
		}

		var data = rom.Slice(offset, size);

		return bitDepth switch {
			2 => ExtractTiles2bpp(data),
			4 => ExtractTiles4bpp(data),
			8 => ExtractTiles8bpp(data),
			_ => throw new ArgumentException($"Unsupported bit depth: {bitDepth}")
		};
	}

	/// <summary>
	/// Extract 2bpp tiles (16 bytes per 8x8 tile)
	/// Used for Mode 0 backgrounds, some sprites
	/// </summary>
	public static TileData ExtractTiles2bpp(ReadOnlySpan<byte> data) {
		const int bytesPerTile = 16;
		int tileCount = data.Length / bytesPerTile;
		var pixels = TileGraphics.Decode2bppPlanar(data, tileCount);

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
	/// Extract 4bpp tiles (32 bytes per 8x8 tile)
	/// Most common SNES format - Modes 1, 2, 3 backgrounds and sprites
	/// Format: 2 planes interleaved (bytes 0-15: planes 0,1; bytes 16-31: planes 2,3)
	/// </summary>
	public static TileData ExtractTiles4bpp(ReadOnlySpan<byte> data) {
		const int bytesPerTile = 32;
		int tileCount = data.Length / bytesPerTile;
		var pixels = Decode4bppSnesPlanar(data, tileCount);

		int tilesPerRow = 16;
		int tilesPerCol = (tileCount + tilesPerRow - 1) / tilesPerRow;

		return new TileData {
			Pixels = pixels,
			Width = tilesPerRow * 8,
			Height = tilesPerCol * 8,
			TileCount = tileCount,
			BitDepth = 4
		};
	}

	/// <summary>
	/// Extract 8bpp tiles (64 bytes per 8x8 tile)
	/// Used for Mode 3/4 backgrounds, direct color mode
	/// </summary>
	public static TileData ExtractTiles8bpp(ReadOnlySpan<byte> data) {
		const int bytesPerTile = 64;
		int tileCount = data.Length / bytesPerTile;
		var pixels = Decode8bppSnesPlanar(data, tileCount);

		int tilesPerRow = 16;
		int tilesPerCol = (tileCount + tilesPerRow - 1) / tilesPerRow;

		return new TileData {
			Pixels = pixels,
			Width = tilesPerRow * 8,
			Height = tilesPerCol * 8,
			TileCount = tileCount,
			BitDepth = 8
		};
	}

	/// <summary>
	/// Decode 4bpp SNES planar tiles
	/// Format: rows 0-7 contain planes 0,1 interleaved; rows 8-15 contain planes 2,3 interleaved
	/// </summary>
	public static byte[] Decode4bppSnesPlanar(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 32;
		const int pixelsPerTile = 64;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int row = 0; row < 8; row++) {
				// Planes 0 and 1 are in first 16 bytes (interleaved by row)
				byte plane0 = data[tileOffset + row * 2];
				byte plane1 = data[tileOffset + row * 2 + 1];
				// Planes 2 and 3 are in second 16 bytes (interleaved by row)
				byte plane2 = data[tileOffset + 16 + row * 2];
				byte plane3 = data[tileOffset + 16 + row * 2 + 1];

				for (int col = 0; col < 8; col++) {
					int bit = 7 - col;
					int colorIndex =
						((plane0 >> bit) & 1) |
						(((plane1 >> bit) & 1) << 1) |
						(((plane2 >> bit) & 1) << 2) |
						(((plane3 >> bit) & 1) << 3);
					pixels[pixelOffset + row * 8 + col] = (byte)colorIndex;
				}
			}
		}

		return pixels;
	}

	/// <summary>
	/// Decode 8bpp SNES planar tiles
	/// Format: Four pairs of 2bpp planes interleaved
	/// </summary>
	public static byte[] Decode8bppSnesPlanar(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 64;
		const int pixelsPerTile = 64;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int row = 0; row < 8; row++) {
				// Planes 0-1 at offset 0-15
				byte plane0 = data[tileOffset + row * 2];
				byte plane1 = data[tileOffset + row * 2 + 1];
				// Planes 2-3 at offset 16-31
				byte plane2 = data[tileOffset + 16 + row * 2];
				byte plane3 = data[tileOffset + 16 + row * 2 + 1];
				// Planes 4-5 at offset 32-47
				byte plane4 = data[tileOffset + 32 + row * 2];
				byte plane5 = data[tileOffset + 32 + row * 2 + 1];
				// Planes 6-7 at offset 48-63
				byte plane6 = data[tileOffset + 48 + row * 2];
				byte plane7 = data[tileOffset + 48 + row * 2 + 1];

				for (int col = 0; col < 8; col++) {
					int bit = 7 - col;
					int colorIndex =
						((plane0 >> bit) & 1) |
						(((plane1 >> bit) & 1) << 1) |
						(((plane2 >> bit) & 1) << 2) |
						(((plane3 >> bit) & 1) << 3) |
						(((plane4 >> bit) & 1) << 4) |
						(((plane5 >> bit) & 1) << 5) |
						(((plane6 >> bit) & 1) << 6) |
						(((plane7 >> bit) & 1) << 7);
					pixels[pixelOffset + row * 8 + col] = (byte)colorIndex;
				}
			}
		}

		return pixels;
	}

	/// <summary>
	/// Extract and convert SNES BGR555 palette to ARGB
	/// </summary>
	public static uint[] ExtractPalette(ReadOnlySpan<byte> data, int offset, int colorCount) {
		var colors = new uint[colorCount];

		for (int i = 0; i < colorCount && offset + i * 2 + 1 < data.Length; i++) {
			ushort bgr555 = (ushort)(data[offset + i * 2] | (data[offset + i * 2 + 1] << 8));
			colors[i] = Bgr555ToArgb(bgr555);
		}

		return colors;
	}

	/// <summary>
	/// Convert SNES BGR555 color to ARGB32
	/// </summary>
	public static uint Bgr555ToArgb(ushort bgr555) {
		int r = (bgr555 & 0x1f) << 3;
		int g = ((bgr555 >> 5) & 0x1f) << 3;
		int b = ((bgr555 >> 10) & 0x1f) << 3;

		// Expand 5-bit to 8-bit with proper rounding
		r |= r >> 5;
		g |= g >> 5;
		b |= b >> 5;

		return 0xff000000 | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
	}

	/// <summary>
	/// Save tiles as BMP image
	/// </summary>
	public static void SaveAsBmp(TileData tiles, string path, uint[]? palette = null) {
		palette ??= GetGrayscalePalette(1 << tiles.BitDepth);

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

		// BMP Header
		int rowSize = ((width * 3 + 3) / 4) * 4; // Rows are 4-byte aligned
		int imageSize = rowSize * height;
		int fileSize = 54 + imageSize;

		// File header (14 bytes)
		writer.Write((byte)'B');
		writer.Write((byte)'M');
		writer.Write(fileSize);
		writer.Write((short)0);
		writer.Write((short)0);
		writer.Write(54);

		// DIB header (40 bytes)
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
	/// Save tile data with metadata
	/// </summary>
	public static void SaveWithMetadata(TileData tiles, string basePath, string name, int romOffset, uint[]? palette = null) {
		var dir = Path.GetDirectoryName(basePath);
		if (!string.IsNullOrEmpty(dir)) {
			Directory.CreateDirectory(dir);
		}

		// Save image
		SaveAsBmp(tiles, basePath + ".bmp", palette);

		// Save metadata
		var metadata = new {
			name,
			romOffset = $"0x{romOffset:x6}",
			tileCount = tiles.TileCount,
			bitDepth = tiles.BitDepth,
			width = tiles.Width,
			height = tiles.Height,
			bytesPerTile = tiles.BitDepth switch { 2 => 16, 4 => 32, 8 => 64, _ => 0 },
			format = tiles.BitDepth switch { 2 => "SNES 2bpp planar", 4 => "SNES 4bpp planar", 8 => "SNES 8bpp planar", _ => "unknown" }
		};

		var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions {
			WriteIndented = true
		});
		File.WriteAllText(basePath + ".json", json);
	}

	/// <summary>
	/// Get a grayscale palette for the given color count
	/// </summary>
	public static uint[] GetGrayscalePalette(int colors) {
		var palette = new uint[colors];
		for (int i = 0; i < colors; i++) {
			int gray = (255 * i) / (colors - 1);
			palette[i] = 0xff000000 | ((uint)gray << 16) | ((uint)gray << 8) | (uint)gray;
		}
		return palette;
	}

	/// <summary>
	/// Standard SNES 4-color grayscale palette
	/// </summary>
	public static readonly uint[] Grayscale4 = [0xff000000, 0xff555555, 0xffaaaaaa, 0xffffffff];

	/// <summary>
	/// Standard SNES 16-color grayscale palette
	/// </summary>
	public static readonly uint[] Grayscale16 = GetGrayscalePalette(16);

	/// <summary>
	/// Standard SNES 256-color grayscale palette
	/// </summary>
	public static readonly uint[] Grayscale256 = GetGrayscalePalette(256);
}

/// <summary>
/// SNES Mode 7 graphics extractor
/// </summary>
public static class SnesMode7Extractor {
	/// <summary>
	/// Extract Mode 7 tileset (8x8 tiles, 8bpp, 128 tiles max)
	/// Mode 7 tiles are 8bpp linear (one byte per pixel, no bitplanes)
	/// </summary>
	public static TileData ExtractMode7Tiles(ReadOnlySpan<byte> data, int offset, int tileCount = 256) {
		const int bytesPerTile = 64; // 8x8 * 1 byte
		const int pixelsPerTile = 64;

		if (tileCount > 256) tileCount = 256;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = offset + tile * bytesPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			int pixelOffset = tile * pixelsPerTile;
			data.Slice(tileOffset, bytesPerTile).CopyTo(pixels.AsSpan(pixelOffset));
		}

		int tilesPerRow = 16;
		int tilesPerCol = (tileCount + tilesPerRow - 1) / tilesPerRow;

		return new TileData {
			Pixels = pixels,
			Width = tilesPerRow * 8,
			Height = tilesPerCol * 8,
			TileCount = tileCount,
			BitDepth = 8
		};
	}

	/// <summary>
	/// Extract Mode 7 tilemap (128x128 grid of tile indices)
	/// </summary>
	public static byte[] ExtractMode7Tilemap(ReadOnlySpan<byte> data, int offset) {
		const int mapSize = 128 * 128;
		var tilemap = new byte[mapSize];

		if (offset + mapSize <= data.Length) {
			data.Slice(offset, mapSize).CopyTo(tilemap);
		}

		return tilemap;
	}
}

/// <summary>
/// SNES sprite/OAM extraction utilities
/// </summary>
public static class SnesSpriteExtractor {
	/// <summary>
	/// Extract sprite tiles (always 4bpp on SNES)
	/// </summary>
	public static TileData ExtractSpriteTiles(ReadOnlySpan<byte> data, int offset, int size) {
		var spriteData = data.Slice(offset, Math.Min(size, data.Length - offset));
		return SnesChrExtractor.ExtractTiles4bpp(spriteData);
	}

	/// <summary>
	/// Extract 16x16 sprite as 4 tiles arranged properly
	/// </summary>
	public static byte[] Extract16x16Sprite(ReadOnlySpan<byte> data, int offset) {
		const int bytesPerTile = 32;
		var pixels = new byte[256]; // 16x16

		// SNES 16x16 sprites are stored as 4 8x8 tiles
		// Layout: [0,1] (top), [2,3] (bottom)
		int[] tileOrder = [0, 1, 2, 3];

		for (int t = 0; t < 4; t++) {
			int tileOffset = offset + tileOrder[t] * bytesPerTile;
			if (tileOffset + bytesPerTile > data.Length) break;

			var tilePixels = SnesChrExtractor.Decode4bppSnesPlanar(data.Slice(tileOffset, bytesPerTile), 1);

			int baseX = (t % 2) * 8;
			int baseY = (t / 2) * 8;

			for (int y = 0; y < 8; y++) {
				for (int x = 0; x < 8; x++) {
					pixels[(baseY + y) * 16 + baseX + x] = tilePixels[y * 8 + x];
				}
			}
		}

		return pixels;
	}
}
