# Nexen Debug API Research — Issue #71

## Overview

This document analyzes the Nexen emulator's debug API to understand what
capabilities are available for Peony integration via IPC.

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌───────────────┐
│ Nexen C# UI │────>│ InteropDLL   │────>│ C++ Core      │
│ (DebugApi)  │     │ (DllExport)  │     │ (Debugger)    │
└─────────────┘     └──────────────┘     └───────────────┘
       │
       │ Named Pipe (proposed)
       v
┌─────────────┐
│ Peony IPC   │
│ Client      │
└─────────────┘
```

The debug API is exposed through C++ functions in InteropDLL/DebugApiWrapper.cpp,
which are P/Invoke'd by the C# DebugApi class in UI/Interop/DebugApi.cs.

## Available APIs — Grouped by Peony Use Case

### 1. CDL Data (Highest Priority for Peony)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetCdlData(offset, length, MemoryType)` | Read CDL flags per byte | Primary data source for StaticAnalyzer |
| `GetCdlData(CpuType)` | Full PRG ROM CDL dump | Convenience wrapper |
| `GetCdlFunctions(MemoryType)` | Addresses of detected functions | Sub-entry points for disassembly |
| `GetCdlStatistics(MemoryType)` | Coverage stats (code/data/unknown %) | CDL coverage reporting |
| `SaveCdlFile(MemoryType, path)` | Save CDL to file | For offline analysis |
| `StartLightweightCdl()` | Start low-overhead CDL recording | ~15ns/instruction |
| `StopLightweightCdl()` | Stop CDL recording | |
| `ResetCdl(MemoryType)` | Clear all CDL data | |

**CDL Flags (CdlFlags enum):**
- `Code = 0x01`
- `Data = 0x02`
- `JumpTarget = 0x04`
- `SubEntryPoint = 0x08`
- `Drawn = 0x10` (graphics accessed by PPU)
- `Read = 0x20` (read by CPU as data)

### 2. Memory Access (High Priority)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetMemoryState(MemoryType)` | Full dump of any memory region | PRG ROM, WRAM, SRAM, VRAM, OAM, etc. |
| `GetMemoryValues(MemoryType, start, end)` | Partial memory read | Range-based access |
| `GetMemorySize(MemoryType)` | Query region size | For validation |
| `GetMemoryValue(MemoryType, addr)` | Single byte read | |

**Key MemoryType values for NES:**
- `NesPrgRom` — PRG ROM (compare with Peony's ROM)
- `NesInternalRam` — CPU RAM ($0000-$07ff)
- `NesWorkRam` — WRAM (if mapper supports)
- `NesSaveRam` — SRAM (battery-backed)
- `NesVideoRam` — VRAM (nametables, $2000-$2fff)
- `NesSpriteRam` — OAM (sprite attributes)
- `NesPaletteRam` — Palette ($3f00-$3f1f)
- `NesChrRom` / `NesChrRam` — CHR data (tile graphics)

### 3. PPU/VRAM State (Medium Priority)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetPpuState<T>(CpuType)` | Read PPU register state | Scroll, control, mask, etc. |
| `GetTilemap(...)` | Rendered tilemap as pixel buffer | Visual nametable display |
| `GetTileView(...)` | CHR tile viewer | Pattern table display |
| `GetSpriteList(...)` | All sprites with attributes | OAM analysis |
| `GetPaletteInfo(...)` | Full palette data | Color analysis |

### 4. CPU Registers (Medium Priority)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetCpuState<T>(CpuType)` | All CPU registers | A, X, Y, SP, PC, P for 6502 |
| `GetProgramCounter(CpuType)` | Current PC | For live tracking |
| `GetCallstack(CpuType)` | Call stack frames | Subroutine analysis |

### 5. Labels/Symbols (Medium Priority — Bidirectional)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `SetLabel(addr, MemoryType, label, comment)` | Push label to debugger | Peony → Nexen |
| `ClearLabels()` | Remove all labels | Use with caution |
| `GetAbsoluteAddress(AddressInfo)` | CPU → ROM offset | Address translation |
| `GetRelativeAddress(AddressInfo, CpuType)` | ROM offset → CPU | Address translation |

### 6. Execution Trace (Low Priority)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetExecutionTrace(offset, count)` | Recent instruction trace | Code flow analysis |
| `GetExecutionTraceSize()` | Trace buffer size | |
| `GetProfilerData(CpuType)` | Per-function timing | Hot path detection |

### 7. Address Translation (Critical for Multi-Bank)

| C# Method | Purpose | Notes |
|-----------|---------|-------|
| `GetAbsoluteAddress(AddressInfo)` | CPU addr → file offset | Handles bank switching |
| `GetRelativeAddress(AddressInfo, CpuType)` | File offset → CPU addr | Reverse mapping |

These are essential for correct bank-switched ROM analysis.

## Peony Integration Strategy

### Phase 1: Offline Enhanced Analysis (Current)
- Use `.nexen-pack.zip` CDL + Pansy files
- Enhanced NexenPackLoader (completed in #69)
- StaticAnalyzer uses CDL data (completed in #61)

### Phase 2: Named Pipe Server (Requires Nexen Changes)
- Add named pipe server to Nexen's C# UI layer
- Expose subset of DebugApi via IPC protocol
- Peony connects as client

### Phase 3: Live Sync
- Real-time CDL updates from running emulator
- Peony pushes discovered labels back to Nexen
- VRAM analysis for graphics region detection

## Nexen-Side Implementation Notes

The named pipe server would live in the C# UI layer alongside `BackgroundPansyExporter`.
It would wrap DebugApi calls:

```csharp
// Proposed location: UI/Debugger/PeonyIpcServer.cs
public class PeonyIpcServer {
    private NamedPipeServerStream? _server;

    public void Start() {
        _server = new NamedPipeServerStream(
            $"nexen-debug-{Environment.ProcessId}",
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte);
    }

    private void HandleGetCdlData(BinaryReader reader, BinaryWriter writer) {
        var memType = (MemoryType)reader.ReadByte();
        var offset = reader.ReadUInt32();
        var length = reader.ReadUInt32();
        var data = DebugApi.GetCdlData(offset, length, memType);
        // Write response...
    }
}
```

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Named pipe adds overhead | Only transfer on explicit request, not polling |
| Thread safety | DebugApi is already thread-safe via mutex in C++ core |
| No ROM loaded | Check IsDebuggerRunning() before any operation |
| Version mismatch | Protocol version in handshake, graceful degradation |
| Large transfers (full ROM) | Optional DEFLATE compression flag |
