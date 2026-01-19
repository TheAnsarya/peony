namespace Peony.Platform.GBA;

using Peony.Core;

/// <summary>
/// Game Boy Advance text extractor with common GBA RPG text patterns
/// </summary>
public class GbaTextExtractor : ITextExtractor {
	/// <summary>
	/// Extract text from ROM using a table file
	/// </summary>
	public string ExtractText(ReadOnlySpan<byte> data, int offset, int maxLength, TableFile table) {
		return TextExtraction.ExtractText(data, offset, maxLength, table);
	}

	/// <summary>
	/// Extract all text blocks from ROM
	/// </summary>
	public List<TextBlock> ExtractAllText(ReadOnlySpan<byte> data, TableFile table, TextExtractionOptions options) {
		return TextExtraction.ScanForText(data, table, options);
	}

	/// <summary>
	/// Extract text from a 32-bit pointer table (common in GBA)
	/// </summary>
	public List<TextBlock> ExtractFromPointerTable32(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		TableFile table,
		string? category = null) {

		var results = new List<TextBlock>();

		for (int i = 0; i < pointerCount; i++) {
			int ptrOffset = pointerTableOffset + i * 4;
			if (ptrOffset + 4 > data.Length) break;

			// Read 32-bit pointer (little-endian)
			uint ptr = (uint)(data[ptrOffset] |
				(data[ptrOffset + 1] << 8) |
				(data[ptrOffset + 2] << 16) |
				(data[ptrOffset + 3] << 24));

			// Convert GBA ROM address to file offset
			int fileOffset = GbaAddressToOffset(ptr);
			if (fileOffset < 0 || fileOffset >= data.Length) continue;

			var text = TextExtraction.ExtractText(data, fileOffset, 4096, table);

			results.Add(new TextBlock {
				Offset = fileOffset,
				Text = text,
				RawBytes = GetTextBytes(data, fileOffset, table),
				Length = text.Length,
				Category = category
			});
		}

		return results;
	}

	/// <summary>
	/// Extract text from a 16-bit pointer table with base address
	/// </summary>
	public List<TextBlock> ExtractFromPointerTable16(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		int baseOffset,
		TableFile table,
		string? category = null) {

		var options = new TextExtractionOptions {
			NullTerminated = true,
			IncludeControlCodes = true,
			Category = category
		};

		return TextExtraction.ExtractFromPointerTable(
			data, pointerTableOffset, pointerCount, baseOffset, table, options);
	}

	/// <summary>
	/// Convert GBA ROM address to file offset
	/// GBA ROM starts at 0x08000000 (Wait State 0) or 0x0A000000/0x0C000000
	/// </summary>
	public static int GbaAddressToOffset(uint address) {
		// ROM Wait State 0: 0x08000000 - 0x09FFFFFF
		if (address >= 0x08000000 && address < 0x0a000000) {
			return (int)(address - 0x08000000);
		}
		// ROM Wait State 1: 0x0A000000 - 0x0BFFFFFF (mirror)
		if (address >= 0x0a000000 && address < 0x0c000000) {
			return (int)(address - 0x0a000000);
		}
		// ROM Wait State 2: 0x0C000000 - 0x0DFFFFFF (mirror)
		if (address >= 0x0c000000 && address < 0x0e000000) {
			return (int)(address - 0x0c000000);
		}

		// Not a ROM address
		return -1;
	}

	/// <summary>
	/// Convert file offset to GBA ROM address (Wait State 0)
	/// </summary>
	public static uint OffsetToGbaAddress(int offset) {
		return (uint)(0x08000000 + offset);
	}

