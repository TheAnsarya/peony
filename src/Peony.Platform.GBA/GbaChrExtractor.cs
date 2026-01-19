namespace Peony.Platform.GBA;

using Peony.Core;

/// <summary>
/// GBA graphics extractor supporting 4bpp and 8bpp tile formats,
/// plus Mode 3/5 direct color bitmaps
/// </summary>
public class GbaChrExtractor : IGraphicsExtractor {
	public string Platform => "GBA";

	/// <summary>
	/// Extract all graphics from a GBA ROM
	/// </summary>
	public GraphicsExtractionResult ExtractAll(ReadOnlySpan<byte> rom, GraphicsExtractionOptions options) {
		var tilesets = new List<TileSetInfo>();
		var palettes = new List<PaletteInfo>();
		var outputFiles = new List<string>();

		// GBA ROMs have graphics scattered throughout
		// We scan for common patterns and extract based on known offsets
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
			4 => ExtractTiles4bpp(data),
			8 => ExtractTiles8bpp(data),
			_ => throw new ArgumentException($"Unsupported bit depth: {bitDepth}. GBA supports 4bpp and 8bpp.")
		};
	}

	/// <summary>
	/// Extract 4bpp linear tiles (32 bytes per 8x8 tile)
	/// Most common GBA format - used for backgrounds and sprites
	/// </summary>
	public static TileData ExtractTiles4bpp(ReadOnlySpan<byte> data) {
		const int bytesPerTile = 32;
		int tileCount = data.Length / bytesPerTile;
		var pixels = Decode4bppLinear(data, tileCount);

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
	/// Extract 8bpp linear tiles (64 bytes per 8x8 tile)
	/// Used for 256-color mode
	/// </summary>
	public static TileData ExtractTiles8bpp(ReadOnlySpan<byte> data) {
		const int bytesPerTile = 64;
		int tileCount = data.Length / bytesPerTile;
		var pixels = Decode8bppLinear(data, tileCount);

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
	/// Decode 4bpp linear tiles (GBA format)
	/// Each byte contains 2 pixels (low nibble first, then high nibble)
	/// </summary>
	public static byte[] Decode4bppLinear(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 32;
		const int pixelsPerTile = 64;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int i = 0; i < bytesPerTile; i++) {
				byte b = data[tileOffset + i];
				// Low nibble first
				pixels[pixelOffset + i * 2] = (byte)(b & 0x0f);
				// High nibble second
				pixels[pixelOffset + i * 2 + 1] = (byte)((b >> 4) & 0x0f);
			}
		}

		return pixels;
	}

	/// <summary>
	/// Decode 8bpp linear tiles (GBA format)
	/// Each byte is one pixel (palette index 0-255)
	/// </summary>
	public static byte[] Decode8bppLinear(ReadOnlySpan<byte> data, int tileCount) {
		const int bytesPerTile = 64;
		const int pixelsPerTile = 64;

		var pixels = new byte[tileCount * pixelsPerTile];

		for (int tile = 0; tile < tileCount; tile++) {
			int tileOffset = tile * bytesPerTile;
			int pixelOffset = tile * pixelsPerTile;

			if (tileOffset + bytesPerTile > data.Length)
				break;

			for (int i = 0; i < bytesPerTile; i++) {
				pixels[pixelOffset + i] = data[tileOffset + i];
			}
		}

		return pixels;
	}

	/// <summary>
	/// Extract Mode 3 bitmap (240x160 @ 15-bit direct color)
	/// </summary>
	public static uint[] ExtractMode3Bitmap(ReadOnlySpan<byte> data, int offset = 0) {
		const int width = 240;
		const int height = 160;
		const int totalPixels = width * height;
		const int bytesNeeded = totalPixels * 2;

		if (offset + bytesNeeded > data.Length) {
			throw new ArgumentException($"Not enough data for Mode 3 bitmap. Need {bytesNeeded} bytes.");
		}

		var pixels = new uint[totalPixels];
		var slice = data.Slice(offset, bytesNeeded);

		for (int i = 0; i < totalPixels; i++) {
			ushort bgr555 = (ushort)(slice[i * 2] | (slice[i * 2 + 1] << 8));
			pixels[i] = Bgr555ToArgb(bgr555);
		}

		return pixels;
	}

	/// <summary>
	/// Extract Mode 5 bitmap (160x128 @ 15-bit direct color, double-buffered)
	/// </summary>
	public static uint[] ExtractMode5Bitmap(ReadOnlySpan<byte> data, int offset = 0, int frame = 0) {
		const int width = 160;
		const int height = 128;
		const int totalPixels = width * height;
		const int bytesPerFrame = totalPixels * 2;

		int frameOffset = offset + (frame * bytesPerFrame);
		if (frameOffset + bytesPerFrame > data.Length) {
			throw new ArgumentException($"Not enough data for Mode 5 frame {frame}.");
		}

		var pixels = new uint[totalPixels];
		var slice = data.Slice(frameOffset, bytesPerFrame);

		for (int i = 0; i < totalPixels; i++) {
			ushort bgr555 = (ushort)(slice[i * 2] | (slice[i * 2 + 1] << 8));
			pixels[i] = Bgr555ToArgb(bgr555);
		}

		return pixels;
	}

	/// <summary>
	/// Extract and convert GBA BGR555 palette to ARGB
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
	/// Convert GBA BGR555 color to ARGB32
	/// GBA uses same 15-bit format as SNES: xBBBBBGG GGGRRRRR
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
		var bmpData = TileGraphics.SaveToBmp(imageData, tiles.Width, tiles.Height);
		File.WriteAllBytes(path, bmpData);
	}

	/// <summary>
	/// Save direct color bitmap as BMP
	/// </summary>
	public static void SaveBitmapAsBmp(uint[] pixels, int width, int height, string path) {
		var bmpData = TileGraphics.SaveToBmp(pixels, width, height);
		File.WriteAllBytes(path, bmpData);
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
			bytesPerTile = tiles.BitDepth switch { 4 => 32, 8 => 64, _ => 0 },
			format = tiles.BitDepth switch { 4 => "GBA 4bpp linear", 8 => "GBA 8bpp linear", _ => "unknown" }
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
	/// Standard GBA 16-color grayscale palette
	/// </summary>
	public static readonly uint[] Grayscale16 = GetGrayscalePalette(16);

	/// <summary>
	/// Standard GBA 256-color grayscale palette
	/// </summary>
	public static readonly uint[] Grayscale256 = GetGrayscalePalette(256);
}

