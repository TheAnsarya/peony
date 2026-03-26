namespace Peony.Platform.Atari2600;

/// <summary>
/// Atari 2600 controller types detected from I/O access patterns.
/// </summary>
[Flags]
public enum ControllerType {
	/// <summary>No controller usage detected.</summary>
	Unknown = 0,

	/// <summary>Standard joystick (SWCHA directional + INPT4/5 fire button).</summary>
	Joystick = 1,

	/// <summary>Paddle controller (INPT0-3 potentiometer reads with threshold checks).</summary>
	Paddle = 2,

	/// <summary>Keypad controller (SWCHA column writes followed by INPT0-3 row reads).</summary>
	Keypad = 4,

	/// <summary>Driving controller (SWCHA reads with 2-bit gray code patterns).</summary>
	Driving = 8,
}

/// <summary>
/// Result of controller type detection for player 1 and player 2.
/// </summary>
public record ControllerDetectionResult(
	ControllerType Player1,
	ControllerType Player2,
	int SwchaReadCount,
	int SwchaWriteCount,
	int Inpt0Count,
	int Inpt1Count,
	int Inpt2Count,
	int Inpt3Count,
	int Inpt4Count,
	int Inpt5Count
);

/// <summary>
/// Detects Atari 2600 controller types by analyzing I/O register access patterns
/// in ROM code. Scans for characteristic 6502 instruction sequences that
/// read/write TIA input ports and RIOT port registers.
/// </summary>
/// <remarks>
/// <para>
/// Controller detection heuristics:
/// </para>
/// <list type="bullet">
/// <item><description>Joystick: SWCHA reads (bits 4-7 P0, bits 0-3 P1) with INPT4/5 fire button reads</description></item>
/// <item><description>Paddle: INPT0-3 reads followed by BPL/BMI threshold branches</description></item>
/// <item><description>Keypad: SWCHA writes (column select) followed by INPT0-3 reads (row scan)</description></item>
/// <item><description>Driving: Similar to joystick but uses 2-bit gray code from SWCHA</description></item>
/// </list>
/// </remarks>
public static class ControllerTypeDetector {
	// RIOT / TIA addresses (zero-page / mapped)
	private const byte SWCHA = 0x80;    // RIOT: $280 but accessed via mirrored $80 in many games
	private const ushort SWCHA_RIOT = 0x0280;
	private const byte SWACNT = 0x81;
	private const ushort SWACNT_RIOT = 0x0281;

	// TIA input ports (read registers)
	private const byte INPT0 = 0x08;
	private const byte INPT1 = 0x09;
	private const byte INPT2 = 0x0a;
	private const byte INPT3 = 0x0b;
	private const byte INPT4 = 0x0c;
	private const byte INPT5 = 0x0d;

	// 6502 opcodes for reads/writes
	private const byte LDA_ZPG = 0xa5;
	private const byte LDX_ZPG = 0xa6;
	private const byte LDY_ZPG = 0xa4;
	private const byte BIT_ZPG = 0x24;
	private const byte STA_ZPG = 0x85;
	private const byte LDA_ABS = 0xad;
	private const byte STA_ABS = 0x8d;
	private const byte BPL = 0x10;
	private const byte BMI = 0x30;

	/// <summary>
	/// Analyze ROM code to detect controller types for both players.
	/// </summary>
	public static ControllerDetectionResult Detect(ReadOnlySpan<byte> rom) {
		int swchaReads = 0;
		int swchaWrites = 0;
		int inpt0 = 0, inpt1 = 0, inpt2 = 0, inpt3 = 0;
		int inpt4 = 0, inpt5 = 0;
		int paddleThresholds = 0;
		int swacntWrites = 0;

		for (int i = 0; i < rom.Length; i++) {
			// Check for SWCHA reads (LDA/LDX/LDY/BIT $80 or LDA $0280)
			if (IsZpgRead(rom, i, SWCHA) || IsAbsRead(rom, i, SWCHA_RIOT)) {
				swchaReads++;
			}

			// Check for SWCHA writes (STA $80 or STA $0280)
			if (IsZpgWrite(rom, i, SWCHA) || IsAbsWrite(rom, i, SWCHA_RIOT)) {
				swchaWrites++;
			}

			// Check for SWACNT writes (STA $81 or STA $0281) — used by keypad
			if (IsZpgWrite(rom, i, SWACNT) || IsAbsWrite(rom, i, SWACNT_RIOT)) {
				swacntWrites++;
			}

			// Check for TIA input port reads
			if (IsZpgRead(rom, i, INPT0)) { inpt0++; CheckPaddleThreshold(rom, i, ref paddleThresholds); }
			if (IsZpgRead(rom, i, INPT1)) { inpt1++; CheckPaddleThreshold(rom, i, ref paddleThresholds); }
			if (IsZpgRead(rom, i, INPT2)) { inpt2++; CheckPaddleThreshold(rom, i, ref paddleThresholds); }
			if (IsZpgRead(rom, i, INPT3)) { inpt3++; CheckPaddleThreshold(rom, i, ref paddleThresholds); }
			if (IsZpgRead(rom, i, INPT4)) inpt4++;
			if (IsZpgRead(rom, i, INPT5)) inpt5++;
		}

		var p1 = InferControllerType(
			swchaReads, swchaWrites, swacntWrites,
			inpt0, inpt1, inpt4, paddleThresholds);

		var p2 = InferPlayer2Type(
			swchaReads, swchaWrites, swacntWrites,
			inpt2, inpt3, inpt5, paddleThresholds);

		return new ControllerDetectionResult(
			p1, p2,
			swchaReads, swchaWrites,
			inpt0, inpt1, inpt2, inpt3, inpt4, inpt5);
	}

