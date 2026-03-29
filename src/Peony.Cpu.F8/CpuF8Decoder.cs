namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// Fairchild F8 CPU instruction decoder for Channel F disassembly.
/// Decodes all 256 opcodes including register-encoded and branch instructions.
/// </summary>
public sealed class CpuF8Decoder : ICpuDecoder {
	public string Architecture => "F8";

	// Opcode table: (mnemonic, operandFunc, bytes)
	// For 1-byte register-encoded opcodes, operand is generated dynamically.
	private static readonly (string Mnemonic, int Bytes, OpcodeKind Kind)[] OpcodeTable = new (string, int, OpcodeKind)[256];

	private enum OpcodeKind {
		Implied,        // 1 byte, no operand
		Immediate,      // 2 bytes, #$nn
		Absolute,       // 3 bytes, $mmnn
		Relative,       // 2 bytes, branch offset
		RegLow4,        // 1 byte, register in low nibble (0-15)
		RegLow3,        // 1 byte, 3-bit value in low 3 bits (0-7)
		RegLow4Imm,     // 1 byte, 4-bit immediate in low nibble (0-15)
		Undefined       // Undefined opcode
	}

	static CpuF8Decoder() {
		InitializeOpcodeTable();
	}

	public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
		if (data.Length == 0)
			return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

		var opcode = data[0];
		var (mnemonic, length, kind) = OpcodeTable[opcode];

		if (data.Length < length)
			return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

		var bytes = data[..length].ToArray();
		var operand = FormatOperand(data, opcode, kind, address);
		var mode = KindToMode(kind);

