# CDL & Pansy Integration — Complete Guide

> How Code/Data Logs and Pansy metadata dramatically improve disassembly quality.

## Why CDL and Pansy Matter

Static disassembly has a fundamental problem: it cannot distinguish code from data without executing the program. This is called the **halting problem** — and it means pure static analysis will always make mistakes.

**CDL (Code/Data Log)** files from emulators solve this by recording which bytes were *actually executed* as code and which were *actually read* as data during gameplay. **Pansy** files go further, providing symbols, comments, memory region definitions, and cross-references.

### Quality Comparison

| Disassembly Source | Code Accuracy | Data Accuracy | Labels | Comments |
|-------------------|---------------|---------------|--------|----------|
| No metadata | ~60-70% | ~30-40% | Auto-generated only | None |
| CDL only | ~95%+ | ~90%+ | Auto + CDL sub-entries | None |
| Pansy only | ~95%+ | ~90%+ | User + auto | User comments |
| CDL + Pansy | ~99%+ | ~95%+ | Best of both | Full |

---

## CDL (Code/Data Log) Files

### What is CDL?

A CDL file contains one byte of flags per byte of ROM. Each flag byte records how the emulator accessed that ROM byte during execution:

```
ROM:  [opcode] [operand] [operand] [data] [data] [opcode] [operand] ...
CDL:  [CODE]   [CODE]    [CODE]    [DATA] [DATA] [CODE]   [CODE]    ...
```

### CDL Flag Definitions

```
Bit 0 (0x01) — CODE         Byte was executed as an instruction
Bit 1 (0x02) — DATA         Byte was read as data
Bit 2 (0x04) — JUMP_TARGET  Byte was the target of a jump/branch
Bit 3 (0x08) — SUB_ENTRY    Byte was the target of a subroutine call (JSR/JSL)
Bit 4 (0x10) — OPCODE       This specific byte is an opcode (not operand)
Bit 5 (0x20) — DRAWN        Byte was read by PPU as graphics data
Bit 6 (0x40) — READ         Byte was read explicitly (LDA, LDX, etc.)
Bit 7 (0x80) — INDIRECT     Byte was accessed via indirect addressing
```

### How Flags Combine

A single byte can have multiple flags:
- `0x01 | 0x10 = 0x11` — Code byte that is specifically the opcode
- `0x01 | 0x04 = 0x05` — Code byte that is a jump target
- `0x02 | 0x20 = 0x22` — Data byte read as graphics
- `0x01 | 0x08 | 0x10 = 0x19` — Subroutine entry point opcode

### Supported CDL Formats

Peony's `CdlLoader` supports four emulator CDL formats:

#### 1. FCEUX Format (Raw)
```
File: {romname}.cdl
Size: Exactly matches PRG-ROM size (NES only)
Structure: Raw bytes, one per ROM byte
No header

Flags (NES-specific):
  Bit 0: Code
  Bit 1: Data
  Bit 2: Rendered (CHR access)
  Bit 3: Read (explicit data read)
```

**Detection:** File size matches expected PRG-ROM size, no magic header.

#### 2. Mesen Format
```
File: {romname}.cdl
Magic: "CDL\x01" (4 bytes)
Header: 4 bytes magic
Data: One byte per ROM byte
Size: ROM size + 4

Flags (extended):
  Bit 0: Code
  Bit 1: Data
  Bit 2: Jump target
  Bit 3: Sub-entry point
  Bit 4: Opcode (vs operand)
  Bit 5: Drawn (graphics)
  Bit 6: Read
  Bit 7: Indirect
```

**Detection:** First 4 bytes are `43 44 4C 01` ("CDL\x01").

#### 3. Mesen2/Nexen Format
```
File: {romname}.cdl
Magic: "CDLv2" (5 bytes)
Header: 5 bytes magic + 4 bytes CRC32
Data: One byte per ROM byte
Size: ROM size + 9

CRC32 matches the ROM, allowing verification.
Same flags as Mesen format.
```

**Detection:** First 5 bytes are `43 44 4C 76 32` ("CDLv2").

