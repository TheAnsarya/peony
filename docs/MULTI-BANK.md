# Multi-Bank Architecture — Per-Platform Banking Guide

> How Peony handles ROM bank switching across all supported platforms.

## Why Banking Matters for Disassembly

Most retro consoles have a CPU address space smaller than the ROM. **Banking** maps different "pages" of the ROM into the CPU's address window at runtime. The disassembler must understand which bank is active to correctly map CPU addresses back to file offsets.

Getting banking wrong produces catastrophic disassembly errors — the engine decodes instructions from the wrong part of the ROM, creating a cascade of misidentified code and data.

---

## Platform Banking Overview

| Platform | Address Bits | ROM Window | Max ROM | Bank Size | Mechanisms |
|----------|-------------|------------|---------|-----------|------------|
| NES | 16-bit | $8000-$FFFF (32 KB) | 1 MB | 16-32 KB | Mapper chips (MMC1, MMC3, etc.) |
| SNES | 24-bit | Banks $00-$FF | 4-6 MB | 32-64 KB | LoROM / HiROM addressing |
| Game Boy | 16-bit | $0000-$7FFF (32 KB) | 8 MB | 16 KB | MBC chips (MBC1, MBC3, MBC5, etc.) |
| GBA | 32-bit | Linear $08000000+ | 32 MB | None | No banking needed |
| Atari 2600 | 13-bit | $F000-$FFFF (4 KB) | 64 KB | 1-4 KB | Hotspot-triggered (F8, F6, etc.) |
| Atari Lynx | 16-bit | RAM-loaded | 512 KB | Variable | Cart hardware loads to RAM |

---

## NES Banking

### Memory Map

```
$0000 ┌──────────────────┐
      │ RAM (2 KB)       │  $0000-$07FF  (mirrored to $1FFF)
$2000 ├──────────────────┤
      │ PPU Registers    │  $2000-$3FFF  (mirrored every 8 bytes)
$4000 ├──────────────────┤
      │ APU / IO         │  $4000-$401F
$4020 ├──────────────────┤
      │ Expansion ROM    │  $4020-$5FFF
$6000 ├──────────────────┤
      │ SRAM             │  $6000-$7FFF  (battery-backed)
$8000 ├──────────────────┤
      │ PRG Bank 0       │  $8000-$BFFF  ← SWITCHABLE (most mappers)
$C000 ├──────────────────┤
      │ PRG Bank 1       │  $C000-$FFFF  ← Usually FIXED to last bank
      │ Vectors at $FFFA │
$FFFF └──────────────────┘
```

### Mapper Configurations

**NROM (Mapper 0) — No Banking**
- PRG ROM: 16 KB or 32 KB mapped directly
- 16 KB PRG mirrored: $8000-$BFFF == $C000-$FFFF
- `IsInSwitchableRegion()` → always false

**MMC1 (Mapper 1) — 16 KB Switchable / 16 KB Fixed**
- $8000-$BFFF: Switchable (bank 0 through N-1)
- $C000-$FFFF: Fixed to last bank
- Bank select via serial writes to $8000-$FFFF

**UxROM (Mapper 2) — 16 KB Switchable / 16 KB Fixed**
- $8000-$BFFF: Switchable
- $C000-$FFFF: Fixed to last bank
- Bank select via write to $8000-$FFFF

**MMC3 (Mapper 4) — 8 KB Fine-Grained**
- Splits $8000-$FFFF into four 8 KB slots
- More granular banking (not yet fully supported by Peony)

### NES AddressToOffset

```
AddressToOffset(address, romLength, bank):

  prgSize = romLength - 16  (subtract iNES header)

  // Fixed to last bank ($C000-$FFFF or all of $8000-$FFFF for NROM)
  if prgSize <= 16384:                           // 16KB ROM
    return 16 + ((address - $8000) & $3FFF)      // Mirror 16KB

  if address >= $C000:                            // Fixed high bank
    return 16 + prgSize - 16384 + (address - $C000)

  // Switchable ($8000-$BFFF)
  return 16 + (bank * 16384) + (address - $8000)
```

### NES Entry Points

Read from interrupt vector table in fixed bank:
- `$FFFA-$FFFB` → NMI handler address
- `$FFFC-$FFFD` → RESET handler address (main entry)
- `$FFFE-$FFFF` → IRQ/BRK handler address

---

