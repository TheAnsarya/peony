namespace Peony.Core;

using System.Text.RegularExpressions;

/// <summary>
/// Formats disassembly output in Poppy assembler format with bank annotations.
/// Implements both legacy IOutputFormatter and new IOutputGenerator interfaces.
/// Used as the default output generator for platforms without a custom generator.
/// </summary>
public sealed class PoppyFormatter : IOutputFormatter, IOutputGenerator {
	/// <summary>Shared singleton instance for use as default output generator</summary>
	public static readonly PoppyFormatter Instance = new();

public string Name => "Poppy";
public string Extension => ".pasm";

public void Generate(DisassemblyResult result, string outputPath) {
var content = Format(result);
File.WriteAllText(outputPath, content);
}

public string Format(DisassemblyResult result) {
var estimatedSize = result.Blocks.Sum(b => b.Lines.Count) * 64 + 1024;
var sb = new System.Text.StringBuilder(estimatedSize);

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
// Emit Poppy-compatible platform directive (e.g., .snes, .nes, .gb)
var poppyPlatform = PlatformResolver.Resolve(result.RomInfo.Platform)?.PoppyPlatformId
	?? result.RomInfo.Platform.ToLowerInvariant();
sb.AppendLine($".{poppyPlatform}");
sb.AppendLine();

// Check if we have multiple banks
var hasMultipleBanks = result.BankBlocks.Count > 1 &&
   result.BankBlocks.Values.Any(b => b.Count > 0);

// Build set of label names that collide across banks (need bank qualification)
HashSet<string>? collidingLabels = null;
// Build set of addresses that actually have labels defined in the output
HashSet<(int Bank, uint Address)>? definedLabelAddresses = null;
Dictionary<(int Bank, uint Address), string>? definedLabelNames = null;
if (hasMultipleBanks) {
	var labelNameBanks = new Dictionary<string, HashSet<int>>();
	foreach (var kvp in result.BankLabels) {
		if (!labelNameBanks.TryGetValue(kvp.Value, out var banks)) {
			banks = [];
			labelNameBanks[kvp.Value] = banks;
		}
		banks.Add(kvp.Key.Bank);
	}
	collidingLabels = new HashSet<string>(
		labelNameBanks.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Key)
	);

	// Collect all addresses that have label definitions in the output
	// Maps (bank, address) → qualified label name as actually emitted
	definedLabelAddresses = [];
	definedLabelNames = new();
	foreach (var (bank, blocks) in result.BankBlocks) {
		foreach (var block in blocks) {
			foreach (var line in block.Lines) {
				if (!string.IsNullOrEmpty(line.Label)) {
					definedLabelAddresses.Add((bank, line.Address));
					definedLabelNames[(bank, line.Address)] = QualifyLabel(line.Label, bank, collidingLabels);
				}
			}
		}
	}
}

if (hasMultipleBanks) {
// Output bank by bank, including data-only banks for byte-identical roundtrip
var totalBanks = result.RomData is not null ? result.RomData.Length / 0x8000 : result.BankBlocks.Keys.Max() + 1;
for (int bank = 0; bank < totalBanks; bank++) {
result.BankBlocks.TryGetValue(bank, out var blocks);
var hasBlocks = blocks is not null && blocks.Count > 0;

sb.AppendLine();
sb.AppendLine($"; ===========================================================================");
sb.AppendLine($"; BANK {bank}");
sb.AppendLine($"; ===========================================================================");
sb.AppendLine();

// Add bank directive
sb.AppendLine($"\t.bank {bank}");
sb.AppendLine();

// Emit all bytes in the bank: code blocks as instructions, gaps as raw .db
var sortedBlocks = hasBlocks ? blocks!.OrderBy(b => b.StartAddress).ToList() : [];
uint bankStart = 0x8000;
uint bankEnd = 0xffff;
uint currentAddr = bankStart;

for (int bi = 0; bi < sortedBlocks.Count; bi++) {
	var block = sortedBlocks[bi];
	var blockLocalStart = (uint)(block.StartAddress & 0xffff);
	var blockLocalEnd = (uint)(block.EndAddress & 0xffff);

	// Verify block bytes match ROM at this bank's offset — skip misplaced blocks
	var blockMatchesRom = true;
	if (result.RomData is not null && result.PlatformAnalyzer is not null) {
		var romOff = result.PlatformAnalyzer.AddressToOffset(blockLocalStart, result.RomData.Length, bank);
		if (romOff >= 0) {
			foreach (var line in block.Lines) {
				var lineLocal = (uint)(line.Address & 0xffff);
				var lineOff = result.PlatformAnalyzer.AddressToOffset(lineLocal, result.RomData.Length, bank);
				if (lineOff >= 0 && lineOff + line.Bytes.Length <= result.RomData.Length) {
					for (int j = 0; j < line.Bytes.Length; j++) {
						if (result.RomData[lineOff + j] != line.Bytes[j]) {
							blockMatchesRom = false;
							break;
						}
					}
				}
				if (!blockMatchesRom) break;
			}
		}
	}

	if (blockMatchesRom) {
		// Fill gap before this block with raw ROM data
		if (blockLocalStart > currentAddr && result.RomData is not null && result.PlatformAnalyzer is not null) {
			EmitRawGap(sb, currentAddr, blockLocalStart, bank, result);
		}

		FormatBlock(sb, block, result, bank, collidingLabels, definedLabelAddresses, definedLabelNames);
		currentAddr = blockLocalEnd;
	}
	// If block doesn't match ROM, skip it — the trailing gap fill will emit correct ROM data
}

// Fill trailing gap after last block to end of bank
if (currentAddr <= bankEnd && result.RomData is not null && result.PlatformAnalyzer is not null) {
	EmitRawGap(sb, currentAddr, bankEnd + 1, bank, result);
}
}
} else {
// Single bank output (original behavior)
			IEnumerable<DisassembledBlock> allBlocks = result.Blocks.Count > 0 ? result.Blocks :
				result.BankBlocks.Values.SelectMany(b => b);

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

	/// <summary>
	/// Emit raw ROM bytes as .db directives for gaps between code blocks.
	/// </summary>
	private static void EmitRawGap(System.Text.StringBuilder sb, uint startAddr, uint endAddr, int bank, DisassemblyResult result) {
		if (startAddr >= endAddr || result.RomData is null || result.PlatformAnalyzer is null) return;

		var romOffset = result.PlatformAnalyzer.AddressToOffset(startAddr, result.RomData.Length, bank);
		if (romOffset < 0) return;

		var length = (int)(endAddr - startAddr);
		if (romOffset + length > result.RomData.Length) {
			length = result.RomData.Length - romOffset;
		}
		if (length <= 0) return;

		var localStart = startAddr & 0xffff;
		var localEnd = endAddr & 0xffff;
		if (localEnd == 0) localEnd = 0x10000; // handle wrap at bank boundary
		sb.AppendLine($"\t.org ${localStart:x4}");
		sb.AppendLine($"; --- Data gap ${localStart:x4}-${localEnd:x4} ({length} bytes) ---");

		// Emit in rows of 16 bytes
		for (int i = 0; i < length; i += 16) {
			var count = Math.Min(16, length - i);
			var values = new string[count];
			for (int j = 0; j < count; j++) {
				values[j] = $"${result.RomData[romOffset + i + j]:x2}";
			}
			sb.AppendLine($"\t.db {string.Join(", ", values)}");
		}
		sb.AppendLine();
	}

	private static void FormatBlock(System.Text.StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1, HashSet<string>? collidingLabels = null, HashSet<(int Bank, uint Address)>? definedLabelAddresses = null, Dictionary<(int Bank, uint Address), string>? definedLabelNames = null) {
// Emit .org for each block so Poppy tracks the correct address (blocks may have gaps between them)
if (bank >= 0) {
	var localAddr = block.StartAddress & 0xffff;
	sb.AppendLine($"\t.org ${localAddr:x4}");
}
sb.AppendLine($"; --- Block at ${block.StartAddress:x4}-${block.EndAddress:x4} ---");

if (block.Type is MemoryRegion.Data or MemoryRegion.Graphics or MemoryRegion.Audio) {
	FormatDataBlock(sb, block, result, bank, collidingLabels);
} else {
	foreach (var line in block.Lines) {
		FormatLine(sb, line, result, bank, collidingLabels, definedLabelAddresses, definedLabelNames);
	}
}
sb.AppendLine();
}

/// <summary>
/// Format a data block using .db/.dw/.dl directives based on DataDefinition.
/// </summary>
private static void FormatDataBlock(System.Text.StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1, HashSet<string>? collidingLabels = null) {
	foreach (var line in block.Lines) {
		// Check if there's a data definition at this address
		if (result.DataRegions.TryGetValue(line.Address, out var dataDef)) {
			// Label on its own line
			if (!string.IsNullOrEmpty(line.Label)) {
				sb.AppendLine($"{QualifyLabel(line.Label, bank, collidingLabels)}:");
			} else if (!string.IsNullOrEmpty(dataDef.Name)) {
				sb.AppendLine($"{QualifyLabel(dataDef.Name, bank, collidingLabels)}:");
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
				var bytes = FormatBytesComma(line.Bytes);
				sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}");
			}
		} else {
			// No data definition — emit raw .db
			if (!string.IsNullOrEmpty(line.Label)) {
				sb.AppendLine($"{QualifyLabel(line.Label, bank, collidingLabels)}:");
			}
			var bytes = FormatBytesComma(line.Bytes);
			var comment = !string.IsNullOrEmpty(line.Comment) ? $" {line.Comment}" : "";
			sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}{comment}");
		}
	}
}

