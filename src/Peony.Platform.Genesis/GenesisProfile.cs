namespace Peony.Platform.Genesis;

using Peony.Core;

/// <summary>
/// Sega Genesis/Mega Drive platform profile — ties together M68000 decoder,
/// Genesis analyzer, and generic Poppy output generator.
/// </summary>
public sealed class GenesisProfile : IPlatformProfile {
	public static readonly GenesisProfile Instance = new();

	public PlatformId Platform => PlatformId.Genesis;
	public string DisplayName => "Sega Genesis";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	public IGraphicsExtractor? GraphicsExtractor => null;
	public ITextExtractor? TextExtractor => null;

	public IReadOnlyList<string> RomExtensions { get; } = [".md", ".gen", ".bin"];
	public byte? PansyPlatformId => 0x05;
	public string PoppyPlatformId => "genesis";

	private GenesisProfile() {
		var analyzer = new GenesisAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
