# 🌺 Peony Disassembly Workflow — Step by Step

> How Peony disassembles a ROM from start to finish, and how to get the best results.

## Overview

Peony is a **static disassembler** — it analyzes ROM binaries and produces readable assembly source code (`.pasm` files) that can be reassembled by Poppy into identical ROMs.

The quality of disassembly depends heavily on **input hints** — CDL logs, Pansy metadata, symbol files, and other analysis data from emulators and tools.

---

## Step 1: Gather Input Files

Before running Peony, collect as many analysis files as possible for your ROM:

### Required

| File | Description |
|------|-------------|
| **ROM file** | The binary ROM image (`.nes`, `.sfc`, `.gb`, `.gba`, `.a26`, `.lnx`) |

### Recommended (Greatly Improves Output)

| File | Source | Description |
|------|--------|-------------|
| **CDL file** (`.cdl`) | Nexen, Mesen2, FCEUX | Per-byte CODE/DATA classification from emulator runtime |
| **Pansy file** (`.pansy`) | Nexen, Poppy, Peony | Comprehensive metadata: symbols, comments, code maps, cross-refs, bookmarks |
| **Symbol file** (`.mlb`, `.nl`, `.sym`, `.json`) | Various tools | Named labels for addresses |

### Optional

| File | Source | Description |
|------|--------|-------------|
| **DIZ file** (`.diz`) | DiztinGUIsh | XML project with symbols, comments, data annotations |
| **Table file** (`.tbl`) | Manual creation | Character encoding tables for text extraction |

### How to Generate CDL Files

1. **Nexen/Mesen2**: Load ROM → Play game thoroughly → Tools → CDL → Save CDL
2. **FCEUX**: Load ROM → Debug → Code/Data Logger → Start → Play → Save
3. **bsnes**: Load ROM → Usage maps are auto-generated

### How to Generate Pansy Files

1. **Nexen**: Load ROM → Tools → Export Pansy Metadata
2. **Poppy**: Compile a project with `--pansy output.pansy`
3. **Peony**: Re-export from a previous disassembly with `peony export`

> **Tip:** The more you play the game in an emulator with CDL logging enabled, the better the disassembly. CDL tells Peony definitively which bytes are code vs. data — without it, Peony must guess.

---

## Step 2: Run Peony Disassembly

### Basic Usage

```bash
# Minimal — auto-detect platform, output to stdout
peony disasm game.nes

# With output file
peony disasm game.nes -o game.pasm

# With CDL hints (recommended)
peony disasm game.nes --cdl game.cdl -o game.pasm

# With Pansy metadata (best results)
peony disasm game.nes --pansy game.pansy --cdl game.cdl -o game.pasm

# Full options — all available hints
peony disasm game.nes \
    --cdl game.cdl \
    --pansy game.pansy \
    --symbols game.mlb \
    --diz game.diz \
    --all-banks \
    -o game.pasm
```

### Platform-Specific Examples

```bash
# NES with iNES header auto-detection
peony disasm zelda.nes --cdl zelda.cdl -o zelda.pasm

# SNES with explicit platform
peony disasm ffmq.sfc -p snes --cdl ffmq.cdl -o ffmq.pasm

# Game Boy
peony disasm pokemon.gb --cdl pokemon.cdl -o pokemon.pasm

# GBA (large ROMs)
peony disasm advance.gba -o advance.pasm

# Atari 2600 (small ROMs)
peony disasm pitfall.a26 -o pitfall.pasm

# Atari Lynx
peony disasm comlynx.lnx -o comlynx.pasm
```

---

## Step 3: What Happens Inside

When you run `peony disasm`, the following pipeline executes:

### 3.1 — ROM Loading

```
ROM File → RomLoader.Load()
    ├─ Read raw bytes
    ├─ Detect platform from file extension + header magic
    ├─ Parse ROM header (iNES, SNES cartridge, GB header, etc.)
    └─ Return RomInfo (size, platform, mapper, CRC32, etc.)
```

### 3.2 — Platform Analysis

```
RomInfo → PlatformAnalyzer.Analyze()
    ├─ Parse platform-specific header structure
    ├─ Identify memory mapping mode (LoROM, HiROM, mappers, etc.)
    ├─ Extract hardware entry points (RESET, NMI, IRQ vectors)
    ├─ Determine bank count and addressing scheme
    └─ Return entry points + memory map configuration
```

### 3.3 — Hint Loading

```
CDL/Pansy/Symbols/DIZ → SymbolLoader
    ├─ CDL: Mark bytes as CODE/DATA/JUMP_TARGET/SUB_ENTRY
    ├─ Pansy: Import symbols, comments, data types, cross-refs, bookmarks
    ├─ Symbols: Load named labels from various formats
    ├─ DIZ: Load DiztinGUIsh project data
    └─ Merge all hints into unified analysis state
```

### 3.4 — Disassembly Engine (Recursive Descent)

