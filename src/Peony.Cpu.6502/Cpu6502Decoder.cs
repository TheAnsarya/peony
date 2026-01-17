namespace Peony.Cpu;

using Peony.Core;

/// <summary>
/// 6502 CPU instruction decoder with illegal opcode support
/// </summary>
public class Cpu6502Decoder : ICpuDecoder {
public string Architecture => "6502";
public bool IncludeIllegalOpcodes { get; set; } = true;

// Opcode table: (mnemonic, addressing mode, bytes, isIllegal)
private static readonly (string Mnemonic, AddressingMode Mode, int Bytes, bool Illegal)[] OpcodeTable = new (string, AddressingMode, int, bool)[256];

static Cpu6502Decoder() {
InitializeOpcodeTable();
}

public DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address) {
if (data.Length == 0)
return new DecodedInstruction("???", "", [0xff], AddressingMode.Implied);

var opcode = data[0];
var (mnemonic, mode, length, illegal) = OpcodeTable[opcode];

if (data.Length < length)
return new DecodedInstruction("???", "", [opcode], AddressingMode.Implied);

var bytes = data[..length].ToArray();
var operand = FormatOperand(data, mode, address);

// Mark illegal opcodes with asterisk
if (illegal && IncludeIllegalOpcodes)
mnemonic = "*" + mnemonic;

return new DecodedInstruction(mnemonic, operand, bytes, mode);
}

public bool IsControlFlow(DecodedInstruction instruction) {
var mnem = instruction.Mnemonic.TrimStart('*');
return mnem is "jmp" or "jsr" or "rts" or "rti" or "brk" or
"bcc" or "bcs" or "beq" or "bmi" or "bne" or "bpl" or "bvc" or "bvs" or
"jam" or "kil" or "hlt"; // Illegal halts
}

public IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address) {
var nextAddress = address + (uint)instruction.Bytes.Length;

switch (instruction.Mode) {
case AddressingMode.Absolute when instruction.Mnemonic.TrimStart('*') is "jmp" or "jsr":
yield return (uint)(instruction.Bytes[1] | (instruction.Bytes[2] << 8));
if (instruction.Mnemonic.TrimStart('*') == "jsr")
yield return nextAddress;
break;

case AddressingMode.Relative:
var offset = (sbyte)instruction.Bytes[1];
yield return (uint)(nextAddress + offset);
yield return nextAddress;
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
// Initialize all as illegal NOPs or JAM
for (int i = 0; i < 256; i++)
OpcodeTable[i] = ("nop", AddressingMode.Implied, 1, true);

// === OFFICIAL OPCODES ===

// ADC
OpcodeTable[0x69] = ("adc", AddressingMode.Immediate, 2, false);
OpcodeTable[0x65] = ("adc", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x75] = ("adc", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x6d] = ("adc", AddressingMode.Absolute, 3, false);
OpcodeTable[0x7d] = ("adc", AddressingMode.AbsoluteX, 3, false);
OpcodeTable[0x79] = ("adc", AddressingMode.AbsoluteY, 3, false);
OpcodeTable[0x61] = ("adc", AddressingMode.IndirectX, 2, false);
OpcodeTable[0x71] = ("adc", AddressingMode.IndirectY, 2, false);

// AND
OpcodeTable[0x29] = ("and", AddressingMode.Immediate, 2, false);
OpcodeTable[0x25] = ("and", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x35] = ("and", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x2d] = ("and", AddressingMode.Absolute, 3, false);
OpcodeTable[0x3d] = ("and", AddressingMode.AbsoluteX, 3, false);
OpcodeTable[0x39] = ("and", AddressingMode.AbsoluteY, 3, false);
OpcodeTable[0x21] = ("and", AddressingMode.IndirectX, 2, false);
OpcodeTable[0x31] = ("and", AddressingMode.IndirectY, 2, false);

// ASL
OpcodeTable[0x0a] = ("asl", AddressingMode.Implied, 1, false);
OpcodeTable[0x06] = ("asl", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x16] = ("asl", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x0e] = ("asl", AddressingMode.Absolute, 3, false);
OpcodeTable[0x1e] = ("asl", AddressingMode.AbsoluteX, 3, false);

// Branches
OpcodeTable[0x90] = ("bcc", AddressingMode.Relative, 2, false);
OpcodeTable[0xb0] = ("bcs", AddressingMode.Relative, 2, false);
OpcodeTable[0xf0] = ("beq", AddressingMode.Relative, 2, false);
OpcodeTable[0x30] = ("bmi", AddressingMode.Relative, 2, false);
OpcodeTable[0xd0] = ("bne", AddressingMode.Relative, 2, false);
OpcodeTable[0x10] = ("bpl", AddressingMode.Relative, 2, false);
OpcodeTable[0x50] = ("bvc", AddressingMode.Relative, 2, false);
OpcodeTable[0x70] = ("bvs", AddressingMode.Relative, 2, false);

// BIT
OpcodeTable[0x24] = ("bit", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x2c] = ("bit", AddressingMode.Absolute, 3, false);

// BRK
OpcodeTable[0x00] = ("brk", AddressingMode.Implied, 1, false);

// Clear flags
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

// DEX/DEY
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

// INC
OpcodeTable[0xe6] = ("inc", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0xf6] = ("inc", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0xee] = ("inc", AddressingMode.Absolute, 3, false);
OpcodeTable[0xfe] = ("inc", AddressingMode.AbsoluteX, 3, false);

// INX/INY
OpcodeTable[0xe8] = ("inx", AddressingMode.Implied, 1, false);
OpcodeTable[0xc8] = ("iny", AddressingMode.Implied, 1, false);

// JMP
OpcodeTable[0x4c] = ("jmp", AddressingMode.Absolute, 3, false);
OpcodeTable[0x6c] = ("jmp", AddressingMode.Indirect, 3, false);

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
OpcodeTable[0x4a] = ("lsr", AddressingMode.Implied, 1, false);
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

// Push/Pull
OpcodeTable[0x48] = ("pha", AddressingMode.Implied, 1, false);
OpcodeTable[0x08] = ("php", AddressingMode.Implied, 1, false);
OpcodeTable[0x68] = ("pla", AddressingMode.Implied, 1, false);
OpcodeTable[0x28] = ("plp", AddressingMode.Implied, 1, false);

// ROL
OpcodeTable[0x2a] = ("rol", AddressingMode.Implied, 1, false);
OpcodeTable[0x26] = ("rol", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x36] = ("rol", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x2e] = ("rol", AddressingMode.Absolute, 3, false);
OpcodeTable[0x3e] = ("rol", AddressingMode.AbsoluteX, 3, false);

// ROR
OpcodeTable[0x6a] = ("ror", AddressingMode.Implied, 1, false);
OpcodeTable[0x66] = ("ror", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x76] = ("ror", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x6e] = ("ror", AddressingMode.Absolute, 3, false);
OpcodeTable[0x7e] = ("ror", AddressingMode.AbsoluteX, 3, false);

// RTI/RTS
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

// Set flags
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

// STX
OpcodeTable[0x86] = ("stx", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x96] = ("stx", AddressingMode.ZeroPageY, 2, false);
OpcodeTable[0x8e] = ("stx", AddressingMode.Absolute, 3, false);

// STY
OpcodeTable[0x84] = ("sty", AddressingMode.ZeroPage, 2, false);
OpcodeTable[0x94] = ("sty", AddressingMode.ZeroPageX, 2, false);
OpcodeTable[0x8c] = ("sty", AddressingMode.Absolute, 3, false);

// Transfers
OpcodeTable[0xaa] = ("tax", AddressingMode.Implied, 1, false);
OpcodeTable[0xa8] = ("tay", AddressingMode.Implied, 1, false);
OpcodeTable[0xba] = ("tsx", AddressingMode.Implied, 1, false);
OpcodeTable[0x8a] = ("txa", AddressingMode.Implied, 1, false);
OpcodeTable[0x9a] = ("txs", AddressingMode.Implied, 1, false);
OpcodeTable[0x98] = ("tya", AddressingMode.Implied, 1, false);

// === ILLEGAL OPCODES ===

// JAM/KIL/HLT - Halt CPU (various opcodes)
OpcodeTable[0x02] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x12] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x22] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x32] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x42] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x52] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x62] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x72] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0x92] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0xb2] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0xd2] = ("jam", AddressingMode.Implied, 1, true);
OpcodeTable[0xf2] = ("jam", AddressingMode.Implied, 1, true);

// SLO (ASL + ORA)
OpcodeTable[0x07] = ("slo", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x17] = ("slo", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x0f] = ("slo", AddressingMode.Absolute, 3, true);
OpcodeTable[0x1f] = ("slo", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x1b] = ("slo", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x03] = ("slo", AddressingMode.IndirectX, 2, true);
OpcodeTable[0x13] = ("slo", AddressingMode.IndirectY, 2, true);

// RLA (ROL + AND)
OpcodeTable[0x27] = ("rla", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x37] = ("rla", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x2f] = ("rla", AddressingMode.Absolute, 3, true);
OpcodeTable[0x3f] = ("rla", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x3b] = ("rla", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x23] = ("rla", AddressingMode.IndirectX, 2, true);
OpcodeTable[0x33] = ("rla", AddressingMode.IndirectY, 2, true);

// SRE (LSR + EOR)
OpcodeTable[0x47] = ("sre", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x57] = ("sre", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x4f] = ("sre", AddressingMode.Absolute, 3, true);
OpcodeTable[0x5f] = ("sre", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x5b] = ("sre", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x43] = ("sre", AddressingMode.IndirectX, 2, true);
OpcodeTable[0x53] = ("sre", AddressingMode.IndirectY, 2, true);

// RRA (ROR + ADC)
OpcodeTable[0x67] = ("rra", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x77] = ("rra", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x6f] = ("rra", AddressingMode.Absolute, 3, true);
OpcodeTable[0x7f] = ("rra", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x7b] = ("rra", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x63] = ("rra", AddressingMode.IndirectX, 2, true);
OpcodeTable[0x73] = ("rra", AddressingMode.IndirectY, 2, true);