/// <summary>
/// GBA OAM (Object Attribute Memory) sprite parser
/// </summary>
public static class GbaOamParser {
	/// <summary>
	/// OAM Entry (8 bytes per sprite)
	/// </summary>
	public record OamEntry(
		int Y,              // Y coordinate (can be negative via sign extension)
		bool Mosaic,        // Mosaic effect
		bool Disable,       // 0=Normal, 1=Disabled (or double-size if affine)
		ObjMode Mode,       // Normal/Semi-transparent/Window
		bool Affine,        // Use affine transformation
		int Shape,          // 0=Square, 1=Wide, 2=Tall
		int X,              // X coordinate (can be negative)
		int AffineIndex,    // Affine parameter index (if affine)
		bool HFlip,         // Horizontal flip (if not affine)
		bool VFlip,         // Vertical flip (if not affine)
		int Size,           // Size index (combined with Shape for actual dimensions)
		int TileIndex,      // Tile index in VRAM
		int Priority,       // Drawing priority (0-3)
		int Palette         // Palette bank (for 4bpp mode)
	) {
		/// <summary>
		/// Get sprite dimensions in pixels based on shape and size
		/// </summary>
		public (int Width, int Height) GetDimensions() {
			return (Shape, Size) switch {
				// Square
				(0, 0) => (8, 8),
				(0, 1) => (16, 16),
				(0, 2) => (32, 32),
				(0, 3) => (64, 64),
				// Wide (horizontal)
				(1, 0) => (16, 8),
				(1, 1) => (32, 8),
				(1, 2) => (32, 16),
				(1, 3) => (64, 32),
				// Tall (vertical)
				(2, 0) => (8, 16),
				(2, 1) => (8, 32),
				(2, 2) => (16, 32),
				(2, 3) => (32, 64),
				_ => (8, 8)
			};
		}

		/// <summary>
		/// Get number of tiles needed for this sprite
		/// </summary>
		public int GetTileCount() {
			var (w, h) = GetDimensions();
			return (w / 8) * (h / 8);
		}
	}

	public enum ObjMode {
		Normal = 0,
		SemiTransparent = 1,
		Window = 2,
		Prohibited = 3
	}

