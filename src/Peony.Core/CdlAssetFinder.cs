namespace Peony.Core;

/// <summary>
/// CDL-guided asset region detection.
/// Uses Code/Data Log DRAWN flag to identify ROM regions containing graphics data
/// that was actually transferred to VRAM during emulation.
/// </summary>
public static class CdlAssetFinder {
	/// <summary>CDL flag: byte was DMA'd to VRAM (rendered as graphics)</summary>
	private const byte DRAWN = 0x20;

	/// <summary>CDL flag: byte was read as data</summary>
	private const byte DATA = 0x02;

	/// <summary>CDL flag: byte was executed as code</summary>
	private const byte CODE = 0x01;

	/// <summary>
	/// Find contiguous regions marked as DRAWN in CDL data.
	/// These regions were transferred to VRAM and are very likely tile/tilemap/palette data.
	/// </summary>
	public static List<DetectedAssetRegion> FindDrawnRegions(ReadOnlySpan<byte> cdl, int minSize = 32) {
		var regions = new List<DetectedAssetRegion>();
		int start = -1;

		for (int i = 0; i < cdl.Length; i++) {
			bool isDrawn = (cdl[i] & DRAWN) != 0;

			if (isDrawn) {
				if (start < 0) start = i;
			} else {
				if (start >= 0) {
					int size = i - start;
					if (size >= minSize) {
						regions.Add(ClassifyDrawnRegion(start, size));
					}
					start = -1;
				}
			}
		}

		// Handle region at end of CDL
		if (start >= 0) {
			int size = cdl.Length - start;
			if (size >= minSize) {
				regions.Add(ClassifyDrawnRegion(start, size));
			}
		}

		return regions;
	}

	/// <summary>
	/// Find DATA-only regions (not CODE, not DRAWN) that may be palettes or tilemaps.
	/// </summary>
	public static List<DetectedAssetRegion> FindDataRegions(ReadOnlySpan<byte> cdl, int minSize = 8) {
		var regions = new List<DetectedAssetRegion>();
		int start = -1;

		for (int i = 0; i < cdl.Length; i++) {
			byte flags = cdl[i];
			bool isDataOnly = (flags & DATA) != 0 && (flags & CODE) == 0 && (flags & DRAWN) == 0;

			if (isDataOnly) {
				if (start < 0) start = i;
			} else {
				if (start >= 0) {
					int size = i - start;
					if (size >= minSize) {
						regions.Add(new DetectedAssetRegion {
							Type = AssetRegionType.Unknown,
							RomOffset = start,
							Size = size,
							Confidence = 0.5, // Medium — it's data but not specifically DRAWN
							Source = DetectionSource.CDL,
							Details = "DATA-only region (not CODE, not DRAWN)"
						});
					}
					start = -1;
				}
			}
		}

		if (start >= 0) {
			int size = cdl.Length - start;
			if (size >= minSize) {
				regions.Add(new DetectedAssetRegion {
					Type = AssetRegionType.Unknown,
					RomOffset = start,
					Size = size,
					Confidence = 0.5,
					Source = DetectionSource.CDL,
					Details = "DATA-only region (not CODE, not DRAWN)"
				});
			}
		}

		return regions;
	}

