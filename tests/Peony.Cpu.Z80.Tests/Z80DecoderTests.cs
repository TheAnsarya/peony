namespace Peony.Cpu.Z80.Tests;

using Peony.Core;
using Peony.Cpu;
using Xunit;

public class Z80DecoderTests {
	private readonly Z80Decoder _decoder = new();

	[Fact]
	public void Architecture_ReturnsZ80() {
		Assert.Equal("Z80", _decoder.Architecture);
	}

	// --- Basic instructions ---

	[Fact]
	public void Decode_NOP_ReturnsImplied() {
		var result = _decoder.Decode([0x00], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_HALT_ReturnsImplied() {
		var result = _decoder.Decode([0x76], 0x0000);

		Assert.Equal("halt", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_DI_ReturnsImplied() {
		var result = _decoder.Decode([0xf3], 0x0000);

		Assert.Equal("di", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_EI_ReturnsImplied() {
		var result = _decoder.Decode([0xfb], 0x0000);

		Assert.Equal("ei", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	// --- 8-bit loads ---

	[Fact]
	public void Decode_LD_B_Immediate() {
		var result = _decoder.Decode([0x06, 0x42], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("b,$42", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_A_Immediate() {
		var result = _decoder.Decode([0x3e, 0xff], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("a,$ff", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_A_B_Register() {
		// LD A,B = 0x78
		var result = _decoder.Decode([0x78], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("a,b", result.Operand);
		Assert.Single(result.Bytes);
	}

	// --- 16-bit loads ---

	[Fact]
	public void Decode_LD_BC_Immediate16() {
		var result = _decoder.Decode([0x01, 0x34, 0x12], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("bc,$1234", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_HL_Immediate16() {
		var result = _decoder.Decode([0x21, 0x00, 0xc0], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("hl,$c000", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_SP_Immediate16() {
		var result = _decoder.Decode([0x31, 0xff, 0xdf], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("sp,$dfff", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	// --- Absolute memory ---

	[Fact]
	public void Decode_LD_nn_A() {
		var result = _decoder.Decode([0x32, 0x00, 0xc0], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("($c000),a", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_A_nn() {
		var result = _decoder.Decode([0x3a, 0x00, 0xc0], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("a,($c000)", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	// --- ALU ---

	[Fact]
	public void Decode_ADD_A_Immediate() {
		var result = _decoder.Decode([0xc6, 0x10], 0x0000);

		Assert.Equal("add", result.Mnemonic);
		Assert.Equal("$10", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_CP_Immediate() {
		var result = _decoder.Decode([0xfe, 0x80], 0x0000);

		Assert.Equal("cp", result.Mnemonic);
		Assert.Equal("$80", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_AND_Immediate() {
		var result = _decoder.Decode([0xe6, 0x0f], 0x0000);

		Assert.Equal("and", result.Mnemonic);
		Assert.Equal("$0f", result.Operand);
	}

	[Fact]
	public void Decode_ADD_A_B_Register() {
		// ADD A,B = 0x80
		var result = _decoder.Decode([0x80], 0x0000);

		Assert.Equal("add", result.Mnemonic);
		Assert.Equal("b", result.Operand);
	}

	// --- INC/DEC ---

	[Fact]
	public void Decode_INC_B() {
		var result = _decoder.Decode([0x04], 0x0000);

		Assert.Equal("inc", result.Mnemonic);
		Assert.Equal("b", result.Operand);
	}

	[Fact]
	public void Decode_DEC_A() {
		var result = _decoder.Decode([0x3d], 0x0000);

		Assert.Equal("dec", result.Mnemonic);
		Assert.Equal("a", result.Operand);
	}

	[Fact]
	public void Decode_INC_BC() {
		var result = _decoder.Decode([0x03], 0x0000);

		Assert.Equal("inc", result.Mnemonic);
		Assert.Equal("bc", result.Operand);
	}

	// --- Jumps ---

	[Fact]
	public void Decode_JP_Absolute() {
		var result = _decoder.Decode([0xc3, 0x00, 0x80], 0x0000);

		Assert.Equal("jp", result.Mnemonic);
		Assert.Equal("$8000", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JP_NZ_Conditional() {
		var result = _decoder.Decode([0xc2, 0x00, 0x40], 0x0000);

		Assert.Equal("jp", result.Mnemonic);
		Assert.Equal("nz,$4000", result.Operand);
	}

	[Fact]
	public void Decode_JR_Relative() {
		// JR +5 at address $1000 → target $1007
		var result = _decoder.Decode([0x18, 0x05], 0x1000);

		Assert.Equal("jr", result.Mnemonic);
		Assert.Equal("$1007", result.Operand);
	}

	[Fact]
	public void Decode_JR_NZ_Conditional() {
		// JR NZ, -3 at address $2000 → target $1fff
		var result = _decoder.Decode([0x20, 0xfd], 0x2000);

		Assert.Equal("jr", result.Mnemonic);
		Assert.Equal("nz,$1fff", result.Operand);
	}

	[Fact]
	public void Decode_DJNZ_Relative() {
		// DJNZ -2 at address $3000 → target $3000
		var result = _decoder.Decode([0x10, 0xfe], 0x3000);

		Assert.Equal("djnz", result.Mnemonic);
		Assert.Equal("$3000", result.Operand);
	}

	// --- Calls ---

	[Fact]
	public void Decode_CALL_Absolute() {
		var result = _decoder.Decode([0xcd, 0x00, 0x40], 0x0000);

		Assert.Equal("call", result.Mnemonic);
		Assert.Equal("$4000", result.Operand);
	}

	[Fact]
	public void Decode_CALL_Z_Conditional() {
		var result = _decoder.Decode([0xcc, 0x50, 0x60], 0x0000);

		Assert.Equal("call", result.Mnemonic);
		Assert.Equal("z,$6050", result.Operand);
	}

	// --- Return ---

	[Fact]
	public void Decode_RET() {
		var result = _decoder.Decode([0xc9], 0x0000);

		Assert.Equal("ret", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_RET_NZ() {
		var result = _decoder.Decode([0xc0], 0x0000);

		Assert.Equal("ret", result.Mnemonic);
		Assert.Equal("nz", result.Operand);
	}

	// --- RST ---

	[Fact]
	public void Decode_RST_38() {
		var result = _decoder.Decode([0xff], 0x0000);

		Assert.Equal("rst", result.Mnemonic);
		Assert.Equal("$38", result.Operand);
	}

	[Fact]
	public void Decode_RST_00() {
		var result = _decoder.Decode([0xc7], 0x0000);

		Assert.Equal("rst", result.Mnemonic);
		Assert.Equal("$00", result.Operand);
	}

	// --- Stack ---

	[Fact]
	public void Decode_PUSH_BC() {
		var result = _decoder.Decode([0xc5], 0x0000);

		Assert.Equal("push", result.Mnemonic);
		Assert.Equal("bc", result.Operand);
	}

	[Fact]
	public void Decode_POP_AF() {
		var result = _decoder.Decode([0xf1], 0x0000);

		Assert.Equal("pop", result.Mnemonic);
		Assert.Equal("af", result.Operand);
	}

	// --- CB prefix (bit operations) ---

	[Fact]
	public void Decode_CB_BIT_7_A() {
		// CB 7F = BIT 7,A
		var result = _decoder.Decode([0xcb, 0x7f], 0x0000);

		Assert.Equal("bit", result.Mnemonic);
		Assert.Equal("7,a", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_CB_SET_0_B() {
		// CB C0 = SET 0,B
		var result = _decoder.Decode([0xcb, 0xc0], 0x0000);

		Assert.Equal("set", result.Mnemonic);
		Assert.Equal("0,b", result.Operand);
	}

	[Fact]
	public void Decode_CB_RES_3_C() {
		// CB 99 = RES 3,C
		var result = _decoder.Decode([0xcb, 0x99], 0x0000);

		Assert.Equal("res", result.Mnemonic);
		Assert.Equal("3,c", result.Operand);
	}

	[Fact]
	public void Decode_CB_RLC_A() {
		// CB 07 = RLC A
		var result = _decoder.Decode([0xcb, 0x07], 0x0000);

		Assert.Equal("rlc", result.Mnemonic);
		Assert.Equal("a", result.Operand);
	}

	// --- ED prefix (extended) ---

	[Fact]
	public void Decode_ED_LDIR() {
		var result = _decoder.Decode([0xed, 0xb0], 0x0000);

		Assert.Equal("ldir", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_ED_RETI() {
		var result = _decoder.Decode([0xed, 0x4d], 0x0000);

		Assert.Equal("reti", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_ED_IM_1() {
		var result = _decoder.Decode([0xed, 0x56], 0x0000);

		Assert.Equal("im", result.Mnemonic);
		Assert.Equal("1", result.Operand);
	}

	[Fact]
	public void Decode_ED_IN_A_C() {
		var result = _decoder.Decode([0xed, 0x78], 0x0000);

		Assert.Equal("in", result.Mnemonic);
		Assert.Equal("a,(c)", result.Operand);
	}

	[Fact]
	public void Decode_ED_LD_nn_BC() {
		var result = _decoder.Decode([0xed, 0x43, 0x00, 0xc0], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal("($c000),bc", result.Operand);
		Assert.Equal(4, result.Bytes.Length);
	}

	// --- I/O ---

	[Fact]
	public void Decode_OUT_n_A() {
		var result = _decoder.Decode([0xd3, 0xbe], 0x0000);

		Assert.Equal("out", result.Mnemonic);
		Assert.Equal("($be),a", result.Operand);
	}

	[Fact]
	public void Decode_IN_A_n() {
		var result = _decoder.Decode([0xdb, 0x7e], 0x0000);

		Assert.Equal("in", result.Mnemonic);
		Assert.Equal("a,($7e)", result.Operand);
	}

	// --- Control flow analysis ---

	[Fact]
	public void IsControlFlow_JP_ReturnsTrue() {
		var instr = _decoder.Decode([0xc3, 0x00, 0x80], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_NOP_ReturnsFalse() {
		var instr = _decoder.Decode([0x00], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_LD_ReturnsFalse() {
		var instr = _decoder.Decode([0x3e, 0x42], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	// --- GetTargets ---

	[Fact]
	public void GetTargets_JP_Unconditional_ReturnsTarget() {
		var instr = _decoder.Decode([0xc3, 0x00, 0x80], 0x0000);
		var targets = _decoder.GetTargets(instr, 0x0000).ToArray();

		Assert.Single(targets);
		Assert.Equal(0x8000u, targets[0]);
	}

	[Fact]
	public void GetTargets_JP_Conditional_ReturnsTargetAndFallthrough() {
		var instr = _decoder.Decode([0xc2, 0x00, 0x40], 0x1000);
		var targets = _decoder.GetTargets(instr, 0x1000).ToArray();

		Assert.Equal(2, targets.Length);
		Assert.Equal(0x4000u, targets[0]);
		Assert.Equal(0x1003u, targets[1]); // fallthrough
	}

	[Fact]
	public void GetTargets_CALL_ReturnsTargetAndFallthrough() {
		var instr = _decoder.Decode([0xcd, 0x00, 0x40], 0x2000);
		var targets = _decoder.GetTargets(instr, 0x2000).ToArray();

		Assert.Equal(2, targets.Length);
		Assert.Equal(0x4000u, targets[0]);
		Assert.Equal(0x2003u, targets[1]);
	}

	[Fact]
	public void GetTargets_JR_RelativeForward() {
		var instr = _decoder.Decode([0x18, 0x05], 0x1000);
		var targets = _decoder.GetTargets(instr, 0x1000).ToArray();

		Assert.Single(targets);
		Assert.Equal(0x1007u, targets[0]);
	}

	[Fact]
	public void GetTargets_RST_ReturnsVector() {
		var instr = _decoder.Decode([0xff], 0x5000);
		var targets = _decoder.GetTargets(instr, 0x5000).ToArray();

		Assert.Equal(2, targets.Length);
		Assert.Equal(0x38u, targets[0]);
		Assert.Equal(0x5001u, targets[1]);
	}

	// --- Edge cases ---

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0000);

		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_TruncatedInstruction_ReturnsUnknown() {
		// LD BC,nn needs 3 bytes but only 1 given
		var result = _decoder.Decode([0x01], 0x0000);

		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_EXX() {
		var result = _decoder.Decode([0xd9], 0x0000);

		Assert.Equal("exx", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_EX_AF_AF() {
		var result = _decoder.Decode([0x08], 0x0000);

		Assert.Equal("ex", result.Mnemonic);
		Assert.Equal("af,af'", result.Operand);
	}
}
