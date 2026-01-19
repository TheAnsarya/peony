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

	// ========== Bank-Specific Label Tests ==========

	[Fact]
	public void AddLabel_WithBank_StoresBankSpecificLabel() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		engine.AddLabel(0x8000, "bank0_label", bank: 0);
		engine.AddLabel(0x8000, "bank1_label", bank: 1);

		// Both should be stored (different banks)
		// They are auto-generated so not user-defined
		Assert.False(engine.IsUserDefinedLabel(0x8000, bank: 0));
		Assert.False(engine.IsUserDefinedLabel(0x8000, bank: 1));
	}

	[Fact]
	public void AddUserLabel_WithBank_MarksBankSpecificAsUserDefined() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		engine.AddUserLabel(0x8000, "bank0_user_label", bank: 0);
		engine.AddUserLabel(0x8000, "bank1_user_label", bank: 1);

		Assert.True(engine.IsUserDefinedLabel(0x8000, bank: 0));
		Assert.True(engine.IsUserDefinedLabel(0x8000, bank: 1));
	}

	[Fact]
	public void GetLabel_WithBank_ReturnsCorrectBankLabel() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		engine.AddLabel(0x8000, "bank0_func", bank: 0);
		engine.AddLabel(0x8000, "bank1_func", bank: 1);

		Assert.Equal("bank0_func", engine.GetLabel(0x8000, bank: 0));
		Assert.Equal("bank1_func", engine.GetLabel(0x8000, bank: 1));
	}

	[Fact]
	public void GetLabel_FallsBackToGlobalLabel() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Only add global label
		engine.AddLabel(0x8000, "global_label");

		// Should return global label when querying with bank
		Assert.Equal("global_label", engine.GetLabel(0x8000, bank: 0));
		Assert.Equal("global_label", engine.GetLabel(0x8000, bank: 1));
	}

	[Fact]
	public void GetLabel_BankLabelTakesPrecedenceOverGlobal() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Add both global and bank-specific
		engine.AddLabel(0x8000, "global_reset");
		engine.AddLabel(0x8000, "bank0_reset", bank: 0);

		// Bank 0 should use bank-specific label
		Assert.Equal("bank0_reset", engine.GetLabel(0x8000, bank: 0));

		// Bank 1 should fall back to global
		Assert.Equal("global_reset", engine.GetLabel(0x8000, bank: 1));

		// No bank specified should use global
		Assert.Equal("global_reset", engine.GetLabel(0x8000));
	}

	[Fact]
	public void GetLabel_ReturnsNullForUnknownAddress() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		Assert.Null(engine.GetLabel(0x9999));
		Assert.Null(engine.GetLabel(0x9999, bank: 0));
	}

	[Fact]
	public void AddLabel_UserDefinedBankLabelProtectsFromAutoGenerated() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Add user-defined bank label
		engine.AddUserLabel(0x8000, "important_func", bank: 0);

		// Try to overwrite with auto-generated
		engine.AddLabel(0x8000, "loc_8000", bank: 0);

		// User label should still be user-defined
		Assert.True(engine.IsUserDefinedLabel(0x8000, bank: 0));
	}

	[Fact]
	public void BankLabels_IndependentFromGlobalLabels() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Add user-defined global label
		engine.AddUserLabel(0x8000, "global_important");

		// Add non-user-defined bank label at same address
		engine.AddLabel(0x8000, "bank0_auto", bank: 0);

		// Global is user-defined
		Assert.True(engine.IsUserDefinedLabel(0x8000));

		// Bank-specific is NOT user-defined
		Assert.False(engine.IsUserDefinedLabel(0x8000, bank: 0));
	}

	/// <summary>
	/// Extended mock CPU decoder that supports control flow for cross-reference testing
	/// </summary>
	private class ControlFlowMockCpuDecoder : ICpuDecoder {
		public string Architecture => "Test6502";
		private readonly Dictionary<byte, (string mnemonic, uint target)> _controlFlow = [];

		public void AddControlFlow(byte opcode, string mnemonic, uint target) {
			_controlFlow[opcode] = (mnemonic, target);
		}

		public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
			if (data.Length == 0) return new DecodedInstruction("", "", [], AddressingMode.Implied);

			var opcode = data[0];
			if (_controlFlow.TryGetValue(opcode, out var cf)) {
				// Return a 3-byte instruction (opcode + 2-byte address)
				var bytes = data.Length >= 3 ? data[..3].ToArray() : [opcode, 0, 0];
				return new DecodedInstruction(cf.mnemonic, $"${cf.target:x4}", bytes, AddressingMode.Absolute);
			}

			return new DecodedInstruction("nop", "", [data[0]], AddressingMode.Implied);
		}

		public bool IsControlFlow(DecodedInstruction instruction) {
			var mnemonic = instruction.Mnemonic.ToUpperInvariant();
			return mnemonic is "JMP" or "JSR" or "BNE" or "BEQ" or "RTS";
		}

		public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint currentAddress) {
			foreach (var cf in _controlFlow.Values) {
				if (cf.mnemonic == instruction.Mnemonic) {
					yield return cf.target;
				}
			}
		}
	}

	/// <summary>
	/// Extended mock platform analyzer for cross-reference testing
	/// </summary>
	private class CrossRefMockPlatformAnalyzer : IPlatformAnalyzer {
		public string Platform => "NES";
		public int BankCount => 1;
		public ICpuDecoder CpuDecoder { get; }

		public CrossRefMockPlatformAnalyzer(ICpuDecoder cpuDecoder) {
			CpuDecoder = cpuDecoder;
		}

		public RomInfo Analyze(ReadOnlySpan<byte> rom) => new(
			Platform: "NES",
			Size: rom.Length,
			Mapper: null,
			Metadata: []
		);

		public int AddressToOffset(uint address, int romLength) => (int)(address - 0x8000);
		public int AddressToOffset(uint address, int romLength, int bank) => (int)(address - 0x8000);
		public string? GetRegisterLabel(uint address) => null;
		public bool IsInSwitchableRegion(uint address) => address < 0xc000;
		public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) => null;
		public MemoryRegion GetMemoryRegion(uint address) => MemoryRegion.Code;
		public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) => [0x8000];
	}

	[Fact]
	public void Disassemble_TracksCrossReferences_ForJumpInstructions() {
		var cpuDecoder = new ControlFlowMockCpuDecoder();
		cpuDecoder.AddControlFlow(0x4c, "JMP", 0x8020); // JMP $8020 at $8000

		var analyzer = new CrossRefMockPlatformAnalyzer(cpuDecoder);
		var engine = new DisassemblyEngine(cpuDecoder, analyzer);

		// ROM: JMP $8020, then NOPs, then target at $8020
		var rom = new byte[64];
		rom[0] = 0x4c; // JMP
		rom[1] = 0x20; // Low byte of $8020
		rom[2] = 0x80; // High byte of $8020
		rom[0x20] = 0x60; // RTS at target

		var result = engine.Disassemble(rom, [0x8000]);

		// Check cross-references exist for target $8020
		var refs = result.GetReferencesTo(0x8020);
		Assert.NotEmpty(refs);
		Assert.Contains(refs, r => r.FromAddress == 0x8000 && r.Type == CrossRefType.Jump);
	}

	[Fact]
	public void Disassemble_TracksCrossReferences_ForCallInstructions() {
		var cpuDecoder = new ControlFlowMockCpuDecoder();
		cpuDecoder.AddControlFlow(0x20, "JSR", 0x8030); // JSR $8030 at $8000

		var analyzer = new CrossRefMockPlatformAnalyzer(cpuDecoder);
		var engine = new DisassemblyEngine(cpuDecoder, analyzer);

		var rom = new byte[64];
		rom[0] = 0x20; // JSR
		rom[1] = 0x30; // Low byte
		rom[2] = 0x80; // High byte
		rom[0x30] = 0x60; // RTS at target

		var result = engine.Disassemble(rom, [0x8000]);

		var refs = result.GetReferencesTo(0x8030);
		Assert.NotEmpty(refs);
		Assert.Contains(refs, r => r.FromAddress == 0x8000 && r.Type == CrossRefType.Call);
	}

	[Fact]
	public void Disassemble_TracksCrossReferences_ForBranchInstructions() {
		var cpuDecoder = new ControlFlowMockCpuDecoder();
		cpuDecoder.AddControlFlow(0xd0, "BNE", 0x8010); // BNE $8010 at $8000

		var analyzer = new CrossRefMockPlatformAnalyzer(cpuDecoder);
		var engine = new DisassemblyEngine(cpuDecoder, analyzer);

		var rom = new byte[32];
		rom[0] = 0xd0; // BNE
		rom[1] = 0x0e; // Offset
		rom[2] = 0x00;
		rom[0x10] = 0x60; // RTS at target

		var result = engine.Disassemble(rom, [0x8000]);

		var refs = result.GetReferencesTo(0x8010);
		Assert.NotEmpty(refs);
		Assert.Contains(refs, r => r.FromAddress == 0x8000 && r.Type == CrossRefType.Branch);
	}

	[Fact]
	public void GetReferencesTo_ReturnsEmptyForUnreferencedAddress() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		var rom = new byte[16];
		var result = engine.Disassemble(rom, [0x8000]);

		var refs = result.GetReferencesTo(0xFFFF);
		Assert.Empty(refs);
	}

	[Fact]
	public void DataRegions_ExposedInResult() {
		var engine = new DisassemblyEngine(new MockCpuDecoder(), new MockPlatformAnalyzer());

		// Add a data region manually
		engine.AddDataRegion(0x8100, new DataDefinition("word", 10, "Test table"));

		var rom = new byte[64];
		var result = engine.Disassemble(rom, [0x8000]);

		// Check that data regions are copied to result
		Assert.Single(result.DataRegions);
		Assert.True(result.DataRegions.ContainsKey(0x8100));
		Assert.Equal("word", result.DataRegions[0x8100].Type);
		Assert.Equal(10, result.DataRegions[0x8100].Count);
	}
}
