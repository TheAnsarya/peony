namespace Peony.Platform.SNES;

using System.Text;
using System.Text.RegularExpressions;

using Peony.Core;

/// <summary>
/// SNES-specific output generator for .pasm files.
/// Generates Poppy-compatible assembly with SNES directives:
/// .lorom/.hirom, .a8/.a16/.i8/.i16, bank boundaries, header reproduction.
/// </summary>
public sealed class SnesOutputGenerator : IOutputGenerator {
	public string Name => "SNES Poppy";
	public string Extension => ".pasm";

	public void Generate(DisassemblyResult result, string outputPath) {
		var content = Format(result);
		File.WriteAllText(outputPath, content, new UTF8Encoding(true));
	}

	public string Format(DisassemblyResult result) {
		var estimatedSize = result.Blocks.Sum(b => b.Lines.Count) * 64 + 2048;
		var sb = new StringBuilder(estimatedSize);

		// Header
		sb.AppendLine("; Disassembled by Peony — SNES Platform");
		sb.AppendLine($"; Platform: {result.RomInfo.Platform}");
		if (!string.IsNullOrEmpty(result.RomInfo.Mapper)) {
			sb.AppendLine($"; Mapper: {result.RomInfo.Mapper}");
		}
		foreach (var kvp in result.RomInfo.Metadata) {
			sb.AppendLine($"; {kvp.Key}: {kvp.Value}");
		}
		sb.AppendLine();

		// SNES-specific directives
		sb.AppendLine($"\t.system snes");
		var mapMode = GetMapMode(result);
		sb.AppendLine($"\t.{mapMode}");
		sb.AppendLine();

		// Initial processor state — SNES starts in emulation mode (8-bit A and XY)
		sb.AppendLine("\t.a8");
		sb.AppendLine("\t.i8");
		sb.AppendLine();

		// Output bank by bank
		var hasMultipleBanks = result.BankBlocks.Count > 1 &&
			result.BankBlocks.Values.Any(b => b.Count > 0);

		if (hasMultipleBanks) {
			foreach (var bank in result.BankBlocks.Keys.OrderBy(b => b)) {
				var blocks = result.BankBlocks[bank];
				if (blocks.Count == 0) continue;

				FormatBank(sb, bank, blocks, result, mapMode);
			}
		} else {
			IEnumerable<DisassembledBlock> allBlocks = result.Blocks.Count > 0 ? result.Blocks :
				result.BankBlocks.Values.SelectMany(b => b);

			var bank = -1;
			if (result.BankBlocks.Count > 0) {
				var firstBank = result.BankBlocks.Keys.FirstOrDefault();
				if (firstBank >= 0) bank = firstBank;
			}

			foreach (var block in allBlocks.OrderBy(b => b.StartAddress)) {
				FormatBlock(sb, block, result, bank);
			}
		}

		return sb.ToString();
	}

	private static void FormatBank(StringBuilder sb, int bank, List<DisassembledBlock> blocks, DisassemblyResult result, string mapMode) {
		sb.AppendLine();
		sb.AppendLine($"; ===========================================================================");
		sb.AppendLine($"; BANK ${bank:x2}");
		sb.AppendLine($"; ===========================================================================");
		sb.AppendLine();

		sb.AppendLine($"\t.bank {bank}");

		// LoROM banks start at $8000, HiROM at $0000
		var orgAddress = mapMode is "lorom" or "exlorom" ? "$8000" : "$0000";
		sb.AppendLine($"\t.org {orgAddress}");
		sb.AppendLine();

		// Track processor state within the bank
		var accIs8 = true;
		var idxIs8 = true;

		foreach (var block in blocks.OrderBy(b => b.StartAddress)) {
			FormatBlockWithState(sb, block, result, bank, ref accIs8, ref idxIs8);
		}
	}

	private static void FormatBlockWithState(StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank, ref bool accIs8, ref bool idxIs8) {
		sb.AppendLine($"; --- Block at ${block.StartAddress:x4}-${block.EndAddress:x4} ---");

		if (block.Type is MemoryRegion.Data or MemoryRegion.Graphics or MemoryRegion.Audio) {
			FormatDataBlock(sb, block, result, bank);
		} else {
			foreach (var line in block.Lines) {
				// Detect SEP/REP instructions that change processor state
				EmitStateDirectives(sb, line, ref accIs8, ref idxIs8);
				FormatLine(sb, line, result, bank);
			}
		}
		sb.AppendLine();
	}

	/// <summary>
	/// Track SEP/REP instructions and emit .a8/.a16/.i8/.i16 directives when state changes
	/// </summary>
	private static void EmitStateDirectives(StringBuilder sb, DisassembledLine line, ref bool accIs8, ref bool idxIs8) {
		if (line.Bytes.Length < 2) return;

		var opcode = line.Bytes[0];
		var operand = line.Bytes[1];

		if (opcode == 0xc2) { // REP — clear bits (make 16-bit)
			if ((operand & 0x20) != 0 && accIs8) {
				accIs8 = false;
				sb.AppendLine("\t.a16");
			}
			if ((operand & 0x10) != 0 && idxIs8) {
				idxIs8 = false;
				sb.AppendLine("\t.i16");
			}
		} else if (opcode == 0xe2) { // SEP — set bits (make 8-bit)
			if ((operand & 0x20) != 0 && !accIs8) {
				accIs8 = true;
				sb.AppendLine("\t.a8");
			}
			if ((operand & 0x10) != 0 && !idxIs8) {
				idxIs8 = true;
				sb.AppendLine("\t.i8");
			}
		}
	}

