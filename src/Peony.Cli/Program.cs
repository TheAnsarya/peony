using System.CommandLine;
using Peony.Core;
using Peony.Platform.Atari2600;
using Peony.Platform.NES;
using Spectre.Console;

// 🌺 Peony Disassembler CLI

var rootCommand = new RootCommand("🌺 Peony - Multi-system ROM disassembler");

// Disasm command
var disasmCommand = new Command("disasm", "Disassemble a ROM file");
var romArg = new Argument<FileInfo>("rom", "ROM file to disassemble");
var outputOpt = new Option<FileInfo?>("--output", "Output file (default: stdout)");
var platformOpt = new Option<string?>("--platform", "Platform (auto-detected if not specified)");
var formatOpt = new Option<string>("--format", () => "asm", "Output format: asm, poppy");
var allBanksOpt = new Option<bool>("--all-banks", "Disassemble all banks for banked ROMs (MMC1, etc.)");
var symbolsOpt = new Option<FileInfo?>("--symbols", "Symbol file to load (JSON, .nl, .mlb, .sym)");

disasmCommand.AddArgument(romArg);
disasmCommand.AddOption(outputOpt);
disasmCommand.AddOption(platformOpt);
disasmCommand.AddOption(formatOpt);
disasmCommand.AddOption(allBanksOpt);
disasmCommand.AddOption(symbolsOpt);

disasmCommand.SetHandler((rom, output, platform, format, allBanks, symbols) => {
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
}, romArg, outputOpt, platformOpt, formatOpt, allBanksOpt, symbolsOpt);

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

var ext = format == "poppy" ? ".pasm" : ".asm";
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

// Version
rootCommand.SetHandler(() => {
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Disassembler v0.3.0[/]");
AnsiConsole.MarkupLine("Multi-system ROM disassembler with multi-bank support");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("Supported platforms:");
AnsiConsole.MarkupLine("  • Atari 2600 (6502)");
AnsiConsole.MarkupLine("  • NES (6502) with MMC1 multi-bank");
AnsiConsole.MarkupLine("  • [grey]SNES (planned)[/]");
AnsiConsole.MarkupLine("  • [grey]Game Boy (planned)[/]");
});

return await rootCommand.InvokeAsync(args);

// Helper methods
static string GetDefaultOutputPath(FileInfo rom, string format) {
var ext = format == "poppy" ? ".pasm" : ".asm";
return Path.Combine(rom.DirectoryName!, Path.GetFileNameWithoutExtension(rom.Name) + ext);
}

static void WriteAsmOutput(TextWriter writer, FileInfo rom, RomInfo info, DisassemblyResult result) {
writer.WriteLine($"; 🌺 Peony Disassembly");
writer.WriteLine($"; ROM: {rom.Name}");
writer.WriteLine($"; Platform: {info.Platform}");
writer.WriteLine($"; Size: {info.Size} bytes");
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
