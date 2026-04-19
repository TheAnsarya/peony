using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for PoppyFormatter class - output generation in Poppy assembly format
/// </summary>
public class PoppyFormatterTests {
	private static DisassemblyResult CreateResult() {
		return new DisassemblyResult {
			RomInfo = new RomInfo("NES", 0x8000, null, new Dictionary<string, string>())
		};
	}

	#region Cross-Reference Tests

	[Fact]
	public void Format_IncludesCrossRefComments_ForLabeledAddresses() {
		var result = CreateResult();

		result.Labels[0x8000] = "reset";
		result.Labels[0x8020] = "main_loop";

		result.CrossReferences[0x8020] = [
			new CrossRef(0x8010, 0, CrossRefType.Call)
		];

		var lines = new List<DisassembledLine> {
			new(0x8020, [0xea], "main_loop", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8020, 0x8021, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("Referenced by:", output);
		Assert.Contains("Called from $8010", output);
	}

	[Fact]
	public void Format_GroupsCrossRefsByType() {
		var result = CreateResult();

		result.Labels[0x8050] = "target";
		result.CrossReferences[0x8050] = [
			new CrossRef(0x8010, 0, CrossRefType.Call),
			new CrossRef(0x8020, 0, CrossRefType.Call),
			new CrossRef(0x8030, 0, CrossRefType.Jump)
		];

		var lines = new List<DisassembledLine> {
			new(0x8050, [0xea], "target", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8050, 0x8051, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("Called from $8010, $8020", output);
		Assert.Contains("Jump from $8030", output);
	}

	[Fact]
	public void Format_LimitsCrossRefsToFive() {
		var result = CreateResult();

		result.Labels[0x8100] = "hot_function";

		var refs = new List<CrossRef>();
		for (uint i = 0; i < 8; i++) {
			refs.Add(new CrossRef(0x8000 + (i * 0x10), 0, CrossRefType.Call));
		}
		result.CrossReferences[0x8100] = refs;

		var lines = new List<DisassembledLine> {
			new(0x8100, [0xea], "hot_function", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8100, 0x8101, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("(+3 more)", output);
	}

	[Fact]
	public void Format_NoCrossRefComment_WhenNoLabel() {
		var result = CreateResult();

		result.CrossReferences[0x8000] = [
			new CrossRef(0x8010, 0, CrossRefType.Jump)
		];

		var lines = new List<DisassembledLine> {
			new(0x8000, [0xea], null, "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.DoesNotContain("Referenced by:", output);
	}

	[Fact]
	public void Format_IncludesBranchRefs() {
		var result = CreateResult();

		result.Labels[0x8030] = "loop_start";
		result.CrossReferences[0x8030] = [
			new CrossRef(0x8040, 0, CrossRefType.Branch)
		];

		var lines = new List<DisassembledLine> {
			new(0x8030, [0xea], "loop_start", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8030, 0x8031, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("Branch from $8040", output);
	}

	#endregion

	#region Header Tests

	[Fact]
	public void Format_IncludesHeaderWithPlatform() {
		var result = CreateResult();

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("; Disassembled by Peony", output);
		Assert.Contains("; Platform: NES", output);
		Assert.Contains(".nes", output);
	}

	[Fact]
	public void Format_IncludesMapperWhenPresent() {
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 0x8000, "MMC3", new Dictionary<string, string>())
		};

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("; Mapper: MMC3", output);
	}

	[Fact]
	public void Format_IncludesMetadata() {
		var metadata = new Dictionary<string, string> {
			["Title"] = "Test ROM",
			["Version"] = "1.0"
		};
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 0x8000, null, metadata)
		};

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("; Title: Test ROM", output);
		Assert.Contains("; Version: 1.0", output);
	}

	#endregion

	#region Data Block Tests

	[Fact]
	public void Format_DataBlock_EmitsDbDirective() {
		var result = CreateResult();

		var lines = new List<DisassembledLine> {
			new(0x8000, [0xff, 0x00, 0xab], null, ".db $ff, $00, $ab", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8003, MemoryRegion.Data, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains(".db", output);
		Assert.Contains("$ff", output);
	}

	[Fact]
	public void Format_DataBlock_WithLabel() {
		var result = CreateResult();

		var lines = new List<DisassembledLine> {
			new(0x8000, [0x10, 0x20], "data_table", ".db $10, $20", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8002, MemoryRegion.Data, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("data_table:", output);
	}

	[Fact]
	public void Format_DataBlock_WordDirective() {
		var result = CreateResult();

		// DataDefinition with word type
		result.DataRegions[0x8000] = new DataDefinition("word", 2, "pointer_table");

		var lines = new List<DisassembledLine> {
			new(0x8000, [0x00, 0x80, 0x10, 0xc0], null, "", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8004, MemoryRegion.Data, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains(".dw", output);
		Assert.Contains("pointer_table:", output);
	}

	[Fact]
	public void Format_DataBlock_TextDirective() {
		var result = CreateResult();

		result.DataRegions[0x8000] = new DataDefinition("text", 5, "greeting");

		var lines = new List<DisassembledLine> {
			new(0x8000, "HELLO"u8.ToArray(), null, "", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8005, MemoryRegion.Data, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains(".dt \"HELLO\"", output);
		Assert.Contains("greeting:", output);
	}

	#endregion

	#region Label Substitution Tests

	[Fact]
	public void Format_SubstitutesLabelsForAddresses() {
		var result = CreateResult();

		result.Labels[0x8000] = "reset";
		result.Labels[0xc000] = "data_lookup";

		var lines = new List<DisassembledLine> {
			new(0x8010, [0xad, 0x00, 0xc0], null, "lda $c000", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8010, 0x8013, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("lda data_lookup", output);
	}

	[Fact]
	public void Format_DoesNotSubstituteImmediateMode() {
		var result = CreateResult();

		// Label at address $00 but immediate mode #$00 should not be replaced
		result.Labels[0x00] = "zero_page_var";

		var lines = new List<DisassembledLine> {
			new(0x8010, [0xa9, 0x00], null, "lda #$00", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8010, 0x8012, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		// Should keep #$00 as literal, not replace with label
		Assert.Contains("#$00", output);
	}

	#endregion

	#region Multi-Bank Tests

	[Fact]
	public void Format_MultipleBanks_EmitsBankDirectives() {
		var result = CreateResult();

		var bank0Lines = new List<DisassembledLine> {
			new(0x8000, [0xea], "bank0_start", "nop", null, 0)
		};
		var bank1Lines = new List<DisassembledLine> {
			new(0x8000, [0x60], "bank1_start", "rts", null, 1)
		};

		result.BankBlocks[0] = [new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, bank0Lines, 0)];
		result.BankBlocks[1] = [new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, bank1Lines, 1)];

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("BANK 0", output);
		Assert.Contains("BANK 1", output);
		Assert.Contains(".bank 0", output);
		Assert.Contains(".bank 1", output);
	}

	#endregion

	#region Empty/Edge Case Tests

	[Fact]
	public void Format_EmptyResult_ReturnsHeaderOnly() {
		var result = CreateResult();

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("; Disassembled by Peony", output);
		Assert.DoesNotContain("; --- Block", output);
	}

	[Fact]
	public void Format_EmptyBlock_EmitsBlockComment() {
		var result = CreateResult();

		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8010, MemoryRegion.Code, [], 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("; --- Block at $8000-$8010 ---", output);
	}

	[Fact]
	public void Format_InlineComment_IncludedInOutput() {
		var result = CreateResult();

		var lines = new List<DisassembledLine> {
			new(0x8000, [0xea], null, "nop", "do nothing", 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("do nothing", output);
	}

	[Fact]
	public void Format_BlockComment_RenderedAboveInstruction() {
		var result = CreateResult();

		result.TypedComments[0x8000] = new Pansy.Core.CommentEntry("Block comment text", Pansy.Core.CommentType.Block);

		var lines = new List<DisassembledLine> {
			new(0x8000, [0xea], "entry", "nop", "Block comment text", 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		// Block comments should be rendered as ; comment above the instruction
		Assert.Contains("; Block comment text", output);
	}

	[Fact]
	public void Format_TodoComment_IncludesTodoPrefix() {
		var result = CreateResult();

		result.TypedComments[0x8000] = new Pansy.Core.CommentEntry("Fix this later", Pansy.Core.CommentType.Todo);

		var lines = new List<DisassembledLine> {
			new(0x8000, [0xea], null, "nop", "Fix this later", 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		Assert.Contains("TODO: Fix this later", output);
	}

	#endregion

	#region Properties Tests

	[Fact]
	public void Name_ReturnsPoppy() {
		var formatter = new PoppyFormatter();
		Assert.Equal("Poppy", formatter.Name);
	}

	[Fact]
	public void Extension_ReturnsPasm() {
		var formatter = new PoppyFormatter();
		Assert.Equal(".pasm", formatter.Extension);
	}

	#endregion
}
