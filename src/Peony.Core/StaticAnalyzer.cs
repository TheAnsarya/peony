using Pansy.Core;

namespace Peony.Core;

/// <summary>
/// Classification flags for a ROM byte, determined by static analysis.
/// Multiple flags can be set simultaneously.
/// </summary>
[Flags]
public enum ByteClassification : byte {
	/// <summary>Not yet classified by any source.</summary>
	Unknown = 0x00,
	/// <summary>Executable code (instruction bytes).</summary>
	Code = 0x01,
	/// <summary>Generic data (tables, lookup values).</summary>
	Data = 0x02,
	/// <summary>Graphics/CHR tile data (CDL DRAWN flag).</summary>
	Graphics = 0x04,
	/// <summary>Part of a pointer table (address entries).</summary>
	Pointer = 0x08,
	/// <summary>Text/string data.</summary>
	Text = 0x10,
	/// <summary>Interrupt/reset vector entry.</summary>
	Vector = 0x20,
	/// <summary>Padding or filler bytes ($00 or $ff runs).</summary>
	Padding = 0x40,
	/// <summary>Hardware register address space.</summary>
	Hardware = 0x80,
}

/// <summary>
/// Identifies what data source determined a byte's classification.
/// </summary>
public enum ClassificationSource : byte {
	/// <summary>Not yet classified.</summary>
	Unknown,
	/// <summary>CDL flags from emulator — highest confidence.</summary>
	Cdl,
	/// <summary>Pansy code/data map flags (IsCode, IsData, IsJumpTarget, etc.).</summary>
	PansyCodeMap,
	/// <summary>Pansy cross-reference data.</summary>
	PansyCrossRef,
	/// <summary>Pansy symbol type.</summary>
	PansySymbol,
	/// <summary>Pansy memory region.</summary>
	PansyRegion,
	/// <summary>ROM interrupt/reset vector.</summary>
	RomVector,
	/// <summary>Instruction operand target analysis.</summary>
	OperandTrace,
	/// <summary>Platform memory map knowledge base.</summary>
	PlatformMap,
}

/// <summary>
/// A contiguous region with uniform classification.
/// </summary>
public record ClassifiedRegion(
	int StartOffset,
	int EndOffset,
	ByteClassification Classification,
	ClassificationSource Source,
	string? Annotation
);

/// <summary>
/// A data reference discovered from instruction operand analysis.
/// </summary>
public record DataReference(
	int InstructionOffset,
	uint InstructionAddress,
	uint TargetAddress,
	DataRefType RefType
);

/// <summary>
/// How an instruction references an address.
/// </summary>
public enum DataRefType {
	/// <summary>Read access (LDA, LDX, LDY).</summary>
	Read,
	/// <summary>Write access (STA, STX, STY).</summary>
	Write,
	/// <summary>Jump (JMP, BRA).</summary>
	Jump,
	/// <summary>Subroutine call (JSR).</summary>
	Call,
	/// <summary>Conditional branch (BNE, BEQ, etc.).</summary>
	Branch,
	/// <summary>Indirect reference (JMP ($xxxx)).</summary>
	Indirect,
}

/// <summary>
/// Statistics about classification coverage.
/// </summary>
public record ClassificationStats(
	int TotalBytes,
	int CodeBytes,
	int DataBytes,
	int GraphicsBytes,
	int VectorBytes,
	int UnknownBytes,
	int CdlClassified,
	int PansyClassified,
	int VectorClassified,
	int OperandClassified,
	int PlatformClassified
) {
	public double CoveragePercent => TotalBytes > 0
		? (TotalBytes - UnknownBytes) * 100.0 / TotalBytes
		: 0;
}

/// <summary>
/// Result of static analysis classification for a ROM.
/// </summary>
public sealed class ClassificationResult {
	/// <summary>Per-byte classification flags.</summary>
	public ByteClassification[] Map { get; }

	/// <summary>Classification source for each byte.</summary>
	public ClassificationSource[] Sources { get; }

