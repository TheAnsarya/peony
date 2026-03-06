# Peony Static Analysis Architecture

## Overview

Replace percentage-based/heuristic data classification with **deterministic static analysis** that leverages all available information sources:

1. **CDL (Code/Data Log)** — From Nexen emulator runtime tracing
2. **Pansy metadata** — Symbols, comments, cross-refs, memory regions, CDL flags
3. **Label files** — .nexen-labels, .mlb, .nl symbol definitions
4. **ROM structure** — Headers, vectors, known memory maps
5. **Assembly instruction knowledge** — Opcode semantics, control flow, data references
6. **Poppy compatibility** — Output must reassemble identically via Poppy

## Current Problems

The current `DataDetector.cs` uses heuristic percentage-based classification:
- `IsLikelyGraphics()` — scores byte patterns, returns true if score > 50%
- `IsLikelyPointerTable()` — checks if > 50% of words point to ROM space
- `IsLikelyText()` — checks if > 70% of bytes are "printable"
- `ClassifyByte()` — uses a 16-byte window to guess classification

**Problems with this approach:**
- False positives: code regions classified as data
- False negatives: data regions disassembled as code
- No integration with CDL/Pansy data which is **authoritative**
- Percentage thresholds are arbitrary and platform-dependent
- No use of instruction semantics to trace data references

## Proposed Architecture

### Phase 1: CDL/Pansy-Driven Classification (Authoritative)

CDL data from Nexen is the most reliable source — it records which bytes were actually executed as code and which were read as data during emulation.

```text
Priority cascade:
1. CDL flags (CODE/DATA/DRAWN) — highest confidence
2. Pansy cross-references — show what references what
3. Pansy symbols — user/tool-defined labels with types
4. Pansy memory regions — named regions with types (ROM/RAM/VRAM)
5. ROM vectors — entry points from interrupt/reset vectors
6. Instruction operand analysis — static data refs from code
7. Pattern analysis — LAST resort, only for unclassified regions
```

### Phase 2: Instruction Operand Analysis

For regions identified as code (by CDL or recursive descent), analyze instruction operands to find data references:

```text
LDA $8200    → $8200 is likely DATA (read target)
STA $2007    → $2007 is HARDWARE (PPU register)
JSR $c050    → $c050 is CODE (call target)
JMP ($fffc)  → $fffc is POINTER (indirect jump vector)
LDA #$ff     → immediate, no memory reference

For each data reference found:
- Check if target is in ROM space → mark as DATA in classification map
- Check if target is in known memory map → annotate with region type
- Check if target is a hardware register → annotate with register name
- Follow pointer chains for indirect references
```

### Phase 3: Platform-Specific Knowledge

Each platform has known memory layouts that provide deterministic classification:

**NES:**
- `$0000-$07ff` — RAM (2KB, mirrored)
- `$2000-$2007` — PPU registers (mirrored every 8 bytes to $3fff)
- `$4000-$4017` — APU/IO registers
- `$4020-$5fff` — Expansion ROM
- `$6000-$7fff` — SRAM (battery-backed)
- `$8000-$ffff` — PRG ROM (where code/data lives)
- `$fffa-$fffb` — NMI vector (always a pointer)
- `$fffc-$fffd` — Reset vector (always a pointer)
- `$fffe-$ffff` — IRQ vector (always a pointer)

**CHR data** is separate in NES (CHR ROM/RAM) — never mixed with PRG. If CDL has DRAWN flag, it's graphics.

### Phase 4: Cross-Reference Graph

Build a complete cross-reference graph from all sources:

```text
Sources:
1. Pansy cross-references (authoritative, from emulator)
2. Instruction operand analysis (computed, from static analysis)
3. Pointer table detection (computed, from data analysis)

Uses:
- Dead code detection: code with no incoming references
- Data table boundaries: where references point
- Function boundaries: JSR targets = function starts
- Jump table detection: indirect JMP with preceding pointer table
```

## Nexen Integration for Runtime Data

### IPC Protocol