/// <summary>
/// Check if an opcode is a relative branch instruction (8-bit or 16-bit offset).
/// </summary>
private static bool IsBranchOpcode(byte opcode) => opcode switch {
	0x10 => true, // bpl
	0x30 => true, // bmi
	0x50 => true, // bvc
	0x70 => true, // bvs
	0x80 => true, // bra
	0x82 => true, // brl
	0x90 => true, // bcc
	0xb0 => true, // bcs
	0xd0 => true, // bne
	0xf0 => true, // beq
	_ => false,
};

/// <summary>
/// 65816 opcodes that use immediate addressing and are affected by M/X flags.
/// Maps opcode byte to expected total instruction size for 8-bit mode.
/// </summary>
private static readonly HashSet<byte> _accImmediateOpcodes = [
	0x09, // ora #imm
	0x29, // and #imm
	0x49, // eor #imm
	0x69, // adc #imm
	0x89, // bit #imm
	0xa9, // lda #imm
	0xc9, // cmp #imm
	0xe9, // sbc #imm
];

private static readonly HashSet<byte> _idxImmediateOpcodes = [
	0xa0, // ldy #imm
	0xa2, // ldx #imm
	0xc0, // cpy #imm
	0xe0, // cpx #imm
];

/// <summary>
/// Add .b or .w size suffix to immediate-mode instructions where the operand size
/// depends on 65816 M/X processor flags, making encoding unambiguous.
/// </summary>
private static string AddSizeSuffix(string formatted, byte[] bytes) {
	if (bytes.Length < 2) return formatted;

	var opcode = bytes[0];
	bool needsSuffix;
	int expectedSize8Bit;

	if (_accImmediateOpcodes.Contains(opcode)) {
		needsSuffix = true;
		expectedSize8Bit = 2; // opcode + 1 byte
	} else if (_idxImmediateOpcodes.Contains(opcode)) {
		needsSuffix = true;
		expectedSize8Bit = 2; // opcode + 1 byte
	} else {
		return formatted;
	}

	if (!needsSuffix) return formatted;

	// Determine suffix based on actual byte count
	var suffix = bytes.Length > expectedSize8Bit ? ".w" : ".b";

	// Insert suffix after the mnemonic (before the space)
	var spaceIdx = formatted.IndexOf(' ');
	if (spaceIdx > 0) {
		return formatted[..spaceIdx] + suffix + formatted[spaceIdx..];
	}

	return formatted;
}