```
DisassemblyEngine.Disassemble()
    │
    ├─ Phase 1: Entry Point Collection
    │  ├─ Platform vectors (RESET, NMI, IRQ)
    │  ├─ CDL sub-entry-point marks
    │  ├─ DIZ opcode-marked addresses
    │  ├─ Pansy jump target marks
    │  └─ Pansy cross-reference JSR/JMP/Branch targets
    │
    ├─ Phase 2: Recursive Descent
    │  ├─ Dequeue next entry point
    │  ├─ Decode instruction using CPU decoder
    │  ├─ Check CDL: Is this address marked as DATA? → stop
    │  ├─ Record cross-reference (call/jump/branch)
    │  ├─ Queue new targets (JSR → sub_*, JMP → loc_*)
    │  ├─ Continue until unconditional jump/return
    │  └─ Repeat until queue is empty
    │
    ├─ Phase 3: Block Building
    │  ├─ Group visited addresses into contiguous code blocks
    │  ├─ Mark unvisited ranges as data blocks
    │  ├─ Apply data region definitions (Pansy DataTypes)
    │  └─ Assign blocks to banks (for banked ROMs)
    │
    ├─ Phase 4: Pointer Table Detection
    │  ├─ Phase 4a: Pansy Pointer-type DataTypeEntries (authoritative)
    │  └─ Phase 4b: Heuristic scan (3+ consecutive pointers → known code)
    │
    └─ Return DisassemblyResult
        ├─ Blocks (code + data)
        ├─ Labels (auto-generated + imported)
        ├─ Comments (imported + generated)
        ├─ Cross-references
        ├─ Data regions
        └─ Pansy roundtrip data (typed symbols/comments/bookmarks)
```

### 3.5 — Output Generation

```
DisassemblyResult → PoppyFormatter.Generate()
    ├─ Write file header (platform, origin, config)
    ├─ For each block:
    │  ├─ Code blocks → format as assembly instructions
    │  │  ├─ Block comments (above the line)
    │  │  ├─ Labels
    │  │  ├─ Instruction mnemonic + operand
    │  │  └─ Inline comments / Todo comments
    │  └─ Data blocks → format as data directives
    │     ├─ .db (bytes), .dw (words), .dl (longs)
    │     ├─ .dt (text) for printable ASCII strings
    │     └─ Named labels from DataDefinitions
    └─ Write to .pasm output file
```

---

## Step 4: Review and Improve Output

The initial disassembly is a starting point. To improve it:

### 4.1 — Export Symbols for Refinement

```bash
# Export to Pansy format for roundtrip editing
peony export game.nes --pansy game.pansy -f pansy -o game-analysis.pansy

# Export to Mesen format for use in Nexen
peony export game.nes -f mesen -o game.mlb
```

### 4.2 — Iterative Refinement Cycle

```
1. Disassemble with Peony → game.pasm
2. Load game.pasm in editor, identify issues
3. Play game in Nexen with CDL logging → better CDL
4. Add labels/comments in Nexen → export .pansy/.mlb
5. Re-disassemble with updated hints → improved game.pasm
6. Repeat until satisfied
```

### 4.3 — Verify Roundtrip

```bash
# Assemble with Poppy
poppy build game.pasm -o game-rebuilt.nes

# Verify identical output
peony verify game.nes --reassembled game-rebuilt.nes --report report.txt
```

---

## Step 5: Build with Poppy

Once the disassembly is complete, you can use Poppy to reassemble:

```bash
# Create a Poppy project (poppy.json) in the output directory
# Then build:
poppy build --project poppy.json

# Or directly:
poppy build game.pasm -o game.nes -p nes
```

---

## Supported Platforms

| Platform | CPU | ROM Extensions | Bank Support | Asset Extraction |
|----------|-----|----------------|--------------|------------------|
| NES | MOS 6502 | `.nes`, `.unf` | Yes (mapper-based) | CHR tiles, text |
| SNES | WDC 65816 | `.sfc`, `.smc` | Yes (LoROM/HiROM) | — |
| Game Boy | Sharp SM83 | `.gb`, `.gbc` | Yes (MBC mappers) | — |
| GBA | ARM7TDMI | `.gba` | No (flat 32MB) | — |
| Atari 2600 | MOS 6507 | `.a26`, `.bin` | Limited (bank switching) | — |
| Atari Lynx | WDC 65SC02 | `.lnx` | Yes (cart banks) | — |

---

## Input Quality Guide

| Hints Available | Expected Quality | Notes |
|----------------|------------------|-------|
| ROM only | Poor | Many data/code misclassifications |
| ROM + CDL | Good | Accurate code/data separation |
| ROM + CDL + Symbols | Very Good | Named labels + accurate classification |
| ROM + CDL + Pansy | Excellent | Full metadata: symbols, comments, cross-refs, data types |
| ROM + CDL + Pansy + DIZ | Best | All available information combined |

> **Rule:** More input data = better disassembly. Always provide as many hint files as possible.

---

## Quick Reference

```bash
# One-liner: Best quality disassembly
peony disasm game.nes --cdl game.cdl --pansy game.pansy --all-banks -o game.pasm

# Export analysis for later refinement
peony export game.nes --cdl game.cdl --pansy game.pansy -f pansy -o analysis.pansy

# View ROM info
peony info game.nes

# Batch disassemble a folder
peony batch ./roms/ -o ./output/ -f poppy

# Verify roundtrip integrity
peony verify game.nes --reassembled rebuilt.nes
```
