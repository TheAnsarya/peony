# Atari 2600 Asset Extraction Workflow

This document describes how to extract, edit, and re-insert assets from Atari 2600 ROMs using Peony and Poppy.

---

## 🎯 Overview

The Atari 2600 has a unique architecture with extremely limited resources:
- **4KB ROM** (standard, up to 512KB with bank switching)
- **128 bytes RAM** (yes, bytes!)
- **TIA (Television Interface Adapter)** - Graphics and sound
- **RIOT (RAM I/O Timer)** - RAM, I/O, and timer

Assets are tightly integrated with code due to extreme size constraints.

---

## 📊 Asset Types

### 1. Graphics Data

#### Sprite Data (Player/Missile Graphics)
- **Format**: 1 bit per pixel (PF), 8 pixels wide
- **Size**: Variable (typically 8-64 bytes per sprite frame)
- **Location**: Embedded in ROM, referenced by code
- **Registers**:
  - `GRP0` ($1b) - Player 0 graphics
  - `GRP1` ($1c) - Player 1 graphics
  - `ENAM0` ($1d) - Missile 0 enable
  - `ENAM1` ($1e) - Missile 1 enable
  - `ENABL` ($1f) - Ball enable

**Example Pattern Recognition:**
```asm
; Sprite data typically appears as sequential bytes
.byte $00, $7e, $ff, $ff, $ff, $7e, $3c, $18  ; Character sprite
.byte $81, $42, $24, $18, $18, $24, $42, $81  ; Explosion sprite
```

#### Playfield Graphics
- **Format**: 20 bits per scanline (reflected or asymmetric)
- **Registers**:
  - `PF0` ($0d) - 4 bits (mirrored)
  - `PF1` ($0e) - 8 bits
  - `PF2` ($0f) - 8 bits
- **Mode**: `CTRLPF` ($0a) bit 0 controls reflection

**Example:**
```asm
.byte $f0, $ff, $ff  ; Left half of playfield (PF0, PF1, PF2)
```

#### Color Palettes
- **Format**: NTSC/PAL color values
- **Registers**:
  - `COLUP0` ($06) - Player 0 color
  - `COLUP1` ($07) - Player 1 color
  - `COLUPF` ($08) - Playfield color
  - `COLUBK` ($09) - Background color
- **Values**: $00-$ff (hue in upper 4 bits, luminance in lower 4 bits)

**Color Table Example:**
```asm
ColorTable:
	.byte $0e  ; White
	.byte $42  ; Red
	.byte $c6  ; Green
	.byte $96  ; Blue
```

---

### 2. Sound Data

#### TIA Sound Effects
- **Channels**: 2 (AUD0, AUD1)
- **Registers**:
  - `AUDC0` ($15) / `AUDC1` ($16) - Sound control (waveform)
  - `AUDF0` ($17) / `AUDF1` ($18) - Sound frequency
  - `AUDV0` ($19) / `AUDV1` ($1a) - Sound volume

**Sound Data Pattern:**
```asm
SoundEffect:
	.byte $04, $1f, $0f  ; Control, Frequency, Volume
	.byte $04, $1c, $0d
	.byte $04, $18, $0b
	.byte $00, $00, $00  ; End
```

---

### 3. Game Data

#### Score Tables
```asm
ScoreValues:
	.byte $01, $02, $05, $10  ; Point values in BCD
```

#### Level Data
```asm
LevelConfig:
	.byte $03  ; Starting lives
	.byte $05  ; Enemy count
	.byte $20  ; Speed
```

#### Text Data (Limited)
```asm
; Text is rare on Atari 2600 due to no built-in text mode
; Usually implemented as sprite graphics
MessageChars:
	.byte $00, $7c, $c6, $fe, $c6, $c6, $c6, $00  ; "A"
	.byte $00, $fc, $c6, $fc, $c6, $c6, $fc, $00  ; "B"
```

---

## 🔍 Detection Patterns

### Identifying Graphics Data

Peony can detect graphics data by analyzing byte patterns:

1. **Sequential Sprite Frames**: 8-16 byte sequences with similar patterns
2. **Symmetry**: Mirrored byte sequences indicate reflected sprites
3. **Alignment**: Graphics often aligned to powers of 2
4. **Low Entropy**: Graphics have repeating patterns

**Detection Example:**
```csharp
// In DisassemblyEngine
bool IsLikelyGraphicsData(ReadOnlySpan<byte> data) {
	// Check for repeating 8-byte patterns
	if (data.Length >= 16 && data.Length % 8 == 0) {
		// Check entropy and alignment
		return true;
	}
	return false;
}
```

### Identifying Sound Tables

1. **Register Writes**: Code writing to AUDC/AUDF/AUDV
2. **Indexed Access**: `lda SoundTable,X`
3. **Frame-Based**: Data in triplets (control, freq, volume)
4. **Zero Termination**: Often end with $00 bytes

### Identifying Game Data

1. **BCD Values**: $00-$99 for scores
2. **Small Values**: Lives, counts typically < $10
3. **Lookup Tables**: Sequential values for calculations

