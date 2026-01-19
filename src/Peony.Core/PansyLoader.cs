using System.IO.Compression;
using System.Text;

namespace Peony.Core;

/// <summary>
/// Loads Pansy (Program ANalysis SYstem) files for comprehensive metadata import.
/// Pansy files contain code/data maps, symbols, comments, cross-references, and more,
/// providing complete roundtrip support between Poppy (assembler) and Peony (disassembler).
/// </summary>
public class PansyLoader {
	private readonly byte[] _data;
	private readonly ushort _version;
	private readonly PansyFlags _flags;
	private readonly byte _platform;
	private readonly uint _romSize;
	private readonly uint _romCrc32;
	private readonly List<SectionInfo> _sections = [];

	// Parsed data
	private byte[]? _codeDataMap;
	private readonly HashSet<int> _codeOffsets = [];
	private readonly HashSet<int> _dataOffsets = [];
	private readonly HashSet<int> _jumpTargets = [];
	private readonly HashSet<int> _subEntryPoints = [];
	private readonly HashSet<int> _opcodeOffsets = [];
	private readonly Dictionary<int, string> _symbols = [];
	private readonly Dictionary<int, string> _comments = [];
	private readonly List<MemoryRegion> _memoryRegions = [];
	private readonly List<CrossReference> _crossRefs = [];
	private string _projectName = "";
	private string _author = "";
	private string _projectVersion = "";

	#region Constants
	// Platform IDs
	/// <summary>Platform ID for NES.</summary>
	public const byte PLATFORM_NES = 0x01;
	/// <summary>Platform ID for SNES.</summary>
	public const byte PLATFORM_SNES = 0x02;
	/// <summary>Platform ID for Game Boy.</summary>
	public const byte PLATFORM_GB = 0x03;
	/// <summary>Platform ID for Game Boy Advance.</summary>
	public const byte PLATFORM_GBA = 0x04;
	/// <summary>Platform ID for Sega Genesis.</summary>
	public const byte PLATFORM_GENESIS = 0x05;
	/// <summary>Platform ID for Sega Master System.</summary>
	public const byte PLATFORM_SMS = 0x06;
	/// <summary>Platform ID for TurboGrafx-16.</summary>
	public const byte PLATFORM_PCE = 0x07;
	/// <summary>Platform ID for Atari 2600.</summary>
	public const byte PLATFORM_ATARI_2600 = 0x08;
	/// <summary>Platform ID for Atari Lynx.</summary>
	public const byte PLATFORM_LYNX = 0x09;
	/// <summary>Platform ID for WonderSwan.</summary>
	public const byte PLATFORM_WONDERSWAN = 0x0A;
	/// <summary>Platform ID for Neo Geo.</summary>
	public const byte PLATFORM_NEOGEO = 0x0B;
	/// <summary>Platform ID for SPC700.</summary>
	public const byte PLATFORM_SPC700 = 0x0C;

	// Section types
	private const uint SECTION_CODE_DATA_MAP = 0x0001;
	private const uint SECTION_SYMBOLS = 0x0002;
	private const uint SECTION_COMMENTS = 0x0003;
	private const uint SECTION_MEMORY_REGIONS = 0x0004;
	private const uint SECTION_DATA_TYPES = 0x0005;
	private const uint SECTION_CROSS_REFS = 0x0006;
	private const uint SECTION_SOURCE_MAP = 0x0007;
	private const uint SECTION_METADATA = 0x0008;

	// Byte flags
	private const byte FLAG_CODE = 0x01;
	private const byte FLAG_DATA = 0x02;
	private const byte FLAG_JUMP_TARGET = 0x04;
	private const byte FLAG_SUB_ENTRY = 0x08;
	private const byte FLAG_OPCODE = 0x10;
	private const byte FLAG_DRAWN = 0x20;
	private const byte FLAG_READ = 0x40;
	private const byte FLAG_INDIRECT = 0x80;
	#endregion

	#region Flags and Types
	/// <summary>
	/// Pansy file flags.
	/// </summary>
	[Flags]
	public enum PansyFlags : ushort {
		/// <summary>No flags.</summary>
		None = 0,
		/// <summary>Section data is compressed.</summary>
		Compressed = 0x0001,
		/// <summary>File contains source map.</summary>
		HasSourceMap = 0x0002,
		/// <summary>File contains cross-references.</summary>
		HasCrossRefs = 0x0004,
		/// <summary>File has detailed CDL data.</summary>
		DetailedCdl = 0x0008,
	}

