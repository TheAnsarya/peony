using Peony.Core;
using Peony.Cpu;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for the F8 CPU decoder covering all opcode groups
/// </summary>
public class CpuF8DecoderTests {
	private readonly CpuF8Decoder _decoder = new();

	// ========== Architecture ==========

	[Fact]
	public void Architecture_ReturnsF8() {
		Assert.Equal("F8", _decoder.Architecture);
	}

	// ========== 1-Byte Implied Opcodes ($00-$1F) ==========

	[Theory]
	[InlineData(0x00, "lr")]  // LR A,KU
	[InlineData(0x01, "lr")]  // LR A,KL
	[InlineData(0x04, "lr")]  // LR KU,A
	[InlineData(0x08, "lr")]  // LR K,P
	[InlineData(0x09, "lr")]  // LR P,K
	[InlineData(0x0a, "lr")]  // LR A,IS
	[InlineData(0x0b, "lr")]  // LR IS,A
	[InlineData(0x10, "lr")]  // LR DC,H
	[InlineData(0x11, "lr")]  // LR H,DC
	public void Decode_LR_RegisterTransfers(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_PK_Implied() {
		var result = _decoder.Decode([0x0c], 0x0800);

		Assert.Equal("pk", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Theory]
	[InlineData(0x12, "sr")]  // SR 1
	[InlineData(0x13, "sl")]  // SL 1
	[InlineData(0x14, "sr")]  // SR 4
	[InlineData(0x15, "sl")]  // SL 4
	public void Decode_ShiftOpcodes(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_LM() {
		var result = _decoder.Decode([0x16], 0x0800);
		Assert.Equal("lm", result.Mnemonic);
	}

	[Fact]
	public void Decode_ST() {
		var result = _decoder.Decode([0x17], 0x0800);
		Assert.Equal("st", result.Mnemonic);
	}

	[Theory]
	[InlineData(0x18, "com")]
	[InlineData(0x19, "lnk")]
	[InlineData(0x1a, "di")]
	[InlineData(0x1b, "ei")]
	[InlineData(0x1c, "pop")]
	[InlineData(0x1f, "inc")]
	public void Decode_MiscImplied(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode], 0x0800);
		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	// ========== 2-Byte Immediate Opcodes ($20-$27) ==========

	[Fact]
	public void Decode_LI_Immediate() {
		var result = _decoder.Decode([0x20, 0x42], 0x0800);

		Assert.Equal("li", result.Mnemonic);
		Assert.Equal("#$42", result.Operand);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Theory]
	[InlineData(0x21, "ni")]
	[InlineData(0x22, "oi")]
	[InlineData(0x23, "xi")]
	[InlineData(0x24, "ai")]
	[InlineData(0x25, "ci")]
	[InlineData(0x26, "in")]
	[InlineData(0x27, "out")]
	public void Decode_ImmediateOpcodes(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode, 0xff], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal("#$ff", result.Operand);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	// ========== 3-Byte Absolute Opcodes ($28-$2A) ==========

	[Fact]
	public void Decode_PI_Absolute() {
		var result = _decoder.Decode([0x28, 0x08, 0x00], 0x0800);

		Assert.Equal("pi", result.Mnemonic);
		Assert.Equal("$0800", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JMP_Absolute() {
		var result = _decoder.Decode([0x29, 0x0a, 0x00], 0x0800);

		Assert.Equal("jmp", result.Mnemonic);
		Assert.Equal("$0a00", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_DCI_Absolute() {
		var result = _decoder.Decode([0x2a, 0x30, 0x00], 0x0800);

		Assert.Equal("dci", result.Mnemonic);
		Assert.Equal("$3000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
	}

	// ========== NOP and XDC ==========

	[Fact]
	public void Decode_NOP() {
		var result = _decoder.Decode([0x2b], 0x0800);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_XDC() {
		var result = _decoder.Decode([0x2c], 0x0800);
		Assert.Equal("xdc", result.Mnemonic);
	}

	// ========== Undefined Opcodes ($2D-$2F) ==========

	[Theory]
	[InlineData(0x2d)]
	[InlineData(0x2e)]
	[InlineData(0x2f)]
	public void Decode_UndefinedOpcodes(byte opcode) {
		var result = _decoder.Decode([opcode], 0x0800);
		Assert.Equal("???", result.Mnemonic);
	}

	// ========== Register-Encoded Opcodes ($30-$5F) ==========

	[Theory]
	[InlineData(0x30, "ds", "r0")]
	[InlineData(0x35, "ds", "r5")]
	[InlineData(0x3a, "ds", "hu")]
	[InlineData(0x3b, "ds", "hl")]
	[InlineData(0x3c, "ds", "s")]
	[InlineData(0x3f, "ds", "r15")]
	public void Decode_DS_Register(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
		Assert.Single(result.Bytes);
	}

	[Theory]
	[InlineData(0x40, "lr", "r0")]    // LR A,r0
	[InlineData(0x49, "lr", "r9")]    // LR A,r9
	[InlineData(0x4a, "lr", "hu")]    // LR A,HU (r10)
	[InlineData(0x4b, "lr", "hl")]    // LR A,HL (r11)
	[InlineData(0x4c, "lr", "s")]     // LR A,S (ISAR indirect)
	public void Decode_LR_A_R(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
	}

	[Theory]
	[InlineData(0x50, "lr", "r0")]    // LR r0,A
	[InlineData(0x5f, "lr", "r15")]   // LR r15,A
	public void Decode_LR_R_A(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
	}

	// ========== LISU/LISL ($60-$6F) ==========

	[Theory]
	[InlineData(0x60, "lisu", "0")]
	[InlineData(0x67, "lisu", "7")]
	[InlineData(0x68, "lisl", "0")]
	[InlineData(0x6f, "lisl", "7")]
	public void Decode_LISU_LISL(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
		Assert.Single(result.Bytes);
	}

	// ========== CLR and LIS ($70-$7F) ==========

	[Fact]
	public void Decode_CLR() {
		var result = _decoder.Decode([0x70], 0x0800);
		Assert.Equal("clr", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Theory]
	[InlineData(0x71, "lis", "#1")]
	[InlineData(0x75, "lis", "#5")]
	[InlineData(0x7f, "lis", "#15")]
	public void Decode_LIS(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
		Assert.Single(result.Bytes);
	}

	// ========== Branch Instructions ($80-$9F) ==========

	[Theory]
	[InlineData(0x81, "bp")]
	[InlineData(0x82, "bc")]
	[InlineData(0x84, "bz")]
	public void Decode_BT_Aliases(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode, 0x10], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal("$0812", result.Operand);  // 0x0800 + 2 + 0x10
		Assert.Equal(AddressingMode.Relative, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_BR_Unconditional() {
		var result = _decoder.Decode([0x90, 0x10], 0x0800);

		Assert.Equal("br", result.Mnemonic);
		Assert.Equal("$0812", result.Operand);
		Assert.Equal(AddressingMode.Relative, result.Mode);
	}

	[Fact]
	public void Decode_BR_BackwardBranch() {
		var result = _decoder.Decode([0x90, 0xfe], 0x0810);  // -2 offset

		Assert.Equal("br", result.Mnemonic);
		Assert.Equal("$0810", result.Operand);  // 0x0810 + 2 - 2 = self-loop
	}

	[Fact]
	public void Decode_BR7() {
		var result = _decoder.Decode([0x8f, 0x05], 0x0800);

		Assert.Equal("br7", result.Mnemonic);
		Assert.Equal("$0807", result.Operand);
	}

	[Theory]
	[InlineData(0x91, "bm")]
	[InlineData(0x92, "bnc")]
	[InlineData(0x94, "bnz")]
	[InlineData(0x98, "bno")]
	public void Decode_BF_Aliases(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode, 0x08], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal("$080a", result.Operand);  // 0x0800 + 2 + 0x08
		Assert.Equal(AddressingMode.Relative, result.Mode);
	}

	// ========== Memory Operations ($88-$8E) ==========

	[Theory]
	[InlineData(0x88, "am")]
	[InlineData(0x89, "amd")]
	[InlineData(0x8a, "nm")]
	[InlineData(0x8b, "om")]
	[InlineData(0x8c, "xm")]
	[InlineData(0x8d, "cm")]
	[InlineData(0x8e, "adc")]
	public void Decode_MemoryOps(byte opcode, string mnemonic) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	// ========== INS/OUTS ($A0-$BF) ==========

	[Theory]
	[InlineData(0xa0, "ins", "#0")]
	[InlineData(0xa1, "ins", "#1")]
	[InlineData(0xaf, "ins", "#15")]
	public void Decode_INS(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
		Assert.Single(result.Bytes);
	}

	[Theory]
	[InlineData(0xb0, "outs", "#0")]
	[InlineData(0xb1, "outs", "#1")]
	[InlineData(0xbf, "outs", "#15")]
	public void Decode_OUTS(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
	}

	// ========== Scratchpad ALU Ops ($C0-$FF) ==========

	[Theory]
	[InlineData(0xc0, "as", "r0")]
	[InlineData(0xc5, "as", "r5")]
	[InlineData(0xd0, "asd", "r0")]
	[InlineData(0xd9, "asd", "r9")]
	[InlineData(0xe0, "xs", "r0")]
	[InlineData(0xef, "xs", "r15")]
	[InlineData(0xf0, "ns", "r0")]
	[InlineData(0xff, "ns", "r15")]
	public void Decode_ScratchpadALU(byte opcode, string mnemonic, string operand) {
		var result = _decoder.Decode([opcode], 0x0800);

		Assert.Equal(mnemonic, result.Mnemonic);
		Assert.Equal(operand, result.Operand);
		Assert.Single(result.Bytes);
	}

	// ========== Control Flow Detection ==========

	[Theory]
	[InlineData(0x29)]  // JMP
	[InlineData(0x28)]  // PI (call)
	[InlineData(0x0c)]  // PK (call via K)
	[InlineData(0x1c)]  // POP (return)
	[InlineData(0x90)]  // BR
	[InlineData(0x8f)]  // BR7
	[InlineData(0x81)]  // BP
	[InlineData(0x82)]  // BC
	[InlineData(0x84)]  // BZ
	[InlineData(0x91)]  // BM
	[InlineData(0x92)]  // BNC
	[InlineData(0x94)]  // BNZ
	[InlineData(0x98)]  // BNO
	public void IsControlFlow_BranchAndJumpOpcodes_ReturnsTrue(byte opcode) {
		byte[] data = opcode is >= 0x28 and <= 0x29
			? [opcode, 0x08, 0x00]
			: opcode is >= 0x80 and <= 0x9f and not 0x88 and not 0x89 and not 0x8a and
			  not 0x8b and not 0x8c and not 0x8d and not 0x8e
				? [opcode, 0x10]
				: [opcode];

		var decoded = _decoder.Decode(data, 0x0800);
		Assert.True(_decoder.IsControlFlow(decoded));
	}

	[Theory]
	[InlineData(0x2b)]  // NOP
	[InlineData(0x20)]  // LI
	[InlineData(0x40)]  // LR A,r0
	[InlineData(0xf0)]  // NS r0
	[InlineData(0x88)]  // AM
	public void IsControlFlow_NonControlFlow_ReturnsFalse(byte opcode) {
		byte[] data = opcode == 0x20 ? [opcode, 0x00] : [opcode];
		var decoded = _decoder.Decode(data, 0x0800);
		Assert.False(_decoder.IsControlFlow(decoded));
	}

	// ========== Get Targets ==========

	[Fact]
	public void GetTargets_JMP_SingleTarget() {
		var decoded = _decoder.Decode([0x29, 0x0a, 0x00], 0x0800);
		var targets = _decoder.GetTargets(decoded, 0x0800).ToList();

		Assert.Single(targets);
		Assert.Equal(0x0a00u, targets[0]);
	}

	[Fact]
	public void GetTargets_PI_TargetAndReturn() {
		var decoded = _decoder.Decode([0x28, 0x0a, 0x00], 0x0800);
		var targets = _decoder.GetTargets(decoded, 0x0800).ToList();

		Assert.Equal(2, targets.Count);
		Assert.Equal(0x0a00u, targets[0]);    // Call target
		Assert.Equal(0x0803u, targets[1]);    // Return address (next instruction)
	}

	[Fact]
	public void GetTargets_BR_SingleTarget() {
		var decoded = _decoder.Decode([0x90, 0x10], 0x0800);
		var targets = _decoder.GetTargets(decoded, 0x0800).ToList();

		Assert.Single(targets);  // Unconditional — no fallthrough
		Assert.Equal(0x0812u, targets[0]);
	}

	[Fact]
	public void GetTargets_ConditionalBranch_TwoTargets() {
		var decoded = _decoder.Decode([0x84, 0x10], 0x0800);  // BZ
		var targets = _decoder.GetTargets(decoded, 0x0800).ToList();

		Assert.Equal(2, targets.Count);
		Assert.Equal(0x0812u, targets[0]);    // Branch target
		Assert.Equal(0x0802u, targets[1]);    // Fallthrough
	}

	// ========== Edge Cases ==========

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0800);
		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_InsufficientData_ForTwoByteOp() {
		var result = _decoder.Decode([0x20], 0x0800);  // LI needs 2 bytes
		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_InsufficientData_ForThreeByteOp() {
		var result = _decoder.Decode([0x29, 0x08], 0x0800);  // JMP needs 3 bytes
		Assert.Equal("???", result.Mnemonic);
	}

	// ========== Static Helper Methods ==========

	[Theory]
	[InlineData(0x00, "lr a,ku")]
	[InlineData(0x01, "lr a,kl")]
	[InlineData(0x08, "lr k,p")]
	[InlineData(0x09, "lr p,k")]
	[InlineData(0x0a, "lr a,is")]
	[InlineData(0x10, "lr dc,h")]
	[InlineData(0x1d, "lr w,j")]
	[InlineData(0x1e, "lr j,w")]
	public void GetFullLrMnemonic_ReturnsCorrectForm(byte opcode, string expected) {
		Assert.Equal(expected, CpuF8Decoder.GetFullLrMnemonic(opcode));
	}

	[Theory]
	[InlineData(0x12, "sr 1")]
	[InlineData(0x13, "sl 1")]
	[InlineData(0x14, "sr 4")]
	[InlineData(0x15, "sl 4")]
	public void GetFullShiftMnemonic_ReturnsCorrectForm(byte opcode, string expected) {
		Assert.Equal(expected, CpuF8Decoder.GetFullShiftMnemonic(opcode));
	}

	// ========== Full Opcode Coverage Smoke Test ==========

	[Fact]
	public void Decode_AllOpcodes_NeverThrows() {
		// Ensure all 256 opcodes decode without throwing
		for (int op = 0; op < 256; op++) {
			// Provide enough data for any instruction (3 bytes max)
			byte[] data = [(byte)op, 0x10, 0x00];
			var result = _decoder.Decode(data, 0x0800);
			Assert.NotNull(result.Mnemonic);
			Assert.NotNull(result.Operand);
			Assert.NotNull(result.Bytes);
		}
	}
}
