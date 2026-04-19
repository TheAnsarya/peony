namespace Peony.Core;

using System.Numerics;

/// <summary>
/// Heuristic analysis for detecting tile data, palettes, and tilemaps in ROM data.
/// Uses entropy analysis, bitplane correlation, and structural pattern detection.
/// </summary>
public static class AssetDetector {
	/// <summary>
	/// Compute Shannon entropy of a data block (bits per byte).
	/// Tile data: ~4.5-6.0, Code: ~6.5-7.5, Compressed: ~7.0-7.9, Random: ~8.0
	/// </summary>
	public static double ShannonEntropy(ReadOnlySpan<byte> data) {
		if (data.Length == 0) return 0;

		Span<int> freq = stackalloc int[256];
		foreach (byte b in data) {
			freq[b]++;
		}

		double entropy = 0;
		double len = data.Length;
		for (int i = 0; i < 256; i++) {
			if (freq[i] == 0) continue;
			double p = freq[i] / len;
			entropy -= p * Math.Log2(p);
		}

		return entropy;
	}

	/// <summary>
	/// Compute bitplane correlation for tile data detection.
	/// In real SNES tiles, adjacent bytes are different bitplanes of the same pixel row.
	/// Real tiles show 55-80%+ bit agreement; random data shows ~50%.
	/// </summary>
	public static double BitplaneCorrelation(ReadOnlySpan<byte> data) {
		if (data.Length < 2) return 0.5;

		int matches = 0;
		int total = 0;

		for (int i = 0; i < data.Length - 1; i += 2) {
			byte plane0 = data[i];
			byte plane1 = data[i + 1];
			// Count bits that agree between adjacent bytes (same row, different bitplanes)
			int agreement = ~(plane0 ^ plane1) & 0xff;
			matches += BitOperations.PopCount((uint)agreement);
			total += 8;
		}

		return total > 0 ? (double)matches / total : 0.5;
	}

	/// <summary>
	/// Compute row similarity score. In real tiles, adjacent rows are similar
	/// (smooth gradients, outlines). XOR consecutive row pairs → low hamming weight.
	/// Returns 0.0-1.0 where higher = more similar (more likely tile data).
	/// </summary>
	public static double RowSimilarity(ReadOnlySpan<byte> data, int bytesPerRow = 2) {
		if (data.Length < bytesPerRow * 2) return 0;

		int totalDiff = 0;
		int totalBits = 0;

		for (int i = 0; i < data.Length - bytesPerRow; i += bytesPerRow) {
			for (int j = 0; j < bytesPerRow && i + bytesPerRow + j < data.Length; j++) {
				byte row1 = data[i + j];
				byte row2 = data[i + bytesPerRow + j];
				int diff = row1 ^ row2;
				totalDiff += BitOperations.PopCount((uint)(diff & 0xff));
				totalBits += 8;
			}
		}

		// Invert: low diff = high similarity
		return totalBits > 0 ? 1.0 - ((double)totalDiff / totalBits) : 0;
	}

	/// <summary>
	/// Check if data block has periodic empty (all-zero) regions typical of tilesets.
	/// Many tilesets have blank tiles interspersed. Returns fraction of empty tiles.
	/// </summary>
	public static double EmptyTileRatio(ReadOnlySpan<byte> data, int tileSize) {
		if (data.Length < tileSize) return 0;

		int tileCount = data.Length / tileSize;
		int emptyCount = 0;

		for (int t = 0; t < tileCount; t++) {
			int offset = t * tileSize;
			bool empty = true;
			for (int i = 0; i < tileSize && offset + i < data.Length; i++) {
				if (data[offset + i] != 0) {
					empty = false;
					break;
				}
			}
			if (empty) emptyCount++;
		}

		return (double)emptyCount / tileCount;
	}

