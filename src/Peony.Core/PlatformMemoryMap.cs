namespace Peony.Core;

/// <summary>
/// A ROM interrupt/reset vector entry.
/// </summary>
public record VectorEntry(uint Address, string Name, int Size = 2);

/// <summary>
/// Platform-specific memory map knowledge base.
/// Provides deterministic classification for known address ranges.
/// </summary>
public static class PlatformMemoryMap {
	/// <summary>
	/// Get the classification for an address on the given platform.
	/// Returns null for addresses that could be either code or data (e.g., ROM space).
	/// </summary>
	public static ByteClassification? GetKnownClassification(string platform, uint address) {
		return platform switch {
			"NES" => GetNesClassification(address),
			"SNES" => GetSnesClassification(address),
			"Game Boy" => GetGameBoyClassification(address),
			"GBA" => GetGbaClassification(address),
			"Atari 2600" => GetAtari2600Classification(address),
			"Atari Lynx" => GetLynxClassification(address),
			_ => null,
		};
	}

	/// <summary>
	/// Get hardware register name for an address, or null if not a register.
	/// </summary>
	public static string? GetHardwareRegisterName(string platform, uint address) {
		return platform switch {
			"NES" => GetNesRegisterName(address),
			"Game Boy" => GetGameBoyRegisterName(address),
			"Atari 2600" => GetAtari2600RegisterName(address),
			_ => null,
		};
	}

	/// <summary>
	/// Get vector addresses for the platform (reset, NMI, IRQ, etc.).
	/// </summary>
	public static IReadOnlyList<VectorEntry> GetVectors(string platform) {
		return platform switch {
			"NES" => NesVectors,
			"SNES" => SnesNativeVectors,
			"Game Boy" => GameBoyVectors,
			"Atari 2600" => Atari2600Vectors,
			"Atari Lynx" => LynxVectors,
			_ => [],
		};
	}

	// ========================================================================
	// NES
	// ========================================================================

	private static readonly VectorEntry[] NesVectors = [
		new(0xfffa, "NMI"),
		new(0xfffc, "RESET"),
		new(0xfffe, "IRQ"),
	];

	private static ByteClassification? GetNesClassification(uint address) {
		return address switch {
			// Internal RAM
			>= 0x0000 and <= 0x07ff => ByteClassification.Data,
			// RAM mirrors
			>= 0x0800 and <= 0x1fff => ByteClassification.Data,
			// PPU registers
			>= 0x2000 and <= 0x2007 => ByteClassification.Hardware,
			// PPU register mirrors
			>= 0x2008 and <= 0x3fff => ByteClassification.Hardware,
			// APU and I/O registers
			>= 0x4000 and <= 0x4017 => ByteClassification.Hardware,
			// APU test registers + mapper
			>= 0x4018 and <= 0x401f => ByteClassification.Hardware,
			// Battery-backed SRAM
			>= 0x6000 and <= 0x7fff => ByteClassification.Data,
			// PRG-ROM: could be code or data, don't guess
			_ => null,
		};
	}

	private static string? GetNesRegisterName(uint address) {
		// PPU registers (mirrored every 8 bytes from $2000-$3fff)
		return (address & 0x2007) switch {
			0x2000 => "PPUCTRL",
			0x2001 => "PPUMASK",
			0x2002 => "PPUSTATUS",
			0x2003 => "OAMADDR",
			0x2004 => "OAMDATA",
			0x2005 => "PPUSCROLL",
			0x2006 => "PPUADDR",
			0x2007 => "PPUDATA",
			_ => address switch {
				0x4000 => "SQ1_VOL",
				0x4001 => "SQ1_SWEEP",
				0x4002 => "SQ1_LO",
				0x4003 => "SQ1_HI",
				0x4004 => "SQ2_VOL",
				0x4005 => "SQ2_SWEEP",
				0x4006 => "SQ2_LO",
				0x4007 => "SQ2_HI",
				0x4008 => "TRI_LINEAR",
				0x400a => "TRI_LO",
				0x400b => "TRI_HI",
				0x400c => "NOISE_VOL",
				0x400e => "NOISE_LO",
				0x400f => "NOISE_HI",
				0x4010 => "DMC_FREQ",
				0x4011 => "DMC_RAW",
				0x4012 => "DMC_START",
				0x4013 => "DMC_LEN",
				0x4014 => "OAMDMA",
				0x4015 => "SND_CHN",
				0x4016 => "JOY1",
				0x4017 => "JOY2",
				_ => null,
			},
		};
	}

