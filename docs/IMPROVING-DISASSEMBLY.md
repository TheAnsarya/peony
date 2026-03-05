# Improving Disassembly — Correcting and Enhancing Output

> A practical guide to identifying problems in disassembly output and fixing them.

## The Iterative Disassembly Process

Disassembly is never a one-shot operation. The first pass produces a starting point, and you iteratively improve it:

```
  ┌─────────────────────────────────────────────────────┐
  │                                                     │
  │  ROM + CDL + Pansy                                  │
  │         │                                           │
  │         ▼                                           │
  │  ┌─────────────┐                                    │
  │  │ Disassemble  │  ← Initial pass                   │
  │  └──────┬──────┘                                    │
  │         │                                           │
  │         ▼                                           │
  │  ┌─────────────┐                                    │
  │  │ Review       │  ← Identify problems              │
  │  └──────┬──────┘                                    │
  │         │                                           │
  │         ▼                                           │
  │  ┌─────────────┐                                    │
  │  │ Annotate     │  ← Fix code/data, add labels      │
  │  └──────┬──────┘                                    │
  │         │                                           │
  │         ▼                                           │
  │  ┌─────────────┐                                    │
  │  │ Re-disassemble│  ← Better output                 │
  │  └──────┬──────┘                                    │
  │         │                                           │
  │         ▼                                           │
  │  ┌─────────────┐         ┌───────────────┐          │
  │  │ Roundtrip    │────────→│ ROM matches?  │          │
  │  │ Verify       │         └──────┬────────┘          │
  │  └─────────────┘                │                   │
  │                          Yes ───┤─── No ───┐        │
  │                                 │          │        │
  │                                 ▼          │        │
  │                          ┌───────────┐     │        │
  │                          │ Done! ✅   │     │        │
  │                          └───────────┘     │        │
  │                                            │        │
  │                                ┌───────────┘        │
  │                                │                    │
  │                                ▼                    │
  │                         Back to Review ─────────────┘
  └─────────────────────────────────────────────────────┘
```

---

## Common Disassembly Problems

### 1. Data Misidentified as Code

**Symptoms:**
- Strange or impossible instructions (e.g., unrecognized opcodes displayed as `???`)
- Instructions that don't make logical sense in context
- Jump targets into the middle of another instruction's operand
- Decreasing "readability" — code stops making sense

**Example of the problem:**
```asm
; This is actually a pointer table, not code!
    sbc ($c0,x)       ; ❌ Bytes: $e1 $c0
    inx               ; ❌ Bytes: $e8
    bcs loc_8505      ; ❌ Bytes: $b0 $03
```

**What it should be:**
```asm
pointer_table:
    .dw $c0e1         ; ✅ Pointer to $c0e1
    .dw $03b0         ; ✅ Pointer to $03b0
```

**How to fix:**
1. Mark the region as DATA in your Pansy file using Pansy.UI
2. Or add a `DataDefinition` via the Peony API:
   ```csharp
   engine.AddDataRegion(0x8500, new DataDefinition("word", 8, "pointer_table"));
   ```
3. Re-run disassembly — the engine will emit `.dw` directives instead

**Root cause:** Static analysis followed a fall-through path into data. CDL would have prevented this.

---

### 2. Code Misidentified as Data

**Symptoms:**
- Blocks of `.db` in areas that CDL marks as code
- Missing subroutines that are called by other code
- Cross-references pointing to data regions

**Example:**
```asm
; This is actually code reached by computed jump!
    .db $a9, $00, $8d, $00, $20    ; ❌ Should be code
    .db $60                         ; ❌ Should be RTS
```

**What it should be:**
```asm
hidden_sub:         ; ✅
    lda #$00        ; ✅ $a9 $00
    sta $2000       ; ✅ $8d $00 $20
    rts             ; ✅ $60
```

**How to fix:**
1. Add the address as an entry point:
   ```bash
   peony disassemble game.nes --entry-point 0x8500
   ```
2. Or add a CODE flag in the Pansy metadata
3. Or add a label (the engine will queue labeled addresses):
   ```csharp
   engine.AddUserLabel(0x8500, "hidden_sub");
   ```

**Root cause:** The subroutine is only reached via computed jump (e.g., `JMP ($1234)`) or indirect JSR. Static analysis can't resolve the target.

---

### 3. Wrong Bank Assignment

**Symptoms:**
- Code in bank N references labels that don't exist in bank N
- Cross-bank calls to nonsensical addresses
- Disassembly of one bank produces instructions that belong in another

**Example:**
```asm
.bank 2
.org $8000

; This doesn't look right — it's calling addresses in bank 0
    jsr $8100         ; ← But $8100 in bank 2 is data!
```

