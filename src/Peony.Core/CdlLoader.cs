namespace Peony.Core;

/// <summary>
/// Loads CDL (Code/Data Log) files from emulators like FCEUX and Mesen.
/// CDL files mark which ROM bytes have been executed as code vs read as data,
/// improving disassembly accuracy.
/// </summary>
public sealed class CdlLoader {
	private readonly byte[] _cdlData;
	private readonly CdlFormat _format;
	private readonly HashSet<int> _codeOffsets = [];
	private readonly HashSet<int> _dataOffsets = [];
	private readonly HashSet<int> _jumpTargets = [];
	private readonly HashSet<int> _subEntryPoints = [];

	// CDL Flag definitions (FCEUX format)
	private const byte FCEUX_CODE = 0x01;
	private const byte FCEUX_DATA = 0x02;
	private const byte FCEUX_PCM_AUDIO = 0x40;
	private const byte FCEUX_INDIRECT_CODE = 0x10;   // Sub entry point (accessed indirectly)
	private const byte FCEUX_INDIRECT_DATA = 0x20;   // Indexed data

	// CDL Flag definitions (Mesen format)
	private const byte MESEN_CODE = 0x01;
	private const byte MESEN_DATA = 0x02;
	private const byte MESEN_JUMP_TARGET = 0x04;
	private const byte MESEN_SUB_ENTRY_POINT = 0x08;
	private const byte MESEN_DRAWN = 0x10;
	private const byte MESEN_READ = 0x20;

	/// <summary>
	/// Supported CDL formats.
	/// </summary>
	public enum CdlFormat {
		/// <summary>Unknown format</summary>
		Unknown,
		/// <summary>FCEUX CDL format (raw bytes, no header)</summary>
		FCEUX,
		/// <summary>Mesen CDL format ("CDL\x01" header + bytes)</summary>
		Mesen,
		/// <summary>Raw Mesen CDL format (no header, Mesen flag encoding)</summary>
		MesenRaw,
		/// <summary>Mesen2 CDL format ("CDLv2" header + CRC32 + bytes)</summary>
		MesenV2,
		/// <summary>bsnes CDL format</summary>
		Bsnes
	}

	/// <summary>
	/// Gets all ROM offsets marked as code.
	/// </summary>
	public IReadOnlySet<int> CodeOffsets => _codeOffsets;

	/// <summary>
	/// Gets all ROM offsets marked as data.
	/// </summary>
	public IReadOnlySet<int> DataOffsets => _dataOffsets;

	/// <summary>
	/// Gets all ROM offsets that are jump targets.
	/// </summary>
	public IReadOnlySet<int> JumpTargets => _jumpTargets;

	/// <summary>
	/// Gets all ROM offsets that are subroutine entry points.
	/// </summary>
	public IReadOnlySet<int> SubEntryPoints => _subEntryPoints;

	/// <summary>
	/// Gets the detected CDL format.
	/// </summary>
	public CdlFormat Format => _format;

	/// <summary>
	/// Creates a CDL loader from raw data.
	/// </summary>
	/// <param name="cdlData">The raw CDL file bytes.</param>
	/// <param name="platformHint">Optional platform hint (for example, "SNES") for format detection.</param>
	public CdlLoader(byte[] cdlData, string? platformHint = null) {
		_cdlData = cdlData;
		_format = DetectFormat(cdlData, platformHint);
		Parse();
	}

	/// <summary>
	/// Loads a CDL file from disk.
	/// </summary>
	/// <param name="path">Path to the CDL file.</param>
	/// <param name="platformHint">Optional platform hint (for example, "SNES") for format detection.</param>
	/// <returns>A new CdlLoader instance.</returns>
	public static CdlLoader Load(string path, string? platformHint = null) {
		var data = File.ReadAllBytes(path);
		return new CdlLoader(data, platformHint);
	}

