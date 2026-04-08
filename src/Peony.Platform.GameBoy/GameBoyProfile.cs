namespace Peony.Platform.GameBoy;

using Peony.Core;

/// <summary>
/// Game Boy platform profile — ties together SM83/GameBoy decoder,
/// Game Boy analyzer, and generic Poppy output generator.
/// </summary>
public sealed class GameBoyProfile : IPlatformProfile {
	public static readonly GameBoyProfile Instance = new();

	public PlatformId Platform => PlatformId.GameBoy;
	public string DisplayName => "Game Boy";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	public IGraphicsExtractor? GraphicsExtractor { get; }
	public ITextExtractor? TextExtractor { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".gb", ".gbc"];
	public byte? PansyPlatformId => 0x03;
	public string PoppyPlatformId => "gb";

	private GameBoyProfile() {
		var analyzer = new GameBoyAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
		GraphicsExtractor = new GameBoyChrExtractor();
		TextExtractor = new GameBoyTextExtractor();
	}
}
