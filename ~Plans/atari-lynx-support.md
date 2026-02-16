# Atari Lynx Disassembly Support for Peony

**Created:** February 16, 2026
**Status:** Planning (Implementation Later)

## Overview

Add Atari Lynx (WDC 65SC02) disassembly support to Peony disassembler.

## Architecture

### CPU: WDC 65SC02

The Lynx uses a WDC 65SC02, which is an enhanced 65C02 with:

- All standard 6502 instructions
- New instructions: `bra`, `phx`, `phy`, `plx`, `ply`, `stz`, `trb`, `tsb`
- New addressing mode: Zero Page Indirect `(zp)`
- Accumulator mode for `inc` and `dec`: `inc a`, `dec a`
- Fixed JMP ($xxFF) page boundary bug
- New indexed indirect: `jmp ($nnnn,x)`

### Memory Map

```
$0000-$00ff   Zero Page
$0100-$01ff   Stack
$0200-$fbff   Work RAM / Program / Display
$fc00-$fcff   Suzy Registers (Sprite/Math/Input)
$fd00-$fdff   Mikey Registers (Timer/Audio/Display/IRQ)
$fe00-$ffff   Boot ROM (mappable)
```

## Implementation Plan

### Phase 1: 65SC02 Decoder

1. **Create 65SC02 instruction decoder**
   - Inherit from existing 6502 decoder
   - Add 65C02-specific instructions
   - Handle new addressing modes

2. **Instruction table**
   - All 256 opcodes
   - New 65C02 opcodes
   - Cycle counts

### Phase 2: Lynx Platform Analyzer

1. **ROM header parsing**
   - Parse 64-byte LNX header
   - Extract game name, manufacturer
   - Determine rotation flag
   - Calculate entry point

2. **Memory region detection**
   - Identify Suzy/Mikey register accesses
   - Map I/O regions
   - Detect display buffer usage

### Phase 3: Symbol Recognition

1. **Hardware register symbols**
   - Auto-label Suzy registers ($fc00-$fcff)
   - Auto-label Mikey registers ($fd00-$fdff)
   - Timer, audio, palette names

2. **Vector detection**
   - Reset vector at $fffc
   - IRQ vector at $fffe
   - Entry point from header

### Phase 4: Output Formats

1. **Poppy-compatible output**
   - `.platform "lynx"` directive
   - Include lynx.inc reference
   - Proper lowercase formatting

2. **Pansy metadata export**
   - Platform ID: 0x09 (PLATFORM_LYNX)
   - Memory regions
   - Auto-detected symbols

## Code Structure

### New Files

```
src/Peony.Core/
├── Decoders/
│   └── Decoder65SC02.cs       # 65SC02 instruction decoder
├── Platforms/
│   └── LynxPlatformAnalyzer.cs # Lynx-specific analysis
└── Headers/
    └── LnxHeaderParser.cs     # LNX ROM header parsing
```

### Decoder65SC02.cs

