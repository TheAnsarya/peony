# Interfaces & Data Structures — API Reference

> Complete reference for Peony's core abstractions and types.

**Source:** `src/Peony.Core/Interfaces.cs`, `src/Peony.Core/SymbolLoader.cs`

---

## Core Interfaces

### ICpuDecoder

The CPU instruction decoder. Each supported architecture implements this interface.

```csharp
public interface ICpuDecoder {
    string Architecture { get; }
    DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address);
    bool IsControlFlow(DecodedInstruction instruction);
    IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address);
}
```

| Member | Purpose |
|--------|---------|
| `Architecture` | CPU name: `"6502"`, `"65816"`, `"65SC02"`, `"SM83"`, `"ARM7TDMI"` |
| `Decode()` | Decode raw bytes at a given address into a structured instruction |
| `IsControlFlow()` | Returns `true` for branches, jumps, calls, returns |
| `GetTargets()` | Returns target addresses for control flow instructions |

**Implementations:**

| Project | Architecture | Systems |
|---------|-------------|---------|
| `Peony.Cpu.6502` | MOS 6502 | NES |
| `Peony.Cpu.65816` | WDC 65816 | SNES |
| `Peony.Cpu.65SC02` | WDC 65SC02 | Atari Lynx |
| `Peony.Cpu.SM83` | Sharp SM83 | Game Boy |
| `Peony.Cpu.ARM7TDMI` | ARM7TDMI | GBA |

**Decode() behavior by architecture:**

| Architecture | Max Instruction Size | Addressing Modes | Notes |
|-------------|---------------------|-------------------|-------|
| 6502 | 3 bytes | 13 modes | Fixed-width opcodes |
| 65816 | 4 bytes | 24+ modes | Variable width (8/16-bit M/X) |
| 65SC02 | 3 bytes | 15 modes | 6502 + extras (ZeroPageIndirect) |
| SM83 | 3 bytes | ~8 modes | CB-prefixed extended ops |
| ARM7TDMI | 4 bytes (ARM) / 2 bytes (Thumb) | Many | Two instruction sets |

---

### IPlatformAnalyzer

Platform-specific ROM analysis. Handles memory mapping, banking, and hardware details.

```csharp
public interface IPlatformAnalyzer {
    string Platform { get; }
    ICpuDecoder CpuDecoder { get; }
    int BankCount { get; }
    int RomDataOffset { get; }
    RomInfo Analyze(ReadOnlySpan<byte> rom);
    string? GetRegisterLabel(uint address);
    MemoryRegion GetMemoryRegion(uint address);
    uint[] GetEntryPoints(ReadOnlySpan<byte> rom);
    int AddressToOffset(uint address, int romLength);
    int AddressToOffset(uint address, int romLength, int bank);
    uint? OffsetToAddress(int offset);
    bool IsInSwitchableRegion(uint address);
    BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank);
}
```

| Member | Purpose |
|--------|---------|
| `Platform` | Platform name: `"NES"`, `"SNES"`, `"Game Boy"`, etc. |
| `CpuDecoder` | The CPU decoder instance for this platform |
| `BankCount` | Number of ROM banks (1 for linear ROM, N for banked) |
| `RomDataOffset` | Bytes to skip in ROM file before actual data (header size) |
| `Analyze()` | Parse ROM header, detect mapper/configuration |
| `GetRegisterLabel()` | Map hardware address to name (e.g., `$2000` → `"PPUCTRL"`) |
| `GetMemoryRegion()` | Classify address (Code, Data, Ram, Hardware, etc.) |
| `GetEntryPoints()` | Read interrupt vectors from ROM |
| `AddressToOffset()` | CPU address → file offset (accounting for banking) |
| `OffsetToAddress()` | File offset → CPU address |
| `IsInSwitchableRegion()` | Whether address is in a bank-switchable region |
| `DetectBankSwitch()` | Detect bank switch patterns at an address |

**Implementations:**

| Project | Platform | Header | Banking | Entry Points |
|---------|----------|--------|---------|-------------|
| `Peony.Platform.NES` | NES | iNES (16 bytes) | Mapper-dependent | $FFFA NMI, $FFFC RESET, $FFFE IRQ |
| `Peony.Platform.SNES` | SNES | None (internal) | LoROM/HiROM | From internal header vectors |
| `Peony.Platform.GB` | Game Boy | Embedded (0x100-0x14F) | MBC type | $0100 entry, $0040/$0048 RSTs |
| `Peony.Platform.GBA` | GBA | 192-byte header | Linear (no banking) | Header entry point ($08000000) |
| `Peony.Platform.Atari2600` | Atari 2600 | None | Bank-switching (F8, F6, etc.) | $FFFC RESET |
| `Peony.Platform.Lynx` | Atari Lynx | LNX (64 bytes) | Cart-based | From LNX header |

