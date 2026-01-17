using Peony.Core;
using Peony.Cpu;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for the 6502 CPU decoder including illegal opcodes
/// </summary>
public class Cpu6502DecoderTests {
	private readonly Cpu6502Decoder _decoder = new();

	// ========== Official Opcodes ==========

	[Fact]
	public void Decode_NOP_ReturnsImplied() {
		var result = _decoder.Decode([0xea], 0x8000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_LDA_Immediate() {
		var result = _decoder.Decode([0xa9, 0x42], 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("#$42", result.Operand);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LDA_Absolute() {
		var result = _decoder.Decode([0xad, 0x00, 0x20], 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("$2000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JMP_Absolute() {
		var result = _decoder.Decode([0x4c, 0x00, 0x80], 0x8000);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal("$8000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
	}

	[Fact]
	public void Decode_JMP_Indirect() {
		var result = _decoder.Decode([0x6c, 0xfc, 0xff], 0x8000);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal("($fffc)", result.Operand);
		Assert.Equal(AddressingMode.Indirect, result.Mode);
	}

	[Fact]
	public void Decode_BNE_Relative_Forward() {
		var result = _decoder.Decode([0xd0, 0x10], 0x8000);

		Assert.Equal("bne", result.Mnemonic);
		Assert.Equal("$8012", result.Operand);  // 0x8000 + 2 + 0x10
		Assert.Equal(AddressingMode.Relative, result.Mode);
	}

	[Fact]
	public void Decode_BNE_Relative_Backward() {
		var result = _decoder.Decode([0xd0, 0xfe], 0x8010);  // -2 = loop to self

		Assert.Equal("bne", result.Mnemonic);
		Assert.Equal("$8010", result.Operand);  // 0x8010 + 2 - 2
	}

	[Fact]
	public void Decode_IndirectX() {
		var result = _decoder.Decode([0xa1, 0x40], 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("($40,x)", result.Operand);
		Assert.Equal(AddressingMode.IndirectX, result.Mode);
	}

	[Fact]
	public void Decode_IndirectY() {
		var result = _decoder.Decode([0xb1, 0x40], 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("($40),y", result.Operand);
		Assert.Equal(AddressingMode.IndirectY, result.Mode);
	}

	// ========== Illegal Opcodes ==========

	[Fact]
	public void Decode_JAM_HaltsWithIllegalMarker() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x02], 0x8000);

		Assert.StartsWith("*", result.Mnemonic);  // Marked as illegal
		Assert.Contains("jam", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_LAX_ZeroPage() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0xa7, 0x40], 0x8000);

		Assert.Equal("*lax", result.Mnemonic);  // LDA + LDX combined
		Assert.Equal("$40", result.Operand);
		Assert.Equal(AddressingMode.ZeroPage, result.Mode);
	}

	[Fact]
	public void Decode_SAX_ZeroPage() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x87, 0x40], 0x8000);

		Assert.Equal("*sax", result.Mnemonic);  // Store A AND X
		Assert.Equal("$40", result.Operand);
	}

	[Fact]
	public void Decode_SLO_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x0f, 0x00, 0x20], 0x8000);

		Assert.Equal("*slo", result.Mnemonic);  // ASL + ORA combined
		Assert.Equal("$2000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
	}

