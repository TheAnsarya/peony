# Pansy Jump Metadata — Consumption Contract

> How Peony reads and applies Pansy cross-reference (jump graph) data to seed
> disassembly entry points, and what producers (Nexen, Pansy CLI, hand-editing)
> must provide for correct results.

## Overview

The **Pansy jump graph** is the set of cross-reference edges stored in a `.pansy`
file that describe where code jumps, branches, or calls. Peony reads this graph
during `DisassemblyPipeline.BuildEntryPoints` and uses it to pre-populate the
work-queue before the recursive-descent engine starts. The more complete the
graph, the better coverage Peony achieves — especially for ROM code reached only
via indirect or computed jumps that static analysis cannot resolve.

---

## Cross-Reference Sections

A Pansy file may contain two complementary cross-reference sections:

### Section 0x0006 — `CrossReferences` (individual edges)

Each record is a single `(From, To, Type)` triple where:

- `From` — ROM byte offset of the instruction that performs the jump/call/branch
- `To` — ROM byte offset of the jump **target**
- `Type` — `CrossRefType` enum (see below)

This is the canonical representation for all one-to-one edges.

### Section — `MultiTargetCrossReferences` (grouped edges)

Each record is `(From, Type, Targets[])` — one source, many targets.

**Important**: `PansyWriter.AddMultiTargetCrossReference` writes BOTH:

1. A grouped `MultiTargetCrossReference` entry
2. Individual legacy `CrossReference` edges (one per target)

Both representations are present in the file. Peony deduplicates by tracking
which CPU addresses have already been added to the work-queue, so double-writing
does not cause duplicate disassembly — but consumers must not assume the two
sections are mutually exclusive.

---

## CrossRefType Values

| Value | Name | Peony action |
|-------|------|--------------|
| 1 | `Jsr` | ✅ Seeded as entry point |
| 2 | `Jmp` | ✅ Seeded as entry point |
| 3 | `Branch` | ✅ Seeded as entry point |
| 4 | `Read` | ❌ Not seeded (data reference) |
| 5 | `Write` | ❌ Not seeded (data reference) |

Only code-flow edge types (`Jsr`, `Jmp`, `Branch`) are used for entry-point
seeding. `Read` and `Write` edges describe data-memory accesses and must never
be placed into the code work-queue.

---

## Entry-Point Expansion Policy

`DisassemblyPipeline.BuildEntryPoints` processes hints from all sources in a
deterministic priority order:

1. Platform primary reset vector
2. Pansy CDM `SUB_ENTRY` bytes (sorted ascending)
3. Pansy CDM `JUMP_TARGET` bytes (sorted ascending)
4. **Pansy xref targets** — individual edges, grouped by source, sorted ascending, capped at `MaxIndirectJumpTargetsPerSource` per source
5. **Pansy multi-target xref targets** — grouped edges, sorted ascending, capped at `MaxIndirectJumpTargetsPerSource` per group
6. CDL `SUB_ENTRY` bytes (sorted ascending)
7. CDL `JUMP_TARGET` bytes (sorted ascending)
8. Remaining platform entry vectors
9. Fallback address `0x8000`

All addresses are deduplicated — the first occurrence wins, later duplicates
are silently skipped.

### Fan-Out Cap

```csharp
DisassemblyPipeline.MaxIndirectJumpTargetsPerSource = 10_000
```

A single source address (from one `From` offset) can contribute at most
`MaxIndirectJumpTargetsPerSource` targets. Targets are sorted ascending before
the cap is applied, so the **lowest addresses are always preferred**.

This cap exists solely to guard against pathological Pansy files with unlimited
jump tables. Real games do not approach this limit. Legitimate entry points
beyond the cap are found by recursive descent from earlier seeds.

---

## Address Format: ROM Offsets

All `From`, `To`, and `Targets` values in both cross-reference sections are
**ROM byte offsets** — not CPU addresses. Peony calls
`IPlatformAnalyzer.OffsetToAddress(int offset)` to translate each offset before
adding it to the entry-point set. Offsets that translate to `null` are silently
discarded (e.g., offsets past the end of ROM).

