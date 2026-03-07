# Peony Project Generator — Code Plan

> Implementation plan for the `project` CLI command and `.peony` archive format.

## Phase 1: Core Project Writer (`ProjectWriter.cs`)

### New Class: `ProjectWriter`

Location: `src/Peony.Core/ProjectWriter.cs`

```csharp
namespace Peony.Core;

public sealed class ProjectWriter {
    // Configuration
    public required string ProjectName { get; init; }
    public required string RomPath { get; init; }
    public bool SplitBanks { get; init; } = true;
    public bool ExtractAssets { get; init; } = true;
    public bool GenerateIncludes { get; init; } = true;
    public bool GenerateDocs { get; init; } = true;
    public bool IncludeRom { get; init; } = true;

    // Input hints (optional)
    public string? CdlPath { get; init; }
    public string? PansyPath { get; init; }
    public string? SymbolPath { get; init; }
    public string? DizPath { get; init; }

    // Main entry point
    public void WriteProjectFolder(string outputDir, DisassemblyResult result, RomInfo romInfo);
    public void WriteProjectArchive(string outputPath, DisassemblyResult result, RomInfo romInfo);

    // Internal generation methods
    private void WritePeonyManifest(string dir, DisassemblyResult result, RomInfo romInfo);
    private void WritePoppyManifest(string dir, RomInfo romInfo);
    private void WriteRomInfo(string dir, RomInfo romInfo);
    private void WriteSourceFiles(string dir, DisassemblyResult result, RomInfo romInfo);
    private void WriteIncludeFiles(string dir, DisassemblyResult result, RomInfo romInfo);
    private void WriteAnalysisFiles(string dir, DisassemblyResult result, RomInfo romInfo);
    private void WriteAssetFiles(string dir, DisassemblyResult result, RomInfo romInfo, byte[] rom);
    private void WriteDocFiles(string dir, DisassemblyResult result, RomInfo romInfo);
    private void WriteCoverageStats(string dir, DisassemblyResult result);
}
```

### Key Behaviors

1. **WriteProjectFolder** — Creates directory tree, writes all files
2. **WriteProjectArchive** — Creates folder in temp, zips to `.peony`
3. **Source splitting** — When `SplitBanks=true`, creates per-bank `.pasm` files and a `main.pasm` with `.include` directives
4. **Include generation** — Platform-specific hardware register defs from known register maps
5. **Analysis export** — Generates Pansy file via `SymbolExporter`, copies CDL if provided, exports cross-refs and coverage as JSON
6. **Asset extraction** — Delegates to existing extractors (ChrExtractor, TextExtractor, etc.)

## Phase 2: Hardware Include Generator (`HardwareIncludeGenerator.cs`)

### New Class: `HardwareIncludeGenerator`

Location: `src/Peony.Core/HardwareIncludeGenerator.cs`

Generates platform-specific `.inc` files with hardware register definitions:

- **NES**: PPU ($2000-$2007), APU ($4000-$4017), mapper registers
- **SNES**: PPU ($2100-$213f), APU ($2140-$2143), DMA ($4300+)
- **GB**: LCD ($ff40-$ff4b), sound ($ff10-$ff3f), system ($ff0f, $ffff)
- **GBA**: Display ($0400:0000-$0400:0056), sound, DMA, timers
- **Atari 2600**: TIA ($00-$2c), RIOT ($280-$297)
- **Lynx**: Suzy ($fc00-$fcff), Mikey ($fd00-$fdff)

## Phase 3: CLI `project` Command

### New Command in Program.cs

```csharp
var projectCmd = new Command("project", "Generate a complete disassembly project");
projectCmd.AddArgument(romArg);
projectCmd.AddOption(outputOpt);
projectCmd.AddOption(new Option<string>("--name", "Project name"));
projectCmd.AddOption(new Option<bool>("--split-banks", () => true, "Split into per-bank files"));
projectCmd.AddOption(new Option<bool>("--extract-assets", () => true, "Extract game assets"));
projectCmd.AddOption(new Option<bool>("--no-archive", "Output as folder instead of .peony archive"));
projectCmd.AddOption(new Option<bool>("--no-rom", "Exclude ROM from archive"));
projectCmd.AddOption(platformOpt);
projectCmd.AddOption(cdlOpt);
projectCmd.AddOption(pansyOpt);
projectCmd.AddOption(symbolsOpt);
projectCmd.AddOption(dizOpt);
```

### Pipeline

```
CLI args
    → RomLoader.Load()
    → PlatformAnalyzer.Analyze()
    → SymbolLoader (CDL + Pansy + Symbols + DIZ)
    → DisassemblyEngine.Disassemble()
    → ProjectWriter.WriteProjectFolder() or WriteProjectArchive()
```

## Phase 4: CLI `open` Command

```csharp
var openCmd = new Command("open", "Extract or inspect a .peony project archive");
openCmd.AddArgument(new Argument<FileInfo>("archive", "Path to .peony file"));
openCmd.AddOption(new Option<DirectoryInfo?>(["-o", "--output"], "Output directory"));
openCmd.AddOption(new Option<bool>("--info", "Show project info without extracting"));
```

## Phase 5: Source File Splitting

### `main.pasm` with Bank Includes

When `SplitBanks=true`, generate:

```asm
; Dragon Warrior 1 — Disassembled by Peony
; Platform: NES (MMC1)

.include "include/hardware.inc"
.include "include/constants.inc"

; Bank includes
.include "banks/bank00.pasm"
.include "banks/bank01.pasm"
; ...
.include "banks/bank07.pasm"

; Interrupt vectors
.include "vectors.pasm"
```

Each bank file is self-contained with:
- `.org` directive for bank base address
- All code/data blocks for that bank
- Local labels and comments

## Phase 6: Coverage and Statistics

### `CoverageAnalyzer.cs`

```csharp
public sealed class CoverageAnalyzer {
    public static CoverageReport Analyze(DisassemblyResult result, RomInfo romInfo);
}

public record CoverageReport {
    public int TotalBytes { get; init; }
    public int CodeBytes { get; init; }
    public int DataBytes { get; init; }
    public int GraphicsBytes { get; init; }
    public int UnknownBytes { get; init; }
    public int LabelCount { get; init; }
    public int CommentCount { get; init; }
    public int CrossRefCount { get; init; }
    public int PointerTableCount { get; init; }
}
```

## Implementation Order

1. `ProjectWriter.cs` — Core class with folder/archive output
2. `HardwareIncludeGenerator.cs` — Platform register definitions
3. `CoverageAnalyzer.cs` — Statistics computation
4. CLI `project` command — Wire up pipeline
5. CLI `open` command — Extract/inspect archives
6. Tests — ProjectWriter roundtrip tests, include generation tests
7. Documentation — Update CLI-REFERENCE.md, README.md

## Dependencies

- `System.IO.Compression` — ZipArchive for `.peony` archives
- `System.Text.Json` — JSON manifest serialization
- Existing: `PoppyFormatter`, `SymbolExporter`, CHR/Text extractors

## File Count Estimate

| File | Lines (est.) | Purpose |
|------|-------------|---------|
| ProjectWriter.cs | 400-500 | Core project generation |
| HardwareIncludeGenerator.cs | 300-400 | Register definitions |
| CoverageAnalyzer.cs | 80-100 | Statistics |
| Program.cs additions | 100-150 | CLI commands |
| ProjectWriterTests.cs | 200-300 | Tests |
| Total | ~1100-1450 | |
