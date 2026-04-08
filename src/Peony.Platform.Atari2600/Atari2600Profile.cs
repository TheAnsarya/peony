namespace Peony.Platform.Atari2600;

using Peony.Core;

/// <summary>
/// Atari 2600 platform profile — ties together 6502 decoder,
/// Atari 2600 analyzer, and generic Poppy output generator.
/// </summary>
public sealed class Atari2600Profile : IPlatformProfile {
	public static readonly Atari2600Profile Instance = new();

	public PlatformId Platform => PlatformId.Atari2600;
	public string DisplayName => "Atari 2600";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	public IGraphicsExtractor? GraphicsExtractor => null;
	public ITextExtractor? TextExtractor => null;

	public IReadOnlyList<string> RomExtensions { get; } = [".a26", ".bin"];
	public byte? PansyPlatformId => 0x08;
	public string PoppyPlatformId => "atari2600";

	private Atari2600Profile() {
		var analyzer = new Atari2600Analyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
