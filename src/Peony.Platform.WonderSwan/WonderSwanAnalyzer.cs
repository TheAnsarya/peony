namespace Peony.Platform.WonderSwan;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Bandai WonderSwan / WonderSwan Color platform analyzer.
/// </summary>
/// <remarks>
/// <para>
/// The WonderSwan uses a NEC V30MZ CPU (80186-compatible) at 3.072 MHz with:
/// - 16KB internal RAM (WS) / 64KB (WSC)
/// - 64KB VRAM (WS: accessed through I/O ports)
/// - Cartridge ROM mapped into segments, up to 16MB
/// </para>
/// <para>
/// Memory map (real mode 20-bit / 1MB):
/// - $00000-$03FFF: Internal RAM (16KB WS, 64KB WSC at $00000-$0FFFF)
/// - $04000-$0FFFF: Extended RAM (WSC only)
/// - $10000-$1FFFF: SRAM (cartridge save RAM, if present)
/// - $20000-$2FFFF: ROM bank (switchable via I/O port $C0-$C3)
/// - $30000-$3FFFF: ROM bank (switchable)
/// - $40000-$FFFFF: Linear ROM access (upper 768KB fixed)
/// </para>
/// <para>
/// The last segment ($F0000-$FFFFF) contains the reset vector at $FFFF0.
/// The 6502-style vectors (FFFFx) are NOT used; instead the CPU starts
/// at segment $FFFF, offset $0000 (physical $FFFF0) in real mode.
/// </para>
/// </remarks>
public sealed class WonderSwanAnalyzer : IPlatformAnalyzer {
	public string Platform => "WonderSwan";
	public ICpuDecoder CpuDecoder { get; } = new V30MZDecoder();
	public int BankCount { get; private set; } = 1;
	public int RomDataOffset { get; private set; }

	private bool _isColor;
	private const int BankSize = 0x10000; // 64KB banks

