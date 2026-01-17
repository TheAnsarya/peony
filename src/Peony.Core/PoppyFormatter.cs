namespace Peony.Core;

/// <summary>
/// Formats disassembly output in Poppy assembler format with bank annotations
/// </summary>
public class PoppyFormatter : IOutputFormatter {
public string Name => "Poppy";
public string Extension => ".pasm";

public void Generate(DisassemblyResult result, string outputPath) {
var content = Format(result);
File.WriteAllText(outputPath, content);
}

public string Format(DisassemblyResult result) {
var sb = new System.Text.StringBuilder();

// Header
sb.AppendLine("; Disassembled by Peony");
sb.AppendLine($"; Platform: {result.RomInfo.Platform}");
if (!string.IsNullOrEmpty(result.RomInfo.Mapper)) {
sb.AppendLine($"; Mapper: {result.RomInfo.Mapper}");
}

foreach (var kvp in result.RomInfo.Metadata) {
sb.AppendLine($"; {kvp.Key}: {kvp.Value}");
}
sb.AppendLine();

// Check if we have multiple banks
var hasMultipleBanks = result.BankBlocks.Count > 1 &&
   result.BankBlocks.Values.Any(b => b.Count > 0);

if (hasMultipleBanks) {
// Output bank by bank
foreach (var bank in result.BankBlocks.Keys.OrderBy(b => b)) {
var blocks = result.BankBlocks[bank];
if (blocks.Count == 0) continue;

sb.AppendLine();
sb.AppendLine($"; ===========================================================================");
sb.AppendLine($"; BANK {bank}");
sb.AppendLine($"; ===========================================================================");
sb.AppendLine();

// Add bank directive
sb.AppendLine($"\t.bank {bank}");
sb.AppendLine($"\t.org $8000");
sb.AppendLine();

foreach (var block in blocks.OrderBy(b => b.StartAddress)) {
FormatBlock(sb, block, result);
}
}
} else {
// Single bank output (original behavior)
var allBlocks = result.Blocks.Count > 0 ? result.Blocks :
result.BankBlocks.Values.SelectMany(b => b).ToList();

foreach (var block in allBlocks.OrderBy(b => b.StartAddress)) {
FormatBlock(sb, block, result);
}
}

return sb.ToString();
}

private static void FormatBlock(System.Text.StringBuilder sb, DisassembledBlock block, DisassemblyResult result) {
sb.AppendLine($"; --- Block at ${block.StartAddress:x4}-${block.EndAddress:x4} ---");

foreach (var line in block.Lines) {
FormatLine(sb, line, result);
}
sb.AppendLine();
}

private static void FormatLine(System.Text.StringBuilder sb, DisassembledLine line, DisassemblyResult result) {
// Label on its own line
if (!string.IsNullOrEmpty(line.Label)) {
sb.AppendLine($"{line.Label}:");
}

// Build the instruction line
var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));

// Check if this operand references a known label
var formatted = FormatWithLabels(line.Content, result);

var instruction = $"\t{formatted,-24}";
var bytesComment = $"; {line.Address:x4}: {bytes,-12}";

if (!string.IsNullOrEmpty(line.Comment)) {
sb.AppendLine($"{instruction}{bytesComment} {line.Comment}");
} else {
sb.AppendLine($"{instruction}{bytesComment}");
}
}

private static string FormatWithLabels(string instruction, DisassemblyResult result) {
	// Try to replace addresses with labels
	// Use regex to ensure we only match complete addresses (not partial)
	foreach (var kvp in result.Labels) {
		// Skip immediate mode for small values (< 0x100) - keep as constants
		// e.g. lda #$00 should stay as #$00, not #gen_byte_00
		if (kvp.Key < 0x100 && instruction.Contains($"#${kvp.Key:x2}", StringComparison.OrdinalIgnoreCase))
			continue;

		// Match $xxxx (4-digit hex) with word boundary
		var pattern4 = $@"\${kvp.Key:x4}(?![0-9a-f])";
		if (System.Text.RegularExpressions.Regex.IsMatch(instruction, pattern4, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
			instruction = System.Text.RegularExpressions.Regex.Replace(instruction, pattern4, kvp.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			continue;
		}

		// For addresses < 0x100, also match short form like $xx (but not immediate mode)
		if (kvp.Key < 0x100) {
			// Only match if it's NOT immediate mode (not preceded by #)
			var pattern2 = $@"(?<!#)\${kvp.Key:x2}(?![0-9a-f])";
			if (System.Text.RegularExpressions.Regex.IsMatch(instruction, pattern2, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
				instruction = System.Text.RegularExpressions.Regex.Replace(instruction, pattern2, kvp.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			}
		}
	}
	return instruction;
}
}
