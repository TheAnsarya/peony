namespace Peony.Cpu.GameBoy;

using Peony.Core;

/// <summary>
/// Game Boy CPU (Sharp LR35902) decoder
/// Mix of 8080 and Z80 instructions with some differences
/// </summary>
public class GameBoyCpuDecoder : ICpuDecoder {
	public string Architecture => "LR35902";
	public string CpuName => "LR35902 (Game Boy)";

	private static readonly Dictionary<byte, (string mnemonic, AddressingMode mode, int bytes)> Opcodes = new() {
		// 8-bit loads
		[0x06] = ("ld", AddressingMode.Immediate, 2),    // ld b, n
		[0x0e] = ("ld", AddressingMode.Immediate, 2),    // ld c, n
		[0x16] = ("ld", AddressingMode.Immediate, 2),    // ld d, n
		[0x1e] = ("ld", AddressingMode.Immediate, 2),    // ld e, n
		[0x26] = ("ld", AddressingMode.Immediate, 2),    // ld h, n
		[0x2e] = ("ld", AddressingMode.Immediate, 2),    // ld l, n
		[0x3e] = ("ld", AddressingMode.Immediate, 2),    // ld a, n

		// 16-bit loads
		[0x01] = ("ld", AddressingMode.Immediate, 3),    // ld bc, nn
		[0x11] = ("ld", AddressingMode.Immediate, 3),    // ld de, nn
		[0x21] = ("ld", AddressingMode.Immediate, 3),    // ld hl, nn
		[0x31] = ("ld", AddressingMode.Immediate, 3),    // ld sp, nn

		// Stack operations
		[0xc1] = ("pop", AddressingMode.Implied, 1),     // pop bc
		[0xd1] = ("pop", AddressingMode.Implied, 1),     // pop de
		[0xe1] = ("pop", AddressingMode.Implied, 1),     // pop hl
		[0xf1] = ("pop", AddressingMode.Implied, 1),     // pop af
		[0xc5] = ("push", AddressingMode.Implied, 1),    // push bc
		[0xd5] = ("push", AddressingMode.Implied, 1),    // push de
		[0xe5] = ("push", AddressingMode.Implied, 1),    // push hl
		[0xf5] = ("push", AddressingMode.Implied, 1),    // push af

		// Jumps
		[0xc2] = ("jp", AddressingMode.Absolute, 3),     // jp nz, nn
		[0xca] = ("jp", AddressingMode.Absolute, 3),     // jp z, nn
		[0xd2] = ("jp", AddressingMode.Absolute, 3),     // jp nc, nn
		[0xda] = ("jp", AddressingMode.Absolute, 3),     // jp c, nn
		[0xc3] = ("jp", AddressingMode.Absolute, 3),     // jp nn
		[0xe9] = ("jp", AddressingMode.Indirect, 1),     // jp (hl)

		// Calls
		[0xc4] = ("call", AddressingMode.Absolute, 3),   // call nz, nn
		[0xcc] = ("call", AddressingMode.Absolute, 3),   // call z, nn
		[0xd4] = ("call", AddressingMode.Absolute, 3),   // call nc, nn
		[0xdc] = ("call", AddressingMode.Absolute, 3),   // call c, nn
		[0xcd] = ("call", AddressingMode.Absolute, 3),   // call nn

		// Returns
		[0xc0] = ("ret", AddressingMode.Implied, 1),     // ret nz
		[0xc8] = ("ret", AddressingMode.Implied, 1),     // ret z
		[0xd0] = ("ret", AddressingMode.Implied, 1),     // ret nc
		[0xd8] = ("ret", AddressingMode.Implied, 1),     // ret c
		[0xc9] = ("ret", AddressingMode.Implied, 1),     // ret
		[0xd9] = ("reti", AddressingMode.Implied, 1),    // reti

		// Restarts
		[0xc7] = ("rst", AddressingMode.Implied, 1),     // rst $00
		[0xcf] = ("rst", AddressingMode.Implied, 1),     // rst $08
		[0xd7] = ("rst", AddressingMode.Implied, 1),     // rst $10
		[0xdf] = ("rst", AddressingMode.Implied, 1),     // rst $18
		[0xe7] = ("rst", AddressingMode.Implied, 1),     // rst $20
		[0xef] = ("rst", AddressingMode.Implied, 1),     // rst $28
		[0xf7] = ("rst", AddressingMode.Implied, 1),     // rst $30
		[0xff] = ("rst", AddressingMode.Implied, 1),     // rst $38

		// ALU operations
		[0x80] = ("add", AddressingMode.Implied, 1),     // add a, b
		[0x81] = ("add", AddressingMode.Implied, 1),     // add a, c
		[0x82] = ("add", AddressingMode.Implied, 1),     // add a, d
		[0x83] = ("add", AddressingMode.Implied, 1),     // add a, e
		[0x84] = ("add", AddressingMode.Implied, 1),     // add a, h
		[0x85] = ("add", AddressingMode.Implied, 1),     // add a, l
		[0x86] = ("add", AddressingMode.Indirect, 1),    // add a, (hl)
		[0x87] = ("add", AddressingMode.Implied, 1),     // add a, a
		[0xc6] = ("add", AddressingMode.Immediate, 2),   // add a, n

		// Compare
		[0xfe] = ("cp", AddressingMode.Immediate, 2),    // cp n

		// Relative jumps
		[0x18] = ("jr", AddressingMode.Relative, 2),     // jr n
		[0x20] = ("jr", AddressingMode.Relative, 2),     // jr nz, n
		[0x28] = ("jr", AddressingMode.Relative, 2),     // jr z, n
		[0x30] = ("jr", AddressingMode.Relative, 2),     // jr nc, n
		[0x38] = ("jr", AddressingMode.Relative, 2),     // jr c, n

		// Misc
		[0x00] = ("nop", AddressingMode.Implied, 1),     // nop
		[0x76] = ("halt", AddressingMode.Implied, 1),    // halt
		[0xf3] = ("di", AddressingMode.Implied, 1),      // di
		[0xfb] = ("ei", AddressingMode.Implied, 1),      // ei
		[0xcb] = ("cb", AddressingMode.Implied, 2),      // CB prefix

		// Special loads
		[0xe0] = ("ldh", AddressingMode.Immediate, 2),   // ldh ($ff00+n), a
		[0xf0] = ("ldh", AddressingMode.Immediate, 2),   // ldh a, ($ff00+n)
		[0xe2] = ("ld", AddressingMode.Indirect, 1),     // ld ($ff00+c), a
		[0xf2] = ("ld", AddressingMode.Indirect, 1),     // ld a, ($ff00+c)
		[0xea] = ("ld", AddressingMode.Absolute, 3),     // ld (nn), a
		[0xfa] = ("ld", AddressingMode.Absolute, 3),     // ld a, (nn)
	};

