# Cross-Repo Platform Parity — Phase 2 Code Plan

## Issues Addressed

- #125 SMS/Game Gear disassembly support (Z80)
- #126 PCE/TurboGrafx-16 disassembly support (HuC6280)
- #127 WonderSwan disassembly support (V30MZ)
- #128 Genesis/Mega Drive disassembly support (M68000)
- #124 Epic: Cross-Repo Platform Parity

## Current Status

All 4 platforms **already have** full analyzer + CPU decoder implementations:

| Platform | Analyzer | CPU Decoder | CLI Dispatch | Tests |
|----------|----------|-------------|-------------|-------|
| SMS | `SmsAnalyzer` | `Z80Decoder` | ✅ disasm/batch/verify/export | ❌ None |
| PCE | `PceAnalyzer` | `HuC6280Decoder` | ✅ disasm/batch/verify/export | ❌ None |
| WonderSwan | `WonderSwanAnalyzer` | `V30MZDecoder` | ✅ disasm/batch/verify/export | ❌ None |
| Genesis | `GenesisAnalyzer` | `M68000Decoder` | ✅ disasm/batch/verify/export | ❌ None |

## Remaining Work

### Phase 2a: CLI Coverage Gap

The `pack` and `coverage` CLI dispatch blocks are missing SMS/PCE/WS/Genesis/ChannelF entries while `disasm`/`batch`/`verify`/`export` include them. Add the missing cases.

### Phase 2b: CPU Decoder Tests

Create test projects for each CPU decoder:

```
tests/
  Peony.Cpu.Z80.Tests/         (Z80 instruction decoding)
  Peony.Cpu.HuC6280.Tests/     (HuC6280 instruction decoding)
  Peony.Cpu.V30MZ.Tests/       (V30MZ instruction decoding)
  Peony.Cpu.M68000.Tests/      (M68000 instruction decoding)
```

**Test categories per CPU:**

1. **Basic opcode decoding** — Round-trip decode each major instruction category
2. **Addressing modes** — Verify all addressing modes decode correctly
3. **Instruction length** — Verify byte counts match
4. **Control flow detection** — Verify IsControlFlow returns correct values
5. **Jump target extraction** — Verify GetTargets works for branches/jumps

### Phase 2c: Platform Analyzer Tests

Create test projects for each platform:

```
tests/
  Peony.Platform.SMS.Tests/         (SMS memory map, entry points, bank switching)
  Peony.Platform.PCE.Tests/         (PCE memory map, entry points, bank switching)
  Peony.Platform.WonderSwan.Tests/  (WS memory map, entry points)
  Peony.Platform.Genesis.Tests/     (Genesis memory map, entry points, bank detection)
```

**Test categories per platform:**

1. **Memory map** — `GetMemoryRegion()` returns correct regions
2. **Entry points** — `GetEntryPoints()` returns reset/IRQ vectors
3. **Address mapping** — `AddressToOffset()` / `OffsetToAddress()` roundtrip
4. **Bank detection** — `DetectBankSwitch()` and `IsInSwitchableRegion()`
5. **Register labels** — `GetRegisterLabel()` for I/O registers
6. **ROM analysis** — `Analyze()` with test ROM data

### Phase 2d: Integration Tests

Add end-to-end disassembly tests using small test ROM binaries for each platform:

- Generate minimal valid ROM headers for each platform
- Disassemble → verify output contains expected labels/instructions
- Verify Pansy export includes correct platform ID

## Priority Order

1. **Phase 2a** — Quick CLI fix, unblocks pack/coverage
2. **Phase 2b** — CPU decoder tests are highest value (verify correctness)
3. **Phase 2c** — Platform analyzer tests
4. **Phase 2d** — Integration tests (requires test ROM creation)

## Estimated Test Counts

| Platform | CPU Tests | Analyzer Tests | Integration | Total |
|----------|-----------|---------------|-------------|-------|
| SMS (Z80) | ~80 | ~30 | ~5 | ~115 |
| PCE (HuC6280) | ~80 | ~30 | ~5 | ~115 |
| WonderSwan (V30MZ) | ~60 | ~25 | ~5 | ~90 |
| Genesis (M68000) | ~150 | ~35 | ~5 | ~190 |
| **Total** | ~370 | ~120 | ~20 | **~510** |
