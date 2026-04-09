namespace Peony.Platform.Genesis;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Sega Genesis / Mega Drive platform analyzer.
/// </summary>
/// <remarks>
/// <para>
/// The Genesis uses a Motorola 68000 CPU at 7.67 MHz as the main processor,
/// with a Z80 at 3.58 MHz for sound. The main CPU has a 24-bit (16MB) address space.
/// </para>
/// <para>
/// Memory map (68000):
/// - $000000-$3FFFFF: Cartridge ROM (4MB max, with bank switching for larger)
/// - $400000-$7FFFFF: Reserved (expansion, Sega CD, 32X)
/// - $A00000-$A0FFFF: Z80 memory space (accessible when bus granted)
/// - $A10000-$A1001F: I/O area (controllers, memory mode, etc.)
/// - $A11100-$A11101: Z80 bus request
/// - $A11200-$A11201: Z80 reset
/// - $C00000-$C00003: VDP data port
/// - $C00004-$C00007: VDP control port
/// - $C00008-$C0000F: VDP HV counter
/// - $C00011: PSG (SN76489)
/// - $E00000-$FFFFFF: 64KB work RAM (mirrored)
/// </para>
/// <para>
/// ROM header at $000100-$0001FF contains game title, serial number, checksum, etc.
/// Vectors at $000000-$0000FF: 68000 exception vector table.
/// </para>
/// </remarks>
public sealed class GenesisAnalyzer : IPlatformAnalyzer {
	public string Platform => "Sega Genesis";
	public ICpuDecoder CpuDecoder { get; } = new M68000Decoder();
	public int BankCount { get; private set; } = 1;
	public int RomDataOffset { get; private set; }

	private const int BankSize = 0x80000; // 512KB ROM banks for SSFII mapper

	// Hardware registers (memory-mapped I/O)
	private static readonly FrozenDictionary<uint, string> HardwareRegisters = new Dictionary<uint, string> {
		// I/O registers
		[0xa10001] = "VERSION",       // Hardware version
		[0xa10003] = "CTRL1_DATA",    // Controller 1 data
		[0xa10005] = "CTRL2_DATA",    // Controller 2 data
		[0xa10007] = "EXP_DATA",      // Expansion port data
		[0xa10009] = "CTRL1_CTRL",    // Controller 1 control
		[0xa1000b] = "CTRL2_CTRL",    // Controller 2 control
		[0xa1000d] = "EXP_CTRL",      // Expansion port control
		[0xa1000f] = "CTRL1_TX",      // Controller 1 serial TX
		[0xa10011] = "CTRL1_RX",      // Controller 1 serial RX
		[0xa10013] = "CTRL1_SCTRL",   // Controller 1 serial control
		[0xa10015] = "CTRL2_TX",      // Controller 2 serial TX
		[0xa10017] = "CTRL2_RX",      // Controller 2 serial RX
		[0xa10019] = "CTRL2_SCTRL",   // Controller 2 serial control
		[0xa1001b] = "EXP_TX",        // Expansion serial TX
		[0xa1001d] = "EXP_RX",        // Expansion serial RX
		[0xa1001f] = "EXP_SCTRL",     // Expansion serial control

		// Memory mode / TMSS
		[0xa14000] = "TMSS_SEGA",     // TMSS "SEGA" register

		// Z80 control
		[0xa11100] = "Z80_BUSREQ",    // Z80 bus request
		[0xa11200] = "Z80_RESET",     // Z80 reset

		// VDP
		[0xc00000] = "VDP_DATA",      // VDP data port
		[0xc00002] = "VDP_DATA_M",    // VDP data port (mirror)
		[0xc00004] = "VDP_CTRL",      // VDP control port
		[0xc00006] = "VDP_CTRL_M",    // VDP control port (mirror)
		[0xc00008] = "VDP_HVCNT",     // VDP HV counter
		[0xc0000a] = "VDP_HVCNT_M",   // VDP HV counter (mirror)
		[0xc00011] = "PSG",           // PSG (SN76489)
	}.ToFrozenDictionary();

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		RomDataOffset = 0;
		var metadata = new Dictionary<string, string>();

		// Read ROM header at $100-$1FF
		if (rom.Length >= 0x200) {
			// Console name at $100-$10F
			var consoleName = ReadAscii(rom, 0x100, 16).Trim();
			metadata["Console"] = consoleName;

			// Domestic name at $120-$14F
			var domesticName = ReadAscii(rom, 0x120, 48).Trim();
			if (!string.IsNullOrWhiteSpace(domesticName))
				metadata["DomesticName"] = domesticName;

			// Overseas name at $150-$17F
			var overseasName = ReadAscii(rom, 0x150, 48).Trim();
			if (!string.IsNullOrWhiteSpace(overseasName))
				metadata["OverseasName"] = overseasName;

			// Serial number at $180-$18D
			var serial = ReadAscii(rom, 0x180, 14).Trim();
			if (!string.IsNullOrWhiteSpace(serial))
				metadata["Serial"] = serial;

			// Checksum at $18E-$18F
			if (rom.Length >= 0x190) {
				var checksum = (rom[0x18e] << 8) | rom[0x18f];
				metadata["Checksum"] = $"${checksum:x4}";
			}

			// ROM start/end at $1A0-$1A7
			if (rom.Length >= 0x1a8) {
				var romStart = ReadLong(rom, 0x1a0);
				var romEnd = ReadLong(rom, 0x1a4);
				metadata["RomRange"] = $"${romStart:x6}-${romEnd:x6}";
			}

			// RAM start/end at $1A8-$1AF
			if (rom.Length >= 0x1b0) {
				var ramStart = ReadLong(rom, 0x1a8);
				var ramEnd = ReadLong(rom, 0x1ac);
				metadata["RamRange"] = $"${ramStart:x6}-${ramEnd:x6}";
			}

			// SRAM info at $1B0-$1B3
			if (rom.Length >= 0x1b4) {
				var sramInfo = ReadAscii(rom, 0x1b0, 4);
				metadata["SramInfo"] = sramInfo;
			}

			// Region codes at $1F0-$1FF
			if (rom.Length >= 0x200) {
				var regionStr = ReadAscii(rom, 0x1f0, 16).Trim();
				metadata["Region"] = regionStr;
			}
		}

