using System.IO.Compression;

namespace Peony.Core;

/// <summary>
/// Result of loading a Nexen game pack (.nexen-pack.zip).
/// Contains resolved file paths for the ROM, CDL, Pansy metadata, and labels.
/// </summary>
public sealed class NexenPackResult {
	/// <summary>Path to the extracted ROM file.</summary>
	public required string RomPath { get; init; }

	/// <summary>Path to the extracted CDL file, or null if not present.</summary>
	public string? CdlPath { get; init; }

	/// <summary>Path to the extracted Pansy metadata file, or null if not present.</summary>
	public string? PansyPath { get; init; }

	/// <summary>Path to the extracted labels file, or null if not present.</summary>
	public string? LabelsPath { get; init; }

	/// <summary>Game name parsed from the manifest.</summary>
	public required string GameName { get; init; }

	/// <summary>Console/system type (e.g., "Nes", "Snes", "Gameboy").</summary>
	public string? System { get; init; }

	/// <summary>ROM CRC32 from the manifest (lowercase hex, no prefix).</summary>
	public string? RomCrc32 { get; init; }

	/// <summary>Original ROM filename from the manifest.</summary>
	public string? RomFileName { get; init; }

	/// <summary>Root directory where the pack was extracted.</summary>
	public required string ExtractDirectory { get; init; }

	/// <summary>All parsed manifest key-value pairs.</summary>
	public IReadOnlyDictionary<string, string> Manifest { get; init; } = new Dictionary<string, string>();
}

/// <summary>
/// Loads Nexen game pack archives (.nexen-pack.zip) exported by the Nexen emulator.
/// Extracts the archive and locates key files (ROM, CDL, Pansy metadata, labels).
/// </summary>
public static class NexenPackLoader {
	/// <summary>File extension for Nexen game pack archives.</summary>
	public const string PackageExtension = ".nexen-pack.zip";

	private static readonly string[] RomExtensions = [
		".nes", ".sfc", ".smc", ".gb", ".gbc", ".gba",
		".sms", ".gg", ".pce", ".ws", ".wsc", ".lnx", ".a26"
	];

	private static readonly string[] CdlExtensions = [".cdl"];
	private static readonly string[] PansyExtensions = [".pansy"];
	private static readonly string[] LabelExtensions = [".nexen-labels", ".mlb", ".nl"];

	/// <summary>
	/// Load and extract a Nexen game pack from a zip file path.
	/// </summary>
	/// <param name="zipPath">Path to the .nexen-pack.zip file.</param>
	/// <param name="outputDirectory">
	/// Directory to extract files into. If null, extracts to a subdirectory
	/// next to the zip file named after the game.
	/// </param>
	/// <returns>The pack result with paths to key files.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the zip file doesn't exist.</exception>
	/// <exception cref="InvalidDataException">Thrown if the archive has no ROM file.</exception>
	public static NexenPackResult Load(string zipPath, string? outputDirectory = null) {
		if (!File.Exists(zipPath))
			throw new FileNotFoundException("Nexen pack not found.", zipPath);

		outputDirectory ??= Path.Combine(
			Path.GetDirectoryName(zipPath) ?? ".",
			Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(zipPath)));

