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

public DisassemblyEngine(ICpuDecoder cpuDecoder, IPlatformAnalyzer platformAnalyzer) {
_cpuDecoder = cpuDecoder;
_platformAnalyzer = platformAnalyzer;
}

/// <summary>
/// Disassemble ROM using recursive descent
/// </summary>
public DisassemblyResult Disassemble(ReadOnlySpan<byte> rom, uint[] entryPoints) {
var romInfo = _platformAnalyzer.Analyze(rom);
var result = new DisassemblyResult { RomInfo = romInfo };

// Queue entry points
foreach (var entry in entryPoints) {
// Filter out obviously invalid entry points
if (IsValidAddress(entry, rom.Length)) {
_codeQueue.Enqueue(entry);
AddLabel(entry, entry == entryPoints[0] ? "reset" : $"entry_{entry:x4}");
}
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

private bool IsValidAddress(uint address, int romLength) {
if (_platformAnalyzer.Platform == "Atari 2600") {
return address >= 0xf000 && address <= 0xffff;
}
if (_platformAnalyzer.Platform == "NES") {
return address >= 0x8000 && address <= 0xffff;
}
return true;
}

private void DisassembleBlock(ReadOnlySpan<byte> rom, uint startAddress, DisassemblyResult result) {
var lines = new List<DisassembledLine>();
var address = startAddress;
var maxInstructions = 10000; // Safety limit

while (maxInstructions-- > 0) {
if (_visited.Contains(address)) break;
_visited.Add(address);

// Use platform analyzer's AddressToOffset for correct mapping
var offset = _platformAnalyzer.AddressToOffset(address, rom.Length);
if (offset < 0 || offset >= rom.Length) break;

// Decode instruction
var slice = rom[offset..];
if (slice.Length == 0) break;

var instruction = _cpuDecoder.Decode(slice, address);
if (instruction.Bytes.Length == 0) break;

var label = _labels.GetValueOrDefault(address);
var operandAddr = GetOperandAddress(instruction);
var hwLabel = operandAddr.HasValue ? _platformAnalyzer.GetRegisterLabel(operandAddr.Value) : null;
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
if (!_visited.Contains(target) && IsValidAddress(target, rom.Length)) {
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
