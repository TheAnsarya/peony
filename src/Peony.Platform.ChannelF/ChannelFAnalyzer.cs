namespace Peony.Platform.ChannelF;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Fairchild Channel F platform analyzer.
/// Memory map: BIOS ROM ($0000-$07FF), Cartridge ROM ($0800-$17FF),
/// RAM ($2800-$2FFF), VRAM ($3000-$37FF), I/O ($3800-$38FF).
/// </summary>
public sealed class ChannelFAnalyzer : IPlatformAnalyzer {
	public string Platform => "Channel F";
	public ICpuDecoder CpuDecoder { get; } = new CpuF8Decoder();
	public int BankCount => 1; // Standard carts have no banking
	public int RomDataOffset => 0; // Channel F ROMs have no file header

	// I/O Port registers
	private static readonly FrozenDictionary<uint, string> IoRegisters = new Dictionary<uint, string> {
		[0x00] = "PORT0_CONSOLE",   // Console buttons and controller hand-off
		[0x01] = "PORT1_RIGHT",     // Right controller input
		[0x04] = "PORT4_LEFT",      // Left controller input
		[0x05] = "PORT5_SOUND",     // Buzzer / sound output
	}.ToFrozenDictionary();

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		var metadata = new Dictionary<string, string> {
			["System"] = "Fairchild Channel F",
			["CPU"] = "Fairchild F8 @ 1.7898 MHz",
		};

		// Identify ROM type based on size
		if (rom.Length <= 0x0400) {
			metadata["RomType"] = "BIOS ROM 1";
		} else if (rom.Length < 0x0800) {
			metadata["RomType"] = "BIOS ROMs";
		} else if (rom.Length <= 0x1000) {
			metadata["RomType"] = "2K Cartridge";
		} else if (rom.Length <= 0x1800) {
			metadata["RomType"] = "6K Cartridge";
		} else {
			metadata["RomType"] = $"Cartridge ({rom.Length / 1024}K)";
		}

		return new RomInfo(Platform, rom.Length, null, metadata);
	}

	public string? GetRegisterLabel(uint address) {
		// I/O ports are accessed via IN/OUT instructions with port numbers,
		// not memory-mapped addresses. The port number is the operand.
		if (IoRegisters.TryGetValue(address, out var label))
			return label;
		return null;
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		return address switch {
			<= 0x07ff => MemoryRegion.Rom,       // BIOS ROMs
			<= 0x17ff => MemoryRegion.Rom,       // Cartridge ROM
			>= 0x2800 and <= 0x2fff => MemoryRegion.Ram,       // System RAM
			>= 0x3000 and <= 0x37ff => MemoryRegion.Graphics,  // Video RAM
			>= 0x3800 and <= 0x38ff => MemoryRegion.Hardware,  // I/O Registers
			_ => MemoryRegion.Unknown
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		// Standard cartridge entry point is $0800 (after BIOS)
		// BIOS jumps to $0800 after initialization
		if (rom.Length > 0x0800) {
			return [0x0800];
		}

		// For BIOS-only ROMs, entry at $0000
		return [0x0000];
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// No banking — address maps directly to file offset
		// For cartridge-only ROMs (no BIOS), subtract $0800
		if (romLength <= 0x1000 && address >= 0x0800) {
			// Cartridge ROM only — offset from cartridge base
			return (int)(address - 0x0800);
		}

		// Full ROM image (includes BIOS) — direct mapping
		if (address < (uint)romLength)
			return (int)address;

		return -1;
	}

	public uint? OffsetToAddress(int offset) {
		// Direct mapping for standard layout
		return (uint)offset;
	}

	public bool IsInSwitchableRegion(uint address) {
		return false; // No bank switching on standard Channel F
	}

	public bool IsValidAddress(uint address) {
		return true;
	}

	public int GetTargetBank(uint target, int currentBank) {
		return currentBank;
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		return null; // No bank switching on standard Channel F
	}
}
