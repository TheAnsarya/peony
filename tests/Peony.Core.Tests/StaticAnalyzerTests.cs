using Peony.Core;
using Peony.Platform.NES;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for StaticAnalyzer — CDL-first deterministic classification.
/// </summary>
public class StaticAnalyzerTests {
	private readonly NesAnalyzer _nesAnalyzer = new();

	/// <summary>
	/// Creates a minimal NES ROM with iNES header and vectors pointing to $8000.
	/// </summary>
	private static byte[] CreateNesRom(int prgBanks = 2) {
		var prgSize = prgBanks * 16384;
		var chrSize = 8192;
		var totalSize = 16 + prgSize + chrSize;
		var rom = new byte[totalSize];

		// iNES header: "NES\x1a"
		rom[0] = 0x4e; rom[1] = 0x45; rom[2] = 0x53; rom[3] = 0x1a;
		rom[4] = (byte)prgBanks;  // PRG banks
		rom[5] = 1;               // CHR banks

		// Reset vector → $8000
		var vecBase = 16 + prgSize - 6;
		rom[vecBase] = 0x00; rom[vecBase + 1] = 0x80;     // NMI → $8000
		rom[vecBase + 2] = 0x00; rom[vecBase + 3] = 0x80; // RESET → $8000
		rom[vecBase + 4] = 0x00; rom[vecBase + 5] = 0x80; // IRQ → $8000

		return rom;
	}

	// ========================================================================
	// Phase 1: CDL Classification Tests
	// ========================================================================

	[Fact]
	public void CdlCodeFlags_ClassifiedAsCode() {
		var rom = CreateNesRom();
		var cdlData = new byte[rom.Length - 16]; // CDL matches PRG size (no header)
		cdlData[0] = 0x01; // FCEUX CODE flag
		cdlData[1] = 0x01;
		cdlData[2] = 0x01;

		var symbolLoader = new SymbolLoader();
		symbolLoader.LoadCdlData(new CdlLoader(cdlData));

		var analyzer = new StaticAnalyzer(_nesAnalyzer, symbolLoader);
		var result = analyzer.Classify(rom);

		// CDL code offsets should map to Code classification
		// CDL offset 0 maps to ROM offset (depends on platform's AddressToOffset)
		Assert.True(result.Stats.CdlClassified > 0, "CDL should classify at least some bytes");
	}

	[Fact]
	public void CdlDataFlags_ClassifiedAsData() {
		var cdlData = new byte[100];
		cdlData[10] = 0x02; // FCEUX DATA flag
		cdlData[11] = 0x02;

		var cdlLoader = new CdlLoader(cdlData);
		Assert.True(cdlLoader.IsData(10));
		Assert.True(cdlLoader.IsData(11));
	}