## SNES Banking

### Address Format

SNES uses 24-bit addressing: **Bank:Offset** ($BB:$AAAA)

```
24-bit address: $01:8000
                ├──┘├──┘
                │   └── Offset within bank (16-bit)
                └────── Bank number ($00-$FF)
```

### LoROM (Mode $20) — 32 KB Windows

```
Banks $00-$3F, $80-$BF:
$8000 ┌──────────────────┐
      │ ROM (32 KB)      │  $8000-$FFFF per bank
$FFFF └──────────────────┘

$0000-$7FFF in these banks: RAM, I/O, PPU registers

File mapping:
  offset = (bank & $7F) * $8000 + (address - $8000)
```

### HiROM (Mode $21) — 64 KB Windows

```
Banks $C0-$FF:
$0000 ┌──────────────────┐
      │ ROM (64 KB)      │  Full 64KB per bank
$FFFF └──────────────────┘

Banks $00-$3F, $80-$BF:
$8000 ┌──────────────────┐
      │ ROM (32 KB)      │  Mirror of upper half
$FFFF └──────────────────┘

File mapping (bank $C0-$FF):
  offset = (bank - $C0) * $10000 + address
File mapping (bank $00-$3F with offset >= $8000):
  offset = bank * $10000 + (address - $8000)
```

### SNES vs NES Banking

SNES doesn't have "switchable" regions in the NES sense. Banks are part of the address itself — `$01:8000` always means bank 1, offset $8000. The CPU directly addresses the bank byte.

This means `IsInSwitchableRegion()` returns **false** for SNES — every address has an explicit bank.

### SNES Entry Points

From the internal ROM header (varies by LoROM/HiROM):
- Native mode vectors at $FFE4-$FFEF (COP, BRK, ABORT, NMI, RESET, IRQ)
- Emulation mode vectors at $FFF4-$FFFF

---

## Game Boy Banking

### Memory Map

```
$0000 ┌──────────────────┐
      │ ROM Bank 0       │  $0000-$3FFF  ← ALWAYS FIXED
      │ (16 KB)          │
$4000 ├──────────────────┤
      │ ROM Bank 1-N     │  $4000-$7FFF  ← SWITCHABLE
      │ (16 KB)          │
$8000 ├──────────────────┤
      │ VRAM (8 KB)      │  $8000-$9FFF
$A000 ├──────────────────┤
      │ External RAM     │  $A000-$BFFF  ← Switchable (if MBC)
$C000 ├──────────────────┤
      │ Work RAM (8 KB)  │  $C000-$DFFF
$E000 ├──────────────────┤
      │ Echo RAM         │  $E000-$FDFF  (mirror of $C000-$DDFF)
$FE00 ├──────────────────┤
      │ OAM              │  $FE00-$FE9F
$FF00 ├──────────────────┤
      │ I/O Registers    │  $FF00-$FF7F
$FF80 ├──────────────────┤
      │ HRAM             │  $FF80-$FFFE
$FFFF │ IE Register      │
      └──────────────────┘
```

### MBC (Memory Bank Controller) Types

| MBC | Max ROM | Max RAM | Features |
|-----|---------|---------|----------|
| None | 32 KB | 0 | 2 banks, no switching |
| MBC1 | 2 MB | 32 KB | Most common early GB |
| MBC2 | 256 KB | 512 bytes | Built-in RAM |
| MBC3 | 2 MB | 32 KB | + Real-Time Clock |
| MBC5 | 8 MB | 128 KB | + Rumble option |
| MBC6 | 2 MB | 32 KB | Split bank registers |
| MBC7 | 2 MB | 256 bytes | + Accelerometer |
| HuC1 | 2 MB | 32 KB | Hudson |
| HuC3 | 2 MB | 32 KB | + RTC + IR |

### Game Boy AddressToOffset

```
AddressToOffset(address, romLength, bank):

  // Fixed Bank 0 ($0000-$3FFF)
  if address < $4000:
    return address    // Always file offset 0-$3FFF

  // Switchable Banks ($4000-$7FFF)
  if address < $8000:
    if bank < 0: bank = 1    // Default to bank 1
    return (bank * $4000) + (address - $4000)

  return -1    // Not ROM
```

### Game Boy Entry Points

- `$0100` — Main entry point (after Nintendo logo check)
- `$0040` — RST $40 vector
- `$0048` — RST $48 vector
- `$0050` — RST $50 (timer interrupt on CGB)

