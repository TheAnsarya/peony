using System.IO.Compression;
using System.Text.Json;
using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

public class ProjectWriterTests {
	private static DisassemblyResult MakeResult() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, "NROM", new Dictionary<string, string>())
		};

		var lines = new List<DisassembledLine> {
			new(0x8000, [0x78], "reset", "sei", null),
			new(0x8001, [0xd8], null, "cld", null)
		};
		var block = new DisassembledBlock(0x8000, 0x800f, MemoryRegion.Code, lines);
		result.Blocks.Add(block);
		result.Labels[0x8000] = "reset";
		return result;
	}

	private static byte[] MakeRomBytes() => new byte[32768];

	[Fact]
	public void WriteProjectFolder_CreatesExpectedStructure() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes"
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), MakeRomBytes());

			Assert.True(File.Exists(Path.Combine(tempDir, "peony-project.json")));
			Assert.True(File.Exists(Path.Combine(tempDir, "poppy.json")));
			Assert.True(File.Exists(Path.Combine(tempDir, "rom", "rom-info.json")));
			Assert.True(File.Exists(Path.Combine(tempDir, "src", "main.pasm")));
			Assert.True(Directory.Exists(Path.Combine(tempDir, "include")));
			Assert.True(Directory.Exists(Path.Combine(tempDir, "analysis")));
			Assert.True(Directory.Exists(Path.Combine(tempDir, "docs")));
			Assert.True(File.Exists(Path.Combine(tempDir, ".peony", "version")));
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void WriteProjectFolder_PeonyManifestContainsExpectedFields() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes"
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), MakeRomBytes());

			var json = File.ReadAllText(Path.Combine(tempDir, "peony-project.json"));
			var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			Assert.Equal("1.0.0", root.GetProperty("formatVersion").GetString());
			Assert.Equal("test-rom", root.GetProperty("name").GetString());
			Assert.Equal("Test Rom", root.GetProperty("displayName").GetString());
			Assert.Equal("nes", root.GetProperty("rom").GetProperty("platform").GetString());
			Assert.Equal(32768, root.GetProperty("rom").GetProperty("size").GetInt32());
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void WriteProjectFolder_PoppyManifestIsValid() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes"
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), MakeRomBytes());

			var json = File.ReadAllText(Path.Combine(tempDir, "poppy.json"));
			var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			Assert.Equal("test-rom", root.GetProperty("name").GetString());
			Assert.Equal("nes", root.GetProperty("platform").GetString());
			Assert.Equal("src/main.pasm", root.GetProperty("entry").GetString());
			Assert.EndsWith(".nes", root.GetProperty("output").GetString());
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void WriteProjectArchive_CreatesValidZip() {
		var archivePath = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}.peony");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes"
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectArchive(archivePath, MakeResult(), MakeRomBytes());

			Assert.True(File.Exists(archivePath));

			using var archive = ZipFile.OpenRead(archivePath);
			var entries = archive.Entries.Select(e => e.FullName).ToHashSet();
			Assert.Contains("peony-project.json", entries);
			Assert.Contains("poppy.json", entries);
		} finally {
			if (File.Exists(archivePath))
				File.Delete(archivePath);
		}
	}

	[Fact]
	public void WriteProjectFolder_IncludesRomWhenEnabled() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes",
				IncludeRom = true
			};
			var romBytes = MakeRomBytes();
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), romBytes);

			Assert.True(File.Exists(Path.Combine(tempDir, "rom", "test.nes")));
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void WriteProjectFolder_ExcludesRomWhenDisabled() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes",
				IncludeRom = false
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), MakeRomBytes());

			Assert.False(File.Exists(Path.Combine(tempDir, "rom", "test.nes")));
			Assert.True(File.Exists(Path.Combine(tempDir, "rom", "rom-info.json")));
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void ComputeCoverage_CountsBytesCorrectly() {
		var result = MakeResult();
		var coverage = ProjectWriter.ComputeCoverage(result);

		Assert.Equal(32768, coverage.TotalBytes);
		Assert.Equal(2, coverage.CodeBytes);
		Assert.Equal(0, coverage.DataBytes);
		Assert.Equal(1, coverage.LabelCount);
	}

	[Fact]
	public void WriteProjectFolder_CoverageJsonIsValid() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"peony-test-{Guid.NewGuid():N}");
		try {
			var options = new ProjectOptions {
				ProjectName = "test-rom",
				RomPath = "test.nes"
			};
			var writer = new ProjectWriter(options);
			writer.WriteProjectFolder(tempDir, MakeResult(), MakeRomBytes());

			var json = File.ReadAllText(Path.Combine(tempDir, "analysis", "coverage.json"));
			var doc = JsonDocument.Parse(json);
			Assert.Equal(32768, doc.RootElement.GetProperty("totalBytes").GetInt32());
		} finally {
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}
}

public class HardwareIncludeGeneratorTests {
	[Theory]
	[InlineData("NES")]
	[InlineData("SNES")]
	[InlineData("Game Boy")]
	[InlineData("GBA")]
	[InlineData("Atari 2600")]
	[InlineData("Lynx")]
	public void Generate_ReturnsNonEmptyForAllPlatforms(string platform) {
		var result = HardwareIncludeGenerator.Generate(platform);
		Assert.NotEmpty(result);
		Assert.Contains("Generated by Peony", result);
	}

	[Fact]
	public void Generate_NesContainsPpuRegisters() {
		var result = HardwareIncludeGenerator.Generate("NES");
		Assert.Contains("PPU_CTRL", result);
		Assert.Contains("$2000", result);
		Assert.Contains("PPU_STATUS", result);
		Assert.Contains("$2002", result);
		Assert.Contains("JOY1", result);
	}

	[Fact]
	public void Generate_SnesContainsInidisp() {
		var result = HardwareIncludeGenerator.Generate("SNES");
		Assert.Contains("INIDISP", result);
		Assert.Contains("$2100", result);
		Assert.Contains("NMITIMEN", result);
		Assert.Contains("$4200", result);
	}

	[Fact]
	public void Generate_GameBoyContainsLcdRegisters() {
		var result = HardwareIncludeGenerator.Generate("Game Boy");
		Assert.Contains("LCDC", result);
		Assert.Contains("$ff40", result);
		Assert.Contains("JOYP", result);
		Assert.Contains("$ff00", result);
	}

	[Fact]
	public void Generate_GbaContainsDispcnt() {
		var result = HardwareIncludeGenerator.Generate("GBA");
		Assert.Contains("DISPCNT", result);
		Assert.Contains("$04000000", result);
		Assert.Contains("KEYINPUT", result);
	}

	[Fact]
	public void Generate_Atari2600ContainsTiaRegisters() {
		var result = HardwareIncludeGenerator.Generate("Atari 2600");
		Assert.Contains("VSYNC", result);
		Assert.Contains("$00", result);
		Assert.Contains("SWCHA", result);
		Assert.Contains("$0280", result);
	}

	[Fact]
	public void Generate_LynxContainsMikeyRegisters() {
		var result = HardwareIncludeGenerator.Generate("Lynx");
		Assert.Contains("TIMER0", result);
		Assert.Contains("$fd00", result);
		Assert.Contains("JOYSTICK", result);
		Assert.Contains("$fcb0", result);
	}

	[Fact]
	public void Generate_UnknownPlatformReturnsPlaceholder() {
		var result = HardwareIncludeGenerator.Generate("Unknown System");
		Assert.Contains("No platform-specific registers defined", result);
	}
}
