namespace Peony.Core;

/// <summary>
/// Core disassembly engine - orchestrates CPU decoding and platform analysis
/// </summary>
public class DisassemblyEngine {
private readonly ICpuDecoder _cpuDecoder;
private readonly IPlatformAnalyzer _platformAnalyzer;
private readonly Dictionary<uint, string> _labels = [];
private readonly Dictionary<uint, string> _comments = [];
private readonly HashSet<uint> _visited = [];
private readonly Queue<uint> _codeQueue = new();
private readonly int _romSize;
private readonly uint _romBase;

public DisassemblyEngine(ICpuDecoder cpuDecoder, IPlatformAnalyzer platformAnalyzer, int romSize = 0) {
_cpuDecoder = cpuDecoder;
_platformAnalyzer = platformAnalyzer;
_romSize = romSize;

// Calculate ROM base address based on platform
_romBase = platformAnalyzer.Platform switch {
"Atari 2600" => romSize switch {
<= 2048 => 0xf800,  // 2K at $F800-$FFFF
<= 4096 => 0xf000,  // 4K at $F000-$FFFF
_ => 0xf000         // Banked ROMs, first bank at $F000
},
"NES" => 0x8000,
"Game Boy" => 0x0000,
_ => 0x0000
};
}

/// <summary>
/// Disassemble ROM using recursive descent
/// </summary>
public DisassemblyResult Disassemble(ReadOnlySpan<byte> rom, uint[] entryPoints) {
var romInfo = _platformAnalyzer.Analyze(rom);
var result = new DisassemblyResult { RomInfo = romInfo };

// Queue entry points
foreach (var entry in entryPoints) {
_codeQueue.Enqueue(entry);
AddLabel(entry, $"entry_{entry:x4}");
}

// Recursive descent disassembly
while (_codeQueue.Count > 0) {
var address = _codeQueue.Dequeue();
if (_visited.Contains(address)) continue;
DisassembleBlock(rom, address, result);
}

// Copy labels and comments
foreach (var kvp in _labels) result.Labels[kvp.Key] = kvp.Value;
foreach (var kvp in _comments) result.Comments[kvp.Key] = kvp.Value;

return result;
}

private void DisassembleBlock(ReadOnlySpan<byte> rom, uint startAddress, DisassemblyResult result) {
var lines = new List<DisassembledLine>();
var address = startAddress;

while (true) {
if (_visited.Contains(address)) break;
_visited.Add(address);

// Check bounds
var offset = AddressToOffset(address, rom.Length);
if (offset < 0 || offset >= rom.Length) break;

// Decode instruction
var instruction = _cpuDecoder.Decode(rom[offset..], address);
var label = _labels.GetValueOrDefault(address);
var hwLabel = _platformAnalyzer.GetRegisterLabel(address);
var comment = _comments.GetValueOrDefault(address) ?? hwLabel;

lines.Add(new DisassembledLine(
address,
instruction.Bytes,
label,
FormatInstruction(instruction),
comment
));

// Handle control flow
if (_cpuDecoder.IsControlFlow(instruction)) {
foreach (var target in _cpuDecoder.GetTargets(instruction, address)) {
if (!_visited.Contains(target)) {
_codeQueue.Enqueue(target);
AddLabel(target, $"loc_{target:x4}");
}
}

// Stop on unconditional jumps/returns
if (IsUnconditionalBranch(instruction)) break;
}

address += (uint)instruction.Bytes.Length;
}

if (lines.Count > 0) {
result.Blocks.Add(new DisassembledBlock(
startAddress,
address,
MemoryRegion.Code,
lines
));
}
}

private static string FormatInstruction(DecodedInstruction instruction) {
return string.IsNullOrEmpty(instruction.Operand)
? instruction.Mnemonic
: $"{instruction.Mnemonic} {instruction.Operand}";
}

private static bool IsUnconditionalBranch(DecodedInstruction instruction) {
return instruction.Mnemonic is "jmp" or "rts" or "rti" or "brk";
}

private int AddressToOffset(uint address, int romLength) {
// Atari 2600: ROM mapped at end of address space
if (_platformAnalyzer.Platform == "Atari 2600") {
// For 8K ROMs with F8 banking, both banks map to $F000-$FFFF
// Bank 0 = first 4K, Bank 1 = second 4K
// Reset vector is in bank 1 (second 4K)
if (romLength == 8192) {
// Addresses $F000-$FFFF map to ROM
if (address >= 0xf000 && address <= 0xffff) {
// For initial disassembly, use bank 1 (second 4K)
return (int)(address - 0xf000) + 4096;
}
}
else if (romLength == 4096) {
if (address >= 0xf000 && address <= 0xffff) {
return (int)(address - 0xf000);
}
}
else if (romLength == 2048) {
if (address >= 0xf800 && address <= 0xffff) {
return (int)(address - 0xf800);
}
}
return -1; // Address not in ROM
}

// Default: direct mapping
return (int)address;
}

public void AddLabel(uint address, string name) {
_labels.TryAdd(address, name);
}

public void AddComment(uint address, string comment) {
_comments[address] = comment;
}
}
