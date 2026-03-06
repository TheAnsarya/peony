# StaticAnalyzer Code Plan

## Purpose

Replace `DataDetector.cs` with a deterministic static analyzer that uses CDL, Pansy,
instruction semantics, and platform memory maps instead of percentage-based heuristics.

## Class: `StaticAnalyzer`

```csharp
namespace Peony.Core;

/// <summary>
/// Deterministic static analyzer that classifies ROM bytes using authoritative data
/// sources (CDL, Pansy, instruction analysis) instead of heuristic percentages.
/// </summary>
public sealed class StaticAnalyzer {
    // Classification priority (highest → lowest):
    // 1. CDL flags from emulator (CODE, DATA, DRAWN)
    // 2. Pansy cross-references
    // 3. Pansy symbols with types
    // 4. Pansy memory regions
    // 5. ROM vectors (reset, NMI, IRQ)
    // 6. Instruction operand analysis (LDA/STA targets)
    // 7. Platform memory map knowledge

    private readonly IPlatformAnalyzer _platform;
    private readonly byte[] _classificationMap;  // per-byte classification
    private readonly SymbolLoader? _symbolLoader;

    // Constructor takes platform + optional symbol data
    public StaticAnalyzer(IPlatformAnalyzer platform, SymbolLoader? symbolLoader = null);

    // Main entry point: classify all bytes in ROM
    public ClassificationResult Classify(ReadOnlySpan<byte> rom);

    // Phase 1: Apply CDL flags (authoritative)
    private void ApplyCdlClassification(ReadOnlySpan<byte> rom);

    // Phase 2: Apply Pansy cross-references
    private void ApplyCrossRefClassification();

    // Phase 3: Apply Pansy symbols
    private void ApplySymbolClassification();

    // Phase 4: Apply ROM vectors
    private void ApplyVectorClassification(ReadOnlySpan<byte> rom);

    // Phase 5: Trace instruction operands to find data refs
    private void ApplyOperandAnalysis(ReadOnlySpan<byte> rom);

    // Phase 6: Platform memory map for remaining unknowns
    private void ApplyPlatformDefaults(ReadOnlySpan<byte> rom);
}
```

## Class: `ClassificationResult`

```csharp
/// <summary>
/// Result of static analysis classification for a ROM.
/// Each byte has a classification and optional annotation.
/// </summary>
public sealed class ClassificationResult {
    /// <summary>Per-byte classification flags</summary>
    public ByteClassification[] Map { get; }

    /// <summary>Detected data regions with boundaries</summary>
    public IReadOnlyList<ClassifiedRegion> Regions { get; }

    /// <summary>Data references found from instruction operands</summary>
    public IReadOnlyList<DataReference> DataReferences { get; }

    /// <summary>Classification source for each byte (what decided it)</summary>
    public ClassificationSource[] Sources { get; }

    /// <summary>Statistics about classification coverage</summary>
    public ClassificationStats Stats { get; }
}

[Flags]
public enum ByteClassification : byte {
    Unknown     = 0x00,
    Code        = 0x01,
    Data        = 0x02,
    Graphics    = 0x04,  // CHR/tile data (CDL DRAWN flag)
    Pointer     = 0x08,  // Part of a pointer table
    Text        = 0x10,  // Text/string data
    Vector      = 0x20,  // Interrupt/reset vector
    Padding     = 0x40,  // Padding/filler bytes ($00 or $ff runs)
    Hardware    = 0x80,  // Hardware register space
}

public enum ClassificationSource : byte {
    Unknown,        // Not yet classified
    Cdl,            // From CDL flags (highest confidence)
    PansyCrossRef,  // From Pansy cross-reference data
    PansySymbol,    // From Pansy symbol type
    PansyRegion,    // From Pansy memory region
    RomVector,      // From ROM interrupt vectors
    OperandTrace,   // From instruction operand analysis
    PlatformMap,    // From platform memory map knowledge
}

public record ClassifiedRegion(
    int StartOffset,
    int EndOffset,
    ByteClassification Classification,
    ClassificationSource Source,
    string? Annotation
);

public record DataReference(
    int InstructionOffset,   // Where the referencing instruction is
    uint InstructionAddress,  // CPU address of instruction
    uint TargetAddress,       // What address it references
    DataRefType RefType       // How it references it
);

public enum DataRefType {
    Read,    // LDA, LDX, LDY (reads data)
    Write,   // STA, STX, STY (writes data)
    Jump,    // JMP, BRA (code target)
    Call,    // JSR (subroutine target)
    Branch,  // BNE, BEQ, etc. (conditional branch)
    Indirect // JMP ($xxxx) (indirect reference)
}
```

