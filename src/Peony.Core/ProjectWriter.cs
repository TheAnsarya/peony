using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Peony.Core;

/// <summary>
/// Configuration for project generation.
/// </summary>
public sealed class ProjectOptions {
	public required string ProjectName { get; init; }
	public required string RomPath { get; init; }
	public bool SplitBanks { get; init; } = true;
	public bool ExtractAssets { get; init; } = true;
	public bool GenerateIncludes { get; init; } = true;
	public bool GenerateDocs { get; init; } = true;
	public bool IncludeRom { get; init; } = true;
	public string? CdlPath { get; init; }
	public string? PansyPath { get; init; }
	public string? SymbolPath { get; init; }
	public string? DizPath { get; init; }
}

/// <summary>
/// Disassembly coverage report for a project.
/// </summary>
public sealed record CoverageReport {
	public int TotalBytes { get; init; }
	public int CodeBytes { get; init; }
	public int DataBytes { get; init; }
	public int GraphicsBytes { get; init; }
	public int UnknownBytes { get; init; }
	public int LabelCount { get; init; }
	public int CommentCount { get; init; }
	public int CrossRefCount { get; init; }
	public int PointerTableCount { get; init; }
	public int BlockCount { get; init; }
	public int EntryPointCount { get; init; }
}

/// <summary>
/// Generates complete disassembly project folders and .peony zip archives
/// containing source files, analysis data, extracted assets, and build manifests.
/// </summary>
public sealed class ProjectWriter {
	private static readonly JsonSerializerOptions JsonOpts = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly ProjectOptions _options;
	private readonly IGraphicsExtractor? _graphicsExtractor;
	private readonly ITextExtractor? _textExtractor;

	public ProjectWriter(ProjectOptions options, IGraphicsExtractor? graphicsExtractor = null, ITextExtractor? textExtractor = null) {
		_options = options;
		_graphicsExtractor = graphicsExtractor;
		_textExtractor = textExtractor;
	}

	/// <summary>
	/// Write a complete project to a directory (extracted form).
	/// </summary>
	public void WriteProjectFolder(string outputDir, DisassemblyResult result, byte[] romBytes) {
		Directory.CreateDirectory(outputDir);

		WritePeonyManifest(outputDir, result, romBytes);
		WritePoppyManifest(outputDir, result.RomInfo);
		WriteRomFiles(outputDir, result.RomInfo, romBytes);
		WriteSourceFiles(outputDir, result);
		if (_options.GenerateIncludes)
			WriteIncludeFiles(outputDir, result.RomInfo);
		WriteAnalysisFiles(outputDir, result, romBytes);
		if (_options.ExtractAssets)
			WriteAssetFiles(outputDir, result, romBytes);
		if (_options.GenerateDocs)
			WriteDocFiles(outputDir, result);
		WriteVersionFile(outputDir);
	}

