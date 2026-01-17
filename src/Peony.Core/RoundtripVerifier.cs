namespace Peony.Core;

/// <summary>
/// Verifies that disassembled code can reassemble to the original ROM.
/// This is the key guarantee of Peony: ROM → ASM → ROM roundtrip fidelity.
/// </summary>
public class RoundtripVerifier {
	/// <summary>
	/// Verification result with detailed information
	/// </summary>
	public record VerificationResult(
		bool Success,
		int OriginalSize,
		int ReassembledSize,
		int ByteMatches,
		int ByteDifferences,
		List<ByteDifference> Differences,
		string? ErrorMessage = null
	) {
		public double MatchPercentage => OriginalSize > 0 ? (double)ByteMatches / OriginalSize * 100 : 0;
	}

	/// <summary>
	/// Represents a single byte difference between original and reassembled ROMs
	/// </summary>
	public record ByteDifference(
		int Offset,
		uint Address,
		byte Original,
		byte Reassembled,
		string? Context = null
	);

	/// <summary>
	/// Verify that a reassembled ROM matches the original
	/// </summary>
	public static VerificationResult Verify(byte[] original, byte[] reassembled) {
		var differences = new List<ByteDifference>();
		var matches = 0;
		var diffs = 0;

		var maxLen = Math.Max(original.Length, reassembled.Length);
		var minLen = Math.Min(original.Length, reassembled.Length);

		// Compare overlapping bytes
		for (int i = 0; i < minLen; i++) {
			if (original[i] == reassembled[i]) {
				matches++;
			} else {
				diffs++;
				if (differences.Count < 100) {  // Limit stored differences
					var address = (uint)(0x8000 + (i % 0x8000));  // Approximate address
					differences.Add(new ByteDifference(i, address, original[i], reassembled[i]));
				}
			}
		}

		// Handle size differences
		for (int i = minLen; i < maxLen; i++) {
			diffs++;
			if (differences.Count < 100) {
				var origByte = i < original.Length ? original[i] : (byte)0xff;
				var reassByte = i < reassembled.Length ? reassembled[i] : (byte)0xff;
				differences.Add(new ByteDifference(i, (uint)(0x8000 + (i % 0x8000)), origByte, reassByte, "Size mismatch"));
			}
		}

		var success = diffs == 0;
		var errorMessage = success ? null :
			original.Length != reassembled.Length
				? $"Size mismatch: original {original.Length} bytes, reassembled {reassembled.Length} bytes"
				: $"{diffs} byte(s) differ";

		return new VerificationResult(
			Success: success,
			OriginalSize: original.Length,
			ReassembledSize: reassembled.Length,
			ByteMatches: matches,
			ByteDifferences: diffs,
			Differences: differences,
			ErrorMessage: errorMessage
		);
	}

	/// <summary>
	/// Verify files on disk
	/// </summary>
	public static VerificationResult VerifyFiles(string originalPath, string reassembledPath) {
		if (!File.Exists(originalPath)) {
			return new VerificationResult(false, 0, 0, 0, 0, [],
				$"Original file not found: {originalPath}");
		}

		if (!File.Exists(reassembledPath)) {
			return new VerificationResult(false, 0, 0, 0, 0, [],
				$"Reassembled file not found: {reassembledPath}");
		}

		var original = File.ReadAllBytes(originalPath);
		var reassembled = File.ReadAllBytes(reassembledPath);

		return Verify(original, reassembled);
	}

	/// <summary>
	/// Verify disassembly result can represent the original ROM.
	/// This checks that all bytes in code regions match the disassembled instructions.
	/// </summary>
	public static VerificationResult VerifyDisassembly(DisassemblyResult result, byte[] original) {
		var differences = new List<ByteDifference>();
		var matches = 0;
		var diffs = 0;

		foreach (var block in result.Blocks) {
			foreach (var line in block.Lines) {
				// Get the offset in the ROM for this address
				var offset = (int)line.Address - 0x8000;  // Simple mapping, should use platform analyzer
				if (offset < 0) offset = (int)line.Address;  // For RAM/ZP addresses

				// Check if disassembled bytes match original
				for (int i = 0; i < line.Bytes.Length; i++) {
					var romOffset = offset + i;
					if (romOffset >= 0 && romOffset < original.Length) {
						if (line.Bytes[i] == original[romOffset]) {
							matches++;
						} else {
							diffs++;
							if (differences.Count < 100) {
								differences.Add(new ByteDifference(
									romOffset,
									(uint)(line.Address + i),
									original[romOffset],
									line.Bytes[i],
									$"In instruction: {line.Content}"
								));
							}
						}
					}
				}
			}
		}

		var totalChecked = matches + diffs;
		var success = diffs == 0;
		var errorMessage = success ? null : $"{diffs} byte(s) differ in disassembled instructions";

		return new VerificationResult(
			Success: success,
			OriginalSize: original.Length,
			ReassembledSize: totalChecked,
			ByteMatches: matches,
			ByteDifferences: diffs,
			Differences: differences,
			ErrorMessage: errorMessage
		);
	}