	/// <summary>Data references found from instruction operands.</summary>
	public List<DataReference> DataReferences { get; } = [];

	/// <summary>Classification statistics.</summary>
	public ClassificationStats Stats { get; internal set; }

	public ClassificationResult(int romSize) {
		Map = new ByteClassification[romSize];
		Sources = new ClassificationSource[romSize];
		Stats = new ClassificationStats(romSize, 0, 0, 0, 0, romSize, 0, 0, 0, 0, 0);
	}

	/// <summary>
	/// Build contiguous regions from the per-byte map.
	/// </summary>
	public IReadOnlyList<ClassifiedRegion> GetRegions() {
		if (Map.Length == 0)
			return [];

		var regions = new List<ClassifiedRegion>();
		var currentClass = Map[0];
		var currentSource = Sources[0];
		int regionStart = 0;

		for (int i = 1; i < Map.Length; i++) {
			if (Map[i] != currentClass || Sources[i] != currentSource) {
				regions.Add(new ClassifiedRegion(
					regionStart, i - 1, currentClass, currentSource, null));
				regionStart = i;
				currentClass = Map[i];
				currentSource = Sources[i];
			}
		}

		regions.Add(new ClassifiedRegion(
			regionStart, Map.Length - 1, currentClass, currentSource, null));

		return regions;
	}
}

/// <summary>
/// Deterministic static analyzer that classifies ROM bytes using authoritative data
/// sources (CDL, Pansy, instruction analysis) instead of heuristic percentages.
///
/// Classification priority (highest → lowest):
/// 1. CDL flags from emulator (CODE, DATA, DRAWN)
/// 2. Pansy cross-references
/// 3. Pansy symbols with types
/// 4. Pansy memory regions
/// 5. ROM vectors (reset, NMI, IRQ)
/// 6. Instruction operand analysis
/// 7. Platform memory map knowledge
///
/// Bytes that cannot be classified by any authoritative source remain Unknown.
/// </summary>
public sealed class StaticAnalyzer {
	private readonly IPlatformAnalyzer _platform;
	private readonly SymbolLoader? _symbolLoader;
	private readonly InstructionAnalyzer _instructionAnalyzer;

	public StaticAnalyzer(IPlatformAnalyzer platform, SymbolLoader? symbolLoader = null) {
		_platform = platform;
		_symbolLoader = symbolLoader;
		_instructionAnalyzer = new InstructionAnalyzer(platform.CpuDecoder, platform);
	}

	/// <summary>
	/// Classify all bytes in the ROM using authoritative data sources.
	/// </summary>
	public ClassificationResult Classify(ReadOnlySpan<byte> rom) {
		var result = new ClassificationResult(rom.Length);

		// Priority cascade: each phase only sets bytes that are still Unknown
		ApplyCdlClassification(result, rom);
		ApplyPansyCodeDataMapClassification(result, rom);
		ApplyCrossRefClassification(result);
		ApplySymbolClassification(result);
		ApplyMemoryRegionClassification(result);
		ApplyVectorClassification(result, rom);
		ApplyOperandAnalysis(result, rom);
		ApplyPlatformDefaults(result);

		// Compute final statistics
		result.Stats = ComputeStats(result);

		return result;
	}

	/// <summary>
	/// Phase 1: Apply CDL flags — the most authoritative source.
	/// CDL CODE → Code, CDL DATA → Data, CDL DRAWN → Graphics.
	/// </summary>
	private void ApplyCdlClassification(ClassificationResult result, ReadOnlySpan<byte> rom) {
		var cdl = _symbolLoader?.CdlData;
		if (cdl is null)
			return;

		for (int i = 0; i < rom.Length; i++) {
			if (cdl.IsCode(i)) {
				SetClassification(result, i, ByteClassification.Code, ClassificationSource.Cdl);
			} else if (cdl.IsData(i)) {
				SetClassification(result, i, ByteClassification.Data, ClassificationSource.Cdl);
			}
		}

		// Also check Pansy loader for Drawn (graphics) offsets
		var pansy = _symbolLoader?.PansyData;
		if (pansy is not null) {
			for (int i = 0; i < rom.Length; i++) {
				if (pansy.IsDrawn(i)) {
					SetClassification(result, i, ByteClassification.Graphics, ClassificationSource.Cdl);
				}
			}
		}
	}

