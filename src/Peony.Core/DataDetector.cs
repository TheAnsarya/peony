namespace Peony.Analysis;

using Peony.Core;

/// <summary>
/// Detects non-code regions in ROMs (data tables, graphics, etc.)
/// </summary>
public static class DataDetector {
/// <summary>
/// Analyzes memory regions to classify as code, data, or graphics
/// </summary>
public static List<DetectedRegion> Analyze(byte[] rom, int codeStart, int codeEnd, string platform) {
var regions = new List<DetectedRegion>();
var codeMap = new bool[rom.Length];

// First pass: mark known code regions
MarkCodeRegions(rom, codeMap, codeStart, codeEnd);

// Second pass: classify unmarked regions
int regionStart = -1;
RegionType? currentType = null;

for (int i = codeStart; i < codeEnd && i < rom.Length; i++) {
if (!codeMap[i]) {
var type = ClassifyByte(rom, i, platform);

if (regionStart == -1) {
regionStart = i;
currentType = type;
} else if (type != currentType) {
// End current region, start new one
if (regionStart >= 0 && currentType.HasValue) {
regions.Add(new DetectedRegion(regionStart, i - 1, currentType.Value));
}
regionStart = i;
currentType = type;
}
} else {
// Code region - end any data region
if (regionStart >= 0 && currentType.HasValue) {
regions.Add(new DetectedRegion(regionStart, i - 1, currentType.Value));
regionStart = -1;
currentType = null;
}
}
}

// Finalize last region
if (regionStart >= 0 && currentType.HasValue) {
regions.Add(new DetectedRegion(regionStart, Math.Min(codeEnd, rom.Length) - 1, currentType.Value));
}

// Merge adjacent regions of same type
return MergeRegions(regions);
}

/// <summary>
/// Heuristic checks for graphics data patterns
/// </summary>
public static bool IsLikelyGraphics(byte[] data, int start, int length) {
if (length < 8) return false;

// Check for 8-byte tile patterns (common in NES/GB)
int patternScore = 0;
int samples = Math.Min(length / 8, 16);

for (int i = 0; i < samples; i++) {
int offset = start + i * 8;
if (offset + 8 > data.Length) break;

// Check for bitplane patterns
bool hasVariation = false;
for (int j = 0; j < 8; j++) {
if (data[offset + j] != data[offset]) {
hasVariation = true;
break;
}
}

if (hasVariation) patternScore++;
}

return patternScore >= samples / 2;
}

/// <summary>
/// Checks if region looks like a pointer table
/// </summary>
public static bool IsLikelyPointerTable(byte[] data, int start, int length, int romBase) {
if (length < 4) return false;

int validPointers = 0;
int samples = Math.Min(length / 2, 16);

for (int i = 0; i < samples; i++) {
int offset = start + i * 2;
if (offset + 2 > data.Length) break;

// Read little-endian word
int addr = data[offset] | (data[offset + 1] << 8);

// Check if it points within ROM space
if (addr >= romBase && addr < romBase + data.Length) {
validPointers++;
}
}

return validPointers >= samples / 2;
}

/// <summary>
/// Checks if region looks like text data
/// </summary>
public static bool IsLikelyText(byte[] data, int start, int length) {
if (length < 4) return false;

int printable = 0;
int samples = Math.Min(length, 64);

for (int i = 0; i < samples; i++) {
byte b = data[start + i];
// Check for printable ASCII or common text encoding values
if ((b >= 0x20 && b <= 0x7e) || // ASCII printable
(b >= 0x80 && b <= 0x9f) || // Common DTE range
b == 0x00 || b == 0xff)     // String terminators
{
printable++;
}
}

return printable >= samples * 0.7;
}

private static void MarkCodeRegions(byte[] rom, bool[] codeMap, int start, int end) {
// Simple pass - mark areas with valid opcodes
int i = start;
while (i < end && i < rom.Length) {
byte opcode = rom[i];
int instrLen = Get6502InstructionLength(opcode);

// Check for JAM (illegal halt) opcodes
if (IsJamOpcode(opcode)) {
i++;
continue;
}

// Mark instruction bytes
for (int j = 0; j < instrLen && i + j < codeMap.Length; j++) {
codeMap[i + j] = true;
}

i += instrLen;
}
}

private static RegionType ClassifyByte(byte[] rom, int offset, string platform) {
// Look at context to classify
int windowSize = 16;
int start = Math.Max(0, offset - windowSize / 2);
int end = Math.Min(rom.Length, offset + windowSize / 2);
var window = rom[start..end];

// Check patterns
if (IsLikelyGraphics(rom, start, window.Length))
return RegionType.Graphics;

if (IsLikelyPointerTable(rom, start, window.Length, GetRomBase(platform)))
return RegionType.PointerTable;

if (IsLikelyText(rom, start, window.Length))
return RegionType.Text;

return RegionType.Data;
}

private static int GetRomBase(string platform) => platform switch {
"Atari 2600" => 0xf000,
"NES" => 0x8000,
"SNES" => 0x8000,
"Game Boy" => 0x0000,
_ => 0x0000
};

private static int Get6502InstructionLength(byte opcode) {
// Addressing mode determines length
return (opcode & 0x1f) switch {
0x00 when (opcode & 0xe0) == 0x00 => (opcode == 0x00 || opcode == 0x40 || opcode == 0x60) ? 1 : 2,
0x00 when (opcode & 0xe0) == 0x20 => 3, // JSR
0x01 => 2, // (zp,x)
0x05 => 2, // zp
0x09 => 2, // #imm
0x0d => 3, // abs
0x11 => 2, // (zp),y
0x15 => 2, // zp,x
0x19 => 3, // abs,y
0x1d => 3, // abs,x
_ => 1
};
}

private static bool IsJamOpcode(byte opcode) {
// JAM opcodes that halt the CPU
return opcode is 0x02 or 0x12 or 0x22 or 0x32 or 0x42 or 0x52 or 0x62 or 0x72 or 0x92 or 0xb2 or 0xd2 or 0xf2;
}

private static List<DetectedRegion> MergeRegions(List<DetectedRegion> regions) {
if (regions.Count < 2) return regions;

var merged = new List<DetectedRegion>();
var current = regions[0];

for (int i = 1; i < regions.Count; i++) {
var next = regions[i];
if (next.Type == current.Type && next.Start <= current.End + 8) {
// Merge
current = new DetectedRegion(current.Start, next.End, current.Type);
} else {
merged.Add(current);
current = next;
}
}
merged.Add(current);

return merged;
}
}

public record DetectedRegion(int Start, int End, RegionType Type);

public enum RegionType {
Code,
Data,
Graphics,
PointerTable,
Text,
Unknown
}