	private static ControllerType InferControllerType(
		int swchaReads, int swchaWrites, int swacntWrites,
		int inpt0, int inpt1, int fireButtonReads, int paddleThresholds) {

		// Keypad: SWACNT writes (to configure port direction) + INPT reads for row scanning
		if (swacntWrites > 0 && swchaWrites > 0 && (inpt0 + inpt1) > 0) {
			return ControllerType.Keypad;
		}

		// Paddle: INPT0/1 reads with threshold comparisons (BPL/BMI after read)
		if ((inpt0 + inpt1) > 0 && paddleThresholds > 0) {
			return ControllerType.Paddle;
		}

		// Joystick: SWCHA reads with fire button reads, no paddle indicators
		if (swchaReads > 0 && fireButtonReads > 0) {
			return ControllerType.Joystick;
		}

		// SWCHA reads only, no fire button — could be driving
		if (swchaReads > 0 && fireButtonReads == 0) {
			return ControllerType.Driving;
		}

		return ControllerType.Unknown;
	}

	private static ControllerType InferPlayer2Type(
		int swchaReads, int swchaWrites, int swacntWrites,
		int inpt2, int inpt3, int fireButtonReads, int paddleThresholds) {

		// Keypad: same indicators but for player 2 ports
		if (swacntWrites > 0 && swchaWrites > 0 && (inpt2 + inpt3) > 0) {
			return ControllerType.Keypad;
		}

		// Paddle: INPT2/3 reads with threshold comparisons
		if ((inpt2 + inpt3) > 0 && paddleThresholds > 0) {
			return ControllerType.Paddle;
		}

		// Joystick with fire button for player 2
		if (swchaReads > 0 && fireButtonReads > 0) {
			return ControllerType.Joystick;
		}

		if (swchaReads > 0 && fireButtonReads == 0) {
			return ControllerType.Driving;
		}

		return ControllerType.Unknown;
	}

	/// <summary>
	/// Check if a paddle threshold comparison follows an INPT read.
	/// Paddle games read INPTx then use BPL or BMI to test the capacitor discharge.
	/// </summary>
	private static void CheckPaddleThreshold(ReadOnlySpan<byte> rom, int readOffset, ref int count) {
		// After LDA INPTx (2 bytes), look for BPL or BMI within next few bytes
		int checkStart = readOffset + 2;
		int checkEnd = Math.Min(checkStart + 4, rom.Length);

		for (int j = checkStart; j < checkEnd; j++) {
			if (rom[j] == BPL || rom[j] == BMI) {
				count++;
				return;
			}
		}
	}

	private static bool IsZpgRead(ReadOnlySpan<byte> rom, int offset, byte register) {
		if (offset + 1 >= rom.Length) return false;
		var opcode = rom[offset];
		return (opcode == LDA_ZPG || opcode == LDX_ZPG || opcode == LDY_ZPG || opcode == BIT_ZPG)
			   && rom[offset + 1] == register;
	}

	private static bool IsAbsRead(ReadOnlySpan<byte> rom, int offset, ushort address) {
		if (offset + 2 >= rom.Length) return false;
		return rom[offset] == LDA_ABS
			   && rom[offset + 1] == (byte)(address & 0xff)
			   && rom[offset + 2] == (byte)(address >> 8);
	}

	private static bool IsZpgWrite(ReadOnlySpan<byte> rom, int offset, byte register) {
		if (offset + 1 >= rom.Length) return false;
		return rom[offset] == STA_ZPG && rom[offset + 1] == register;
	}

	private static bool IsAbsWrite(ReadOnlySpan<byte> rom, int offset, ushort address) {
		if (offset + 2 >= rom.Length) return false;
		return rom[offset] == STA_ABS
			   && rom[offset + 1] == (byte)(address & 0xff)
			   && rom[offset + 2] == (byte)(address >> 8);
	}
}
