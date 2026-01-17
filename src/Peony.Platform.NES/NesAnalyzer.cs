namespace Peony.Platform.NES;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// NES platform analyzer with PPU/APU/mapper support
/// </summary>
public class NesAnalyzer : IPlatformAnalyzer {
public string Platform => "NES";
public ICpuDecoder CpuDecoder { get; } = new Cpu6502Decoder();
public int Mapper { get; private set; }
public int PrgBanks { get; private set; }
public int ChrBanks { get; private set; }

// PPU Registers ($2000-$2007)
private static readonly Dictionary<uint, string> PpuRegisters = new() {
[0x2000] = "PPUCTRL",   [0x2001] = "PPUMASK",   [0x2002] = "PPUSTATUS",
[0x2003] = "OAMADDR",   [0x2004] = "OAMDATA",   [0x2005] = "PPUSCROLL",
[0x2006] = "PPUADDR",   [0x2007] = "PPUDATA"
};

// APU Registers ($4000-$4017)
private static readonly Dictionary<uint, string> ApuRegisters = new() {
[0x4000] = "SQ1_VOL",   [0x4001] = "SQ1_SWEEP", [0x4002] = "SQ1_LO",    [0x4003] = "SQ1_HI",
[0x4004] = "SQ2_VOL",   [0x4005] = "SQ2_SWEEP", [0x4006] = "SQ2_LO",    [0x4007] = "SQ2_HI",
[0x4008] = "TRI_LINEAR",[0x400a] = "TRI_LO",    [0x400b] = "TRI_HI",
[0x400c] = "NOISE_VOL", [0x400e] = "NOISE_LO",  [0x400f] = "NOISE_HI",
[0x4010] = "DMC_FREQ",  [0x4011] = "DMC_RAW",   [0x4012] = "DMC_START", [0x4013] = "DMC_LEN",
[0x4014] = "OAMDMA",    [0x4015] = "SND_CHN",   [0x4016] = "JOY1",      [0x4017] = "JOY2"
};

public RomInfo Analyze(ReadOnlySpan<byte> rom) {
// Parse iNES header (NES<0x1a>)
if (rom.Length >= 16 && rom[0] == 0x4e && rom[1] == 0x45 && rom[2] == 0x53 && rom[3] == 0x1a) {
PrgBanks = rom[4];
ChrBanks = rom[5];
Mapper = (rom[6] >> 4) | (rom[7] & 0xf0);

// NES 2.0 extended mapper
if ((rom[7] & 0x0c) == 0x08)
Mapper |= (rom[8] & 0x0f) << 8;
}

return new RomInfo(
Platform,
rom.Length,
GetMapperName(Mapper),
new Dictionary<string, string> {
["Mapper"] = Mapper.ToString(),
["MapperName"] = GetMapperName(Mapper) ?? "Unknown",
["PRG"] = $"{PrgBanks * 16}K",
["CHR"] = ChrBanks > 0 ? $"{ChrBanks * 8}K" : "RAM"
}
);
}

public string? GetRegisterLabel(uint address) {
// PPU mirrors ($2000-$3FFF)
if (address >= 0x2000 && address < 0x4000) {
var ppuAddr = 0x2000 + (address & 0x07);
if (PpuRegisters.TryGetValue((uint)ppuAddr, out var label))
return label;
}

// APU/IO ($4000-$4017)
if (ApuRegisters.TryGetValue(address, out var apuLabel))
return apuLabel;

return null;
}

public MemoryRegion GetMemoryRegion(uint address) {
if (address < 0x0800) return MemoryRegion.Ram;
if (address < 0x2000) return MemoryRegion.Ram; // Mirrors
if (address < 0x4000) return MemoryRegion.Hardware; // PPU
if (address < 0x4020) return MemoryRegion.Hardware; // APU/IO
if (address < 0x6000) return MemoryRegion.Unknown; // Expansion
if (address < 0x8000) return MemoryRegion.Ram; // SRAM
return MemoryRegion.Rom; // PRG ROM
}

public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
// First parse header to get PRG size
if (rom.Length < 16) return [0x8000];

var prgBanks = rom[4];
var prgSize = prgBanks * 16384;

// Vectors are at end of PRG ROM (mapped to $FFFA-$FFFF)
// In iNES file: offset = 16 (header) + prgSize - 6 (vectors)
var vectorBase = 16 + prgSize - 6;
if (vectorBase < 16 || vectorBase + 5 >= rom.Length)
return [0x8000];

var nmi = (uint)(rom[vectorBase] | (rom[vectorBase + 1] << 8));
var reset = (uint)(rom[vectorBase + 2] | (rom[vectorBase + 3] << 8));
var irq = (uint)(rom[vectorBase + 4] | (rom[vectorBase + 5] << 8));

var entries = new HashSet<uint>();
if (reset >= 0x8000 && reset <= 0xffff) entries.Add(reset);
if (nmi >= 0x8000 && nmi <= 0xffff && nmi != reset) entries.Add(nmi);
if (irq >= 0x8000 && irq <= 0xffff && irq != reset && irq != nmi) entries.Add(irq);

return entries.Count > 0 ? [.. entries] : [0x8000];
}

public int AddressToOffset(uint address, int romLength) {
if (address < 0x8000) return -1;

// Get header info
var prgSize = PrgBanks * 16384;
if (prgSize == 0) {
// Fallback: assume header is 16 bytes and rest is PRG (no CHR)
prgSize = romLength - 16;
}

// For mappers like MMC1, the last bank is typically fixed at $C000-$FFFF
// This is a simplification - full emulation would track bank switching

if (Mapper == 0) {
// NROM: 16K or 32K, no banking
if (prgSize <= 16384) {
// 16K PRG mirrored at $8000 and $C000
return 16 + (int)((address - 0x8000) & 0x3fff);
} else {
// 32K PRG at $8000-$FFFF
return 16 + (int)(address - 0x8000);
}
} else if (Mapper == 1) {
// MMC1: Last 16K bank is fixed at $C000-$FFFF (in most configurations)
// For disassembly, map $C000-$FFFF to last bank, $8000-$BFFF to first bank
if (address >= 0xc000) {
// Map to last 16K of PRG
return 16 + prgSize - 16384 + (int)(address - 0xc000);
} else {
// Map to first 16K of PRG (bank 0)
return 16 + (int)(address - 0x8000);
}
} else {
// Default: map $8000-$FFFF to last 32K
var offset = (int)(address - 0x8000);
if (offset < prgSize) {
// Map to end of PRG for vectors to work
if (address >= 0xc000) {
return 16 + prgSize - 16384 + (int)(address - 0xc000);
}
return 16 + offset;
}
return -1;
}
}

private static string? GetMapperName(int mapper) => mapper switch {
0 => "NROM", 1 => "MMC1", 2 => "UxROM", 3 => "CNROM", 4 => "MMC3",
5 => "MMC5", 7 => "AxROM", 9 => "MMC2", 10 => "MMC4", 11 => "Color Dreams",
66 => "GxROM", 71 => "Camerica", 79 => "NINA-03/06", 206 => "DxROM",
_ => null
};
}
