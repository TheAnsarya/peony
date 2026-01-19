using System.CommandLine;
using Peony.Core;
using Peony.Platform.Atari2600;
using Peony.Platform.NES;
using Peony.Platform.GameBoy;
using Peony.Platform.GBA;
using Spectre.Console;

// 🌺 Peony Disassembler CLI

var rootCommand = new RootCommand("🌺 Peony - Multi-system ROM disassembler");

// Disasm command
var disasmCommand = new Command("disasm", "Disassemble a ROM file");
var romArg = new Argument<FileInfo>("rom", "ROM file to disassemble");
var outputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file (default: stdout)");
var platformOpt = new Option<string?>(["--platform", "-p"], "Platform (auto-detected if not specified)");
var formatOpt = new Option<string>(["--format", "-f"], () => "asm", "Output format: asm, poppy");
var allBanksOpt = new Option<bool>(["--all-banks", "-b"], "Disassemble all banks for banked ROMs (MMC1, etc.)");
var symbolsOpt = new Option<FileInfo?>(["--symbols", "-s"], "Symbol file to load (JSON, .nl, .mlb, .sym)");
var cdlOpt = new Option<FileInfo?>(["--cdl", "-c"], "CDL (Code/Data Log) file for code/data hints");
var dizOpt = new Option<FileInfo?>(["--diz", "-d"], "DIZ (DiztinGUIsh) project file for labels and data types");

disasmCommand.AddArgument(romArg);
disasmCommand.AddOption(outputOpt);
disasmCommand.AddOption(platformOpt);
disasmCommand.AddOption(formatOpt);
disasmCommand.AddOption(allBanksOpt);
disasmCommand.AddOption(symbolsOpt);
disasmCommand.AddOption(cdlOpt);
disasmCommand.AddOption(dizOpt);

disasmCommand.SetHandler((rom, output, platform, format, allBanks, symbols, cdlFile, dizFile) => {
try {
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Disassembler[/]");
AnsiConsole.WriteLine();

// Load ROM
AnsiConsole.MarkupLine($"[grey]Loading:[/] {Markup.Escape(rom.FullName)}");
var romData = RomLoader.Load(rom.FullName);
AnsiConsole.MarkupLine($"[grey]Size:[/] {romData.Length} bytes ({romData.Length / 1024}K)");

// Detect platform
platform ??= RomLoader.DetectPlatform(romData, rom.FullName);
AnsiConsole.MarkupLine($"[grey]Platform:[/] {platform}");

// Get platform analyzer
IPlatformAnalyzer analyzer = platform?.ToLowerInvariant() switch {
"atari2600" or "atari 2600" or "2600" => new Atari2600Analyzer(),
"nes" => new NesAnalyzer(),
"snes" or "super nintendo" or "super nes" => new Peony.Platform.SNES.SnesAnalyzer(),
			"gameboy" or "game boy" or "gb" => new GameBoyAnalyzer(),
			"gba" or "game boy advance" or "gameboy advance" or "advance" => new GbaAnalyzer(),
			_ => throw new NotSupportedException($"Platform not supported: {platform}")
		};

		// Analyze ROM
		var info = analyzer.Analyze(romData);
		AnsiConsole.MarkupLine($"[grey]Mapper:[/] {info.Mapper ?? "None"}");

		if (analyzer.BankCount > 1) {
			AnsiConsole.MarkupLine($"[grey]Banks:[/] {analyzer.BankCount}");
			if (allBanks) {
				AnsiConsole.MarkupLine($"[cyan]Multi-bank mode enabled[/]");
			}
		}

		// Load symbols if provided
		SymbolLoader? symbolLoader = null;
		if (symbols?.Exists == true) {
			symbolLoader = new SymbolLoader();
			symbolLoader.Load(symbols.FullName);
			AnsiConsole.MarkupLine($"[grey]Symbols:[/] {symbolLoader.Labels.Count} labels loaded");
		}

AnsiConsole.WriteLine();

// Get entry points
uint[] entryPoints = analyzer switch {
Atari2600Analyzer a2600 => a2600.GetEntryPoints(romData),
NesAnalyzer nes => nes.GetEntryPoints(romData),
_ => [0x8000]
};

AnsiConsole.MarkupLine($"[grey]Entry points:[/] {string.Join(", ", entryPoints.Select(e => $"${e:x4}"))}");

// Create engine and disassemble
var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);

// Set symbol loader for CDL/DIZ hints
if (symbolLoader != null) {
engine.SetSymbolLoader(symbolLoader);
}

// Add symbols to engine
if (symbolLoader != null) {
foreach (var (addr, label) in symbolLoader.Labels) {
engine.AddLabel(addr, label);
}
foreach (var (addr, comment) in symbolLoader.Comments) {
engine.AddComment(addr, comment);
}
foreach (var (addr, dataDef) in symbolLoader.DataDefinitions) {
engine.AddDataRegion(addr, dataDef);
}
if (symbolLoader.DataDefinitions.Count > 0) {
AnsiConsole.MarkupLine($"[grey]Data regions:[/] {symbolLoader.DataDefinitions.Count}");
}
}

var result = engine.Disassemble(romData, entryPoints, allBanks);
result.RomInfo = info;

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"[green]Disassembled {result.Blocks.Count} blocks[/]");

if (allBanks) {
foreach (var bank in result.BankBlocks.Keys.OrderBy(b => b)) {
var count = result.BankBlocks[bank].Count;
if (count > 0) {
AnsiConsole.MarkupLine($"[grey]  Bank {bank}:[/] {count} blocks");
}
}
}

// Determine output path
var outputPath = output?.FullName ?? GetDefaultOutputPath(rom, format);

// Output based on format
if (format == "poppy") {
var formatter = new PoppyFormatter();
formatter.Generate(result, outputPath);
AnsiConsole.MarkupLine($"[green]Poppy output written to:[/] {Markup.Escape(outputPath)}");
} else {
// Standard ASM output
using var writer = output != null
? new StreamWriter(outputPath)
: Console.Out;

WriteAsmOutput(writer, rom, info, result);

if (output != null)
AnsiConsole.MarkupLine($"[green]Output written to:[/] {Markup.Escape(outputPath)}");
}
}
catch (Exception ex) {
AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
Environment.Exit(1);
}
}, romArg, outputOpt, platformOpt, formatOpt, allBanksOpt, symbolsOpt, cdlOpt, dizOpt);

