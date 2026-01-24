namespace Peony.Platform.GameBoy;

using Peony.Core;
using Peony.Cpu.GameBoy;

/// <summary>
/// Game Boy platform analyzer with ROM/RAM/hardware register detection
/// </summary>
public class GameBoyAnalyzer : IPlatformAnalyzer {
	public string Platform => "Game Boy";
	public ICpuDecoder CpuDecoder { get; } = new GameBoyCpuDecoder();
	public int BankCount => _romBanks;

	private int _romBanks = 1;
	private int _ramBanks = 0;
	private string? _mbcType;

	// Hardware registers ($ff00-$ff7f)
	private static readonly Dictionary<uint, string> HardwareRegisters = new() {
		// Joypad
		[0xff00] = "P1",

		// Serial transfer
		[0xff01] = "SB",
		[0xff02] = "SC",

		// Timer
		[0xff04] = "DIV",
		[0xff05] = "TIMA",
		[0xff06] = "TMA",
		[0xff07] = "TAC",

		// Interrupts
		[0xff0f] = "IF",
		[0xffff] = "IE",

		// Sound
		[0xff10] = "NR10", [0xff11] = "NR11", [0xff12] = "NR12", [0xff13] = "NR13", [0xff14] = "NR14",
		[0xff16] = "NR21", [0xff17] = "NR22", [0xff18] = "NR23", [0xff19] = "NR24",
		[0xff1a] = "NR30", [0xff1b] = "NR31", [0xff1c] = "NR32", [0xff1d] = "NR33", [0xff1e] = "NR34",
		[0xff20] = "NR41", [0xff21] = "NR42", [0xff22] = "NR43", [0xff23] = "NR44",
		[0xff24] = "NR50", [0xff25] = "NR51", [0xff26] = "NR52",

		// Wave pattern RAM ($ff30-$ff3f)
		[0xff30] = "WAVE_RAM", // Just label the start

		// LCD
		[0xff40] = "LCDC",
		[0xff41] = "STAT",
		[0xff42] = "SCY",
		[0xff43] = "SCX",
		[0xff44] = "LY",
		[0xff45] = "LYC",
		[0xff46] = "DMA",
		[0xff47] = "BGP",
		[0xff48] = "OBP0",
		[0xff49] = "OBP1",
		[0xff4a] = "WY",
		[0xff4b] = "WX",

		// CGB only (ignored for DMG)
		[0xff4d] = "KEY1",
		[0xff4f] = "VBK",
		[0xff51] = "HDMA1",
		[0xff52] = "HDMA2",
		[0xff53] = "HDMA3",
		[0xff54] = "HDMA4",
		[0xff55] = "HDMA5",
		[0xff68] = "BCPS",
		[0xff69] = "BCPD",
		[0xff6a] = "OCPS",
		[0xff6b] = "OCPD",
		[0xff70] = "SVBK",
	};

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		if (rom.Length < 0x150)
			return new RomInfo(Platform, rom.Length, null, new Dictionary<string, string>());

		// Read cartridge header at $0100-$014f
		var cartridgeType = rom[0x147];
		var romSize = rom[0x148];
		var ramSize = rom[0x149];

		_mbcType = GetMbcType(cartridgeType);
		_romBanks = GetRomBankCount(romSize);
		_ramBanks = GetRamBankCount(ramSize);

		var title = GetTitle(rom[0x134..0x144]);
		var cgbFlag = rom[0x143];
		var isCgb = (cgbFlag & 0x80) != 0;

