using System.Text;
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
			writer.WriteLine($".system:{info.Platform}");
			if (info.Mapper != null)
				writer.WriteLine($"; Mapper: {info.Mapper}");
		} else {
			writer.WriteLine();
		}
		if (result.Labels.Count > 0)
			writer.WriteLine($"; Labels: {result.Labels.Count}");
		writer.WriteLine();

		foreach (var block in result.Blocks.OrderBy(b => b.StartAddress)) {
			writer.WriteLine($"; === Block ${block.StartAddress:x4}-${block.EndAddress:x4} ({block.Type}) ===");
			foreach (var line in block.Lines) {
				var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));
				var comment = line.Comment != null ? $" {line.Comment}" : "";

				if (line.Label != null)
					writer.WriteLine($"{line.Label}:");

				var content = FormatWithLabels(line.Content, result);
				writer.WriteLine($"\t{content,-24} ; ${line.Address:x4}: {bytes}{comment}".TrimEnd());
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