Producers must ensure all recorded offsets are valid within the ROM they
accompany.

---

## CDM vs. Cross-References: When to Use Which

| Scenario | Best representation |
|----------|---------------------|
| Byte was executed as code | CDM `CODE` flag |
| Byte was a jump target at runtime | CDM `JUMP_TARGET` flag |
| Byte was a subroutine entry at runtime | CDM `SUB_ENTRY` flag |
| A specific `jsr addr` instruction was observed | `CrossReference(From, addr, Jsr)` |
| An indirect jump loaded `addr` from a table | `MultiTargetCrossReference(From, Jmp, [addr1, addr2, ...])` |

Use CDM flags when you know a byte was accessed as code but not the source of
the access. Use cross-references when you know the precise `(from, to)` pair —
especially for indirect jumps that emit multiple targets.

---

## Producer Expectations

### Nexen Emulator

Nexen should record a `MultiTargetCrossReference` edge for every **indirect
jump** (e.g. `jmp ($xxxx,X)` on SNES, `jmp ($xxxx)` on NES) whenever the
CPU resolves the target address. Over the lifetime of a play session, multiple
targets accumulate in each group. Write these via
`PansyWriter.AddMultiTargetCrossReference` so both the grouped section and the
legacy individual edges are present.

Nexen must also set CDM `JUMP_TARGET` and `SUB_ENTRY` flags on all confirmed
code bytes — these are consumed before the xref graph and provide the best
coverage for non-indirect jumps.

### Pansy CLI / Hand-Edited Files

When manually adding cross-references for indirect jump tables:

1. Identify the jump instruction offset (the `From` address) in the ROM
2. Enumerate all reachable targets from the table
3. Add a `MultiTargetCrossReference` via `PansyWriter.AddMultiTargetCrossReference`

If you add individual `CrossReference` edges for `Read` or `Write` to the same
file, they will NOT be seeded as entry points — this is correct behaviour.

### Avoiding Common Mistakes

| Mistake | Result | Fix |
|---------|--------|-----|
| Using `Read` or `Write` type for code jump targets | Target not seeded | Use `Jmp` or `Jsr` |
| Recording CPU addresses instead of ROM offsets | Wrong entry points or `null` translations | Convert to ROM offsets first |
| Recording only one target for an indirect jump | Missed alternative targets | Record all observed targets over multiple play sessions |
| Not setting CDM `SUB_ENTRY` for subroutines | Relies solely on xref graph seeding | Set both the xref and CDM flag |

---

## Troubleshooting: Missing Code Regions

If Peony produces large stretches of `.db` bytes in a known-code area:

### 1. Check CDM coverage

```
peony info game.pansy --show-cdl-stats
```

Low CDM `CODE` coverage means the emulator session was short. Play more of the
game (or use a TAS) and regenerate the CDL.

### 2. Check xref count

```
peony info game.pansy --show-xref-stats
```

Zero multi-target cross-references for a platform with indirect jumps (SNES,
NES) suggests Nexen is not recording them. Verify the Nexen Pansy export
settings.

### 3. Inspect specific missing regions

If bank N is entirely `.db`:

- Confirm the bank's reset vector / interrupt vectors are in the Pansy xref graph
- Check that `OffsetToAddress` for bank N returns non-null values
- Verify CDM `CODE` flags are set for the bank's known code bytes

### 4. Check address translation

Offsets that translate to `null` are silently dropped. Use:

```csharp
var addr = analyzer.OffsetToAddress((int)xref.To);
// If null, the offset is invalid for this platform/mapper
```

---

## Related Documentation

- [CDL & Pansy Integration](CDL-PANSY-INTEGRATION.md) — Broad overview of how CDL and Pansy improve disassembly
- [Disassembly Engine](DISASSEMBLY-ENGINE.md) — Recursive descent algorithm details
- [Interfaces & Types](INTERFACES.md) — `IPlatformAnalyzer.OffsetToAddress` contract