// SAX (Store A AND X)
OpcodeTable[0x87] = ("sax", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x97] = ("sax", AddressingMode.ZeroPageY, 2, true);
OpcodeTable[0x8f] = ("sax", AddressingMode.Absolute, 3, true);
OpcodeTable[0x83] = ("sax", AddressingMode.IndirectX, 2, true);

// LAX (LDA + LDX)
OpcodeTable[0xa7] = ("lax", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0xb7] = ("lax", AddressingMode.ZeroPageY, 2, true);
OpcodeTable[0xaf] = ("lax", AddressingMode.Absolute, 3, true);
OpcodeTable[0xbf] = ("lax", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0xa3] = ("lax", AddressingMode.IndirectX, 2, true);
OpcodeTable[0xb3] = ("lax", AddressingMode.IndirectY, 2, true);

// DCP (DEC + CMP)
OpcodeTable[0xc7] = ("dcp", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0xd7] = ("dcp", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0xcf] = ("dcp", AddressingMode.Absolute, 3, true);
OpcodeTable[0xdf] = ("dcp", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0xdb] = ("dcp", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0xc3] = ("dcp", AddressingMode.IndirectX, 2, true);
OpcodeTable[0xd3] = ("dcp", AddressingMode.IndirectY, 2, true);

// ISC/ISB (INC + SBC)
OpcodeTable[0xe7] = ("isc", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0xf7] = ("isc", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0xef] = ("isc", AddressingMode.Absolute, 3, true);
OpcodeTable[0xff] = ("isc", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0xfb] = ("isc", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0xe3] = ("isc", AddressingMode.IndirectX, 2, true);
OpcodeTable[0xf3] = ("isc", AddressingMode.IndirectY, 2, true);

// ANC (AND + set C from bit 7)
OpcodeTable[0x0b] = ("anc", AddressingMode.Immediate, 2, true);
OpcodeTable[0x2b] = ("anc", AddressingMode.Immediate, 2, true);

// ALR/ASR (AND + LSR)
OpcodeTable[0x4b] = ("alr", AddressingMode.Immediate, 2, true);

// ARR (AND + ROR, weird flags)
OpcodeTable[0x6b] = ("arr", AddressingMode.Immediate, 2, true);

// XAA/ANE (A = (A | magic) & X & imm)
OpcodeTable[0x8b] = ("xaa", AddressingMode.Immediate, 2, true);

// LAX immediate (unstable)
OpcodeTable[0xab] = ("lax", AddressingMode.Immediate, 2, true);

// AXS/SBX (X = (A & X) - imm)
OpcodeTable[0xcb] = ("axs", AddressingMode.Immediate, 2, true);

// SBC (duplicate)
OpcodeTable[0xeb] = ("sbc", AddressingMode.Immediate, 2, true);

// Illegal NOPs with different sizes
OpcodeTable[0x1a] = ("nop", AddressingMode.Implied, 1, true);
OpcodeTable[0x3a] = ("nop", AddressingMode.Implied, 1, true);
OpcodeTable[0x5a] = ("nop", AddressingMode.Implied, 1, true);
OpcodeTable[0x7a] = ("nop", AddressingMode.Implied, 1, true);
OpcodeTable[0xda] = ("nop", AddressingMode.Implied, 1, true);
OpcodeTable[0xfa] = ("nop", AddressingMode.Implied, 1, true);

// 2-byte NOPs (skip 1 byte)
OpcodeTable[0x80] = ("nop", AddressingMode.Immediate, 2, true);
OpcodeTable[0x82] = ("nop", AddressingMode.Immediate, 2, true);
OpcodeTable[0x89] = ("nop", AddressingMode.Immediate, 2, true);
OpcodeTable[0xc2] = ("nop", AddressingMode.Immediate, 2, true);
OpcodeTable[0xe2] = ("nop", AddressingMode.Immediate, 2, true);
OpcodeTable[0x04] = ("nop", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x44] = ("nop", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x64] = ("nop", AddressingMode.ZeroPage, 2, true);
OpcodeTable[0x14] = ("nop", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x34] = ("nop", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x54] = ("nop", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0x74] = ("nop", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0xd4] = ("nop", AddressingMode.ZeroPageX, 2, true);
OpcodeTable[0xf4] = ("nop", AddressingMode.ZeroPageX, 2, true);

// 3-byte NOPs
OpcodeTable[0x0c] = ("nop", AddressingMode.Absolute, 3, true);
OpcodeTable[0x1c] = ("nop", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x3c] = ("nop", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x5c] = ("nop", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x7c] = ("nop", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0xdc] = ("nop", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0xfc] = ("nop", AddressingMode.AbsoluteX, 3, true);

// Highly unstable opcodes
OpcodeTable[0x9b] = ("tas", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x9c] = ("shy", AddressingMode.AbsoluteX, 3, true);
OpcodeTable[0x9e] = ("shx", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x9f] = ("sha", AddressingMode.AbsoluteY, 3, true);
OpcodeTable[0x93] = ("sha", AddressingMode.IndirectY, 2, true);
OpcodeTable[0xbb] = ("las", AddressingMode.AbsoluteY, 3, true);
}
}