	/// <summary>
	/// Phase 1b: Apply Pansy code/data map flags directly.
	/// Uses IsCode, IsData, IsJumpTarget, IsSubEntryPoint, IsRead, IsIndirect flags.
	/// This runs after CDL (which has higher authority from live emulation) but before
	/// cross-reference analysis, providing direct classification from prior analysis.
	/// </summary>
	private void ApplyPansyCodeDataMapClassification(ClassificationResult result, ReadOnlySpan<byte> rom) {
		var pansy = _symbolLoader?.PansyData;
		if (pansy is null || !pansy.HasCodeDataMap)
			return;

		for (int i = 0; i < rom.Length; i++) {
			if (result.Sources[i] != ClassificationSource.Unknown)
				continue;

			if (pansy.IsCode(i)) {
				SetClassification(result, i, ByteClassification.Code, ClassificationSource.PansyCodeMap);
			} else if (pansy.IsData(i)) {
				SetClassification(result, i, ByteClassification.Data, ClassificationSource.PansyCodeMap);
			}
		}

		// Mark jump targets (may overlap with already-classified code bytes)
		foreach (var offset in pansy.JumpTargets) {
			if (offset >= 0 && offset < rom.Length && result.Sources[offset] == ClassificationSource.Unknown) {
				SetClassification(result, offset, ByteClassification.Code, ClassificationSource.PansyCodeMap);
			}
		}

		// Mark sub-entry-points
		foreach (var offset in pansy.SubEntryPoints) {
			if (offset >= 0 && offset < rom.Length && result.Sources[offset] == ClassificationSource.Unknown) {
				SetClassification(result, offset, ByteClassification.Code, ClassificationSource.PansyCodeMap);
			}
		}

		// Mark read-accessed offsets as data (if not already classified)
		foreach (var offset in pansy.ReadOffsets) {
			if (offset >= 0 && offset < rom.Length && result.Sources[offset] == ClassificationSource.Unknown) {
				SetClassification(result, offset, ByteClassification.Data, ClassificationSource.PansyCodeMap);
			}
		}

		// Mark drawn offsets as graphics (if not already set by CDL)
		foreach (var offset in pansy.DrawnOffsets) {
			if (offset >= 0 && offset < rom.Length && result.Sources[offset] == ClassificationSource.Unknown) {
				SetClassification(result, offset, ByteClassification.Graphics, ClassificationSource.PansyCodeMap);
			}
		}
	}

	/// <summary>
	/// Phase 2: Apply Pansy cross-reference data.
	/// JSR/JMP/Branch targets → Code. Read/Write targets → Data.
	/// </summary>
	private void ApplyCrossRefClassification(ClassificationResult result) {
		var pansy = _symbolLoader?.PansyData;
		if (pansy is null)
			return;

		foreach (var xref in pansy.CrossReferences) {
			int targetOffset = _platform.AddressToOffset(xref.To, result.Map.Length);
			if (targetOffset < 0 || targetOffset >= result.Map.Length)
				continue;
			if (result.Sources[targetOffset] != ClassificationSource.Unknown)
				continue;

			var classification = xref.Type switch {
				Pansy.Core.CrossRefType.Jsr => ByteClassification.Code,
				Pansy.Core.CrossRefType.Jmp => ByteClassification.Code,
				Pansy.Core.CrossRefType.Branch => ByteClassification.Code,
				Pansy.Core.CrossRefType.Read => ByteClassification.Data,
				Pansy.Core.CrossRefType.Write => ByteClassification.Data,
				_ => ByteClassification.Unknown,
			};

			if (classification != ByteClassification.Unknown) {
				SetClassification(result, targetOffset, classification, ClassificationSource.PansyCrossRef);
			}
		}
	}

