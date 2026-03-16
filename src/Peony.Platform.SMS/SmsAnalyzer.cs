namespace Peony.Platform.SMS;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Sega Master System / Game Gear platform analyzer.
/// </summary>
/// <remarks>
/// <para>
/// The SMS uses a Z80 CPU at ~3.58 MHz with up to 8KB RAM and 16KB VRAM.
/// ROMs are mapped into the 64KB address space with bank switching via
/// Sega mapper registers at $fffc-$ffff or CodeMasters mapper at $0000/$4000/$8000.
/// </para>
/// <para>
/// Memory map:
/// - $0000-$03FF: ROM bank 0 (first 1KB always fixed)
/// - $0400-$3FFF: ROM bank 0 (switchable page 0)
/// - $4000-$7FFF: ROM bank 1 (switchable page 1)
/// - $8000-$BFFF: ROM bank 2 (switchable page 2, or cartridge RAM)
/// - $C000-$DFFF: 8KB system RAM
/// - $E000-$FFFF: RAM mirror (with mapper regs at $FFFC-$FFFF)
/// </para>
/// </remarks>
public sealed class SmsAnalyzer : IPlatformAnalyzer {
	public string Platform => "Sega Master System";
	public ICpuDecoder CpuDecoder { get; } = new Z80Decoder();
	public int BankCount { get; private set; } = 1;
	public int RomDataOffset { get; private set; }

	private string _mapperType = "None";
	private const int BankSize = 0x4000; // 16KB banks

	// VDP / I/O ports
	private static readonly FrozenDictionary<uint, string> IoRegisters = new Dictionary<uint, string> {
		// Memory control
		[0x3e] = "MEMCTRL",
		[0x3f] = "IOCTRL",
		// VDP
		[0x7e] = "VCOUNTER",     // Read: V counter
		[0x7f] = "HCOUNTER",     // Read: H counter
		[0xbe] = "VDP_DATA",     // VDP data port
		[0xbf] = "VDP_CTRL",     // VDP control / status
		// Sound
		[0x7e] = "PSG",          // Write: PSG (SN76489)
		// I/O
		[0xdc] = "JOYPAD1",     // Controller port 1
		[0xdd] = "JOYPAD2",     // Controller port 2
		// Game Gear specific
		[0x00] = "GG_START",     // GG start button + region
		[0x01] = "GG_SERIAL1",
		[0x02] = "GG_SERIAL2",
		[0x03] = "GG_SERIAL3",
		[0x04] = "GG_SERIAL4",
		[0x05] = "GG_SERIAL5",
		[0x06] = "GG_STEREO",   // GG stereo control
	}.ToFrozenDictionary();

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		RomDataOffset = 0; // SMS ROMs have no header to strip
		var metadata = new Dictionary<string, string>();

		// Detect mapper type
		_mapperType = DetectMapper(rom);
		metadata["Mapper"] = _mapperType;

		// Calculate bank count
		BankCount = Math.Max(1, rom.Length / BankSize);
		metadata["Banks"] = BankCount.ToString();
		metadata["RomSize"] = $"{rom.Length / 1024}KB";

		// Read TMR SEGA header if present
		var headerOffset = FindSmsHeader(rom);
		if (headerOffset >= 0) {
			metadata["HeaderOffset"] = $"${headerOffset:x4}";

			// Read region/size byte
			if (headerOffset + 0x0f < rom.Length) {
				var regionByte = rom[headerOffset + 0x0f];
				var region = (regionByte >> 4) switch {
					3 => "SMS Japan",
					4 => "SMS Export",
					5 => "GG Japan",
					6 => "GG Export",
					7 => "GG International",
					_ => "Unknown"
				};
				metadata["Region"] = region;
			}
		}

