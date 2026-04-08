namespace Peony.Platform.WonderSwan;

using Peony.Core;

/// <summary>
/// WonderSwan platform profile — ties together V30MZ decoder,
/// WonderSwan analyzer, and generic Poppy output generator.
/// </summary>
public sealed class WonderSwanProfile : IPlatformProfile {
	public static readonly WonderSwanProfile Instance = new();

	public PlatformId Platform => PlatformId.WonderSwan;
	public string DisplayName => "WonderSwan";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".ws", ".wsc"];
	public byte? PansyPlatformId => 0x0a;

	private WonderSwanProfile() {
		var analyzer = new WonderSwanAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