#### 4. bsnes Format
```
File: {romname}.cdl
Size: Typically matches SNES ROM size
Structure: Raw bytes with bsnes-specific flag encoding

Flags:
  Bit 0-3: Usage type
  Bit 4-7: Region/bank info
```

**Detection:** File associated with `.sfc`/`.smc` ROM, specific flag patterns.

### CdlLoader API

```csharp
public class CdlLoader {
    // Load CDL from file
    public void Load(string cdlPath, int expectedRomSize);

    // Load CDL from byte array
    public void Load(byte[] cdlData, int expectedRomSize);

    // Detected format
    public CdlFormat DetectedFormat { get; }

    // Extracted sets
    public IReadOnlySet<int> CodeOffsets { get; }      // ROM offsets with CODE flag
    public IReadOnlySet<int> DataOffsets { get; }      // ROM offsets with DATA flag
    public IReadOnlySet<int> JumpTargets { get; }      // ROM offsets with JUMP_TARGET
    public IReadOnlySet<int> SubEntryPoints { get; }   // ROM offsets with SUB_ENTRY
}

public enum CdlFormat {
    Unknown,
    FCEUX,
    Mesen,
    Mesen2,
    Bsnes
}
```

### How CDL Improves Disassembly

```
Without CDL:                        With CDL:

.org $8000                          .org $8000
reset:                              reset:               ; SUB_ENTRY
    lda #$80                            lda #$80         ; CODE
    sta $2000                           sta $2000        ; CODE
    ; ... code ...                      ; ... code ...

; Static analysis continues            .db $ff, $a0, $30  ; DATA (table)
; decoding bytes as code...            .db $ff, $b0, $20  ; DATA
; WRONG — these are data!              .db $00, $c0, $10  ; DATA

loc_8100:                           sub_8100:            ; SUB_ENTRY from CDL
    lda some_table,x                    lda some_table,x
```

Key improvements:
1. **Data regions stop code flow** — CDL DATA flag prevents decoding data as instructions
2. **Sub-entry discovery** — CDL SUB_ENTRY marks subroutines not reachable from static entry points
3. **Jump targets** — CDL JUMP_TARGET confirms computed branch destinations
4. **Graphics data** — CDL DRAWN flag identifies CHR/tile data for `.incbin` directives

---

## Pansy Metadata Files

### What is Pansy?

Pansy (Program ANalysis SYstem) is a comprehensive metadata format. Where CDL only provides per-byte flags, Pansy provides rich structured data:

- **Symbols** — Named addresses with types (function, label, constant, etc.)
- **Comments** — Per-address annotations (inline, block, todo)
- **Memory Regions** — Named address ranges with types (ROM, RAM, VRAM, IO)
- **Cross-References** — Caller → callee relationships with types
- **Code/Data Map** — CDL-equivalent per-byte flags
- **Metadata** — Project name, author, version, timestamps

### Pansy File Structure

```
┌──────────────────────────────────────────┐
│ Header (32 bytes)                        │
│   Magic: "PANSY\0\0\0" (8 bytes)        │
│   Version: uint16 (0x0100)              │
│   Flags: uint16 (bit 0 = compressed)    │
│   Platform: byte (0x01=NES, etc.)       │
│   Reserved: 3 bytes                      │
│   ROM Size: uint32                       │
│   ROM CRC32: uint32                      │
│   Section Count: uint32                  │
│   Reserved: 4 bytes                      │
├──────────────────────────────────────────┤
│ Section Table                            │
│   [SectionId: uint16] [Offset: uint32]  │
│   [Size: uint32]                         │
│   ... repeated for each section          │
├──────────────────────────────────────────┤
│ Section Data (optionally DEFLATE)        │
│   Section 0x0001: Code/Data Map          │
│   Section 0x0002: Symbols                │
│   Section 0x0003: Comments               │
│   Section 0x0004: Memory Regions         │
│   Section 0x0006: Cross-References       │
│   Section 0x0008: Metadata               │
└──────────────────────────────────────────┘
```

### Section Details

#### Section 0x0001 — Code/Data Map

Same flags as CDL (one byte per ROM byte). This is the Pansy equivalent of a CDL file.

```
Offset 0:    FLAGS[0]     → flags for first ROM byte
Offset 1:    FLAGS[1]     → flags for second ROM byte
...
Offset N-1:  FLAGS[N-1]   → flags for last ROM byte
```

