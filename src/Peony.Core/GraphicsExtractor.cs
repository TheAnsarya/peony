namespace Peony.Core;

/// <summary>
/// Interface for graphics extraction from ROMs
/// </summary>
public interface IGraphicsExtractor {
	/// <summary>Platform name</summary>
	string Platform { get; }

	/// <summary>Extract all graphics from ROM</summary>
	GraphicsExtractionResult ExtractAll(ReadOnlySpan<byte> rom, GraphicsExtractionOptions options);

	/// <summary>Extract graphics from specific offset and size</summary>
	TileData ExtractTiles(ReadOnlySpan<byte> rom, int offset, int size, int bitDepth);
}

/// <summary>
/// Options for graphics extraction
/// </summary>
public record GraphicsExtractionOptions {
	/// <summary>Output directory for extracted files</summary>
	public string OutputDirectory { get; init; } = "output/graphics";

	/// <summary>Output image format (png, bmp)</summary>
	public string ImageFormat { get; init; } = "png";

	/// <summary>Tiles per row in output image</summary>
	public int TilesPerRow { get; init; } = 16;

	/// <summary>Generate metadata JSON</summary>
	public bool GenerateMetadata { get; init; } = true;

	/// <summary>Custom palette (null = use grayscale)</summary>
	public uint[]? Palette { get; init; }
}

/// <summary>
/// Result of graphics extraction
/// </summary>
public record GraphicsExtractionResult {
	/// <summary>Extracted tile sets</summary>
	public required List<TileSetInfo> TileSets { get; init; }

	/// <summary>Extracted palettes</summary>
	public required List<PaletteInfo> Palettes { get; init; }

	/// <summary>Total tiles extracted</summary>
	public int TotalTiles => TileSets.Sum(t => t.TileCount);

	/// <summary>Output files generated</summary>
	public required List<string> OutputFiles { get; init; }
}

/// <summary>
/// Information about an extracted tileset
/// </summary>
public record TileSetInfo {
	/// <summary>Name/identifier for this tileset</summary>
	public required string Name { get; init; }

	/// <summary>ROM offset where tiles start</summary>
	public int RomOffset { get; init; }

	/// <summary>Size in bytes</summary>
	public int SizeBytes { get; init; }

	/// <summary>Number of tiles</summary>
	public int TileCount { get; init; }

	/// <summary>Bits per pixel</summary>
	public int BitDepth { get; init; }

	/// <summary>Tile width in pixels</summary>
	public int TileWidth { get; init; } = 8;

	/// <summary>Tile height in pixels</summary>
	public int TileHeight { get; init; } = 8;

	/// <summary>Output file path (if generated)</summary>
	public string? OutputPath { get; init; }
}

/// <summary>
/// Information about an extracted palette
/// </summary>
public record PaletteInfo {
	/// <summary>Name/identifier</summary>
	public required string Name { get; init; }

	/// <summary>ROM offset</summary>
	public int RomOffset { get; init; }

	/// <summary>Colors in ARGB format</summary>
	public required uint[] Colors { get; init; }
}

/// <summary>
/// Raw tile data
/// </summary>
public record TileData {
	/// <summary>Indexed pixel data (one byte per pixel)</summary>
	public required byte[] Pixels { get; init; }

	/// <summary>Width in pixels</summary>
	public int Width { get; init; }

	/// <summary>Height in pixels</summary>
	public int Height { get; init; }

	/// <summary>Number of tiles</summary>
	public int TileCount { get; init; }

	/// <summary>Bits per pixel</summary>
	public int BitDepth { get; init; }
}