Peony can connect to a running Nexen instance to query runtime state:

```text
Nexen Debug API (via DLL exports):
- GetMemoryState(type) → dump any memory type
- GetCdlData(offset, length, type) → CDL flags for memory range
- GetPpuState(state, cpuType) → PPU register state
- GetTileView() → rendered tile data
- GetTilemap() → tilemap layout
- GetSpriteList() → sprite OAM data
- GetPaletteInfo() → palette colors
- SaveCdlFile() → export CDL to file
- GetMemoryAccessCounts() → read/write/exec counts per address
```

### Integration Approaches

**Option A: File-based Exchange (Simplest)**
1. Nexen exports `.nexen-pack.zip` with ROM + CDL + Pansy + labels
2. Peony loads pack via `NexenPackLoader` (already implemented)
3. Peony disassembles using all metadata
4. This is already partially working — enhance it

**Option B: Shared Memory / Named Pipe (Real-time)**
1. Nexen opens a named pipe server when debugger is active
2. Peony connects as client
3. Protocol: request/response for memory dumps, CDL, PPU state
4. Enables live disassembly updates as user plays

**Option C: REST API (Cross-process)**
1. Nexen hosts a minimal HTTP server on localhost (debug mode only)
2. Peony queries endpoints for memory state
3. Most portable but highest overhead

**Recommended: Option A first, then Option B for advanced features.**

## VS Code Extension Integration

The Poppy VS Code extension already provides `.pasm` file support. Peony's output is `.pasm` files, so:

1. **Syntax highlighting** — already handled by Poppy extension
2. **Semantic tokens** — Peony could emit metadata annotations for:
   - Code vs data regions (different coloring)
   - Cross-reference info (hover shows callers/callees)
   - Memory region types (ROM/RAM/VRAM indicators)
3. **Go to definition** — labels resolved from Pansy symbols
4. **Find references** — from Pansy cross-reference data
5. **Diagnostics** — warn about unresolved labels, suspicious disassembly

### Peony Language Server

A Peony LSP server could provide:
- Live disassembly view connected to Nexen
- Navigate by address, label, or cross-reference
- VRAM viewer integrated into VS Code panel
- Memory map visualization
- CDL coverage overlay on disassembly

## Implementation Phases

### Phase 1: Refactor DataDetector → StaticAnalyzer
- Remove percentage-based heuristics
- Add CDL-first classification
- Add instruction operand data reference tracking
- Add platform memory map knowledge
- **Issues:** #4 (research), #5 (implement), #6 (tests)

### Phase 2: Enhanced Nexen Pack Integration
- Improve `NexenPackLoader` to extract more metadata
- Add Nexen → Peony CDL/Pansy pipeline automation
- Add memory access count analysis for hot path detection
- **Issues:** #7 (research), #8 (implement)

### Phase 3: Nexen IPC Connection
- Named pipe protocol for live debug data exchange
- Real-time CDL streaming during emulation
- VRAM/PPU state queries for graphics analysis
- **Issues:** #9 (protocol design), #10 (Nexen server), #11 (Peony client)

### Phase 4: VS Code Extension / LSP
- Peony language server for `.pasm` navigation
- Cross-reference hover/go-to-definition
- Memory map visualization panel
- CDL coverage overlay
- **Issues:** #12+ (multiple sub-issues)

## File Changes

### Remove / Replace
- `DataDetector.cs` → `StaticAnalyzer.cs` (complete rewrite)

### New Files
- `StaticAnalyzer.cs` — CDL/instruction-driven classification
- `InstructionAnalyzer.cs` — Operand data reference extraction
- `MemoryMap.cs` — Platform memory map knowledge base
- `CrossRefGraph.cs` — Cross-reference graph builder
- `NexenConnection.cs` — IPC client for live Nexen data (Phase 3)

### Modified Files
- `DisassemblyEngine.cs` — Use StaticAnalyzer instead of DataDetector
- `Interfaces.cs` — Add new types for analysis results
- `SymbolLoader.cs` — Enhanced CDL/Pansy integration