	// ========================================================================
	// SNES
	// ========================================================================

	private static readonly VectorEntry[] SnesNativeVectors = [
		new(0xffe4, "COP"),
		new(0xffe6, "BRK"),
		new(0xffe8, "ABORT"),
		new(0xffea, "NMI"),
		new(0xffee, "IRQ"),
		new(0xfffc, "RESET"),
	];

	private static ByteClassification? GetSnesClassification(uint address) {
		// Bank $00 system area
		if ((address & 0xff0000) == 0) {
			return (address & 0xffff) switch {
				// WRAM direct page + stack
				>= 0x0000 and <= 0x1fff => ByteClassification.Data,
				// PPU registers
				>= 0x2100 and <= 0x213f => ByteClassification.Hardware,
				// APU registers
				>= 0x2140 and <= 0x2143 => ByteClassification.Hardware,
				// WRAM access
				>= 0x2180 and <= 0x2183 => ByteClassification.Hardware,
				// CPU registers
				>= 0x4200 and <= 0x421f => ByteClassification.Hardware,
				// DMA registers
				>= 0x4300 and <= 0x437f => ByteClassification.Hardware,
				_ => null,
			};
		}
		// Banks $7e-$7f: WRAM
		if ((address & 0xff0000) is >= 0x7e0000 and <= 0x7f0000)
			return ByteClassification.Data;

		return null;
	}

	// ========================================================================
	// Game Boy
	// ========================================================================

	private static readonly VectorEntry[] GameBoyVectors = [
		new(0x0040, "VBLANK_ISR"),
		new(0x0048, "LCD_STAT_ISR"),
		new(0x0050, "TIMER_ISR"),
		new(0x0058, "SERIAL_ISR"),
		new(0x0060, "JOYPAD_ISR"),
		new(0x0100, "ENTRY_POINT"),
	];

	private static ByteClassification? GetGameBoyClassification(uint address) {
		return address switch {
			// VRAM
			>= 0x8000 and <= 0x9fff => ByteClassification.Graphics,
			// External RAM
			>= 0xa000 and <= 0xbfff => ByteClassification.Data,
			// WRAM
			>= 0xc000 and <= 0xdfff => ByteClassification.Data,
			// Echo RAM (mirror of c000-ddff)
			>= 0xe000 and <= 0xfdff => ByteClassification.Data,
			// OAM
			>= 0xfe00 and <= 0xfe9f => ByteClassification.Data,
			// I/O Registers
			>= 0xff00 and <= 0xff7f => ByteClassification.Hardware,
			// HRAM
			>= 0xff80 and <= 0xfffe => ByteClassification.Data,
			// Interrupt enable register
			0xffff => ByteClassification.Hardware,
			_ => null,
		};
	}

	private static string? GetGameBoyRegisterName(uint address) {
		return address switch {
			0xff00 => "JOYP",
			0xff01 => "SB",
			0xff02 => "SC",
			0xff04 => "DIV",
			0xff05 => "TIMA",
			0xff06 => "TMA",
			0xff07 => "TAC",
			0xff0f => "IF",
			0xff10 => "NR10",
			0xff11 => "NR11",
			0xff12 => "NR12",
			0xff13 => "NR13",
			0xff14 => "NR14",
			0xff40 => "LCDC",
			0xff41 => "STAT",
			0xff42 => "SCY",
			0xff43 => "SCX",
			0xff44 => "LY",
			0xff45 => "LYC",
			0xff46 => "DMA",
			0xff47 => "BGP",
			0xff48 => "OBP0",
			0xff49 => "OBP1",
			0xff4a => "WY",
			0xff4b => "WX",
			0xffff => "IE",
			_ => null,
		};
	}

	// ========================================================================
	// GBA
	// ========================================================================

	private static ByteClassification? GetGbaClassification(uint address) {
		return address switch {
			// BIOS
			>= 0x00000000 and <= 0x00003fff => ByteClassification.Code,
			// EWRAM
			>= 0x02000000 and <= 0x0203ffff => ByteClassification.Data,
			// IWRAM
			>= 0x03000000 and <= 0x03007fff => ByteClassification.Data,
			// I/O Registers
			>= 0x04000000 and <= 0x040003fe => ByteClassification.Hardware,
			// Palette RAM
			>= 0x05000000 and <= 0x050003ff => ByteClassification.Data,
			// VRAM
			>= 0x06000000 and <= 0x06017fff => ByteClassification.Graphics,
			// OAM
			>= 0x07000000 and <= 0x070003ff => ByteClassification.Data,
			// ROM: could be code or data, don't guess
			_ => null,
		};
	}

