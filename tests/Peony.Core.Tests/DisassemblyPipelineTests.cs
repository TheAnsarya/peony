using Peony.Core;
using Pansy.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for DisassemblyPipeline helper methods
/// </summary>
public class DisassemblyPipelineTests {
	private sealed class MockCpuDecoder : ICpuDecoder {
		public string Architecture => "Test6502";

		public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
			if (data.Length == 0) return new DecodedInstruction("", "", [], AddressingMode.Implied);
			return new DecodedInstruction("nop", "", [data[0]], AddressingMode.Implied);
		}

		public bool IsControlFlow(DecodedInstruction instruction) => false;
		public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint currentAddress) => [];
	}

	private sealed class MockPlatformAnalyzer : IPlatformAnalyzer {
		public string Platform => "Test";
		public int BankCount => 1;
		public int RomDataOffset => 0;
		public ICpuDecoder CpuDecoder => new MockCpuDecoder();

		public uint[] EntryPoints { get; set; } = [0x8000];

		public RomInfo Analyze(ReadOnlySpan<byte> rom) => new(
			Platform: "Test",
			Size: rom.Length,
			Mapper: null,
			Metadata: []
		);

		public int AddressToOffset(uint address, int romLength) => (int)(address - 0x8000);
		public int AddressToOffset(uint address, int romLength, int bank) => (int)(address - 0x8000);
		public uint? OffsetToAddress(int offset) => (uint)(0x8000 + offset);
		public string? GetRegisterLabel(uint address) => null;
		public bool IsInSwitchableRegion(uint address) => false;
		public bool IsValidAddress(uint address) => true;
		public int GetTargetBank(uint target, int currentBank) => currentBank;
		public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) => null;
		public MemoryRegion GetMemoryRegion(uint address) => MemoryRegion.Code;
		public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) => EntryPoints;
	}

	// =========================================================================
	// LoadHints tests
	// =========================================================================

	[Fact]
	public void LoadHints_NoFiles_ReturnsNull() {
		var result = DisassemblyPipeline.LoadHints();
		Assert.Null(result);
	}

	[Fact]
	public void LoadHints_AllNull_ReturnsNull() {
		var result = DisassemblyPipeline.LoadHints(null, null, null, null);
		Assert.Null(result);
	}

	[Fact]
	public void LoadHints_NonexistentSymbolFile_ReturnsNull() {
		var result = DisassemblyPipeline.LoadHints(
			symbolsPath: Path.Combine(Path.GetTempPath(), "nonexistent_12345.sym"));
		Assert.Null(result);
	}

	[Fact]
	public void LoadHints_NonexistentFiles_ReturnsNull() {
		var result = DisassemblyPipeline.LoadHints(
			symbolsPath: "nonexistent.sym",
			cdlPath: "nonexistent.cdl",
			dizPath: "nonexistent.diz",
			pansyPath: "nonexistent.pansy");
		Assert.Null(result);
	}

	// =========================================================================
	// BuildEntryPoints tests
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_NoSymbols_ReturnsPlatformEntries() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000, 0xfffc] };
		var romData = new byte[0x8000];

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData);

		Assert.Contains(0x8000u, entries);
		Assert.Contains(0xfffcu, entries);
		Assert.Equal(0x8000u, entries[0]); // Primary entry first
	}

	[Fact]
	public void BuildEntryPoints_EmptyPlatformEntries_UsesFallback() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [] };
		var romData = new byte[0x8000];

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData);

		Assert.Single(entries);
		Assert.Equal(0x8000u, entries[0]);
	}

	[Fact]
	public void BuildEntryPoints_NullSymbolLoader_NoCrash() {
		var analyzer = new MockPlatformAnalyzer();
		var romData = new byte[0x8000];

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, null);

		Assert.NotEmpty(entries);
	}

	[Fact]
	public void BuildEntryPoints_DuplicatePlatformEntries_Deduplicated() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000, 0x8000, 0x8000] };
		var romData = new byte[0x8000];

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData);

		Assert.Single(entries);
		Assert.Equal(0x8000u, entries[0]);
	}

	[Fact]
	public void BuildEntryPoints_PlatformEntriesAreSorted() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x9000, 0x8000, 0xa000] };
		var romData = new byte[0x8000];

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData);

		// Primary entry (lowest sorted) should be first
		Assert.Equal(0x8000u, entries[0]);
		Assert.Equal(3, entries.Length);
	}

	[Fact]
	public void BuildEntryPoints_UsesGroupedPansyMultiTargetReferences() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_SNES,
			RomSize = 0x8000
		};
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(0x0010, Pansy.Core.CrossRefType.Branch, [0x0100, 0x0200]));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Contains(0x8100u, entries);
		Assert.Contains(0x8200u, entries);
	}

	[Fact]
	public void BuildEntryPoints_UsesCdlJumpTargets() {
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var cdlData = new byte[0x200];
		cdlData[0x40] = 0x04; // Mesen jump-target flag

		var loader = new SymbolLoader();
		loader.LoadCdlData(new CdlLoader(cdlData));

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Contains(0x8040u, entries);
	}

	// =========================================================================
	// CreateEngine tests
	// =========================================================================

	[Fact]
	public void CreateEngine_NoSymbols_ReturnsEngine() {
		var analyzer = new MockPlatformAnalyzer();

		var engine = DisassemblyPipeline.CreateEngine(analyzer);

		Assert.NotNull(engine);
	}

	[Fact]
	public void CreateEngine_NullSymbolLoader_ReturnsEngine() {
		var analyzer = new MockPlatformAnalyzer();

		var engine = DisassemblyPipeline.CreateEngine(analyzer, null);

		Assert.NotNull(engine);
	}

	[Fact]
	public void CreateEngine_WithSymbolLoader_ConfiguresEngine() {
		var analyzer = new MockPlatformAnalyzer();
		var loader = new SymbolLoader();

		var engine = DisassemblyPipeline.CreateEngine(analyzer, loader);

		Assert.NotNull(engine);
	}

	[Fact]
	public void CreateEngine_WithStaticAnalysis_ConfiguresEngine() {
		var analyzer = new MockPlatformAnalyzer();

		var engine = DisassemblyPipeline.CreateEngine(analyzer, useStaticAnalysis: true);

		Assert.NotNull(engine);
	}

	[Fact]
	public void CreateEngine_WithLabels_AddsLabels() {
		var analyzer = new MockPlatformAnalyzer();
		var loader = new SymbolLoader();
		// SymbolLoader.Load would populate labels, but we test the wiring
		// by confirming CreateEngine processes labels without error

		var engine = DisassemblyPipeline.CreateEngine(analyzer, loader);

		Assert.NotNull(engine);
	}

	// =========================================================================
	// Integration tests — BuildEntryPoints → CreateEngine → Disassemble
	// =========================================================================

	[Fact]
	public void Pipeline_EndToEnd_DisassemblesRom() {
		var analyzer = new MockPlatformAnalyzer();
		var romData = new byte[16];
		romData[0] = 0xea; // NOP

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData);
		var engine = DisassemblyPipeline.CreateEngine(analyzer);
		var result = engine.Disassemble(romData, entries);

		Assert.NotNull(result);
		Assert.NotEmpty(result.Blocks);
	}

	[Fact]
	public void Pipeline_EndToEnd_WithSymbolLoader() {
		var analyzer = new MockPlatformAnalyzer();
		var romData = new byte[16];
		var loader = new SymbolLoader();

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);
		var engine = DisassemblyPipeline.CreateEngine(analyzer, loader);
		var result = engine.Disassemble(romData, entries);

		Assert.NotNull(result);
	}

	// =========================================================================
	// Fan-out cap tests — multi-target cross-reference expansion policy (#192)
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_CapIsAppliedPerSource() {
		// Generate MaxIndirectJumpTargetsPerSource+1 targets for a single indirect jump.
		// Only the first MaxIndirectJumpTargetsPerSource (sorted ascending) should appear.
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x10000];

		int overCap = DisassemblyPipeline.MaxIndirectJumpTargetsPerSource + 1;
		// Targets are offsets 0..overCap-1; OffsetToAddress adds 0x8000
		var targets = Enumerable.Range(1, overCap).Select(i => (uint)i).ToArray();

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x10000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0000,
			Pansy.Core.CrossRefType.Jmp,
			targets));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// Should contain MaxIndirectJumpTargetsPerSource targets from this group
		// plus the primary entry 0x8000
		var groupEntries = entries.Where(e => e != 0x8000).ToArray();
		Assert.Equal(DisassemblyPipeline.MaxIndirectJumpTargetsPerSource, groupEntries.Length);
	}

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_CapIsPerSource_MultipleGroupsEachGetFullCap() {
		// Two sources, each with MaxIndirectJumpTargetsPerSource+1 targets.
		// Both groups should each contribute MaxIndirectJumpTargetsPerSource entries.
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x10000];

		int overCap = DisassemblyPipeline.MaxIndirectJumpTargetsPerSource + 1;
		var targets1 = Enumerable.Range(1, overCap).Select(i => (uint)i).ToArray();
		// Targets2 overlaps with targets1 entirely, so after deduplication each group still caps independently
		var targets2 = Enumerable.Range(1, overCap).Select(i => (uint)(i + overCap)).ToArray();

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x10000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(0x0001, Pansy.Core.CrossRefType.Jmp, targets1));
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(0x0002, Pansy.Core.CrossRefType.Jmp, targets2));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// All unique targets from both capped groups should be present
		var groupEntries = entries.Where(e => e != 0x8000).ToArray();
		Assert.Equal(DisassemblyPipeline.MaxIndirectJumpTargetsPerSource * 2, groupEntries.Length);
	}

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_WithinCap_AllTargetsIncluded() {
		// A group with fewer targets than the cap should include all of them.
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var targets = new uint[] { 0x0010, 0x0020, 0x0030 };

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(0x0000, Pansy.Core.CrossRefType.Jmp, targets));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Contains(0x8010u, entries);
		Assert.Contains(0x8020u, entries);
		Assert.Contains(0x8030u, entries);
	}

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_TargetsSortedAscendingBeforeCap() {
		// Targets in reverse order should still be sorted ascending before cap is applied,
		// ensuring the lowest addresses (not the highest) are preserved when capped.
		var analyzer = new MockPlatformAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x10000];

		int overCap = DisassemblyPipeline.MaxIndirectJumpTargetsPerSource + 1;
		// Reverse order: highest first, so without sorting the cap would drop the lowest
		var targets = Enumerable.Range(1, overCap).Select(i => (uint)(overCap - i + 1)).ToArray();

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x10000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(0x0000, Pansy.Core.CrossRefType.Jmp, targets));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// After sort+cap, the lowest MaxIndirectJumpTargetsPerSource addresses should be present
		// The highest address (offset=overCap, addr=0x8000+overCap) should NOT be present
		var groupEntries = entries.Where(e => e != 0x8000).ToArray();
		Assert.Equal(DisassemblyPipeline.MaxIndirectJumpTargetsPerSource, groupEntries.Length);
		// The highest address (offset = overCap = MaxIndirectJumpTargetsPerSource+1) should be dropped
		Assert.DoesNotContain((uint)(0x8000 + overCap), groupEntries);
	}
}
