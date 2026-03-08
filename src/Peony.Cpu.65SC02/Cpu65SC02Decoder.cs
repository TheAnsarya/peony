namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// WDC 65SC02 CPU instruction decoder for Atari Lynx
/// </summary>
/// <remarks>
/// <para>
/// The 65SC02 is a variant of the 65C02 without the bit manipulation instructions
/// (RMB, SMB, BBR, BBS). It is used in the Atari Lynx handheld console.
/// </para>
/// <para>
/// New instructions over the 6502:
/// - BRA: Branch Relative Always
/// - PHX, PLX, PHY, PLY: Push/Pull X and Y registers
/// - STZ: Store Zero to memory
/// - TRB, TSB: Test and Reset/Set Bits
/// - INC, DEC: Accumulator mode (no operand)
/// - (zp): Zero Page Indirect addressing mode
/// - JMP (abs,X): Indexed Indirect Jump
/// </para>
/// </remarks>
public sealed class Cpu65SC02Decoder : ICpuDecoder {
	/// <summary>
	/// Gets the architecture name.
	/// </summary>
	public string Architecture => "65SC02";

	// Opcode table: (mnemonic, addressing mode, bytes, isIllegal)
	private static readonly (string Mnemonic, AddressingMode Mode, int Bytes, bool Illegal)[] OpcodeTable = new (string, AddressingMode, int, bool)[256];

	static Cpu65SC02Decoder() {
		InitializeOpcodeTable();
	}

	/// <summary>
	/// Decodes an instruction at the given address.
	/// </summary>
	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		var opcode = data[0];
		var (mnemonic, mode, length, illegal) = OpcodeTable[opcode];

		if (data.Length < length)
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		var bytes = data[..length].ToArray();
		var operand = FormatOperand(data, mode, address);