	/// <summary>
	/// Checks if a ROM offset is marked as code.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is marked as code.</returns>
	public bool IsCode(int offset) => _codeOffsets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is marked as data.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is marked as data.</returns>
	public bool IsData(int offset) => _dataOffsets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is a jump target (branch destination).
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is a jump target.</returns>
	public bool IsJumpTarget(int offset) => _jumpTargets.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is a subroutine entry point.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is a subroutine entry point.</returns>
	public bool IsSubEntryPoint(int offset) => _subEntryPoints.Contains(offset);

	/// <summary>
	/// Checks if a ROM offset is unreached (neither code nor data).
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset has not been reached.</returns>
	public bool IsUnreached(int offset) => !_codeOffsets.Contains(offset) && !_dataOffsets.Contains(offset);

	/// <summary>
	/// Gets the raw CDL flags byte for a ROM offset.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <param name="flags">The raw flag byte when available.</param>
	/// <returns>True when the offset is in range.</returns>
	public bool TryGetFlags(int offset, out byte flags) {
		flags = 0;
		if (offset < 0) {
			return false;
		}

		var headerSize = _format switch {
			CdlFormat.Mesen => 4,
			CdlFormat.MesenV2 => 9,
			_ => 0,
		};

		var rawIndex = headerSize + offset;
		if (rawIndex < headerSize || rawIndex >= _cdlData.Length) {
			return false;
		}

		flags = _cdlData[rawIndex];
		return true;
	}

	/// <summary>
	/// Gets SNES 65816 M/X mode bits from CDL flags for a ROM offset.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <param name="accIs8">True when M flag is set (8-bit accumulator).</param>
	/// <param name="idxIs8">True when X flag is set (8-bit index registers).</param>
	/// <returns>True when a code byte at this offset has valid M/X mode information.</returns>
	public bool TryGetSnesMxState(int offset, out bool accIs8, out bool idxIs8) {
		accIs8 = true;
		idxIs8 = true;

		if (!TryGetFlags(offset, out var flags)) {
			return false;
		}

		// M/X flags are meaningful only for executed code bytes.
		if ((flags & MESEN_CODE) == 0) {
			return false;
		}

		// SNES CDL extension bits: 0x10 = IndexMode8 (X), 0x20 = MemoryMode8 (M)
		idxIs8 = (flags & 0x10) != 0;
		accIs8 = (flags & 0x20) != 0;
		return true;
	}

	/// <summary>
	/// Gets coverage statistics.
	/// </summary>
	/// <returns>Tuple of (codeBytes, dataBytes, totalSize, coveragePercent).</returns>
	public (int CodeBytes, int DataBytes, int TotalSize, double CoveragePercent) GetCoverageStats() {
		var headerSize = _format switch {
			CdlFormat.Mesen => 4,
			CdlFormat.MesenV2 => 9,
			_ => 0
		};
		var dataSize = _cdlData.Length - headerSize;
		var totalMarked = _codeOffsets.Count + _dataOffsets.Count;
		var coverage = dataSize > 0 ? (totalMarked * 100.0) / dataSize : 0;
		return (_codeOffsets.Count, _dataOffsets.Count, dataSize, coverage);
	}

