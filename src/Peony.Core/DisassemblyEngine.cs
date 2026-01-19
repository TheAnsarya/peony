namespace Peony.Core;

/// <summary>
/// Core disassembly engine with multi-bank support and CDL/DIZ integration
/// </summary>
public class DisassemblyEngine {
	private readonly ICpuDecoder _cpuDecoder;
	private readonly IPlatformAnalyzer _platformAnalyzer;

	// Global labels (non-bank-specific, for fixed regions like $C000-$FFFF on NES)
	private readonly Dictionary<uint, string> _labels = [];

	// Bank-specific labels for switchable regions
	private readonly Dictionary<(uint Address, int Bank), string> _bankLabels = [];

	private readonly Dictionary<uint, string> _comments = [];
	private readonly Dictionary<uint, DataDefinition> _dataDefinitions = [];
	private readonly Dictionary<(uint Address, int Bank), bool> _visited = [];
	private readonly Queue<(uint Address, int Bank)> _codeQueue = new();
	private readonly HashSet<(int TargetBank, uint TargetAddress)> _bankCalls = [];

	// Cross-reference tracking
	private readonly Dictionary<uint, List<CrossRef>> _crossRefs = [];

	// Track which labels are user-defined (from DIZ/symbol files)
	private readonly HashSet<uint> _userDefinedLabels = [];
	private readonly HashSet<(uint Address, int Bank)> _userDefinedBankLabels = [];

	private SymbolLoader? _symbolLoader;

	public DisassemblyEngine(ICpuDecoder cpuDecoder, IPlatformAnalyzer platformAnalyzer) {
		_cpuDecoder = cpuDecoder;
		_platformAnalyzer = platformAnalyzer;
	}

	/// <summary>
	/// Add a cross-reference from one address to another
	/// </summary>
	private void AddCrossRef(uint fromAddress, int fromBank, uint toAddress, CrossRefType type) {
		if (!_crossRefs.TryGetValue(toAddress, out var refs)) {
			refs = [];
			_crossRefs[toAddress] = refs;
		}

		// Avoid duplicates
		var crossRef = new CrossRef(fromAddress, fromBank, type);
		if (!refs.Contains(crossRef)) {
			refs.Add(crossRef);
		}
	}

	/// <summary>
	/// Determine cross-reference type based on instruction mnemonic
	/// </summary>
	private CrossRefType GetCrossRefType(DecodedInstruction instruction) {
		var mnemonic = instruction.Mnemonic.ToUpperInvariant();
		return mnemonic switch {
			"JMP" or "BRA" => CrossRefType.Jump,
			"JSR" or "JSL" => CrossRefType.Call,
			"BCC" or "BCS" or "BEQ" or "BMI" or "BNE" or "BPL" or "BVC" or "BVS" => CrossRefType.Branch,
			"LDA" or "LDX" or "LDY" or "STA" or "STX" or "STY" => CrossRefType.DataRef,
			_ => CrossRefType.Jump // Default for other control flow
		};
	}

	/// <summary>
	/// Set a symbol loader for CDL/DIZ/symbol file integration.
	/// Labels from symbol files are considered user-defined and will not be
	/// overwritten by auto-generated labels during disassembly.
	/// </summary>
	public void SetSymbolLoader(SymbolLoader symbolLoader) {
		_symbolLoader = symbolLoader;

		// Import labels from symbol loader (these are user-defined)
		foreach (var (addr, label) in symbolLoader.Labels) {
			_labels[addr] = label;
			_userDefinedLabels.Add(addr);  // Mark as user-defined
		}

		// Import comments from symbol loader
		foreach (var (addr, comment) in symbolLoader.Comments) {
			_comments[addr] = comment;
		}

		// Import data definitions from symbol loader
		foreach (var (addr, def) in symbolLoader.DataDefinitions) {
			_dataDefinitions.TryAdd(addr, def);
		}

		// If DIZ data has additional comments per-label, import those too
		if (symbolLoader.DizData is not null) {
			foreach (var (addr, dizLabel) in symbolLoader.DizData.Labels) {
				if (!string.IsNullOrWhiteSpace(dizLabel.Comment) && !_comments.ContainsKey((uint)addr)) {
					_comments[(uint)addr] = dizLabel.Comment;
				}
			}
		}
	}

	/// <summary>
	/// Add data definition for a region that should not be disassembled as code
	/// </summary>
	public void AddDataRegion(uint address, DataDefinition definition) {
		_dataDefinitions[address] = definition;
	}

