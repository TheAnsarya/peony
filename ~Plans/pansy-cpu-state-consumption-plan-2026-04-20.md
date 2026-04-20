# Pansy CPU State Consumption Plan (2026-04-20)

## Goal

Make Peony treat Pansy CPU-state entries as the canonical per-address SNES 65816 M/X source during disassembly, with CDL remaining only as fallback input.

## Problem

Nexen already exports SNES X/M width hints into Pansy section `0x0009`, but Peony currently drives decoder width from CDL via `TryGetSnesMxState`. This leaves canonical Pansy CPU-state metadata unused for actual decode decisions.

## Approach

1. Add a fast lookup path from CPU address/bank to imported `CpuStateEntry` values.
2. Resolve M/X in precedence order:
   - propagated entry state
   - Pansy CPU-state entry
   - CDL-derived state
   - default 8-bit/8-bit
3. Add focused regression tests covering:
   - Pansy CPU-state usage without CDL
   - Pansy CPU-state precedence over conflicting CDL bits
   - no regression for CDL-only mode

## Constraints

- Do not change existing code/data-map semantics.
- Do not require Pansy CPU-state for non-65816 platforms.
- Keep changes minimal and localized to the disassembly pipeline/engine.

## Validation

- Run targeted `Peony.Core.Tests` for `DisassemblyEngine` and pipeline coverage.
- Keep behavior deterministic across banked entry points.