		return new DecodedInstruction(mnemonic, operand, bytes, mode);
	}

	public bool IsControlFlow(DecodedInstruction instruction) {
		return instruction.Mnemonic is "jmp" or "pi" or "pk" or "pop" or
			"br" or "br7" or
			"bm" or "bnc" or "bnz" or "bno" or "bp" or "bc" or "bz" or
			"bt" or "bf";
	}

	public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
		var nextAddress = address + (uint)instruction.Bytes.Length;

		switch (instruction.Mode) {
			case AddressingMode.Absolute when instruction.Mnemonic is "jmp":
				// JMP — unconditional, single target
				yield return (uint)((instruction.Bytes[1] << 8) | instruction.Bytes[2]);
				break;

			case AddressingMode.Absolute when instruction.Mnemonic is "pi":
				// PI — call subroutine, returns to next instruction
				yield return (uint)((instruction.Bytes[1] << 8) | instruction.Bytes[2]);
				yield return nextAddress;
				break;

			case AddressingMode.Relative:
				// Branch instructions — offset from next instruction PC
				var offset = (sbyte)instruction.Bytes[1];
				yield return (uint)(nextAddress + offset);
				if (instruction.Mnemonic != "br") // Conditional branches also fall through
					yield return nextAddress;
				break;
		}
	}

	private static AddressingMode KindToMode(OpcodeKind kind) {
		return kind switch {
			OpcodeKind.Immediate => AddressingMode.Immediate,
			OpcodeKind.Absolute => AddressingMode.Absolute,
			OpcodeKind.Relative => AddressingMode.Relative,
			_ => AddressingMode.Implied
		};
	}

	private static string FormatOperand(ReadOnlySpan<byte> data, byte opcode, OpcodeKind kind, uint address) {
		return kind switch {
			OpcodeKind.Implied => "",
			OpcodeKind.Immediate => $"#${data[1]:x2}",
			OpcodeKind.Absolute => $"${(data[1] << 8) | data[2]:x4}",
			OpcodeKind.Relative => FormatRelative(data, address),
			OpcodeKind.RegLow4 => FormatRegister(opcode & 0x0f),
			OpcodeKind.RegLow3 => $"{opcode & 0x07}",
			OpcodeKind.RegLow4Imm => $"#{opcode & 0x0f}",
			OpcodeKind.Undefined => "",
			_ => ""
		};
	}

	private static string FormatRelative(ReadOnlySpan<byte> data, uint address) {
		var offset = (sbyte)data[1];
		var target = (uint)(address + 2 + offset); // PC is at next instruction (address + 2)
		return $"${target:x4}";
	}

	/// <summary>
	/// Format scratchpad register reference. Registers 0-11 are r0-r11,
	/// r12-r13 are K (KU/KL), r14-r15 are Q (QU/QL).
	/// ISAR indirect registers: 10=hu, 11=hl, 12=s (ISAR indirect).
	/// </summary>
	private static string FormatRegister(int reg) {
		return reg switch {
			>= 0 and <= 9 => $"r{reg}",
			10 => "hu",
			11 => "hl",
			12 => "s",    // ISAR indirect
			13 => "i",    // ISAR indirect, auto-increment
			14 => "d",    // ISAR indirect, auto-decrement
			15 => "r15",
			_ => $"r{reg}"
		};
	}

	private static void InitializeOpcodeTable() {
		// Default all to undefined
		for (int i = 0; i < 256; i++)
			OpcodeTable[i] = ("???", 1, OpcodeKind.Undefined);

		// === $00-$07: LR register transfers (A ↔ K/Q) ===
		OpcodeTable[0x00] = ("lr", 1, OpcodeKind.Implied);  // LR A,KU  (operand handled by mnemonic context)
		OpcodeTable[0x01] = ("lr", 1, OpcodeKind.Implied);  // LR A,KL
		OpcodeTable[0x02] = ("lr", 1, OpcodeKind.Implied);  // LR A,QU
		OpcodeTable[0x03] = ("lr", 1, OpcodeKind.Implied);  // LR A,QL
		OpcodeTable[0x04] = ("lr", 1, OpcodeKind.Implied);  // LR KU,A
		OpcodeTable[0x05] = ("lr", 1, OpcodeKind.Implied);  // LR KL,A
		OpcodeTable[0x06] = ("lr", 1, OpcodeKind.Implied);  // LR QU,A
		OpcodeTable[0x07] = ("lr", 1, OpcodeKind.Implied);  // LR QL,A

		// === $08-$0F: More register transfers ===
		OpcodeTable[0x08] = ("lr", 1, OpcodeKind.Implied);  // LR K,P
		OpcodeTable[0x09] = ("lr", 1, OpcodeKind.Implied);  // LR P,K
		OpcodeTable[0x0a] = ("lr", 1, OpcodeKind.Implied);  // LR A,IS
		OpcodeTable[0x0b] = ("lr", 1, OpcodeKind.Implied);  // LR IS,A
		OpcodeTable[0x0c] = ("pk", 1, OpcodeKind.Implied);  // PK (call via K)
		OpcodeTable[0x0d] = ("lr", 1, OpcodeKind.Implied);  // LR P0,Q
		OpcodeTable[0x0e] = ("lr", 1, OpcodeKind.Implied);  // LR Q,DC
		OpcodeTable[0x0f] = ("lr", 1, OpcodeKind.Implied);  // LR DC,Q

		// === $10-$11: DC ↔ H transfers ===
		OpcodeTable[0x10] = ("lr", 1, OpcodeKind.Implied);  // LR DC,H
		OpcodeTable[0x11] = ("lr", 1, OpcodeKind.Implied);  // LR H,DC

		// === $12-$15: Shifts ===
		OpcodeTable[0x12] = ("sr", 1, OpcodeKind.Implied);  // SR 1
		OpcodeTable[0x13] = ("sl", 1, OpcodeKind.Implied);  // SL 1
		OpcodeTable[0x14] = ("sr", 1, OpcodeKind.Implied);  // SR 4
		OpcodeTable[0x15] = ("sl", 1, OpcodeKind.Implied);  // SL 4

		// === $16-$17: Memory operations via DC0 ===
		OpcodeTable[0x16] = ("lm", 1, OpcodeKind.Implied);  // A ← (DC0), DC0++
		OpcodeTable[0x17] = ("st", 1, OpcodeKind.Implied);  // (DC0) ← A, DC0++

		// === $18-$1F: Misc operations ===
		OpcodeTable[0x18] = ("com", 1, OpcodeKind.Implied);  // A ← ~A
		OpcodeTable[0x19] = ("lnk", 1, OpcodeKind.Implied);  // A ← A + carry
		OpcodeTable[0x1a] = ("di", 1, OpcodeKind.Implied);   // Disable interrupts
		OpcodeTable[0x1b] = ("ei", 1, OpcodeKind.Implied);   // Enable interrupts
		OpcodeTable[0x1c] = ("pop", 1, OpcodeKind.Implied);  // Return (PC0 ← PC1)
		OpcodeTable[0x1d] = ("lr", 1, OpcodeKind.Implied);   // LR W,J
		OpcodeTable[0x1e] = ("lr", 1, OpcodeKind.Implied);   // LR J,W
		OpcodeTable[0x1f] = ("inc", 1, OpcodeKind.Implied);  // A ← A + 1

		// === $20-$27: 2-byte immediate operations ===
		OpcodeTable[0x20] = ("li", 2, OpcodeKind.Immediate);   // A ← n
		OpcodeTable[0x21] = ("ni", 2, OpcodeKind.Immediate);   // A ← A AND n
		OpcodeTable[0x22] = ("oi", 2, OpcodeKind.Immediate);   // A ← A OR n
		OpcodeTable[0x23] = ("xi", 2, OpcodeKind.Immediate);   // A ← A XOR n
		OpcodeTable[0x24] = ("ai", 2, OpcodeKind.Immediate);   // A ← A + n
		OpcodeTable[0x25] = ("ci", 2, OpcodeKind.Immediate);   // Compare A with n
		OpcodeTable[0x26] = ("in", 2, OpcodeKind.Immediate);   // A ← port n
		OpcodeTable[0x27] = ("out", 2, OpcodeKind.Immediate);  // port n ← A

		// === $28-$2A: 3-byte absolute address ===
		OpcodeTable[0x28] = ("pi", 3, OpcodeKind.Absolute);   // Call subroutine at mn
		OpcodeTable[0x29] = ("jmp", 3, OpcodeKind.Absolute);  // Jump to mn
		OpcodeTable[0x2a] = ("dci", 3, OpcodeKind.Absolute);  // DC0 ← mn

		// === $2B-$2C: Single byte misc ===
		OpcodeTable[0x2b] = ("nop", 1, OpcodeKind.Implied);  // No operation
		OpcodeTable[0x2c] = ("xdc", 1, OpcodeKind.Implied);  // Exchange DC0, DC1

		// $2D-$2F: Undefined (remain as "???" from default init)

		// === $30-$3F: DS r — Decrement scratchpad register ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0x30 + r] = ("ds", 1, OpcodeKind.RegLow4);

		// === $40-$4F: LR A,r — Load accumulator from scratchpad ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0x40 + r] = ("lr", 1, OpcodeKind.RegLow4);

		// === $50-$5F: LR r,A — Store accumulator to scratchpad ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0x50 + r] = ("lr", 1, OpcodeKind.RegLow4);

		// === $60-$67: LISU i — Load upper ISAR (3-bit) ===
		for (int i = 0; i <= 7; i++)
			OpcodeTable[0x60 + i] = ("lisu", 1, OpcodeKind.RegLow3);

		// === $68-$6F: LISL i — Load lower ISAR (3-bit) ===
		for (int i = 0; i <= 7; i++)
			OpcodeTable[0x68 + i] = ("lisl", 1, OpcodeKind.RegLow3);

		// === $70: CLR ===
		OpcodeTable[0x70] = ("clr", 1, OpcodeKind.Implied);

		// === $71-$7F: LIS i — Load immediate short (4-bit in opcode) ===
		for (int i = 1; i <= 15; i++)
			OpcodeTable[0x70 + i] = ("lis", 1, OpcodeKind.RegLow4Imm);

		// === $80-$87: BT t,n — Branch on true (test bits in opcode) ===
		//   $80=bt 0 (never), $81=bp, $82=bc, $84=bz (common aliases)
		OpcodeTable[0x80] = ("bt", 2, OpcodeKind.Relative);   // BT 0 (never branches)
		OpcodeTable[0x81] = ("bp", 2, OpcodeKind.Relative);   // Branch if positive
		OpcodeTable[0x82] = ("bc", 2, OpcodeKind.Relative);   // Branch if carry
		OpcodeTable[0x83] = ("bt", 2, OpcodeKind.Relative);   // BT 3
		OpcodeTable[0x84] = ("bz", 2, OpcodeKind.Relative);   // Branch if zero
		OpcodeTable[0x85] = ("bt", 2, OpcodeKind.Relative);   // BT 5
		OpcodeTable[0x86] = ("bt", 2, OpcodeKind.Relative);   // BT 6
		OpcodeTable[0x87] = ("bt", 2, OpcodeKind.Relative);   // BT 7

		// === $88-$8E: Memory operations via DC0 ===
		OpcodeTable[0x88] = ("am", 1, OpcodeKind.Implied);   // A ← A + (DC0), DC0++
		OpcodeTable[0x89] = ("amd", 1, OpcodeKind.Implied);  // A ← A + (DC0) decimal, DC0++
		OpcodeTable[0x8a] = ("nm", 1, OpcodeKind.Implied);   // A ← A AND (DC0), DC0++
		OpcodeTable[0x8b] = ("om", 1, OpcodeKind.Implied);   // A ← A OR (DC0), DC0++
		OpcodeTable[0x8c] = ("xm", 1, OpcodeKind.Implied);   // A ← A XOR (DC0), DC0++
		OpcodeTable[0x8d] = ("cm", 1, OpcodeKind.Implied);   // Compare (DC0) with A, DC0++
		OpcodeTable[0x8e] = ("adc", 1, OpcodeKind.Implied);  // DC0 ← DC0 + A

		// === $8F: BR7 — Branch if ISAR lower != 7 ===
		OpcodeTable[0x8f] = ("br7", 2, OpcodeKind.Relative);

		// === $90: BR — Unconditional branch ===
		OpcodeTable[0x90] = ("br", 2, OpcodeKind.Relative);

		// === $91-$9F: BF i,n — Branch on false ===
		//   $91=bm, $92=bnc, $94=bnz, $98=bno (common aliases)
		OpcodeTable[0x91] = ("bm", 2, OpcodeKind.Relative);   // Branch if minus (sign=1)
		OpcodeTable[0x92] = ("bnc", 2, OpcodeKind.Relative);  // Branch if no carry
		OpcodeTable[0x93] = ("bf", 2, OpcodeKind.Relative);   // BF 3
		OpcodeTable[0x94] = ("bnz", 2, OpcodeKind.Relative);  // Branch if not zero
		OpcodeTable[0x95] = ("bf", 2, OpcodeKind.Relative);   // BF 5
		OpcodeTable[0x96] = ("bf", 2, OpcodeKind.Relative);   // BF 6
		OpcodeTable[0x97] = ("bf", 2, OpcodeKind.Relative);   // BF 7
		OpcodeTable[0x98] = ("bno", 2, OpcodeKind.Relative);  // Branch if no overflow
		OpcodeTable[0x99] = ("bf", 2, OpcodeKind.Relative);   // BF 9
		OpcodeTable[0x9a] = ("bf", 2, OpcodeKind.Relative);   // BF $a
		OpcodeTable[0x9b] = ("bf", 2, OpcodeKind.Relative);   // BF $b
		OpcodeTable[0x9c] = ("bf", 2, OpcodeKind.Relative);   // BF $c
		OpcodeTable[0x9d] = ("bf", 2, OpcodeKind.Relative);   // BF $d
		OpcodeTable[0x9e] = ("bf", 2, OpcodeKind.Relative);   // BF $e
		OpcodeTable[0x9f] = ("bf", 2, OpcodeKind.Relative);   // BF $f

		// === $A0-$AF: INS i — Input from port (port in low nibble) ===
		for (int i = 0; i <= 15; i++)
			OpcodeTable[0xa0 + i] = ("ins", 1, OpcodeKind.RegLow4Imm);

		// === $B0-$BF: OUTS i — Output to port (port in low nibble) ===
		for (int i = 0; i <= 15; i++)
			OpcodeTable[0xb0 + i] = ("outs", 1, OpcodeKind.RegLow4Imm);

		// === $C0-$CF: AS r — Add scratchpad to accumulator ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0xc0 + r] = ("as", 1, OpcodeKind.RegLow4);

		// === $D0-$DF: ASD r — Add scratchpad to accumulator (decimal) ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0xd0 + r] = ("asd", 1, OpcodeKind.RegLow4);

		// === $E0-$EF: XS r — XOR scratchpad with accumulator ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0xe0 + r] = ("xs", 1, OpcodeKind.RegLow4);

		// === $F0-$FF: NS r — AND scratchpad with accumulator ===
		for (int r = 0; r <= 15; r++)
			OpcodeTable[0xf0 + r] = ("ns", 1, OpcodeKind.RegLow4);
	}

	/// <summary>
	/// Get the full mnemonic with register operand for LR instructions.
	/// The $00-$11, $1D-$1E range LR opcodes have unique register pairs
	/// that need special formatting beyond the generic table entries.
	/// </summary>
	public static string GetFullLrMnemonic(byte opcode) {
		return opcode switch {
			0x00 => "lr a,ku",
			0x01 => "lr a,kl",
			0x02 => "lr a,qu",
			0x03 => "lr a,ql",
			0x04 => "lr ku,a",
			0x05 => "lr kl,a",
			0x06 => "lr qu,a",
			0x07 => "lr ql,a",
			0x08 => "lr k,p",
			0x09 => "lr p,k",
			0x0a => "lr a,is",
			0x0b => "lr is,a",
			0x0d => "lr p0,q",
			0x0e => "lr q,dc",
			0x0f => "lr dc,q",
			0x10 => "lr dc,h",
			0x11 => "lr h,dc",
			0x1d => "lr w,j",
			0x1e => "lr j,w",
			_ => "lr"
		};
	}

	/// <summary>
	/// Get the full mnemonic with operand for shift instructions.
	/// </summary>
	public static string GetFullShiftMnemonic(byte opcode) {
		return opcode switch {
			0x12 => "sr 1",
			0x13 => "sl 1",
			0x14 => "sr 4",
			0x15 => "sl 4",
			_ => "sr"
		};
	}
}
