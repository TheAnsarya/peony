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

	[Fact]
	public void Format_IncludesCrossRefComments_ForLabeledAddresses() {
		// Create a disassembly result with cross-references
		var result = CreateResult();

		// Add some labels
		result.Labels[0x8000] = "reset";
		result.Labels[0x8020] = "main_loop";

		// Add cross-references: $8010 calls $8020
		result.CrossReferences[0x8020] = [
			new CrossRef(0x8010, 0, CrossRefType.Call)
		];

		// Add a block with the labeled instruction
		var lines = new List<DisassembledLine> {
			new(0x8020, [0xea], "main_loop", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8020, 0x8021, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		// Verify cross-reference comment is included
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

		// Should group calls and jumps separately
		Assert.Contains("Called from $8010, $8020", output);
		Assert.Contains("Jump from $8030", output);
	}

	[Fact]
	public void Format_LimitsCrossRefsToFive() {
		var result = CreateResult();

		result.Labels[0x8100] = "hot_function";

		// Add 8 call references
		var refs = new List<CrossRef>();
		for (uint i = 0; i < 8; i++) {
			refs.Add(new CrossRef(0x8000 + i * 0x10, 0, CrossRefType.Call));
		}
		result.CrossReferences[0x8100] = refs;

		var lines = new List<DisassembledLine> {
			new(0x8100, [0xea], "hot_function", "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8100, 0x8101, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		// Should show first 5 and indicate more
		Assert.Contains("(+3 more)", output);
	}

	[Fact]
	public void Format_NoCrossRefComment_WhenNoLabel() {
		var result = CreateResult();

		// No label for address, but has cross-refs
		result.CrossReferences[0x8000] = [
			new CrossRef(0x8010, 0, CrossRefType.Jump)
		];

		// Line without a label
		var lines = new List<DisassembledLine> {
			new(0x8000, [0xea], null, "nop", null, 0)
		};
		result.Blocks.Add(new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines, 0));

		var formatter = new PoppyFormatter();
		var output = formatter.Format(result);

		// Cross-ref comments only appear when there's also a label
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
}