rootCommand.AddCommand(disasmCommand);

// Batch command for multiple ROMs
var batchCommand = new Command("batch", "Disassemble multiple ROM files");
var inputDirArg = new Argument<DirectoryInfo>("input", "Directory containing ROMs");
var outputDirOpt = new Option<DirectoryInfo?>("--output", "Output directory");
var patternOpt = new Option<string>("--pattern", () => "*.a26", "File pattern to match");
var batchFormatOpt = new Option<string>("--format", () => "poppy", "Output format: asm, poppy");

batchCommand.AddArgument(inputDirArg);
batchCommand.AddOption(outputDirOpt);
batchCommand.AddOption(patternOpt);
batchCommand.AddOption(batchFormatOpt);

batchCommand.SetHandler((inputDir, outputDir, pattern, format) => {
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Batch Disassembler[/]");
AnsiConsole.WriteLine();

outputDir ??= new DirectoryInfo(Path.Combine(inputDir.FullName, "disasm"));
outputDir.Create();

var files = inputDir.GetFiles(pattern);
AnsiConsole.MarkupLine($"[grey]Found {files.Length} files matching '{pattern}'[/]");
AnsiConsole.WriteLine();

int success = 0, failed = 0;

foreach (var file in files) {
try {
var romData = RomLoader.Load(file.FullName);
var platform = RomLoader.DetectPlatform(romData, file.FullName);

IPlatformAnalyzer analyzer = platform?.ToLowerInvariant() switch {
"atari2600" or "atari 2600" => new Atari2600Analyzer(),
"nes" => new NesAnalyzer(),
_ => throw new NotSupportedException($"Unknown platform: {platform}")
};

var info = analyzer.Analyze(romData);
var entryPoints = analyzer switch {
Atari2600Analyzer a2600 => a2600.GetEntryPoints(romData),
NesAnalyzer nes => nes.GetEntryPoints(romData),
_ => [0x8000]
};

var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);
var result = engine.Disassemble(romData, entryPoints);
result.RomInfo = info;

var ext = ".pasm";
var outputPath = Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(file.Name) + ext);

if (format == "poppy") {
var formatter = new PoppyFormatter();
formatter.Generate(result, outputPath);
} else {
using var writer = new StreamWriter(outputPath);
WriteAsmOutput(writer, file, info, result);
}

AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(file.Name)}");
success++;
}
catch (Exception ex) {
AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(file.Name)}: {Markup.Escape(ex.Message)}");
failed++;
}
}

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"[green]Success: {success}[/] | [red]Failed: {failed}[/]");
}, inputDirArg, outputDirOpt, patternOpt, batchFormatOpt);

rootCommand.AddCommand(batchCommand);

// Info command
var infoCommand = new Command("info", "Show ROM information");
var infoRomArg = new Argument<FileInfo>("rom", "ROM file to analyze");
infoCommand.AddArgument(infoRomArg);

infoCommand.SetHandler((rom) => {
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony ROM Info[/]");
AnsiConsole.WriteLine();

var romData = RomLoader.Load(rom.FullName);
var platform = RomLoader.DetectPlatform(romData, rom.FullName);

var table = new Table();
table.AddColumn("Property");
table.AddColumn("Value");

table.AddRow("File", Markup.Escape(rom.Name));
table.AddRow("Size", $"{romData.Length} bytes ({romData.Length / 1024}K)");
table.AddRow("Platform", platform ?? "Unknown");

if (platform?.ToLowerInvariant() is "atari2600" or "atari 2600") {
var analyzer = new Atari2600Analyzer();
var info = analyzer.Analyze(romData);
table.AddRow("Mapper", info.Mapper ?? "None");
table.AddRow("Banks", analyzer.BankCount.ToString());
foreach (var (key, value) in info.Metadata)
table.AddRow(Markup.Escape(key), Markup.Escape(value));
var entries = analyzer.GetEntryPoints(romData);
table.AddRow("Entry Points", string.Join(", ", entries.Select(e => $"${e:x4}")));
} else if (platform?.ToLowerInvariant() == "nes") {
var analyzer = new NesAnalyzer();
var info = analyzer.Analyze(romData);
table.AddRow("Mapper", info.Mapper ?? "Unknown");
table.AddRow("Banks", analyzer.BankCount.ToString());
foreach (var (key, value) in info.Metadata)
table.AddRow(Markup.Escape(key), Markup.Escape(value));
var entries = analyzer.GetEntryPoints(romData);
table.AddRow("Entry Points", string.Join(", ", entries.Select(e => $"${e:x4}")));
}

AnsiConsole.Write(table);
}, infoRomArg);

rootCommand.AddCommand(infoCommand);

