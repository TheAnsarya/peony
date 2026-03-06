using System.IO.Compression;
using Peony.Core;
using StreamHash.Core;
using Xunit;

namespace Peony.Core.Tests;

public class NexenPackLoaderTests : IDisposable {
	private readonly string _tempDir;

	public NexenPackLoaderTests() {
		_tempDir = Path.Combine(Path.GetTempPath(), "peony-test-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(_tempDir);
	}

	public void Dispose() {
		if (Directory.Exists(_tempDir))
			Directory.Delete(_tempDir, true);
	}

	private string CreateTestPack(
		string gameName = "TestGame",
		string? romFileName = "TestGame.nes",
		byte[]? romData = null,
		string? cdlFileName = null,
		byte[]? cdlData = null,
		string? pansyFileName = null,
		byte[]? pansyData = null,
		string? labelsFileName = null,
		byte[]? labelsData = null,
		string? manifestContent = null,
		bool includeManifest = true,
		string[]? saveStateFiles = null,
		string[]? movieFiles = null,
		string[]? saveFiles = null,
		string[]? cheatFiles = null,
		string? debugWorkspaceFile = null) {

		romData ??= new byte[] { 0x4e, 0x45, 0x53, 0x1a, 0x02, 0x01, 0x00, 0x00 };
		string zipPath = Path.Combine(_tempDir, $"{gameName}.nexen-pack.zip");

		using var stream = new FileStream(zipPath, FileMode.Create);
		using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
		string prefix = gameName + "/";

		if (romFileName is not null) {
			var romEntry = archive.CreateEntry(prefix + "ROM/" + romFileName);
			using var romStream = romEntry.Open();
			romStream.Write(romData);
		}

		if (cdlFileName is not null) {
			var cdlEntry = archive.CreateEntry(prefix + "Debug/" + cdlFileName);
			using var cdlStream = cdlEntry.Open();
			cdlStream.Write(cdlData ?? [0x43, 0x44, 0x4c, 0x01, 0x01, 0x02]);
		}

		if (pansyFileName is not null) {
			var pansyEntry = archive.CreateEntry(prefix + "Debug/" + pansyFileName);
			using var pansyStream = pansyEntry.Open();
			pansyStream.Write(pansyData ?? [0x50, 0x41, 0x4e, 0x53, 0x59, 0x00, 0x00, 0x00]);
		}

		if (labelsFileName is not null) {
			var labelsEntry = archive.CreateEntry(prefix + "Debug/" + labelsFileName);
			using var labelsStream = labelsEntry.Open();
			labelsStream.Write(labelsData ?? System.Text.Encoding.UTF8.GetBytes("$8000=reset\n"));
		}

		if (saveStateFiles is not null) {
			foreach (var ssFile in saveStateFiles) {
				var ssEntry = archive.CreateEntry(prefix + "SaveStates/" + ssFile);
				using var ssStream = ssEntry.Open();
				ssStream.Write([0x00, 0x01, 0x02, 0x03]);
			}
		}

		if (movieFiles is not null) {
			foreach (var mvFile in movieFiles) {
				var mvEntry = archive.CreateEntry(prefix + "Movies/" + mvFile);
				using var mvStream = mvEntry.Open();
				mvStream.Write([0x10, 0x20, 0x30]);
			}
		}

		if (saveFiles is not null) {
			foreach (var svFile in saveFiles) {
				var svEntry = archive.CreateEntry(prefix + "Saves/" + svFile);
				using var svStream = svEntry.Open();
				svStream.Write([0xaa, 0xbb]);
			}
		}

		if (cheatFiles is not null) {
			foreach (var ctFile in cheatFiles) {
				var ctEntry = archive.CreateEntry(prefix + "Config/" + ctFile);
				using var ctStream = ctEntry.Open();
				ctStream.Write(System.Text.Encoding.UTF8.GetBytes("{}"));
			}
		}

		if (debugWorkspaceFile is not null) {
			var dwEntry = archive.CreateEntry(prefix + "Config/" + debugWorkspaceFile);
			using var dwStream = dwEntry.Open();
			dwStream.Write(System.Text.Encoding.UTF8.GetBytes("{\"version\":1}"));
		}

		if (includeManifest) {
			manifestContent ??= $"""
				Nexen Game Package
				Created: 2026-01-15 10:30:00
				Emulator: Nexen v1.3.1
				ROM: {gameName}
				System: Nes
				ROM CRC32: a1b2c3d4
				ROM File: {romFileName ?? "TestGame.nes"}

				Files (1):
				  ROM/{romFileName ?? "TestGame.nes"}
				""";

			var manifestEntry = archive.CreateEntry(prefix + "manifest.txt");
			using var manifestStream = manifestEntry.Open();
			using var writer = new StreamWriter(manifestStream);
			writer.Write(manifestContent);
		}

		return zipPath;
	}

	[Fact]
	public void Load_ValidPack_ReturnsCorrectResult() {
		string zipPath = CreateTestPack(
			cdlFileName: "TestGame.cdl",
			pansyFileName: "TestGame.pansy",
			labelsFileName: "TestGame.nexen-labels");

		string outputDir = Path.Combine(_tempDir, "output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal("TestGame", result.GameName);
		Assert.Equal("Nes", result.System);
		Assert.Equal("a1b2c3d4", result.RomCrc32);
		Assert.Equal("TestGame.nes", result.RomFileName);
		Assert.NotNull(result.RomPath);
		Assert.NotNull(result.CdlPath);
		Assert.NotNull(result.PansyPath);
		Assert.NotNull(result.LabelsPath);
		Assert.True(File.Exists(result.RomPath));
		Assert.True(File.Exists(result.CdlPath!));
		Assert.True(File.Exists(result.PansyPath!));
		Assert.True(File.Exists(result.LabelsPath!));
		Assert.Equal(outputDir, result.ExtractDirectory);
	}

	[Fact]
	public void Load_RomOnly_WorksWithoutDebugFiles() {
		string zipPath = CreateTestPack();
		string outputDir = Path.Combine(_tempDir, "rom-only");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.True(File.Exists(result.RomPath));
		Assert.Null(result.CdlPath);
		Assert.Null(result.PansyPath);
		Assert.Null(result.LabelsPath);
	}

	[Fact]
	public void Load_NoManifest_StillFindsRom() {
		string zipPath = CreateTestPack(includeManifest: false);
		string outputDir = Path.Combine(_tempDir, "no-manifest");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.True(File.Exists(result.RomPath));
	}

	[Fact]
	public void Load_NoRom_ThrowsInvalidData() {
		string zipPath = Path.Combine(_tempDir, "empty.nexen-pack.zip");
		using (var stream = new FileStream(zipPath, FileMode.Create))
		using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
			var entry = archive.CreateEntry("TestGame/manifest.txt");
			using var writer = new StreamWriter(entry.Open());
			writer.Write("Nexen Game Package\nROM: TestGame");
		}

		string outputDir = Path.Combine(_tempDir, "empty-output");
		Assert.Throws<InvalidDataException>(() => NexenPackLoader.Load(zipPath, outputDir));
	}

	[Fact]
	public void Load_MissingFile_ThrowsFileNotFound() {
		Assert.Throws<FileNotFoundException>(() =>
			NexenPackLoader.Load(Path.Combine(_tempDir, "nonexistent.zip")));
	}

	[Fact]
	public void Load_DefaultOutputDirectory_CreatesNextToZip() {
		string zipPath = CreateTestPack(gameName: "MyGame");
		var result = NexenPackLoader.Load(zipPath);

		// Default extraction dir should be based on the zip name (minus .nexen-pack.zip)
		Assert.Contains("MyGame", result.ExtractDirectory);
		Assert.True(File.Exists(result.RomPath));
	}

	[Fact]
	public void Load_SnesRom_DetectsCorrectly() {
		string zipPath = CreateTestPack(
			gameName: "SnesGame",
			romFileName: "SnesGame.sfc",
			romData: new byte[] { 0x00, 0x00, 0x00, 0x00 });

		string outputDir = Path.Combine(_tempDir, "snes-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.EndsWith(".sfc", result.RomPath);
	}

	[Fact]
	public void Load_GbaRom_DetectsCorrectly() {
		string zipPath = CreateTestPack(
			gameName: "GbaGame",
			romFileName: "GbaGame.gba",
			romData: new byte[] { 0x00, 0x00, 0x00, 0x00 });

		string outputDir = Path.Combine(_tempDir, "gba-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.EndsWith(".gba", result.RomPath);
	}

	[Fact]
	public void Load_ManifestParsesAllFields() {
		string manifest = """
			Nexen Game Package
			Created: 2026-02-20 14:00:00
			Emulator: Nexen v1.3.1
			ROM: DragonWarrior
			System: Nes
			ROM CRC32: deadbeef
			ROM File: dragon-warrior.nes

			Files (1):
			  ROM/dragon-warrior.nes
			""";

		string zipPath = CreateTestPack(
			gameName: "DragonWarrior",
			romFileName: "dragon-warrior.nes",
			manifestContent: manifest);

		string outputDir = Path.Combine(_tempDir, "manifest-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal("DragonWarrior", result.GameName);
		Assert.Equal("Nes", result.System);
		Assert.Equal("deadbeef", result.RomCrc32);
		Assert.Equal("dragon-warrior.nes", result.RomFileName);
		Assert.Equal("2026-02-20 14:00:00", result.Manifest["Created"]);
		Assert.Equal("Nexen v1.3.1", result.Manifest["Emulator"]);
	}

	[Fact]
	public void Load_MlbLabels_Detected() {
		string zipPath = CreateTestPack(labelsFileName: "TestGame.mlb");
		string outputDir = Path.Combine(_tempDir, "mlb-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.LabelsPath);
		Assert.EndsWith(".mlb", result.LabelsPath!);
	}

	[Fact]
	public void TryLoad_ValidPack_ReturnsTrue() {
		string zipPath = CreateTestPack();
		string outputDir = Path.Combine(_tempDir, "try-output");

		bool success = NexenPackLoader.TryLoad(zipPath, out var result, out var error, outputDir);

		Assert.True(success);
		Assert.NotNull(result);
		Assert.Null(error);
	}

	[Fact]
	public void TryLoad_MissingFile_ReturnsFalse() {
		bool success = NexenPackLoader.TryLoad(
			Path.Combine(_tempDir, "missing.zip"),
			out var result, out var error);

		Assert.False(success);
		Assert.Null(result);
		Assert.NotNull(error);
	}

	[Fact]
	public void TryLoad_NoRom_ReturnsFalse() {
		string zipPath = Path.Combine(_tempDir, "no-rom.nexen-pack.zip");
		using (var stream = new FileStream(zipPath, FileMode.Create))
		using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
			var entry = archive.CreateEntry("Game/readme.txt");
			using var writer = new StreamWriter(entry.Open());
			writer.Write("No ROM here");
		}

		string outputDir = Path.Combine(_tempDir, "no-rom-output");
		bool success = NexenPackLoader.TryLoad(zipPath, out _, out var error, outputDir);

		Assert.False(success);
		Assert.Contains("ROM", error!);
	}

	[Fact]
	public void Load_StreamOverload_Works() {
		string zipPath = CreateTestPack();
		string outputDir = Path.Combine(_tempDir, "stream-output");

		using var stream = File.OpenRead(zipPath);
		var result = NexenPackLoader.Load(stream, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.True(File.Exists(result.RomPath));
		Assert.Equal("TestGame", result.GameName);
	}

	[Fact]
	public void Load_RomDataPreserved() {
		byte[] originalRom = [0x4e, 0x45, 0x53, 0x1a, 0x02, 0x01, 0x00, 0x00, 0xff, 0xaa];
		string zipPath = CreateTestPack(romData: originalRom);
		string outputDir = Path.Combine(_tempDir, "data-output");

		var result = NexenPackLoader.Load(zipPath, outputDir);
		byte[] extractedRom = File.ReadAllBytes(result.RomPath);

		Assert.Equal(originalRom, extractedRom);
	}

	[Fact]
	public void Load_LynxRom_DetectsCorrectly() {
		string zipPath = CreateTestPack(
			gameName: "LynxGame",
			romFileName: "LynxGame.lnx");

		string outputDir = Path.Combine(_tempDir, "lynx-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.EndsWith(".lnx", result.RomPath);
	}

	// ========================================================================
	// Enhanced Pack Loading Tests
	// ========================================================================

	[Fact]
	public void Load_WithSaveStates_FindsAllFiles() {
		string zipPath = CreateTestPack(
			saveStateFiles: ["slot1.nexen-save", "slot2.nexen-save", "quick.nexen-save"]);

		string outputDir = Path.Combine(_tempDir, "ss-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal(3, result.SaveStatePaths.Count);
		Assert.All(result.SaveStatePaths, p => Assert.True(File.Exists(p)));
	}

	[Fact]
	public void Load_WithMovies_FindsAllFiles() {
		string zipPath = CreateTestPack(
			movieFiles: ["speedrun.nexen-movie", "tas.mmo"]);

		string outputDir = Path.Combine(_tempDir, "mv-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal(2, result.MoviePaths.Count);
		Assert.All(result.MoviePaths, p => Assert.True(File.Exists(p)));
	}

	[Fact]
	public void Load_WithSaves_FindsAllFiles() {
		string zipPath = CreateTestPack(
			saveFiles: ["TestGame.sav", "TestGame.srm"]);

		string outputDir = Path.Combine(_tempDir, "sv-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal(2, result.SavePaths.Count);
		Assert.All(result.SavePaths, p => Assert.True(File.Exists(p)));
	}

	[Fact]
	public void Load_WithCheats_FindsCheatFiles() {
		string zipPath = CreateTestPack(
			cheatFiles: ["TestGame.cht"]);

		string outputDir = Path.Combine(_tempDir, "cht-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Single(result.CheatPaths);
		Assert.True(File.Exists(result.CheatPaths[0]));
	}

	[Fact]
	public void Load_WithDebugWorkspace_FindsFile() {
		string zipPath = CreateTestPack(
			debugWorkspaceFile: "TestGame.json");

		string outputDir = Path.Combine(_tempDir, "dw-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.DebugWorkspacePath);
		Assert.True(File.Exists(result.DebugWorkspacePath));
	}

	[Fact]
	public void Load_WithoutExtraFiles_EmptyCollections() {
		string zipPath = CreateTestPack();

		string outputDir = Path.Combine(_tempDir, "minimal-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Empty(result.SaveStatePaths);
		Assert.Empty(result.MoviePaths);
		Assert.Empty(result.SavePaths);
		Assert.Empty(result.CheatPaths);
		Assert.Null(result.DebugWorkspacePath);
	}

	[Fact]
	public void Load_ManifestNexenVersion_Parsed() {
		string manifest = """
			Nexen Game Package
			Nexen Version: 1.5.0
			Created: 2026-03-06 14:00:00
			ROM: TestGame
			System: Nes
			ROM CRC32: a1b2c3d4
			ROM File: TestGame.nes
			""";

		string zipPath = CreateTestPack(manifestContent: manifest);
		string outputDir = Path.Combine(_tempDir, "version-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Equal("1.5.0", result.NexenVersion);
		Assert.Equal("2026-03-06 14:00:00", result.CreatedDate);
	}

	// ========================================================================
	// CRC32 Verification Tests
	// ========================================================================

	[Fact]
	public void VerifyRomCrc32_NoCrc_ReturnsNull() {
		string zipPath = CreateTestPack(manifestContent: """
			Nexen Game Package
			ROM: TestGame
			System: Nes
			ROM File: TestGame.nes
			""");

		string outputDir = Path.Combine(_tempDir, "no-crc-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Null(result.RomCrc32);
		Assert.Null(result.VerifyRomCrc32());
	}

	[Fact]
	public void VerifyRomCrc32_MatchingCrc_ReturnsTrue() {
		byte[] romData = [0x4e, 0x45, 0x53, 0x1a, 0x02, 0x01, 0x00, 0x00];
		// Compute the actual CRC32 of this data to put in manifest
		uint crc = BitConverter.ToUInt32(HashFacade.ComputeCrc32(romData));
		string crcHex = crc.ToString("x8");

		string manifest = $"""
			Nexen Game Package
			ROM: TestGame
			System: Nes
			ROM CRC32: {crcHex}
			ROM File: TestGame.nes
			""";

		string zipPath = CreateTestPack(romData: romData, manifestContent: manifest);
		string outputDir = Path.Combine(_tempDir, "crc-match-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.True(result.VerifyRomCrc32());
	}

	[Fact]
	public void VerifyRomCrc32_MismatchedCrc_ReturnsFalse() {
		string manifest = """
			Nexen Game Package
			ROM: TestGame
			System: Nes
			ROM CRC32: deadbeef
			ROM File: TestGame.nes
			""";

		string zipPath = CreateTestPack(manifestContent: manifest);
		string outputDir = Path.Combine(_tempDir, "crc-mismatch-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.False(result.VerifyRomCrc32());
	}

	// ========================================================================
	// CDL Coverage Statistics Tests
	// ========================================================================

	[Fact]
	public void GetCdlCoverage_NoCdl_ReturnsNull() {
		string zipPath = CreateTestPack();
		string outputDir = Path.Combine(_tempDir, "no-cdl-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.Null(result.GetCdlCoverage());
	}

	[Fact]
	public void GetCdlCoverage_WithCdl_ReturnsStats() {
		// CDL data: 3 code bytes, 2 data bytes, 3 unclassified
		byte[] cdlData = [0x01, 0x01, 0x01, 0x02, 0x02, 0x00, 0x00, 0x00];

		string zipPath = CreateTestPack(
			cdlFileName: "TestGame.cdl",
			cdlData: cdlData);

		string outputDir = Path.Combine(_tempDir, "cdl-stats-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		var stats = result.GetCdlCoverage();
		Assert.NotNull(stats);
		Assert.Equal(8, stats.TotalBytes);
		Assert.Equal(3, stats.CodeBytes);
		Assert.Equal(2, stats.DataBytes);
		Assert.Equal(0, stats.DrawnBytes);
		Assert.Equal(5, stats.ClassifiedBytes);
		Assert.Equal(3, stats.UnclassifiedBytes);
		Assert.Equal(62.5, stats.CoveragePercent);
	}

	[Fact]
	public void GetCdlCoverage_DrawnFlags_Counted() {
		// CDL data with Mesen DRAWN flag (0x10)
		byte[] cdlData = [0x01, 0x10, 0x10, 0x00];

		string zipPath = CreateTestPack(
			cdlFileName: "TestGame.cdl",
			cdlData: cdlData);

		string outputDir = Path.Combine(_tempDir, "cdl-drawn-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		var stats = result.GetCdlCoverage();
		Assert.NotNull(stats);
		Assert.Equal(1, stats.CodeBytes);
		Assert.Equal(2, stats.DrawnBytes);
		Assert.Equal(3, stats.ClassifiedBytes);
		Assert.Equal(75.0, stats.CoveragePercent);
	}

	// ========================================================================
	// Full Pack With All File Types
	// ========================================================================

	[Fact]
	public void Load_FullPack_AllFieldsPopulated() {
		string zipPath = CreateTestPack(
			cdlFileName: "TestGame.cdl",
			pansyFileName: "TestGame.pansy",
			labelsFileName: "TestGame.nexen-labels",
			saveStateFiles: ["slot1.nexen-save"],
			movieFiles: ["tas.nexen-movie"],
			saveFiles: ["TestGame.sav"],
			cheatFiles: ["TestGame.cht"],
			debugWorkspaceFile: "TestGame.json");

		string outputDir = Path.Combine(_tempDir, "full-output");
		var result = NexenPackLoader.Load(zipPath, outputDir);

		Assert.NotNull(result.RomPath);
		Assert.NotNull(result.CdlPath);
		Assert.NotNull(result.PansyPath);
		Assert.NotNull(result.LabelsPath);
		Assert.NotNull(result.DebugWorkspacePath);
		Assert.Single(result.SaveStatePaths);
		Assert.Single(result.MoviePaths);
		Assert.Single(result.SavePaths);
		Assert.Single(result.CheatPaths);
	}
}
