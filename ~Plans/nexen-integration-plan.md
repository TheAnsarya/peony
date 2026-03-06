# Nexen-Peony Integration Plan

## Goal

Enable Peony to consume runtime data from Nexen (CDL, VRAM, memory state, PPU info)
for producing accurate, high-quality disassembly without heuristic guessing.

## Current State

### Already Working
- **NexenPackLoader** loads `.nexen-pack.zip` exports containing ROM + CDL + Pansy + labels
- **SymbolLoader** imports CDL data, DIZ labels, Pansy symbols/comments/cross-refs
- **DisassemblyEngine** uses CDL to distinguish code from data
- **CdlLoader** parses Mesen/Nexen `.cdl` binary format

### Nexen Debug API Surface (C++ DLL exports)

Key functions for Peony integration:

#### Memory Access
- `GetMemoryState(MemoryType, buffer)` — bulk dump any memory type
- `GetMemoryValue(MemoryType, address)` — single byte read
- `GetMemoryValues(MemoryType, start, end, output)` — range read
- `GetMemorySize(MemoryType)` — size of memory region
- `GetGameMemorySize(MemoryType)` — game-specific memory size

#### CDL Data
- `GetCdlData(offset, length, MemoryType, cdlData)` — CDL flags per byte
- `GetCdlStatistics(MemoryType)` — coverage stats
- `GetCdlFunctions(MemoryType, functions[], maxSize)` — detected function entry points
- `SaveCdlFile(MemoryType, path)` — export CDL to file
- `LoadCdlFile(MemoryType, path)` — import CDL
- `StartLightweightCdl()` / `StopLightweightCdl()` — CDL recording without full debugger

#### PPU / Graphics
- `GetPpuState(state, CpuType)` — PPU register state
- `GetPpuToolsState(CpuType, state)` — PPU tools state
- `GetTileView(CpuType, options, source, srcSize, colors, buffer)` — rendered tiles
- `GetTilemap(CpuType, options, state, ppuToolsState, vram, palette, outputBuffer)` — tilemap
- `GetSpriteList(...)` — sprite data from OAM
- `GetPaletteInfo(CpuType, options)` — palette colors

#### Disassembly
- `GetDisassemblyOutput(CpuType, lineIndex, output[], rowCount)` — Nexen's own disassembly
- `GetCallstack(CpuType, callstackArray, size)` — current call stack
- `GetProfilerData(CpuType, profilerData, functionCount)` — profiling info

#### Labels
- `SetLabel(address, MemoryType, label, comment)` — set label in Nexen
- `ClearLabels()` — clear all labels

#### Address Translation
- `GetAbsoluteAddress(AddressInfo)` — relative → absolute address
- `GetRelativeAddress(AddressInfo, CpuType)` — absolute → relative address

## Integration Phases

### Phase 1: Enhanced File Exchange (Current Session)

Improve the existing pack-based workflow:

```text
Nexen                          Peony
  │                              │
  ├─ Run game with CDL ──────►  │
  ├─ Export .nexen-pack.zip ──►  │
  │                              ├─ Load pack (NexenPackLoader)
  │                              ├─ Parse CDL (CdlLoader)
  │                              ├─ Parse Pansy (SymbolLoader)
  │                              ├─ Static analysis (new!)
  │                              ├─ Disassemble (DisassemblyEngine)
  │                              └─ Output .pasm files
```

**New capabilities:**
1. Extract CDL function list from `GetCdlFunctions` data in pack
2. Use memory access counts for hot path identification
3. Use CDL DRAWN flag to identify CHR/graphics regions definitively
4. Cross-reference Pansy symbols with CDL for complete coverage

### Phase 2: Named Pipe IPC Protocol

Add real-time connection between Nexen and Peony:

```text
Protocol: JSON-RPC over named pipe "\\.\pipe\nexen-debug"

Requests Peony → Nexen:
  memory.dump { type: "PrgRom", offset: 0, length: 32768 }
  memory.read { type: "PrgRom", address: 0x8000 }
  cdl.getData { type: "PrgRom", offset: 0, length: 32768 }
  cdl.getFunctions { type: "PrgRom" }
  cdl.getStatistics { type: "PrgRom" }
  ppu.getState { cpuType: "Nes" }
  ppu.getTilemap { cpuType: "Nes", options: {...} }
  ppu.getSprites { cpuType: "Nes", options: {...} }
  ppu.getPalette { cpuType: "Nes" }
  debug.getCallstack { cpuType: "Nes" }
  debug.getProfiler { cpuType: "Nes" }
  rom.getInfo {}
  address.toAbsolute { address: 0x8000, type: "NesPrgRom" }
  address.toRelative { address: 0x100, type: "NesPrgRom" }

Notifications Nexen → Peony:
  cdl.updated { type: "PrgRom", offset: 0, length: 100 }
  execution.paused { pc: 0xc050, reason: "breakpoint" }
  execution.resumed {}
```

### Phase 3: VS Code Integration

The Poppy VS Code extension handles `.pasm` syntax. Peony adds:

1. **Memory map panel** — WebView showing ROM layout with CDL coloring
2. **Cross-reference panel** — Navigate callers/callees
3. **VRAM viewer** — Display tile/tilemap data from Nexen
4. **Coverage overlay** — CDL coverage % per function/region

## Nexen Changes Required

### Phase 1 (File Exchange)
- No Nexen changes needed — already exports packs

### Phase 2 (Named Pipe)
- Add named pipe server to Nexen's debugger module
- Register as optional debug feature (only active when debugger is open)
- JSON-RPC message handler dispatching to existing Debug API
- Configuration: enable/disable in Nexen settings

### Phase 3 (VS Code)
- No additional Nexen changes — pipe server handles everything

## MemoryType Enum Values

From Nexen's `DebugTypes.h`, key memory types for Peony:

```text
NES: NesPrgRom, NesChrRom, NesChrRam, NesInternalRam, NesSaveRam, NesWorkRam
SNES: SnesPrgRom, SnesWorkRam, SnesSaveRam, SnesVideoRam, SnesSpriteRam, SnesCgRam
GB: GbPrgRom, GbWorkRam, GbCartRam, GbVideoRam, GbSpriteRam
GBA: GbaPrgRom, GbaIntWorkRam, GbaExtWorkRam, GbaVideoRam, GbaSpriteRam, GbaPaletteRam
```

## Testing Strategy

1. **Unit tests** — Mock Nexen data, verify classification correctness
2. **Integration tests** — Load real `.nexen-pack.zip`, verify disassembly quality
3. **Roundtrip tests** — Disassemble → Poppy compile → compare ROM bytes
4. **Regression tests** — Known-good disassembly output snapshots
