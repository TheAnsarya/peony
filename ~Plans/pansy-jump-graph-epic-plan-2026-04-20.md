# Pansy Jump Graph Epic Plan (2026-04-20)

## Context

Peony must consume Pansy metadata as authoritative hints for control-flow seeding.
This plan aligns with epic #191 and sub-issues #192, #193, #195, #196.

## Objectives

- Use both CODE_DATA_MAP flags and CROSS_REFS edges for entry seeding.
- Preserve deterministic behavior under dense cross-reference graphs.
- Bound memory/time impact for one-source-many-target patterns.

## Work Breakdown

1. Implement bounded entry expansion policy (#192).
2. Add integration tests for multi-target indirect graphs (#193).
3. Add benchmark scenarios for low/medium/high graph density (#195).
4. Document metadata consumption contract and troubleshooting (#196).

## Guardrails

- Keep deterministic ordering for all seeded entries.
- Avoid duplicate entry points.
- Keep static-analysis opt-in behavior unchanged by default.

## Success Criteria

- Integration tests pass for sparse and dense graph fixtures.
- Benchmarks quantify runtime and memory overhead.
- Documentation clearly defines CDM versus CROSS_REFS behavior.
