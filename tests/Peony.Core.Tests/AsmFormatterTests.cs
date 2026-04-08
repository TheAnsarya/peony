using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for AsmFormatter — ASM output generation and label substitution
/// </summary>
public class AsmFormatterTests {
	private static DisassemblyResult CreateResult(Dictionary<uint, string>? labels = null) {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};
		if (labels != null) {
			foreach (var kvp in labels)
				result.Labels[kvp.Key] = kvp.Value;
		}
		return result;
	}

	// =========================================================================
	// FormatWithLabels tests
	// =========================================================================

	[Fact]
	public void FormatWithLabels_NoLabels_ReturnsUnchanged() {
		var result = CreateResult();
		var output = AsmFormatter.FormatWithLabels("lda $8000", result);
		Assert.Equal("lda $8000", output);
	}

	[Fact]
	public void FormatWithLabels_MatchingLabel_SubstitutesAddress() {
		var result = CreateResult(new() { [0x8000] = "reset_vector" });
		var output = AsmFormatter.FormatWithLabels("jsr $8000", result);
		Assert.Equal("jsr reset_vector", output);
	}

	[Fact]
	public void FormatWithLabels_ImmediateMode_NotSubstituted() {
		var result = CreateResult(new() { [0x00ff] = "gen_byte_ff" });
		var output = AsmFormatter.FormatWithLabels("lda #$ff", result);
		Assert.Equal("lda #$ff", output);
	}

	[Fact]
	public void FormatWithLabels_ZeroPageAddress_Substituted() {
		var result = CreateResult(new() { [0x0010] = "player_x" });
		var output = AsmFormatter.FormatWithLabels("lda $10", result);
		Assert.Equal("lda player_x", output);
	}

	[Fact]
	public void FormatWithLabels_CaseInsensitive_Matches() {
		var result = CreateResult(new() { [0xabcd] = "my_label" });
		var output = AsmFormatter.FormatWithLabels("jmp $ABCD", result);
		Assert.Equal("jmp my_label", output);
	}

	[Fact]
	public void FormatWithLabels_PartialMatch_NotSubstituted() {
		var result = CreateResult(new() { [0x8000] = "reset" });
		var output = AsmFormatter.FormatWithLabels("lda $80001", result);
		Assert.Equal("lda $80001", output);
	}

	[Fact]
	public void FormatWithLabels_MultipleLabels_AllSubstituted() {
		var result = CreateResult(new() { [0x8000] = "reset", [0x8003] = "nmi_handler" });
		var output1 = AsmFormatter.FormatWithLabels("jsr $8000", result);
		var output2 = AsmFormatter.FormatWithLabels("jmp $8003", result);
		Assert.Equal("jsr reset", output1);
		Assert.Equal("jmp nmi_handler", output2);
	}

	// =========================================================================
	// WriteOutput tests
	// =========================================================================

	[Fact]
	public void WriteOutput_EmptyResult_ProducesHeader() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, null, new Dictionary<string, string>())
		};

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result, "test.nes");

		var output = sw.ToString();
		Assert.Contains("; 🌺 Peony Disassembly", output);
		Assert.Contains("; ROM: test.nes", output);
		Assert.Contains("; Platform: NES", output);
		Assert.Contains("; Size: 32768 bytes", output);
		Assert.Contains(".system:NES", output);
	}

	[Fact]
	public void WriteOutput_WithMapper_IncludesMapper() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, "MMC3", new Dictionary<string, string>())
		};

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result, "test.nes");

		var output = sw.ToString();
		Assert.Contains("; Mapper: MMC3", output);
	}

	[Fact]
	public void WriteOutput_WithLabels_IncludesCount() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, null, new Dictionary<string, string>())
		};
		result.Labels[0x8000] = "reset";

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result);

		var output = sw.ToString();
		Assert.Contains("; Labels: 1", output);
	}

	[Fact]
	public void WriteOutput_WithBlock_OutputsBlockMarkers() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};
		result.Blocks.Add(new DisassembledBlock(
			0x8000, 0x8002, MemoryRegion.Code,
			[new DisassembledLine(0x8000, [0xea], null, "nop", null)]
		));

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result);

		var output = sw.ToString();
		Assert.Contains("; === Block $8000-$8002 (Code) ===", output);
		Assert.Contains("nop", output);
		Assert.Contains("ea", output);
	}

	[Fact]
	public void WriteOutput_WithLabel_OutputsLabelLine() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};
		result.Blocks.Add(new DisassembledBlock(
			0x8000, 0x8002, MemoryRegion.Code,
			[new DisassembledLine(0x8000, [0xea], "reset_vector", "nop", null)]
		));

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result);

		var output = sw.ToString();
		Assert.Contains("reset_vector:", output);
	}

	[Fact]
	public void WriteOutput_NoRomName_SkipsRomLine() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};

		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, result);

		var output = sw.ToString();
		Assert.DoesNotContain("; ROM:", output);
	}

	// =========================================================================
	// IOutputGenerator interface tests
	// =========================================================================

	[Fact]
	public void Name_ReturnsASM() {
		Assert.Equal("ASM", AsmFormatter.Instance.Name);
	}

	[Fact]
	public void Extension_ReturnsDotAsm() {
		Assert.Equal(".asm", AsmFormatter.Instance.Extension);
	}

	[Fact]
	public void Format_ReturnsString() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};

		var output = AsmFormatter.Instance.Format(result);
		Assert.Contains("; 🌺 Peony Disassembly", output);
	}

	[Fact]
	public void Generate_WritesToFile() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", 16, null, new Dictionary<string, string>())
		};

		var path = Path.Combine(Path.GetTempPath(), $"peony_test_{Guid.NewGuid()}.asm");
		try {
			AsmFormatter.Instance.Generate(result, path);
			var content = File.ReadAllText(path);
			Assert.Contains("; 🌺 Peony Disassembly", content);
		} finally {
			if (File.Exists(path))
				File.Delete(path);
		}
	}
}
