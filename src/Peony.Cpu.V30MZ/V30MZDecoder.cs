namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// NEC V30MZ CPU decoder for WonderSwan.
/// The V30MZ is an 80186-compatible 16-bit CPU with ModR/M encoding.
/// </summary>
public sealed class V30MZDecoder : ICpuDecoder {
	public string Architecture => "V30MZ";

	// Register names for ModR/M decoding
	private static readonly string[] Reg8 = ["al", "cl", "dl", "bl", "ah", "ch", "dh", "bh"];
	private static readonly string[] Reg16 = ["ax", "cx", "dx", "bx", "sp", "bp", "si", "di"];
	private static readonly string[] SegReg = ["es", "cs", "ss", "ds"];
	private static readonly string[] Rm16 = ["bx+si", "bx+di", "bp+si", "bp+di", "si", "di", "bp", "bx"];

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		int pos = 0;
		string? segOverride = null;

		// Handle segment override prefixes
		while (pos < data.Length) {
			var b = data[pos];
			if (b == 0x26) { segOverride = "es"; pos++; }
			else if (b == 0x2e) { segOverride = "cs"; pos++; }
			else if (b == 0x36) { segOverride = "ss"; pos++; }
			else if (b == 0x3e) { segOverride = "ds"; pos++; }
			else break;
		}

		if (pos >= data.Length)
			return new DecodedInstruction("nop", "", data[..1].ToArray(), AddressingMode.Implied);

		var opcode = data[pos];
		pos++;

		var (mnemonic, operand, totalLen) = DecodeOpcode(data, pos, opcode, address + (uint)pos, segOverride);
		var instrLen = pos + totalLen - (pos - 1 > 0 && segOverride != null ? 0 : 0);

		// Calculate full instruction length including prefix
		var fullLen = Math.Min(pos + totalLen, data.Length);
		var bytes = data[..fullLen].ToArray();

		return new DecodedInstruction(mnemonic, operand, bytes, AddressingMode.Implied);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jmp" or "call" or "ret" or "retf" or "iret" or "int" or
			"jz" or "jnz" or "jc" or "jnc" or "js" or "jns" or "jo" or "jno" or
			"ja" or "jbe" or "jg" or "jge" or "jl" or "jle" or "jp" or "jnp" or
			"jcxz" or "loop" or "loopz" or "loopnz" or
			"je" or "jne" or "jb" or "jnb" or "jae" or "jna";
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		// Short conditional/unconditional jumps
		if (instruction.Mnemonic is "jz" or "jnz" or "jc" or "jnc" or "js" or "jns" or
			"jo" or "jno" or "ja" or "jbe" or "jg" or "jge" or "jl" or "jle" or
			"jp" or "jnp" or "jcxz" or "loop" or "loopz" or "loopnz" or
			"je" or "jne" or "jb" or "jnb" or "jae" or "jna") {
			var last = instruction.Bytes[^1];
			var target = (uint)(nextAddress + (sbyte)last);
			yield return target;
			yield return nextAddress;
		}

		// Near jump/call with 16-bit displacement
		if (instruction.Mnemonic == "jmp" && instruction.Bytes.Length == 3) {
			var disp = (short)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
			yield return (uint)(nextAddress + disp);
		} else if (instruction.Mnemonic == "jmp" && instruction.Bytes.Length == 2) {
			yield return (uint)(nextAddress + (sbyte)instruction.Bytes[1]);
		}