## Integration with DisassemblyEngine

```csharp
// In DisassemblyEngine.Disassemble():

// Before recursive descent, run static analysis
var analyzer = new StaticAnalyzer(_platformAnalyzer, _symbolLoader);
var classification = analyzer.Classify(rom);

// Use classification to:
// 1. Pre-mark data regions (skip during recursive descent)
// 2. Add discovered code entry points to queue
// 3. Annotate data regions in output

// Replace: IsInDataRegion() and ShouldTreatAsCode()
// With: classification.Map[offset].HasFlag(ByteClassification.Code)
//       classification.Map[offset].HasFlag(ByteClassification.Data)
```

## Class: `InstructionAnalyzer`

```csharp
namespace Peony.Core;

/// <summary>
/// Analyzes decoded instructions to extract data references.
/// Works with any ICpuDecoder to trace operand targets.
/// </summary>
public sealed class InstructionAnalyzer {
    private readonly ICpuDecoder _decoder;
    private readonly IPlatformAnalyzer _platform;

    public InstructionAnalyzer(ICpuDecoder decoder, IPlatformAnalyzer platform);

    /// <summary>
    /// Scan a code region and extract all data references from instructions.
    /// </summary>
    public IReadOnlyList<DataReference> FindDataReferences(
        ReadOnlySpan<byte> rom,
        int startOffset,
        int endOffset,
        int bank);

    /// <summary>
    /// Check if an instruction references a data address (not code flow).
    /// </summary>
    public DataReference? GetDataReference(
        DecodedInstruction instruction,
        uint instructionAddress,
        int instructionOffset);

    /// <summary>
    /// Determine if an addressing mode implies a data reference.
    /// </summary>
    private bool IsDataAddressingMode(AddressingMode mode);

    /// <summary>
    /// Extract the target address from an instruction operand.
    /// </summary>
    private uint? ExtractTargetAddress(DecodedInstruction instruction);
}
```

## Platform Memory Maps

```csharp
namespace Peony.Core;

/// <summary>
/// Platform-specific memory map knowledge base.
/// Provides deterministic classification for known address ranges.
/// </summary>
public static class PlatformMemoryMap {
    /// <summary>
    /// Get the memory region type for an address on the given platform.
    /// Returns null for addresses that could be either code or data.
    /// </summary>
    public static ByteClassification? GetKnownClassification(
        string platform, uint address);

    /// <summary>
    /// Get hardware register name for address, or null if not a register.
    /// </summary>
    public static string? GetHardwareRegisterName(
        string platform, uint address);

    /// <summary>
    /// Get vector addresses for the platform (reset, NMI, IRQ, etc.).
    /// </summary>
    public static IReadOnlyList<VectorEntry> GetVectors(string platform);
}

public record VectorEntry(uint Address, string Name, int Size);
```

## Test Plan

### Unit Tests (StaticAnalyzerTests.cs)
1. CDL code flags → marked as Code
2. CDL data flags → marked as Data
3. CDL DRAWN flags → marked as Graphics
4. Pansy symbols with Function type → Code
5. Pansy symbols with Constant type → Data
6. ROM vectors → marked as Vector + Pointer
7. LDA absolute → target marked as Data read
8. STA absolute → target marked as Data write
9. JSR → target marked as Code call
10. Unknown regions stay Unknown (not guessed)

### Integration Tests
1. NES ROM with CDL → correct code/data split
2. ROM with Pansy file → symbols applied correctly
3. Full pipeline: classify → disassemble → format → roundtrip verify

### Regression Tests
1. Known-good classification snapshots for test ROMs
2. Ensure no heuristic false positives on known code regions
