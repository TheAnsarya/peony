namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// Zilog Z80 CPU instruction decoder for Sega Master System
/// </summary>
public sealed class Z80Decoder : ICpuDecoder {
	public string Architecture => "Z80";

	// Base opcode table (unprefixed)
	private static readonly (string Mnemonic, AddressingMode Mode, int Bytes)[] BaseTable = new (string, AddressingMode, int)[256];

	// CB-prefixed opcodes (bit operations)
	private static readonly string[] CbTable = new string[256];

	static Z80Decoder() {
		InitializeBaseTable();
		InitializeCbTable();
	}

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		var opcode = data[0];

		// CB prefix: bit operations
		if (opcode == 0xcb && data.Length >= 2) {
			var cbOp = data[1];
			var mnemonic = CbTable[cbOp];
			var operand = FormatCbOperand(cbOp);
			return new DecodedInstruction(mnemonic, operand, data[..2].ToArray(), AddressingMode.Implied);
		}

		// DD prefix: IX-indexed operations
		if (opcode == 0xdd && data.Length >= 2) {
			return DecodeIndexed(data, address, "ix");
		}

		// FD prefix: IY-indexed operations
		if (opcode == 0xfd && data.Length >= 2) {
			return DecodeIndexed(data, address, "iy");
		}

		// ED prefix: extended operations
		if (opcode == 0xed && data.Length >= 2) {
			return DecodeEdPrefixed(data, address);
		}

		var (mnem, mode, length) = BaseTable[opcode];

		if (data.Length < length)
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		var bytes = data[..length].ToArray();
		var op = FormatBaseOperand(data, opcode, mode, address);

