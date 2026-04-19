namespace Peony.Core.Tests;

using Xunit;

public class AssetDetectorTests {
	#region Shannon Entropy

	[Fact]
	public void ShannonEntropy_AllZeros_ReturnsZero() {
		var data = new byte[256];
		double entropy = AssetDetector.ShannonEntropy(data);
		Assert.Equal(0, entropy, 3);
	}

	[Fact]
	public void ShannonEntropy_AllSameValue_ReturnsZero() {
		var data = new byte[128];
		Array.Fill(data, (byte)0xaa);
		double entropy = AssetDetector.ShannonEntropy(data);
		Assert.Equal(0, entropy, 3);
	}

	[Fact]
	public void ShannonEntropy_TwoValues_ReturnsOne() {
		// 128 bytes of 0x00 and 128 bytes of 0xff → 1.0 bit per byte
		var data = new byte[256];
		for (int i = 128; i < 256; i++) data[i] = 0xff;
		double entropy = AssetDetector.ShannonEntropy(data);
		Assert.Equal(1.0, entropy, 2);
	}

	[Fact]
	public void ShannonEntropy_UniformRandom_HighEntropy() {
		var data = new byte[4096];
		var rng = new Random(42);
		rng.NextBytes(data);
		double entropy = AssetDetector.ShannonEntropy(data);
		// Random data should have entropy close to 8.0
		Assert.InRange(entropy, 7.5, 8.0);
	}

	[Fact]
	public void ShannonEntropy_EmptyData_ReturnsZero() {
		double entropy = AssetDetector.ShannonEntropy(ReadOnlySpan<byte>.Empty);
		Assert.Equal(0, entropy, 3);
	}

	#endregion

	#region Bitplane Correlation

	[Fact]
	public void BitplaneCorrelation_IdenticalPairs_ReturnsOne() {
		// Every pair of bytes is identical → 100% bit agreement
		var data = new byte[64];
		for (int i = 0; i < data.Length; i += 2) {
			data[i] = 0x5a;
			data[i + 1] = 0x5a;
		}
		double corr = AssetDetector.BitplaneCorrelation(data);
		Assert.Equal(1.0, corr, 3);
	}

	[Fact]
	public void BitplaneCorrelation_InversePairs_ReturnsZero() {
		// Every pair is bitwise inverse → 0% agreement
		var data = new byte[64];
		for (int i = 0; i < data.Length; i += 2) {
			data[i] = 0x55;
			data[i + 1] = 0xaa;
		}
		double corr = AssetDetector.BitplaneCorrelation(data);
		Assert.Equal(0.0, corr, 3);
	}

	[Fact]
	public void BitplaneCorrelation_RandomData_NearHalf() {
		var data = new byte[1024];
		var rng = new Random(42);
		rng.NextBytes(data);
		double corr = AssetDetector.BitplaneCorrelation(data);
		// Random data should show ~50% agreement
		Assert.InRange(corr, 0.4, 0.6);
	}

	#endregion

	#region Row Similarity

	[Fact]
	public void RowSimilarity_AllSame_ReturnsOne() {
		var data = new byte[32];
		Array.Fill(data, (byte)0xab);
		double sim = AssetDetector.RowSimilarity(data, 2);
		Assert.Equal(1.0, sim, 3);
	}

	[Fact]
	public void RowSimilarity_Random_LowSimilarity() {
		var data = new byte[128];
		var rng = new Random(42);
		rng.NextBytes(data);
		double sim = AssetDetector.RowSimilarity(data, 2);
		Assert.InRange(sim, 0.3, 0.65);
	}

	#endregion

	#region Empty Tile Ratio

	[Fact]
	public void EmptyTileRatio_AllEmpty_ReturnsOne() {
		var data = new byte[128]; // 4 tiles of 32 bytes
		double ratio = AssetDetector.EmptyTileRatio(data, 32);
		Assert.Equal(1.0, ratio, 3);
	}

