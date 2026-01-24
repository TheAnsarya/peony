namespace Peony.Platform.GBA;

using Peony.Core;
using Peony.Cpu.ARM7TDMI;

/// <summary>
/// Game Boy Advance platform analyzer
/// </summary>
public class GbaAnalyzer : IPlatformAnalyzer {
	public string Platform => "Game Boy Advance";
	public ICpuDecoder CpuDecoder { get; } = new Arm7TdmiDecoder();
	public int BankCount => 1; // GBA doesn't use traditional banking

	// Hardware registers
	private static readonly Dictionary<uint, string> HardwareRegisters = new() {
		// LCD I/O
		[0x04000000] = "DISPCNT",
		[0x04000002] = "DISPSTAT",
		[0x04000004] = "VCOUNT",
		[0x04000008] = "BG0CNT",
		[0x0400000a] = "BG1CNT",
		[0x0400000c] = "BG2CNT",
		[0x0400000e] = "BG3CNT",
		[0x04000010] = "BG0HOFS",
		[0x04000012] = "BG0VOFS",
		[0x04000048] = "WININ",
		[0x0400004a] = "WINOUT",
		[0x04000050] = "BLDCNT",
		[0x04000052] = "BLDALPHA",
		[0x04000054] = "BLDY",

		// Sound
		[0x04000060] = "SOUND1CNT_L",
		[0x04000062] = "SOUND1CNT_H",
		[0x04000064] = "SOUND1CNT_X",
		[0x04000068] = "SOUND2CNT_L",
		[0x0400006c] = "SOUND2CNT_H",
		[0x04000070] = "SOUND3CNT_L",
		[0x04000072] = "SOUND3CNT_H",
		[0x04000074] = "SOUND3CNT_X",
		[0x04000078] = "SOUND4CNT_L",
		[0x0400007c] = "SOUND4CNT_H",
		[0x04000080] = "SOUNDCNT_L",
		[0x04000082] = "SOUNDCNT_H",
		[0x04000084] = "SOUNDCNT_X",
		[0x04000088] = "SOUNDBIAS",

		// DMA
		[0x040000b0] = "DMA0SAD",
		[0x040000b4] = "DMA0DAD",
		[0x040000b8] = "DMA0CNT_L",
		[0x040000ba] = "DMA0CNT_H",
		[0x040000bc] = "DMA1SAD",
		[0x040000c0] = "DMA1DAD",
		[0x040000c4] = "DMA1CNT_L",
		[0x040000c6] = "DMA1CNT_H",
		[0x040000c8] = "DMA2SAD",
		[0x040000cc] = "DMA2DAD",
		[0x040000d0] = "DMA2CNT_L",
		[0x040000d2] = "DMA2CNT_H",
		[0x040000d4] = "DMA3SAD",
		[0x040000d8] = "DMA3DAD",
		[0x040000dc] = "DMA3CNT_L",
		[0x040000de] = "DMA3CNT_H",

		// Timers
		[0x04000100] = "TM0CNT_L",
		[0x04000102] = "TM0CNT_H",
		[0x04000104] = "TM1CNT_L",
		[0x04000106] = "TM1CNT_H",
		[0x04000108] = "TM2CNT_L",
		[0x0400010a] = "TM2CNT_H",
		[0x0400010c] = "TM3CNT_L",
		[0x0400010e] = "TM3CNT_H",

		// Serial
		[0x04000120] = "SIODATA32",
		[0x04000128] = "SIOCNT",
		[0x0400012a] = "SIODATA8",

		// Keypad
		[0x04000130] = "KEYINPUT",
		[0x04000132] = "KEYCNT",

		// Interrupts
		[0x04000200] = "IE",
		[0x04000202] = "IF",
		[0x04000204] = "WAITCNT",
		[0x04000208] = "IME",
		[0x04000300] = "POSTFLG",
		[0x04000301] = "HALTCNT",
	};

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		if (rom.Length < 0xc0)
			return new RomInfo(Platform, rom.Length, null, new Dictionary<string, string>());

