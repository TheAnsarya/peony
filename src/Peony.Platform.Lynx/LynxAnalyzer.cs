namespace Peony.Platform.Lynx;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Atari Lynx platform analyzer with Suzy/Mikey register detection
/// and LNX header parsing support.
/// </summary>
/// <remarks>
/// <para>
/// The Atari Lynx uses a WDC 65SC02 CPU at 4 MHz with 64KB of RAM.
/// Hardware is accessed through two custom chips:
/// - Suzy ($fc00-$fcff): Graphics coprocessor, math coprocessor, collision detection
/// - Mikey ($fd00-$fdff): Audio (4 channels), timers, UART, display control
/// </para>
/// <para>
/// The Boot ROM at $fe00-$ffff handles initial loading from cartridge.
/// Vectors are at $fff8-$ffff (NMI, RESET, IRQ/BRK).
/// </para>
/// </remarks>
public sealed class LynxAnalyzer : IPlatformAnalyzer {
	/// <summary>
	/// Gets the platform name.
	/// </summary>
	public string Platform => "Atari Lynx";

	/// <summary>
	/// Gets the CPU decoder (65SC02).
	/// </summary>
	public ICpuDecoder CpuDecoder { get; } = new Cpu65SC02Decoder();

	/// <summary>
	/// Gets the number of ROM banks.
	/// </summary>
	public int BankCount { get; private set; } = 1;

	/// <summary>
	/// Gets the parsed LNX header, if present.
	/// </summary>
	public LnxHeader? LnxHeader { get; private set; }

	/// <summary>
	/// Gets the ROM data offset (64 for LNX format, 0 for raw).
	/// </summary>
	public int RomDataOffset { get; private set; }

	// Suzy Registers ($fc00-$fcff)
	// Graphics coprocessor, hardware math, sprites
	private static readonly FrozenDictionary<uint, string> SuzyRegisters = new Dictionary<uint, string> {
		// Temporary addresses
		[0xfc00] = "TMPADR_L",    [0xfc01] = "TMPADR_H",
		[0xfc02] = "TILTACUM_L",  [0xfc03] = "TILTACUM_H",
		[0xfc04] = "HOFF_L",      [0xfc05] = "HOFF_H",
		[0xfc06] = "VOFF_L",      [0xfc07] = "VOFF_H",
		[0xfc08] = "VIDBAS_L",    [0xfc09] = "VIDBAS_H",
		[0xfc0a] = "COLLBAS_L",   [0xfc0b] = "COLLBAS_H",
		[0xfc0c] = "VIDADR_L",    [0xfc0d] = "VIDADR_H",
		[0xfc0e] = "COLLADR_L",   [0xfc0f] = "COLLADR_H",

		// Sprite control
		[0xfc10] = "SCBNEXT_L",   [0xfc11] = "SCBNEXT_H",
		[0xfc12] = "SPRDLINE_L",  [0xfc13] = "SPRDLINE_H",
		[0xfc14] = "HPOSSTRT_L",  [0xfc15] = "HPOSSTRT_H",
		[0xfc16] = "VPOSSTRT_L",  [0xfc17] = "VPOSSTRT_H",
		[0xfc18] = "SPRHSIZ_L",   [0xfc19] = "SPRHSIZ_H",
		[0xfc1a] = "SPRVSIZ_L",   [0xfc1b] = "SPRVSIZ_H",
		[0xfc1c] = "STRETCH_L",   [0xfc1d] = "STRETCH_H",
		[0xfc1e] = "TILT_L",      [0xfc1f] = "TILT_H",

		// Math coprocessor
		[0xfc52] = "MATH_A",      [0xfc53] = "MATH_B",
		[0xfc54] = "MATH_C",      [0xfc55] = "MATH_D",
		[0xfc56] = "MATH_E",      [0xfc57] = "MATH_F",
		[0xfc60] = "MATH_G",      [0xfc61] = "MATH_H",
		[0xfc62] = "MATH_J",      [0xfc63] = "MATH_K",
		[0xfc6c] = "MATH_M",      [0xfc6d] = "MATH_N",
		[0xfc6e] = "MATH_P",

		// Sprite system control
		[0xfc80] = "SPRCTL0",     [0xfc81] = "SPRCTL1",
		[0xfc82] = "SPRCOLL",     [0xfc83] = "SPRINIT",
		[0xfc88] = "SUZYBUSEN",   [0xfc89] = "SPRGO",
		[0xfc90] = "SPRSYS",      [0xfc91] = "SPRCTL0_R",
		[0xfc92] = "JOYSTICK",    [0xfc93] = "SWITCHES",
		[0xfca0] = "RCART0",      [0xfca1] = "RCART1",

		// Collision palette
		[0xfcb0] = "PENNDX",
	}.ToFrozenDictionary();

