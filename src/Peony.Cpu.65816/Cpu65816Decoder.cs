namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// 65816 CPU instruction decoder for SNES
/// Handles variable-width registers (8/16-bit A, X, Y)
/// </summary>
public class Cpu65816Decoder : ICpuDecoder {
	public string Architecture => "65816";

	/// <summary>
	/// Current accumulator width (8 or 16 bits)
	/// Set by SEP/REP instructions modifying M flag
	/// </summary>
	public bool AccumulatorIs8Bit { get; set; } = true;

	/// <summary>
	/// Current index register width (8 or 16 bits)
	/// Set by SEP/REP instructions modifying X flag
	/// </summary>
	public bool IndexIs8Bit { get; set; } = true;

	/// <summary>
	/// Whether processor is in emulation mode
	/// In emulation mode, behaves like 6502
	/// </summary>
	public bool EmulationMode { get; set; } = false;

	// 65816-specific addressing modes
	public enum Mode65816 {
		Implied,
		Immediate8,
		Immediate16,
		ImmediateM,	// Depends on M flag (accumulator)
		ImmediateX,	// Depends on X flag (index)
		Direct,		// Zero page / direct page
		DirectX,
		DirectY,
		DirectIndirect,
		DirectIndirectLong,
		DirectIndirectY,
		DirectIndirectLongY,
		DirectIndirectX,
		Absolute,
		AbsoluteX,
		AbsoluteY,
		AbsoluteIndirect,
		AbsoluteIndirectX,
		AbsoluteIndirectLong,
		AbsoluteLong,
		AbsoluteLongX,
		Relative8,
		Relative16,
		StackRelative,
		StackRelativeIndirectY,
		BlockMove
	}

	// Opcode info: (mnemonic, mode, base bytes)
	private static readonly (string Mnemonic, Mode65816 Mode, int BaseBytes)[] OpcodeTable = new (string, Mode65816, int)[256];

	static Cpu65816Decoder() {
		InitializeOpcodeTable();
	}

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		var opcode = data[0];
		var (mnemonic, mode, baseBytes) = OpcodeTable[opcode];

		// Calculate actual byte count based on mode and register widths
		var length = CalculateInstructionLength(mode, baseBytes);

		if (data.Length < length)
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		var bytes = data[..length].ToArray();
		var operand = FormatOperand(data, mode, address);
		var coreMode = MapToAddressingMode(mode);

