using System.Text.Json;
using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

[Collection("PlatformResolver")]
public class ProjectScaffolderTests : IDisposable {
	private readonly string _tempDir;

	public ProjectScaffolderTests() {
		_tempDir = Path.Combine(Path.GetTempPath(), "peony-scaffold-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(_tempDir);
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		if (Directory.Exists(_tempDir))
			Directory.Delete(_tempDir, true);
	}

	private (NexenPackResult Pack, string RomPath) CreateTestPack(
		string gameName = "TestGame",
		string romFileName = "TestGame.nes",
		string system = "Nes") {

		string extractDir = Path.Combine(_tempDir, "extracted");
		string romDir = Path.Combine(extractDir, "ROM");
		string debugDir = Path.Combine(extractDir, "Debug");
		Directory.CreateDirectory(romDir);
		Directory.CreateDirectory(debugDir);

		string romPath = Path.Combine(romDir, romFileName);
		File.WriteAllBytes(romPath, [0x4e, 0x45, 0x53, 0x1a, 0x02, 0x01, 0x00, 0x00]);

		string cdlPath = Path.Combine(debugDir, gameName + ".cdl");
		File.WriteAllBytes(cdlPath, [0x43, 0x44, 0x4c, 0x01]);

		string pansyPath = Path.Combine(debugDir, gameName + ".pansy");
		File.WriteAllBytes(pansyPath, [0x50, 0x41, 0x4e, 0x53, 0x59, 0x00, 0x00, 0x00]);

		string labelsPath = Path.Combine(debugDir, gameName + ".nexen-labels");
		File.WriteAllText(labelsPath, "$8000=reset\n");

		var pack = new NexenPackResult {
			RomPath = romPath,
			CdlPath = cdlPath,
			PansyPath = pansyPath,
			LabelsPath = labelsPath,
			GameName = gameName,
			System = system,
			RomCrc32 = "a1b2c3d4",
			RomFileName = romFileName,
			ExtractDirectory = extractDir,
			Manifest = new Dictionary<string, string> {
				["ROM"] = gameName,
				["System"] = system,
				["ROM CRC32"] = "a1b2c3d4"
			}
		};

		return (pack, romPath);
	}

	[Fact]
	public void Scaffold_CreatesDirectoryStructure() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.True(Directory.Exists(Path.Combine(projectDir, "rom")));
		Assert.True(Directory.Exists(Path.Combine(projectDir, "source")));
		Assert.True(Directory.Exists(Path.Combine(projectDir, "metadata")));
		Assert.True(Directory.Exists(Path.Combine(projectDir, "output")));
	}