		return new RomInfo(Platform, rom.Length, _mapperType, metadata);
	}

	public string? GetRegisterLabel(uint address) {
		// SMS uses I/O ports, not memory-mapped registers in the traditional sense
		// The mapper registers are memory-mapped though
		return address switch {
			0xfffc => "MAPPER_CTRL",   // RAM/ROM select
			0xfffd => "MAPPER_SLOT0",  // Page 0 bank ($0000-$3FFF)
			0xfffe => "MAPPER_SLOT1",  // Page 1 bank ($4000-$7FFF)
			0xffff => "MAPPER_SLOT2",  // Page 2 bank ($8000-$BFFF)
			_ => null
		};
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			< 0xc000 => MemoryRegion.Rom,          // ROM area
			< 0xe000 => MemoryRegion.Ram,           // 8KB system RAM
			< 0xfffc => MemoryRegion.Ram,           // RAM mirror
			_ => MemoryRegion.Hardware               // Mapper registers
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		var entryPoints = new List<uint>();

		// Z80 reset vector starts at $0000
		entryPoints.Add(0x0000);

		// Standard interrupt vectors
		entryPoints.Add(0x0038); // Maskable interrupt (IM 1)
		entryPoints.Add(0x0066); // NMI (pause button)

		return [.. entryPoints];
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		if (address >= 0xc000) return -1; // RAM area

		// First 1KB is always bank 0
		if (address < 0x0400) return (int)address;

		// Page 0: $0000-$3FFF
		if (address < 0x4000) {
			var offset = bank * BankSize + (int)address;
			return offset < romLength ? offset : (int)address;
		}

		// Page 1: $4000-$7FFF
		if (address < 0x8000) {
			var baseOffset = bank * BankSize;
			var offset = baseOffset + (int)(address - 0x4000);
			return offset < romLength ? offset : -1;
		}

		// Page 2: $8000-$BFFF
		if (address < 0xc000) {
			var baseOffset = bank * BankSize;
			var offset = baseOffset + (int)(address - 0x8000);
			return offset < romLength ? offset : -1;
		}

		return -1;
	}

	public uint? OffsetToAddress(int offset) {
		if (offset < 0) return null;
		// First bank maps to $0000-$3FFF
		if (offset < BankSize) return (uint)offset;
		// Other banks - return address in page 1 ($4000-$7FFF)
		var bankOffset = offset % BankSize;
		return (uint)(0x4000 + bankOffset);
	}

	public bool IsInSwitchableRegion(uint address) {
		// First 1KB is always fixed, rest is switchable
		return address >= 0x0400 && address < 0xc000;
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// Look for writes to mapper registers
		// ld a, <bank>; ld ($ffff), a pattern
		var offset = AddressToOffset(address, rom.Length, currentBank);
		if (offset < 0 || offset + 4 >= rom.Length) return null;

		// ld a, imm8 = 0x3E nn
		if (rom[offset] == 0x3e && offset + 3 < rom.Length) {
			var bankNum = rom[offset + 1];
			// ld ($fffe), a = 0x32 0xFE 0xFF or ld ($ffff), a = 0x32 0xFF 0xFF
			if (rom[offset + 2] == 0x32) {
				var addrLo = rom[offset + 3];
				if (offset + 4 < rom.Length) {
					var addrHi = rom[offset + 4];
					if (addrHi == 0xff && (addrLo == 0xfd || addrLo == 0xfe || addrLo == 0xff)) {
						return new BankSwitchInfo(bankNum, address + 5, null);
					}
				}
			}
		}

		return null;
	}

	private static string DetectMapper(ReadOnlySpan<byte> rom) {
		if (rom.Length <= BankSize) return "None";

		// Check for CodeMasters mapper (writes to $0000, $4000, $8000)
		// CodeMasters games typically have specific header patterns
		if (rom.Length >= 0x4000 && rom[0x3fff] != 0 && rom[0x7fff] != 0) {
			// Heuristic: CodeMasters ROMs don't have TMR SEGA header at $7ff0
			if (FindSmsHeaderAt(rom, 0x7ff0) < 0) return "CodeMasters";
		}

		// Default: Sega mapper
		return "Sega";
	}

	private static int FindSmsHeader(ReadOnlySpan<byte> rom) {
		// TMR SEGA header can be at $1ff0, $3ff0, or $7ff0
		int[] offsets = [0x7ff0, 0x3ff0, 0x1ff0];
		foreach (var off in offsets) {
			if (FindSmsHeaderAt(rom, off) >= 0)
				return off;
		}
		return -1;
	}

	private static int FindSmsHeaderAt(ReadOnlySpan<byte> rom, int offset) {
		if (offset + 8 > rom.Length) return -1;
		// "TMR SEGA" signature
		ReadOnlySpan<byte> sig = "TMR SEGA"u8;
		return rom.Slice(offset, 8).SequenceEqual(sig) ? offset : -1;
	}
}