// Export symbols command
var exportCommand = new Command("export", "Export symbols from disassembly to various formats");
var exportRomArg = new Argument<FileInfo>("rom", "ROM file to disassemble for symbols");
var exportOutputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file (required)");
var exportFormatOpt = new Option<string>(["--format", "-f"], () => "mesen", "Symbol format: mesen, fceux, nogba, ca65, wla, bizhawk, pansy");
var exportPlatformOpt = new Option<string?>(["--platform", "-p"], "Platform (auto-detected if not specified)");
var exportSymbolsOpt = new Option<FileInfo?>(["--symbols", "-s"], "Additional symbol file to merge");
var exportDizOpt = new Option<FileInfo?>(["--diz", "-d"], "DIZ project file to merge");

exportCommand.AddArgument(exportRomArg);
exportCommand.AddOption(exportOutputOpt);
exportCommand.AddOption(exportFormatOpt);
exportCommand.AddOption(exportPlatformOpt);
exportCommand.AddOption(exportSymbolsOpt);
exportCommand.AddOption(exportDizOpt);

exportCommand.SetHandler((rom, output, format, platform, symbols, dizFile) => {
	try {
		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Symbol Exporter[/]");
		AnsiConsole.WriteLine();

		if (output == null) {
			AnsiConsole.MarkupLine("[red]Error:[/] Output file is required (--output)");
			Environment.Exit(1);
			return;
		}

		// Load ROM
		AnsiConsole.MarkupLine($"[grey]Loading:[/] {Markup.Escape(rom.FullName)}");
		var romData = RomLoader.Load(rom.FullName);

		// Detect platform
		platform ??= RomLoader.DetectPlatform(romData, rom.FullName);

		// Get platform analyzer
		IPlatformAnalyzer analyzer = platform?.ToLowerInvariant() switch {
			"atari2600" or "atari 2600" or "2600" => new Atari2600Analyzer(),
			"nes" => new NesAnalyzer(),
			"snes" or "super nintendo" or "super nes" => new Peony.Platform.SNES.SnesAnalyzer(),
			"gameboy" or "game boy" or "gb" => new GameBoyAnalyzer(),
			_ => throw new NotSupportedException($"Platform not supported: {platform}")
		};

		// Analyze ROM
		var info = analyzer.Analyze(romData);

		// Load additional symbols
		SymbolLoader? symbolLoader = null;
		if (symbols?.Exists == true) {
			symbolLoader = new SymbolLoader();
			symbolLoader.Load(symbols.FullName);
			AnsiConsole.MarkupLine($"[grey]Merged symbols:[/] {symbolLoader.Labels.Count} labels");
		}

		if (dizFile?.Exists == true) {
			symbolLoader ??= new SymbolLoader();
			symbolLoader.Load(dizFile.FullName);
			AnsiConsole.MarkupLine($"[grey]Merged DIZ:[/] {symbolLoader.Labels.Count} labels");
		}

		// Get entry points and disassemble
		var entryPoints = analyzer.GetEntryPoints(romData);
		var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);

		if (symbolLoader != null) {
			engine.SetSymbolLoader(symbolLoader);
			foreach (var (addr, label) in symbolLoader.Labels) {
				engine.AddLabel(addr, label);
			}
			foreach (var (addr, comment) in symbolLoader.Comments) {
				engine.AddComment(addr, comment);
			}
		}

		var result = engine.Disassemble(romData, entryPoints);
		result.RomInfo = info;

		// Determine symbol format
		var symFormat = format.ToLowerInvariant() switch {
			"mesen" or "mlb" => SymbolFormat.Mesen,
			"fceux" or "nl" => SymbolFormat.FCEUX,
			"nogba" or "nosns" or "no$gba" or "sym" => SymbolFormat.NoGlasses,
			"ca65" or "cc65" or "dbg" => SymbolFormat.Ca65Debug,
			"wla" or "wladx" => SymbolFormat.Wla,
			"bizhawk" or "cht" => SymbolFormat.BizHawk,
			"pansy" => SymbolFormat.Pansy,
			_ => throw new ArgumentException($"Unknown symbol format: {format}")
		};

		// Export
		SymbolExporter.Export(result, output.FullName, symFormat);

		AnsiConsole.MarkupLine($"[green]Exported {result.Labels.Count} labels to:[/] {Markup.Escape(output.FullName)}");
		AnsiConsole.MarkupLine($"[grey]Format:[/] {symFormat}");
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
}, exportRomArg, exportOutputOpt, exportFormatOpt, exportPlatformOpt, exportSymbolsOpt, exportDizOpt);

rootCommand.AddCommand(exportCommand);

// Verify command for roundtrip verification
var verifyCommand = new Command("verify", "Verify ROM roundtrip (disassemble → reassemble → compare)");
var verifyOriginalArg = new Argument<FileInfo>("original", "Original ROM file");
var verifyReassembledOpt = new Option<FileInfo?>(["--reassembled", "-r"], "Reassembled ROM file to compare");
var verifyWorkdirOpt = new Option<DirectoryInfo?>(["--workdir", "-w"], "Working directory for roundtrip test");
var verifyAssemblerOpt = new Option<string>(["--assembler", "-a"], () => "poppy", "Assembler command for roundtrip");
var verifyReportOpt = new Option<FileInfo?>(["--report"], "Write verification report to file");

verifyCommand.AddArgument(verifyOriginalArg);
verifyCommand.AddOption(verifyReassembledOpt);
verifyCommand.AddOption(verifyWorkdirOpt);
verifyCommand.AddOption(verifyAssemblerOpt);
verifyCommand.AddOption(verifyReportOpt);

