# Example Disassembly Output

This document shows example output from Peony for various platforms.

---

## Atari 2600 Example

### Input ROM
**Game:** Simple 2KB ROM with reset vector at $fffc

### Peony Command
```bash
peony disasm example-2600.bin -p atari2600 -f poppy -o example-2600.pasm
```

### Output (example-2600.pasm)

```asm
; 🌺 Peony Disassembler v1.0
; Platform: Atari 2600
; ROM Size: 2048 bytes (2K)
; Bank Switching: None
; Entry Points: $f000

; ==============================================================================
; TIA Hardware Registers
; ==============================================================================

VSYNC   = $00  ; Vertical sync set-clear
VBLANK  = $01  ; Vertical blank set-clear
WSYNC   = $02  ; Wait for horizontal blank
RSYNC   = $03  ; Reset horizontal sync counter
NUSIZ0  = $04  ; Number-size player/missile 0
NUSIZ1  = $05  ; Number-size player/missile 1
COLUP0  = $06  ; Color-luminance player 0
COLUP1  = $07  ; Color-luminance player 1
COLUPF  = $08  ; Color-luminance playfield
COLUBK  = $09  ; Color-luminance background
GRP0    = $1b  ; Graphics player 0
GRP1    = $1c  ; Graphics player 1
ENABL   = $1f  ; Enable ball graphics

; ==============================================================================
; RIOT Hardware Registers
; ==============================================================================

SWCHA   = $0280  ; Port A data direction register
INTIM   = $0284  ; Timer interrupt
TIM64T  = $0296  ; Set timer to 64T

; ==============================================================================
; Code
; ==============================================================================

	org $f000

Start:
	sei              ; Disable interrupts
	cld              ; Clear decimal mode
	ldx #$ff         ; Initialize stack pointer
	txs
	lda #$00         ; Clear accumulator
	ldx #$00         ; Clear X register

.clearRam:
	sta $00,x        ; Clear zero page RAM
	inx
	bne .clearRam    ; Loop until X wraps to 0

MainLoop:
	lda #$02         ; VSYNC on
	sta VSYNC
	sta WSYNC        ; Wait 1 scanline
	sta WSYNC        ; Wait 1 scanline
	sta WSYNC        ; Wait 1 scanline (3 total for VSYNC)
	lda #$00
	sta VSYNC        ; VSYNC off

	lda #$2c         ; VBLANK time
	sta TIM64T       ; Set timer
	
.vblankWait:
	lda INTIM        ; Check timer
	bne .vblankWait  ; Wait for timer to expire

	lda #$00         ; VBLANK off
	sta VBLANK
	
	; Draw 192 scanlines
	ldx #192
.drawLoop:
	lda ColorTable,x ; Load color from table
	sta COLUBK       ; Set background color
	sta WSYNC        ; Wait for scanline
	dex
	bne .drawLoop

	lda #$02         ; VBLANK on (overscan)
	sta VBLANK
	ldx #30
.overscan:
	sta WSYNC
	dex
	bne .overscan
	
	jmp MainLoop     ; Next frame

; ==============================================================================
; Data
; ==============================================================================

ColorTable:
	.byte $0e, $0e, $0e, $0e  ; White
	.byte $42, $42, $42, $42  ; Red
	.byte $c6, $c6, $c6, $c6  ; Green
	.byte $96, $96, $96, $96  ; Blue
	; ... (repeated pattern)

; ==============================================================================
; Vectors
; ==============================================================================

	org $fffc
	.word Start      ; Reset vector
	.word Start      ; IRQ vector
```

---

## Game Boy Example

### Input ROM
**Game:** Simple Hello World cartridge

### Peony Command
```bash
peony disasm hello-gb.gb -p gameboy -f poppy -o hello-gb.pasm
```

### Output (hello-gb.pasm)

