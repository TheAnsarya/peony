# 🦁 Atari Lynx Platform Support

## Overview

Peony provides comprehensive support for disassembling Atari Lynx ROMs, including:

- **LNX Header Parsing**: Extract cart name, manufacturer, rotation, bank info
- **65SC02 Disassembly**: Full WDC 65SC02 instruction set support
- **Hardware Registers**: Suzy and Mikey register labeling
- **Memory Regions**: RAM, hardware, and Boot ROM classification

## Quick Start

```bash
# Analyze a Lynx ROM
peony info game.lnx

# Disassemble with Lynx platform
peony disasm game.lnx -o game.pasm

# Auto-detection works for .lnx/.lyx files
peony info game.lyx  # Platform auto-detected as "lynx"
```

## LNX File Format

The LNX format is the standard Lynx cartridge format:

| Offset | Size | Field |
|--------|------|-------|
| 0x00 | 4 | Magic ("LYNX") |
| 0x04 | 2 | Bank0 page count |
| 0x06 | 2 | Bank1 page count |
| 0x08 | 2 | Version |
| 0x0A | 32 | Cart name |
| 0x2A | 16 | Manufacturer |
| 0x3A | 1 | Rotation |
| 0x3B | 5 | Reserved |

## Hardware Register Labels

### Suzy Registers ($fc00-$fcff)

Graphics coprocessor, math unit, sprite engine:

- TMPADR_L/H - Temporary address
- SPRHSIZ_L/H - Sprite horizontal size
- MATH_A through MATH_P - Math coprocessor
- SPRCTL0/1 - Sprite control
- SPRCOLL - Sprite collision

### Mikey Registers ($fd00-$fdff)

Audio, timers, UART, display control:

- TIM0BKUP through TIM7CTLB - Timer registers
- AUD0VOL through AUD3CTLB - Audio channels
- INTRST/INTSET - Interrupt control
- SYSCTL1 - System control
- DISPADR_L/H - Display address
- GREEN0-F, BLUERED0-F - Color palette

## Memory Regions

| Region | Range | Type |
|--------|-------|------|
| Zero Page | $0000-$00ff | RAM |
| Stack | $0100-$01ff | RAM |
| Work RAM | $0200-$fbff | RAM |
| Suzy | $fc00-$fcff | Hardware |
| Mikey | $fd00-$fdff | Hardware |
| Boot ROM | $fe00-$ffff | ROM |

## Platform Detection

Peony auto-detects Lynx ROMs via:

1. **File extension**: `.lnx`, `.lyx`
2. **Magic bytes**: "LYNX" at offset 0

## Related Issues

- #43 - Atari Lynx Platform Support (Epic)
- #45 - LNX ROM Header Parser ✅
- #46 - Lynx Memory Mapper
- #47 - .lnx Extension Auto-Detection ✅

## Files

- [src/Peony.Platform.Lynx/LnxHeaderParser.cs](../src/Peony.Platform.Lynx/LnxHeaderParser.cs) - Header parsing
- [src/Peony.Platform.Lynx/LynxAnalyzer.cs](../src/Peony.Platform.Lynx/LynxAnalyzer.cs) - Platform analyzer
- [docs/lynx-memory-mapper-design.md](lynx-memory-mapper-design.md) - Memory mapper design
