# Pansy Integration Phase 2 — Deep Pipeline Integration

## Overview

Phase 1 (Epic #76) established foundational Pansy data roundtrip — importing typed symbols/comments/bookmarks/data types and preserving them through export. Phase 2 focuses on making the disassembler **actively use** all Pansy data to produce better disassembly output. No heuristics or entropy analysis — only real information from Pansy metadata drives decisions.

## Gaps Identified

### 1. CLI `disasm` Missing `--pansy` Flag (HIGH)
**File:** `Program.cs` (disasm handler, lines 15-190)
**Problem:** The `disasm` command supports `--symbols`, `--cdl`, `--diz` but has NO `--pansy` option. Users cannot provide a Pansy file during disassembly.
**Solution:** Add `--pansy/-y` option. Load Pansy file via `SymbolLoader.LoadPansy()`. Report stats (symbols, comments, CDL coverage, cross-refs, etc.). Combine Pansy entry points with platform + CDL entries. Also add `--pansy` to the `export` command.

### 2. Pansy Cross-Reference Entry Point Discovery (HIGH)
**File:** `SymbolLoader.cs`, `DisassemblyEngine.cs`
**Problem:** SymbolLoader imports Pansy jump targets and sub-entry-points from the code/data map, but does NOT import cross-reference data. Cross-reference JSR/JMP targets are confirmed code locations that should be queued as entry points.
**Solution:** Import Pansy cross-references in `ImportPansyData()`. In `DisassemblyEngine.Disassemble()`, queue cross-ref JSR/JMP/Branch targets as code entry points — these are definitive, not heuristic.

### 3. PoppyFormatter Typed Comment Support (MEDIUM)
**File:** `PoppyFormatter.cs` (FormatLine, line ~90)
**Problem:** All comments treated identically — Block/Todo/Inline all render inline after instruction bytes.
**Solution:**
- **Block** comments → above the instruction line (as separate comment lines)
- **Todo** comments → `; TODO: <text>` prefix
- **Inline** comments → after instruction bytes (current behavior)

### 4. PoppyFormatter Data Region Formatting (MEDIUM)
**File:** `PoppyFormatter.cs` (FormatLine)
**Problem:** Data regions have no special formatting. `DataDefinition` type/count not used for `.db`/`.dw`/`.dl` directives.
**Solution:** When a line is in a data block (block.Type != Code), format using:
- `byte` → `.db $xx, $xx, ...`
- `word` → `.dw $xxxx, $xxxx, ...`
- `long` → `.dl $xxxxxx, ...`
- `text` → `.dt "string"` (if printable)

### 5. Enhanced Code/Data Map Roundtrip (LOW)
**File:** `SymbolExporter.cs` (PopulateCodeDataMap)
**Problem:** Only marks CODE, OPCODE, DATA from blocks. Does NOT preserve DRAWN, READ, INDIRECT flags through roundtrip.
**Solution:** If `_symbolLoader.PansyData` has code/data map flags at an offset, preserve the original flags (DRAWN/READ/INDIRECT) alongside the new CODE/DATA markings.

### 6. Pansy-Aware Pointer Table Detection (LOW)
**File:** `DisassemblyEngine.cs` (DetectPointerTables)
**Problem:** Uses heuristic "3+ consecutive words pointing to known code" to detect pointer tables.
**Solution:** If Pansy DataTypeEntries with `Pointer` type exist, use them directly instead of heuristic detection. Only fall back to heuristic for addresses not covered by Pansy data.

## Implementation Order

1. **CLI --pansy flag** — Immediate usability improvement
2. **Cross-ref entry point discovery** — Better disassembly coverage using real data
3. **PoppyFormatter typed comments** — Better output quality
4. **PoppyFormatter data formatting** — Better output quality
5. **Enhanced code/data map roundtrip** — Better metadata preservation
6. **Pansy-aware pointer tables** — Better data detection

## Constraints

- **FORBIDDEN:** Entropy analysis, percentage heuristics, statistical guessing
- **REQUIRED:** All decisions must use real Pansy data (code maps, cross-refs, typed symbols)
- **TEST:** All 923+ tests must continue to pass
- **BUILD:** Zero warnings in Release build
