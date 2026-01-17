using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Peony.Core;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for DizLoader class - parsing DiztinGUIsh project files
/// </summary>
public class DizLoaderTests {
	[Fact]
	public void ParsesJsonFormat() {
		var json = """
		{
			"ProjectName": "TestProject",
			"RomMapMode": "LoRom",
			"RomSize": 1024,
			"Labels": {
				"32768": { "Name": "reset", "Comment": "Reset vector", "DataType": 1 },
				"32800": { "Name": "main_loop", "Comment": "Main loop", "DataType": 1 }
			},
			"DataTypes": [0, 1, 2, 1, 1, 3, 3, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var loader = DizLoader.Load(tempFile);

			Assert.Equal("TestProject", loader.ProjectName);
			Assert.Equal("LoRom", loader.MapMode);
			Assert.Equal(1024, loader.RomSize);

			// Check labels
			Assert.Equal(2, loader.Labels.Count);
			var resetLabel = loader.GetLabel(32768);
			Assert.NotNull(resetLabel);
			Assert.Equal("reset", resetLabel.Name);
			Assert.Equal("Reset vector", resetLabel.Comment);

			// Check data types
			Assert.True(loader.IsOpcode(1));  // DataType 1 = Opcode
			Assert.True(loader.IsCode(2));   // DataType 2 = Operand
			Assert.True(loader.IsData(5));   // DataType 3 = Data8
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ParsesGzipCompressedFormat() {
		var json = """
		{
			"ProjectName": "CompressedTest",
			"RomMapMode": "HiRom",
			"RomSize": 512,
			"Labels": {},
			"DataTypes": [0, 1, 1, 1]
		}
		""";

		var tempFile = Path.GetTempFileName() + ".diz";
		try {
			// Write gzip-compressed JSON
			var jsonBytes = Encoding.UTF8.GetBytes(json);
			using (var output = File.Create(tempFile))
			using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) {
				gzip.Write(jsonBytes, 0, jsonBytes.Length);
			}

			var loader = DizLoader.Load(tempFile);

			Assert.Equal("CompressedTest", loader.ProjectName);
			Assert.Equal("HiRom", loader.MapMode);
		} finally {
			if (File.Exists(tempFile)) File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetOpcodeOffsetsReturnsCorrectSet() {
		var json = """
		{
			"ProjectName": "OpcodeTest",
			"RomMapMode": "LoRom",
			"DataTypes": [0, 1, 2, 2, 0, 1, 2, 0, 1]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var loader = DizLoader.Load(tempFile);

			var opcodes = loader.GetOpcodeOffsets();

			Assert.Contains(1, opcodes);
			Assert.Contains(5, opcodes);
			Assert.Contains(8, opcodes);
			Assert.DoesNotContain(0, opcodes);  // Unreached
			Assert.DoesNotContain(2, opcodes);  // Operand
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetDataRegionsGroupsContiguousData() {
		var json = """
		{
			"ProjectName": "DataRegionTest",
			"RomMapMode": "LoRom",
			"DataTypes": [0, 3, 3, 3, 0, 0, 4, 4, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var loader = DizLoader.Load(tempFile);

			var allDataRegions = loader.GetDataRegions();

			// Should have two regions: Data8 (1-3) and Graphics (6-7)
			Assert.Equal(2, allDataRegions.Count);
			Assert.Equal((1, 3, DizLoader.DizDataType.Data8), allDataRegions[0]);
			Assert.Equal((6, 7, DizLoader.DizDataType.Graphics), allDataRegions[1]);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ExportToSymbolLoaderImportsLabels() {
		var json = """
		{
			"ProjectName": "SymbolExportTest",
			"Labels": {
				"8000": { "Name": "start", "Comment": "", "DataType": 1 },
				"8010": { "Name": "sub_init", "Comment": "", "DataType": 1 }
			}
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var dizLoader = DizLoader.Load(tempFile);

			var symbolLoader = new SymbolLoader();
			dizLoader.ExportToSymbolLoader(symbolLoader);

			Assert.Equal("start", symbolLoader.GetLabel(0x8000));
			Assert.Equal("sub_init", symbolLoader.GetLabel(0x8010));
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetCoverageStatsReturnsAccurateValues() {
		var json = """
		{
			"ProjectName": "CoverageTest",
			"RomSize": 100,
			"DataTypes": [0, 1, 2, 2, 1, 2, 0, 3, 3, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var loader = DizLoader.Load(tempFile);

			var (opcodes, operands, dataBytes, unreached, coverage) = loader.GetCoverageStats();

			Assert.Equal(2, opcodes);    // Offsets 1, 4
			Assert.Equal(3, operands);   // Offsets 2, 3, 5
			Assert.Equal(2, dataBytes);  // Offsets 7, 8
		} finally {
			File.Delete(tempFile);
		}
	}
}
