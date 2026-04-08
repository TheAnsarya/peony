namespace Peony.Core;

/// <summary>
/// Master plugin interface for a platform component.
/// Ties together CPU decoder, platform analyzer, output generator, and asset extractors.
/// Each platform implements this interface and registers via PlatformResolver.
/// </summary>
public interface IPlatformProfile {
	/// <summary>Platform identifier</summary>
	PlatformId Platform { get; }

	/// <summary>Human-readable platform name (e.g., "Super Nintendo")</summary>
	string DisplayName { get; }

	/// <summary>CPU instruction decoder for this platform</summary>
	ICpuDecoder CpuDecoder { get; }

	/// <summary>Platform-specific ROM analyzer</summary>
	IPlatformAnalyzer Analyzer { get; }

	/// <summary>Platform-specific output generator for .pasm files</summary>
	IOutputGenerator OutputGenerator { get; }

	/// <summary>Platform-specific asset extractors (graphics, text, palettes, etc.)</summary>
	IReadOnlyList<IAssetExtractor> AssetExtractors { get; }

	/// <summary>Platform-specific graphics/CHR extractor, null if not available</summary>
	IGraphicsExtractor? GraphicsExtractor { get; }

	/// <summary>Platform-specific text extractor, null if not available</summary>
	ITextExtractor? TextExtractor { get; }

	/// <summary>ROM file extensions for auto-detection (e.g., ".sfc", ".smc")</summary>
	IReadOnlyList<string> RomExtensions { get; }

	/// <summary>Pansy platform ID byte (0x01=NES, 0x02=SNES, etc.), null if not mapped</summary>
	byte? PansyPlatformId { get; }
}