---

## GBA — No Banking (Linear Addressing)

### Memory Map

```
$00000000 ┌──────────────────┐
          │ BIOS (16 KB)     │  $00000000-$00003FFF
$02000000 ├──────────────────┤
          │ EWRAM (256 KB)   │  $02000000-$0203FFFF
$03000000 ├──────────────────┤
          │ IWRAM (32 KB)    │  $03000000-$03007FFF
$04000000 ├──────────────────┤
          │ I/O Registers    │  $04000000-$040003FF
$05000000 ├──────────────────┤
          │ Palette RAM      │  $05000000-$050001FF
$06000000 ├──────────────────┤
          │ VRAM (96 KB)     │  $06000000-$06017FFF
$07000000 ├──────────────────┤
          │ OAM (1 KB)       │  $07000000-$070003FF
$08000000 ├──────────────────┤
          │ ROM              │  $08000000-$09FFFFFF  (Wait State 0)
          │ (up to 32 MB)    │  $0A000000-$0BFFFFFF  (Wait State 1 mirror)
          │                  │  $0C000000-$0DFFFFFF  (Wait State 2 mirror)
$0E000000 ├──────────────────┤
          │ SRAM/Flash       │  $0E000000-$0E00FFFF
          └──────────────────┘
```

### GBA AddressToOffset

```
AddressToOffset(address, romLength):

  // ROM mapped at $08000000
  if address >= $08000000 and address < $0E000000:
    offset = (address - $08000000) & $01FFFFFF    // 32 MB mask (handles mirrors)
    return offset < romLength ? offset : -1

  return -1
```

**No banking.** `BankCount = 1`. `IsInSwitchableRegion()` returns false. The entire 32 MB ROM is linearly addressable.

### GBA Entry Points

- `$08000000` — ROM start (entry point from header)
- ARM header contains branch instruction to real entry

### ARM7TDMI Special Considerations

- **ARM mode** (32-bit instructions) and **Thumb mode** (16-bit instructions)
- The decoder must track mode switches (BX instruction)
- Most GBA game code is Thumb for code density
- Interrupt handlers are typically ARM mode

---

## Atari 2600 Banking

### Memory Map

```
$0000 ┌──────────────────┐
      │ TIA (write)      │  $0000-$003F  (graphics/sound registers)
$0080 ├──────────────────┤
      │ RAM (128 bytes)  │  $0080-$00FF
$0280 ├──────────────────┤
      │ RIOT (I/O)       │  $0280-$029F
$1000 ├──────────────────┤
      │ ROM Window       │  $1000-$1FFF  ← 4 KB visible at a time
      │ (4 KB)           │
$FFFF └──────────────────┘

Note: 13-bit address bus, so A13-A15 are ignored.
$F000-$FFFF mirrors $1000-$1FFF.
```

### Bank Switching Schemes

#### F8 (2 banks × 4 KB)

```
Hotspots: $FFF8 = bank 0, $FFF9 = bank 1
Access (read/write) to hotspot triggers bank switch.

ROM layout:
  File [0x0000-0x0FFF] → Bank 0
  File [0x1000-0x1FFF] → Bank 1

offset = (bank * 4096) + (address & $0FFF)
```

#### F6 (4 banks × 4 KB)

```
Hotspots: $FFF6-$FFF9 (bank 0-3)

ROM layout:
  File [0x0000-0x0FFF] → Bank 0
  File [0x1000-0x1FFF] → Bank 1
  File [0x2000-0x2FFF] → Bank 2
  File [0x3000-0x3FFF] → Bank 3

offset = (bank * 4096) + (address & $0FFF)
```

#### F4 (8 banks × 4 KB)

```
Hotspots: $FFF4-$FFFB (bank 0-7)
offset = (bank * 4096) + (address & $0FFF)
```

#### E0 (8 banks × 1 KB — Parker Bros)

```
Four 1 KB slots in the 4 KB ROM window:
  $F000-$F3FF: Slot 0 (switchable)
  $F400-$F7FF: Slot 1 (switchable)
  $F800-$FBFF: Slot 2 (switchable)
  $FC00-$FFFF: Slot 3 (fixed to bank 7)

Hotspots:
  $FE0-$FE7: Switch slot 0 to bank 0-7
  $FF0-$FF7: Switch slot 1 to bank 0-7
  Slot 2 uses separate hotspots
  Slot 3 is always fixed to last bank
```

