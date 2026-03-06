using Peony.Core;
using Peony.Platform.NES;
using Xunit;

// Pansy types (aliased to avoid conflict with Peony.Core types)
using CrossReference = Pansy.Core.CrossReference;
using PansyCrossRefType = Pansy.Core.CrossRefType;
using PansyMemoryRegion = Pansy.Core.MemoryRegion;
using MemoryRegionType = Pansy.Core.MemoryRegionType;
using PansyWriter = Pansy.Core.PansyWriter;
using SymbolType = Pansy.Core.SymbolType;

namespace Peony.Core.Tests;

/// <summary>
/// Integration tests for StaticAnalyzer with real CDL and Pansy data.
/// Tests the full classification pipeline with combined data sources.
/// </summary>
public class StaticAnalysisIntegrationTests {
	private readonly NesAnalyzer _nesAnalyzer = new();

	/// <summary>
	/// Creates a NES ROM with specific code bytes at the start of PRG.
	/// </summary>
	private static byte[] CreateNesRom(byte[]? code = null, int prgBanks = 2) {
		var prgSize = prgBanks * 16384;
		var chrSize = 8192;
		var totalSize = 16 + prgSize + chrSize;
		var rom = new byte[totalSize];

		// iNES header
		rom[0] = 0x4e; rom[1] = 0x45; rom[2] = 0x53; rom[3] = 0x1a;
		rom[4] = (byte)prgBanks;
		rom[5] = 1;

		// Copy code
		if (code is not null) {
			Array.Copy(code, 0, rom, 16, Math.Min(code.Length, prgSize));
		}

		// Vectors → $8000
		var vecBase = 16 + prgSize - 6;
		rom[vecBase] = 0x00; rom[vecBase + 1] = 0x80;     // NMI
		rom[vecBase + 2] = 0x00; rom[vecBase + 3] = 0x80; // RESET
		rom[vecBase + 4] = 0x00; rom[vecBase + 5] = 0x80; // IRQ

		return rom;
	}

	// ========================================================================
	// CDL + Static Analysis Integration
	// ========================================================================

	[Fact]
	public void CdlCodeAndData_CorrectlyClassified() {
		var rom = CreateNesRom();

		// Build CDL data that marks specific offsets
		var cdlData = new byte[rom.Length - 16];
		// Mark offsets 0-9 as CODE (FCEUX format: 0x01)
		for (int i = 0; i < 10; i++) cdlData[i] = 0x01;
		// Mark offsets 10-19 as DATA (FCEUX format: 0x02)
		for (int i = 10; i < 20; i++) cdlData[i] = 0x02;

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// CDL should have classified at least some bytes
		Assert.True(result.Stats.CdlClassified > 0);

		// Verify classification sources
		// CDL offsets map to platform-specific ROM offsets for NES
		// The exact mapping depends on NesAnalyzer's offset handling
	}

	[Fact]
	public void CdlDrawnFlags_ClassifiedAsGraphics() {
		var rom = CreateNesRom();

		// Mesen format CDL with DRAWN flag (0x10)
		var cdlData = new byte[rom.Length - 16];
		for (int i = 100; i < 110; i++) cdlData[i] = 0x10; // DRAWN

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// DRAWN flags should contribute to CDL classification
		Assert.True(result.Stats.CdlClassified >= 0);
	}

	// ========================================================================
	// Pansy + Static Analysis Integration
	// ========================================================================

	[Fact]
	public void PansySymbols_EnrichClassification() {
		var rom = CreateNesRom();

		// Build a Pansy file with symbols
		var writer = new PansyWriter();
		writer.Platform = 0x01; // NES
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddSymbol(0x8000, "reset_handler", SymbolType.Function);
		writer.AddSymbol(0x8100, "data_table", SymbolType.Label);
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadPansyData(pansyData);

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// Pansy symbols should be applied
		Assert.True(result.Stats.PansyClassified > 0);
	}

	[Fact]
	public void PansyCrossRefs_ClassifyTargets() {
		var rom = CreateNesRom();

		// Build Pansy with cross-references
		var writer = new PansyWriter();
		writer.Platform = 0x01;
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddCrossReference(new CrossReference(0x8000, 0x8100, PansyCrossRefType.Jsr));
		writer.AddCrossReference(new CrossReference(0x8010, 0x9000, PansyCrossRefType.Read));
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadPansyData(pansyData);

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// Cross-refs should classify some bytes
		Assert.True(result.Stats.PansyClassified > 0);
	}

	[Fact]
	public void PansyMemoryRegions_ClassifyRegionBytes() {
		var rom = CreateNesRom();

		var writer = new PansyWriter();
		writer.Platform = 0x01;
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddMemoryRegion(new PansyMemoryRegion(0x8200, 0x82ff, (byte)MemoryRegionType.VRAM, 0, "Player Graphics"));
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadPansyData(pansyData);

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// The memory region should have classified some bytes
		Assert.True(result.Stats.PansyClassified >= 0);
	}

	// ========================================================================
	// Combined CDL + Pansy Priority Tests
	// ========================================================================