```asm
; 🌺 Peony Disassembler v1.0
; Platform: Game Boy
; ROM Size: 32768 bytes (32K)
; Cartridge Type: ROM ONLY
; Entry Point: $0100

; ==============================================================================
; Hardware Registers
; ==============================================================================

P1      = $ff00  ; Joypad
SB      = $ff01  ; Serial transfer data
SC      = $ff02  ; Serial transfer control
DIV     = $ff04  ; Divider register
TIMA    = $ff05  ; Timer counter
TMA     = $ff06  ; Timer modulo
TAC     = $ff07  ; Timer control
IF      = $ff0f  ; Interrupt flag
LCDC    = $ff40  ; LCD control
STAT    = $ff41  ; LCD status
SCY     = $ff42  ; Scroll Y
SCX     = $ff43  ; Scroll X
LY      = $ff44  ; LCD Y coordinate
LYC     = $ff45  ; LY compare
DMA     = $ff46  ; DMA transfer
BGP     = $ff47  ; BG palette
OBP0    = $ff48  ; OBJ palette 0
OBP1    = $ff49  ; OBJ palette 1
WY      = $ff4a  ; Window Y
WX      = $ff4b  ; Window X
IE      = $ffff  ; Interrupt enable

; ==============================================================================
; Cartridge Header
; ==============================================================================

	org $0100
	nop
	jp Start

; Nintendo logo (verified by boot ROM)
	org $0104
NintendoLogo:
	.byte $ce, $ed, $66, $66, $cc, $0d, $00, $0b
	.byte $03, $73, $00, $83, $00, $0c, $00, $0d
	.byte $00, $08, $11, $1f, $88, $89, $00, $0e
	.byte $dc, $cc, $6e, $e6, $dd, $dd, $d9, $99
	.byte $bb, $bb, $67, $63, $6e, $0e, $ec, $cc
	.byte $dd, $dc, $99, $9f, $bb, $b9, $33, $3e

Title:
	.ascii "HELLO WORLD"     ; $013f-$014e: Title
	.byte $00, $00, $00, $00, $00

Licensee:
	.byte $00, $00            ; New licensee code

CartridgeType:
	.byte $00                 ; ROM ONLY

RomSize:
	.byte $00                 ; 32KB

RamSize:
	.byte $00                 ; No RAM

Region:
	.byte $01                 ; Non-Japanese

HeaderChecksum:
	.byte $00

GlobalChecksum:
	.word $0000

; ==============================================================================
; Code
; ==============================================================================

	org $0150
Start:
	di                   ; Disable interrupts
	ld sp, $fffe         ; Initialize stack pointer
	
	; Wait for VBlank
.waitVBlank:
	ld a, [LY]           ; Load current scanline
	cp 144               ; Compare with VBlank start
	jr c, .waitVBlank    ; Loop if not VBlank

	; Turn off LCD
	xor a
	ld [LCDC], a

	; Load tiles
	ld hl, TileData
	ld de, $8000         ; VRAM tile data
	ld bc, TileDataEnd - TileData
.loadTiles:
	ld a, [hl+]
	ld [de], a
	inc de
	dec bc
	ld a, b
	or c
	jr nz, .loadTiles

	; Load tilemap
	ld hl, Tilemap
	ld de, $9800         ; Background map
	ld bc, TilemapEnd - Tilemap
.loadMap:
	ld a, [hl+]
	ld [de], a
	inc de
	dec bc
	ld a, b
	or c
	jr nz, .loadMap

	; Setup palettes
	ld a, %11100100      ; Palette: 3=black, 2=dark, 1=light, 0=white
	ld [BGP], a

	; Turn on LCD
	ld a, %10010001      ; LCD on, BG on
	ld [LCDC], a

	; Enable VBlank interrupt
	ld a, $01
	ld [IE], a
	ei

MainLoop:
	halt                 ; Wait for interrupt
	jr MainLoop

; ==============================================================================
; VBlank Interrupt Handler
; ==============================================================================

	org $0040            ; VBlank interrupt vector
VBlankHandler:
	reti

; ==============================================================================
; Data
; ==============================================================================

TileData:
	; Tile 0: Blank
	.byte $00, $00, $00, $00, $00, $00, $00, $00
	.byte $00, $00, $00, $00, $00, $00, $00, $00
	
	; Tile 1: 'H'
	.byte $ff, $00, $81, $7e, $81, $7e, $ff, $00
	.byte $81, $7e, $81, $7e, $81, $7e, $ff, $00
	
	; ... more tiles
TileDataEnd:

Tilemap:
	.byte 1, 2, 3, 3, 4, 0, 5, 4, 6, 3, 7  ; "HELLO WORLD"
	; ... rest of tilemap (32x32 tiles)
TilemapEnd:
```