private static void FormatLine(System.Text.StringBuilder sb, DisassembledLine line, DisassemblyResult result, int bank = -1, HashSet<string>? collidingLabels = null, HashSet<(int Bank, uint Address)>? definedLabelAddresses = null, Dictionary<(int Bank, uint Address), string>? definedLabelNames = null) {
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
		sb.AppendLine($"{QualifyLabel(line.Label, bank, collidingLabels)}:");
	}

	// Build the instruction line
	var bytes = FormatBytesSpaced(line.Bytes);

	// Emit all instructions as raw .db bytes to guarantee byte-identical roundtrip
	// The disassembled instruction is preserved in the comment for readability
	{
		var dbValues = string.Join(", ", line.Bytes.Select(b => $"${b:x2}"));
		var rawInstruction = $"\t.db {dbValues}";
		var padded = $"{rawInstruction,-26}";
		var content = FormatWithLabels(line.Content, result, bank, collidingLabels, definedLabelAddresses, definedLabelNames);
		var rawComment = $"; {line.Address:x4}: {bytes,-12} {content}";

		// Include TODO prefix for todo comments, or inline comment (single-line only)
		if (!string.IsNullOrEmpty(line.Comment) && !line.Comment.Contains('\n') &&
			result.TypedComments.TryGetValue(line.Address, out var todoComment) &&
			todoComment.Type == Pansy.Core.CommentType.Todo) {
			rawComment += $" ; TODO: {line.Comment}";
		} else if (!string.IsNullOrEmpty(line.Comment) && !line.Comment.Contains('\n') &&
			(!result.TypedComments.TryGetValue(line.Address, out var tc) || tc.Type != Pansy.Core.CommentType.Block)) {
			rawComment += $" ; {line.Comment}";
		}

		sb.AppendLine($"{padded}{rawComment}");
		return;
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

private static string FormatWithLabels(string instruction, DisassemblyResult result, int bank = -1, HashSet<string>? collidingLabels = null, HashSet<(int Bank, uint Address)>? definedLabelAddresses = null, Dictionary<(int Bank, uint Address), string>? definedLabelNames = null) {
	// Try to replace addresses with labels
	// First try bank-specific labels, then fall back to global labels

	// In multi-bank mode, use definedLabelNames for substitution
	// This ensures we only substitute labels that exist in the output
	if (bank >= 0 && definedLabelNames is not null) {
		foreach (var kvp in definedLabelNames) {
			if (kvp.Key.Bank != bank) continue;
			var address = kvp.Key.Address;
			var label = kvp.Value;

			// Skip immediate mode for small values (< 0x100) - keep as constants
			if (address < 0x100 && instruction.Contains($"#${address:x2}", StringComparison.OrdinalIgnoreCase))
				continue;

			// Match $xxxx (4-digit hex) with word boundary
			var regex4 = GetOrCreateHexPattern4(address);
			if (regex4.IsMatch(instruction)) {
				instruction = regex4.Replace(instruction, label);
				continue;
			}

			// For addresses < 0x100, also match short form like $xx (but not immediate mode)
			if (address < 0x100) {
				var regex2 = GetOrCreateHexPattern2(address);
				if (regex2.IsMatch(instruction)) {
					instruction = regex2.Replace(instruction, label);
				}
			}
		}
		return instruction;
	}

	// Skip global label substitution in multi-bank mode to avoid undefined symbols
	// (global labels include hardware registers and symbols from non-code banks)
	if (collidingLabels is not null) return instruction;

	// Fall back to global labels (single-bank mode only)
	foreach (var kvp in result.Labels) {
		// Skip immediate mode for small values (< 0x100) - keep as constants
		// e.g. lda #$00 should stay as #$00, not #gen_byte_00
		if (kvp.Key < 0x100 && instruction.Contains($"#${kvp.Key:x2}", StringComparison.OrdinalIgnoreCase))
			continue;

		// Match $xxxx (4-digit hex) with word boundary
		var regex4 = GetOrCreateHexPattern4(kvp.Key);
		if (regex4.IsMatch(instruction)) {
			instruction = regex4.Replace(instruction, kvp.Value);
			continue;
		}

		// For addresses < 0x100, also match short form like $xx (but not immediate mode)
		if (kvp.Key < 0x100) {
			// Only match if it's NOT immediate mode (not preceded by #)
			var regex2 = GetOrCreateHexPattern2(kvp.Key);
			if (regex2.IsMatch(instruction)) {
				instruction = regex2.Replace(instruction, kvp.Value);
			}
		}
	}
	return instruction;
}

/// <summary>
/// Returns a bank-qualified label name if the label collides across banks,
/// otherwise returns the label as-is.
/// In multi-bank mode (collisions is not null), always qualifies to avoid
/// collisions between global labels used in multiple banks.
/// </summary>
private static string QualifyLabel(string label, int bank, HashSet<string>? collisions) {
	if (collisions is null || bank < 0) return label;
	return $"b{bank:d2}_{label}";
}

private static readonly Dictionary<uint, Regex> _hexPattern4Cache = [];
private static readonly Dictionary<uint, Regex> _hexPattern2Cache = [];

private static Regex GetOrCreateHexPattern4(uint address) {
	if (!_hexPattern4Cache.TryGetValue(address, out var regex)) {
		regex = new Regex($@"\${address:x4}(?![0-9a-f])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		_hexPattern4Cache[address] = regex;
	}
	return regex;
}

private static Regex GetOrCreateHexPattern2(uint address) {
	if (!_hexPattern2Cache.TryGetValue(address, out var regex)) {
		regex = new Regex($@"(?<!#)\${address:x2}(?![0-9a-f])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		_hexPattern2Cache[address] = regex;
	}
	return regex;
}

/// <summary>
/// Format byte array as space-separated hex (e.g., "a9 00 8d").
/// Avoids LINQ allocation overhead.
/// </summary>
private static string FormatBytesSpaced(byte[] bytes) {
	if (bytes.Length == 0) return "";
	// Each byte = 2 hex chars + 1 space, minus the last space
	return string.Create(bytes.Length * 3 - 1, bytes, static (span, b) => {
		for (int i = 0; i < b.Length; i++) {
			if (i > 0) span[i * 3 - 1] = ' ';
			b[i].TryFormat(span[(i * 3)..], out _, "x2");
		}
	});
}

/// <summary>
/// Format byte array as comma-separated hex with $ prefix (e.g., "$a9, $00, $8d").
/// Avoids LINQ allocation overhead.
/// </summary>
private static string FormatBytesComma(byte[] bytes) {
	if (bytes.Length == 0) return "";
	// Each byte = "$" + 2 hex + ", " (4 chars per byte except last has no ", ")
	return string.Create(bytes.Length * 5 - 2, bytes, static (span, b) => {
		int pos = 0;
		for (int i = 0; i < b.Length; i++) {
			if (i > 0) {
				span[pos++] = ',';
				span[pos++] = ' ';
			}
			span[pos++] = '$';
			b[i].TryFormat(span[pos..], out _, "x2");
			pos += 2;
		}
	});
}
}
