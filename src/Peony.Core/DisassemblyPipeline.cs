namespace Peony.Core;

/// <summary>
/// Shared helpers for the disassembly pipeline stages.
/// Eliminates duplication across CLI commands that follow the same
/// ROM → detect → analyze → load hints → build entries → disassemble pattern.
/// </summary>
public static class DisassemblyPipeline {
	/// <summary>
	/// Loads hint data from various source files into a <see cref="SymbolLoader"/>.
	/// Returns null if no hint files were provided or none exist.
	/// </summary>
	public static SymbolLoader? LoadHints(
		string? symbolsPath = null,
		string? cdlPath = null,
		string? dizPath = null,
		string? pansyPath = null,
		IPlatformAnalyzer? analyzer = null) {
		SymbolLoader? loader = null;

		if (symbolsPath != null && File.Exists(symbolsPath)) {
			loader ??= new SymbolLoader();
			loader.Load(symbolsPath);
		}

		if (cdlPath != null && File.Exists(cdlPath)) {
			loader ??= new SymbolLoader();
			loader.LoadCdl(cdlPath, analyzer);
		}

		if (dizPath != null && File.Exists(dizPath)) {
			loader ??= new SymbolLoader();
			loader.LoadDiz(dizPath);
		}

		if (pansyPath != null && File.Exists(pansyPath)) {
			loader ??= new SymbolLoader();
			loader.LoadPansy(pansyPath);
		}

		return loader;
	}

	/// <summary>
	/// Maximum number of targets consumed from a single multi-target cross-reference
	/// (indirect jump) source during entry-point expansion.
	///
	/// <para>
	/// Policy rationale: A single indirect jump (e.g. a jump table) may legitimately
	/// resolve to thousands of targets. To keep seeding deterministic and bounded, we
	/// cap the number of targets accepted per source group. Any legitimate sub-entries
	/// beyond this limit will still be discovered at runtime via recursive descent once
	/// the surrounding code is disassembled.
	/// </para>
	///
	/// <para>
	/// Performance: at 10,000 targets per source and typical SNES/NES game complexity,
	/// this cap is effectively never hit in practice. Its purpose is to guard against
	/// pathological or malformed Pansy files generating unbounded queue growth
	/// (O(N) entries, O(N) labels) before disassembly begins.
	/// </para>
	/// </summary>
	public const int MaxIndirectJumpTargetsPerSource = 10_000;