	/// <summary>
	/// Symbol entry types.
	/// </summary>
	public enum SymbolType : byte {
		/// <summary>Code or data label.</summary>
		Label = 1,
		/// <summary>Named constant.</summary>
		Constant = 2,
		/// <summary>Enumeration member.</summary>
		Enum = 3,
		/// <summary>Structure definition.</summary>
		Struct = 4,
		/// <summary>Macro definition.</summary>
		Macro = 5,
		/// <summary>Local label.</summary>
		Local = 6,
		/// <summary>Anonymous label.</summary>
		Anonymous = 7,
	}

	/// <summary>
	/// Cross-reference types.
	/// </summary>
	public enum CrossRefType : byte {
		/// <summary>Subroutine call.</summary>
		Jsr = 1,
		/// <summary>Jump.</summary>
		Jmp = 2,
		/// <summary>Branch.</summary>
		Branch = 3,
		/// <summary>Read access.</summary>
		Read = 4,
		/// <summary>Write access.</summary>
		Write = 5,
	}

	/// <summary>
	/// Information about a section in the Pansy file.
	/// </summary>
	public record SectionInfo(uint Type, uint Offset, uint CompressedSize, uint UncompressedSize);

	/// <summary>
	/// Memory region definition.
	/// </summary>
	public record MemoryRegion(uint Start, uint End, byte Type, byte Bank, string Name);

	/// <summary>
	/// Cross-reference entry.
	/// </summary>
	public record CrossReference(uint From, uint To, CrossRefType Type);
	#endregion

	#region Properties
	/// <summary>Gets the format version.</summary>
	public ushort Version => _version;

	/// <summary>Gets the file flags.</summary>
	public PansyFlags Flags => _flags;

	/// <summary>Gets the platform ID.</summary>
	public byte Platform => _platform;

	/// <summary>Gets the ROM size.</summary>
	public uint RomSize => _romSize;

	/// <summary>Gets the ROM CRC32.</summary>
	public uint RomCrc32 => _romCrc32;

	/// <summary>Gets all ROM offsets marked as code.</summary>
	public IReadOnlySet<int> CodeOffsets => _codeOffsets;

	/// <summary>Gets all ROM offsets marked as data.</summary>
	public IReadOnlySet<int> DataOffsets => _dataOffsets;

	/// <summary>Gets all ROM offsets that are jump targets.</summary>
	public IReadOnlySet<int> JumpTargets => _jumpTargets;

	/// <summary>Gets all ROM offsets that are subroutine entry points.</summary>
	public IReadOnlySet<int> SubEntryPoints => _subEntryPoints;

	/// <summary>Gets all ROM offsets that are opcodes (vs operands).</summary>
	public IReadOnlySet<int> OpcodeOffsets => _opcodeOffsets;

	/// <summary>Gets symbols by address.</summary>
	public IReadOnlyDictionary<int, string> Symbols => _symbols;

	/// <summary>Gets comments by address.</summary>
	public IReadOnlyDictionary<int, string> Comments => _comments;

	/// <summary>Gets memory regions.</summary>
	public IReadOnlyList<MemoryRegion> MemoryRegions => _memoryRegions;

	/// <summary>Gets cross-references.</summary>
	public IReadOnlyList<CrossReference> CrossReferences => _crossRefs;

	/// <summary>Gets the project name.</summary>
	public string ProjectName => _projectName;

	/// <summary>Gets the author.</summary>
	public string Author => _author;

	/// <summary>Gets the project version.</summary>
	public string ProjectVersion => _projectVersion;
	#endregion

