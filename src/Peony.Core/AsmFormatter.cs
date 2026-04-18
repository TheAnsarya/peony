using System.Text.RegularExpressions;

namespace Peony.Core;

/// <summary>
/// Generic ASM output formatter for disassembly results.
/// Produces human-readable assembly source with label substitution.
/// </summary>
public sealed class AsmFormatter : IOutputGenerator {
	/// <summary>Singleton instance</summary>
	public static readonly AsmFormatter Instance = new();

	/// <inheritdoc />
	public string Name => "ASM";

	/// <inheritdoc />
	public string Extension => ".asm";

	/// <inheritdoc />
	public void Generate(DisassemblyResult result, string outputPath) {
		using var writer = new StreamWriter(outputPath);
		WriteOutput(writer, result);
	}

	/// <inheritdoc />
	public string Format(DisassemblyResult result) {
		using var sw = new StringWriter();
		WriteOutput(sw, result);
		return sw.ToString();
	}

	/// <summary>
	/// Writes disassembly output with metadata header, block markers, and label substitution.
	/// </summary>
	public void WriteOutput(TextWriter writer, DisassemblyResult result, string? romName = null) {
		writer.WriteLine($"; 🌺 Peony Disassembly");
		if (romName != null)
			writer.WriteLine($"; ROM: {romName}");
		if (result.RomInfo is { } info) {
			writer.WriteLine($"; Platform: {info.Platform}");
			writer.WriteLine($"; Size: {info.Size} bytes");
			writer.WriteLine();
			// Emit Poppy-compatible platform directive (e.g., .snes, .nes, .gb)
			var poppyPlatform = PlatformResolver.Resolve(info.Platform)?.PoppyPlatformId
				?? info.Platform.ToLowerInvariant();
			writer.WriteLine($".{poppyPlatform}");
			if (info.Mapper != null)
				writer.WriteLine($"; Mapper: {info.Mapper}");
		} else {
			writer.WriteLine();
		}
		if (result.Labels.Count > 0)
			writer.WriteLine($"; Labels: {result.Labels.Count}");
		writer.WriteLine();

		// Deduplicate overlapping blocks: when multiple blocks cover the same address range,
		// keep only the first one (sorted by start address, then longest range first)
		var sortedBlocks = result.Blocks
			.OrderBy(b => b.StartAddress)
			.ThenByDescending(b => b.EndAddress)
			.ToList();
		var emittedUpTo = 0u;
		var keptBlocks = new List<DisassembledBlock>();
		foreach (var block in sortedBlocks) {
			if (block.StartAddress < emittedUpTo) {
				continue; // Skip overlapping block
			}
			emittedUpTo = block.EndAddress + 1;
			keptBlocks.Add(block);
		}

		// Collect all labels that appear as definitions on kept (non-overlapping) blocks
		var definedLabels = new HashSet<string>(StringComparer.Ordinal);
		foreach (var block in keptBlocks) {
			foreach (var line in block.Lines) {
				if (!string.IsNullOrEmpty(line.Label)) {
					definedLabels.Add(line.Label);
				}
			}
		}

		// Emit .equ directives for labels that have no definition (e.g., RAM addresses)
		var undefinedLabels = new List<(uint Address, string Name)>();
		foreach (var kvp in result.Labels) {
			if (!definedLabels.Contains(kvp.Value)) {
				undefinedLabels.Add((kvp.Key, kvp.Value));
			}
		}
		foreach (var kvp in result.BankLabels) {
			if (!definedLabels.Contains(kvp.Value)) {
				undefinedLabels.Add((kvp.Key.Address, kvp.Value));
			}
		}
		if (undefinedLabels.Count > 0) {
			writer.WriteLine("; --- External/RAM symbol definitions ---");
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var (address, name) in undefinedLabels.DistinctBy(x => x.Name).OrderBy(x => x.Address)) {
				if (seen.Add(name)) {
					writer.WriteLine($"{name} = ${address:x4}");
				}
			}
			writer.WriteLine();
		}

		foreach (var block in keptBlocks) {
			// Emit .org to set assembler PC for correct branch offset calculation
			writer.WriteLine($".org ${block.StartAddress:x4}");
			writer.WriteLine($"; === Block ${block.StartAddress:x4}-${block.EndAddress:x4} ({block.Type}) ===");
			foreach (var line in block.Lines) {
				var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));

				if (line.Label != null)
					writer.WriteLine($"{line.Label}:");

				// Handle multi-line comments: first line inline, rest as ; prefixed lines
				var commentText = line.Comment ?? "";
				var commentLines = commentText.Split('\n');
				var firstComment = commentLines.Length > 0 && !string.IsNullOrEmpty(commentLines[0])
					? $" {commentLines[0].TrimEnd()}"
					: "";

				var content = FormatWithLabels(line.Content, result);
				writer.WriteLine($"\t{content,-24} ; ${line.Address:x4}: {bytes}{firstComment}".TrimEnd());

				// Emit remaining comment lines as separate ; prefixed lines
				for (int i = 1; i < commentLines.Length; i++) {
					var cl = commentLines[i].TrimEnd();
					if (!string.IsNullOrEmpty(cl))
						writer.WriteLine($"; {cl}");
				}
			}
			writer.WriteLine();
		}
	}

	/// <summary>
	/// Substitutes known label names for hex address operands in an instruction string.
	/// </summary>
	public static string FormatWithLabels(string instruction, DisassemblyResult result) {
		foreach (var kvp in result.Labels) {
			// Skip immediate mode for small values (< 0x100) — keep as constants
			if (kvp.Key < 0x100 && instruction.Contains($"#${kvp.Key:x2}", StringComparison.OrdinalIgnoreCase))
				continue;

			// Match $xxxx (4-digit hex) with word boundary
			var pattern4 = $@"\${kvp.Key:x4}(?![0-9a-f])";
			if (Regex.IsMatch(instruction, pattern4, RegexOptions.IgnoreCase)) {
				instruction = Regex.Replace(instruction, pattern4, kvp.Value, RegexOptions.IgnoreCase);
				continue;
			}

			// For addresses < 0x100, match short form $xx (but not immediate mode)
			if (kvp.Key < 0x100) {
				var pattern2 = $@"(?<!#)\${kvp.Key:x2}(?![0-9a-f])";
				if (Regex.IsMatch(instruction, pattern2, RegexOptions.IgnoreCase)) {
					instruction = Regex.Replace(instruction, pattern2, kvp.Value, RegexOptions.IgnoreCase);
				}
			}
		}
		return instruction;
	}
}
