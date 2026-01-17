---
name: Enhancement - Multi-Bank Traversal Incomplete
about: --all-banks flag doesn't fully traverse all banks in banked ROMs
title: 'Improve multi-bank traversal to analyze all banks completely'
labels: enhancement, bank-switching, medium-priority
assignees: ''
milestone: 'Milestone 2: Multi-Game Support'
---

## Problem

The `--all-banks` flag is incomplete - it doesn't fully traverse all banks in multi-bank ROMs. Some banks are analyzed, but code flow doesn't properly follow jumps across bank boundaries.

## Steps to Reproduce

```bash
peony disasm "path/to/8KB_F8_ROM.a26" --all-banks -o output.pasm
```

**Expected**: All banks analyzed with cross-bank references  
**Actual**: Bank 0 fully analyzed, Bank 1 partially analyzed

## Affected Mappers

- **F8** (8KB, 2 banks)
- **F6** (16KB, 4 banks)
- **F4** (32KB, 8 banks)
- **E0** (8KB Parker Bros)
- **FE** (8KB Activision)

## Current Behavior

- Entry points found in bank 0
- Bank switching code detected but not followed
- Some banks have 0 blocks despite containing code
- No cross-bank label references

## Proposed Solution

1. **Bank Switch Detection**
   - Detect bank switch writes (`STA $FFF8/$FFF9` for F8)
   - Track which bank is active after switch
   - Queue entry points in new bank

2. **Cross-Bank Analysis**
   - Create per-bank disassembly results
   - Merge results with bank annotations
   - Generate labels like `bank1_routine`

3. **Improved Traversal**
   - BFS/DFS across bank boundaries
   - Track visited addresses per bank
   - Handle bank-switch subroutines specially

## Acceptance Criteria

- [ ] All banks in F8/F6/F4 ROMs are analyzed
- [ ] Cross-bank jumps generate proper labels
- [ ] Output shows bank switch instructions clearly
- [ ] `--all-banks` produces complete disassembly
- [ ] Test with 10+ multi-bank ROMs

## Test Cases

- **F8**: Combat, Space Invaders (8KB)
- **F6**: Ms. Pac-Man, Donkey Kong (16KB)
- **F4**: Fatal Run, Midnight Magic (32KB)

## Related

- `DisassemblyEngine.cs` - traversal logic
- `Atari2600Analyzer.cs` - bank detection
- Issue #1 - DPC mapper (similar bank handling needed)

## Priority

**Medium** - Affects many commercial games, but workaround exists (single bank mode)