	[Fact]
	public void EmptyTileRatio_NoEmpty_ReturnsZero() {
		var data = new byte[128];
		Array.Fill(data, (byte)0xff);
		double ratio = AssetDetector.EmptyTileRatio(data, 32);
		Assert.Equal(0.0, ratio, 3);
	}

	[Fact]
	public void EmptyTileRatio_HalfEmpty_ReturnsHalf() {
		var data = new byte[128];
		// First 2 tiles non-empty
		for (int i = 0; i < 64; i++) data[i] = (byte)(i + 1);
		// Last 2 tiles empty (already zeros)
		double ratio = AssetDetector.EmptyTileRatio(data, 32);
		Assert.Equal(0.5, ratio, 3);
	}

	#endregion

	#region Tile Confidence

	[Fact]
	public void AnalyzeTileConfidence_ReturnsStructuredResult() {
		var data = new byte[256]; // 8 tiles of 4bpp
		var rng = new Random(42);
		rng.NextBytes(data);
		var conf = AssetDetector.AnalyzeTileConfidence(data, 4);

		Assert.InRange(conf.Combined, 0, 1);
		Assert.InRange(conf.EntropyScore, 0, 1);
		Assert.InRange(conf.CorrelationScore, 0, 1);
		Assert.InRange(conf.RowScore, 0, 1);
		Assert.InRange(conf.EmptyScore, 0, 1);
		Assert.True(conf.Entropy > 0);
	}

	[Fact]
	public void AnalyzeTileConfidence_TooSmall_ReturnsZero() {
		var data = new byte[8]; // Too small for a 4bpp tile
		var conf = AssetDetector.AnalyzeTileConfidence(data, 4);
		Assert.Equal(0, conf.Combined, 3);
	}

	[Fact]
	public void AnalyzeTileConfidence_AllZeros_LowConfidence() {
		var data = new byte[256];
		var conf = AssetDetector.AnalyzeTileConfidence(data, 4);
		// All zeros = blank data, entropy is zero but other heuristics may still score
		Assert.InRange(conf.Combined, 0, 0.7);
	}

	#endregion

	#region Palette Confidence

	[Fact]
	public void PaletteConfidence_ValidBgr555Palette_HighConfidence() {
		// Build a palette that looks real: starts with black, has variety
		var data = new byte[32]; // 16 colors
		data[0] = 0x00; data[1] = 0x00; // Black
		data[2] = 0x1f; data[3] = 0x00; // Red
		data[4] = 0xe0; data[5] = 0x03; // Green
		data[6] = 0x00; data[7] = 0x7c; // Blue
		data[8] = 0xff; data[9] = 0x03; // Yellow
		data[10] = 0x1f; data[11] = 0x7c; // Magenta
		data[12] = 0xe0; data[13] = 0x7f; // Cyan
		data[14] = 0xff; data[15] = 0x7f; // White
		data[16] = 0x10; data[17] = 0x02; // Dark gray-ish
		data[18] = 0x0f; data[19] = 0x00; // Dark red
		data[20] = 0x00; data[21] = 0x00; // Black duplicate
		data[22] = 0x55; data[23] = 0x2a; // Mixed
		data[24] = 0x00; data[25] = 0x40; // Some blue
		data[26] = 0xef; data[27] = 0x01; // Olive-ish
		data[28] = 0x31; data[29] = 0x46; // Teal-ish
		data[30] = 0x11; data[31] = 0x11; // Mid-tone

		double conf = AssetDetector.PaletteConfidence(data);
		Assert.True(conf >= 0.5, $"Expected palette confidence >= 0.5, got {conf}");
	}

	[Fact]
	public void PaletteConfidence_InvalidBit15Set_ReturnsZero() {
		// All entries have bit 15 set — invalid BGR555
		var data = new byte[32];
		for (int i = 0; i < data.Length; i += 2) {
			data[i] = 0xff;
			data[i + 1] = 0xff; // bit 15 set
		}
		double conf = AssetDetector.PaletteConfidence(data);
		Assert.Equal(0, conf, 3);
	}

	[Fact]
	public void PaletteConfidence_OddSize_ReturnsZero() {
		var data = new byte[31];
		double conf = AssetDetector.PaletteConfidence(data);
		Assert.Equal(0, conf, 3);
	}