/// <summary>
/// Utility class for tile graphics operations
/// </summary>
public static class TileGraphics {
	/// <summary>
	/// Decode 2bpp planar tiles (NES format)
	/// Each tile is 16 bytes: 8 bytes low plane, 8 bytes high plane
	/// </summary>
	public static byte[] Decode2bppPlanar(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 16;
		const int pixelsPerTile = 64; // 8x8

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int row = 0; row < 8; row++) {
				byte lowPlane = data[tileOffset + row];
				byte highPlane = data[tileOffset + row + 8];

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
	/// Decode 4bpp planar tiles (SNES format)
	/// Each tile is 32 bytes: interleaved bitplanes
	/// </summary>
	public static byte[] Decode4bppPlanar(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 32;
		const int pixelsPerTile = 64;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int row = 0; row < 8; row++) {
				// SNES 4bpp: bitplane 0,1 in first 16 bytes, bitplane 2,3 in next 16 bytes
				byte bp0 = data[tileOffset + row * 2];
				byte bp1 = data[tileOffset + row * 2 + 1];
				byte bp2 = data[tileOffset + 16 + row * 2];
				byte bp3 = data[tileOffset + 16 + row * 2 + 1];

				for (int col = 0; col < 8; col++) {
					int bit = 7 - col;
					int colorIndex =
						((bp0 >> bit) & 1) |
						(((bp1 >> bit) & 1) << 1) |
						(((bp2 >> bit) & 1) << 2) |
						(((bp3 >> bit) & 1) << 3);
					pixels[pixelOffset + row * 8 + col] = (byte)colorIndex;
				}
			}
		}

		return pixels;
	}

	/// <summary>
	/// Default NES grayscale palette
	/// </summary>
	public static readonly uint[] NesGrayscale = [
		0xff000000, // Black
		0xff555555, // Dark gray
		0xffaaaaaa, // Light gray
		0xffffffff  // White
	];

	/// <summary>
	/// Standard NES PPU palette (approximate RGB)
	/// </summary>
	public static readonly uint[] NesPalette = [
		0xff7c7c7c, 0xff0000fc, 0xff0000bc, 0xff4428bc,
		0xff940084, 0xffa80020, 0xffa81000, 0xff881400,
		0xff503000, 0xff007800, 0xff006800, 0xff005800,
		0xff004058, 0xff000000, 0xff000000, 0xff000000,
		0xffbcbcbc, 0xff0078f8, 0xff0058f8, 0xff6844fc,
		0xffd800cc, 0xffe40058, 0xfff83800, 0xffe45c10,
		0xffac7c00, 0xff00b800, 0xff00a800, 0xff00a844,
		0xff008888, 0xff000000, 0xff000000, 0xff000000,
		0xfff8f8f8, 0xff3cbcfc, 0xff6888fc, 0xff9878f8,
		0xfff878f8, 0xfff85898, 0xfff87858, 0xfffca044,
		0xfff8b800, 0xffb8f818, 0xff58d854, 0xff58f898,
		0xff00e8d8, 0xff787878, 0xff000000, 0xff000000,
		0xfffcfcfc, 0xffa4e4fc, 0xffb8b8f8, 0xffd8b8f8,
		0xfff8b8f8, 0xfff8a4c0, 0xfff0d0b0, 0xfffce0a8,
		0xfff8d878, 0xffd8f878, 0xffb8f8b8, 0xffb8f8d8,
		0xff00fcfc, 0xfff8d8f8, 0xff000000, 0xff000000
	];

	/// <summary>
	/// Convert indexed pixels to ARGB image data
	/// </summary>
	public static uint[] ApplyPalette(byte[] pixels, uint[] palette) {
		var result = new uint[pixels.Length];
		for (int i = 0; i < pixels.Length; i++) {
			int index = pixels[i] % palette.Length;
			result[i] = palette[index];
		}
		return result;
	}

	/// <summary>
	/// Arrange tiles into a 2D image
	/// </summary>
	public static uint[] ArrangeTiles(byte[] indexedPixels, int tileCount, int tilesPerRow, uint[] palette) {
		int tileRows = (tileCount + tilesPerRow - 1) / tilesPerRow;
		int imageWidth = tilesPerRow * 8;
		int imageHeight = tileRows * 8;

		var image = new uint[imageWidth * imageHeight];

		for (int tileIndex = 0; tileIndex < tileCount; tileIndex++) {
			int tileX = (tileIndex % tilesPerRow) * 8;
			int tileY = (tileIndex / tilesPerRow) * 8;
			int tilePixelOffset = tileIndex * 64;

			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					int srcIndex = tilePixelOffset + row * 8 + col;
					if (srcIndex >= indexedPixels.Length) continue;

					int dstIndex = (tileY + row) * imageWidth + (tileX + col);
					int colorIndex = indexedPixels[srcIndex] % palette.Length;
					image[dstIndex] = palette[colorIndex];
				}
			}
		}

		return image;
	}
}
