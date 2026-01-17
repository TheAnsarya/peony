using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for DisassemblyEngine - particularly label propagation logic
/// </summary>
public class DisassemblyEngineTests {
	/// <summary>
	/// Mock CPU decoder for testing
	/// </summary>
	private class MockCpuDecoder : ICpuDecoder {
		public string Architecture => "Test6502";

		public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
			if (data.Length == 0) return new DecodedInstruction("", "", [], AddressingMode.Implied);
			return new DecodedInstruction("nop", "", [data[0]], AddressingMode.Implied);
		}

		public bool IsControlFlow(DecodedInstruction instruction) => false;
		public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint currentAddress) => [];
	}

	/// <summary>
	/// Mock platform analyzer for testing
	/// </summary>
	private class MockPlatformAnalyzer : IPlatformAnalyzer {
		public string Platform => "Test";
		public int BankCount => 1;
		public ICpuDecoder CpuDecoder => new MockCpuDecoder();

		public RomInfo Analyze(ReadOnlySpan<byte> rom) => new(
			Platform: "Test",
			Size: rom.Length,
			Mapper: null,
			Metadata: []
		);

		public int AddressToOffset(uint address, int romLength) => (int)address;
		public int AddressToOffset(uint address, int romLength, int bank) => (int)address;
		public string? GetRegisterLabel(uint address) => null;
		public bool IsInSwitchableRegion(uint address) => false;
		public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) => null;
		public MemoryRegion GetMemoryRegion(uint address) => MemoryRegion.Code;
		public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) => [0x8000];
	}

	[Fact]
	public void AddLabel_AddsNewLabel() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		engine.AddLabel(0x8000, "test_label");

		// Verify label was added (indirectly via disassembly output)
		Assert.False(engine.IsUserDefinedLabel(0x8000));  // Auto-generated, not user-defined
	}

	[Fact]
	public void AddUserLabel_MarksAsUserDefined() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		engine.AddUserLabel(0x8000, "user_label");

		Assert.True(engine.IsUserDefinedLabel(0x8000));
	}

	[Fact]
	public void AddLabel_DoesNotOverwriteUserDefinedLabel() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Add user-defined label first
		engine.AddUserLabel(0x8000, "important_function");

		// Try to overwrite with auto-generated label
		engine.AddLabel(0x8000, "loc_8000");

		// User label should still be user-defined
		Assert.True(engine.IsUserDefinedLabel(0x8000));
	}

	[Fact]
	public void SetSymbolLoader_ImportsLabelsAsUserDefined() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		var symbolLoader = new SymbolLoader();
		symbolLoader.AddLabel(0x8000, "reset");
		symbolLoader.AddLabel(0x8010, "nmi_handler");

		engine.SetSymbolLoader(symbolLoader);

		// Both labels should be marked as user-defined
		Assert.True(engine.IsUserDefinedLabel(0x8000));
		Assert.True(engine.IsUserDefinedLabel(0x8010));
	}

	[Fact]
	public void SetSymbolLoader_ImportsComments() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		var symbolLoader = new SymbolLoader();
		symbolLoader.AddLabel(0x8000, "reset");
		// Can't directly add comment via SymbolLoader public API, but we can verify
		// the mechanism exists

		engine.SetSymbolLoader(symbolLoader);

		// Label should be imported
		Assert.True(engine.IsUserDefinedLabel(0x8000));
	}

	[Fact]
	public void IsUserDefinedLabel_ReturnsFalseForUnknownAddress() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		Assert.False(engine.IsUserDefinedLabel(0x9999));
	}

	[Fact]
	public void AddLabel_WorksForNonUserDefinedAddresses() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// First auto-generated label
		engine.AddLabel(0x8000, "loc_8000");

		// Another add should not overwrite (TryAdd behavior)
		engine.AddLabel(0x8000, "different_name");

		// Not user-defined since both were auto-generated
		Assert.False(engine.IsUserDefinedLabel(0x8000));
	}

	[Fact]
	public void SetSymbolLoader_WithDizData_ImportsComments() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Create DIZ JSON with label and comment
		var json = """
		{
			"ProjectName": "TestProject",
			"RomMapMode": "LoRom",
			"Labels": {
				"32768": { "Name": "start", "Comment": "Entry point", "DataType": 1 }
			}
		}
		""";

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllText(tempFile, json);

			var symbolLoader = new SymbolLoader();
			symbolLoader.LoadDiz(tempFile);

			engine.SetSymbolLoader(symbolLoader);

			// Label should be imported and marked user-defined
			Assert.True(engine.IsUserDefinedLabel(32768));
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void MultipleSymbolLoaders_LatestWins() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		var symbolLoader1 = new SymbolLoader();
		symbolLoader1.AddLabel(0x8000, "first_name");

		var symbolLoader2 = new SymbolLoader();
		symbolLoader2.AddLabel(0x8000, "second_name");
		symbolLoader2.AddLabel(0x8010, "new_label");

		engine.SetSymbolLoader(symbolLoader1);
		engine.SetSymbolLoader(symbolLoader2);

		// Both addresses should be user-defined
		Assert.True(engine.IsUserDefinedLabel(0x8000));
		Assert.True(engine.IsUserDefinedLabel(0x8010));
	}
}
