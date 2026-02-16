using Xunit;

namespace Peony.Platform.Lynx.Tests;

/// <summary>
/// Tests for <see cref="LnxHeaderParser"/>.
/// </summary>
public class LnxHeaderParserTests {
	/// <summary>
	/// Valid LNX header bytes for testing.
	/// 64 bytes: magic (4) + bank0 (2) + bank1 (2) + version (2) + cartname (32) + manufacturer (16) + rotation (1) + spare (5)
	/// </summary>
	private static byte[] CreateValidLnxHeader(
		ushort bank0Pages = 2,
		ushort bank1Pages = 0,
		string cartName = "Test Game",
		string manufacturer = "TestMfg",
		LnxRotation rotation = LnxRotation.None) {
		var header = new byte[64];

		// Magic "LYNX"
		header[0] = (byte)'L';
		header[1] = (byte)'Y';
		header[2] = (byte)'N';
		header[3] = (byte)'X';

		// Bank 0 page count (little-endian)
		header[4] = (byte)(bank0Pages & 0xff);
		header[5] = (byte)((bank0Pages >> 8) & 0xff);

		// Bank 1 page count (little-endian)
		header[6] = (byte)(bank1Pages & 0xff);
		header[7] = (byte)((bank1Pages >> 8) & 0xff);

		// Version (1.0)
		header[8] = 0x01;
		header[9] = 0x00;

		// Cart name (32 bytes, null-padded)
		var nameBytes = System.Text.Encoding.ASCII.GetBytes(cartName);
		Array.Copy(nameBytes, 0, header, 10, Math.Min(nameBytes.Length, 32));

		// Manufacturer (16 bytes, null-padded)
		var mfgBytes = System.Text.Encoding.ASCII.GetBytes(manufacturer);
		Array.Copy(mfgBytes, 0, header, 42, Math.Min(mfgBytes.Length, 16));

		// Rotation
		header[58] = (byte)rotation;

		return header;
	}

	[Fact]
	public void HasLnxHeader_WithValidMagic_ReturnsTrue() {
		var header = CreateValidLnxHeader();

		var result = LnxHeaderParser.HasLnxHeader(header);

		Assert.True(result);
	}

	[Fact]
	public void HasLnxHeader_WithInvalidMagic_ReturnsFalse() {
		var data = new byte[64];
		data[0] = (byte)'N';
		data[1] = (byte)'E';
		data[2] = (byte)'S';
		data[3] = 0x1a;

		var result = LnxHeaderParser.HasLnxHeader(data);

		Assert.False(result);
	}

	[Fact]
	public void HasLnxHeader_WithTooShortData_ReturnsFalse() {
		var data = new byte[3]; // Less than 4 bytes

		var result = LnxHeaderParser.HasLnxHeader(data);

		Assert.False(result);
	}

	[Fact]
	public void Parse_WithValidHeader_ReturnsLnxHeader() {
		var header = CreateValidLnxHeader(bank0Pages: 4, bank1Pages: 2, cartName: "My Game", manufacturer: "Atari");

		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal(4, result.Bank0Pages);
		Assert.Equal(2, result.Bank1Pages);
		Assert.Equal("My Game", result.CartName);
		Assert.Equal("Atari", result.Manufacturer);
		Assert.Equal(LnxRotation.None, result.Rotation);
	}

	[Fact]
	public void Parse_WithLeftRotation_ParsesRotationCorrectly() {
		var header = CreateValidLnxHeader(rotation: LnxRotation.Left);

		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal(LnxRotation.Left, result.Rotation);
	}

	[Fact]
	public void Parse_WithRightRotation_ParsesRotationCorrectly() {
		var header = CreateValidLnxHeader(rotation: LnxRotation.Right);

		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal(LnxRotation.Right, result.Rotation);
	}

	[Fact]
	public void Parse_WithInvalidMagic_ReturnsNull() {
		var data = new byte[64];

		var result = LnxHeaderParser.Parse(data);

		Assert.Null(result);
	}

	[Fact]
	public void Parse_WithTooShortData_ReturnsNull() {
		var header = CreateValidLnxHeader();
		var shortData = header[0..32]; // Only 32 bytes

		var result = LnxHeaderParser.Parse(shortData);

		Assert.Null(result);
	}

	[Fact]
	public void GetRomDataOffset_WithLnxHeader_Returns64() {
		var header = CreateValidLnxHeader();

		var result = LnxHeaderParser.GetRomDataOffset(header);

		Assert.Equal(64, result);
	}

	[Fact]
	public void GetRomDataOffset_WithRawRom_Returns0() {
		var rawRom = new byte[1024]; // No LNX header

		var result = LnxHeaderParser.GetRomDataOffset(rawRom);

		Assert.Equal(0, result);
	}

	[Fact]
	public void LnxHeader_RomSize_CalculatesCorrectly() {
		// Bank0 = 4 pages × 256 bytes = 1024
		// Bank1 = 2 pages × 256 bytes = 512
		// Total = 1536 bytes
		var header = CreateValidLnxHeader(bank0Pages: 4, bank1Pages: 2);
		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal(1024, result.Bank0Size);
		Assert.Equal(512, result.Bank1Size);
		Assert.Equal(1536, result.RomSize);
		Assert.Equal(2, result.BankCount);
	}

	[Fact]
	public void LnxHeader_SingleBank_BankCountIs1() {
		var header = CreateValidLnxHeader(bank0Pages: 4, bank1Pages: 0);
		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal(1, result.BankCount);
	}

	[Fact]
	public void Parse_TrimsNullsFromStrings() {
		var header = CreateValidLnxHeader(cartName: "Game\0\0\0", manufacturer: "Mfg\0\0");

		var result = LnxHeaderParser.Parse(header);

		Assert.NotNull(result);
		Assert.Equal("Game", result.CartName);
		Assert.Equal("Mfg", result.Manufacturer);
	}
}
