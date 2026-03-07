using System.Text.Json;

namespace Peony.Core;

/// <summary>
/// Per-bank coverage breakdown.
/// </summary>
public sealed record BankCoverage {
	public int Bank { get; init; }
	public int TotalBytes { get; init; }
	public int CodeBytes { get; init; }
	public int DataBytes { get; init; }
	public int GraphicsBytes { get; init; }
	public int UnknownBytes { get; init; }
	public int LabelCount { get; init; }
	public int BlockCount { get; init; }
}

/// <summary>
/// Analyzes disassembly results to produce coverage statistics, per-bank breakdowns,
/// and JSON/text reports.
/// </summary>
public static class CoverageAnalyzer {
	private static readonly JsonSerializerOptions JsonOpts = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// Compute overall coverage statistics for a disassembly result.
	/// </summary>
	public static CoverageReport Analyze(DisassemblyResult result) {
		int codeBytes = 0, dataBytes = 0, graphicsBytes = 0;
		int blockCount = result.Blocks.Count;
		int pointerTableCount = result.DataRegions.Count;

		foreach (var block in result.Blocks) {
			var byteCount = block.Lines.Sum(l => l.Bytes.Length);
			switch (block.Type) {
				case MemoryRegion.Code:
					codeBytes += byteCount;
					break;
				case MemoryRegion.Data:
					dataBytes += byteCount;
					break;
				case MemoryRegion.Graphics:
					graphicsBytes += byteCount;
					break;
				default:
					dataBytes += byteCount;
					break;
			}
		}

		var totalBytes = result.RomInfo?.Size ?? (codeBytes + dataBytes + graphicsBytes);
		var unknownBytes = Math.Max(0, totalBytes - codeBytes - dataBytes - graphicsBytes);

		return new CoverageReport {
			TotalBytes = totalBytes,
			CodeBytes = codeBytes,
			DataBytes = dataBytes,
			GraphicsBytes = graphicsBytes,
			UnknownBytes = unknownBytes,
			LabelCount = result.Labels.Count + result.BankLabels.Count,
			CommentCount = result.Comments.Count,
			CrossRefCount = result.CrossReferences.Values.Sum(refs => refs.Count),
			PointerTableCount = pointerTableCount,
			BlockCount = blockCount,
			EntryPointCount = result.Blocks.Count(b => b.Type == MemoryRegion.Code)
		};
	}

	/// <summary>
	/// Compute per-bank coverage breakdowns.
	/// </summary>
	public static List<BankCoverage> AnalyzeByBank(DisassemblyResult result) {
		var bankGroups = result.Blocks
			.GroupBy(b => b.Bank)
			.OrderBy(g => g.Key);

		var banks = new List<BankCoverage>();

		foreach (var group in bankGroups) {
			int code = 0, data = 0, graphics = 0, total = 0, labels = 0;

			foreach (var block in group) {
				var byteCount = block.Lines.Sum(l => l.Bytes.Length);
				total += byteCount;

				switch (block.Type) {
					case MemoryRegion.Code:
						code += byteCount;
						break;
					case MemoryRegion.Data:
						data += byteCount;
						break;
					case MemoryRegion.Graphics:
						graphics += byteCount;
						break;
					default:
						data += byteCount;
						break;
				}

				labels += block.Lines.Count(l => l.Label is not null);
			}

			banks.Add(new BankCoverage {
				Bank = group.Key,
				TotalBytes = total,
				CodeBytes = code,
				DataBytes = data,
				GraphicsBytes = graphics,
				UnknownBytes = Math.Max(0, total - code - data - graphics),
				LabelCount = labels,
				BlockCount = group.Count()
			});
		}

		return banks;
	}

