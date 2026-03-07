using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Peony.Core;
using Xunit;

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
				"32768": { "Name": "start", "Comment": "", "DataType": 1 },
				"32784": { "Name": "sub_init", "Comment": "", "DataType": 1 }
			}
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var dizLoader = DizLoader.Load(tempFile);

			var symbolLoader = new SymbolLoader();
			dizLoader.ExportToSymbolLoader(symbolLoader);

			Assert.Equal("start", symbolLoader.GetLabel(32768));  // 0x8000
			Assert.Equal("sub_init", symbolLoader.GetLabel(32784));  // 0x8010
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

			Assert.Equal(2, opcodes);	// Offsets 1, 4
			Assert.Equal(3, operands);   // Offsets 2, 3, 5
			Assert.Equal(2, dataBytes);  // Offsets 7, 8
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ParsesNativeXmlFormat() {
		// Simulated DiztinGUIsh native XML format
		var xml = """
		<?xml version="1.0" encoding="utf-8"?>
		<ProjectXmlSerializer-Root xmlns:sys="https://extendedxmlserializer.github.io/system" xmlns="clr-namespace:Diz.Core.serialization.xml_serializer;assembly=Diz.Core">
			<Project InternalRomGameName="TEST GAME" InternalCheckSum="12345">
			</Project>
			<Data RomMapMode="LoRom" RomSpeed="SlowRom">
				<Labels>
					<sys:Item Key="32768">
						<Value Name="reset_vector" Comment="Entry point" />
					</sys:Item>
					<sys:Item Key="33024">
						<Value Name="nmi_handler" Comment="NMI interrupt" />
					</sys:Item>
				</Labels>
				<RomBytes>
		version:201,compress_groupblocks
		+C
		.C
		.C
		+C
		.C
		U
		+D
		.D
				</RomBytes>
			</Data>
		</ProjectXmlSerializer-Root>
		""";

		var tempFile = Path.GetTempFileName() + ".diz";
		try {
			File.WriteAllText(tempFile, xml);
			var loader = DizLoader.Load(tempFile);

			Assert.Equal("TEST GAME", loader.ProjectName);
			Assert.Equal("LoRom", loader.MapMode);

			// Check labels
			Assert.Equal(2, loader.Labels.Count);
			var resetLabel = loader.GetLabel(32768);
			Assert.NotNull(resetLabel);
			Assert.Equal("reset_vector", resetLabel.Name);
			Assert.Equal("Entry point", resetLabel.Comment);

			var nmiLabel = loader.GetLabel(33024);
			Assert.NotNull(nmiLabel);
			Assert.Equal("nmi_handler", nmiLabel.Name);

			// Check data types parsed from RomBytes
			Assert.True(loader.IsOpcode(0));  // +C = Opcode
			Assert.True(loader.IsCode(1));    // .C = Operand (still code)
			Assert.Equal(DizLoader.DizDataType.Unreached, loader.GetDataType(5));  // U = Unreached
			Assert.True(loader.IsData(6));    // +D = Data
		} finally {
			if (File.Exists(tempFile)) File.Delete(tempFile);
		}
	}

	[Fact]
	public void ParsesGzipCompressedXmlFormat() {
		var xml = """
		<?xml version="1.0" encoding="utf-8"?>
		<ProjectXmlSerializer-Root xmlns:sys="https://extendedxmlserializer.github.io/system" xmlns="clr-namespace:Diz.Core.serialization.xml_serializer;assembly=Diz.Core">
			<Project InternalRomGameName="COMPRESSED XML" />
			<Data RomMapMode="HiRom">
				<Labels>
					<sys:Item Key="65534">
						<Value Name="reset" Comment="" />
					</sys:Item>
				</Labels>
				<RomBytes>
		version:201
		+C
		.C
				</RomBytes>
			</Data>
		</ProjectXmlSerializer-Root>
		""";

		var tempFile = Path.GetTempFileName() + ".diz";
		try {
			// Write gzip-compressed XML
			var xmlBytes = Encoding.UTF8.GetBytes(xml);
			using (var output = File.Create(tempFile))
			using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) {
				gzip.Write(xmlBytes, 0, xmlBytes.Length);
			}

			var loader = DizLoader.Load(tempFile);

			Assert.Equal("COMPRESSED XML", loader.ProjectName);
			Assert.Equal("HiRom", loader.MapMode);
			Assert.Single(loader.Labels);
			Assert.Equal("reset", loader.GetLabel(65534)?.Name);
		} finally {
			if (File.Exists(tempFile)) File.Delete(tempFile);
		}
	}

	[Fact]
	public void LoadsRealFfmqDizFile() {
		// Test with actual FFMQ DIZ file if available
		var dizPath = @"C:\Users\me\source\repos\GameInfo\DarkRepos\Wiki\SNES\Final_Fantasy_Mystic_Quest\Files\Diz\Final Fantasy - Mystic Quest (U) (V1.1).diz";

		if (!File.Exists(dizPath)) {
			// Skip test if file not available
			return;
		}

		var loader = DizLoader.Load(dizPath);

		// Verify basic properties
		Assert.Contains("MYSTIC QUEST", loader.ProjectName.ToUpperInvariant());
		Assert.Equal("LoRom", loader.MapMode);
		Assert.True(loader.RomSize > 0, "ROM size should be > 0");

		// Verify labels were loaded (FFMQ has known labels)
		Assert.True(loader.Labels.Count > 0, "Should have labels");

		// Check for known FFMQ labels
		var hasNativeReset = loader.Labels.Values.Any(l =>
			l.Name.Contains("Native", StringComparison.OrdinalIgnoreCase) ||
			l.Name.Contains("Reset", StringComparison.OrdinalIgnoreCase) ||
			l.Name.Contains("Emulation", StringComparison.OrdinalIgnoreCase));
		Assert.True(hasNativeReset, "Should have vector labels");

		// Verify data types were parsed
		Assert.True(loader.DataTypes.Count > 0, "Should have data type info");

		// Check coverage stats
		var (opcodes, operands, dataBytes, unreached, coverage) = loader.GetCoverageStats();
		Assert.True(opcodes > 0, "Should have opcodes");
		Assert.True(coverage > 0, "Should have some coverage");
	}

	#region DIZ → Pansy Conversion Tests

	[Fact]
	public void ConvertToPansyBytes_EmptyDiz_ProducesValidPansy() {
		var json = """
		{
			"ProjectName": "Empty",
			"RomMapMode": "LoRom",
			"RomSize": 0
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();

		Assert.NotNull(pansyBytes);
		Assert.True(pansyBytes.Length >= 32, "Pansy file should have at least a header");

		var pansyLoader = new Pansy.Core.PansyLoader(pansyBytes);
		Assert.Equal("Empty", pansyLoader.ProjectName);
	}

	[Fact]
	public void ConvertToPansyBytes_OpcodesMappedToCodeAndOpcode() {
		var json = """
		{
			"ProjectName": "OpcodeTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [0, 1, 2, 2, 0, 1, 2, 0]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Opcodes (offsets 1, 5) should be CODE + OPCODE
		Assert.True(pansy.IsCode(1));
		Assert.True(pansy.IsCode(5));

		// Operands (offsets 2, 3, 6) should be CODE only
		Assert.True(pansy.IsCode(2));
		Assert.True(pansy.IsCode(3));
		Assert.True(pansy.IsCode(6));

		// Unreached (offsets 0, 4, 7) should be neither code nor data
		Assert.False(pansy.IsCode(0));
		Assert.False(pansy.IsCode(4));
		Assert.False(pansy.IsCode(7));
	}

	[Fact]
	public void ConvertToPansyBytes_Data8MappedToData() {
		var json = """
		{
			"ProjectName": "DataTest",
			"RomMapMode": "LoRom",
			"RomSize": 4,
			"DataTypes": [3, 3, 0, 3]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.True(pansy.IsData(0));
		Assert.True(pansy.IsData(1));
		Assert.False(pansy.IsData(2));
		Assert.True(pansy.IsData(3));
	}

	[Fact]
	public void ConvertToPansyBytes_GraphicsMappedToDataDrawn() {
		var json = """
		{
			"ProjectName": "GfxTest",
			"RomMapMode": "LoRom",
			"RomSize": 4,
			"DataTypes": [4, 4, 0, 0]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Graphics → DATA + DRAWN
		Assert.True(pansy.IsData(0));
		Assert.True(pansy.IsData(1));

		// Verify the drawn flag is set by checking the raw CDL bytes
		var cdlBytes = pansy.CodeDataMapBytes;
		Assert.NotNull(cdlBytes);
		Assert.True((cdlBytes[0] & 0x20) != 0, "DRAWN flag should be set for graphics");
		Assert.True((cdlBytes[1] & 0x20) != 0, "DRAWN flag should be set for graphics");
	}

	[Fact]
	public void ConvertToPansyBytes_PointersMappedToDataIndirect() {
		var json = """
		{
			"ProjectName": "PtrTest",
			"RomMapMode": "LoRom",
			"RomSize": 6,
			"DataTypes": [8, 8, 10, 10, 12, 12]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// All pointer types → DATA + INDIRECT
		for (int i = 0; i < 6; i++) {
			Assert.True(pansy.IsData(i), $"Offset {i} should be data");
		}

		var cdlBytes = pansy.CodeDataMapBytes;
		Assert.NotNull(cdlBytes);
		for (int i = 0; i < 6; i++) {
			Assert.True((cdlBytes[i] & 0x80) != 0, $"INDIRECT flag should be set at offset {i}");
		}
	}

	[Fact]
	public void ConvertToPansyBytes_TextMappedToDataRead() {
		var json = """
		{
			"ProjectName": "TextTest",
			"RomMapMode": "LoRom",
			"RomSize": 4,
			"DataTypes": [13, 13, 0, 0]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.True(pansy.IsData(0));
		Assert.True(pansy.IsData(1));

		var cdlBytes = pansy.CodeDataMapBytes;
		Assert.NotNull(cdlBytes);
		Assert.True((cdlBytes[0] & 0x40) != 0, "READ flag should be set for text");
	}

	[Fact]
	public void ConvertToPansyBytes_MusicMappedToData() {
		var json = """
		{
			"ProjectName": "MusicTest",
			"RomMapMode": "LoRom",
			"RomSize": 2,
			"DataTypes": [5, 5]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.True(pansy.IsData(0));
		Assert.True(pansy.IsData(1));
	}

	[Fact]
	public void ConvertToPansyBytes_Data16Data24Data32MappedToData() {
		var json = """
		{
			"ProjectName": "DataSizeTest",
			"RomMapMode": "LoRom",
			"RomSize": 6,
			"DataTypes": [7, 7, 9, 9, 11, 11]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		for (int i = 0; i < 6; i++) {
			Assert.True(pansy.IsData(i), $"Offset {i} should be data");
		}
	}

	[Fact]
	public void ConvertToPansyBytes_LabelsConvertedToSymbols() {
		var json = """
		{
			"ProjectName": "LabelTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"0": { "Name": "reset", "Comment": "", "DataType": 1 },
				"128": { "Name": "main_loop", "Comment": "Entry point", "DataType": 1 }
			},
			"DataTypes": [1, 2, 2]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Symbols should be present
		var symbols = pansy.AllSymbolEntries;
		Assert.True(symbols.ContainsKey(0));
		Assert.Contains(symbols[0], s => s.Name == "reset");
		Assert.True(symbols.ContainsKey(128));
		Assert.Contains(symbols[128], s => s.Name == "main_loop");
	}

	[Fact]
	public void ConvertToPansyBytes_CommentsConvertedToComments() {
		var json = """
		{
			"ProjectName": "CommentTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"100": { "Name": "handler", "Comment": "Interrupt handler", "DataType": 1 }
			}
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		var comments = pansy.AllCommentEntries;
		Assert.True(comments.ContainsKey(100));
		Assert.Contains(comments[100], c => c.Text == "Interrupt handler");
	}

	[Fact]
	public void ConvertToPansyBytes_OpcodeLabelsMarkedAsSubroutines() {
		var json = """
		{
			"ProjectName": "SubTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"10": { "Name": "sub_init", "Comment": "", "DataType": 1 }
			},
			"DataTypes": [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.Contains(10, pansy.SubEntryPoints);
	}

	[Fact]
	public void ConvertToPansyBytes_AllOpcodesMarkedAsJumpTargets() {
		var json = """
		{
			"ProjectName": "JmpTargetTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [0, 1, 2, 2, 0, 1, 2, 0]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.Contains(1, pansy.JumpTargets);
		Assert.Contains(5, pansy.JumpTargets);
		Assert.DoesNotContain(0, pansy.JumpTargets);
		Assert.DoesNotContain(2, pansy.JumpTargets);
	}

	[Fact]
	public void ConvertToPansyBytes_ProjectNamePreserved() {
		var json = """
		{
			"ProjectName": "My Great ROM",
			"RomMapMode": "HiRom",
			"RomSize": 512
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.Equal("My Great ROM", pansy.ProjectName);
	}

	[Fact]
	public void ConvertToPansyBytes_RomSizePreserved() {
		var json = """
		{
			"ProjectName": "SizeTest",
			"RomMapMode": "LoRom",
			"RomSize": 2048
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		Assert.Equal(2048u, pansy.RomSize);
	}

	[Fact]
	public void ConvertToPansyBytes_MixedCodeAndData() {
		// Mixed: opcode, operand, operand, data8, data8, graphics, pointer16, text
		var json = """
		{
			"ProjectName": "MixedTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [1, 2, 2, 3, 3, 4, 8, 13]
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Code region (0-2)
		Assert.True(pansy.IsCode(0));
		Assert.True(pansy.IsCode(1));
		Assert.True(pansy.IsCode(2));

		// Data region (3-7)
		Assert.True(pansy.IsData(3));
		Assert.True(pansy.IsData(4));
		Assert.True(pansy.IsData(5));
		Assert.True(pansy.IsData(6));
		Assert.True(pansy.IsData(7));

		// Verify special flags
		var cdlBytes = pansy.CodeDataMapBytes;
		Assert.NotNull(cdlBytes);
		Assert.True((cdlBytes[5] & 0x20) != 0, "Graphics should have DRAWN flag");
		Assert.True((cdlBytes[6] & 0x80) != 0, "Pointer16 should have INDIRECT flag");
		Assert.True((cdlBytes[7] & 0x40) != 0, "Text should have READ flag");
	}

	[Fact]
	public void ConvertToPansyBytes_EmptyLabelsSkipped() {
		var json = """
		{
			"ProjectName": "EmptyLabelTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"0": { "Name": "", "Comment": "", "DataType": 0 },
				"100": { "Name": "valid_label", "Comment": "", "DataType": 1 }
			}
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		var symbols = pansy.AllSymbolEntries;
		Assert.False(symbols.ContainsKey(0), "Empty label should not be in symbols");
		Assert.True(symbols.ContainsKey(100));
	}

	[Fact]
	public void ConvertToPansyBytes_EmptyCommentsSkipped() {
		var json = """
		{
			"ProjectName": "EmptyCommentTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"0": { "Name": "label_no_comment", "Comment": "", "DataType": 0 },
				"100": { "Name": "label_with_comment", "Comment": "Has comment", "DataType": 0 }
			}
		}
		""";

		var loader = LoadFromJson(json);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		var comments = pansy.AllCommentEntries;
		Assert.False(comments.ContainsKey(0), "Empty comment should not be in comments");
		Assert.True(comments.ContainsKey(100));
	}

	[Fact]
	public void ConvertToPansyBytes_LargeDataTypesArray() {
		// Simulate larger ROM with many entries
		var dataTypes = new int[1000];
		for (int i = 0; i < 300; i++) dataTypes[i] = 1; // Opcode
		for (int i = 300; i < 600; i++) dataTypes[i] = 2; // Operand
		for (int i = 600; i < 800; i++) dataTypes[i] = 3; // Data8
		// 800-999 remain Unreached

		var jsonDoc = new {
			ProjectName = "LargeTest",
			RomMapMode = "LoRom",
			RomSize = 1000,
			DataTypes = dataTypes
		};

		var json = JsonSerializer.Serialize(jsonDoc);
		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var loader = DizLoader.Load(tempFile);
			var pansyBytes = loader.ConvertToPansyBytes();
			var pansy = new Pansy.Core.PansyLoader(pansyBytes);

			// Verify code and data classification
			Assert.True(pansy.IsCode(0));
			Assert.True(pansy.IsCode(299));
			Assert.True(pansy.IsCode(300));  // Operand is still code
			Assert.True(pansy.IsCode(599));
			Assert.True(pansy.IsData(600));
			Assert.True(pansy.IsData(799));
			Assert.False(pansy.IsCode(800)); // Unreached
			Assert.False(pansy.IsData(800)); // Unreached
		} finally {
			File.Delete(tempFile);
		}
	}

	#endregion

	#region SymbolLoader DIZ-via-Pansy Integration Tests

	[Fact]
	public void SymbolLoader_LoadDiz_UseCombinedPansyPath() {
		var json = """
		{
			"ProjectName": "IntegrationTest",
			"RomMapMode": "LoRom",
			"RomSize": 16,
			"Labels": {
				"0": { "Name": "start", "Comment": "Entry", "DataType": 1 }
			},
			"DataTypes": [1, 2, 2, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			// Should have Pansy data loaded
			Assert.NotNull(symbolLoader.PansyData);
			Assert.True(symbolLoader.PansyData.HasCodeDataMap);

			// IsCode/IsData should work through Pansy path
			Assert.True(symbolLoader.IsCode(0));   // Opcode
			Assert.True(symbolLoader.IsCode(1));   // Operand
			Assert.True(symbolLoader.IsData(3));   // Data8
			Assert.True(symbolLoader.IsData(4));   // Data8

			// Labels should still be imported
			Assert.Equal("start", symbolLoader.GetLabel(0));
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_PansyJumpTargetsPopulated() {
		var json = """
		{
			"ProjectName": "JumpTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [0, 1, 2, 2, 0, 1, 2, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			// Opcodes should appear as jump targets via Pansy
			Assert.Contains(1, symbolLoader.PansyJumpTargets);
			Assert.Contains(5, symbolLoader.PansyJumpTargets);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_PansySubEntryPointsPopulated() {
		var json = """
		{
			"ProjectName": "SubEntryTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"10": { "Name": "sub_handler", "Comment": "", "DataType": 1 }
			},
			"DataTypes": [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			Assert.Contains(10, symbolLoader.PansySubEntryPoints);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_TypedSymbolsPopulated() {
		var json = """
		{
			"ProjectName": "TypedSymbolTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"100": { "Name": "my_func", "Comment": "Does stuff", "DataType": 1 }
			}
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			Assert.True(symbolLoader.TypedSymbols.ContainsKey(100));
			Assert.Equal("my_func", symbolLoader.TypedSymbols[100].Name);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_TypedCommentsPopulated() {
		var json = """
		{
			"ProjectName": "TypedCommentTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"200": { "Name": "handler", "Comment": "NMI handler entry", "DataType": 1 }
			}
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			Assert.True(symbolLoader.TypedComments.ContainsKey(200));
			Assert.Equal("NMI handler entry", symbolLoader.TypedComments[200].Text);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_NullReturnsForUnreachedBytes() {
		var json = """
		{
			"ProjectName": "UnreachedTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [1, 2, 0, 0, 3, 0, 0, 0]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			// Unreached offsets should not be classified as code or data
			Assert.False(symbolLoader.IsCode(2) == true);
			Assert.False(symbolLoader.IsData(2) == true);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_DizDataStillAccessible() {
		var json = """
		{
			"ProjectName": "BackwardCompatTest",
			"RomMapMode": "LoRom",
			"RomSize": 256,
			"Labels": {
				"50": { "Name": "test_label", "Comment": "", "DataType": 1 }
			},
			"DataTypes": [1, 2, 3]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			// DizData should still be accessible for backward compatibility
			Assert.NotNull(symbolLoader.DizData);
			Assert.Equal("BackwardCompatTest", symbolLoader.DizData.ProjectName);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void SymbolLoader_LoadDiz_AlsoLoadsPansyData() {
		var json = """
		{
			"ProjectName": "BothPathTest",
			"RomMapMode": "LoRom",
			"RomSize": 8,
			"DataTypes": [1, 2, 2, 3]
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			// Both DizData and PansyData should be populated
			Assert.NotNull(symbolLoader.DizData);
			Assert.NotNull(symbolLoader.PansyData);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ConvertToPansyBytes_RoundtripPreservesCodeData() {
		var json = """
		{
			"ProjectName": "RoundtripTest",
			"RomMapMode": "LoRom",
			"RomSize": 10,
			"Labels": {
				"0": { "Name": "start", "Comment": "Reset vector", "DataType": 1 },
				"5": { "Name": "data_table", "Comment": "Lookup table", "DataType": 3 }
			},
			"DataTypes": [1, 2, 2, 1, 2, 3, 3, 3, 4, 8]
		}
		""";

		var loader = LoadFromJson(json);

		// Convert to Pansy and reload
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Verify code classification matches original
		Assert.True(pansy.IsCode(0));  // Opcode
		Assert.True(pansy.IsCode(1));  // Operand
		Assert.True(pansy.IsCode(2));  // Operand
		Assert.True(pansy.IsCode(3));  // Opcode
		Assert.True(pansy.IsCode(4));  // Operand

		// Verify data classification matches original
		Assert.True(pansy.IsData(5));  // Data8
		Assert.True(pansy.IsData(6));  // Data8
		Assert.True(pansy.IsData(7));  // Data8
		Assert.True(pansy.IsData(8));  // Graphics
		Assert.True(pansy.IsData(9));  // Pointer16

		// Verify symbols roundtrip
		Assert.True(pansy.AllSymbolEntries.ContainsKey(0));
		Assert.True(pansy.AllSymbolEntries.ContainsKey(5));

		// Verify comments roundtrip
		Assert.True(pansy.AllCommentEntries.ContainsKey(0));
		Assert.Contains(pansy.AllCommentEntries[0], c => c.Text == "Reset vector");
		Assert.True(pansy.AllCommentEntries.ContainsKey(5));
		Assert.Contains(pansy.AllCommentEntries[5], c => c.Text == "Lookup table");
	}

	[Fact]
	public void ConvertToPansyBytes_XmlFormatConvertsCorrectly() {
		var xml = """
		<?xml version="1.0" encoding="utf-8"?>
		<ProjectXmlSerializer-Root xmlns:sys="https://extendedxmlserializer.github.io/system" xmlns="clr-namespace:Diz.Core.serialization.xml_serializer;assembly=Diz.Core">
			<Project InternalRomGameName="XML TO PANSY" />
			<Data RomMapMode="HiRom">
				<Labels>
					<sys:Item Key="100">
						<Value Name="xml_label" Comment="From XML" />
					</sys:Item>
				</Labels>
				<RomBytes>
		version:201,compress_groupblocks
		+C
		.C
		.C
		+D
		.D
		U
				</RomBytes>
			</Data>
		</ProjectXmlSerializer-Root>
		""";

		var tempFile = Path.GetTempFileName() + ".diz";
		try {
			File.WriteAllText(tempFile, xml);
			var loader = DizLoader.Load(tempFile);
			var pansyBytes = loader.ConvertToPansyBytes();
			var pansy = new Pansy.Core.PansyLoader(pansyBytes);

			// Code from XML
			Assert.True(pansy.IsCode(0));  // +C = Opcode
			Assert.True(pansy.IsCode(1));  // .C = Operand
			Assert.True(pansy.IsCode(2));  // .C = Operand

			// Data from XML
			Assert.True(pansy.IsData(3));  // +D = Data
			Assert.True(pansy.IsData(4));  // .D = Data

			// Unreached
			Assert.False(pansy.IsCode(5));
			Assert.False(pansy.IsData(5));

			// Labels from XML
			Assert.True(pansy.AllSymbolEntries.ContainsKey(100));
		} finally {
			if (File.Exists(tempFile)) File.Delete(tempFile);
		}
	}

	[Fact]
	public void ConvertToPansyBytes_RealFfmqDizConverts() {
		var dizPath = @"C:\Users\me\source\repos\GameInfo\DarkRepos\Wiki\SNES\Final_Fantasy_Mystic_Quest\Files\Diz\Final Fantasy - Mystic Quest (U) (V1.1).diz";

		if (!File.Exists(dizPath)) return;

		var loader = DizLoader.Load(dizPath);
		var pansyBytes = loader.ConvertToPansyBytes();
		var pansy = new Pansy.Core.PansyLoader(pansyBytes);

		// Verify valid Pansy file
		Assert.True(pansyBytes.Length > 32);
		Assert.True(pansy.HasCodeDataMap);

		// Verify symbols were converted
		Assert.True(pansy.AllSymbolEntries.Count > 0);

		// Verify code/data classification preserved
		var (opcodes, _, dataBytes, _, _) = loader.GetCoverageStats();
		// Pansy should have both code and data flags set
		Assert.True(pansy.JumpTargets.Count > 0, "Should have jump targets from opcode offsets");
	}

	/// <summary>
	/// Helper to load a DizLoader from a JSON string without temp file management.
	/// </summary>
	private static DizLoader LoadFromJson(string json) {
		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);
			return DizLoader.Load(tempFile);
		} finally {
			File.Delete(tempFile);
		}
	}

	#endregion
}
