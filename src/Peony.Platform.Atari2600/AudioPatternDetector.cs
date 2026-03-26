namespace Peony.Platform.Atari2600;

/// <summary>
/// Types of audio patterns detected in Atari 2600 ROMs.
/// </summary>
public enum AudioPatternType {
	/// <summary>Sequential writes to AUDC + AUDF + AUDV for one channel.</summary>
	SoundSetup,

	/// <summary>Writes to all 6 audio registers — full audio initialization.</summary>
	AudioInit,

	/// <summary>Indexed table lookup feeding AUDFx (frequency table).</summary>
	FrequencyTable,

	/// <summary>Indexed table lookup feeding AUDVx (volume envelope).</summary>
	VolumeEnvelope,
}

/// <summary>
/// A detected audio pattern region in ROM.
/// </summary>
/// <param name="RomOffset">Start offset in ROM bytes.</param>
/// <param name="Length">Length of the detected pattern in bytes.</param>
/// <param name="Type">Type of audio pattern detected.</param>
/// <param name="Channel">Audio channel (0 or 1), or -1 for both channels.</param>
/// <param name="SuggestedLabel">Suggested label for this audio region.</param>
public record DetectedAudioRegion(
	int RomOffset,
	int Length,
	AudioPatternType Type,
	int Channel,
	string SuggestedLabel);

/// <summary>
/// Detects audio code patterns in Atari 2600 ROMs by scanning for
/// TIA audio register write sequences (AUDC, AUDF, AUDV).
/// </summary>
/// <remarks>
/// <para>
/// Detection heuristics:
/// </para>
/// <list type="bullet">
/// <item><description>Sound setup: 3 writes to AUDCx + AUDFx + AUDVx within a window</description></item>
/// <item><description>Audio init: writes to all 6 audio registers (both channels)</description></item>
/// <item><description>Frequency table: indexed load (LDA abs,X/Y) followed by STA AUDFx</description></item>
/// <item><description>Volume envelope: indexed load (LDA abs,X/Y) followed by STA AUDVx</description></item>
/// </list>
/// </remarks>
public static class AudioPatternDetector {
	// TIA audio write registers
	private const byte AUDC0 = 0x15;
	private const byte AUDC1 = 0x16;
	private const byte AUDF0 = 0x17;
	private const byte AUDF1 = 0x18;
	private const byte AUDV0 = 0x19;
	private const byte AUDV1 = 0x1a;

	// 6502 opcodes
	private const byte LDA_IMM = 0xa9;
	private const byte LDX_IMM = 0xa2;
	private const byte LDY_IMM = 0xa0;
	private const byte STA_ZPG = 0x85;
	private const byte STX_ZPG = 0x86;
	private const byte STY_ZPG = 0x84;
	private const byte LDA_ABS_X = 0xbd;
	private const byte LDA_ABS_Y = 0xb9;

	/// <summary>
	/// Maximum byte window to search for a complete channel triplet (AUDC + AUDF + AUDV).
	/// </summary>
	private const int TripletWindow = 24;

	private static int _labelCounter;

	/// <summary>
	/// Scan ROM bytes for audio code patterns.
	/// </summary>
	public static List<DetectedAudioRegion> Detect(ReadOnlySpan<byte> rom) {
		_labelCounter = 0;
		var results = new List<DetectedAudioRegion>();

		DetectSoundSetupPatterns(rom, results);
		DetectFrequencyTableReferences(rom, results);
		DetectVolumeEnvelopeReferences(rom, results);
		PromoteAudioInits(results);

		return results;
	}

	/// <summary>
	/// Detect sequential writes to AUDCx + AUDFx + AUDVx for channel 0 or 1.
	/// A "sound setup" is three writes to the same channel's registers within a window.
	/// </summary>
	private static void DetectSoundSetupPatterns(ReadOnlySpan<byte> rom, List<DetectedAudioRegion> results) {
		for (int i = 0; i < rom.Length - 3; i++) {
			// Look for STA to an audio register (start of potential triplet)
			if (!IsAudioWrite(rom, i))
				continue;

			byte firstReg = rom[i + 1];
			int channel = GetChannel(firstReg);
			if (channel < 0)
				continue;

			// Scan forward within window for the other two registers of this channel
			var regsFound = new HashSet<byte> { firstReg };
			int end = i + 2; // end of first write instruction

			for (int j = i + 2; j < rom.Length - 1 && j < i + TripletWindow; j++) {
				if (!IsAudioWrite(rom, j))
					continue;

				byte reg = rom[j + 1];
				if (GetChannel(reg) == channel) {
					regsFound.Add(reg);
					end = j + 2;
				}

				if (regsFound.Count == 3)
					break;
			}

			if (regsFound.Count == 3) {
				results.Add(new DetectedAudioRegion(
					i,
					end - i,
					AudioPatternType.SoundSetup,
					channel,
					$"sound_setup_{_labelCounter++:d4}"));
				i = end - 1; // skip past this pattern
			}
		}
	}

