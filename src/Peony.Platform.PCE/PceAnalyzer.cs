namespace Peony.Platform.PCE;

using System.Collections.Frozen;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// PC Engine / TurboGrafx-16 platform analyzer.
/// </summary>
/// <remarks>
/// <para>
/// The PCE uses a HuC6280 CPU (65C02 superset) at 7.16 MHz with:
/// - 8KB base RAM (work RAM)
/// - 64KB VRAM (managed by HuC6270 VDC)
/// - 8 MPR (Memory Page Registers) mapping 8KB pages across 21-bit address space
/// </para>
/// <para>
/// Memory map (logical, before MPR mapping):
/// - $0000-$1FFF: Page mapped by MPR0 (typically I/O)
/// - $2000-$3FFF: Page mapped by MPR1 (typically RAM)
/// - $4000-$5FFF: Page mapped by MPR2
/// - $6000-$7FFF: Page mapped by MPR3
/// - $8000-$9FFF: Page mapped by MPR4
/// - $A000-$BFFF: Page mapped by MPR5
/// - $C000-$DFFF: Page mapped by MPR6
/// - $E000-$FFFF: Page mapped by MPR7 (typically ROM with vectors)
/// </para>
/// <para>
/// I/O page ($0000-$1FFF when MPR0=$FF):
/// - $0000-$03FF: VDC (HuC6270)
/// - $0400-$07FF: VCE (HuC6260)
/// - $0800-$0BFF: PSG (HuC6280 built-in)
/// - $0C00-$0FFF: Timer
/// - $1000-$13FF: I/O port
/// - $1400-$17FF: Interrupt control
/// </para>
/// </remarks>
public sealed class PceAnalyzer : IPlatformAnalyzer {
	public string Platform => "PC Engine";
	public ICpuDecoder CpuDecoder { get; } = new HuC6280Decoder();
	public int BankCount { get; private set; } = 1;
	public int RomDataOffset { get; private set; }

	private const int PageSize = 0x2000; // 8KB pages

	// Hardware I/O registers (when MPR0 maps to $FF page = I/O area)
	private static readonly FrozenDictionary<uint, string> IoRegisters = new Dictionary<uint, string> {
		// VDC (HuC6270) - $0000-$03FF
		[0x0000] = "VDC_STATUS",   // VDC status / register select
		[0x0001] = "VDC_STATUS_H",
		[0x0002] = "VDC_DATA_L",   // VDC data low
		[0x0003] = "VDC_DATA_H",   // VDC data high

		// VCE (HuC6260) - $0400-$07FF
		[0x0400] = "VCE_CTRL",     // VCE control
		[0x0402] = "VCE_ADDR_L",   // Color table address low
		[0x0403] = "VCE_ADDR_H",   // Color table address high
		[0x0404] = "VCE_DATA_L",   // Color data low
		[0x0405] = "VCE_DATA_H",   // Color data high

		// PSG - $0800-$0BFF
		[0x0800] = "PSG_SELECT",   // PSG channel select
		[0x0801] = "PSG_BALANCE",  // Main volume balance
		[0x0802] = "PSG_FREQ_L",   // Frequency low
		[0x0803] = "PSG_FREQ_H",   // Frequency high
		[0x0804] = "PSG_CTRL",     // Channel control
		[0x0805] = "PSG_CHBALANCE",// Channel volume balance
		[0x0806] = "PSG_WAVDATA",  // Waveform data
		[0x0807] = "PSG_NOISE",    // Noise channel control
		[0x0808] = "PSG_LFOFREQ",  // LFO frequency
		[0x0809] = "PSG_LFOCTRL",  // LFO control

		// Timer - $0C00-$0FFF
		[0x0c00] = "TIMER_COUNT",  // Timer counter
		[0x0c01] = "TIMER_CTRL",   // Timer control

		// I/O port - $1000-$13FF
		[0x1000] = "IO_PORT",      // Joypad I/O

		// Interrupt control - $1400-$17FF
		[0x1402] = "IRQ_DISABLE",  // Interrupt disable
		[0x1403] = "IRQ_STATUS",   // Interrupt status/acknowledge
	}.ToFrozenDictionary();

	public RomInfo Analyze(ReadOnlySpan<byte> rom) {
		RomDataOffset = 0;
		var metadata = new Dictionary<string, string>();

		// PCE ROMs can have a 512-byte header (from copiers)
		if (rom.Length % 0x2000 == 512) {
			RomDataOffset = 512;
			metadata["Format"] = "Headered";
		} else {
			metadata["Format"] = "Raw";
		}

		var romData = rom[RomDataOffset..];
		BankCount = Math.Max(1, romData.Length / PageSize);
		metadata["Banks"] = BankCount.ToString();
		metadata["RomSize"] = $"{romData.Length / 1024}KB";

		// Detect ROM type
		if (romData.Length > 0x100000) {
			metadata["Type"] = "CD-ROM";
		} else if (romData.Length > 0x60000) {
			metadata["Type"] = "SF2 Mapper";
		} else {
			metadata["Type"] = "HuCard";
		}

		return new RomInfo(Platform, rom.Length, null, metadata);
	}

