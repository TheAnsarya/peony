namespace Peony.Platform.Atari2600;

/// <summary>
/// Type of detected graphics pattern in an Atari 2600 ROM.
/// </summary>
public enum GraphicsType {
	/// <summary>Player sprite data (GRP0/GRP1 sequential writes).</summary>
	SpriteInline,

	/// <summary>Playfield data (PF0/PF1/PF2 triplet writes).</summary>
	PlayfieldInline,

	/// <summary>Sprite data table referenced by indexed addressing.</summary>
	SpriteTable,
}

/// <summary>
/// A detected graphics data region in the ROM.
/// </summary>
public record DetectedGraphicsRegion(
	int RomOffset,
	int Length,
	GraphicsType Type,
	string SuggestedLabel
);

/// <summary>
/// Detects graphics data patterns in Atari 2600 ROMs by scanning for
/// characteristic 6502 instruction sequences that write to TIA graphics registers.
/// </summary>
/// <remarks>
/// <para>
/// Atari 2600 sprite and playfield data is commonly written inline using
/// sequential LDA #imm / STA GRPx or LDA #imm / STA PFx patterns.
/// This detector identifies such sequences and returns their ROM locations
/// for marking as DATA in the CDL.
/// </para>
/// <para>
/// Also detects indexed table references (LDA table,X / STA GRPx) that
/// indicate sprite data tables at the operand address.
/// </para>
/// </remarks>
public static class GraphicsPatternDetector {
	// TIA graphics register addresses (zero-page)
	private const byte GRP0 = 0x1b;
	private const byte GRP1 = 0x1c;
	private const byte PF0 = 0x0d;
	private const byte PF1 = 0x0e;
	private const byte PF2 = 0x0f;

	// 6502 opcodes
	private const byte LDA_IMM = 0xa9;
	private const byte STA_ZPG = 0x85;
	private const byte STY_ZPG = 0x84;
	private const byte STX_ZPG = 0x86;
	private const byte LDA_ABS_X = 0xbd;
	private const byte LDA_ABS_Y = 0xb9;
	private const byte LDY_ABS_X = 0xbc;
	private const byte LDX_ABS_Y = 0xbe;
	private const byte LDA_ZPG_X = 0xb5;

	/// <summary>
	/// Minimum number of consecutive GRP writes to consider a sprite inline sequence.
	/// </summary>
	private const int MinSpriteChainLength = 3;

	/// <summary>
	/// Scan ROM for graphics data patterns and return detected regions.
	/// </summary>
	public static List<DetectedGraphicsRegion> Detect(ReadOnlySpan<byte> rom) {
		var regions = new List<DetectedGraphicsRegion>();
		int spriteCounter = 0;
		int tableCounter = 0;
		int pfCounter = 0;

		DetectInlineSpritePatterns(rom, regions, ref spriteCounter);
		DetectPlayfieldTriplets(rom, regions, ref pfCounter);
		DetectSpriteTableReferences(rom, regions, ref tableCounter);

		return regions;
	}

	/// <summary>
	/// Detect sequential LDA #imm / STA GRP0 or STA GRP1 chains.
	/// A chain of 3+ consecutive pairs indicates inline sprite data.
	/// </summary>
	private static void DetectInlineSpritePatterns(
		ReadOnlySpan<byte> rom,
		List<DetectedGraphicsRegion> regions,
		ref int counter) {

		int i = 0;
		while (i < rom.Length - 3) {
			// Look for start of a chain: LDA #imm, STA/STY/STX GRPx
			if (!IsImmediateLoadAndGraphicsStore(rom, i, out var register, out var pairLength)) {
				i++;
				continue;
			}

			// Found one pair — count consecutive pairs
			int chainStart = i;
			int chainLength = 0;

			while (i < rom.Length - 3 &&
				   IsImmediateLoadAndGraphicsStore(rom, i, out var nextReg, out var nextLen) &&
				   nextReg == register) {
				chainLength++;
				i += nextLen;
			}

			if (chainLength >= MinSpriteChainLength) {
				var regName = register == GRP0 ? "grp0" : "grp1";
				regions.Add(new DetectedGraphicsRegion(
					chainStart,
					i - chainStart,
					GraphicsType.SpriteInline,
					$"sprite_{regName}_{counter:d4}"));
				counter++;
			}
		}
	}

