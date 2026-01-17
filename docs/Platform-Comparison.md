# Platform Comparison

Detailed comparison of all platforms supported by Peony.

---

## 🎮 Platform Overview

| Platform | CPU | Clock Speed | RAM | ROM | Release | Manufacturer |
|----------|-----|-------------|-----|-----|---------|--------------|
| **Atari 2600** | 6507 | 1.19 MHz | 128 B | 4KB-512KB | 1977 | Atari |
| **NES** | 6502 | 1.79 MHz | 2KB | 32KB-8MB | 1983 | Nintendo |
| **SNES** | 65816 | 3.58 MHz | 128KB | 512KB-6MB | 1990 | Nintendo |
| **Game Boy** | LR35902 | 4.19 MHz | 8KB | 32KB-8MB | 1989 | Nintendo |
| **GBA** | ARM7TDMI | 16.78 MHz | 384KB | 32MB | 2001 | Nintendo |

---

## 💾 Memory Architecture

### Atari 2600
```
$0000-$007f   TIA (write)
$0000-$000d   TIA (read)
$0080-$00ff   RIOT RAM (128 bytes)
$0280-$0297   RIOT I/O
$f000-$ffff   ROM (4KB, mirrored)
```

**Bank Switching**: Extends ROM via hotspots
- F8: 8KB (2 banks)
- F6: 16KB (4 banks)
- F4: 32KB (8 banks)
- 3F: Up to 512KB (Tigervision)
- E0: 8KB (Parker Bros, 4 segments)
- FE: 8KB (Activision, D0 monitor)

### NES
```
$0000-$07ff   RAM (2KB, mirrored to $1fff)
$2000-$2007   PPU registers (mirrored to $3fff)
$4000-$4017   APU and I/O
$4020-$ffff   Cartridge space (PRG-ROM/RAM)
$8000-$ffff   PRG-ROM (32KB typical)
```

**Mappers**: 200+ types (MMC1, MMC3, etc.)
- Bank switching for PRG-ROM (16KB/32KB)
- Bank switching for CHR-ROM (4KB/8KB)
- Some have IRQ counters, RAM

### SNES
```
$000000-$1fffff  LoROM bank 0-31
$7e0000-$7fffff  WRAM (128KB)
$800000-$ffffff  HiROM / extended banks
```

**Memory Modes**:
- LoROM: $8000-$ffff per bank
- HiROM: $0000-$ffff per bank
- ExHiROM: Extended addressing

### Game Boy
```
$0000-$3fff   ROM Bank 0 (16KB, fixed)
$4000-$7fff   ROM Bank 1-n (16KB, switchable)
$8000-$9fff   VRAM (8KB)
$a000-$bfff   External RAM (8KB, switchable)
$c000-$dfff   Work RAM (8KB)
$e000-$fdff   Echo RAM (prohibited)
$fe00-$fe9f   OAM (160 bytes)
$fea0-$feff   Prohibited
$ff00-$ff7f   Hardware I/O
$ff80-$fffe   High RAM (127 bytes)
$ffff         Interrupt Enable
```

**MBC Types**:
- MBC1: 2MB ROM, 32KB RAM, mode switching
- MBC2: 256KB ROM, 512x4bit RAM
- MBC3: 2MB ROM, 32KB RAM, RTC
- MBC5: 8MB ROM, 128KB RAM
- MBC7: 2MB ROM, 256B EEPROM, accelerometer

### GBA
```
$00000000-$00003fff  BIOS (16KB)
$02000000-$0203ffff  On-board WRAM (256KB)
$03000000-$03007fff  In-chip WRAM (32KB)
$04000000-$040003ff  I/O Registers
$05000000-$050003ff  Palette RAM (1KB)
$06000000-$06017fff  VRAM (96KB)
$07000000-$070003ff  OAM (1KB)
$08000000-$09ffffff  Game Pak ROM (Wait 0)
$0a000000-$0bffffff  Game Pak ROM (Wait 1)
$0c000000-$0dffffff  Game Pak ROM (Wait 2)
$0e000000-$0effffff  Game Pak SRAM
```

---

## 🖼️ Graphics Capabilities

