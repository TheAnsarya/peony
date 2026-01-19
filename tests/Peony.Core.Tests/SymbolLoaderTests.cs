using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for SymbolLoader class - loading and managing symbols from various formats
/// </summary>
public class SymbolLoaderTests {
	#region Pansy Integration Tests

	/// <summary>
	/// Creates a minimal valid Pansy file with symbols and comments.
	/// </summary>
	private static byte[] CreatePansyWithSymbolsAndComments(
		Dictionary<int, string> symbols,
		Dictionary<int, string> comments) {
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);

		// Calculate section data
		byte[] symbolSection = CreateSymbolSectionData(symbols);
		byte[] commentSection = CreateCommentSectionData(comments);

		var sectionCount = 0u;
		if (symbols.Count > 0) sectionCount++;
		if (comments.Count > 0) sectionCount++;

		// Header offset: 32 bytes
		// Section table: 16 bytes per section
		var headerSize = 32;
		var sectionTableSize = (int)(sectionCount * 16);
		var symbolSectionOffset = headerSize + sectionTableSize;
		var commentSectionOffset = symbolSectionOffset + symbolSection.Length;

		// Write Header (32 bytes)
		writer.Write("PANSY\0\0\0"u8.ToArray()); // Magic (8 bytes)
		writer.Write((ushort)0x0100);  // Version
		writer.Write((ushort)0);       // Flags (no compression)
		writer.Write((byte)PansyLoader.PLATFORM_NES); // Platform
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write((byte)0);         // Reserved
		writer.Write(0x8000u);         // ROM Size
		writer.Write(0u);              // ROM CRC32
		writer.Write(sectionCount);    // Section count
		writer.Write(0u);              // Reserved

		// Write Section Table
		var offset = symbolSectionOffset;
		if (symbols.Count > 0) {
			writer.Write(0x0002u);      // Type: SYMBOLS
			writer.Write((uint)offset);
			writer.Write((uint)symbolSection.Length);
			writer.Write((uint)symbolSection.Length);
			offset += symbolSection.Length;
		}

		if (comments.Count > 0) {
			writer.Write(0x0003u);      // Type: COMMENTS
			writer.Write((uint)offset);
			writer.Write((uint)commentSection.Length);
			writer.Write((uint)commentSection.Length);
		}

		// Write Section Data
		if (symbols.Count > 0) {
			writer.Write(symbolSection);
		}
		if (comments.Count > 0) {
			writer.Write(commentSection);
		}

