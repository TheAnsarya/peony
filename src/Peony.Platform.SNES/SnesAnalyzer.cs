namespace Peony.Platform.SNES;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// SNES platform analyzer with LoRom/HiRom address mapping support
/// </summary>
public class SnesAnalyzer : IPlatformAnalyzer {
	public string Platform => "SNES";
	public ICpuDecoder CpuDecoder { get; }
	public int BankCount => _romBanks;

	/// <summary>
	/// SNES ROM mapping mode
	/// </summary>
	public SnesMapMode MapMode { get; private set; } = SnesMapMode.LoRom;

	/// <summary>
	/// Whether ROM has a 512-byte copier header
	/// </summary>
	public bool HasCopierHeader { get; private set; } = false;

	private int _romBanks;
	private int _romSize;

	// PPU Registers ($2100-$213F)
	private static readonly Dictionary<uint, string> PpuRegisters = new() {
		[0x2100] = "INIDISP",    [0x2101] = "OBSEL",      [0x2102] = "OAMADDL",   [0x2103] = "OAMADDH",
		[0x2104] = "OAMDATA",    [0x2105] = "BGMODE",     [0x2106] = "MOSAIC",    [0x2107] = "BG1SC",
		[0x2108] = "BG2SC",      [0x2109] = "BG3SC",      [0x210a] = "BG4SC",     [0x210b] = "BG12NBA",
		[0x210c] = "BG34NBA",    [0x210d] = "BG1HOFS",    [0x210e] = "BG1VOFS",   [0x210f] = "BG2HOFS",
		[0x2110] = "BG2VOFS",    [0x2111] = "BG3HOFS",    [0x2112] = "BG3VOFS",   [0x2113] = "BG4HOFS",
		[0x2114] = "BG4VOFS",    [0x2115] = "VMAIN",      [0x2116] = "VMADDL",    [0x2117] = "VMADDH",
		[0x2118] = "VMDATAL",    [0x2119] = "VMDATAH",    [0x211a] = "M7SEL",
		[0x211b] = "M7A",        [0x211c] = "M7B",        [0x211d] = "M7C",       [0x211e] = "M7D",
		[0x211f] = "M7X",        [0x2120] = "M7Y",        [0x2121] = "CGADD",     [0x2122] = "CGDATA",
		[0x2123] = "W12SEL",     [0x2124] = "W34SEL",     [0x2125] = "WOBJSEL",   [0x2126] = "WH0",
		[0x2127] = "WH1",        [0x2128] = "WH2",        [0x2129] = "WH3",       [0x212a] = "WBGLOG",
		[0x212b] = "WOBJLOG",    [0x212c] = "TM",         [0x212d] = "TS",        [0x212e] = "TMW",
		[0x212f] = "TSW",        [0x2130] = "CGWSEL",     [0x2131] = "CGADSUB",   [0x2132] = "COLDATA",
		[0x2133] = "SETINI",     [0x2134] = "MPYL",       [0x2135] = "MPYM",      [0x2136] = "MPYH",
		[0x2137] = "SLHV",       [0x2138] = "RDOAM",      [0x2139] = "RDVRAML",   [0x213a] = "RDVRAMH",
		[0x213b] = "RDCGRAM",    [0x213c] = "OPHCT",      [0x213d] = "OPVCT",     [0x213e] = "STAT77",
		[0x213f] = "STAT78"
	};

	// APU I/O Registers ($2140-$2143)
	private static readonly Dictionary<uint, string> ApuRegisters = new() {
		[0x2140] = "APUIO0",     [0x2141] = "APUIO1",     [0x2142] = "APUIO2",    [0x2143] = "APUIO3"
	};

	// WRAM Registers ($2180-$2183)
	private static readonly Dictionary<uint, string> WramRegisters = new() {
		[0x2180] = "WMDATA",     [0x2181] = "WMADDL",     [0x2182] = "WMADDM",    [0x2183] = "WMADDH"
	};