| Platform | Resolution | Colors | Sprites | Backgrounds | Special |
|----------|------------|--------|---------|-------------|---------|
| **Atari 2600** | 160x192 | 128 (NTSC) | 2 players + 2 missiles + ball | 20-bit playfield | Hardware collision |
| **NES** | 256x240 | 52 palette | 64 (8x8/8x16) | 4 layers | Scrolling, sprite 0 hit |
| **SNES** | 256x224-512x448 | 32,768 palette | 128 (8x8 to 64x64) | 4 layers (Mode 7: rotation) | Mode 7, HDMA, transparency |
| **Game Boy** | 160x144 | 4 shades | 40 (8x8) | 1 layer + window | Hardware scrolling |
| **GBA** | 240x160 | 32,768 palette | 128 (8x8 to 64x64) | 4 layers | Mode 7-like rotation, alpha blend |

### Graphics Formats

**Atari 2600:**
- 1 bpp sprites (8 pixels wide)
- Playfield: 20 bits (reflected or asymmetric)
- Colors: Per-object

**NES:**
- 2bpp tiles (8x8 pixels)
- 4 colors per palette (3 + transparency)
- CHR-ROM or CHR-RAM

**SNES:**
- 2/4/8bpp tiles
- 16-color palettes
- Multiple background modes (0-7)

**Game Boy:**
- 2bpp tiles (8x8 pixels)
- 4 shades of gray
- Palettes: BGP, OBP0, OBP1

**GBA:**
- 4/8bpp tiles
- 256-color palette (16 sub-palettes)
- Bitmap modes (3, 4, 5)

---

## 🔊 Sound Capabilities

| Platform | Channels | Type | Capabilities |
|----------|----------|------|--------------|
| **Atari 2600** | 2 | TIA | 4-bit volume, 5-bit frequency, 4-bit waveform |
| **NES** | 5 | APU | 2 pulse, 1 triangle, 1 noise, 1 DMC |
| **SNES** | 8 | SPC700 | Sample-based, ADSR, echo, FIR filter |
| **Game Boy** | 4 | PSG | 2 pulse, 1 wave, 1 noise |
| **GBA** | 6 | DMA + PSG | 4 DMA (PCM), 2 PSG (GB compat) |

---

## 🧠 CPU Instruction Sets

### 6507 (Atari 2600)
- **Architecture**: 8-bit 6502 variant
- **Registers**: A, X, Y, S, P
- **Address Bus**: 13-bit (8KB address space)
- **Opcodes**: ~150 (6502 subset)
- **Modes**: 13 addressing modes
- **Special**: No IRQ pin, no BRK instruction

### 6502 (NES)
- **Architecture**: 8-bit
- **Registers**: A, X, Y, S, P
- **Address Bus**: 16-bit (64KB)
- **Opcodes**: ~150 official + ~100 unofficial
- **Modes**: 13 addressing modes
- **Special**: BCD mode disabled

### 65816 (SNES)
- **Architecture**: 16-bit 6502 extension
- **Registers**: A, X, Y, S, P, D, DB, PB
- **Address Bus**: 24-bit (16MB)
- **Opcodes**: ~250
- **Modes**: 25 addressing modes
- **Special**: 8/16-bit switchable, multiple banks

### Sharp LR35902 (Game Boy)
- **Architecture**: 8-bit (Z80-like)
- **Registers**: A, B, C, D, E, H, L, SP, PC
- **Address Bus**: 16-bit (64KB)
- **Opcodes**: ~250 + ~120 CB-prefix
- **Modes**: Register, immediate, indirect, indexed
- **Special**: No shadow registers, different flags

### ARM7TDMI (GBA)
- **Architecture**: 32-bit RISC
- **Registers**: R0-R15 (R13=SP, R14=LR, R15=PC)
- **Instruction Sets**: ARM (32-bit) + Thumb (16-bit)
- **Opcodes**: ~100 ARM + ~50 Thumb
- **Modes**: User, FIQ, IRQ, Supervisor, Abort, Undefined
- **Special**: Conditional execution, barrel shifter

---

## 🔧 Peony Support Status

| Platform | Decoder | Analyzer | Tests | Bank Switching | Asset Extract | Status |
|----------|---------|----------|-------|----------------|---------------|--------|
| **Atari 2600** | ✅ 6507 | ✅ | 32 | ✅ F8/F6/F4/3F/E0/FE | 📝 Documented | ✅ Complete |
| **NES** | ✅ 6502 | ✅ | ~50 | ✅ Mapper detection | Planned | ✅ Complete |
| **SNES** | ✅ 65816 | ✅ | ~57 | ✅ LoROM/HiROM | Planned | ✅ Complete |
| **Game Boy** | ✅ LR35902 | ✅ | 0 | ✅ MBC1-7 | Planned | ✅ Complete |
| **GBA** | ✅ ARM7TDMI | ✅ | 0 | N/A (no banking) | Planned | ✅ Complete |

