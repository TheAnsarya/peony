using System.CommandLine;
using Peony.Core;
using Peony.Cpu;
using Peony.Platform.Atari2600;
using Spectre.Console;

// 🌺 Peony Disassembler CLI

var rootCommand = new RootCommand("🌺 Peony - Multi-system ROM disassembler");

// Disasm command
var disasmCommand = new Command("disasm", "Disassemble a ROM file");
var romArg = new Argument<FileInfo>("rom", "ROM file to disassemble");
var outputOpt = new Option<FileInfo?>("--output", "Output file (default: stdout)");
var platformOpt = new Option<string?>("--platform", "Platform (auto-detected if not specified)");

disasmCommand.AddArgument(romArg);
disasmCommand.AddOption(outputOpt);
disasmCommand.AddOption(platformOpt);

disasmCommand.SetHandler((rom, output, platform) => {
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
IPlatformAnalyzer analyzer = platform switch {
"atari2600" => new Atari2600Analyzer(),
_ => throw new NotSupportedException($"Platform not supported: {platform}")
};

// Analyze ROM
var info = analyzer.Analyze(romData);
AnsiConsole.MarkupLine($"[grey]Mapper:[/] {info.Mapper ?? "None"}");
AnsiConsole.WriteLine();

// Get entry points
var entryPoints = analyzer switch {
Atari2600Analyzer a2600 => a2600.GetEntryPoints(romData),
_ => [(uint)0x1000]
};

AnsiConsole.MarkupLine($"[grey]Entry points:[/] {string.Join(", ", entryPoints.Select(e => $"${e:x4}"))}");

// Create engine and disassemble
var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);
var result = engine.Disassemble(romData, entryPoints);

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"[green]Disassembled {result.Blocks.Count} blocks[/]");

// Output
using var writer = output != null
? new StreamWriter(output.FullName)
: Console.Out;

writer.WriteLine($"; 🌺 Peony Disassembly");
writer.WriteLine($"; ROM: {rom.Name}");
writer.WriteLine($"; Platform: {info.Platform}");
writer.WriteLine($"; Size: {info.Size} bytes");
writer.WriteLine();

foreach (var block in result.Blocks.OrderBy(b => b.StartAddress)) {
writer.WriteLine($"; Block ${block.StartAddress:x4}-${block.EndAddress:x4} ({block.Type})");
foreach (var line in block.Lines) {
var label = line.Label != null ? $"{line.Label}:" : "";
var bytes = string.Join(" ", line.Bytes.Select(b => $"{b:x2}"));
var comment = line.Comment != null ? $"; {line.Comment}" : "";

if (!string.IsNullOrEmpty(label))
writer.WriteLine(label);

writer.WriteLine($"\t{line.Content,-20} ; ${line.Address:x4}: {bytes} {comment}".TrimEnd());
}
writer.WriteLine();
}

if (output != null)
AnsiConsole.MarkupLine($"[green]Output written to:[/] {Markup.Escape(output.FullName)}");
}
catch (Exception ex) {
AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
Environment.Exit(1);
}
}, romArg, outputOpt, platformOpt);

rootCommand.AddCommand(disasmCommand);

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
table.AddRow("Platform", platform);

if (platform == "atari2600") {
var analyzer = new Atari2600Analyzer();
var info = analyzer.Analyze(romData);
table.AddRow("Mapper", info.Mapper ?? "None");
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
AnsiConsole.MarkupLine("[bold magenta]🌺 Peony Disassembler v0.1.0[/]");
AnsiConsole.MarkupLine("Multi-system ROM disassembler");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("Supported platforms:");
AnsiConsole.MarkupLine("  • Atari 2600 (6502)");
AnsiConsole.MarkupLine("  • [grey]NES (planned)[/]");
AnsiConsole.MarkupLine("  • [grey]SNES (planned)[/]");
AnsiConsole.MarkupLine("  • [grey]Game Boy (planned)[/]");
});

return await rootCommand.InvokeAsync(args);