	/// <summary>
	/// Compute a combined confidence score for whether data is tile graphics.
	/// Returns 0.0-1.0 where higher = more confident this is tile data.
	/// </summary>
	public static TileConfidence AnalyzeTileConfidence(ReadOnlySpan<byte> data, int bitDepth = 4) {
		int tileSize = bitDepth switch { 2 => 16, 4 => 32, 8 => 64, _ => 32 };

		if (data.Length < tileSize) {
			return new TileConfidence(0, 0, 0, 0, 0, 0);
		}

		double entropy = ShannonEntropy(data);
		double correlation = BitplaneCorrelation(data);
		double rowSim = RowSimilarity(data, bitDepth switch { 2 => 2, 4 => 2, 8 => 2, _ => 2 });
		double emptyRatio = EmptyTileRatio(data, tileSize);

		// Entropy score: peak at 4.5-6.0 for tile data
		double entropyScore;
		if (entropy < 1.0)
			entropyScore = 0.3; // Very low entropy — could be blank/simple data
		else if (entropy < 4.5)
			entropyScore = 0.5 + 0.1 * (entropy - 1.0) / 3.5;
		else if (entropy <= 6.0)
			entropyScore = 1.0; // Sweet spot for tile data
		else if (entropy <= 6.5)
			entropyScore = 1.0 - 0.5 * (entropy - 6.0) / 0.5;
		else
			entropyScore = Math.Max(0, 0.5 - 0.5 * (entropy - 6.5) / 1.5);

		// Correlation score: > 0.55 suggests tile data
		double correlationScore = Math.Clamp((correlation - 0.45) / 0.35, 0, 1);

		// Row similarity: higher is better for tiles
		double rowScore = Math.Clamp((rowSim - 0.4) / 0.3, 0, 1);

		// Empty tiles: some empty tiles (5-30%) is normal for tilesets
		double emptyScore = emptyRatio switch {
			> 0.8 => 0.2,  // Too many empty — probably just zeros
			> 0.3 => 0.5,
			> 0.02 => 0.9, // Some empty tiles = typical tileset
			_ => 0.6       // No empty tiles — still plausible
		};

		// Weighted combination
		double combined = (entropyScore * 0.35) + (correlationScore * 0.30) + (rowScore * 0.20) + (emptyScore * 0.15);

		return new TileConfidence(combined, entropyScore, correlationScore, rowScore, emptyScore, entropy);
	}

	/// <summary>
	/// Check if a block of data looks like a SNES BGR555 palette.
	/// Returns 0.0-1.0 confidence.
	/// </summary>
	public static double PaletteConfidence(ReadOnlySpan<byte> data) {
		if (data.Length < 4 || data.Length % 2 != 0 || data.Length > 512)
			return 0;

		int colorCount = data.Length / 2;
		int validColors = 0;
		int darkColors = 0;
		int blackCount = 0;
		int whiteCount = 0;
		var uniqueColors = new HashSet<ushort>();

		for (int i = 0; i < data.Length; i += 2) {
			ushort color = (ushort)(data[i] | (data[i + 1] << 8));

			// Bit 15 must be 0 for valid BGR555
			if ((color & 0x8000) != 0) return 0;

			validColors++;
			uniqueColors.Add(color);

			int r = color & 0x1f;
			int g = (color >> 5) & 0x1f;
			int b = (color >> 10) & 0x1f;
			int brightness = r + g + b;

			if (brightness < 10) darkColors++;
			if (color == 0x0000) blackCount++;
			if (color == 0x7fff) whiteCount++;
		}

		double score = 0;

		// Must have at least one dark/black color (common in SNES palettes)
		if (darkColors > 0) score += 0.25;

		// Color diversity: real palettes have limited but varied colors
		double diversity = (double)uniqueColors.Count / colorCount;
		if (diversity > 0.3 && diversity < 0.95) score += 0.25;
		else if (diversity >= 0.95) score += 0.1; // All unique — still possible but less likely

		// Standard subpalette sizes
		if (colorCount is 4 or 16 or 32 or 64 or 128 or 256) score += 0.25;
		else if (colorCount % 16 == 0) score += 0.15;

		// Having a black entry is very common
		if (blackCount > 0) score += 0.15;

		// Having all zeros (every color is black) is NOT a palette
		if (uniqueColors.Count <= 1) return 0.1;

		// Penalize if ALL entries look like sequential or patterned data
		bool isSequential = true;
		for (int i = 2; i < data.Length; i += 2) {
			ushort prev = (ushort)(data[i - 2] | (data[i - 1] << 8));
			ushort curr = (ushort)(data[i] | (data[i + 1] << 8));
			if (Math.Abs(curr - prev) > 1) {
				isSequential = false;
				break;
			}
		}
		if (isSequential && colorCount > 8) score *= 0.3;

		return Math.Clamp(score, 0, 1);
	}

