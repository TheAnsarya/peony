using Peony.Core;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for CdlLoader class - parsing Code/Data Log files from emulators
/// </summary>
public class CdlLoaderTests {
	[Fact]
	public void DetectsEmptyAsUnreached() {
		// All zeros = unreached
		var cdlData = new byte[100];
		var loader = new CdlLoader(cdlData);

		Assert.Equal(CdlLoader.CdlFormat.FCEUX, loader.Format);
		Assert.Empty(loader.CodeOffsets);
		Assert.Empty(loader.DataOffsets);
	}

	[Fact]
	public void ParsesFceuxCodeFlags() {
		// FCEUX format: 0x01 = code executed
		var cdlData = new byte[16];
		cdlData[0] = 0x01;  // Code
		cdlData[5] = 0x01;  // Code
		cdlData[10] = 0x02; // Data

		var loader = new CdlLoader(cdlData);

		Assert.True(loader.IsCode(0));
		Assert.True(loader.IsCode(5));
		Assert.False(loader.IsCode(10));
		Assert.True(loader.IsData(10));
		Assert.True(loader.IsUnreached(3));
	}

	[Fact]
	public void ParsesMesenFormat() {
		// Mesen format: "CDL\x01" header + data
		var cdlData = new byte[20];
		cdlData[0] = (byte)'C';
		cdlData[1] = (byte)'D';
		cdlData[2] = (byte)'L';
		cdlData[3] = 0x01;
		cdlData[4] = 0x01;  // Code at offset 0
		cdlData[5] = 0x02;  // Data at offset 1
		cdlData[6] = 0x04;  // Jump target at offset 2
		cdlData[7] = 0x08;  // Sub entry point at offset 3

		var loader = new CdlLoader(cdlData);

		Assert.Equal(CdlLoader.CdlFormat.Mesen, loader.Format);
		Assert.True(loader.IsCode(0));
		Assert.True(loader.IsData(1));
		Assert.True(loader.IsJumpTarget(2));
		Assert.True(loader.IsSubEntryPoint(3));
	}

	[Fact]
	public void GetCodeRegionsReturnsContiguousRanges() {
		var cdlData = new byte[20];
		// Mark offsets 0-4 as code
		for (int i = 0; i <= 4; i++) cdlData[i] = 0x01;
		// Mark offsets 10-12 as code
		for (int i = 10; i <= 12; i++) cdlData[i] = 0x01;

		var loader = new CdlLoader(cdlData);
		var regions = loader.GetCodeRegions();

		Assert.Equal(2, regions.Count);
		Assert.Equal((0, 4), regions[0]);
		Assert.Equal((10, 12), regions[1]);
	}

	[Fact]
	public void GetCoverageStatsCalculatesCorrectly() {
		var cdlData = new byte[100];
		// 10 code bytes
		for (int i = 0; i < 10; i++) cdlData[i] = 0x01;
		// 5 data bytes
		for (int i = 20; i < 25; i++) cdlData[i] = 0x02;

		var loader = new CdlLoader(cdlData);
		var (codeBytes, dataBytes, totalSize, coverage) = loader.GetCoverageStats();

		Assert.Equal(10, codeBytes);
		Assert.Equal(5, dataBytes);
		Assert.Equal(100, totalSize);
		Assert.Equal(15.0, coverage);
	}

	[Fact]
	public void SubEntryPointsAreIdentified() {
		// FCEUX: 0x10 = sub entry point
		var cdlData = new byte[20];
		cdlData[0] = 0x10;  // Sub entry point
		cdlData[5] = 0x11;  // Code + sub entry point
		cdlData[10] = 0x01; // Just code

		var loader = new CdlLoader(cdlData);

		Assert.Contains(0, loader.SubEntryPoints);
		Assert.Contains(5, loader.SubEntryPoints);
		Assert.DoesNotContain(10, loader.SubEntryPoints);
	}
}