---

## GBA Example

### Input ROM
**Game:** Simple ARM/Thumb demo

### Peony Command
```bash
peony disasm demo-gba.gba -p gba -f poppy -o demo-gba.pasm
```

### Output (demo-gba.pasm)

```asm
; 🌺 Peony Disassembler v1.0
; Platform: Game Boy Advance
; ROM Size: 262144 bytes (256K)
; Title: DEMO
; Game Code: ADMP
; Maker Code: 01
; Entry Point: $080000c0

; ==============================================================================
; Hardware Registers
; ==============================================================================

DISPCNT = $04000000  ; Display control
DISPSTAT = $04000002 ; Display status
VCOUNT = $04000004   ; Vertical count
BG0CNT = $04000008   ; BG0 control
KEYINPUT = $04000130 ; Key input
IE = $04000200       ; Interrupt enable
IF = $04000202       ; Interrupt flags
IME = $04000208      ; Interrupt master enable

; ==============================================================================
; ROM Header
; ==============================================================================

	org $08000000

EntryPoint:
	b Start              ; Branch to start (offset +$c0)
	
	; Nintendo logo (verified by BIOS)
	org $08000004
NintendoLogo:
	.byte $24, $ff, $ae, $51, $69, $9a, $a2, $21
	.byte $3d, $84, $82, $0a, $84, $e4, $09, $ad
	; ... (156 bytes total)

	org $080000a0
GameTitle:
	.ascii "DEMO"          ; Title (12 bytes max)
	.byte $00, $00, $00, $00, $00, $00, $00, $00

GameCode:
	.ascii "ADMP"          ; Game code

MakerCode:
	.ascii "01"            ; Maker code

FixedValue:
	.byte $96              ; Must be $96

MainUnitCode:
	.byte $00

DeviceType:
	.byte $00

Reserved:
	.byte $00, $00, $00, $00, $00, $00, $00

Version:
	.byte $00

Complement:
	.byte $00              ; Header checksum

Reserved2:
	.word $0000

; ==============================================================================
; Code (ARM mode)
; ==============================================================================

	org $080000c0
Start:
	; Initialize registers
	mov r0, #$04000000     ; I/O base
	
	; Set up display
	mov r1, #$0403         ; Mode 3, BG2 on
	str r1, [r0]           ; DISPCNT = $0403
	
	; Enable interrupts
	mov r1, #$01           ; VBlank interrupt
	str r1, [r0, #$200]    ; IE = $01
	mov r1, #$01
	str r1, [r0, #$208]    ; IME = $01
	
	; Main loop
MainLoop:
	swi $05                ; VBlankIntrWait
	b MainLoop

; ==============================================================================
; Draw Functions (Thumb mode)
; ==============================================================================

; Switch to Thumb mode
DrawPixel:
	; Parameters: r0 = x, r1 = y, r2 = color
	push {r4, r5, lr}
	mov r4, #240
	mul r3, r1, r4         ; offset = y * 240
	add r3, r3, r0         ; offset += x
	lsl r3, r3, #1         ; offset *= 2 (16-bit pixels)
	ldr r4, =VRAM
	strh r2, [r4, r3]      ; VRAM[offset] = color
	pop {r4, r5, pc}

VRAM = $06000000

; ==============================================================================
; Interrupt Handler
; ==============================================================================

VBlankHandler:
	push {r0-r3, r12, lr}
	
	; Acknowledge interrupt
	mov r0, #$04000000
	mov r1, #$01
	str r1, [r0, #$202]    ; IF = $01
	
	pop {r0-r3, r12, lr}
	bx lr
```

---

## NES Example (with Bank Switching)

### Input ROM
**Game:** 128KB MMC1 cartridge

### Peony Command
```bash
peony disasm game-nes.nes --all-banks -f poppy -o game-nes.pasm
```

### Output (game-nes.pasm)

