# Asset Extraction Pipeline — Technical Plan

> Epic: [#168](https://github.com/TheAnsarya/peony/issues/168) — Intelligent Asset Extraction Pipeline

## Problem Statement

Peony's current asset extraction is fundamentally broken:

- `ExtractAll()` is a stub returning empty results
- CLI `chr` command requires manual offsets — blind dumps produce garbled tiles from code regions
- Palette reads from offset `$000000` (ROM header/code) — not actual palette data
- Text scanner interprets opcodes as characters, producing ~21k blocks of garbage
- Output is BMP only — should be PNG (smaller, cross-platform, transparency)

## Architecture

### Phase 1: Foundation (Issues #169, #176)

- **PNG output** via SixLabors.ImageSharp — replace all BMP output
- **Test infrastructure** — synthetic tile/palette/tilemap test data generators

### Phase 2: Detection (Issues #170, #171, #172, #173)

- **CDL-guided regions** — DRAWN flag identifies VRAM-bound data
- **Heuristic detection** — entropy, bitplane correlation, row similarity
- **Palette detection** — BGR555 validation, subpalette structure
- **Tilemap detection** — word format analysis, spatial coherence

### Phase 3: Integration (Issues #174, #175)

- **ExtractAll() implementation** — combines CDL + heuristics pipeline
- **Unified CLI command** — `peony extract <rom>` with CDL/Pansy/config hints

## Key Data Structures

### Tile Detection

```
TileRegion:
  - RomOffset (int)
  - Size (int)
  - BitDepth (2/4/8)
  - Confidence (0.0-1.0)
  - Source (CDL | Heuristic | Manual)
  - TileCount (int)
```

### Palette Detection

```
PaletteRegion:
  - RomOffset (int)
  - ColorCount (int)
  - Colors (uint[])
  - SubpaletteSize (4/16/256)
  - Confidence (0.0-1.0)
```

### Tilemap Detection

```
TilemapRegion:
  - RomOffset (int)
  - Width (32/64)
  - Height (32/64)
  - Entries (TilemapEntry[])
  - Confidence (0.0-1.0)
```

## Heuristic Algorithms

### Shannon Entropy

- Tile data: ~4.5-6.0 bits/byte
- Code: ~6.5-7.5 bits/byte
- Compressed data: ~7.0-7.9 bits/byte
- Random: ~8.0 bits/byte

### Bitplane Correlation

- Adjacent bytes in SNES tiles are different bitplanes of the same row
- Real tiles show 60-80%+ bit agreement between adjacent bytes
- Random data shows ~50% agreement

### Tile Boundary Alignment

- Real tile data starts at offsets divisible by tile size (16/32/64)
- Candidate regions score higher when aligned

## CDL Integration

CDL byte flags (Pansy spec):

| Bit | Flag | Asset Relevance |
|-----|------|-----------------|
| 5 | DRAWN | **Primary** — byte was DMA'd to VRAM = graphics/tilemap |
| 1 | DATA | Secondary — byte was read as data (may be palette/tilemap) |
| 0 | CODE | Negative — byte is code, not an asset |

Strategy:

1. Find contiguous DRAWN regions → tile candidates
2. Find DATA-only regions near DRAWN regions → palette/tilemap candidates
3. Classify by size and alignment

## PNG Output Strategy

Using SixLabors.ImageSharp:

- **Indexed PNG** for raw tilesets — preserves palette indices, smallest file size
- **RGBA PNG** for composed images (tilemap + tiles + palette rendering)
- **Palette JSON** sidecar for each tileset — maps index → SNES BGR555 → RGB

## File Changes Summary

| File | Changes |
|------|---------|
| `Peony.Core.csproj` | Add ImageSharp NuGet reference |
| `GraphicsExtractor.cs` | Add `SaveToPng()`, `PngWriter` utility, update interfaces |
| `AssetDetector.cs` | **NEW** — Heuristic detection engine |
| `CdlAssetFinder.cs` | **NEW** — CDL-guided region detection |
| `PaletteDetector.cs` | **NEW** — Smart palette detection |
| `TilemapDetector.cs` | **NEW** — Tilemap structure detection |
| `SnesChrExtractor.cs` | Implement `ExtractAll()` with CDL + heuristics |
| `Program.cs` | Add `extract` CLI command, update `chr` to default PNG |
| Tests | New test classes for each detector |
| Benchmarks | New benchmark suite for extraction pipeline |