		return new DecodedInstruction(mnem, op, bytes, mode);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jp" or "jr" or "call" or "ret" or "reti" or "retn" or
			"rst" or "djnz";
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		switch (instruction.Mnemonic) {
			case "jp" when instruction.Bytes.Length == 3: {
				var target = (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				yield return target;
				// Conditional JP also falls through
				if (instruction.Bytes[0] != 0xc3)
					yield return nextAddress;
				break;
			}
			case "jr": {
				var offset = (sbyte)instruction.Bytes[1];
				yield return (uint)(nextAddress + offset);
				// Conditional JR falls through; unconditional (0x18) does not
				if (instruction.Bytes[0] != 0x18)
					yield return nextAddress;
				break;
			}
			case "djnz": {
				var offset = (sbyte)instruction.Bytes[1];
				yield return (uint)(nextAddress + offset);
				yield return nextAddress;
				break;
			}
			case "call": {
				var target = (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
				yield return target;
				yield return nextAddress;
				break;
			}
			case "rst": {
				uint target = (uint)(instruction.Bytes[0] & 0x38);
				yield return target;
				yield return nextAddress;
				break;
			}
		}
	}

	private DecodedInstruction DecodeIndexed(ReadOnlySpan<byte> data, uint address, string reg) {
		var op2 = data[1];

		// DD CB dd op / FD CB dd op — indexed bit operations
		if (op2 == 0xcb && data.Length >= 4) {
			var displacement = (sbyte)data[2];
			var bitOp = data[3];
			var mnemonic = CbTable[bitOp];
			var sign = displacement >= 0 ? "+" : "";
			var cbOperand = FormatIndexedCbOperand(bitOp, reg, sign, displacement);
			return new DecodedInstruction(mnemonic, cbOperand, data[..4].ToArray(), AddressingMode.Implied);
		}

		// Map DD/FD prefixed opcodes — replace HL with IX/IY, (HL) with (IX/IY+d)
		var (mnem, mode, length) = BaseTable[op2];
		if (mnem == "nop" || length == 0) {
			// Undefined DD/FD prefix — treat as NOP + the following byte
			return new DecodedInstruction("nop", "", data[..1].ToArray(), AddressingMode.Implied);
		}

		// Instructions that use (HL) become (IX+d)/(IY+d) with a displacement byte
		bool usesHlIndirect = UsesHlIndirect(op2);
		int totalLength = usesHlIndirect ? length + 1 : length; // +1 for displacement
		if (data.Length < totalLength + 1) // +1 for prefix
			return new DecodedInstruction("???", "", data[..1].ToArray(), AddressingMode.Implied);

		var bytes = data[..(totalLength + 1)].ToArray();
		var operand = FormatIndexedOperand(data[1..], op2, mode, address, reg, usesHlIndirect);

		// Replace HL register references in mnemonic context
		return new DecodedInstruction(mnem, operand, bytes, mode);
	}

	private DecodedInstruction DecodeEdPrefixed(ReadOnlySpan<byte> data, uint address) {
		var op2 = data[1];

		var (mnem, operand, length) = op2 switch {
			// 16-bit load/store
			0x43 => ("ld", $"(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4}),bc", 4),
			0x53 => ("ld", $"(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4}),de", 4),
			0x63 => ("ld", $"(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4}),hl", 4),
			0x73 => ("ld", $"(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4}),sp", 4),
			0x4b => ("ld", $"bc,(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4})", 4),
			0x5b => ("ld", $"de,(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4})", 4),
			0x6b => ("ld", $"hl,(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4})", 4),
			0x7b => ("ld", $"sp,(${(data.Length >= 4 ? data[2] | (data[3] << 8) : 0):x4})", 4),

			// ADC/SBC HL
			0x4a => ("adc", "hl,bc", 2),
			0x5a => ("adc", "hl,de", 2),
			0x6a => ("adc", "hl,hl", 2),
			0x7a => ("adc", "hl,sp", 2),
			0x42 => ("sbc", "hl,bc", 2),
			0x52 => ("sbc", "hl,de", 2),
			0x62 => ("sbc", "hl,hl", 2),
			0x72 => ("sbc", "hl,sp", 2),

			// Interrupt/misc
			0x44 or 0x54 or 0x64 or 0x74 => ("neg", "", 2),
			0x45 or 0x55 or 0x65 or 0x75 => ("retn", "", 2),
			0x4d => ("reti", "", 2),
			0x46 or 0x66 => ("im", "0", 2),
			0x56 or 0x76 => ("im", "1", 2),
			0x5e or 0x7e => ("im", "2", 2),

			// I/O
			0x40 => ("in", "b,(c)", 2),
			0x48 => ("in", "c,(c)", 2),
			0x50 => ("in", "d,(c)", 2),
			0x58 => ("in", "e,(c)", 2),
			0x60 => ("in", "h,(c)", 2),
			0x68 => ("in", "l,(c)", 2),
			0x78 => ("in", "a,(c)", 2),
			0x41 => ("out", "(c),b", 2),
			0x49 => ("out", "(c),c", 2),
			0x51 => ("out", "(c),d", 2),
			0x59 => ("out", "(c),e", 2),
			0x61 => ("out", "(c),h", 2),
			0x69 => ("out", "(c),l", 2),
			0x79 => ("out", "(c),a", 2),

			// Register transfers
			0x47 => ("ld", "i,a", 2),
			0x4f => ("ld", "r,a", 2),
			0x57 => ("ld", "a,i", 2),
			0x5f => ("ld", "a,r", 2),

			// Rotate
			0x67 => ("rrd", "", 2),
			0x6f => ("rld", "", 2),

			// Block transfer
			0xa0 => ("ldi", "", 2),
			0xb0 => ("ldir", "", 2),
			0xa8 => ("ldd", "", 2),
			0xb8 => ("lddr", "", 2),

			// Block compare
			0xa1 => ("cpi", "", 2),
			0xb1 => ("cpir", "", 2),
			0xa9 => ("cpd", "", 2),
			0xb9 => ("cpdr", "", 2),

			// Block I/O
			0xa2 => ("ini", "", 2),
			0xb2 => ("inir", "", 2),
			0xaa => ("ind", "", 2),
			0xba => ("indr", "", 2),
			0xa3 => ("outi", "", 2),
			0xb3 => ("otir", "", 2),
			0xab => ("outd", "", 2),
			0xbb => ("otdr", "", 2),

			_ => ("nop", "", 2),
		};

		if (data.Length < length)
			return new DecodedInstruction("???", "", data[..2].ToArray(), AddressingMode.Implied);

		return new DecodedInstruction(mnem, operand, data[..length].ToArray(), AddressingMode.Implied);
	}

	private static bool UsesHlIndirect(byte opcode) {
		// Opcodes that reference (HL) in the base table
		int row = opcode >> 3;
		int col = opcode & 0x07;
		// Column 6 in the register encoding = (HL)
		if (col == 6 && opcode >= 0x40 && opcode <= 0x7f && opcode != 0x76) return true; // LD r,(HL) / LD (HL),r
		if (col == 6 && opcode >= 0x80 && opcode <= 0xbf) return true; // ALU (HL)
		return opcode is 0x34 or 0x35 or 0x36 or 0x46 or 0x4e or 0x56 or 0x5e or
			0x66 or 0x6e or 0x70 or 0x71 or 0x72 or 0x73 or 0x74 or 0x75 or 0x77 or
			0x7e or 0x86 or 0x8e or 0x96 or 0x9e or 0xa6 or 0xae or 0xb6 or 0xbe;
	}

	private static string FormatIndexedOperand(ReadOnlySpan<byte> data, byte opcode, AddressingMode mode, uint address, string reg, bool hasDisplacement) {
		if (!hasDisplacement) {
			// Simple HL→IX/IY replacement in operand text
			return FormatBaseOperand(data, opcode, mode, address).Replace("hl", reg);
		}

		// (IX+d)/(IY+d) replacement
		var d = (sbyte)data[1];
		var sign = d >= 0 ? "+" : "";
		var baseOperand = FormatBaseOperand(data, opcode, mode, address);
		return baseOperand.Replace("(hl)", $"({reg}{sign}{d})").Replace("hl", reg);
	}

	private static string FormatIndexedCbOperand(byte bitOp, string reg, string sign, sbyte displacement) {
		int bit = (bitOp >> 3) & 7;
		if (bitOp >= 0x40 && bitOp <= 0x7f)
			return $"{bit},({reg}{sign}{displacement})";
		if (bitOp >= 0x80 && bitOp <= 0xbf)
			return $"{bit},({reg}{sign}{displacement})";
		if (bitOp >= 0xc0)
			return $"{bit},({reg}{sign}{displacement})";
		// Rotate/shift
		return $"({reg}{sign}{displacement})";
	}

	private static string FormatCbOperand(byte opcode) {
		int bit = (opcode >> 3) & 7;
		var reg = GetRegName(opcode & 0x07);

		if (opcode < 0x40) {
			// Rotate/shift — single register operand
			return reg;
		}
		// BIT/RES/SET — bit number + register
		return $"{bit},{reg}";
	}

	private static string GetRegName(int index) {
		return index switch {
			0 => "b",
			1 => "c",
			2 => "d",
			3 => "e",
			4 => "h",
			5 => "l",
			6 => "(hl)",
			7 => "a",
			_ => "?"
		};
	}

	private static string FormatBaseOperand(ReadOnlySpan<byte> data, byte opcode, AddressingMode mode, uint address) {
		return opcode switch {
			// NOP, HALT, etc.
			0x00 or 0x76 or 0xf3 or 0xfb => "",

			// LD r,r' — register to register
			>= 0x40 and <= 0x7f when opcode != 0x76 => $"{GetRegName((opcode >> 3) & 7)},{GetRegName(opcode & 7)}",

			// LD r,n — register immediate
			0x06 => $"b,${data[1]:x2}",
			0x0e => $"c,${data[1]:x2}",
			0x16 => $"d,${data[1]:x2}",
			0x1e => $"e,${data[1]:x2}",
			0x26 => $"h,${data[1]:x2}",
			0x2e => $"l,${data[1]:x2}",
			0x36 => $"(hl),${data[1]:x2}",
			0x3e => $"a,${data[1]:x2}",

			// LD rr,nn
			0x01 => $"bc,${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",
			0x11 => $"de,${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",
			0x21 => $"hl,${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",
			0x31 => $"sp,${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",

			// LD (nn),A / A,(nn)
			0x32 => $"(${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}),a",
			0x3a => $"a,(${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4})",

			// LD (nn),HL / HL,(nn)
			0x22 => $"(${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}),hl",
			0x2a => $"hl,(${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4})",

			// LD (BC),A / A,(BC) / (DE),A / A,(DE)
			0x02 => "(bc),a",
			0x0a => "a,(bc)",
			0x12 => "(de),a",
			0x1a => "a,(de)",

			// LD SP,HL
			0xf9 => "sp,hl",

			// PUSH/POP
			0xc5 or 0xc1 => opcode == 0xc5 ? "bc" : "bc",
			0xd5 or 0xd1 => opcode == 0xd5 ? "de" : "de",
			0xe5 or 0xe1 => opcode == 0xe5 ? "hl" : "hl",
			0xf5 or 0xf1 => opcode == 0xf5 ? "af" : "af",

			// EX
			0xeb => "de,hl",
			0x08 => "af,af'",
			0xe3 => "(sp),hl",
			0xd9 => "", // EXX

			// ALU A,r (immediate forms)
			0xc6 => $"${data[1]:x2}",
			0xce => $"${data[1]:x2}",
			0xd6 => $"${data[1]:x2}",
			0xde => $"${data[1]:x2}",
			0xe6 => $"${data[1]:x2}",
			0xee => $"${data[1]:x2}",
			0xf6 => $"${data[1]:x2}",
			0xfe => $"${data[1]:x2}",

			// ALU A,r (register forms, 0x80-0xBF)
			>= 0x80 and <= 0xbf => GetRegName(opcode & 0x07),

			// INC/DEC r
			0x04 or 0x05 => opcode == 0x04 ? "b" : "b",
			0x0c or 0x0d => opcode == 0x0c ? "c" : "c",
			0x14 or 0x15 => opcode == 0x14 ? "d" : "d",
			0x1c or 0x1d => opcode == 0x1c ? "e" : "e",
			0x24 or 0x25 => opcode == 0x24 ? "h" : "h",
			0x2c or 0x2d => opcode == 0x2c ? "l" : "l",
			0x34 or 0x35 => "(hl)",
			0x3c or 0x3d => opcode == 0x3c ? "a" : "a",

			// INC/DEC rr
			0x03 => "bc",
			0x0b => "bc",
			0x13 => "de",
			0x1b => "de",
			0x23 => "hl",
			0x2b => "hl",
			0x33 => "sp",
			0x3b => "sp",

			// ADD HL,rr
			0x09 => "hl,bc",
			0x19 => "hl,de",
			0x29 => "hl,hl",
			0x39 => "hl,sp",

			// JP nn
			0xc3 => $"${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",
			// JP cc,nn
			0xc2 or 0xca or 0xd2 or 0xda or 0xe2 or 0xea or 0xf2 or 0xfa =>
				$"{GetCondition((opcode >> 3) & 7)},${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",

			// JP (HL)
			0xe9 => "(hl)",

			// JR e
			0x18 => $"${(uint)(address + 2 + (sbyte)data[1]):x4}",
			// JR cc,e
			0x20 => $"nz,${(uint)(address + 2 + (sbyte)data[1]):x4}",
			0x28 => $"z,${(uint)(address + 2 + (sbyte)data[1]):x4}",
			0x30 => $"nc,${(uint)(address + 2 + (sbyte)data[1]):x4}",
			0x38 => $"c,${(uint)(address + 2 + (sbyte)data[1]):x4}",

			// DJNZ
			0x10 => $"${(uint)(address + 2 + (sbyte)data[1]):x4}",

			// CALL nn
			0xcd => $"${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",
			// CALL cc,nn
			0xc4 or 0xcc or 0xd4 or 0xdc or 0xe4 or 0xec or 0xf4 or 0xfc =>
				$"{GetCondition((opcode >> 3) & 7)},${(data.Length >= 3 ? data[1] | (data[2] << 8) : 0):x4}",

			// RET cc
			0xc0 or 0xc8 or 0xd0 or 0xd8 or 0xe0 or 0xe8 or 0xf0 or 0xf8 =>
				GetCondition((opcode >> 3) & 7),

			// RET
			0xc9 => "",

			// RST
			0xc7 or 0xcf or 0xd7 or 0xdf or 0xe7 or 0xef or 0xf7 or 0xff =>
				$"${opcode & 0x38:x2}",

			// OUT (n),A / IN A,(n)
			0xd3 => $"(${data[1]:x2}),a",
			0xdb => $"a,(${data[1]:x2})",

			_ => ""
		};
	}

	private static string GetCondition(int index) {
		return index switch {
			0 => "nz",
			1 => "z",
			2 => "nc",
			3 => "c",
			4 => "po",
			5 => "pe",
			6 => "p",
			7 => "m",
			_ => "?"
		};
	}

	private static void InitializeBaseTable() {
		// Default all to NOP
		for (int i = 0; i < 256; i++)
			BaseTable[i] = ("nop", AddressingMode.Implied, 1);

		// NOP
		BaseTable[0x00] = ("nop", AddressingMode.Implied, 1);

		// LD rr,nn (16-bit immediate loads)
		BaseTable[0x01] = ("ld", AddressingMode.Immediate, 3);
		BaseTable[0x11] = ("ld", AddressingMode.Immediate, 3);
		BaseTable[0x21] = ("ld", AddressingMode.Immediate, 3);
		BaseTable[0x31] = ("ld", AddressingMode.Immediate, 3);

		// LD (BC),A / LD (DE),A
		BaseTable[0x02] = ("ld", AddressingMode.Indirect, 1);
		BaseTable[0x12] = ("ld", AddressingMode.Indirect, 1);
		BaseTable[0x0a] = ("ld", AddressingMode.Indirect, 1);
		BaseTable[0x1a] = ("ld", AddressingMode.Indirect, 1);

		// INC/DEC rr
		BaseTable[0x03] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x13] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x23] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x33] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x0b] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x1b] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x2b] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x3b] = ("dec", AddressingMode.Implied, 1);

		// INC/DEC r
		BaseTable[0x04] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x0c] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x14] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x1c] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x24] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x2c] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x34] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x3c] = ("inc", AddressingMode.Implied, 1);
		BaseTable[0x05] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x0d] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x15] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x1d] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x25] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x2d] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x35] = ("dec", AddressingMode.Implied, 1);
		BaseTable[0x3d] = ("dec", AddressingMode.Implied, 1);

		// LD r,n (8-bit immediate)
		BaseTable[0x06] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x0e] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x16] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x1e] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x26] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x2e] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x36] = ("ld", AddressingMode.Immediate, 2);
		BaseTable[0x3e] = ("ld", AddressingMode.Immediate, 2);

		// Rotates (A)
		BaseTable[0x07] = ("rlca", AddressingMode.Implied, 1);
		BaseTable[0x0f] = ("rrca", AddressingMode.Implied, 1);
		BaseTable[0x17] = ("rla", AddressingMode.Implied, 1);
		BaseTable[0x1f] = ("rra", AddressingMode.Implied, 1);

		// EX AF,AF'
		BaseTable[0x08] = ("ex", AddressingMode.Implied, 1);

		// ADD HL,rr
		BaseTable[0x09] = ("add", AddressingMode.Implied, 1);
		BaseTable[0x19] = ("add", AddressingMode.Implied, 1);
		BaseTable[0x29] = ("add", AddressingMode.Implied, 1);
		BaseTable[0x39] = ("add", AddressingMode.Implied, 1);

		// DJNZ
		BaseTable[0x10] = ("djnz", AddressingMode.Relative, 2);

		// JR
		BaseTable[0x18] = ("jr", AddressingMode.Relative, 2);
		BaseTable[0x20] = ("jr", AddressingMode.Relative, 2);
		BaseTable[0x28] = ("jr", AddressingMode.Relative, 2);
		BaseTable[0x30] = ("jr", AddressingMode.Relative, 2);
		BaseTable[0x38] = ("jr", AddressingMode.Relative, 2);

		// LD (nn),HL / HL,(nn) / (nn),A / A,(nn)
		BaseTable[0x22] = ("ld", AddressingMode.Absolute, 3);
		BaseTable[0x2a] = ("ld", AddressingMode.Absolute, 3);
		BaseTable[0x32] = ("ld", AddressingMode.Absolute, 3);
		BaseTable[0x3a] = ("ld", AddressingMode.Absolute, 3);

		// DAA, CPL, SCF, CCF
		BaseTable[0x27] = ("daa", AddressingMode.Implied, 1);
		BaseTable[0x2f] = ("cpl", AddressingMode.Implied, 1);
		BaseTable[0x37] = ("scf", AddressingMode.Implied, 1);
		BaseTable[0x3f] = ("ccf", AddressingMode.Implied, 1);

		// LD r,r' (0x40-0x7F, except HALT at 0x76)
		for (int i = 0x40; i <= 0x7f; i++) {
			if (i == 0x76) continue;
			BaseTable[i] = ("ld", AddressingMode.Implied, 1);
		}
		BaseTable[0x76] = ("halt", AddressingMode.Implied, 1);

		// ALU A,r (0x80-0xBF)
		string[] aluOps = ["add", "adc", "sub", "sbc", "and", "xor", "or", "cp"];
		for (int i = 0x80; i <= 0xbf; i++) {
			BaseTable[i] = (aluOps[(i >> 3) & 7], AddressingMode.Implied, 1);
		}

		// RET cc
		BaseTable[0xc0] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xc8] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xd0] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xd8] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xe0] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xe8] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xf0] = ("ret", AddressingMode.Implied, 1);
		BaseTable[0xf8] = ("ret", AddressingMode.Implied, 1);

		// POP
		BaseTable[0xc1] = ("pop", AddressingMode.Implied, 1);
		BaseTable[0xd1] = ("pop", AddressingMode.Implied, 1);
		BaseTable[0xe1] = ("pop", AddressingMode.Implied, 1);
		BaseTable[0xf1] = ("pop", AddressingMode.Implied, 1);

		// JP cc,nn / JP nn
		BaseTable[0xc2] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xc3] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xca] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xd2] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xda] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xe2] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xea] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xf2] = ("jp", AddressingMode.Absolute, 3);
		BaseTable[0xfa] = ("jp", AddressingMode.Absolute, 3);

		// CALL cc,nn / CALL nn
		BaseTable[0xc4] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xcc] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xcd] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xd4] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xdc] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xe4] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xec] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xf4] = ("call", AddressingMode.Absolute, 3);
		BaseTable[0xfc] = ("call", AddressingMode.Absolute, 3);

		// PUSH
		BaseTable[0xc5] = ("push", AddressingMode.Implied, 1);
		BaseTable[0xd5] = ("push", AddressingMode.Implied, 1);
		BaseTable[0xe5] = ("push", AddressingMode.Implied, 1);
		BaseTable[0xf5] = ("push", AddressingMode.Implied, 1);

		// ALU A,n (immediate)
		BaseTable[0xc6] = ("add", AddressingMode.Immediate, 2);
		BaseTable[0xce] = ("adc", AddressingMode.Immediate, 2);
		BaseTable[0xd6] = ("sub", AddressingMode.Immediate, 2);
		BaseTable[0xde] = ("sbc", AddressingMode.Immediate, 2);
		BaseTable[0xe6] = ("and", AddressingMode.Immediate, 2);
		BaseTable[0xee] = ("xor", AddressingMode.Immediate, 2);
		BaseTable[0xf6] = ("or", AddressingMode.Immediate, 2);
		BaseTable[0xfe] = ("cp", AddressingMode.Immediate, 2);

		// RST
		BaseTable[0xc7] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xcf] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xd7] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xdf] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xe7] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xef] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xf7] = ("rst", AddressingMode.Implied, 1);
		BaseTable[0xff] = ("rst", AddressingMode.Implied, 1);

		// RET
		BaseTable[0xc9] = ("ret", AddressingMode.Implied, 1);

		// Prefixes (handled separately in Decode)
		BaseTable[0xcb] = ("prefix_cb", AddressingMode.Implied, 2);
		BaseTable[0xdd] = ("prefix_dd", AddressingMode.Implied, 2);
		BaseTable[0xed] = ("prefix_ed", AddressingMode.Implied, 2);
		BaseTable[0xfd] = ("prefix_fd", AddressingMode.Implied, 2);

		// JP (HL)
		BaseTable[0xe9] = ("jp", AddressingMode.Indirect, 1);

		// EX DE,HL / EX (SP),HL / EXX
		BaseTable[0xeb] = ("ex", AddressingMode.Implied, 1);
		BaseTable[0xe3] = ("ex", AddressingMode.Implied, 1);
		BaseTable[0xd9] = ("exx", AddressingMode.Implied, 1);

		// OUT (n),A / IN A,(n)
		BaseTable[0xd3] = ("out", AddressingMode.Immediate, 2);
		BaseTable[0xdb] = ("in", AddressingMode.Immediate, 2);

		// DI / EI
		BaseTable[0xf3] = ("di", AddressingMode.Implied, 1);
		BaseTable[0xfb] = ("ei", AddressingMode.Implied, 1);

		// LD SP,HL
		BaseTable[0xf9] = ("ld", AddressingMode.Implied, 1);
	}

	private static void InitializeCbTable() {
		string[] shiftOps = ["rlc", "rrc", "rl", "rr", "sla", "sra", "sll", "srl"];

		for (int i = 0; i < 256; i++) {
			if (i < 0x40) {
				CbTable[i] = shiftOps[(i >> 3) & 7];
			} else if (i < 0x80) {
				CbTable[i] = "bit";
			} else if (i < 0xc0) {
				CbTable[i] = "res";
			} else {
				CbTable[i] = "set";
			}
		}
	}
}
