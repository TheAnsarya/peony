# Nexen Game Pack Workflow — Disassembling from .nexen-pack.zip

> How to create a disassembly project starting from a Nexen game package.

## What is a .nexen-pack.zip?

A `.nexen-pack.zip` is Nexen's game package export format. It bundles the ROM with all debug and analysis data accumulated during emulation into a single portable archive. This is the **best possible starting point** for disassembly because it contains both the ROM and rich metadata.

### Archive Structure

```
{GameName}/
├── ROM/
│   └── {romfile}              # Original ROM file
├── SaveStates/
│   └── *.mss                  # Save state files (optional)
├── Saves/
│   └── *.sav                  # Battery save files (optional)
├── Debug/
│   ├── {romname}.cdl           # Code/Data Log (Mesen2 format)
│   ├── {romname}.pansy         # Pansy metadata file
│   └── {romname}.mlb           # Mesen label file
├── Movies/
│   └── *.msm                  # TAS movie files (optional)
├── Config/
│   ├── {romname}.cheats.json   # Cheat codes
│   ├── {romname}.game.json     # Game-specific config
│   └── {romname}.dbg.json      # Debug workspace config
└── manifest.txt                # Package metadata
```

### Key Files for Disassembly

| File | Purpose | Priority |
|------|---------|----------|
| `ROM/{romfile}` | The ROM to disassemble | **Required** |
| `Debug/{romname}.cdl` | Code/data classification from emulation | **Critical** |
| `Debug/{romname}.pansy` | Full metadata (symbols, comments, regions) | **Critical** |
| `Debug/{romname}.mlb` | Mesen-format labels | Nice to have |
| `Config/{romname}.game.json` | Platform/mapper info | Helpful |

---

## Current Workflow (Manual)

Until Peony has built-in `.nexen-pack.zip` support, follow these steps:

### Step 1: Create the Game Pack in Nexen

1. Open Nexen and load the ROM
2. **Play the game thoroughly** — every byte executed gets logged in the CDL
   - Play through different levels/areas
   - Open all menus, options, debug screens
   - Use save states to reach different game states
   - For best results: play a TAS recording
3. **File → Export Game Package** (or the keyboard shortcut)
4. The `.nexen-pack.zip` is saved to `Documents/Nexen/GamePacks/`

### Step 2: Extract the Archive

```powershell
# Extract to a working directory
$pack = "C:\Users\me\Documents\Nexen\GamePacks\Super Mario Bros (2026-01-15 14-30-00).nexen-pack.zip"
$workDir = "C:\disassembly\smb"
Expand-Archive -Path $pack -DestinationPath $workDir

# The ROM and debug files are now in subdirectories
Get-ChildItem -Recurse $workDir
```

### Step 3: Locate the Key Files

```powershell
# Find ROM
$rom = Get-ChildItem "$workDir\*\ROM\*" -File | Select-Object -First 1

# Find CDL
$cdl = Get-ChildItem "$workDir\*\Debug\*.cdl" -File | Select-Object -First 1

# Find Pansy metadata
$pansy = Get-ChildItem "$workDir\*\Debug\*.pansy" -File | Select-Object -First 1

# Find labels
$labels = Get-ChildItem "$workDir\*\Debug\*.mlb" -File | Select-Object -First 1
```

### Step 4: Run Peony Disassembly

```bash
# Full disassembly with all metadata
peony disassemble "$rom" \
  --cdl "$cdl" \
  --pansy "$pansy" \
  --all-banks \
  --output "$workDir/output/" \
  --export-pansy "$workDir/output/result.pansy"

# Or with labels fallback (if no Pansy file)
peony disassemble "$rom" \
  --cdl "$cdl" \
  --labels "$labels" \
  --all-banks \
  --output "$workDir/output/"
```

### Step 5: Verify the Output

```bash
# Check the generated .pasm file
cat "$workDir/output/game.pasm"

# Roundtrip verification (requires Poppy)
poppy assemble "$workDir/output/game.pasm" -o "$workDir/output/rebuilt.nes"
peony verify "$rom" "$workDir/output/rebuilt.nes"
```

---

## Planned Workflow (Automated)

The goal is a single command that does everything:

### peony import Command (Planned)

```bash
# Import from game pack — creates a full project
peony import "Super Mario Bros (2026-01-15).nexen-pack.zip" \
  --project-dir ./smb-disassembly/ \
  --all-banks

# This would:
# 1. Extract the zip to a temp directory
# 2. Detect platform from ROM header
# 3. Load CDL + Pansy automatically
# 4. Run disassembly with optimal settings
# 5. Create a project structure:
#    smb-disassembly/
#    ├── rom/
#    │   └── smb.nes
#    ├── source/
#    │   ├── main.pasm
#    │   ├── bank00.pasm (if multi-bank)
#    │   ├── bank01.pasm
#    │   └── ...
#    ├── metadata/
#    │   ├── game.cdl
#    │   ├── game.pansy
#    │   └── game.mlb
#    ├── output/
#    │   └── (assembled ROM goes here)
#    ├── peony.json (project config)
#    └── README.md (auto-generated project docs)
```

