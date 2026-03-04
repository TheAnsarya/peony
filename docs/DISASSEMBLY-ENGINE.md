# Disassembly Engine — Detailed Algorithm

> How Peony converts ROM bytes into structured assembly code.

## Overview

The `DisassemblyEngine` class implements a **recursive descent disassembler** — it starts at known entry points and follows control flow to discover all reachable code. This is the heart of Peony.

**Source:** `src/Peony.Core/DisassemblyEngine.cs`

---

## Internal State

```csharp
// CPU decoder (6502, 65816, SM83, etc.)
ICpuDecoder _cpuDecoder;

// Platform analyzer (NES, SNES, GB, etc.)
IPlatformAnalyzer _platformAnalyzer;

// Global labels (for fixed memory regions)
Dictionary<uint, string> _labels;

// Bank-specific labels (for switchable regions)
Dictionary<(uint Address, int Bank), string> _bankLabels;

// Comments to include in output
Dictionary<uint, string> _comments;

// Explicitly defined data regions
Dictionary<uint, DataDefinition> _dataDefinitions;

// Visited tracking — prevents infinite loops
Dictionary<(uint Address, int Bank), bool> _visited;

// Work queue — addresses to disassemble
Queue<(uint Address, int Bank)> _codeQueue;

// Cross-bank call tracking
HashSet<(int TargetBank, uint TargetAddress)> _bankCalls;

// Cross-reference graph
Dictionary<uint, List<CrossRef>> _crossRefs;

// Protected labels (from user/symbol files — cannot be overwritten)
HashSet<uint> _userDefinedLabels;
HashSet<(uint Address, int Bank)> _userDefinedBankLabels;

// Optional symbol/CDL/Pansy loader
SymbolLoader? _symbolLoader;
```

---

## Algorithm: Step by Step

### Phase 1: Initialization

```
Disassemble(byte[] rom, uint[] entryPoints, bool allBanks):

1. Get entry points from platform analyzer:
   - IPlatformAnalyzer.GetEntryPoints(rom)
   - These are interrupt vectors: RESET, NMI, IRQ
   - e.g., NES reads $FFFA (NMI), $FFFC (RESET), $FFFE (IRQ)

2. Merge with user-provided entry points

3. If SymbolLoader is set:
   a. Import all user-defined labels → AddUserLabel()
   b. Import all comments → AddComment()
   c. Import CDL sub-entry points → add to code queue
   d. Import Pansy symbol addresses → add to code queue
   e. Import DIZ code-marked addresses → add to code queue

4. Queue all entry points:
   For each entry point address:
     - Determine bank: IPlatformAnalyzer.AddressToOffset(addr)
     - If in switchable region: bank = 0 (default)
     - Add label: "reset", "nmi_handler", "irq_handler"
     - Enqueue: _codeQueue.Enqueue((address, bank))

5. If allBanks is true:
   For each bank 0..BankCount-1:
     - Queue the entry points for that bank's vector table
```

### Phase 2: Recursive Descent

