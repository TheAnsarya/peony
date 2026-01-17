---
name: Bug - DPC Mapper Support Broken
about: Pitfall II (DPC chip) produces 0 blocks when disassembling
title: 'Fix DPC mapper support for Pitfall II and other DPC ROMs'
labels: bug, atari2600, mapper, high-priority
assignees: ''
milestone: 'Milestone 1: Basic Roundtrip'
---

## Problem

Pitfall II (DPC chip) ROMs produce 0 disassembled blocks. The DPC (Display Processor Chip) mapper requires special handling but currently fails silently.

## Steps to Reproduce

```bash
peony disasm "path/to/Pitfall II - Lost Caverns (1984) (Activision).a26" -o output.pasm
```

**Expected**: ~100+ code blocks disassembled  
**Actual**: 0 blocks, empty output

## ROM Details

- **File**: Pitfall II - Lost Caverns (1984) (Activision).a26
- **Size**: 10KB (10240 bytes)
- **Mapper**: DPC (Display Processor Chip)
- **Bank Count**: 2 banks + DPC registers

## Root Cause

The DPC mapper uses special hardware registers at `$1000-$107F` that need to be mapped differently than standard ROMs. Current Atari2600Analyzer doesn't detect or handle DPC chips.

## Proposed Solution

1. Add DPC detection to `Atari2600Analyzer.Analyze()`
2. Create `DpcMapper` class with proper bank/register mapping
3. Add DPC-specific entry point detection
4. Handle DPC audio/graphics fetch registers

## Acceptance Criteria

- [ ] Pitfall II disassembles successfully with 100+ blocks
- [ ] DPC registers are properly labeled/commented
- [ ] Test with all DPC ROMs in Good2600 collection
- [ ] Documentation updated with DPC mapper details

## Related

- Test file: `GameInfo/docs/Atari-2600/test-results/pitfall2.asm` (currently empty)
- Documentation: `GameInfo/docs/Atari-2600/Bank-Switching.md` (DPC section)

## Priority

**High** - Blocks testing of important commercial games
