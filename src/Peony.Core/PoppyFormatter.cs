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
sb.AppendLine($".system:{result.RomInfo.Platform}");
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
FormatBlock(sb, block, result, bank);
}
}
} else {
// Single bank output (original behavior)
var allBlocks = result.Blocks.Count > 0 ? result.Blocks :
result.BankBlocks.Values.SelectMany(b => b).ToList();

			// For single-bank systems, determine which bank to pass for label formatting
			var bank = -1; // Default for global labels
			if (result.BankBlocks.Count > 0) {
				// If there are bank-specific blocks, use the first bank
				var firstBank = result.BankBlocks.Keys.FirstOrDefault();
				if (firstBank >= 0) bank = firstBank;
			}

			foreach (var block in allBlocks.OrderBy(b => b.StartAddress)) {
				FormatBlock(sb, block, result, bank);
			}
		}

		return sb.ToString();
	}

	private static void FormatBlock(System.Text.StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1) {
sb.AppendLine($"; --- Block at ${block.StartAddress:x4}-${block.EndAddress:x4} ---");

if (block.Type is MemoryRegion.Data or MemoryRegion.Graphics or MemoryRegion.Audio) {
	FormatDataBlock(sb, block, result, bank);
} else {
	foreach (var line in block.Lines) {
		FormatLine(sb, line, result, bank);
	}
}
sb.AppendLine();
}

/// <summary>
/// Format a data block using .db/.dw/.dl directives based on DataDefinition.
/// </summary>
private static void FormatDataBlock(System.Text.StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1) {
	foreach (var line in block.Lines) {
		// Check if there's a data definition at this address
		if (result.DataRegions.TryGetValue(line.Address, out var dataDef)) {
			// Label on its own line
			if (!string.IsNullOrEmpty(line.Label)) {
				sb.AppendLine($"{line.Label}:");
			} else if (!string.IsNullOrEmpty(dataDef.Name)) {
				sb.AppendLine($"{dataDef.Name}:");
			}

			var directive = dataDef.Type switch {
				"word" => ".dw",
				"long" => ".dl",
				"text" => ".dt",
				_ => ".db",
			};
			var elementSize = dataDef.Type switch {
				"word" => 2,
				"long" => 3,
				_ => 1,
			};

			if (dataDef.Type == "text" && line.Bytes.All(b => b >= 0x20 && b < 0x7f)) {
				// Printable ASCII text
				var text = System.Text.Encoding.ASCII.GetString(line.Bytes);
				sb.AppendLine($"\t{directive} \"{text}\"\t\t; {line.Address:x4}");
			} else if (elementSize > 1 && line.Bytes.Length >= elementSize) {
				// Multi-byte data (words, longs)
				var values = new List<string>();
				for (int i = 0; i + elementSize <= line.Bytes.Length; i += elementSize) {
					var val = elementSize switch {
						2 => $"${(line.Bytes[i] | (line.Bytes[i + 1] << 8)):x4}",
						3 => $"${(line.Bytes[i] | (line.Bytes[i + 1] << 8) | (line.Bytes[i + 2] << 16)):x6}",
						_ => $"${line.Bytes[i]:x2}",
					};
					values.Add(val);
				}
				sb.AppendLine($"\t{directive} {string.Join(", ", values)}\t; {line.Address:x4}");
			} else {
				// Fall back to byte-by-byte .db
				var bytes = string.Join(", ", line.Bytes.Select(b => $"${b:x2}"));
				sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}");
			}
		} else {
			// No data definition — emit raw .db
			if (!string.IsNullOrEmpty(line.Label)) {
				sb.AppendLine($"{line.Label}:");
			}
			var bytes = string.Join(", ", line.Bytes.Select(b => $"${b:x2}"));
			var comment = !string.IsNullOrEmpty(line.Comment) ? $" {line.Comment}" : "";
			sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}{comment}");
		}
	}
}

