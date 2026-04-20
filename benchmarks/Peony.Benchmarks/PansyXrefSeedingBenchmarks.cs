using BenchmarkDotNet.Attributes;
using Pansy.Core;
using Peony.Core;
using Peony.Platform.SNES;

// Aliases to avoid ambiguity between Pansy.Core and Peony.Core
using PansyCrossRefType = Pansy.Core.CrossRefType;

namespace Peony.Benchmarks;

/// <summary>
/// Benchmarks for BuildEntryPoints with varying Pansy cross-reference graph density (#195).
/// Measures runtime and allocation cost at low, medium, and high xref density,
/// as well as the memory overhead of loading a dense multi-target xref graph.
/// </summary>
[MemoryDiagnoser]
public class PansyXrefSeedingBenchmarks {
	// -------------------------------------------------------------------------
	// Shared state
	// -------------------------------------------------------------------------
	private SnesAnalyzer _analyzer = null!;
	private byte[] _romData = null!;

	// Pre-built SymbolLoader instances for each density scenario
	private SymbolLoader _loaderNone = null!;       // No xrefs
	private SymbolLoader _loaderLow = null!;        // 10 sources × 5 targets
	private SymbolLoader _loaderMedium = null!;     // 100 sources × 50 targets
	private SymbolLoader _loaderHigh = null!;       // 1,000 sources × 200 targets
	private SymbolLoader _loaderMultiTarget = null!; // 10 groups × 500 targets (grouped API)

	[GlobalSetup]
	public void Setup() {
		_analyzer = new SnesAnalyzer();

		// 512KB flat ROM — large enough to cover all generated offsets
		_romData = new byte[524288];

		// None
		_loaderNone = BuildLoader([]);

		// Low: 10 sources, each with 5 unique targets
		_loaderLow = BuildLoader(GenerateIndividualEdges(sources: 10, targetsPerSource: 5, sourceBase: 0x0001, targetBase: 0x1000));

		// Medium: 100 sources × 50 targets
		_loaderMedium = BuildLoader(GenerateIndividualEdges(sources: 100, targetsPerSource: 50, sourceBase: 0x0001, targetBase: 0x10000));

		// High: 1,000 sources × 200 targets (all individual edges, worst-case step 4)
		_loaderHigh = BuildLoader(GenerateIndividualEdges(sources: 1_000, targetsPerSource: 200, sourceBase: 0x0001, targetBase: 0x20000));

		// Multi-target grouped API: 10 groups, each with 500 unique targets
		_loaderMultiTarget = BuildMultiTargetLoader(groups: 10, targetsPerGroup: 500, targetBase: 0x1000);
	}

	// =========================================================================
	// Entry-point seeding benchmarks
	// =========================================================================

	/// <summary>
	/// Baseline: BuildEntryPoints with no Pansy data. Sets the floor latency.
	/// </summary>
	[Benchmark(Baseline = true)]
	public uint[] Seed_NoPansyData() =>
		DisassemblyPipeline.BuildEntryPoints(_analyzer, _romData);

	/// <summary>
	/// Low density: 50 individual xref edges. Realistic for small scripts.
	/// </summary>
	[Benchmark]
	public uint[] Seed_LowDensity_10Sources_5TargetsEach() =>
		DisassemblyPipeline.BuildEntryPoints(_analyzer, _romData, _loaderLow);

	/// <summary>
	/// Medium density: 5,000 individual xref edges. Typical CDL-informed session.
	/// </summary>
	[Benchmark]
	public uint[] Seed_MediumDensity_100Sources_50TargetsEach() =>
		DisassemblyPipeline.BuildEntryPoints(_analyzer, _romData, _loaderMedium);

	/// <summary>
	/// High density: 200,000 individual xref edges. Stress test / adversarial Pansy.
	/// </summary>
	[Benchmark]
	public uint[] Seed_HighDensity_1000Sources_200TargetsEach() =>
		DisassemblyPipeline.BuildEntryPoints(_analyzer, _romData, _loaderHigh);

	/// <summary>
	/// Multi-target grouped API: 10 groups × 500 targets. Tests GroupBy path in step 4
	/// and the grouped iteration in step 4b simultaneously.
	/// </summary>
	[Benchmark]
	public uint[] Seed_MultiTargetGrouped_10Groups_500TargetsEach() =>
		DisassemblyPipeline.BuildEntryPoints(_analyzer, _romData, _loaderMultiTarget);

	// =========================================================================
	// Helpers
	// =========================================================================

	/// <summary>
	/// Generates a flat list of individual CrossReference edges.
	/// Source offsets: sourceBase .. sourceBase+sources-1
	/// Target offsets for source i: targetBase + i*targetsPerSource .. targetBase + (i+1)*targetsPerSource - 1
	/// This ensures each source has unique targets and no target overlap between sources.
	/// </summary>
	private static IEnumerable<CrossReference> GenerateIndividualEdges(
		int sources, int targetsPerSource, uint sourceBase, uint targetBase) {
		for (int s = 0; s < sources; s++) {
			for (int t = 0; t < targetsPerSource; t++) {
				yield return new CrossReference(
					(uint)(sourceBase + s),
					(uint)(targetBase + (s * targetsPerSource) + t),
					PansyCrossRefType.Jmp);
			}
		}
	}

	private static SymbolLoader BuildLoader(IEnumerable<CrossReference> edges) {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_SNES,
			RomSize = 524288
		};
		foreach (var edge in edges) {
			writer.AddCrossReference(edge);
		}
		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());
		return loader;
	}

	private static SymbolLoader BuildMultiTargetLoader(int groups, int targetsPerGroup, uint targetBase) {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_SNES,
			RomSize = 524288
		};
		for (int g = 0; g < groups; g++) {
			var targets = Enumerable.Range(0, targetsPerGroup)
				.Select(t => (uint)(targetBase + (g * targetsPerGroup) + t))
				.ToArray();
			writer.AddMultiTargetCrossReference(new MultiTargetCrossReference(
				(uint)(0x0001 + g),
				PansyCrossRefType.Jmp,
				targets));
		}
		var loader = new SymbolLoader();
		loader.LoadPansyData(writer.Generate());
		return loader;
	}
}