	[Fact]
	public void Scaffold_CopiesRom() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-rom");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.True(File.Exists(result.RomPath));
		byte[] romData = File.ReadAllBytes(result.RomPath);
		Assert.Equal(8, romData.Length);
		Assert.Equal(0x4e, romData[0]); // 'N' from NES header
	}

	[Fact]
	public void Scaffold_CopiesMetadataFiles() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-meta");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.Equal(3, result.MetadataFileCount);
		Assert.True(File.Exists(Path.Combine(projectDir, "metadata", "TestGame.cdl")));
		Assert.True(File.Exists(Path.Combine(projectDir, "metadata", "TestGame.pansy")));
		Assert.True(File.Exists(Path.Combine(projectDir, "metadata", "TestGame.nexen-labels")));
	}

	[Fact]
	public void Scaffold_GeneratesPeonyJson() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-json");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.True(File.Exists(result.PeonyJsonPath));
		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.Equal("1.0", root.GetProperty("version").GetString());
		Assert.Equal("nes", root.GetProperty("platform").GetString());
		Assert.Equal("rom/TestGame.nes", root.GetProperty("rom").GetProperty("path").GetString());
		Assert.Equal("a1b2c3d4", root.GetProperty("rom").GetProperty("crc32").GetString());
		Assert.Equal(8, root.GetProperty("rom").GetProperty("size").GetInt64());
	}

	[Fact]
	public void Scaffold_PeonyJsonHasMetadataPaths() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-meta-json");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		var metadata = doc.RootElement.GetProperty("metadata");

		Assert.Equal("metadata/TestGame.cdl", metadata.GetProperty("cdl").GetString());
		Assert.Equal("metadata/TestGame.pansy", metadata.GetProperty("pansy").GetString());
	}

	[Fact]
	public void Scaffold_PeonyJsonHasOutputConfig() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-output");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		var output = doc.RootElement.GetProperty("output");

		Assert.Equal("poppy", output.GetProperty("format").GetString());
		Assert.Equal("source/", output.GetProperty("directory").GetString());
	}

	[Fact]
	public void Scaffold_PeonyJsonHasSourceInfo() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-source");
		var options = new ScaffoldOptions { PackZipPath = "MyGame.nexen-pack.zip" };

		var result = ProjectScaffolder.Scaffold(pack, projectDir, options);

		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		var source = doc.RootElement.GetProperty("source");

		Assert.Equal("MyGame.nexen-pack.zip", source.GetProperty("nexenPack").GetString());
		Assert.True(source.TryGetProperty("importDate", out _));
	}

	[Fact]
	public void Scaffold_GeneratesReadme() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "project-readme");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.True(File.Exists(result.ReadmePath));
		string readme = File.ReadAllText(result.ReadmePath);
		Assert.Contains("TestGame", readme);
		Assert.Contains("nes", readme);
		Assert.Contains("poppy", readme);
	}

	[Fact]
	public void Scaffold_ExistingNonEmptyDir_ThrowsWithoutForce() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "existing");
		Directory.CreateDirectory(projectDir);
		File.WriteAllText(Path.Combine(projectDir, "existing.txt"), "content");

		Assert.Throws<InvalidOperationException>(() =>
			ProjectScaffolder.Scaffold(pack, projectDir));
	}

	[Fact]
	public void Scaffold_ExistingNonEmptyDir_SucceedsWithForce() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "force");
		Directory.CreateDirectory(projectDir);
		File.WriteAllText(Path.Combine(projectDir, "existing.txt"), "content");

		var result = ProjectScaffolder.Scaffold(pack, projectDir, new ScaffoldOptions { Force = true });

		Assert.True(File.Exists(result.RomPath));
		Assert.True(File.Exists(result.PeonyJsonPath));
	}

	[Fact]
	public void Scaffold_EmptyExistingDir_SucceedsWithoutForce() {
		var (pack, _) = CreateTestPack();
		string projectDir = Path.Combine(_tempDir, "empty-dir");
		Directory.CreateDirectory(projectDir);

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.True(File.Exists(result.PeonyJsonPath));
	}

	[Fact]
	public void Scaffold_NoMetadataFiles_WorksCorrectly() {
		string extractDir = Path.Combine(_tempDir, "no-meta-extracted");
		string romDir = Path.Combine(extractDir, "ROM");
		Directory.CreateDirectory(romDir);
		string romPath = Path.Combine(romDir, "game.nes");
		File.WriteAllBytes(romPath, [0x00, 0x01, 0x02, 0x03]);

		var pack = new NexenPackResult {
			RomPath = romPath,
			GameName = "Game",
			ExtractDirectory = extractDir,
			Manifest = new Dictionary<string, string>()
		};

		string projectDir = Path.Combine(_tempDir, "no-meta-project");
		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		Assert.Equal(0, result.MetadataFileCount);
		Assert.True(File.Exists(result.RomPath));

		// peony.json should not have metadata section
		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		Assert.False(doc.RootElement.TryGetProperty("metadata", out _));
	}

	[Fact]
	public void Scaffold_SnesPlatform_NormalizesCorrectly() {
		var (pack, _) = CreateTestPack(system: "SuperFamicom", romFileName: "game.sfc");
		string projectDir = Path.Combine(_tempDir, "snes-project");

		var result = ProjectScaffolder.Scaffold(pack, projectDir);

		string json = File.ReadAllText(result.PeonyJsonPath);
		using var doc = JsonDocument.Parse(json);
		Assert.Equal("snes", doc.RootElement.GetProperty("platform").GetString());
	}

	[Theory]
	[InlineData("Nes", "nes")]
	[InlineData("SNES", "snes")]
	[InlineData("SuperFamicom", "snes")]
	[InlineData("Gameboy", "gb")]
	[InlineData("GameboyAdvance", "gba")]
	[InlineData("Lynx", "lynx")]
	[InlineData("Atari2600", "atari2600")]
	[InlineData("WonderSwan", "ws")]
	[InlineData("MasterSystem", "sms")]
	[InlineData("PCEngine", "pce")]
	[InlineData(null, "unknown")]
	[InlineData("", "unknown")]
	public void NormalizePlatform_ConvertsCorrectly(string? input, string expected) {
		Assert.Equal(expected, ProjectScaffolder.NormalizePlatform(input));
	}
}