		if (instruction.Mnemonic == "call" && instruction.Bytes.Length == 3) {
			var disp = (short)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
			yield return (uint)(nextAddress + disp);
			yield return nextAddress;
		}
	}

	private (string Mnemonic, string Operand, int ConsumedBytes) DecodeOpcode(
		ReadOnlySpan<byte> data, int pos, byte opcode, uint address, string? segOverride) {

		// ALU operations: ADD, OR, ADC, SBB, AND, SUB, XOR, CMP
		// Pattern: 0x00-0x3F in groups of 8
		if (opcode <= 0x3f) {
			var aluGroup = opcode >> 3;
			var variant = opcode & 0x07;
			string[] aluOps = ["add", "or", "adc", "sbb", "and", "sub", "xor", "cmp"];
			var mnem = aluOps[aluGroup & 7];

			return variant switch {
				0 => DecodeModRM(data, pos, mnem, false, false, segOverride), // r/m8, r8
				1 => DecodeModRM(data, pos, mnem, true, false, segOverride),  // r/m16, r16
				2 => DecodeModRM(data, pos, mnem, false, true, segOverride),  // r8, r/m8
				3 => DecodeModRM(data, pos, mnem, true, true, segOverride),   // r16, r/m16
				4 => (mnem, $"al,${GetImm8(data, pos):x2}", 1),              // AL, imm8
				5 => (mnem, $"ax,${GetImm16(data, pos):x4}", 2),             // AX, imm16
				6 when aluGroup < 4 => (SegPushPop(aluGroup, true), SegReg[aluGroup & 3], 0),
				7 when aluGroup < 4 => (SegPushPop(aluGroup, false), SegReg[aluGroup & 3], 0),
				_ => ("nop", "", 0)
			};
		}

		return opcode switch {
			// INC/DEC r16
			>= 0x40 and <= 0x47 => ("inc", Reg16[opcode & 7], 0),
			>= 0x48 and <= 0x4f => ("dec", Reg16[opcode & 7], 0),

			// PUSH/POP r16
			>= 0x50 and <= 0x57 => ("push", Reg16[opcode & 7], 0),
			>= 0x58 and <= 0x5f => ("pop", Reg16[opcode & 7], 0),

			// PUSHA/POPA
			0x60 => ("pusha", "", 0),
			0x61 => ("popa", "", 0),

			// PUSH imm16/imm8
			0x68 => ("push", $"${GetImm16(data, pos):x4}", 2),
			0x6a => ("push", $"${GetImm8(data, pos):x2}", 1),

			// IMUL
			0x69 => DecodeImulImm16(data, pos, segOverride),
			0x6b => DecodeImulImm8(data, pos, segOverride),

			// Short conditional jumps
			0x70 => ("jo", FormatRel8(data, pos, address), 1),
			0x71 => ("jno", FormatRel8(data, pos, address), 1),
			0x72 => ("jc", FormatRel8(data, pos, address), 1),
			0x73 => ("jnc", FormatRel8(data, pos, address), 1),
			0x74 => ("jz", FormatRel8(data, pos, address), 1),
			0x75 => ("jnz", FormatRel8(data, pos, address), 1),
			0x76 => ("jbe", FormatRel8(data, pos, address), 1),
			0x77 => ("ja", FormatRel8(data, pos, address), 1),
			0x78 => ("js", FormatRel8(data, pos, address), 1),
			0x79 => ("jns", FormatRel8(data, pos, address), 1),
			0x7a => ("jp", FormatRel8(data, pos, address), 1),
			0x7b => ("jnp", FormatRel8(data, pos, address), 1),
			0x7c => ("jl", FormatRel8(data, pos, address), 1),
			0x7d => ("jge", FormatRel8(data, pos, address), 1),
			0x7e => ("jle", FormatRel8(data, pos, address), 1),
			0x7f => ("jg", FormatRel8(data, pos, address), 1),

			// ALU r/m8, imm8
			0x80 or 0x82 => DecodeGroupAlu8(data, pos, segOverride),
			0x81 => DecodeGroupAlu16(data, pos, segOverride),
			0x83 => DecodeGroupAlu16Imm8(data, pos, segOverride),

			// TEST/XCHG
			0x84 => DecodeModRM(data, pos, "test", false, false, segOverride),
			0x85 => DecodeModRM(data, pos, "test", true, false, segOverride),
			0x86 => DecodeModRM(data, pos, "xchg", false, false, segOverride),
			0x87 => DecodeModRM(data, pos, "xchg", true, false, segOverride),

			// MOV r/m, r
			0x88 => DecodeModRM(data, pos, "mov", false, false, segOverride),
			0x89 => DecodeModRM(data, pos, "mov", true, false, segOverride),
			0x8a => DecodeModRM(data, pos, "mov", false, true, segOverride),
			0x8b => DecodeModRM(data, pos, "mov", true, true, segOverride),

			// MOV r/m16, sreg / LEA / MOV sreg, r/m16
			0x8c => DecodeModRMSreg(data, pos, "mov", false, segOverride),
			0x8d => DecodeModRM(data, pos, "lea", true, true, segOverride),
			0x8e => DecodeModRMSreg(data, pos, "mov", true, segOverride),

			// NOP / XCHG AX,r16
			0x90 => ("nop", "", 0),
			>= 0x91 and <= 0x97 => ("xchg", $"ax,{Reg16[opcode & 7]}", 0),

			// CBW/CWD
			0x98 => ("cbw", "", 0),
			0x99 => ("cwd", "", 0),

			// CALL FAR / WAIT
			0x9a => ("call", FormatFarAddr(data, pos), 4),
			0x9b => ("wait", "", 0),

			// PUSHF/POPF/SAHF/LAHF
			0x9c => ("pushf", "", 0),
			0x9d => ("popf", "", 0),
			0x9e => ("sahf", "", 0),
			0x9f => ("lahf", "", 0),

			// MOV AL/AX, moffs
			0xa0 => ("mov", $"al,[{FormatSegAddr(segOverride, GetImm16(data, pos))}]", 2),
			0xa1 => ("mov", $"ax,[{FormatSegAddr(segOverride, GetImm16(data, pos))}]", 2),
			0xa2 => ("mov", $"[{FormatSegAddr(segOverride, GetImm16(data, pos))}],al", 2),
			0xa3 => ("mov", $"[{FormatSegAddr(segOverride, GetImm16(data, pos))}],ax", 2),

			// MOVSB/MOVSW/CMPSB/CMPSW
			0xa4 => ("movsb", "", 0),
			0xa5 => ("movsw", "", 0),
			0xa6 => ("cmpsb", "", 0),
			0xa7 => ("cmpsw", "", 0),

			// TEST AL/AX, imm
			0xa8 => ("test", $"al,${GetImm8(data, pos):x2}", 1),
			0xa9 => ("test", $"ax,${GetImm16(data, pos):x4}", 2),

			// STOSB/STOSW/LODSB/LODSW/SCASB/SCASW
			0xaa => ("stosb", "", 0),
			0xab => ("stosw", "", 0),
			0xac => ("lodsb", "", 0),
			0xad => ("lodsw", "", 0),
			0xae => ("scasb", "", 0),
			0xaf => ("scasw", "", 0),

			// MOV r8/r16, imm
			>= 0xb0 and <= 0xb7 => ("mov", $"{Reg8[opcode & 7]},${GetImm8(data, pos):x2}", 1),
			>= 0xb8 and <= 0xbf => ("mov", $"{Reg16[opcode & 7]},${GetImm16(data, pos):x4}", 2),

			// Shift/rotate group
			0xc0 => DecodeGroupShift8(data, pos, true, segOverride),
			0xc1 => DecodeGroupShift16(data, pos, true, segOverride),

			// RET imm16 / RET
			0xc2 => ("ret", $"${GetImm16(data, pos):x4}", 2),
			0xc3 => ("ret", "", 0),

			// LES/LDS
			0xc4 => DecodeModRM(data, pos, "les", true, true, segOverride),
			0xc5 => DecodeModRM(data, pos, "lds", true, true, segOverride),

			// MOV r/m8, imm8 / MOV r/m16, imm16
			0xc6 => DecodeMovImm8(data, pos, segOverride),
			0xc7 => DecodeMovImm16(data, pos, segOverride),

			// RETF imm16 / RETF
			0xca => ("retf", $"${GetImm16(data, pos):x4}", 2),
			0xcb => ("retf", "", 0),

			// INT 3 / INT n / INTO / IRET
			0xcc => ("int", "3", 0),
			0xcd => ("int", $"${GetImm8(data, pos):x2}", 1),
			0xce => ("into", "", 0),
			0xcf => ("iret", "", 0),

			// Shift group (by 1 / CL)
			0xd0 => DecodeGroupShift8(data, pos, false, segOverride),
			0xd1 => DecodeGroupShift16(data, pos, false, segOverride),
			0xd2 => DecodeGroupShiftCL8(data, pos, segOverride),
			0xd3 => DecodeGroupShiftCL16(data, pos, segOverride),

			// LOOP/LOOPZ/LOOPNZ/JCXZ
			0xe0 => ("loopnz", FormatRel8(data, pos, address), 1),
			0xe1 => ("loopz", FormatRel8(data, pos, address), 1),
			0xe2 => ("loop", FormatRel8(data, pos, address), 1),
			0xe3 => ("jcxz", FormatRel8(data, pos, address), 1),

			// IN/OUT
			0xe4 => ("in", $"al,${GetImm8(data, pos):x2}", 1),
			0xe5 => ("in", $"ax,${GetImm8(data, pos):x2}", 1),
			0xe6 => ("out", $"${GetImm8(data, pos):x2},al", 1),
			0xe7 => ("out", $"${GetImm8(data, pos):x2},ax", 1),

			// CALL/JMP near
			0xe8 => ("call", FormatRel16(data, pos, address), 2),
			0xe9 => ("jmp", FormatRel16(data, pos, address), 2),
			0xea => ("jmp", FormatFarAddr(data, pos), 4),
			0xeb => ("jmp", FormatRel8(data, pos, address), 1),

			// IN/OUT DX
			0xec => ("in", "al,dx", 0),
			0xed => ("in", "ax,dx", 0),
			0xee => ("out", "dx,al", 0),
			0xef => ("out", "dx,ax", 0),

			// LOCK/REP
			0xf0 => ("lock", "", 0),
			0xf2 => ("repnz", "", 0),
			0xf3 => ("rep", "", 0),

			// HLT/CMC
			0xf4 => ("hlt", "", 0),
			0xf5 => ("cmc", "", 0),

			// Unary group (NOT/NEG/MUL/IMUL/DIV/IDIV)
			0xf6 => DecodeGroupUnary8(data, pos, segOverride),
			0xf7 => DecodeGroupUnary16(data, pos, segOverride),

			// CLC/STC/CLI/STI/CLD/STD
			0xf8 => ("clc", "", 0),
			0xf9 => ("stc", "", 0),
			0xfa => ("cli", "", 0),
			0xfb => ("sti", "", 0),
			0xfc => ("cld", "", 0),
			0xfd => ("std", "", 0),

			// INC/DEC group
			0xfe => DecodeGroupIncDec8(data, pos, segOverride),
			0xff => DecodeGroupFF(data, pos, segOverride),

			_ => ("db", $"${opcode:x2}", 0)
		};
	}

	// Helper methods for ModR/M decoding
	private static (string Mnemonic, string Operand, int ConsumedBytes) DecodeModRM(
		ReadOnlySpan<byte> data, int pos, string mnemonic, bool wide, bool regFirst, string? segOverride) {
		if (pos >= data.Length) return (mnemonic, "???", 0);
		var modrm = data[pos];
		var mod = (modrm >> 6) & 3;
		var reg = (modrm >> 3) & 7;
		var rm = modrm & 7;
		var consumed = 1;

		var regName = wide ? Reg16[reg] : Reg8[reg];
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, wide, segOverride);
		consumed += extra;

		var operand = regFirst ? $"{regName},{rmStr}" : $"{rmStr},{regName}";
		return (mnemonic, operand, consumed);
	}

	private static (string Mnemonic, string Operand, int ConsumedBytes) DecodeModRMSreg(
		ReadOnlySpan<byte> data, int pos, string mnemonic, bool sregFirst, string? segOverride) {
		if (pos >= data.Length) return (mnemonic, "???", 0);
		var modrm = data[pos];
		var mod = (modrm >> 6) & 3;
		var reg = (modrm >> 3) & 7;
		var rm = modrm & 7;
		var consumed = 1;

		var sregName = reg < 4 ? SegReg[reg] : $"sr{reg}";
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;

		var operand = sregFirst ? $"{sregName},{rmStr}" : $"{rmStr},{sregName}";
		return (mnemonic, operand, consumed);
	}

	private static (string Formatted, int ExtraBytes) FormatModRM(
		ReadOnlySpan<byte> data, int pos, int mod, int rm, bool wide, string? segOverride) {
		var prefix = wide ? "word " : "byte ";

		if (mod == 3) {
			return (wide ? Reg16[rm] : Reg8[rm], 0);
		}

		if (mod == 0 && rm == 6) {
			// Direct address
			var addr = GetImm16Safe(data, pos);
			return ($"{prefix}[{FormatSegAddr(segOverride, addr)}]", 2);
		}

		var baseStr = Rm16[rm];
		if (mod == 0) {
			return ($"{prefix}[{(segOverride != null ? $"{segOverride}:" : "")}{baseStr}]", 0);
		}

		if (mod == 1) {
			var disp = pos < data.Length ? (sbyte)data[pos] : 0;
			var sign = disp >= 0 ? "+" : "";
			return ($"{prefix}[{(segOverride != null ? $"{segOverride}:" : "")}{baseStr}{sign}{disp}]", 1);
		}

		// mod == 2
		var disp16 = GetImm16Safe(data, pos);
		return ($"{prefix}[{(segOverride != null ? $"{segOverride}:" : "")}{baseStr}+${disp16:x4}]", 2);
	}

	// Group decoders for ALU/shift/unary operations
	private static (string, string, int) DecodeGroupAlu8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["add", "or", "adc", "sbb", "and", "sub", "xor", "cmp"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		var imm = pos + consumed < data.Length ? data[pos + consumed] : 0;
		consumed++;
		return (ops[reg], $"{rmStr},${imm:x2}", consumed);
	}

	private static (string, string, int) DecodeGroupAlu16(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["add", "or", "adc", "sbb", "and", "sub", "xor", "cmp"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		var imm = GetImm16Safe(data, pos + consumed);
		consumed += 2;
		return (ops[reg], $"{rmStr},${imm:x4}", consumed);
	}

	private static (string, string, int) DecodeGroupAlu16Imm8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["add", "or", "adc", "sbb", "and", "sub", "xor", "cmp"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		var imm = pos + consumed < data.Length ? (sbyte)data[pos + consumed] : 0;
		consumed++;
		return (ops[reg], $"{rmStr},${imm & 0xff:x2}", consumed);
	}

	private static (string, string, int) DecodeGroupShift8(ReadOnlySpan<byte> data, int pos, bool hasImm, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["rol", "ror", "rcl", "rcr", "shl", "shr", "sal", "sar"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		if (hasImm) {
			var imm = pos + consumed < data.Length ? data[pos + consumed] : 0;
			consumed++;
			return (ops[reg], $"{rmStr},${imm:x2}", consumed);
		}
		return (ops[reg], $"{rmStr},1", consumed);
	}

	private static (string, string, int) DecodeGroupShift16(ReadOnlySpan<byte> data, int pos, bool hasImm, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["rol", "ror", "rcl", "rcr", "shl", "shr", "sal", "sar"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		if (hasImm) {
			var imm = pos + consumed < data.Length ? data[pos + consumed] : 0;
			consumed++;
			return (ops[reg], $"{rmStr},${imm:x2}", consumed);
		}
		return (ops[reg], $"{rmStr},1", consumed);
	}

	private static (string, string, int) DecodeGroupShiftCL8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["rol", "ror", "rcl", "rcr", "shl", "shr", "sal", "sar"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		return (ops[reg], $"{rmStr},cl", consumed);
	}

	private static (string, string, int) DecodeGroupShiftCL16(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["rol", "ror", "rcl", "rcr", "shl", "shr", "sal", "sar"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		return (ops[reg], $"{rmStr},cl", consumed);
	}

	private static (string, string, int) DecodeGroupUnary8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["test", "test", "not", "neg", "mul", "imul", "div", "idiv"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		if (reg <= 1) {
			var imm = pos + consumed < data.Length ? data[pos + consumed] : 0;
			consumed++;
			return (ops[reg], $"{rmStr},${imm:x2}", consumed);
		}
		return (ops[reg], rmStr, consumed);
	}

	private static (string, string, int) DecodeGroupUnary16(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["test", "test", "not", "neg", "mul", "imul", "div", "idiv"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		if (reg <= 1) {
			var imm = GetImm16Safe(data, pos + consumed);
			consumed += 2;
			return (ops[reg], $"{rmStr},${imm:x4}", consumed);
		}
		return (ops[reg], rmStr, consumed);
	}

	private static (string, string, int) DecodeGroupIncDec8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		return (reg == 0 ? "inc" : "dec", rmStr, consumed);
	}

	private static (string, string, int) DecodeGroupFF(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		string[] ops = ["inc", "dec", "call", "call", "jmp", "jmp", "push", "???"];
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		return (ops[reg], rmStr, consumed);
	}

	private static (string, string, int) DecodeMovImm8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		var modrm = data[pos];
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, false, segOverride);
		consumed += extra;
		var imm = pos + consumed < data.Length ? data[pos + consumed] : 0;
		consumed++;
		return ("mov", $"{rmStr},${imm:x2}", consumed);
	}

	private static (string, string, int) DecodeMovImm16(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		var modrm = data[pos];
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		var imm = GetImm16Safe(data, pos + consumed);
		consumed += 2;
		return ("mov", $"{rmStr},${imm:x4}", consumed);
	}

	private static (string, string, int) DecodeImulImm16(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		var imm = GetImm16Safe(data, pos + consumed);
		consumed += 2;
		return ("imul", $"{Reg16[reg]},{rmStr},${imm:x4}", consumed);
	}

	private static (string, string, int) DecodeImulImm8(ReadOnlySpan<byte> data, int pos, string? segOverride) {
		if (pos >= data.Length) return ("???", "", 0);
		var modrm = data[pos];
		var reg = (modrm >> 3) & 7;
		var mod = (modrm >> 6) & 3;
		var rm = modrm & 7;
		var consumed = 1;
		var (rmStr, extra) = FormatModRM(data, pos + 1, mod, rm, true, segOverride);
		consumed += extra;
		var imm = pos + consumed < data.Length ? (sbyte)data[pos + consumed] : 0;
		consumed++;
		return ("imul", $"{Reg16[reg]},{rmStr},${imm & 0xff:x2}", consumed);
	}

	// Utility methods
	private static string SegPushPop(int group, bool isPush) =>
		group switch { 0 => isPush ? "push" : "pop", 1 => isPush ? "push" : "pop",
			2 => isPush ? "push" : "pop", 3 => isPush ? "push" : "pop", _ => "nop" };

	private static string FormatRel8(ReadOnlySpan<byte> data, int pos, uint address) {
		if (pos >= data.Length) return "$????";
		var offset = (sbyte)data[pos];
		return $"${(uint)(address + 1 + offset):x4}";
	}

	private static string FormatRel16(ReadOnlySpan<byte> data, int pos, uint address) {
		var disp = (short)GetImm16Safe(data, pos);
		return $"${(uint)(address + 2 + disp):x4}";
	}

	private static string FormatFarAddr(ReadOnlySpan<byte> data, int pos) {
		var off = GetImm16Safe(data, pos);
		var seg = GetImm16Safe(data, pos + 2);
		return $"${seg:x4}:${off:x4}";
	}

	private static string FormatSegAddr(string? segOverride, int addr) {
		var prefix = segOverride != null ? $"{segOverride}:" : "";
		return $"{prefix}${addr:x4}";
	}

	private static byte GetImm8(ReadOnlySpan<byte> data, int pos) =>
		pos < data.Length ? data[pos] : (byte)0;

	private static int GetImm16(ReadOnlySpan<byte> data, int pos) =>
		pos + 1 < data.Length ? data[pos] | (data[pos + 1] << 8) : 0;

	private static int GetImm16Safe(ReadOnlySpan<byte> data, int pos) =>
		pos + 1 < data.Length ? data[pos] | (data[pos + 1] << 8) : 0;
}