	private static void FormatBlock(StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1) {
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

	private static void FormatDataBlock(StringBuilder sb, DisassembledBlock block, DisassemblyResult result, int bank = -1) {
		foreach (var line in block.Lines) {
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
					var text = Encoding.ASCII.GetString(line.Bytes);
					sb.AppendLine($"\t{directive} \"{text}\"\t\t; {line.Address:x4}");
				} else if (elementSize > 1 && line.Bytes.Length >= elementSize) {
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
					var bytes = FormatBytesComma(line.Bytes);
					sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}");
				}
			} else {
				if (!string.IsNullOrEmpty(line.Label)) {
					sb.AppendLine($"{line.Label}:");
				}
				var bytes = FormatBytesComma(line.Bytes);
				var comment = !string.IsNullOrEmpty(line.Comment) ? $" {line.Comment}" : "";
				sb.AppendLine($"\t.db {bytes}\t\t; {line.Address:x4}{comment}");
			}
		}
	}

	private static void FormatLine(StringBuilder sb, DisassembledLine line, DisassemblyResult result, int bank = -1) {
		// Cross-reference comment before label
		var refs = result.GetReferencesTo(line.Address);
		if (refs.Count > 0 && !string.IsNullOrEmpty(line.Label)) {
			sb.AppendLine($"; Referenced by: {FormatCrossRefs(refs)}");
		}

		// Block comments above the instruction
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

		// Instruction
		var bytes = FormatBytesSpaced(line.Bytes);
		var formatted = FormatWithLabels(line.Content, result, bank);
		var instruction = $"\t{formatted,-24}";
		var bytesComment = $"; {line.Address:x4}: {bytes,-12}";

		// Inline/todo comments
		var inlineComment = "";
		if (!string.IsNullOrEmpty(line.Comment)) {
			if (result.TypedComments.TryGetValue(line.Address, out var tc)) {
				inlineComment = tc.Type switch {
					Pansy.Core.CommentType.Block => "",
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

			if (addrs.Count > 5) {
				parts.Add($"{prefix} {string.Join(", ", addrs.Take(5))} (+{addrs.Count - 5} more)");
			} else {
				parts.Add($"{prefix} {string.Join(", ", addrs)}");
			}
		}

		return string.Join("; ", parts);
	}

	private static string FormatWithLabels(string instruction, DisassemblyResult result, int bank = -1) {
		// Try bank-specific labels first
		if (bank >= 0 && result.BankLabels.Count > 0) {
			foreach (var kvp in result.BankLabels) {
				if (kvp.Key.Bank != bank) continue;
				var address = kvp.Key.Address;
				var label = kvp.Value;

				if (address < 0x100 && instruction.Contains($"#${address:x2}", StringComparison.OrdinalIgnoreCase))
					continue;

				var regex4 = GetOrCreateHexPattern4(address);
				if (regex4.IsMatch(instruction)) {
					instruction = regex4.Replace(instruction, label);
					continue;
				}

				if (address < 0x100) {
					var regex2 = GetOrCreateHexPattern2(address);
					if (regex2.IsMatch(instruction)) {
						instruction = regex2.Replace(instruction, label);
					}
				}
			}
		}

		// Fall back to global labels
		foreach (var kvp in result.Labels) {
			if (kvp.Key < 0x100 && instruction.Contains($"#${kvp.Key:x2}", StringComparison.OrdinalIgnoreCase))
				continue;

			var regex4 = GetOrCreateHexPattern4(kvp.Key);
			if (regex4.IsMatch(instruction)) {
				instruction = regex4.Replace(instruction, kvp.Value);
				continue;
			}

			if (kvp.Key < 0x100) {
				var regex2 = GetOrCreateHexPattern2(kvp.Key);
				if (regex2.IsMatch(instruction)) {
					instruction = regex2.Replace(instruction, kvp.Value);
				}
			}
		}
		return instruction;
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

	private static string GetMapMode(DisassemblyResult result) {
		if (result.RomInfo.Metadata.TryGetValue("MapMode", out var mode)) {
			return mode.ToLowerInvariant() switch {
				"lorom" => "lorom",
				"hirom" => "hirom",
				"exlorom" => "exlorom",
				"exhirom" => "exhirom",
				_ => "lorom"
			};
		}
		return "lorom";
	}

	private static string FormatBytesSpaced(byte[] bytes) {
		if (bytes.Length == 0) return "";
		return string.Create(bytes.Length * 3 - 1, bytes, static (span, b) => {
			for (int i = 0; i < b.Length; i++) {
				if (i > 0) span[i * 3 - 1] = ' ';
				b[i].TryFormat(span[(i * 3)..], out _, "x2");
			}
		});
	}

	private static string FormatBytesComma(byte[] bytes) {
		if (bytes.Length == 0) return "";
		return string.Create(bytes.Length * 5 - 2, bytes, static (span, b) => {
			int pos = 0;
			for (int i = 0; i < b.Length; i++) {
				if (i > 0) {
					span[pos++] = ',';
					span[pos++] = ' ';
				}
				span[pos++] = '$';
				b[i].TryFormat(span[pos..], out _, "x2", System.Globalization.CultureInfo.InvariantCulture);
				pos += 2;
			}
		});
	}
}