	// CB-prefixed opcodes (bit operations)
	private static readonly Dictionary<byte, (string mnemonic, AddressingMode mode)> CbOpcodes = new() {
		// Bit test
		[0x40] = ("bit", AddressingMode.Implied),  // bit 0, b
		[0x47] = ("bit", AddressingMode.Implied),  // bit 0, a
		[0x4e] = ("bit", AddressingMode.Indirect), // bit 1, (hl)
		[0x56] = ("bit", AddressingMode.Indirect), // bit 2, (hl)
		[0x5e] = ("bit", AddressingMode.Indirect), // bit 3, (hl)
		[0x66] = ("bit", AddressingMode.Indirect), // bit 4, (hl)
		[0x6e] = ("bit", AddressingMode.Indirect), // bit 5, (hl)
		[0x76] = ("bit", AddressingMode.Indirect), // bit 6, (hl)
		[0x7e] = ("bit", AddressingMode.Indirect), // bit 7, (hl)

		// Rotate left
		[0x00] = ("rlc", AddressingMode.Implied),  // rlc b
		[0x07] = ("rlc", AddressingMode.Implied),  // rlc a
		[0x06] = ("rlc", AddressingMode.Indirect), // rlc (hl)

		// Rotate right
		[0x08] = ("rrc", AddressingMode.Implied),  // rrc b
		[0x0f] = ("rrc", AddressingMode.Implied),  // rrc a
		[0x0e] = ("rrc", AddressingMode.Indirect), // rrc (hl)

		// Shift left arithmetic
		[0x20] = ("sla", AddressingMode.Implied),  // sla b
		[0x27] = ("sla", AddressingMode.Implied),  // sla a
		[0x26] = ("sla", AddressingMode.Indirect), // sla (hl)

		// Shift right arithmetic
		[0x28] = ("sra", AddressingMode.Implied),  // sra b
		[0x2f] = ("sra", AddressingMode.Implied),  // sra a
		[0x2e] = ("sra", AddressingMode.Indirect), // sra (hl)

		// Swap nibbles
		[0x30] = ("swap", AddressingMode.Implied), // swap b
		[0x37] = ("swap", AddressingMode.Implied), // swap a
		[0x36] = ("swap", AddressingMode.Indirect), // swap (hl)

		// Set bit
		[0xc0] = ("set", AddressingMode.Implied),  // set 0, b
		[0xc7] = ("set", AddressingMode.Implied),  // set 0, a
		[0xce] = ("set", AddressingMode.Indirect), // set 1, (hl)

		// Reset bit
		[0x80] = ("res", AddressingMode.Implied),  // res 0, b
		[0x87] = ("res", AddressingMode.Implied),  // res 0, a
		[0x8e] = ("res", AddressingMode.Indirect), // res 1, (hl)
	};

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.IsEmpty)
			return new DecodedInstruction("???", "", [], AddressingMode.Implied);

		var opcode = data[0];

		// CB prefix
		if (opcode == 0xcb) {
			if (data.Length < 2)
				return new DecodedInstruction("cb", "???", data[..1].ToArray(), AddressingMode.Implied);

			var cbOp = data[1];
			if (CbOpcodes.TryGetValue(cbOp, out var cbInfo)) {
				var cbOperand = FormatCbOperand(cbOp);
				return new DecodedInstruction(cbInfo.mnemonic, cbOperand, data[..2].ToArray(), cbInfo.mode);
			}

			return new DecodedInstruction("???", "", data[..2].ToArray(), AddressingMode.Implied);
		}

		// Regular opcodes
		if (!Opcodes.TryGetValue(opcode, out var info))
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		if (data.Length < info.bytes)
			return new DecodedInstruction(info.mnemonic, "???", data[..1].ToArray(), info.mode);

		var bytes = data[..info.bytes].ToArray();
		var operand = FormatOperand(opcode, data, address);

		return new DecodedInstruction(info.mnemonic, operand, bytes, info.mode);
	}

	private static string FormatOperand(byte opcode, ReadOnlySpan<byte> data, uint address) {
		return opcode switch {
			// 8-bit loads
			0x06 => $"b, ${data[1]:x2}",
			0x0e => $"c, ${data[1]:x2}",
			0x16 => $"d, ${data[1]:x2}",
			0x1e => $"e, ${data[1]:x2}",
			0x26 => $"h, ${data[1]:x2}",
			0x2e => $"l, ${data[1]:x2}",
			0x3e => $"a, ${data[1]:x2}",

			// 16-bit loads
			0x01 or 0x11 or 0x21 or 0x31 => FormatWord(data[1], data[2], opcode),

			// Stack
			0xc1 => "bc",
			0xd1 => "de",
			0xe1 => "hl",
			0xf1 => "af",
			0xc5 => "bc",
			0xd5 => "de",
			0xe5 => "hl",
			0xf5 => "af",

			// Jumps
			0xc2 => $"nz, ${(data[1] | (data[2] << 8)):x4}",
			0xca => $"z, ${(data[1] | (data[2] << 8)):x4}",
			0xd2 => $"nc, ${(data[1] | (data[2] << 8)):x4}",
			0xda => $"c, ${(data[1] | (data[2] << 8)):x4}",
			0xc3 => $"${(data[1] | (data[2] << 8)):x4}",
			0xe9 => "(hl)",

			// Calls
			0xc4 => $"nz, ${(data[1] | (data[2] << 8)):x4}",
			0xcc => $"z, ${(data[1] | (data[2] << 8)):x4}",
			0xd4 => $"nc, ${(data[1] | (data[2] << 8)):x4}",
			0xdc => $"c, ${(data[1] | (data[2] << 8)):x4}",
			0xcd => $"${(data[1] | (data[2] << 8)):x4}",

			// Returns
			0xc0 => "nz",
			0xc8 => "z",
			0xd0 => "nc",
			0xd8 => "c",

			// Restarts
			0xc7 => "$00",
			0xcf => "$08",
			0xd7 => "$10",
			0xdf => "$18",
			0xe7 => "$20",
			0xef => "$28",
			0xf7 => "$30",
			0xff => "$38",

			// ALU
			0x80 => "a, b",
			0x81 => "a, c",
			0x82 => "a, d",
			0x83 => "a, e",
			0x84 => "a, h",
			0x85 => "a, l",
			0x86 => "a, (hl)",
			0x87 => "a, a",
			0xc6 => $"a, ${data[1]:x2}",

			// Compare
			0xfe => $"${data[1]:x2}",

			// Relative jumps
			0x18 or 0x20 or 0x28 or 0x30 or 0x38 => FormatRelative(data[1], address, opcode),

			// Special loads
			0xe0 => $"($ff{data[1]:x2}), a",
			0xf0 => $"a, ($ff{data[1]:x2})",
			0xe2 => "($ff00+c), a",
			0xf2 => "a, ($ff00+c)",
			0xea => $"(${(data[1] | (data[2] << 8)):x4}), a",
			0xfa => $"a, (${(data[1] | (data[2] << 8)):x4})",

			_ => ""
		};
	}

	private static string FormatWord(byte low, byte high, byte opcode) {
		var reg = opcode switch {
			0x01 => "bc",
			0x11 => "de",
			0x21 => "hl",
			0x31 => "sp",
			_ => "??"
		};
		return $"{reg}, ${(low | (high << 8)):x4}";
	}

	private static string FormatRelative(byte offset, uint address, byte opcode) {
		var signedOffset = (sbyte)offset;
		var target = (uint)(address + 2 + signedOffset);
		var condition = opcode switch {
			0x20 => "nz, ",
			0x28 => "z, ",
			0x30 => "nc, ",
			0x38 => "c, ",
			_ => ""
		};
		return $"{condition}${target:x4}";
	}

	private static string FormatCbOperand(byte cbOp) {
		var bit = (cbOp >> 3) & 7;
		var reg = cbOp & 7;
		var regName = reg switch {
			0 => "b", 1 => "c", 2 => "d", 3 => "e",
			4 => "h", 5 => "l", 6 => "(hl)", 7 => "a",
			_ => "?"
		};

		return (cbOp & 0xc0) switch {
			0x40 => $"{bit}, {regName}", // bit
			0x80 => $"{bit}, {regName}", // res
			0xc0 => $"{bit}, {regName}", // set
			_ => regName // rotate/shift/swap
		};
	}

	private static bool IsControlFlowInstruction(string mnemonic) {
		return mnemonic is "jp" or "jr" or "call" or "ret" or "reti" or "rst";
	}

	private static uint? GetTarget(string mnemonic, AddressingMode mode, ReadOnlySpan<byte> data, uint address) {
		return mnemonic switch {
			"jp" when mode == AddressingMode.Absolute => (uint)(data[1] | (data[2] << 8)),
			"call" when mode == AddressingMode.Absolute => (uint)(data[1] | (data[2] << 8)),
			"jr" => (uint)(address + 2 + (sbyte)data[1]),
			"rst" => data[0] switch {
				0xc7 => 0x00u, 0xcf => 0x08u, 0xd7 => 0x10u, 0xdf => 0x18u,
				0xe7 => 0x20u, 0xef => 0x28u, 0xf7 => 0x30u, 0xff => 0x38u,
				_ => null
			},
			_ => null
		};
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return IsControlFlowInstruction(instruction.Mnemonic);
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		// Extract target from the decoded bytes
		var target = GetTarget(instruction.Mnemonic, instruction.Mode, instruction.Bytes, address);
		if (target.HasValue)
			yield return target.Value;
	}
}