	/// <summary>
	/// Check if an address is in a data region (from definitions or CDL/DIZ)
	/// </summary>
	private bool IsInDataRegion(uint address, int romOffset = -1) {
		// Check explicit data definitions first
		foreach (var (dataAddr, def) in _dataDefinitions) {
			var size = def.Type.ToLowerInvariant() switch {
				"byte" => 1,
				"word" => 2,
				_ => 1
			} * def.Count;
			if (address >= dataAddr && address < dataAddr + size)
				return true;
		}

		// Check CDL/DIZ data if available and we have a ROM offset
		if (_symbolLoader is not null && romOffset >= 0) {
			var isData = _symbolLoader.IsData(romOffset);
			if (isData == true)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Check if a ROM offset should be treated as code according to CDL/DIZ
	/// </summary>
	private bool? ShouldTreatAsCode(int romOffset) {
		if (_symbolLoader is null)
			return null;

		return _symbolLoader.IsCode(romOffset);
	}

	/// <summary>
	/// Disassemble ROM using recursive descent with multi-bank support and CDL/DIZ hints
	/// </summary>
	public DisassemblyResult Disassemble(ReadOnlySpan<byte> rom, uint[] entryPoints, bool allBanks = false) {
		var romInfo = _platformAnalyzer.Analyze(rom);
		var result = new DisassemblyResult { RomInfo = romInfo };

		// Initialize bank blocks dictionary
		for (int i = 0; i < _platformAnalyzer.BankCount; i++) {
			result.BankBlocks[i] = [];
		}

		// Determine which bank the entry points are in (usually last bank for NES)
		var fixedBank = _platformAnalyzer.BankCount - 1;

		// Queue entry points in fixed bank
		foreach (var entry in entryPoints) {
			if (IsValidAddress(entry)) {
				var bank = _platformAnalyzer.IsInSwitchableRegion(entry) ? 0 : fixedBank;
				if (entry >= 0xc000) bank = fixedBank; // Fixed bank for NES MMC1
				_codeQueue.Enqueue((entry, bank));
				AddLabel(entry, entry == entryPoints[0] ? "reset" : $"entry_{entry:x4}", bank);
			}
		}

		// If allBanks, also queue $8000 for each switchable bank
		if (allBanks && _platformAnalyzer.BankCount > 1) {
			for (int bank = 0; bank < _platformAnalyzer.BankCount - 1; bank++) {
				_codeQueue.Enqueue((0x8000, bank));
				AddLabel(0x8000, $"bank{bank}_start", bank);
			}
		}

		// Add CDL-identified code entry points (subroutine starts)
		if (_symbolLoader?.CdlData is not null) {
			foreach (var offset in _symbolLoader.CdlData.SubEntryPoints) {
				var address = RomOffsetToAddress((uint)offset, fixedBank);
				if (address.HasValue && IsValidAddress(address.Value)) {
					if (!_visited.ContainsKey((address.Value, fixedBank))) {
						_codeQueue.Enqueue((address.Value, fixedBank));
						if (GetLabel(address.Value, fixedBank) is null) {
							AddLabel(address.Value, $"sub_{offset:x4}", fixedBank);
						}
					}
				}
			}
		}

		// Add DIZ-identified opcode entry points
		if (_symbolLoader?.DizData is not null) {
			foreach (var offset in _symbolLoader.DizData.GetOpcodeOffsets()) {
				var address = RomOffsetToAddress((uint)offset, fixedBank);
				if (address.HasValue && IsValidAddress(address.Value)) {
					if (!_visited.ContainsKey((address.Value, fixedBank))) {
						_codeQueue.Enqueue((address.Value, fixedBank));
					}
				}
			}
		}

		// Recursive descent disassembly
		while (_codeQueue.Count > 0) {
			var (address, bank) = _codeQueue.Dequeue();
			if (_visited.ContainsKey((address, bank))) continue;
			DisassembleBlock(rom, address, bank, result);
		}

		// Process any discovered bank calls
		ProcessBankCalls(rom, result);

		// Detect pointer tables in unvisited regions
		DetectPointerTables(rom, result);

		// Copy global labels and comments
		foreach (var kvp in _labels) result.Labels[kvp.Key] = kvp.Value;
		foreach (var kvp in _comments) result.Comments[kvp.Key] = kvp.Value;

		// Copy bank-specific labels
		foreach (var kvp in _bankLabels) {
			result.BankLabels[kvp.Key] = kvp.Value;
		}

		// Copy cross-references
		foreach (var kvp in _crossRefs) {
			result.CrossReferences[kvp.Key] = [.. kvp.Value];
		}

		// Copy detected data regions
		foreach (var kvp in _dataDefinitions) {
			result.DataRegions[kvp.Key] = kvp.Value;
		}

		// Also add blocks to main list
		foreach (var bankBlocks in result.BankBlocks.Values) {
			result.Blocks.AddRange(bankBlocks);
		}

		return result;
	}

	/// <summary>
	/// Detect pointer tables in unvisited ROM regions.
	/// A pointer table is consecutive word values that point to known code locations.
	/// </summary>
	private void DetectPointerTables(ReadOnlySpan<byte> rom, DisassemblyResult result) {
		// Only run if we have cross-references to match against
		if (_crossRefs.Count == 0) return;

		var knownCodeAddresses = new HashSet<uint>(_visited.Keys.Select(k => k.Address));
		var fixedBank = _platformAnalyzer.BankCount - 1;

		// Scan through ROM looking for pointer table patterns
		var headerSize = _platformAnalyzer.Platform == "NES" ? 16 : 0;
		var scanStart = headerSize;
		var scanEnd = Math.Min(rom.Length - 1, 0x10000); // Limit scan size

		for (int offset = scanStart; offset < scanEnd - 1; offset += 2) {
			// Check if this offset is already visited/known
			var addr = RomOffsetToAddress((uint)offset, fixedBank);
			if (!addr.HasValue) continue;
			if (_visited.ContainsKey((addr.Value, fixedBank))) continue;

			// Try to detect a pointer table starting here
			var tableInfo = TryDetectPointerTable(rom, offset, knownCodeAddresses, fixedBank);
			if (tableInfo.HasValue) {
				var (tableAddr, count) = tableInfo.Value;

				// Add as a data region
				_dataDefinitions[tableAddr] = new DataDefinition("word", count, $"Pointer table ({count} entries)");
				result.Comments[tableAddr] = $"Detected pointer table with {count} entries";

				// Skip past this table
				offset += (count * 2) - 2;
			}
		}
	}

	/// <summary>
	/// Try to detect a pointer table at the given ROM offset.
	/// Returns (address, entry count) if a table is found, null otherwise.
	/// </summary>
	private (uint Address, int Count)? TryDetectPointerTable(
		ReadOnlySpan<byte> rom,
		int offset,
		HashSet<uint> knownCodeAddresses,
		int bank) {
		const int MinTableSize = 3; // At least 3 consecutive pointers
		const int MaxTableSize = 256; // Reasonable limit

		var addr = RomOffsetToAddress((uint)offset, bank);
		if (!addr.HasValue) return null;

		int count = 0;
		int currentOffset = offset;

		while (count < MaxTableSize && currentOffset + 1 < rom.Length) {
			// Read potential pointer (little-endian word)
			var lo = rom[currentOffset];
			var hi = rom[currentOffset + 1];
			var pointer = (uint)(lo | (hi << 8));

			// Check if this points to a known code address
			if (!IsValidAddress(pointer) || !knownCodeAddresses.Contains(pointer)) {
				break;
			}

			count++;
			currentOffset += 2;
		}

		if (count >= MinTableSize) {
			return (addr.Value, count);
		}

		return null;
	}

	/// <summary>
	/// Convert ROM file offset to CPU address (inverse of AddressToOffset)
	/// </summary>
	private uint? RomOffsetToAddress(uint offset, int bank) {
		if (_platformAnalyzer.Platform == "NES") {
			// NES: PRG ROM typically starts at $8000, with 16-byte iNES header
			if (offset >= 0x10) {
				var prgOffset = offset - 0x10;
				return (uint)(0x8000 + prgOffset);
			}
		}
		if (_platformAnalyzer.Platform == "Atari 2600") {
			// Atari 2600: ROM at $f000-$ffff for 4K
			return (uint)(0xf000 + offset);
		}
		// Default: assume direct mapping
		return offset;
	}

	private bool IsValidAddress(uint address) {
		if (_platformAnalyzer.Platform == "Atari 2600") {
			return address >= 0xf000 && address <= 0xffff;
		}
		if (_platformAnalyzer.Platform == "NES") {
			return address >= 0x8000 && address <= 0xffff;
		}
		return true;
	}

	/// <summary>
	/// Determine the target bank for an address using Pansy memory regions if available,
	/// otherwise fall back to platform-specific logic.
	/// </summary>
	private int GetTargetBank(uint target, int currentBank) {
		// First, try to get bank from Pansy memory regions
		if (_symbolLoader is not null) {
			var pansyBank = _symbolLoader.GetBankForAddress(target);
			if (pansyBank.HasValue) {
				return pansyBank.Value;
			}
		}

		// Fall back to platform-specific bank detection
		if (_platformAnalyzer.Platform == "NES") {
			// In NES MMC1, $C000-$FFFF is fixed (last bank)
			// $8000-$BFFF uses current switchable bank
			if (target >= 0xc000) {
				return _platformAnalyzer.BankCount - 1;
			}
		}
		else if (_platformAnalyzer.Platform == "SNES") {
			// For SNES LoROM, use bank byte from address
			// Address format: $BB:XXXX where BB is bank
			if (target > 0xFFFF) {
				return (int)(target >> 16);
			}
		}
		else if (_platformAnalyzer.Platform == "Game Boy") {
			// For Game Boy, ROM bank is in $4000-$7FFF region
			// $0000-$3FFF is always bank 0, $4000-$7FFF is switchable
			if (target >= 0x4000 && target < 0x8000) {
				// Use current bank for switchable region
				return currentBank;
			}
			else if (target < 0x4000) {
				// Fixed bank 0
				return 0;
			}
		}
		else if (_platformAnalyzer.Platform == "GBA") {
			// For GBA, ROM typically maps to $08000000-$09FFFFFF
			// Bank is determined by ROM size (typically 32KB banks)
			if (target >= 0x08000000 && target < 0x0A000000) {
				return (int)((target - 0x08000000) / 0x8000);
			}
		}

		// Default: keep current bank
		return currentBank;
	}

	private void DisassembleBlock(ReadOnlySpan<byte> rom, uint startAddress, int bank, DisassemblyResult result) {
		var lines = new List<DisassembledLine>();
		var address = startAddress;
		var maxInstructions = 10000;

		while (maxInstructions-- > 0) {
			if (_visited.ContainsKey((address, bank))) break;

			// Use bank-aware address mapping to get ROM offset
			var offset = _platformAnalyzer.AddressToOffset(address, rom.Length, bank);
			if (offset < 0 || offset >= rom.Length) break;

			// Stop if we hit a data region (check both definitions and CDL/DIZ)
			if (IsInDataRegion(address, offset)) break;

			// Check CDL/DIZ for explicit data marking
			var shouldBeCode = ShouldTreatAsCode(offset);
			if (shouldBeCode == false) {
				// CDL/DIZ says this is data, not code - stop disassembly here
				break;
			}

			_visited[(address, bank)] = true;

			var slice = rom[offset..];
			if (slice.Length == 0) break;

			var instruction = _cpuDecoder.Decode(slice, address);
			if (instruction.Bytes.Length == 0) break;

// Check for BRK-based bank switch
if (instruction.Mnemonic == "brk") {
var bankSwitch = _platformAnalyzer.DetectBankSwitch(rom, address, bank);
if (bankSwitch != null) {
// Record bank call for later processing
_bankCalls.Add((bankSwitch.TargetBank, bankSwitch.TargetAddress));
AddLabel(bankSwitch.TargetAddress, bankSwitch.FunctionName ?? $"bank{bankSwitch.TargetBank}_func", bankSwitch.TargetBank);

// Add comment about bank call
var comment = $"Bank call: bank {bankSwitch.TargetBank}, {bankSwitch.FunctionName}";
lines.Add(new DisassembledLine(
address, instruction.Bytes, GetLabel(address, bank),
FormatInstruction(instruction), comment, bank
));

// Add the two data bytes following BRK
if (offset + 2 < rom.Length) {
var funcIdx = rom[offset + 1];
var targetBank = rom[offset + 2];
lines.Add(new DisassembledLine(
address + 1, [funcIdx], null, $".byte ${funcIdx:x2}",
$"Function index", bank
));
lines.Add(new DisassembledLine(
address + 2, [targetBank], null, $".byte ${targetBank:x2}",
$"Target bank", bank
));
}

address += 3; // BRK + 2 data bytes
continue;
}
}

var label = GetLabel(address, bank);
var operandAddr = GetOperandAddress(instruction);
var hwLabel = operandAddr.HasValue ? _platformAnalyzer.GetRegisterLabel(operandAddr.Value) : null;
var lineComment = _comments.GetValueOrDefault(address) ?? hwLabel;

lines.Add(new DisassembledLine(
address, instruction.Bytes, label,
FormatInstruction(instruction), lineComment, bank
));

// Handle control flow
if (_cpuDecoder.IsControlFlow(instruction)) {
	var crossRefType = GetCrossRefType(instruction);

	foreach (var target in _cpuDecoder.GetTargets(instruction, address)) {
		if (!IsValidAddress(target)) continue;

		// Record cross-reference
		AddCrossRef(address, bank, target, crossRefType);

		// Determine target bank using Pansy memory regions if available
		var targetBank = GetTargetBank(target, bank);

		// ALWAYS create label (even for backward branches)
		AddLabel(target, $"loc_{target:x4}", targetBank);

		if (!_visited.ContainsKey((target, targetBank))) {
			_codeQueue.Enqueue((target, targetBank));
		}
	}

	if (IsUnconditionalBranch(instruction)) break;
}

address += (uint)instruction.Bytes.Length;
}

if (lines.Count > 0) {
var block = new DisassembledBlock(startAddress, address, MemoryRegion.Code, lines, bank);
if (result.BankBlocks.TryGetValue(bank, out var bankBlocks)) {
bankBlocks.Add(block);
}
}
}

private void ProcessBankCalls(ReadOnlySpan<byte> rom, DisassemblyResult result) {
// Process any bank calls we discovered
foreach (var (targetBank, targetAddress) in _bankCalls) {
if (!_visited.ContainsKey((targetAddress, targetBank))) {
_codeQueue.Enqueue((targetAddress, targetBank));
}
}

// Continue disassembly for bank calls
while (_codeQueue.Count > 0) {
var (address, bank) = _codeQueue.Dequeue();
if (_visited.ContainsKey((address, bank))) continue;
DisassembleBlock(rom, address, bank, result);
}
}

private static uint? GetOperandAddress(DecodedInstruction instruction) {
var operand = instruction.Operand;
if (string.IsNullOrEmpty(operand)) return null;

operand = operand.TrimStart('#', '(').TrimEnd(')', ',', 'x', 'y', 'X', 'Y', ' ');
if (operand.StartsWith('$') && uint.TryParse(operand[1..], System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
return addr;
}
return null;
}

private static string FormatInstruction(DecodedInstruction instruction) {
return string.IsNullOrEmpty(instruction.Operand)
? instruction.Mnemonic
: $"{instruction.Mnemonic} {instruction.Operand}";
}

private static bool IsUnconditionalBranch(DecodedInstruction instruction) {
return instruction.Mnemonic is "jmp" or "rts" or "rti" or "brk";
}

/// <summary>
	/// Add a label if one doesn't exist.
	/// User-defined labels (from DIZ/symbol files) are never overwritten by auto-generated labels.
	/// For bank-specific addresses, uses bank parameter to determine storage location.
	/// </summary>
	public void AddLabel(uint address, string name, int? bank = null) {
		// If bank is explicitly specified, store as bank-specific
		if (bank.HasValue) {
			var key = (address, bank.Value);
			// Never overwrite user-defined labels
			if (_userDefinedBankLabels.Contains(key))
				return;
			_bankLabels.TryAdd(key, name);
		} else {
			// Global label (no bank specified)
			if (_userDefinedLabels.Contains(address))
				return;
			_labels.TryAdd(address, name);
		}
	}

	/// <summary>
	/// Add a label from user input (symbol file, DIZ, etc.) which takes priority.
	/// </summary>
	public void AddUserLabel(uint address, string name, int? bank = null) {
		if (bank.HasValue) {
			var key = (address, bank.Value);
			_bankLabels[key] = name;
			_userDefinedBankLabels.Add(key);
		} else {
			_labels[address] = name;
			_userDefinedLabels.Add(address);
		}
	}

	/// <summary>
	/// Get a label for an address, considering bank context.
	/// Returns bank-specific label if available, otherwise falls back to global label.
	/// </summary>
	public string? GetLabel(uint address, int? bank = null) {
		// Try bank-specific label first
		if (bank.HasValue && _bankLabels.TryGetValue((address, bank.Value), out var bankLabel))
			return bankLabel;

		// Fall back to global label
		return _labels.GetValueOrDefault(address);
	}

	/// <summary>
	/// Checks if a label at the given address is user-defined.
	/// When bank is specified, only checks bank-specific labels (no fallback to global).
	/// </summary>
	public bool IsUserDefinedLabel(uint address, int? bank = null) {
		if (bank.HasValue)
			return _userDefinedBankLabels.Contains((address, bank.Value));
		return _userDefinedLabels.Contains(address);
	}

public void AddComment(uint address, string comment) {
_comments[address] = comment;
}
}
