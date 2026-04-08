namespace Peony.Core;

/// <summary>
/// Per-platform output generator for producing .pasm source files.
/// Each platform implements platform-specific directives, bank layout, and formatting.
/// </summary>
public interface IOutputGenerator {
	/// <summary>Generator name (e.g., "SNES Poppy")</summary>
	string Name { get; }

	/// <summary>Output file extension (e.g., ".pasm")</summary>
	string Extension { get; }

	/// <summary>Generate output files from disassembly result</summary>
	void Generate(DisassemblyResult result, string outputPath);

	/// <summary>Format disassembly result as a string (single file output)</summary>
	string Format(DisassemblyResult result);
}
