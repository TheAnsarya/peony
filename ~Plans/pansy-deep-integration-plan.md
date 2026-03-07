# 🌼🌺 Pansy Deep Integration Plan

> **Epic 8**: Leverage all Pansy v1.0 capabilities for comprehensive disassembly

## Overview

Pansy v1.0 now provides rich metadata that Peony only partially consumes. This plan
integrates **all** available Pansy sections into the disassembly pipeline:

- **Code/Data Map flags**: IsCode, IsData, IsJumpTarget, IsSubEntryPoint, IsOpcode, IsDrawn, IsRead, IsIndirect
- **Typed symbols**: SymbolEntry with SymbolType (Function, InterruptVector, Constant, etc.)
- **Typed comments**: CommentEntry with CommentType (Inline, Block, Todo)
- **Cross-references**: Indexed by From/To address with type (Jsr, Jmp, Branch, Read, Write)
- **Memory regions**: Named regions with type (ROM, RAM, VRAM, IO, etc.) and bank
- **Bookmarks**: User-placed navigation points
- **Data types**: Structured data annotations (byte/word/long/pointer/string)
- **Source map**: ROM address → source file:line:column mappings
- **Batch APIs**: AddSymbols/AddComments/AddCrossReferences/AddMemoryRegions for efficient export

## Current State (What Works)

### SymbolLoader (import)
- ✅ LoadPansy() imports symbols (name only, not type) and comments (text only, not type)
- ✅ PansyData exposes raw PansyLoader for StaticAnalyzer
- ✅ IsCode/IsData checks HasCodeDataMap → code/data flags
- ✅ GetBankForAddress() uses memory regions
- ✅ GetMemoryRegions() exposes region list

### StaticAnalyzer (classification)
- ✅ Phase 1: CDL code/data + Pansy IsDrawn→Graphics
- ✅ Phase 2: Pansy cross-refs (Jsr/Jmp/Branch→Code, Read/Write→Data)
- ✅ Phase 3: Pansy symbols (Function/InterruptVector/Label→Code, Constant→Data)
- ✅ Phase 4: Pansy memory regions (VRAM→Graphics, IO→Hardware, etc.)
- ✅ Phase 5: ROM vectors → Vector|Pointer classification
- ✅ Phase 6: Operand analysis
- ✅ Phase 7: Platform defaults

### DisassemblyEngine (core)
- ✅ Uses ClassificationResult for IsInDataRegion/ShouldTreatAsCode
- ✅ Queues CDL sub-entry-points and DIZ opcodes as code entry points
- ✅ Uses GetTargetBank() with Pansy memory regions
- ✅ Recursive descent with cross-ref tracking

### SymbolExporter (export)
- ✅ ExportPansy() writes symbols, comments, code/data map, cross-refs, memory regions
- ✅ Uses PansyWriter with compression
- ✅ DetectPansySymbolType() infers Function, InterruptVector, Constant, Local, Label
- ✅ MapCrossRefType() converts Peony→Pansy cross-ref types

## Gaps (What's Missing)

### SymbolLoader — Import Gaps
1. **Symbol types not imported** — LoadPansy() only imports name, ignores SymbolType
2. **Comment types not imported** — Only imports text, ignores CommentType
3. **Jump targets not queued** — Pansy JumpTargets not used as code entry points
4. **Sub-entry-points not queued** — Pansy SubEntryPoints not used as code entry points
5. **Bookmarks not imported** — Not read at all
6. **Data types not imported** — Not read, not converted to DataDefinitions
7. **Source map not imported** — Not read at all
8. **IsRead/IsIndirect flags not exposed** — Not consumed by StaticAnalyzer

### StaticAnalyzer — Classification Gaps
1. **Pansy code/data map direct** — Phase 1 only uses CDL, not Pansy code/data map flags directly
2. **IsRead flag** — Not used (could mark accessed data)
3. **IsIndirect flag** — Not used (could mark indirect jump targets)
4. **IsOpcode flag** — Not used (could refine code byte vs operand byte)
5. **Data types → classification** — Pansy data types could classify regions as Data

### DisassemblyEngine — Entry Point Gaps
1. **Pansy jump targets as entries** — Not queued alongside CDL sub-entries
2. **Pansy sub-entry-points as entries** — Not queued
3. **Pansy data types → DataDefinitions** — Not fed into engine
4. **Pansy bookmarks → labels** — Could auto-generate labels at bookmarked addresses

### SymbolExporter — Export Gaps
1. **No batch API usage** — Adds symbols/comments/cross-refs one by one
2. **No bookmark export** — Bookmarks not written
3. **No data type export** — DataDefinitions not exported as Pansy DataTypeEntry
4. **No source map export** — Source map not written
5. **Typed comments lost on re-export** — All comments become Inline
6. **Typed symbols from import lost** — Original SymbolType not preserved through roundtrip

## Implementation Plan