	// CPU Registers ($4200-$421F)
	private static readonly Dictionary<uint, string> CpuRegisters = new() {
		[0x4200] = "NMITIMEN",   [0x4201] = "WRIO",       [0x4202] = "WRMPYA",    [0x4203] = "WRMPYB",
		[0x4204] = "WRDIVL",     [0x4205] = "WRDIVH",     [0x4206] = "WRDIVB",    [0x4207] = "HTIMEL",
		[0x4208] = "HTIMEH",     [0x4209] = "VTIMEL",     [0x420a] = "VTIMEH",    [0x420b] = "MDMAEN",
		[0x420c] = "HDMAEN",     [0x420d] = "MEMSEL",     [0x4210] = "RDNMI",     [0x4211] = "TIMEUP",
		[0x4212] = "HVBJOY",     [0x4213] = "RDIO",       [0x4214] = "RDDIVL",    [0x4215] = "RDDIVH",
		[0x4216] = "RDMPYL",     [0x4217] = "RDMPYH",     [0x4218] = "JOY1L",     [0x4219] = "JOY1H",
		[0x421a] = "JOY2L",      [0x421b] = "JOY2H",      [0x421c] = "JOY3L",     [0x421d] = "JOY3H",
		[0x421e] = "JOY4L",      [0x421f] = "JOY4H"
	};

	public SnesAnalyzer() {
		CpuDecoder = new Cpu65816Decoder();
	}

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		// Detect copier header (512 bytes)
		HasCopierHeader = (rom.Length % 0x8000) == 512;
		var offset = HasCopierHeader ? 512 : 0;
		var data = rom[offset..];

		_romSize = data.Length;

		// Detect map mode from ROM header
		DetectMapMode(data);

		// Calculate bank count
		_romBanks = MapMode switch {
			SnesMapMode.LoRom or SnesMapMode.ExLoRom => _romSize / 0x8000,
			SnesMapMode.HiRom or SnesMapMode.ExHiRom => _romSize / 0x10000,
			_ => _romSize / 0x8000
		};

		// Read ROM name from header
		var headerOffset = MapMode == SnesMapMode.LoRom ? 0x7fc0 : 0xffc0;
		var romName = "";
		if (data.Length >= headerOffset + 21) {
			var nameBytes = data.Slice(headerOffset, 21).ToArray();
			romName = System.Text.Encoding.ASCII.GetString(nameBytes).TrimEnd();
		}

