# Nexen-Peony IPC Protocol Specification

## Overview

This document specifies the inter-process communication (IPC) protocol between
the Nexen emulator and the Peony disassembler. The protocol enables Peony to
access live emulation data from Nexen for enhanced static analysis.

## Transport Layer

**Named Pipes** (Windows) / **Unix Domain Sockets** (Linux/macOS)

- Pipe name: `nexen-debug-{pid}` where `{pid}` is the Nexen process ID
- Discovery: Peony scans for available pipes matching the pattern
- Connection: Single client per pipe (Peony connects, Nexen listens)
- Encoding: Little-endian binary, UTF-8 for strings

## Message Format

All messages use a length-prefixed binary envelope:

```
+--------+--------+--------+--------+--------+...
| Length  | MsgId  | Type   | Flags  | Payload ...
| uint32  | uint32  | uint16  | uint16  |
+--------+--------+--------+--------+--------+...
```

- **Length**: Total message size in bytes (including header)
- **MsgId**: Unique message identifier for request/response matching
- **Type**: Message type (see Message Types below)
- **Flags**: Bitfield (0x01 = compressed payload, 0x02 = response, 0x04 = error)

## Message Types

### Connection Management

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0001 | HANDSHAKE | Peony → Nexen | Initial connection with version negotiation |
| 0x0002 | HANDSHAKE_ACK | Nexen → Peony | Confirm connection with emulator info |
| 0x0003 | DISCONNECT | Either | Graceful disconnect |
| 0x0004 | HEARTBEAT | Either | Keep-alive ping |

### Memory Access

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0100 | GET_MEMORY | Peony → Nexen | Read memory region |
| 0x0101 | GET_MEMORY_RESP | Nexen → Peony | Memory data response |
| 0x0102 | GET_MEMORY_SIZE | Peony → Nexen | Query memory region size |
| 0x0103 | GET_MEMORY_SIZE_RESP | Nexen → Peony | Memory size response |

### CPU State

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0200 | GET_CPU_STATE | Peony → Nexen | Read CPU registers |
| 0x0201 | GET_CPU_STATE_RESP | Nexen → Peony | CPU register values |
| 0x0202 | GET_PROGRAM_COUNTER | Peony → Nexen | Get current PC |
| 0x0203 | GET_PROGRAM_COUNTER_RESP | Nexen → Peony | PC value |

### PPU/VRAM Access

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0300 | GET_PPU_STATE | Peony → Nexen | Read PPU registers |
| 0x0301 | GET_PPU_STATE_RESP | Nexen → Peony | PPU register values |
| 0x0302 | GET_VRAM | Peony → Nexen | Read VRAM data |
| 0x0303 | GET_VRAM_RESP | Nexen → Peony | VRAM data response |
| 0x0304 | GET_PALETTE | Peony → Nexen | Read palette data |
| 0x0305 | GET_PALETTE_RESP | Nexen → Peony | Palette data response |
| 0x0306 | GET_SPRITE_LIST | Peony → Nexen | Read OAM/sprite data |
| 0x0307 | GET_SPRITE_LIST_RESP | Nexen → Peony | Sprite data response |

### CDL (Code/Data Log)

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0400 | GET_CDL_DATA | Peony → Nexen | Read CDL flags |
| 0x0401 | GET_CDL_DATA_RESP | Nexen → Peony | CDL data response |
| 0x0402 | GET_CDL_STATS | Peony → Nexen | Get CDL statistics |
| 0x0403 | GET_CDL_STATS_RESP | Nexen → Peony | CDL statistics |
| 0x0404 | GET_CDL_FUNCTIONS | Peony → Nexen | Get detected function list |
| 0x0405 | GET_CDL_FUNCTIONS_RESP | Nexen → Peony | Function addresses |

### Labels & Symbols

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0500 | SET_LABEL | Peony → Nexen | Push label to Nexen debugger |
| 0x0501 | SET_LABEL_ACK | Nexen → Peony | Label set confirmation |
| 0x0502 | GET_LABELS | Peony → Nexen | Request all labels |
| 0x0503 | GET_LABELS_RESP | Nexen → Peony | Label list |

### Address Translation

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0600 | GET_ABS_ADDRESS | Peony → Nexen | CPU → absolute address |
| 0x0601 | GET_ABS_ADDRESS_RESP | Nexen → Peony | Absolute address |
| 0x0602 | GET_REL_ADDRESS | Peony → Nexen | Absolute → CPU address |
| 0x0603 | GET_REL_ADDRESS_RESP | Nexen → Peony | Relative address |