	/// <summary>
	/// Full CDL-guided asset extraction: find DRAWN regions, classify by content,
	/// then check DATA regions for palettes/tilemaps.
	/// </summary>
	public static List<DetectedAssetRegion> FindAllAssets(ReadOnlySpan<byte> cdl, ReadOnlySpan<byte> rom) {
		var results = new List<DetectedAssetRegion>();

		// Phase 1: DRAWN regions → tiles (high confidence)
		var drawnRegions = FindDrawnRegions(cdl);
		foreach (var region in drawnRegions) {
			if (region.RomOffset + region.Size <= rom.Length) {
				var data = rom.Slice(region.RomOffset, region.Size);

				// Determine best bit depth by trying each and scoring
				int bestBpp = DetectBitDepth(data);
				var confidence = AssetDetector.AnalyzeTileConfidence(data, bestBpp);

				results.Add(region with {
					BitDepth = bestBpp,
					TileCount = region.Size / (bestBpp switch { 2 => 16, 4 => 32, 8 => 64, _ => 32 }),
					Confidence = Math.Max(region.Confidence, confidence.Combined),
					Details = $"CDL DRAWN, {bestBpp}bpp, entropy={confidence.Entropy:f2}"
				});
			}
		}

		// Phase 2: DATA regions → check for palettes and tilemaps
		var dataRegions = FindDataRegions(cdl);
		foreach (var region in dataRegions) {
			if (region.RomOffset + region.Size > rom.Length) continue;
			var data = rom.Slice(region.RomOffset, region.Size);

			// Check if it's a palette
			if (region.Size is >= 8 and <= 512 && region.Size % 2 == 0) {
				double palConf = AssetDetector.PaletteConfidence(data);
				if (palConf >= 0.5) {
					results.Add(region with {
						Type = AssetRegionType.Palette,
						Confidence = palConf * 0.9, // Slightly lower than DRAWN regions
						TileCount = region.Size / 2,
						Details = $"CDL DATA, {region.Size / 2} colors, conf={palConf:f2}"
					});
					continue;
				}
			}

			// Check if it's a tilemap
			if (region.Size is >= 2048 and <= 8192 && region.Size % 2 == 0) {
				double mapConf = AssetDetector.TilemapConfidence(data);
				if (mapConf >= 0.5) {
					results.Add(region with {
						Type = AssetRegionType.Tilemap,
						Confidence = mapConf * 0.9,
						TileCount = region.Size / 2,
						Details = $"CDL DATA, tilemap conf={mapConf:f2}"
					});
					continue;
				}
			}

			// Check if it might be tiles that weren't flagged DRAWN (rare but possible)
			if (region.Size >= 32) {
				int bpp = DetectBitDepth(data);
				var tileConf = AssetDetector.AnalyzeTileConfidence(data, bpp);
				if (tileConf.Combined >= 0.6) {
					results.Add(region with {
						Type = AssetRegionType.Tiles,
						BitDepth = bpp,
						TileCount = region.Size / (bpp switch { 2 => 16, 4 => 32, 8 => 64, _ => 32 }),
						Confidence = tileConf.Combined * 0.8,
						Details = $"CDL DATA, {bpp}bpp tiles, conf={tileConf.Combined:f2}"
					});
				}
			}
		}

		return results;
	}

	/// <summary>
	/// Classify a DRAWN region by size and alignment.
	/// </summary>
	private static DetectedAssetRegion ClassifyDrawnRegion(int offset, int size) {
		// Determine probable bit depth from size alignment
		int bitDepth = 4; // Default SNES

		if (size % 64 == 0 && size >= 256) {
			// Could be 8bpp — check if also divisible by 32
			bitDepth = 8;
		} else if (size % 32 == 0) {
			bitDepth = 4;
		} else if (size % 16 == 0) {
			bitDepth = 2;
		}

		int tileSize = bitDepth switch { 2 => 16, 4 => 32, 8 => 64, _ => 32 };

		return new DetectedAssetRegion {
			Type = AssetRegionType.Tiles,
			RomOffset = offset,
			Size = size,
			Confidence = 0.85, // CDL DRAWN is high confidence
			BitDepth = bitDepth,
			TileCount = size / tileSize,
			Source = DetectionSource.CDL,
			Details = $"CDL DRAWN region ({size} bytes)"
		};
	}

	/// <summary>
	/// Detect the most likely bit depth for tile data by scoring each option.
	/// </summary>
	public static int DetectBitDepth(ReadOnlySpan<byte> data) {
		double best4 = 0, best2 = 0, best8 = 0;

		if (data.Length >= 32) {
			var conf4 = AssetDetector.AnalyzeTileConfidence(data, 4);
			best4 = conf4.Combined;
		}

		if (data.Length >= 16) {
			var conf2 = AssetDetector.AnalyzeTileConfidence(data, 2);
			best2 = conf2.Combined;
		}

		if (data.Length >= 64) {
			var conf8 = AssetDetector.AnalyzeTileConfidence(data, 8);
			best8 = conf8.Combined;
		}

		if (best4 >= best2 && best4 >= best8) return 4;
		if (best2 >= best8) return 2;
		return 8;
	}
}