	/// <summary>
	/// Phase 3: Apply Pansy symbol types.
	/// Function/InterruptVector → Code. Constant → Data.
	/// </summary>
	private void ApplySymbolClassification(ClassificationResult result) {
		var pansy = _symbolLoader?.PansyData;
		if (pansy is null)
			return;

		foreach (var (address, entry) in pansy.SymbolEntries) {
			int offset = _platform.AddressToOffset((uint)address, result.Map.Length);
			if (offset < 0 || offset >= result.Map.Length)
				continue;
			if (result.Sources[offset] != ClassificationSource.Unknown)
				continue;

			var classification = entry.Type switch {
				SymbolType.Function => ByteClassification.Code,
				SymbolType.InterruptVector => ByteClassification.Code | ByteClassification.Vector,
				SymbolType.Label => ByteClassification.Code,  // Labels typically mark code
				SymbolType.Constant => ByteClassification.Data,
				_ => ByteClassification.Unknown,
			};

			if (classification != ByteClassification.Unknown) {
				SetClassification(result, offset, classification, ClassificationSource.PansySymbol);
			}
		}
	}

	/// <summary>
	/// Phase 4: Apply Pansy memory region types.
	/// VRAM → Graphics, IO → Hardware, RAM/SRAM/WRAM → Data.
	/// </summary>
	private void ApplyMemoryRegionClassification(ClassificationResult result) {
		var pansy = _symbolLoader?.PansyData;
		if (pansy is null)
			return;

		foreach (var region in pansy.MemoryRegions) {
			var classification = region.Type switch {
				(byte)MemoryRegionType.VRAM => ByteClassification.Graphics,
				(byte)MemoryRegionType.IO => ByteClassification.Hardware,
				(byte)MemoryRegionType.RAM => ByteClassification.Data,
				(byte)MemoryRegionType.SRAM => ByteClassification.Data,
				(byte)MemoryRegionType.WRAM => ByteClassification.Data,
				_ => ByteClassification.Unknown,
			};

			if (classification == ByteClassification.Unknown)
				continue;

			// Apply to all offsets in this region
			for (uint addr = region.Start; addr <= region.End; addr++) {
				int offset = _platform.AddressToOffset(addr, result.Map.Length);
				if (offset < 0 || offset >= result.Map.Length)
					continue;
				if (result.Sources[offset] != ClassificationSource.Unknown)
					continue;

				SetClassification(result, offset, classification, ClassificationSource.PansyRegion);
			}
		}
	}

	/// <summary>
	/// Phase 5: Apply ROM vector classification.
	/// Platform entry points (reset, NMI, IRQ vectors) are pointers to code.
	/// </summary>
	private void ApplyVectorClassification(ClassificationResult result, ReadOnlySpan<byte> rom) {
		var vectors = PlatformMemoryMap.GetVectors(_platform.Platform);
		foreach (var vector in vectors) {
			// The vector itself is a pointer (data)
			int vectorOffset = _platform.AddressToOffset(vector.Address, rom.Length);
			for (int i = 0; i < vector.Size && vectorOffset + i >= 0 && vectorOffset + i < rom.Length; i++) {
				if (result.Sources[vectorOffset + i] != ClassificationSource.Unknown)
					continue;
				SetClassification(result, vectorOffset + i,
					ByteClassification.Vector | ByteClassification.Pointer,
					ClassificationSource.RomVector);
			}

			// The address pointed to by the vector is code
			if (vectorOffset >= 0 && vectorOffset + 1 < rom.Length) {
				uint target = (uint)(rom[vectorOffset] | (rom[vectorOffset + 1] << 8));
				int targetOffset = _platform.AddressToOffset(target, rom.Length);
				if (targetOffset >= 0 && targetOffset < rom.Length
					&& result.Sources[targetOffset] == ClassificationSource.Unknown) {
					SetClassification(result, targetOffset,
						ByteClassification.Code,
						ClassificationSource.RomVector);
				}
			}
		}
	}

