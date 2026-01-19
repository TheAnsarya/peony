using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for PansyLoader class - parsing Pansy metadata files
/// </summary>
public class PansyLoaderTests {
	/// <summary>
	/// Creates a minimal valid Pansy file header.
	/// </summary>
	private static byte[] CreateMinimalPansyFile(byte platform = PansyLoader.PLATFORM_NES, uint romSize = 0x8000) {
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// Header (32 bytes)
		writer.Write("PANSY\0\0\0"u8.ToArray()); // Magic (8 bytes)
		writer.Write((ushort)0x0100);  // Version
		writer.Write((ushort)0);       // Flags
		writer.Write(platform);        // Platform
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write(romSize);         // ROM Size
		writer.Write(0u);              // ROM CRC32
		writer.Write(0u);              // Section count
		writer.Write(0u);              // Reserved

		return ms.ToArray();
	}

	/// <summary>
	/// Creates a Pansy file with a code/data map section.
	/// </summary>
	private static byte[] CreatePansyWithCodeDataMap(byte[] codeDataMap) {
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		var sectionOffset = 32 + 16; // Header + 1 section entry

		// Header (32 bytes)
		writer.Write("PANSY\0\0\0"u8.ToArray()); // Magic (8 bytes)
		writer.Write((ushort)0x0100);  // Version
		writer.Write((ushort)0);       // Flags (no compression)
		writer.Write((byte)PansyLoader.PLATFORM_NES); // Platform
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write((uint)codeDataMap.Length); // ROM Size
		writer.Write(0u);              // ROM CRC32
		writer.Write(1u);              // Section count
		writer.Write(0u);              // Reserved

		// Section table entry
		writer.Write(0x0001u);         // Type: CODE_DATA_MAP
		writer.Write((uint)sectionOffset); // Offset
		writer.Write((uint)codeDataMap.Length); // Compressed size
		writer.Write((uint)codeDataMap.Length); // Uncompressed size

		// Section data
		writer.Write(codeDataMap);

		return ms.ToArray();
	}

	[Fact]
	public void ParsesValidHeader() {
		var data = CreateMinimalPansyFile();
		var loader = new PansyLoader(data);

		Assert.Equal(0x0100, loader.Version);
		Assert.Equal(PansyLoader.PLATFORM_NES, loader.Platform);
		Assert.Equal(0x8000u, loader.RomSize);
	}

	[Fact]
	public void ThrowsOnInvalidMagic() {
		var badData = new byte[32];
		badData[0] = (byte)'X'; // Wrong magic

		Assert.Throws<InvalidDataException>(() => new PansyLoader(badData));
	}

	[Fact]
	public void ThrowsOnTruncatedFile() {
		var shortData = new byte[16]; // Less than 32 byte header

		Assert.Throws<InvalidDataException>(() => new PansyLoader(shortData));
	}

	[Fact]
	public void ParsesCodeDataMapSection() {
		var codeDataMap = new byte[16];
		codeDataMap[0] = 0x11; // Code + Opcode
		codeDataMap[1] = 0x01; // Code (operand)
		codeDataMap[2] = 0x01; // Code (operand)
		codeDataMap[5] = 0x02; // Data
		codeDataMap[10] = 0x04; // Jump target
		codeDataMap[11] = 0x08; // Sub entry point

		var data = CreatePansyWithCodeDataMap(codeDataMap);
		var loader = new PansyLoader(data);

		Assert.True(loader.IsCode(0));
		Assert.True(loader.IsCode(1));
		Assert.True(loader.IsCode(2));
		Assert.True(loader.IsOpcode(0));
		Assert.False(loader.IsOpcode(1));
		Assert.True(loader.IsData(5));
		Assert.True(loader.IsJumpTarget(10));
		Assert.True(loader.IsSubEntryPoint(11));
	}