verifyCommand.SetHandler(async (original, reassembled, workdir, assembler, report) => {
	try {
		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Roundtrip Verifier[/]");
		AnsiConsole.WriteLine();

		RoundtripVerifier.VerificationResult result;

		if (reassembled?.Exists == true) {
			// Direct file comparison
			AnsiConsole.MarkupLine($"[grey]Original:[/]     {Markup.Escape(original.FullName)}");
			AnsiConsole.MarkupLine($"[grey]Reassembled:[/]  {Markup.Escape(reassembled.FullName)}");
			AnsiConsole.WriteLine();

			result = RoundtripVerifier.VerifyFiles(original.FullName, reassembled.FullName);
		} else if (workdir != null) {
			// Full roundtrip test
			AnsiConsole.MarkupLine($"[grey]Original:[/]    {Markup.Escape(original.FullName)}");
			AnsiConsole.MarkupLine($"[grey]Work dir:[/]    {Markup.Escape(workdir.FullName)}");
			AnsiConsole.MarkupLine($"[grey]Assembler:[/]   {assembler}");
			AnsiConsole.WriteLine();

			// Detect platform for roundtrip
			var romData = RomLoader.Load(original.FullName);
			var platform = RomLoader.DetectPlatform(romData, original.FullName);
			IPlatformAnalyzer rtAnalyzer = platform?.ToLowerInvariant() switch {
				"atari2600" or "atari 2600" or "2600" => new Atari2600Analyzer(),
				"nes" => new NesAnalyzer(),
				"snes" or "super nintendo" => new Peony.Platform.SNES.SnesAnalyzer(),
				_ => throw new NotSupportedException($"Platform not supported: {platform}")
			};

			AnsiConsole.Status()
				.Spinner(Spinner.Known.Dots)
				.Start("Running roundtrip test...", ctx => {
					// Can't easily make this async in Spectre.Console
				});

			result = await RoundtripVerifier.RunRoundtripAsync(
				original.FullName,
				workdir.FullName,
				rtAnalyzer,
				assembler);
		} else {
			// Verify disassembly internally
			AnsiConsole.MarkupLine($"[grey]Verifying disassembly of:[/] {Markup.Escape(original.FullName)}");
			AnsiConsole.WriteLine();

			var romData = RomLoader.Load(original.FullName);
			var platform = RomLoader.DetectPlatform(romData, original.FullName);

			IPlatformAnalyzer analyzer = platform?.ToLowerInvariant() switch {
				"atari2600" or "atari 2600" or "2600" => new Atari2600Analyzer(),
				"nes" => new NesAnalyzer(),
				"snes" or "super nintendo" => new Peony.Platform.SNES.SnesAnalyzer(),
				_ => throw new NotSupportedException($"Platform not supported: {platform}")
			};

			var info = analyzer.Analyze(romData);
			var entryPoints = analyzer.GetEntryPoints(romData);
			var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);
			var disasm = engine.Disassemble(romData, entryPoints);

			result = RoundtripVerifier.VerifyDisassembly(disasm, romData);
		}

		// Display results
		if (result.Success) {
			AnsiConsole.MarkupLine("[bold green]✓ VERIFICATION PASSED[/]");
			AnsiConsole.MarkupLine($"[grey]All {result.ByteMatches} bytes match[/]");
		} else {
			AnsiConsole.MarkupLine("[bold red]✗ VERIFICATION FAILED[/]");
			if (result.ErrorMessage != null) {
				AnsiConsole.MarkupLine($"[red]{Markup.Escape(result.ErrorMessage)}[/]");
			}
		}

		AnsiConsole.WriteLine();

		// Show statistics
		var statsTable = new Table();
		statsTable.AddColumn("Metric");
		statsTable.AddColumn(new TableColumn("Value").RightAligned());

		statsTable.AddRow("Original size", $"{result.OriginalSize:N0} bytes");
		statsTable.AddRow("Reassembled size", $"{result.ReassembledSize:N0} bytes");
		statsTable.AddRow("Bytes matching", $"{result.ByteMatches:N0}");
		statsTable.AddRow("Bytes different", result.ByteDifferences > 0
			? $"[red]{result.ByteDifferences:N0}[/]"
			: $"{result.ByteDifferences:N0}");
		statsTable.AddRow("Match percentage", result.MatchPercentage >= 100
			? $"[green]{result.MatchPercentage:F2}%[/]"
			: $"[yellow]{result.MatchPercentage:F2}%[/]");

		AnsiConsole.Write(statsTable);

		// Show differences if any
		if (result.Differences.Count > 0) {
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine("[yellow]First differences:[/]");

			var diffTable = new Table();
			diffTable.AddColumn("Offset");
			diffTable.AddColumn("Address");
			diffTable.AddColumn("Original");
			diffTable.AddColumn("Reassembled");

			foreach (var diff in result.Differences.Take(10)) {
				diffTable.AddRow(
					$"0x{diff.Offset:x6}",
					$"${diff.Address:x4}",
					$"0x{diff.Original:x2}",
					$"0x{diff.Reassembled:x2}"
				);
			}

			AnsiConsole.Write(diffTable);

			if (result.Differences.Count > 10) {
				AnsiConsole.MarkupLine($"[grey]... and {result.Differences.Count - 10} more differences[/]");
			}
		}

		// Write report file if requested
		if (report != null) {
			var reportContent = RoundtripVerifier.GenerateReport(result);
			File.WriteAllText(report.FullName, reportContent);
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine($"[grey]Report written to:[/] {Markup.Escape(report.FullName)}");
		}

		Environment.Exit(result.Success ? 0 : 1);
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
}, verifyOriginalArg, verifyReassembledOpt, verifyWorkdirOpt, verifyAssemblerOpt, verifyReportOpt);

rootCommand.AddCommand(verifyCommand);

// ============================================================================
// Asset Pipeline Commands
// ============================================================================