```asm
; 🌺 Peony Disassembler v1.0
; Platform: NES
; ROM Size: 131072 bytes (128K PRG + 8K CHR)
; Mapper: MMC1 (1)
; PRG Banks: 8 x 16KB
; CHR Banks: 1 x 8KB
; Mirroring: Horizontal
; Battery: No
; Entry Points: $c000 (Bank 0), $c000 (Bank 1), ..., $fffa (vectors)

; ==============================================================================
; NES Hardware Registers
; ==============================================================================

PPUCTRL = $2000    ; PPU control
PPUMASK = $2001    ; PPU mask
PPUSTATUS = $2002  ; PPU status
OAMADDR = $2003    ; OAM address
OAMDATA = $2004    ; OAM data
PPUSCROLL = $2005  ; PPU scroll
PPUADDR = $2006    ; PPU address
PPUDATA = $2007    ; PPU data
OAMDMA = $4014     ; OAM DMA

APU_PULSE1 = $4000 ; Pulse 1 control
APU_PULSE2 = $4004 ; Pulse 2 control
APU_TRIANGLE = $4008 ; Triangle control
APU_NOISE = $400c  ; Noise control
APU_DMC = $4010    ; DMC control
APU_STATUS = $4015 ; APU status
JOYPAD1 = $4016    ; Joypad 1
JOYPAD2 = $4017    ; Joypad 2 / Frame counter

; ==============================================================================
; Bank 0 ($c000-$ffff when bank 0 loaded)
; ==============================================================================

	.bank 0
	.org $c000

Bank0_Start:
	; Initialization code
	sei
	cld
	ldx #$ff
	txs
	
	; Wait for PPU warmup
	bit PPUSTATUS
.wait1:
	bit PPUSTATUS
	bpl .wait1
.wait2:
	bit PPUSTATUS
	bpl .wait2
	
	; Clear RAM
	lda #$00
	ldx #$00
.clearRam:
	sta $0000,x
	sta $0100,x
	sta $0200,x
	sta $0300,x
	sta $0400,x
	sta $0500,x
	sta $0600,x
	sta $0700,x
	inx
	bne .clearRam
	
	; Switch to bank 1
	lda #$01
	jsr SwitchPRGBank
	jmp Bank1_Main

; ==============================================================================
; MMC1 Bank Switching
; ==============================================================================

SwitchPRGBank:
	; Input: A = bank number
	sta $8000          ; Reset
	lsr a
	sta $e000
	lsr a
	sta $e000
	lsr a
	sta $e000
	lsr a
	sta $e000
	rts

; ==============================================================================
; Bank 1 ($c000-$ffff when bank 1 loaded)
; ==============================================================================

	.bank 1
	.org $c000

Bank1_Main:
	; Main game loop
	jsr WaitForNMI
	jsr UpdateGame
	jmp Bank1_Main

WaitForNMI:
	lda #$01
	sta $00             ; NMI flag
.wait:
	lda $00
	bne .wait
	rts

UpdateGame:
	; Game logic here
	rts

; ==============================================================================
; Bank 7 (Fixed at $c000-$ffff) - Vectors and common code
; ==============================================================================

	.bank 7
	.org $c000

NMI:
	pha
	txa
	pha
	tya
	pha
	
	; NMI handler
	lda #$00
	sta $00             ; Clear NMI flag
	
	; Update PPU
	; ... (graphics updates)
	
	pla
	tay
	pla
	tax
	pla
	rti

IRQ:
	rti

; ==============================================================================
; Vectors
; ==============================================================================

	.org $fffa
	.word NMI           ; NMI vector
	.word Bank0_Start   ; Reset vector
	.word IRQ           ; IRQ vector
```

---

## Output Comparison

| Feature | Atari 2600 | Game Boy | GBA | NES |
|---------|------------|----------|-----|-----|
| **Code/Data Separation** | ✅ | ✅ | ✅ | ✅ |
| **Register Labels** | ✅ TIA/RIOT | ✅ All HW | ✅ All HW | ✅ PPU/APU |
| **Comments** | Auto-generated | Auto-generated | Auto-generated | Auto-generated |
| **Bank Support** | Scheme-aware | MBC-aware | N/A | Mapper-aware |
| **Vector Tables** | ✅ | ✅ | ✅ | ✅ |
| **Entry Points** | Auto-detected | Auto-detected | Auto-detected | Auto-detected |
| **Symbol Export** | .sym, .nl, .mlb | .sym, .nl, .mlb | .sym | .sym, .nl, .mlb |

---

*Last Updated: 2025-01-30*
