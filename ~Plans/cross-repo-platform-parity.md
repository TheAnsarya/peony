# Cross-Repo Platform Parity Plan

## Overview

Ensure Nexen, Poppy, Pansy, and Peony all support the same set of game platforms.
The union of all platforms across the four repos defines the target platform set.

## Platform Parity Matrix (Current State)

| # | Platform | CPU | Nexen | Pansy | Poppy | Peony |
|---|----------|-----|-------|-------|-------|-------|
| 1 | NES | MOS 6502 | ✅ | ✅ | ✅ | ✅ |
| 2 | SNES | WDC 65816 | ✅ | ✅ | ✅ | ✅ |
| 3 | Game Boy | Sharp SM83/LR35902 | ✅ | ✅ | ✅ | ✅ |
| 4 | GBA | ARM7TDMI | ✅ | ✅ | 🚧 | ✅ |
| 5 | SMS | Zilog Z80 | ✅ | ✅ | ❌ | ❌ |
| 6 | PCE/TG16 | Hudson HuC6280 | ✅ | ✅ | ❌ | ❌ |
| 7 | WonderSwan | NEC V30MZ | ✅ | ✅ | ❌ | ❌ |
| 8 | Atari Lynx | WDC 65SC02 | ✅ | ✅ | ✅ | ✅ |
| 9 | Atari 2600 | MOS 6507 | ❌ | ✅ | ✅ | ✅ |
| 10 | Sega Genesis | Motorola M68000 | ❌ | ✅ | ❌ | ❌ |

Legend: ✅ Complete | 🚧 In Progress | ❌ Missing

## Gap Analysis

### Pansy (Metadata Format) — No Gaps

Already supports 30+ platforms. No work needed.

### Peony (Disassembler) — 4 Missing Platforms

| Platform | CPU Decoder Needed | Platform Analyzer Needed |
|----------|--------------------|--------------------------|
| SMS | `Peony.Cpu.Z80` | `Peony.Platform.SMS` |
| PCE/TG16 | `Peony.Cpu.HuC6280` | `Peony.Platform.PCE` |
| WonderSwan | `Peony.Cpu.V30MZ` | `Peony.Platform.WonderSwan` |
| Genesis | `Peony.Cpu.M68000` | `Peony.Platform.Genesis` |

Each platform requires:

- CPU decoder project implementing `ICpuDecoder` (opcode table, decode, control flow, targets)
- Platform analyzer project implementing `IPlatformAnalyzer` (ROM analysis, memory map, registers, entry points)
- Test project with instruction decode verification
- Solution + CLI registration
- RomLoader platform detection updates

### Poppy (Assembler) — 4 Missing Instruction Encoders

ROM builders already exist for SMS, PCE, WonderSwan, and Genesis.
What's missing is actual instruction encoding — the code generator falls back to 6502 encoding (wrong).

| Platform | ROM Builder | Instruction Encoding | Status |
|----------|-------------|----------------------|--------|
| SMS (Z80) | ✅ Complete | ❌ Falls to 6502 | Non-functional |
| PCE (HuC6280) | ✅ Complete | ❌ Falls to 6502 | Non-functional |
| WonderSwan (V30MZ) | ✅ Complete | ❌ Falls to 6502 | Non-functional |
| Genesis (M68000) | ✅ Complete | ❌ Falls to 6502 | Non-functional |

### Nexen (Emulator) — 2 Missing Systems (Long-Term Plan Only)

| Platform | CPU | Notes |
|----------|-----|-------|
| Atari 2600 | MOS 6507 | Simple system, 6502-subset CPU already in Nexen |
| Sega Genesis | M68000 | Complex system, new CPU core needed |

Emulation is a much larger job — these go to long-term roadmap only.

## Implementation Order

### Phase 1: Peony Platform Scaffolding

1. Create CPU decoder projects (Z80, HuC6280, V30MZ, M68000)
2. Create platform analyzer projects (SMS, PCE, WonderSwan, Genesis)
3. Register in CLI, RomLoader, SymbolExporter
4. Create test project scaffolding

### Phase 2: Peony CPU Decoder Implementation

1. Z80 decoder (256 base opcodes + CB/DD/ED/FD prefix tables)
2. HuC6280 decoder (65C02 superset with block transfer, timer, I/O)
3. V30MZ decoder (8086/80186 compatible, 256 opcodes + 0F prefix)
4. M68000 decoder (complex instruction encoding, multiple sizes)

### Phase 3: Poppy Instruction Encoding

1. Z80 encoding for SMS
2. HuC6280 encoding for PCE
3. V30MZ encoding for WonderSwan
4. M68000 encoding for Genesis

### Phase 4: Nexen Long-Term Planning

1. Document Atari 2600 emulation feasibility
2. Document Genesis emulation feasibility
3. Create roadmap issues