### Phase 1: SymbolLoader Enhanced Import [8.1]

```
SymbolLoader.cs changes:
- Store typed symbols: Dictionary<uint, SymbolEntry> _typedSymbols
- Import SymbolEntries (with type) alongside plain labels
- Import CommentEntries (with type) alongside plain comments
- Import Pansy JumpTargets as _jumpTargetOffsets
- Import Pansy SubEntryPoints as _subEntryPointOffsets
- Import Pansy Bookmarks
- Import Pansy DataTypes → convert to DataDefinitions
- Expose new properties:
  - IReadOnlyDictionary<uint, SymbolEntry> TypedSymbols
  - IReadOnlyDictionary<uint, CommentEntry> TypedComments
  - IReadOnlySet<int> PansyJumpTargets
  - IReadOnlySet<int> PansySubEntryPoints
  - IReadOnlyList<Bookmark> Bookmarks
  - IReadOnlyList<DataTypeEntry> PansyDataTypes
```

### Phase 2: StaticAnalyzer Pansy Code Map [8.2]

```
StaticAnalyzer.cs changes:
- Add Phase 1b: ApplyPansyCodeDataMap() — uses PansyLoader code/data/jumpTarget/subEntry flags
  - IsCode → Code, IsData → Data
  - IsJumpTarget → Code (mark as jump target)
  - IsSubEntryPoint → Code (mark as subroutine entry)
  - IsRead → Data (mark as accessed data)
  - IsIndirect → Code|Data (indirect reference target)
  - IsDrawn → Graphics (already done in CDL phase, add Pansy-specific)
- Extend Phase 1: Use Pansy IsRead, IsIndirect if available
- Add ClassificationSource.PansyCodeMap
```

### Phase 3: DisassemblyEngine Entry Points [8.3]

```
DisassemblyEngine.cs changes:
- In Disassemble(), after CDL/DIZ entries, add:
  - Queue Pansy JumpTargets as code entry points
  - Queue Pansy SubEntryPoints as code entry points
- Import Pansy DataTypes as DataDefinitions early
- Generate labels at Pansy Bookmark addresses
- Preserve Pansy typed symbols through disassembly
```

### Phase 4: SymbolExporter Batch & New Sections [8.4]

```
SymbolExporter.cs changes:
- Use AddSymbols() batch API instead of per-symbol AddSymbol()
- Use AddComments() batch API with CommentType preservation
- Use AddCrossReferences() batch API
- Use AddMemoryRegions() batch API
- Export Bookmarks from SymbolLoader (roundtrip preservation)
- Export DataDefinitions as Pansy DataTypeEntry section
- Preserve original CommentType through roundtrip
- Preserve original SymbolType through roundtrip (if imported from Pansy)
```

### Phase 5: Tests [8.5]

```
New test files:
- PansyImportEnhancedTests.cs — typed symbols, typed comments, jump targets, sub entries, bookmarks, data types
- PansyExportBatchTests.cs — batch API usage, bookmark/datatype/sourcemap roundtrip
- PansyRoundtripTests.cs — full import→disassemble→export roundtrip with all sections
```

### Phase 6: Documentation [8.6]

```
Updates:
- docs/CDL-PANSY-INTEGRATION.md — new sections, flags, capabilities
- docs/ARCHITECTURE.md — updated pipeline description
- docs/IMPROVING-DISASSEMBLY.md — Pansy-based improvement workflows
- README.md — updated capabilities list
```

## Type Conflict Resolution

Peony.Core has its own `MemoryRegion` (enum) and `CrossRefType` (enum) that conflict with
Pansy.Core's `MemoryRegion` (record) and `CrossRefType` (enum). Resolution:

- Use explicit namespace qualification: `Pansy.Core.MemoryRegion`, `Pansy.Core.CrossRefType`
- Already done in SymbolExporter.cs — extend pattern to new code
- Internal Peony types remain unchanged

## Priority Order

1. **[8.1] SymbolLoader import** — Foundation for everything else
2. **[8.2] StaticAnalyzer code map** — Better classification from Pansy flags
3. **[8.3] DisassemblyEngine entries** — More complete code discovery
4. **[8.4] SymbolExporter batch** — Efficient + complete export
5. **[8.5] Tests** — Verify all integration
6. **[8.6] Documentation** — Record changes

## Acceptance Criteria

- [ ] All Pansy sections consumed on import (symbols, comments, CDL, cross-refs, regions, bookmarks, data types, source map)
- [ ] Pansy code/data map flags used in StaticAnalyzer classification
- [ ] Pansy jump targets and sub-entry-points queued as code entry points
- [ ] Pansy data types converted to DataDefinitions
- [ ] Pansy bookmarks imported and labels generated
- [ ] SymbolExporter uses batch APIs for efficiency
- [ ] Bookmarks, data types roundtrip through export
- [ ] All 171+ existing tests still pass
- [ ] New tests cover all integration points
- [ ] Documentation updated
