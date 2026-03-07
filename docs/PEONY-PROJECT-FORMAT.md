# 🌺 Peony Project Format (`.peony`) — Design Specification

> A zip-based container format for comprehensive disassembly projects.

## Overview

A `.peony` file is a **zip archive** containing everything needed to represent, edit, rebuild, and share a complete disassembly project. When extracted, it forms a standard project folder that can be compiled by Poppy.

### Design Goals

1. **Self-contained** — Contains ROM, analysis data, source code, extracted assets, and metadata
2. **Poppy-compatible** — Extracted project can be built by `poppy build --project poppy.json`
3. **Roundtrip preserving** — Rebuilding produces identical ROM to the original
4. **Iterative** — Can be updated with new analysis data and re-exported
5. **Shareable** — Single file contains everything needed (except copyrighted ROM data, which is optional)

---

## File Extension

- **`.peony`** — Peony project archive (zip format)
- MIME type: `application/x-peony-project`

---

## Archive Structure

```
project-name.peony (zip archive)
│
├── peony-project.json          # Project manifest (required)
├── poppy.json                  # Poppy build manifest (required)
│
├── rom/                        # Original ROM data
│   ├── game.nes                # Original ROM file
│   └── rom-info.json           # ROM metadata (CRC32, size, platform, mapper)
│
├── src/                        # Disassembled source code
│   ├── main.pasm               # Entry point / monolithic output
│   ├── vectors.pasm            # Interrupt vectors (NMI, RESET, IRQ)
│   └── banks/                  # Per-bank source files (for banked ROMs)
│       ├── bank00.pasm
│       ├── bank01.pasm
│       └── ...
│
├── include/                    # Include files for Poppy
│   ├── hardware.inc            # Platform hardware register definitions
│   ├── constants.inc           # Game-specific constants
│   ├── macros.inc              # Reusable macros
│   └── memory-map.inc          # Memory region definitions
│
├── assets/                     # Extracted game assets
│   ├── graphics/               # Tile/sprite graphics
│   │   ├── chr-bank-0.png      # Converted CHR data
│   │   ├── chr-bank-0.chr      # Raw CHR binary
│   │   └── tilemap.json        # Tile arrangement metadata
│   ├── text/                   # Extracted text strings
│   │   ├── strings.txt         # Human-readable text dump
│   │   └── encoding.tbl        # Character encoding table
│   ├── music/                  # Music/sound data
│   │   └── sound-data.bin      # Raw sound engine data
│   └── palettes/               # Color palette data
│       └── palettes.json       # Extracted palette definitions
│
├── analysis/                   # Analysis and metadata files
│   ├── game.pansy              # Pansy metadata (symbols, comments, cross-refs)
│   ├── game.cdl                # CDL code/data log (if provided as input)
│   ├── symbols.json            # Merged symbol table
│   ├── cross-refs.json         # Cross-reference database
│   ├── data-regions.json       # Data region annotations
│   └── coverage.json           # Disassembly coverage statistics
│
├── docs/                       # Project documentation
│   ├── README.md               # Auto-generated project overview
│   ├── memory-map.md           # Memory layout documentation
│   ├── register-map.md         # Hardware register reference
│   └── disassembly-notes.md    # Analysis notes and observations
│
└── .peony/                     # Peony internal state (for re-analysis)
    ├── version                 # Format version ("1.0.0")
    ├── history.json            # Analysis history log
    └── settings.json           # Peony-specific settings
```

---

## Manifest Files

### `peony-project.json` — Peony Project Manifest

```json
{
    "formatVersion": "1.0.0",
    "name": "dragon-warrior-1",
    "displayName": "Dragon Warrior 1",
    "description": "Complete disassembly of Dragon Warrior (NES, 1989)",
    "author": "",
    "created": "2026-03-07T12:00:00Z",
    "modified": "2026-03-07T12:00:00Z",

    "rom": {
        "filename": "rom/dragon-warrior.nes",
        "platform": "nes",
        "crc32": "a9e4ea5c",
        "sha256": "...",
        "size": 81936,
        "mapper": "MMC1",
        "banks": 8,
        "includeRom": true
    },

    "analysis": {
        "pansy": "analysis/dragon-warrior.pansy",
        "cdl": "analysis/dragon-warrior.cdl",
        "symbols": "analysis/symbols.json",
        "crossRefs": "analysis/cross-refs.json",
        "coverage": "analysis/coverage.json"
    },

    "source": {
        "entry": "src/main.pasm",
        "bankFiles": [
            "src/banks/bank00.pasm",
            "src/banks/bank01.pasm"
        ],
        "includeFiles": [
            "include/hardware.inc",
            "include/constants.inc",
            "include/macros.inc",
            "include/memory-map.inc"
        ]
    },

    "assets": {
        "graphics": [
            { "path": "assets/graphics/chr-bank-0.chr", "format": "chr", "offset": 0, "size": 8192 }
        ],
        "text": [
            { "path": "assets/text/strings.txt", "encoding": "assets/text/encoding.tbl" }
        ],
        "palettes": [
            { "path": "assets/palettes/palettes.json" }
        ]
    },

    "statistics": {
        "totalBytes": 65536,
        "codeBytes": 42000,
        "dataBytes": 18000,
        "unknownBytes": 5536,
        "codeCoverage": 0.64,
        "dataCoverage": 0.27,
        "totalCoverage": 0.92,
        "labelCount": 350,
        "commentCount": 120,
        "crossRefCount": 890,
        "blockCount": 245
    },

    "settings": {
        "splitBanks": true,
        "extractAssets": true,
        "generateIncludes": true,
        "generateDocs": true,
        "includeRomInArchive": true
    }
}
```