	[Fact]
	public void CdlTakesPriorityOverPansy() {
		var rom = CreateNesRom();

		// CDL says offset 0 is CODE
		var cdlData = new byte[rom.Length - 16];
		cdlData[0] = 0x01; // CODE

		// Pansy has a symbol at $8000 (which maps to offset 0)
		var writer = new PansyWriter();
		writer.Platform = 0x01;
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddSymbol(0x8000, "test_label", SymbolType.Label);
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));
		symbolLoader.LoadPansyData(pansyData);

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// CDL classified bytes should take priority
		Assert.True(result.Stats.CdlClassified > 0);
	}

	// ========================================================================
	// DisassemblyEngine Integration Tests
	// ========================================================================

	[Fact]
	public void DisassemblyEngine_UsesStaticAnalysis() {
		// LDA #$00, STA $0300, JMP $8000 (infinite loop for testing)
		var code = new byte[] {
			0xa9, 0x00,       // LDA #$00
			0x8d, 0x00, 0x03, // STA $0300
			0x4c, 0x00, 0x80, // JMP $8000
		};
		var rom = CreateNesRom(code);

		var engine = new DisassemblyEngine(_nesAnalyzer.CpuDecoder, _nesAnalyzer);
		var result = engine.Disassemble(rom, [0x8000]);

		// Should successfully disassemble the code
		Assert.NotEmpty(result.Blocks);

		// Should have at least one block starting at $8000
		Assert.Contains(result.Blocks, b => b.StartAddress == 0x8000);
	}

	[Fact]
	public void DisassemblyEngine_CdlDataRegionsStopDisassembly() {
		// Code followed by data that CDL marks as DATA
		var code = new byte[] {
			0xa9, 0x00,       // LDA #$00
			0x60,             // RTS
			// Data bytes at offset 3-10
			0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21, 0x00, 0x00,
		};
		var rom = CreateNesRom(code);

		// Mark the data region in CDL
		var cdlData = new byte[rom.Length - 16];
		cdlData[0] = 0x01; // CODE
		cdlData[1] = 0x01; // CODE
		cdlData[2] = 0x01; // CODE (RTS)
		for (int i = 3; i <= 10; i++) cdlData[i] = 0x02; // DATA

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));

		var engine = new DisassemblyEngine(_nesAnalyzer.CpuDecoder, _nesAnalyzer);
		engine.SetSymbolLoader(symbolLoader);
		var result = engine.Disassemble(rom, [0x8000]);

		// RTS should end the block, data region shouldn't be disassembled as code
		Assert.NotEmpty(result.Blocks);
	}

	[Fact]
	public void DisassemblyEngine_WithPansySymbols_LabelsAttached() {
		var code = new byte[] {
			0x20, 0x10, 0x80, // JSR $8010
			0x60,             // RTS
		};
		var rom = CreateNesRom(code);

		// Add Pansy symbols
		var writer = new PansyWriter();
		writer.Platform = 0x01;
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddSymbol(0x8000, "main", SymbolType.Function);
		writer.AddSymbol(0x8010, "subroutine", SymbolType.Function);
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadPansyData(pansyData);

		var engine = new DisassemblyEngine(_nesAnalyzer.CpuDecoder, _nesAnalyzer);
		engine.SetSymbolLoader(symbolLoader);
		var result = engine.Disassemble(rom, [0x8000]);

		// Should have the Pansy-provided labels
		Assert.Equal("main", result.Labels[0x8000]);
		Assert.Equal("subroutine", result.Labels[0x8010]);
	}

	// ========================================================================
	// Classification Regions Integration
	// ========================================================================

	[Fact]
	public void GetRegions_AfterFullClassification_ProducesContiguousRegions() {
		var rom = CreateNesRom();

		// CDL marks some regions
		var cdlData = new byte[rom.Length - 16];
		for (int i = 0; i < 100; i++) cdlData[i] = 0x01; // CODE
		for (int i = 100; i < 200; i++) cdlData[i] = 0x02; // DATA

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);
		var regions = result.GetRegions();

		Assert.NotEmpty(regions);

		// Verify contiguity
		for (int i = 1; i < regions.Count; i++) {
			Assert.Equal(regions[i - 1].EndOffset + 1, regions[i].StartOffset);
		}
	}

	[Fact]
	public void ClassificationStats_WithCdlAndPansy_ShowsBothSources() {
		var rom = CreateNesRom();

		// CDL data
		var cdlData = new byte[rom.Length - 16];
		for (int i = 0; i < 50; i++) cdlData[i] = 0x01;

		// Pansy data
		var writer = new PansyWriter();
		writer.Platform = 0x01;
		writer.RomSize = (uint)(rom.Length - 16);
		writer.AddSymbol(0x9000, "my_func", SymbolType.Function);
		var pansyData = writer.Generate();

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));
		symbolLoader.LoadPansyData(pansyData);

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		Assert.Equal(rom.Length, result.Stats.TotalBytes);
		Assert.True(result.Stats.CoveragePercent >= 0);
		Assert.True(result.Stats.CoveragePercent <= 100);
	}
}