	// ========================================================================
	// Atari 2600
	// ========================================================================

	private static readonly VectorEntry[] Atari2600Vectors = [
		new(0xfffa, "NMI"),
		new(0xfffc, "RESET"),
		new(0xfffe, "IRQ"),
	];

	private static ByteClassification? GetAtari2600Classification(uint address) {
		// Atari 2600 uses 13-bit addressing (8KB mirrored)
		uint masked = address & 0x1fff;
		return masked switch {
			// TIA (write)
			>= 0x0000 and <= 0x002c => ByteClassification.Hardware,
			// TIA (read)
			>= 0x0030 and <= 0x003d => ByteClassification.Hardware,
			// RIOT RAM
			>= 0x0080 and <= 0x00ff => ByteClassification.Data,
			// RIOT Registers
			>= 0x0280 and <= 0x029f => ByteClassification.Hardware,
			// ROM space: don't guess
			_ => null,
		};
	}

	private static string? GetAtari2600RegisterName(uint address) {
		uint masked = address & 0x1fff;
		return masked switch {
			// TIA Write registers
			0x0000 => "VSYNC",
			0x0001 => "VBLANK",
			0x0002 => "WSYNC",
			0x0003 => "RSYNC",
			0x0004 => "NUSIZ0",
			0x0005 => "NUSIZ1",
			0x0006 => "COLUP0",
			0x0007 => "COLUP1",
			0x0008 => "COLUPF",
			0x0009 => "COLUBK",
			0x000a => "CTRLPF",
			0x000b => "REFP0",
			0x000c => "REFP1",
			0x000d => "PF0",
			0x000e => "PF1",
			0x000f => "PF2",
			0x0010 => "RESP0",
			0x0011 => "RESP1",
			0x0012 => "RESM0",
			0x0013 => "RESM1",
			0x0014 => "RESBL",
			0x001b => "GRP0",
			0x001c => "GRP1",
			0x001d => "ENAM0",
			0x001e => "ENAM1",
			0x001f => "ENABL",
			0x0020 => "HMP0",
			0x0021 => "HMP1",
			0x0022 => "HMM0",
			0x0023 => "HMM1",
			0x0024 => "HMBL",
			0x0025 => "VDELP0",
			0x0026 => "VDELP1",
			0x0027 => "VDELBL",
			0x0028 => "RESMP0",
			0x0029 => "RESMP1",
			0x002a => "HMOVE",
			0x002b => "HMCLR",
			0x002c => "CXCLR",
			// TIA Read registers
			0x0030 => "CXM0P",
			0x0031 => "CXM1P",
			0x0032 => "CXP0FB",
			0x0033 => "CXP1FB",
			0x0034 => "CXM0FB",
			0x0035 => "CXM1FB",
			0x0036 => "CXBLPF",
			0x0037 => "CXPPMM",
			0x0038 => "INPT0",
			0x0039 => "INPT1",
			0x003a => "INPT2",
			0x003b => "INPT3",
			0x003c => "INPT4",
			0x003d => "INPT5",
			// RIOT registers
			0x0280 => "SWCHA",
			0x0281 => "SWACNT",
			0x0282 => "SWCHB",
			0x0283 => "SWBCNT",
			0x0284 => "INTIM",
			0x0285 => "TIMINT",
			0x0294 => "TIM1T",
			0x0295 => "TIM8T",
			0x0296 => "TIM64T",
			0x0297 => "T1024T",
			_ => null,
		};
	}

	// ========================================================================
	// Atari Lynx (65SC02)
	// ========================================================================

	private static readonly VectorEntry[] LynxVectors = [
		new(0xfffa, "NMI"),
		new(0xfffc, "RESET"),
		new(0xfffe, "IRQ"),
	];

	private static ByteClassification? GetLynxClassification(uint address) {
		return address switch {
			// Zero page
			>= 0x0000 and <= 0x00ff => ByteClassification.Data,
			// Stack
			>= 0x0100 and <= 0x01ff => ByteClassification.Data,
			// Mikey (hardware)
			>= 0xfd00 and <= 0xfdff => ByteClassification.Hardware,
			// Suzy (hardware)
			>= 0xfc00 and <= 0xfcff => ByteClassification.Hardware,
			// ROM space: don't guess
			_ => null,
		};
	}
}