private static void FormatLine(System.Text.StringBuilder sb, DisassembledLine line, DisassemblyResult result, int bank = -1) {
	// Add cross-reference comment before label if there are incoming references
	var refs = result.GetReferencesTo(line.Address);
	if (refs.Count > 0 && !string.IsNullOrEmpty(line.Label)) {
		sb.AppendLine($"; Referenced by: {FormatCrossRefs(refs)}");
	}

	// Add block comments above the instruction line
	if (!string.IsNullOrEmpty(line.Comment) &&
		result.TypedComments.TryGetValue(line.Address, out var typedComment) &&
		typedComment.Type == Pansy.Core.CommentType.Block) {
		foreach (var commentLine in line.Comment.Split('\n')) {
			sb.AppendLine($"; {commentLine.TrimEnd()}");
		}
	}

	// Label on its own line
	if (!string.IsNullOrEmpty(line.Label)) {
		sb.AppendLine($"{line.Label}:");
	}

	// Build the instruction line
	var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));

	// Check if this operand references a known label (with bank awareness)
	var formatted = FormatWithLabels(line.Content, result, bank);

	var instruction = $"\t{formatted,-24}";
	var bytesComment = $"; {line.Address:x4}: {bytes,-12}";

	// Format inline/todo comments (block comments already placed above)
	var inlineComment = "";
	if (!string.IsNullOrEmpty(line.Comment)) {
		if (result.TypedComments.TryGetValue(line.Address, out var tc)) {
			inlineComment = tc.Type switch {
				Pansy.Core.CommentType.Block => "", // Already rendered above
				Pansy.Core.CommentType.Todo => $" TODO: {line.Comment}",
				_ => $" {line.Comment}",
			};
		} else {
			inlineComment = $" {line.Comment}";
		}
	}

	if (!string.IsNullOrEmpty(inlineComment)) {
		sb.AppendLine($"{instruction}{bytesComment}{inlineComment}");
	} else {
		sb.AppendLine($"{instruction}{bytesComment}");
	}
}

/// <summary>
/// Format cross-references for display in comments
/// </summary>
private static string FormatCrossRefs(IReadOnlyList<CrossRef> refs) {
	var grouped = refs.GroupBy(r => r.Type);
	var parts = new List<string>();

	foreach (var group in grouped.OrderBy(g => g.Key)) {
		var addrs = group.Select(r => $"${r.FromAddress:x4}").ToList();
		var prefix = group.Key switch {
			CrossRefType.Call => "Called from",
			CrossRefType.Jump => "Jump from",
			CrossRefType.Branch => "Branch from",
			CrossRefType.DataRef => "Data ref from",
			CrossRefType.Pointer => "Pointer from",
			_ => "Ref from"
		};

		// Limit to first 5 refs to avoid overly long comments
		if (addrs.Count > 5) {
			parts.Add($"{prefix} {string.Join(", ", addrs.Take(5))} (+{addrs.Count - 5} more)");
		} else {
			parts.Add($"{prefix} {string.Join(", ", addrs)}");
		}
	}

	return string.Join("; ", parts);
}

private static string FormatWithLabels(string instruction, DisassemblyResult result, int bank = -1) {
	// Try to replace addresses with labels
	// First try bank-specific labels, then fall back to global labels

	// Try bank-specific labels first if we have a bank context
	if (bank >= 0 && result.BankLabels.Count > 0) {
		foreach (var kvp in result.BankLabels) {
			if (kvp.Key.Bank != bank) continue;
			var address = kvp.Key.Address;
			var label = kvp.Value;

			// Skip immediate mode for small values (< 0x100) - keep as constants
			if (address < 0x100 && instruction.Contains($"#${address:x2}", StringComparison.OrdinalIgnoreCase))
				continue;

			// Match $xxxx (4-digit hex) with word boundary
			var pattern4 = $@"\${address:x4}(?![0-9a-f])";
			if (System.Text.RegularExpressions.Regex.IsMatch(instruction, pattern4, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
				instruction = System.Text.RegularExpressions.Regex.Replace(instruction, pattern4, label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				continue;
			}

			// For addresses < 0x100, also match short form like $xx (but not immediate mode)
			if (address < 0x100) {
				var pattern2 = $@"(?<!#)\${address:x2}(?![0-9a-f])";
				if (System.Text.RegularExpressions.Regex.IsMatch(instruction, pattern2, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
					instruction = System.Text.RegularExpressions.Regex.Replace(instruction, pattern2, label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				}
			}
		}
	}

	// Fall back to global labels
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
