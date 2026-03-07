# Code Plan: Peony Type-Preserving Pansy Import

**Issue:** peony#101 (Epic), peony#102, peony#103
**Created:** 2026-03-08

---

## Overview

Update Peony's SymbolLoader and related classes to preserve symbol types and comment types when importing .pansy files, so they survive the import-export roundtrip.

## Current Architecture

```
SymbolLoader.LoadPansy(path)
├── PansyLoader.Load(path) → PansyFile
├── foreach symbol: _symbols[addr] = name    ← TYPE LOST HERE
├── foreach comment: _comments[addr] = text  ← TYPE LOST HERE
├── foreach cdlFlag: _codeDataMap[addr] = flags
└── foreach region: _memoryRegions.Add(...)

SymbolExporter.ExportPansy(path, engine)
├── PansyWriter writer = new(...)
├── foreach symbol: writer.AddSymbol(addr, name, InferType(name))  ← RE-INFERS TYPE
├── foreach comment: writer.AddComment(addr, text, CommentType.Inline)  ← HARDCODED
└── ...
```

## Target Architecture

```
SymbolLoader.LoadPansy(path)
├── PansyLoader.Load(path) → PansyFile
├── foreach symbol: _symbols[addr] = new SymbolInfo(name, type)    ← TYPE PRESERVED
├── foreach comment: _comments[addr] = new CommentInfo(text, type) ← TYPE PRESERVED
├── foreach cdlFlag: _codeDataMap[addr] = flags
└── foreach region: _memoryRegions.Add(...)

SymbolExporter.ExportPansy(path, engine)
├── PansyWriter writer = new(...)
├── foreach symbol: writer.AddSymbol(addr, name, symbol.Type ?? InferType(name))  ← USE STORED
├── foreach comment: writer.AddComment(addr, text, comment.Type ?? Inline)  ← USE STORED
└── ...
```

## Data Model Changes

### New Types (or extend existing)

```csharp
// In Peony.Core
public record struct SymbolInfo(string Name, PansySymbolType? Type = null);
public record struct CommentInfo(string Text, PansyCommentType? Type = null);
```

### SymbolLoader Changes

```csharp
// Change from Dictionary<int, string> to Dictionary<int, SymbolInfo>
private readonly Dictionary<int, SymbolInfo> _symbols = new();
private readonly Dictionary<int, CommentInfo> _comments = new();

public void LoadPansy(string path) {
	var file = PansyLoader.Load(path);

	foreach (var symbol in file.Symbols) {
		_symbols[symbol.Address] = new SymbolInfo(symbol.Name, symbol.Type);
	}

	foreach (var comment in file.Comments) {
		_comments[comment.Address] = new CommentInfo(comment.Text, comment.Type);
	}
	// ... rest unchanged
}
```

### SymbolExporter Changes

```csharp
public void ExportPansy(string path, DisassemblyEngine engine) {
	// ...
	foreach (var (addr, symbol) in symbolLoader.Symbols) {
		var type = symbol.Type ?? InferSymbolType(symbol.Name, addr, engine);
		writer.AddSymbol((uint)addr, symbol.Name, type);
	}

	foreach (var (addr, comment) in symbolLoader.Comments) {
		writer.AddComment((uint)addr, comment.Text, comment.Type ?? PansyCommentType.Inline);
	}
	// ...
}
```

## CDL Hint Queuing (peony#104, peony#105)

### SymbolLoader Changes

```csharp
public void LoadPansy(string path) {
	var file = PansyLoader.Load(path);
	// ... existing symbol/comment loading ...

	// Queue CDL hints for analysis
	if (file.CodeDataMap != null) {
		for (int i = 0; i < file.CodeDataMap.Length; i++) {
			byte flags = file.CodeDataMap[i];

			if ((flags & 0x04) != 0) { // JUMP_TARGET
				_jumpTargetHints.Add(i);
			}

			if ((flags & 0x08) != 0) { // SUB_ENTRY
				_subEntryHints.Add(i);
			}
		}
	}
}

public IReadOnlySet<int> JumpTargetHints => _jumpTargetHints;
public IReadOnlySet<int> SubEntryHints => _subEntryHints;
```

### DisassemblyEngine Integration

```csharp
// After SymbolLoader finishes, queue hints
foreach (int addr in symbolLoader.JumpTargetHints) {
	if (!IsClassified(addr)) {
		QueueForAnalysis(addr, AnalysisReason.PansyJumpTarget);
	}
}

foreach (int addr in symbolLoader.SubEntryHints) {
	if (!IsClassified(addr)) {
		QueueForAnalysis(addr, AnalysisReason.PansySubEntry);
		if (!symbolLoader.HasSymbol(addr)) {
			symbolLoader.AddSymbol(addr, new SymbolInfo($"sub_{addr:x6}", PansySymbolType.Function));
		}
	}
}
```

## Test Plan

1. **SymbolTypeRoundtrip** — Write .pansy with Function/Constant/Label symbols → import → export → verify types match
2. **CommentTypeRoundtrip** — Write .pansy with Inline/Block/Todo comments → import → export → verify types match
3. **TypeFallback** — Import without types → export → verify InferType is used
4. **JumpTargetQueuing** — Import .pansy with JUMP_TARGET CDL → verify addresses queued for analysis
5. **SubEntryQueuing** — Import .pansy with SUB_ENTRY CDL → verify addresses queued + auto-labeled
6. **MixedContent** — All types together in one .pansy → full roundtrip
7. **BackwardCompatibility** — Existing tests still pass with new data model

## Risk Assessment

- **Low risk:** Data model change (SymbolInfo/CommentInfo) is additive — existing code uses `.Name`/`.Text` which still work
- **Medium risk:** CDL hint queuing could cause analysis regressions if bad hints are queued — need to filter already-classified addresses
- **Mitigation:** Run all 991 tests after each change