```
While _codeQueue is not empty:

  (address, bank) = _codeQueue.Dequeue()

  // Skip if already visited in this bank
  if _visited.ContainsKey((address, bank)):
    continue
  _visited[(address, bank)] = true

  // Start a new disassembled block
  block = new DisassembledBlock(address, bank)

  // Decode loop — follow sequential instructions
  While true:

    // 1. Bounds check
    offset = IPlatformAnalyzer.AddressToOffset(address, rom.Length, bank)
    if offset < 0 or offset >= rom.Length:
      break  // Out of ROM bounds

    // 2. Check CDL: is this address marked as DATA?
    if SymbolLoader?.IsData(offset) == true:
      break  // CDL says this is data, not code

    // 3. Check explicit data definitions
    if _dataDefinitions.ContainsKey(address):
      break  // User marked this as data

    // 4. Decode the instruction
    instructionBytes = rom[offset .. offset + MAX_INSTRUCTION_SIZE]
    instruction = ICpuDecoder.Decode(instructionBytes, address)

    if instruction is null or instruction.Bytes.Length == 0:
      break  // Invalid instruction — possible data

    // 5. Create disassembled line
    line = new DisassembledLine(
      Address: address,
      Bytes: instruction.Bytes,
      Label: GetLabel(address, bank),
      Content: FormatInstruction(instruction),
      Comment: GetComment(address),
      Bank: bank
    )
    block.Lines.Add(line)

    // 6. Handle control flow
    if ICpuDecoder.IsControlFlow(instruction):

      // Get all possible targets
      targets = ICpuDecoder.GetTargets(instruction, address)

      foreach target in targets:
        // Record cross-reference
        type = GetCrossRefType(instruction)
        AddCrossRef(address, bank, target, type)

        // Auto-generate label at target
        labelName = (type == Call) ? "sub_{target:x4}" : "loc_{target:x4}"
        AddLabel(target, labelName)  // Won't overwrite user labels

        // Determine target bank
        targetBank = DetermineTargetBank(target, bank, address, rom)
        if targetBank != bank:
          _bankCalls.Add((targetBank, target))

        // Queue target for disassembly
        _codeQueue.Enqueue((target, targetBank))

      // If unconditional (JMP, RTS, RTI, BRA):
      if IsUnconditionalBranch(instruction):
        break  // End this block — no fall-through

    // 7. Advance to next instruction
    address += instruction.Bytes.Length
    block.EndAddress = address

  // Save the completed block
  result.Blocks.Add(block)
  result.BankBlocks[bank].Add(block)
```

### Phase 3: Post-Processing

```
After all code is disassembled:

1. Identify ungrouped data regions:
   - Scan ROM bytes not covered by any code block
   - Classify using CDL (DATA flag) or DataDetector heuristics
   - Create DataDefinition entries

2. Sort blocks by address (per bank)

3. Aggregate cross-references:
   - For each label address, collect all incoming references
   - Sort by type (Calls first, then Jumps, then Branches)

4. Build DisassemblyResult:
   - All blocks (sorted)
   - All labels (global + bank-specific)
   - All comments
   - All cross-references
   - All data definitions
   - Per-bank block lists
   - ROM info from platform analyzer
```

---

## Label Management

Labels are the named addresses that make disassembly readable.

### Label Hierarchy

```
Priority (highest to lowest):
1. User-defined labels (from .pansy, .mlb, .nl, .diz files)
   → Protected: never overwritten by auto-generation
   → Stored in _userDefinedLabels / _userDefinedBankLabels sets

2. Vector labels (auto-generated from entry points)
   → "reset", "nmi_handler", "irq_handler"

3. Auto-generated labels (from disassembly)
   → "sub_XXXX" — subroutine targets (JSR/JSL targets)
   → "loc_XXXX" — jump/branch targets
   → "bank{N}_sub_XXXX" — bank-specific subroutine labels
```

### Label Lookup Rules

```
GetLabel(address, bank):

1. If address is in a switchable region:
   a. Try bank-specific label: _bankLabels[(address, bank)]
   b. Fall back to global: _labels[address]

2. If address is in a fixed region (e.g., NES $C000-$FFFF):
   a. Use global label: _labels[address]
   b. Bank-specific labels ignored for fixed regions

3. If address matches a hardware register:
   a. Return IPlatformAnalyzer.GetRegisterLabel(address)
   b. e.g., $2000 → "PPUCTRL", $4016 → "JOY1"
```

### AddLabel vs AddUserLabel

```csharp
// Auto-generated — respects protection
AddLabel(uint address, string name, int? bank):
  if _userDefinedLabels.Contains(address): return  // Protected!
  if _labels.ContainsKey(address): return           // Already has label
  _labels[address] = name;

// User-defined — takes priority, marks as protected
AddUserLabel(uint address, string name, int? bank):
  _labels[address] = name;                           // Overwrite anything
  _userDefinedLabels.Add(address);                    // Mark as protected
```

---

## Cross-Reference Tracking

Every branch, jump, and call instruction creates a cross-reference:

```csharp
CrossRef(uint FromAddress, int FromBank, CrossRefType Type)

CrossRefType:
  Jump    — JMP, BRA (unconditional)
  Call    — JSR, JSL (subroutine call, will return)
  Branch  — BCC, BCS, BEQ, BMI, BNE, BPL, BVC, BVS (conditional)
  DataRef — LDA abs, STA abs (data access to known address)
  Pointer — Pointer table entry
```

