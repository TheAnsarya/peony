namespace Peony.Core;

/// <summary>
/// Loads and manages symbols (labels, comments, data definitions) for disassembly
/// Supports common formats: FCEUX .nl files, Mesen .mlb files, JSON
/// </summary>
public class SymbolLoader {
private readonly Dictionary<uint, string> _labels = [];
private readonly Dictionary<uint, string> _comments = [];
private readonly Dictionary<uint, DataDefinition> _dataDefinitions = [];
private readonly Dictionary<(int Bank, uint Address), string> _bankLabels = [];

public IReadOnlyDictionary<uint, string> Labels => _labels;
public IReadOnlyDictionary<uint, string> Comments => _comments;
public IReadOnlyDictionary<uint, DataDefinition> DataDefinitions => _dataDefinitions;
public IReadOnlyDictionary<(int Bank, uint Address), string> BankLabels => _bankLabels;

/// <summary>
/// Load symbols from file (auto-detect format)
/// </summary>
public void Load(string path) {
var ext = Path.GetExtension(path).ToLowerInvariant();
var content = File.ReadAllText(path);

switch (ext) {
case ".nl":
LoadFceuxNl(content);
break;
case ".mlb":
LoadMesenMlb(content);
break;
case ".json":
LoadJson(content);
break;
case ".sym":
LoadGenericSym(content);
break;
default:
// Try to auto-detect
if (content.TrimStart().StartsWith('{'))
LoadJson(content);
else
LoadGenericSym(content);
break;
}
}

/// <summary>
/// Load FCEUX .nl (name list) format
/// Format: $XXXX#LabelName#Comment
/// </summary>
private void LoadFceuxNl(string content) {
foreach (var line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
var trimmed = line.Trim();
if (string.IsNullOrEmpty(trimmed) || !trimmed.StartsWith('$')) continue;

var parts = trimmed.Split('#');
if (parts.Length < 2) continue;

if (uint.TryParse(parts[0][1..], System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
if (!string.IsNullOrWhiteSpace(parts[1]))
_labels[addr] = parts[1].Trim();
if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
_comments[addr] = parts[2].Trim();
}
}
}

/// <summary>
/// Load Mesen .mlb (label) format
/// Format: Type:Address:Name
/// </summary>
private void LoadMesenMlb(string content) {
foreach (var line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
var trimmed = line.Trim();
if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';')) continue;

var parts = trimmed.Split(':');
if (parts.Length < 3) continue;

var type = parts[0];
if (!uint.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out var addr))
continue;

var name = parts[2].Trim();
if (string.IsNullOrWhiteSpace(name)) continue;

// Mesen types: P=PRG, C=CHR, R=RAM, G=Register, S=Save
if (type is "P" or "R" or "G") {
_labels[addr] = name;
}
}
}

/// <summary>
/// Load JSON symbol format
/// </summary>
private void LoadJson(string content) {
// Simple JSON parsing without external dependencies
// Expected format: { "labels": { "8000": "reset", ... }, "comments": { ... } }
// For full implementation, use System.Text.Json

var json = System.Text.Json.JsonDocument.Parse(content);
var root = json.RootElement;

if (root.TryGetProperty("labels", out var labels)) {
foreach (var prop in labels.EnumerateObject()) {
if (uint.TryParse(prop.Name, System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
_labels[addr] = prop.Value.GetString() ?? "";
}
}
}

if (root.TryGetProperty("comments", out var comments)) {
foreach (var prop in comments.EnumerateObject()) {
if (uint.TryParse(prop.Name, System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
_comments[addr] = prop.Value.GetString() ?? "";
}
}
}

if (root.TryGetProperty("data", out var data)) {
foreach (var prop in data.EnumerateObject()) {
if (uint.TryParse(prop.Name, System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
var def = prop.Value;
var type = def.GetProperty("type").GetString() ?? "byte";
var count = def.TryGetProperty("count", out var c) ? c.GetInt32() : 1;
var name = def.TryGetProperty("name", out var n) ? n.GetString() : null;
_dataDefinitions[addr] = new DataDefinition(type, count, name);
}
}
}
}

/// <summary>
/// Load generic symbol file format
/// Format: ADDRESS LABEL [; comment]
/// </summary>
private void LoadGenericSym(string content) {
foreach (var line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
var trimmed = line.Trim();
if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
continue;

// Split by whitespace, handling comments
var commentIdx = trimmed.IndexOf(';');
string? comment = null;
if (commentIdx >= 0) {
comment = trimmed[(commentIdx + 1)..].Trim();
trimmed = trimmed[..commentIdx].Trim();
}

var parts = trimmed.Split([' ', '\t', '='], StringSplitOptions.RemoveEmptyEntries);
if (parts.Length < 2) continue;

// Try both orders: ADDR LABEL and LABEL = ADDR
uint addr;
string label;

var addrStr = parts[0].TrimStart('$', '0').TrimStart('x');
if (uint.TryParse(addrStr, System.Globalization.NumberStyles.HexNumber, null, out addr)) {
label = parts[1];
} else {
label = parts[0];
addrStr = parts[^1].TrimStart('$', '0').TrimStart('x');
if (!uint.TryParse(addrStr, System.Globalization.NumberStyles.HexNumber, null, out addr))
continue;
}

_labels[addr] = label;
if (!string.IsNullOrWhiteSpace(comment))
_comments[addr] = comment;
}
}

/// <summary>
/// Add a label manually
/// </summary>
public void AddLabel(uint address, string label) {
_labels[address] = label;
}

/// <summary>
/// Add a bank-specific label
/// </summary>
public void AddBankLabel(int bank, uint address, string label) {
_bankLabels[(bank, address)] = label;
}

/// <summary>
/// Get label for address, checking bank-specific first
/// </summary>
public string? GetLabel(uint address, int bank = -1) {
if (bank >= 0 && _bankLabels.TryGetValue((bank, address), out var bankLabel))
return bankLabel;
return _labels.GetValueOrDefault(address);
}
}

/// <summary>
/// Data definition for structured data areas
/// </summary>
public record DataDefinition(string Type, int Count, string? Name);