		return new DecodedInstruction(mnemonic, operand, bytes, coreMode);
	}

	private int CalculateInstructionLength(Mode65816 mode, int baseBytes) {
		return mode switch {
			Mode65816.ImmediateM => AccumulatorIs8Bit ? 2 : 3,
			Mode65816.ImmediateX => IndexIs8Bit ? 2 : 3,
			_ => baseBytes
		};
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is
			"jmp" or "jml" or "jsr" or "jsl" or
			"rts" or "rtl" or "rti" or "brk" or "cop" or
			"bcc" or "bcs" or "beq" or "bmi" or "bne" or "bpl" or "bvc" or "bvs" or
			"bra" or "brl";
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		var (_, mode, _) = OpcodeTable[instruction.Bytes[0]];

		switch (mode) {
			case Mode65816.Absolute when instruction.Mnemonic is "jmp" or "jsr":
				yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				if (instruction.Mnemonic == "jsr")
					yield return nextAddress;
				break;

			case Mode65816.AbsoluteLong when instruction.Mnemonic is "jml" or "jsl":
				yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8) | (instruction.Bytes[3] << 16));
				if (instruction.Mnemonic == "jsl")
					yield return nextAddress;
				break;

			case Mode65816.Relative8:
				var offset8 = (sbyte)instruction.Bytes[1];
				yield return (uint)(nextAddress + offset8);
				if (instruction.Mnemonic != "bra")
					yield return nextAddress;
				break;

			case Mode65816.Relative16:
				var offset16 = (short)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				yield return (uint)(nextAddress + offset16);
				break;
		}
	}

	private string FormatOperand(ReadOnlySpan<byte> data, Mode65816 mode, uint address) {
		return mode switch {
			Mode65816.Implied => "",
			Mode65816.Immediate8 => $"#${data[1]:x2}",
			Mode65816.Immediate16 => $"#${data[1] | (data[2] << 8):x4}",
			Mode65816.ImmediateM => AccumulatorIs8Bit ? $"#${data[1]:x2}" : $"#${data[1] | (data[2] << 8):x4}",
			Mode65816.ImmediateX => IndexIs8Bit ? $"#${data[1]:x2}" : $"#${data[1] | (data[2] << 8):x4}",
			Mode65816.Direct => $"${data[1]:x2}",
			Mode65816.DirectX => $"${data[1]:x2},x",
			Mode65816.DirectY => $"${data[1]:x2},y",
			Mode65816.DirectIndirect => $"(${data[1]:x2})",
			Mode65816.DirectIndirectLong => $"[${data[1]:x2}]",
			Mode65816.DirectIndirectY => $"(${data[1]:x2}),y",
			Mode65816.DirectIndirectLongY => $"[${data[1]:x2}],y",
			Mode65816.DirectIndirectX => $"(${data[1]:x2},x)",
			Mode65816.Absolute => $"${data[1] | (data[2] << 8):x4}",
			Mode65816.AbsoluteX => $"${data[1] | (data[2] << 8):x4},x",
			Mode65816.AbsoluteY => $"${data[1] | (data[2] << 8):x4},y",
			Mode65816.AbsoluteIndirect => $"(${data[1] | (data[2] << 8):x4})",
			Mode65816.AbsoluteIndirectX => $"(${data[1] | (data[2] << 8):x4},x)",
			Mode65816.AbsoluteIndirectLong => $"[${data[1] | (data[2] << 8):x4}]",
			Mode65816.AbsoluteLong => $"${data[1] | (data[2] << 8) | (data[3] << 16):x6}",
			Mode65816.AbsoluteLongX => $"${data[1] | (data[2] << 8) | (data[3] << 16):x6},x",
			Mode65816.Relative8 => FormatRelative8(data, address),
			Mode65816.Relative16 => FormatRelative16(data, address),
			Mode65816.StackRelative => $"${data[1]:x2},s",
			Mode65816.StackRelativeIndirectY => $"(${data[1]:x2},s),y",
			Mode65816.BlockMove => $"${data[1]:x2},${data[2]:x2}",
			_ => ""
		};
	}

	private static string FormatRelative8(ReadOnlySpan<byte> data, uint address) {
		var offset = (sbyte)data[1];
		var target = address + 2 + offset;
		return $"${target:x4}";
	}

	private static string FormatRelative16(ReadOnlySpan<byte> data, uint address) {
		var offset = (short)(data[1] | (data[2] << 8));
		var target = address + 3 + offset;
		return $"${target:x4}";
	}

	private static AddressingMode MapToAddressingMode(Mode65816 mode) {
		return mode switch {
			Mode65816.Implied => AddressingMode.Implied,
			Mode65816.Immediate8 or Mode65816.Immediate16 or Mode65816.ImmediateM or Mode65816.ImmediateX => AddressingMode.Immediate,
			Mode65816.Direct => AddressingMode.ZeroPage,
			Mode65816.DirectX => AddressingMode.ZeroPageX,
			Mode65816.DirectY => AddressingMode.ZeroPageY,
			Mode65816.Absolute or Mode65816.AbsoluteLong => AddressingMode.Absolute,
			Mode65816.AbsoluteX or Mode65816.AbsoluteLongX => AddressingMode.AbsoluteX,
			Mode65816.AbsoluteY => AddressingMode.AbsoluteY,
			Mode65816.AbsoluteIndirect or Mode65816.AbsoluteIndirectLong => AddressingMode.Indirect,
			Mode65816.DirectIndirectX or Mode65816.AbsoluteIndirectX => AddressingMode.IndirectX,
			Mode65816.DirectIndirectY or Mode65816.DirectIndirectLongY => AddressingMode.IndirectY,
			Mode65816.Relative8 or Mode65816.Relative16 => AddressingMode.Relative,
			_ => AddressingMode.Implied
		};
	}

	private static void InitializeOpcodeTable() {
		// Initialize all as undefined
		for (int i = 0; i < 256; i++)
			OpcodeTable[i] = ("???", Mode65816.Implied, 1);

		// === 65816 OPCODES ===
		// Based on official WDC 65816 documentation

		// ADC - Add with Carry
		OpcodeTable[0x69] = ("adc", Mode65816.ImmediateM, 2);
		OpcodeTable[0x65] = ("adc", Mode65816.Direct, 2);
		OpcodeTable[0x75] = ("adc", Mode65816.DirectX, 2);
		OpcodeTable[0x6d] = ("adc", Mode65816.Absolute, 3);
		OpcodeTable[0x7d] = ("adc", Mode65816.AbsoluteX, 3);
		OpcodeTable[0x79] = ("adc", Mode65816.AbsoluteY, 3);
		OpcodeTable[0x72] = ("adc", Mode65816.DirectIndirect, 2);
		OpcodeTable[0x67] = ("adc", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0x71] = ("adc", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0x77] = ("adc", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0x61] = ("adc", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0x6f] = ("adc", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0x7f] = ("adc", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0x63] = ("adc", Mode65816.StackRelative, 2);
		OpcodeTable[0x73] = ("adc", Mode65816.StackRelativeIndirectY, 2);

		// AND - Logical AND
		OpcodeTable[0x29] = ("and", Mode65816.ImmediateM, 2);
		OpcodeTable[0x25] = ("and", Mode65816.Direct, 2);
		OpcodeTable[0x35] = ("and", Mode65816.DirectX, 2);
		OpcodeTable[0x2d] = ("and", Mode65816.Absolute, 3);
		OpcodeTable[0x3d] = ("and", Mode65816.AbsoluteX, 3);
		OpcodeTable[0x39] = ("and", Mode65816.AbsoluteY, 3);
		OpcodeTable[0x32] = ("and", Mode65816.DirectIndirect, 2);
		OpcodeTable[0x27] = ("and", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0x31] = ("and", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0x37] = ("and", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0x21] = ("and", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0x2f] = ("and", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0x3f] = ("and", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0x23] = ("and", Mode65816.StackRelative, 2);
		OpcodeTable[0x33] = ("and", Mode65816.StackRelativeIndirectY, 2);

		// ASL - Arithmetic Shift Left
		OpcodeTable[0x0a] = ("asl", Mode65816.Implied, 1);  // Accumulator
		OpcodeTable[0x06] = ("asl", Mode65816.Direct, 2);
		OpcodeTable[0x16] = ("asl", Mode65816.DirectX, 2);
		OpcodeTable[0x0e] = ("asl", Mode65816.Absolute, 3);
		OpcodeTable[0x1e] = ("asl", Mode65816.AbsoluteX, 3);

		// BCC, BCS, BEQ, BMI, BNE, BPL, BVC, BVS - Branch
		OpcodeTable[0x90] = ("bcc", Mode65816.Relative8, 2);
		OpcodeTable[0xb0] = ("bcs", Mode65816.Relative8, 2);
		OpcodeTable[0xf0] = ("beq", Mode65816.Relative8, 2);
		OpcodeTable[0x30] = ("bmi", Mode65816.Relative8, 2);
		OpcodeTable[0xd0] = ("bne", Mode65816.Relative8, 2);
		OpcodeTable[0x10] = ("bpl", Mode65816.Relative8, 2);
		OpcodeTable[0x50] = ("bvc", Mode65816.Relative8, 2);
		OpcodeTable[0x70] = ("bvs", Mode65816.Relative8, 2);

		// BRA - Branch Always (65816)
		OpcodeTable[0x80] = ("bra", Mode65816.Relative8, 2);

		// BRL - Branch Long (65816)
		OpcodeTable[0x82] = ("brl", Mode65816.Relative16, 3);

		// BIT - Bit Test
		OpcodeTable[0x89] = ("bit", Mode65816.ImmediateM, 2);
		OpcodeTable[0x24] = ("bit", Mode65816.Direct, 2);
		OpcodeTable[0x34] = ("bit", Mode65816.DirectX, 2);
		OpcodeTable[0x2c] = ("bit", Mode65816.Absolute, 3);
		OpcodeTable[0x3c] = ("bit", Mode65816.AbsoluteX, 3);

		// BRK - Software Break
		OpcodeTable[0x00] = ("brk", Mode65816.Immediate8, 2);

		// CLC, CLD, CLI, CLV - Clear flags
		OpcodeTable[0x18] = ("clc", Mode65816.Implied, 1);
		OpcodeTable[0xd8] = ("cld", Mode65816.Implied, 1);
		OpcodeTable[0x58] = ("cli", Mode65816.Implied, 1);
		OpcodeTable[0xb8] = ("clv", Mode65816.Implied, 1);

		// CMP - Compare Accumulator
		OpcodeTable[0xc9] = ("cmp", Mode65816.ImmediateM, 2);
		OpcodeTable[0xc5] = ("cmp", Mode65816.Direct, 2);
		OpcodeTable[0xd5] = ("cmp", Mode65816.DirectX, 2);
		OpcodeTable[0xcd] = ("cmp", Mode65816.Absolute, 3);
		OpcodeTable[0xdd] = ("cmp", Mode65816.AbsoluteX, 3);
		OpcodeTable[0xd9] = ("cmp", Mode65816.AbsoluteY, 3);
		OpcodeTable[0xd2] = ("cmp", Mode65816.DirectIndirect, 2);
		OpcodeTable[0xc7] = ("cmp", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0xd1] = ("cmp", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0xd7] = ("cmp", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0xc1] = ("cmp", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0xcf] = ("cmp", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0xdf] = ("cmp", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0xc3] = ("cmp", Mode65816.StackRelative, 2);
		OpcodeTable[0xd3] = ("cmp", Mode65816.StackRelativeIndirectY, 2);

		// COP - Co-Processor (65816)
		OpcodeTable[0x02] = ("cop", Mode65816.Immediate8, 2);

		// CPX - Compare X
		OpcodeTable[0xe0] = ("cpx", Mode65816.ImmediateX, 2);
		OpcodeTable[0xe4] = ("cpx", Mode65816.Direct, 2);
		OpcodeTable[0xec] = ("cpx", Mode65816.Absolute, 3);

		// CPY - Compare Y
		OpcodeTable[0xc0] = ("cpy", Mode65816.ImmediateX, 2);
		OpcodeTable[0xc4] = ("cpy", Mode65816.Direct, 2);
		OpcodeTable[0xcc] = ("cpy", Mode65816.Absolute, 3);

		// DEC - Decrement
		OpcodeTable[0x3a] = ("dec", Mode65816.Implied, 1);  // Accumulator (65816)
		OpcodeTable[0xc6] = ("dec", Mode65816.Direct, 2);
		OpcodeTable[0xd6] = ("dec", Mode65816.DirectX, 2);
		OpcodeTable[0xce] = ("dec", Mode65816.Absolute, 3);
		OpcodeTable[0xde] = ("dec", Mode65816.AbsoluteX, 3);

		// DEX, DEY - Decrement Index
		OpcodeTable[0xca] = ("dex", Mode65816.Implied, 1);
		OpcodeTable[0x88] = ("dey", Mode65816.Implied, 1);

		// EOR - Exclusive OR
		OpcodeTable[0x49] = ("eor", Mode65816.ImmediateM, 2);
		OpcodeTable[0x45] = ("eor", Mode65816.Direct, 2);
		OpcodeTable[0x55] = ("eor", Mode65816.DirectX, 2);
		OpcodeTable[0x4d] = ("eor", Mode65816.Absolute, 3);
		OpcodeTable[0x5d] = ("eor", Mode65816.AbsoluteX, 3);
		OpcodeTable[0x59] = ("eor", Mode65816.AbsoluteY, 3);
		OpcodeTable[0x52] = ("eor", Mode65816.DirectIndirect, 2);
		OpcodeTable[0x47] = ("eor", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0x51] = ("eor", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0x57] = ("eor", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0x41] = ("eor", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0x4f] = ("eor", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0x5f] = ("eor", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0x43] = ("eor", Mode65816.StackRelative, 2);
		OpcodeTable[0x53] = ("eor", Mode65816.StackRelativeIndirectY, 2);

		// INC - Increment
		OpcodeTable[0x1a] = ("inc", Mode65816.Implied, 1);  // Accumulator (65816)
		OpcodeTable[0xe6] = ("inc", Mode65816.Direct, 2);
		OpcodeTable[0xf6] = ("inc", Mode65816.DirectX, 2);
		OpcodeTable[0xee] = ("inc", Mode65816.Absolute, 3);
		OpcodeTable[0xfe] = ("inc", Mode65816.AbsoluteX, 3);

		// INX, INY - Increment Index
		OpcodeTable[0xe8] = ("inx", Mode65816.Implied, 1);
		OpcodeTable[0xc8] = ("iny", Mode65816.Implied, 1);

		// JMP - Jump
		OpcodeTable[0x4c] = ("jmp", Mode65816.Absolute, 3);
		OpcodeTable[0x6c] = ("jmp", Mode65816.AbsoluteIndirect, 3);
		OpcodeTable[0x7c] = ("jmp", Mode65816.AbsoluteIndirectX, 3);

		// JML - Jump Long (65816)
		OpcodeTable[0x5c] = ("jml", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0xdc] = ("jml", Mode65816.AbsoluteIndirectLong, 3);

		// JSR - Jump to Subroutine
		OpcodeTable[0x20] = ("jsr", Mode65816.Absolute, 3);
		OpcodeTable[0xfc] = ("jsr", Mode65816.AbsoluteIndirectX, 3);

		// JSL - Jump to Subroutine Long (65816)
		OpcodeTable[0x22] = ("jsl", Mode65816.AbsoluteLong, 4);

		// LDA - Load Accumulator
		OpcodeTable[0xa9] = ("lda", Mode65816.ImmediateM, 2);
		OpcodeTable[0xa5] = ("lda", Mode65816.Direct, 2);
		OpcodeTable[0xb5] = ("lda", Mode65816.DirectX, 2);
		OpcodeTable[0xad] = ("lda", Mode65816.Absolute, 3);
		OpcodeTable[0xbd] = ("lda", Mode65816.AbsoluteX, 3);
		OpcodeTable[0xb9] = ("lda", Mode65816.AbsoluteY, 3);
		OpcodeTable[0xb2] = ("lda", Mode65816.DirectIndirect, 2);
		OpcodeTable[0xa7] = ("lda", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0xb1] = ("lda", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0xb7] = ("lda", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0xa1] = ("lda", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0xaf] = ("lda", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0xbf] = ("lda", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0xa3] = ("lda", Mode65816.StackRelative, 2);
		OpcodeTable[0xb3] = ("lda", Mode65816.StackRelativeIndirectY, 2);

		// LDX - Load X
		OpcodeTable[0xa2] = ("ldx", Mode65816.ImmediateX, 2);
		OpcodeTable[0xa6] = ("ldx", Mode65816.Direct, 2);
		OpcodeTable[0xb6] = ("ldx", Mode65816.DirectY, 2);
		OpcodeTable[0xae] = ("ldx", Mode65816.Absolute, 3);
		OpcodeTable[0xbe] = ("ldx", Mode65816.AbsoluteY, 3);

		// LDY - Load Y
		OpcodeTable[0xa0] = ("ldy", Mode65816.ImmediateX, 2);
		OpcodeTable[0xa4] = ("ldy", Mode65816.Direct, 2);
		OpcodeTable[0xb4] = ("ldy", Mode65816.DirectX, 2);
		OpcodeTable[0xac] = ("ldy", Mode65816.Absolute, 3);
		OpcodeTable[0xbc] = ("ldy", Mode65816.AbsoluteX, 3);

		// LSR - Logical Shift Right
		OpcodeTable[0x4a] = ("lsr", Mode65816.Implied, 1);  // Accumulator
		OpcodeTable[0x46] = ("lsr", Mode65816.Direct, 2);
		OpcodeTable[0x56] = ("lsr", Mode65816.DirectX, 2);
		OpcodeTable[0x4e] = ("lsr", Mode65816.Absolute, 3);
		OpcodeTable[0x5e] = ("lsr", Mode65816.AbsoluteX, 3);

		// MVN, MVP - Block Move (65816)
		OpcodeTable[0x54] = ("mvn", Mode65816.BlockMove, 3);
		OpcodeTable[0x44] = ("mvp", Mode65816.BlockMove, 3);

		// NOP - No Operation
		OpcodeTable[0xea] = ("nop", Mode65816.Implied, 1);

		// ORA - Logical OR
		OpcodeTable[0x09] = ("ora", Mode65816.ImmediateM, 2);
		OpcodeTable[0x05] = ("ora", Mode65816.Direct, 2);
		OpcodeTable[0x15] = ("ora", Mode65816.DirectX, 2);
		OpcodeTable[0x0d] = ("ora", Mode65816.Absolute, 3);
		OpcodeTable[0x1d] = ("ora", Mode65816.AbsoluteX, 3);
		OpcodeTable[0x19] = ("ora", Mode65816.AbsoluteY, 3);
		OpcodeTable[0x12] = ("ora", Mode65816.DirectIndirect, 2);
		OpcodeTable[0x07] = ("ora", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0x11] = ("ora", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0x17] = ("ora", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0x01] = ("ora", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0x0f] = ("ora", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0x1f] = ("ora", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0x03] = ("ora", Mode65816.StackRelative, 2);
		OpcodeTable[0x13] = ("ora", Mode65816.StackRelativeIndirectY, 2);

		// PEA, PEI, PER - Push Effective (65816)
		OpcodeTable[0xf4] = ("pea", Mode65816.Absolute, 3);
		OpcodeTable[0xd4] = ("pei", Mode65816.Direct, 2);
		OpcodeTable[0x62] = ("per", Mode65816.Relative16, 3);

		// PHA, PHB, PHD, PHK, PHP, PHX, PHY - Push
		OpcodeTable[0x48] = ("pha", Mode65816.Implied, 1);
		OpcodeTable[0x8b] = ("phb", Mode65816.Implied, 1);
		OpcodeTable[0x0b] = ("phd", Mode65816.Implied, 1);
		OpcodeTable[0x4b] = ("phk", Mode65816.Implied, 1);
		OpcodeTable[0x08] = ("php", Mode65816.Implied, 1);
		OpcodeTable[0xda] = ("phx", Mode65816.Implied, 1);
		OpcodeTable[0x5a] = ("phy", Mode65816.Implied, 1);

		// PLA, PLB, PLD, PLP, PLX, PLY - Pull
		OpcodeTable[0x68] = ("pla", Mode65816.Implied, 1);
		OpcodeTable[0xab] = ("plb", Mode65816.Implied, 1);
		OpcodeTable[0x2b] = ("pld", Mode65816.Implied, 1);
		OpcodeTable[0x28] = ("plp", Mode65816.Implied, 1);
		OpcodeTable[0xfa] = ("plx", Mode65816.Implied, 1);
		OpcodeTable[0x7a] = ("ply", Mode65816.Implied, 1);

		// REP - Reset Processor Status Bits (65816)
		OpcodeTable[0xc2] = ("rep", Mode65816.Immediate8, 2);

		// ROL - Rotate Left
		OpcodeTable[0x2a] = ("rol", Mode65816.Implied, 1);  // Accumulator
		OpcodeTable[0x26] = ("rol", Mode65816.Direct, 2);
		OpcodeTable[0x36] = ("rol", Mode65816.DirectX, 2);
		OpcodeTable[0x2e] = ("rol", Mode65816.Absolute, 3);
		OpcodeTable[0x3e] = ("rol", Mode65816.AbsoluteX, 3);

		// ROR - Rotate Right
		OpcodeTable[0x6a] = ("ror", Mode65816.Implied, 1);  // Accumulator
		OpcodeTable[0x66] = ("ror", Mode65816.Direct, 2);
		OpcodeTable[0x76] = ("ror", Mode65816.DirectX, 2);
		OpcodeTable[0x6e] = ("ror", Mode65816.Absolute, 3);
		OpcodeTable[0x7e] = ("ror", Mode65816.AbsoluteX, 3);

		// RTI - Return from Interrupt
		OpcodeTable[0x40] = ("rti", Mode65816.Implied, 1);

		// RTL - Return Long (65816)
		OpcodeTable[0x6b] = ("rtl", Mode65816.Implied, 1);

		// RTS - Return from Subroutine
		OpcodeTable[0x60] = ("rts", Mode65816.Implied, 1);

		// SBC - Subtract with Carry
		OpcodeTable[0xe9] = ("sbc", Mode65816.ImmediateM, 2);
		OpcodeTable[0xe5] = ("sbc", Mode65816.Direct, 2);
		OpcodeTable[0xf5] = ("sbc", Mode65816.DirectX, 2);
		OpcodeTable[0xed] = ("sbc", Mode65816.Absolute, 3);
		OpcodeTable[0xfd] = ("sbc", Mode65816.AbsoluteX, 3);
		OpcodeTable[0xf9] = ("sbc", Mode65816.AbsoluteY, 3);
		OpcodeTable[0xf2] = ("sbc", Mode65816.DirectIndirect, 2);
		OpcodeTable[0xe7] = ("sbc", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0xf1] = ("sbc", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0xf7] = ("sbc", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0xe1] = ("sbc", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0xef] = ("sbc", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0xff] = ("sbc", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0xe3] = ("sbc", Mode65816.StackRelative, 2);
		OpcodeTable[0xf3] = ("sbc", Mode65816.StackRelativeIndirectY, 2);

		// SEC, SED, SEI - Set flags
		OpcodeTable[0x38] = ("sec", Mode65816.Implied, 1);
		OpcodeTable[0xf8] = ("sed", Mode65816.Implied, 1);
		OpcodeTable[0x78] = ("sei", Mode65816.Implied, 1);

		// SEP - Set Processor Status Bits (65816)
		OpcodeTable[0xe2] = ("sep", Mode65816.Immediate8, 2);

		// STA - Store Accumulator
		OpcodeTable[0x85] = ("sta", Mode65816.Direct, 2);
		OpcodeTable[0x95] = ("sta", Mode65816.DirectX, 2);
		OpcodeTable[0x8d] = ("sta", Mode65816.Absolute, 3);
		OpcodeTable[0x9d] = ("sta", Mode65816.AbsoluteX, 3);
		OpcodeTable[0x99] = ("sta", Mode65816.AbsoluteY, 3);
		OpcodeTable[0x92] = ("sta", Mode65816.DirectIndirect, 2);
		OpcodeTable[0x87] = ("sta", Mode65816.DirectIndirectLong, 2);
		OpcodeTable[0x91] = ("sta", Mode65816.DirectIndirectY, 2);
		OpcodeTable[0x97] = ("sta", Mode65816.DirectIndirectLongY, 2);
		OpcodeTable[0x81] = ("sta", Mode65816.DirectIndirectX, 2);
		OpcodeTable[0x8f] = ("sta", Mode65816.AbsoluteLong, 4);
		OpcodeTable[0x9f] = ("sta", Mode65816.AbsoluteLongX, 4);
		OpcodeTable[0x83] = ("sta", Mode65816.StackRelative, 2);
		OpcodeTable[0x93] = ("sta", Mode65816.StackRelativeIndirectY, 2);

		// STP - Stop Processor (65816)
		OpcodeTable[0xdb] = ("stp", Mode65816.Implied, 1);

		// STX - Store X
		OpcodeTable[0x86] = ("stx", Mode65816.Direct, 2);
		OpcodeTable[0x96] = ("stx", Mode65816.DirectY, 2);
		OpcodeTable[0x8e] = ("stx", Mode65816.Absolute, 3);

		// STY - Store Y
		OpcodeTable[0x84] = ("sty", Mode65816.Direct, 2);
		OpcodeTable[0x94] = ("sty", Mode65816.DirectX, 2);
		OpcodeTable[0x8c] = ("sty", Mode65816.Absolute, 3);

		// STZ - Store Zero (65816)
		OpcodeTable[0x64] = ("stz", Mode65816.Direct, 2);
		OpcodeTable[0x74] = ("stz", Mode65816.DirectX, 2);
		OpcodeTable[0x9c] = ("stz", Mode65816.Absolute, 3);
		OpcodeTable[0x9e] = ("stz", Mode65816.AbsoluteX, 3);

		// TAX, TAY, TCD, TCS, TDC, TSC, TSX, TXA, TXS, TXY, TYA, TYX - Transfer
		OpcodeTable[0xaa] = ("tax", Mode65816.Implied, 1);
		OpcodeTable[0xa8] = ("tay", Mode65816.Implied, 1);
		OpcodeTable[0x5b] = ("tcd", Mode65816.Implied, 1);
		OpcodeTable[0x1b] = ("tcs", Mode65816.Implied, 1);
		OpcodeTable[0x7b] = ("tdc", Mode65816.Implied, 1);
		OpcodeTable[0x3b] = ("tsc", Mode65816.Implied, 1);
		OpcodeTable[0xba] = ("tsx", Mode65816.Implied, 1);
		OpcodeTable[0x8a] = ("txa", Mode65816.Implied, 1);
		OpcodeTable[0x9a] = ("txs", Mode65816.Implied, 1);
		OpcodeTable[0x9b] = ("txy", Mode65816.Implied, 1);
		OpcodeTable[0x98] = ("tya", Mode65816.Implied, 1);
		OpcodeTable[0xbb] = ("tyx", Mode65816.Implied, 1);

		// TRB - Test and Reset Bits (65816)
		OpcodeTable[0x14] = ("trb", Mode65816.Direct, 2);
		OpcodeTable[0x1c] = ("trb", Mode65816.Absolute, 3);

		// TSB - Test and Set Bits (65816)
		OpcodeTable[0x04] = ("tsb", Mode65816.Direct, 2);
		OpcodeTable[0x0c] = ("tsb", Mode65816.Absolute, 3);

		// WAI - Wait for Interrupt (65816)
		OpcodeTable[0xcb] = ("wai", Mode65816.Implied, 1);

		// WDM - Reserved (65816)
		OpcodeTable[0x42] = ("wdm", Mode65816.Immediate8, 2);

		// XBA - Exchange B and A (65816)
		OpcodeTable[0xeb] = ("xba", Mode65816.Implied, 1);

		// XCE - Exchange Carry and Emulation (65816)
		OpcodeTable[0xfb] = ("xce", Mode65816.Implied, 1);
	}
}