		return new RomInfo(
			Platform,
			rom.Length,
			romName,
			new Dictionary<string, string> {
				["MapMode"] = MapMode.ToString(),
				["RomSize"] = $"{_romSize / 1024}K",
				["Banks"] = _romBanks.ToString(),
				["CopierHeader"] = HasCopierHeader.ToString()
			}
		);
	}

	public string? GetRegisterLabel(uint address) {
		var addr = address & 0xffff; // Mask to 16-bit

		if (PpuRegisters.TryGetValue(addr, out var ppuLabel))
			return ppuLabel;
		if (ApuRegisters.TryGetValue(addr, out var apuLabel))
			return apuLabel;
		if (WramRegisters.TryGetValue(addr, out var wramLabel))
			return wramLabel;
		if (CpuRegisters.TryGetValue(addr, out var cpuLabel))
			return cpuLabel;

		// DMA registers ($43x0-$43xA)
		if (addr >= 0x4300 && addr < 0x4380) {
			var channel = (addr >> 4) & 0x07;
			var reg = addr & 0x0f;
			return reg switch {
				0x00 => $"DMAP{channel}",
				0x01 => $"BBAD{channel}",
				0x02 => $"A1TL{channel}",
				0x03 => $"A1TH{channel}",
				0x04 => $"A1B{channel}",
				0x05 => $"DAS{channel}L",
				0x06 => $"DAS{channel}H",
				0x07 => $"DASB{channel}",
				0x08 => $"A2A{channel}L",
				0x09 => $"A2A{channel}H",
				0x0a => $"NTRL{channel}",
				_ => null
			};
		}

		return null;
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		var bank = (address >> 16) & 0xff;
		var offset = address & 0xffff;

		// Banks $00-$3F (and mirrors $80-$BF)
		if (bank < 0x40 || (bank >= 0x80 && bank < 0xc0)) {
			if (offset < 0x2000) return MemoryRegion.Ram;      // WRAM (first 8K)
			if (offset < 0x2100) return MemoryRegion.Unknown;  // Unused
			if (offset < 0x2200) return MemoryRegion.Hardware; // PPU
			if (offset < 0x4000) return MemoryRegion.Unknown;  // Reserved
			if (offset < 0x4400) return MemoryRegion.Hardware; // CPU/DMA
			if (offset < 0x8000) return MemoryRegion.Unknown;  // Expansion
			return MemoryRegion.Rom;                           // ROM
		}

		// Banks $40-$6F (and $C0-$EF) - typically ROM
		if (bank < 0x70 || (bank >= 0xc0 && bank < 0xf0))
			return MemoryRegion.Rom;

		// Banks $70-$7D (and $F0-$FF) - SRAM or ROM depending on game
		if (bank < 0x7e || bank >= 0xf0)
			return offset < 0x8000 ? MemoryRegion.Ram : MemoryRegion.Rom;

		// Banks $7E-$7F - WRAM (128K)
		if (bank == 0x7e || bank == 0x7f)
			return MemoryRegion.Ram;

		return MemoryRegion.Unknown;
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		var offset = HasCopierHeader ? 512 : 0;
		var data = rom[offset..];

		// Read reset vector from header
		var headerOffset = MapMode == SnesMapMode.LoRom ? 0x7fc0 : 0xffc0;

		if (data.Length < headerOffset + 0x40)
			return [0x8000];

		var entries = new HashSet<uint>();

		// Native mode vectors (at header + $24-$2F)
		// $FFE4 = COP, $FFE6 = BRK, $FFE8 = ABORT, $FFEA = NMI, $FFEC = unused, $FFEE = IRQ
		var nativeCop = (uint)(data[headerOffset + 0x24] | (data[headerOffset + 0x25] << 8));
		var nativeBrk = (uint)(data[headerOffset + 0x26] | (data[headerOffset + 0x27] << 8));
		var nativeNmi = (uint)(data[headerOffset + 0x2a] | (data[headerOffset + 0x2b] << 8));
		var nativeIrq = (uint)(data[headerOffset + 0x2e] | (data[headerOffset + 0x2f] << 8));

		// Emulation mode vectors (at header + $34-$3F)
		// $FFF4 = COP, $FFF8 = ABORT, $FFFA = NMI, $FFFC = RESET, $FFFE = IRQ/BRK
		var emuReset = (uint)(data[headerOffset + 0x3c] | (data[headerOffset + 0x3d] << 8));
		var emuNmi = (uint)(data[headerOffset + 0x3a] | (data[headerOffset + 0x3b] << 8));
		var emuIrq = (uint)(data[headerOffset + 0x3e] | (data[headerOffset + 0x3f] << 8));

		// Add valid entry points (must be in ROM space)
		if (emuReset >= 0x8000) entries.Add(emuReset);
		if (emuNmi >= 0x8000 && emuNmi != emuReset) entries.Add(emuNmi);
		if (emuIrq >= 0x8000 && emuIrq != emuReset && emuIrq != emuNmi) entries.Add(emuIrq);
		if (nativeNmi >= 0x8000) entries.Add(nativeNmi);
		if (nativeIrq >= 0x8000) entries.Add(nativeIrq);

		return entries.Count > 0 ? [.. entries] : [0x8000];
	}

	public bool IsInSwitchableRegion(uint address) {
		// SNES doesn't have fixed/switchable regions like NES mappers
		// Banks are directly addressed via the bank byte
		return false;
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		var adjustedLength = HasCopierHeader ? romLength - 512 : romLength;
		var headerOffset = HasCopierHeader ? 512 : 0;

		var addrBank = (int)((address >> 16) & 0xff);
		var offset = (int)(address & 0xffff);

		// Use address bank if specified, otherwise use parameter
		if (addrBank > 0) bank = addrBank;

		return MapMode switch {
			SnesMapMode.LoRom => LoRomAddressToOffset(bank, offset, adjustedLength) + headerOffset,
			SnesMapMode.HiRom => HiRomAddressToOffset(bank, offset, adjustedLength) + headerOffset,
			SnesMapMode.ExLoRom => ExLoRomAddressToOffset(bank, offset, adjustedLength) + headerOffset,
			SnesMapMode.ExHiRom => ExHiRomAddressToOffset(bank, offset, adjustedLength) + headerOffset,
			_ => -1
		};
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// SNES uses direct long addressing (JSL/JML) for cross-bank calls
		// No BRK-based switching like NES mappers
		return null;
	}

	/// <summary>
	/// Detect ROM map mode from internal header
	/// </summary>
	private void DetectMapMode(ReadOnlySpan<byte> rom) {
		// Try LoRom header location ($7fc0)
		if (rom.Length >= 0x8000 && IsValidHeader(rom, 0x7fc0)) {
			MapMode = GetMapModeFromByte(rom[0x7fd5]);
			return;
		}

		// Try HiRom header location ($ffc0)
		if (rom.Length >= 0x10000 && IsValidHeader(rom, 0xffc0)) {
			MapMode = GetMapModeFromByte(rom[0xffd5]);
			return;
		}

		// Try ExLoRom header location ($407fc0)
		if (rom.Length >= 0x408000 && IsValidHeader(rom, 0x407fc0)) {
			MapMode = SnesMapMode.ExLoRom;
			return;
		}

		// Try ExHiRom header location ($40ffc0)
		if (rom.Length >= 0x410000 && IsValidHeader(rom, 0x40ffc0)) {
			MapMode = SnesMapMode.ExHiRom;
			return;
		}

		// Default to LoRom
		MapMode = SnesMapMode.LoRom;
	}

	private static bool IsValidHeader(ReadOnlySpan<byte> rom, int offset) {
		if (offset + 0x40 > rom.Length)
			return false;

		// Check checksum complement
		var checksum = (ushort)(rom[offset + 0x1e] | (rom[offset + 0x1f] << 8));
		var complement = (ushort)(rom[offset + 0x1c] | (rom[offset + 0x1d] << 8));

		return (ushort)(checksum + complement) == 0xffff;
	}

	private static SnesMapMode GetMapModeFromByte(byte mapByte) {
		return (mapByte & 0x0f) switch {
			0x00 or 0x01 => SnesMapMode.LoRom,
			0x02 or 0x03 => SnesMapMode.HiRom,
			0x05 => SnesMapMode.ExLoRom,
			0x06 => SnesMapMode.ExHiRom,
			_ => SnesMapMode.LoRom
		};
	}

	// LoRom: 32KB banks, ROM at $8000-$FFFF
	private static int LoRomAddressToOffset(int bank, int offset, int romLength) {
		// Mirror high banks to low banks
		bank &= 0x7f;

		// LoRom only uses upper 32K of each bank
		if (offset < 0x8000) return -1;

		var fileOffset = (bank * 0x8000) + (offset - 0x8000);
		return fileOffset < romLength ? fileOffset : -1;
	}

	// HiRom: 64KB banks, ROM at $0000-$FFFF
	private static int HiRomAddressToOffset(int bank, int offset, int romLength) {
		int fileOffset;

		if (bank >= 0xc0) {
			// Direct mapping $C0-$FF
			fileOffset = ((bank - 0xc0) * 0x10000) + offset;
		} else if (bank >= 0x80) {
			// Mirror of $C0-$FF at $80-$BF
			fileOffset = ((bank - 0x80) * 0x10000) + offset;
		} else if (bank >= 0x40) {
			// Banks $40-$7D
			fileOffset = ((bank - 0x40) * 0x10000) + offset;
		} else if (offset >= 0x8000) {
			// Banks $00-$3F, offset $8000-$FFFF
			fileOffset = (bank * 0x10000) + (offset - 0x8000);
		} else {
			return -1; // Not ROM space
		}

		return fileOffset < romLength ? fileOffset : -1;
	}

	// ExLoRom: Extended LoRom for ROMs > 4MB
	private static int ExLoRomAddressToOffset(int bank, int offset, int romLength) {
		if (offset < 0x8000) return -1;

		int fileOffset;
		if (bank >= 0x80) {
			// Extended range
			fileOffset = ((bank - 0x80) * 0x8000) + (offset - 0x8000) + 0x400000;
		} else {
			// Standard LoRom range
			fileOffset = (bank * 0x8000) + (offset - 0x8000);
		}

		return fileOffset < romLength ? fileOffset : -1;
	}

	// ExHiRom: Extended HiRom for ROMs > 4MB
	private static int ExHiRomAddressToOffset(int bank, int offset, int romLength) {
		int fileOffset;

		if (bank >= 0xc0) {
			fileOffset = ((bank - 0xc0) * 0x10000) + offset;
		} else if (bank >= 0x40) {
			fileOffset = ((bank - 0x40) * 0x10000) + offset + 0x400000;
		} else if (offset >= 0x8000) {
			fileOffset = (bank * 0x10000) + (offset - 0x8000);
		} else {
			return -1;
		}

		return fileOffset < romLength ? fileOffset : -1;
	}
}

/// <summary>
/// SNES ROM mapping modes
/// </summary>
public enum SnesMapMode {
	LoRom,      // Mode 20 - 32KB banks
	HiRom,      // Mode 21 - 64KB banks
	ExLoRom,    // Mode 25 - Extended LoRom (48Mbit+)
	ExHiRom     // Mode 26 - Extended HiRom (48Mbit+)
}
