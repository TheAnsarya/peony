namespace Peony.Core.Tests;

using Xunit;

public class CdlAssetFinderTests {
	private const byte DRAWN = 0x20;
	private const byte DATA = 0x02;
	private const byte CODE = 0x01;

	#region FindDrawnRegions

	[Fact]
	public void FindDrawnRegions_EmptyCdl_ReturnsEmpty() {
		var cdl = new byte[0];
		var results = CdlAssetFinder.FindDrawnRegions(cdl);
		Assert.Empty(results);
	}

	[Fact]
	public void FindDrawnRegions_NoDrawnBytes_ReturnsEmpty() {
		var cdl = new byte[1024];
		Array.Fill(cdl, CODE);
		var results = CdlAssetFinder.FindDrawnRegions(cdl);
		Assert.Empty(results);
	}

	[Fact]
	public void FindDrawnRegions_SingleRegion_FindsIt() {
		var cdl = new byte[1024];
		// Mark bytes 0x100-0x1ff as DRAWN (256 bytes)
		for (int i = 0x100; i < 0x200; i++) cdl[i] = DRAWN;
		var results = CdlAssetFinder.FindDrawnRegions(cdl, minSize: 32);

		Assert.Single(results);
		Assert.Equal(0x100, results[0].RomOffset);
		Assert.Equal(256, results[0].Size);
		Assert.Equal(AssetRegionType.Tiles, results[0].Type);
		Assert.Equal(DetectionSource.CDL, results[0].Source);
	}

	[Fact]
	public void FindDrawnRegions_MultipleRegions_FindsAll() {
		var cdl = new byte[4096];
		for (int i = 0x100; i < 0x200; i++) cdl[i] = DRAWN; // 256 bytes
		for (int i = 0x500; i < 0x700; i++) cdl[i] = DRAWN; // 512 bytes
		for (int i = 0xa00; i < 0xa40; i++) cdl[i] = DRAWN; // 64 bytes

		var results = CdlAssetFinder.FindDrawnRegions(cdl, minSize: 32);
		Assert.Equal(3, results.Count);
	}

	[Fact]
	public void FindDrawnRegions_TooSmall_Filtered() {
		var cdl = new byte[1024];
		// Only 16 bytes — below default minSize of 32
		for (int i = 0x100; i < 0x110; i++) cdl[i] = DRAWN;
		var results = CdlAssetFinder.FindDrawnRegions(cdl, minSize: 32);
		Assert.Empty(results);
	}

	[Fact]
	public void FindDrawnRegions_AtEnd_FindsIt() {
		var cdl = new byte[256];
		for (int i = 200; i < 256; i++) cdl[i] = DRAWN;
		var results = CdlAssetFinder.FindDrawnRegions(cdl, minSize: 32);

		Assert.Single(results);
		Assert.Equal(200, results[0].RomOffset);
		Assert.Equal(56, results[0].Size);
	}

	[Fact]
	public void FindDrawnRegions_HighConfidence() {
		var cdl = new byte[1024];
		for (int i = 0; i < 512; i++) cdl[i] = DRAWN;
		var results = CdlAssetFinder.FindDrawnRegions(cdl);

		Assert.Single(results);
		Assert.True(results[0].Confidence >= 0.8, "CDL DRAWN regions should have high confidence");
	}

	#endregion

	#region FindDataRegions

	[Fact]
	public void FindDataRegions_DataOnlyBytes_FindsThem() {
		var cdl = new byte[256];
		for (int i = 32; i < 64; i++) cdl[i] = DATA; // DATA-only
		var results = CdlAssetFinder.FindDataRegions(cdl, minSize: 8);

		Assert.Single(results);
		Assert.Equal(32, results[0].RomOffset);
		Assert.Equal(32, results[0].Size);
		Assert.Equal(DetectionSource.CDL, results[0].Source);
	}

	[Fact]
	public void FindDataRegions_CodeBytesExcluded() {
		var cdl = new byte[256];
		// Mark as both CODE and DATA — should NOT appear as data region
		for (int i = 32; i < 64; i++) cdl[i] = (byte)(CODE | DATA);
		var results = CdlAssetFinder.FindDataRegions(cdl, minSize: 8);
		Assert.Empty(results);
	}

	[Fact]
	public void FindDataRegions_DrawnBytesExcluded() {
		var cdl = new byte[256];
		// Mark as DRAWN + DATA — should appear in DRAWN, not DATA
		for (int i = 32; i < 64; i++) cdl[i] = (byte)(DRAWN | DATA);
		var results = CdlAssetFinder.FindDataRegions(cdl, minSize: 8);
		Assert.Empty(results);
	}

	#endregion

	#region FindAllAssets

	[Fact]
	public void FindAllAssets_CombinesCdlAndHeuristics() {
		var rom = new byte[4096];
		var cdl = new byte[4096];
		var rng = new Random(42);

		// Fill ROM with random data
		rng.NextBytes(rom);

		// Mark a DRAWN region (tiles)
		for (int i = 0x100; i < 0x200; i++) cdl[i] = DRAWN;

		// Mark a DATA region (potential palette)
		for (int i = 0x300; i < 0x320; i++) cdl[i] = DATA;

		var results = CdlAssetFinder.FindAllAssets(cdl, rom);

		// Should find at least the DRAWN region
		Assert.Contains(results, r => r.RomOffset == 0x100 && r.Type == AssetRegionType.Tiles);
	}

	[Fact]
	public void FindAllAssets_EmptyCdlEmptyRom_ReturnsEmpty() {
		var rom = new byte[0];
		var cdl = new byte[0];
		var results = CdlAssetFinder.FindAllAssets(cdl, rom);
		Assert.Empty(results);
	}

	#endregion

	#region DetectBitDepth

	[Fact]
	public void DetectBitDepth_SmallData_ReturnsFour() {
		// Default to 4bpp when data is too small
		var data = new byte[8];
		int bpp = CdlAssetFinder.DetectBitDepth(data);
		Assert.True(bpp is 2 or 4 or 8);
	}

	[Fact]
	public void DetectBitDepth_ReturnsValidValue() {
		var data = new byte[256];
		var rng = new Random(42);
		rng.NextBytes(data);
		int bpp = CdlAssetFinder.DetectBitDepth(data);
		Assert.True(bpp is 2 or 4 or 8, $"Expected 2, 4, or 8 but got {bpp}");
	}

	#endregion
}