	/// <summary>
	/// Detect playfield triplet patterns: LDA #imm / STA PF0, LDA #imm / STA PF1, LDA #imm / STA PF2.
	/// </summary>
	private static void DetectPlayfieldTriplets(
		ReadOnlySpan<byte> rom,
		List<DetectedGraphicsRegion> regions,
		ref int counter) {

		int i = 0;
		while (i < rom.Length - 11) {
			// Need at least 12 bytes for a full PF0+PF1+PF2 triplet (4+4+4)
			if (IsImmLoadStaZpg(rom, i, PF0) &&
				IsImmLoadStaZpg(rom, i + 4, PF1) &&
				IsImmLoadStaZpg(rom, i + 8, PF2)) {

				int tripletStart = i;
				int tripletCount = 0;

				// Count consecutive triplets
				while (i < rom.Length - 11 &&
					   IsImmLoadStaZpg(rom, i, PF0) &&
					   IsImmLoadStaZpg(rom, i + 4, PF1) &&
					   IsImmLoadStaZpg(rom, i + 8, PF2)) {
					tripletCount++;
					i += 12;
				}

				regions.Add(new DetectedGraphicsRegion(
					tripletStart,
					i - tripletStart,
					GraphicsType.PlayfieldInline,
					$"playfield_data_{counter:d4}"));
				counter++;
			} else {
				i++;
			}
		}
	}

	/// <summary>
	/// Detect indexed table references: LDA table,X / STA GRPx or LDA table,Y / STA GRPx.
	/// The operand address points to a sprite data table.
	/// </summary>
	private static void DetectSpriteTableReferences(
		ReadOnlySpan<byte> rom,
		List<DetectedGraphicsRegion> regions,
		ref int counter) {

		for (int i = 0; i < rom.Length - 4; i++) {
			// LDA abs,X (BD xx xx) or LDA abs,Y (B9 xx xx)
			if (rom[i] != LDA_ABS_X && rom[i] != LDA_ABS_Y)
				continue;

			// Next instruction should be STA/STY/STX to GRP0 or GRP1
			int storeOffset = i + 3;
			if (storeOffset >= rom.Length - 1)
				continue;

			if (!IsGraphicsStore(rom, storeOffset, out var register))
				continue;

			// The operand is the table address
			var tableAddr = rom[i + 1] | (rom[i + 2] << 8);

			regions.Add(new DetectedGraphicsRegion(
				i,
				5, // 3 bytes LDA + 2 bytes STA
				GraphicsType.SpriteTable,
				$"sprite_table_ref_{counter:d4}"));
			counter++;
		}
	}

	/// <summary>
	/// Check if bytes at offset form a LDA #imm followed by a store to GRP0 or GRP1.
	/// </summary>
	private static bool IsImmediateLoadAndGraphicsStore(
		ReadOnlySpan<byte> rom, int offset,
		out byte register, out int totalLength) {

		register = 0;
		totalLength = 0;

		if (offset + 3 >= rom.Length)
			return false;

		// LDA #imm = A9 xx (2 bytes)
		if (rom[offset] != LDA_IMM)
			return false;

		// STA/STY/STX zpg GRP0 or GRP1
		int storeOffset = offset + 2;
		if (!IsGraphicsStore(rom, storeOffset, out register))
			return false;

		totalLength = 4; // 2 (LDA #imm) + 2 (STA zpg)
		return true;
	}

	/// <summary>
	/// Check if bytes at offset form a STA/STY/STX to GRP0 or GRP1 (zero-page).
	/// </summary>
	private static bool IsGraphicsStore(ReadOnlySpan<byte> rom, int offset, out byte register) {
		register = 0;
		if (offset + 1 >= rom.Length)
			return false;

		var opcode = rom[offset];
		if (opcode != STA_ZPG && opcode != STY_ZPG && opcode != STX_ZPG)
			return false;

		var reg = rom[offset + 1];
		if (reg != GRP0 && reg != GRP1)
			return false;

		register = reg;
		return true;
	}

	/// <summary>
	/// Check if bytes at offset form LDA #imm / STA zpg to the specified register.
	/// </summary>
	private static bool IsImmLoadStaZpg(ReadOnlySpan<byte> rom, int offset, byte register) {
		if (offset + 3 >= rom.Length)
			return false;

		return rom[offset] == LDA_IMM &&
			   rom[offset + 2] == STA_ZPG &&
			   rom[offset + 3] == register;
	}
}