	/// <summary>
	/// Check if a block of data looks like a SNES tilemap.
	/// Returns 0.0-1.0 confidence.
	/// </summary>
	public static double TilemapConfidence(ReadOnlySpan<byte> data) {
		if (data.Length < 4 || data.Length % 2 != 0)
			return 0;

		int entries = data.Length / 2;

		// Standard tilemap sizes: 1024 (32x32), 2048 (64x32 or 32x64), 4096 (64x64)
		bool isStandardSize = entries is 1024 or 2048 or 4096;

		int maxTileNum = 0;
		int flipCount = 0;
		Span<int> paletteHist = stackalloc int[8];
		int zeroEntries = 0;

		for (int i = 0; i < data.Length; i += 2) {
			ushort word = (ushort)(data[i] | (data[i + 1] << 8));
			int tileNum = word & 0x3ff;
			int palette = (word >> 10) & 7;
			bool xFlip = (word & 0x4000) != 0;
			bool yFlip = (word & 0x8000) != 0;

			if (tileNum > maxTileNum) maxTileNum = tileNum;
			paletteHist[palette]++;
			if (xFlip || yFlip) flipCount++;
			if (word == 0) zeroEntries++;
		}

		double score = 0;

		// Tile numbers should be bounded (not using all 1024)
		if (maxTileNum > 0 && maxTileNum < 512) score += 0.25;
		else if (maxTileNum > 0 && maxTileNum < 800) score += 0.15;

		// Most entries use 1-3 palettes
		int activePalettes = 0;
		for (int i = 0; i < 8; i++) {
			if (paletteHist[i] > entries / 20) activePalettes++;
		}
		if (activePalettes is >= 1 and <= 3) score += 0.25;
		else if (activePalettes <= 5) score += 0.15;

		// Flip bits should be minority
		double flipRatio = (double)flipCount / entries;
		if (flipRatio < 0.3) score += 0.2;
		else if (flipRatio < 0.5) score += 0.1;

		// Standard tilemap size bonus
		if (isStandardSize) score += 0.15;

		// Not all zeros
		if ((double)zeroEntries / entries > 0.9) return 0.1;

		// Some zeros are normal (empty/background tiles)
		if ((double)zeroEntries / entries is > 0.05 and < 0.5) score += 0.1;

		return Math.Clamp(score, 0, 1);
	}

	/// <summary>
	/// Scan ROM for potential tile data regions using heuristic analysis.
	/// </summary>
	public static List<DetectedAssetRegion> ScanForTiles(ReadOnlySpan<byte> rom, AssetScanOptions? options = null) {
		options ??= new AssetScanOptions();
		var regions = new List<DetectedAssetRegion>();

		int[] bitDepths = options.BitDepths ?? [4, 2, 8];

		foreach (int bpp in bitDepths) {
			int tileSize = bpp switch { 2 => 16, 4 => 32, 8 => 64, _ => 32 };
			int windowSize = options.WindowSize > 0 ? options.WindowSize : tileSize * 64; // 64 tiles per window
			int step = options.StepSize > 0 ? options.StepSize : tileSize * 16; // 16-tile steps

			for (int offset = 0; offset + windowSize <= rom.Length; offset += step) {
				// Skip if not aligned to tile boundary
				if (offset % tileSize != 0) continue;

				var window = rom.Slice(offset, windowSize);
				var confidence = AnalyzeTileConfidence(window, bpp);

				if (confidence.Combined >= options.MinConfidence) {
					// Try to expand the region
					int regionEnd = offset + windowSize;
					while (regionEnd + tileSize * 16 <= rom.Length) {
						var nextBlock = rom.Slice(regionEnd, Math.Min(tileSize * 16, rom.Length - regionEnd));
						var nextConf = AnalyzeTileConfidence(nextBlock, bpp);
						if (nextConf.Combined < options.MinConfidence * 0.8)
							break;
						regionEnd += nextBlock.Length;
					}

					int regionSize = regionEnd - offset;
					int tileCount = regionSize / tileSize;

					// Re-score the full region
					var fullRegion = rom.Slice(offset, regionSize);
					var fullConfidence = AnalyzeTileConfidence(fullRegion, bpp);

					regions.Add(new DetectedAssetRegion {
						Type = AssetRegionType.Tiles,
						RomOffset = offset,
						Size = regionSize,
						Confidence = fullConfidence.Combined,
						BitDepth = bpp,
						TileCount = tileCount,
						Source = DetectionSource.Heuristic,
						Details = $"entropy={fullConfidence.Entropy:f2}, corr={fullConfidence.CorrelationScore:f2}, rows={fullConfidence.RowScore:f2}"
					});

					// Skip past this region
					offset = regionEnd - step;
				}
			}
		}

		// Merge overlapping regions, keeping highest confidence
		return MergeOverlapping(regions);
	}

	/// <summary>
	/// Scan ROM for potential palette regions.
	/// </summary>
	public static List<DetectedAssetRegion> ScanForPalettes(ReadOnlySpan<byte> rom, AssetScanOptions? options = null) {
		options ??= new AssetScanOptions();
		var regions = new List<DetectedAssetRegion>();

		// Standard SNES palette sizes in bytes
		int[] paletteSizes = [32, 64, 128, 256, 512];

		foreach (int size in paletteSizes) {
			for (int offset = 0; offset + size <= rom.Length; offset += 2) {
				var window = rom.Slice(offset, size);
				double confidence = PaletteConfidence(window);

				if (confidence >= options.MinConfidence) {
					regions.Add(new DetectedAssetRegion {
						Type = AssetRegionType.Palette,
						RomOffset = offset,
						Size = size,
						Confidence = confidence,
						BitDepth = 0,
						TileCount = size / 2,
						Source = DetectionSource.Heuristic,
						Details = $"{size / 2} colors"
					});

					// Skip past this region
					offset += size - 2;
				}
			}
		}

		return MergeOverlapping(regions);
	}