		return new DecodedInstruction(mnemonic, operand, bytes, mode);
	}

	/// <summary>
	/// Checks if an instruction affects control flow.
	/// </summary>
	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jmp" or "jsr" or "rts" or "rti" or "brk" or
			"bcc" or "bcs" or "beq" or "bmi" or "bne" or "bpl" or "bvc" or "bvs" or
			"bra"; // 65C02 unconditional branch
	}

	/// <summary>
	/// Gets the target addresses for a control flow instruction.
	/// </summary>
	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		switch (instruction.Mode) {
			case AddressingMode.Absolute when instruction.Mnemonic is "jmp" or "jsr":
				yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				if (instruction.Mnemonic == "jsr")
					yield return nextAddress;
				break;

			case AddressingMode.Relative:
				var offset = (sbyte)instruction.Bytes[1];
				yield return (uint)(nextAddress + offset);
				// BRA is unconditional, no fallthrough
				if (instruction.Mnemonic != "bra")
					yield return nextAddress;
				break;
		}
	}

	private static string FormatOperand(ReadOnlySpan<byte> data, AddressingMode mode, uint address) {
		return mode switch {
			AddressingMode.Implied or AddressingMode.Accumulator => "",
			AddressingMode.Immediate => $"#${data[1]:x2}",
			AddressingMode.ZeroPage => $"${data[1]:x2}",
			AddressingMode.ZeroPageX => $"${data[1]:x2},x",
			AddressingMode.ZeroPageY => $"${data[1]:x2},y",
			AddressingMode.Absolute => $"${data[1] | (data[2] << 8):x4}",
			AddressingMode.AbsoluteX => $"${data[1] | (data[2] << 8):x4},x",
			AddressingMode.AbsoluteY => $"${data[1] | (data[2] << 8):x4},y",
			AddressingMode.Indirect => $"(${data[1] | (data[2] << 8):x4})",
			AddressingMode.IndirectX => $"(${data[1]:x2},x)",
			AddressingMode.IndirectY => $"(${data[1]:x2}),y",
			AddressingMode.ZeroPageIndirect => $"(${data[1]:x2})", // 65C02 mode
			AddressingMode.AbsoluteIndirectX => $"(${data[1] | (data[2] << 8):x4},x)", // 65C02 JMP mode
			AddressingMode.Relative => FormatRelative(data, address),
			_ => ""
		};
	}

	private static string FormatRelative(ReadOnlySpan<byte> data, uint address) {
		var offset = (sbyte)data[1];
		var target = address + 2 + offset;
		return $"${target:x4}";
	}

	private static void InitializeOpcodeTable() {
		// Initialize all as NOPs (the 65C02 family has no illegal opcodes)
		for (int i = 0; i < 256; i++)
			OpcodeTable[i] = ("nop", AddressingMode.Implied, 1, false);

		// === BASE 6502 OPCODES ===

		// ADC
		OpcodeTable[0x69] = ("adc", AddressingMode.Immediate, 2, false);
		OpcodeTable[0x65] = ("adc", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x75] = ("adc", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x6d] = ("adc", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x7d] = ("adc", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x79] = ("adc", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0x61] = ("adc", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0x71] = ("adc", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0x72] = ("adc", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// AND
		OpcodeTable[0x29] = ("and", AddressingMode.Immediate, 2, false);
		OpcodeTable[0x25] = ("and", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x35] = ("and", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x2d] = ("and", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x3d] = ("and", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x39] = ("and", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0x21] = ("and", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0x31] = ("and", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0x32] = ("and", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// ASL
		OpcodeTable[0x0a] = ("asl", AddressingMode.Accumulator, 1, false);
		OpcodeTable[0x06] = ("asl", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x16] = ("asl", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x0e] = ("asl", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x1e] = ("asl", AddressingMode.AbsoluteX, 3, false);

		// BCC, BCS, BEQ, BMI, BNE, BPL, BVC, BVS
		OpcodeTable[0x90] = ("bcc", AddressingMode.Relative, 2, false);
		OpcodeTable[0xb0] = ("bcs", AddressingMode.Relative, 2, false);
		OpcodeTable[0xf0] = ("beq", AddressingMode.Relative, 2, false);
		OpcodeTable[0x30] = ("bmi", AddressingMode.Relative, 2, false);
		OpcodeTable[0xd0] = ("bne", AddressingMode.Relative, 2, false);
		OpcodeTable[0x10] = ("bpl", AddressingMode.Relative, 2, false);
		OpcodeTable[0x50] = ("bvc", AddressingMode.Relative, 2, false);
		OpcodeTable[0x70] = ("bvs", AddressingMode.Relative, 2, false);

		// BRA - 65C02 unconditional branch
		OpcodeTable[0x80] = ("bra", AddressingMode.Relative, 2, false);

		// BIT
		OpcodeTable[0x24] = ("bit", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x2c] = ("bit", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x89] = ("bit", AddressingMode.Immediate, 2, false); // 65C02
		OpcodeTable[0x34] = ("bit", AddressingMode.ZeroPageX, 2, false); // 65C02
		OpcodeTable[0x3c] = ("bit", AddressingMode.AbsoluteX, 3, false); // 65C02

		// BRK
		OpcodeTable[0x00] = ("brk", AddressingMode.Implied, 1, false);

		// CLC, CLD, CLI, CLV
		OpcodeTable[0x18] = ("clc", AddressingMode.Implied, 1, false);
		OpcodeTable[0xd8] = ("cld", AddressingMode.Implied, 1, false);
		OpcodeTable[0x58] = ("cli", AddressingMode.Implied, 1, false);
		OpcodeTable[0xb8] = ("clv", AddressingMode.Implied, 1, false);

		// CMP
		OpcodeTable[0xc9] = ("cmp", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xc5] = ("cmp", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xd5] = ("cmp", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xcd] = ("cmp", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xdd] = ("cmp", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0xd9] = ("cmp", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0xc1] = ("cmp", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0xd1] = ("cmp", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0xd2] = ("cmp", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// CPX
		OpcodeTable[0xe0] = ("cpx", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xe4] = ("cpx", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xec] = ("cpx", AddressingMode.Absolute, 3, false);

		// CPY
		OpcodeTable[0xc0] = ("cpy", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xc4] = ("cpy", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xcc] = ("cpy", AddressingMode.Absolute, 3, false);

		// DEC
		OpcodeTable[0xc6] = ("dec", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xd6] = ("dec", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xce] = ("dec", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xde] = ("dec", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x3a] = ("dec", AddressingMode.Accumulator, 1, false); // 65C02 (DEA)

		// DEX, DEY
		OpcodeTable[0xca] = ("dex", AddressingMode.Implied, 1, false);
		OpcodeTable[0x88] = ("dey", AddressingMode.Implied, 1, false);

		// EOR
		OpcodeTable[0x49] = ("eor", AddressingMode.Immediate, 2, false);
		OpcodeTable[0x45] = ("eor", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x55] = ("eor", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x4d] = ("eor", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x5d] = ("eor", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x59] = ("eor", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0x41] = ("eor", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0x51] = ("eor", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0x52] = ("eor", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// INC
		OpcodeTable[0xe6] = ("inc", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xf6] = ("inc", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xee] = ("inc", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xfe] = ("inc", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x1a] = ("inc", AddressingMode.Accumulator, 1, false); // 65C02 (INA)

		// INX, INY
		OpcodeTable[0xe8] = ("inx", AddressingMode.Implied, 1, false);
		OpcodeTable[0xc8] = ("iny", AddressingMode.Implied, 1, false);

		// JMP
		OpcodeTable[0x4c] = ("jmp", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x6c] = ("jmp", AddressingMode.Indirect, 3, false);
		OpcodeTable[0x7c] = ("jmp", AddressingMode.AbsoluteIndirectX, 3, false); // 65C02

		// JSR
		OpcodeTable[0x20] = ("jsr", AddressingMode.Absolute, 3, false);

		// LDA
		OpcodeTable[0xa9] = ("lda", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xa5] = ("lda", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xb5] = ("lda", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xad] = ("lda", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xbd] = ("lda", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0xb9] = ("lda", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0xa1] = ("lda", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0xb1] = ("lda", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0xb2] = ("lda", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// LDX
		OpcodeTable[0xa2] = ("ldx", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xa6] = ("ldx", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xb6] = ("ldx", AddressingMode.ZeroPageY, 2, false);
		OpcodeTable[0xae] = ("ldx", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xbe] = ("ldx", AddressingMode.AbsoluteY, 3, false);

		// LDY
		OpcodeTable[0xa0] = ("ldy", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xa4] = ("ldy", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xb4] = ("ldy", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xac] = ("ldy", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xbc] = ("ldy", AddressingMode.AbsoluteX, 3, false);

		// LSR
		OpcodeTable[0x4a] = ("lsr", AddressingMode.Accumulator, 1, false);
		OpcodeTable[0x46] = ("lsr", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x56] = ("lsr", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x4e] = ("lsr", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x5e] = ("lsr", AddressingMode.AbsoluteX, 3, false);

		// NOP
		OpcodeTable[0xea] = ("nop", AddressingMode.Implied, 1, false);

		// ORA
		OpcodeTable[0x09] = ("ora", AddressingMode.Immediate, 2, false);
		OpcodeTable[0x05] = ("ora", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x15] = ("ora", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x0d] = ("ora", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x1d] = ("ora", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x19] = ("ora", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0x01] = ("ora", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0x11] = ("ora", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0x12] = ("ora", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// PHA, PHP
		OpcodeTable[0x48] = ("pha", AddressingMode.Implied, 1, false);
		OpcodeTable[0x08] = ("php", AddressingMode.Implied, 1, false);

		// PHX, PHY - 65C02
		OpcodeTable[0xda] = ("phx", AddressingMode.Implied, 1, false);
		OpcodeTable[0x5a] = ("phy", AddressingMode.Implied, 1, false);

		// PLA, PLP
		OpcodeTable[0x68] = ("pla", AddressingMode.Implied, 1, false);
		OpcodeTable[0x28] = ("plp", AddressingMode.Implied, 1, false);

		// PLX, PLY - 65C02
		OpcodeTable[0xfa] = ("plx", AddressingMode.Implied, 1, false);
		OpcodeTable[0x7a] = ("ply", AddressingMode.Implied, 1, false);

		// ROL
		OpcodeTable[0x2a] = ("rol", AddressingMode.Accumulator, 1, false);
		OpcodeTable[0x26] = ("rol", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x36] = ("rol", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x2e] = ("rol", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x3e] = ("rol", AddressingMode.AbsoluteX, 3, false);

		// ROR
		OpcodeTable[0x6a] = ("ror", AddressingMode.Accumulator, 1, false);
		OpcodeTable[0x66] = ("ror", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x76] = ("ror", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x6e] = ("ror", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x7e] = ("ror", AddressingMode.AbsoluteX, 3, false);

		// RTI, RTS
		OpcodeTable[0x40] = ("rti", AddressingMode.Implied, 1, false);
		OpcodeTable[0x60] = ("rts", AddressingMode.Implied, 1, false);

		// SBC
		OpcodeTable[0xe9] = ("sbc", AddressingMode.Immediate, 2, false);
		OpcodeTable[0xe5] = ("sbc", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0xf5] = ("sbc", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0xed] = ("sbc", AddressingMode.Absolute, 3, false);
		OpcodeTable[0xfd] = ("sbc", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0xf9] = ("sbc", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0xe1] = ("sbc", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0xf1] = ("sbc", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0xf2] = ("sbc", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// SEC, SED, SEI
		OpcodeTable[0x38] = ("sec", AddressingMode.Implied, 1, false);
		OpcodeTable[0xf8] = ("sed", AddressingMode.Implied, 1, false);
		OpcodeTable[0x78] = ("sei", AddressingMode.Implied, 1, false);

		// STA
		OpcodeTable[0x85] = ("sta", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x95] = ("sta", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x8d] = ("sta", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x9d] = ("sta", AddressingMode.AbsoluteX, 3, false);
		OpcodeTable[0x99] = ("sta", AddressingMode.AbsoluteY, 3, false);
		OpcodeTable[0x81] = ("sta", AddressingMode.IndirectX, 2, false);
		OpcodeTable[0x91] = ("sta", AddressingMode.IndirectY, 2, false);
		OpcodeTable[0x92] = ("sta", AddressingMode.ZeroPageIndirect, 2, false); // 65C02

		// STX
		OpcodeTable[0x86] = ("stx", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x96] = ("stx", AddressingMode.ZeroPageY, 2, false);
		OpcodeTable[0x8e] = ("stx", AddressingMode.Absolute, 3, false);

		// STY
		OpcodeTable[0x84] = ("sty", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x94] = ("sty", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x8c] = ("sty", AddressingMode.Absolute, 3, false);

		// STZ - 65C02 Store Zero
		OpcodeTable[0x64] = ("stz", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x74] = ("stz", AddressingMode.ZeroPageX, 2, false);
		OpcodeTable[0x9c] = ("stz", AddressingMode.Absolute, 3, false);
		OpcodeTable[0x9e] = ("stz", AddressingMode.AbsoluteX, 3, false);

		// TAX, TAY, TSX, TXA, TXS, TYA
		OpcodeTable[0xaa] = ("tax", AddressingMode.Implied, 1, false);
		OpcodeTable[0xa8] = ("tay", AddressingMode.Implied, 1, false);
		OpcodeTable[0xba] = ("tsx", AddressingMode.Implied, 1, false);
		OpcodeTable[0x8a] = ("txa", AddressingMode.Implied, 1, false);
		OpcodeTable[0x9a] = ("txs", AddressingMode.Implied, 1, false);
		OpcodeTable[0x98] = ("tya", AddressingMode.Implied, 1, false);

		// TRB - 65C02 Test and Reset Bits
		OpcodeTable[0x14] = ("trb", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x1c] = ("trb", AddressingMode.Absolute, 3, false);

		// TSB - 65C02 Test and Set Bits
		OpcodeTable[0x04] = ("tsb", AddressingMode.ZeroPage, 2, false);
		OpcodeTable[0x0c] = ("tsb", AddressingMode.Absolute, 3, false);
	}
}
