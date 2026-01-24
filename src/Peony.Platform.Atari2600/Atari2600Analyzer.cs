namespace Peony.Platform.Atari2600;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Atari 2600 platform analyzer with TIA/RIOT register detection
/// and comprehensive bank switching scheme support
/// </summary>
public class Atari2600Analyzer : IPlatformAnalyzer {
public string Platform => "Atari 2600";
public ICpuDecoder CpuDecoder { get; } = new Cpu6502Decoder();
public string? DetectedScheme { get; private set; }
public int BankCount => GetBankCount(_romLength, DetectedScheme);

private int _romLength;

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

// Bank switching hotspot addresses
private static readonly Dictionary<string, uint[]> BankHotspots = new() {
["F8"] = new uint[] { 0xfff8, 0xfff9 },
["F6"] = new uint[] { 0xfff6, 0xfff7, 0xfff8, 0xfff9 },
["F4"] = new uint[] { 0xfff4, 0xfff5, 0xfff6, 0xfff7, 0xfff8, 0xfff9, 0xfffa, 0xfffb },
["3F"] = new uint[] { 0x003f },
["E0"] = new uint[] { 0xfe0, 0xfe1, 0xfe2, 0xfe3, 0xfe4, 0xfe5, 0xfe6, 0xfe7,
                      0xff0, 0xff1, 0xff2, 0xff3, 0xff4, 0xff5, 0xff6, 0xff7 },
["FE"] = new uint[] { 0x01fe, 0x01ff },
["E7"] = new uint[] { 0xfe0, 0xfe1, 0xfe2, 0xfe3, 0xfe4, 0xfe5, 0xfe6, 0xfe7 },
["F0"] = new uint[] { 0xfff0 },
["UA"] = new uint[] { 0x0220, 0x0240 },
["CV"] = new uint[] { 0x03ff },
};

public RomInfo Analyze(ReadOnlySpan<byte> rom) {
_romLength = rom.Length;
DetectedScheme = DetectBankSwitching(rom);

return new RomInfo(
Platform,
rom.Length,
DetectedScheme,
new Dictionary<string, string> {
["BankScheme"] = DetectedScheme ?? "None",
["Banks"] = GetBankCount(rom.Length, DetectedScheme).ToString(),
["BankSize"] = GetBankSize(DetectedScheme).ToString()
}
);
}

public string? GetRegisterLabel(uint address) {
var addr = address & 0x1fff;

if ((addr & 0x1080) == 0x0000) {
var tiaAddr = addr & 0x3f;
if (TiaWriteRegisters.TryGetValue(tiaAddr, out var label))
return label;
}

if ((addr & 0x1280) == 0x0280) {
var riotAddr = 0x280u | (addr & 0x1f);
if (RiotRegisters.TryGetValue(riotAddr, out var label))
return label;
}

if (DetectedScheme != null && BankHotspots.TryGetValue(DetectedScheme, out var hotspots)) {
var idx = Array.IndexOf(hotspots, address & 0xffff);
if (idx >= 0)
return $"BANK{idx}";
}

return null;
}

public MemoryRegion GetMemoryRegion(uint address) {
var addr = address & 0x1fff;

if ((addr & 0x1080) == 0x0000) return MemoryRegion.Hardware;
if ((addr & 0x1280) == 0x0080) return MemoryRegion.Ram;
if ((addr & 0x1280) == 0x0280) return MemoryRegion.Hardware;
if (addr >= 0x1000) return MemoryRegion.Rom;

return MemoryRegion.Unknown;
}

public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
var resetOffset = rom.Length - 4;
if (resetOffset < 0) return [0xf000];

var resetVector = (uint)(rom[resetOffset] | (rom[resetOffset + 1] << 8));
var irqVector = (uint)(rom[resetOffset + 2] | (rom[resetOffset + 3] << 8));

return resetVector == irqVector ? [resetVector] : [resetVector, irqVector];
}