---

## 🛠️ Extraction Workflow

### Step 1: Disassemble with Peony

```bash
# Disassemble ROM with automatic platform detection
peony disasm game.bin -o game.pasm

# Or specify platform and format
peony disasm game.bin -p atari2600 -f poppy -o game.pasm
```

### Step 2: Identify Asset Regions

Review disassembly output:

```asm
; Peony adds comments for data regions
SpriteData:  ; [Data: 64 bytes]
	.byte $00, $3c, $7e, $ff, $ff, $7e, $3c, $00
	.byte $18, $3c, $7e, $ff, $ff, $7e, $3c, $18
	; ... more sprite frames

ColorTable:  ; [Data: 8 bytes]
	.byte $0e, $42, $c6, $96, $2a, $d4, $7e, $f8
```

### Step 3: Export Assets

**Manual Extraction (Current):**
1. Identify byte ranges from disassembly
2. Extract using hex editor or binary tools
3. Convert to editable format (PNG for graphics)

**Planned Automation:**
```bash
# Future Peony feature
peony extract game.bin --type graphics -o assets/graphics/
peony extract game.bin --type sound -o assets/sound/
peony extract game.bin --type data -o assets/data/
```

### Step 4: Convert to Editable Formats

**Graphics → PNG:**
```python
# Python tool example
from PIL import Image
import numpy as np

def atari_sprite_to_png(sprite_bytes, width=8):
	"""Convert Atari 2600 sprite data to PNG."""
	height = len(sprite_bytes)
	img = np.zeros((height, width, 3), dtype=np.uint8)
	
	for y, byte in enumerate(sprite_bytes):
		for x in range(width):
			if byte & (0x80 >> x):
				img[y, x] = [255, 255, 255]  # White pixel
	
	Image.fromarray(img).save('sprite.png')
```

**Sound → JSON:**
```json
{
	"soundEffects": [
		{
			"name": "shoot",
			"frames": [
				{"control": 4, "frequency": 31, "volume": 15},
				{"control": 4, "frequency": 28, "volume": 13},
				{"control": 4, "frequency": 24, "volume": 11},
				{"control": 0, "frequency": 0, "volume": 0}
			]
		}
	]
}
```

**Data → JSON:**
```json
{
	"scoreValues": [1, 2, 5, 10, 20],
	"levelConfig": {
		"startingLives": 3,
		"enemyCount": 5,
		"speed": 32
	}
}
```

### Step 5: Edit Assets

- **Graphics**: Edit PNG files with any image editor (maintain 8-pixel width)
- **Sound**: Edit JSON with desired frequency/volume curves
- **Data**: Modify JSON values

### Step 6: Convert Back to Binary

```bash
# Convert PNG back to binary sprite data
png-to-atari sprite.png -o sprite.bin

# Convert JSON to binary
json-to-binary sound.json -o sound.bin
```

### Step 7: Reassemble with Poppy

```bash
# Assemble modified source back to ROM
poppy game.pasm -o game-modified.bin

# With .include directives for assets
poppy game.pasm --include-path assets/ -o game-modified.bin
```

### Step 8: Verify

```bash
# Compare with original
cmp game.bin game-modified.bin

# Or test in emulator (Stella recommended)
stella game-modified.bin
```

---

## 📝 Asset Organization

### Recommended Directory Structure

```
GameProject/
├── roms/
│   └── original.bin          # Original ROM
├── src/
│   ├── main.pasm             # Main source file
│   ├── graphics.pasm         # Graphics data includes
│   ├── sound.pasm            # Sound data includes
│   └── data.pasm             # Game data includes
├── assets/
│   ├── graphics/
│   │   ├── player.png        # Editable sprites
│   │   ├── enemies.png
│   │   └── playfield.png
│   ├── sound/
│   │   └── effects.json      # Sound effect definitions
│   └── data/
│       ├── scores.json       # Score tables
│       └── levels.json       # Level configurations
├── build/
│   └── modified.bin          # Built ROM
└── docs/
    └── assets.md             # Asset documentation
```

### Source File Organization

```asm
; main.pasm
	processor 6502
	org $f000

; Include asset binaries
	.include "graphics.pasm"
	.include "sound.pasm"
	.include "data.pasm"

; Main code
Start:
	; Game initialization
	...

; graphics.pasm
PlayerSprite:
	.incbin "assets/graphics/player.bin"

EnemySprites:
	.incbin "assets/graphics/enemies.bin"

; sound.pasm
SoundEffects:
	.incbin "assets/sound/effects.bin"

; data.pasm
ScoreTable:
	.incbin "assets/data/scores.bin"
```

---

## 🔧 Tools Reference

### Peony (Disassembler)
```bash
# Basic disassembly
peony disasm rom.bin

# With symbol file
peony disasm rom.bin --symbols game.sym

# Poppy-compatible output
peony disasm rom.bin -f poppy -o game.pasm

# All banks (for banked ROMs)
peony disasm rom.bin --all-banks
```

