# Peony CLI Reference

> Complete command reference for the Peony disassembler CLI.

## Commands

| Command | Description |
|---------|-------------|
| `disasm` | Disassemble a ROM file |
| `batch` | Disassemble multiple ROM files |
| `info` | Show ROM information |
| `export` | Export symbols to various formats |
| `verify` | Verify ROM roundtrip |
| `import` | Import a Nexen game pack |
| `chr` | Extract tile graphics |
| `text` | Extract text with table files |
| `palette` | Extract color palettes |
| `tbl` | Generate/convert table files |
| `pansy` | View and inspect Pansy metadata files |

---

## disasm — Disassemble a ROM

```bash
peony disasm <rom> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<rom>` | — | required | ROM file to disassemble |
| `--output` | `-o` | stdout | Output file path |
| `--platform` | `-p` | auto | Platform: `nes`, `snes`, `gameboy`, `gba`, `atari2600`, `lynx` |
| `--format` | `-f` | `asm` | Output format: `asm`, `poppy` |
| `--all-banks` | `-b` | false | Disassemble all banks for banked ROMs |
| `--symbols` | `-s` | — | Symbol file (.json, .nl, .mlb, .sym) |
| `--cdl` | `-c` | — | CDL (Code/Data Log) file |
| `--diz` | `-d` | — | DIZ (DiztinGUIsh) project file |

### Examples

```bash
# Basic disassembly with auto-detection
peony disasm game.nes -o game.pasm

# With CDL hints from Nexen/Mesen2
peony disasm game.nes --cdl game.cdl -f poppy -o game.pasm

# All banks for banked NES ROM
peony disasm game.nes --all-banks -f poppy -o game.pasm

# SNES with symbol file
peony disasm game.sfc --symbols game.mlb -f poppy -o game.pasm
```

---

## batch — Disassemble Multiple ROMs

```bash
peony batch <input-dir> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<input-dir>` | — | required | Directory containing ROM files |
| `--output` | `-o` | `input-dir/disasm` | Output directory |
| `--pattern` | — | `*.*` | File glob pattern |
| `--format` | `-f` | `asm` | Output format: `asm`, `poppy` |

### Examples

```bash
# Disassemble all files in a directory
peony batch ./roms/ -o ./output/

# Only NES ROMs
peony batch ./roms/ --pattern "*.nes" -f poppy
```

---

## info — Show ROM Information

```bash
peony info <rom> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<rom>` | — | required | ROM file to analyze |
| `--platform` | `-p` | auto | Platform hint |

### Examples

```bash
peony info game.nes
peony info game.sfc -p snes
```

---

## export — Export Symbols

```bash
peony export <rom> --output <file> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<rom>` | — | required | ROM file to disassemble for symbols |
| `--output` | `-o` | required | Output file path |
| `--format` | `-f` | `mesen` | Format: `mesen`, `fceux`, `nogba`, `ca65`, `wla`, `bizhawk`, `pansy` |
| `--platform` | `-p` | auto | Platform hint |
| `--symbols` | `-s` | — | Additional symbol file to merge |
| `--diz` | `-d` | — | DIZ project file to merge |

### Examples

```bash
# Export as Mesen labels
peony export game.nes -o game.mlb -f mesen

# Export as Pansy metadata
peony export game.nes -o game.pansy -f pansy

# Merge existing symbols
peony export game.nes -o game.mlb -f mesen --symbols existing.sym
```

---

## verify — Roundtrip Verification

```bash
peony verify <original> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<original>` | — | required | Original ROM file |
| `--reassembled` | `-r` | — | Reassembled ROM to compare |
| `--workdir` | `-w` | — | Working directory for full roundtrip test |
| `--assembler` | `-a` | `poppy` | Assembler command for roundtrip |
| `--report` | — | — | Write verification report to file |

### Examples

```bash
# Compare original with reassembled ROM
peony verify original.nes -r rebuilt.nes

# Full roundtrip test (disassemble → assemble → compare)
peony verify original.nes -w ./roundtrip-test/

# Internal disassembly verification
peony verify original.nes

# Save report
peony verify original.nes -r rebuilt.nes --report report.txt
```

---

## import — Nexen Pack Import

```bash
peony import <pack-path> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<pack-path>` | — | required | Path to `.nexen-pack.zip` file |
| `--project-dir` | `-d` | game name | Project directory to create |
| `--all-banks` | `-b` | false | Disassemble all banks |
| `--format` | `-f` | `poppy` | Output format: `poppy`, `asm` |
| `--no-scaffold` | — | false | Skip project scaffolding |
| `--force` | — | false | Overwrite existing project directory |

### Examples

```bash
# Full import with project scaffolding
peony import "Super Mario Bros.nexen-pack.zip"

# Custom project directory
peony import game.nexen-pack.zip --project-dir ./my-project/

# All banks for banked ROM
peony import game.nexen-pack.zip --all-banks

# Just disassemble, no project structure
peony import game.nexen-pack.zip --no-scaffold

# Overwrite existing project
peony import game.nexen-pack.zip -d ./project/ --force
```

### Pipeline Steps

1. Extract `.nexen-pack.zip` and parse manifest
2. Detect platform from manifest or ROM header
3. Create project directory structure (unless `--no-scaffold`)
4. Load CDL, Pansy, and label metadata from pack
5. Run disassembly engine with all hints
6. Write `.pasm` output
7. Export Pansy metadata file

See [Nexen Pack Workflow](NEXEN-PACK-WORKFLOW.md) for detailed documentation.

---

## chr — Extract Tile Graphics

```bash
peony chr <rom> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<rom>` | — | required | ROM file |
| `--output` | `-o` | auto | Output BMP file |
| `--offset` | — | `0` | ROM offset (hex with `$` or `0x`) |
| `--size` | `-s` | auto | Size in bytes to extract |
| `--bits` | `-b` | `2` | Bits per pixel (2 for NES, 4 for SNES) |
| `--tiles-per-row` | `-t` | `16` | Tiles per row in output |
| `--palette` | `-p` | — | Palette: `grayscale`, `nes`, or custom hex |
| `--platform` | — | — | Platform hint |

---

## text — Extract Text

```bash
peony text <rom> [options]
```

Extracts text from ROM using table files for character mapping.

---

## palette — Extract Palettes

```bash
peony palette <rom> [options]
```

Extracts color palette data from ROM.

---

## tbl — Table File Operations

```bash
peony tbl <input> [options]
```

Generate or convert character mapping table files.

---

## pansy — Inspect Pansy Files

```bash
peony pansy <file> [options]
```

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `<file>` | — | required | Pansy (.pansy) file to inspect |
| `--verbose` | `-v` | false | Show detailed symbol listing |

### Examples

```bash
# View Pansy file summary
peony pansy game.pansy

# Verbose with symbol listing
peony pansy game.pansy -v
```
