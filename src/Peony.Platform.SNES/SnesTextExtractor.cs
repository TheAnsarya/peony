namespace Peony.Platform.SNES;

using Peony.Core;

/// <summary>
/// SNES-specific text extractor with common RPG text patterns
/// </summary>
public class SnesTextExtractor : ITextExtractor {
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
	/// Extract text from a 24-bit (long) pointer table
	/// Common in SNES RPGs for cross-bank text access
	/// </summary>
	public List<TextBlock> ExtractFromLongPointerTable(
		ReadOnlySpan<byte> data,
		int pointerTableOffset,
		int pointerCount,
		TableFile table,
		SnesMapMode mapMode = SnesMapMode.LoRom,
		string? category = null) {

		var results = new List<TextBlock>();

		for (int i = 0; i < pointerCount; i++) {
			int ptrOffset = pointerTableOffset + i * 3;
			if (ptrOffset + 3 > data.Length) break;

			// Read 24-bit pointer (little-endian)
			uint lowWord = (uint)(data[ptrOffset] | (data[ptrOffset + 1] << 8));
			uint bank = data[ptrOffset + 2];
			uint snesAddress = (bank << 16) | lowWord;

			// Convert SNES address to file offset
			int fileOffset = SnesAddressToOffset(snesAddress, mapMode);
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
	/// Create a Final Fantasy style table (SNES RPGs)
	/// </summary>
	public static TableFile CreateFinalFantasyTable() {
		var content = """
			@name=Final Fantasy (SNES)
			@end=00

			; Uppercase letters
			82=A
			83=B
			84=C
			85=D
			86=E
			87=F
			88=G
			89=H
			8A=I
			8B=J
			8C=K
			8D=L
			8E=M
			8F=N
			90=O
			91=P
			92=Q
			93=R
			94=S
			95=T
			96=U
			97=V
			98=W
			99=X
			9A=Y
			9B=Z

			; Lowercase letters
			9C=a
			9D=b
			9E=c
			9F=d
			A0=e
			A1=f
			A2=g
			A3=h
			A4=i
			A5=j
			A6=k
			A7=l
			A8=m
			A9=n
			AA=o
			AB=p
			AC=q
			AD=r
			AE=s
			AF=t
			B0=u
			B1=v
			B2=w
			B3=x
			B4=y
			B5=z

			; Numbers
			B6=0
			B7=1
			B8=2
			B9=3
			BA=4
			BB=5
			BC=6
			BD=7
			BE=8
			BF=9

			; Punctuation
			C0=!
			C1=?
			C2=/
			C3=:
			C4="
			C5='
			C6=-
			C7=.
			C8=,
			C9=;
			CA=#
			CB=+
			CC=(
			CD=)
			CE=%
			CF=~
			FF=<space>

			; Control codes
			01=[LINE]
			02=[END]
			03=[WAIT]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Chrono Trigger style table
	/// </summary>
	public static TableFile CreateChronoTriggerTable() {
		var content = """
			@name=Chrono Trigger
			@end=00

			; Standard ASCII-like mapping starting at 0x20
			20=<space>
			21=!
			22="
			23=#
			24=$
			25=%
			26=&
			27='
			28=(
			29=)
			2A=*
			2B=+
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
			3C=<
			3D==
			3E=>
			3F=?
			40=@
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
			5B=[
			5C=\
			5D=]
			5E=^
			5F=_
			60=`
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
			03=[CRONO]
			04=[MARLE]
			05=[LUCCA]
			06=[ROBO]
			07=[FROG]
			08=[AYLA]
			09=[MAGUS]
			0A=[EPOCH]
			0B=[ITEM]
			0C=[NUM]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Dragon Quest/Warrior style SNES table
	/// </summary>
	public static TableFile CreateDragonQuestTable() {
		var content = """
			@name=Dragon Quest (SNES)
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
			41='
			42=-
			43=!
			44=?
			45=(
			46=)
			47=:

			; Control codes
			F0=[NAME]
			F1=[ITEM]
			F2=[NUM]
			F3=[MONSTER]
			F4=[SPELL]
			F5=[WAIT]
			F6=[LINE]
			F7=[PAGE]
			F8=[CHOICE]
			FF=[END]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Convert SNES address to file offset
	/// </summary>
	public static int SnesAddressToOffset(uint snesAddress, SnesMapMode mapMode) {
		uint bank = (snesAddress >> 16) & 0xff;
		uint offset = snesAddress & 0xffff;

		return mapMode switch {
			SnesMapMode.LoRom => LoRomToOffset(bank, offset),
			SnesMapMode.HiRom => HiRomToOffset(bank, offset),
			_ => -1
		};
	}

	private static int LoRomToOffset(uint bank, uint offset) {
		// LoROM: Banks $00-$7D map to ROM at $8000-$FFFF
		if (offset < 0x8000) {
			// Mirror or WRAM area
			return -1;
		}

		if (bank <= 0x7d) {
			return (int)((bank * 0x8000) + (offset - 0x8000));
		}
		if (bank >= 0x80 && bank <= 0xff) {
			// Mirror of banks $00-$7F
			return (int)(((bank - 0x80) * 0x8000) + (offset - 0x8000));
		}

		return -1;
	}

	private static int HiRomToOffset(uint bank, uint offset) {
		// HiROM: Banks $C0-$FF map to ROM at $0000-$FFFF
		if (bank >= 0xc0) {
			return (int)(((bank - 0xc0) * 0x10000) + offset);
		}
		if (bank >= 0x40 && bank <= 0x7d) {
			return (int)(((bank - 0x40) * 0x10000) + offset);
		}

		return -1;
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