### Execution Control

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0700 | GET_EXEC_STATUS | Peony → Nexen | Query running/paused state |
| 0x0701 | GET_EXEC_STATUS_RESP | Nexen → Peony | Execution status |

### ROM Info

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0800 | GET_ROM_INFO | Peony → Nexen | Get loaded ROM metadata |
| 0x0801 | GET_ROM_INFO_RESP | Nexen → Peony | ROM info (name, CRC, system, size) |
| 0x0802 | GET_ROM_HEADER | Peony → Nexen | Get ROM header bytes |
| 0x0803 | GET_ROM_HEADER_RESP | Nexen → Peony | ROM header data |

### Disassembly Sync

| Type | Name | Direction | Description |
|------|------|-----------|-------------|
| 0x0900 | GET_DISASSEMBLY | Peony → Nexen | Get Nexen's disassembly at address |
| 0x0901 | GET_DISASSEMBLY_RESP | Nexen → Peony | Disassembled lines |

## Key Payload Formats

### HANDSHAKE (0x0001)
```
uint16  protocolVersion    // Current: 1
uint16  clientType         // 1 = Peony
uint32  capabilities       // Bitfield of supported features
char[]  clientVersion      // Null-terminated version string
```

### HANDSHAKE_ACK (0x0002)
```
uint16  protocolVersion
uint8   consoleType        // NES=1, SNES=2, GB=3, GBA=4, etc.
uint32  romCrc32
uint32  romSize
char[]  romName            // Null-terminated
char[]  nexenVersion       // Null-terminated
```

### GET_MEMORY (0x0100)
```
uint8   memoryType         // MemoryType enum value
uint32  startAddress
uint32  length
```

### GET_MEMORY_RESP (0x0101)
```
uint8   memoryType
uint32  startAddress
uint32  length
byte[]  data               // Raw memory bytes
```

### GET_CDL_DATA (0x0400)
```
uint8   memoryType
uint32  offset
uint32  length
```

### GET_CDL_DATA_RESP (0x0401)
```
uint8   memoryType
uint32  offset
uint32  length
byte[]  cdlFlags           // Per-byte CDL flags
```

### SET_LABEL (0x0500)
```
uint32  address
uint8   memoryType
char[]  label              // Null-terminated
char[]  comment            // Null-terminated (empty string if none)
```

### GET_ROM_INFO_RESP (0x0801)
```
uint8   consoleType
uint32  romCrc32
uint32  romSize
char[]  romName            // Null-terminated
char[]  romFileName        // Null-terminated
```

## Connection Flow

```
Peony                                Nexen
  |                                    |
  |--- HANDSHAKE ---------------------->|
  |<--- HANDSHAKE_ACK ------------------|
  |                                    |
  |--- GET_ROM_INFO ------------------->|
  |<--- GET_ROM_INFO_RESP --------------|
  |                                    |
  |--- GET_CDL_DATA ------------------->|
  |<--- GET_CDL_DATA_RESP --------------|
  |                                    |
  |--- GET_MEMORY (VRAM) -------------->|
  |<--- GET_MEMORY_RESP ----------------|
  |                                    |
  |--- SET_LABEL ---------------------->|
  |<--- SET_LABEL_ACK ------------------|
  |                                    |
  |--- DISCONNECT --------------------->|
  |                                    |
```

## Error Handling

Error responses use the 0x04 flag bit and include:
```
uint16  errorCode
char[]  errorMessage       // Null-terminated
```

Error codes:
- 0x0001: Unknown message type
- 0x0002: Invalid memory type
- 0x0003: Address out of range
- 0x0004: No ROM loaded
- 0x0005: Unsupported operation for console type

## Implementation Priority

### Phase 1 (MVP)
- Handshake / disconnect
- GET_ROM_INFO — verify ROM match
- GET_CDL_DATA — live CDL for StaticAnalyzer
- GET_MEMORY (PRG ROM) — ROM data verification

### Phase 2 (Enhanced Analysis)
- GET_MEMORY (VRAM, SRAM, WRAM) — memory analysis
- GET_PPU_STATE — VRAM layout understanding
- GET_CPU_STATE — register context
- SET_LABEL — push Peony labels to Nexen debugger

### Phase 3 (Full Sync)
- Address translation
- Disassembly comparison
- GET_CDL_FUNCTIONS — function boundary detection
- Execution status monitoring
