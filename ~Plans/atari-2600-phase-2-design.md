# Atari 2600 Phase 2 Design — Peony Advanced Analysis

## Goal

Add intelligent pattern detection to the Atari 2600 disassembler for graphics data, audio code, and controller type inference.

## Current State

- `Atari2600Analyzer.cs`: 44 TIA write + 14 TIA read + RIOT register labeling
- 10 bankswitching scheme detection (F8, F6, F4, 3F, E0, FE, E7, F0, UA, CV)
- `Cpu6502Decoder.cs`: Full 6502 decoder with illegal opcode support
- Basic test coverage in `Atari2600AnalyzerTests.cs`

## Phase 2 Work Items

### 1. Graphics Data Detection (#130)

**Problem:** Graphics data (sprite bitmaps, playfield patterns) stored inline in ROM gets disassembled as 6502 instructions, producing nonsense opcodes.

**Solution:** Pattern matcher that identifies common graphics data patterns:

```
; GRP0 sprite pattern — 8 bytes of data loaded sequentially
lda #$18      ; ....XX..
sta GRP0
lda #$3c      ; ..XXXX..
sta GRP0
; ... repeating pattern
```

**Detection heuristics:**

1. Sequential `LDA #imm` / `STA GRP0` or `STA GRP1` chains (8+ consecutive)
2. `LDA #imm` / `STA PF0` / `LDA #imm` / `STA PF1` / `LDA #imm` / `STA PF2` triplets
3. Data tables referenced by indexed addressing (`LDA table,X` / `STA GRP0`)

**Output:** Mark detected regions as DATA in CDL, generate labels like `sprite_data_0000`.

### 2. Bankswitching Validation (#131)

Enhance existing detection to validate that hotspot accesses are actual bank switches:

- Verify hotspot address is accessed via JMP/JSR/LDA (not just any opcode)
- Cross-reference bank switch sites with control flow
- Warn about unusual hotspot access patterns

Test each of 10 schemes with real-world ROM patterns.

### 3. Controller Type Detection (#132)

**Heuristic analysis of I/O access patterns:**

| Controller | Pattern |
|------------|---------|
| Joystick | SWCHA reads (bits 4-7 for P0, bits 0-3 for P1), INPT4/5 fire |
| Paddle | INPT0-3 reads with `BPL`/`BMI` threshold checks |
| Keypad | SWCHA writes (column select) followed by INPT0-3 reads (row scan) |
| Driving | SWCHA reads checking 2-bit gray code patterns |

**Implementation:**

1. Scan all TIA/RIOT register accesses in ROM
2. Build access pattern profile
3. Match against known controller patterns
4. Emit controller type in Pansy metadata

### 4. Audio Code Detection (#133)

**Recognize audio initialization sequences:**

```
; Common audio setup pattern
lda #$0c      ; Pure tone waveform
sta AUDC0
lda #$1f      ; Frequency
sta AUDF0
lda #$0f      ; Max volume
sta AUDV0
```

**Detection:** Sequential writes to AUDC/AUDF/AUDV register triplets.

**Output:** Label detected audio routines, annotate waveform types.

## Architecture

All detection runs as post-processing passes after initial disassembly:

```
1. Decode all instructions (Cpu6502Decoder)
2. Detect bankswitching scheme (existing)
3. Label registers (existing)
4. NEW: Graphics data detection pass
5. NEW: Audio pattern detection pass
6. NEW: Controller type inference pass
7. Generate CDL + symbols + cross-refs
8. Export Pansy metadata
```

## File Changes

- `src/Peony.Platform.Atari2600/GraphicsPatternDetector.cs` — New
- `src/Peony.Platform.Atari2600/AudioPatternDetector.cs` — New
- `src/Peony.Platform.Atari2600/ControllerTypeDetector.cs` — New
- `src/Peony.Platform.Atari2600/Atari2600Analyzer.cs` — Wire in new detectors
- `tests/Peony.Platform.Atari2600.Tests/GraphicsPatternTests.cs` — New
- `tests/Peony.Platform.Atari2600.Tests/AudioPatternTests.cs` — New
- `tests/Peony.Platform.Atari2600.Tests/ControllerDetectionTests.cs` — New
- `tests/Peony.Platform.Atari2600.Tests/BankswitchingValidationTests.cs` — New
