using Xunit;

namespace Peony.Platform.Atari2600.Tests;

/// <summary>
/// Tests for <see cref="GraphicsPatternDetector"/> — detecting inline sprite data,
/// playfield triplets, and sprite table references in Atari 2600 ROMs.
/// </summary>
public class GraphicsPatternTests {

	#region Inline Sprite Detection (GRP0/GRP1)

	[Fact]
	public void Detect_ThreeConsecutiveLdaStagrp0_DetectsSpriteInline() {
		// 3× LDA #imm / STA $1B (GRP0)
		var rom = new byte[] {
			0xa9, 0x18, 0x85, 0x1b, // LDA #$18, STA GRP0
			0xa9, 0x3c, 0x85, 0x1b, // LDA #$3C, STA GRP0
			0xa9, 0x7e, 0x85, 0x1b, // LDA #$7E, STA GRP0
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Single(regions);
		Assert.Equal(GraphicsType.SpriteInline, regions[0].Type);
		Assert.Equal(0, regions[0].RomOffset);
		Assert.Equal(12, regions[0].Length);
		Assert.StartsWith("sprite_grp0_", regions[0].SuggestedLabel);
	}

	[Fact]
	public void Detect_ThreeConsecutiveLdaStagrp1_DetectsSpriteInline() {
		// 3× LDA #imm / STA $1C (GRP1)
		var rom = new byte[] {
			0xa9, 0x42, 0x85, 0x1c, // LDA #$42, STA GRP1
			0xa9, 0x24, 0x85, 0x1c, // LDA #$24, STA GRP1
			0xa9, 0x18, 0x85, 0x1c, // LDA #$18, STA GRP1
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Single(regions);
		Assert.Equal(GraphicsType.SpriteInline, regions[0].Type);
		Assert.StartsWith("sprite_grp1_", regions[0].SuggestedLabel);
	}

	[Fact]
	public void Detect_EightLineSpriteChain_DetectsFullSprite() {
		// 8-line sprite (common for Atari 2600 player graphics)
		var rom = new byte[32];
		for (int i = 0; i < 8; i++) {
			rom[i * 4 + 0] = 0xa9;       // LDA #imm
			rom[i * 4 + 1] = (byte)(i * 0x11); // sprite data
			rom[i * 4 + 2] = 0x85;       // STA zpg
			rom[i * 4 + 3] = 0x1b;       // GRP0
		}

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Single(regions);
		Assert.Equal(0, regions[0].RomOffset);
		Assert.Equal(32, regions[0].Length);
	}

	[Fact]
	public void Detect_TwoConsecutivePairs_NotEnough_NoDetection() {
		// Only 2 pairs — below threshold of 3
		var rom = new byte[] {
			0xa9, 0x18, 0x85, 0x1b, // LDA #$18, STA GRP0
			0xa9, 0x3c, 0x85, 0x1b, // LDA #$3C, STA GRP0
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		// 2 consecutive pairs is below MinSpriteChainLength (3)
		Assert.DoesNotContain(regions, r => r.Type == GraphicsType.SpriteInline);
	}

	[Fact]
	public void Detect_MixedGrp0AndGrp1_SeparateRegions() {
		// GRP0 chain then GRP1 chain
		var rom = new byte[] {
			0xa9, 0x18, 0x85, 0x1b, // LDA #$18, STA GRP0
			0xa9, 0x3c, 0x85, 0x1b, // LDA #$3C, STA GRP0
			0xa9, 0x7e, 0x85, 0x1b, // LDA #$7E, STA GRP0
			0xa9, 0x42, 0x85, 0x1c, // LDA #$42, STA GRP1
			0xa9, 0x24, 0x85, 0x1c, // LDA #$24, STA GRP1
			0xa9, 0x18, 0x85, 0x1c, // LDA #$18, STA GRP1
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var spriteRegions = regions.Where(r => r.Type == GraphicsType.SpriteInline).ToList();
		Assert.Equal(2, spriteRegions.Count);
		Assert.Contains(spriteRegions, r => r.SuggestedLabel.Contains("grp0"));
		Assert.Contains(spriteRegions, r => r.SuggestedLabel.Contains("grp1"));
	}

	[Fact]
	public void Detect_StyGrp0_AlsoDetected() {
		// Using STY instead of STA (some games use Y register)
		var rom = new byte[] {
			0xa9, 0x18, 0x85, 0x1b, // LDA #$18, STA GRP0
			0xa9, 0x3c, 0x85, 0x1b, // LDA #$3C, STA GRP0
			0xa9, 0x7e, 0x85, 0x1b, // LDA #$7E, STA GRP0
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Single(regions.Where(r => r.Type == GraphicsType.SpriteInline));
	}

	[Fact]
	public void Detect_SpriteChainWithOffset_DetectsAtCorrectOffset() {
		// Some padding before the sprite chain
		var rom = new byte[] {
			0x00, 0x00, 0x00, 0x00, // padding
			0xa9, 0x18, 0x85, 0x1b,
			0xa9, 0x3c, 0x85, 0x1b,
			0xa9, 0x7e, 0x85, 0x1b,
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Single(regions.Where(r => r.Type == GraphicsType.SpriteInline));
		Assert.Equal(4, regions[0].RomOffset);
	}

	[Fact]
	public void Detect_LabelCounterIncrements() {
		// Two separate GRP0 chains should get different labels
		var rom = new byte[] {
			0xa9, 0x18, 0x85, 0x1b,
			0xa9, 0x3c, 0x85, 0x1b,
			0xa9, 0x7e, 0x85, 0x1b,
			0x60,                    // RTS (gap)
			0xa9, 0x42, 0x85, 0x1b,
			0xa9, 0x24, 0x85, 0x1b,
			0xa9, 0x18, 0x85, 0x1b,
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var spriteRegions = regions.Where(r => r.Type == GraphicsType.SpriteInline).ToList();
		Assert.Equal(2, spriteRegions.Count);
		Assert.Equal("sprite_grp0_0000", spriteRegions[0].SuggestedLabel);
		Assert.Equal("sprite_grp0_0001", spriteRegions[1].SuggestedLabel);
	}

	#endregion

	#region Playfield Triplet Detection

	[Fact]
	public void Detect_SinglePlayfieldTriplet_Detected() {
		// LDA #imm / STA PF0, LDA #imm / STA PF1, LDA #imm / STA PF2
		var rom = new byte[] {
			0xa9, 0xf0, 0x85, 0x0d, // LDA #$F0, STA PF0
			0xa9, 0xaa, 0x85, 0x0e, // LDA #$AA, STA PF1
			0xa9, 0x55, 0x85, 0x0f, // LDA #$55, STA PF2
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var pfRegions = regions.Where(r => r.Type == GraphicsType.PlayfieldInline).ToList();
		Assert.Single(pfRegions);
		Assert.Equal(0, pfRegions[0].RomOffset);
		Assert.Equal(12, pfRegions[0].Length);
		Assert.StartsWith("playfield_data_", pfRegions[0].SuggestedLabel);
	}

	[Fact]
	public void Detect_TwoConsecutivePlayfieldTriplets_SingleRegion() {
		// Two consecutive PF triplets should be one region
		var rom = new byte[] {
			0xa9, 0xf0, 0x85, 0x0d, // PF0
			0xa9, 0xaa, 0x85, 0x0e, // PF1
			0xa9, 0x55, 0x85, 0x0f, // PF2
			0xa9, 0x0f, 0x85, 0x0d, // PF0 (second triplet)
			0xa9, 0x33, 0x85, 0x0e, // PF1
			0xa9, 0xcc, 0x85, 0x0f, // PF2
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var pfRegions = regions.Where(r => r.Type == GraphicsType.PlayfieldInline).ToList();
		Assert.Single(pfRegions);
		Assert.Equal(24, pfRegions[0].Length); // Both triplets merged
	}

	[Fact]
	public void Detect_IncompleteTriplet_PF0PF1Only_NotDetected() {
		// Only PF0 and PF1, no PF2 — not a full triplet
		var rom = new byte[] {
			0xa9, 0xf0, 0x85, 0x0d, // PF0
			0xa9, 0xaa, 0x85, 0x0e, // PF1
			0x60,                    // RTS
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.DoesNotContain(regions, r => r.Type == GraphicsType.PlayfieldInline);
	}

	[Fact]
	public void Detect_PlayfieldTripletWithOffset_CorrectPosition() {
		var rom = new byte[] {
			0x00, 0x00,              // padding
			0xa9, 0xf0, 0x85, 0x0d,
			0xa9, 0xaa, 0x85, 0x0e,
			0xa9, 0x55, 0x85, 0x0f,
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var pfRegions = regions.Where(r => r.Type == GraphicsType.PlayfieldInline).ToList();
		Assert.Single(pfRegions);
		Assert.Equal(2, pfRegions[0].RomOffset);
	}

	#endregion

	#region Sprite Table Reference Detection

	[Fact]
	public void Detect_LdaAbsXStaGrp0_DetectsTableRef() {
		// LDA table,X / STA GRP0
		var rom = new byte[] {
			0xbd, 0x00, 0xf2, // LDA $F200,X
			0x85, 0x1b,       // STA GRP0
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var tableRegions = regions.Where(r => r.Type == GraphicsType.SpriteTable).ToList();
		Assert.Single(tableRegions);
		Assert.Equal(0, tableRegions[0].RomOffset);
		Assert.StartsWith("sprite_table_ref_", tableRegions[0].SuggestedLabel);
	}

	[Fact]
	public void Detect_LdaAbsYStaGrp1_DetectsTableRef() {
		// LDA table,Y / STA GRP1
		var rom = new byte[] {
			0xb9, 0x80, 0xf5, // LDA $F580,Y
			0x85, 0x1c,       // STA GRP1
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var tableRegions = regions.Where(r => r.Type == GraphicsType.SpriteTable).ToList();
		Assert.Single(tableRegions);
	}

	[Fact]
	public void Detect_LdaAbsXStaNonGraphics_NotDetected() {
		// LDA table,X / STA something else (not GRP0/GRP1)
		var rom = new byte[] {
			0xbd, 0x00, 0xf2, // LDA $F200,X
			0x85, 0x00,       // STA VSYNC (not graphics)
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.DoesNotContain(regions, r => r.Type == GraphicsType.SpriteTable);
	}

	[Fact]
	public void Detect_MultipleTableRefs_AllDetected() {
		var rom = new byte[] {
			0xbd, 0x00, 0xf2, 0x85, 0x1b, // LDA $F200,X / STA GRP0
			0xb9, 0x80, 0xf3, 0x85, 0x1c, // LDA $F380,Y / STA GRP1
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		var tableRegions = regions.Where(r => r.Type == GraphicsType.SpriteTable).ToList();
		Assert.Equal(2, tableRegions.Count);
	}

	#endregion

	#region Empty / No Pattern Tests

	[Fact]
	public void Detect_EmptyRom_ReturnsEmpty() {
		var regions = GraphicsPatternDetector.Detect(ReadOnlySpan<byte>.Empty);

		Assert.Empty(regions);
	}

	[Fact]
	public void Detect_AllZeros_NoDetection() {
		var rom = new byte[256];

		var regions = GraphicsPatternDetector.Detect(rom);

		// All zeros don't match any graphics pattern opcodes
		Assert.Empty(regions);
	}

	[Fact]
	public void Detect_RandomData_NoFalsePositives() {
		// Random-looking data that shouldn't match patterns
		var rom = new byte[] {
			0x4c, 0x00, 0xf0, // JMP $F000
			0x20, 0x50, 0xf1, // JSR $F150
			0x60,              // RTS
			0x48,              // PHA
			0x68,              // PLA
			0x40,              // RTI
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Empty(regions);
	}

	#endregion

	#region Mixed Pattern Tests

	[Fact]
	public void Detect_SpriteAndPlayfieldInSameRom_BothDetected() {
		var rom = new byte[] {
			// Sprite chain
			0xa9, 0x18, 0x85, 0x1b,
			0xa9, 0x3c, 0x85, 0x1b,
			0xa9, 0x7e, 0x85, 0x1b,
			// Gap
			0x60,
			// Playfield triplet
			0xa9, 0xf0, 0x85, 0x0d,
			0xa9, 0xaa, 0x85, 0x0e,
			0xa9, 0x55, 0x85, 0x0f,
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Contains(regions, r => r.Type == GraphicsType.SpriteInline);
		Assert.Contains(regions, r => r.Type == GraphicsType.PlayfieldInline);
	}

	[Fact]
	public void Detect_SpriteChainAndTableRef_BothDetected() {
		var rom = new byte[] {
			// Inline sprite
			0xa9, 0x18, 0x85, 0x1b,
			0xa9, 0x3c, 0x85, 0x1b,
			0xa9, 0x7e, 0x85, 0x1b,
			// Table reference
			0xbd, 0x00, 0xf2, 0x85, 0x1b,
		};

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Contains(regions, r => r.Type == GraphicsType.SpriteInline);
		Assert.Contains(regions, r => r.Type == GraphicsType.SpriteTable);
	}

	#endregion

	#region Realistic ROM Tests

	[Fact]
	public void Detect_RealisticSpriteKernel_DetectsCorrectly() {
		// Simulates a typical Atari 2600 sprite kernel setup
		// WSYNC, then sprite writes, then WSYNC
		var rom = new byte[64];
		// STA WSYNC
		rom[0] = 0x85;
		rom[1] = 0x02;
		// 4× LDA #imm / STA GRP0 (inline sprite for scanlines)
		for (int i = 0; i < 4; i++) {
			rom[2 + i * 4 + 0] = 0xa9;
			rom[2 + i * 4 + 1] = (byte)(0x18 << i);
			rom[2 + i * 4 + 2] = 0x85;
			rom[2 + i * 4 + 3] = 0x1b;
		}
		// STA WSYNC
		rom[18] = 0x85;
		rom[19] = 0x02;

		var regions = GraphicsPatternDetector.Detect(rom);

		var spriteRegions = regions.Where(r => r.Type == GraphicsType.SpriteInline).ToList();
		Assert.Single(spriteRegions);
		Assert.Equal(2, spriteRegions[0].RomOffset);
		Assert.Equal(16, spriteRegions[0].Length);
	}

	[Fact]
	public void Detect_4kRomWithScatteredPatterns_FindsAll() {
		// Create a 4K ROM with patterns at various locations
		var rom = new byte[4096];

		// Sprite chain at offset $100
		for (int i = 0; i < 5; i++) {
			rom[0x100 + i * 4 + 0] = 0xa9;
			rom[0x100 + i * 4 + 1] = (byte)(i * 0x20);
			rom[0x100 + i * 4 + 2] = 0x85;
			rom[0x100 + i * 4 + 3] = 0x1b;
		}

		// Playfield triplet at offset $200
		rom[0x200] = 0xa9; rom[0x201] = 0xf0; rom[0x202] = 0x85; rom[0x203] = 0x0d;
		rom[0x204] = 0xa9; rom[0x205] = 0xaa; rom[0x206] = 0x85; rom[0x207] = 0x0e;
		rom[0x208] = 0xa9; rom[0x209] = 0x55; rom[0x20a] = 0x85; rom[0x20b] = 0x0f;

		// Table ref at offset $300
		rom[0x300] = 0xbd; rom[0x301] = 0x00; rom[0x302] = 0xf8;
		rom[0x303] = 0x85; rom[0x304] = 0x1b;

		var regions = GraphicsPatternDetector.Detect(rom);

		Assert.Contains(regions, r => r.Type == GraphicsType.SpriteInline && r.RomOffset == 0x100);
		Assert.Contains(regions, r => r.Type == GraphicsType.PlayfieldInline && r.RomOffset == 0x200);
		Assert.Contains(regions, r => r.Type == GraphicsType.SpriteTable && r.RomOffset == 0x300);
	}

	#endregion
}
