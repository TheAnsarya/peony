namespace Peony.Core;

/// <summary>
/// Per-platform asset extractor for extracting game assets (graphics, text, palettes, etc.)
/// </summary>
public interface IAssetExtractor {
	/// <summary>Asset type name (e.g., "Graphics", "Text", "Palettes")</summary>
	string AssetType { get; }

	/// <summary>Extract assets from ROM data to the output directory</summary>
	void Extract(ReadOnlySpan<byte> rom, RomInfo romInfo, string outputDir);

	/// <summary>Check if this extractor can handle the given ROM</summary>
	bool CanExtract(RomInfo romInfo);
}
