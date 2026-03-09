using Peony.Core;
using Peony.Cpu.GameBoy;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for the Game Boy (SM83) CPU decoder.
/// Validates FrozenDictionary-based opcode lookup correctness (#121).
/// </summary>
public class GameBoyCpuDecoderTests {
	private readonly GameBoyCpuDecoder _decoder = new();

	// ===== Basic opcode decoding =====

	[Fact]
	public void Decode_NOP_ReturnsImplied() {
		var result = _decoder.Decode([0x00], 0x0000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_LD_B_Immediate() {
		var result = _decoder.Decode([0x06, 0x42], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LD_BC_Immediate16() {
		var result = _decoder.Decode([0x01, 0x34, 0x12], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JP_Absolute() {
		var result = _decoder.Decode([0xc3, 0x50, 0x01], 0x0000);

		Assert.Equal("jp", result.Mnemonic);
		Assert.Equal("$0150", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JR_Relative() {
		var result = _decoder.Decode([0x18, 0x05], 0x0100);

		Assert.Equal("jr", result.Mnemonic);
		Assert.Equal(AddressingMode.Relative, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_CALL_Absolute() {
		var result = _decoder.Decode([0xcd, 0x00, 0x40], 0x0000);

		Assert.Equal("call", result.Mnemonic);
		Assert.Equal("$4000", result.Operand);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
	}

	[Fact]
	public void Decode_RET() {
		var result = _decoder.Decode([0xc9], 0x0000);

		Assert.Equal("ret", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_ADD_A_B() {
		var result = _decoder.Decode([0x80], 0x0000);

		Assert.Equal("add", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_CP_Immediate() {
		var result = _decoder.Decode([0xfe, 0x90], 0x0000);

		Assert.Equal("cp", result.Mnemonic);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_HALT() {
		var result = _decoder.Decode([0x76], 0x0000);

		Assert.Equal("halt", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_DI() {
		var result = _decoder.Decode([0xf3], 0x0000);

		Assert.Equal("di", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_EI() {
		var result = _decoder.Decode([0xfb], 0x0000);

		Assert.Equal("ei", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	// ===== CB-prefixed opcodes =====

	[Fact]
	public void Decode_CB_BIT_0_B() {
		var result = _decoder.Decode([0xcb, 0x40], 0x0000);

		Assert.Equal("bit", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_CB_BIT_0_A() {
		var result = _decoder.Decode([0xcb, 0x47], 0x0000);

		Assert.Equal("bit", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_CB_BIT_1_HL() {
		var result = _decoder.Decode([0xcb, 0x4e], 0x0000);

		Assert.Equal("bit", result.Mnemonic);
		Assert.Equal(AddressingMode.Indirect, result.Mode);
	}

	// ===== Edge cases =====

	[Fact]
	public void Decode_EmptyData_ReturnsUnknown() {
		var result = _decoder.Decode([], 0x0000);

		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_UnknownOpcode_ReturnsUnknown() {
		// 0xd3 is unused on SM83
		var result = _decoder.Decode([0xd3], 0x0000);

		Assert.Equal("???", result.Mnemonic);
	}

	[Fact]
	public void Decode_CBPrefix_TruncatedData_ReturnsPartial() {
		// CB prefix without the second byte
		var result = _decoder.Decode([0xcb], 0x0000);

		Assert.Equal("cb", result.Mnemonic);
	}

	[Fact]
	public void Decode_MultiByte_TruncatedData_ReturnsPartial() {
		// JP needs 3 bytes but only 1 provided
		var result = _decoder.Decode([0xc3], 0x0000);

		Assert.Equal("jp", result.Mnemonic);
		Assert.Equal("???", result.Operand);
	}

	// ===== Special loads =====

	[Fact]
	public void Decode_LDH_Store() {
		var result = _decoder.Decode([0xe0, 0x40], 0x0000);

		Assert.Equal("ldh", result.Mnemonic);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
	}

	[Fact]
	public void Decode_LDH_Load() {
		var result = _decoder.Decode([0xf0, 0x44], 0x0000);

		Assert.Equal("ldh", result.Mnemonic);
		Assert.Equal(AddressingMode.Immediate, result.Mode);
	}

	[Fact]
	public void Decode_LD_Absolute_Store() {
		var result = _decoder.Decode([0xea, 0x00, 0xc0], 0x0000);

		Assert.Equal("ld", result.Mnemonic);
		Assert.Equal(AddressingMode.Absolute, result.Mode);
		Assert.Equal(3, result.Bytes.Length);
	}

	// ===== Stack operations =====

	[Fact]
	public void Decode_POP_BC() {
		var result = _decoder.Decode([0xc1], 0x0000);

		Assert.Equal("pop", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_PUSH_HL() {
		var result = _decoder.Decode([0xe5], 0x0000);

		Assert.Equal("push", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	// ===== RST instructions =====

	[Fact]
	public void Decode_RST_00() {
		var result = _decoder.Decode([0xc7], 0x0000);

		Assert.Equal("rst", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	[Fact]
	public void Decode_RST_38() {
		var result = _decoder.Decode([0xff], 0x0000);

		Assert.Equal("rst", result.Mnemonic);
		Assert.Equal(AddressingMode.Implied, result.Mode);
	}

	// ===== Sequential decoding (validates FrozenDictionary under iteration) =====

	[Fact]
	public void Decode_SequentialBytes_AllReturnValidResults() {
		// Decode a sequence of known opcodes
		byte[] rom = [0x00, 0x06, 0x42, 0xc9, 0x76, 0xf3, 0xfb];
		int offset = 0;
		int count = 0;

		while (offset < rom.Length) {
			var result = _decoder.Decode(rom.AsSpan(offset), (uint)offset);
			Assert.NotNull(result);
			Assert.NotEmpty(result.Mnemonic);
			offset += Math.Max(1, result.Bytes.Length);
			count++;
		}

		Assert.True(count > 0);
	}

	[Fact]
	public void Decode_32KB_RandomData_NeverThrows() {
		var rng = new Random(42);
		var rom = new byte[32768];
		rng.NextBytes(rom);

		int offset = 0;
		int count = 0;
		while (offset < rom.Length) {
			var result = _decoder.Decode(rom.AsSpan(offset), (uint)offset);
			Assert.NotNull(result);
			offset += Math.Max(1, result.Bytes.Length);
			count++;
		}

		Assert.True(count > 1000);
	}
}