### peony.json Project Config (Planned)

```json
{
  "version": "1.0",
  "platform": "nes",
  "rom": {
    "path": "rom/smb.nes",
    "crc32": "d445f698",
    "size": 40976
  },
  "metadata": {
    "cdl": "metadata/game.cdl",
    "pansy": "metadata/game.pansy",
    "labels": "metadata/game.mlb"
  },
  "output": {
    "format": "poppy",
    "directory": "source/",
    "splitBanks": true
  },
  "source": {
    "nexenPack": "Super Mario Bros (2026-01-15).nexen-pack.zip",
    "importDate": "2026-01-20T10:30:00Z"
  }
}
```

---

## What Each Metadata File Contributes

### CDL File Contribution

The CDL file from Nexen captures every byte accessed during emulation:

```
Without CDL:
  - Engine guesses code vs data from static analysis
  - ~60-70% accuracy on typical NES/SNES ROMs
  - Misidentifies data tables as code (garbled instructions)
  - Misses subroutines not reachable from reset vector

With CDL:
  - Every executed byte is marked CODE
  - Every read byte is marked DATA
  - Sub-entry points reveal hidden subroutines
  - Jump targets confirm computed branch destinations
  - Graphics data (DRAWN) enables .incbin directives
```

### Pansy File Contribution

The Pansy file from Nexen goes beyond CDL:

```
Pansy adds (over CDL):
  - Named symbols from Nexen's debugger
  - User-added comments and annotations
  - Memory region definitions (ROM, RAM, VRAM, IO, SRAM, WRAM)
  - Cross-reference graph (caller → callee)
  - Platform ID and ROM CRC32 for verification
  - Project metadata (for traceability)
```

### Mesen Label File Contribution

The `.mlb` file contains simple address-to-name mappings:

```
P:8000:Reset
P:C100:ReadInput
P:C200:UpdateSprites
R:0010:player_x
R:0011:player_y
R:0012:player_state
```

These supplement Pansy labels if the Pansy file was created before label editing.

---

## Tips for Maximum CDL Coverage

### Before Exporting the Game Pack

1. **Play extensively** — The more you play, the more bytes get logged
2. **Use save states** — Load different game states to trigger different code paths
3. **Visit all menus** — Title screen, options, credits, game over
4. **Trigger edge cases** — Die, pause, use special items
5. **Use Game Genie/cheats** — Access debug menus or unused content
6. **Play multiple sessions** — CDL accumulates across sessions in Nexen
7. **Use TAS playback** — Automated inputs cover precise paths

### Checking CDL Coverage in Nexen

1. **Debug → Code/Data Logger** — Shows coverage percentage
2. **Debug → Memory Viewer** — Color-coded bytes (code = blue, data = green)
3. The CDL stats tell you what percentage of ROM has been classified

### Multiple Game Packs

You can create multiple game packs at different play stages and merge their metadata:

```bash
# Import first pack (early game)
peony import pack1.nexen-pack.zip --project-dir ./game/

# Re-import with additional CDL (late game)
peony import pack2.nexen-pack.zip --project-dir ./game/ --merge

# The merged CDL will have better coverage than either alone
```

---

## Platform-Specific Notes

### NES Games

- CDL coverage typically reaches 40-60% on a single playthrough
- Use all warp zones and special paths
- Mapper detection is automatic from iNES header
- Multi-bank games benefit greatly from `--all-banks`

### SNES Games

- Larger ROMs need more playtime for good coverage
- LoROM/HiROM detection from internal header
- Co-processor games (SA-1, SuperFX) need special handling
- CDL is critical for separating code from compressed data

### Game Boy Games

- Relatively small ROMs, good coverage in shorter sessions
- MBC type detected from cart header byte
- Bank 0 is always fully covered if any code runs

### GBA Games

- Large ROMs (8-32 MB) — coverage varies widely
- ARM/Thumb mode switching is recorded in CDL
- Most code is Thumb mode for density
- DMA operations may copy code to IWRAM — CDL tracks this

### Atari 2600 Games

- Small ROMs (2-64 KB) — usually near-complete coverage
- Bank switching scheme auto-detected from ROM size
- Hotspot addresses are automatically excluded from labels

---

## Troubleshooting

### "No CDL file found in package"

The game pack may have been exported before any gameplay. Load the ROM in Nexen, play for a while, then re-export.

### "CRC32 mismatch in Pansy file"

The Pansy file was created for a different ROM version. Re-export from Nexen with the correct ROM loaded.

### "Garbled output in bank N"

Bank N may not have been accessed during gameplay (no CDL data). Options:
1. Play more of the game and re-export
2. Use `--all-banks` to attempt static disassembly of uncovered banks
3. Manually annotate with Pansy.UI after initial disassembly

### "Invalid instruction at $XXXX"

This address contains data misidentified as code. The CDL file should prevent this, but if coverage is incomplete:
1. Add a `.data` annotation at that address
2. Re-run disassembly
3. Or use Pansy.UI to mark the region as DATA