	/// <summary>
	/// Write a complete project as a .peony zip archive.
	/// </summary>
	public void WriteProjectArchive(string archivePath, DisassemblyResult result, byte[] romBytes) {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-{Guid.NewGuid():N}");
		try {
			WriteProjectFolder(tempDir, result, romBytes);

			if (File.Exists(archivePath))
				File.Delete(archivePath);

			ZipFile.CreateFromDirectory(tempDir, archivePath, CompressionLevel.Optimal, includeBaseDirectory: false);
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, recursive: true);
		}
	}

	/// <summary>
	/// Compute coverage statistics for a disassembly result.
	/// Delegates to <see cref="CoverageAnalyzer.Analyze"/>.
	/// </summary>
	public static CoverageReport ComputeCoverage(DisassemblyResult result) =>
		CoverageAnalyzer.Analyze(result);

	private void WritePeonyManifest(string dir, DisassemblyResult result, byte[] romBytes) {
		var romInfo = result.RomInfo;
		var coverage = ComputeCoverage(result);
		var romFilename = Path.GetFileName(_options.RomPath);
		var crc32 = ComputeCrc32Hex(romBytes);
		var sha256 = ComputeSha256Hex(romBytes);

		var manifest = new Dictionary<string, object> {
			["formatVersion"] = "1.0.0",
			["name"] = _options.ProjectName,
			["displayName"] = FormatDisplayName(_options.ProjectName),
			["description"] = $"Disassembly of {romFilename} ({romInfo.Platform})",
			["author"] = "",
			["created"] = DateTime.UtcNow.ToString("o"),
			["modified"] = DateTime.UtcNow.ToString("o"),
			["rom"] = new Dictionary<string, object> {
				["filename"] = $"rom/{romFilename}",
				["platform"] = romInfo.Platform.ToLowerInvariant(),
				["crc32"] = crc32,
				["sha256"] = sha256,
				["size"] = romBytes.Length,
				["mapper"] = romInfo.Mapper ?? "",
				["includeRom"] = _options.IncludeRom
			},
			["analysis"] = new Dictionary<string, object> {
				["pansy"] = $"analysis/{_options.ProjectName}.pansy",
				["coverage"] = "analysis/coverage.json",
				["crossRefs"] = "analysis/cross-refs.json"
			},
			["source"] = new Dictionary<string, object> {
				["entry"] = "src/main.pasm"
			},
			["statistics"] = new Dictionary<string, object> {
				["totalBytes"] = coverage.TotalBytes,
				["codeBytes"] = coverage.CodeBytes,
				["dataBytes"] = coverage.DataBytes,
				["graphicsBytes"] = coverage.GraphicsBytes,
				["unknownBytes"] = coverage.UnknownBytes,
				["labelCount"] = coverage.LabelCount,
				["commentCount"] = coverage.CommentCount,
				["crossRefCount"] = coverage.CrossRefCount,
				["blockCount"] = coverage.BlockCount
			},
			["settings"] = new Dictionary<string, object> {
				["splitBanks"] = _options.SplitBanks,
				["extractAssets"] = _options.ExtractAssets,
				["generateIncludes"] = _options.GenerateIncludes,
				["generateDocs"] = _options.GenerateDocs,
				["includeRomInArchive"] = _options.IncludeRom
			}
		};

		var json = JsonSerializer.Serialize(manifest, JsonOpts);
		File.WriteAllText(Path.Combine(dir, "peony-project.json"), json, Encoding.UTF8);
	}

	private void WritePoppyManifest(string dir, RomInfo romInfo) {
		var manifest = new Dictionary<string, object> {
			["name"] = _options.ProjectName,
			["version"] = "1.0.0",
			["platform"] = NormalizePoppyPlatform(romInfo.Platform),
			["entry"] = "src/main.pasm",
			["output"] = $"build/{_options.ProjectName}.{GetRomExtension(romInfo.Platform)}",
			["compiler"] = new Dictionary<string, object> {
				["target"] = NormalizePoppyPlatform(romInfo.Platform),
				["options"] = new Dictionary<string, object> {
					["optimize"] = false,
					["debug"] = false,
					["warnings"] = "all"
				}
			},
			["build"] = new Dictionary<string, object> {
				["includePaths"] = new[] { "include/" },
				["defines"] = new Dictionary<string, object>()
			},
			["metadata"] = new Dictionary<string, object> {
				["description"] = "Disassembled by Peony",
				["tags"] = new[] { "disassembly", romInfo.Platform.ToLowerInvariant(), "peony" }
			}
		};

		var json = JsonSerializer.Serialize(manifest, JsonOpts);
		File.WriteAllText(Path.Combine(dir, "poppy.json"), json, Encoding.UTF8);
	}

	private void WriteRomFiles(string dir, RomInfo romInfo, byte[] romBytes) {
		var romDir = Path.Combine(dir, "rom");
		Directory.CreateDirectory(romDir);

		if (_options.IncludeRom) {
			var romFilename = Path.GetFileName(_options.RomPath);
			File.WriteAllBytes(Path.Combine(romDir, romFilename), romBytes);
		}

		var romInfoObj = new Dictionary<string, object> {
			["filename"] = Path.GetFileName(_options.RomPath),
			["platform"] = romInfo.Platform,
			["size"] = romBytes.Length,
			["crc32"] = ComputeCrc32Hex(romBytes),
			["sha256"] = ComputeSha256Hex(romBytes),
			["mapper"] = romInfo.Mapper ?? ""
		};

		if (romInfo.Metadata.Count > 0)
			romInfoObj["header"] = romInfo.Metadata;

		var json = JsonSerializer.Serialize(romInfoObj, JsonOpts);
		File.WriteAllText(Path.Combine(romDir, "rom-info.json"), json, Encoding.UTF8);
	}

	private void WriteSourceFiles(string dir, DisassemblyResult result) {
		var srcDir = Path.Combine(dir, "src");
		Directory.CreateDirectory(srcDir);

		if (_options.SplitBanks && result.BankBlocks.Count > 1) {
			WriteSplitBankSource(srcDir, result);
		} else {
			WriteMonolithicSource(srcDir, result);
		}
	}

	private void WriteSplitBankSource(string srcDir, DisassemblyResult result) {
		var banksDir = Path.Combine(srcDir, "banks");
		Directory.CreateDirectory(banksDir);

		var formatter = new PoppyFormatter();
		var bankFiles = new List<string>();

		foreach (var (bank, blocks) in result.BankBlocks.OrderBy(kv => kv.Key)) {
			var bankName = $"bank{bank:d2}";
			var bankFile = $"banks/{bankName}.pasm";
			bankFiles.Add(bankFile);

			// Create a sub-result for this bank
			var bankResult = new DisassemblyResult { RomInfo = result.RomInfo };
			foreach (var block in blocks)
				bankResult.Blocks.Add(block);

			// Copy labels and comments relevant to this bank
			foreach (var block in blocks) {
				foreach (var line in block.Lines) {
					if (result.Labels.TryGetValue(line.Address, out var label))
						bankResult.Labels[line.Address] = label;
					if (result.Comments.TryGetValue(line.Address, out var comment))
						bankResult.Comments[line.Address] = comment;
					if (result.DataRegions.TryGetValue(line.Address, out var dataDef))
						bankResult.DataRegions[line.Address] = dataDef;
					if (result.TypedComments.TryGetValue(line.Address, out var typedComment))
						bankResult.TypedComments[line.Address] = typedComment;
				}
			}

			formatter.Generate(bankResult, Path.Combine(srcDir, bankFile));
		}

		// Write main.pasm with includes
		var sb = new StringBuilder();
		sb.AppendLine($"; {FormatDisplayName(_options.ProjectName)} — Disassembled by Peony");
		sb.AppendLine($"; Platform: {result.RomInfo.Platform}");
		if (result.RomInfo.Mapper is not null)
			sb.AppendLine($"; Mapper: {result.RomInfo.Mapper}");
		sb.AppendLine();

		if (_options.GenerateIncludes) {
			sb.AppendLine(".include \"include/hardware.inc\"");
			sb.AppendLine(".include \"include/constants.inc\"");
			sb.AppendLine();
		}

		foreach (var bankFile in bankFiles) {
			sb.AppendLine($".include \"{bankFile}\"");
		}

		File.WriteAllText(Path.Combine(srcDir, "main.pasm"), sb.ToString(), Encoding.UTF8);
	}

	private void WriteMonolithicSource(string srcDir, DisassemblyResult result) {
		var formatter = new PoppyFormatter();
		formatter.Generate(result, Path.Combine(srcDir, "main.pasm"));
	}

	private void WriteIncludeFiles(string dir, RomInfo romInfo) {
		var includeDir = Path.Combine(dir, "include");
		Directory.CreateDirectory(includeDir);

		var hardware = HardwareIncludeGenerator.Generate(romInfo.Platform);
		File.WriteAllText(Path.Combine(includeDir, "hardware.inc"), hardware, Encoding.UTF8);

		var constants = GenerateConstantsInclude();
		File.WriteAllText(Path.Combine(includeDir, "constants.inc"), constants, Encoding.UTF8);

		var macros = GenerateMacrosInclude();
		File.WriteAllText(Path.Combine(includeDir, "macros.inc"), macros, Encoding.UTF8);
	}

	private void WriteAnalysisFiles(string dir, DisassemblyResult result, byte[] romBytes) {
		var analysisDir = Path.Combine(dir, "analysis");
		Directory.CreateDirectory(analysisDir);

		// Export Pansy file
		var pansyPath = Path.Combine(analysisDir, $"{_options.ProjectName}.pansy");
		SymbolExporter.ExportPansy(result, pansyPath);

		// Copy input CDL if provided
		if (_options.CdlPath is not null && File.Exists(_options.CdlPath)) {
			File.Copy(_options.CdlPath, Path.Combine(analysisDir, Path.GetFileName(_options.CdlPath)), overwrite: true);
		}

		// Copy input Pansy if provided (separate from exported)
		if (_options.PansyPath is not null && File.Exists(_options.PansyPath)) {
			var inputPansyName = $"input-{Path.GetFileName(_options.PansyPath)}";
			File.Copy(_options.PansyPath, Path.Combine(analysisDir, inputPansyName), overwrite: true);
		}

		// Coverage statistics (with per-bank breakdown)
		File.WriteAllText(Path.Combine(analysisDir, "coverage.json"),
			CoverageAnalyzer.ToJson(result), Encoding.UTF8);

		// Cross-references as JSON
		var xrefList = new List<Dictionary<string, object>>();
		foreach (var (target, refs) in result.CrossReferences) {
			foreach (var xref in refs) {
				xrefList.Add(new Dictionary<string, object> {
					["from"] = $"${xref.FromAddress:x4}",
					["to"] = $"${target:x4}",
					["type"] = xref.Type.ToString().ToLowerInvariant(),
					["bank"] = xref.FromBank
				});
			}
		}
		File.WriteAllText(Path.Combine(analysisDir, "cross-refs.json"),
			JsonSerializer.Serialize(xrefList, JsonOpts), Encoding.UTF8);
	}

	private void WriteAssetFiles(string dir, DisassemblyResult result, byte[] romBytes) {
		var assetsDir = Path.Combine(dir, "assets");
		Directory.CreateDirectory(assetsDir);

		// Extract graphics if extractor is available
		if (_graphicsExtractor is not null) {
			var gfxDir = Path.Combine(assetsDir, "graphics");
			Directory.CreateDirectory(gfxDir);

			try {
				var gfxOptions = new GraphicsExtractionOptions {
					OutputDirectory = gfxDir,
					ImageFormat = "bmp",
					TilesPerRow = 16,
					GenerateMetadata = true
				};
				var gfxResult = _graphicsExtractor.ExtractAll(romBytes, gfxOptions);

				// Write extraction summary
				var summary = new Dictionary<string, object> {
					["platform"] = _graphicsExtractor.Platform,
					["tilesExtracted"] = gfxResult.TotalTiles,
					["tilesets"] = gfxResult.TileSets.Count,
					["palettes"] = gfxResult.Palettes.Count,
					["files"] = gfxResult.OutputFiles
				};

				File.WriteAllText(Path.Combine(gfxDir, "extraction-summary.json"),
					JsonSerializer.Serialize(summary, JsonOpts), Encoding.UTF8);
			} catch (Exception) {
				// Extraction failed — write a placeholder noting the failure
				File.WriteAllText(Path.Combine(gfxDir, "README.md"),
					"# Graphics\n\nAutomatic graphics extraction was not successful.\nUse `peony chr` to extract manually.\n",
					Encoding.UTF8);
			}
		}

		// Extract text if extractor is available
		if (_textExtractor is not null) {
			var textDir = Path.Combine(assetsDir, "text");
			Directory.CreateDirectory(textDir);

			try {
				var defaultTable = TableFile.CreateAsciiTable();
				var textOptions = new TextExtractionOptions {
					StartOffset = 0,
					EndOffset = romBytes.Length,
					MinLength = 4
				};
				var textBlocks = _textExtractor.ExtractAllText(romBytes, defaultTable, textOptions);

				if (textBlocks.Count > 0) {
					var sb = new StringBuilder();
					foreach (var block in textBlocks) {
						sb.AppendLine($"; ${block.Offset:x6}: {block.Text}");
					}
					File.WriteAllText(Path.Combine(textDir, "extracted-text.txt"), sb.ToString(), Encoding.UTF8);

					// JSON version
					var jsonBlocks = textBlocks.Select(b => new Dictionary<string, object> {
						["offset"] = $"${b.Offset:x6}",
						["length"] = b.Length,
						["text"] = b.Text,
						["label"] = b.Label ?? "",
						["category"] = b.Category ?? ""
					}).ToList();
					File.WriteAllText(Path.Combine(textDir, "extracted-text.json"),
						JsonSerializer.Serialize(jsonBlocks, JsonOpts), Encoding.UTF8);
				}
			} catch (Exception) {
				File.WriteAllText(Path.Combine(textDir, "README.md"),
					"# Text\n\nAutomatic text extraction was not successful.\nUse `peony text` with a `.tbl` file to extract manually.\n",
					Encoding.UTF8);
			}
		}

		// Always create assets README
		if (_graphicsExtractor is null && _textExtractor is null) {
			File.WriteAllText(Path.Combine(assetsDir, "README.md"),
				"# Assets\n\nNo extractors were configured for this platform.\nUse `peony chr`, `peony text`, and `peony palette` to extract manually.\n",
				Encoding.UTF8);
		}
	}

	private void WriteDocFiles(string dir, DisassemblyResult result) {
		var docsDir = Path.Combine(dir, "docs");
		Directory.CreateDirectory(docsDir);

		var coverage = ComputeCoverage(result);

		// Project README
		var sb = new StringBuilder();
		sb.AppendLine($"# {FormatDisplayName(_options.ProjectName)}");
		sb.AppendLine();
		sb.AppendLine("> Disassembled by Peony");
		sb.AppendLine();
		sb.AppendLine("## ROM Info");
		sb.AppendLine();
		sb.AppendLine($"- **Platform:** {result.RomInfo.Platform}");
		sb.AppendLine($"- **ROM File:** {Path.GetFileName(_options.RomPath)}");
		sb.AppendLine($"- **Size:** {result.RomInfo.Size:N0} bytes");
		if (result.RomInfo.Mapper is not null)
			sb.AppendLine($"- **Mapper:** {result.RomInfo.Mapper}");
		sb.AppendLine();
		sb.AppendLine("## Coverage");
		sb.AppendLine();
		sb.AppendLine($"- **Code:** {coverage.CodeBytes:N0} bytes ({Pct(coverage.CodeBytes, coverage.TotalBytes)}%)");
		sb.AppendLine($"- **Data:** {coverage.DataBytes:N0} bytes ({Pct(coverage.DataBytes, coverage.TotalBytes)}%)");
		sb.AppendLine($"- **Unknown:** {coverage.UnknownBytes:N0} bytes ({Pct(coverage.UnknownBytes, coverage.TotalBytes)}%)");
		sb.AppendLine($"- **Labels:** {coverage.LabelCount}");
		sb.AppendLine($"- **Cross-references:** {coverage.CrossRefCount}");
		sb.AppendLine();
		sb.AppendLine("## Build");
		sb.AppendLine();
		sb.AppendLine("```bash");
		sb.AppendLine("poppy build --project poppy.json");
		sb.AppendLine("```");
		sb.AppendLine();
		sb.AppendLine("## Directory Structure");
		sb.AppendLine();
		sb.AppendLine("```");
		sb.AppendLine("src/          — Disassembled source files (.pasm)");
		sb.AppendLine("include/      — Hardware and constant definitions (.inc)");
		sb.AppendLine("analysis/     — Pansy metadata, CDL, cross-refs, coverage");
		sb.AppendLine("rom/          — Original ROM file and metadata");
		sb.AppendLine("assets/       — Extracted graphics, text, palettes");
		sb.AppendLine("docs/         — Project documentation");
		sb.AppendLine("build/        — Assembled ROM output (after building)");
		sb.AppendLine("```");

		File.WriteAllText(Path.Combine(dir, "README.md"), sb.ToString(), Encoding.UTF8);
	}

	private static void WriteVersionFile(string dir) {
		var peonyDir = Path.Combine(dir, ".peony");
		Directory.CreateDirectory(peonyDir);
		File.WriteAllText(Path.Combine(peonyDir, "version"), "1.0.0", Encoding.UTF8);
	}

	private static string FormatDisplayName(string name) {
		// Convert kebab-case to Title Case
		return string.Join(' ', name.Split('-', '_')
			.Select(w => w.Length > 0 ? char.ToUpperInvariant(w[0]) + w[1..] : w));
	}

	private static string Pct(int part, int total) =>
		total > 0 ? (100.0 * part / total).ToString("F1") : "0.0";

	private static string NormalizePoppyPlatform(string platform) {
		return platform.ToLowerInvariant() switch {
			"atari 2600" or "atari2600" or "a26" => "atari2600",
			"atari lynx" or "lynx" => "lynx",
			"game boy" or "gameboy" or "gb" => "gb",
			"game boy advance" or "gba" => "gba",
			"nes" => "nes",
			"snes" or "super nes" or "super nintendo" => "snes",
			_ => platform.ToLowerInvariant()
		};
	}

	private static string GetRomExtension(string platform) {
		return platform.ToLowerInvariant() switch {
			"nes" => "nes",
			"snes" or "super nes" => "sfc",
			"game boy" or "gameboy" or "gb" => "gb",
			"game boy advance" or "gba" => "gba",
			"atari 2600" or "atari2600" or "a26" => "a26",
			"atari lynx" or "lynx" => "lnx",
			"genesis" or "mega drive" => "bin",
			_ => "bin"
		};
	}

	private static string ComputeCrc32Hex(byte[] data) {
		uint crc = 0xffffffff;
		foreach (var b in data) {
			crc ^= b;
			for (int i = 0; i < 8; i++)
				crc = (crc >> 1) ^ (0xedb88320 & ~((crc & 1) - 1));
		}
		return (crc ^ 0xffffffff).ToString("x8");
	}

	private static string ComputeSha256Hex(byte[] data) {
		var hash = SHA256.HashData(data);
		return Convert.ToHexStringLower(hash);
	}

	private static string GenerateConstantsInclude() {
		var sb = new StringBuilder();
		sb.AppendLine("; Game Constants");
		sb.AppendLine("; Generated by Peony — add game-specific constants here");
		sb.AppendLine();
		sb.AppendLine("; (Add your constants below)");
		return sb.ToString();
	}

	private static string GenerateMacrosInclude() {
		var sb = new StringBuilder();
		sb.AppendLine("; Macros");
		sb.AppendLine("; Generated by Peony — add reusable macros here");
		sb.AppendLine();
		sb.AppendLine("; (Add your macros below)");
		return sb.ToString();
	}
}
