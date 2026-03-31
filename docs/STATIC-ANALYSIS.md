# Static Analysis — Quarantine Documentation

## Overview

Peony includes a static byte classification system (`StaticAnalyzer`) that can categorize ROM bytes into types (code, data, graphics, pointers, text, vectors, padding, hardware). This feature is **quarantined by default** — it must be explicitly opted into via the `--static-analysis` CLI flag.

## Why Quarantined?

Static analysis uses probabilistic and heuristic techniques that can produce false positives. The default disassembly pipeline is designed to be **deterministic and hint-driven**:

1. Start from known entry points (reset vectors, interrupt vectors)
2. Recursive descent from those entry points
3. Use authoritative hints from `.pansy` and `.cdl` files when available

This ensures reproducible, predictable output. Static analysis adds speculative classification that may not match the actual ROM behavior observed during emulation.

## Enabling Static Analysis

```powershell
# Enable static analysis with --static-analysis flag
dotnet run --project src/Peony.Cli -- disasm --rom game.nes --static-analysis

# Without the flag, static analysis is completely skipped (default)
dotnet run --project src/Peony.Cli -- disasm --rom game.nes
```

## Classification Priority Cascade

When static analysis is enabled, bytes are classified using a strict priority hierarchy. Each phase only sets bytes that remain `Unknown` from higher-priority sources:

| Priority | Source | Confidence | Description |
|----------|--------|------------|-------------|
| 1 | CDL flags | Highest | Live emulation data (CODE, DATA, DRAWN) |
| 2 | Pansy code/data map | High | Prior analysis flags (IsCode, IsData, etc.) |
| 3 | Cross-references | High | Pansy cross-ref targets (JSR, JMP, branch) |
| 4 | Symbols | Medium-High | Pansy symbol types (Label, Function, etc.) |
| 5 | Memory regions | Medium | Pansy memory region classifications |
| 6 | ROM vectors | Medium | Reset, NMI, IRQ vector addresses |
| 7 | Operand analysis | Low-Medium | Instruction operand target inference |
| 8 | Platform defaults | Low | Platform memory map knowledge base |

## Byte Classification Flags

```csharp
[Flags]
public enum ByteClassification : byte {
	Unknown   = 0x00,  // Not yet classified
	Code      = 0x01,  // Executable instruction bytes
	Data      = 0x02,  // Tables, lookup values
	Graphics  = 0x04,  // CHR/tile data (CDL DRAWN)
	Pointer   = 0x08,  // Pointer table entries
	Text      = 0x10,  // String/text data
	Vector    = 0x20,  // Interrupt/reset vectors
	Padding   = 0x40,  // Filler bytes ($00/$ff runs)
	Hardware  = 0x80,  // Hardware register space
}
```

## Classification Sources

```csharp
public enum ClassificationSource : byte {
	Unknown,       // Not classified
	Cdl,           // CDL from emulator — highest authority
	PansyCodeMap,  // Pansy code/data map flags
	PansyCrossRef, // Pansy cross-reference data
	PansySymbol,   // Pansy symbol type
	PansyRegion,   // Pansy memory region
	RomVector,     // ROM interrupt/reset vector
	OperandTrace,  // Instruction operand target analysis
	PlatformMap,   // Platform memory map knowledge
}
```

## Architecture

### Key Files

- [DisassemblyEngine.cs](../src/Peony.Core/DisassemblyEngine.cs) — Main engine, calls `SetStaticAnalysisEnabled()`
- [StaticAnalyzer.cs](../src/Peony.Core/StaticAnalyzer.cs) — Classification pipeline
- [InstructionAnalyzer.cs](../src/Peony.Core/InstructionAnalyzer.cs) — Operand data reference extraction
- [Program.cs](../src/Peony.Cli/Program.cs) — CLI `--static-analysis` flag

### How It Integrates

```
ROM → DisassemblyEngine.Disassemble()
         │
         ├─ if (staticAnalysisEnabled)
         │    └─ StaticAnalyzer.Classify(rom)
         │         ├─ CDL phase
         │         ├─ Pansy code/data map phase
         │         ├─ Cross-ref phase
         │         ├─ Symbol phase
         │         ├─ Memory region phase
         │         ├─ Vector phase
         │         ├─ Operand analysis phase
         │         └─ Platform defaults phase
         │
         └─ Recursive descent from entry points
              └─ (uses classification hints if available)
```

## Best Practices

1. **Always prefer `.pansy` + `.cdl` hints** over raw static analysis
2. Use static analysis for **investigation** when no CDL/Pansy data exists
3. Use `--static-analysis` to generate an initial classification, then refine with emulation data
4. Review classification output carefully — especially `OperandTrace` and `PlatformMap` sources which have lower confidence

## Related

- [Static Analysis Architecture Plan](../~Plans/static-analysis-architecture.md)
- [Static Analyzer Code Plan](../~Plans/static-analyzer-code-plan.md)
- [Pansy Integration](../docs/CLI-REFERENCE.md)
