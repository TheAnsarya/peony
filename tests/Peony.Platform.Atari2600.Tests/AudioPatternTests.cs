using Xunit;

namespace Peony.Platform.Atari2600.Tests;

/// <summary>
/// Tests for <see cref="AudioPatternDetector"/> — detecting audio code patterns
/// in Atari 2600 ROMs from TIA audio register write sequences.
/// </summary>
public class AudioPatternTests {

	#region Sound Setup Detection (Channel Triplet)

	[Fact]
	public void Detect_Channel0Triplet_DetectsSoundSetup() {
		// LDA #$04 / STA AUDC0 / LDA #$1f / STA AUDF0 / LDA #$0f / STA AUDV0
		var rom = new byte[] {
			0xa9, 0x04, // LDA #$04
			0x85, 0x15, // STA AUDC0
			0xa9, 0x1f, // LDA #$1F
			0x85, 0x17, // STA AUDF0
			0xa9, 0x0f, // LDA #$0F
			0x85, 0x19, // STA AUDV0
		};

		var results = AudioPatternDetector.Detect(rom);

		var setup = Assert.Single(results);
		Assert.Equal(AudioPatternType.SoundSetup, setup.Type);
		Assert.Equal(0, setup.Channel);
		Assert.Equal(2, setup.RomOffset); // first STA at offset 2
	}

	[Fact]
	public void Detect_Channel1Triplet_DetectsSoundSetup() {
		var rom = new byte[] {
			0xa9, 0x08, // LDA #$08
			0x85, 0x16, // STA AUDC1
			0xa9, 0x00, // LDA #$00
			0x85, 0x18, // STA AUDF1
			0xa9, 0x0f, // LDA #$0F
			0x85, 0x1a, // STA AUDV1
		};

		var results = AudioPatternDetector.Detect(rom);

		var setup = Assert.Single(results);
		Assert.Equal(AudioPatternType.SoundSetup, setup.Type);
		Assert.Equal(1, setup.Channel);
	}

