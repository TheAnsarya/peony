namespace Peony.Platform.M68000;

using Peony.Core;

/// <summary>
/// Flat M68000 platform profile — for arcade systems and other 68000-based
/// hardware with a flat (non-banked) memory map.
/// </summary>
public sealed class M68000Profile : IPlatformProfile {
    public static readonly M68000Profile Instance = new();

    public PlatformId Platform => PlatformId.M68000;
    public string DisplayName => "M68000 (Flat)";

    public ICpuDecoder CpuDecoder { get; }
    public IPlatformAnalyzer Analyzer { get; }
    public IOutputGenerator OutputGenerator { get; }
    public IReadOnlyList<IAssetExtractor> AssetExtractors { get; }
    public IGraphicsExtractor? GraphicsExtractor => null;
    public ITextExtractor? TextExtractor => null;

    public IReadOnlyList<string> RomExtensions { get; } = [".bin", ".rom", ".68k"];
    public byte? PansyPlatformId => null;  // No Pansy mapping yet
    public string PoppyPlatformId => "m68000";

    private M68000Profile() {
        var analyzer = new M68000Analyzer();
        CpuDecoder = analyzer.CpuDecoder;
        Analyzer = analyzer;
        OutputGenerator = PoppyFormatter.Instance;
        AssetExtractors = [];
    }
}