	[Fact]
	public void PaletteConfidence_AllBlack_VeryLow() {
		var data = new byte[32]; // All zeros — all black
		double conf = AssetDetector.PaletteConfidence(data);
		Assert.InRange(conf, 0, 0.15);
	}

	#endregion

	#region Tilemap Confidence

	[Fact]
	public void TilemapConfidence_ValidTilemap_HighConfidence() {
		// Create a 32x32 tilemap with realistic entries
		var data = new byte[2048]; // 1024 entries
		var rng = new Random(42);
		for (int i = 0; i < data.Length; i += 2) {
			// Tile number 0-127, palette 0-2, no flips
			int tileNum = rng.Next(0, 128);
			int palette = rng.Next(0, 3);
			ushort word = (ushort)(tileNum | (palette << 10));
			data[i] = (byte)(word & 0xff);
			data[i + 1] = (byte)(word >> 8);
		}
		double conf = AssetDetector.TilemapConfidence(data);
		Assert.True(conf >= 0.5, $"Expected tilemap confidence >= 0.5, got {conf}");
	}

	[Fact]
	public void TilemapConfidence_AllZeros_VeryLow() {
		var data = new byte[2048];
		double conf = AssetDetector.TilemapConfidence(data);
		Assert.InRange(conf, 0, 0.2);
	}

	[Fact]
	public void TilemapConfidence_OddSize_ReturnsZero() {
		var data = new byte[2049];
		double conf = AssetDetector.TilemapConfidence(data);
		Assert.Equal(0, conf, 3);
	}

	#endregion

	#region Scan For Tiles

	[Fact]
	public void ScanForTiles_EmptyRom_ReturnsEmpty() {
		var rom = new byte[0];
		var results = AssetDetector.ScanForTiles(rom);
		Assert.Empty(results);
	}

	[Fact]
	public void ScanForTiles_SmallRom_ReturnsEmpty() {
		var rom = new byte[16]; // Too small
		var results = AssetDetector.ScanForTiles(rom);
		Assert.Empty(results);
	}

	[Fact]
	public void ScanForTiles_ReturnsRegionsWithCorrectTypes() {
		// Create a ROM with a tile-like block in the middle
		var rom = new byte[0x10000];
		var rng = new Random(42);
		rng.NextBytes(rom);

		// Write a block of tile-like data at offset 0x4000
		// Tiles: pairs of bytes with high correlation, moderate entropy
		for (int t = 0; t < 64; t++) { // 64 tiles
			int baseOff = 0x4000 + t * 32;
			for (int row = 0; row < 8; row++) {
				// Two correlated bitplanes per row
				byte plane = (byte)((row * 37 + t * 13) & 0xff);
				rom[baseOff + row * 2] = plane;
				rom[baseOff + row * 2 + 1] = (byte)(plane ^ 0x0f); // Slight difference
				rom[baseOff + 16 + row * 2] = (byte)(plane >> 1);
				rom[baseOff + 16 + row * 2 + 1] = (byte)((plane >> 1) ^ 0x03);
			}
		}

		var results = AssetDetector.ScanForTiles(rom, new AssetScanOptions { MinConfidence = 0.4 });

		foreach (var r in results) {
			Assert.Equal(AssetRegionType.Tiles, r.Type);
			Assert.Equal(DetectionSource.Heuristic, r.Source);
			Assert.True(r.Size > 0);
			Assert.True(r.TileCount > 0);
		}
	}

	#endregion

	#region Scan For Palettes

	[Fact]
	public void ScanForPalettes_EmptyRom_ReturnsEmpty() {
		var rom = new byte[0];
		var results = AssetDetector.ScanForPalettes(rom);
		Assert.Empty(results);
	}

	#endregion

	#region Scan For Tilemaps

	[Fact]
	public void ScanForTilemaps_EmptyRom_ReturnsEmpty() {
		var rom = new byte[0];
		var results = AssetDetector.ScanForTilemaps(rom);
		Assert.Empty(results);
	}

	#endregion
}
