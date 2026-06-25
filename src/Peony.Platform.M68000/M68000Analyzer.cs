namespace Peony.Platform.M68000;

using System.Collections.Frozen;
using System.Linq;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// Flat M68000 analyzer — for arcade systems and other 68000-based hardware
/// with a flat (non-banked) memory map. No ROM header detection, no banking,
/// flat address space from 0x000000 to 0xFFFFFF (24-bit).
/// </summary>
public sealed class M68000Analyzer : IPlatformAnalyzer {
    public string Platform => "M68000 (Flat)";
    public ICpuDecoder CpuDecoder { get; } = new M68000Decoder();
    public int BankCount { get; private set; } = 1;
    public int RomDataOffset { get; private set; }

    // No hardware registers defined — this is a generic profile.
    // Platform-specific profiles can be built on top of this.
    private static readonly FrozenDictionary<uint, string> HardwareRegisters =
        new Dictionary<uint, string>().ToFrozenDictionary();

    public RomInfo Analyze(ReadOnlySpan<byte> rom) {
        RomDataOffset = 0;
        var metadata = new Dictionary<string, string>();

        // Read the initial reset vector to determine ROM size hint
        uint initialPC = 0;
        uint initialSP = 0;
        if (rom.Length >= 8) {
            initialSP = ReadLong(rom, 0);
            initialPC = ReadLong(rom, 4);
        }

        metadata["InitialSP"] = $"0x{initialSP:08X}";
        metadata["InitialPC"] = $"0x{initialPC:08X}";
        metadata["RomSize"] = $"{rom.Length / 1024}KB";
        metadata["RomSizeBytes"] = rom.Length.ToString();

        // Determine effective ROM size from the reset vector
        // If PC points to an address beyond the current ROM size, the ROM might
        // be mapped at a different base address
        if (initialPC > 0 && initialPC < 0x1000000) {
            // Round up to nearest power of 2 or common ROM size
            var effectiveSize = rom.Length;
            metadata["EffectiveRomSize"] = $"{effectiveSize / 1024}KB";
        }

        var entryPoints = GetEntryPoints(rom).OrderBy(x => x).ToArray();
        metadata["EntryPointCount"] = entryPoints.Length.ToString();

        return new RomInfo(Platform, rom.Length, "None", metadata);
    }

    public string BuildDisassemblyScaffold(ReadOnlySpan<byte> rom) {
        var entryPoints = GetEntryPoints(rom).Distinct().OrderBy(x => x).ToArray();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("; M68000 Flat Disassembly Scaffold");
        sb.AppendLine(".m68000");
        sb.AppendLine($"; entry-points={entryPoints.Length}");
        foreach (var entry in entryPoints) {
            sb.AppendLine($"; entry=0x{entry:X6}");
        }
        return sb.ToString();
    }

    public string? GetRegisterLabel(uint address) {
        return HardwareRegisters.GetValueOrDefault(address);
    }

    public MemoryRegion GetMemoryRegion(uint address) {
        // Flat memory map: everything is either ROM (if within ROM bounds) or RAM
        // The disassembly engine will use AddressToOffset to determine if an
        // address maps to ROM data
        return MemoryRegion.Unknown;
    }

    public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
        var entryPoints = new List<uint>();

        if (rom.Length < 8)
            return [.. entryPoints];

        // 68000 vector table at $000000-$0003FF (256 vectors, 4 bytes each)
        uint initialPC = ReadLong(rom, 4);

        // Add the reset vector as the primary entry point
        if (initialPC > 0 && initialPC < 0x1000000) {
            entryPoints.Add(initialPC);
        }

        // Add all exception handler vectors
        for (int i = 2; i < 256 && (i * 4 + 4) <= rom.Length; i++) {
            uint vecAddr = ReadLong(rom, i * 4);
            if (vecAddr > 0 && vecAddr != 0xFFFFFFFF && vecAddr < 0x1000000) {
                if (!entryPoints.Contains(vecAddr))
                    entryPoints.Add(vecAddr);
            }
        }

        return [.. entryPoints];
    }

    public IEnumerable<(uint Address, string Name)> GetDefaultLabels(ReadOnlySpan<byte> rom) {
        // NOTE: Cannot use yield return with ReadOnlySpan<byte> parameter
        // (ref struct cannot cross await/yield boundary).
        // Return empty — callers can use CDL files for vector labels instead.
        return Enumerable.Empty<(uint, string)>();
    }

    public int AddressToOffset(uint address, int romLength) {
        return AddressToOffset(address, romLength, 0);
    }

    public int AddressToOffset(uint address, int romLength, int bank) {
        // Flat mapping: CPU address = ROM file offset
        // This is the key difference from the Genesis profile
        if (address < (uint)romLength) {
            return (int)address;
        }
        return -1;
    }

    public uint? OffsetToAddress(int offset) {
        if (offset < 0) return null;
        return (uint)offset;
    }

    public bool IsInSwitchableRegion(uint address) {
        return false;  // No bank switching in flat mode
    }

    public bool IsValidAddress(uint address) {
        return address < 0x1000000;  // 24-bit address space
    }

    public int GetTargetBank(uint target, int currentBank) {
        return 0;  // Always bank 0 (no banking)
    }

    public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
        return null;  // No bank switching
    }

    private static uint ReadLong(ReadOnlySpan<byte> data, int offset) {
        if (offset + 3 >= data.Length) return 0;
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) | data[offset + 3];
    }
}
