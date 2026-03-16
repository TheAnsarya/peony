namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// Hudson HuC6280 CPU decoder for PC Engine / TurboGrafx-16.
/// The HuC6280 is a 65C02 superset with block transfers, VDC I/O,
/// memory mapping (TAM/TMA), and additional register ops.
/// </summary>
public sealed class HuC6280Decoder : ICpuDecoder {
	public string Architecture => "HuC6280";

	// Full 256-entry opcode table
	private static readonly (string Mnemonic, AddressingMode Mode, int Bytes)[] OpcodeTable = new (string, AddressingMode, int)[256];

	static HuC6280Decoder() {
		InitializeOpcodeTable();
	}

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		var opcode = data[0];
		var (mnemonic, mode, length) = OpcodeTable[opcode];

		if (data.Length < length)
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		var bytes = data[..length].ToArray();
		var operand = FormatOperand(data, opcode, mode, length, address);

		return new DecodedInstruction(mnemonic, operand, bytes, mode);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jmp" or "jsr" or "bsr" or "rts" or "rti" or "brk" or
			"bra" or "bcc" or "bcs" or "beq" or "bne" or "bmi" or "bpl" or "bvs" or "bvc" or
			"bbr0" or "bbr1" or "bbr2" or "bbr3" or "bbr4" or "bbr5" or "bbr6" or "bbr7" or
			"bbs0" or "bbs1" or "bbs2" or "bbs3" or "bbs4" or "bbs5" or "bbs6" or "bbs7";
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		switch (instruction.Mnemonic) {
			case "jmp" when instruction.Mode == AddressingMode.Absolute:
				yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				break;

			case "jsr" or "bsr":
				if (instruction.Mnemonic == "jsr") {
					yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				} else {
					// BSR is relative
					var offset16 = (short)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
					yield return (uint)(nextAddress + offset16);
				}
				yield return nextAddress;
				break;

			case "bra":
				yield return (uint)(nextAddress + (sbyte)instruction.Bytes[1]);
				break;

			case "bcc" or "bcs" or "beq" or "bne" or "bmi" or "bpl" or "bvs" or "bvc":
				yield return (uint)(nextAddress + (sbyte)instruction.Bytes[1]);
				yield return nextAddress;
				break;

			// BBR/BBS: test zero-page bit, branch relative
			case var m when m.StartsWith("bbr") || m.StartsWith("bbs"):
				yield return (uint)(nextAddress + (sbyte)instruction.Bytes[2]);
				yield return nextAddress;
				break;
		}
	}

	private static string FormatOperand(ReadOnlySpan<byte> data, byte opcode, AddressingMode mode, int length, uint address) {
		// Block transfer instructions (7 bytes: opcode, src_lo, src_hi, dst_lo, dst_hi, len_lo, len_hi)
		if (opcode is 0x73 or 0xc3 or 0xd3 or 0xe3 or 0xf3) {
			var src = data[1] | (data[2] << 8);
			var dst = data[3] | (data[4] << 8);
			var len = data[5] | (data[6] << 8);
			return $"${src:x4},${dst:x4},${len:x4}";
		}

		// BSR — 16-bit relative
		if (opcode == 0x44) {
			var offset16 = (short)(data[1] | (data[2] << 8));
			var target = (uint)(address + 3 + offset16);
			return $"${target:x4}";
		}

		return mode switch {
			AddressingMode.Implied => "",
			AddressingMode.Accumulator => "a",
			AddressingMode.Immediate => $"#${data[1]:x2}",
			AddressingMode.ZeroPage => $"${data[1]:x2}",
			AddressingMode.ZeroPageX => $"${data[1]:x2},x",
			AddressingMode.ZeroPageY => $"${data[1]:x2},y",
			AddressingMode.Absolute => $"${(data[1] | (data[2] << 8)):x4}",
			AddressingMode.AbsoluteX => $"${(data[1] | (data[2] << 8)):x4},x",
			AddressingMode.AbsoluteY => $"${(data[1] | (data[2] << 8)):x4},y",
			AddressingMode.Indirect => $"(${(data[1] | (data[2] << 8)):x4})",
			AddressingMode.IndirectX => $"(${data[1]:x2},x)",
			AddressingMode.IndirectY => $"(${data[1]:x2}),y",
			AddressingMode.Relative => FormatRelative(data, address, length),
			AddressingMode.ZeroPageIndirect => $"(${data[1]:x2})",
			AddressingMode.AbsoluteIndirectX => $"(${(data[1] | (data[2] << 8)):x4},x)",
			// ZP bit test + relative (BBR/BBS): 3 bytes
			AddressingMode.ZeroPageRelative => $"${data[1]:x2},${(uint)(address + 3 + (sbyte)data[2]):x4}",
			_ => ""
		};
	}

	private static string FormatRelative(ReadOnlySpan<byte> data, uint address, int length) {
		var offset = (sbyte)data[1];
		var target = (uint)(address + length + offset);
		return $"${target:x4}";
	}

	private static void InitializeOpcodeTable() {
		// Default all to NOP/1
		for (int i = 0; i < 256; i++)
			OpcodeTable[i] = ("nop", AddressingMode.Implied, 1);

		// === Standard 65C02 instructions ===
		OpcodeTable[0x00] = ("brk", AddressingMode.Implied, 1);
		OpcodeTable[0x01] = ("ora", AddressingMode.IndirectX, 2);
		OpcodeTable[0x02] = ("sxy", AddressingMode.Implied, 1); // HuC6280: swap X,Y
		OpcodeTable[0x03] = ("st0", AddressingMode.Immediate, 2); // HuC6280: VDC I/O
		OpcodeTable[0x04] = ("tsb", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x05] = ("ora", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x06] = ("asl", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x07] = ("rmb0", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x08] = ("php", AddressingMode.Implied, 1);
		OpcodeTable[0x09] = ("ora", AddressingMode.Immediate, 2);
		OpcodeTable[0x0a] = ("asl", AddressingMode.Accumulator, 1);
		OpcodeTable[0x0c] = ("tsb", AddressingMode.Absolute, 3);
		OpcodeTable[0x0d] = ("ora", AddressingMode.Absolute, 3);
		OpcodeTable[0x0e] = ("asl", AddressingMode.Absolute, 3);
		OpcodeTable[0x0f] = ("bbr0", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x10] = ("bpl", AddressingMode.Relative, 2);
		OpcodeTable[0x11] = ("ora", AddressingMode.IndirectY, 2);
		OpcodeTable[0x12] = ("ora", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0x13] = ("st1", AddressingMode.Immediate, 2); // HuC6280: VDC I/O
		OpcodeTable[0x14] = ("trb", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x15] = ("ora", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x16] = ("asl", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x17] = ("rmb1", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x18] = ("clc", AddressingMode.Implied, 1);
		OpcodeTable[0x19] = ("ora", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0x1a] = ("inc", AddressingMode.Accumulator, 1);
		OpcodeTable[0x1c] = ("trb", AddressingMode.Absolute, 3);
		OpcodeTable[0x1d] = ("ora", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x1e] = ("asl", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x1f] = ("bbr1", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x20] = ("jsr", AddressingMode.Absolute, 3);
		OpcodeTable[0x21] = ("and", AddressingMode.IndirectX, 2);
		OpcodeTable[0x22] = ("sax", AddressingMode.Implied, 1); // HuC6280: swap A,X
		OpcodeTable[0x23] = ("st2", AddressingMode.Immediate, 2); // HuC6280: VDC I/O
		OpcodeTable[0x24] = ("bit", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x25] = ("and", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x26] = ("rol", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x27] = ("rmb2", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x28] = ("plp", AddressingMode.Implied, 1);
		OpcodeTable[0x29] = ("and", AddressingMode.Immediate, 2);
		OpcodeTable[0x2a] = ("rol", AddressingMode.Accumulator, 1);
		OpcodeTable[0x2c] = ("bit", AddressingMode.Absolute, 3);
		OpcodeTable[0x2d] = ("and", AddressingMode.Absolute, 3);
		OpcodeTable[0x2e] = ("rol", AddressingMode.Absolute, 3);
		OpcodeTable[0x2f] = ("bbr2", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x30] = ("bmi", AddressingMode.Relative, 2);
		OpcodeTable[0x31] = ("and", AddressingMode.IndirectY, 2);
		OpcodeTable[0x32] = ("and", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0x34] = ("bit", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x35] = ("and", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x36] = ("rol", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x37] = ("rmb3", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x38] = ("sec", AddressingMode.Implied, 1);
		OpcodeTable[0x39] = ("and", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0x3a] = ("dec", AddressingMode.Accumulator, 1);
		OpcodeTable[0x3c] = ("bit", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x3d] = ("and", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x3e] = ("rol", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x3f] = ("bbr3", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x40] = ("rti", AddressingMode.Implied, 1);
		OpcodeTable[0x41] = ("eor", AddressingMode.IndirectX, 2);
		OpcodeTable[0x42] = ("say", AddressingMode.Implied, 1); // HuC6280: swap A,Y
		OpcodeTable[0x43] = ("tma", AddressingMode.Immediate, 2); // HuC6280: read MPR
		OpcodeTable[0x44] = ("bsr", AddressingMode.Absolute, 3); // HuC6280: branch to subroutine (relative)
		OpcodeTable[0x45] = ("eor", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x46] = ("lsr", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x47] = ("rmb4", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x48] = ("pha", AddressingMode.Implied, 1);
		OpcodeTable[0x49] = ("eor", AddressingMode.Immediate, 2);
		OpcodeTable[0x4a] = ("lsr", AddressingMode.Accumulator, 1);
		OpcodeTable[0x4c] = ("jmp", AddressingMode.Absolute, 3);
		OpcodeTable[0x4d] = ("eor", AddressingMode.Absolute, 3);
		OpcodeTable[0x4e] = ("lsr", AddressingMode.Absolute, 3);
		OpcodeTable[0x4f] = ("bbr4", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x50] = ("bvc", AddressingMode.Relative, 2);
		OpcodeTable[0x51] = ("eor", AddressingMode.IndirectY, 2);
		OpcodeTable[0x52] = ("eor", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0x53] = ("tam", AddressingMode.Immediate, 2); // HuC6280: write MPR
		OpcodeTable[0x54] = ("csl", AddressingMode.Implied, 1); // HuC6280: low speed
		OpcodeTable[0x55] = ("eor", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x56] = ("lsr", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x57] = ("rmb5", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x58] = ("cli", AddressingMode.Implied, 1);
		OpcodeTable[0x59] = ("eor", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0x5a] = ("phy", AddressingMode.Implied, 1);
		OpcodeTable[0x5d] = ("eor", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x5e] = ("lsr", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x5f] = ("bbr5", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x60] = ("rts", AddressingMode.Implied, 1);
		OpcodeTable[0x61] = ("adc", AddressingMode.IndirectX, 2);
		OpcodeTable[0x62] = ("cla", AddressingMode.Implied, 1); // HuC6280: clear A
		OpcodeTable[0x64] = ("stz", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x65] = ("adc", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x66] = ("ror", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x67] = ("rmb6", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x68] = ("pla", AddressingMode.Implied, 1);
		OpcodeTable[0x69] = ("adc", AddressingMode.Immediate, 2);
		OpcodeTable[0x6a] = ("ror", AddressingMode.Accumulator, 1);
		OpcodeTable[0x6c] = ("jmp", AddressingMode.Indirect, 3);
		OpcodeTable[0x6d] = ("adc", AddressingMode.Absolute, 3);
		OpcodeTable[0x6e] = ("ror", AddressingMode.Absolute, 3);
		OpcodeTable[0x6f] = ("bbr6", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x70] = ("bvs", AddressingMode.Relative, 2);
		OpcodeTable[0x71] = ("adc", AddressingMode.IndirectY, 2);
		OpcodeTable[0x72] = ("adc", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0x73] = ("tii", AddressingMode.Implied, 7); // HuC6280: block transfer increment
		OpcodeTable[0x74] = ("stz", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x75] = ("adc", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x76] = ("ror", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x77] = ("rmb7", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x78] = ("sei", AddressingMode.Implied, 1);
		OpcodeTable[0x79] = ("adc", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0x7a] = ("ply", AddressingMode.Implied, 1);
		OpcodeTable[0x7c] = ("jmp", AddressingMode.AbsoluteIndirectX, 3);
		OpcodeTable[0x7d] = ("adc", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x7e] = ("ror", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x7f] = ("bbr7", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x80] = ("bra", AddressingMode.Relative, 2);
		OpcodeTable[0x81] = ("sta", AddressingMode.IndirectX, 2);
		OpcodeTable[0x82] = ("clx", AddressingMode.Implied, 1); // HuC6280: clear X
		OpcodeTable[0x84] = ("sty", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x85] = ("sta", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x86] = ("stx", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x87] = ("smb0", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x88] = ("dey", AddressingMode.Implied, 1);
		OpcodeTable[0x89] = ("bit", AddressingMode.Immediate, 2);
		OpcodeTable[0x8a] = ("txa", AddressingMode.Implied, 1);
		OpcodeTable[0x8c] = ("sty", AddressingMode.Absolute, 3);
		OpcodeTable[0x8d] = ("sta", AddressingMode.Absolute, 3);
		OpcodeTable[0x8e] = ("stx", AddressingMode.Absolute, 3);
		OpcodeTable[0x8f] = ("bbs0", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0x90] = ("bcc", AddressingMode.Relative, 2);
		OpcodeTable[0x91] = ("sta", AddressingMode.IndirectY, 2);
		OpcodeTable[0x92] = ("sta", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0x94] = ("sty", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x95] = ("sta", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0x96] = ("stx", AddressingMode.ZeroPageY, 2);
		OpcodeTable[0x97] = ("smb1", AddressingMode.ZeroPage, 2);
		OpcodeTable[0x98] = ("tya", AddressingMode.Implied, 1);
		OpcodeTable[0x99] = ("sta", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0x9a] = ("txs", AddressingMode.Implied, 1);
		OpcodeTable[0x9c] = ("stz", AddressingMode.Absolute, 3);
		OpcodeTable[0x9d] = ("sta", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x9e] = ("stz", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0x9f] = ("bbs1", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xa0] = ("ldy", AddressingMode.Immediate, 2);
		OpcodeTable[0xa1] = ("lda", AddressingMode.IndirectX, 2);
		OpcodeTable[0xa2] = ("ldx", AddressingMode.Immediate, 2);
		OpcodeTable[0xa4] = ("ldy", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xa5] = ("lda", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xa6] = ("ldx", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xa7] = ("smb2", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xa8] = ("tay", AddressingMode.Implied, 1);
		OpcodeTable[0xa9] = ("lda", AddressingMode.Immediate, 2);
		OpcodeTable[0xaa] = ("tax", AddressingMode.Implied, 1);
		OpcodeTable[0xac] = ("ldy", AddressingMode.Absolute, 3);
		OpcodeTable[0xad] = ("lda", AddressingMode.Absolute, 3);
		OpcodeTable[0xae] = ("ldx", AddressingMode.Absolute, 3);
		OpcodeTable[0xaf] = ("bbs2", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xb0] = ("bcs", AddressingMode.Relative, 2);
		OpcodeTable[0xb1] = ("lda", AddressingMode.IndirectY, 2);
		OpcodeTable[0xb2] = ("lda", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0xb4] = ("ldy", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xb5] = ("lda", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xb6] = ("ldx", AddressingMode.ZeroPageY, 2);
		OpcodeTable[0xb7] = ("smb3", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xb8] = ("clv", AddressingMode.Implied, 1);
		OpcodeTable[0xb9] = ("lda", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0xba] = ("tsx", AddressingMode.Implied, 1);
		OpcodeTable[0xbc] = ("ldy", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xbd] = ("lda", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xbe] = ("ldx", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0xbf] = ("bbs3", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xc0] = ("cpy", AddressingMode.Immediate, 2);
		OpcodeTable[0xc1] = ("cmp", AddressingMode.IndirectX, 2);
		OpcodeTable[0xc2] = ("cly", AddressingMode.Implied, 1); // HuC6280: clear Y
		OpcodeTable[0xc3] = ("tdd", AddressingMode.Implied, 7); // HuC6280: block transfer decrement
		OpcodeTable[0xc4] = ("cpy", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xc5] = ("cmp", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xc6] = ("dec", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xc7] = ("smb4", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xc8] = ("iny", AddressingMode.Implied, 1);
		OpcodeTable[0xc9] = ("cmp", AddressingMode.Immediate, 2);
		OpcodeTable[0xca] = ("dex", AddressingMode.Implied, 1);
		OpcodeTable[0xcc] = ("cpy", AddressingMode.Absolute, 3);
		OpcodeTable[0xcd] = ("cmp", AddressingMode.Absolute, 3);
		OpcodeTable[0xce] = ("dec", AddressingMode.Absolute, 3);
		OpcodeTable[0xcf] = ("bbs4", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xd0] = ("bne", AddressingMode.Relative, 2);
		OpcodeTable[0xd1] = ("cmp", AddressingMode.IndirectY, 2);
		OpcodeTable[0xd2] = ("cmp", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0xd3] = ("tin", AddressingMode.Implied, 7); // HuC6280: block transfer to I/O
		OpcodeTable[0xd4] = ("csh", AddressingMode.Implied, 1); // HuC6280: high speed
		OpcodeTable[0xd5] = ("cmp", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xd6] = ("dec", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xd7] = ("smb5", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xd8] = ("cld", AddressingMode.Implied, 1);
		OpcodeTable[0xd9] = ("cmp", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0xda] = ("phx", AddressingMode.Implied, 1);
		OpcodeTable[0xdd] = ("cmp", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xde] = ("dec", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xdf] = ("bbs5", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xe0] = ("cpx", AddressingMode.Immediate, 2);
		OpcodeTable[0xe1] = ("sbc", AddressingMode.IndirectX, 2);
		OpcodeTable[0xe3] = ("tia", AddressingMode.Implied, 7); // HuC6280: block transfer to alternate
		OpcodeTable[0xe4] = ("cpx", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xe5] = ("sbc", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xe6] = ("inc", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xe7] = ("smb6", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xe8] = ("inx", AddressingMode.Implied, 1);
		OpcodeTable[0xe9] = ("sbc", AddressingMode.Immediate, 2);
		OpcodeTable[0xea] = ("nop", AddressingMode.Implied, 1);
		OpcodeTable[0xec] = ("cpx", AddressingMode.Absolute, 3);
		OpcodeTable[0xed] = ("sbc", AddressingMode.Absolute, 3);
		OpcodeTable[0xee] = ("inc", AddressingMode.Absolute, 3);
		OpcodeTable[0xef] = ("bbs6", AddressingMode.ZeroPageRelative, 3);

		OpcodeTable[0xf0] = ("beq", AddressingMode.Relative, 2);
		OpcodeTable[0xf1] = ("sbc", AddressingMode.IndirectY, 2);
		OpcodeTable[0xf2] = ("sbc", AddressingMode.ZeroPageIndirect, 2);
		OpcodeTable[0xf3] = ("tai", AddressingMode.Implied, 7); // HuC6280: block transfer from alternate
		OpcodeTable[0xf4] = ("set", AddressingMode.Implied, 1); // HuC6280: set T flag
		OpcodeTable[0xf5] = ("sbc", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xf6] = ("inc", AddressingMode.ZeroPageX, 2);
		OpcodeTable[0xf7] = ("smb7", AddressingMode.ZeroPage, 2);
		OpcodeTable[0xf8] = ("sed", AddressingMode.Implied, 1);
		OpcodeTable[0xf9] = ("sbc", AddressingMode.AbsoluteY, 3);
		OpcodeTable[0xfa] = ("plx", AddressingMode.Implied, 1);
		OpcodeTable[0xfd] = ("sbc", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xfe] = ("inc", AddressingMode.AbsoluteX, 3);
		OpcodeTable[0xff] = ("bbs7", AddressingMode.ZeroPageRelative, 3);
	}
}