		return ms.ToArray();
	}

	private static byte[] CreateSymbolSectionData(Dictionary<int, string> symbols) {
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);

		foreach (var (address, name) in symbols) {
			writer.Write((uint)address);  // Address (24-bit in lower bits)
			writer.Write((byte)0);        // Type (label)
			writer.Write((byte)0);        // Flags
			var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
			writer.Write((ushort)nameBytes.Length);
			writer.Write(nameBytes);
			writer.Write((ushort)0);      // Value length (no value)
		}

		return ms.ToArray();
	}

	private static byte[] CreateCommentSectionData(Dictionary<int, string> comments) {
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);

		foreach (var (address, text) in comments) {
			writer.Write((uint)address);
			writer.Write((byte)0);       // Type (line comment)
			var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
			writer.Write((ushort)textBytes.Length);
			writer.Write(textBytes);
		}

		return ms.ToArray();
	}

	/// <summary>
	/// Creates a Pansy file with code/data map for IsCode/IsData testing.
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
	public void LoadPansyData_ImportsSymbols() {
		var symbols = new Dictionary<int, string> {
			[0x8000] = "Reset",
			[0x8100] = "NMI",
			[0x8200] = "IRQ"
		};

		var pansyData = CreatePansyWithSymbolsAndComments(symbols, []);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal("Reset", loader.GetLabel(0x8000));
		Assert.Equal("NMI", loader.GetLabel(0x8100));
		Assert.Equal("IRQ", loader.GetLabel(0x8200));
	}

	[Fact]
	public void LoadPansyData_ImportsComments() {
		var comments = new Dictionary<int, string> {
			[0x8000] = "Reset vector",
			[0xC000] = "Main loop"
		};

		var pansyData = CreatePansyWithSymbolsAndComments([], comments);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal("Reset vector", loader.GetComment(0x8000));
		Assert.Equal("Main loop", loader.GetComment(0xC000));
	}

	[Fact]
	public void LoadPansyData_ImportsSymbolsAndComments() {
		var symbols = new Dictionary<int, string> {
			[0x8000] = "Reset"
		};
		var comments = new Dictionary<int, string> {
			[0x8000] = "Entry point"
		};

		var pansyData = CreatePansyWithSymbolsAndComments(symbols, comments);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.Equal("Reset", loader.GetLabel(0x8000));
		Assert.Equal("Entry point", loader.GetComment(0x8000));
	}

	[Fact]
	public void LoadPansyData_ExposedPansyData() {
		var symbols = new Dictionary<int, string> {
			[0x8000] = "Main"
		};

		var pansyData = CreatePansyWithSymbolsAndComments(symbols, []);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.NotNull(loader.PansyData);
		Assert.Equal("Main", loader.PansyData!.GetSymbol(0x8000));
	}

	[Fact]
	public void IsCode_UsesPansyData() {
		var codeDataMap = new byte[16];
		codeDataMap[0] = 0x11; // Code + Opcode
		codeDataMap[1] = 0x01; // Code (operand)
		codeDataMap[5] = 0x02; // Data

		var pansyData = CreatePansyWithCodeDataMap(codeDataMap);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.True(loader.IsCode(0));
		Assert.True(loader.IsCode(1));
		Assert.False(loader.IsCode(5));
	}

	[Fact]
	public void IsData_UsesPansyData() {
		var codeDataMap = new byte[16];
		codeDataMap[0] = 0x01; // Code
		codeDataMap[5] = 0x02; // Data
		codeDataMap[10] = 0x02; // Data

		var pansyData = CreatePansyWithCodeDataMap(codeDataMap);
		var loader = new SymbolLoader();
		loader.LoadPansyData(pansyData);

		Assert.False(loader.IsData(0));
		Assert.True(loader.IsData(5));
		Assert.True(loader.IsData(10));
	}

	[Fact]
	public void IsCode_ReturnsNull_WithoutData() {
		var loader = new SymbolLoader();

		Assert.Null(loader.IsCode(0));
	}

	[Fact]
	public void IsData_ReturnsNull_WithoutData() {
		var loader = new SymbolLoader();

		Assert.Null(loader.IsData(0));
	}

	#endregion

	#region General SymbolLoader Tests

	[Fact]
	public void AddLabel_StoresLabel() {
		var loader = new SymbolLoader();
		loader.AddLabel(0x8000, "Reset");

		Assert.Equal("Reset", loader.GetLabel(0x8000));
	}

	[Fact]
	public void AddBankLabel_StoresLabelWithBank() {
		var loader = new SymbolLoader();
		// Signature is AddBankLabel(bank, address, label)
		loader.AddBankLabel(2, 0x8000, "BankRoutine");

		// Bank 2 at 0x8000 can be retrieved with bank parameter
		Assert.Equal("BankRoutine", loader.GetLabel(0x8000, bank: 2));
	}

	[Fact]
	public void GetLabel_ReturnsNull_WhenNotFound() {
		var loader = new SymbolLoader();

		Assert.Null(loader.GetLabel(0x8000));
	}

	[Fact]
	public void GetComment_ReturnsNull_WhenNotFound() {
		var loader = new SymbolLoader();

		Assert.Null(loader.GetComment(0x8000));
	}

	[Fact]
	public void Labels_ReturnsAllLabels() {
		var loader = new SymbolLoader();
		loader.AddLabel(0x8000, "Reset");
		loader.AddLabel(0x8100, "NMI");

		var labels = loader.Labels;

		Assert.Equal(2, labels.Count);
		Assert.Equal("Reset", labels[0x8000]);
		Assert.Equal("NMI", labels[0x8100]);
	}

	#endregion
}
