using Peony.Core;
using Peony.Platform.Atari2600;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for Atari 2600 platform analyzer including bank switching detection
/// </summary>
public class Atari2600AnalyzerTests {
	private readonly Atari2600Analyzer _analyzer = new();

	[Fact]
	public void Analyze_2KRom_NoBankSwitching() {
		var rom = new byte[2048];
		// Add reset vector
		rom[2044] = 0x00;
		rom[2045] = 0xf8;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("Atari 2600", info.Platform);
		Assert.Equal(2048, info.Size);
		Assert.Null(info.Mapper);
		Assert.Equal("None", info.Metadata["BankScheme"]);
		Assert.Equal("1", info.Metadata["Banks"]);
	}

	[Fact]
	public void Analyze_4KRom_NoBankSwitching() {
		var rom = new byte[4096];
		rom[4092] = 0x00;
		rom[4093] = 0xf0;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("None", info.Metadata["BankScheme"]);
		Assert.Equal("1", info.Metadata["Banks"]);
		Assert.Equal("4096", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_8KRom_DetectsF8() {
		var rom = new byte[8192];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F8", info.Metadata["BankScheme"]);
		Assert.Equal("2", info.Metadata["Banks"]);
		Assert.Equal("4096", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_16KRom_DetectsF6() {
		var rom = new byte[16384];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F6", info.Metadata["BankScheme"]);
		Assert.Equal("4", info.Metadata["Banks"]);
		Assert.Equal("4096", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_32KRom_DetectsF4() {
		var rom = new byte[32768];

		var info = _analyzer.Analyze(rom);

		Assert.Equal("F4", info.Metadata["BankScheme"]);
		Assert.Equal("8", info.Metadata["Banks"]);
		Assert.Equal("4096", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_8KRom_With3FSignature_Detects3F() {
		var rom = new byte[8192];
		// STA $3F instruction
		rom[100] = 0x85;
		rom[101] = 0x3f;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("3F", info.Metadata["BankScheme"]);
		Assert.Equal("4", info.Metadata["Banks"]); // 8K / 2K = 4 banks
		Assert.Equal("2048", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_8KRom_WithE0Signature_DetectsE0() {
		var rom = new byte[8192];
		// STA $1FE0 instruction
		rom[100] = 0x8d;
		rom[101] = 0xe0;
		rom[102] = 0x1f;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("E0", info.Metadata["BankScheme"]);
		Assert.Equal("8", info.Metadata["Banks"]);
		Assert.Equal("1024", info.Metadata["BankSize"]);
	}

	[Fact]
	public void Analyze_8KRom_WithFESignature_DetectsFE() {
		var rom = new byte[8192];
		// JSR $01FE instruction
		rom[100] = 0x20;
		rom[101] = 0xfe;
		rom[102] = 0x01;

		var info = _analyzer.Analyze(rom);

		Assert.Equal("FE", info.Metadata["BankScheme"]);
		Assert.Equal("2", info.Metadata["Banks"]);
		Assert.Equal("8192", info.Metadata["BankSize"]);
	}

	[Fact]
	public void GetRegisterLabel_TIAWrite_ReturnsLabel() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal("VSYNC", _analyzer.GetRegisterLabel(0x00));
		Assert.Equal("VBLANK", _analyzer.GetRegisterLabel(0x01));
		Assert.Equal("COLUP0", _analyzer.GetRegisterLabel(0x06));
		Assert.Equal("GRP0", _analyzer.GetRegisterLabel(0x1b));
		Assert.Equal("HMOVE", _analyzer.GetRegisterLabel(0x2a));
	}

	[Fact]
	public void GetRegisterLabel_TIARead_ReturnsLabel() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		// TIA read registers overlap with write registers
		// In practice, the same addresses are used for both read/write
		// The analyzer returns write labels, which is fine
		Assert.Equal("VSYNC", _analyzer.GetRegisterLabel(0x00)); // Both CXM0P (read) and VSYNC (write)
		Assert.Equal("REFP1", _analyzer.GetRegisterLabel(0x0c)); // INPT4 read overlaps with REFP1 write
		Assert.Equal("PF0", _analyzer.GetRegisterLabel(0x0d)); // INPT5 read overlaps with PF0 write
	}

	[Fact]
	public void GetRegisterLabel_RIOT_ReturnsLabel() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal("SWCHA", _analyzer.GetRegisterLabel(0x280));
		Assert.Equal("INTIM", _analyzer.GetRegisterLabel(0x284));
		Assert.Equal("TIM64T", _analyzer.GetRegisterLabel(0x296));
	}

	[Fact]
	public void GetRegisterLabel_BankHotspot_F8_ReturnsLabel() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom); // Should detect F8

		Assert.Equal("BANK0", _analyzer.GetRegisterLabel(0xfff8));
		Assert.Equal("BANK1", _analyzer.GetRegisterLabel(0xfff9));
	}

	[Fact]
	public void GetRegisterLabel_BankHotspot_F6_ReturnsLabel() {
		var rom = new byte[16384];
		_analyzer.Analyze(rom); // Should detect F6

		Assert.Equal("BANK0", _analyzer.GetRegisterLabel(0xfff6));
		Assert.Equal("BANK1", _analyzer.GetRegisterLabel(0xfff7));
		Assert.Equal("BANK2", _analyzer.GetRegisterLabel(0xfff8));
		Assert.Equal("BANK3", _analyzer.GetRegisterLabel(0xfff9));
	}

	[Fact]
	public void GetRegisterLabel_BankHotspot_F4_ReturnsLabel() {
		var rom = new byte[32768];
		_analyzer.Analyze(rom); // Should detect F4

		Assert.Equal("BANK0", _analyzer.GetRegisterLabel(0xfff4));
		Assert.Equal("BANK7", _analyzer.GetRegisterLabel(0xfffb));
	}

	[Fact]
	public void GetMemoryRegion_Hardware_TIA() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x00));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x2c));
	}

