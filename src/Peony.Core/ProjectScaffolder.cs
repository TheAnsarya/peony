using System.Text.Json;

namespace Peony.Core;

/// <summary>
/// Options for project scaffolding.
/// </summary>
public sealed class ScaffoldOptions {
	/// <summary>Whether to overwrite an existing project directory.</summary>
	public bool Force { get; init; }

	/// <summary>Original pack zip path for source provenance tracking.</summary>
	public string? PackZipPath { get; init; }
}

/// <summary>
/// Result of scaffolding a project directory.
/// </summary>
public sealed class ScaffoldResult {
	/// <summary>Root project directory path.</summary>
	public required string ProjectDirectory { get; init; }

	/// <summary>Path to the generated peony.json file.</summary>
	public required string PeonyJsonPath { get; init; }

	/// <summary>Path to the ROM file in the project.</summary>
	public required string RomPath { get; init; }

	/// <summary>Path to the generated README.md.</summary>
	public required string ReadmePath { get; init; }

	/// <summary>Number of metadata files copied.</summary>
	public int MetadataFileCount { get; init; }
}

/// <summary>
/// Creates organized disassembly project directories from NexenPackResult data.
/// Generates the standard project structure with rom/, source/, metadata/, output/
/// folders and a peony.json configuration file.
/// </summary>
public static class ProjectScaffolder {
	private static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// Create a project directory from a NexenPackResult.
	/// </summary>
	/// <param name="pack">The loaded Nexen pack result.</param>
	/// <param name="projectDir">Directory to create the project in.</param>
	/// <param name="options">Scaffolding options.</param>
	/// <returns>Result with paths to created files.</returns>
	/// <exception cref="InvalidOperationException">Thrown if directory exists and Force is false.</exception>
	public static ScaffoldResult Scaffold(NexenPackResult pack, string projectDir, ScaffoldOptions? options = null) {
		options ??= new ScaffoldOptions();

		if (Directory.Exists(projectDir) && !options.Force) {
			// Check if it's non-empty
			if (Directory.EnumerateFileSystemEntries(projectDir).Any())
				throw new InvalidOperationException($"Project directory already exists and is not empty: {projectDir}. Use Force option to overwrite.");
		}

		// Create directory structure
		string romDir = Path.Combine(projectDir, "rom");
		string sourceDir = Path.Combine(projectDir, "source");
		string metadataDir = Path.Combine(projectDir, "metadata");
		string outputDir = Path.Combine(projectDir, "output");

		Directory.CreateDirectory(romDir);
		Directory.CreateDirectory(sourceDir);
		Directory.CreateDirectory(metadataDir);
		Directory.CreateDirectory(outputDir);

		// Copy ROM
		string romFileName = Path.GetFileName(pack.RomPath);
		string destRomPath = Path.Combine(romDir, romFileName);
		File.Copy(pack.RomPath, destRomPath, overwrite: true);

		// Copy metadata files
		int metadataCount = 0;

		string? cdlRelative = null;
		if (pack.CdlPath is not null && File.Exists(pack.CdlPath)) {
			string cdlFileName = Path.GetFileName(pack.CdlPath);
			File.Copy(pack.CdlPath, Path.Combine(metadataDir, cdlFileName), overwrite: true);
			cdlRelative = "metadata/" + cdlFileName;
			metadataCount++;
		}

		string? pansyRelative = null;
		if (pack.PansyPath is not null && File.Exists(pack.PansyPath)) {
			string pansyFileName = Path.GetFileName(pack.PansyPath);
			File.Copy(pack.PansyPath, Path.Combine(metadataDir, pansyFileName), overwrite: true);
			pansyRelative = "metadata/" + pansyFileName;
			metadataCount++;
		}

		if (pack.LabelsPath is not null && File.Exists(pack.LabelsPath)) {
			string labelsFileName = Path.GetFileName(pack.LabelsPath);
			File.Copy(pack.LabelsPath, Path.Combine(metadataDir, labelsFileName), overwrite: true);
			metadataCount++;
		}

		// Compute ROM size and CRC32 info
		var romInfo = new FileInfo(destRomPath);
		string romCrc32 = pack.RomCrc32 ?? "";

		// Determine platform from system string
		string platform = NormalizePlatform(pack.System);

		// Generate peony.json
		string peonyJsonPath = Path.Combine(projectDir, "peony.json");
		var peonyJson = BuildPeonyJson(
			platform: platform,
			romPath: "rom/" + romFileName,
			romCrc32: romCrc32,
			romSize: romInfo.Length,
			cdlPath: cdlRelative,
			pansyPath: pansyRelative,
			packZipPath: options.PackZipPath);

		File.WriteAllText(peonyJsonPath, peonyJson);

		// Generate README.md
		string readmePath = Path.Combine(projectDir, "README.md");
		string readmeContent = BuildReadme(pack.GameName, platform, romFileName);
		File.WriteAllText(readmePath, readmeContent);

		return new ScaffoldResult {
			ProjectDirectory = projectDir,
			PeonyJsonPath = peonyJsonPath,
			RomPath = destRomPath,
			ReadmePath = readmePath,
			MetadataFileCount = metadataCount
		};
	}

