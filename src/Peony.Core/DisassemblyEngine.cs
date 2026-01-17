namespace Peony.Core;

/// <summary>
/// Core disassembly engine with multi-bank support and CDL/DIZ integration
/// </summary>
public class DisassemblyEngine {
	private readonly ICpuDecoder _cpuDecoder;
	private readonly IPlatformAnalyzer _platformAnalyzer;
	private readonly Dictionary<uint, string> _labels = [];
	private readonly Dictionary<uint, string> _comments = [];
	private readonly Dictionary<uint, DataDefinition> _dataDefinitions = [];
	private readonly Dictionary<(uint Address, int Bank), bool> _visited = [];
	private readonly Queue<(uint Address, int Bank)> _codeQueue = new();
	private readonly HashSet<(int TargetBank, uint TargetAddress)> _bankCalls = [];
	private SymbolLoader? _symbolLoader;

	public DisassemblyEngine(ICpuDecoder cpuDecoder, IPlatformAnalyzer platformAnalyzer) {
		_cpuDecoder = cpuDecoder;
		_platformAnalyzer = platformAnalyzer;
	}

	/// <summary>
	/// Set a symbol loader for CDL/DIZ/symbol file integration
	/// </summary>
	public void SetSymbolLoader(SymbolLoader symbolLoader) {
		_symbolLoader = symbolLoader;

		// Import labels from symbol loader
		foreach (var (addr, label) in symbolLoader.Labels) {
			_labels.TryAdd(addr, label);
		}

		// Import comments from symbol loader
		foreach (var (addr, comment) in symbolLoader.Comments) {
			_comments.TryAdd(addr, comment);
		}

		// Import data definitions from symbol loader
		foreach (var (addr, def) in symbolLoader.DataDefinitions) {
			_dataDefinitions.TryAdd(addr, def);
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
				AddLabel(entry, entry == entryPoints[0] ? "reset" : $"entry_{entry:x4}");
			}
		}

		// If allBanks, also queue $8000 for each switchable bank
		if (allBanks && _platformAnalyzer.BankCount > 1) {
			for (int bank = 0; bank < _platformAnalyzer.BankCount - 1; bank++) {
				_codeQueue.Enqueue((0x8000, bank));
				AddLabel(0x8000, $"bank{bank}_start");
			}
		}

		// Add CDL-identified code entry points (subroutine starts)
		if (_symbolLoader?.CdlData is not null) {
			foreach (var offset in _symbolLoader.CdlData.SubEntryPoints) {
				var address = RomOffsetToAddress((uint)offset, fixedBank);
				if (address.HasValue && IsValidAddress(address.Value)) {
					if (!_visited.ContainsKey((address.Value, fixedBank))) {
						_codeQueue.Enqueue((address.Value, fixedBank));
						if (!_labels.ContainsKey(address.Value)) {
							AddLabel(address.Value, $"sub_{offset:x4}");
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

		// Copy labels and comments
		foreach (var kvp in _labels) result.Labels[kvp.Key] = kvp.Value;
		foreach (var kvp in _comments) result.Comments[kvp.Key] = kvp.Value;

		// Also add blocks to main list
		foreach (var bankBlocks in result.BankBlocks.Values) {
			result.Blocks.AddRange(bankBlocks);
		}

		return result;
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
AddLabel(bankSwitch.TargetAddress, bankSwitch.FunctionName ?? $"bank{bankSwitch.TargetBank}_func");

// Add comment about bank call
var comment = $"Bank call: bank {bankSwitch.TargetBank}, {bankSwitch.FunctionName}";
lines.Add(new DisassembledLine(
address, instruction.Bytes, _labels.GetValueOrDefault(address),
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

var label = _labels.GetValueOrDefault(address);
var operandAddr = GetOperandAddress(instruction);
var hwLabel = operandAddr.HasValue ? _platformAnalyzer.GetRegisterLabel(operandAddr.Value) : null;
var lineComment = _comments.GetValueOrDefault(address) ?? hwLabel;

lines.Add(new DisassembledLine(
address, instruction.Bytes, label,
FormatInstruction(instruction), lineComment, bank
));

// Handle control flow
if (_cpuDecoder.IsControlFlow(instruction)) {
foreach (var target in _cpuDecoder.GetTargets(instruction, address)) {
if (!IsValidAddress(target)) continue;

// Determine target bank
var targetBank = bank;
if (_platformAnalyzer.Platform == "NES") {
// In NES MMC1, $C000-$FFFF is fixed (last bank)
// $8000-$BFFF uses current switchable bank
if (target >= 0xc000) {
targetBank = _platformAnalyzer.BankCount - 1;
}
}

if (!_visited.ContainsKey((target, targetBank))) {
_codeQueue.Enqueue((target, targetBank));
AddLabel(target, $"loc_{target:x4}");
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

public void AddLabel(uint address, string name) {
_labels.TryAdd(address, name);
}

public void AddComment(uint address, string comment) {
_comments[address] = comment;
}
}