	// I/O port registers ($00-$FF)
	private static readonly FrozenDictionary<uint, string> IoRegisters = new Dictionary<uint, string> {
		// Display control
		[0x00] = "DISP_CTRL",       // Display control
		[0x01] = "BACK_COLOR",      // Background color
		[0x02] = "LINE_CUR",        // Current scanline
		[0x03] = "LINE_CMP",        // Line compare (for interrupt)
		[0x04] = "SPR_BASE",        // Sprite table base
		[0x05] = "SPR_START",       // First sprite index
		[0x06] = "SPR_COUNT",       // Sprite count
		[0x07] = "MAP_BASE",        // Scroll map bases
		[0x08] = "SCR2_WIN_X1",     // Scroll layer 2 window X start
		[0x09] = "SCR2_WIN_Y1",     // Scroll layer 2 window Y start
		[0x0a] = "SCR2_WIN_X2",     // Scroll layer 2 window X end
		[0x0b] = "SCR2_WIN_Y2",     // Scroll layer 2 window Y end
		[0x0c] = "SPR_WIN_X1",      // Sprite window X start
		[0x0d] = "SPR_WIN_Y1",      // Sprite window Y start
		[0x0e] = "SPR_WIN_X2",      // Sprite window X end
		[0x0f] = "SPR_WIN_Y2",      // Sprite window Y end

		// Scroll registers
		[0x10] = "SCR1_X",          // Scroll layer 1 X offset
		[0x11] = "SCR1_Y",          // Scroll layer 1 Y offset
		[0x12] = "SCR2_X",          // Scroll layer 2 X offset
		[0x13] = "SCR2_Y",          // Scroll layer 2 Y offset

		// LCD control
		[0x14] = "LCD_CTRL",        // LCD control
		[0x15] = "LCD_ICON",        // LCD icons (volume, battery, etc.)

		// Palette
		[0x1c] = "PAL_MONO_0_1",    // Monochrome palettes (WS)
		[0x1d] = "PAL_MONO_2_3",
		[0x1e] = "PAL_MONO_4_5",
		[0x1f] = "PAL_MONO_6_7",

		// DMA
		[0x40] = "DMA_SRC_L",       // DMA source address low
		[0x41] = "DMA_SRC_M",       // DMA source address mid
		[0x42] = "DMA_SRC_H",       // DMA source address high
		[0x44] = "DMA_DST_L",       // DMA destination low
		[0x45] = "DMA_DST_H",       // DMA destination high
		[0x46] = "DMA_LEN_L",       // DMA length low
		[0x47] = "DMA_LEN_H",       // DMA length high
		[0x48] = "DMA_CTRL",        // DMA control

		// Sound
		[0x80] = "SND_CH1_PITCH_L", // Sound channel 1 pitch low
		[0x81] = "SND_CH1_PITCH_H", // Sound channel 1 pitch high
		[0x82] = "SND_CH2_PITCH_L",
		[0x83] = "SND_CH2_PITCH_H",
		[0x84] = "SND_CH3_PITCH_L",
		[0x85] = "SND_CH3_PITCH_H",
		[0x86] = "SND_CH4_PITCH_L",
		[0x87] = "SND_CH4_PITCH_H",
		[0x88] = "SND_CH1_VOL",     // Channel 1 volume
		[0x89] = "SND_CH2_VOL",
		[0x8a] = "SND_CH3_VOL",
		[0x8b] = "SND_CH4_VOL",
		[0x8c] = "SND_SWEEP",       // Sweep value
		[0x8d] = "SND_SWEEP_TIME",  // Sweep time
		[0x8e] = "SND_NOISE",       // Noise control
		[0x8f] = "SND_WAVE_BASE",   // Wave data base address
		[0x90] = "SND_CTRL",        // Sound control
		[0x91] = "SND_OUTPUT",      // Sound output
		[0x92] = "SND_RANDOM_L",    // Noise LFSR low
		[0x93] = "SND_RANDOM_H",    // Noise LFSR high
		[0x94] = "SND_VOICE_CTRL",  // Voice control (WS only: HyperVoice)

		// System control
		[0xa0] = "HWTYPE",          // Hardware type / color mode
		[0xa2] = "TIMER_CTRL",      // Timer control
		[0xa4] = "HTIMER_FREQ_L",   // H-blank timer frequency low
		[0xa5] = "HTIMER_FREQ_H",   // H-blank timer frequency high
		[0xa6] = "VTIMER_FREQ_L",   // V-blank timer frequency low
		[0xa7] = "VTIMER_FREQ_H",   // V-blank timer frequency high
		[0xa8] = "HTIMER_CNT_L",    // H-blank timer counter low
		[0xa9] = "HTIMER_CNT_H",
		[0xaa] = "VTIMER_CNT_L",    // V-blank timer counter low
		[0xab] = "VTIMER_CNT_H",

		// Interrupt control
		[0xb0] = "INT_BASE",        // Interrupt vector base
		[0xb1] = "SER_DATA",        // Serial data
		[0xb2] = "INT_ENABLE",      // Interrupt enable
		[0xb3] = "SER_STATUS",      // Serial status
		[0xb4] = "INT_STATUS",      // Interrupt status
		[0xb5] = "KEYPAD",          // Keypad input
		[0xb6] = "INT_ACK",         // Interrupt acknowledge
		[0xb7] = "NMI_CTRL",        // NMI control

		// Bank switching / cartridge
		[0xc0] = "BANK_ROM2",       // ROM bank for $20000-$2FFFF
		[0xc1] = "BANK_SRAM",       // SRAM bank
		[0xc2] = "BANK_ROM0",       // ROM bank for $30000-$3FFFF
		[0xc3] = "BANK_ROM1",       // ROM bank for $40000-$FFFFF linear base

		// WSC-specific
		[0xfe] = "WSC_SYSTEM",      // WSC system control
	}.ToFrozenDictionary();

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		RomDataOffset = 0;
		var metadata = new Dictionary<string, string>();