		return new RomInfo(
			Platform,
			rom.Length,
			_mbcType,
			new Dictionary<string, string> {
				["Title"] = title,
				["MBC"] = _mbcType ?? "None",
				["RomBanks"] = _romBanks.ToString(),
				["RamBanks"] = _ramBanks.ToString(),
				["RomSize"] = FormatSize(rom.Length),
				["RamSize"] = FormatSize(_ramBanks * 8192),
				["CGB"] = isCgb ? "Yes" : "No"
			}
		);
	}

	public string? GetRegisterLabel(uint address) {
		if (HardwareRegisters.TryGetValue(address, out var label))
			return label;

		// Wave RAM range
		if (address >= 0xff30 && address <= 0xff3f)
			return $"WAVE_RAM+${(address - 0xff30):x}";

		return null;
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			< 0x8000 => MemoryRegion.Rom,        // ROM banks
			< 0xa000 => MemoryRegion.Graphics,   // VRAM
			< 0xc000 => MemoryRegion.Rom,        // External RAM (cartridge)
			< 0xe000 => MemoryRegion.Ram,        // Work RAM
			< 0xfe00 => MemoryRegion.Ram,        // Echo RAM (mirror of 0xc000-0xddff)
			< 0xfea0 => MemoryRegion.Graphics,   // OAM (sprite attribute table)
			< 0xff00 => MemoryRegion.Unknown,    // Not usable
			< 0xff80 => MemoryRegion.Hardware,   // Hardware registers
			< 0xffff => MemoryRegion.Ram,        // High RAM (HRAM)
			_ => MemoryRegion.Hardware            // IE register
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		// Game Boy starts at $0100 (after the Nintendo logo)
		// Additional entry points from interrupts
		return [
			0x0100, // Start
			0x0040, // VBlank interrupt
			0x0048, // LCD STAT interrupt
			0x0050, // Timer interrupt
			0x0058, // Serial interrupt
			0x0060  // Joypad interrupt
		];
	}

	public bool IsInSwitchableRegion(uint address) {
		// ROM bank 1+ at $4000-$7fff is switchable
		// External RAM at $a000-$bfff is switchable (if present)
		return (address >= 0x4000 && address < 0x8000) ||
		       (address >= 0xa000 && address < 0xc000 && _ramBanks > 0);
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, -1);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// ROM bank 0 ($0000-$3fff) is always fixed
		if (address < 0x4000)
			return (int)address;

		// ROM bank 1+ ($4000-$7fff) is switchable
		if (address < 0x8000) {
			if (bank < 0) bank = 1; // Default to bank 1
			return (bank * 0x4000) + (int)(address - 0x4000);
		}

		// Other regions not in ROM file
		return -1;
	}

	public uint? OffsetToAddress(int offset) {
		// GB: Bank 0 at $0000-$3fff, bank 1+ at $4000-$7fff
		if (offset < 0x4000)
			return (uint)offset;
		// Return switchable bank address
		return (uint)(0x4000 + (offset % 0x4000));
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// Game Boy doesn't use BRK for bank switching
		// Bank switches happen via MBC register writes
		return null;
	}

	private static string? GetMbcType(byte type) {
		return type switch {
			0x00 => null,           // No MBC
			0x01 => "MBC1",
			0x02 => "MBC1+RAM",
			0x03 => "MBC1+RAM+BATTERY",
			0x05 => "MBC2",
			0x06 => "MBC2+BATTERY",
			0x08 => "ROM+RAM",
			0x09 => "ROM+RAM+BATTERY",
			0x0b => "MMM01",
			0x0c => "MMM01+RAM",
			0x0d => "MMM01+RAM+BATTERY",
			0x0f => "MBC3+TIMER+BATTERY",
			0x10 => "MBC3+TIMER+RAM+BATTERY",
			0x11 => "MBC3",
			0x12 => "MBC3+RAM",
			0x13 => "MBC3+RAM+BATTERY",
			0x19 => "MBC5",
			0x1a => "MBC5+RAM",
			0x1b => "MBC5+RAM+BATTERY",
			0x1c => "MBC5+RUMBLE",
			0x1d => "MBC5+RUMBLE+RAM",
			0x1e => "MBC5+RUMBLE+RAM+BATTERY",
			0x20 => "MBC6",
			0x22 => "MBC7+SENSOR+RUMBLE+RAM+BATTERY",
			0xfc => "POCKET CAMERA",
			0xfd => "BANDAI TAMA5",
			0xfe => "HuC3",
			0xff => "HuC1+RAM+BATTERY",
			_ => $"Unknown (${type:x2})"
		};
	}

	private static int GetRomBankCount(byte size) {
		// Size code: 0 = 32KB (2 banks), 1 = 64KB (4 banks), etc.
		return size switch {
			<= 8 => 2 << size, // 0 = 2, 1 = 4, 2 = 8, 3 = 16, ..., 8 = 512
			0x52 => 72,
			0x53 => 80,
			0x54 => 96,
			_ => 2
		};
	}

	private static int GetRamBankCount(byte size) {
		return size switch {
			0 => 0,   // No RAM
			1 => 0,   // Unused (but some use 2KB)
			2 => 1,   // 8KB (1 bank)
			3 => 4,   // 32KB (4 banks)
			4 => 16,  // 128KB (16 banks)
			5 => 8,   // 64KB (8 banks)
			_ => 0
		};
	}

	private static string GetTitle(ReadOnlySpan<byte> titleBytes) {
		var chars = new List<char>();
		foreach (var b in titleBytes) {
			if (b == 0) break;
			if (b >= 0x20 && b <= 0x7e)
				chars.Add((char)b);
		}
		return new string(chars.ToArray()).Trim();
	}

	private static string FormatSize(int bytes) {
		if (bytes < 1024) return $"{bytes} B";
		if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
		return $"{bytes / (1024 * 1024)} MB";
	}
}
