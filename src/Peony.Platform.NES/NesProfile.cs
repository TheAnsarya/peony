namespace Peony.Platform.NES;

using Peony.Core;

/// <summary>
/// NES platform profile — ties together 6502 decoder, NES analyzer,
/// and generic Poppy output generator.
/// </summary>
public sealed class NesProfile : IPlatformProfile {
	public static readonly NesProfile Instance = new();

	public PlatformId Platform => PlatformId.NES;
	public string DisplayName => "Nintendo Entertainment System";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
	public IGraphicsExtractor? GraphicsExtractor { get; }
	public ITextExtractor? TextExtractor { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".nes"];
	public byte? PansyPlatformId => 0x01;
	public string PoppyPlatformId => "nes";

	private NesProfile() {
		var analyzer = new NesAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
		GraphicsExtractor = new NesChrExtractor();
		TextExtractor = new NesTextExtractor();
	}
}
