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

| System | CPU | Status |
|--------|-----|--------|
| Atari 2600 | 6507 | 🎯 Priority |
| NES | 6502 | Planned |
| SNES | 65816 | Planned |
| Game Boy | SM83 | Planned |
| Atari Lynx | 65C02 | Planned |

## ✨ Features

- **Roundtrip Guarantee**: Disassembled code reassembles to identical ROM
- **Multiple Algorithms**: Linear sweep, recursive descent, speculative, hybrid
- **Platform-Aware**: Automatic register labeling, kernel detection
- **Integration**: Import from Mesen2 CDL, DiztinGUIsh, FCEUX
- **Poppy Output**: Native .pasm output format

## 🚀 Quick Start

```bash
# Disassemble an Atari 2600 ROM
peony disasm --platform atari2600 game.a26 -o game/

# Disassemble with CDL hints
peony disasm --platform nes game.nes --cdl game.cdl -o game/

# Verify roundtrip
peony verify game/ --original game.nes
```

## 📁 Project Structure

```
src/
├── Peony.Core/              # Core framework
├── Peony.Cpu.6502/          # 6502/6507/65C02 decoder
├── Peony.Cpu.65816/         # 65816 decoder
├── Peony.Cpu.SM83/          # Game Boy CPU decoder
├── Peony.Platform.Atari2600/# Atari 2600 analysis
├── Peony.Platform.NES/      # NES analysis
├── Peony.Platform.SNES/     # SNES analysis
├── Peony.Platform.GB/       # Game Boy analysis
└── Peony.Cli/               # CLI application
```

## 🔧 Building

```bash
dotnet build
dotnet test
dotnet pack
```

## 📖 Documentation

- [Architecture](docs/Architecture.md)
- [CPU Support](docs/CPU-Support.md)
- [Platform Support](docs/Platform-Support.md)
- [Output Formats](docs/Output-Formats.md)

## 🤝 Related Projects

- [Poppy](https://github.com/TheAnsarya/poppy) - Multi-system assembler
- [BPS-Patch](https://github.com/TheAnsarya/bps-patch) - Binary patching
- [GameInfo](https://github.com/TheAnsarya/GameInfo) - Game documentation

## 📜 License

MIT License - See [LICENSE](LICENSE) for details.
