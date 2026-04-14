# 🌺 Peony - Multi-System Disassembler Framework

> The anti-Poppy: ROM → Source code conversion

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Unlicense-green)](LICENSE)

## Overview

Peony is a multi-system disassembler framework designed to work alongside [Poppy](https://github.com/TheAnsarya/poppy) to provide complete bidirectional ROM ↔ Source conversion.

| Direction | Tool | Input | Output |
|-----------|------|-------|--------|
| Assembly | **Poppy** | .pasm source | ROM binary |
| Disassembly | **Peony** | ROM binary | .pasm source |

## 🎮 Supported Systems

| System | CPU | Profile | Status |
|--------|-----|---------|--------|
| **Atari 2600** | 6507 | `Atari2600Profile` | ✅ Complete |
| **Atari Lynx** | 65SC02 | `LynxProfile` | ✅ Complete |
| **NES** | 6502 | `NesProfile` | ✅ Complete |
| **SNES** | 65816 | `SnesProfile` | ✅ Complete |
| **Game Boy** | Sharp LR35902 | `GameBoyProfile` | ✅ Complete |
| **GBA** | ARM7TDMI | `GbaProfile` | ✅ Complete |
| **Genesis** | M68000 | `GenesisProfile` | ✅ Complete |
| **Sega Master System** | Z80 | `SmsProfile` | ✅ Complete |
| **PC Engine** | HuC6280 | `PceProfile` | ✅ Complete |
| **WonderSwan** | V30MZ | `WonderSwanProfile` | ✅ Complete |
| **Channel F** | F8 | `ChannelFProfile` | ✅ Complete |

**Total**: 11 platforms, 2,034 tests passing

## ✨ Features

- **Roundtrip Guarantee**: Disassembled code reassembles to identical ROM
- **Multiple Algorithms**: Linear sweep, recursive descent, speculative, hybrid
- **Platform-Aware**: Automatic register labeling, kernel detection
- **Project Output**: Generate complete `.peony` project archives or folders
- **Asset Extraction**: Extract graphics, text, and palettes automatically
- **Coverage Analysis**: Per-bank and overall coverage statistics
- **Integration**: Import from Mesen2 CDL, DiztinGUIsh, FCEUX, Pansy metadata
- **Nexen Integration**: Import `.nexen-pack.zip` game packages with CDL + Pansy
- **Poppy Output**: Native .pasm output ready to build with Poppy

## 🚀 Quick Start

```bash
# Disassemble an Atari 2600 ROM
peony disasm game.a26 -p atari2600 -o game.pasm

# Disassemble a Game Boy ROM
peony disasm game.gb -p gameboy -o game.pasm

# Disassemble a GBA ROM
peony disasm game.gba -p gba -o game.pasm

# Disassemble with CDL hints (NES)
peony disasm game.nes --cdl game.cdl -o game.pasm

# Poppy-compatible output
peony disasm game.bin -f poppy -o game.pasm

# Disassemble all banks (for banked ROMs)
peony disasm game.nes --all-banks -o game.pasm

# Import a Nexen game pack (extracts, scaffolds, disassembles)
peony import game.nexen-pack.zip --project-dir ./my-project/

# Generate a complete disassembly project folder
peony project game.nes --name my-game --output ./my-game/

# Generate a .peony archive (zip-based)
peony project game.nes --name my-game --archive

# Inspect a .peony archive
peony open my-game.peony --info

# Extract a .peony archive
peony open my-game.peony --extract ./output/

# Verify roundtrip (disassemble → reassemble → compare)
peony verify original.nes -r rebuilt.nes
```

## 🏗️ Architecture

Peony uses a **plugin architecture** where each platform is a self-contained component implementing `IPlatformProfile`:

```
IPlatformProfile
├── PlatformId          (enum identifier)
├── DisplayName         (human-readable name)
├── CpuDecoder          (ICpuDecoder — instruction decoding)
├── Analyzer            (IPlatformAnalyzer — ROM analysis)
├── OutputGenerator     (IOutputGenerator — .pasm output)
├── AssetExtractors     (IAssetExtractor[] — graphics/text/palette)
├── GraphicsExtractor?  (IAssetExtractor — platform graphics)
├── TextExtractor?      (IAssetExtractor — platform text/dialogue)
├── RomExtensions       (file extension auto-detection)
├── DefaultRomExtension (derived from RomExtensions)
├── PoppyPlatformId     (Poppy project platform string)
└── PansyPlatformId     (Pansy metadata mapping)
```

Platforms register at startup via `PlatformResolver` — no reflection, no scanning, just explicit calls.

Key core components:

- **`PlatformResolver`** — Central registry for platform lookup by name, alias, or extension
- **`DisassemblyPipeline`** — Static orchestrator for hint loading, entry point building, and engine creation
- **`AsmFormatter`** — Shared assembly output formatting (labels, comments, cross-refs)
- **`PlatformMemoryMap`** — Address classification, hardware register names, and interrupt vectors
- **`SymbolExporter`** — Exports symbols/metadata to Pansy, DiztinGUIsh, and label formats

## 📁 Project Structure

```
src/
├── Peony.Core/               # Core framework (PlatformResolver, interfaces)
│   └── Platform/             # IPlatformProfile, PlatformId, IOutputGenerator, IAssetExtractor
├── Peony.Cpu.6502/           # 6502/6507 decoder
├── Peony.Cpu.65816/          # 65816 decoder
├── Peony.Cpu.65SC02/         # 65SC02 decoder (Lynx)
├── Peony.Cpu.GameBoy/        # Sharp LR35902 decoder
├── Peony.Cpu.ARM7TDMI/       # ARM7TDMI decoder (ARM + Thumb)
├── Peony.Cpu.F8/             # F8 decoder (Channel F)
├── Peony.Platform.Atari2600/ # Atari 2600 (Profile + Registration)
├── Peony.Platform.Lynx/      # Atari Lynx
├── Peony.Platform.NES/       # NES
├── Peony.Platform.SNES/      # SNES (custom SnesOutputGenerator)
├── Peony.Platform.GameBoy/   # Game Boy
├── Peony.Platform.GBA/       # GBA
├── Peony.Platform.Genesis/   # Genesis / Mega Drive
├── Peony.Platform.SMS/       # Master System / Game Gear
├── Peony.Platform.PCE/       # PC Engine / TurboGrafx-16
├── Peony.Platform.WonderSwan/# WonderSwan
├── Peony.Platform.ChannelF/  # Fairchild Channel F
└── Peony.Cli/                # CLI application
tests/
├── Peony.Core.Tests/              # Core + PlatformResolver tests
├── Peony.Cpu.Z80.Tests/
├── Peony.Cpu.HuC6280.Tests/
├── Peony.Cpu.V30MZ.Tests/
├── Peony.Cpu.M68000.Tests/
├── Peony.Platform.GBA.Tests/
├── Peony.Platform.Atari2600.Tests/
├── Peony.Platform.Lynx.Tests/
├── Peony.Platform.SNES.Tests/
├── Peony.Platform.GameBoy.Tests/
├── Peony.Platform.SMS.Tests/
├── Peony.Platform.PCE.Tests/
├── Peony.Platform.WonderSwan.Tests/
└── Peony.Platform.Genesis.Tests/
benchmarks/
└── Peony.Benchmarks/              # BenchmarkDotNet performance suite
```

## 📦 .peony Project Format

Peony can generate complete, self-contained disassembly projects as folders or `.peony` archives (ZIP-based).

A project includes:
- **Source code** — `.pasm` files (monolithic or bank-split) ready for Poppy
- **Include files** — Platform hardware register definitions, constants, macros
- **Analysis data** — Pansy metadata, CDL, cross-references, coverage statistics
- **Extracted assets** — Graphics (BMP), text, palettes (when available)
- **Build manifest** — `poppy.json` project file for building with Poppy
- **Documentation** — Auto-generated README with coverage stats

See [Peony Project Format](docs/PEONY-PROJECT-FORMAT.md) for the full specification.

## 🔧 Building

```bash
dotnet build
dotnet test
dotnet pack
```

## 📖 Documentation

### Architecture & Design
- [Architecture Overview](docs/ARCHITECTURE.md) — High-level design, pipeline, project structure
- [Disassembly Engine](docs/DISASSEMBLY-ENGINE.md) — Recursive descent algorithm in detail
- [Interfaces & Types](docs/INTERFACES.md) — Complete API reference for all core abstractions
- [Multi-Bank Architecture](docs/MULTI-BANK.md) — Per-platform banking guide (NES, SNES, GB, GBA, 2600, Lynx)

### Integration & Workflow
- [CLI Reference](docs/CLI-REFERENCE.md) — Complete command reference for all CLI commands
- [Disassembly Workflow](docs/DISASSEMBLY-WORKFLOW.md) — Step-by-step guide to disassembling ROMs
- [Peony Project Format](docs/PEONY-PROJECT-FORMAT.md) — `.peony` archive/folder specification
- [CDL & Pansy Integration](docs/CDL-PANSY-INTEGRATION.md) — How CDL/Pansy metadata improves disassembly
- [Nexen Game Pack Workflow](docs/NEXEN-PACK-WORKFLOW.md) — Disassemble from `.nexen-pack.zip` files
- [Improving Disassembly](docs/IMPROVING-DISASSEMBLY.md) — Guide to correcting and enhancing output

### Platform-Specific

- [Atari 2600 Asset Extraction](docs/Atari-2600-Asset-Extraction.md)
- [Platform Comparison](docs/Platform-Comparison.md)
- [Example Output](docs/Example-Output.md)
- [Lynx Memory Mapper Design](docs/lynx-memory-mapper-design.md)
- [Lynx Platform](docs/lynx-platform.md)

### Performance

- [Benchmarks](docs/BENCHMARKS.md) — Running and interpreting the BenchmarkDotNet suite
- [Static Analysis](docs/STATIC-ANALYSIS.md) — Quarantined static byte classification (opt-in via `--static-analysis`)

### Project

- [Release Notes v1.0.0](RELEASE-NOTES-1.0.0.md)
- [Session Logs](~docs/session-logs/)
- [Chat Logs](~docs/chat-logs/)

### Development Planning

- [Development Plans](~Plans/) — Technical plans, roadmaps, and research documents

## 🌷 Integrated Pipeline

Peony is the **disassembly** stage of the **Flower Toolchain** — an integrated pipeline for playing, debugging, disassembling, editing, and rebuilding retro games:

| Stage | Tool | Peony Role |
|-------|------|------------|
| 1. Play & Debug | [Nexen](https://github.com/TheAnsarya/Nexen) | — |
| 2. Disassemble | **Peony** | ROM → `.pasm` source + Pansy metadata |
| 3. Edit & Document | Editor + [Pansy](https://github.com/TheAnsarya/pansy) UI | — |
| 4. Build | [Poppy](https://github.com/TheAnsarya/poppy) | — |
| 5. Verify | [Game Garden](https://github.com/TheAnsarya/game-garden) | Roundtrip byte-identical rebuild |

See the [Integrated Pipeline Master Plan](https://github.com/TheAnsarya/pansy/blob/main/~Plans/integrated-pipeline-master-plan.md) for architecture details.

## 🤝 Related Projects

- **[Nexen](https://github.com/TheAnsarya/Nexen)** - Multi-system emulator & debugger
- **[🌼 Pansy](https://github.com/TheAnsarya/pansy)** - Universal disassembly metadata format
- **[🌸 Poppy](https://github.com/TheAnsarya/poppy)** - Multi-system assembler
- **[🌱 Game Garden](https://github.com/TheAnsarya/game-garden)** - Games disassembly & recompilation
- **[GameInfo](https://github.com/TheAnsarya/GameInfo)** - ROM hacking toolkit
- **[BPS-Patch](https://github.com/TheAnsarya/bps-patch)** - Binary patching system

## 📜 License

Unlicense - See [LICENSE](LICENSE) for details.