	// Mikey Registers ($fd00-$fdff)
	// Audio, timers, UART, display control
	private static readonly FrozenDictionary<uint, string> MikeyRegisters = new Dictionary<uint, string> {
		// Timers
		[0xfd00] = "TIM0BKUP",    [0xfd01] = "TIM0CTLA",
		[0xfd02] = "TIM0CNT",     [0xfd03] = "TIM0CTLB",
		[0xfd04] = "TIM1BKUP",    [0xfd05] = "TIM1CTLA",
		[0xfd06] = "TIM1CNT",     [0xfd07] = "TIM1CTLB",
		[0xfd08] = "TIM2BKUP",    [0xfd09] = "TIM2CTLA",
		[0xfd0a] = "TIM2CNT",     [0xfd0b] = "TIM2CTLB",
		[0xfd0c] = "TIM3BKUP",    [0xfd0d] = "TIM3CTLA",
		[0xfd0e] = "TIM3CNT",     [0xfd0f] = "TIM3CTLB",
		[0xfd10] = "TIM4BKUP",    [0xfd11] = "TIM4CTLA",
		[0xfd12] = "TIM4CNT",     [0xfd13] = "TIM4CTLB",
		[0xfd14] = "TIM5BKUP",    [0xfd15] = "TIM5CTLA",
		[0xfd16] = "TIM5CNT",     [0xfd17] = "TIM5CTLB",
		[0xfd18] = "TIM6BKUP",    [0xfd19] = "TIM6CTLA",
		[0xfd1a] = "TIM6CNT",     [0xfd1b] = "TIM6CTLB",
		[0xfd1c] = "TIM7BKUP",    [0xfd1d] = "TIM7CTLA",
		[0xfd1e] = "TIM7CNT",     [0xfd1f] = "TIM7CTLB",

		// Audio channels (timers 0-3 linked to audio)
		[0xfd20] = "AUD0VOL",     [0xfd21] = "AUD0FEED",
		[0xfd22] = "AUD0OUT",     [0xfd23] = "AUD0SHIFT",
		[0xfd24] = "AUD0BKUP",    [0xfd25] = "AUD0CTLA",
		[0xfd26] = "AUD0CNT",     [0xfd27] = "AUD0CTLB",
		[0xfd28] = "AUD1VOL",     [0xfd29] = "AUD1FEED",
		[0xfd2a] = "AUD1OUT",     [0xfd2b] = "AUD1SHIFT",
		[0xfd2c] = "AUD1BKUP",    [0xfd2d] = "AUD1CTLA",
		[0xfd2e] = "AUD1CNT",     [0xfd2f] = "AUD1CTLB",
		[0xfd30] = "AUD2VOL",     [0xfd31] = "AUD2FEED",
		[0xfd32] = "AUD2OUT",     [0xfd33] = "AUD2SHIFT",
		[0xfd34] = "AUD2BKUP",    [0xfd35] = "AUD2CTLA",
		[0xfd36] = "AUD2CNT",     [0xfd37] = "AUD2CTLB",
		[0xfd38] = "AUD3VOL",     [0xfd39] = "AUD3FEED",
		[0xfd3a] = "AUD3OUT",     [0xfd3b] = "AUD3SHIFT",
		[0xfd3c] = "AUD3BKUP",    [0xfd3d] = "AUD3CTLA",
		[0xfd3e] = "AUD3CNT",     [0xfd3f] = "AUD3CTLB",

		// Audio control
		[0xfd40] = "MSTEREO",     [0xfd41] = "ATTENREG0",
		[0xfd42] = "ATTENREG1",   [0xfd43] = "ATTENREG2",
		[0xfd44] = "ATTENREG3",   [0xfd50] = "MPAN",

		// Interrupts
		[0xfd80] = "INTSET",      [0xfd81] = "INTRST",

		// System control
		[0xfd84] = "MAGRDY0",     [0xfd85] = "MAGRDY1",
		[0xfd86] = "AUDIN",       [0xfd87] = "SYSCTL1",
		[0xfd88] = "MIKEYHREV",   [0xfd89] = "MIKEYSREV",
		[0xfd8a] = "IODIR",       [0xfd8b] = "IODAT",
		[0xfd8c] = "SERCTL",      [0xfd8d] = "SERDAT",

		// Display control
		[0xfd90] = "SDONEACK",    [0xfd91] = "CPUSLEEP",
		[0xfd92] = "DISPCTL",     [0xfd93] = "PBKUP",
		[0xfd94] = "DISPADRL",    [0xfd95] = "DISPADRH",

		// Palette (Green 16 bytes, Blue/Red 16 bytes)
		[0xfda0] = "GREEN0",      [0xfda1] = "GREEN1",
		[0xfda2] = "GREEN2",      [0xfda3] = "GREEN3",
		[0xfda4] = "GREEN4",      [0xfda5] = "GREEN5",
		[0xfda6] = "GREEN6",      [0xfda7] = "GREEN7",
		[0xfda8] = "GREEN8",      [0xfda9] = "GREEN9",
		[0xfdaa] = "GREENA",      [0xfdab] = "GREENB",
		[0xfdac] = "GREENC",      [0xfdad] = "GREEND",
		[0xfdae] = "GREENE",      [0xfdaf] = "GREENF",
		[0xfdb0] = "BLUERED0",    [0xfdb1] = "BLUERED1",
		[0xfdb2] = "BLUERED2",    [0xfdb3] = "BLUERED3",
		[0xfdb4] = "BLUERED4",    [0xfdb5] = "BLUERED5",
		[0xfdb6] = "BLUERED6",    [0xfdb7] = "BLUERED7",
		[0xfdb8] = "BLUERED8",    [0xfdb9] = "BLUERED9",
		[0xfdba] = "BLUEREDA",    [0xfdbb] = "BLUEREDB",
		[0xfdbc] = "BLUEREDC",    [0xfdbd] = "BLUEREDD",
		[0xfdbe] = "BLUEREDE",    [0xfdbf] = "BLUEREDF",
	}.ToFrozenDictionary();