---

### IOutputFormatter

Generates output files from a disassembly result.

```csharp
public interface IOutputFormatter {
    string Name { get; }
    string Extension { get; }
    void Generate(DisassemblyResult result, string outputPath);
}
```

| Member | Purpose |
|--------|---------|
| `Name` | Format name: `"Poppy"` |
| `Extension` | File extension: `".pasm"` |
| `Generate()` | Write disassembly to output file(s) |

**Current Implementation:** `PoppyFormatter` — generates `.pasm` files compatible with the Poppy assembler.

---

## Core Data Structures

### DecodedInstruction

Represents a single decoded CPU instruction.

```csharp
public record DecodedInstruction(
    string Mnemonic,      // e.g., "lda", "jsr", "bne"
    string Operand,       // e.g., "#$80", "$2000", "($ff,x)"
    byte[] Bytes,         // Raw instruction bytes
    AddressingMode Mode   // Addressing mode classification
);
```

**Examples:**

| Bytes | Mnemonic | Operand | Mode |
|-------|----------|---------|------|
| `A9 80` | `lda` | `#$80` | Immediate |
| `8D 00 20` | `sta` | `$2000` | Absolute |
| `D0 FE` | `bne` | `$8002` | Relative |
| `20 00 C0` | `jsr` | `$c000` | Absolute |
| `6C FF FF` | `jmp` | `($ffff)` | Indirect |

---

### AddressingMode

Enumeration of all CPU addressing modes across all supported architectures.

```csharp
public enum AddressingMode {
    // Common (6502, 65SC02, 65816)
    Implied,                    // clc, rts
    Accumulator,                // asl a
    Immediate,                  // lda #$ff
    ZeroPage,                   // lda $00
    ZeroPageX,                  // lda $00,x
    ZeroPageY,                  // ldx $00,y
    Absolute,                   // lda $1234
    AbsoluteX,                  // lda $1234,x
    AbsoluteY,                  // lda $1234,y
    Indirect,                   // jmp ($1234)
    IndirectX,                  // lda ($00,x)
    IndirectY,                  // lda ($00),y
    Relative,                   // bne label

    // 65C02/65SC02 additions
    ZeroPageIndirect,           // lda ($00)
    AbsoluteIndirectX,          // jmp ($1234,x)

    // 65816 additions
    RelativeLong,               // brl label
    StackRelative,              // lda $03,s
    StackRelativeIndirectY,     // lda ($03,s),y
    Direct,                     // lda dp
    DirectX,                    // lda dp,x
    DirectY,                    // lda dp,y
    DirectIndirect,             // lda (dp)
    DirectIndirectLong,         // lda [dp]
    DirectIndirectLongY,        // lda [dp],y
    AbsoluteLong,               // lda $123456
    AbsoluteLongX,              // lda $123456,x
    BlockMove                   // mvp $01,$02
}
```

---

### DisassemblyResult

The output of the entire disassembly process.

```csharp
public class DisassemblyResult {
    RomInfo RomInfo { get; set; }

    // All disassembled blocks, ordered by address
    List<DisassembledBlock> Blocks { get; }

    // Global labels (address → name)
    Dictionary<uint, string> Labels { get; }

    // Bank-specific labels ((address, bank) → name)
    Dictionary<(uint Address, int Bank), string> BankLabels { get; }

    // Comments (address → text)
    Dictionary<uint, string> Comments { get; }

    // Blocks organized by bank number
    Dictionary<int, List<DisassembledBlock>> BankBlocks { get; }

    // Cross-references (target address → list of sources)
    Dictionary<uint, List<CrossRef>> CrossReferences { get; }

    // Detected data regions (start address → definition)
    Dictionary<uint, DataDefinition> DataRegions { get; }

    // Helpers
    string? GetLabel(uint address, int? bank = null);
    IReadOnlyList<CrossRef> GetReferencesTo(uint address);
}
```

**Blocks vs BankBlocks:**
- `Blocks` — Flat list of all blocks, sorted by address
- `BankBlocks` — Same blocks organized by bank number (key = bank index)