	/// <summary>
	/// Scan ROM for potential tilemap regions.
	/// </summary>
	public static List<DetectedAssetRegion> ScanForTilemaps(ReadOnlySpan<byte> rom, AssetScanOptions? options = null) {
		options ??= new AssetScanOptions();
		var regions = new List<DetectedAssetRegion>();

		// Standard tilemap sizes: 2048, 4096, 8192 bytes
		int[] tilemapSizes = [2048, 4096, 8192];

		foreach (int size in tilemapSizes) {
			for (int offset = 0; offset + size <= rom.Length; offset += 2) {
				var window = rom.Slice(offset, size);
				double confidence = TilemapConfidence(window);

				if (confidence >= options.MinConfidence) {
					int entries = size / 2;
					int width = entries switch {
						1024 => 32,
						2048 => 64,
						4096 => 64,
						_ => 32
					};
					int height = entries / width;

					regions.Add(new DetectedAssetRegion {
						Type = AssetRegionType.Tilemap,
						RomOffset = offset,
						Size = size,
						Confidence = confidence,
						BitDepth = 0,
						TileCount = entries,
						Source = DetectionSource.Heuristic,
						Details = $"{width}x{height} ({entries} entries)"
					});

					offset += size - 2;
				}
			}
		}

		return MergeOverlapping(regions);
	}

	/// <summary>
	/// Merge overlapping detected regions, keeping the highest confidence.
	/// </summary>
	private static List<DetectedAssetRegion> MergeOverlapping(List<DetectedAssetRegion> regions) {
		if (regions.Count <= 1) return regions;

		var sorted = regions.OrderBy(r => r.RomOffset).ThenByDescending(r => r.Confidence).ToList();
		var merged = new List<DetectedAssetRegion>();

		foreach (var region in sorted) {
			bool overlaps = false;
			for (int i = 0; i < merged.Count; i++) {
				var existing = merged[i];
				if (existing.Type != region.Type) continue;

				int existEnd = existing.RomOffset + existing.Size;
				int regEnd = region.RomOffset + region.Size;

				// Check overlap
				if (region.RomOffset < existEnd && regEnd > existing.RomOffset) {
					overlaps = true;
					// Keep the higher-confidence one, or the larger one if equal
					if (region.Confidence > existing.Confidence ||
						(Math.Abs(region.Confidence - existing.Confidence) < 0.01 && region.Size > existing.Size)) {
						merged[i] = region;
					}
					break;
				}
			}

			if (!overlaps) {
				merged.Add(region);
			}
		}

		return merged;
	}
}

/// <summary>
/// Tile data confidence analysis result
/// </summary>
public record TileConfidence(
	double Combined,
	double EntropyScore,
	double CorrelationScore,
	double RowScore,
	double EmptyScore,
	double Entropy
);

/// <summary>
/// A detected asset region in the ROM
/// </summary>
public record DetectedAssetRegion {
	/// <summary>Type of asset detected</summary>
	public required AssetRegionType Type { get; init; }

	/// <summary>ROM file offset</summary>
	public int RomOffset { get; init; }

	/// <summary>Size in bytes</summary>
	public int Size { get; init; }

	/// <summary>Confidence score (0.0-1.0)</summary>
	public double Confidence { get; init; }

	/// <summary>Bit depth for tile data (2/4/8), 0 for non-tile types</summary>
	public int BitDepth { get; init; }

	/// <summary>Tile count (or color count for palettes, entry count for tilemaps)</summary>
	public int TileCount { get; init; }

	/// <summary>How this region was detected</summary>
	public DetectionSource Source { get; init; }

	/// <summary>Human-readable details</summary>
	public string Details { get; init; } = "";
}

/// <summary>
/// Type of detected asset region
/// </summary>
public enum AssetRegionType {
	Tiles,
	Palette,
	Tilemap,
	CompressedTiles,
	SpriteTable,
	Unknown
}

/// <summary>
/// How an asset region was detected
/// </summary>
public enum DetectionSource {
	Manual,
	CDL,
	Heuristic,
	DmaTrace,
	Config
}

/// <summary>
/// Options for asset scanning
/// </summary>
public record AssetScanOptions {
	/// <summary>Minimum confidence threshold (0.0-1.0)</summary>
	public double MinConfidence { get; init; } = 0.6;

	/// <summary>Window size in bytes for heuristic scanning</summary>
	public int WindowSize { get; init; } = 0;

	/// <summary>Step size in bytes between scan windows</summary>
	public int StepSize { get; init; } = 0;

	/// <summary>Bit depths to scan for (null = all: 4, 2, 8)</summary>
	public int[]? BitDepths { get; init; }
}