	/// <summary>
	/// Create a Pokemon (GBA) style table
	/// Used by Pokemon Ruby/Sapphire/FireRed/LeafGreen/Emerald
	/// </summary>
	public static TableFile CreatePokemonGbaTable() {
		var content = """
			@name=Pokemon (GBA)
			@end=FF

			; Space
			00=<space>

			; Uppercase letters
			BB=A
			BC=B
			BD=C
			BE=D
			BF=E
			C0=F
			C1=G
			C2=H
			C3=I
			C4=J
			C5=K
			C6=L
			C7=M
			C8=N
			C9=O
			CA=P
			CB=Q
			CC=R
			CD=S
			CE=T
			CF=U
			D0=V
			D1=W
			D2=X
			D3=Y
			D4=Z

			; Lowercase letters
			D5=a
			D6=b
			D7=c
			D8=d
			D9=e
			DA=f
			DB=g
			DC=h
			DD=i
			DE=j
			DF=k
			E0=l
			E1=m
			E2=n
			E3=o
			E4=p
			E5=q
			E6=r
			E7=s
			E8=t
			E9=u
			EA=v
			EB=w
			EC=x
			ED=y
			EE=z

			; Numbers
			A1=0
			A2=1
			A3=2
			A4=3
			A5=4
			A6=5
			A7=6
			A8=7
			A9=8
			AA=9

			; Punctuation
			AB=!
			AC=?
			AD=.
			AE=-
			B0=...
			B1="
			B2="
			B3='
			B4='

			; Special control codes
			FA=[PLAYER]
			FB=[STR1]
			FC=[CTRL]
			FD=[VAR]
			FE=[LINE]
			FF=[END]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Golden Sun style table
	/// </summary>
	public static TableFile CreateGoldenSunTable() {
		var content = """
			@name=Golden Sun (GBA)
			@end=00

			; Standard ASCII-ish mapping
			20=<space>
			21=!
			22="
			27='
			28=(
			29=)
			2C=,
			2D=-
			2E=.
			2F=/
			30=0
			31=1
			32=2
			33=3
			34=4
			35=5
			36=6
			37=7
			38=8
			39=9
			3A=:
			3B=;
			3F=?
			41=A
			42=B
			43=C
			44=D
			45=E
			46=F
			47=G
			48=H
			49=I
			4A=J
			4B=K
			4C=L
			4D=M
			4E=N
			4F=O
			50=P
			51=Q
			52=R
			53=S
			54=T
			55=U
			56=V
			57=W
			58=X
			59=Y
			5A=Z
			61=a
			62=b
			63=c
			64=d
			65=e
			66=f
			67=g
			68=h
			69=i
			6A=j
			6B=k
			6C=l
			6D=m
			6E=n
			6F=o
			70=p
			71=q
			72=r
			73=s
			74=t
			75=u
			76=v
			77=w
			78=x
			79=y
			7A=z

			; Control codes
			01=[LINE]
			02=[WAIT]
			03=[CLEAR]
			04=[CHOICE]
			05=[NAME]
			06=[ITEM]
			07=[NUM]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Fire Emblem (GBA) style table
	/// Used by Fire Emblem 6/7/8
	/// </summary>
	public static TableFile CreateFireEmblemTable() {
		var content = """
			@name=Fire Emblem (GBA)
			@end=00

			; Standard ASCII mapping
			20=<space>
			21=!
			22="
			27='
			28=(
			29=)
			2C=,
			2D=-
			2E=.
			2F=/
			30=0
			31=1
			32=2
			33=3
			34=4
			35=5
			36=6
			37=7
			38=8
			39=9
			3A=:
			3B=;
			3F=?
			41=A
			42=B
			43=C
			44=D
			45=E
			46=F
			47=G
			48=H
			49=I
			4A=J
			4B=K
			4C=L
			4D=M
			4E=N
			4F=O
			50=P
			51=Q
			52=R
			53=S
			54=T
			55=U
			56=V
			57=W
			58=X
			59=Y
			5A=Z
			61=a
			62=b
			63=c
			64=d
			65=e
			66=f
			67=g
			68=h
			69=i
			6A=j
			6B=k
			6C=l
			6D=m
			6E=n
			6F=o
			70=p
			71=q
			72=r
			73=s
			74=t
			75=u
			76=v
			77=w
			78=x
			79=y
			7A=z

			; Control codes
			01=[LINE]
			02=[WAIT]
			03=[PARAGRAPH]
			04=[OPEN]
			05=[FACE]
			06=[PAUSE]
			07=[SPEED]
			08=[SOUND]
			09=[MUSIC]
			0A=[TACTICIAN]
			0B=[AUTOCLEAR]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Final Fantasy Advance style table
	/// Used by FF1&2 Dawn of Souls, FF4-6 Advance, FF Tactics Advance
	/// </summary>
	public static TableFile CreateFinalFantasyAdvanceTable() {
		var content = """
			@name=Final Fantasy Advance
			@end=00

			; Standard ASCII mapping
			20=<space>
			21=!
			22="
			27='
			28=(
			29=)
			2C=,
			2D=-
			2E=.
			2F=/
			30=0
			31=1
			32=2
			33=3
			34=4
			35=5
			36=6
			37=7
			38=8
			39=9
			3A=:
			3B=;
			3F=?
			41=A
			42=B
			43=C
			44=D
			45=E
			46=F
			47=G
			48=H
			49=I
			4A=J
			4B=K
			4C=L
			4D=M
			4E=N
			4F=O
			50=P
			51=Q
			52=R
			53=S
			54=T
			55=U
			56=V
			57=W
			58=X
			59=Y
			5A=Z
			61=a
			62=b
			63=c
			64=d
			65=e
			66=f
			67=g
			68=h
			69=i
			6A=j
			6B=k
			6C=l
			6D=m
			6E=n
			6F=o
			70=p
			71=q
			72=r
			73=s
			74=t
			75=u
			76=v
			77=w
			78=x
			79=y
			7A=z

			; Control codes
			01=[LINE]
			02=[END]
			03=[WAIT]
			04=[NAME]
			05=[ITEM]
			06=[NUM]
			07=[COLOR]
			08=[ICON]
			""";

		return TableFile.LoadFromTbl(content);
	}

	private static byte[] GetTextBytes(ReadOnlySpan<byte> data, int offset, TableFile table) {
		var bytes = new List<byte>();
		int pos = offset;

		while (pos < data.Length) {
			byte b = data[pos];
			bytes.Add(b);

			if (table.EndByte.HasValue && b == table.EndByte.Value) {
				break;
			}
			if (b == 0x00 && !table.EndByte.HasValue) {
				break;
			}

			pos++;

			// Safety limit
			if (bytes.Count > 4096) break;
		}

		return [.. bytes];
	}
}