		// Check for "SEGA" at $100
		var hasMdHeader = rom.Length >= 0x105 &&
			((rom[0x100] == 'S' && rom[0x101] == 'E' && rom[0x102] == 'G' && rom[0x103] == 'A') ||
			 (rom[0x100] == ' ' && rom[0x101] == 'S' && rom[0x102] == 'E' && rom[0x103] == 'G'));

		metadata["ValidHeader"] = hasMdHeader ? "Yes" : "No";
		metadata["RomSize"] = $"{rom.Length / 1024}KB";

		// Detect mapper (e.g., SSFII mapper for >4MB ROMs)
		var mapper = rom.Length > 0x400000 ? "SSFII" : "None";
		metadata["Mapper"] = mapper;

		BankCount = Math.Max(1, rom.Length / 0x80000); // 512KB banks
		metadata["Banks"] = BankCount.ToString();

		return new RomInfo(Platform, rom.Length, mapper, metadata);
	}

	public string? GetRegisterLabel(uint address) {
		return HardwareRegisters.GetValueOrDefault(address);
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			< 0x400000 => MemoryRegion.Rom,        // Cartridge ROM
			< 0xa00000 => MemoryRegion.Rom,        // Expansion / CD / 32X
			< 0xa10000 => MemoryRegion.Ram,        // Z80 address space
			< 0xa10020 => MemoryRegion.Hardware,   // I/O registers
			< 0xa12000 => MemoryRegion.Hardware,   // Z80 control + expansion
			< 0xc00000 => MemoryRegion.Hardware,   // Reserved / expansion
			< 0xc00020 => MemoryRegion.Hardware,   // VDP
			< 0xe00000 => MemoryRegion.Hardware,   // VDP mirrors / reserved
			_ => MemoryRegion.Ram                   // 64KB work RAM (mirrored)
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		var entryPoints = new List<uint>();

		// 68000 vector table at $000000-$0000FF
		if (rom.Length >= 8) {
			// Vector 0: Initial SSP (stack pointer)
			// Vector 1: Initial PC (entry point) at $000004
			var initialPC = (uint)ReadLong(rom, 4);
			entryPoints.Add(initialPC);

			// Read common exception vectors
			uint[] vectorOffsets = [
				0x08,  // Bus error
				0x0c,  // Address error
				0x10,  // Illegal instruction
				0x14,  // Division by zero
				0x18,  // CHK instruction
				0x1c,  // TRAPV
				0x20,  // Privilege violation
				0x24,  // Trace
				0x60,  // Spurious interrupt
				0x64,  // Level 1 interrupt (external)
				0x68,  // Level 2 interrupt (external)
				0x6c,  // Level 3 interrupt
				0x70,  // Level 4 interrupt (H-blank)
				0x74,  // Level 5 interrupt
				0x78,  // Level 6 interrupt (V-blank)
				0x7c,  // Level 7 interrupt (NMI)
			];

			foreach (var vecOff in vectorOffsets) {
				if (vecOff + 4 <= rom.Length) {
					var vecAddr = (uint)ReadLong(rom, (int)vecOff);
					if (vecAddr > 0 && vecAddr < 0x400000 && !entryPoints.Contains(vecAddr))
						entryPoints.Add(vecAddr);
				}
			}
		}

		return [.. entryPoints];
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// ROM is at $000000-$3FFFFF
		if (address < 0x400000) {
			var offset = (int)address;
			return offset < romLength ? offset : -1;
		}
		return -1;
	}

	public uint? OffsetToAddress(int offset) {
		if (offset < 0) return null;
		// Direct mapping: ROM offset = CPU address
		return (uint)offset;
	}

	public bool IsInSwitchableRegion(uint address) {
		// With SSFII mapper, banks above $080000 are switchable
		// With standard mapping, nothing is switchable
		return false;
	}

	public bool IsValidAddress(uint address) {
		return true;
	}

	public int GetTargetBank(uint target, int currentBank) {
		return currentBank;
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// SSFII mapper uses writes to $A130F1-$A130FF
		// Most Genesis games don't use bank switching
		return null;
	}

	private static int ReadLong(ReadOnlySpan<byte> data, int offset) {
		if (offset + 3 >= data.Length) return 0;
		return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
	}

	private static string ReadAscii(ReadOnlySpan<byte> data, int offset, int length) {
		if (offset + length > data.Length) return "";
		var chars = new char[length];
		for (var i = 0; i < length; i++) {
			var b = data[offset + i];
			chars[i] = b >= 0x20 && b < 0x7f ? (char)b : ' ';
		}
		return new string(chars);
	}
}
