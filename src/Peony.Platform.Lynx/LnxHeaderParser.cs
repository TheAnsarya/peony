namespace Peony.Platform.Lynx;

/// <summary>
/// Parser for Atari Lynx LNX ROM header format.
/// </summary>
/// <remarks>
/// <para>
/// The LNX format is a container format for Atari Lynx ROMs. It consists of
/// a 64-byte header followed by the ROM data (encrypted or unencrypted).
/// </para>
/// <para>
/// LNX Header Format (64 bytes):
/// - Offset 0-3: Magic "LYNX" (4 bytes)
/// - Offset 4-5: Bank 0 page count (16-bit little-endian)
/// - Offset 6-7: Bank 1 page count (16-bit little-endian)
/// - Offset 8-9: Version (16-bit little-endian)
/// - Offset 10-41: Cart name (32 bytes, null-terminated ASCII)
/// - Offset 42-57: Manufacturer name (16 bytes, null-terminated ASCII)
/// - Offset 58: Rotation (0=None, 1=Left, 2=Right)
/// - Offset 59-63: Reserved/spare bytes (5 bytes)
/// </para>
/// <para>
/// Each page is 256 bytes. Total ROM size = (Bank0Pages + Bank1Pages) * 256.
/// </para>
/// </remarks>
public class LnxHeaderParser {
	/// <summary>
	/// The LNX header magic bytes "LYNX".
	/// </summary>
	public static readonly byte[] Magic = [0x4c, 0x59, 0x4e, 0x58]; // "LYNX"

	/// <summary>
	/// Size of the LNX header in bytes.
	/// </summary>
	public const int HeaderSize = 64;

	/// <summary>
	/// Size of each page in bytes.
	/// </summary>
	public const int PageSize = 256;

	/// <summary>
	/// Parses an LNX header from ROM data.
	/// </summary>
	/// <param name="data">The ROM data including the header.</param>
	/// <returns>The parsed header, or null if not a valid LNX file.</returns>
	public static LnxHeader? Parse(ReadOnlySpan<byte> data) {
		if (data.Length < HeaderSize) {
			return null;
		}

		// Check magic bytes
		if (data[0] != Magic[0] || data[1] != Magic[1] || data[2] != Magic[2] || data[3] != Magic[3]) {
			return null;
		}

		// Parse fields
		var bank0Pages = (ushort)(data[4] | (data[5] << 8));
		var bank1Pages = (ushort)(data[6] | (data[7] << 8));
		var version = (ushort)(data[8] | (data[9] << 8));

		// Parse cart name (null-terminated, max 32 bytes)
		var cartName = ParseNullTerminatedString(data.Slice(10, 32));

		// Parse manufacturer name (null-terminated, max 16 bytes)
		var manufacturer = ParseNullTerminatedString(data.Slice(42, 16));

		// Rotation
		var rotation = (LnxRotation)data[58];
		if (!Enum.IsDefined(rotation)) {
			rotation = LnxRotation.None;
		}

		// Spare bytes (offset 59-63)
		var spare = data.Slice(59, 5).ToArray();

		return new LnxHeader(
			Bank0Pages: bank0Pages,
			Bank1Pages: bank1Pages,
			Version: version,
			CartName: cartName,
			Manufacturer: manufacturer,
			Rotation: rotation,
			SpareBytes: spare
		);
	}

	/// <summary>
	/// Checks if the ROM data has a valid LNX header.
	/// </summary>
	/// <param name="data">The ROM data to check.</param>
	/// <returns>True if the data starts with an LNX header, false otherwise.</returns>
	public static bool HasLnxHeader(ReadOnlySpan<byte> data) {
		if (data.Length < 4) {
			return false;
		}

		return data[0] == Magic[0] && data[1] == Magic[1] && data[2] == Magic[2] && data[3] == Magic[3];
	}

	/// <summary>
	/// Gets the ROM data offset (after the header) for LNX files.
	/// </summary>
	/// <param name="data">The ROM data to check.</param>
	/// <returns>The offset where ROM data begins (64 for LNX, 0 otherwise).</returns>
	public static int GetRomDataOffset(ReadOnlySpan<byte> data) {
		return HasLnxHeader(data) ? HeaderSize : 0;
	}

	/// <summary>
	/// Parses a null-terminated ASCII string from a span.
	/// </summary>
	private static string ParseNullTerminatedString(ReadOnlySpan<byte> data) {
		var nullIndex = data.IndexOf((byte)0);
		var length = nullIndex >= 0 ? nullIndex : data.Length;

		// Find the actual end (trim trailing spaces)
		while (length > 0 && (data[length - 1] == 0 || data[length - 1] == 0x20)) {
			length--;
		}

		if (length == 0) {
			return string.Empty;
		}

		return System.Text.Encoding.ASCII.GetString(data.Slice(0, length));
	}
}

/// <summary>
/// Represents a parsed LNX header.
/// </summary>
/// <param name="Bank0Pages">Number of 256-byte pages in bank 0.</param>
/// <param name="Bank1Pages">Number of 256-byte pages in bank 1.</param>
/// <param name="Version">LNX version number.</param>
/// <param name="CartName">Cartridge name (up to 32 characters).</param>
/// <param name="Manufacturer">Manufacturer name (up to 16 characters).</param>
/// <param name="Rotation">Screen rotation mode.</param>
/// <param name="SpareBytes">Reserved bytes (5 bytes).</param>
public record LnxHeader(
	ushort Bank0Pages,
	ushort Bank1Pages,
	ushort Version,
	string CartName,
	string Manufacturer,
	LnxRotation Rotation,
	byte[] SpareBytes
) {
	/// <summary>
	/// Gets the total ROM size in bytes (excluding header).
	/// </summary>
	public int RomSize => (Bank0Pages + Bank1Pages) * LnxHeaderParser.PageSize;

	/// <summary>
	/// Gets the size of bank 0 in bytes.
	/// </summary>
	public int Bank0Size => Bank0Pages * LnxHeaderParser.PageSize;

	/// <summary>
	/// Gets the size of bank 1 in bytes.
	/// </summary>
	public int Bank1Size => Bank1Pages * LnxHeaderParser.PageSize;

	/// <summary>
	/// Gets the total number of banks (0, 1, or 2).
	/// </summary>
	public int BankCount {
		get {
			var count = 0;
			if (Bank0Pages > 0) count++;
			if (Bank1Pages > 0) count++;
			return count;
		}
	}
}

/// <summary>
/// Screen rotation modes for Lynx games.
/// </summary>
public enum LnxRotation : byte {
	/// <summary>No rotation (horizontal).</summary>
	None = 0,

	/// <summary>Rotate left 90 degrees.</summary>
	Left = 1,

	/// <summary>Rotate right 90 degrees.</summary>
	Right = 2
}