For single-bank ROMs (GBA, simple NES), `BankBlocks[0]` == `Blocks`.
For multi-bank ROMs, each bank gets its own sorted list.

---

### DisassembledBlock

A contiguous range of disassembled instructions or data.

```csharp
public record DisassembledBlock(
    uint StartAddress,          // First address in block
    uint EndAddress,            // Address after last byte
    MemoryRegion Type,          // Code, Data, Graphics, etc.
    List<DisassembledLine> Lines, // All lines in the block
    int Bank = -1               // Bank number (-1 = global/fixed)
);
```

**Block types:**
- `Code` — Sequential instructions ending at a control flow break
- `Data` — `.db` / `.dw` data bytes
- `Graphics` — Tile/sprite data

---

### DisassembledLine

A single line of output (one instruction or data directive).

```csharp
public record DisassembledLine(
    uint Address,               // CPU address
    byte[] Bytes,               // Raw bytes
    string? Label,              // Label at this address (if any)
    string Content,             // Formatted instruction or data directive
    string? Comment,            // Inline comment (if any)
    int Bank = -1               // Bank number
);
```

**Output example:**
```asm
reset:                          ; → label
    lda #$80                    ; → content (with comment)
    sta PPUCTRL                 ; → content uses register label
```

---

### CrossRef & CrossRefType

Cross-reference tracking.

```csharp
public record CrossRef(
    uint FromAddress,           // Source of the reference
    int FromBank,               // Bank containing the source
    CrossRefType Type           // Type of reference
);

public enum CrossRefType {
    Jump,       // JMP, BRA — unconditional transfer
    Call,       // JSR, JSL — subroutine call (expects return)
    Branch,     // BCC, BNE — conditional branch
    DataRef,    // LDA abs — data access to a known address
    Pointer     // Address found in a pointer table
}
```

**Pansy mapping (for export):**

| Peony CrossRefType | Pansy CrossRefType | Pansy Value |
|--------------------|--------------------|-------------|
| Call | Jsr | 1 |
| Jump | Jmp | 2 |
| Branch | Branch | 3 |
| DataRef | Read | 4 |
| Pointer | — | (exported as symbol) |

---

### DataDefinition

Describes a structured data region.

```csharp
public record DataDefinition(
    string Type,      // "byte", "word", "pointer", "text", "graphics"
    int Count,        // Number of elements
    string? Name      // Optional label for the data region
);
```

**Output mapping:**

| Type | Directive | Example |
|------|-----------|---------|
| `"byte"` | `.db` | `.db $00, $ff, $a0` |
| `"word"` | `.dw` | `.dw $8000, $c000` |
| `"pointer"` | `.dw` (with labels) | `.dw sub_8000, sub_c000` |
| `"text"` | `.text` | `.text "HELLO"` |
| `"graphics"` | `.incbin` | `.incbin "tiles.chr"` |

---

### RomInfo

ROM analysis results from `IPlatformAnalyzer.Analyze()`.

```csharp
public record RomInfo(
    string Platform,                      // "NES", "SNES", etc.
    int Size,                             // ROM size in bytes
    string? Mapper,                       // Mapper name (NES) or layout (SNES)
    Dictionary<string, string> Metadata   // Platform-specific details
);
```

**Metadata examples by platform:**

| Platform | Key | Value Example |
|----------|-----|---------------|
| NES | `"mapper"` | `"MMC1"` |
| NES | `"prg_banks"` | `"8"` |
| NES | `"chr_banks"` | `"16"` |
| NES | `"mirroring"` | `"vertical"` |
| SNES | `"layout"` | `"LoROM"` |
| SNES | `"rom_speed"` | `"FastROM"` |
| GB | `"cart_type"` | `"MBC1+RAM+BATTERY"` |
| GB | `"rom_banks"` | `"8"` |
| GBA | `"title"` | `"POKEMON RUBY"` |
| Lynx | `"cart_name"` | `"GAME TITLE"` |

---

### MemoryRegion (Enum)

Classification for address ranges.

```csharp
public enum MemoryRegion {
    Unknown,      // Not classified
    Code,         // Executable code
    Data,         // General data
    Graphics,     // Tile/sprite data
    Audio,        // Sound/music data
    Ram,          // Work RAM
    Rom,          // Non-code ROM data
    Hardware      // Memory-mapped I/O registers
}
```

---

### BankSwitchInfo

Detected bank switch operation.