		using var stream = File.OpenRead(zipPath);
		return Load(stream, outputDirectory);
	}

	/// <summary>
	/// Load and extract a Nexen game pack from a stream.
	/// </summary>
	/// <param name="zipStream">Stream containing the zip archive.</param>
	/// <param name="outputDirectory">Directory to extract files into.</param>
	/// <returns>The pack result with paths to key files.</returns>
	/// <exception cref="InvalidDataException">Thrown if the archive has no ROM file.</exception>
	public static NexenPackResult Load(Stream zipStream, string outputDirectory) {
		Directory.CreateDirectory(outputDirectory);

		using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

		// Find the root prefix (all entries are under {GameName}/)
		string? archivePrefix = DetectArchivePrefix(archive);

		// Extract all entries
		var extractedFiles = new List<string>();
		foreach (var entry in archive.Entries) {
			if (string.IsNullOrEmpty(entry.Name))
				continue; // Skip directory entries

			string relativePath = archivePrefix is not null && entry.FullName.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase)
				? entry.FullName[archivePrefix.Length..]
				: entry.FullName;

			// Sanitize path to prevent directory traversal
			relativePath = SanitizePath(relativePath);
			if (string.IsNullOrEmpty(relativePath))
				continue;

			string destPath = Path.GetFullPath(Path.Combine(outputDirectory, relativePath));

			// Verify the destination is within the output directory
			if (!destPath.StartsWith(Path.GetFullPath(outputDirectory), StringComparison.OrdinalIgnoreCase))
				continue;

			string? destDir = Path.GetDirectoryName(destPath);
			if (destDir is not null)
				Directory.CreateDirectory(destDir);

			entry.ExtractToFile(destPath, overwrite: true);
			extractedFiles.Add(relativePath);
		}

		// Parse manifest
		var manifest = ParseManifest(Path.Combine(outputDirectory, "manifest.txt"));

		// Locate key files
		string? romPath = FindFileByFolder(outputDirectory, "ROM", RomExtensions)
			?? FindFileByExtension(extractedFiles, outputDirectory, RomExtensions);
		string? cdlPath = FindFileByFolder(outputDirectory, "Debug", CdlExtensions)
			?? FindFileByExtension(extractedFiles, outputDirectory, CdlExtensions);
		string? pansyPath = FindFileByFolder(outputDirectory, "Debug", PansyExtensions)
			?? FindFileByExtension(extractedFiles, outputDirectory, PansyExtensions);
		string? labelsPath = FindFileByFolder(outputDirectory, "Debug", LabelExtensions)
			?? FindFileByExtension(extractedFiles, outputDirectory, LabelExtensions);

		if (romPath is null)
			throw new InvalidDataException("No ROM file found in Nexen pack.");

		string gameName = manifest.GetValueOrDefault("ROM", "")
			?? archivePrefix?.TrimEnd('/') ?? Path.GetFileNameWithoutExtension(romPath);

		return new NexenPackResult {
			RomPath = romPath,
			CdlPath = cdlPath,
			PansyPath = pansyPath,
			LabelsPath = labelsPath,
			GameName = gameName,
			System = manifest.GetValueOrDefault("System"),
			RomCrc32 = manifest.GetValueOrDefault("ROM CRC32"),
			RomFileName = manifest.GetValueOrDefault("ROM File"),
			ExtractDirectory = outputDirectory,
			Manifest = manifest
		};
	}

	/// <summary>
	/// Try to load a Nexen pack, returning success/failure without throwing.
	/// </summary>
	public static bool TryLoad(string zipPath, out NexenPackResult? result, out string? error, string? outputDirectory = null) {
		try {
			result = Load(zipPath, outputDirectory);
			error = null;
			return true;
		} catch (Exception ex) {
			result = null;
			error = ex.Message;
			return false;
		}
	}

	/// <summary>
	/// Detect the common prefix directory in the archive (e.g., "GameName/").
	/// </summary>
	private static string? DetectArchivePrefix(ZipArchive archive) {
		string? prefix = null;
		foreach (var entry in archive.Entries) {
			if (string.IsNullOrEmpty(entry.Name))
				continue;

			int slashIndex = entry.FullName.IndexOf('/');
			if (slashIndex < 0)
				return null; // Entry at root level — no common prefix

			string candidate = entry.FullName[..(slashIndex + 1)];
			if (prefix is null)
				prefix = candidate;
			else if (!string.Equals(prefix, candidate, StringComparison.OrdinalIgnoreCase))
				return null; // Different prefixes — no common prefix
		}
		return prefix;
	}

	/// <summary>
	/// Parse the manifest.txt file into key-value pairs.
	/// </summary>
	private static Dictionary<string, string> ParseManifest(string manifestPath) {
		var manifest = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		if (!File.Exists(manifestPath))
			return manifest;

		foreach (string line in File.ReadLines(manifestPath)) {
			string trimmed = line.Trim();
			if (string.IsNullOrEmpty(trimmed))
				continue;

			// Skip header line and file list
			if (trimmed.Equals("Nexen Game Package", StringComparison.OrdinalIgnoreCase))
				continue;
			if (trimmed.StartsWith("Files (", StringComparison.OrdinalIgnoreCase))
				break; // Stop at file listing

			int colonIndex = trimmed.IndexOf(':');
			if (colonIndex > 0 && colonIndex < trimmed.Length - 1) {
				string key = trimmed[..colonIndex].Trim();
				string value = trimmed[(colonIndex + 1)..].Trim();
				manifest.TryAdd(key, value);
			}
		}

		return manifest;
	}

	/// <summary>
	/// Find a file in a specific subfolder by extension.
	/// </summary>
	private static string? FindFileByFolder(string outputDir, string subfolder, string[] extensions) {
		string folderPath = Path.Combine(outputDir, subfolder);
		if (!Directory.Exists(folderPath))
			return null;

		foreach (string file in Directory.GetFiles(folderPath)) {
			string ext = Path.GetExtension(file);
			if (extensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase)))
				return file;
		}
		return null;
	}

	/// <summary>
	/// Find a file anywhere in extracted files by extension.
	/// </summary>
	private static string? FindFileByExtension(List<string> extractedFiles, string outputDir, string[] extensions) {
		foreach (string relativePath in extractedFiles) {
			string ext = Path.GetExtension(relativePath);
			if (extensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase)))
				return Path.Combine(outputDir, relativePath);
		}
		return null;
	}

	/// <summary>
	/// Sanitize a path to prevent directory traversal attacks.
	/// </summary>
	private static string SanitizePath(string path) {
		// Remove leading slashes and any .. components
		string sanitized = path.Replace('\\', '/').TrimStart('/');
		var parts = sanitized.Split('/');
		var safe = new List<string>();
		foreach (string part in parts) {
			if (part == ".." || part == ".")
				continue;
			if (!string.IsNullOrWhiteSpace(part))
				safe.Add(part);
		}
		return Path.Combine(safe.ToArray());
	}
}