#### 3F (Tigervision — 2 KB banks)

```
Upper 2 KB ($F800-$FFFF): Fixed to last bank
Lower 2 KB ($F000-$F7FF): Switchable via write to $003F

Write value & 0x03 = bank number
```

### Atari 2600 Scheme Detection

Peony auto-detects the bank switching scheme based on ROM size and hotspot patterns:

```
ROM Size → Scheme:
  2 KB  → No banking (2K cart)
  4 KB  → No banking (4K cart)
  8 KB  → F8 (2 banks)
  16 KB → F6 (4 banks)
  32 KB → F4 (8 banks)
  64 KB → F0 (16 banks)

Special cases detected by scanning ROM for hotspot access patterns.
```

---

## Atari Lynx Banking

### Memory Map

```
$0000 ┌──────────────────┐
      │ Zero Page        │  $0000-$00FF
$0100 ├──────────────────┤
      │ Stack            │  $0100-$01FF
$0200 ├──────────────────┤
      │ RAM              │  $0200-$FBFF  ← ROM loaded here
      │ (code + data)    │
$FC00 ├──────────────────┤
      │ Suzy Registers   │  $FC00-$FCFF  (hardware)
$FD00 ├──────────────────┤
      │ Mikey Registers  │  $FD00-$FDFF  (hardware)
$FE00 ├──────────────────┤
      │ Boot ROM         │  $FE00-$FFFF
      └──────────────────┘
```

### LNX File Format

```
Offset  Size  Description
$00     4     Magic: "LYNX" ($4C, $59, $4E, $58)
$04     2     Bank 0 page count (256 bytes per page)
$06     2     Bank 1 page count
$08     2     Version
$0A     32    Cart name (null-terminated ASCII)
$2A     16    Manufacturer
$3A     1     Rotation (0=none, 1=left, 2=right)
$3B     5     Reserved
$40     ...   ROM data starts here
```

### Lynx Banking Approach

The Lynx is unique — it doesn't have traditional bank switching. The boot ROM loads cartridge data into RAM through the cart hardware, and the CPU executes from RAM. Multiple "banks" in the LNX file represent separate loadable segments.

`IsInSwitchableRegion()` returns **false** — once code is loaded to RAM, it's fixed.

---

## How Banking Affects Disassembly

### The Bank Ambiguity Problem

When disassembling banked code, the same CPU address (e.g., $8000) can correspond to different file offsets depending on which bank is active. This creates ambiguity:

```
CPU Address $8000 could be:
  File offset 0x0010 (bank 0) → LDA #$80
  File offset 0x4010 (bank 1) → JSR $C000
  File offset 0x8010 (bank 2) → .db $FF, $00 (data)
```

### How Peony Resolves It

1. **CDL data** — Tells us which banks were actually switched to at runtime
2. **Bank tracking** — The engine tracks bank per `(address, bank)` tuple
3. **Fixed region identification** — Fixed banks (e.g., NES $C000-$FFFF) always map to the same offset
4. **Cross-bank calls** — When a `DetectBankSwitch()` identifies a bank change, the engine queues the target in the correct bank
5. **All-banks mode** — With `--all-banks`, disassemble every bank's entry points

### Disassembly Output

Multi-bank output uses `.bank` directives:

```asm
.system nes

.bank 0
.org $8000

reset:
    sei
    lda #$00
    sta PPUCTRL
    jsr sub_8100

sub_8100:
    ; bank 0 code
    rts

.bank 1
.org $8000

; Different code at same address, different bank
data_table:
    .db $10, $20, $30, $40

.bank 7
.org $c000
; Fixed bank (always mapped here)
nmi_handler:
    pha
    ; ...
    pla
    rti
```

---

## Best Practices

1. **Always provide CDL** — Without CDL, multi-bank disassembly is guesswork
2. **Use `--all-banks`** — Disassemble all banks to get complete coverage
3. **Verify with roundtrip** — Assemble back with Poppy and compare to original ROM
4. **Check cross-bank calls** — Review `_bankCalls` output for correctness
5. **Provide Pansy memory regions** — Helps the engine map addresses to the correct banks
6. **Start with fixed banks** — The fixed bank code is always correct; use it as an anchor
