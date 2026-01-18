namespace Peony.Platform.NES;

using Peony.Core;
using System.Text.Json;

/// <summary>
/// NES CHR (Character ROM) graphics extractor
/// Extracts 2bpp tile data from NES ROMs and converts to images
/// </summary>
public class NesChrExtractor : IGraphicsExtractor {
	public string Platform => "NES";

	private const int HeaderSize = 16;
	private const int PrgBankSize = 16384; // 16KB
	private const int ChrBankSize = 8192;  // 8KB
	private const int BytesPerTile = 16;   // 2bpp, 8x8 = 16 bytes
	private const int TilesPerChrBank = 512; // 8192 / 16

	/// <summary>
	/// Extract all CHR graphics from an NES ROM
	/// </summary>
	public GraphicsExtractionResult ExtractAll(ReadOnlySpan<byte> rom, GraphicsExtractionOptions options) {
		var tileSets = new List<TileSetInfo>();
		var palettes = new List<PaletteInfo>();
		var outputFiles = new List<string>();

		// Validate iNES header
		if (!ValidateHeader(rom)) {
			return new GraphicsExtractionResult {
				TileSets = tileSets,
				Palettes = palettes,
				OutputFiles = outputFiles
			};
		}

		// Parse header
		int prgBanks = rom[4];
		int chrBanks = rom[5];
		bool hasTrainer = (rom[6] & 0x04) != 0;

		// Calculate CHR-ROM offset
		int headerSize = HeaderSize + (hasTrainer ? 512 : 0);
		int chrOffset = headerSize + (prgBanks * PrgBankSize);
		int chrSize = chrBanks * ChrBankSize;

		if (chrBanks == 0) {
			// CHR-RAM - no graphics in ROM
			return new GraphicsExtractionResult {
				TileSets = tileSets,
				Palettes = palettes,
				OutputFiles = outputFiles
			};
		}

		// Ensure output directory exists
		Directory.CreateDirectory(options.OutputDirectory);

		// Extract each CHR bank
		for (int bank = 0; bank < chrBanks; bank++) {
			int bankOffset = chrOffset + (bank * ChrBankSize);
			int bankSize = Math.Min(ChrBankSize, rom.Length - bankOffset);

			if (bankSize <= 0) break;

			var tileSet = ExtractChrBank(rom, bankOffset, bankSize, bank, options);
			tileSets.Add(tileSet);

			if (tileSet.OutputPath != null) {
				outputFiles.Add(tileSet.OutputPath);
			}
		}

		// Generate metadata JSON
		if (options.GenerateMetadata) {
			var metadataPath = Path.Combine(options.OutputDirectory, "extraction_manifest.json");
			GenerateMetadata(tileSets, palettes, metadataPath);
			outputFiles.Add(metadataPath);
		}

		return new GraphicsExtractionResult {
			TileSets = tileSets,
			Palettes = palettes,
			OutputFiles = outputFiles
		};
	}

	/// <summary>
	/// Extract tiles from specific ROM offset
	/// </summary>
	public TileData ExtractTiles(ReadOnlySpan<byte> rom, int offset, int size, int bitDepth) {
		if (bitDepth != 2) {
			throw new ArgumentException("NES only supports 2bpp tiles", nameof(bitDepth));
		}

		int tileCount = size / BytesPerTile;
		var chrData = rom.Slice(offset, Math.Min(size, rom.Length - offset));
		var pixels = TileGraphics.Decode2bppPlanar(chrData, tileCount);

		return new TileData {
			Pixels = pixels,
			Width = 8,
			Height = tileCount * 8, // One tile per row conceptually
			TileCount = tileCount,
			BitDepth = 2
		};
	}

