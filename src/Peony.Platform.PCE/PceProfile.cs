namespace Peony.Platform.PCE;

using Peony.Core;

/// <summary>
/// PC Engine/TurboGrafx-16 platform profile — ties together HuC6280 decoder,
/// PCE analyzer, and generic Poppy output generator.
/// </summary>
public sealed class PceProfile : IPlatformProfile {
	public static readonly PceProfile Instance = new();

	public PlatformId Platform => PlatformId.PCE;
	public string DisplayName => "PC Engine";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	public IGraphicsExtractor? GraphicsExtractor => null;
	public ITextExtractor? TextExtractor => null;

	public IReadOnlyList<string> RomExtensions { get; } = [".pce"];
	public byte? PansyPlatformId => 0x07;
	public string PoppyPlatformId => "pce";

	private PceProfile() {
		var analyzer = new PceAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
