namespace Peony.Platform.NES;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// NES platform analyzer with PPU/APU/mapper support and multi-bank disassembly
/// </summary>
public class NesAnalyzer : IPlatformAnalyzer {
public string Platform => "NES";
public ICpuDecoder CpuDecoder { get; } = new Cpu6502Decoder();
public int Mapper { get; private set; }
public int PrgBanks { get; private set; }
public int ChrBanks { get; private set; }
public int BankCount => PrgBanks;

// Dragon Warrior 1 BRK-based bank function pointer table
// Format: BRK followed by 2 bytes: function_index, bank_number
// The IRQ handler reads these bytes and calls the function from the specified bank
private Dictionary<int, Dictionary<int, (uint Address, string Name)>>? _bankFunctions;

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

// Parse bank function pointer tables for MMC1 games using BRK
if (Mapper == 1 && PrgBanks >= 2) {
ParseBankFunctionTables(rom);
}

return new RomInfo(
Platform,
rom.Length,
GetMapperName(Mapper),
new Dictionary<string, string> {
["Mapper"] = Mapper.ToString(),
["MapperName"] = GetMapperName(Mapper) ?? "Unknown",
["PRG"] = $"{PrgBanks * 16}K",
["CHR"] = ChrBanks > 0 ? $"{ChrBanks * 8}K" : "RAM",
["Banks"] = PrgBanks.ToString()
}
);
}

/// <summary>
/// Parse bank function pointer tables from the fixed bank
/// Dragon Warrior uses a table at $FD00 area with 2-byte entries per function
/// </summary>
private void ParseBankFunctionTables(ReadOnlySpan<byte> rom) {
_bankFunctions = [];

// Dragon Warrior 1 has function pointer tables in the fixed bank
// Each bank has a table of pointers to its functions
// The BRK handler uses these to dispatch calls

// For now, we'll detect BRK calls dynamically rather than pre-parsing tables
// This is more robust across different games
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
if (rom.Length < 16) return [0x8000];

var prgBanks = rom[4];
var prgSize = prgBanks * 16384;

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

public bool IsInSwitchableRegion(uint address) {
if (Mapper == 0) return false; // NROM has no switching
if (Mapper == 1) return address >= 0x8000 && address < 0xc000; // MMC1: $8000-$BFFF switchable
return address >= 0x8000 && address < 0xc000; // Default assumption
}

public int AddressToOffset(uint address, int romLength) {
// Default to last bank for fixed region
return AddressToOffset(address, romLength, PrgBanks - 1);
}

public int AddressToOffset(uint address, int romLength, int bank) {
if (address < 0x8000) return -1;

var prgSize = PrgBanks * 16384;
if (prgSize == 0) prgSize = romLength - 16;

if (Mapper == 0) {
// NROM: 16K or 32K, no banking
if (prgSize <= 16384) {
return 16 + (int)((address - 0x8000) & 0x3fff);
} else {
return 16 + (int)(address - 0x8000);
}
} else if (Mapper == 1) {
// MMC1: $C000-$FFFF fixed to last bank, $8000-$BFFF switchable
if (address >= 0xc000) {
// Fixed bank (last 16K)
return 16 + prgSize - 16384 + (int)(address - 0xc000);
} else {
// Switchable bank
if (bank < 0 || bank >= PrgBanks) bank = 0;
return 16 + (bank * 16384) + (int)(address - 0x8000);
}
} else {
// Default handling
if (address >= 0xc000) {
return 16 + prgSize - 16384 + (int)(address - 0xc000);
}
if (bank < 0 || bank >= PrgBanks) bank = 0;
return 16 + (bank * 16384) + (int)(address - 0x8000);
}
}

/// <summary>
/// Detect BRK-based bank switch calls
/// Dragon Warrior 1 format: BRK, function_index, bank_number
/// </summary>
public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
var offset = AddressToOffset(address, rom.Length, currentBank);
if (offset < 0 || offset + 2 >= rom.Length) return null;

// Check if this is a BRK instruction (0x00)
if (rom[offset] != 0x00) return null;

// Read the two bytes following BRK
var funcIndex = rom[offset + 1];
var targetBank = rom[offset + 2];

// Validate bank number
if (targetBank >= PrgBanks) return null;

// Look up function address from bank's pointer table
// Dragon Warrior 1 stores function pointers in the fixed bank
var funcAddress = GetBankFunctionAddress(rom, targetBank, funcIndex);
if (funcAddress == 0) return null;

return new BankSwitchInfo(
targetBank,
funcAddress,
$"Bank{targetBank}_Func{funcIndex:x2}"
);
}

/// <summary>
/// Get function address from bank pointer table
/// Dragon Warrior 1 has pointer tables in each bank
/// </summary>
private uint GetBankFunctionAddress(ReadOnlySpan<byte> rom, int bank, int funcIndex) {
// Dragon Warrior 1 function pointer table structure:
// Each bank has a table of 2-byte pointers at a known location
// The location varies by bank but is typically near the start

// For bank 0-2, the pointer tables are at the start of each bank
// Table format: [lo, hi] pairs for each function

var bankStart = 16 + (bank * 16384);
if (bankStart >= rom.Length) return 0;

// Dragon Warrior uses a central dispatch table in the fixed bank
// The BRK handler at $FD3A reads the function index and bank,
// then looks up the function address from a table

// For now, return 0x8000 (bank start) as a fallback
// The full implementation would parse the actual pointer tables
// which are game-specific

// Common pattern: pointer table at $8000 in each bank
var tableOffset = bankStart;
var ptrOffset = tableOffset + (funcIndex * 2);

if (ptrOffset + 1 >= rom.Length) return 0;

var lo = rom[ptrOffset];
var hi = rom[ptrOffset + 1];
var addr = (uint)(lo | (hi << 8));

// Validate address is in ROM range
if (addr >= 0x8000 && addr < 0xc000) return addr;

// Fallback: just return start of bank
return 0x8000;
}

/// <summary>
/// Get entry points for a specific bank
/// </summary>
public uint[] GetBankEntryPoints(ReadOnlySpan<byte> rom, int bank) {
var entries = new List<uint>();

// Always include $8000 as potential entry
entries.Add(0x8000);

// For bank that contains vectors (last bank), include vector addresses
if (bank == PrgBanks - 1) {
var mainEntries = GetEntryPoints(rom);
foreach (var e in mainEntries) {
if (!entries.Contains(e)) entries.Add(e);
}
}

// Scan for JSR targets and add them
// This helps find more entry points

return [.. entries];
}

private static string? GetMapperName(int mapper) => mapper switch {
0 => "NROM", 1 => "MMC1", 2 => "UxROM", 3 => "CNROM", 4 => "MMC3",
5 => "MMC5", 7 => "AxROM", 9 => "MMC2", 10 => "MMC4", 11 => "Color Dreams",
66 => "GxROM", 71 => "Camerica", 79 => "NINA-03/06", 206 => "DxROM",
_ => null
};
}