	/// <summary>
	/// Extract a single CHR bank
	/// </summary>
	private TileSetInfo ExtractChrBank(ReadOnlySpan<byte> rom, int offset, int size, int bankIndex, GraphicsExtractionOptions options) {
		int tileCount = size / BytesPerTile;
		var chrData = rom.Slice(offset, size);
		var pixels = TileGraphics.Decode2bppPlanar(chrData, tileCount);

		string? outputPath = null;

		// Generate image if requested
		if (options.ImageFormat.Equals("png", StringComparison.OrdinalIgnoreCase) ||
			options.ImageFormat.Equals("bmp", StringComparison.OrdinalIgnoreCase)) {
			var palette = options.Palette ?? TileGraphics.NesGrayscale;
			var imageData = TileGraphics.ArrangeTiles(pixels, tileCount, options.TilesPerRow, palette);

			int imageWidth = options.TilesPerRow * 8;
			int imageHeight = ((tileCount + options.TilesPerRow - 1) / options.TilesPerRow) * 8;

			outputPath = Path.Combine(options.OutputDirectory, $"chr_bank_{bankIndex:x2}.{options.ImageFormat}");
			SaveImage(imageData, imageWidth, imageHeight, outputPath);

			// Also save raw CHR binary
			var binPath = Path.Combine(options.OutputDirectory, $"chr_bank_{bankIndex:x2}.chr");
			File.WriteAllBytes(binPath, chrData.ToArray());
		}

		return new TileSetInfo {
			Name = $"CHR Bank {bankIndex:X2}",
			RomOffset = offset,
			SizeBytes = size,
			TileCount = tileCount,
			BitDepth = 2,
			TileWidth = 8,
			TileHeight = 8,
			OutputPath = outputPath
		};
	}

	/// <summary>
	/// Validate iNES header
	/// </summary>
	private static bool ValidateHeader(ReadOnlySpan<byte> rom) {
		if (rom.Length < HeaderSize) return false;
		return rom[0] == 0x4e && rom[1] == 0x45 && rom[2] == 0x53 && rom[3] == 0x1a;
	}

	/// <summary>
	/// Save ARGB image data to file (BMP format - no external dependencies)
	/// </summary>
	private static void SaveImage(uint[] pixels, int width, int height, string path) {
		// For now, save as simple BMP (can be viewed in any image viewer)
		// Note: BMP is stored bottom-up
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
		writer.Write((short)0); // Reserved
		writer.Write((short)0); // Reserved
		writer.Write(54); // Pixel data offset

		// DIB header (40 bytes - BITMAPINFOHEADER)
		writer.Write(40); // Header size
		writer.Write(width);
		writer.Write(height);
		writer.Write((short)1); // Color planes
		writer.Write((short)24); // Bits per pixel
		writer.Write(0); // Compression (none)
		writer.Write(imageSize);
		writer.Write(2835); // Horizontal resolution (72 DPI)
		writer.Write(2835); // Vertical resolution
		writer.Write(0); // Colors in palette
		writer.Write(0); // Important colors

		// Pixel data (bottom-up, BGR)
		var rowBuffer = new byte[rowSize];
		for (int y = height - 1; y >= 0; y--) {
			for (int x = 0; x < width; x++) {
				uint argb = pixels[y * width + x];
				rowBuffer[x * 3 + 0] = (byte)(argb & 0xff);        // Blue
				rowBuffer[x * 3 + 1] = (byte)((argb >> 8) & 0xff); // Green
				rowBuffer[x * 3 + 2] = (byte)((argb >> 16) & 0xff);// Red
			}
			writer.Write(rowBuffer);
		}
	}

	/// <summary>
	/// Generate extraction metadata JSON
	/// </summary>
	private static void GenerateMetadata(List<TileSetInfo> tileSets, List<PaletteInfo> palettes, string path) {
		var metadata = new {
			generated = DateTime.UtcNow.ToString("o"),
			platform = "NES",
			total_tiles = tileSets.Sum(t => t.TileCount),
			tilesets = tileSets.Select(t => new {
				name = t.Name,
				rom_offset = $"0x{t.RomOffset:x}",
				size_bytes = t.SizeBytes,
				tile_count = t.TileCount,
				bit_depth = t.BitDepth,
				output_path = t.OutputPath
			}),
			palettes = palettes.Select(p => new {
				name = p.Name,
				rom_offset = $"0x{p.RomOffset:x}",
				color_count = p.Colors.Length
			})
		};

		var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
		});
		File.WriteAllText(path, json);
	}
}
