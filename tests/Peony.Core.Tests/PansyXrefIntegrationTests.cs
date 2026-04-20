using Peony.Core;
using Pansy.Core;
using Xunit;

// Aliases to avoid ambiguity
using PansyCrossRefType = Pansy.Core.CrossRefType;

namespace Peony.Core.Tests;

/// <summary>
/// Integration tests for Pansy cross-reference seeding of disassembly entry points (#193).
/// Covers: which CrossRefType values seed entry points, deduplication, sparse/dense graphs,
/// deterministic ordering, and grouped multi-target API behaviour.
/// </summary>
public class PansyXrefIntegrationTests {
	// -------------------------------------------------------------------------
	// Minimal test doubles
	// -------------------------------------------------------------------------

	private sealed class MockDecoder : ICpuDecoder {
		public string Architecture => "Test";
		public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) =>
			new("nop", "", data.Length > 0 ? [data[0]] : [], AddressingMode.Implied);
		public bool IsControlFlow(DecodedInstruction i) => false;
		public IEnumerable<uint> GetTargets(DecodedInstruction i, uint cur) => [];
	}

	/// <summary>
	/// Flat address space: CPU address = ROM offset + 0x8000.
	/// </summary>
	private sealed class FlatAnalyzer : IPlatformAnalyzer {
		public string Platform => "Test";
		public int BankCount => 1;
		public int RomDataOffset => 0;
		public ICpuDecoder CpuDecoder => new MockDecoder();
		public uint[] EntryPoints { get; set; } = [0x8000];
		public RomInfo Analyze(ReadOnlySpan<byte> rom) =>
			new(Platform: "Test", Size: rom.Length, Mapper: null, Metadata: []);
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

	/// <summary>
	/// Builds a SymbolLoader from a single individual CrossReference.
	/// </summary>
	private static SymbolLoader BuildLoader(uint from, uint to, PansyCrossRefType type, uint romSize = 0x8000) {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_SNES,
			RomSize = romSize
		};
		writer.AddCrossReference(new CrossReference(from, to, type));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());
		return loader;
	}

	// =========================================================================
	// Which CrossRefType values seed entry points?
	// =========================================================================

	[Theory]
	[InlineData(PansyCrossRefType.Jsr)]
	[InlineData(PansyCrossRefType.Jmp)]
	[InlineData(PansyCrossRefType.Branch)]
	public void BuildEntryPoints_CodeRefType_SeedsTarget(PansyCrossRefType type) {
		// Arrange — one xref from offset 0 to offset 0x10, CPU address 0x8010
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];
		var loader = BuildLoader(0x0000, 0x0010, type);

		// Act
		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// Assert — target 0x8010 should be seeded
		Assert.Contains(0x8010u, entries);
	}

	[Theory]
	[InlineData(PansyCrossRefType.Read)]
	[InlineData(PansyCrossRefType.Write)]
	public void BuildEntryPoints_DataRefType_DoesNotSeedTarget(PansyCrossRefType type) {
		// Data references are not code entry points
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];
		var loader = BuildLoader(0x0000, 0x0010, type);

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.DoesNotContain(0x8010u, entries);
	}

	// =========================================================================
	// Deduplication
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_SameTargetFromTwoSources_AppearsOnce() {
		// Both sources jump to offset 0x0010 — the target address should appear exactly once.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddCrossReference(new CrossReference(0x0001, 0x0010, PansyCrossRefType.Jmp));
		writer.AddCrossReference(new CrossReference(0x0002, 0x0010, PansyCrossRefType.Jsr));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Equal(1, entries.Count(e => e == 0x8010u));
	}

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_DuplicateTargetsWithinGroup_AppearsOnce() {
		// A multi-target group with repeated target values should deduplicate.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		// Same offset twice in the targets list
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0000,
			PansyCrossRefType.Jmp,
			[0x0010u, 0x0010u, 0x0020u]));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Equal(1, entries.Count(e => e == 0x8010u));
		Assert.Equal(1, entries.Count(e => e == 0x8020u));
	}

	[Fact]
	public void BuildEntryPoints_GroupedAndIndividualXref_SameTarget_Deduplicated() {
		// A target that appears in both AddMultiTargetCrossReference (grouped) and
		// a separate individual AddCrossReference should appear in the entry list once.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0001,
			PansyCrossRefType.Jmp,
			[0x0010u, 0x0020u]));
		// Explicit individual edge to same target as above
		writer.AddCrossReference(new CrossReference(0x0002, 0x0010, PansyCrossRefType.Jmp));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Equal(1, entries.Count(e => e == 0x8010u));
	}

	// =========================================================================
	// Sparse graph
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_SparseGraph_ManySources_AllTargetsSeeded() {
		// 50 distinct sources, each with one unique target — all 50 targets must be seeded.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		// Sources at offsets 0x0001..0x0032, targets at 0x0100, 0x0101, ..., 0x0131
		for (uint i = 0; i < 50; i++) {
			writer.AddCrossReference(new CrossReference(
				i + 1,
				0x0100 + i,
				PansyCrossRefType.Jsr));
		}

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		for (uint i = 0; i < 50; i++) {
			Assert.Contains(0x8100u + i, entries);
		}
	}

	// =========================================================================
	// Dense graph (small, realistic scale)
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_DenseGraph_SingleSourceManyTargets_AllSeeded() {
		// One indirect jump to 200 targets (well under cap) — all must be seeded.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var targets = Enumerable.Range(1, 200).Select(i => (uint)(0x0100 + i)).ToArray();

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0000,
			PansyCrossRefType.Jmp,
			targets));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		foreach (var t in targets) {
			Assert.Contains(0x8000u + t, entries);
		}
	}

	[Fact]
	public void BuildEntryPoints_DenseGraph_MultipleTypes_AllCodeTypesSeeded() {
		// Three indirect jumps: one Jmp, one Jsr, one Branch — verify all types contribute.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0001, PansyCrossRefType.Jmp, [0x0010u, 0x0020u]));
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0002, PansyCrossRefType.Jsr, [0x0030u, 0x0040u]));
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0003, PansyCrossRefType.Branch, [0x0050u, 0x0060u]));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		Assert.Contains(0x8010u, entries);
		Assert.Contains(0x8020u, entries);
		Assert.Contains(0x8030u, entries);
		Assert.Contains(0x8040u, entries);
		Assert.Contains(0x8050u, entries);
		Assert.Contains(0x8060u, entries);
	}

	[Fact]
	public void BuildEntryPoints_DenseGraph_ReadAndWriteGroupsNotSeeded() {
		// Multi-target groups with Read/Write types should contribute zero entry points.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0001, PansyCrossRefType.Read, [0x0010u, 0x0020u]));
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0002, PansyCrossRefType.Write, [0x0030u, 0x0040u]));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// Only the primary entry 0x8000 should appear
		Assert.Equal([0x8000u], entries);
	}

	// =========================================================================
	// Deterministic ordering
	// =========================================================================

	[Fact]
	public void BuildEntryPoints_XrefTargets_OrderedAscendingAfterPrimary() {
		// Targets added in reverse order — the entry list (after primary) should be ascending.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		// Insert targets in descending order of address
		for (uint i = 10; i >= 1; i--) {
			writer.AddCrossReference(new CrossReference(0x0000, i * 0x10, PansyCrossRefType.Jmp));
		}

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		// Remove primary entry, then verify ascending order
		var xrefEntries = entries.Where(e => e != 0x8000).ToArray();
		Assert.Equal(xrefEntries.OrderBy(e => e).ToArray(), xrefEntries);
	}

	[Fact]
	public void BuildEntryPoints_MultiTargetXref_OrderedAscendingWithinGroup() {
		// Targets within a grouped xref provided in random order — should appear ascending.
		var analyzer = new FlatAnalyzer { EntryPoints = [0x8000] };
		var romData = new byte[0x8000];

		// Supply targets in scrambled order
		uint[] scrambled = [0x0050u, 0x0010u, 0x0040u, 0x0020u, 0x0030u];

		var writer = new PansyWriter { Platform = PansyLoader.PLATFORM_SNES, RomSize = 0x8000 };
		writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
			0x0000, PansyCrossRefType.Jmp, scrambled));

		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());

		var entries = DisassemblyPipeline.BuildEntryPoints(analyzer, romData, loader);

		var xrefEntries = entries.Where(e => e != 0x8000).ToArray();
		Assert.Equal(xrefEntries.OrderBy(e => e).ToArray(), xrefEntries);
	}
}