#### Section 0x0002 — Symbols

```
For each symbol:
  Address:  uint32
  Type:     byte (1=Label, 2=Constant, 3=Enum, 4=Struct, 5=Macro,
                   6=Local, 7=Anonymous, 8=InterruptVector, 9=Function)
  NameLen:  uint16
  Name:     UTF-8 string (NameLen bytes)
```

Types and their output:
```
Label (1)             → sub_8000:
Constant (2)          → MAX_ENEMIES = $10
Enum (3)              → .enum GameState
Struct (4)            → .struct PlayerData
Macro (5)             → .macro WaitVBlank
Local (6)             → .loop:
Anonymous (7)         → +:
InterruptVector (8)   → nmi_handler:  (vector table entries)
Function (9)          → GameLoop:     (documented subroutines)
```

#### Section 0x0003 — Comments

```
For each comment:
  Address:    uint32
  Type:       byte (1=Inline, 2=Block, 3=Todo)
  TextLen:    uint16
  Text:       UTF-8 string (TextLen bytes)
```

Output formatting:
```asm
; Block comment appears above the instruction    (Type 2)
; TODO: This needs investigation                  (Type 3)
    lda $2002            ; Inline comment         (Type 1)
```

#### Section 0x0004 — Memory Regions

```
For each region:
  StartAddr:  uint32
  EndAddr:    uint32
  Type:       byte (1=ROM, 2=RAM, 3=VRAM, 4=IO, 5=SRAM, 6=WRAM, 7=OpenBus, 8=Mirror)
  Bank:       uint16
  NameLen:    uint16
  Name:       UTF-8 string
```

#### Section 0x0006 — Cross-References

```
For each cross-reference:
  FromAddr:  uint32
  ToAddr:    uint32
  Type:      byte (1=Jsr, 2=Jmp, 3=Branch, 4=Read, 5=Write)
```

### SymbolLoader Pansy Integration

```csharp
public class SymbolLoader {
    // Load Pansy file
    void LoadPansy(string pansyPath);

    // After loading, provides:
    Dictionary<uint, string> Labels;           // Merged with other sources
    Dictionary<uint, string> Comments;         // Address → text
    Dictionary<uint, DataDefinition> DataDefinitions;  // From CDL map
    Dictionary<(uint, int), string> BankLabels;        // Bank-specific

    // CDL-equivalent queries
    bool IsCode(int offset);    // Code/Data Map CODE flag
    bool IsData(int offset);    // Code/Data Map DATA flag
    int? GetBankForAddress(uint address);  // From memory regions

    // Priority: Pansy symbols > user labels > auto-generated
}
```

### SymbolExporter Pansy Generation

```csharp
public class SymbolExporter {
    void ExportPansy(string path, DisassemblyResult result, byte[] rom);
}
```

The export process:
1. Create `PansyWriter` instance with platform ID and ROM CRC32
2. Set `Compressed = true` (DEFLATE sections)
3. Populate code/data map from visited addresses and CDL data
4. Export all labels as symbols (with type inference)
5. Export all comments with types
6. Export all cross-references from the engine
7. Export memory regions from platform analyzer
8. Set metadata (project name, version, timestamp)
9. Write to `.pansy` file

---

## Integration Flow

### Loading: CDL + Pansy → Engine

```
┌─────────────┐                    ┌─────────────┐
│  CDL File    │                    │ Pansy File  │
│ (.cdl)       │                    │ (.pansy)    │
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       ▼                                  ▼
 ┌─────────────┐                   ┌─────────────┐
 │ CdlLoader   │                   │ PansyLoader │
 │             │                   │ (Core lib)  │
 │ CodeOffsets │                   │ Symbols     │
 │ DataOffsets │                   │ Comments    │
 │ JumpTargets │                   │ CDL Map     │
 │ SubEntries  │                   │ MemRegions  │
 └──────┬──────┘                   │ CrossRefs   │
        │                          └──────┬──────┘
        │                                 │
        ▼                                 ▼
   ┌──────────────────────────────────────────┐
   │              SymbolLoader                 │
   │                                           │
   │  Merges all sources into unified data:    │
   │  - Labels (global + bank-specific)        │
   │  - Comments                               │
   │  - Data definitions                       │
   │  - Code/data classification               │
   │                                           │
   │  Priority: User > Pansy > CDL > Auto      │
   └─────────────────┬────────────────────────┘
                     │
                     ▼
            ┌─────────────────┐
            │ DisassemblyEngine │
            │                   │
            │ Uses SymbolLoader │
            │ for every decode  │
            │ decision          │
            └───────────────────┘
```

