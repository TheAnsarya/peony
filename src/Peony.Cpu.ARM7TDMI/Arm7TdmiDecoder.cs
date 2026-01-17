namespace Peony.Cpu.ARM7TDMI;

using Peony.Core;

/// <summary>
/// ARM7TDMI CPU decoder (Game Boy Advance)
/// Supports both ARM (32-bit) and Thumb (16-bit) instruction sets
/// </summary>
public class Arm7TdmiDecoder : ICpuDecoder {
	public string Architecture => "ARM7TDMI";
	
	private bool _thumbMode = false;

	/// <summary>Set CPU mode (ARM or Thumb)</summary>
	public bool ThumbMode {
		get => _thumbMode;
		set => _thumbMode = value;
	}

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (_thumbMode)
			return DecodeThumb(data, address);
		else
			return DecodeArm(data, address);
	}

	private DecodedInstruction DecodeArm(ReadOnlySpan<byte> data, uint address) {
		if (data.Length < 4)
			return new DecodedInstruction("???", "", [], AddressingMode.Implied);

		var instr = (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
		var cond = (instr >> 28) & 0xf;
		var bytes = data[..4].ToArray();

		// Branch and exchange (BX)
		if ((instr & 0x0ffffff0) == 0x012fff10) {
			var rn = (int)(instr & 0xf);
			return new DecodedInstruction("bx", $"r{rn}", bytes, AddressingMode.Implied);
		}

		// Branch/Branch with Link
		if ((instr & 0x0e000000) == 0x0a000000) {
			var link = (instr & 0x01000000) != 0;
			var offset = (int)(instr & 0x00ffffff);
			if ((offset & 0x00800000) != 0) offset |= unchecked((int)0xff000000); // Sign extend
			var target = (uint)(address + 8 + (offset << 2));
			var mnemonic = link ? "bl" : "b";
			return new DecodedInstruction(mnemonic, $"${target:x8}", bytes, AddressingMode.Absolute);
		}

		// Data processing
		if ((instr & 0x0c000000) == 0x00000000) {
			var opcode = (int)((instr >> 21) & 0xf);
			var s = (instr & 0x00100000) != 0;
			var rn = (int)((instr >> 16) & 0xf);
			var rd = (int)((instr >> 12) & 0xf);
			var imm = (instr & 0x02000000) != 0;

			var mnemonic = opcode switch {
				0x0 => "and", 0x1 => "eor", 0x2 => "sub", 0x3 => "rsb",
				0x4 => "add", 0x5 => "adc", 0x6 => "sbc", 0x7 => "rsc",
				0x8 => "tst", 0x9 => "teq", 0xa => "cmp", 0xb => "cmn",
				0xc => "orr", 0xd => "mov", 0xe => "bic", 0xf => "mvn",
				_ => "???"
			};

			if (s && opcode >= 0x8 && opcode <= 0xb) {
				// Test/compare don't write result
				var op2 = FormatOperand2(instr);
				return new DecodedInstruction(mnemonic, $"r{rn}, {op2}", bytes, AddressingMode.Implied);
			}

			if (opcode == 0xd || opcode == 0xf) {
				// MOV/MVN only use one source register
				var op2 = FormatOperand2(instr);
				return new DecodedInstruction(mnemonic, $"r{rd}, {op2}", bytes, AddressingMode.Implied);
			}

			var operand2 = FormatOperand2(instr);
			return new DecodedInstruction(mnemonic, $"r{rd}, r{rn}, {operand2}", bytes, AddressingMode.Implied);
		}

		// Load/Store
		if ((instr & 0x0c000000) == 0x04000000) {
			var load = (instr & 0x00100000) != 0;
			var isByte = (instr & 0x00400000) != 0;
			var rd = (int)((instr >> 12) & 0xf);
			var rn = (int)((instr >> 16) & 0xf);

			var mnemonic = load ? (isByte ? "ldrb" : "ldr") : (isByte ? "strb" : "str");
			var offset = FormatLoadStoreOffset(instr);
			return new DecodedInstruction(mnemonic, $"r{rd}, [r{rn}{offset}]", bytes, AddressingMode.Indirect);
		}

		// Multiply
		if ((instr & 0x0fc000f0) == 0x00000090) {
			var acc = (instr & 0x00200000) != 0;
			var rd = (int)((instr >> 16) & 0xf);
			var rn = (int)((instr >> 12) & 0xf);
			var rs = (int)((instr >> 8) & 0xf);
			var rm = (int)(instr & 0xf);

			if (acc) {
				return new DecodedInstruction("mla", $"r{rd}, r{rm}, r{rs}, r{rn}", bytes, AddressingMode.Implied);
			} else {
				return new DecodedInstruction("mul", $"r{rd}, r{rm}, r{rs}", bytes, AddressingMode.Implied);
			}
		}

		// Software interrupt
		if ((instr & 0x0f000000) == 0x0f000000) {
			var swi = instr & 0x00ffffff;
			return new DecodedInstruction("swi", $"${swi:x6}", bytes, AddressingMode.Immediate);
		}

		return new DecodedInstruction("???", $"${instr:x8}", bytes, AddressingMode.Implied);
	}

	private DecodedInstruction DecodeThumb(ReadOnlySpan<byte> data, uint address) {
		if (data.Length < 2)
			return new DecodedInstruction("???", "", [], AddressingMode.Implied);

		var instr = (ushort)(data[0] | (data[1] << 8));
		var bytes = data[..2].ToArray();

		// Branch
		if ((instr & 0xf000) == 0xd000) {
			var cond = (instr >> 8) & 0xf;
			if (cond == 0xe) {
				// Undefined
				return new DecodedInstruction("udf", $"${instr:x4}", bytes, AddressingMode.Implied);
			}
			if (cond == 0xf) {
				// SWI
				var swi = instr & 0xff;
				return new DecodedInstruction("swi", $"${swi:x2}", bytes, AddressingMode.Immediate);
			}
			// Conditional branch
			var offset = (sbyte)(instr & 0xff);
			var target = (uint)(address + 4 + (offset << 1));
			var condName = GetConditionName(cond);
			return new DecodedInstruction($"b{condName}", $"${target:x8}", bytes, AddressingMode.Relative);
		}

		// Unconditional branch
		if ((instr & 0xf800) == 0xe000) {
			var offset = (int)(instr & 0x7ff);
			if ((offset & 0x400) != 0) offset |= unchecked((int)0xfffff800); // Sign extend
			var target = (uint)(address + 4 + (offset << 1));
			return new DecodedInstruction("b", $"${target:x8}", bytes, AddressingMode.Relative);
		}

		// Long branch with link
		if ((instr & 0xf000) == 0xf000) {
			var offset = (int)(instr & 0x7ff);
			var h = (instr >> 11) & 3;
			if (h == 0) {
				// BL prefix
				if ((offset & 0x400) != 0) offset |= unchecked((int)0xfffff800);
				return new DecodedInstruction("bl", $"prefix ${offset:x}", bytes, AddressingMode.Immediate);
			} else if (h == 1 || h == 3) {
				// BL suffix
				return new DecodedInstruction("bl", $"offset ${offset:x}", bytes, AddressingMode.Immediate);
			}
		}

		// Load/Store with register offset
		if ((instr & 0xf200) == 0x5000) {
			var load = (instr & 0x0800) != 0;
			var isByte = (instr & 0x0400) != 0;
			var ro = (int)((instr >> 6) & 0x7);
			var rb = (int)((instr >> 3) & 0x7);
			var rd = (int)(instr & 0x7);
			var mnemonic = load ? (isByte ? "ldrb" : "ldr") : (isByte ? "strb" : "str");
			return new DecodedInstruction(mnemonic, $"r{rd}, [r{rb}, r{ro}]", bytes, AddressingMode.Indirect);
		}

		// Load/Store with immediate offset
		if ((instr & 0xe000) == 0x6000) {
			var isByte = (instr & 0x1000) != 0;
			var load = (instr & 0x0800) != 0;
			var offset = (int)((instr >> 6) & 0x1f);
			if (!isByte) offset <<= 2;
			var rb = (int)((instr >> 3) & 0x7);
			var rd = (int)(instr & 0x7);
			var mnemonic = load ? (isByte ? "ldrb" : "ldr") : (isByte ? "strb" : "str");
			return new DecodedInstruction(mnemonic, $"r{rd}, [r{rb}, #{offset}]", bytes, AddressingMode.Indirect);
		}

		// ADD/SUB
		if ((instr & 0xf800) == 0x1800) {
			var sub = (instr & 0x0200) != 0;
			var imm = (instr & 0x0400) != 0;
			var rnOrImm = (int)((instr >> 6) & 0x7);
			var rs = (int)((instr >> 3) & 0x7);
			var rd = (int)(instr & 0x7);
			var mnemonic = sub ? "sub" : "add";
			var op3 = imm ? $"#{rnOrImm}" : $"r{rnOrImm}";
			return new DecodedInstruction(mnemonic, $"r{rd}, r{rs}, {op3}", bytes, AddressingMode.Implied);
		}

		// MOV/CMP immediate
		if ((instr & 0xe000) == 0x2000) {
			var opcode = (instr >> 11) & 0x3;
			var rd = (int)((instr >> 8) & 0x7);
			var imm = (int)(instr & 0xff);
			var mnemonic = opcode switch {
				0 => "mov", 1 => "cmp", 2 => "add", 3 => "sub", _ => "???"
			};
			if (opcode == 1)
				return new DecodedInstruction(mnemonic, $"r{rd}, #{imm}", bytes, AddressingMode.Immediate);
			else
				return new DecodedInstruction(mnemonic, $"r{rd}, #{imm}", bytes, AddressingMode.Immediate);
		}

		// ALU operations
		if ((instr & 0xfc00) == 0x4000) {
			var opcode = (int)((instr >> 6) & 0xf);
			var rs = (int)((instr >> 3) & 0x7);
			var rd = (int)(instr & 0x7);
			var mnemonic = opcode switch {
				0x0 => "and", 0x1 => "eor", 0x2 => "lsl", 0x3 => "lsr",
				0x4 => "asr", 0x5 => "adc", 0x6 => "sbc", 0x7 => "ror",
				0x8 => "tst", 0x9 => "neg", 0xa => "cmp", 0xb => "cmn",
				0xc => "orr", 0xd => "mul", 0xe => "bic", 0xf => "mvn",
				_ => "???"
			};
			return new DecodedInstruction(mnemonic, $"r{rd}, r{rs}", bytes, AddressingMode.Implied);
		}

		// Push/Pop
		if ((instr & 0xf600) == 0xb400) {
			var load = (instr & 0x0800) != 0;
			var r = (instr & 0x0100) != 0;
			var rlist = instr & 0xff;
			var mnemonic = load ? "pop" : "push";
			var regs = FormatRegisterList(rlist, r, load);
			return new DecodedInstruction(mnemonic, $"{{{regs}}}", bytes, AddressingMode.Implied);
		}

		return new DecodedInstruction("???", $"${instr:x4}", bytes, AddressingMode.Implied);
	}

	private static string FormatOperand2(uint instr) {
		var imm = (instr & 0x02000000) != 0;
		if (imm) {
			var value = (int)(instr & 0xff);
			var rotate = (int)((instr >> 8) & 0xf) * 2;
			var rotated = (uint)((value >> rotate) | (value << (32 - rotate)));
			return $"#{rotated}";
		} else {
			var rm = (int)(instr & 0xf);
			var shift = (instr >> 4) & 0xff;
			if (shift == 0)
				return $"r{rm}";
			
			var shiftType = (int)((shift >> 1) & 0x3);
			var shiftAmount = (int)(shift >> 3);
			var shiftName = shiftType switch {
				0 => "lsl", 1 => "lsr", 2 => "asr", 3 => "ror", _ => "???"
			};
			return $"r{rm}, {shiftName} #{shiftAmount}";
		}
	}

	private static string FormatLoadStoreOffset(uint instr) {
		var imm = (instr & 0x02000000) == 0;
		var up = (instr & 0x00800000) != 0;
		var sign = up ? "" : "-";

		if (imm) {
			var offset = (int)(instr & 0xfff);
			return offset == 0 ? "" : $", #{sign}{offset}";
		} else {
			var rm = (int)(instr & 0xf);
			return $", {sign}r{rm}";
		}
	}

	private static string GetConditionName(int cond) {
		return cond switch {
			0x0 => "eq", 0x1 => "ne", 0x2 => "cs", 0x3 => "cc",
			0x4 => "mi", 0x5 => "pl", 0x6 => "vs", 0x7 => "vc",
			0x8 => "hi", 0x9 => "ls", 0xa => "ge", 0xb => "lt",
			0xc => "gt", 0xd => "le", 0xe => "", 0xf => "nv",
			_ => "??"
		};
	}

	private static string FormatRegisterList(int rlist, bool extraReg, bool load) {
		var regs = new List<string>();
		for (int i = 0; i < 8; i++) {
			if ((rlist & (1 << i)) != 0)
				regs.Add($"r{i}");
		}
		if (extraReg)
			regs.Add(load ? "pc" : "lr");
		return string.Join(", ", regs);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		var m = instruction.Mnemonic;
		return m.StartsWith("b") || m == "bx" || m == "swi" || 
		       (m == "pop" && instruction.Operand.Contains("pc")) ||
		       (m == "mov" && instruction.Operand.StartsWith("pc,"));
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		if (instruction.Mode == AddressingMode.Absolute || instruction.Mode == AddressingMode.Relative) {
			// Extract target from operand (format: $xxxxxxxx)
			var operand = instruction.Operand;
			var dollarIdx = operand.IndexOf('$');
			if (dollarIdx >= 0) {
				var hexStart = dollarIdx + 1;
				var hexEnd = hexStart;
				while (hexEnd < operand.Length && Uri.IsHexDigit(operand[hexEnd]))
					hexEnd++;
				
				if (hexEnd > hexStart && uint.TryParse(operand[hexStart..hexEnd], 
					System.Globalization.NumberStyles.HexNumber, null, out var target)) {
					yield return target;
				}
			}
		}
	}
}
