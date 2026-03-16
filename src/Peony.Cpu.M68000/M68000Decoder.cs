namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// Motorola 68000 CPU decoder for Sega Genesis/Mega Drive.
/// 16/32-bit CISC processor with 14 addressing modes.
/// Instructions are 2-10 bytes, always word-aligned.
/// </summary>
public sealed class M68000Decoder : ICpuDecoder {
	public string Architecture => "M68000";

	private static readonly string[] DataRegs = ["d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7"];
	private static readonly string[] AddrRegs = ["a0", "a1", "a2", "a3", "a4", "a5", "a6", "sp"];
	private static readonly string[] Conditions = [
		"t", "f", "hi", "ls", "cc", "cs", "ne", "eq",
		"vc", "vs", "pl", "mi", "ge", "lt", "gt", "le"
	];

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length < 2)
			return new DecodedInstruction("???", "", data.Length > 0 ? [data[0]] : [0xff], AddressingMode.Implied);

		var word = ReadWord(data, 0);
		var group = (word >> 12) & 0xf;

		var (mnemonic, operand, length) = group switch {
			0x0 => DecodeLine0(data, word, address),
			0x1 => DecodeMove(data, word, 1),  // MOVE.B
			0x2 => DecodeMove(data, word, 2),  // MOVE.L
			0x3 => DecodeMove(data, word, 1),  // MOVE.W (size code 1 for word)
			0x4 => DecodeLine4(data, word, address),
			0x5 => DecodeLine5(data, word, address),
			0x6 => DecodeLine6(data, word, address),
			0x7 => DecodeLine7(word),
			0x8 => DecodeLine8(data, word),
			0x9 => DecodeArith(data, word, "sub"),
			0xa => ("dc.w", $"${word:x4}", 2),  // A-line trap
			0xb => DecodeLine_B(data, word),
			0xc => DecodeLine_C(data, word),
			0xd => DecodeArith(data, word, "add"),
			0xe => DecodeLine_E(data, word),
			0xf => ("dc.w", $"${word:x4}", 2),  // F-line trap
			_ => ("dc.w", $"${word:x4}", 2)
		};

		length = Math.Min(length, data.Length);
		var bytes = data[..length].ToArray();
		return new DecodedInstruction(mnemonic, operand, bytes, AddressingMode.Implied);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jmp" or "jsr" or "rts" or "rte" or "rtr" or
			"bra" or "bsr" or "trap" or "trapv" or "illegal" or
			"bra.w" or "bsr.w" or "bra.s" or "bsr.s" ||
			instruction.Mnemonic.StartsWith("bcc") || instruction.Mnemonic.StartsWith("b") &&
			instruction.Mnemonic.Length <= 5 && instruction.Mnemonic != "bchg" &&
			instruction.Mnemonic != "bclr" && instruction.Mnemonic != "bset" &&
			instruction.Mnemonic != "btst" ||
			instruction.Mnemonic.StartsWith("db");
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		// Branch instructions (Bcc, BRA, BSR)
		if (instruction.Mnemonic.StartsWith("b") && instruction.Bytes.Length >= 2) {
			var disp = (sbyte)(instruction.Bytes[1]);
			if (disp == 0 && instruction.Bytes.Length >= 4) {
				// Word displacement
				var wordDisp = (short)((instruction.Bytes[2] << 8) | instruction.Bytes[3]);
				yield return (uint)(address + 2 + wordDisp);
			} else if (disp != 0) {
				yield return (uint)(address + 2 + disp);
			}

			// Conditional branches fall through
			if (instruction.Mnemonic != "bra" && instruction.Mnemonic != "bra.s" &&
				instruction.Mnemonic != "bra.w") {
				yield return nextAddress;
			}
		}

		// JSR/JMP with absolute address
		if ((instruction.Mnemonic is "jsr" or "jmp") && instruction.Operand.StartsWith("$")) {
			if (uint.TryParse(instruction.Operand[1..], System.Globalization.NumberStyles.HexNumber, null, out var target)) {
				yield return target;
				if (instruction.Mnemonic == "jsr")
					yield return nextAddress;
			}
		}

		// DBcc
		if (instruction.Mnemonic.StartsWith("db") && instruction.Bytes.Length >= 4) {
			var disp = (short)((instruction.Bytes[2] << 8) | instruction.Bytes[3]);
			yield return (uint)(address + 2 + disp);
			yield return nextAddress;
		}
	}

	// Line 0: Bit operations, ORI/ANDI/SUBI/ADDI/EORI/CMPI, MOVEP
	private (string, string, int) DecodeLine0(ReadOnlySpan<byte> data, int word, uint address) {
		var subGroup = (word >> 8) & 0xf;

		// Immediate operations to CCR/SR or general
		if ((word & 0x0100) == 0) {
			var op = ((word >> 9) & 7) switch {
				0 => "ori",
				1 => "andi",
				2 => "subi",
				3 => "addi",
				4 => "btst",
				5 => "eori",
				6 => "cmpi",
				_ => "???"
			};

			if (op is "btst" && ((word >> 8) & 1) == 0) {
				// Static bit operations
				var size = GetSize((word >> 6) & 3);
				var (eaStr, eaLen) = DecodeEA(data, 4, word & 0x3f, 1);
				return ($"btst", $"#${ReadWordSafe(data, 2) & 0xff:x2},{eaStr}", 2 + 2 + eaLen);
			}

			var sz = (word >> 6) & 3;
			if (sz <= 2) {
				var immLen = sz == 2 ? 4 : 2;
				var imm = sz == 2 ? ReadLongSafe(data, 2) : ReadWordSafe(data, 2);
				var (eaStr, eaLen) = DecodeEA(data, 2 + immLen, word & 0x3f, sz == 0 ? 1 : sz == 1 ? 2 : 4);
				var suf = GetSize(sz);
				return ($"{op}{suf}", $"#${imm:x},{eaStr}", 2 + immLen + eaLen);
			}
		}

		// BTST/BCHG/BCLR/BSET with register
		if ((word & 0x0100) != 0 && ((word >> 6) & 3) <= 3) {
			string[] bitOps = ["btst", "bchg", "bclr", "bset"];
			var bitOp = bitOps[(word >> 6) & 3];
			var regNum = (word >> 9) & 7;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1);
			return (bitOp, $"{DataRegs[regNum]},{eaStr}", 2 + eaLen);
		}

		return ("dc.w", $"${word:x4}", 2);
	}

	// Lines 1/2/3: MOVE
	private (string, string, int) DecodeMove(ReadOnlySpan<byte> data, int word, int srcSize) {
		var sizeCode = (word >> 12) & 3;
		var size = sizeCode switch { 1 => 1, 3 => 2, 2 => 4, _ => 2 };
		var suf = sizeCode switch { 1 => ".b", 3 => ".w", 2 => ".l", _ => ".w" };

		var srcMode = word & 0x3f;
		var (srcStr, srcLen) = DecodeEA(data, 2, srcMode, size);

		// Destination EA: register in bits 9-11, mode in bits 6-8
		var dstReg = (word >> 9) & 7;
		var dstMode = (word >> 6) & 7;
		var dstEA = (dstMode << 3) | dstReg;
		var (dstStr, dstLen) = DecodeEA(data, 2 + srcLen, dstEA, size);

		// MOVEA check
		var mnem = dstMode == 1 ? $"movea{suf}" : $"move{suf}";
		return (mnem, $"{srcStr},{dstStr}", 2 + srcLen + dstLen);
	}

	// Line 4: Misc (LEA, PEA, CLR, NEG, NOT, TST, MOVEM, JSR, JMP, RTS, etc.)
	private (string, string, int) DecodeLine4(ReadOnlySpan<byte> data, int word, uint address) {
		// Single-operand instructions
		if ((word & 0xff00) == 0x4e00) {
			return (word & 0xff) switch {
				0x70 => ("reset", "", 2),
				0x71 => ("nop", "", 2),
				0x72 => ("stop", $"#${ReadWordSafe(data, 2):x4}", 4),
				0x73 => ("rte", "", 2),
				0x75 => ("rts", "", 2),
				0x76 => ("trapv", "", 2),
				0x77 => ("rtr", "", 2),
				>= 0x40 and <= 0x4f => ("trap", $"#${(word & 0xf)}", 2),
				>= 0x50 and <= 0x57 => ("link", $"{AddrRegs[word & 7]},#${ReadWordSafe(data, 2):x4}", 4),
				>= 0x58 and <= 0x5f => ("unlk", AddrRegs[word & 7], 2),
				>= 0x60 and <= 0x67 => ("move", $"{AddrRegs[word & 7]},usp", 2),
				>= 0x68 and <= 0x6f => ("move", $"usp,{AddrRegs[word & 7]}", 2),
				>= 0x80 and <= 0xbf => DecodeJsrJmp(data, word),
				_ => ("dc.w", $"${word:x4}", 2)
			};
		}

		// LEA
		if ((word & 0xf1c0) == 0x41c0) {
			var reg = (word >> 9) & 7;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 4);
			return ("lea", $"{eaStr},{AddrRegs[reg]}", 2 + eaLen);
		}

		// PEA
		if ((word & 0xffc0) == 0x4840) {
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 4);
			return ("pea", eaStr, 2 + eaLen);
		}

		// CLR
		if ((word & 0xff00) == 0x4200) {
			var sz = (word >> 6) & 3;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
			return ($"clr{GetSize(sz)}", eaStr, 2 + eaLen);
		}

		// NEG
		if ((word & 0xff00) == 0x4400) {
			var sz = (word >> 6) & 3;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
			return ($"neg{GetSize(sz)}", eaStr, 2 + eaLen);
		}

		// NOT
		if ((word & 0xff00) == 0x4600) {
			var sz = (word >> 6) & 3;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
			return ($"not{GetSize(sz)}", eaStr, 2 + eaLen);
		}

		// TST
		if ((word & 0xff00) == 0x4a00) {
			var sz = (word >> 6) & 3;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
			return ($"tst{GetSize(sz)}", eaStr, 2 + eaLen);
		}

		// SWAP
		if ((word & 0xfff8) == 0x4840) {
			return ("swap", DataRegs[word & 7], 2);
		}

		// EXT
		if ((word & 0xfff8) == 0x4880) {
			return ("ext.w", DataRegs[word & 7], 2);
		}
		if ((word & 0xfff8) == 0x48c0) {
			return ("ext.l", DataRegs[word & 7], 2);
		}

		// MOVEM
		if ((word & 0xfb80) == 0x4880) {
			var dir = (word >> 10) & 1;  // 0=reg-to-mem, 1=mem-to-reg
			var sz = ((word >> 6) & 1) == 0 ? ".w" : ".l";
			var mask = ReadWordSafe(data, 2);
			var (eaStr, eaLen) = DecodeEA(data, 4, word & 0x3f, 2);
			var regList = FormatRegList(mask, ((word >> 3) & 7) == 4);
			return dir == 0
				? ($"movem{sz}", $"{regList},{eaStr}", 4 + eaLen)
				: ($"movem{sz}", $"{eaStr},{regList}", 4 + eaLen);
		}

		return ("dc.w", $"${word:x4}", 2);
	}

	// Line 5: ADDQ/SUBQ/Scc/DBcc
	private (string, string, int) DecodeLine5(ReadOnlySpan<byte> data, int word, uint address) {
		var sz = (word >> 6) & 3;

		// DBcc
		if (sz == 3 && ((word >> 3) & 7) == 1) {
			var cond = Conditions[(word >> 8) & 0xf];
			var reg = word & 7;
			var disp = (short)ReadWordSafe(data, 2);
			return ($"db{cond}", $"{DataRegs[reg]},${(uint)(address + 2 + disp):x6}", 4);
		}

		// Scc
		if (sz == 3) {
			var cond = Conditions[(word >> 8) & 0xf];
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 1);
			return ($"s{cond}", eaStr, 2 + eaLen);
		}

		// ADDQ/SUBQ
		var qData = ((word >> 9) & 7);
		if (qData == 0) qData = 8;
		var isAdd = ((word >> 8) & 1) == 0;
		var (ea, eLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
		return (isAdd ? $"addq{GetSize(sz)}" : $"subq{GetSize(sz)}", $"#${qData},{ea}", 2 + eLen);
	}

	// Line 6: BRA/BSR/Bcc
	private (string, string, int) DecodeLine6(ReadOnlySpan<byte> data, int word, uint address) {
		var cond = (word >> 8) & 0xf;
		var disp8 = (sbyte)(word & 0xff);

		string mnem;
		if (cond == 0) mnem = "bra";
		else if (cond == 1) mnem = "bsr";
		else mnem = $"b{Conditions[cond]}";

		if (disp8 == 0) {
			// Word displacement
			var disp16 = (short)ReadWordSafe(data, 2);
			return ($"{mnem}", $"${(uint)(address + 2 + disp16):x6}", 4);
		}

		return ($"{mnem}.s", $"${(uint)(address + 2 + disp8):x6}", 2);
	}

	// Line 7: MOVEQ
	private (string, string, int) DecodeLine7(int word) {
		var reg = (word >> 9) & 7;
		var imm = (sbyte)(word & 0xff);
		return ("moveq", $"#${imm & 0xff:x2},{DataRegs[reg]}", 2);
	}

	// Line 8: OR/DIV/SBCD
	private (string, string, int) DecodeLine8(ReadOnlySpan<byte> data, int word) {
		var sz = (word >> 6) & 3;

		// DIVU/DIVS
		if (sz == 3) {
			var reg = (word >> 9) & 7;
			var isSigned = ((word >> 8) & 1) != 0;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 2);
			return (isSigned ? "divs.w" : "divu.w", $"{eaStr},{DataRegs[reg]}", 2 + eaLen);
		}

		// SBCD
		if (sz == 0 && ((word >> 8) & 1) != 0 && ((word >> 3) & 0x1f) <= 0x0f) {
			var src = word & 7;
			var dst = (word >> 9) & 7;
			var isMemory = ((word >> 3) & 1) != 0;
			return isMemory
				? ("sbcd", $"-({AddrRegs[src]}),-({AddrRegs[dst]})", 2)
				: ("sbcd", $"{DataRegs[src]},{DataRegs[dst]}", 2);
		}

		// OR
		var dir = ((word >> 8) & 1);
		var regNum = (word >> 9) & 7;
		var (eStr, eLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
		return dir == 0
			? ($"or{GetSize(sz)}", $"{eStr},{DataRegs[regNum]}", 2 + eLen)
			: ($"or{GetSize(sz)}", $"{DataRegs[regNum]},{eStr}", 2 + eLen);
	}

	// Line 9/D: ADD/SUB
	private (string, string, int) DecodeArith(ReadOnlySpan<byte> data, int word, string baseOp) {
		var sz = (word >> 6) & 3;
		var regNum = (word >> 9) & 7;

		// ADDA/SUBA
		if (sz == 3) {
			var opSz = ((word >> 8) & 1) == 0 ? ".w" : ".l";
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, opSz == ".w" ? 2 : 4);
			return ($"{baseOp}a{opSz}", $"{eaStr},{AddrRegs[regNum]}", 2 + eaLen);
		}

		var dir = ((word >> 8) & 1);
		var (ea, eLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
		return dir == 0
			? ($"{baseOp}{GetSize(sz)}", $"{ea},{DataRegs[regNum]}", 2 + eLen)
			: ($"{baseOp}{GetSize(sz)}", $"{DataRegs[regNum]},{ea}", 2 + eLen);
	}

	// Line B: CMP/EOR
	private (string, string, int) DecodeLine_B(ReadOnlySpan<byte> data, int word) {
		var sz = (word >> 6) & 3;
		var regNum = (word >> 9) & 7;

		// CMPA
		if (sz == 3) {
			var opSz = ((word >> 8) & 1) == 0 ? ".w" : ".l";
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, opSz == ".w" ? 2 : 4);
			return ($"cmpa{opSz}", $"{eaStr},{AddrRegs[regNum]}", 2 + eaLen);
		}

		var dir = ((word >> 8) & 1);
		var (ea, eLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
		return dir == 0
			? ($"cmp{GetSize(sz)}", $"{ea},{DataRegs[regNum]}", 2 + eLen)
			: ($"eor{GetSize(sz)}", $"{DataRegs[regNum]},{ea}", 2 + eLen);
	}

	// Line C: AND/MUL/ABCD/EXG
	private (string, string, int) DecodeLine_C(ReadOnlySpan<byte> data, int word) {
		var sz = (word >> 6) & 3;
		var regNum = (word >> 9) & 7;

		// MULU/MULS
		if (sz == 3) {
			var isSigned = ((word >> 8) & 1) != 0;
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 2);
			return (isSigned ? "muls.w" : "mulu.w", $"{eaStr},{DataRegs[regNum]}", 2 + eaLen);
		}

		// EXG
		if (((word >> 8) & 1) != 0 && ((word >> 3) & 0x1f) is 0x08 or 0x09 or 0x11) {
			var opMode = (word >> 3) & 0x1f;
			var otherReg = word & 7;
			return opMode switch {
				0x08 => ("exg", $"{DataRegs[regNum]},{DataRegs[otherReg]}", 2),
				0x09 => ("exg", $"{AddrRegs[regNum]},{AddrRegs[otherReg]}", 2),
				0x11 => ("exg", $"{DataRegs[regNum]},{AddrRegs[otherReg]}", 2),
				_ => ("dc.w", $"${word:x4}", 2)
			};
		}

		// AND
		var dir = ((word >> 8) & 1);
		var (ea, eLen) = DecodeEA(data, 2, word & 0x3f, 1 << sz);
		return dir == 0
			? ($"and{GetSize(sz)}", $"{ea},{DataRegs[regNum]}", 2 + eLen)
			: ($"and{GetSize(sz)}", $"{DataRegs[regNum]},{ea}", 2 + eLen);
	}

	// Line E: Shift/Rotate
	private (string, string, int) DecodeLine_E(ReadOnlySpan<byte> data, int word) {
		var sz = (word >> 6) & 3;

		// Memory shift (size == 3)
		if (sz == 3) {
			string[] ops = ["asr", "asl", "lsr", "lsl", "roxr", "roxl", "ror", "rol"];
			var opIdx = ((word >> 8) & 7) * 2 + ((word >> 8) & 1);
			var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 2);
			var op = ((word >> 9) & 3) switch {
				0 => ((word >> 8) & 1) == 0 ? "asr" : "asl",
				1 => ((word >> 8) & 1) == 0 ? "lsr" : "lsl",
				2 => ((word >> 8) & 1) == 0 ? "roxr" : "roxl",
				3 => ((word >> 8) & 1) == 0 ? "ror" : "rol",
				_ => "???"
			};
			return (op, eaStr, 2 + eaLen);
		}

		// Register shift
		var dir2 = ((word >> 8) & 1); // 0=right, 1=left
		var regType = ((word >> 5) & 1); // 0=immediate count, 1=register count
		var shiftType = (word >> 3) & 3;
		string baseOp = shiftType switch {
			0 => dir2 == 0 ? "asr" : "asl",
			1 => dir2 == 0 ? "lsr" : "lsl",
			2 => dir2 == 0 ? "roxr" : "roxl",
			3 => dir2 == 0 ? "ror" : "rol",
			_ => "???"
		};

		var count = (word >> 9) & 7;
		var dstReg = word & 7;

		if (regType == 0) {
			if (count == 0) count = 8;
			return ($"{baseOp}{GetSize(sz)}", $"#${count},{DataRegs[dstReg]}", 2);
		}
		return ($"{baseOp}{GetSize(sz)}", $"{DataRegs[count]},{DataRegs[dstReg]}", 2);
	}

	private (string, string, int) DecodeJsrJmp(ReadOnlySpan<byte> data, int word) {
		var subOp = (word >> 6) & 3;
		var mnem = subOp switch { 2 => "jsr", 3 => "jmp", _ => "???" };
		var (eaStr, eaLen) = DecodeEA(data, 2, word & 0x3f, 4);
		return (mnem, eaStr, 2 + eaLen);
	}

	// Effective Address decoding
	private (string Formatted, int ExtraBytes) DecodeEA(ReadOnlySpan<byte> data, int pos, int ea, int size) {
		var mode = (ea >> 3) & 7;
		var reg = ea & 7;

		return mode switch {
			0 => (DataRegs[reg], 0),                           // Dn
			1 => (AddrRegs[reg], 0),                           // An
			2 => ($"({AddrRegs[reg]})", 0),                    // (An)
			3 => ($"({AddrRegs[reg]})+", 0),                   // (An)+
			4 => ($"-({AddrRegs[reg]})", 0),                   // -(An)
			5 => ($"${ReadWordSafe(data, pos):x4}({AddrRegs[reg]})", 2), // d16(An)
			6 => DecodeExtWord(data, pos, reg),                // d8(An,Xn)
			7 => reg switch {
				0 => ($"${ReadWordSafe(data, pos):x4}.w", 2),     // addr.W
				1 => ($"${ReadLongSafe(data, pos):x8}", 4),       // addr.L
				2 => ($"${ReadWordSafe(data, pos):x4}(pc)", 2),   // d16(PC)
				3 => DecodeExtWordPC(data, pos),                   // d8(PC,Xn)
				4 => size switch {                                  // #imm
					1 => ($"#${ReadWordSafe(data, pos) & 0xff:x2}", 2),
					2 => ($"#${ReadWordSafe(data, pos):x4}", 2),
					4 => ($"#${ReadLongSafe(data, pos):x8}", 4),
					_ => ($"#${ReadWordSafe(data, pos):x4}", 2)
				},
				_ => ("???", 0)
			},
			_ => ("???", 0)
		};
	}

	private (string, int) DecodeExtWord(ReadOnlySpan<byte> data, int pos, int baseReg) {
		var ext = ReadWordSafe(data, pos);
		var idxReg = (ext >> 12) & 7;
		var idxIsAddr = ((ext >> 15) & 1) != 0;
		var idxSize = ((ext >> 11) & 1) != 0 ? ".l" : ".w";
		var disp = (sbyte)(ext & 0xff);
		var idxName = idxIsAddr ? AddrRegs[idxReg] : DataRegs[idxReg];
		return ($"${disp & 0xff:x2}({AddrRegs[baseReg]},{idxName}{idxSize})", 2);
	}

	private (string, int) DecodeExtWordPC(ReadOnlySpan<byte> data, int pos) {
		var ext = ReadWordSafe(data, pos);
		var idxReg = (ext >> 12) & 7;
		var idxIsAddr = ((ext >> 15) & 1) != 0;
		var idxSize = ((ext >> 11) & 1) != 0 ? ".l" : ".w";
		var disp = (sbyte)(ext & 0xff);
		var idxName = idxIsAddr ? AddrRegs[idxReg] : DataRegs[idxReg];
		return ($"${disp & 0xff:x2}(pc,{idxName}{idxSize})", 2);
	}

	private static string FormatRegList(int mask, bool reversed) {
		var parts = new List<string>();
		for (var i = 0; i < 8; i++) {
			var bit = reversed ? 15 - i : i;
			if ((mask & (1 << bit)) != 0)
				parts.Add(DataRegs[i]);
		}
		for (var i = 0; i < 8; i++) {
			var bit = reversed ? 7 - i : i + 8;
			if ((mask & (1 << bit)) != 0)
				parts.Add(AddrRegs[i]);
		}
		return string.Join("/", parts);
	}

	private static string GetSize(int sz) =>
		sz switch { 0 => ".b", 1 => ".w", 2 => ".l", _ => "" };

	private static int ReadWord(ReadOnlySpan<byte> data, int pos) =>
		pos + 1 < data.Length ? (data[pos] << 8) | data[pos + 1] : 0;

	private static int ReadWordSafe(ReadOnlySpan<byte> data, int pos) =>
		pos + 1 < data.Length ? (data[pos] << 8) | data[pos + 1] : 0;

	private static int ReadLongSafe(ReadOnlySpan<byte> data, int pos) =>
		pos + 3 < data.Length
			? (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3]
			: 0;
}
