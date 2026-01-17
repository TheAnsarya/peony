namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// 6502 CPU instruction decoder
/// </summary>
public class Cpu6502Decoder : ICpuDecoder {
public string Architecture => "6502";

// Opcode table: (mnemonic, addressing mode, bytes)
private static readonly (string, AddressingMode, int)[] OpcodeTable = new (string, AddressingMode, int)[256];

static Cpu6502Decoder() {
InitializeOpcodeTable();
}

public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
if (data.Length == 0)
return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

var opcode = data[0];
var (mnemonic, mode, length) = OpcodeTable[opcode];

// Ensure we have enough bytes
if (data.Length < length)
return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

var bytes = data[..length].ToArray();
var operand = FormatOperand(data, mode, address);

return new DecodedInstruction(mnemonic, operand, bytes, mode);
}

public bool IsControlFlow(DecodedInstruction instruction) {
return instruction.Mnemonic is
"jmp" or "jsr" or "rts" or "rti" or "brk" or
"bcc" or "bcs" or "beq" or "bmi" or "bne" or "bpl" or "bvc" or "bvs";
}

public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
var nextAddress = address + (uint)instruction.Bytes.Length;

switch (instruction.Mode) {
case AddressingMode.Absolute when instruction.Mnemonic is "jmp" or "jsr":
yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
if (instruction.Mnemonic == "jsr")
yield return nextAddress; // JSR returns
break;

case AddressingMode.Relative:
var offset = (sbyte)instruction.Bytes[1];
yield return (uint)(nextAddress + offset);
yield return nextAddress; // Conditional branches can fall through
break;
}
}