### Poppy (Assembler) - Future
```bash
# Assemble source
poppy game.pasm -o game.bin

# With listing file
poppy game.pasm -o game.bin -l game.lst

# Generate symbols
poppy game.pasm -o game.bin -s game.sym
```

### Helper Tools (Planned)
```bash
# Graphics conversion
atari-gfx-extract rom.bin --offset 0x1000 --length 64 -o sprite.png
atari-gfx-insert sprite.png --offset 0x1000 -o rom-modified.bin

# Sound extraction
atari-sound-extract rom.bin -o sounds.json
atari-sound-insert sounds.json -o rom-modified.bin

# Data table extraction
atari-data-extract rom.bin --table-offset 0x1f00 --length 16 -o data.json
```

---

## 🎨 Graphics Format Details

### Sprite Format (1bpp)

Each byte represents 8 horizontal pixels:
```
Byte: $ff = 11111111 = ████████
Byte: $81 = 10000001 = █      █
Byte: $c3 = 11000011 = ██    ██
```

**Example 8x8 Sprite:**
```asm
SmileSprite:
	.byte $3c  ; 00111100
	.byte $42  ; 01000010
	.byte $a5  ; 10100101
	.byte $81  ; 10000001
	.byte $a5  ; 10100101
	.byte $99  ; 10011001
	.byte $42  ; 01000010
	.byte $3c  ; 00111100
```

Renders as:
```
  ████  
 █    █ 
█ █  █ █
█      █
█ █  █ █
█  ██  █
 █    █ 
  ████  
```

### Playfield Format (20-bit)

Playfield uses 3 registers for 20 bits:
- **PF0**: 4 bits (mirrored: bits 4-7 only)
- **PF1**: 8 bits
- **PF2**: 8 bits

**Example:**
```asm
	lda #$f0    ; PF0 = 1111....
	sta PF0
	lda #$ff    ; PF1 = 11111111
	sta PF1
	lda #$ff    ; PF2 = 11111111
	sta PF2
```

Creates: `████████████████████` (20 pixels wide, reflected to 40)

---

## 🎵 Sound Format Details

### TIA Audio Registers

Each sound channel (0/1) has 3 registers:

1. **AUDC (Control)** - Waveform selection
   - `$00`: Silent
   - `$01`: 4-bit poly
   - `$02`: 4-bit poly with 15-bit poly
   - `$03`: 5-bit poly with 4-bit poly
   - `$04`: Pure tone (best for music)
   - `$05`: Pure tone
   - `$06`: 31-bit poly
   - `$07`: 5-bit poly with 5-bit poly
   - `$08`: 9-bit poly
   - `$0c`: 6-bit pure tone
   - `$0f`: 5-bit poly with 5-bit poly

2. **AUDF (Frequency)** - Pitch control
   - `$00-$1f`: Frequency divider (higher = lower pitch)

3. **AUDV (Volume)** - Amplitude
   - `$00-$0f`: Volume level

### Sound Effect Example

**Laser shot:**
```asm
	lda #$04    ; Pure tone
	sta AUDC0
	lda #$1f    ; High pitch
	sta AUDF0
	lda #$0f    ; Full volume
	sta AUDV0
	; ... gradually decrease frequency and volume
```

**Explosion:**
```asm
	lda #$08    ; 9-bit poly (noise)
	sta AUDC0
	lda #$00    ; Low pitch
	sta AUDF0
	lda #$0f    ; Full volume
	sta AUDV0
	; ... gradually decrease volume
```

---

## 📊 Data Formats

### BCD (Binary-Coded Decimal)

Atari 2600 often uses BCD for scores:
```asm
Score:
	.byte $00, $00, $00  ; 000000 (6 digits)
	
; Increment by 10 points
	sed              ; Set decimal mode
	clc
	lda Score+2
	adc #$10
	sta Score+2
	cld              ; Clear decimal mode
```

### Lookup Tables

Common for calculations:
```asm
SquareTable:
	.byte $00, $01, $04, $09, $10, $19, $24, $31
	; 0², 1², 2², 3², 4², 5², 6², 7²
```

---

## 🔄 Workflow Automation (Future)

### Build System Integration

**Makefile example:**
```makefile
all: game.bin

game.bin: src/main.pasm assets
	poppy src/main.pasm -o game.bin

assets: graphics sound data

graphics:
	png-to-atari assets/graphics/player.png -o build/player.bin

sound:
	json-to-atari assets/sound/effects.json -o build/sound.bin

data:
	json-to-atari assets/data/scores.json -o build/scores.bin

clean:
	rm -rf build/*.bin

.PHONY: all assets graphics sound data clean
```

---

## 📚 References

- [Stella Programmer's Guide](https://www.atarihq.com/danb/files/2600_Manual.pdf)
- [TIA Hardware Notes](https://www.atarihq.com/danb/files/TIA_HW_Notes.txt)
- [Atari 2600 Memory Map](https://www.atarihq.com/danb/files/2600_Map.txt)
- [DASM Assembler Manual](https://dasm-assembler.github.io/)

---

*Last Updated: 2025-01-30*