	/// <summary>
	/// Builds a comprehensive, deterministically-ordered entry point list by
	/// merging platform reset vectors with hints from Pansy and CDL data.
	///
	/// <para>
	/// Expansion policy (in priority order):
	/// <list type="number">
	///   <item>Platform primary reset vector (deterministic start)</item>
	///   <item>Pansy sub-entry points (CODE/DATA map SUB_ENTRY flag)</item>
	///   <item>Pansy jump targets (CODE/DATA map JUMP_TARGET flag)</item>
	///   <item>Pansy cross-reference targets (JSR, JMP, Branch edges)</item>
	///   <item>Pansy multi-target indirect jump targets (fan-out capped at <see cref="MaxIndirectJumpTargetsPerSource"/> per source)</item>
	///   <item>CDL sub-entry points</item>
	///   <item>CDL jump targets</item>
	///   <item>Remaining platform entry vectors</item>
	///   <item>Fallback to 0x8000 if empty</item>
	/// </list>
	/// All hint-derived addresses are sorted ascending before insertion to ensure
	/// deterministic output regardless of Pansy file serialization order.
	/// </para>
	/// </summary>
	public static uint[] BuildEntryPoints(
		IPlatformAnalyzer analyzer,
		byte[] romData,
		SymbolLoader? symbolLoader = null) {
		uint[] platformEntryPoints = analyzer.GetEntryPoints(romData);
		var ordered = platformEntryPoints.Distinct().OrderBy(x => x).ToArray();

		var entryPoints = new List<uint>();
		var seen = new HashSet<uint>();

		void Add(uint addr) {
			if (seen.Add(addr))
				entryPoints.Add(addr);
		}

		// 1. Primary platform entry point (deterministic start)
		if (ordered.Length > 0)
			Add(ordered[0]);

		// 2. Pansy sub-entry points (from code/data map)
		if (symbolLoader?.PansySubEntryPoints.Count > 0) {
			foreach (var offset in symbolLoader.PansySubEntryPoints.OrderBy(x => x)) {
				var addr = analyzer.OffsetToAddress(offset);
				if (addr.HasValue)
					Add(addr.Value);
			}
		}

		// 3. Pansy jump targets (from code/data map)
		if (symbolLoader?.PansyJumpTargets.Count > 0) {
			foreach (var offset in symbolLoader.PansyJumpTargets.OrderBy(x => x)) {
				var addr = analyzer.OffsetToAddress(offset);
				if (addr.HasValue)
					Add(addr.Value);
			}
		}

		// 4. Pansy cross-reference targets (JSR, JMP, Branch).
		// Grouped by source so that any single from-address can contribute at most
		// MaxIndirectJumpTargetsPerSource targets — this caps the legacy edge
		// representation of indirect/multi-target jumps that also writes individual
		// CrossReference edges via PansyWriter.AddMultiTargetCrossReference.
		// Targets within each source group are sorted ascending before the cap is
		// applied so the lowest addresses are always preferred, ensuring determinism
		// regardless of serialisation order in the Pansy file.
		if (symbolLoader?.PansyData is { } pd && pd.CrossReferences.Count > 0) {
			var bySource = pd.CrossReferences
				.Where(x => x.Type is Pansy.Core.CrossRefType.Jsr or Pansy.Core.CrossRefType.Jmp or Pansy.Core.CrossRefType.Branch)
				.GroupBy(x => x.From);

			foreach (var group in bySource) {
				var cappedTargets = group
					.Select(x => x.To)
					.Distinct()
					.OrderBy(x => x)
					.Take(MaxIndirectJumpTargetsPerSource);

				foreach (var target in cappedTargets) {
					var addr = analyzer.OffsetToAddress((int)target);
					if (addr.HasValue)
						Add(addr.Value);
				}
			}
		}

		// 4b. Explicit grouped one-source-many-target references (indirect jumps).
		// Each source group is capped at MaxIndirectJumpTargetsPerSource to bound
		// queue growth from pathological or high-fan-out jump tables.
		// Targets within each group are sorted ascending for deterministic ordering.
		if (symbolLoader?.PansyData is { } grouped && grouped.MultiTargetCrossReferences.Count > 0) {
			foreach (var group in grouped.MultiTargetCrossReferences
				.Where(x => x.Type is Pansy.Core.CrossRefType.Jsr or Pansy.Core.CrossRefType.Jmp or Pansy.Core.CrossRefType.Branch)) {
				var cappedTargets = group.Targets
					.Distinct()
					.OrderBy(x => x)
					.Take(MaxIndirectJumpTargetsPerSource);

				foreach (var target in cappedTargets) {
					var addr = analyzer.OffsetToAddress((int)target);
					if (addr.HasValue)
						Add(addr.Value);
				}
			}
		}

		// 5. CDL sub-entry points
		if (symbolLoader?.CdlData is { } cdl && cdl.SubEntryPoints.Count > 0) {
			foreach (var offset in cdl.SubEntryPoints.OrderBy(x => x)) {
				var addr = analyzer.OffsetToAddress(offset);
				if (addr.HasValue)
					Add(addr.Value);
			}
		}

		// 6. CDL jump targets
		if (symbolLoader?.CdlData is { } cdlJump && cdlJump.JumpTargets.Count > 0) {
			foreach (var offset in cdlJump.JumpTargets.OrderBy(x => x)) {
				var addr = analyzer.OffsetToAddress(offset);
				if (addr.HasValue)
					Add(addr.Value);
			}
		}

		// 7. Remaining platform entry points
		for (int i = 1; i < ordered.Length; i++)
			Add(ordered[i]);

		// 8. Fallback if nothing found
		if (entryPoints.Count == 0)
			Add(0x8000);

		return entryPoints.ToArray();
	}

	/// <summary>
	/// Creates and configures a <see cref="DisassemblyEngine"/> with loaded symbol data.
	/// </summary>
	public static DisassemblyEngine CreateEngine(
		IPlatformAnalyzer analyzer,
		SymbolLoader? symbolLoader = null,
		bool useStaticAnalysis = false) {
		var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);
		engine.SetStaticAnalysisEnabled(useStaticAnalysis);

		if (symbolLoader != null) {
			engine.SetSymbolLoader(symbolLoader);
			foreach (var (addr, label) in symbolLoader.Labels)
				engine.AddLabel(addr, label);
			foreach (var (addr, comment) in symbolLoader.Comments)
				engine.AddComment(addr, comment);
			foreach (var (addr, dataDef) in symbolLoader.DataDefinitions)
				engine.AddDataRegion(addr, dataDef);
		}

		return engine;
	}
}