### Exporting: Engine → CDL + Pansy

```
            ┌───────────────────┐
            │ DisassemblyResult │
            │                   │
            │ Blocks, Labels,   │
            │ Comments, XRefs,  │
            │ DataDefs          │
            └────────┬──────────┘
                     │
                     ▼
            ┌────────────────┐
            │ SymbolExporter │
            └───┬───────┬────┘
                │       │
     ┌──────────┘       └──────────┐
     ▼                             ▼
┌─────────┐                  ┌─────────────┐
│ .pansy  │                  │ .mlb / .nl  │
│ file    │                  │ / .sym / etc│
└─────────┘                  └─────────────┘
```

---

## Getting CDL Data from Nexen

### Method 1: Export Game Package

1. In Nexen, load the ROM and play through as much as possible
2. **File → Export Game Package** creates a `.nexen-pack.zip`
3. The zip contains `Debug/{romname}.cdl` (Mesen2 format) and `Debug/{romname}.pansy`
4. Extract and use with Peony:
   ```bash
   peony disassemble game.nes --cdl game.cdl --pansy game.pansy -o output/
   ```

### Method 2: Direct CDL Export

1. In Nexen, load ROM and play
2. **Debug → Code/Data Logger → Export CDL File**
3. Save as `.cdl` file
4. Use with Peony:
   ```bash
   peony disassemble game.nes --cdl game.cdl -o output/
   ```

### Method 3: Pansy Export Only

1. In Nexen, load ROM and play
2. **Debug → Export Pansy Metadata**
3. Save as `.pansy` file
4. Use with Peony:
   ```bash
   peony disassemble game.nes --pansy game.pansy -o output/
   ```

---

## Improving Disassembly with CDL

### CDL Coverage

CDL only records bytes accessed during actual gameplay. Common coverage gaps:

| Scenario | Typical CDL Coverage |
|----------|---------------------|
| Quick 1-minute test | 10-30% |
| Play through first level | 30-50% |
| Play full game with saves | 60-80% |
| Systematic testing + debug | 80-95% |
| TAS playback (100% path) | 90-99% |

### Improving Coverage

1. **Use save states** — Load states for different game sections
2. **Enable all features** — Visit menus, options, debug modes
3. **Multiple playthroughs** — CDL accumulates across sessions
4. **TAS playback** — Automated input sequences cover specific paths
5. **Merge CDL files** — Combine CDL from multiple sessions (OR the flags)

### What CDL Cannot Capture

- **Dead code** — Unused functions linked but never called
- **Conditional paths** — Rare error handlers, edge case branches
- **Self-modifying code** — Code written at runtime
- **Computed jumps** — Indirect JMP/JSR with dynamic targets
- **DMA-loaded code** — Code copied to RAM and executed there

These require manual analysis or additional Pansy annotations.

---

## Pansy vs CDL: When to Use Each

| Feature | CDL | Pansy |
|---------|-----|-------|
| Code/data classification | ✅ | ✅ (includes CDL map) |
| Sub-entry discovery | ✅ | ✅ |
| Jump targets | ✅ | ✅ |
| Symbol names | ❌ | ✅ |
| Comments | ❌ | ✅ |
| Memory regions | ❌ | ✅ |
| Cross-references | ❌ | ✅ |
| Graphics flags | ✅ | ✅ |
| Platform verification | ❌ | ✅ (CRC32 check) |
| Human editable | ❌ | Via Pansy.UI |
| File size | Small (1:1 ROM) | Medium (compressed) |

**Recommendation:** Always export both CDL and Pansy from Nexen. Use Pansy as the primary metadata source — it includes everything CDL has plus much more.