	public string? GetRegisterLabel(uint address) {
		return IoRegisters.GetValueOrDefault(address);
	}

	public MemoryRegion GetMemoryRegion(uint address) {
		// This is the logical view based on typical MPR configuration
		return address switch {
			< 0x2000 => MemoryRegion.Hardware,    // I/O page (MPR0 = $FF)
			< 0x4000 => MemoryRegion.Ram,          // Work RAM (MPR1 = $F8)
			< 0xe000 => MemoryRegion.Rom,          // ROM pages (MPR2-6)
			_ => MemoryRegion.Rom                   // ROM page (MPR7)
		};
	}

	public uint[] GetEntryPoints(ReadOnlySpan<byte> rom) {
		var romData = rom[RomDataOffset..];
		var entryPoints = new List<uint>();

		// The reset vector is at the end of the last 8KB page
		// PCE vectors at $FFF6-$FFFF:
		// $FFF6: IRQ2/BRK vector
		// $FFF8: IRQ1 (VDC) vector
		// $FFFA: Timer IRQ vector
		// $FFFC: NMI vector
		// $FFFE: RESET vector
		if (romData.Length >= 6) {
			var lastPage = romData.Slice(romData.Length - PageSize);
			if (lastPage.Length >= PageSize) {
				var resetVec = (uint)(lastPage[PageSize - 2] | (lastPage[PageSize - 1] << 8));
				var nmiVec = (uint)(lastPage[PageSize - 4] | (lastPage[PageSize - 3] << 8));
				var timerVec = (uint)(lastPage[PageSize - 6] | (lastPage[PageSize - 5] << 8));
				var irq1Vec = (uint)(lastPage[PageSize - 8] | (lastPage[PageSize - 7] << 8));
				var irq2Vec = (uint)(lastPage[PageSize - 10] | (lastPage[PageSize - 9] << 8));

				entryPoints.Add(resetVec);
				if (nmiVec != resetVec) entryPoints.Add(nmiVec);
				if (timerVec != resetVec && !entryPoints.Contains(timerVec)) entryPoints.Add(timerVec);
				if (irq1Vec != resetVec && !entryPoints.Contains(irq1Vec)) entryPoints.Add(irq1Vec);
				if (irq2Vec != resetVec && !entryPoints.Contains(irq2Vec)) entryPoints.Add(irq2Vec);
			}
		}

		if (entryPoints.Count == 0)
			entryPoints.Add(0xe000); // Default reset vector location

		return [.. entryPoints];
	}

	public int AddressToOffset(uint address, int romLength) {
		return AddressToOffset(address, romLength, 0);
	}

	public int AddressToOffset(uint address, int romLength, int bank) {
		// With typical MPR setup, ROM is mapped starting at page boundaries
		// For simplicity, treat addresses $4000-$FFFF as ROM
		if (address < 0x4000) return -1; // I/O or RAM

		var romData = romLength - RomDataOffset;
		var offset = bank * PageSize + (int)(address % PageSize) + RomDataOffset;
		return offset < romLength ? offset : -1;
	}

	public uint? OffsetToAddress(int offset) {
		if (offset < RomDataOffset) return null;
		var romOffset = offset - RomDataOffset;
		// Map to $E000-$FFFF for the last page
		var pageOffset = romOffset % PageSize;
		return (uint)(0xe000 + pageOffset);
	}

	public bool IsInSwitchableRegion(uint address) {
		// All ROM pages are switchable via MPR
		return address >= 0x4000;
	}

	public bool IsValidAddress(uint address) {
		return true;
	}

	public int GetTargetBank(uint target, int currentBank) {
		return currentBank;
	}

	public BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank) {
		// Look for TAM instruction (0x53) which sets MPR
		var offset = AddressToOffset(address, rom.Length, currentBank);
		if (offset < 0 || offset + 2 >= rom.Length) return null;

		// lda #<bank>; tam #<bit> pattern
		// lda = 0xA9 nn, tam = 0x53 nn
		if (rom[offset] == 0xa9 && offset + 3 < rom.Length && rom[offset + 2] == 0x53) {
			var bankNum = rom[offset + 1];
			return new BankSwitchInfo(bankNum, address + 4, null);
		}

		return null;
	}
}