	/// <summary>
	/// Phase 6: Analyze instruction operands in classified code regions to find data refs.
	/// </summary>
	private void ApplyOperandAnalysis(ClassificationResult result, ReadOnlySpan<byte> rom) {
		var refs = _instructionAnalyzer.FindDataReferences(rom, result.Map, 0, rom.Length);

		foreach (var dataRef in refs) {
			result.DataReferences.Add(dataRef);

			// Mark the target as data if it's still unknown
			int targetOffset = _platform.AddressToOffset(dataRef.TargetAddress, rom.Length);
			if (targetOffset >= 0 && targetOffset < rom.Length
				&& result.Sources[targetOffset] == ClassificationSource.Unknown) {
				var targetClass = dataRef.RefType switch {
					DataRefType.Read => ByteClassification.Data,
					DataRefType.Write => ByteClassification.Data,
					DataRefType.Call => ByteClassification.Code,
					DataRefType.Jump => ByteClassification.Code,
					DataRefType.Branch => ByteClassification.Code,
					DataRefType.Indirect => ByteClassification.Pointer,
					_ => ByteClassification.Unknown,
				};

				if (targetClass != ByteClassification.Unknown) {
					SetClassification(result, targetOffset, targetClass, ClassificationSource.OperandTrace);
				}
			}
		}
	}

	/// <summary>
	/// Phase 7: Apply platform-specific defaults for known address ranges.
	/// </summary>
	private void ApplyPlatformDefaults(ClassificationResult result) {
		for (int i = 0; i < result.Map.Length; i++) {
			if (result.Sources[i] != ClassificationSource.Unknown)
				continue;

			uint address = _platform.OffsetToAddress(i) ?? (uint)i;
			var known = PlatformMemoryMap.GetKnownClassification(_platform.Platform, address);
			if (known.HasValue) {
				SetClassification(result, i, known.Value, ClassificationSource.PlatformMap);
			}
		}
	}

	/// <summary>
	/// Set classification for a byte, only if it hasn't been set by a higher-priority source.
	/// </summary>
	private static void SetClassification(ClassificationResult result, int offset,
		ByteClassification classification, ClassificationSource source) {
		if (offset < 0 || offset >= result.Map.Length)
			return;
		if (result.Sources[offset] != ClassificationSource.Unknown)
			return;

		result.Map[offset] = classification;
		result.Sources[offset] = source;
	}

	/// <summary>
	/// Compute classification statistics.
	/// </summary>
	private static ClassificationStats ComputeStats(ClassificationResult result) {
		int code = 0, data = 0, graphics = 0, vector = 0, unknown = 0;
		int cdl = 0, pansy = 0, vec = 0, operand = 0, platform = 0;

		for (int i = 0; i < result.Map.Length; i++) {
			if (result.Map[i].HasFlag(ByteClassification.Code)) code++;
			if (result.Map[i].HasFlag(ByteClassification.Data)) data++;
			if (result.Map[i].HasFlag(ByteClassification.Graphics)) graphics++;
			if (result.Map[i].HasFlag(ByteClassification.Vector)) vector++;
			if (result.Map[i] == ByteClassification.Unknown) unknown++;

			switch (result.Sources[i]) {
				case ClassificationSource.Cdl: cdl++; break;
				case ClassificationSource.PansyCodeMap:
				case ClassificationSource.PansyCrossRef:
				case ClassificationSource.PansySymbol:
				case ClassificationSource.PansyRegion:
					pansy++; break;
				case ClassificationSource.RomVector: vec++; break;
				case ClassificationSource.OperandTrace: operand++; break;
				case ClassificationSource.PlatformMap: platform++; break;
			}
		}

		return new ClassificationStats(
			result.Map.Length, code, data, graphics, vector, unknown,
			cdl, pansy, vec, operand, platform);
	}
}
