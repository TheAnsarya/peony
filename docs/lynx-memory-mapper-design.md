# Lynx Memory Mapper Design

## Overview

The Atari Lynx has a unique memory architecture where cartridge data is loaded into RAM before execution. This differs from most cartridge-based systems where code is executed directly from ROM.

## Memory Map

| Range | Size | Description |
|-------|------|-------------|
| $0000-$00ff | 256B | Zero Page (fast access) |
| $0100-$01ff | 256B | Stack |
| $0200-$fbff | ~63KB | RAM (code/data) |
| $fc00-$fcff | 256B | Suzy hardware registers |
| $fd00-$fdff | 256B | Mikey hardware registers |
| $fe00-$fff7 | 504B | Boot ROM |
| $fff8-$ffff | 8B | Vectors (NMI, RST, IRQ/BRK) |

## Boot Process

1. Power on: CPU starts at RESET vector in Boot ROM
2. Boot ROM initializes hardware
3. Boot ROM reads first block from cartridge
4. First block contains loader code and entry point
5. Loader reads remaining cartridge data into RAM
6. Loader jumps to game code at typical address $0200

## LNX File Format

```
+----------+
| Header   | 64 bytes
+----------+
| Bank 0   | Bank0Pages × 256 bytes
+----------+
| Bank 1   | Bank1Pages × 256 bytes (optional)
+----------+
```

## Memory Mapper Implementation

### Class: LynxMemoryMapper

```csharp
public class LynxMemoryMapper : IMemoryMapper {
	private readonly LnxHeader? _header;
	private readonly int _romDataOffset;
	private readonly int _loadAddress;

	public LynxMemoryMapper(ReadOnlySpan<byte> rom) {
		_header = LnxHeaderParser.Parse(rom);
		_romDataOffset = LnxHeaderParser.GetRomDataOffset(rom);
		_loadAddress = 0x0200; // Typical load address
	}

	/// <summary>
	/// Convert file offset to CPU address.
	/// </summary>
	public uint OffsetToAddress(int offset) {
		// ROM data loads at $0200
		return (uint)(_loadAddress + (offset - _romDataOffset));
	}

	/// <summary>
	/// Convert CPU address to file offset.
	/// </summary>
	public int AddressToOffset(uint address) {
		if (address < _loadAddress) return -1;
		if (address >= 0xfc00) return -1; // Hardware
		return (int)(address - _loadAddress + _romDataOffset);
	}

	/// <summary>
	/// Check if address is in executable RAM.
	/// </summary>
	public bool IsExecutable(uint address) {
		return address >= 0x0200 && address < 0xfc00;
	}

	/// <summary>
	/// Get bank number for address.
	/// </summary>
	public int GetBank(uint address) {
		// For single-bank ROMs, always bank 0
		// For multi-bank, depends on bank switching state
		return 0;
	}
}
```

### Integration with DisassemblyEngine

The `DisassemblyEngine` needs to use the memory mapper to:

1. **Translate entry points**: Convert file-based entry points to RAM addresses
2. **Follow jumps**: When encountering JMP/JSR, map target addresses back to file offsets
3. **Read instruction bytes**: Use AddressToOffset to read from correct file position
4. **Detect hardware access**: Recognize Suzy/Mikey register addresses

### Bank Switching

Lynx cartridges can have two banks:

- **Bank 0**: Primary ROM data (always mapped)
- **Bank 1**: Secondary ROM data (switched in/out)

Bank switching is controlled by:
- SUZY IODIR/IODAT registers
- Cartridge-specific hardware

For disassembly, we typically disassemble each bank separately and note potential bank switch points.

## Disassembly Challenges

### 1. Self-Modifying Code

Since code runs from RAM, it can modify itself. The disassembler should:
- Mark potential SMC locations
- Note writes to code areas

### 2. Dynamic Entry Points

Games may compute jump targets. The disassembler should:
- Use recursive descent for known entry points
- Mark indirect jumps for manual review

### 3. Data in Code Areas

Embedded data (tables, text) in RAM. The disassembler should:
- Use CDL hints when available
- Detect obvious non-code patterns

## Implementation Plan

### Phase 1: Basic Memory Mapping
- [x] LnxHeaderParser (completed)
- [ ] LynxMemoryMapper class
- [ ] OffsetToAddress/AddressToOffset methods

### Phase 2: Disassembly Integration
- [ ] Update DisassemblyEngine to use memory mapper
- [ ] Set entry point at $0200 with mMapped offset
- [ ] Follow jumps through mapper

### Phase 3: Bank Support
- [ ] Multi-bank handling
- [ ] Bank identification in output

## Testing

Test cases:
1. Simple single-bank ROM → disassembles at $0200+
2. Multi-bank ROM → both banks disassembled
3. Hardware access → Suzy/Mikey labels applied
4. Roundtrip: disassemble → reassemble → byte-identical

## References

- [Lynx Programming Tutorial](http://www.monlynx.de/lynx/lynxprog.html)
- [Handy Source Code](https://github.com/handy-lynx/handy)
- [Mednafen Lynx Core](https://mednafen.github.io/)
- Peony Issue #46: Implement Lynx Memory Mapper
