namespace Peony.Cpu.HuC6280.Tests;

using Peony.Core;
using Peony.Cpu;
using Xunit;

public class HuC6280DecoderTests {
	private readonly HuC6280Decoder _decoder = new();

	[Fact]
	public void Architecture_ReturnsHuC6280() {
		Assert.Equal("HuC6280", _decoder.Architecture);
	}

	// --- Basic 6502-compatible instructions ---

	[Fact]
	public void Decode_NOP_ReturnsImplied() {
		var result = _decoder.Decode([0xea], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_LDA_Immediate() {
		var result = _decoder.Decode([0xa9, 0x42], 0x0000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("#$42", result.Operand);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_STA_Absolute() {
		var result = _decoder.Decode([0x8d, 0x00, 0x20], 0x0000);

		Assert.Equal("sta", result.Mnemonic);
		Assert.Equal("$2000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LDX_Immediate() {
		var result = _decoder.Decode([0xa2, 0xff], 0x0000);

		Assert.Equal("ldx", result.Mnemonic);
		Assert.Equal("#$ff", result.Operand);
	}

	[Fact]
	public void Decode_LDY_ZeroPage() {
		var result = _decoder.Decode([0xa4, 0x10], 0x0000);

		Assert.Equal("ldy", result.Mnemonic);
		Assert.Equal("$10", result.Operand);
		Assert.Equal(AddressingMode.ZeroPage, result.Mode);
	}

	// --- Branches ---

	[Fact]
	public void Decode_BRA_Relative() {
		var result = _decoder.Decode([0x80, 0x05], 0x1000);

		Assert.Equal("bra", result.Mnemonic);
		Assert.Equal("$1007", result.Operand);
		Assert.Equal(AddressingMode.Relative, result.Mode);
	}

	[Fact]
	public void Decode_BEQ_Relative() {
		var result = _decoder.Decode([0xf0, 0xfe], 0x2000);

		Assert.Equal("beq", result.Mnemonic);
		Assert.Equal("$2000", result.Operand);
	}

	[Fact]
	public void Decode_BNE_Relative() {
		var result = _decoder.Decode([0xd0, 0x10], 0x3000);

		Assert.Equal("bne", result.Mnemonic);
		Assert.Equal("$3012", result.Operand);
	}

	// --- Jumps/Calls ---

	[Fact]
	public void Decode_JMP_Absolute() {
		var result = _decoder.Decode([0x4c, 0x00, 0x80], 0x0000);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal("$8000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
	}

	[Fact]
	public void Decode_JSR_Absolute() {
		var result = _decoder.Decode([0x20, 0x00, 0x40], 0x0000);

		Assert.Equal("jsr", result.Mnemonic);
		Assert.Equal("$4000", result.Operand);
	}

	[Fact]
	public void Decode_RTS() {
		var result = _decoder.Decode([0x60], 0x0000);

		Assert.Equal("rts", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_RTI() {
		var result = _decoder.Decode([0x40], 0x0000);

		Assert.Equal("rti", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	// --- Stack ---

	[Fact]
	public void Decode_PHA() {
		var result = _decoder.Decode([0x48], 0x0000);

		Assert.Equal("pha", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_PLA() {
		var result = _decoder.Decode([0x68], 0x0000);

		Assert.Equal("pla", result.Mnemonic);
	}

	// --- 65C02 extensions ---

	[Fact]
	public void Decode_STZ_ZeroPage() {
		var result = _decoder.Decode([0x64, 0x20], 0x0000);

		Assert.Equal("stz", result.Mnemonic);
		Assert.Equal("$20", result.Operand);
	}

	[Fact]
	public void Decode_PHX() {
		var result = _decoder.Decode([0xda], 0x0000);

		Assert.Equal("phx", result.Mnemonic);
	}

	[Fact]
	public void Decode_PHY() {
		var result = _decoder.Decode([0x5a], 0x0000);

		Assert.Equal("phy", result.Mnemonic);
	}

	// --- HuC6280 specific: memory mapping ---

	[Fact]
	public void Decode_TAM_MemoryMap() {
		// TAM #$01 — map page to MPR
		var result = _decoder.Decode([0x53, 0x01], 0x0000);

		Assert.Equal("tam", result.Mnemonic);
		Assert.Equal("#$01", result.Operand);
	}

	[Fact]
	public void Decode_TMA_MemoryMap() {
		// TMA #$02 — read MPR
		var result = _decoder.Decode([0x43, 0x02], 0x0000);

		Assert.Equal("tma", result.Mnemonic);
		Assert.Equal("#$02", result.Operand);
	}

	// --- HuC6280 specific: VDC I/O ---

	[Fact]
	public void Decode_ST0_VdcWrite() {
		var result = _decoder.Decode([0x03, 0x00], 0x0000);

		Assert.Equal("st0", result.Mnemonic);
		Assert.Equal("#$00", result.Operand);
	}

	[Fact]
	public void Decode_ST1_VdcWrite() {
		var result = _decoder.Decode([0x13, 0x10], 0x0000);

		Assert.Equal("st1", result.Mnemonic);
		Assert.Equal("#$10", result.Operand);
	}

	[Fact]
	public void Decode_ST2_VdcWrite() {
		var result = _decoder.Decode([0x23, 0x20], 0x0000);

		Assert.Equal("st2", result.Mnemonic);
		Assert.Equal("#$20", result.Operand);
	}

	// --- HuC6280 specific: block transfers (7 bytes) ---

	[Fact]
	public void Decode_TII_BlockTransfer() {
		// TII src=$2000, dst=$4000, len=$0100
		var result = _decoder.Decode([0x73, 0x00, 0x20, 0x00, 0x40, 0x00, 0x01], 0x0000);

		Assert.Equal("tii", result.Mnemonic);
		Assert.Equal(7, result.Bytes.Length);
	}

	[Fact]
	public void Decode_TDD_BlockTransfer() {
		var result = _decoder.Decode([0xc3, 0x00, 0x20, 0x00, 0x40, 0x00, 0x01], 0x0000);

		Assert.Equal("tdd", result.Mnemonic);
		Assert.Equal(7, result.Bytes.Length);
	}

	// --- HuC6280 specific: BSR (relative subroutine call) ---

	[Fact]
	public void Decode_BSR_Relative() {
		var result = _decoder.Decode([0x44, 0x10, 0x00], 0x1000);

		Assert.Equal("bsr", result.Mnemonic);
	}

	// --- HuC6280 specific: BBR/BBS ---

	[Fact]
	public void Decode_BBR0_TestAndBranch() {
		// BBR0 zp, rel
		var result = _decoder.Decode([0x0f, 0x42, 0x05], 0x2000);

		Assert.Equal("bbr0", result.Mnemonic);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_BBS7_TestAndBranch() {
		// BBS7 zp, rel
		var result = _decoder.Decode([0xff, 0x10, 0x03], 0x3000);

		Assert.Equal("bbs7", result.Mnemonic);
	}

	// --- HuC6280 specific: CSL/CSH (clock speed) ---

	[Fact]
	public void Decode_CSL_ClockSpeedLow() {
		var result = _decoder.Decode([0x54], 0x0000);

		Assert.Equal("csl", result.Mnemonic);
	}

	[Fact]
	public void Decode_CSH_ClockSpeedHigh() {
		var result = _decoder.Decode([0xd4], 0x0000);

		Assert.Equal("csh", result.Mnemonic);
	}

	// --- Control flow analysis ---

	[Fact]
	public void IsControlFlow_JMP_ReturnsTrue() {
		var instr = _decoder.Decode([0x4c, 0x00, 0x80], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_BRA_ReturnsTrue() {
		var instr = _decoder.Decode([0x80, 0x05], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_BSR_ReturnsTrue() {
		var instr = _decoder.Decode([0x44, 0x10, 0x00], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_LDA_ReturnsFalse() {
		var instr = _decoder.Decode([0xa9, 0x42], 0x0000);
		Assert.False(_decoder.IsControlFlow(instr));
	}

	[Fact]
	public void IsControlFlow_BBR0_ReturnsTrue() {
		var instr = _decoder.Decode([0x0f, 0x42, 0x05], 0x0000);
		Assert.True(_decoder.IsControlFlow(instr));
	}

	// --- GetTargets ---

	[Fact]
	public void GetTargets_JMP_ReturnsTarget() {
		var instr = _decoder.Decode([0x4c, 0x00, 0x80], 0x0000);
		var targets = _decoder.GetTargets(instr, 0x0000).ToArray();

		Assert.Single(targets);
		Assert.Equal(0x8000u, targets[0]);
	}

	[Fact]
	public void GetTargets_BEQ_ReturnsTargetAndFallthrough() {
		var instr = _decoder.Decode([0xf0, 0x05], 0x1000);
		var targets = _decoder.GetTargets(instr, 0x1000).ToArray();

		Assert.Equal(2, targets.Length);
		Assert.Equal(0x1007u, targets[0]);
		Assert.Equal(0x1002u, targets[1]);
	}

	[Fact]
	public void GetTargets_BRA_ReturnsOnlyTarget() {
		var instr = _decoder.Decode([0x80, 0x10], 0x2000);
		var targets = _decoder.GetTargets(instr, 0x2000).ToArray();

		Assert.Single(targets);
		Assert.Equal(0x2012u, targets[0]);
	}

	// --- Edge cases ---

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0000);
		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_TruncatedInstruction_ReturnsUnknown() {
		// JSR needs 3 bytes but only 1 given
		var result = _decoder.Decode([0x20], 0x0000);
		Assert.Equal("???", result.Mnemonic);
	}
}