**How to fix:**
1. Provide Pansy memory regions that specify correct bank assignments
2. Check CDL data — it records which bank was active during execution
3. Use `--entry-point bank:address` syntax to specify banked entry points
4. Review cross-bank calls in the output for correctness

---

### 4. Missing Labels

**Symptoms:**
- Hex addresses instead of meaningful names
- Hard to follow control flow
- Multiple references to the same address with no label

**Example:**
```asm
    jsr $c120         ; ❌ What does this do?
    lda $0010         ; ❌ What variable is this?
    sta $2001         ; ✅ This one gets a register label (PPUMASK)
```

**Better:**
```asm
    jsr ReadInput     ; ✅ Labeled subroutine
    lda player_x      ; ✅ Labeled RAM variable
    sta PPUMASK        ; ✅ Register label (auto from platform)
```

**How to fix:**
1. Import existing label files (`.mlb`, `.nl`, `.sym`, `.diz`)
2. Add labels in Pansy.UI and re-export
3. Use Nexen's debugger to label addresses during debugging
4. The disassembler auto-generates `sub_XXXX` and `loc_XXXX` — rename these

---

### 5. Overlapping Code (6502 quirk)

**Symptoms:**
- An instruction's operand bytes are also valid opcodes when entered at a different offset
- Two different disassembly interpretations of the same bytes

**Example:**
```
Bytes at $8000: $2C $A9 $00

Entering at $8000:  BIT $00A9    (3 bytes: $2C $A9 $00)
Entering at $8001:  LDA #$00     (2 bytes: $A9 $00)
```

This is intentional in some 6502 games — the BIT instruction's operand ($A9 $00) is a valid LDA #$00 when the code branches to $8001 directly.

**How to handle:**
- Peony currently uses first-visitor-wins — the first path to reach these bytes determines the interpretation
- CDL can help by showing both code paths were executed
- Document overlapping regions with comments

---

## Annotation Workflow

### Using Pansy.UI

1. Open the disassembly result's `.pansy` file in Pansy.UI
2. Navigate to the problem address
3. Change classification:
   - Mark DATA regions → removes from code flow
   - Mark CODE regions → adds to entry points
   - Add symbols → meaningful names
   - Add comments → explain intent
4. Save the modified `.pansy` file
5. Re-run Peony with the updated metadata

### Using Label Files

Create or edit a `.mlb` file (Mesen format):

```
P:8000:Reset
P:8100:ReadController
P:C000:NmiHandler
P:C100:DrawSprites
R:0010:player_x
R:0011:player_y
R:0012:player_state
R:0020:scroll_x
G:0000:tile_data
```

Prefix meanings:
- `P` — PRG ROM label
- `R` — RAM label
- `G` — Register/hardware label

### Using JSON Symbol Files

```json
{
  "labels": {
    "0x8000": "Reset",
    "0x8100": "ReadController",
    "0xC000": "NmiHandler"
  },
  "comments": {
    "0x8000": "Main entry point after power-on",
    "0x8100": "Reads both controllers, stores in $0010-$0013"
  },
  "data": {
    "0x8500": { "type": "word", "count": 16, "name": "level_pointers" },
    "0x8600": { "type": "byte", "count": 256, "name": "color_table" }
  }
}
```

---

## Analysis Techniques

### Following Interrupt Vectors

Start with the three NES vectors (or platform equivalent) and work outward:

1. **RESET** ($FFFC) — Main initialization. Follow the entire init sequence.
2. **NMI** ($FFFA) — VBlank handler. Called 60 times per second. Usually updates graphics.
3. **IRQ** ($FFFE) — Interrupt handler. Used by some mappers.

Every subroutine called from these three vectors is guaranteed real code.

### Identifying Data Tables

Common data patterns to look for:

| Pattern | What It Is | Directive |
|---------|-----------|-----------|
| Repeating 2-byte values in ROM range | Pointer table | `.dw label1, label2, ...` |
| Sequential increasing values | Index/offset table | `.db 0, 1, 2, 3, ...` |
| Printable ASCII bytes | Text strings | `.text "HELLO"` |
| 8/16-byte aligned blocks | Tile/sprite data | `.incbin "tiles.chr"` |
| Values 0-15 or 0-3 | Palette/color indices | `.db` with comments |

### Tracing Computed Jumps

When you find a `JMP ($XXXX)` or `JMP (table,X)`:

1. Find the pointer table at `$XXXX`
2. Read all entries from the table
3. Each entry is a code entry point — add as labels
4. Re-disassemble to follow these new paths

```asm
; Found: JMP ($8500)
; Look at $8500:
jump_table:
    .dw handler_idle      ; state 0
    .dw handler_walking   ; state 1
    .dw handler_jumping   ; state 2
    .dw handler_falling   ; state 3

; Now add these as entry points for the next disassembly pass
```