	/// <summary>
	/// Normalize a system string from Nexen to a platform identifier.
	/// </summary>
	public static string NormalizePlatform(string? system) {
		if (string.IsNullOrEmpty(system))
			return "unknown";

		return PlatformResolver.Resolve(system)?.PoppyPlatformId
			?? system.ToLowerInvariant();
	}

	private static string BuildPeonyJson(
		string platform,
		string romPath,
		string romCrc32,
		long romSize,
		string? cdlPath,
		string? pansyPath,
		string? packZipPath) {

		var obj = new Dictionary<string, object> {
			["version"] = "1.0",
			["platform"] = platform,
			["rom"] = new Dictionary<string, object> {
				["path"] = romPath,
				["crc32"] = romCrc32,
				["size"] = romSize
			}
		};

		var metadata = new Dictionary<string, object>();
		if (cdlPath is not null)
			metadata["cdl"] = cdlPath;
		if (pansyPath is not null)
			metadata["pansy"] = pansyPath;
		if (metadata.Count > 0)
			obj["metadata"] = metadata;

		obj["output"] = new Dictionary<string, object> {
			["format"] = "poppy",
			["directory"] = "source/",
			["splitBanks"] = false
		};

		var source = new Dictionary<string, object> {
			["importDate"] = DateTime.UtcNow.ToString("o")
		};
		if (packZipPath is not null)
			source["nexenPack"] = Path.GetFileName(packZipPath);
		obj["source"] = source;

		return JsonSerializer.Serialize(obj, JsonOptions);
	}

	private static string BuildReadme(string gameName, string platform, string romFileName) {
		return $"""
			# {gameName} — Disassembly Project

			> Generated by Peony from a Nexen game pack

			## Info

			- **Platform:** {platform}
			- **ROM:** {romFileName}

			## Directory Structure

			```
			rom/        — Original ROM file
			source/     — Disassembled .pasm source files
			metadata/   — CDL, Pansy, and label files
			output/     — Assembled ROM output
			```

			## Build

			```bash
			# Assemble with Poppy (auto-verifies against original ROM)
			poppy source/main.pasm -o output/{Path.GetFileNameWithoutExtension(romFileName)}.{GetRomExtension(platform)}
			```

			## Roundtrip Verification

			Poppy automatically detects `peony.json` and verifies the assembled ROM
			matches the original byte-for-byte. Disable with `--no-verify`.
			""";
	}

	private static string GetRomExtension(string platform) {
		return PlatformResolver.Resolve(platform)?.DefaultRomExtension
			?? "bin";
	}
}