### `poppy.json` — Poppy Build Manifest

```json
{
    "name": "dragon-warrior-1",
    "version": "1.0.0",
    "platform": "nes",
    "entry": "src/main.pasm",
    "output": "build/dragon-warrior.nes",
    "compiler": {
        "target": "nes",
        "options": {
            "optimize": false,
            "debug": false,
            "warnings": "all"
        }
    },
    "build": {
        "includePaths": ["include/"],
        "defines": {}
    },
    "metadata": {
        "description": "Disassembled by Peony",
        "author": "",
        "tags": ["disassembly", "nes", "peony"]
    }
}
```

### `rom/rom-info.json` — ROM Metadata

```json
{
    "filename": "dragon-warrior.nes",
    "platform": "nes",
    "size": 81936,
    "romSize": 65536,
    "crc32": "a9e4ea5c",
    "sha256": "...",
    "header": {
        "format": "iNES",
        "mapper": 1,
        "mapperName": "MMC1",
        "prgSize": 65536,
        "chrSize": 16384,
        "mirroring": "vertical",
        "battery": true
    },
    "detected": {
        "method": "header+extension",
        "confidence": "high"
    }
}
```

### `analysis/coverage.json` — Coverage Statistics

```json
{
    "generated": "2026-03-07T12:00:00Z",
    "totalBytes": 65536,
    "classified": {
        "code": { "bytes": 42000, "blocks": 180, "percentage": 64.1 },
        "data": { "bytes": 18000, "blocks": 45, "percentage": 27.5 },
        "graphics": { "bytes": 0, "blocks": 0, "percentage": 0.0 },
        "unknown": { "bytes": 5536, "blocks": 20, "percentage": 8.4 }
    },
    "analysis": {
        "entryPoints": 24,
        "labels": 350,
        "comments": 120,
        "crossRefs": 890,
        "pointerTables": 8
    },
    "sources": {
        "cdl": true,
        "pansy": true,
        "symbols": false,
        "diz": false
    }
}
```

---

## CLI Commands

### Create a `.peony` Project

```bash
# Full project generation from ROM + hints
peony project game.nes \
    --cdl game.cdl \
    --pansy game.pansy \
    --output game.peony \
    --name "game-disassembly" \
    --split-banks \
    --extract-assets

# Minimal project (ROM only)
peony project game.nes -o game.peony

# Extract to folder instead of archive
peony project game.nes -o ./game-project/ --no-archive

# Without including ROM in archive (for sharing without copyrighted data)
peony project game.nes -o game.peony --no-rom
```

### Open/Extract a `.peony` Project

```bash
# Extract to folder
peony open game.peony -o ./game-project/

# View project info without extracting
peony open game.peony --info

# Update analysis in existing project
peony update game.peony --pansy updated.pansy --cdl updated.cdl
```

### Build from `.peony` Project

```bash
# Extract and build in one step
peony build game.peony -o game-rebuilt.nes

# Or extract first, then use Poppy
peony open game.peony -o ./game-project/
cd game-project
poppy build --project poppy.json
```

---

## Format Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-03-07 | Initial format specification |

---

## Implementation Notes

### Zip Format

- Standard ZIP archive (no encryption)
- DEFLATE compression for text files (.pasm, .json, .md, .inc)
- STORE (no compression) for binary files (.nes, .chr, .cdl, .pansy)
- UTF-8 filenames with forward slashes

### ROM Inclusion

- ROM can be optionally excluded from archive (`--no-rom`)
- When ROM is excluded, `rom-info.json` still contains CRC32/SHA256 for verification
- On extraction, Peony can prompt for ROM location if missing

### Asset Extraction

- CHR/tile data → `.chr` (raw) + `.png` (rendered preview)
- Text → `.txt` (decoded) with `.tbl` encoding table
- Palettes → `.json` with RGB values
- Music → `.bin` (raw) — format-specific extraction is future work

### Include File Generation

- `hardware.inc` — Auto-generated from platform definitions (PPU regs, APU regs, etc.)
- `constants.inc` — Named constants from analysis (Pansy ConstantType symbols)
- `macros.inc` — Common macros (placeholder for manual additions)
- `memory-map.inc` — Memory region definitions from Pansy/analysis

---

## Related Formats

| Format | Extension | Purpose |
|--------|-----------|---------|
| Peony Project | `.peony` | Complete disassembly project archive |
| Pansy Metadata | `.pansy` | Binary metadata (symbols, CDL, cross-refs) |
| Poppy Source | `.pasm` | Assembly source code |
| Poppy Manifest | `poppy.json` | Build configuration |