	/// <summary>
	/// Creates a Pansy loader from raw data.
	/// </summary>
	/// <param name="data">The raw Pansy file bytes.</param>
	public PansyLoader(byte[] data) {
		_data = data;

		// Validate magic
		if (data.Length < 32 ||
			data[0] != 'P' || data[1] != 'A' || data[2] != 'N' ||
			data[3] != 'S' || data[4] != 'Y') {
			throw new InvalidDataException("Invalid Pansy file: bad magic number");
		}

		// Parse header
		_version = BitConverter.ToUInt16(data, 8);
		_flags = (PansyFlags)BitConverter.ToUInt16(data, 10);
		_platform = data[12];
		_romSize = BitConverter.ToUInt32(data, 16);
		_romCrc32 = BitConverter.ToUInt32(data, 20);
		var sectionCount = BitConverter.ToUInt32(data, 24);

		// Parse section table
		var tableOffset = 32;
		for (int i = 0; i < sectionCount; i++) {
			var type = BitConverter.ToUInt32(data, tableOffset);
			var offset = BitConverter.ToUInt32(data, tableOffset + 4);
			var compSize = BitConverter.ToUInt32(data, tableOffset + 8);
			var uncompSize = BitConverter.ToUInt32(data, tableOffset + 12);
			_sections.Add(new SectionInfo(type, offset, compSize, uncompSize));
			tableOffset += 16;
		}

		// Parse sections
		foreach (var section in _sections) {
			ParseSection(section);
		}
	}

	/// <summary>
	/// Loads a Pansy file from disk.
	/// </summary>
	/// <param name="path">Path to the Pansy file.</param>
	/// <returns>A new PansyLoader instance.</returns>
	public static PansyLoader Load(string path) {
		var data = File.ReadAllBytes(path);
		return new PansyLoader(data);
	}

	/// <summary>
	/// Checks if a ROM offset is marked as code.
	/// </summary>
	public bool IsCode(int offset) => _codeOffsets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is marked as data.
	/// </summary>
	public bool IsData(int offset) => _dataOffsets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is a jump target.
	/// </summary>
	public bool IsJumpTarget(int offset) => _jumpTargets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is a subroutine entry point.
	/// </summary>
	public bool IsSubEntryPoint(int offset) => _subEntryPoints.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is an opcode.
	/// </summary>
	public bool IsOpcode(int offset) => _opcodeOffsets.Contains(offset);

	/// <summary>
	/// Gets the symbol at an address, or null if none.
	/// </summary>
	public string? GetSymbol(int address) => _symbols.GetValueOrDefault(address);

	/// <summary>
	/// Gets the comment at an address, or null if none.
	/// </summary>
	public string? GetComment(int address) => _comments.GetValueOrDefault(address);

	/// <summary>
	/// Gets coverage statistics.
	/// </summary>
	public (int CodeBytes, int DataBytes, int TotalSize, double CoveragePercent) GetCoverageStats() {
		var totalSize = (int)_romSize;
		var totalMarked = _codeOffsets.Count + _dataOffsets.Count;
		var coverage = totalSize > 0 ? (totalMarked * 100.0) / totalSize : 0;
		return (_codeOffsets.Count, _dataOffsets.Count, totalSize, coverage);
	}

	/// <summary>
	/// Gets platform name from ID.
	/// </summary>
	public static string GetPlatformName(byte platformId) => platformId switch {
		PLATFORM_NES => "NES",
		PLATFORM_SNES => "SNES",
		PLATFORM_GB => "Game Boy",
		PLATFORM_GBA => "Game Boy Advance",
		PLATFORM_GENESIS => "Sega Genesis",
		PLATFORM_SMS => "Sega Master System",
		PLATFORM_PCE => "TurboGrafx-16",
		PLATFORM_ATARI_2600 => "Atari 2600",
		PLATFORM_LYNX => "Atari Lynx",
		PLATFORM_WONDERSWAN => "WonderSwan",
		PLATFORM_NEOGEO => "Neo Geo",
		PLATFORM_SPC700 => "SPC700",
		_ => "Unknown"
	};

	/// <summary>
	/// Decompresses section data if needed.
	/// </summary>
	private byte[] GetSectionData(SectionInfo section) {
		var compressedData = _data.AsSpan((int)section.Offset, (int)section.CompressedSize);

		// Check if compressed (different sizes)
		if (section.CompressedSize != section.UncompressedSize && _flags.HasFlag(PansyFlags.Compressed)) {
			try {
				using var compStream = new MemoryStream(compressedData.ToArray());
				using var deflate = new DeflateStream(compStream, CompressionMode.Decompress);
				using var result = new MemoryStream();
				deflate.CopyTo(result);
				return result.ToArray();
			} catch {
				// If decompression fails, return raw data
				return compressedData.ToArray();
			}
		}

		return compressedData.ToArray();
	}

