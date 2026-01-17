using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for RoundtripVerifier functionality
/// </summary>
public class RoundtripVerifierTests {
	// ========== Verify Method Tests ==========

	[Fact]
	public void Verify_IdenticalBytes_ReturnsSuccess() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
		var reassembled = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.True(result.Success);
		Assert.Equal(5, result.ByteMatches);
		Assert.Equal(0, result.ByteDifferences);
		Assert.Empty(result.Differences);
		Assert.Null(result.ErrorMessage);
	}

	[Fact]
	public void Verify_DifferentBytes_ReturnsFailure() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
		var reassembled = new byte[] { 0x00, 0xff, 0x02, 0x03, 0x04 };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.False(result.Success);
		Assert.Equal(4, result.ByteMatches);
		Assert.Equal(1, result.ByteDifferences);
		Assert.Single(result.Differences);
		Assert.Equal(1, result.Differences[0].Offset);
		Assert.Equal(0x01, result.Differences[0].Original);
		Assert.Equal(0xff, result.Differences[0].Reassembled);
	}

	[Fact]
	public void Verify_DifferentSizes_ReturnsFailure() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
		var reassembled = new byte[] { 0x00, 0x01, 0x02 };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.False(result.Success);
		Assert.Equal(5, result.OriginalSize);
		Assert.Equal(3, result.ReassembledSize);
		Assert.Contains("Size mismatch", result.ErrorMessage);
	}

	[Fact]
	public void Verify_EmptyArrays_ReturnsSuccess() {
		var original = Array.Empty<byte>();
		var reassembled = Array.Empty<byte>();

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.True(result.Success);
		Assert.Equal(0, result.ByteMatches);
		Assert.Equal(0, result.ByteDifferences);
	}

	[Fact]
	public void Verify_MultipleDifferences_CapturesAll() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
		var reassembled = new byte[] { 0xff, 0x01, 0xff, 0x03, 0xff };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.False(result.Success);
		Assert.Equal(2, result.ByteMatches);
		Assert.Equal(3, result.ByteDifferences);
		Assert.Equal(3, result.Differences.Count);
	}

	// ========== MatchPercentage Tests ==========

	[Fact]
	public void MatchPercentage_AllMatch_Returns100() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
		var reassembled = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.Equal(100.0, result.MatchPercentage);
	}

	[Fact]
	public void MatchPercentage_HalfMatch_Returns50() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03 };
		var reassembled = new byte[] { 0x00, 0x01, 0xff, 0xff };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.Equal(50.0, result.MatchPercentage);
	}

	[Fact]
	public void MatchPercentage_NoMatch_Returns0() {
		var original = new byte[] { 0x00, 0x01, 0x02, 0x03 };
		var reassembled = new byte[] { 0xff, 0xfe, 0xfd, 0xfc };

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.Equal(0.0, result.MatchPercentage);
	}

	// ========== VerifyFiles Tests ==========

	[Fact]
	public void VerifyFiles_MissingOriginal_ReturnsError() {
		var result = RoundtripVerifier.VerifyFiles(
			"/nonexistent/original.bin",
			"/nonexistent/reassembled.bin"
		);

		Assert.False(result.Success);
		Assert.Contains("not found", result.ErrorMessage);
	}

	[Fact]
	public void VerifyFiles_IdenticalFiles_ReturnsSuccess() {
		var tempFile1 = Path.GetTempFileName();
		var tempFile2 = Path.GetTempFileName();

		try {
			var data = new byte[] { 0xa9, 0x00, 0x8d, 0x00, 0x20, 0x60 };
			File.WriteAllBytes(tempFile1, data);
			File.WriteAllBytes(tempFile2, data);

			var result = RoundtripVerifier.VerifyFiles(tempFile1, tempFile2);

			Assert.True(result.Success);
			Assert.Equal(6, result.ByteMatches);
		} finally {
			File.Delete(tempFile1);
			File.Delete(tempFile2);
		}
	}

	// ========== GenerateReport Tests ==========

	[Fact]
	public void GenerateReport_Success_ContainsPassedMessage() {
		var result = new RoundtripVerifier.VerificationResult(
			Success: true,
			OriginalSize: 1000,
			ReassembledSize: 1000,
			ByteMatches: 1000,
			ByteDifferences: 0,
			Differences: []
		);

		var report = RoundtripVerifier.GenerateReport(result);

		Assert.Contains("VERIFICATION PASSED", report);
		Assert.Contains("1000", report);
	}

	[Fact]
	public void GenerateReport_Failure_ContainsDifferences() {
		var result = new RoundtripVerifier.VerificationResult(
			Success: false,
			OriginalSize: 1000,
			ReassembledSize: 1000,
			ByteMatches: 995,
			ByteDifferences: 5,
			Differences: [
				new RoundtripVerifier.ByteDifference(100, 0x8064, 0xa9, 0x00, "Test"),
			],
			ErrorMessage: "5 byte(s) differ"
		);

		var report = RoundtripVerifier.GenerateReport(result);

		Assert.Contains("VERIFICATION FAILED", report);
		Assert.Contains("5 byte(s) differ", report);
		Assert.Contains("0x000064", report);  // Offset in hex
	}

	// ========== VerifyDisassembly Tests ==========

	[Fact]
	public void VerifyDisassembly_MatchingBytes_ReturnsSuccess() {
		var rom = new byte[] { 0xa9, 0x00, 0x8d, 0x00, 0x20, 0x60 };
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", rom.Length, null, [])
		};

		// Create a block with matching bytes
		var lines = new List<DisassembledLine> {
			new(0x8000, [0xa9, 0x00], null, "lda #$00", null),
			new(0x8002, [0x8d, 0x00, 0x20], null, "sta $2000", null),
			new(0x8005, [0x60], null, "rts", null)
		};
		var block = new DisassembledBlock(0x8000, 0x8005, MemoryRegion.Code, lines);
		result.Blocks.Add(block);

		var verifyResult = RoundtripVerifier.VerifyDisassembly(result, rom);

		Assert.True(verifyResult.Success);
		Assert.Equal(6, verifyResult.ByteMatches);
	}

	[Fact]
	public void VerifyDisassembly_MismatchedBytes_ReturnsFailure() {
		var rom = new byte[] { 0xa9, 0x00, 0x8d, 0x00, 0x20, 0x60 };
		var result = new DisassemblyResult {
			RomInfo = new RomInfo("Test", rom.Length, null, [])
		};

		// Create a block with wrong bytes
		var lines = new List<DisassembledLine> {
			new(0x8000, [0xa9, 0xff], null, "lda #$ff", null)  // Wrong!
		};
		var block = new DisassembledBlock(0x8000, 0x8001, MemoryRegion.Code, lines);
		result.Blocks.Add(block);

		var verifyResult = RoundtripVerifier.VerifyDisassembly(result, rom);

		Assert.False(verifyResult.Success);
		Assert.Equal(1, verifyResult.ByteDifferences);
	}

	// ========== Difference Limit Test ==========

	[Fact]
	public void Verify_ManyDifferences_LimitsCaptured() {
		var original = new byte[1000];
		var reassembled = new byte[1000];
		Array.Fill(reassembled, (byte)0xff);

		var result = RoundtripVerifier.Verify(original, reassembled);

		Assert.False(result.Success);
		Assert.Equal(1000, result.ByteDifferences);
		Assert.Equal(100, result.Differences.Count);  // Limited to 100
	}
}
