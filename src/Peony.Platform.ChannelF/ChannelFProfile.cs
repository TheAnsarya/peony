namespace Peony.Platform.ChannelF;

using Peony.Core;

/// <summary>
/// Fairchild Channel F platform profile — ties together F8 decoder,
/// Channel F analyzer, and generic Poppy output generator.
/// </summary>
public sealed class ChannelFProfile : IPlatformProfile {
	public static readonly ChannelFProfile Instance = new();

	public PlatformId Platform => PlatformId.ChannelF;
	public string DisplayName => "Fairchild Channel F";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".chf"];
	public byte? PansyPlatformId => null;

	private ChannelFProfile() {
		var analyzer = new ChannelFAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
