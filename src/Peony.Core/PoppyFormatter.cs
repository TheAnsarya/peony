namespace Peony.Output;

using Peony.Core;

/// <summary>
/// Poppy assembler output formatter (.pasm files)
/// </summary>
public class PoppyFormatter : IOutputFormatter {
public string Name => "Poppy";
public string Extension => ".pasm";

public void Generate(DisassemblyResult result, string outputPath) {
using var writer = new StreamWriter(outputPath);

WriteHeader(writer, result.RomInfo);
WriteConstants(writer, result);
WriteLabels(writer, result);
WriteCode(writer, result);
WriteVectors(writer, result);
}

private static void WriteHeader(StreamWriter writer, RomInfo info) {
writer.WriteLine($"; 🌺 Peony Disassembly → 🌸 Poppy Assembly");
writer.WriteLine($"; Platform: {info.Platform}");
writer.WriteLine($"; Size: {info.Size} bytes");
if (info.Mapper != null)
writer.WriteLine($"; Mapper: {info.Mapper}");
writer.WriteLine();

// Platform-specific directives
var cpu = info.Platform switch {
"Atari 2600" => "6502",
"NES" => "6502",
"SNES" => "65816",
"Game Boy" => "sm83",
_ => "6502"
};

writer.WriteLine($".cpu {cpu}");
writer.WriteLine();
}

private static void WriteConstants(StreamWriter writer, DisassemblyResult result) {
var hwLabels = new Dictionary<uint, string>();

// Collect hardware register references
foreach (var block in result.Blocks) {
foreach (var line in block.Lines) {
if (line.Comment != null && !line.Comment.StartsWith(";")) {
// This is a hardware label
// Try to extract address from instruction
var parts = line.Content.Split(' ', 2);
if (parts.Length == 2) {
var operand = parts[1].Trim();
if (TryParseAddress(operand, out var addr)) {
hwLabels.TryAdd(addr, line.Comment);
}
}
}
}
}

if (hwLabels.Count > 0) {
writer.WriteLine("; Hardware Registers");
foreach (var (addr, label) in hwLabels.OrderBy(x => x.Key)) {
writer.WriteLine($"{label,-16} = ${addr:x4}");
}
writer.WriteLine();
}
}

private static void WriteLabels(StreamWriter writer, DisassemblyResult result) {
// RAM labels (zero page and work RAM)
var ramLabels = result.Labels
.Where(x => x.Key < 0x0800 || (x.Key >= 0x0080 && x.Key < 0x0100))
.OrderBy(x => x.Key)
.ToList();

if (ramLabels.Count > 0) {
writer.WriteLine("; RAM Labels");
foreach (var (addr, label) in ramLabels) {
writer.WriteLine($"{label,-16} = ${addr:x4}");
}
writer.WriteLine();
}
}

private static void WriteCode(StreamWriter writer, DisassemblyResult result) {
var sortedBlocks = result.Blocks.OrderBy(b => b.StartAddress).ToList();

foreach (var block in sortedBlocks) {
// Section comment
var typeStr = block.Type switch {
MemoryRegion.Code => "Code",
MemoryRegion.Data => "Data",
MemoryRegion.Graphics => "Graphics",
_ => "Unknown"
};

writer.WriteLine($"; === {typeStr} Block ${block.StartAddress:x4}-${block.EndAddress:x4} ===");
writer.WriteLine($".org ${block.StartAddress:x4}");
writer.WriteLine();

foreach (var line in block.Lines) {
// Write label on its own line
if (line.Label != null) {
writer.WriteLine($"{line.Label}:");
}

// Write instruction with comment
var instruction = line.Content;
var comment = FormatComment(line);

if (string.IsNullOrEmpty(comment)) {
writer.WriteLine($"\t{instruction}");
} else {
writer.WriteLine($"\t{instruction,-24} {comment}");
}
}

writer.WriteLine();
}
}

private static void WriteVectors(StreamWriter writer, DisassemblyResult result) {
// For platforms with vector tables
var platform = result.RomInfo.Platform;

if (platform == "NES") {
writer.WriteLine("; === Vectors ===");
writer.WriteLine(".org $fffa");
writer.WriteLine("\t.word nmi_handler");
writer.WriteLine("\t.word reset");
writer.WriteLine("\t.word irq_handler");
} else if (platform == "Atari 2600") {
writer.WriteLine("; === Vectors ===");
writer.WriteLine(".org $fffc");
writer.WriteLine("\t.word reset");
writer.WriteLine("\t.word reset");
}
}

private static string FormatComment(DisassembledLine line) {
var parts = new List<string>();

// Address and bytes
var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));
parts.Add($"; ${line.Address:x4}: {bytes}");

// Hardware label
if (line.Comment != null)
parts.Add(line.Comment);

return string.Join(" ", parts);
}

private static bool TryParseAddress(string operand, out uint address) {
address = 0;
operand = operand.TrimStart('#', '(').TrimEnd(')', ',', 'x', 'y', 'X', 'Y');

if (operand.StartsWith('$')) {
return uint.TryParse(operand[1..], System.Globalization.NumberStyles.HexNumber, null, out address);
}
return false;
}
}
