namespace Peony.Cpu.M68000.Tests;

using Peony.Core;
using Peony.Cpu;
using Xunit;

public class M68000DecoderTests {
	private readonly M68000Decoder _decoder = new();

	[Fact]
	public void Architecture_ReturnsM68000() {
		Assert.Equal("M68000", _decoder.Architecture);
	}

	// --- Basic instructions ---

	[Fact]
	public void Decode_NOP() {
		// NOP = $4e71
		var result = _decoder.Decode([0x4e, 0x71], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_RTS() {
		// RTS = $4e75
		var result = _decoder.Decode([0x4e, 0x75], 0x0000);

		Assert.Equal("rts", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_RTE() {
		// RTE = $4e73
		var result = _decoder.Decode([0x4e, 0x73], 0x0000);

		Assert.Equal("rte", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_RTR() {
		// RTR = $4e77
		var result = _decoder.Decode([0x4e, 0x77], 0x0000);

		Assert.Equal("rtr", result.Mnemonic);
	}

	[Fact]
	public void Decode_RESET() {
		// RESET = $4e70
		var result = _decoder.Decode([0x4e, 0x70], 0x0000);

		Assert.Equal("reset", result.Mnemonic);
	}

	[Fact]
	public void Decode_TRAPV() {
		// TRAPV = $4e76
		var result = _decoder.Decode([0x4e, 0x76], 0x0000);

		Assert.Equal("trapv", result.Mnemonic);
	}

	// --- TRAP ---

	[Fact]
	public void Decode_TRAP_0() {
		// TRAP #0 = $4e40
		var result = _decoder.Decode([0x4e, 0x40], 0x0000);

		Assert.Equal("trap", result.Mnemonic);
		Assert.Equal("#$0", result.Operand);
	}

	[Fact]
	public void Decode_TRAP_15() {
		// TRAP #15 = $4e4f
		var result = _decoder.Decode([0x4e, 0x4f], 0x0000);

		Assert.Equal("trap", result.Mnemonic);
		Assert.Equal("#$15", result.Operand);
	}

	// --- MOVEQ (line 7) ---

	[Fact]
	public void Decode_MOVEQ() {
		// MOVEQ #$42, D0 = $7042
		var result = _decoder.Decode([0x70, 0x42], 0x0000);

		Assert.Equal("moveq", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_MOVEQ_Negative() {
		// MOVEQ #-1, D3 = $76ff
		var result = _decoder.Decode([0x76, 0xff], 0x0000);

		Assert.Equal("moveq", result.Mnemonic);
	}

	// --- Branch instructions (line 6) ---

	[Fact]
	public void Decode_BRA_Short() {
		// BRA.S +$10 = $6010
		var result = _decoder.Decode([0x60, 0x10], 0x1000);

		Assert.StartsWith("bra", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_BSR_Short() {
		// BSR.S +$20 = $6120
		var result = _decoder.Decode([0x61, 0x20], 0x2000);

		Assert.StartsWith("bsr", result.Mnemonic);
	}

	[Fact]
	public void Decode_BEQ_Short() {
		// BEQ.S +$04 = $6704
		var result = _decoder.Decode([0x67, 0x04], 0x3000);

		Assert.StartsWith("b", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_BNE_Short() {
		// BNE.S +$08 = $6608
		var result = _decoder.Decode([0x66, 0x08], 0x4000);

		Assert.StartsWith("b", result.Mnemonic);
	}

	[Fact]
	public void Decode_BRA_Word() {
		// BRA.W $0100 = $6000 $0100
		var result = _decoder.Decode([0x60, 0x00, 0x01, 0x00], 0x5000);

		Assert.StartsWith("bra", result.Mnemonic);
		Assert.Equal(4, result.Bytes.Length);
	}

	// --- A-line / F-line traps ---

	[Fact]
	public void Decode_ALineTrap() {
		// A-line trap = $Axxx
		var result = _decoder.Decode([0xa0, 0x00], 0x0000);

		Assert.Equal("dc.w", result.Mnemonic);
		Assert.Equal("$a000", result.Operand);
	}

	[Fact]
	public void Decode_FLineTrap() {
		// F-line trap = $Fxxx
		var result = _decoder.Decode([0xf0, 0x00], 0x0000);

		Assert.Equal("dc.w", result.Mnemonic);
		Assert.Equal("$f000", result.Operand);
	}

	// --- LINK/UNLK ---

	[Fact]
	public void Decode_LINK_A6() {
		// LINK A6, #$FFF8 = $4e56 $fff8
		var result = _decoder.Decode([0x4e, 0x56, 0xff, 0xf8], 0x0000);

		Assert.Equal("link", result.Mnemonic);
		Assert.Equal(4, result.Bytes.Length);
	}

	[Fact]
	public void Decode_UNLK_A6() {
		// UNLK A6 = $4e5e
		var result = _decoder.Decode([0x4e, 0x5e], 0x0000);

		Assert.Equal("unlk", result.Mnemonic);
	}

	// --- MOVE (lines 1-3) basic forms ---

	[Fact]
	public void Decode_MOVE_B() {
		// Line 1: MOVE.B
		// Move.B D0,D1 = $1200
		var result = _decoder.Decode([0x12, 0x00], 0x0000);

		Assert.StartsWith("move", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_MOVE_W() {
		// Line 3: MOVE.W
		// Move.W D0,D1 = $3200
		var result = _decoder.Decode([0x32, 0x00], 0x0000);

		Assert.StartsWith("move", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_MOVE_L() {
		// Line 2: MOVE.L
		// Move.L D0,D1 = $2200
		var result = _decoder.Decode([0x22, 0x00], 0x0000);

		Assert.StartsWith("move", result.Mnemonic);
		Assert.Equal(2, result.Bytes.Length);
	}

	// --- Control flow analysis ---

	[Fact]
	public void IsControlFlow_RTS_ReturnsTrue() {
		var instr = _decoder.Decode([0x4e, 0x75], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_BRA_ReturnsTrue() {
		var instr = _decoder.Decode([0x60, 0x10], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_NOP_ReturnsFalse() {
		var instr = _decoder.Decode([0x4e, 0x71], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_TRAP_ReturnsTrue() {
		var instr = _decoder.Decode([0x4e, 0x40], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	// --- GetTargets ---

	[Fact]
	public void GetTargets_BRA_Short_ReturnsTarget() {
		var instr = _decoder.Decode([0x60, 0x10], 0x1000);
		var targets = _decoder.GetTargets(instr, 0x1000).ToArray();

		Assert.Contains(0x1012u, targets);
	}

	[Fact]
	public void GetTargets_BEQ_Short_ReturnsTargetAndFallthrough() {
		// BEQ.S +4 at $2000
		var instr = _decoder.Decode([0x67, 0x04], 0x2000);
		var targets = _decoder.GetTargets(instr, 0x2000).ToArray();

		Assert.True(targets.Length >= 1);
		Assert.Contains(0x2006u, targets); // target = $2000+2+4
	}

	[Fact]
	public void GetTargets_BRA_Word_ReturnsTarget() {
		// BRA.W with 16-bit displacement of $0100 at $3000
		var instr = _decoder.Decode([0x60, 0x00, 0x01, 0x00], 0x3000);
		var targets = _decoder.GetTargets(instr, 0x3000).ToArray();

		Assert.Contains(0x3102u, targets); // $3000+2+$0100
	}

	// --- Edge cases ---

	[Fact]
	public void Decode_TooShort_ReturnsUnknown() {
		var result = _decoder.Decode([0x4e], 0x0000);
		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0000);
		Assert.Equal("???", result.Mnemonic);
	}

	// --- STOP ---

	[Fact]
	public void Decode_STOP() {
		// STOP #$2700 = $4e72 $2700
		var result = _decoder.Decode([0x4e, 0x72, 0x27, 0x00], 0x0000);

		Assert.Equal("stop", result.Mnemonic);
		Assert.Equal(4, result.Bytes.Length);
	}
}