		// WonderSwan ROM footer is in the last 16 bytes
		if (rom.Length >= 16) {
			var footer = rom.Slice(rom.Length - 16, 16);

			// Read publisher ID
			metadata["PublisherID"] = $"${footer[0]:x2}";

			// Color flag
			_isColor = footer[1] != 0;
			metadata["Color"] = _isColor ? "Yes" : "No";

			// Game ID
			metadata["GameID"] = $"${footer[2]:x2}";

			// Game version
			metadata["Version"] = $"${footer[3]:x2}";

			// ROM size byte
			var romSizeByte = footer[4];
			metadata["RomSizeCode"] = $"${romSizeByte:x2}";

			// SRAM size
			var sramByte = footer[5];
			metadata["SramSize"] = sramByte switch {
				0x00 => "None",
				0x01 => "64Kbit (8KB)",
				0x02 => "256Kbit (32KB)",
				0x03 => "1Mbit (128KB)",
				0x04 => "2Mbit (256KB)",
				0x05 => "4Mbit (512KB)",
				_ => $"Unknown (${sramByte:x2})"
			};

			// Orientation
			var flags = footer[6];
			metadata["Orientation"] = (flags & 1) != 0 ? "Vertical" : "Horizontal";
			metadata["RTC"] = (flags & 2) != 0 ? "Yes" : "No";
		}

		BankCount = Math.Max(1, rom.Length / 0x10000);
		metadata["Banks"] = BankCount.ToString();
		metadata["RomSize"] = $"{rom.Length / 1024}KB";
		metadata["Platform"] = _isColor ? "WonderSwan Color" : "WonderSwan";

		return new RomInfo(Platform, rom.Length, null, metadata);
	}

	public string? GetRegisterLabel(uint address) {
		// WonderSwan uses I/O ports for registers, not memory-mapped
		// This would be used if addresses map to I/O space
		return IoRegisters.GetValueOrDefault(address);
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		// 20-bit address space (1MB)
		return address switch {
			< 0x4000 => MemoryRegion.Ram,        // Internal RAM (16KB WS)
			< 0x10000 => MemoryRegion.Ram,        // Extended RAM (WSC: 64KB)
			< 0x20000 => MemoryRegion.Ram,        // SRAM
			< 0x30000 => MemoryRegion.Rom,        // Switchable ROM bank
			< 0x40000 => MemoryRegion.Rom,        // Switchable ROM bank
			< 0x100000 => MemoryRegion.Rom,       // Linear ROM
			_ => MemoryRegion.Rom
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		var entryPoints = new List<uint>();

		// V30MZ starts at $FFFF0 (segment $FFFF, offset $0000)
		// In a WonderSwan, this maps to the last 16 bytes of ROM
		// The reset vector is typically a JMP instruction at ROM end - 16 bytes
		entryPoints.Add(0xffff0);

		// WonderSwan interrupt vectors are set via INT_BASE I/O port
		// Typical interrupt base is at segment 0, so vectors are in RAM
		// We can't statically determine them without tracing

		return [.. entryPoints];
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// For linear ROM area ($40000-$FFFFF), map to end of ROM
		if (address >= 0x40000 && address < 0x100000) {
			// The ROM is mapped at the END of the 1MB space
			// So $FFFFF maps to the last byte of ROM
			var offset = romLength - (0x100000 - (int)address);
			return offset >= 0 && offset < romLength ? offset : -1;
		}

		// Switchable banks ($20000-$3FFFF) use bank registers
		if (address >= 0x20000 && address < 0x40000) {
			var bankOffset = (int)(address - 0x20000);
			var offset = bank * 0x10000 + bankOffset;
			return offset >= 0 && offset < romLength ? offset : -1;
		}

		return -1; // RAM area
	}

	public uint? OffsetToAddress(int offset) {
		if (offset < 0) return null;
		// Map ROM offset to linear ROM area
		// Last byte of ROM = $FFFFF
		return null; // Need ROM length to compute this
	}

	public bool IsInSwitchableRegion(uint address) {
		return address >= 0x20000 && address < 0x40000;
	}

	public bool IsValidAddress(uint address) {
		return true;
	}

	public int GetTargetBank(uint target, int currentBank) {
		return currentBank;
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// Look for OUT instructions to ports $C0-$C3
		var offset = AddressToOffset(address, rom.Length, currentBank);
		if (offset < 0 || offset + 3 >= rom.Length) return null;

		// mov al, imm8 (0xB0 nn) followed by out port, al (0xE6 nn)
		if (rom[offset] == 0xb0 && offset + 3 < rom.Length && rom[offset + 2] == 0xe6) {
			var bankNum = rom[offset + 1];
			var port = rom[offset + 3];
			if (port is 0xc0 or 0xc1 or 0xc2 or 0xc3) {
				return new BankSwitchInfo(bankNum, address + 4, null);
			}
		}

		return null;
	}
}
