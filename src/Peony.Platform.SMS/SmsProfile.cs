namespace Peony.Platform.SMS;

using Peony.Core;

/// <summary>
/// Sega Master System platform profile — ties together Z80 decoder,
/// SMS analyzer, and generic Poppy output generator.
/// </summary>
public sealed class SmsProfile : IPlatformProfile {
	public static readonly SmsProfile Instance = new();

	public PlatformId Platform => PlatformId.SMS;
	public string DisplayName => "Sega Master System";

	public ICpuDecoder CpuDecoder { get; }
	public IPlatformAnalyzer Analyzer { get; }
	public IOutputGenerator OutputGenerator { get; }
	public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	public IReadOnlyList<string> RomExtensions { get; } = [".sms", ".gg"];
	public byte? PansyPlatformId => 0x06;

	private SmsProfile() {
		var analyzer = new SmsAnalyzer();
		CpuDecoder = analyzer.CpuDecoder;
		Analyzer = analyzer;
		OutputGenerator = PoppyFormatter.Instance;
		AssetExtractors = [];
	}
}
