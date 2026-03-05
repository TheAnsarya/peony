# 🌺 Peony - Multi-System Disassembler Framework

> The anti-Poppy: ROM → Source code conversion

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Overview

Peony is a multi-system disassembler framework designed to work alongside [Poppy](https://github.com/TheAnsarya/poppy) to provide complete bidirectional ROM ↔ Source conversion.

| Direction | Tool | Input | Output |
|-----------|------|-------|--------|
| Assembly | **Poppy** | .pasm source | ROM binary |
| Disassembly | **Peony** | ROM binary | .pasm source |

## 🎮 Supported Systems

| System | CPU | Tests | Status |
|--------|-----|-------|--------|
| **Atari 2600** | 6507 | 32 | ✅ Complete |
| **NES** | 6502 | ~50 | ✅ Complete |
| **SNES** | 65816 | ~57 | ✅ Complete |
| **Game Boy** | Sharp LR35902 | 0 | ✅ Complete |
| **GBA** | ARM7TDMI | 0 | ✅ Complete |

**Total**: 5 platforms, 171 tests passing

## ✨ Features

- **Roundtrip Guarantee**: Disassembled code reassembles to identical ROM
- **Multiple Algorithms**: Linear sweep, recursive descent, speculative, hybrid
- **Platform-Aware**: Automatic register labeling, kernel detection
- **Integration**: Import from Mesen2 CDL, DiztinGUIsh, FCEUX, Pansy metadata
- **Nexen Integration**: Import `.nexen-pack.zip` game packages with CDL + Pansy
- **Poppy Output**: Native .pasm output format

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

# Verify roundtrip (disassemble → reassemble → compare)
peony verify original.nes -r rebuilt.nes
```

## 📁 Project Structure

```
src/
├── Peony.Core/              # Core framework
├── Peony.Cpu.6502/          # 6502/6507 decoder
├── Peony.Cpu.65816/         # 65816 decoder
├── Peony.Cpu.GameBoy/       # Sharp LR35902 decoder
├── Peony.Cpu.ARM7TDMI/      # ARM7TDMI decoder (ARM + Thumb)
├── Peony.Platform.Atari2600/# Atari 2600 analysis
├── Peony.Platform.NES/      # NES analysis
├── Peony.Platform.SNES/     # SNES analysis
├── Peony.Platform.GameBoy/  # Game Boy analysis
├── Peony.Platform.GBA/      # Game Boy Advance analysis
└── Peony.Cli/               # CLI application
tests/
└── Peony.Core.Tests/        # 171 tests
```

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
- [CDL & Pansy Integration](docs/CDL-PANSY-INTEGRATION.md) — How CDL/Pansy metadata improves disassembly
- [Nexen Game Pack Workflow](docs/NEXEN-PACK-WORKFLOW.md) — Disassemble from `.nexen-pack.zip` files
- [Improving Disassembly](docs/IMPROVING-DISASSEMBLY.md) — Guide to correcting and enhancing output

### Platform-Specific
- [Atari 2600 Asset Extraction](docs/Atari-2600-Asset-Extraction.md)
- [Platform Comparison](docs/Platform-Comparison.md)

### Project
- [Session Logs](~docs/session-logs/)

## 🤝 Related Projects

- [Poppy](https://github.com/TheAnsarya/poppy) - Multi-system assembler
- [BPS-Patch](https://github.com/TheAnsarya/bps-patch) - Binary patching
- [GameInfo](https://github.com/TheAnsarya/GameInfo) - Game documentation

## 📜 License

MIT License - See [LICENSE](LICENSE) for details.
