namespace Peony.Platform.GBA;

using Peony.Core;

/// <summary>
/// Game Boy Advance platform profile — ties together ARM7TDMI decoder,
/// GBA analyzer, and generic Poppy output generator.
/// </summary>
public sealed class GbaProfile : IPlatformProfile {
	public static readonly GbaProfile Instance = new();

	public PlatformId Platform => PlatformId.GBA;
	public string DisplayName => "Game Boy Advance";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".gba"];
	public byte? PansyPlatformId => 0x04;

	private GbaProfile() {
		var analyzer = new GbaAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