---

## 📊 Complexity Comparison

### Instruction Decoding Complexity

```
Simple ──────────────────────────────────────────► Complex

6507 < 6502 < LR35902 < 65816 < ARM7TDMI
```

**Factors:**
1. **6507/6502**: Simple 8-bit, fixed-width opcodes
2. **LR35902**: CB-prefix instructions add complexity
3. **65816**: 8/16-bit mode switching, longer addressing
4. **ARM7TDMI**: Dual instruction sets (ARM/Thumb), condition codes

### Platform Analysis Complexity

```
Simple ──────────────────────────────────────────► Complex

Atari 2600 < Game Boy < GBA < NES < SNES
```

**Factors:**
1. **Atari 2600**: Fixed memory map, simple bank switching
2. **Game Boy**: MBC types, but predictable
3. **GBA**: No banking, but complex memory layout
4. **NES**: 200+ mapper types
5. **SNES**: Mode switching, complex memory layouts

### Roundtrip Difficulty

```
Easy ────────────────────────────────────────────► Hard

Game Boy < GBA < Atari 2600 < NES < SNES
```

**Factors:**
1. **Game Boy**: Clean architecture, well-documented
2. **GBA**: Modern design, good tools
3. **Atari 2600**: Tight code, but simple
4. **NES**: Mapper complexity, timing-sensitive
5. **SNES**: Mode 7, complex PPU, DMA timing

---

## 🎯 Detection Patterns

### Platform Auto-Detection

Peony can auto-detect platforms from file headers:

**NES (.nes):**
```
$00-$03: "NES" + $1a
$04: PRG ROM size (16KB units)
$05: CHR ROM size (8KB units)
$06-$07: Flags (mapper, mirroring, etc.)
```

**SNES (.sfc/.smc):**
```
$ffc0-$ffd4: Internal header (title, ROM size, etc.)
$ffdc-$ffdd: Complement check
$ffde-$ffdf: Checksum
```

**Game Boy (.gb/.gbc):**
```
$0100-$0103: Entry point
$0104-$0133: Nintendo logo
$0134-$013e: Title
$013f-$0142: Manufacturer code
$0143: CGB flag
$0147: Cartridge type (MBC)
$0148: ROM size
$0149: RAM size
```

**GBA (.gba):**
```
$00-$03: Entry point (branch)
$04-$9f: Nintendo logo
$a0-$ab: Title
$ac-$af: Game code
$b0-$b1: Maker code
$b2-$bd: Fixed values and checksums
```

**Atari 2600 (.a26/.bin):**
- No header (raw ROM)
- Detection via size (2KB, 4KB, 8KB, 16KB, 32KB)
- Bank switching hotspot analysis
- Vector table check ($fffc-$ffff)

---

## 🚀 Performance Characteristics

### Disassembly Speed (estimated)

| Platform | 32KB ROM | 128KB ROM | 1MB ROM | 4MB ROM |
|----------|----------|-----------|---------|---------|
| **Atari 2600** | <1s | N/A | N/A | N/A |
| **NES** | <1s | 2-3s | 10-15s | 40-60s |
| **SNES** | <1s | 2-3s | 10-15s | 40-60s |
| **Game Boy** | <1s | 2-3s | 10-15s | 40-60s |
| **GBA** | <1s | 2-3s | 10-15s | 40-60s |

**Note**: Linear sweep algorithm, single-threaded, modern PC

### Memory Usage

| Platform | Typical ROM | Peak RAM | Symbol Table |
|----------|-------------|----------|--------------|
| **Atari 2600** | 4KB | ~10MB | ~5KB |
| **NES** | 256KB | ~50MB | ~20KB |
| **SNES** | 2MB | ~150MB | ~50KB |
| **Game Boy** | 1MB | ~100MB | ~30KB |
| **GBA** | 16MB | ~500MB | ~200KB |

---

## 🔮 Future Platforms (Planned)

| Platform | CPU | Priority | Complexity |
|----------|-----|----------|------------|
| **Sega Master System** | Z80 | Medium | Low |
| **Genesis/Mega Drive** | 68000 | High | Medium |
| **TurboGrafx-16** | HuC6280 | Low | Medium |
| **Neo Geo** | 68000 + Z80 | Low | High |
| **Atari Lynx** | 65C02 | Medium | Medium |

---

*Last Updated: 2025-01-30*