	/// <summary>
	/// Parses a section based on its type.
	/// </summary>
	private void ParseSection(SectionInfo section) {
		var data = GetSectionData(section);

		switch (section.Type) {
			case SECTION_CODE_DATA_MAP:
				ParseCodeDataMap(data);
				break;
			case SECTION_SYMBOLS:
				ParseSymbols(data);
				break;
			case SECTION_COMMENTS:
				ParseComments(data);
				break;
			case SECTION_MEMORY_REGIONS:
				ParseMemoryRegions(data);
				break;
			case SECTION_CROSS_REFS:
				ParseCrossRefs(data);
				break;
			case SECTION_METADATA:
				ParseMetadata(data);
				break;
			// DATA_TYPES and SOURCE_MAP are reserved for future use
		}
	}

	private void ParseCodeDataMap(byte[] data) {
		_codeDataMap = data;

		for (int i = 0; i < data.Length; i++) {
			var flags = data[i];
			if (flags == 0) continue;

			if ((flags & FLAG_CODE) != 0)
				_codeOffsets.Add(i);

			if ((flags & FLAG_DATA) != 0)
				_dataOffsets.Add(i);

			if ((flags & FLAG_JUMP_TARGET) != 0)
				_jumpTargets.Add(i);

			if ((flags & FLAG_SUB_ENTRY) != 0)
				_subEntryPoints.Add(i);

			if ((flags & FLAG_OPCODE) != 0)
				_opcodeOffsets.Add(i);
		}
	}

	private void ParseSymbols(byte[] data) {
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms, Encoding.UTF8);

		while (ms.Position < ms.Length) {
			try {
				var addr24 = reader.ReadUInt32();
				var address = (int)(addr24 & 0xffffff);
				var type = (SymbolType)reader.ReadByte();
				var flags = reader.ReadByte();
				var nameLength = reader.ReadUInt16();
				var name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
				var valueLength = reader.ReadUInt16();
				if (valueLength > 0) {
					reader.ReadBytes(valueLength); // Skip value for now
				}

				_symbols[address] = name;
			} catch (EndOfStreamException) {
				break;
			}
		}
	}

	private void ParseComments(byte[] data) {
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms, Encoding.UTF8);

		while (ms.Position < ms.Length) {
			try {
				var address = (int)reader.ReadUInt32();
				var type = reader.ReadByte();
				var length = reader.ReadUInt16();
				var text = Encoding.UTF8.GetString(reader.ReadBytes(length));

				_comments[address] = text;
			} catch (EndOfStreamException) {
				break;
			}
		}
	}

	private void ParseMemoryRegions(byte[] data) {
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms, Encoding.UTF8);

		while (ms.Position < ms.Length) {
			try {
				var start = reader.ReadUInt32();
				var end = reader.ReadUInt32();
				var type = reader.ReadByte();
				var bank = reader.ReadByte();
				var flags = reader.ReadUInt16();
				var nameLength = reader.ReadUInt16();
				var name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

				_memoryRegions.Add(new MemoryRegion(start, end, type, bank, name));
			} catch (EndOfStreamException) {
				break;
			}
		}
	}

	private void ParseCrossRefs(byte[] data) {
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms, Encoding.UTF8);

		while (ms.Position < ms.Length) {
			try {
				var from = reader.ReadUInt32();
				var to = reader.ReadUInt32();
				var type = (CrossRefType)reader.ReadByte();

				_crossRefs.Add(new CrossReference(from, to, type));
			} catch (EndOfStreamException) {
				break;
			}
		}
	}

	private void ParseMetadata(byte[] data) {
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms, Encoding.UTF8);

		try {
			var nameLength = reader.ReadUInt16();
			_projectName = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

			var authorLength = reader.ReadUInt16();
			_author = Encoding.UTF8.GetString(reader.ReadBytes(authorLength));

			var versionLength = reader.ReadUInt16();
			_projectVersion = Encoding.UTF8.GetString(reader.ReadBytes(versionLength));

			// Timestamps are ignored for now
		} catch (EndOfStreamException) {
			// OK if metadata is truncated
		}
	}
}