	/// <summary>
	/// Analyzes the ROM and detects its configuration.
	/// </summary>
	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		// Check for LNX header
		LnxHeader = LnxHeaderParser.Parse(rom);
		RomDataOffset = LnxHeaderParser.GetRomDataOffset(rom);

		var metadata = new Dictionary<string, string>();

		if (LnxHeader != null) {
			metadata["Format"] = "LNX";
			metadata["CartName"] = LnxHeader.CartName;
			metadata["Manufacturer"] = LnxHeader.Manufacturer;
			metadata["Rotation"] = LnxHeader.Rotation.ToString();
			metadata["Bank0Size"] = $"{LnxHeader.Bank0Size / 1024}KB";
			metadata["Bank1Size"] = LnxHeader.Bank1Size > 0 ? $"{LnxHeader.Bank1Size / 1024}KB" : "None";
			metadata["Version"] = LnxHeader.Version.ToString();
			BankCount = LnxHeader.BankCount > 0 ? LnxHeader.BankCount : 1;
		} else {
			metadata["Format"] = "Raw";
			BankCount = 1;
		}

		var romSize = LnxHeader?.RomSize ?? rom.Length;
		metadata["RomSize"] = $"{romSize / 1024}KB";

		return new RomInfo(
			Platform,
			rom.Length,
			null, // No mapper for Lynx
			metadata
		);
	}

	/// <summary>
	/// Gets the hardware register label for an address.
	/// </summary>
	public string? GetRegisterLabel(uint address) {
		// Suzy registers ($fc00-$fcff)
		if (SuzyRegisters.TryGetValue(address, out var suzyLabel)) {
			return suzyLabel;
		}

		// Mikey registers ($fd00-$fdff)
		if (MikeyRegisters.TryGetValue(address, out var mikeyLabel)) {
			return mikeyLabel;
		}

		return null;
	}

	/// <summary>
	/// Gets the memory region type for an address.
	/// </summary>
	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			// Zero page and stack
			< 0x0100 => MemoryRegion.Ram,
			< 0x0200 => MemoryRegion.Ram, // Stack

			// RAM
			< 0xfc00 => MemoryRegion.Ram,

			// Suzy hardware
			< 0xfd00 => MemoryRegion.Hardware,

			// Mikey hardware
			< 0xfe00 => MemoryRegion.Hardware,

			// Boot ROM
			_ => MemoryRegion.Rom
		};
	}

	/// <summary>
	/// Gets entry points (vectors) from the ROM.
	/// </summary>
	/// <remarks>
	/// The Lynx vectors are at $fff8-$ffff:
	/// - $fff8-$fff9: NMI vector
	/// - $fffa-$fffb: RESET vector
	/// - $fffc-$fffd: IRQ/BRK vector
	/// Note: These are in the Boot ROM area, so they point to boot code
	/// which then loads and jumps to the cartridge code.
	/// </remarks>
	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		// For Lynx, the entry point is typically at the start of ROM data
		// since the Boot ROM handles initial loading
		var offset = RomDataOffset;
		var entryPoints = new List<uint>();

		// The first bytes of the cartridge are loaded by the boot ROM
		// Actual entry point depends on the cartridge header structure
		// For now, assume $0200 as the typical load address
		entryPoints.Add(0x0200);

		// If we have ROM data, check for vectors at the end
		var romData = rom.Slice(offset);
		if (romData.Length >= 6) {
			// Some ROMs include vectors at the end
			var lastSix = romData.Slice(romData.Length - 6, 6);
			var nmiVector = (uint)(lastSix[0] | (lastSix[1] << 8));
			var resetVector = (uint)(lastSix[2] | (lastSix[3] << 8));
			var irqVector = (uint)(lastSix[4] | (lastSix[5] << 8));

			// Only add if they look like valid RAM addresses
			if (resetVector >= 0x0200 && resetVector < 0xfc00) {
				entryPoints.Add(resetVector);
			}
			if (nmiVector >= 0x0200 && nmiVector < 0xfc00 && !entryPoints.Contains(nmiVector)) {
				entryPoints.Add(nmiVector);
			}
			if (irqVector >= 0x0200 && irqVector < 0xfc00 && !entryPoints.Contains(irqVector)) {
				entryPoints.Add(irqVector);
			}
		}

		return [.. entryPoints];
	}

	/// <summary>
	/// Converts a CPU address to file offset.
	/// </summary>
	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	/// <summary>
	/// Converts a CPU address to file offset for a specific bank.
	/// </summary>
	public int AddressToOffset(uint address, int romLength, int bank) {
		// Lynx loads ROM into RAM, so this is a direct offset from load address
		// Typical load address is $0200
		if (address < 0x0200 || address >= 0xfc00) {
			return -1; // Not in ROM range
		}

		var offset = (int)(address - 0x0200) + RomDataOffset;

		// Add bank offset if multi-bank
		if (LnxHeader != null && bank > 0) {
			offset += LnxHeader.Bank0Size;
		}

		if (offset < 0 || offset >= romLength) {
			return -1;
		}

		return offset;
	}

	/// <summary>
	/// Converts a file offset to CPU address.
	/// </summary>
	public uint? OffsetToAddress(int offset) {
		if (offset < RomDataOffset) {
			return null; // In header
		}

		// Calculate address assuming $0200 base
		var address = (uint)(offset - RomDataOffset + 0x0200);

		if (address >= 0xfc00) {
			return null; // Beyond RAM
		}

		return address;
	}

	/// <summary>
	/// Checks if an address is in a switchable bank region.
	/// </summary>
	public bool IsInSwitchableRegion(uint address) {
		// Lynx doesn't use traditional bank switching
		// The entire ROM is loaded to RAM
		return false;
	}

	/// <summary>
	/// Detects bank switch calls.
	/// </summary>
	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// Lynx doesn't use traditional bank switching
		return null;
	}
}
