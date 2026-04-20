# SNES CDL/Pansy Integration Fixes - Technical Plan

> Epic: #179 | Created: 2026-04-20

## Problem Statement

Peony's SNES disassembly pipeline has 5 compounding bugs that cause massive information loss when using CDL and Pansy metadata. Using FFMQ as a test case:

- CDL reports 73,089 code bytes + 333,827 data bytes (77.6% coverage)
- Peony only disassembles 44,583 code bytes, marks 0 data bytes, leaves 479,705 unknown

## Root Causes (Priority Order)

### 1. CRITICAL: Bank 0 Address Mapping (#180)

**File:** `src/Peony.Platform.SNES/SnesAnalyzer.cs`

```csharp
// CURRENT (BROKEN):
var addrBank = (int)((address >> 16) & 0xff);
if (addrBank > 0) bank = addrBank;  // Bank 0 treated as "unspecified"!

// FIX: Use address bit width to detect embedded bank
// 24-bit addresses (>= 0x10000) have explicit bank; 16-bit don't
if (address >= 0x10000) bank = addrBank;
// OR: change API to use nullable bank parameter
```

**Impact:** All bank 0 code is read from wrong ROM offset. Bank 0 is where the SNES reset vector lives - it's the most important bank.

### 2. CRITICAL: Missing 65816 Terminators (#181)

**File:** `src/Peony.Core/DisassemblyEngine.cs:703`

```csharp
// CURRENT (BROKEN):
return instruction.Mnemonic is "jmp" or "rts" or "rti" or "brk";

// FIX: Add all 65816 unconditional branches
return instruction.Mnemonic is "jmp" or "jml" or "rts" or "rtl"
    or "rti" or "brk" or "bra" or "brl";
```

**Impact:** Decoder falls through past long jumps/returns into garbage bytes.

### 3. HIGH: CDL JumpTargets Not Seeded (#182)

**File:** `src/Peony.Core/DisassemblyEngine.cs` (entry point seeding section)

```csharp
// ADD after CDL SubEntryPoints loop (~line 248):
if (_symbolLoader?.CdlData is not null) {
    foreach (var offset in _symbolLoader.CdlData.JumpTargets) {
        var address = RomOffsetToAddress((uint)offset, fixedBank);
        if (address.HasValue && IsValidAddress(address.Value)) {
            if (!_visited.ContainsKey((address.Value, fixedBank))) {
                _codeQueue.Enqueue((address.Value, fixedBank));
                if (GetLabel(address.Value, fixedBank) is null) {
                    AddLabel(address.Value, $"jmp_{offset:x4}", fixedBank);
                }
            }
        }
    }
}
```

### 4. MEDIUM: CDL Label Address Translation (#183)

**File:** `src/Peony.Core/SymbolLoader.cs`

The `LoadCdlData()` method needs a platform analyzer reference to convert ROM offsets to CPU addresses.

### 5. MEDIUM: Bank Propagation (#184)

**File:** `src/Peony.Core/DisassemblyEngine.cs`

Extract bank from 24-bit address when seeding entry points instead of always using `fixedBank`.

## Implementation Order

1. **#181** (terminators) - Trivial 1-line fix, zero risk
2. **#182** (CDL jump targets) - Simple loop addition, low risk
3. **#180** (bank 0 mapping) - Requires careful API change, medium risk
4. **#184** (bank propagation) - Depends on #180 fix
5. **#183** (label translation) - Depends on platform analyzer availability in SymbolLoader
6. **#185** (tests) - After bug fixes, verify with synthetic tests
7. **#186** (benchmark) - After fixes, measure FFMQ improvement

## Validation

After each fix, run:

```powershell
# Run all tests
dotnet test Peony.sln -c Release --nologo -v m

# Run FFMQ disassembly and count metrics
dotnet run --project src/Peony.Cli -c Release -- disasm "C:\~reference-roms\snes\ffmq.smc" -f poppy -y "path/to/ffmq-nexen.pansy" -c "path/to/ffmq-nexen-coverage.cdl" -o output.pasm -b

# Count instructions vs .db lines
$content = Get-Content output.pasm
($content | Select-String "^\s+(lda|sta|jsr|jsl|jmp|jml|rts|rtl)" | Measure-Object).Count
($content | Select-String "^\s+\.db " | Measure-Object).Count
```

## Success Criteria

- All 1342+ existing tests still pass
- Bank 0 has disassembled code (not all .db)
- Instruction count > 40,000 (vs current 10,279)
- All 16 banks have code blocks
- jml/rtl/bra/brl terminate blocks correctly