	[Fact]
	public void ParsesPlatformIds() {
		// Test each platform
		Assert.Equal("NES", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_NES));
		Assert.Equal("SNES", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_SNES));
		Assert.Equal("Game Boy", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_GB));
		Assert.Equal("Game Boy Advance", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_GBA));
		Assert.Equal("Sega Genesis", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_GENESIS));
		Assert.Equal("Sega Master System", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_SMS));
		Assert.Equal("TurboGrafx-16", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_PCE));
		Assert.Equal("Atari 2600", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_ATARI_2600));
		Assert.Equal("Atari Lynx", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_LYNX));
		Assert.Equal("WonderSwan", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_WONDERSWAN));
		Assert.Equal("Neo Geo", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_NEOGEO));
		Assert.Equal("SPC700", PansyLoader.GetPlatformName(PansyLoader.PLATFORM_SPC700));
		Assert.Equal("Unknown", PansyLoader.GetPlatformName(0xFF));
	}

	[Fact]
	public void GetCoverageStatsCalculatesCorrectly() {
		var codeDataMap = new byte[100];
		// 10 code bytes
		for (int i = 0; i < 10; i++) codeDataMap[i] = 0x01;
		// 5 data bytes
		for (int i = 50; i < 55; i++) codeDataMap[i] = 0x02;

		var data = CreatePansyWithCodeDataMap(codeDataMap);
		var loader = new PansyLoader(data);

		var (codeBytes, dataBytes, totalSize, coverage) = loader.GetCoverageStats();
		Assert.Equal(10, codeBytes);
		Assert.Equal(5, dataBytes);
		Assert.Equal(100, totalSize);
		Assert.Equal(15.0, coverage);
	}

	[Fact]
	public void ParsesDifferentPlatforms() {
		// NES
		var nesData = CreateMinimalPansyFile(PansyLoader.PLATFORM_NES);
		var nesLoader = new PansyLoader(nesData);
		Assert.Equal(PansyLoader.PLATFORM_NES, nesLoader.Platform);

		// SNES
		var snesData = CreateMinimalPansyFile(PansyLoader.PLATFORM_SNES);
		var snesLoader = new PansyLoader(snesData);
		Assert.Equal(PansyLoader.PLATFORM_SNES, snesLoader.Platform);

		// Game Boy
		var gbData = CreateMinimalPansyFile(PansyLoader.PLATFORM_GB);
		var gbLoader = new PansyLoader(gbData);
		Assert.Equal(PansyLoader.PLATFORM_GB, gbLoader.Platform);
	}

	[Fact]
	public void PlatformIdsHaveCorrectValues() {
		// Verify platform IDs match the specification
		Assert.Equal(0x01, PansyLoader.PLATFORM_NES);
		Assert.Equal(0x02, PansyLoader.PLATFORM_SNES);
		Assert.Equal(0x03, PansyLoader.PLATFORM_GB);
		Assert.Equal(0x04, PansyLoader.PLATFORM_GBA);
		Assert.Equal(0x05, PansyLoader.PLATFORM_GENESIS);
		Assert.Equal(0x06, PansyLoader.PLATFORM_SMS);
		Assert.Equal(0x07, PansyLoader.PLATFORM_PCE);
		Assert.Equal(0x08, PansyLoader.PLATFORM_ATARI_2600);
		Assert.Equal(0x09, PansyLoader.PLATFORM_LYNX);
		Assert.Equal(0x0A, PansyLoader.PLATFORM_WONDERSWAN);
		Assert.Equal(0x0B, PansyLoader.PLATFORM_NEOGEO);
		Assert.Equal(0x0C, PansyLoader.PLATFORM_SPC700);
	}

	[Fact]
	public void HandlesEmptyCodeDataMap() {
		var emptyMap = new byte[100]; // All zeros = unreached
		var data = CreatePansyWithCodeDataMap(emptyMap);
		var loader = new PansyLoader(data);

		Assert.Empty(loader.CodeOffsets);
		Assert.Empty(loader.DataOffsets);
		Assert.Empty(loader.JumpTargets);
		Assert.Empty(loader.SubEntryPoints);
	}

	[Fact]
	public void ReturnsNullForMissingSymbol() {
		var data = CreateMinimalPansyFile();
		var loader = new PansyLoader(data);

		Assert.Null(loader.GetSymbol(0x8000));
	}

	[Fact]
	public void ReturnsNullForMissingComment() {
		var data = CreateMinimalPansyFile();
		var loader = new PansyLoader(data);

		Assert.Null(loader.GetComment(0x8000));
	}
}
