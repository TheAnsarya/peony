namespace Peony.Core;

/// <summary>
/// Analyzes decoded instructions to extract data references from operands.
/// This is the core intelligence behind Phase 6 of the static analysis pipeline.
/// Can be used independently of StaticAnalyzer for targeted instruction analysis.
/// </summary>
public class InstructionAnalyzer {
	private readonly ICpuDecoder _decoder;
	private readonly IPlatformAnalyzer _platform;

	public InstructionAnalyzer(ICpuDecoder decoder, IPlatformAnalyzer platform) {
		_decoder = decoder;
		_platform = platform;
	}

	/// <summary>
	/// Scan a ROM range for data references from code instructions.
	/// Only analyzes offsets where the classification map indicates Code.
	/// </summary>
	/// <param name="rom">The full ROM data.</param>
	/// <param name="classificationMap">Per-byte classification map (from StaticAnalyzer or manual).</param>
	/// <param name="start">Start offset (inclusive).</param>
	/// <param name="end">End offset (exclusive).</param>
	/// <returns>List of discovered data references.</returns>
	public IReadOnlyList<DataReference> FindDataReferences(
		ReadOnlySpan<byte> rom,
		ByteClassification[] classificationMap,
		int start,
		int end) {
		var refs = new List<DataReference>();
		int i = start;

		while (i < end && i < rom.Length) {
			if (!classificationMap[i].HasFlag(ByteClassification.Code)) {
				i++;
				continue;
			}

			uint address = _platform.OffsetToAddress(i) ?? (uint)i;
			DecodedInstruction instr;
			try {
				instr = _decoder.Decode(rom[i..], address);
			} catch {
				i++;
				continue;
			}

			if (instr.Bytes.Length == 0) {
				i++;
				continue;
			}

			var dataRef = GetDataReference(instr, address, i);
			if (dataRef is not null) {
				refs.Add(dataRef);
			}

			i += instr.Bytes.Length;
		}

		return refs;
	}

	/// <summary>
	/// Analyze a single decoded instruction to determine if it references data.
	/// </summary>
	/// <param name="instruction">The decoded instruction.</param>
	/// <param name="address">The CPU address of the instruction.</param>
	/// <param name="offset">The ROM file offset of the instruction.</param>
	/// <returns>A DataReference if the instruction references data, null otherwise.</returns>
	public DataReference? GetDataReference(DecodedInstruction instruction, uint address, int offset) {
		if (!IsDataAddressingMode(instruction.Mode))
			return null;

		uint? target = ExtractTargetAddress(instruction);
		if (target is null)
			return null;

		// Skip hardware register accesses — those aren't data in the ROM
		if (_platform.GetRegisterLabel(target.Value) is not null)
			return null;

		var refType = GetRefType(instruction.Mnemonic, instruction.Mode);
		if (refType is null)
			return null;

		return new DataReference(offset, address, target.Value, refType.Value);
	}

	/// <summary>
	/// Check if an addressing mode implies a memory address reference.
	/// </summary>
	public static bool IsDataAddressingMode(AddressingMode mode) => mode switch {
		AddressingMode.Absolute => true,
		AddressingMode.AbsoluteX => true,
		AddressingMode.AbsoluteY => true,
		AddressingMode.ZeroPage => true,
		AddressingMode.ZeroPageX => true,
		AddressingMode.ZeroPageY => true,
		AddressingMode.Indirect => true,
		AddressingMode.IndirectX => true,
		AddressingMode.IndirectY => true,
		AddressingMode.AbsoluteLong => true,
		AddressingMode.AbsoluteLongX => true,
		_ => false,
	};

	/// <summary>
	/// Extract the target address from instruction bytes based on addressing mode.
	/// </summary>
	public static uint? ExtractTargetAddress(DecodedInstruction instr) {
		if (instr.Bytes.Length < 2)
			return null;

		return instr.Mode switch {
			AddressingMode.ZeroPage or AddressingMode.ZeroPageX or AddressingMode.ZeroPageY
				=> instr.Bytes[1],

			AddressingMode.Absolute or AddressingMode.AbsoluteX or AddressingMode.AbsoluteY
			or AddressingMode.Indirect or AddressingMode.IndirectX or AddressingMode.IndirectY
				when instr.Bytes.Length >= 3
				=> (uint)(instr.Bytes[1] | (instr.Bytes[2] << 8)),

			AddressingMode.AbsoluteLong or AddressingMode.AbsoluteLongX
				when instr.Bytes.Length >= 4
				=> (uint)(instr.Bytes[1] | (instr.Bytes[2] << 8) | (instr.Bytes[3] << 16)),

			_ => null,
		};
	}

	/// <summary>
	/// Determine the reference type based on instruction mnemonic and addressing mode.
	/// </summary>
	public static DataRefType? GetRefType(string mnemonic, AddressingMode mode) {
		var lower = mnemonic.ToLowerInvariant();

		// Code references
		if (lower is "jsr" or "call")
			return DataRefType.Call;
		if (lower is "jmp" or "bra" or "brl") {
			return mode == AddressingMode.Indirect ? DataRefType.Indirect : DataRefType.Jump;
		}
		if (lower is "bne" or "beq" or "bcc" or "bcs" or "bpl" or "bmi" or "bvc" or "bvs"
			or "jr" or "jp")
			return DataRefType.Branch;

		// Data read references
		if (lower is "lda" or "ldx" or "ldy" or "ld" or "cmp" or "cpx" or "cpy"
			or "adc" or "sbc" or "and" or "ora" or "eor" or "bit")
			return DataRefType.Read;

		// Data write references
		if (lower is "sta" or "stx" or "sty" or "stz")
			return DataRefType.Write;

		return null;
	}
}