	[Fact]
	public void NoCdlNoPansy_OnlyVectorsAndPlatformClassified() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// Without any CDL or Pansy, only vectors and platform defaults apply
		Assert.Equal(0, result.Stats.CdlClassified);
		Assert.Equal(0, result.Stats.PansyClassified);
		Assert.True(result.Stats.VectorClassified > 0 || result.Stats.PlatformClassified > 0,
			"Vectors or platform defaults should classify some bytes");
	}

	// ========================================================================
	// Phase 5: ROM Vector Classification Tests
	// ========================================================================

	[Fact]
	public void NesVectors_ClassifiedAsVectorAndPointer() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// NES vectors are at $fffa-$ffff (last 6 bytes of PRG)
		// In the ROM file, these are at offset = 16 + prgSize - 6
		var prgSize = 2 * 16384;
		var vectorOffset = 16 + prgSize - 6;

		// NMI vector bytes ($fffa-$fffb)
		Assert.True(result.Map[vectorOffset].HasFlag(ByteClassification.Vector),
			"NMI vector byte should be classified as Vector");
		Assert.True(result.Map[vectorOffset].HasFlag(ByteClassification.Pointer),
			"NMI vector byte should have Pointer flag");

		// RESET vector bytes ($fffc-$fffd)
		Assert.True(result.Map[vectorOffset + 2].HasFlag(ByteClassification.Vector),
			"RESET vector byte should be classified as Vector");

		// IRQ vector bytes ($fffe-$ffff)
		Assert.True(result.Map[vectorOffset + 4].HasFlag(ByteClassification.Vector),
			"IRQ vector byte should be classified as Vector");
	}

	[Fact]
	public void VectorTargets_ClassifiedAsCode() {
		var rom = CreateNesRom();
		// Vectors point to $8000 → PRG offset 0 → ROM offset 16
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// The target address $8000 should be classified as Code (from vector)
		// $8000 → offset 16 (start of PRG data in ROM file)
		var targetOffset = _nesAnalyzer.AddressToOffset(0x8000, rom.Length);
		if (targetOffset >= 0 && targetOffset < rom.Length) {
			Assert.True(result.Map[targetOffset].HasFlag(ByteClassification.Code),
				$"Vector target at offset {targetOffset} should be Code");
			Assert.Equal(ClassificationSource.RomVector, result.Sources[targetOffset]);
		}
	}

	// ========================================================================
	// Classification Priority Tests
	// ========================================================================

	[Fact]
	public void UnknownBytes_RemainUnknown() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// Without any CDL or Pansy, most classification comes from vectors + platform
		// Some bytes should still remain Unknown (e.g., unclassified PRG data)
		Assert.True(result.Stats.UnknownBytes > 0, "Some bytes should remain Unknown");
	}

	[Fact]
	public void ClassificationStats_SumsCorrectly() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// The total should match the ROM size
		Assert.Equal(rom.Length, result.Stats.TotalBytes);

		// Coverage should be between 0 and 100
		Assert.True(result.Stats.CoveragePercent >= 0);
		Assert.True(result.Stats.CoveragePercent <= 100);
	}

	[Fact]
	public void ClassificationSources_TrackedPerByte() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		// Sources array should have same length as Map
		Assert.Equal(result.Map.Length, result.Sources.Length);

		// Classified bytes should have non-Unknown source
		for (int i = 0; i < result.Map.Length; i++) {
			if (result.Map[i] != ByteClassification.Unknown) {
				Assert.NotEqual(ClassificationSource.Unknown, result.Sources[i]);
			}
		}

		// Unknown bytes should have Unknown source
		for (int i = 0; i < result.Map.Length; i++) {
			if (result.Map[i] == ByteClassification.Unknown) {
				Assert.Equal(ClassificationSource.Unknown, result.Sources[i]);
			}
		}
	}

	// ========================================================================
	// Region Generation Tests
	// ========================================================================

	[Fact]
	public void GetRegions_ProducesContiguousRegions() {
		var rom = CreateNesRom();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);
		var regions = result.GetRegions();

		Assert.NotEmpty(regions);

		// Regions should be contiguous (no gaps)
		for (int i = 1; i < regions.Count; i++) {
			Assert.Equal(regions[i - 1].EndOffset + 1, regions[i].StartOffset);
		}

		// First region starts at 0
		Assert.Equal(0, regions[0].StartOffset);

		// Last region ends at rom.Length - 1
		Assert.Equal(rom.Length - 1, regions[^1].EndOffset);
	}

	[Fact]
	public void EmptyRom_NoException() {
		var rom = Array.Empty<byte>();
		var analyzer = new StaticAnalyzer(_nesAnalyzer);
		var result = analyzer.Classify(rom);

		Assert.Equal(0, result.Stats.TotalBytes);
		Assert.Empty(result.Map);
		var regions = result.GetRegions();
		Assert.Empty(regions);
	}

	// ========================================================================
	// PlatformMemoryMap Tests
	// ========================================================================

	[Fact]
	public void NesPpuRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("NES", 0x2000);
		Assert.Equal(ByteClassification.Hardware, classification);

		classification = PlatformMemoryMap.GetKnownClassification("NES", 0x2007);
		Assert.Equal(ByteClassification.Hardware, classification);
	}

	[Fact]
	public void NesRam_ClassifiedAsData() {
		var classification = PlatformMemoryMap.GetKnownClassification("NES", 0x0000);
		Assert.Equal(ByteClassification.Data, classification);

		classification = PlatformMemoryMap.GetKnownClassification("NES", 0x07ff);
		Assert.Equal(ByteClassification.Data, classification);
	}

	[Fact]
	public void NesRomSpace_ReturnsNull() {
		// PRG-ROM ($8000-$ffff) could be code or data — no assumption
		var classification = PlatformMemoryMap.GetKnownClassification("NES", 0x8000);
		Assert.Null(classification);

		classification = PlatformMemoryMap.GetKnownClassification("NES", 0xc000);
		Assert.Null(classification);
	}

	[Fact]
	public void NesVectors_ReturnedCorrectly() {
		var vectors = PlatformMemoryMap.GetVectors("NES");
		Assert.Equal(3, vectors.Count);
		Assert.Contains(vectors, v => v.Name == "NMI" && v.Address == 0xfffa);
		Assert.Contains(vectors, v => v.Name == "RESET" && v.Address == 0xfffc);
		Assert.Contains(vectors, v => v.Name == "IRQ" && v.Address == 0xfffe);
	}

	[Fact]
	public void NesSram_ClassifiedAsData() {
		var classification = PlatformMemoryMap.GetKnownClassification("NES", 0x6000);
		Assert.Equal(ByteClassification.Data, classification);

		classification = PlatformMemoryMap.GetKnownClassification("NES", 0x7fff);
		Assert.Equal(ByteClassification.Data, classification);
	}

	[Fact]
	public void NesApuRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("NES", 0x4000);
		Assert.Equal(ByteClassification.Hardware, classification);

		classification = PlatformMemoryMap.GetKnownClassification("NES", 0x4017);
		Assert.Equal(ByteClassification.Hardware, classification);
	}

	[Fact]
	public void GameBoyVram_ClassifiedAsGraphics() {
		var classification = PlatformMemoryMap.GetKnownClassification("Game Boy", 0x8000);
		Assert.Equal(ByteClassification.Graphics, classification);

		classification = PlatformMemoryMap.GetKnownClassification("Game Boy", 0x9fff);
		Assert.Equal(ByteClassification.Graphics, classification);
	}

	[Fact]
	public void GameBoyIoRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("Game Boy", 0xff00);
		Assert.Equal(ByteClassification.Hardware, classification);

		classification = PlatformMemoryMap.GetKnownClassification("Game Boy", 0xff40);
		Assert.Equal(ByteClassification.Hardware, classification);
	}

	[Fact]
	public void GameBoyVectors_ReturnedCorrectly() {
		var vectors = PlatformMemoryMap.GetVectors("Game Boy");
		Assert.True(vectors.Count >= 5);
		Assert.Contains(vectors, v => v.Name == "VBLANK_ISR" && v.Address == 0x0040);
		Assert.Contains(vectors, v => v.Name == "ENTRY_POINT" && v.Address == 0x0100);
	}

	[Fact]
	public void GbaVram_ClassifiedAsGraphics() {
		var classification = PlatformMemoryMap.GetKnownClassification("GBA", 0x06000000);
		Assert.Equal(ByteClassification.Graphics, classification);
	}

	[Fact]
	public void GbaIoRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("GBA", 0x04000000);
		Assert.Equal(ByteClassification.Hardware, classification);
	}

	[Fact]
	public void Atari2600TiaRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("Atari 2600", 0x0000);
		Assert.Equal(ByteClassification.Hardware, classification);

		classification = PlatformMemoryMap.GetKnownClassification("Atari 2600", 0x002c);
		Assert.Equal(ByteClassification.Hardware, classification);
	}

	[Fact]
	public void Atari2600RiotRam_ClassifiedAsData() {
		var classification = PlatformMemoryMap.GetKnownClassification("Atari 2600", 0x0080);
		Assert.Equal(ByteClassification.Data, classification);

		classification = PlatformMemoryMap.GetKnownClassification("Atari 2600", 0x00ff);
		Assert.Equal(ByteClassification.Data, classification);
	}

	[Fact]
	public void UnknownPlatform_ReturnsNull() {
		var classification = PlatformMemoryMap.GetKnownClassification("Unknown", 0x0000);
		Assert.Null(classification);
	}

	[Fact]
	public void UnknownPlatform_ReturnsNoVectors() {
		var vectors = PlatformMemoryMap.GetVectors("Unknown");
		Assert.Empty(vectors);
	}

	// ========================================================================
	// Hardware Register Name Tests
	// ========================================================================

	[Fact]
	public void NesRegisterNames_Correct() {
		Assert.Equal("PPUCTRL", PlatformMemoryMap.GetHardwareRegisterName("NES", 0x2000));
		Assert.Equal("PPUMASK", PlatformMemoryMap.GetHardwareRegisterName("NES", 0x2001));
		Assert.Equal("PPUSTATUS", PlatformMemoryMap.GetHardwareRegisterName("NES", 0x2002));
		Assert.Equal("OAMDMA", PlatformMemoryMap.GetHardwareRegisterName("NES", 0x4014));
		Assert.Equal("JOY1", PlatformMemoryMap.GetHardwareRegisterName("NES", 0x4016));
	}

	[Fact]
	public void GameBoyRegisterNames_Correct() {
		Assert.Equal("JOYP", PlatformMemoryMap.GetHardwareRegisterName("Game Boy", 0xff00));
		Assert.Equal("LCDC", PlatformMemoryMap.GetHardwareRegisterName("Game Boy", 0xff40));
		Assert.Equal("LY", PlatformMemoryMap.GetHardwareRegisterName("Game Boy", 0xff44));
		Assert.Equal("IE", PlatformMemoryMap.GetHardwareRegisterName("Game Boy", 0xffff));
	}

	[Fact]
	public void Atari2600RegisterNames_Correct() {
		Assert.Equal("VSYNC", PlatformMemoryMap.GetHardwareRegisterName("Atari 2600", 0x0000));
		Assert.Equal("WSYNC", PlatformMemoryMap.GetHardwareRegisterName("Atari 2600", 0x0002));
		Assert.Equal("COLUP0", PlatformMemoryMap.GetHardwareRegisterName("Atari 2600", 0x0006));
		Assert.Equal("SWCHA", PlatformMemoryMap.GetHardwareRegisterName("Atari 2600", 0x0280));
		Assert.Equal("INTIM", PlatformMemoryMap.GetHardwareRegisterName("Atari 2600", 0x0284));
	}

	// ========================================================================
	// DataRefType Classification Tests
	// ========================================================================

	[Fact]
	public void ByteClassification_FlagsWork() {
		var combined = ByteClassification.Vector | ByteClassification.Pointer;
		Assert.True(combined.HasFlag(ByteClassification.Vector));
		Assert.True(combined.HasFlag(ByteClassification.Pointer));
		Assert.False(combined.HasFlag(ByteClassification.Code));
	}

	[Fact]
	public void ClassificationResult_DataReferencesInitiallyEmpty() {
		var result = new ClassificationResult(100);
		Assert.Empty(result.DataReferences);
	}

	[Fact]
	public void ClassificationResult_MapInitializedToUnknown() {
		var result = new ClassificationResult(50);
		Assert.All(result.Map, b => Assert.Equal(ByteClassification.Unknown, b));
		Assert.All(result.Sources, s => Assert.Equal(ClassificationSource.Unknown, s));
	}

	[Fact]
	public void SnesVectors_ReturnedCorrectly() {
		var vectors = PlatformMemoryMap.GetVectors("SNES");
		Assert.True(vectors.Count >= 4);
		Assert.Contains(vectors, v => v.Name == "RESET" && v.Address == 0xfffc);
		Assert.Contains(vectors, v => v.Name == "NMI" && v.Address == 0xffea);
		Assert.Contains(vectors, v => v.Name == "IRQ" && v.Address == 0xffee);
	}

	[Fact]
	public void Atari2600Vectors_ReturnedCorrectly() {
		var vectors = PlatformMemoryMap.GetVectors("Atari 2600");
		Assert.Equal(3, vectors.Count);
		Assert.Contains(vectors, v => v.Name == "RESET" && v.Address == 0xfffc);
	}

	[Fact]
	public void SnesWram_ClassifiedAsData() {
		// Banks $7e-$7f are WRAM
		var classification = PlatformMemoryMap.GetKnownClassification("SNES", 0x7e0000);
		Assert.Equal(ByteClassification.Data, classification);
	}

	[Fact]
	public void SnesPpuRegisters_ClassifiedAsHardware() {
		var classification = PlatformMemoryMap.GetKnownClassification("SNES", 0x2100);
		Assert.Equal(ByteClassification.Hardware, classification);
	}
}