	[Fact]
	public void Detect_TwoRegistersOnly_NoDetection() {
		// Only AUDC0 + AUDF0 — no AUDV0, so not a complete triplet
		var rom = new byte[] {
			0xa9, 0x04,
			0x85, 0x15, // STA AUDC0
			0xa9, 0x1f,
			0x85, 0x17, // STA AUDF0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Empty(results);
	}

	[Fact]
	public void Detect_AnyOrderTriplet_DetectsSetup() {
		// AUDV0, AUDF0, AUDC0 — reversed order
		var rom = new byte[] {
			0x85, 0x19, // STA AUDV0
			0x85, 0x17, // STA AUDF0
			0x85, 0x15, // STA AUDC0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.SoundSetup, results[0].Type);
		Assert.Equal(0, results[0].Channel);
	}

	[Fact]
	public void Detect_StxAndStyWrites_Detected() {
		// STX and STY also write audio registers
		var rom = new byte[] {
			0x86, 0x15, // STX AUDC0
			0x84, 0x17, // STY AUDF0
			0x85, 0x19, // STA AUDV0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.SoundSetup, results[0].Type);
	}

	[Fact]
	public void Detect_MixedChannels_NotDetected() {
		// AUDC0 + AUDF1 + AUDV0 — mixed channels, should not form a triplet
		var rom = new byte[] {
			0x85, 0x15, // STA AUDC0 (channel 0)
			0x85, 0x18, // STA AUDF1 (channel 1!)
			0x85, 0x19, // STA AUDV0 (channel 0)
		};

		var results = AudioPatternDetector.Detect(rom);

		// Should NOT detect as single setup — channels are mixed
		Assert.DoesNotContain(results, r =>
			r.Type == AudioPatternType.SoundSetup && r.Channel == 0 && r.Length >= 6);
	}

	[Fact]
	public void Detect_SoundSetupLabel_HasCounter() {
		var rom = new byte[] {
			0x85, 0x15, 0x85, 0x17, 0x85, 0x19, // Ch0 triplet
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.StartsWith("sound_setup_", results[0].SuggestedLabel);
	}

	#endregion

	#region Audio Init (Both Channels)

	[Fact]
	public void Detect_BothChannelsTriplets_PromotesToAudioInit() {
		// Channel 0 + channel 1 sequential triplets
		var rom = new byte[] {
			// Channel 0
			0x85, 0x15, // STA AUDC0
			0x85, 0x17, // STA AUDF0
			0x85, 0x19, // STA AUDV0
			// Channel 1
			0x85, 0x16, // STA AUDC1
			0x85, 0x18, // STA AUDF1
			0x85, 0x1a, // STA AUDV1
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.AudioInit, results[0].Type);
		Assert.Equal(-1, results[0].Channel); // both channels
		Assert.StartsWith("audio_init_", results[0].SuggestedLabel);
	}

	[Fact]
	public void Detect_BothChannelsReversed_PromotesToAudioInit() {
		// Channel 1 first, then channel 0
		var rom = new byte[] {
			0x85, 0x16, 0x85, 0x18, 0x85, 0x1a, // ch1
			0x85, 0x15, 0x85, 0x17, 0x85, 0x19, // ch0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.AudioInit, results[0].Type);
	}

	[Fact]
	public void Detect_BothChannelsFarApart_NotPromoted() {
		// Two channel setups but far apart (beyond window)
		var rom = new byte[64];
		// Channel 0 at start
		rom[0] = 0x85; rom[1] = 0x15;
		rom[2] = 0x85; rom[3] = 0x17;
		rom[4] = 0x85; rom[5] = 0x19;
		// Channel 1 at end (far beyond TripletWindow gap)
		rom[56] = 0x85; rom[57] = 0x16;
		rom[58] = 0x85; rom[59] = 0x18;
		rom[60] = 0x85; rom[61] = 0x1a;

		var results = AudioPatternDetector.Detect(rom);

		Assert.Equal(2, results.Count);
		Assert.All(results, r => Assert.Equal(AudioPatternType.SoundSetup, r.Type));
	}

	#endregion

	#region Frequency Table References

	[Fact]
	public void Detect_LdaAbsXStaAudf0_DetectsFrequencyTable() {
		// LDA $F080,X / STA AUDF0
		var rom = new byte[] {
			0xbd, 0x80, 0xf0, // LDA $F080,X
			0x85, 0x17,       // STA AUDF0
		};

		var results = AudioPatternDetector.Detect(rom);

		var freq = Assert.Single(results);
		Assert.Equal(AudioPatternType.FrequencyTable, freq.Type);
		Assert.Equal(0, freq.Channel);
		Assert.Equal(0, freq.RomOffset);
		Assert.Equal(5, freq.Length);
	}

	[Fact]
	public void Detect_LdaAbsYStaAudf1_DetectsFrequencyTable() {
		// LDA $F100,Y / STA AUDF1
		var rom = new byte[] {
			0xb9, 0x00, 0xf1, // LDA $F100,Y
			0x85, 0x18,       // STA AUDF1
		};

		var results = AudioPatternDetector.Detect(rom);

		var freq = Assert.Single(results);
		Assert.Equal(AudioPatternType.FrequencyTable, freq.Type);
		Assert.Equal(1, freq.Channel);
	}

	[Fact]
	public void Detect_LdaAbsXStaNonAudio_NotDetected() {
		// LDA $F080,X / STA $80 (SWCHA, not audio)
		var rom = new byte[] {
			0xbd, 0x80, 0xf0, // LDA $F080,X
			0x85, 0x80,       // STA $80 (not an audio register)
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Empty(results);
	}

	[Fact]
	public void Detect_FrequencyTableLabel_HasCounter() {
		var rom = new byte[] {
			0xbd, 0x80, 0xf0, 0x85, 0x17, // LDA abs,X / STA AUDF0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.StartsWith("freq_table_ref_", results[0].SuggestedLabel);
	}

	#endregion

	#region Volume Envelope References

	[Fact]
	public void Detect_LdaAbsXStaAudv0_DetectsVolumeEnvelope() {
		// LDA $F0C0,X / STA AUDV0
		var rom = new byte[] {
			0xbd, 0xc0, 0xf0, // LDA $F0C0,X
			0x85, 0x19,       // STA AUDV0
		};

		var results = AudioPatternDetector.Detect(rom);

		var vol = Assert.Single(results);
		Assert.Equal(AudioPatternType.VolumeEnvelope, vol.Type);
		Assert.Equal(0, vol.Channel);
	}

	[Fact]
	public void Detect_LdaAbsYStaAudv1_DetectsVolumeEnvelope() {
		// LDA $F100,Y / STA AUDV1
		var rom = new byte[] {
			0xb9, 0x00, 0xf1, // LDA $F100,Y
			0x85, 0x1a,       // STA AUDV1
		};

		var results = AudioPatternDetector.Detect(rom);

		var vol = Assert.Single(results);
		Assert.Equal(AudioPatternType.VolumeEnvelope, vol.Type);
		Assert.Equal(1, vol.Channel);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void Detect_EmptyRom_ReturnsEmpty() {
		var results = AudioPatternDetector.Detect(ReadOnlySpan<byte>.Empty);

		Assert.Empty(results);
	}

	[Fact]
	public void Detect_AllZeros_NoFalseDetection() {
		var rom = new byte[256];

		var results = AudioPatternDetector.Detect(rom);

		Assert.Empty(results);
	}

	[Fact]
	public void Detect_SmallRom_NoOutOfBounds() {
		var rom = new byte[] { 0x85 }; // truncated STA

		var results = AudioPatternDetector.Detect(rom);

		Assert.Empty(results);
	}

	[Fact]
	public void Detect_RandomBytes_NoFalsePositives() {
		var rom = new byte[] {
			0x4c, 0x00, 0xf0, // JMP
			0x60,              // RTS
			0xea, 0xea, 0xea,  // NOP NOP NOP
			0xa2, 0x00,        // LDX #$00
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Empty(results);
	}

	#endregion

	#region Mixed Patterns

	[Fact]
	public void Detect_SoundSetupAndFrequencyTable_BothDetected() {
		var rom = new byte[] {
			// Sound setup for channel 0
			0xa9, 0x04, 0x85, 0x15, // LDA #$04 / STA AUDC0
			0xa9, 0x1f, 0x85, 0x17, // LDA #$1F / STA AUDF0
			0xa9, 0x0f, 0x85, 0x19, // LDA #$0F / STA AUDV0
			// Later: frequency table reference
			0xea, 0xea, 0xea, 0xea, // padding
			0xbd, 0x80, 0xf0,       // LDA $F080,X
			0x85, 0x17,             // STA AUDF0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Equal(2, results.Count);
		Assert.Contains(results, r => r.Type == AudioPatternType.SoundSetup);
		Assert.Contains(results, r => r.Type == AudioPatternType.FrequencyTable);
	}

	[Fact]
	public void Detect_MultipleFrequencyTables_AllDetected() {
		var rom = new byte[] {
			0xbd, 0x80, 0xf0, 0x85, 0x17, // LDA $F080,X / STA AUDF0
			0xea, 0xea,                     // NOP NOP
			0xb9, 0x90, 0xf0, 0x85, 0x18, // LDA $F090,Y / STA AUDF1
		};

		var results = AudioPatternDetector.Detect(rom);

		var freqResults = results.Where(r => r.Type == AudioPatternType.FrequencyTable).ToList();
		Assert.Equal(2, freqResults.Count);
		Assert.Equal(0, freqResults[0].Channel);
		Assert.Equal(1, freqResults[1].Channel);
	}

	#endregion

	#region Realistic Audio Patterns

	[Fact]
	public void Detect_LaserSoundEffect_DetectsSetup() {
		// Classic laser sound: pure tone, high pitch, full volume
		var rom = new byte[] {
			0xa9, 0x04, // LDA #$04 (pure tone)
			0x85, 0x15, // STA AUDC0
			0xa9, 0x1f, // LDA #$1F (high pitch)
			0x85, 0x17, // STA AUDF0
			0xa9, 0x0f, // LDA #$0F (full volume)
			0x85, 0x19, // STA AUDV0
			0x60,       // RTS
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.SoundSetup, results[0].Type);
		Assert.Equal(0, results[0].Channel);
	}

	[Fact]
	public void Detect_SilenceRoutine_DetectsAudioInit() {
		// Common silence/init: set all channels to volume 0
		var rom = new byte[] {
			0xa9, 0x00, // LDA #$00
			0x85, 0x15, // STA AUDC0
			0x85, 0x17, // STA AUDF0
			0x85, 0x19, // STA AUDV0
			0x85, 0x16, // STA AUDC1
			0x85, 0x18, // STA AUDF1
			0x85, 0x1a, // STA AUDV1
			0x60,       // RTS
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Single(results);
		Assert.Equal(AudioPatternType.AudioInit, results[0].Type);
		Assert.Equal(-1, results[0].Channel);
	}

	[Fact]
	public void Detect_MusicEngine_DetectsFreqAndVolTables() {
		// Typical music engine: load frequency and volume from tables
		var rom = new byte[] {
			0xbd, 0x00, 0xf1, // LDA freq_table,X
			0x85, 0x17,       // STA AUDF0
			0xbd, 0x20, 0xf1, // LDA vol_table,X
			0x85, 0x19,       // STA AUDV0
		};

		var results = AudioPatternDetector.Detect(rom);

		Assert.Equal(2, results.Count);
		Assert.Contains(results, r => r.Type == AudioPatternType.FrequencyTable);
		Assert.Contains(results, r => r.Type == AudioPatternType.VolumeEnvelope);
	}

	#endregion
}
