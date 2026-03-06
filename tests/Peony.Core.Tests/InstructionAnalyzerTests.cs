using Peony.Core;
using Peony.Platform.NES;
using Peony.Cpu;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for InstructionAnalyzer — operand data reference extraction.
/// </summary>
public class InstructionAnalyzerTests {
	private readonly NesAnalyzer _nesAnalyzer = new();
	private readonly InstructionAnalyzer _analyzer;

	public InstructionAnalyzerTests() {
		_analyzer = new InstructionAnalyzer(_nesAnalyzer.CpuDecoder, _nesAnalyzer);
	}

	// ========================================================================
	// Addressing Mode Classification Tests
	// ========================================================================

	[Theory]
	[InlineData(AddressingMode.Absolute, true)]
	[InlineData(AddressingMode.AbsoluteX, true)]
	[InlineData(AddressingMode.AbsoluteY, true)]
	[InlineData(AddressingMode.ZeroPage, true)]
	[InlineData(AddressingMode.ZeroPageX, true)]
	[InlineData(AddressingMode.ZeroPageY, true)]
	[InlineData(AddressingMode.Indirect, true)]
	[InlineData(AddressingMode.IndirectX, true)]
	[InlineData(AddressingMode.IndirectY, true)]
	[InlineData(AddressingMode.Implied, false)]
	[InlineData(AddressingMode.Accumulator, false)]
	[InlineData(AddressingMode.Immediate, false)]
	[InlineData(AddressingMode.Relative, false)]
	public void IsDataAddressingMode_CorrectForMode(AddressingMode mode, bool expected) {
		Assert.Equal(expected, InstructionAnalyzer.IsDataAddressingMode(mode));
	}

	// ========================================================================
	// Reference Type Classification Tests
	// ========================================================================

