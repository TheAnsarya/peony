using System.Text.Json;
using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

public class CoverageAnalyzerTests {
	private static DisassemblyResult MakeResult(int bankCount = 1) {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, "NROM", new Dictionary<string, string>())
		};

		for (int bank = 0; bank < bankCount; bank++) {
			uint baseAddr = (uint)(0x8000 + bank * 0x4000);

			var codeLines = new List<DisassembledLine> {
				new(baseAddr, [0x78], bank == 0 ? "reset" : $"bank{bank}_start", "sei", null),
				new(baseAddr + 1, [0xd8], null, "cld", null),
				new(baseAddr + 2, [0xa9, 0x00], null, "lda #$00", null)
			};
			result.Blocks.Add(new DisassembledBlock(baseAddr, baseAddr + 3, MemoryRegion.Code, codeLines, bank));

			var dataLines = new List<DisassembledLine> {
				new(baseAddr + 0x100, [0x01, 0x02, 0x03], "data_table", ".db $01, $02, $03", null)
			};
			result.Blocks.Add(new DisassembledBlock(baseAddr + 0x100, baseAddr + 0x102, MemoryRegion.Data, dataLines, bank));

			result.Labels[baseAddr] = bank == 0 ? "reset" : $"bank{bank}_start";
			result.Labels[baseAddr + 0x100] = "data_table";
		}

		return result;
	}

	[Fact]
	public void Analyze_CountsBytesCorrectly() {
		var result = MakeResult();
		var coverage = CoverageAnalyzer.Analyze(result);

		Assert.Equal(32768, coverage.TotalBytes);
		Assert.Equal(4, coverage.CodeBytes); // sei(1) + cld(1) + lda(2)
		Assert.Equal(3, coverage.DataBytes);
		Assert.Equal(0, coverage.GraphicsBytes);
		Assert.Equal(32761, coverage.UnknownBytes); // 32768 - 4 - 3
		Assert.Equal(2, coverage.LabelCount); // reset, data_table
		Assert.Equal(2, coverage.BlockCount);
		Assert.Equal(1, coverage.EntryPointCount); // 1 Code block
	}

	[Fact]
	public void Analyze_CountsGraphicsBlocks() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 8192, "NROM", new Dictionary<string, string>())
		};

		var gfxLines = new List<DisassembledLine> {
			new(0x0000, new byte[16], null, ".db ...", null)
		};
		result.Blocks.Add(new DisassembledBlock(0x0000, 0x000f, MemoryRegion.Graphics, gfxLines));

		var coverage = CoverageAnalyzer.Analyze(result);
		Assert.Equal(16, coverage.GraphicsBytes);
		Assert.Equal(0, coverage.CodeBytes);
		Assert.Equal(8176, coverage.UnknownBytes); // 8192 - 16
	}

	[Fact]
	public void Analyze_EmptyResult_ReturnsZeros() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 0, null, new Dictionary<string, string>())
		};
		var coverage = CoverageAnalyzer.Analyze(result);

		Assert.Equal(0, coverage.TotalBytes);
		Assert.Equal(0, coverage.CodeBytes);
		Assert.Equal(0, coverage.UnknownBytes);
		Assert.Equal(0, coverage.BlockCount);
	}

	[Fact]
	public void AnalyzeByBank_SingleBankReturnsOneEntry() {
		var result = MakeResult(1);
		var banks = CoverageAnalyzer.AnalyzeByBank(result);

		Assert.Single(banks);
		Assert.Equal(0, banks[0].Bank);
		Assert.Equal(4, banks[0].CodeBytes);
		Assert.Equal(3, banks[0].DataBytes);
		Assert.Equal(2, banks[0].BlockCount);
	}

	[Fact]
	public void AnalyzeByBank_MultipleBanksReturnsSeparateEntries() {
		var result = MakeResult(4);
		var banks = CoverageAnalyzer.AnalyzeByBank(result);

		Assert.Equal(4, banks.Count);
		foreach (var bank in banks) {
			Assert.Equal(4, bank.CodeBytes);
			Assert.Equal(3, bank.DataBytes);
			Assert.Equal(2, bank.BlockCount);
		}
		Assert.Equal(0, banks[0].Bank);
		Assert.Equal(3, banks[3].Bank);
	}

	[Fact]
	public void AnalyzeByBank_LabelsCounted() {
		var result = MakeResult(1);
		var banks = CoverageAnalyzer.AnalyzeByBank(result);

		// Lines with non-null Label: "reset" and "data_table"
		Assert.Equal(2, banks[0].LabelCount);
	}

	[Fact]
	public void ToJson_ContainsExpectedFields() {
		var result = MakeResult();
		var json = CoverageAnalyzer.ToJson(result);
		var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.True(root.TryGetProperty("totalBytes", out var total));
		Assert.Equal(32768, total.GetInt32());

		Assert.True(root.TryGetProperty("classified", out var classified));
		Assert.True(classified.TryGetProperty("code", out _));
		Assert.True(classified.TryGetProperty("data", out _));
		Assert.True(classified.TryGetProperty("graphics", out _));
		Assert.True(classified.TryGetProperty("unknown", out _));

		Assert.True(root.TryGetProperty("analysis", out var analysis));
		Assert.True(analysis.TryGetProperty("labels", out _));
		Assert.True(analysis.TryGetProperty("entryPoints", out _));
	}

	[Fact]
	public void ToJson_MultiBankIncludesBanksArray() {
		var result = MakeResult(3);
		var json = CoverageAnalyzer.ToJson(result);
		var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.True(root.TryGetProperty("banks", out var banks));
		Assert.Equal(JsonValueKind.Array, banks.ValueKind);
		Assert.Equal(3, banks.GetArrayLength());
	}

	[Fact]
	public void ToJson_SingleBankOmitsBanksArray() {
		var result = MakeResult(1);
		var json = CoverageAnalyzer.ToJson(result);
		var doc = JsonDocument.Parse(json);

		Assert.False(doc.RootElement.TryGetProperty("banks", out _));
	}

	[Fact]
	public void ToText_ContainsFormattedOutput() {
		var result = MakeResult(2);
		var text = CoverageAnalyzer.ToText(result);

		Assert.Contains("Coverage Report", text);
		Assert.Contains("Code:", text);
		Assert.Contains("Data:", text);
		Assert.Contains("Per-Bank Breakdown:", text);
		Assert.Contains("Bank   0:", text);
		Assert.Contains("Bank   1:", text);
	}

	[Fact]
	public void ToText_SingleBankOmitsBankBreakdown() {
		var result = MakeResult(1);
		var text = CoverageAnalyzer.ToText(result);

		Assert.Contains("Coverage Report", text);
		Assert.DoesNotContain("Per-Bank Breakdown:", text);
	}

	[Fact]
	public void Analyze_CrossRefsAndCommentsCounted() {
		var result = MakeResult(1);
		result.Comments[0x8000] = "entry point";
		result.CrossReferences[0x8100] = [new CrossRef(0x8002, 0, CrossRefType.Call)];

		var coverage = CoverageAnalyzer.Analyze(result);
		Assert.Equal(1, coverage.CommentCount);
		Assert.Equal(1, coverage.CrossRefCount);
	}
}