public bool IsInSwitchableRegion(uint address) {
// For most Atari 2600 bank schemes, $F000-$FFFF is the ROM window
// With banking, some or all of this can be switched
return DetectedScheme != null && address >= 0xf000;
}

public int AddressToOffset(uint address, int romLength) {
return AddressToOffset(address, romLength, -1);
}

public int AddressToOffset(uint address, int romLength, int bank) {
var scheme = DetectedScheme;
var bankSize = GetBankSize(scheme);

if (bank < 0) bank = GetBankCount(romLength, scheme) - 1;

return scheme switch {
null when romLength == 2048 => (address >= 0xf800) ? (int)(address - 0xf800) : -1,
null when romLength == 4096 => (address >= 0xf000) ? (int)(address - 0xf000) : -1,
"F8" or "F6" or "F4" => (address >= 0xf000) ? (bank * bankSize) + (int)(address - 0xf000) : -1,
"3F" => (address >= 0xf800) ? (bank * 2048) + (int)(address - 0xf800) :
        (address >= 0xf000) ? (romLength - 2048) + (int)(address - 0xf000) : -1,
"E0" => GetE0Offset(address, romLength, bank),
"FE" => (address >= 0xd000) ? (bank * 8192) + (int)(address - 0xd000) : -1,
_ => (address >= 0xf000) ? (int)(address - 0xf000) : -1
};
}

public uint? OffsetToAddress(int offset) {
	// Atari 2600: ROM maps to $F000-$FFFF (4K) or $F800-$FFFF (2K)
	// Just return $F000 + offset for simple mapping
	return (uint)(0xf000 + offset);
}

public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
// Atari 2600 doesn't use BRK for bank switching
// Bank switches happen via hotspot accesses
return null;
}

private static int GetE0Offset(uint address, int romLength, int bank) {
if (address < 0xf000) return -1;
var slot = (int)((address - 0xf000) / 0x400);
var offset = (int)(address & 0x3ff);
return (slot == 3) ? (7 * 1024) + offset : (bank * 1024) + offset;
}

private static string? DetectBankSwitching(ReadOnlySpan<byte> rom) {
var size = rom.Length;

if (size <= 2048) return null;
if (size <= 4096) return null;

if (Has3FSignature(rom)) return "3F";
if (HasE0Signature(rom)) return "E0";
if (HasFESignature(rom)) return "FE";

return size switch {
8192 => "F8",
16384 => "F6",
32768 => "F4",
65536 => "F0",
_ => "F8"
};
}

private static bool Has3FSignature(ReadOnlySpan<byte> rom) {
for (int i = 0; i < rom.Length - 2; i++) {
if ((rom[i] == 0x85 || rom[i] == 0x86) && rom[i + 1] == 0x3f)
return true;
}
return false;
}

private static bool HasE0Signature(ReadOnlySpan<byte> rom) {
for (int i = 0; i < rom.Length - 2; i++) {
if (rom[i] == 0x8d) {
var addr = rom[i + 1] | (rom[i + 2] << 8);
if ((addr >= 0x1fe0 && addr <= 0x1fe7) || (addr >= 0x1ff0 && addr <= 0x1ff7))
return true;
}
}
return false;
}

private static bool HasFESignature(ReadOnlySpan<byte> rom) {
for (int i = 0; i < rom.Length - 2; i++) {
if (rom[i] == 0x20) {
var addr = rom[i + 1] | (rom[i + 2] << 8);
if (addr == 0x01fe || addr == 0x01ff)
return true;
}
}
return false;
}

private static int GetBankCount(int romSize, string? scheme) => scheme switch {
"F8" => 2, "F6" => 4, "F4" => 8,
"3F" => romSize / 2048,
"E0" => 8, "E7" => 8,
"FE" => 2, "F0" => 16,
_ => 1
};

private static int GetBankSize(string? scheme) => scheme switch {
"F8" or "F6" or "F4" => 4096,
"3F" => 2048,
"E0" or "E7" => 1024,
"FE" => 8192,
"F0" => 4096,
_ => 4096
};
}
