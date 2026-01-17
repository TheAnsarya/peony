namespace Peony.Core;

/// <summary>
/// ROM file loader with format detection
/// </summary>
public static class RomLoader {
/// <summary>
/// Load ROM from file
/// </summary>
public static byte[] Load(string path) {
if (!File.Exists(path))
throw new FileNotFoundException($"ROM file not found: {path}");

var data = File.ReadAllBytes(path);
return StripHeader(data, path);
}

/// <summary>
/// Strip platform-specific headers
/// </summary>
private static byte[] StripHeader(byte[] data, string path) {
var ext = Path.GetExtension(path).ToLowerInvariant();

return ext switch {
// NES iNES header (16 bytes, starts with NES^Z)
".nes" when data.Length > 16 && data[0] == 'N' && data[1] == 'E' && data[2] == 'S' && data[3] == 0x1a
=> data[16..],

// SNES with copier header (512 bytes if size % 1024 == 512)
".sfc" or ".smc" when data.Length % 1024 == 512
=> data[512..],

// Atari 2600 - no header
".a26" or ".bin" => data,

// Game Boy (no header to strip, header is part of ROM)
".gb" or ".gbc" => data,

// Default: no header
_ => data
};
}

/// <summary>
/// Detect platform from ROM content
/// </summary>
public static string DetectPlatform(ReadOnlySpan<byte> rom, string path) {
var ext = Path.GetExtension(path).ToLowerInvariant();

// Extension hints
if (ext is ".a26") return "atari2600";
if (ext is ".nes") return "nes";
if (ext is ".sfc" or ".smc") return "snes";
if (ext is ".gb" or ".gbc") return "gb";

// Size-based detection for Atari 2600
if (rom.Length is 2048 or 4096 or 8192 or 16384 or 32768) {
// Small ROMs are likely Atari 2600
if (rom.Length <= 4096) return "atari2600";
}

// NES detection (after header strip, PRG is multiple of 16KB)
if (rom.Length % 16384 == 0 && rom.Length <= 512 * 1024)
return "nes";

// Default to unknown
return "unknown";
}
}