	/// <summary>
	/// Detects the CDL file format.
	/// </summary>
	private static CdlFormat DetectFormat(byte[] data, string? platformHint) {
		if (data.Length < 4)
			return CdlFormat.FCEUX;  // Too small for header

		// Check for Mesen header "CDL\x01" (old) or "CDLv2" (new)
		if (data[0] == 'C' && data[1] == 'D' && data[2] == 'L') {
			if (data[3] == 0x01)
				return CdlFormat.Mesen;
			if (data.Length >= 5 && data[3] == 'v' && data[4] == '2')
				return CdlFormat.MesenV2;
		}

		// Heuristic: detect raw Mesen CDL (no header) vs FCEUX.
		// Mesen uses bits 0x04 (JumpTarget) and 0x08 (SubEntryPoint) which FCEUX never sets.
		// FCEUX only uses: 0x01 (Code), 0x02 (Data), 0x10 (IndirectCode), 0x20 (IndirectData), 0x40 (PCMAudio).
		// If any byte has 0x04 or 0x08 set, this is raw Mesen format.
		for (int i = 0; i < Math.Min(data.Length, 65536); i++) {
			if ((data[i] & 0x0c) != 0) // 0x04 | 0x08 = JumpTarget | SubEntryPoint
				return CdlFormat.MesenRaw; // Raw Mesen format (no header)
		}

		// SNES raw CDL can use upper-nibble CPU-state flags (M/X/GSU/CX4).
		// Those overlap FCEUX meanings, so prefer MesenRaw when platform is SNES.
		if (!string.IsNullOrWhiteSpace(platformHint) &&
			platformHint.Contains("snes", StringComparison.OrdinalIgnoreCase)) {
			return CdlFormat.MesenRaw;
		}

		return CdlFormat.FCEUX;
	}

	/// <summary>
	/// Parses the CDL data into code/data sets.
	/// </summary>
	private void Parse() {
		var startOffset = 0;
		ReadOnlySpan<byte> cdl;

		if (_format == CdlFormat.Mesen) {
			// Skip "CDL\x01" header (4 bytes)
			startOffset = 4;
		} else if (_format == CdlFormat.MesenV2) {
			// Skip "CDLv2" header (5 bytes) + CRC32 (4 bytes) = 9 bytes
			startOffset = 9;
		}
		// MesenRaw has no header, startOffset stays 0

		cdl = _cdlData.AsSpan(startOffset);

		for (int i = 0; i < cdl.Length; i++) {
			var flags = cdl[i];
			if (flags == 0) continue;  // Unreached

			if (_format == CdlFormat.Mesen || _format == CdlFormat.MesenV2 || _format == CdlFormat.MesenRaw) {
				if ((flags & MESEN_CODE) != 0)
					_codeOffsets.Add(i);

				if ((flags & MESEN_DATA) != 0)
					_dataOffsets.Add(i);

				if ((flags & MESEN_JUMP_TARGET) != 0)
					_jumpTargets.Add(i);

				if ((flags & MESEN_SUB_ENTRY_POINT) != 0)
					_subEntryPoints.Add(i);
			} else {
				// FCEUX format
				if ((flags & FCEUX_CODE) != 0)
					_codeOffsets.Add(i);

				if ((flags & FCEUX_DATA) != 0)
					_dataOffsets.Add(i);

				if ((flags & FCEUX_INDIRECT_CODE) != 0)
					_subEntryPoints.Add(i);
			}
		}
	}

	/// <summary>
	/// Gets all code regions as ranges (start, end).
	/// </summary>
	/// <returns>List of (start, end) tuples for code regions.</returns>
	public List<(int Start, int End)> GetCodeRegions() {
		return GetContiguousRegions(_codeOffsets);
	}

	/// <summary>
	/// Gets all data regions as ranges (start, end).
	/// </summary>
	/// <returns>List of (start, end) tuples for data regions.</returns>
	public List<(int Start, int End)> GetDataRegions() {
		return GetContiguousRegions(_dataOffsets);
	}

	/// <summary>
	/// Converts a set of offsets into contiguous regions.
	/// </summary>
	private static List<(int Start, int End)> GetContiguousRegions(HashSet<int> offsets) {
		var regions = new List<(int Start, int End)>();
		if (offsets.Count == 0) return regions;

		var sorted = offsets.OrderBy(x => x).ToArray();
		int start = sorted[0];
		int end = sorted[0];

		for (int i = 1; i < sorted.Length; i++) {
			if (sorted[i] == end + 1) {
				end = sorted[i];
			} else {
				regions.Add((start, end));
				start = sorted[i];
				end = sorted[i];
			}
		}

		regions.Add((start, end));
		return regions;
	}
}