	/// <summary>
	/// Parse OAM entries from raw data
	/// </summary>
	public static List<OamEntry> ParseOam(ReadOnlySpan<byte> oamData, int maxEntries = 128) {
		var entries = new List<OamEntry>();
		int count = Math.Min(maxEntries, oamData.Length / 8);

		for (int i = 0; i < count; i++) {
			int offset = i * 8;

			ushort attr0 = (ushort)(oamData[offset] | (oamData[offset + 1] << 8));
			ushort attr1 = (ushort)(oamData[offset + 2] | (oamData[offset + 3] << 8));
			ushort attr2 = (ushort)(oamData[offset + 4] | (oamData[offset + 5] << 8));
			// attr3 (offset + 6, 7) is used for affine parameters, not stored in entry

			// Parse Attribute 0
			int y = attr0 & 0xff;
			if (y >= 160) y -= 256; // Sign extend for negative Y
			bool affine = (attr0 & 0x0100) != 0;
			bool disable = (attr0 & 0x0200) != 0;
			var mode = (ObjMode)((attr0 >> 10) & 0x03);
			bool mosaic = (attr0 & 0x1000) != 0;
			bool is8bpp = (attr0 & 0x2000) != 0;
			int shape = (attr0 >> 14) & 0x03;

			// Parse Attribute 1
			int x = attr1 & 0x1ff;
			if (x >= 240) x -= 512; // Sign extend for negative X
			int affineIdx = (attr1 >> 9) & 0x1f;
			bool hFlip = !affine && (attr1 & 0x1000) != 0;
			bool vFlip = !affine && (attr1 & 0x2000) != 0;
			int size = (attr1 >> 14) & 0x03;

			// Parse Attribute 2
			int tileIndex = attr2 & 0x3ff;
			int priority = (attr2 >> 10) & 0x03;
			int palette = (attr2 >> 12) & 0x0f;

			entries.Add(new OamEntry(
				y, mosaic, disable, mode, affine, shape,
				x, affineIdx, hFlip, vFlip, size,
				tileIndex, priority, palette
			));
		}

		return entries;
	}

	/// <summary>
	/// Extract sprite tiles from VRAM based on OAM entry
	/// </summary>
	public static byte[] ExtractSpriteTiles(ReadOnlySpan<byte> vramData, OamEntry entry, bool is8bpp) {
		var (width, height) = entry.GetDimensions();
		int tilesX = width / 8;
		int tilesY = height / 8;
		int bytesPerTile = is8bpp ? 64 : 32;
		int totalBytes = tilesX * tilesY * bytesPerTile;

		var result = new byte[totalBytes];
		int tileIndex = entry.TileIndex;

		// GBA uses 1D or 2D tile mapping based on DISPCNT.6
		// For simplicity, assume 1D mapping (most common)
		int srcOffset = tileIndex * bytesPerTile;
		if (srcOffset + totalBytes <= vramData.Length) {
			vramData.Slice(srcOffset, totalBytes).CopyTo(result);
		}

		return result;
	}
}

/// <summary>
/// GBA LZ77 decompression (BIOS format)
/// </summary>
public static class GbaLz77 {
	/// <summary>
	/// Check if data appears to be LZ77 compressed (BIOS format)
	/// </summary>
	public static bool IsLz77Compressed(ReadOnlySpan<byte> data) {
		if (data.Length < 4) return false;
		// GBA BIOS LZ77 format starts with 0x10
		return data[0] == 0x10;
	}

	/// <summary>
	/// Decompress LZ77 data (BIOS format)
	/// Format: byte[0]=0x10, byte[1-3]=decompressed size (little endian)
	/// </summary>
	public static byte[] Decompress(ReadOnlySpan<byte> data) {
		if (data.Length < 4 || data[0] != 0x10) {
			throw new ArgumentException("Not valid LZ77 BIOS format (expected 0x10 header)");
		}

		int decompSize = data[1] | (data[2] << 8) | (data[3] << 16);
		var output = new byte[decompSize];
		int srcPos = 4;
		int dstPos = 0;

		while (dstPos < decompSize && srcPos < data.Length) {
			byte flags = data[srcPos++];

			for (int i = 0; i < 8 && dstPos < decompSize; i++) {
				if ((flags & 0x80) != 0) {
					// Compressed block
					if (srcPos + 1 >= data.Length) break;

					byte b1 = data[srcPos++];
					byte b2 = data[srcPos++];

					int length = ((b1 >> 4) & 0x0f) + 3;
					int disp = ((b1 & 0x0f) << 8) | b2;
					int srcOff = dstPos - disp - 1;

					for (int j = 0; j < length && dstPos < decompSize; j++) {
						output[dstPos++] = srcOff >= 0 ? output[srcOff++] : (byte)0;
					}
				} else {
					// Uncompressed byte
					if (srcPos >= data.Length) break;
					output[dstPos++] = data[srcPos++];
				}

				flags <<= 1;
			}
		}

		return output;
	}

	/// <summary>
	/// Try to decompress, returning null if not valid LZ77
	/// </summary>
	public static byte[]? TryDecompress(ReadOnlySpan<byte> data) {
		try {
			if (!IsLz77Compressed(data)) return null;
			return Decompress(data);
		}
		catch {
			return null;
		}
	}
}