### Using Cross-References

Cross-references reveal the call graph:

```asm
ReadController:          ; Referenced by:
                         ;   Call from $8042 (Reset)
                         ;   Call from $C010 (NmiHandler)
                         ;   Call from $C200 (GameLoop)
    lda JOY1
    ; ...
```

If a subroutine has no incoming references and isn't a vector target, it might be:
- Dead code (unused)
- Called via computed dispatch
- Code that was superseded during development

### Identifying RAM Variables

Look for patterns in load/store instructions:

```asm
; Same address used consistently → it's a variable
    lda $0010           ; Read player X position
    clc
    adc $0018           ; Add X velocity
    sta $0010           ; Store updated position

; Name it:
; $0010 = player_x
; $0018 = player_x_velocity
```

---

## Quality Checklist

Use this checklist to assess disassembly quality before considering it "done":

### Phase 1: Structure
- [ ] All interrupt vectors resolved and labeled
- [ ] All reachable subroutines have `sub_` or meaningful labels
- [ ] No `???` / unknown instructions (if present → data misclassified as code)
- [ ] Jump tables identified and entries labeled
- [ ] Data tables identified and using `.db`/`.dw` directives

### Phase 2: Annotations
- [ ] Hardware registers use platform names (PPUCTRL, not $2000)
- [ ] RAM variables have meaningful names
- [ ] Cross-references present as comments
- [ ] Key subroutines documented with block comments
- [ ] Data tables described (purpose, element format)

### Phase 3: Verification
- [ ] Roundtrip test passes (assembled output matches original ROM)
- [ ] CDL coverage is documented (% of ROM classified)
- [ ] Unclassified regions are documented as TODO
- [ ] Updated Pansy file exported for future use

### Phase 4: Documentation
- [ ] README.md in project directory explaining the ROM
- [ ] Memory map documented (variables, tables, code regions)
- [ ] Bank assignments documented for multi-bank ROMs
- [ ] Known issues listed (overlapping code, self-modifying, etc.)

---

## Advanced Techniques

### Merging Multiple CDL Files

If you have CDL from multiple play sessions:

```csharp
// Pseudocode for CDL merging
byte[] merged = new byte[romSize];
foreach (var cdlFile in cdlFiles) {
    var cdl = LoadCdl(cdlFile);
    for (int i = 0; i < romSize; i++) {
        merged[i] |= cdl[i];  // OR flags together
    }
}
// merged[] now has the union of all sessions' flags
```

### Iterative Refinement Script

```powershell
# Automated refinement loop
$rom = "game.nes"
$cdl = "game.cdl"
$pansy = "game.pansy"

# Pass 1: Basic disassembly
peony disassemble $rom --cdl $cdl --pansy $pansy -o output/ --export-pansy pass1.pansy

# Manually review and annotate pass1.pansy with Pansy.UI
# ... add labels, fix code/data, add comments ...

# Pass 2: Improved disassembly with annotations
peony disassemble $rom --cdl $cdl --pansy pass1-annotated.pansy -o output/ --export-pansy pass2.pansy

# Roundtrip verify
poppy assemble output/game.pasm -o rebuilt.nes
peony verify $rom rebuilt.nes
```

### Comparing Disassembly Versions

When you re-disassemble with improved metadata, diff the outputs:

```powershell
# Compare two passes
diff output-pass1/game.pasm output-pass2/game.pasm

# Check for:
# - Fewer .db (data) lines → more code discovered
# - More labeled addresses → better readability
# - Fewer ??? instructions → better code/data separation
```

---

## Platform-Specific Tips

### NES

- PPU register writes ($2000-$2007) are critical — they control all graphics
- Bank switching usually happens in the fixed bank ($C000+)
- CHR ROM (graphics) is separate from PRG ROM — don't try to disassemble it as code
- DPCM samples are in PRG ROM but are data — CDL marks them with READ flag

### SNES

- DMA transfers ($420B) move large data blocks — follow the source/dest addresses
- Mode 7 graphics data can be huge — CDL DRAWN flag helps
- Co-processor games (SA-1, SuperFX) have two CPUs — handle separately
- Compressed data is common — CDL READ flag reveals compressed blocks

### Game Boy

- RST instructions ($C7, $CF, etc.) are fast calls to fixed addresses — always label them
- Memory bank controller writes ($2000-$3FFF for MBC1) trigger bank switches
- HALT instruction waits for interrupt — marks end of loop
- GB vs GBC: CGB has double-speed mode and VRAM banking

### GBA

- SWI (software interrupt) calls BIOS functions — document the function number
- Thumb/ARM mode switches at BX/BLX instructions — track current mode
- IWRAM code (copied via DMA) needs separate handling
- Link cable / multiplayer code may be unreachable in single-player CDL
