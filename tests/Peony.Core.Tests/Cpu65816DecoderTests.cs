namespace Peony.Tests;

using Peony.Cpu;
using Xunit;

/// <summary>
/// Tests for the 65816 CPU decoder
/// </summary>
public class Cpu65816DecoderTests {
	private readonly Cpu65816Decoder _decoder = new();

	[Fact]
	public void Decode_Nop_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xea };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("nop", result.Mnemonic);
		Assert.Equal("", result.Operand);
		Assert.Single(result.Bytes);
	}

	[Fact]
	public void Decode_LdaImmediate8Bit_ReturnsCorrectInstruction() {
		_decoder.AccumulatorIs8Bit = true;
		var data = new byte[] { 0xa9, 0x42 };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("#$42", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LdaImmediate16Bit_ReturnsCorrectInstruction() {
		_decoder.AccumulatorIs8Bit = false;
		var data = new byte[] { 0xa9, 0x34, 0x12 };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("#$1234", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JslAbsoluteLong_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x22, 0x00, 0x80, 0x01 };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("jsl", result.Mnemonic);
		Assert.Equal("$018000", result.Operand);
		Assert.Equal(4, result.Bytes.Length);
	}

	[Fact]
	public void Decode_JmlAbsoluteLong_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x5c, 0x00, 0xc0, 0x02 };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("jml", result.Mnemonic);
		Assert.Equal("$02c000", result.Operand);
		Assert.Equal(4, result.Bytes.Length);
	}

	[Fact]
	public void Decode_Brl_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x82, 0xfd, 0xff }; // BRL $8000 (branch back 3 bytes)
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("brl", result.Mnemonic);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_Sep_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xe2, 0x30 }; // SEP #$30 (set M and X flags)
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("sep", result.Mnemonic);
		Assert.Equal("#$30", result.Operand);
	}

	[Fact]
	public void Decode_Rep_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xc2, 0x30 }; // REP #$30 (clear M and X flags)
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("rep", result.Mnemonic);
		Assert.Equal("#$30", result.Operand);
	}

	[Fact]
	public void Decode_Mvn_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x54, 0x7e, 0x00 }; // MVN $7E,$00
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("mvn", result.Mnemonic);
		Assert.Equal("$7e,$00", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_Mvp_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x44, 0x00, 0x7e }; // MVP $00,$7E
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("mvp", result.Mnemonic);
		Assert.Equal("$00,$7e", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}

	[Fact]
	public void Decode_DirectIndirectLong_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xa7, 0x10 }; // LDA [$10]
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("[$10]", result.Operand);
	}

	[Fact]
	public void Decode_StackRelative_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xa3, 0x05 }; // LDA $05,s
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("$05,s", result.Operand);
	}

	[Fact]
	public void Decode_StackRelativeIndirectY_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xb3, 0x03 }; // LDA ($03,s),y
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("lda", result.Mnemonic);
		Assert.Equal("($03,s),y", result.Operand);
	}

	[Fact]
	public void Decode_Pea_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xf4, 0x00, 0x80 }; // PEA $8000
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("pea", result.Mnemonic);
		Assert.Equal("$8000", result.Operand);
	}

	[Fact]
	public void Decode_Phb_ReturnsCorrectInstruction() {
		var data = new byte[] { 0x8b };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("phb", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_Plb_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xab };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("plb", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_Xba_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xeb };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("xba", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void Decode_Xce_ReturnsCorrectInstruction() {
		var data = new byte[] { 0xfb };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("xce", result.Mnemonic);
		Assert.Equal("", result.Operand);
	}

	[Fact]
	public void IsControlFlow_Jsl_ReturnsTrue() {
		var data = new byte[] { 0x22, 0x00, 0x80, 0x01 };
		var instruction = _decoder.Decode(data, 0x8000);

		Assert.True(_decoder.IsControlFlow(instruction));
	}

	[Fact]
	public void IsControlFlow_Rtl_ReturnsTrue() {
		var data = new byte[] { 0x6b };
		var instruction = _decoder.Decode(data, 0x8000);

		Assert.True(_decoder.IsControlFlow(instruction));
	}

	[Fact]
	public void GetTargets_Jsl_ReturnsTargetAndReturn() {
		var data = new byte[] { 0x22, 0x00, 0xc0, 0x02 };
		var instruction = _decoder.Decode(data, 0x8000);
		var targets = _decoder.GetTargets(instruction, 0x8000).ToArray();

		Assert.Equal(2, targets.Length);
		Assert.Contains(0x02c000u, targets); // Target
		Assert.Contains(0x8004u, targets);   // Return address
	}

	[Fact]
	public void GetTargets_Jml_ReturnsOnlyTarget() {
		var data = new byte[] { 0x5c, 0x00, 0xc0, 0x02 };
		var instruction = _decoder.Decode(data, 0x8000);
		var targets = _decoder.GetTargets(instruction, 0x8000).ToArray();

		Assert.Single(targets);
		Assert.Equal(0x02c000u, targets[0]);
	}

	[Fact]
	public void Decode_LdxImmediate8Bit_ReturnsCorrectInstruction() {
		_decoder.IndexIs8Bit = true;
		var data = new byte[] { 0xa2, 0xff };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("ldx", result.Mnemonic);
		Assert.Equal("#$ff", result.Operand);
		Assert.Equal(2, result.Bytes.Length);
	}

	[Fact]
	public void Decode_LdxImmediate16Bit_ReturnsCorrectInstruction() {
		_decoder.IndexIs8Bit = false;
		var data = new byte[] { 0xa2, 0x00, 0x10 };
		var result = _decoder.Decode(data, 0x8000);

		Assert.Equal("ldx", result.Mnemonic);
		Assert.Equal("#$1000", result.Operand);
		Assert.Equal(3, result.Bytes.Length);
	}
}
