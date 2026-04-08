namespace Peony.Platform.SNES;

using Peony.Core;
using Peony.Cpu;

/// <summary>
/// SNES platform profile — ties together 65816 decoder, SNES analyzer,
/// SNES output generator, and SNES asset extractors.
/// </summary>
public sealed class SnesProfile : IPlatformProfile {
	public static readonly SnesProfile Instance = new();

	public PlatformId Platform => PlatformId.SNES;
	public string DisplayName => "Super Nintendo";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".sfc", ".smc"];
	public byte? PansyPlatformId => 0x02;

	private SnesProfile() {
		var analyzer = new SnesAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = new SnesOutputGenerator();
		AssetExtractors = [];
	}
}