```csharp
namespace Peony.Core.Decoders;

/// <summary>
/// WDC 65SC02 instruction decoder for Atari Lynx disassembly.
/// </summary>
public sealed class Decoder65SC02 : Decoder6502 {
    // Additional 65C02 opcodes
    private static readonly Dictionary<byte, (string Mnemonic, AddressMode Mode, int Cycles)> Opcodes65C02 = new() {
        { 0x80, ("bra", AddressMode.Relative, 3) },
        { 0xda, ("phx", AddressMode.Implied, 3) },
        { 0x5a, ("phy", AddressMode.Implied, 3) },
        { 0xfa, ("plx", AddressMode.Implied, 4) },
        { 0x7a, ("ply", AddressMode.Implied, 4) },
        { 0x64, ("stz", AddressMode.ZeroPage, 3) },
        { 0x74, ("stz", AddressMode.ZeroPageX, 4) },
        { 0x9c, ("stz", AddressMode.Absolute, 4) },
        { 0x9e, ("stz", AddressMode.AbsoluteX, 5) },
        { 0x14, ("trb", AddressMode.ZeroPage, 5) },
        { 0x1c, ("trb", AddressMode.Absolute, 6) },
        { 0x04, ("tsb", AddressMode.ZeroPage, 5) },
        { 0x0c, ("tsb", AddressMode.Absolute, 6) },
        { 0x1a, ("inc", AddressMode.Accumulator, 2) },
        { 0x3a, ("dec", AddressMode.Accumulator, 2) },
        { 0x12, ("ora", AddressMode.ZeroPageIndirect, 5) },
        { 0x32, ("and", AddressMode.ZeroPageIndirect, 5) },
        { 0x52, ("eor", AddressMode.ZeroPageIndirect, 5) },
        { 0x72, ("adc", AddressMode.ZeroPageIndirect, 5) },
        { 0x92, ("sta", AddressMode.ZeroPageIndirect, 5) },
        { 0xb2, ("lda", AddressMode.ZeroPageIndirect, 5) },
        { 0xd2, ("cmp", AddressMode.ZeroPageIndirect, 5) },
        { 0xf2, ("sbc", AddressMode.ZeroPageIndirect, 5) },
        { 0x7c, ("jmp", AddressMode.AbsoluteIndexedIndirect, 6) },
        { 0x89, ("bit", AddressMode.Immediate, 2) },
        { 0x34, ("bit", AddressMode.ZeroPageX, 4) },
        { 0x3c, ("bit", AddressMode.AbsoluteX, 4) },
    };
    
    // Override Decode to handle 65C02 instructions
}
```

### LnxHeaderParser.cs

```csharp
namespace Peony.Core.Headers;

/// <summary>
/// Parser for Atari Lynx ROM headers (LNX format).
/// </summary>
public sealed class LnxHeaderParser {
    public const int HeaderSize = 64;
    public const uint Magic = 0x584e594c; // "LYNX" little-endian
    
    public record LnxHeader(
        ushort PageSize,
        ushort LoadAddress,
        ushort StartAddress,
        string GameName,
        string Manufacturer,
        byte Rotation
    );
    
    public static LnxHeader Parse(byte[] data) {
        // Magic check
        if (data[0] != 'L' || data[1] != 'Y' || data[2] != 'N' || data[3] != 'X') {
            throw new InvalidDataException("Invalid LNX header magic");
        }
        
        // Parse fields...
    }
}
```

## GitHub Issues

### Epic: Atari Lynx Platform Support

**Labels:** `epic`, `enhancement`, `platform`, `lynx`

### Sub-Issues:

1. **[Lynx] Implement 65SC02 instruction decoder**
   - Labels: `enhancement`, `lynx`, `decoder`
   - Priority: High
   - Create Decoder65SC02 class

2. **[Lynx] Implement LNX header parser**
   - Labels: `enhancement`, `lynx`, `headers`
   - Priority: High
   - Parse 64-byte LNX header

3. **[Lynx] Create Lynx platform analyzer**
   - Labels: `enhancement`, `lynx`, `analysis`
   - Priority: Medium
   - Memory region detection, register access

4. **[Lynx] Add hardware register symbol generation**
   - Labels: `enhancement`, `lynx`, `symbols`
   - Priority: Medium
   - Auto-label Suzy/Mikey registers

5. **[Lynx] Generate Poppy-compatible output**
   - Labels: `enhancement`, `lynx`, `output`
   - Priority: Low
   - .platform "lynx", include lynx.inc

6. **[Lynx] Documentation and examples**
   - Labels: `documentation`, `lynx`
   - Priority: Low

## External References

- [Atari Lynx Dev](https://www.monlynx.de/lynx/index.html)
- [65C02 Datasheet](https://www.westerndesigncenter.com/wdc/documentation/w65c02s.pdf)
- [Nexen Lynx Core](https://github.com/TheAnsarya/Nexen)
- [Poppy Lynx Guide](../poppy/docs/atari-lynx-guide.md)

## Notes

Implementation will be done after core 6502/65816 disassembly is stable. The 65SC02 decoder can reuse most of the 6502 infrastructure with additional opcodes.