	[Fact]
	public void Decode_RLA_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x2f, 0x00, 0x20], 0x8000);

		Assert.Equal("*rla", result.Mnemonic);  // ROL + AND combined
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_SRE_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x4f, 0x00, 0x20], 0x8000);

		Assert.Equal("*sre", result.Mnemonic);  // LSR + EOR combined
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_RRA_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x6f, 0x00, 0x20], 0x8000);

		Assert.Equal("*rra", result.Mnemonic);  // ROR + ADC combined
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_DCP_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0xcf, 0x00, 0x20], 0x8000);

		Assert.Equal("*dcp", result.Mnemonic);  // DEC + CMP combined
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_ISC_Absolute() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0xef, 0x00, 0x20], 0x8000);

		Assert.Equal("*isc", result.Mnemonic);  // INC + SBC combined
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_ANC_Immediate() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x0b, 0xff], 0x8000);

		Assert.Equal("*anc", result.Mnemonic);  // AND + set C from bit 7
		Assert.Equal("#$ff", result.Operand);
	}

	[Fact]
	public void Decode_ALR_Immediate() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x4b, 0xff], 0x8000);

		Assert.Equal("*alr", result.Mnemonic);  // AND + LSR
		Assert.Equal("#$ff", result.Operand);
	}

	[Fact]
	public void Decode_ARR_Immediate() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x6b, 0xff], 0x8000);

		Assert.Equal("*arr", result.Mnemonic);  // AND + ROR (weird flags)
		Assert.Equal("#$ff", result.Operand);
	}

	[Fact]
	public void Decode_AXS_Immediate() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0xcb, 0x10], 0x8000);

		Assert.Equal("*axs", result.Mnemonic);  // X = (A & X) - imm
		Assert.Equal("#$10", result.Operand);
	}

	[Fact]
	public void Decode_IllegalNOP_TwoBytes() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x80, 0x00], 0x8000);

		Assert.Equal("*nop", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_IllegalNOP_ThreeBytes() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x0c, 0x00, 0x20], 0x8000);

		Assert.Equal("*nop", result.Mnemonic);
		Assert.Equal(3, result.Bytes.Length);
	}

	// ========== Unstable Opcodes ==========

	[Fact]
	public void Decode_SHY() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x9c, 0x00, 0x20], 0x8000);

		Assert.Equal("*shy", result.Mnemonic);
		Assert.Equal("$2000,x", result.Operand);
	}

	[Fact]
	public void Decode_SHX() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0x9e, 0x00, 0x20], 0x8000);

		Assert.Equal("*shx", result.Mnemonic);
		Assert.Equal("$2000,y", result.Operand);
	}

	[Fact]
	public void Decode_LAS() {
		_decoder.IncludeIllegalOpcodes = true;
		var result = _decoder.Decode([0xbb, 0x00, 0x20], 0x8000);

		Assert.Equal("*las", result.Mnemonic);
		Assert.Equal("$2000,y", result.Operand);
	}

	// ========== Control Flow ==========

	[Fact]
	public void IsControlFlow_BranchInstructions() {
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("bne", "", [], AddressingMode.Relative)));
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("beq", "", [], AddressingMode.Relative)));
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("bcc", "", [], AddressingMode.Relative)));
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("bcs", "", [], AddressingMode.Relative)));
	}

	[Fact]
	public void IsControlFlow_JumpInstructions() {
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("jmp", "", [], AddressingMode.Absolute)));
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("jsr", "", [], AddressingMode.Absolute)));
	}

	[Fact]
	public void IsControlFlow_ReturnInstructions() {
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("rts", "", [], AddressingMode.Implied)));
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("rti", "", [], AddressingMode.Implied)));
	}

	[Fact]
	public void IsControlFlow_IllegalJAM() {
		Assert.True(_decoder.IsControlFlow(new DecodedInstruction("*jam", "", [], AddressingMode.Implied)));
	}

	[Fact]
	public void GetTargets_JMP_ReturnsSingleTarget() {
		var instruction = new DecodedInstruction("jmp", "$8100", [0x4c, 0x00, 0x81], AddressingMode.Absolute);
		var targets = _decoder.GetTargets(instruction, 0x8000).ToList();

		Assert.Single(targets);
		Assert.Equal(0x8100u, targets[0]);
	}

	[Fact]
	public void GetTargets_JSR_ReturnsTwoTargets() {
		var instruction = new DecodedInstruction("jsr", "$8100", [0x20, 0x00, 0x81], AddressingMode.Absolute);
		var targets = _decoder.GetTargets(instruction, 0x8000).ToList();

		Assert.Equal(2, targets.Count);
		Assert.Contains(0x8100u, targets);  // Subroutine target
		Assert.Contains(0x8003u, targets);  // Return address
	}

	[Fact]
	public void GetTargets_Branch_ReturnsTwoTargets() {
		var instruction = new DecodedInstruction("bne", "$8010", [0xd0, 0x0e], AddressingMode.Relative);
		var targets = _decoder.GetTargets(instruction, 0x8000).ToList();

		Assert.Equal(2, targets.Count);
		Assert.Contains(0x8010u, targets);  // Branch taken
		Assert.Contains(0x8002u, targets);  // Branch not taken
	}
}
