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
		string? pansyPath = null) {
		SymbolLoader? loader = null;

		if (symbolsPath != null && File.Exists(symbolsPath)) {
			loader ??= new SymbolLoader();
			loader.Load(symbolsPath);
		}

		if (cdlPath != null && File.Exists(cdlPath)) {
			loader ??= new SymbolLoader();
			loader.LoadCdl(cdlPath);
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
	/// Builds a comprehensive, deterministically-ordered entry point list by
	/// merging platform reset vectors with hints from Pansy and CDL data.
	/// Order: platform primary → Pansy sub-entries → Pansy jump targets →
	/// Pansy cross-refs → CDL sub-entries → CDL jump targets →
	/// remaining platform entries → fallback.
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

		// 4. Pansy cross-reference targets (JSR, JMP, Branch)
		if (symbolLoader?.PansyData is { } pd && pd.CrossReferences.Count > 0) {
			var targets = pd.CrossReferences
				.Where(x => x.Type is Pansy.Core.CrossRefType.Jsr or Pansy.Core.CrossRefType.Jmp or Pansy.Core.CrossRefType.Branch)
				.Select(x => x.To)
				.Distinct()
				.OrderBy(x => x);

			foreach (var target in targets) {
				var addr = analyzer.OffsetToAddress((int)target);
				if (addr.HasValue)
					Add(addr.Value);
			}
		}

		// 4b. Explicit grouped one-source-many-target references (if present)
		if (symbolLoader?.PansyData is { } grouped && grouped.MultiTargetCrossReferences.Count > 0) {
			var groupedTargets = grouped.MultiTargetCrossReferences
				.Where(x => x.Type is Pansy.Core.CrossRefType.Jsr or Pansy.Core.CrossRefType.Jmp or Pansy.Core.CrossRefType.Branch)
				.SelectMany(x => x.Targets)
				.Distinct()
				.OrderBy(x => x);

			foreach (var target in groupedTargets) {
				var addr = analyzer.OffsetToAddress((int)target);
				if (addr.HasValue)
					Add(addr.Value);
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