Cross-references are stored per target address:
```csharp
_crossRefs[targetAddress] = List<CrossRef> {
  CrossRef(fromAddr=0x8042, fromBank=0, type=Call),
  CrossRef(fromAddr=0x80FF, fromBank=1, type=Jump),
  ...
}
```

In output, cross-references appear as comments:
```asm
reset:                          ; Referenced by:
                                ;   Call from $8042 (bank 0)
                                ;   Jump from $80FF (bank 1)
                                ;   Branch from $8123 (bank 0) (+2 more)
    lda #$00
```

---

## Bank Determination Algorithm

When the engine encounters a branch target, it must determine which bank to use:

```
DetermineTargetBank(targetAddress, currentBank, sourceAddress, rom):

1. If target is in a FIXED region (e.g., NES $C000-$FFFF):
   → bank = last bank (fixed to end of ROM)

2. If SymbolLoader has Pansy memory regions:
   → Look up target address in memory regions
   → If region has a bank number, use it

3. If platform detects bank switching:
   → result = IPlatformAnalyzer.DetectBankSwitch(rom, sourceAddress, currentBank)
   → If result is not null, use result.TargetBank

4. Default:
   → Use currentBank (assume same bank as caller)
```

---

## Data Detection

Bytes that aren't disassembled as code need to be classified:

### CDL-Guided Detection
- `CODE` flag (0x01) → disassemble as code
- `DATA` flag (0x02) → emit as `.db` / `.dw` data
- `DRAWN` flag (0x20) → graphics data (`.incbin` or tile)
- No flags → unknown — use heuristics

### Heuristic Detection (DataDetector)
```
For each uncovered byte range:

1. Check for pointer table patterns:
   - Consecutive 16-bit values pointing into ROM space ($8000-$FFFF)
   - 3+ valid pointers in a row → PointerTable

2. Check for text patterns:
   - 4+ consecutive printable ASCII bytes → Text

3. Check for tile/graphics patterns:
   - 8-byte or 16-byte repeating structures → Graphics

4. Default:
   → Mark as generic Data (.db bytes)
```

---

## Error Handling & Edge Cases

### Invalid Instructions
When the decoder returns null or zero-length instruction:
- Stop the current block
- Mark remaining bytes as data
- Continue processing queue

### Infinite Loops
- The `_visited` dictionary prevents re-processing `(address, bank)` pairs
- A `JMP` to self (infinite loop) is decoded once and produces a single-line block

### Overlapping Code
NES/6502 code can intentionally overlap (one instruction's operand is another's opcode):
- Currently: first visitor wins
- Improvement needed: track byte-level ownership

### Self-Modifying Code
- Not detectable without emulator CDL data
- CDL from Nexen captures the actual execution, including SMC effects
- Without CDL, self-modifying code regions will be misclassified

---

## Key Method Signatures

```csharp
public class DisassemblyEngine {
    // Constructor
    DisassemblyEngine(ICpuDecoder cpuDecoder, IPlatformAnalyzer platformAnalyzer)

    // Main entry point
    DisassemblyResult Disassemble(byte[] rom, uint[] entryPoints, bool allBanks = false)

    // Symbol integration
    void SetSymbolLoader(SymbolLoader loader)

    // Label management
    void AddLabel(uint address, string name, int? bank = null)
    void AddUserLabel(uint address, string name, int? bank = null)
    string? GetLabel(uint address, int? bank = null)
    bool IsUserDefinedLabel(uint address, int? bank = null)

    // Comments
    void AddComment(uint address, string comment)

    // Data definitions
    void AddDataDefinition(uint address, DataDefinition def)
}
```

---

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Decode instruction | O(1) | Table lookup |
| Queue processing | O(n) | n = unique (address, bank) pairs |
| Label lookup | O(1) | Dictionary hash |
| Cross-ref add | O(1) | List append |
| Full disassembly | O(R) | R = reachable code bytes |
| Post-processing | O(B log B) | B = blocks, for sorting |

Typical NES ROM (256 KB): < 1 second
SNES ROM (2-4 MB): 2-5 seconds
GBA ROM (16-32 MB): 10-30 seconds