	/// <summary>
	/// Detect indexed table lookups feeding AUDFx: LDA abs,X/Y followed by STA AUDFx.
	/// </summary>
	private static void DetectFrequencyTableReferences(ReadOnlySpan<byte> rom, List<DetectedAudioRegion> results) {
		for (int i = 0; i < rom.Length - 4; i++) {
			if (rom[i] != LDA_ABS_X && rom[i] != LDA_ABS_Y)
				continue;

			// LDA abs,X/Y is 3 bytes, then check for STA AUDFx
			int staOffset = i + 3;
			if (staOffset + 1 >= rom.Length)
				continue;

			if (rom[staOffset] == STA_ZPG && (rom[staOffset + 1] == AUDF0 || rom[staOffset + 1] == AUDF1)) {
				int channel = rom[staOffset + 1] == AUDF0 ? 0 : 1;
				results.Add(new DetectedAudioRegion(
					i,
					5, // 3 bytes LDA + 2 bytes STA
					AudioPatternType.FrequencyTable,
					channel,
					$"freq_table_ref_{_labelCounter++:d4}"));
			}
		}
	}

	/// <summary>
	/// Detect indexed table lookups feeding AUDVx: LDA abs,X/Y followed by STA AUDVx.
	/// </summary>
	private static void DetectVolumeEnvelopeReferences(ReadOnlySpan<byte> rom, List<DetectedAudioRegion> results) {
		for (int i = 0; i < rom.Length - 4; i++) {
			if (rom[i] != LDA_ABS_X && rom[i] != LDA_ABS_Y)
				continue;

			int staOffset = i + 3;
			if (staOffset + 1 >= rom.Length)
				continue;

			if (rom[staOffset] == STA_ZPG && (rom[staOffset + 1] == AUDV0 || rom[staOffset + 1] == AUDV1)) {
				int channel = rom[staOffset + 1] == AUDV0 ? 0 : 1;
				results.Add(new DetectedAudioRegion(
					i,
					5,
					AudioPatternType.VolumeEnvelope,
					channel,
					$"vol_envelope_ref_{_labelCounter++:d4}"));
			}
		}
	}

	/// <summary>
	/// Promote adjacent channel 0 + channel 1 SoundSetup regions to AudioInit
	/// when both channels are initialized near each other.
	/// </summary>
	private static void PromoteAudioInits(List<DetectedAudioRegion> results) {
		for (int i = 0; i < results.Count - 1; i++) {
			var a = results[i];
			var b = results[i + 1];

			if (a.Type != AudioPatternType.SoundSetup || b.Type != AudioPatternType.SoundSetup)
				continue;

			// Two different channels adjacent or overlapping
			if (a.Channel == b.Channel)
				continue;

			int gap = b.RomOffset - (a.RomOffset + a.Length);
			if (gap > TripletWindow)
				continue;

			// Replace both with a single AudioInit
			int start = a.RomOffset;
			int end = b.RomOffset + b.Length;
			results[i] = new DetectedAudioRegion(
				start,
				end - start,
				AudioPatternType.AudioInit,
				-1,
				$"audio_init_{_labelCounter++:d4}");
			results.RemoveAt(i + 1);
			i--; // re-check in case of triple
		}
	}

	/// <summary>
	/// Check if the instruction at offset is a zero-page write to an audio register.
	/// Accepts STA, STX, or STY.
	/// </summary>
	private static bool IsAudioWrite(ReadOnlySpan<byte> rom, int offset) {
		if (offset + 1 >= rom.Length) return false;
		var opcode = rom[offset];
		if (opcode != STA_ZPG && opcode != STX_ZPG && opcode != STY_ZPG)
			return false;
		var reg = rom[offset + 1];
		return reg is AUDC0 or AUDC1 or AUDF0 or AUDF1 or AUDV0 or AUDV1;
	}

	/// <summary>
	/// Get channel number (0 or 1) for an audio register address, or -1 if not audio.
	/// </summary>
	private static int GetChannel(byte register) => register switch {
		AUDC0 or AUDF0 or AUDV0 => 0,
		AUDC1 or AUDF1 or AUDV1 => 1,
		_ => -1,
	};
}
