namespace Peony.Platform.Atari2600;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Atari 2600 platform analyzer with TIA/RIOT register detection
/// </summary>
public class Atari2600Analyzer : IPlatformAnalyzer {
public string Platform => "Atari 2600";
public ICpuDecoder CpuDecoder { get; } = new Cpu6502Decoder();

// TIA Write Registers ($00-$2C)
private static readonly Dictionary<uint, string> TiaWriteRegisters = new() {
[0x00] = "VSYNC",   [0x01] = "VBLANK",  [0x02] = "WSYNC",   [0x03] = "RSYNC",
[0x04] = "NUSIZ0",  [0x05] = "NUSIZ1",  [0x06] = "COLUP0",  [0x07] = "COLUP1",
[0x08] = "COLUPF",  [0x09] = "COLUBK",  [0x0a] = "CTRLPF",  [0x0b] = "REFP0",
[0x0c] = "REFP1",   [0x0d] = "PF0",     [0x0e] = "PF1",     [0x0f] = "PF2",
[0x10] = "RESP0",   [0x11] = "RESP1",   [0x12] = "RESM0",   [0x13] = "RESM1",
[0x14] = "RESBL",   [0x15] = "AUDC0",   [0x16] = "AUDC1",   [0x17] = "AUDF0",
[0x18] = "AUDF1",   [0x19] = "AUDV0",   [0x1a] = "AUDV1",   [0x1b] = "GRP0",
[0x1c] = "GRP1",    [0x1d] = "ENAM0",   [0x1e] = "ENAM1",   [0x1f] = "ENABL",
[0x20] = "HMP0",    [0x21] = "HMP1",    [0x22] = "HMM0",    [0x23] = "HMM1",
[0x24] = "HMBL",    [0x25] = "VDELP0",  [0x26] = "VDELP1",  [0x27] = "VDELBL",
[0x28] = "RESMP0",  [0x29] = "RESMP1",  [0x2a] = "HMOVE",   [0x2b] = "HMCLR",
[0x2c] = "CXCLR"
};

// TIA Read Registers ($00-$0D, mirrored)
private static readonly Dictionary<uint, string> TiaReadRegisters = new() {
[0x00] = "CXM0P",   [0x01] = "CXM1P",   [0x02] = "CXP0FB",  [0x03] = "CXP1FB",
[0x04] = "CXM0FB",  [0x05] = "CXM1FB",  [0x06] = "CXBLPF",  [0x07] = "CXPPMM",
[0x08] = "INPT0",   [0x09] = "INPT1",   [0x0a] = "INPT2",   [0x0b] = "INPT3",
[0x0c] = "INPT4",   [0x0d] = "INPT5"
};

// RIOT Registers ($280-$297)
private static readonly Dictionary<uint, string> RiotRegisters = new() {
[0x280] = "SWCHA",   [0x281] = "SWACNT",  [0x282] = "SWCHB",   [0x283] = "SWBCNT",
[0x284] = "INTIM",   [0x285] = "TIMINT",
[0x294] = "TIM1T",   [0x295] = "TIM8T",   [0x296] = "TIM64T",  [0x297] = "T1024T"
};

public RomInfo Analyze(ReadOnlySpan<byte> rom) {
var bankScheme = DetectBankSwitching(rom);

return new RomInfo(
Platform,
rom.Length,
bankScheme,
new Dictionary<string, string> {
["BankScheme"] = bankScheme ?? "None",
["Banks"] = GetBankCount(rom.Length, bankScheme).ToString()
}
);
}

public string? GetRegisterLabel(uint address) {
// Mask to 13-bit address space
var addr = address & 0x1fff;

// TIA ($00-$7F, mirrored)
if ((addr & 0x1080) == 0x0000) {
var tiaAddr = addr & 0x3f;
if (TiaWriteRegisters.TryGetValue(tiaAddr, out var label))
return label;
}

// RIOT ($280-$29F, mirrored)
if ((addr & 0x1280) == 0x0280) {
var riotAddr = 0x280u | (addr & 0x1f);
if (RiotRegisters.TryGetValue(riotAddr, out var label))
return label;
}

return null;
}

public MemoryRegion GetMemoryRegion(uint address) {
var addr = address & 0x1fff;

// TIA
if ((addr & 0x1080) == 0x0000)
return MemoryRegion.Hardware;

// RAM ($80-$FF, mirrored)
if ((addr & 0x1280) == 0x0080)
return MemoryRegion.Ram;

// RIOT
if ((addr & 0x1280) == 0x0280)
return MemoryRegion.Hardware;

// ROM ($1000-$1FFF)
if (addr >= 0x1000)
return MemoryRegion.Rom;

return MemoryRegion.Unknown;
}

/// <summary>
/// Get entry points for Atari 2600 ROMs
/// </summary>
public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
// Reset vector at $FFFC-$FFFD (mapped to end of ROM)
var resetOffset = rom.Length - 4;
if (resetOffset < 0) return [0xf000];

var resetVector = (uint)(rom[resetOffset] | (rom[resetOffset + 1] << 8));

// IRQ/BRK vector at $FFFE-$FFFF (rarely used on 2600)
var irqVector = (uint)(rom[resetOffset + 2] | (rom[resetOffset + 3] << 8));

return resetVector == irqVector
? [resetVector]
: [resetVector, irqVector];
}

private static string? DetectBankSwitching(ReadOnlySpan<byte> rom) {
return rom.Length switch {
2048 => null,        // 2K - no banking
4096 => null,        // 4K - no banking
8192 => "F8",        // 8K - F8 banking (most common)
16384 => "F6",       // 16K - F6 banking
32768 => "F4",       // 32K - F4 banking
_ when rom.Length > 4096 => DetectBankSchemeFromContent(rom),
_ => null
};
}

private static string? DetectBankSchemeFromContent(ReadOnlySpan<byte> rom) {
// Check for Tigervision (3F) - writes to $3F
// Check for Parker Bros (E0) - specific hotspot pattern
// Check for Activision (FE) - stack manipulation
// Default to F8 for 8K+
return "F8";
}

private static int GetBankCount(int romSize, string? scheme) {
return scheme switch {
"F8" => 2,
"F6" => 4,
"F4" => 8,
"3F" => romSize / 2048,
"E0" => 8,
_ => 1
};
}
}