	/// <summary>
	/// Generate a difference report as text
	/// </summary>
	public static string GenerateReport(VerificationResult result) {
		var sb = new System.Text.StringBuilder();

		sb.AppendLine("═══════════════════════════════════════════════════════════════");
		sb.AppendLine("                    ROUNDTRIP VERIFICATION REPORT              ");
		sb.AppendLine("═══════════════════════════════════════════════════════════════");
		sb.AppendLine();

		if (result.Success) {
			sb.AppendLine("✓ VERIFICATION PASSED - ROMs are identical");
		} else {
			sb.AppendLine("✗ VERIFICATION FAILED");
			if (result.ErrorMessage != null) {
				sb.AppendLine($"  Error: {result.ErrorMessage}");
			}
		}

		sb.AppendLine();
		sb.AppendLine($"Original size:      {result.OriginalSize,8} bytes");
		sb.AppendLine($"Reassembled size:   {result.ReassembledSize,8} bytes");
		sb.AppendLine($"Bytes matching:     {result.ByteMatches,8}");
		sb.AppendLine($"Bytes different:    {result.ByteDifferences,8}");
		sb.AppendLine($"Match percentage:   {result.MatchPercentage,8:F2}%");

		if (result.Differences.Count > 0) {
			sb.AppendLine();
			sb.AppendLine("First differences:");
			sb.AppendLine("───────────────────────────────────────────────────────────────");
			sb.AppendLine("  Offset    Address   Original   Reassembled   Context");
			sb.AppendLine("───────────────────────────────────────────────────────────────");

			foreach (var diff in result.Differences.Take(20)) {
				var context = diff.Context ?? "";
				sb.AppendLine($"  0x{diff.Offset:x6}  ${diff.Address:x4}     0x{diff.Original:x2}       0x{diff.Reassembled:x2}         {context}");
			}

			if (result.Differences.Count > 20) {
				sb.AppendLine($"  ... and {result.Differences.Count - 20} more differences");
			}
		}

		sb.AppendLine();
		sb.AppendLine("═══════════════════════════════════════════════════════════════");

		return sb.ToString();
	}

	/// <summary>
	/// Run a full roundtrip test: disassemble, reassemble, verify
	/// This requires an external assembler (like Poppy) to be available.
	/// </summary>
	/// <param name="originalRomPath">Path to the original ROM file</param>
	/// <param name="workingDirectory">Working directory for temporary files</param>
	/// <param name="analyzer">Platform analyzer (required - determines disassembly strategy)</param>
	/// <param name="assemblerCommand">Assembler command to run (default: poppy)</param>
	public static async Task<VerificationResult> RunRoundtripAsync(
		string originalRomPath,
		string workingDirectory,
		IPlatformAnalyzer analyzer,
		string assemblerCommand = "poppy") {

		// Step 1: Load original ROM
		if (!File.Exists(originalRomPath)) {
			return new VerificationResult(false, 0, 0, 0, 0, [],
				$"Original ROM not found: {originalRomPath}");
		}

		var originalRom = File.ReadAllBytes(originalRomPath);

		// Step 2: Disassemble
		var info = analyzer.Analyze(originalRom);
		var entryPoints = analyzer.GetEntryPoints(originalRom);
		var engine = new DisassemblyEngine(analyzer.CpuDecoder, analyzer);
		var result = engine.Disassemble(originalRom, entryPoints);
		result.RomInfo = info;

		// Step 3: Generate assembly file
		Directory.CreateDirectory(workingDirectory);
		var asmPath = Path.Combine(workingDirectory, "disasm.pasm");
		var formatter = new PoppyFormatter();
		formatter.Generate(result, asmPath);

		// Step 4: Reassemble (requires external assembler)
		var outputPath = Path.Combine(workingDirectory, "reassembled.bin");
		var assembleResult = await RunAssemblerAsync(assemblerCommand, asmPath, outputPath);

		if (!assembleResult.Success) {
			return new VerificationResult(false, originalRom.Length, 0, 0, 0, [],
				$"Assembly failed: {assembleResult.ErrorMessage}");
		}

		// Step 5: Verify
		if (!File.Exists(outputPath)) {
			return new VerificationResult(false, originalRom.Length, 0, 0, 0, [],
				"Reassembled ROM not created");
		}

		return VerifyFiles(originalRomPath, outputPath);
	}

	private static async Task<(bool Success, string? ErrorMessage)> RunAssemblerAsync(
		string command, string inputPath, string outputPath) {

		try {
			var process = new System.Diagnostics.Process {
				StartInfo = new System.Diagnostics.ProcessStartInfo {
					FileName = command,
					Arguments = $"\"{inputPath}\" -o \"{outputPath}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			var stderr = await process.StandardError.ReadToEndAsync();
			await process.WaitForExitAsync();

			if (process.ExitCode != 0) {
				return (false, stderr.Length > 0 ? stderr : $"Assembler exited with code {process.ExitCode}");
			}

			return (true, null);
		}
		catch (Exception ex) {
			return (false, $"Failed to run assembler: {ex.Message}");
		}
	}
}