	[Fact]
	public void GetMemoryRegion_Ram_ZeroPage() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0x80));
		Assert.Equal(MemoryRegion.Ram, _analyzer.GetMemoryRegion(0xff));
	}

	[Fact]
	public void GetMemoryRegion_Hardware_RIOT() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x280));
		Assert.Equal(MemoryRegion.Hardware, _analyzer.GetMemoryRegion(0x297));
	}

	[Fact]
	public void GetMemoryRegion_Rom() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xf800));
		Assert.Equal(MemoryRegion.Rom, _analyzer.GetMemoryRegion(0xffff));
	}

	[Fact]
	public void GetEntryPoints_ExtractsResetVector() {
		var rom = new byte[4096];
		// Reset vector at $FFFC-$FFFD
		rom[4092] = 0x00;
		rom[4093] = 0xf8; // $F800
		// IRQ vector at $FFFE-$FFFF (initialize to same value to get single entry point)
		rom[4094] = 0x00;
		rom[4095] = 0xf8; // $F800 (same as reset)

		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Single(entryPoints);
		Assert.Equal(0xf800u, entryPoints[0]);
	}

	[Fact]
	public void GetEntryPoints_DifferentResetAndIRQ_ReturnsBoth() {
		var rom = new byte[4096];
		rom[4092] = 0x00; // Reset low
		rom[4093] = 0xf8; // Reset high = $F800
		rom[4094] = 0x00; // IRQ low
		rom[4095] = 0xfc; // IRQ high = $FC00

		var entryPoints = _analyzer.GetEntryPoints(rom);

		Assert.Equal(2, entryPoints.Length);
		Assert.Contains(0xf800u, entryPoints);
		Assert.Contains(0xfc00u, entryPoints);
	}

	[Fact]
	public void IsInSwitchableRegion_NoBanking_ReturnsFalse() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.False(_analyzer.IsInSwitchableRegion(0xf800));
		Assert.False(_analyzer.IsInSwitchableRegion(0xffff));
	}

	[Fact]
	public void IsInSwitchableRegion_WithBanking_ReturnsTrue() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom); // F8 banking

		Assert.True(_analyzer.IsInSwitchableRegion(0xf000));
		Assert.True(_analyzer.IsInSwitchableRegion(0xffff));
	}

	[Fact]
	public void AddressToOffset_2KRom_CorrectMapping() {
		var rom = new byte[2048];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf800, 2048));
		Assert.Equal(0x100, _analyzer.AddressToOffset(0xf900, 2048));
		Assert.Equal(0x7ff, _analyzer.AddressToOffset(0xffff, 2048));
		Assert.Equal(-1, _analyzer.AddressToOffset(0xf000, 2048)); // Below range
	}

	[Fact]
	public void AddressToOffset_4KRom_CorrectMapping() {
		var rom = new byte[4096];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf000, 4096));
		Assert.Equal(0x100, _analyzer.AddressToOffset(0xf100, 4096));
		Assert.Equal(0xfff, _analyzer.AddressToOffset(0xffff, 4096));
	}

	[Fact]
	public void AddressToOffset_F8_Bank0() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom);

		Assert.Equal(0, _analyzer.AddressToOffset(0xf000, 8192, 0));
		Assert.Equal(0xfff, _analyzer.AddressToOffset(0xffff, 8192, 0));
	}

	[Fact]
	public void AddressToOffset_F8_Bank1() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom);

		Assert.Equal(4096, _analyzer.AddressToOffset(0xf000, 8192, 1));
		Assert.Equal(8191, _analyzer.AddressToOffset(0xffff, 8192, 1));
	}

	[Fact]
	public void AddressToOffset_F6_AllBanks() {
		var rom = new byte[16384];
		_analyzer.Analyze(rom);

		for (int bank = 0; bank < 4; bank++) {
			var expected = bank * 4096;
			var actual = _analyzer.AddressToOffset(0xf000, 16384, bank);
			Assert.Equal(expected, actual);
		}
	}

	[Fact]
	public void AddressToOffset_3F_Tigervision() {
		var rom = new byte[8192];
		// Force 3F detection
		rom[100] = 0x85;
		rom[101] = 0x3f;
		_analyzer.Analyze(rom);

		// Bank 0 at $F800
		Assert.Equal(0, _analyzer.AddressToOffset(0xf800, 8192, 0));
		// Bank 1 at $F800
		Assert.Equal(2048, _analyzer.AddressToOffset(0xf800, 8192, 1));
		// Fixed bank at $F000 (last 2K)
		Assert.Equal(6144, _analyzer.AddressToOffset(0xf000, 8192, -1));
	}

	[Fact]
	public void AddressToOffset_E0_ParkerBros() {
		var rom = new byte[8192];
		// Force E0 detection
		rom[100] = 0x8d;
		rom[101] = 0xe0;
		rom[102] = 0x1f;
		_analyzer.Analyze(rom);

		// Slot 0 ($F000-$F3FF) with bank 0
		Assert.Equal(0, _analyzer.AddressToOffset(0xf000, 8192, 0));
		// Slot 3 ($FC00-$FFFF) is always bank 7
		Assert.Equal(7 * 1024, _analyzer.AddressToOffset(0xfc00, 8192, -1));
	}

	[Fact]
	public void AddressToOffset_FE_Activision() {
		var rom = new byte[8192];
		// Force FE detection
		rom[100] = 0x20;
		rom[101] = 0xfe;
		rom[102] = 0x01;
		_analyzer.Analyze(rom);

		// Bank 0
		Assert.Equal(0, _analyzer.AddressToOffset(0xd000, 8192, 0));
		// Below $D000 returns -1
		Assert.Equal(-1, _analyzer.AddressToOffset(0xc000, 8192, 0));
	}

	[Fact]
	public void BankCount_MatchesBankScheme() {
		var testCases = new[] {
			(2048, "None", 1),
			(4096, "None", 1),
			(8192, "F8", 2),
			(16384, "F6", 4),
			(32768, "F4", 8),
		};

		foreach (var (size, expectedScheme, expectedBanks) in testCases) {
			var rom = new byte[size];
			_analyzer.Analyze(rom);

			Assert.Equal(expectedBanks, _analyzer.BankCount);
		}
	}

	[Fact]
	public void DetectedScheme_Persists() {
		var rom = new byte[8192];
		_analyzer.Analyze(rom);

		Assert.Equal("F8", _analyzer.DetectedScheme);
		Assert.Equal(2, _analyzer.BankCount);
	}
}