		// GBA ROM header at 0x00-0xbf
		var entryPoint = rom[0..4];
		var logo = rom[0x04..0xa0]; // Nintendo logo
		var title = GetTitle(rom[0xa0..0xac]);
		var gameCode = GetGameCode(rom[0xac..0xb0]);
		var makerCode = GetMakerCode(rom[0xb0..0xb2]);
		var version = rom[0xbc];

		return new RomInfo(
			Platform,
			rom.Length,
			null, // No mapper
			new Dictionary<string, string> {
				["Title"] = title,
				["GameCode"] = gameCode,
				["MakerCode"] = makerCode,
				["Version"] = $"1.{version}",
				["RomSize"] = FormatSize(rom.Length)
			}
		);
	}

	public string? GetRegisterLabel(uint address) {
		if (HardwareRegisters.TryGetValue(address, out var label))
			return label;
		return null;
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			< 0x02000000 => MemoryRegion.Rom,        // BIOS
			< 0x03000000 => MemoryRegion.Ram,        // EWRAM (256KB)
			< 0x04000000 => MemoryRegion.Ram,        // IWRAM (32KB)
			< 0x05000000 => MemoryRegion.Hardware,   // I/O Registers
			< 0x06000000 => MemoryRegion.Graphics,   // Palette RAM
			< 0x07000000 => MemoryRegion.Graphics,   // VRAM
			< 0x08000000 => MemoryRegion.Graphics,   // OAM
			< 0x0e000000 => MemoryRegion.Rom,        // Game Pak ROM (Wait State 0-2)
			< 0x10000000 => MemoryRegion.Rom,        // Game Pak SRAM/Flash
			_ => MemoryRegion.Unknown
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		if (rom.Length < 4)
			return [0x08000000];

		// Entry point is at ROM start
		// ARM/Thumb mode determined by bit 0 of PC
		var entry = (uint)(rom[0] | (rom[1] << 8) | (rom[2] << 16) | (rom[3] << 24));

		// Typical GBA entry point is a branch instruction
		// b 0x080000c0 (or similar)
		if ((entry & 0xff000000) == 0xea000000) {
			var offset = (int)(entry & 0x00ffffff);
			if ((offset & 0x00800000) != 0) offset |= unchecked((int)0xff000000);
			var target = (uint)(8 + (offset << 2));
			return [0x08000000, 0x08000000 + target];
		}

		return [0x08000000];
	}

	public bool IsInSwitchableRegion(uint address) {
		// GBA doesn't use traditional banking
		return false;
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, -1);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// GBA ROM is mapped at 0x08000000
		if (address >= 0x08000000 && address < 0x0e000000) {
			var offset = (int)(address - 0x08000000);
			// Handle mirroring (ROM repeats every 32MB)
			offset &= 0x01ffffff;
			return offset < romLength ? offset : -1;
		}
		return -1;
	}

	public uint? OffsetToAddress(int offset) {
		// GBA: ROM at $08000000+
		return (uint)(0x08000000 + offset);
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// GBA doesn't use bank switching
		return null;
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

	private static string GetGameCode(ReadOnlySpan<byte> codeBytes) {
		var chars = new List<char>();
		foreach (var b in codeBytes) {
			if (b >= 0x20 && b <= 0x7e)
				chars.Add((char)b);
		}
		return new string(chars.ToArray());
	}

	private static string GetMakerCode(ReadOnlySpan<byte> makerBytes) {
		var chars = new List<char>();
		foreach (var b in makerBytes) {
			if (b >= 0x20 && b <= 0x7e)
				chars.Add((char)b);
		}
		return new string(chars.ToArray());
	}

	private static string FormatSize(int bytes) {
		if (bytes < 1024) return $"{bytes} B";
		if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
		return $"{bytes / (1024 * 1024)} MB";
	}
}
