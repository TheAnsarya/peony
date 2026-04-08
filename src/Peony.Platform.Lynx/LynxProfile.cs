namespace Peony.Platform.Lynx;

using Peony.Core;

/// <summary>
/// Atari Lynx platform profile — ties together 65SC02 decoder,
/// Lynx analyzer, and generic Poppy output generator.
/// </summary>
public sealed class LynxProfile : IPlatformProfile {
	public static readonly LynxProfile Instance = new();

	public PlatformId Platform => PlatformId.Lynx;
	public string DisplayName => "Atari Lynx";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".lnx", ".lyx"];
	public byte? PansyPlatformId => 0x09;

	private LynxProfile() {
		var analyzer = new LynxAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