// CHR command - Extract and convert graphics
var chrCommand = new Command("chr", "Extract and convert CHR/tile graphics from ROM");
var chrRomArg = new Argument<FileInfo>("rom", "ROM file to extract graphics from");
var chrOutputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file (default: rom_chr.bmp)");
var chrOffsetOpt = new Option<string>(["--offset"], () => "0", "ROM offset to start (hex with $ or 0x prefix)");
var chrSizeOpt = new Option<string?>(["--size", "-s"], "Size in bytes to extract (default: auto-detect)");
var chrBitsOpt = new Option<int>(["--bits", "-b"], () => 2, "Bits per pixel (2 for NES, 4 for SNES)");
var chrTilesPerRowOpt = new Option<int>(["--tiles-per-row", "-t"], () => 16, "Tiles per row in output image");
var chrPaletteOpt = new Option<string?>(["--palette", "-p"], "Palette: grayscale, nes, or custom hex values");
var chrPlatformOpt = new Option<string?>(["--platform"], "Platform hint for auto-detection");

chrCommand.AddArgument(chrRomArg);
chrCommand.AddOption(chrOutputOpt);
chrCommand.AddOption(chrOffsetOpt);
chrCommand.AddOption(chrSizeOpt);
chrCommand.AddOption(chrBitsOpt);
chrCommand.AddOption(chrTilesPerRowOpt);
chrCommand.AddOption(chrPaletteOpt);
chrCommand.AddOption(chrPlatformOpt);

chrCommand.SetHandler((rom, output, offsetStr, sizeStr, bits, tilesPerRow, paletteStr, platform) => {
	try {
		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony CHR Extractor[/]");
		AnsiConsole.WriteLine();

		// Load ROM
		AnsiConsole.MarkupLine($"[grey]Loading:[/] {Markup.Escape(rom.FullName)}");
		var romData = RomLoader.Load(rom.FullName);

		// Parse offset
		int offset = ParseHexOrDecimal(offsetStr);
		AnsiConsole.MarkupLine($"[grey]Offset:[/] ${offset:x6}");

		// Determine size
		int size;
		if (!string.IsNullOrEmpty(sizeStr)) {
			size = ParseHexOrDecimal(sizeStr);
		} else {
			// Auto-detect based on platform
			platform ??= RomLoader.DetectPlatform(romData, rom.FullName);
			size = platform?.ToLowerInvariant() switch {
				"nes" => romData.Length >= 0x4010 ? 0x2000 : romData.Length - offset, // Default 8KB CHR
				"snes" => Math.Min(0x4000, romData.Length - offset), // 16KB default
				"gb" or "gameboy" => Math.Min(0x2000, romData.Length - offset),
				_ => Math.Min(0x2000, romData.Length - offset)
			};
		}
		AnsiConsole.MarkupLine($"[grey]Size:[/] ${size:x4} ({size} bytes)");

		// Calculate tile count
		int bytesPerTile = bits switch {
			2 => 16,
			4 => 32,
			8 => 64,
			_ => 16
		};
		int tileCount = size / bytesPerTile;
		AnsiConsole.MarkupLine($"[grey]Tiles:[/] {tileCount} ({bits}bpp)");

		// Extract tiles
		var slice = romData.AsSpan(offset, size);
		byte[] pixels = bits switch {
			2 => TileGraphics.Decode2bppPlanar(slice, tileCount),
			4 => TileGraphics.Decode4bppPlanar(slice, tileCount),
			_ => TileGraphics.Decode2bppPlanar(slice, tileCount)
		};

		// Get palette
		uint[] palette = (paletteStr?.ToLowerInvariant()) switch {
			null or "grayscale" or "gray" => TileGraphics.NesGrayscale,
			"nes" => TileGraphics.NesPalette,
			_ => ParseCustomPalette(paletteStr) ?? TileGraphics.NesGrayscale
		};

		// Generate image
		var bmpData = TileGraphics.ExportTilesToBmp(pixels, tileCount, tilesPerRow, palette);

		// Write output
		var outputPath = output?.FullName ?? Path.Combine(
			rom.DirectoryName!,
			Path.GetFileNameWithoutExtension(rom.Name) + "_chr.bmp");

		File.WriteAllBytes(outputPath, bmpData);

		int imageWidth = tilesPerRow * 8;
		int imageHeight = ((tileCount + tilesPerRow - 1) / tilesPerRow) * 8;

		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[green]✓ Exported {tileCount} tiles ({imageWidth}x{imageHeight})[/]");
		AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(outputPath)}");
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
}, chrRomArg, chrOutputOpt, chrOffsetOpt, chrSizeOpt, chrBitsOpt, chrTilesPerRowOpt, chrPaletteOpt, chrPlatformOpt);

rootCommand.AddCommand(chrCommand);

// Text command - Extract text using table files
var textCommand = new Command("text", "Extract text from ROM using table file");
var textRomArg = new Argument<FileInfo>("rom", "ROM file to extract text from");
var textTableOpt = new Option<FileInfo?>(["--table", "-t"], "Table file (.tbl) for character mapping");
var textOutputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file (default: stdout)");
var textOffsetOpt = new Option<string>(["--offset"], "ROM offset to start extraction");
var textLengthOpt = new Option<int>(["--length", "-l"], () => 256, "Maximum length to extract");
var textEndBytesOpt = new Option<string?>(["--end", "-e"], "End byte(s) in hex (e.g., 'ff' or '00,ff')");
var textFormatOpt = new Option<string>(["--format", "-f"], () => "text", "Output format: text, json, script");
var textPointerOpt = new Option<string?>(["--pointer-table", "-p"], "Extract from pointer table at offset");
var textPointerCountOpt = new Option<int>(["--pointer-count", "-c"], () => 0, "Number of pointers in table");
var textMinLengthOpt = new Option<int>(["--min-length"], () => 3, "Minimum text length for scanning");
var textScanOpt = new Option<bool>(["--scan"], "Scan ROM for text instead of fixed offset");

