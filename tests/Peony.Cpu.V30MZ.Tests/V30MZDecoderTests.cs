namespace Peony.Cpu.V30MZ.Tests;

using Peony.Core;
using Peony.Cpu;
using Xunit;

public class V30MZDecoderTests {
	private readonly V30MZDecoder _decoder = new();

	[Fact]
	public void Architecture_ReturnsV30MZ() {
		Assert.Equal("V30MZ", _decoder.Architecture);
	}

	// --- Basic instructions ---

	[Fact]
	public void Decode_NOP() {
		var result = _decoder.Decode([0x90], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_HLT() {
		var result = _decoder.Decode([0xf4], 0x0000);

		Assert.Equal("hlt", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_CLI() {
		var result = _decoder.Decode([0xfa], 0x0000);

		Assert.Equal("cli", result.Mnemonic);
	}

	[Fact]
	public void Decode_STI() {
		var result = _decoder.Decode([0xfb], 0x0000);

		Assert.Equal("sti", result.Mnemonic);
	}

	[Fact]
	public void Decode_CLD() {
		var result = _decoder.Decode([0xfc], 0x0000);

		Assert.Equal("cld", result.Mnemonic);
	}

	[Fact]
	public void Decode_STD() {
		var result = _decoder.Decode([0xfd], 0x0000);

		Assert.Equal("std", result.Mnemonic);
	}

	// --- MOV register immediate ---

	[Fact]
	public void Decode_MOV_AL_Immediate() {
		var result = _decoder.Decode([0xb0, 0x42], 0x0000);

		Assert.Equal("mov", result.Mnemonic);
		Assert.Equal("al,$42", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_MOV_AX_Immediate16() {
		var result = _decoder.Decode([0xb8, 0x34, 0x12], 0x0000);

		Assert.Equal("mov", result.Mnemonic);
		Assert.Equal("ax,$1234", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_MOV_BX_Immediate16() {
		var result = _decoder.Decode([0xbb, 0xff, 0x00], 0x0000);

		Assert.Equal("mov", result.Mnemonic);
		Assert.Equal("bx,$00ff", result.Operand);
	}

	[Fact]
	public void Decode_MOV_SP_Immediate16() {
		var result = _decoder.Decode([0xbc, 0x00, 0x10], 0x0000);

		Assert.Equal("mov", result.Mnemonic);
		Assert.Equal("sp,$1000", result.Operand);
	}

	// --- INC/DEC r16 ---

	[Fact]
	public void Decode_INC_AX() {
		var result = _decoder.Decode([0x40], 0x0000);

		Assert.Equal("inc", result.Mnemonic);
		Assert.Equal("ax", result.Operand);
	}

	[Fact]
	public void Decode_DEC_CX() {
		var result = _decoder.Decode([0x49], 0x0000);

		Assert.Equal("dec", result.Mnemonic);
		Assert.Equal("cx", result.Operand);
	}

	// --- PUSH/POP r16 ---

	[Fact]
	public void Decode_PUSH_AX() {
		var result = _decoder.Decode([0x50], 0x0000);

		Assert.Equal("push", result.Mnemonic);
		Assert.Equal("ax", result.Operand);
	}

	[Fact]
	public void Decode_POP_BX() {
		var result = _decoder.Decode([0x5b], 0x0000);

		Assert.Equal("pop", result.Mnemonic);
		Assert.Equal("bx", result.Operand);
	}

	[Fact]
	public void Decode_PUSH_Immediate16() {
		var result = _decoder.Decode([0x68, 0x34, 0x12], 0x0000);

		Assert.Equal("push", result.Mnemonic);
		Assert.Equal("$1234", result.Operand);
	}

	[Fact]
	public void Decode_PUSHA() {
		var result = _decoder.Decode([0x60], 0x0000);

		Assert.Equal("pusha", result.Mnemonic);
	}

	[Fact]
	public void Decode_POPA() {
		var result = _decoder.Decode([0x61], 0x0000);

		Assert.Equal("popa", result.Mnemonic);
	}

	// --- ALU immediate with accumulator ---

	[Fact]
	public void Decode_ADD_AL_Immediate() {
		var result = _decoder.Decode([0x04, 0x10], 0x0000);

		Assert.Equal("add", result.Mnemonic);
		Assert.Equal("al,$10", result.Operand);
	}

	[Fact]
	public void Decode_CMP_AX_Immediate() {
		var result = _decoder.Decode([0x3d, 0x00, 0x80], 0x0000);

		Assert.Equal("cmp", result.Mnemonic);
		Assert.Equal("ax,$8000", result.Operand);
	}

	[Fact]
	public void Decode_AND_AL_Immediate() {
		var result = _decoder.Decode([0x24, 0x0f], 0x0000);

		Assert.Equal("and", result.Mnemonic);
		Assert.Equal("al,$0f", result.Operand);
	}

	[Fact]
	public void Decode_XOR_AX_Immediate() {
		var result = _decoder.Decode([0x35, 0xff, 0xff], 0x0000);

		Assert.Equal("xor", result.Mnemonic);
		Assert.Equal("ax,$ffff", result.Operand);
	}

	// --- Conditional jumps ---

	[Fact]
	public void Decode_JZ_Short() {
		var result = _decoder.Decode([0x74, 0x05], 0x1000);

		Assert.Equal("jz", result.Mnemonic);
	}

	[Fact]
	public void Decode_JNZ_Short() {
		var result = _decoder.Decode([0x75, 0xfe], 0x2000);

		Assert.Equal("jnz", result.Mnemonic);
	}

	[Fact]
	public void Decode_JC_Short() {
		var result = _decoder.Decode([0x72, 0x10], 0x3000);

		Assert.Equal("jc", result.Mnemonic);
	}

	// --- CALL/JMP near ---

	[Fact]
	public void Decode_CALL_Near() {
		var result = _decoder.Decode([0xe8, 0x00, 0x10], 0x0000);

		Assert.Equal("call", result.Mnemonic);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JMP_Near() {
		var result = _decoder.Decode([0xe9, 0x00, 0x10], 0x0000);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JMP_Short() {
		var result = _decoder.Decode([0xeb, 0x05], 0x1000);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_RET() {
		var result = _decoder.Decode([0xc3], 0x0000);

		Assert.Equal("ret", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_RETF() {
		var result = _decoder.Decode([0xcb], 0x0000);

		Assert.Equal("retf", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_IRET() {
		var result = _decoder.Decode([0xcf], 0x0000);

		Assert.Equal("iret", result.Mnemonic);
	}

	// --- INT ---

	[Fact]
	public void Decode_INT_3() {
		var result = _decoder.Decode([0xcc], 0x0000);

		Assert.Equal("int", result.Mnemonic);
		Assert.Equal("3", result.Operand);
	}

	[Fact]
	public void Decode_INT_N() {
		var result = _decoder.Decode([0xcd, 0x21], 0x0000);

		Assert.Equal("int", result.Mnemonic);
		Assert.Equal("$21", result.Operand);
	}

	// --- IN/OUT ---

	[Fact]
	public void Decode_IN_AL_Port() {
		var result = _decoder.Decode([0xe4, 0x60], 0x0000);

		Assert.Equal("in", result.Mnemonic);
		Assert.Equal("al,$60", result.Operand);
	}

	[Fact]
	public void Decode_OUT_Port_AL() {
		var result = _decoder.Decode([0xe6, 0x61], 0x0000);

		Assert.Equal("out", result.Mnemonic);
		Assert.Equal("$61,al", result.Operand);
	}

	[Fact]
	public void Decode_IN_AL_DX() {
		var result = _decoder.Decode([0xec], 0x0000);

		Assert.Equal("in", result.Mnemonic);
		Assert.Equal("al,dx", result.Operand);
	}

	// --- String instructions ---

	[Fact]
	public void Decode_MOVSB() {
		var result = _decoder.Decode([0xa4], 0x0000);

		Assert.Equal("movsb", result.Mnemonic);
	}

	[Fact]
	public void Decode_STOSW() {
		var result = _decoder.Decode([0xab], 0x0000);

		Assert.Equal("stosw", result.Mnemonic);
	}

	[Fact]
	public void Decode_LODSB() {
		var result = _decoder.Decode([0xac], 0x0000);

		Assert.Equal("lodsb", result.Mnemonic);
	}

	[Fact]
	public void Decode_REP_Prefix() {
		var result = _decoder.Decode([0xf3], 0x0000);

		Assert.Equal("rep", result.Mnemonic);
	}

	// --- LOOP ---

	[Fact]
	public void Decode_LOOP() {
		var result = _decoder.Decode([0xe2, 0xfe], 0x2000);

		Assert.Equal("loop", result.Mnemonic);
	}

	[Fact]
	public void Decode_JCXZ() {
		var result = _decoder.Decode([0xe3, 0x05], 0x3000);

		Assert.Equal("jcxz", result.Mnemonic);
	}

	// --- Flags ---

	[Fact]
	public void Decode_CLC() {
		var result = _decoder.Decode([0xf8], 0x0000);
		Assert.Equal("clc", result.Mnemonic);
	}

	[Fact]
	public void Decode_STC() {
		var result = _decoder.Decode([0xf9], 0x0000);
		Assert.Equal("stc", result.Mnemonic);
	}

	[Fact]
	public void Decode_PUSHF() {
		var result = _decoder.Decode([0x9c], 0x0000);
		Assert.Equal("pushf", result.Mnemonic);
	}

	[Fact]
	public void Decode_POPF() {
		var result = _decoder.Decode([0x9d], 0x0000);
		Assert.Equal("popf", result.Mnemonic);
	}

	// --- XCHG ---

	[Fact]
	public void Decode_XCHG_AX_BX() {
		var result = _decoder.Decode([0x93], 0x0000);

		Assert.Equal("xchg", result.Mnemonic);
		Assert.Equal("ax,bx", result.Operand);
	}

	// --- CBW/CWD ---

	[Fact]
	public void Decode_CBW() {
		var result = _decoder.Decode([0x98], 0x0000);
		Assert.Equal("cbw", result.Mnemonic);
	}

	[Fact]
	public void Decode_CWD() {
		var result = _decoder.Decode([0x99], 0x0000);
		Assert.Equal("cwd", result.Mnemonic);
	}

	// --- TEST immediate ---

	[Fact]
	public void Decode_TEST_AL_Immediate() {
		var result = _decoder.Decode([0xa8, 0x01], 0x0000);

		Assert.Equal("test", result.Mnemonic);
		Assert.Equal("al,$01", result.Operand);
	}

	// --- Control flow analysis ---

	[Fact]
	public void IsControlFlow_JMP_ReturnsTrue() {
		var instr = _decoder.Decode([0xe9, 0x00, 0x10], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_CALL_ReturnsTrue() {
		var instr = _decoder.Decode([0xe8, 0x00, 0x10], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_JZ_ReturnsTrue() {
		var instr = _decoder.Decode([0x74, 0x05], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_NOP_ReturnsFalse() {
		var instr = _decoder.Decode([0x90], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_MOV_ReturnsFalse() {
		var instr = _decoder.Decode([0xb0, 0x42], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_RET_ReturnsTrue() {
		var instr = _decoder.Decode([0xc3], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_INT_ReturnsTrue() {
		var instr = _decoder.Decode([0xcd, 0x21], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_LOOP_ReturnsTrue() {
		var instr = _decoder.Decode([0xe2, 0xfe], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	// --- Edge cases ---

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0000);
		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_SegmentOverride_ES() {
		// ES: prefix + NOP
		var result = _decoder.Decode([0x26, 0x90], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
	}
}
