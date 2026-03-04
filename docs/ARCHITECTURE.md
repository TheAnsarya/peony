# 🌺 Peony Disassembler — Architecture & Design

> **Comprehensive technical documentation** for understanding, correcting, and improving the Peony disassembler.

## Table of Contents

1. [Overview](#overview)
2. [High-Level Architecture](#high-level-architecture)
3. [Disassembly Pipeline](#disassembly-pipeline)
4. [CDL & Pansy Integration](#cdl--pansy-integration)
5. [Multi-Bank Architecture](#multi-bank-architecture)
6. [Project Structure](#project-structure)
7. [Sub-Documents](#sub-documents)

---

## Overview

**Peony** is a multi-system disassembler that converts ROM binary files back into assembly source code (`.pasm` files) that can be reassembled to identical ROMs using the **Poppy** assembler. It supports multiple CPU architectures and gaming platforms.

### Core Guarantee

**Roundtrip Fidelity:** `Original ROM → Peony disassembly → Poppy assembly → Identical ROM`

This guarantee is the #1 engineering constraint — every feature and optimization must preserve byte-for-byte output correctness.

### Supported Systems

| Platform        | CPU Architecture | Decoder Project      | Analyzer Project         |
|-----------------|------------------|----------------------|--------------------------|
| NES             | MOS 6502         | Peony.Cpu.6502       | Peony.Platform.NES       |
| SNES            | WDC 65816        | Peony.Cpu.65816      | Peony.Platform.SNES      |
| Game Boy        | Sharp SM83       | Peony.Cpu.GameBoy    | Peony.Platform.GameBoy   |
| Game Boy Advance| ARM7TDMI         | Peony.Cpu.ARM7TDMI   | Peony.Platform.GBA       |
| Atari 2600      | MOS 6507 (6502)  | Peony.Cpu.6502       | Peony.Platform.Atari2600 |
| Atari Lynx      | WDC 65SC02       | Peony.Cpu.65SC02     | Peony.Platform.Lynx      |

---

## High-Level Architecture

Peony uses a layered **plugin architecture** with three abstraction interfaces:

```
┌──────────────────────────────────────────────────────────────┐
│                    CLI (Program.cs)                           │
│         disasm  │  batch  │  info  │  export  │  verify      │
├──────────────────────────────────────────────────────────────┤
│                DisassemblyEngine                             │
│          (Recursive Descent + CDL/Pansy Hints)               │
├──────────┬───────────────┬───────────────┬───────────────────┤
│          │               │               │                   │
│  ICpuDecoder      IPlatformAnalyzer    IOutputFormatter      │
│  ─────────        ─────────────────    ────────────────      │
│  6502             NES                  PoppyFormatter         │
│  65816            SNES                 (ASM internal)         │
│  65SC02           Game Boy                                    │
│  ARM7TDMI         GBA                                         │
│  SM83             Atari 2600                                  │
│                   Lynx                                        │
├──────────────────────────────────────────────────────────────┤
│  Input Loaders                   │  Output Exporters          │
│  ──────────────                  │  ────────────────          │
│  CdlLoader     (.cdl)           │  SymbolExporter             │
│  SymbolLoader  (.mlb/.nl/.sym)  │    .mlb, .nl, .sym          │
│  DizLoader     (.diz)           │    .dbg, .wla, .cht         │
│  PansyLoader   (.pansy)         │    .pansy (via PansyWriter)  │
├──────────────────────────────────────────────────────────────┤
│  Extractors (optional analysis passes)                       │
│  TextExtractor │ DataTableExtractor │ GraphicsExtractor       │
│  DataDetector  │ DictionaryTextCompressor                     │
├──────────────────────────────────────────────────────────────┤
│  RoundtripVerifier  (disassemble → assemble → compare)       │
└──────────────────────────────────────────────────────────────┘
```

### Three Core Interfaces

1. **`ICpuDecoder`** — Decodes binary instructions into mnemonic + operand + addressing mode. Pure CPU-level logic with no platform knowledge.

2. **`IPlatformAnalyzer`** — Provides platform-specific context: memory maps, hardware register names, entry points (interrupt vectors), address-to-file-offset translation, bank switching detection.

3. **`IOutputFormatter`** — Generates output files from `DisassemblyResult`. Currently: `PoppyFormatter` for `.pasm` files.

See [INTERFACES.md](INTERFACES.md) for complete API documentation.

---

## Disassembly Pipeline

The full disassembly pipeline from ROM file to `.pasm` output:

```
    ROM File (.nes, .sfc, .a26, .lnx, .gba, .gb)
         │
         ▼
    ┌─────────────┐
    │  RomLoader   │  Detect platform, strip copier headers,
    │              │  keep format headers (iNES, LNX, etc.)
    └──────┬──────┘
           │
           ▼
    ┌──────────────────┐
    │ PlatformAnalyzer  │  Parse ROM header (mapper, banks, regions)
    │                   │  Extract entry points (RESET, NMI, IRQ vectors)
    │                   │  Build memory map
    └──────┬───────────┘
           │
    ┌──────┼───────────────────────────┐
    │      ▼                           │
    │ ┌─────────────┐                  │  Optional hint sources:
    │ │ CdlLoader   │ ← .cdl file     │  - CDL: code/data byte flags
    │ ├─────────────┤                  │  - Pansy: symbols + CDL + regions
    │ │SymbolLoader │ ← .pansy/.mlb   │  - DIZ: DiztinGUIsh projects
    │ ├─────────────┤                  │  - Symbol files: .nl/.sym/.json
    │ │ DizLoader   │ ← .diz file     │
    │ └──────┬──────┘                  │
    │        │                         │
    └────────┼─────────────────────────┘
             │
             ▼
    ┌────────────────────────────────────┐
    │       DisassemblyEngine            │
    │                                    │
    │  1. Seed code queue with:          │
    │     - Vector entry points          │
    │     - CDL subroutine entries       │
    │     - DIZ code-marked addresses    │
    │     - Pansy symbol addresses       │
    │                                    │
    │  2. Recursive descent:             │
    │     While queue not empty:         │
    │       Dequeue (address, bank)      │
    │       Decode instruction           │
    │       Check CDL (is it data?)      │
    │       Record cross-references      │
    │       Queue branch targets         │
    │       Stop block on unconditional  │
    │         jump or data region        │
    │                                    │
    │  3. Label management:              │
    │     - User labels (protected)      │
    │     - Auto: sub_XXXX, loc_XXXX     │
    │     - Bank-specific labels         │
    │                                    │
    │  4. Data region classification     │
    │  5. Cross-reference aggregation    │
    └──────────┬─────────────────────────┘
               │
               ▼
    ┌──────────────────┐
    │ DisassemblyResult │  Contains: Blocks, Labels, Comments,
    │                   │  CrossRefs, DataRegions, BankBlocks
    └──────────┬───────┘
               │
        ┌──────┴──────┐
        ▼             ▼
  ┌───────────┐ ┌──────────────┐
  │  Poppy    │ │ Symbol       │
  │ Formatter │ │ Exporter     │
  │ (.pasm)   │ │ (.pansy/etc) │
  └───────────┘ └──────────────┘
```

See [DISASSEMBLY-ENGINE.md](DISASSEMBLY-ENGINE.md) for the detailed algorithm.

---

## CDL & Pansy Integration

### What is CDL?

A **Code/Data Log** (CDL) is a byte-per-byte map of a ROM file where each byte has flags indicating whether it was executed as code, read as data, or accessed as a jump/call target during emulation. CDL files are generated by running a game in an emulator (like Nexen/Mesen).

### CDL Flag Definitions

| Bit | Flag             | Meaning                                          |
|-----|------------------|--------------------------------------------------|
| 0   | `CODE` (0x01)    | Byte was executed as code (opcode or operand)    |
| 1   | `DATA` (0x02)    | Byte was read as data                            |
| 2   | `JUMP_TARGET` (0x04) | Address is the target of a branch/jump       |
| 3   | `SUB_ENTRY` (0x08)   | Address is a subroutine entry point           |
| 4   | `OPCODE` (0x10)  | Byte is the first byte of an instruction         |
| 5   | `DRAWN` (0x20)   | Byte was read by the PPU (graphics data)         |
| 6   | `READ` (0x40)    | Byte was read by any source                      |
| 7   | `INDIRECT` (0x80)| Byte was accessed via indirect addressing        |

### CDL Formats Supported

| Format    | Header               | Source Emulator        |
|-----------|----------------------|------------------------|
| FCEUX     | (none — raw bytes)   | FCEUX NES emulator     |
| Mesen     | `"CDL\x01"` (4 bytes)| Mesen NES emulator     |
| Mesen2    | `"CDLv2"` + CRC32    | Mesen2 / Nexen         |
| bsnes     | (flag pattern detect) | bsnes SNES emulator    |

### How CDL Improves Disassembly

Without CDL, the disassembler can only follow code from known entry points (interrupt vectors). This misses:

- **Callback functions** called via indirect jumps (`JMP ($addr)`)
- **Data tables** that look like valid instructions
- **Unreachable code** behind conditional branches not taken during analysis

CDL provides ground truth from actual emulator execution:

```
Without CDL:                    With CDL:
─────────────                   ──────────
$8000: lda #$00    ✓ found      $8000: lda #$00    ✓ found
$8002: sta $2000   ✓ found      $8002: sta $2000   ✓ found
$8005: jmp ($FFFE) ✓ found      $8005: jmp ($FFFE) ✓ found
$8008: .db $A9     ✗ missed     $8008: lda #$42    ✓ CDL: CODE+OPCODE
$800A: .db $42                  $800A:              ✓ CDL: CODE (operand)
$800B: .db $FF                  $800B: .db $FF     ✓ CDL: DATA
```

### What is Pansy?

**Pansy** (Program ANalysis SYstem) is a binary metadata format that extends CDL with rich type information:

| Section          | ID     | Contents                                           |
|------------------|--------|----------------------------------------------------|
| Code/Data Map    | 0x0001 | Per-byte CDL flags (same as CDL but in Pansy file) |
| Symbols          | 0x0002 | Address → Name + Type (Label, Function, Constant)  |
| Comments         | 0x0003 | Address → Text + Type (Inline, Block, Todo)        |
| Memory Regions   | 0x0004 | Named regions with types (ROM, RAM, VRAM, IO)      |
| Cross-References | 0x0006 | From/To address pairs (Jsr, Jmp, Branch, Read)     |
| Metadata         | 0x0008 | Project name, author, version, timestamps          |

### How Pansy Improves Disassembly

Pansy provides everything CDL does PLUS:

1. **Named symbols** — Instead of auto-generated `sub_8042`, use emulator-discovered names
2. **Comments** — Preserve analysis notes from the emulator session
3. **Memory regions** — Know which addresses are ROM, RAM, VRAM, I/O registers
4. **Cross-references** — Pre-computed call graph from emulator analysis
5. **Bank information** — Memory regions include bank numbers for multi-bank ROMs

### Integration Flow

```
Nexen Emulator Session:
  1. Load ROM, run game
  2. Background CDL recording marks code/data bytes
  3. User adds labels/comments in debugger
  4. Export → .nexen-pack.zip containing:
     ├── ROM/game.nes
     ├── Debug/game.cdl
     ├── Debug/game.pansy
     └── Debug/game.nexen-labels

Peony Disassembly:
  1. peony disasm game.nes --cdl game.cdl --symbols game.pansy
  2. CdlLoader reads .cdl: marks code/data offsets, jump targets, sub entries
  3. SymbolLoader reads .pansy: loads symbols, comments, regions, cross-refs
  4. DisassemblyEngine uses CDL to:
     a. Seed additional entry points (SUB_ENTRY flags)
     b. Stop disassembly at DATA boundaries
     c. Validate code vs data classification
  5. DisassemblyEngine uses Pansy to:
     a. Import user-defined labels (protected from overwrite)
     b. Import comments
     c. Use memory regions for bank detection
     d. Pre-populate cross-reference data
  6. Output: game.pasm with rich annotations
```

See [CDL-PANSY-INTEGRATION.md](CDL-PANSY-INTEGRATION.md) for detailed format specs and usage.

---

## Multi-Bank Architecture

Most retro consoles have address spaces smaller than the ROM size. **Bank switching** maps different segments of the ROM into the CPU's visible address space at runtime.

### How Peony Handles Banks

All internal data structures use `(address, bank)` tuples:

```csharp
// The same CPU address $8000 can appear in multiple banks
Dictionary<(uint Address, int Bank), string> _bankLabels;
Dictionary<(uint Address, int Bank), bool> _visited;
Queue<(uint Address, int Bank)> _codeQueue;
```

### Platform-Specific Banking

| Platform | Fixed Region     | Switchable Region | Bank Size |
|----------|------------------|-------------------|-----------|
| NES NROM | $8000-$FFFF (all)| None              | 16/32 KB  |
| NES MMC1 | $C000-$FFFF      | $8000-$BFFF       | 16 KB     |
| SNES     | (none — 24-bit)  | All (direct)       | 32/64 KB  |
| Game Boy | $0000-$3FFF      | $4000-$7FFF        | 16 KB     |
| GBA      | (32-bit linear)  | All                 | 32 KB     |

### Bank-Aware Disassembly

When the engine encounters a branch to a switchable address:

1. Determine which bank the target is in (via Pansy regions, CDL context, or platform heuristics)
2. Create a bank-specific label: `bank3_sub_8042` vs `bank5_sub_8042`
3. Queue the target with its bank number: `(0x8042, bank=3)`
4. Track visited separately per bank

See [MULTI-BANK.md](MULTI-BANK.md) for per-platform banking details.

---

## Project Structure

```
peony/
├── src/
│   ├── Peony.Core/                  # Core library
│   │   ├── Interfaces.cs            # ICpuDecoder, IPlatformAnalyzer, IOutputFormatter
│   │   ├── DisassemblyEngine.cs     # Main recursive descent engine
│   │   ├── RomLoader.cs             # ROM loading + platform detection
│   │   ├── CdlLoader.cs             # CDL file loading (FCEUX/Mesen/Mesen2/bsnes)
│   │   ├── SymbolLoader.cs          # Multi-format symbol/label loading
│   │   ├── SymbolExporter.cs        # Multi-format symbol/label export
│   │   ├── PoppyFormatter.cs        # .pasm output generation
│   │   ├── RoundtripVerifier.cs     # Disassemble → assemble → compare
│   │   ├── DizLoader.cs             # DiztinGUIsh project loader
│   │   ├── DataDetector.cs          # Code vs data classification heuristics
│   │   ├── TextExtractor.cs         # ROM text extraction with .tbl files
│   │   ├── DataTableExtractor.cs    # Structured data table extraction
│   │   ├── GraphicsExtractor.cs     # Tile graphics decoding
│   │   └── DictionaryTextCompressor.cs  # DTE/MTE text compression
│   │
│   ├── Peony.Cli/                   # Command-line interface
│   │   └── Program.cs               # CLI commands: disasm, batch, info, export, verify
│   │
│   ├── Peony.Cpu.6502/              # MOS 6502 decoder (NES, Atari 2600)
│   ├── Peony.Cpu.65816/             # WDC 65816 decoder (SNES)
│   ├── Peony.Cpu.65SC02/            # WDC 65SC02 decoder (Atari Lynx)
│   ├── Peony.Cpu.ARM7TDMI/          # ARM7TDMI decoder (GBA)
│   ├── Peony.Cpu.SM83/              # Sharp SM83 decoder (Game Boy)
│   ├── Peony.Cpu.GameBoy/           # Legacy Game Boy decoder
│   │
│   ├── Peony.Platform.NES/          # NES: iNES header, PPU/APU registers, MMC1
│   ├── Peony.Platform.SNES/         # SNES: LoRom/HiRom/ExRom, PPU/APU registers
│   ├── Peony.Platform.GB/           # Game Boy platform
│   ├── Peony.Platform.GBA/          # GBA platform
│   ├── Peony.Platform.Atari2600/    # Atari 2600: TIA/RIOT, mirrored $F000-$FFFF
│   └── Peony.Platform.Lynx/         # Atari Lynx: LNX header, Suzy/Mikey
│
├── tests/
│   ├── Peony.Core.Tests/            # Core unit tests
│   ├── Peony.Cpu.6502.Tests/
│   ├── Peony.Platform.Atari2600.Tests/
│   ├── Peony.Platform.GameBoy.Tests/
│   ├── Peony.Platform.GBA.Tests/
│   ├── Peony.Platform.Lynx.Tests/
│   └── Peony.Platform.SNES.Tests/
│
├── docs/                            # Documentation (this directory)
│   ├── ARCHITECTURE.md              # ← You are here
│   ├── DISASSEMBLY-ENGINE.md        # Detailed engine algorithm
│   ├── CDL-PANSY-INTEGRATION.md     # CDL/Pansy format details & usage
│   ├── INTERFACES.md                # Complete interface API documentation
│   ├── MULTI-BANK.md                # Per-platform bank switching
│   ├── NEXEN-PACK-WORKFLOW.md       # Using .nexen-pack.zip for disassembly projects
│   ├── IMPROVING-DISASSEMBLY.md     # Guide to correcting & improving output
│   └── Platform-Comparison.md       # Platform feature matrix
│
├── output/                          # Generated .pasm output files
├── ~Plans/                          # Technical plans and code-plans
└── ~docs/                           # Session logs and chat history
```

---

## Sub-Documents

| Document | Description |
|----------|-------------|
| [DISASSEMBLY-ENGINE.md](DISASSEMBLY-ENGINE.md) | Detailed recursive descent algorithm, label management, data region detection, cross-reference tracking |
| [CDL-PANSY-INTEGRATION.md](CDL-PANSY-INTEGRATION.md) | CDL format specifications, Pansy section details, how hint files improve disassembly quality |
| [INTERFACES.md](INTERFACES.md) | Complete API documentation for `ICpuDecoder`, `IPlatformAnalyzer`, `IOutputFormatter`, and all data structures |
| [MULTI-BANK.md](MULTI-BANK.md) | Per-platform bank switching: NES MMC1, SNES LoRom/HiRom, Game Boy MBC, GBA linear |
| [NEXEN-PACK-WORKFLOW.md](NEXEN-PACK-WORKFLOW.md) | How to create a disassembly project from a Nexen `.nexen-pack.zip` export |
| [IMPROVING-DISASSEMBLY.md](IMPROVING-DISASSEMBLY.md) | Practical guide to identifying and fixing disassembly errors |

---

## Quick Start

```bash
# Basic disassembly (auto-detect platform)
peony disasm game.nes --output game.pasm

# With CDL hints from emulator
peony disasm game.nes --cdl game.cdl --output game.pasm

# With Pansy metadata (symbols + CDL + comments)
peony disasm game.nes --symbols game.pansy --output game.pasm

# Full pipeline with all hints
peony disasm game.nes --cdl game.cdl --symbols game.pansy --output game.pasm --all-banks

# Verify roundtrip
peony verify game.nes --reassembled game-rebuilt.nes

# Export symbols to Pansy format
peony export game.nes --output game.pansy --format pansy --symbols game.nexen-labels

# ROM information
peony info game.nes
```