	/// <summary>
	/// Generate a full coverage JSON document including per-bank breakdowns.
	/// </summary>
	public static string ToJson(DisassemblyResult result) {
		var coverage = Analyze(result);
		var banks = AnalyzeByBank(result);

		var obj = new Dictionary<string, object> {
			["generated"] = DateTime.UtcNow.ToString("o"),
			["totalBytes"] = coverage.TotalBytes,
			["classified"] = new Dictionary<string, object> {
				["code"] = new { bytes = coverage.CodeBytes, percentage = Pct(coverage.CodeBytes, coverage.TotalBytes) },
				["data"] = new { bytes = coverage.DataBytes, percentage = Pct(coverage.DataBytes, coverage.TotalBytes) },
				["graphics"] = new { bytes = coverage.GraphicsBytes, percentage = Pct(coverage.GraphicsBytes, coverage.TotalBytes) },
				["unknown"] = new { bytes = coverage.UnknownBytes, percentage = Pct(coverage.UnknownBytes, coverage.TotalBytes) }
			},
			["analysis"] = new Dictionary<string, object> {
				["labels"] = coverage.LabelCount,
				["comments"] = coverage.CommentCount,
				["crossRefs"] = coverage.CrossRefCount,
				["pointerTables"] = coverage.PointerTableCount,
				["blocks"] = coverage.BlockCount,
				["entryPoints"] = coverage.EntryPointCount
			}
		};

		if (banks.Count > 1) {
			var bankList = banks.Select(b => new Dictionary<string, object> {
				["bank"] = b.Bank,
				["totalBytes"] = b.TotalBytes,
				["codeBytes"] = b.CodeBytes,
				["dataBytes"] = b.DataBytes,
				["graphicsBytes"] = b.GraphicsBytes,
				["unknownBytes"] = b.UnknownBytes,
				["labels"] = b.LabelCount,
				["blocks"] = b.BlockCount
			}).ToList();
			obj["banks"] = bankList;
		}

		return JsonSerializer.Serialize(obj, JsonOpts);
	}

	/// <summary>
	/// Generate a plain-text coverage summary suitable for console output.
	/// </summary>
	public static string ToText(DisassemblyResult result) {
		var coverage = Analyze(result);
		var banks = AnalyzeByBank(result);
		var lines = new List<string> {
			$"Coverage Report for {result.RomInfo?.Platform ?? "Unknown"} ROM ({coverage.TotalBytes:N0} bytes)",
			new string('─', 60),
			$"  Code:      {coverage.CodeBytes,10:N0} bytes ({Pct(coverage.CodeBytes, coverage.TotalBytes),5:F1}%)",
			$"  Data:      {coverage.DataBytes,10:N0} bytes ({Pct(coverage.DataBytes, coverage.TotalBytes),5:F1}%)",
			$"  Graphics:  {coverage.GraphicsBytes,10:N0} bytes ({Pct(coverage.GraphicsBytes, coverage.TotalBytes),5:F1}%)",
			$"  Unknown:   {coverage.UnknownBytes,10:N0} bytes ({Pct(coverage.UnknownBytes, coverage.TotalBytes),5:F1}%)",
			"",
			$"  Labels:         {coverage.LabelCount,6}",
			$"  Comments:       {coverage.CommentCount,6}",
			$"  Cross-refs:     {coverage.CrossRefCount,6}",
			$"  Pointer tables: {coverage.PointerTableCount,6}",
			$"  Blocks:         {coverage.BlockCount,6}",
			$"  Entry points:   {coverage.EntryPointCount,6}"
		};

		if (banks.Count > 1) {
			lines.Add("");
			lines.Add("Per-Bank Breakdown:");
			lines.Add(new string('─', 60));

			foreach (var bank in banks) {
				lines.Add($"  Bank {bank.Bank,3}: {bank.TotalBytes,8:N0} bytes " +
					$"(code={bank.CodeBytes:N0}, data={bank.DataBytes:N0}, " +
					$"gfx={bank.GraphicsBytes:N0}) — {bank.LabelCount} labels, {bank.BlockCount} blocks");
			}
		}

		return string.Join(Environment.NewLine, lines);
	}

	private static double Pct(int part, int total) =>
		total > 0 ? Math.Round(100.0 * part / total, 1) : 0.0;
}
