namespace Peony.Platform.NES;

using Peony.Core;

/// <summary>
/// NES-specific text extractor with common RPG text patterns
/// </summary>
public class NesTextExtractor : ITextExtractor {
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
	/// Extract text from a standard NES pointer table
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
	/// Create a Dragon Quest/Warrior style table file
	/// </summary>
	public static TableFile CreateDragonQuestTable() {
		var table = new TableFile { Name = "Dragon Quest" };

		// Standard uppercase letters (typical DQ encoding)
		for (int i = 0; i < 26; i++) {
			table.ByteMappings.GetType(); // Trick to allow private access workaround
		}

		// Use LoadFromTbl for actual initialization
		var content = """
			@name=Dragon Quest
			@end=FC

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
			9A=a
			9B=b
			9C=c
			9D=d
			9E=e
			9F=f
			A0=g
			A1=h
			A2=i
			A3=j
			A4=k
			A5=l
			A6=m
			A7=n
			A8=o
			A9=p
			AA=q
			AB=r
			AC=s
			AD=t
			AE=u
			AF=v
			B0=w
			B1=x
			B2=y
			B3=z

			; Numbers
			B4=0
			B5=1
			B6=2
			B7=3
			B8=4
			B9=5
			BA=6
			BB=7
			BC=8
			BD=9

			; Punctuation
			BE='
			BF=,
			C0=.
			C1=-
			C2=?
			C3=!
			C4=(
			C5=)
			C6=<space>

			; Control codes (common DQ patterns)
			F0=[WAIT]
			F1=[LINE]
			F2=[PAGE]
			F8=[NAME]
			F9=[ITEM]
			FA=[NUM]
			FB=[ENEMY]
			FC=[END]
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Create a Final Fantasy style table file
	/// </summary>
	public static TableFile CreateFinalFantasyTable() {
		var content = """
			@name=Final Fantasy
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
			80=0
			81=1

			; Space
			FF=<space>

			; Punctuation
			C0='
			C1=,
			C2=.
			C3=:
			C4=;
			C5=?
			C6=!
			C7=(
			C8=)
			""";

		return TableFile.LoadFromTbl(content);
	}

	/// <summary>
	/// Extract item names from a typical NES RPG format
	/// </summary>
	public List<TextBlock> ExtractFixedLengthStrings(
		ReadOnlySpan<byte> data,
		int offset,
		int count,
		int stringLength,
		TableFile table,
		string category = "items") {

		var blocks = new List<TextBlock>();

		for (int i = 0; i < count; i++) {
			int strOffset = offset + (i * stringLength);
			if (strOffset + stringLength > data.Length) break;

			var rawBytes = data.Slice(strOffset, stringLength).ToArray();

			// Find actual string end (trim trailing spaces/padding)
			int actualLength = stringLength;
			while (actualLength > 0 && (rawBytes[actualLength - 1] == 0x00 || rawBytes[actualLength - 1] == 0xff)) {
				actualLength--;
			}

			var options = new TextExtractionOptions {
				NullTerminated = false,
				IncludeControlCodes = false
			};

			var text = TextExtraction.ExtractText(data, strOffset, actualLength, table, options);

			blocks.Add(new TextBlock {
				Offset = strOffset,
				Length = stringLength,
				Text = text.Trim(),
				RawBytes = rawBytes,
				Label = $"{category}_{i:x2}",
				Category = category
			});
		}

		return blocks;
	}

	/// <summary>
	/// Common NES RPG control codes
	/// </summary>
	public static Dictionary<byte, string> CommonControlCodes => new() {
		[0xf0] = "WAIT",
		[0xf1] = "LINE",
		[0xf2] = "PAGE",
		[0xf3] = "CLEAR",
		[0xf4] = "DELAY",
		[0xf5] = "SOUND",
		[0xf6] = "MUSIC",
		[0xf7] = "CHOICE",
		[0xf8] = "NAME",
		[0xf9] = "ITEM",
		[0xfa] = "NUM",
		[0xfb] = "ENEMY",
		[0xfc] = "END",
		[0xfd] = "NEWLINE",
		[0xfe] = "PAUSE",
		[0xff] = "SPACE"
	};
}
