# Architecture Separation Plan

## Goal

Split Peony into a strong core framework with separate per-platform components, mirroring Poppy's architecture separation pattern. Each platform (NES, SNES, GB, GBA, etc.) is a self-contained component with its own CPU decoder, platform analyzer, output formatter, and asset extractors — registered dynamically at startup via a central resolver.

## Current State

Peony already has:

- `ICpuDecoder` - Per-CPU instruction decoding (13 CPU projects)
- `IPlatformAnalyzer` - Per-platform ROM analysis (11 platform projects)
- `IOutputFormatter` - Output generation (PoppyFormatter in Core)

What's missing:

- **No unified profile interface** tying CPU + Platform + Output + Assets together
- **No central resolver** for dynamic platform dispatch
- **No per-platform Registration pattern** — CLI manually wires everything
- **Output formatters live in Core** instead of per-platform projects
- **Asset extractors scattered in CLI** instead of per-platform projects
- **No platform-specific .pasm output** — all platforms share one formatter

## Target Architecture (Poppy Parity)

### New Core Abstractions

```
Peony.Core/Platform/
├── IPlatformProfile.cs      # Master plugin interface (like Poppy's ITargetProfile)
├── PlatformResolver.cs      # Central registry (like Poppy's TargetResolver)
├── IInstructionDecoder.cs   # Renamed from ICpuDecoder for clarity
├── IRomAnalyzer.cs          # Renamed from IPlatformAnalyzer for clarity
├── IOutputGenerator.cs      # Per-platform output (replaces IOutputFormatter)
├── IAssetExtractor.cs       # Per-platform asset extraction
└── PlatformId.cs            # Enum of all platforms
```

### IPlatformProfile Interface

```csharp
public interface IPlatformProfile {
	PlatformId Platform { get; }
	string DisplayName { get; }
	ICpuDecoder CpuDecoder { get; }
	IPlatformAnalyzer Analyzer { get; }
	IOutputGenerator OutputGenerator { get; }
	IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	IReadOnlyList<string> RomExtensions { get; }
	byte? PansyPlatformId { get; }
}
```

### PlatformResolver

```csharp
public static class PlatformResolver {
	static void Register(IPlatformProfile profile);
	static IPlatformProfile? Resolve(string name);
	static IPlatformProfile? ResolveByExtension(string ext);
	static IPlatformProfile GetProfile(PlatformId platform);
	static IReadOnlyList<IPlatformProfile> GetAll();
}
```

### Per-Platform Registration

Each `Peony.Platform.*` project exposes:

```csharp
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(SnesProfile.Instance);
	}
}
```

### CLI Startup

```csharp
// Program.cs — explicit registration (like Poppy)
Peony.Platform.NES.Registration.RegisterAll();
Peony.Platform.SNES.Registration.RegisterAll();
Peony.Platform.GameBoy.Registration.RegisterAll();
// ... etc
```

## SNES Focus — End-to-End Disassembly

### Components

1. **Cpu65816Decoder** (existing) — Full 65816 instruction set with M/X flag tracking
2. **SnesAnalyzer** (existing) — LoROM/HiROM mapping, header parsing, register labels
3. **SnesProfile** (new) — Ties everything together
4. **SnesOutputGenerator** (new) — SNES-specific .pasm output with:
	- `.lorom` / `.hirom` directives
	- `.a8` / `.a16` / `.i8` / `.i16` state tracking
	- Bank boundary awareness
	- SNES header reproduction directives
	- Memory map comments
5. **SnesAssetExtractor** (new) — SNES-specific asset extraction:
	- 4bpp/2bpp tile graphics
	- SNES palettes (15-bit BGR)
	- Tilemaps
	- SPC audio data

### Disassembly Pipeline for SNES

```
ROM → RomLoader.Load()
    → SnesAnalyzer.Analyze() → RomInfo + LoROM/HiROM detection
    → SnesAnalyzer.GetEntryPoints() → Reset/NMI/IRQ vectors
    → DisassemblyEngine.Disassemble() → Recursive descent
    → SnesOutputGenerator.Generate() → .pasm files
    → Optional: SnesAssetExtractor.Extract() → Graphics/palettes/text
```

## Phase Plan

### Phase 1: Core Framework (this session)

- Add `IPlatformProfile` interface
- Add `PlatformResolver` static class
- Add `PlatformId` enum
- Add `IOutputGenerator` interface
- Add `IAssetExtractor` interface

### Phase 2: SNES Component (this session)

- Create `SnesProfile` implementing `IPlatformProfile`
- Create `SnesOutputGenerator` for .pasm output
- Create `SnesRegistration` class
- Wire up CLI to use `PlatformResolver`

### Phase 3: Other Platforms (future)

- NES, GB, GBA, Atari 2600, Lynx, SMS, PCE, Genesis, WonderSwan, Channel F
- Each gets its own Profile + Registration + OutputGenerator

## Non-Goals

- No statistical/probabilistic disassembly
- No reflection-based discovery (explicit registration only)
- No breaking changes to existing ICpuDecoder/IPlatformAnalyzer interfaces (extend, don't replace)