textCommand.AddArgument(textRomArg);
textCommand.AddOption(textTableOpt);
textCommand.AddOption(textOutputOpt);
textCommand.AddOption(textOffsetOpt);
textCommand.AddOption(textLengthOpt);
textCommand.AddOption(textEndBytesOpt);
textCommand.AddOption(textFormatOpt);
textCommand.AddOption(textPointerOpt);
textCommand.AddOption(textPointerCountOpt);
textCommand.AddOption(textMinLengthOpt);
textCommand.AddOption(textScanOpt);

textCommand.SetHandler((context) => {
	try {
		var rom = context.ParseResult.GetValueForArgument(textRomArg);
		var tableFile = context.ParseResult.GetValueForOption(textTableOpt);
		var output = context.ParseResult.GetValueForOption(textOutputOpt);
		var offsetStr = context.ParseResult.GetValueForOption(textOffsetOpt);
		var length = context.ParseResult.GetValueForOption(textLengthOpt);
		var endBytesStr = context.ParseResult.GetValueForOption(textEndBytesOpt);
		var format = context.ParseResult.GetValueForOption(textFormatOpt);
		var pointerStr = context.ParseResult.GetValueForOption(textPointerOpt);
		var pointerCount = context.ParseResult.GetValueForOption(textPointerCountOpt);
		var minLength = context.ParseResult.GetValueForOption(textMinLengthOpt);
		var scan = context.ParseResult.GetValueForOption(textScanOpt);

		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Text Extractor[/]");
		AnsiConsole.WriteLine();

		// Load ROM
		AnsiConsole.MarkupLine($"[grey]Loading:[/] {Markup.Escape(rom.FullName)}");
		var romData = RomLoader.Load(rom.FullName);

		// Load or create table
		TableFile table;
		if (tableFile?.Exists == true) {
			var content = File.ReadAllText(tableFile.FullName);
			table = TableFile.LoadFromTbl(content);
			AnsiConsole.MarkupLine($"[grey]Table:[/] {tableFile.Name} ({table.ByteMappings.Count} entries)");
		} else {
			table = TableFile.CreateAsciiTable();
			AnsiConsole.MarkupLine("[grey]Table:[/] ASCII (default)");
		}

		// Parse end byte (single byte)
		byte? endByte = null;
		if (!string.IsNullOrEmpty(endBytesStr)) {
			endByte = (byte)ParseHexOrDecimal(endBytesStr.Split(',')[0].Trim());
		}

		var options = new TextExtractionOptions {
			EndByte = endByte,
			MinLength = minLength,
			MaxLength = length
		};

		List<TextBlock> blocks;

		if (scan) {
			// Scan entire ROM for text
			AnsiConsole.MarkupLine("[cyan]Scanning ROM for text...[/]");
			blocks = TextExtraction.ScanForText(romData, table, options);
		} else if (!string.IsNullOrEmpty(pointerStr) && pointerCount > 0) {
			// Extract from pointer table
			int ptrOffset = ParseHexOrDecimal(pointerStr);
			AnsiConsole.MarkupLine($"[grey]Pointer table:[/] ${ptrOffset:x6} ({pointerCount} pointers)");
			// Use 0 as text bank offset (relative pointers)
			blocks = TextExtraction.ExtractFromPointerTable(romData, ptrOffset, pointerCount, 0, table, options);
		} else if (!string.IsNullOrEmpty(offsetStr)) {
			// Extract from fixed offset
			int offset = ParseHexOrDecimal(offsetStr);
			var text = TextExtraction.ExtractText(romData, offset, length, table, options);
			blocks = [new TextBlock { Offset = offset, Text = text, Length = text.Length, RawBytes = [] }];
		} else {
			AnsiConsole.MarkupLine("[yellow]No offset specified. Use --offset, --pointer-table, or --scan[/]");
			return;
		}

		AnsiConsole.MarkupLine($"[green]Found {blocks.Count} text block(s)[/]");
		AnsiConsole.WriteLine();

		// Output based on format
		if (output != null) {
			switch (format.ToLowerInvariant()) {
				case "json":
					TextExtraction.SaveAsJson(blocks, output.FullName);
					break;
				case "script":
					TextExtraction.SaveAsScript(blocks, output.FullName);
					break;
				default:
					using (var writer = new StreamWriter(output.FullName)) {
						foreach (var block in blocks) {
							writer.WriteLine($"; ${block.Offset:x6}");
							writer.WriteLine(block.Text);
							writer.WriteLine();
						}
					}
					break;
			}
			AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(output.FullName)}");
		} else {
			// Output to console
			foreach (var block in blocks.Take(20)) {
				AnsiConsole.MarkupLine($"[grey]${block.Offset:x6}:[/] {Markup.Escape(block.Text.Replace("\n", "\\n"))}");
			}
			if (blocks.Count > 20) {
				AnsiConsole.MarkupLine($"[grey]... and {blocks.Count - 20} more[/]");
			}
		}
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
});

rootCommand.AddCommand(textCommand);

