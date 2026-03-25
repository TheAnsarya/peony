using Xunit;

namespace Peony.Platform.Atari2600.Tests;

/// <summary>
/// Tests for <see cref="ControllerTypeDetector"/> — detecting controller types
/// from I/O register access patterns in Atari 2600 ROMs.
/// </summary>
public class ControllerDetectionTests {

	#region Joystick Detection

	[Fact]
	public void Detect_SwchaReadAndInpt4_DetectsJoystick() {
		// Standard joystick: LDA SWCHA + LDA INPT4
		var rom = new byte[] {
			0xa5, 0x80, // LDA $80 (SWCHA)
			0xa5, 0x0c, // LDA $0C (INPT4 — fire button)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Joystick, result.Player1);
	}

	[Fact]
	public void Detect_SwchaReadAndInpt5_DetectsJoystickP2() {
		// Player 2 joystick: LDA SWCHA + LDA INPT5
		var rom = new byte[] {
			0xa5, 0x80, // LDA $80 (SWCHA)
			0xa5, 0x0d, // LDA $0D (INPT5 — P2 fire button)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Joystick, result.Player2);
	}

	[Fact]
	public void Detect_BitSwcha_DetectsRead() {
		// BIT $80 also reads SWCHA
		var rom = new byte[] {
			0x24, 0x80, // BIT $80 (SWCHA)
			0xa5, 0x0c, // LDA INPT4
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.True(result.SwchaReadCount > 0);
	}

	[Fact]
	public void Detect_AbsoluteSwchaRead_Detected() {
		// LDA $0280 (absolute RIOT address)
		var rom = new byte[] {
			0xad, 0x80, 0x02, // LDA $0280 (SWCHA)
			0xa5, 0x0c,       // LDA INPT4
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Joystick, result.Player1);
		Assert.True(result.SwchaReadCount > 0);
	}

	#endregion

	#region Paddle Detection

	[Fact]
	public void Detect_Inpt0ReadWithBpl_DetectsPaddle() {
		// Paddle: LDA INPT0 followed by BPL (threshold check)
		var rom = new byte[] {
			0xa5, 0x08, // LDA $08 (INPT0)
			0x10, 0x02, // BPL +2
			0xa5, 0x80, // LDA SWCHA (for context)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Paddle, result.Player1);
	}

	[Fact]
	public void Detect_Inpt1ReadWithBmi_DetectsPaddle() {
		// Paddle: LDA INPT1 followed by BMI
		var rom = new byte[] {
			0xa5, 0x09, // LDA $09 (INPT1)
			0x30, 0x02, // BMI +2
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Paddle, result.Player1);
	}

	[Fact]
	public void Detect_Inpt2WithThreshold_DetectsPaddleP2() {
		// Player 2 paddle: INPT2 read with threshold
		var rom = new byte[] {
			0xa5, 0x80, // LDA SWCHA
			0xa5, 0x0a, // LDA $0A (INPT2)
			0x10, 0x02, // BPL +2
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Paddle, result.Player2);
	}

	[Fact]
	public void Detect_Inpt3WithThreshold_DetectsPaddleP2() {
		var rom = new byte[] {
			0xa5, 0x80, // LDA SWCHA
			0xa5, 0x0b, // LDA $0B (INPT3)
			0x30, 0x02, // BMI +2
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Paddle, result.Player2);
	}

	[Fact]
	public void Detect_InptReadWithoutThreshold_NotPaddle() {
		// INPT0 read but no BPL/BMI threshold — doesn't look like paddle
		var rom = new byte[] {
			0xa5, 0x08, // LDA $08 (INPT0)
			0x85, 0x90, // STA $90 (store to RAM, no threshold)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.NotEqual(ControllerType.Paddle, result.Player1);
	}

	#endregion

	#region Keypad Detection

	[Fact]
	public void Detect_SwacntWriteAndSwchaWriteAndInptRead_DetectsKeypad() {
		// Keypad: STA SWACNT (configure direction), STA SWCHA (column select), LDA INPT0 (row read)
		var rom = new byte[] {
			0x85, 0x81, // STA $81 (SWACNT — set port direction)
			0x85, 0x80, // STA $80 (SWCHA — column select)
			0xa5, 0x08, // LDA $08 (INPT0 — row read)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Keypad, result.Player1);
	}

	[Fact]
	public void Detect_AbsoluteSwacntWrite_DetectsKeypad() {
		// Keypad with absolute addressing: STA $0281, STA $0280, LDA INPT1
		var rom = new byte[] {
			0x8d, 0x81, 0x02, // STA $0281 (SWACNT)
			0x8d, 0x80, 0x02, // STA $0280 (SWCHA)
			0xa5, 0x09,       // LDA $09 (INPT1)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Keypad, result.Player1);
	}

	#endregion

	#region Driving Controller Detection

	[Fact]
	public void Detect_SwchaReadOnly_NoFireButton_DetectsDriving() {
		// Driving controller: reads SWCHA but no fire button reads
		var rom = new byte[] {
			0xa5, 0x80, // LDA $80 (SWCHA)
			0x29, 0x03, // AND #$03 (mask gray code bits)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Driving, result.Player1);
	}

	#endregion

	#region Counter Verification

	[Fact]
	public void Detect_CountsSwchaReadsCorrectly() {
		var rom = new byte[] {
			0xa5, 0x80, // LDA $80 (SWCHA read 1)
			0x60,       // RTS
			0xa5, 0x80, // LDA $80 (SWCHA read 2)
			0xa5, 0x0c, // LDA INPT4
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(2, result.SwchaReadCount);
	}

	[Fact]
	public void Detect_CountsSwchaWritesCorrectly() {
		var rom = new byte[] {
			0x85, 0x80, // STA $80 (SWCHA write 1)
			0x85, 0x80, // STA $80 (SWCHA write 2)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(2, result.SwchaWriteCount);
	}

	[Fact]
	public void Detect_CountsAllInptPortsSeparately() {
		var rom = new byte[] {
			0xa5, 0x08, // INPT0
			0xa5, 0x09, // INPT1
			0xa5, 0x0a, // INPT2
			0xa5, 0x0b, // INPT3
			0xa5, 0x0c, // INPT4
			0xa5, 0x0d, // INPT5
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(1, result.Inpt0Count);
		Assert.Equal(1, result.Inpt1Count);
		Assert.Equal(1, result.Inpt2Count);
		Assert.Equal(1, result.Inpt3Count);
		Assert.Equal(1, result.Inpt4Count);
		Assert.Equal(1, result.Inpt5Count);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void Detect_EmptyRom_ReturnsUnknown() {
		var result = ControllerTypeDetector.Detect(ReadOnlySpan<byte>.Empty);

		Assert.Equal(ControllerType.Unknown, result.Player1);
		Assert.Equal(ControllerType.Unknown, result.Player2);
	}

	[Fact]
	public void Detect_NoIoAccess_ReturnsUnknown() {
		var rom = new byte[] {
			0x4c, 0x00, 0xf0, // JMP $F000
			0x60,              // RTS
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Unknown, result.Player1);
		Assert.Equal(ControllerType.Unknown, result.Player2);
	}

	[Fact]
	public void Detect_AllZeros_NoFalseDetection() {
		var rom = new byte[256];

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(0, result.SwchaReadCount);
		Assert.Equal(0, result.Inpt4Count);
	}

	#endregion

	#region Realistic ROM Patterns

	[Fact]
	public void Detect_TypicalJoystickGame_DetectsBothPlayers() {
		// Typical joystick game reads SWCHA and both fire buttons
		var rom = new byte[] {
			// Read joystick directions
			0xa5, 0x80, // LDA SWCHA
			0x29, 0xf0, // AND #$F0 (P0 direction bits)
			0x85, 0x90, // STA $90
			// Read P0 fire
			0xa5, 0x0c, // LDA INPT4
			0x30, 0x02, // BMI (fire pressed)
			0xea, 0xea, // NOP NOP
			// Read P1 fire
			0xa5, 0x0d, // LDA INPT5
			0x30, 0x02, // BMI
			0xea, 0xea,
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Joystick, result.Player1);
		Assert.Equal(ControllerType.Joystick, result.Player2);
	}

	[Fact]
	public void Detect_TypicalPaddleGame_DetectsP1Paddle() {
		// Paddle game: reads INPT0 with capacitor threshold check
		var rom = new byte[] {
			0xa5, 0x80, // LDA SWCHA
			0xa5, 0x08, // LDA INPT0
			0x10, 0x04, // BPL (capacitor not discharged yet)
			0xa9, 0x01, // LDA #$01
			0x85, 0x90, // STA paddle_value
			0xa5, 0x0c, // LDA INPT4 (fire button)
		};

		var result = ControllerTypeDetector.Detect(rom);

		Assert.Equal(ControllerType.Paddle, result.Player1);
	}

	#endregion

	#region ControllerType Flags

	[Fact]
	public void ControllerType_IsFlags_CanCombine() {
		var combined = ControllerType.Joystick | ControllerType.Paddle;

		Assert.True(combined.HasFlag(ControllerType.Joystick));
		Assert.True(combined.HasFlag(ControllerType.Paddle));
		Assert.False(combined.HasFlag(ControllerType.Keypad));
	}

	[Fact]
	public void ControllerType_Unknown_IsZero() {
		Assert.Equal(0, (int)ControllerType.Unknown);
	}

	#endregion
}