```csharp
public record BankSwitchInfo(
    int TargetBank,         // Bank being switched to
    uint TargetAddress,     // Address to continue execution at
    string? FunctionName    // Name of the bank switch routine (if known)
);
```

**Platform examples:**

| Platform | Bank Switch Pattern | Detection |
|----------|-------------------|-----------|
| NES MMC1 | Write to $8000-$FFFF | Shift register writes |
| NES MMC3 | Write to $8000/$8001 | Bank select register |
| SNES | — | Bank byte in 24-bit address |
| GB MBC1 | Write to $2000-$3FFF | ROM bank number |
| Atari 2600 | Read $1FF8-$1FF9 | Hotspot access |
| Lynx | Cart register writes | Cart hardware I/O |

---

## Utility Classes

### SymbolLoader

Loads symbols from multiple file formats.

```csharp
public class SymbolLoader {
    // Loaded data
    Dictionary<uint, string> Labels { get; }
    Dictionary<uint, string> Comments { get; }
    IReadOnlyDictionary<uint, DataDefinition> DataDefinitions { get; }
    Dictionary<(uint, int), string> BankLabels { get; }

    // Loading methods
    void LoadFceuxNl(string path);          // FCEUX .nl label files
    void LoadMesenMlb(string path);         // Mesen .mlb label files
    void LoadJsonSymbols(string path);       // JSON symbol files
    void LoadSymFile(string path);           // .sym files (No$gba, RGBDS)
    void LoadCdl(string cdlPath, int size);  // CDL files (via CdlLoader)
    void LoadDiz(string dizPath);            // DiztinGUIsh .diz files
    void LoadPansy(string pansyPath);        // Pansy .pansy files

    // Query methods
    bool IsCode(int offset);
    bool IsData(int offset);
    int? GetBankForAddress(uint address);
}
```

### SymbolExporter

Exports disassembly results to multiple formats.

```csharp
public class SymbolExporter {
    void ExportMesenMlb(string path, DisassemblyResult result);
    void ExportFceuxNl(string path, DisassemblyResult result);
    void ExportNoCashSym(string path, DisassemblyResult result);
    void ExportCa65Dbg(string path, DisassemblyResult result);
    void ExportWlaSymbols(string path, DisassemblyResult result);
    void ExportBizHawkCht(string path, DisassemblyResult result);
    void ExportPansy(string path, DisassemblyResult result, byte[] rom);
}
```

### DataDetector

Heuristic analysis for unclassified memory regions.

```csharp
// In Peony.Analysis namespace
public static class DataDetector {
    List<DetectedRegion> Analyze(byte[] rom, int codeStart, int codeEnd, string platform);
    bool IsLikelyGraphics(byte[] data, int start, int length);
}

public record DetectedRegion(int Start, int End, RegionType Type);

public enum RegionType {
    Unknown, Data, Code, Graphics, Text, PointerTable
}
```

### RoundtripVerifier

Verifies that disassembled output reassembles to the original ROM.

```csharp
public class RoundtripVerifier {
    // Compare original ROM with assembled output
    RoundtripResult Verify(string originalRomPath, string assembledRomPath);
    RoundtripResult Verify(byte[] originalRom, byte[] assembledRom);
}

public record RoundtripResult(
    bool Success,
    int MismatchCount,
    List<(int Offset, byte Expected, byte Actual)> Mismatches
);
```

---

## Type Relationships

```
          ┌──────────────────────────────────────────────────┐
          │                DisassemblyEngine                  │
          │                                                   │
          │  Uses: ICpuDecoder, IPlatformAnalyzer,           │
          │        SymbolLoader                               │
          │                                                   │
          │  Produces: DisassemblyResult                      │
          └──────────────────┬───────────────────────────────┘
                             │
                             ▼
          ┌──────────────────────────────────────────────────┐
          │              DisassemblyResult                    │
          │                                                   │
          │  Contains: RomInfo                                │
          │            DisassembledBlock[] ──→ DisassembledLine[] │
          │            Labels, BankLabels                     │
          │            Comments                               │
          │            CrossRef[]                              │
          │            DataDefinition[]                        │
          └──────────────────┬───────────────────────────────┘
                             │
                 ┌───────────┼───────────┐
                 ▼           ▼           ▼
         IOutputFormatter  SymbolExporter  RoundtripVerifier
         (PoppyFormatter)  (.pansy, .mlb)  (ROM comparison)
                 │           │
                 ▼           ▼
              .pasm      .pansy/.mlb/.nl
              files      symbol files
```
