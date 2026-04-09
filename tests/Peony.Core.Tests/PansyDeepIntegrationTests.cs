using Pansy.Core;
using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for deep Pansy integration: typed symbols/comments, jump targets,
/// sub-entry-points, bookmarks, data types, batch API export, and roundtrip.
/// </summary>
public class PansyDeepIntegrationTests {
	/// <summary>
	/// Creates a Pansy file with symbols, comments, code/data map, bookmarks, and data types
	/// using PansyWriter for accurate binary generation.
	/// </summary>
	private static byte[] CreateRichPansyFile() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
			ProjectName = "TestRom",
			ProjectVersion = "1.0",
		};

		// Add typed symbols
		writer.AddSymbol(0x8000, "reset", SymbolType.InterruptVector);
		writer.AddSymbol(0x8100, "main_loop", SymbolType.Function);
		writer.AddSymbol(0x8200, "data_table", SymbolType.Constant);
		writer.AddSymbol(0x8300, ".local_label", SymbolType.Local);

		// Add typed comments
		writer.AddComment(0x8000, "Entry point", CommentType.Block);
		writer.AddComment(0x8100, "Main game loop", CommentType.Inline);
		writer.AddComment(0x8200, "TODO: verify table contents", CommentType.Todo);

		// Add code/data map flags (uint offsets)
		writer.MarkAsCode(0u);
		writer.MarkAsOpcode(0u);
		writer.MarkAsCode(1u);
		writer.MarkAsCode(2u);
		writer.MarkAsJumpTarget(0x10u);
		writer.MarkAsSubroutine(0x100u);
		writer.MarkAsData(0x200u);

		// Add cross-references
		writer.AddCrossReference(new CrossReference(0x8010, 0x8100, Pansy.Core.CrossRefType.Jsr));
		writer.AddCrossReference(new CrossReference(0x8020, 0x8200, Pansy.Core.CrossRefType.Read));

		// Add bookmarks
		writer.AddBookmark(new Bookmark(0x8000, "Start", 1));
		writer.AddBookmark(new Bookmark(0x8100, "MainLoop", 2));

		// Add data types
		writer.AddDataType(new DataTypeEntry(0x8200, 16, 2, 8, DataElementType.Word, "ptr_table"));
		writer.AddDataType(new DataTypeEntry(0x8300, 32, 1, 32, DataElementType.Byte, "sprite_data"));

		// Add memory regions
		writer.AddMemoryRegion(new Pansy.Core.MemoryRegion(0x8000, 0x81ff, (byte)MemoryRegionType.ROM, 0, "code_region"));
		writer.AddMemoryRegion(new Pansy.Core.MemoryRegion(0x8200, 0x83ff, (byte)MemoryRegionType.ROM, 0, "data_region"));

		return writer.Generate();
	}

	#region SymbolLoader Enhanced Import Tests

	[Fact]
	public void LoadPansy_ImportsTypedSymbols() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		// Plain labels should be imported
		Assert.True(loader.Labels.ContainsKey(0x8000));
		Assert.Equal("reset", loader.Labels[0x8000]);
		Assert.Equal("main_loop", loader.Labels[0x8100]);

		// Typed symbols should be preserved
		Assert.True(loader.TypedSymbols.ContainsKey(0x8000));
		Assert.Equal(SymbolType.InterruptVector, loader.TypedSymbols[0x8000].Type);
		Assert.Equal(SymbolType.Function, loader.TypedSymbols[0x8100].Type);
		Assert.Equal(SymbolType.Constant, loader.TypedSymbols[0x8200].Type);
		Assert.Equal(SymbolType.Local, loader.TypedSymbols[0x8300].Type);
	}

	[Fact]
	public void LoadPansy_ImportsTypedComments() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		// Plain comments should be imported
		Assert.True(loader.Comments.ContainsKey(0x8000));
		Assert.Equal("Entry point", loader.Comments[0x8000]);

		// Typed comments should be preserved
		Assert.True(loader.TypedComments.ContainsKey(0x8000));
		Assert.Equal(Pansy.Core.CommentType.Block, loader.TypedComments[0x8000].Type);
		Assert.Equal(Pansy.Core.CommentType.Inline, loader.TypedComments[0x8100].Type);
		Assert.Equal(Pansy.Core.CommentType.Todo, loader.TypedComments[0x8200].Type);
	}

	[Fact]
	public void LoadPansy_ImportsJumpTargets() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.True(loader.PansyJumpTargets.Contains(0x0010));
		Assert.False(loader.PansyJumpTargets.Contains(0x0000));
	}

	[Fact]
	public void LoadPansy_ImportsSubEntryPoints() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.True(loader.PansySubEntryPoints.Contains(0x0100));
		Assert.False(loader.PansySubEntryPoints.Contains(0x0000));
	}

	[Fact]
	public void LoadPansy_ImportsBookmarks() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal(2, loader.Bookmarks.Count);
		Assert.Equal("Start", loader.Bookmarks[0].Name);
		Assert.Equal(0x8000u, loader.Bookmarks[0].Address);
		Assert.Equal("MainLoop", loader.Bookmarks[1].Name);
	}

	[Fact]
	public void LoadPansy_ImportsDataTypes() {
		var pansyData = CreateRichPansyFile();
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal(2, loader.PansyDataTypes.Count);

		// Data types should also be converted to DataDefinitions
		Assert.True(loader.DataDefinitions.ContainsKey(0x8200));
		Assert.Equal("word", loader.DataDefinitions[0x8200].Type);
		Assert.Equal(8, loader.DataDefinitions[0x8200].Count);

		Assert.True(loader.DataDefinitions.ContainsKey(0x8300));
		Assert.Equal("byte", loader.DataDefinitions[0x8300].Type);
		Assert.Equal(32, loader.DataDefinitions[0x8300].Count);
	}

	[Fact]
	public void LoadPansy_BookmarksGenerateLabels() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
		};
		writer.AddBookmark(new Bookmark(0x9000, "bookmark_here", 0));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		// Bookmark address should generate a label
		Assert.True(loader.Labels.ContainsKey(0x9000));
		Assert.Equal("bookmark_here", loader.Labels[0x9000]);
	}

	[Fact]
	public void LoadPansy_ExistingLabelNotOverwrittenByBookmark() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
		};
		writer.AddSymbol(0x9000, "my_function", SymbolType.Function);
		writer.AddBookmark(new Bookmark(0x9000, "bookmark_name", 0));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		// Symbol name should take precedence over bookmark name
		Assert.Equal("my_function", loader.Labels[0x9000]);
	}

	#endregion

	#region SymbolExporter Batch API Tests

	[Fact]
	public void ExportPansy_PreservesTypedSymbols() {
		var result = CreateTestDisassemblyResult();
		result.Labels[0x8000] = "reset";
		result.Labels[0x8100] = "main_loop";
		result.TypedSymbols[0x8000] = new SymbolEntry("reset", SymbolType.InterruptVector);
		result.TypedSymbols[0x8100] = new SymbolEntry("main_loop", SymbolType.Function);

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var data = File.ReadAllBytes(outputPath);
			var loader = new PansyLoader(data);

			Assert.Equal(SymbolType.InterruptVector, loader.GetSymbolType(0x8000));
			Assert.Equal(SymbolType.Function, loader.GetSymbolType(0x8100));
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void ExportPansy_PreservesTypedComments() {
		var result = CreateTestDisassemblyResult();
		result.Comments[0x8000] = "Entry point";
		result.Comments[0x8100] = "TODO fix";
		result.TypedComments[0x8000] = new CommentEntry("Entry point", Pansy.Core.CommentType.Block);
		result.TypedComments[0x8100] = new CommentEntry("TODO fix", Pansy.Core.CommentType.Todo);

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var data = File.ReadAllBytes(outputPath);
			var loader = new PansyLoader(data);

			Assert.Equal(Pansy.Core.CommentType.Block, loader.GetCommentType(0x8000));
			Assert.Equal(Pansy.Core.CommentType.Todo, loader.GetCommentType(0x8100));
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void ExportPansy_ExportsBookmarks() {
		var result = CreateTestDisassemblyResult();
		result.Bookmarks.Add(new Bookmark(0x8000, "Start", 1));
		result.Bookmarks.Add(new Bookmark(0x8100, "Loop", 2));

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var data = File.ReadAllBytes(outputPath);
			var loader = new PansyLoader(data);

			Assert.Equal(2, loader.Bookmarks.Count);
			Assert.Equal("Start", loader.Bookmarks[0].Name);
			Assert.Equal(0x8000u, loader.Bookmarks[0].Address);
			Assert.Equal(1, loader.Bookmarks[0].Color);
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void ExportPansy_ExportsDataTypes() {
		var result = CreateTestDisassemblyResult();
		result.DataTypes.Add(new DataTypeEntry(0x8200, 16, 2, 8, DataElementType.Word, "ptr_table"));

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var data = File.ReadAllBytes(outputPath);
			var loader = new PansyLoader(data);

			Assert.Single(loader.DataTypes);
			Assert.Equal(0x8200u, loader.DataTypes[0].Address);
			Assert.Equal("ptr_table", loader.DataTypes[0].Name);
			Assert.Equal(DataElementType.Word, loader.DataTypes[0].Type);
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void ExportPansy_ExportsDiscoveredDataRegionsAsDataTypes() {
		var result = CreateTestDisassemblyResult();
		result.DataRegions[0x8300] = new DataDefinition("word", 4, "jump_table");

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var data = File.ReadAllBytes(outputPath);
			var loader = new PansyLoader(data);

			Assert.Single(loader.DataTypes);
			Assert.Equal(0x8300u, loader.DataTypes[0].Address);
			Assert.Equal("jump_table", loader.DataTypes[0].Name);
			Assert.Equal(DataElementType.Word, loader.DataTypes[0].Type);
			Assert.Equal(4, loader.DataTypes[0].ElementCount);
		} finally {
			File.Delete(outputPath);
		}
	}

	#endregion

	#region Full Roundtrip Tests

	[Fact]
	public void Roundtrip_TypedSymbolsPreserved() {
		// Create initial Pansy file with typed symbols
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
		};
		writer.AddSymbol(0x8000, "reset", SymbolType.InterruptVector);
		writer.AddSymbol(0x8100, "main_loop", SymbolType.Function);
		writer.AddSymbol(0x8200, "const_val", SymbolType.Constant);

		// Import into SymbolLoader
		var sldr = new SymbolLoader();
		sldr.LoadPansyData(writer.Generate());

		// Build a DisassemblyResult that carries the typed data
		var result = CreateTestDisassemblyResult();
		foreach (var kvp in sldr.Labels) result.Labels[kvp.Key] = kvp.Value;
		foreach (var kvp in sldr.Comments) result.Comments[kvp.Key] = kvp.Value;
		foreach (var kvp in sldr.TypedSymbols) result.TypedSymbols[kvp.Key] = kvp.Value;
		foreach (var kvp in sldr.TypedComments) result.TypedComments[kvp.Key] = kvp.Value;

		// Export back to Pansy
		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var loader = new PansyLoader(File.ReadAllBytes(outputPath));

			// Verify types survived the roundtrip
			Assert.Equal(SymbolType.InterruptVector, loader.GetSymbolType(0x8000));
			Assert.Equal(SymbolType.Function, loader.GetSymbolType(0x8100));
			Assert.Equal(SymbolType.Constant, loader.GetSymbolType(0x8200));
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void Roundtrip_BookmarksPreserved() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
		};
		writer.AddBookmark(new Bookmark(0x8000, "Start", 1));
		writer.AddBookmark(new Bookmark(0x8100, "Loop", 2));

		var sldr = new SymbolLoader();
		sldr.LoadPansyData(writer.Generate());

		var result = CreateTestDisassemblyResult();
		foreach (var kvp in sldr.Labels) result.Labels[kvp.Key] = kvp.Value;
		foreach (var bookmark in sldr.Bookmarks) result.Bookmarks.Add(bookmark);

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var loader = new PansyLoader(File.ReadAllBytes(outputPath));
			Assert.Equal(2, loader.Bookmarks.Count);
			Assert.Equal("Start", loader.Bookmarks[0].Name);
			Assert.Equal(0x8100u, loader.Bookmarks[1].Address);
		} finally {
			File.Delete(outputPath);
		}
	}

	[Fact]
	public void Roundtrip_DataTypesPreserved() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 0x8000,
		};
		writer.AddDataType(new DataTypeEntry(0x8200, 16, 2, 8, DataElementType.Word, "ptr_table"));
		writer.AddDataType(new DataTypeEntry(0x8300, 32, 1, 32, DataElementType.Byte, "sprite_data"));

		var sldr = new SymbolLoader();
		sldr.LoadPansyData(writer.Generate());

		var result = CreateTestDisassemblyResult();
		foreach (var kvp in sldr.Labels) result.Labels[kvp.Key] = kvp.Value;
		foreach (var dt in sldr.PansyDataTypes) result.DataTypes.Add(dt);

		var outputPath = Path.GetTempFileName();
		try {
			SymbolExporter.ExportPansy(result, outputPath);

			var loader = new PansyLoader(File.ReadAllBytes(outputPath));
			Assert.Equal(2, loader.DataTypes.Count);
			Assert.Equal("ptr_table", loader.DataTypes[0].Name);
			Assert.Equal(DataElementType.Word, loader.DataTypes[0].Type);
			Assert.Equal("sprite_data", loader.DataTypes[1].Name);
			Assert.Equal(DataElementType.Byte, loader.DataTypes[1].Type);
		} finally {
			File.Delete(outputPath);
		}
	}

	#endregion

	#region StaticAnalyzer Pansy Code Map Tests

	[Fact]
	public void StaticAnalyzer_UsesPansyCodeDataMap() {
		// Create Pansy file with code/data map
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 16,
		};
		writer.MarkAsCode(0);
		writer.MarkAsCode(1);
		writer.MarkAsCode(2);
		writer.MarkAsData(8);
		writer.MarkAsData(9);
		writer.MarkAsJumpTarget(4);
		writer.MarkAsSubroutine(6);

		var sldr = new SymbolLoader();
		sldr.LoadPansyData(writer.Generate());

		// Create a mock platform analyzer that handles the small ROM
		var analyzer = new StaticAnalyzer(new TestPlatformAnalyzer(), sldr);
		var rom = new byte[16];
		var result = analyzer.Classify(rom);

		// Pansy code map should classify bytes
		Assert.Equal(ByteClassification.Code, result.Map[0]);
		Assert.Equal(ByteClassification.Code, result.Map[1]);
		Assert.Equal(ByteClassification.Data, result.Map[8]);
		Assert.Equal(ByteClassification.Code, result.Map[4]); // Jump target
		Assert.Equal(ByteClassification.Code, result.Map[6]); // Sub entry

		// Source should be PansyCodeMap
		Assert.Equal(ClassificationSource.PansyCodeMap, result.Sources[0]);
		Assert.Equal(ClassificationSource.PansyCodeMap, result.Sources[8]);
	}

	/// <summary>
	/// Verify CPU state entries are imported from Pansy and roundtrip through disassembly.
	/// </summary>
	[Fact]
	public void PansyCpuState_ImportAndRoundtrip() {
		// Create a Pansy file with CPU state entries
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_SNES,
			RomSize = 0x8000,
		};
		writer.AddCpuState(new CpuStateEntry(0x8000, 0x03, 0x80, 0x0000, CpuMode.Native65816));
		writer.AddCpuState(new CpuStateEntry(0x8100, 0x00, 0x7e, 0x1800, CpuMode.Emulation6502));
		var pansyData = writer.Generate();

		// Load into SymbolLoader
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		// Verify CPU states were imported
		Assert.Equal(2, loader.PansyCpuStates.Count);
		Assert.Equal(0x8000u, loader.PansyCpuStates[0].Address);
		Assert.Equal(0x03, loader.PansyCpuStates[0].Flags);
		Assert.Equal(CpuMode.Native65816, loader.PansyCpuStates[0].Mode);
		Assert.Equal(0x8100u, loader.PansyCpuStates[1].Address);
		Assert.Equal(CpuMode.Emulation6502, loader.PansyCpuStates[1].Mode);

		// Verify roundtrip through DisassemblyResult → SymbolExporter → PansyLoader
		var result = CreateTestDisassemblyResult();
		foreach (var cpu in loader.PansyCpuStates) {
			result.CpuStates.Add(cpu);
		}
		Assert.Equal(2, result.CpuStates.Count);

		// Export to Pansy and re-read
		var tempFile = Path.GetTempFileName();
		try {
			result.RomInfo = new RomInfo("SNES", 0x8000, null, new Dictionary<string, string>());
			SymbolExporter.ExportPansy(result, tempFile);

			var reloaded = new PansyLoader(File.ReadAllBytes(tempFile));
			Assert.Equal(2, reloaded.CpuStateEntries.Count);
			Assert.Equal(0x8000u, reloaded.CpuStateEntries[0].Address);
			Assert.Equal(0x03, reloaded.CpuStateEntries[0].Flags);
			Assert.Equal(0x80, reloaded.CpuStateEntries[0].DataBank);
			Assert.Equal(0x0000, reloaded.CpuStateEntries[0].DirectPage);
			Assert.Equal(CpuMode.Native65816, reloaded.CpuStateEntries[0].Mode);
			Assert.Equal(0x8100u, reloaded.CpuStateEntries[1].Address);
			Assert.Equal(CpuMode.Emulation6502, reloaded.CpuStateEntries[1].Mode);
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Verify GBA ARM/THUMB CPU state entries roundtrip correctly.
	/// </summary>
	[Fact]
	public void PansyCpuState_ArmThumbRoundtrip() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_GBA,
			RomSize = 0x8000,
		};
		writer.AddCpuState(new CpuStateEntry(0x08000000, 0x00, 0x00, 0x0000, CpuMode.ARM));
		writer.AddCpuState(new CpuStateEntry(0x08001000, 0x00, 0x00, 0x0000, CpuMode.THUMB));
		var pansyData = writer.Generate();

		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal(2, loader.PansyCpuStates.Count);
		Assert.Equal(CpuMode.ARM, loader.PansyCpuStates[0].Mode);
		Assert.Equal(CpuMode.THUMB, loader.PansyCpuStates[1].Mode);
	}

	#endregion

	#region Helpers

	private static DisassemblyResult CreateTestDisassemblyResult() {
		return new DisassemblyResult {
			RomInfo = new RomInfo("NES", 0x8000, null, new Dictionary<string, string> {
				["EntryPoint"] = "8000",
			}),
		};
	}

	/// <summary>
	/// Minimal platform analyzer for StaticAnalyzer tests.
	/// </summary>
	private sealed class TestPlatformAnalyzer : IPlatformAnalyzer {
		public string Platform => "NES";
		public int BankCount => 1;
		public int RomDataOffset => 0;
		public ICpuDecoder CpuDecoder { get; } = new TestCpuDecoder();

		public RomInfo Analyze(ReadOnlySpan<byte> rom) =>
			new("NES", rom.Length, null, new Dictionary<string, string> { ["EntryPoint"] = "0" });

		public int AddressToOffset(uint address, int romLength) => (int)address;
		public int AddressToOffset(uint address, int romLength, int bank) => (int)address;
		public uint? OffsetToAddress(int offset) => (uint)offset;
		public string? GetRegisterLabel(uint address) => null;
		public MemoryRegion GetMemoryRegion(uint address) => MemoryRegion.Unknown;
		public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) => [];
		public bool IsInSwitchableRegion(uint address) => false;
		public bool IsValidAddress(uint address) => true;
		public int GetTargetBank(uint target, int currentBank) => currentBank;
		public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) => null;
	}

	private sealed class TestCpuDecoder : ICpuDecoder {
		public string Architecture => "Test";

		public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) =>
			new("nop", "", [data[0]], AddressingMode.Implied);

		public bool IsControlFlow(DecodedInstruction instruction) => false;
		public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) => [];
	}

	#endregion
}