// Palette command - Extract palettes
var paletteCommand = new Command("palette", "Extract palette data from ROM");
var paletteRomArg = new Argument<FileInfo>("rom", "ROM file to extract palette from");
var paletteOutputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file");
var paletteOffsetOpt = new Option<string>(["--offset"], () => "0", "ROM offset");
var paletteCountOpt = new Option<int>(["--count", "-c"], () => 16, "Number of colors");
var paletteFormatOpt = new Option<string>(["--format", "-f"], () => "nes", "Format: nes, snes, gb, gba, raw");
var paletteOutputFormatOpt = new Option<string>(["--output-format"], () => "json", "Output format: json, asm, hex");

paletteCommand.AddArgument(paletteRomArg);
paletteCommand.AddOption(paletteOutputOpt);
paletteCommand.AddOption(paletteOffsetOpt);
paletteCommand.AddOption(paletteCountOpt);
paletteCommand.AddOption(paletteFormatOpt);
paletteCommand.AddOption(paletteOutputFormatOpt);

paletteCommand.SetHandler((rom, output, offsetStr, count, format, outputFormat) => {
	try {
		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Palette Extractor[/]");
		AnsiConsole.WriteLine();

		var romData = RomLoader.Load(rom.FullName);
		int offset = ParseHexOrDecimal(offsetStr);

		AnsiConsole.MarkupLine($"[grey]Offset:[/] ${offset:x6}");
		AnsiConsole.MarkupLine($"[grey]Colors:[/] {count}");
		AnsiConsole.MarkupLine($"[grey]Format:[/] {format}");

		// Extract palette based on format
		var colors = new uint[count];
		var slice = romData.AsSpan(offset);

		for (int i = 0; i < count; i++) {
			colors[i] = format.ToLowerInvariant() switch {
				"nes" => i < slice.Length ? TileGraphics.NesPalette[slice[i] & 0x3f] : 0xff000000,
				"snes" => ConvertSnesColor(slice, i * 2),
				"gba" => ConvertGbaColor(slice, i * 2),
				"gb" or "gameboy" => ConvertGbColor(i < slice.Length ? slice[i] : (byte)0),
				_ => (uint)(0xff000000 | (i < slice.Length ? (slice[i] << 16 | slice[i] << 8 | slice[i]) : 0))
			};
		}

		// Display palette preview
		AnsiConsole.WriteLine();
		for (int row = 0; row < (count + 15) / 16; row++) {
			var line = new System.Text.StringBuilder();
			for (int col = 0; col < 16 && row * 16 + col < count; col++) {
				int idx = row * 16 + col;
				uint c = colors[idx];
				byte r = (byte)((c >> 16) & 0xff);
				byte g = (byte)((c >> 8) & 0xff);
				byte b = (byte)(c & 0xff);
				line.Append($"[rgb({r},{g},{b})]██[/]");
			}
			AnsiConsole.MarkupLine(line.ToString());
		}

		// Output
		if (output != null) {
			switch (outputFormat.ToLowerInvariant()) {
				case "json":
					var json = System.Text.Json.JsonSerializer.Serialize(new {
						offset = $"0x{offset:x6}",
						count,
						format,
						colors = colors.Select(c => $"#{c & 0xffffff:x6}").ToArray()
					}, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(output.FullName, json);
					break;
				case "asm":
					using (var writer = new StreamWriter(output.FullName)) {
						writer.WriteLine($"; Palette extracted from ${offset:x6}");
						writer.WriteLine($"; Format: {format}");
						writer.WriteLine();
						for (int i = 0; i < count; i++) {
							uint c = colors[i];
							writer.WriteLine($"\t.db ${(c >> 16) & 0xff:x2}, ${(c >> 8) & 0xff:x2}, ${c & 0xff:x2} ; Color {i}");
						}
					}
					break;
				default:
					File.WriteAllText(output.FullName, string.Join("\n", colors.Select(c => $"#{c & 0xffffff:x6}")));
					break;
			}
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(output.FullName)}");
		}
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
}, paletteRomArg, paletteOutputOpt, paletteOffsetOpt, paletteCountOpt, paletteFormatOpt, paletteOutputFormatOpt);

rootCommand.AddCommand(paletteCommand);

// TBL command - Generate or convert table files
var tblCommand = new Command("tbl", "Generate or convert text table files");
var tblOutputOpt = new Option<FileInfo?>(["--output", "-o"], "Output file");
var tblTemplateOpt = new Option<string>(["--template", "-t"], () => "ascii", "Template: ascii, sjis, pokemon, dw, ff, earthbound, zelda, metroid, castlevania, megaman");
var tblFromOpt = new Option<FileInfo?>(["--from", "-f"], "Convert from existing table file");
var tblToFormatOpt = new Option<string>(["--to-format"], () => "tbl", "Output format: tbl, json, asm");

tblCommand.AddOption(tblOutputOpt);
tblCommand.AddOption(tblTemplateOpt);
tblCommand.AddOption(tblFromOpt);
tblCommand.AddOption(tblToFormatOpt);

tblCommand.SetHandler((output, template, fromFile, toFormat) => {
	try {
		AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Table Generator[/]");
		AnsiConsole.WriteLine();

		TableFile table;

		if (fromFile?.Exists == true) {
			// Convert existing table
			var content = File.ReadAllText(fromFile.FullName);
			table = TableFile.LoadFromTbl(content);
			AnsiConsole.MarkupLine($"[grey]Loaded:[/] {fromFile.Name} ({table.ByteMappings.Count} entries)");
		} else {
			// Generate from template
			table = TableFile.GetTemplate(template);
			AnsiConsole.MarkupLine($"[grey]Template:[/] {template} → {table.Name} ({table.ByteMappings.Count} byte + {table.WordMappings.Count} word entries)");
		}

		if (output != null) {
			switch (toFormat.ToLowerInvariant()) {
				case "json":
					var json = System.Text.Json.JsonSerializer.Serialize(new {
						mappings = table.ByteMappings.ToDictionary(
							kvp => $"0x{kvp.Key:x2}",
							kvp => kvp.Value)
					}, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(output.FullName, json);
					break;
				case "asm":
					using (var writer = new StreamWriter(output.FullName)) {
						writer.WriteLine("; Text table (ASM format)");
						foreach (var kvp in table.ByteMappings.OrderBy(k => k.Key)) {
							var escaped = kvp.Value.Replace("\"", "\\\"");
							writer.WriteLine($".define CHAR_{kvp.Key:x2} = \"{escaped}\"");
						}
					}
					break;
				default: // tbl
					using (var writer = new StreamWriter(output.FullName)) {
						foreach (var kvp in table.ByteMappings.OrderBy(k => k.Key)) {
							writer.WriteLine($"{kvp.Key:X2}={kvp.Value}");
						}
					}
					break;
			}
			AnsiConsole.MarkupLine($"[green]Output:[/] {Markup.Escape(output.FullName)}");
		} else {
			// Preview to console
			AnsiConsole.MarkupLine("[grey]Sample mappings:[/]");
			foreach (var kvp in table.ByteMappings.Take(16)) {
				AnsiConsole.MarkupLine($"  ${kvp.Key:x2} = {Markup.Escape(kvp.Value)}");
			}
			if (table.ByteMappings.Count > 16) {
				AnsiConsole.MarkupLine($"  ... and {table.ByteMappings.Count - 16} more");
			}
		}
	}
	catch (Exception ex) {
		AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
		Environment.Exit(1);
	}
}, tblOutputOpt, tblTemplateOpt, tblFromOpt, tblToFormatOpt);

rootCommand.AddCommand(tblCommand);

// Version
rootCommand.SetHandler(() => {
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Disassembler v0.4.0[/]");
AnsiConsole.MarkupLine("Multi-system ROM disassembler with asset pipeline");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("Supported platforms:");
AnsiConsole.MarkupLine("  • Atari 2600 (6502)");
AnsiConsole.MarkupLine("  • NES (6502) with MMC1 multi-bank");
AnsiConsole.MarkupLine("  • SNES (65816)");
AnsiConsole.MarkupLine("  • Game Boy (SM83)");
AnsiConsole.MarkupLine("  • GBA (ARM7TDMI)");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("Asset pipeline commands:");
AnsiConsole.MarkupLine("  • chr     - Extract tile graphics");
AnsiConsole.MarkupLine("  • text    - Extract text with table files");
AnsiConsole.MarkupLine("  • palette - Extract color palettes");
AnsiConsole.MarkupLine("  • tbl     - Generate/convert table files");
});

return await rootCommand.InvokeAsync(args);

// ============================================================================
// Helper functions
// ============================================================================

static int ParseHexOrDecimal(string value) {
	value = value.Trim();
	if (value.StartsWith("$") || value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
		var hex = value.StartsWith("$") ? value[1..] : value[2..];
		return Convert.ToInt32(hex, 16);
	}
	return int.Parse(value);
}

static uint[]? ParseCustomPalette(string value) {
	try {
		var colors = value.Split(',', ';', ' ')
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Select(s => {
				var hex = s.Trim().TrimStart('#', '$');
				if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					hex = hex[2..];
				return 0xff000000 | (uint)Convert.ToInt32(hex, 16);
			})
			.ToArray();
		return colors.Length > 0 ? colors : null;
	}
	catch {
		return null;
	}
}

static uint ConvertSnesColor(ReadOnlySpan<byte> data, int offset) {
	if (offset + 2 > data.Length) return 0xff000000;
	ushort bgr = (ushort)(data[offset] | (data[offset + 1] << 8));
	byte r = (byte)((bgr & 0x1f) << 3);
	byte g = (byte)(((bgr >> 5) & 0x1f) << 3);
	byte b = (byte)(((bgr >> 10) & 0x1f) << 3);
	return 0xff000000 | ((uint)r << 16) | ((uint)g << 8) | b;
}

static uint ConvertGbaColor(ReadOnlySpan<byte> data, int offset) {
	// GBA uses same format as SNES (15-bit BGR)
	return ConvertSnesColor(data, offset);
}

static uint ConvertGbColor(byte value) {
	// Game Boy 2-bit grayscale
	byte gray = value switch {
		0 => 255,
		1 => 170,
		2 => 85,
		_ => 0
	};
	return 0xff000000 | ((uint)gray << 16) | ((uint)gray << 8) | gray;
}

// Helper methods
static string GetDefaultOutputPath(FileInfo rom, string format) {
var ext = ".pasm";
return Path.Combine(rom.DirectoryName!, Path.GetFileNameWithoutExtension(rom.Name) + ext);
}

static void WriteAsmOutput(TextWriter writer, FileInfo rom, RomInfo info, DisassemblyResult result) {
writer.WriteLine($"; 🌺 Peony Disassembly");
writer.WriteLine($"; ROM: {rom.Name}");
writer.WriteLine($"; Platform: {info.Platform}");
writer.WriteLine($"; Size: {info.Size} bytes");
	writer.WriteLine();
	writer.WriteLine($".system:{info.Platform}");
if (info.Mapper != null)
writer.WriteLine($"; Mapper: {info.Mapper}");
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

// Apply label substitution to operands
var content = FormatWithLabels(line.Content, result);
writer.WriteLine($"\t{content,-24} ; ${line.Address:x4}: {bytes}{comment}".TrimEnd());
}
writer.WriteLine();
}
}

static string FormatWithLabels(string instruction, DisassemblyResult result) {
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
