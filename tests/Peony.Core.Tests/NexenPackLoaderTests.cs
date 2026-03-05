using System.IO.Compression;
using Peony.Core;
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
		bool includeManifest = true) {

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
}
