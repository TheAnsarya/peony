namespace Peony.Platform.GameBoy;

using Peony.Core;

/// <summary>
/// Game Boy text extractor with common RPG text patterns
/// </summary>
public class GameBoyTextExtractor : ITextExtractor {
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
	/// Extract text from a 16-bit pointer table
	/// </summary>
	public List<TextBlock> ExtractFromPointerTable(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		int textBankOffset,
		TableFile table,
		string? category = null) {

		var options = new TextExtractionOptions {
			NullTerminated = true,
			IncludeControlCodes = true,
			Category = category
		};

		return TextExtraction.ExtractFromPointerTable(
			data, pointerTableOffset, pointerCount, textBankOffset, table, options);
	}

	/// <summary>
	/// Extract text from a banked pointer table
	/// Game Boy uses memory banks, pointers are within bank context
	/// </summary>
	public List<TextBlock> ExtractFromBankedPointers(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		int bank,
		TableFile table,
		string? category = null) {

		var results = new List<TextBlock>();

		for (int i = 0; i < pointerCount; i++) {
			int ptrOffset = pointerTableOffset + i * 2;
			if (ptrOffset + 2 > data.Length) break;

			// Read 16-bit pointer (little-endian)
			int ptr = data[ptrOffset] | (data[ptrOffset + 1] << 8);

			// Convert banked address to file offset
			int fileOffset = GameBoyAddressToOffset(ptr, bank);
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
	/// Convert Game Boy address to file offset
	/// </summary>
	public static int GameBoyAddressToOffset(int address, int bank) {
		// Bank 0: $0000-$3FFF maps to file offset 0
		// Bank n: $4000-$7FFF maps to file offset bank * 0x4000
		if (address < 0x4000) {
			// Bank 0 (fixed)
			return address;
		} else if (address >= 0x4000 && address <= 0x7fff) {
			// Switchable bank
			return (bank * 0x4000) + (address - 0x4000);
		}

		// Address outside ROM range
		return -1;
	}

	/// <summary>
	/// Convert file offset to Game Boy address
	/// Returns (address, bank) tuple
	/// </summary>
	public static (int Address, int Bank) OffsetToGameBoyAddress(int offset) {
		if (offset < 0x4000) {
			return (offset, 0);
		}

		int bank = offset / 0x4000;
		int address = 0x4000 + (offset % 0x4000);
		return (address, bank);
	}

	/// <summary>
	/// Create a Pokemon Red/Blue style table
	/// </summary>
	public static TableFile CreatePokemonTable() {
		var content = """
			@name=Pokemon (Game Boy)
			@end=50

			; Uppercase letters
			80=A
			81=B
			82=C
			83=D
			84=E
			85=F
			86=G
			87=H
			88=I
			89=J
			8A=K
			8B=L
			8C=M
			8D=N
			8E=O
			8F=P
			90=Q
			91=R
			92=S
			93=T
			94=U
			95=V
			96=W
			97=X
			98=Y
			99=Z

			; Lowercase letters
			A0=a
			A1=b
			A2=c
			A3=d
			A4=e
			A5=f
			A6=g
			A7=h
			A8=i
			A9=j
			AA=k
			AB=l
			AC=m
			AD=n
			AE=o
			AF=p
			B0=q
			B1=r
			B2=s
			B3=t
			B4=u
			B5=v
			B6=w
			B7=x
			B8=y
			B9=z

			; Numbers
			F6=0
			F7=1
			F8=2
			F9=3
			FA=4
			FB=5
			FC=6
			FD=7
			FE=8
			FF=9

			; Punctuation
			E3=!
			E6=?
			E0='
			E1="
			E2="
			75=...
			F4=,
			F5=.
			7F=<space>

			; Special characters
			4F=[LINE]
			4E=[NEXT]
			51=[PARA]
			52=[PLAYER]
			53=[RIVAL]
			55=[CONT]
			57=[DONE]
			58=[PROMPT]
			50=[END]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Dragon Warrior (Game Boy) style table
	/// </summary>
	public static TableFile CreateDragonWarriorGbTable() {
		var content = """
			@name=Dragon Warrior (Game Boy)
			@end=FF

			; Uppercase letters
			00=A
			01=B
			02=C
			03=D
			04=E
			05=F
			06=G
			07=H
			08=I
			09=J
			0A=K
			0B=L
			0C=M
			0D=N
			0E=O
			0F=P
			10=Q
			11=R
			12=S
			13=T
			14=U
			15=V
			16=W
			17=X
			18=Y
			19=Z

			; Lowercase letters
			1A=a
			1B=b
			1C=c
			1D=d
			1E=e
			1F=f
			20=g
			21=h
			22=i
			23=j
			24=k
			25=l
			26=m
			27=n
			28=o
			29=p
			2A=q
			2B=r
			2C=s
			2D=t
			2E=u
			2F=v
			30=w
			31=x
			32=y
			33=z

			; Numbers
			34=0
			35=1
			36=2
			37=3
			38=4
			39=5
			3A=6
			3B=7
			3C=8
			3D=9

			; Punctuation
			3E=<space>
			3F=.
			40=,
			41=-
			42='
			43=!
			44=?
			45=(
			46=)
			47=:

			; Control codes
			F0=[NAME]
			F1=[LINE]
			F2=[END]
			FF=[STOP]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Zelda (Link's Awakening) style table
	/// </summary>
	public static TableFile CreateZeldaTable() {
		var content = """
			@name=Zelda (Game Boy)
			@end=FF

			; Standard ASCII-like starting at 0x20
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
			F0=[LINE]
			F1=[CHOICE]
			F2=[ITEM]
			F3=[NAME]
			FE=[SCROLL]
			FF=[END]
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