private static string FormatOperand(ReadOnlySpan<byte> data, AddressingMode mode, uint address) {
return mode switch {
AddressingMode.Implied => "",
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
// Initialize all as illegal
for (int i = 0; i < 256; i++)
OpcodeTable[i] = ("???", AddressingMode.Implied, 1);

// Official 6502 opcodes
// ADC
OpcodeTable[0x69] = ("adc", AddressingMode.Immediate, 2);
OpcodeTable[0x65] = ("adc", AddressingMode.ZeroPage, 2);
OpcodeTable[0x75] = ("adc", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x6d] = ("adc", AddressingMode.Absolute, 3);
OpcodeTable[0x7d] = ("adc", AddressingMode.AbsoluteX, 3);
OpcodeTable[0x79] = ("adc", AddressingMode.AbsoluteY, 3);
OpcodeTable[0x61] = ("adc", AddressingMode.IndirectX, 2);
OpcodeTable[0x71] = ("adc", AddressingMode.IndirectY, 2);

// AND
OpcodeTable[0x29] = ("and", AddressingMode.Immediate, 2);
OpcodeTable[0x25] = ("and", AddressingMode.ZeroPage, 2);
OpcodeTable[0x35] = ("and", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x2d] = ("and", AddressingMode.Absolute, 3);
OpcodeTable[0x3d] = ("and", AddressingMode.AbsoluteX, 3);
OpcodeTable[0x39] = ("and", AddressingMode.AbsoluteY, 3);
OpcodeTable[0x21] = ("and", AddressingMode.IndirectX, 2);
OpcodeTable[0x31] = ("and", AddressingMode.IndirectY, 2);

// ASL
OpcodeTable[0x0a] = ("asl", AddressingMode.Implied, 1);
OpcodeTable[0x06] = ("asl", AddressingMode.ZeroPage, 2);
OpcodeTable[0x16] = ("asl", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x0e] = ("asl", AddressingMode.Absolute, 3);
OpcodeTable[0x1e] = ("asl", AddressingMode.AbsoluteX, 3);

// Branches
OpcodeTable[0x90] = ("bcc", AddressingMode.Relative, 2);
OpcodeTable[0xb0] = ("bcs", AddressingMode.Relative, 2);
OpcodeTable[0xf0] = ("beq", AddressingMode.Relative, 2);
OpcodeTable[0x30] = ("bmi", AddressingMode.Relative, 2);
OpcodeTable[0xd0] = ("bne", AddressingMode.Relative, 2);
OpcodeTable[0x10] = ("bpl", AddressingMode.Relative, 2);
OpcodeTable[0x50] = ("bvc", AddressingMode.Relative, 2);
OpcodeTable[0x70] = ("bvs", AddressingMode.Relative, 2);

// BIT
OpcodeTable[0x24] = ("bit", AddressingMode.ZeroPage, 2);
OpcodeTable[0x2c] = ("bit", AddressingMode.Absolute, 3);

// BRK
OpcodeTable[0x00] = ("brk", AddressingMode.Implied, 1);

// Clear flags
OpcodeTable[0x18] = ("clc", AddressingMode.Implied, 1);
OpcodeTable[0xd8] = ("cld", AddressingMode.Implied, 1);
OpcodeTable[0x58] = ("cli", AddressingMode.Implied, 1);
OpcodeTable[0xb8] = ("clv", AddressingMode.Implied, 1);

// CMP
OpcodeTable[0xc9] = ("cmp", AddressingMode.Immediate, 2);
OpcodeTable[0xc5] = ("cmp", AddressingMode.ZeroPage, 2);
OpcodeTable[0xd5] = ("cmp", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xcd] = ("cmp", AddressingMode.Absolute, 3);
OpcodeTable[0xdd] = ("cmp", AddressingMode.AbsoluteX, 3);
OpcodeTable[0xd9] = ("cmp", AddressingMode.AbsoluteY, 3);
OpcodeTable[0xc1] = ("cmp", AddressingMode.IndirectX, 2);
OpcodeTable[0xd1] = ("cmp", AddressingMode.IndirectY, 2);

// CPX
OpcodeTable[0xe0] = ("cpx", AddressingMode.Immediate, 2);
OpcodeTable[0xe4] = ("cpx", AddressingMode.ZeroPage, 2);
OpcodeTable[0xec] = ("cpx", AddressingMode.Absolute, 3);

// CPY
OpcodeTable[0xc0] = ("cpy", AddressingMode.Immediate, 2);
OpcodeTable[0xc4] = ("cpy", AddressingMode.ZeroPage, 2);
OpcodeTable[0xcc] = ("cpy", AddressingMode.Absolute, 3);

// DEC
OpcodeTable[0xc6] = ("dec", AddressingMode.ZeroPage, 2);
OpcodeTable[0xd6] = ("dec", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xce] = ("dec", AddressingMode.Absolute, 3);
OpcodeTable[0xde] = ("dec", AddressingMode.AbsoluteX, 3);

// DEX/DEY
OpcodeTable[0xca] = ("dex", AddressingMode.Implied, 1);
OpcodeTable[0x88] = ("dey", AddressingMode.Implied, 1);

// EOR
OpcodeTable[0x49] = ("eor", AddressingMode.Immediate, 2);
OpcodeTable[0x45] = ("eor", AddressingMode.ZeroPage, 2);
OpcodeTable[0x55] = ("eor", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x4d] = ("eor", AddressingMode.Absolute, 3);
OpcodeTable[0x5d] = ("eor", AddressingMode.AbsoluteX, 3);
OpcodeTable[0x59] = ("eor", AddressingMode.AbsoluteY, 3);
OpcodeTable[0x41] = ("eor", AddressingMode.IndirectX, 2);
OpcodeTable[0x51] = ("eor", AddressingMode.IndirectY, 2);

// INC
OpcodeTable[0xe6] = ("inc", AddressingMode.ZeroPage, 2);
OpcodeTable[0xf6] = ("inc", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xee] = ("inc", AddressingMode.Absolute, 3);
OpcodeTable[0xfe] = ("inc", AddressingMode.AbsoluteX, 3);

// INX/INY
OpcodeTable[0xe8] = ("inx", AddressingMode.Implied, 1);
OpcodeTable[0xc8] = ("iny", AddressingMode.Implied, 1);

// JMP
OpcodeTable[0x4c] = ("jmp", AddressingMode.Absolute, 3);
OpcodeTable[0x6c] = ("jmp", AddressingMode.Indirect, 3);

// JSR
OpcodeTable[0x20] = ("jsr", AddressingMode.Absolute, 3);

// LDA
OpcodeTable[0xa9] = ("lda", AddressingMode.Immediate, 2);
OpcodeTable[0xa5] = ("lda", AddressingMode.ZeroPage, 2);
OpcodeTable[0xb5] = ("lda", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xad] = ("lda", AddressingMode.Absolute, 3);
OpcodeTable[0xbd] = ("lda", AddressingMode.AbsoluteX, 3);
OpcodeTable[0xb9] = ("lda", AddressingMode.AbsoluteY, 3);
OpcodeTable[0xa1] = ("lda", AddressingMode.IndirectX, 2);
OpcodeTable[0xb1] = ("lda", AddressingMode.IndirectY, 2);

// LDX
OpcodeTable[0xa2] = ("ldx", AddressingMode.Immediate, 2);
OpcodeTable[0xa6] = ("ldx", AddressingMode.ZeroPage, 2);
OpcodeTable[0xb6] = ("ldx", AddressingMode.ZeroPageY, 2);
OpcodeTable[0xae] = ("ldx", AddressingMode.Absolute, 3);
OpcodeTable[0xbe] = ("ldx", AddressingMode.AbsoluteY, 3);

// LDY
OpcodeTable[0xa0] = ("ldy", AddressingMode.Immediate, 2);
OpcodeTable[0xa4] = ("ldy", AddressingMode.ZeroPage, 2);
OpcodeTable[0xb4] = ("ldy", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xac] = ("ldy", AddressingMode.Absolute, 3);
OpcodeTable[0xbc] = ("ldy", AddressingMode.AbsoluteX, 3);

// LSR
OpcodeTable[0x4a] = ("lsr", AddressingMode.Implied, 1);
OpcodeTable[0x46] = ("lsr", AddressingMode.ZeroPage, 2);
OpcodeTable[0x56] = ("lsr", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x4e] = ("lsr", AddressingMode.Absolute, 3);
OpcodeTable[0x5e] = ("lsr", AddressingMode.AbsoluteX, 3);

// NOP
OpcodeTable[0xea] = ("nop", AddressingMode.Implied, 1);

// ORA
OpcodeTable[0x09] = ("ora", AddressingMode.Immediate, 2);
OpcodeTable[0x05] = ("ora", AddressingMode.ZeroPage, 2);
OpcodeTable[0x15] = ("ora", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x0d] = ("ora", AddressingMode.Absolute, 3);
OpcodeTable[0x1d] = ("ora", AddressingMode.AbsoluteX, 3);
OpcodeTable[0x19] = ("ora", AddressingMode.AbsoluteY, 3);
OpcodeTable[0x01] = ("ora", AddressingMode.IndirectX, 2);
OpcodeTable[0x11] = ("ora", AddressingMode.IndirectY, 2);

// Push/Pull
OpcodeTable[0x48] = ("pha", AddressingMode.Implied, 1);
OpcodeTable[0x08] = ("php", AddressingMode.Implied, 1);
OpcodeTable[0x68] = ("pla", AddressingMode.Implied, 1);
OpcodeTable[0x28] = ("plp", AddressingMode.Implied, 1);

// ROL
OpcodeTable[0x2a] = ("rol", AddressingMode.Implied, 1);
OpcodeTable[0x26] = ("rol", AddressingMode.ZeroPage, 2);
OpcodeTable[0x36] = ("rol", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x2e] = ("rol", AddressingMode.Absolute, 3);
OpcodeTable[0x3e] = ("rol", AddressingMode.AbsoluteX, 3);

// ROR
OpcodeTable[0x6a] = ("ror", AddressingMode.Implied, 1);
OpcodeTable[0x66] = ("ror", AddressingMode.ZeroPage, 2);
OpcodeTable[0x76] = ("ror", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x6e] = ("ror", AddressingMode.Absolute, 3);
OpcodeTable[0x7e] = ("ror", AddressingMode.AbsoluteX, 3);

// RTI/RTS
OpcodeTable[0x40] = ("rti", AddressingMode.Implied, 1);
OpcodeTable[0x60] = ("rts", AddressingMode.Implied, 1);

// SBC
OpcodeTable[0xe9] = ("sbc", AddressingMode.Immediate, 2);
OpcodeTable[0xe5] = ("sbc", AddressingMode.ZeroPage, 2);
OpcodeTable[0xf5] = ("sbc", AddressingMode.ZeroPageX, 2);
OpcodeTable[0xed] = ("sbc", AddressingMode.Absolute, 3);
OpcodeTable[0xfd] = ("sbc", AddressingMode.AbsoluteX, 3);
OpcodeTable[0xf9] = ("sbc", AddressingMode.AbsoluteY, 3);
OpcodeTable[0xe1] = ("sbc", AddressingMode.IndirectX, 2);
OpcodeTable[0xf1] = ("sbc", AddressingMode.IndirectY, 2);

// Set flags
OpcodeTable[0x38] = ("sec", AddressingMode.Implied, 1);
OpcodeTable[0xf8] = ("sed", AddressingMode.Implied, 1);
OpcodeTable[0x78] = ("sei", AddressingMode.Implied, 1);

// STA
OpcodeTable[0x85] = ("sta", AddressingMode.ZeroPage, 2);
OpcodeTable[0x95] = ("sta", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x8d] = ("sta", AddressingMode.Absolute, 3);
OpcodeTable[0x9d] = ("sta", AddressingMode.AbsoluteX, 3);
OpcodeTable[0x99] = ("sta", AddressingMode.AbsoluteY, 3);
OpcodeTable[0x81] = ("sta", AddressingMode.IndirectX, 2);
OpcodeTable[0x91] = ("sta", AddressingMode.IndirectY, 2);

// STX
OpcodeTable[0x86] = ("stx", AddressingMode.ZeroPage, 2);
OpcodeTable[0x96] = ("stx", AddressingMode.ZeroPageY, 2);
OpcodeTable[0x8e] = ("stx", AddressingMode.Absolute, 3);

// STY
OpcodeTable[0x84] = ("sty", AddressingMode.ZeroPage, 2);
OpcodeTable[0x94] = ("sty", AddressingMode.ZeroPageX, 2);
OpcodeTable[0x8c] = ("sty", AddressingMode.Absolute, 3);

// Transfers
OpcodeTable[0xaa] = ("tax", AddressingMode.Implied, 1);
OpcodeTable[0xa8] = ("tay", AddressingMode.Implied, 1);
OpcodeTable[0xba] = ("tsx", AddressingMode.Implied, 1);
OpcodeTable[0x8a] = ("txa", AddressingMode.Implied, 1);
OpcodeTable[0x9a] = ("txs", AddressingMode.Implied, 1);
OpcodeTable[0x98] = ("tya", AddressingMode.Implied, 1);
}
}