	[Fact]
	public void GetRefType_JsrIsCall() {
		Assert.Equal(DataRefType.Call, InstructionAnalyzer.GetRefType("jsr", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Call, InstructionAnalyzer.GetRefType("JSR", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_JmpIsJump() {
		Assert.Equal(DataRefType.Jump, InstructionAnalyzer.GetRefType("jmp", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_JmpIndirectIsIndirect() {
		Assert.Equal(DataRefType.Indirect, InstructionAnalyzer.GetRefType("jmp", AddressingMode.Indirect));
	}

	[Fact]
	public void GetRefType_BranchInstructions() {
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("beq", AddressingMode.Relative));
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("bne", AddressingMode.Relative));
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("bcc", AddressingMode.Relative));
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("bcs", AddressingMode.Relative));
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("bpl", AddressingMode.Relative));
		Assert.Equal(DataRefType.Branch, InstructionAnalyzer.GetRefType("bmi", AddressingMode.Relative));
	}

	[Fact]
	public void GetRefType_LoadInstructionsAreRead() {
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("lda", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("ldx", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("ldy", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_StoreInstructionsAreWrite() {
		Assert.Equal(DataRefType.Write, InstructionAnalyzer.GetRefType("sta", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Write, InstructionAnalyzer.GetRefType("stx", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Write, InstructionAnalyzer.GetRefType("sty", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_CompareInstructionsAreRead() {
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("cmp", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("cpx", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("cpy", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_ArithmeticInstructionsAreRead() {
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("adc", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("sbc", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("and", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("ora", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("eor", AddressingMode.Absolute));
		Assert.Equal(DataRefType.Read, InstructionAnalyzer.GetRefType("bit", AddressingMode.Absolute));
	}

	[Fact]
	public void GetRefType_UnknownMnemonicReturnsNull() {
		Assert.Null(InstructionAnalyzer.GetRefType("nop", AddressingMode.Implied));
		Assert.Null(InstructionAnalyzer.GetRefType("pha", AddressingMode.Implied));
		Assert.Null(InstructionAnalyzer.GetRefType("rts", AddressingMode.Implied));
	}

	// ========================================================================
	// Target Address Extraction Tests
	// ========================================================================

	[Fact]
	public void ExtractTargetAddress_ZeroPage() {
		var instr = new DecodedInstruction("lda", "$42", [0xa5, 0x42], AddressingMode.ZeroPage);
		Assert.Equal(0x42u, InstructionAnalyzer.ExtractTargetAddress(instr));
	}

	[Fact]
	public void ExtractTargetAddress_Absolute() {
		var instr = new DecodedInstruction("lda", "$8000", [0xad, 0x00, 0x80], AddressingMode.Absolute);
		Assert.Equal(0x8000u, InstructionAnalyzer.ExtractTargetAddress(instr));
	}

	[Fact]
	public void ExtractTargetAddress_AbsoluteLong() {
		var instr = new DecodedInstruction("lda", "$018000", [0xaf, 0x00, 0x80, 0x01], AddressingMode.AbsoluteLong);
		Assert.Equal(0x018000u, InstructionAnalyzer.ExtractTargetAddress(instr));
	}

	[Fact]
	public void ExtractTargetAddress_TooShort_ReturnsNull() {
		var instr = new DecodedInstruction("nop", "", [0xea], AddressingMode.Implied);
		Assert.Null(InstructionAnalyzer.ExtractTargetAddress(instr));
	}

	// ========================================================================
	// GetDataReference Integration Tests
	// ========================================================================

	[Fact]
	public void GetDataReference_LdaAbsolute_IsDataRead() {
		// LDA $c000 — loads from address in PRG-ROM, not a hardware register
		var instr = new DecodedInstruction("lda", "$c000", [0xad, 0x00, 0xc0], AddressingMode.Absolute);
		var result = _analyzer.GetDataReference(instr, 0x8000, 16);

		Assert.NotNull(result);
		Assert.Equal(0xc000u, result.TargetAddress);
		Assert.Equal(DataRefType.Read, result.RefType);
		Assert.Equal(16, result.InstructionOffset);
		Assert.Equal(0x8000u, result.InstructionAddress);
	}

	[Fact]
	public void GetDataReference_StaAbsolute_IsDataWrite() {
		// STA $c100
		var instr = new DecodedInstruction("sta", "$c100", [0x8d, 0x00, 0xc1], AddressingMode.Absolute);
		var result = _analyzer.GetDataReference(instr, 0x8010, 32);

		Assert.NotNull(result);
		Assert.Equal(0xc100u, result.TargetAddress);
		Assert.Equal(DataRefType.Write, result.RefType);
	}

	[Fact]
	public void GetDataReference_JsrAbsolute_IsCall() {
		// JSR $8100
		var instr = new DecodedInstruction("jsr", "$8100", [0x20, 0x00, 0x81], AddressingMode.Absolute);
		var result = _analyzer.GetDataReference(instr, 0x8000, 16);

		Assert.NotNull(result);
		Assert.Equal(0x8100u, result.TargetAddress);
		Assert.Equal(DataRefType.Call, result.RefType);
	}

	[Fact]
	public void GetDataReference_HardwareRegister_ReturnsNull() {
		// LDA $2002 — PPUSTATUS is a hardware register, not data
		var instr = new DecodedInstruction("lda", "$2002", [0xad, 0x02, 0x20], AddressingMode.Absolute);
		var result = _analyzer.GetDataReference(instr, 0x8000, 16);

		Assert.Null(result);
	}

	[Fact]
	public void GetDataReference_Immediate_ReturnsNull() {
		// LDA #$ff — immediate mode, no memory reference
		var instr = new DecodedInstruction("lda", "#$ff", [0xa9, 0xff], AddressingMode.Immediate);
		var result = _analyzer.GetDataReference(instr, 0x8000, 16);

		Assert.Null(result);
	}

	[Fact]
	public void GetDataReference_Implied_ReturnsNull() {
		// NOP — no operand
		var instr = new DecodedInstruction("nop", "", [0xea], AddressingMode.Implied);
		var result = _analyzer.GetDataReference(instr, 0x8000, 16);

		Assert.Null(result);
	}

	// ========================================================================
	// FindDataReferences Range Scan Tests
	// ========================================================================

	[Fact]
	public void FindDataReferences_OnlyScansCodeRegions() {
		// Create a ROM with: LDA $c000 (code), then some data bytes
		// The iNES header is 16 bytes, PRG starts at offset 16
		var rom = CreateNesRomWithCode(new byte[] {
			0xad, 0x00, 0xc0, // LDA $c000
			0x60,             // RTS
			0xff, 0xff, 0xff, // Data bytes
		});

		// Classification: mark first 4 bytes as Code, rest as Unknown
		var map = new ByteClassification[rom.Length];
		for (int i = 16; i < 20; i++) {
			map[i] = ByteClassification.Code;
		}

		var refs = _analyzer.FindDataReferences(rom, map, 16, 23);

		// Should find LDA $c000 as a Read reference
		Assert.Single(refs);
		Assert.Equal(0xc000u, refs[0].TargetAddress);
		Assert.Equal(DataRefType.Read, refs[0].RefType);
	}

	[Fact]
	public void FindDataReferences_SkipsNonCodeBytes() {
		// Create ROM where all bytes are marked Unknown (no code)
		var rom = new byte[100];
		var map = new ByteClassification[100]; // All Unknown

		var refs = _analyzer.FindDataReferences(rom, map, 0, 100);

		Assert.Empty(refs);
	}

	/// <summary>
	/// Create a NES ROM with code injected at the start of PRG-ROM.
	/// </summary>
	private static byte[] CreateNesRomWithCode(byte[] code) {
		var prgSize = 2 * 16384;
		var chrSize = 8192;
		var totalSize = 16 + prgSize + chrSize;
		var rom = new byte[totalSize];

		// iNES header
		rom[0] = 0x4e; rom[1] = 0x45; rom[2] = 0x53; rom[3] = 0x1a;
		rom[4] = 2; // PRG banks
		rom[5] = 1; // CHR banks

		// Copy code into start of PRG-ROM
		Array.Copy(code, 0, rom, 16, Math.Min(code.Length, prgSize));

		// Vectors at end of PRG
		var vecBase = 16 + prgSize - 6;
		rom[vecBase] = 0x00; rom[vecBase + 1] = 0x80;     // NMI
		rom[vecBase + 2] = 0x00; rom[vecBase + 3] = 0x80; // RESET
		rom[vecBase + 4] = 0x00; rom[vecBase + 5] = 0x80; // IRQ

		return rom;
	}
}
