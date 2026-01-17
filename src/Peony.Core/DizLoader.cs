using System.IO.Compression;
using System.Text.Json;
using System.Xml;

namespace Peony.Core;

/// <summary>
/// Loads DiztinGUIsh (.diz) project files for enhanced disassembly.
/// DIZ files contain labels, comments, and code/data type information
/// from the DiztinGUIsh SNES disassembler.
/// </summary>
public class DizLoader {
	private readonly Dictionary<int, DizLabel> _labels = [];
	private readonly Dictionary<int, DizDataType> _dataTypes = [];
	private readonly string _projectName;
	private readonly string _mapMode;
	private readonly int _romSize;

	/// <summary>
	/// Data types from DiztinGUIsh.
	/// </summary>
	public enum DizDataType {
		Unreached = 0,
		Opcode = 1,
		Operand = 2,
		Data8 = 3,
		Graphics = 4,
		Music = 5,
		Empty = 6,
		Data16 = 7,
		Pointer16 = 8,
		Data24 = 9,
		Pointer24 = 10,
		Data32 = 11,
		Pointer32 = 12,
		Text = 13
	}

	/// <summary>
	/// Represents a label from a DIZ file.
	/// </summary>
	public record DizLabel(string Name, string Comment, DizDataType DataType);

	/// <summary>
	/// Gets all labels indexed by address.
	/// </summary>
	public IReadOnlyDictionary<int, DizLabel> Labels => _labels;

	/// <summary>
	/// Gets per-byte data type information.
	/// </summary>
	public IReadOnlyDictionary<int, DizDataType> DataTypes => _dataTypes;

	/// <summary>
	/// Gets the project name.
	/// </summary>
	public string ProjectName => _projectName;

	/// <summary>
	/// Gets the ROM map mode (e.g., "LoRom", "HiRom").
	/// </summary>
	public string MapMode => _mapMode;

	/// <summary>
	/// Gets the ROM size in bytes.
	/// </summary>
	public int RomSize => _romSize;

	/// <summary>
	/// Private constructor - use Load() factory method.
	/// </summary>
	private DizLoader(string projectName, string mapMode, int romSize) {
		_projectName = projectName;
		_mapMode = mapMode;
		_romSize = romSize;
	}

	/// <summary>
	/// Loads a DIZ file from disk.
	/// Supports both native DiztinGUIsh XML format and simplified JSON format.
	/// </summary>
	/// <param name="path">Path to the .diz file.</param>
	/// <returns>A new DizLoader instance.</returns>
	public static DizLoader Load(string path) {
		var data = File.ReadAllBytes(path);

		// Check if gzip compressed
		byte[] contentBytes;
		if (data.Length >= 2 && data[0] == 0x1f && data[1] == 0x8b) {
			using var input = new MemoryStream(data);
			using var gzip = new GZipStream(input, CompressionMode.Decompress);
			using var output = new MemoryStream();
			gzip.CopyTo(output);
			contentBytes = output.ToArray();
		} else {
			contentBytes = data;
		}

		// Detect format: XML starts with '<', JSON starts with '{'
		var firstChar = (char)contentBytes.FirstOrDefault(b => !char.IsWhiteSpace((char)b));
		if (firstChar == '<') {
			return ParseXml(contentBytes);
		} else {
			return ParseJson(contentBytes);
		}
	}

	/// <summary>
	/// Parses the JSON content of a DIZ file.
	/// </summary>
	private static DizLoader ParseJson(byte[] jsonBytes) {
		using var doc = JsonDocument.Parse(jsonBytes);
		var root = doc.RootElement;

		var projectName = root.TryGetProperty("ProjectName", out var pn) ? pn.GetString() ?? "Unknown" : "Unknown";
		var mapMode = root.TryGetProperty("RomMapMode", out var mm) ? mm.GetString() ?? "Unknown" : "Unknown";
		var romSize = root.TryGetProperty("RomSize", out var rs) ? rs.GetInt32() : 0;

		var loader = new DizLoader(projectName, mapMode, romSize);

		// Parse labels
		if (root.TryGetProperty("Labels", out var labels)) {
			foreach (var prop in labels.EnumerateObject()) {
				if (!int.TryParse(prop.Name, out var address)) continue;

				var labelElement = prop.Value;
				var name = labelElement.TryGetProperty("Name", out var n) ? n.GetString() ?? "" : "";
				var comment = labelElement.TryGetProperty("Comment", out var c) ? c.GetString() ?? "" : "";
				var dataType = labelElement.TryGetProperty("DataType", out var dt)
					? (DizDataType)dt.GetInt32()
					: DizDataType.Unreached;

				loader._labels[address] = new DizLabel(name, comment, dataType);
			}
		}

		// Parse data types array
		if (root.TryGetProperty("DataTypes", out var dataTypes)) {
			var index = 0;
			foreach (var element in dataTypes.EnumerateArray()) {
				var dt = (DizDataType)element.GetInt32();
				if (dt != DizDataType.Unreached) {
					loader._dataTypes[index] = dt;
				}
				index++;
			}
		}

		// Alternative format: RomBytes with per-byte data
		if (root.TryGetProperty("RomBytes", out var romBytes)) {
			var index = 0;
			foreach (var byteElement in romBytes.EnumerateArray()) {
				if (byteElement.TryGetProperty("DataType", out var dt)) {
					var dataType = (DizDataType)dt.GetInt32();
					if (dataType != DizDataType.Unreached) {
						loader._dataTypes[index] = dataType;
					}
				}
				index++;
			}
		}

		return loader;
	}

	/// <summary>
	/// Parses the native DiztinGUIsh XML format.
	/// </summary>
	private static DizLoader ParseXml(byte[] xmlBytes) {
		var doc = new XmlDocument();
		using var ms = new MemoryStream(xmlBytes);
		doc.Load(ms);

		var nsMgr = new XmlNamespaceManager(doc.NameTable);
		nsMgr.AddNamespace("diz", "clr-namespace:Diz.Core.serialization.xml_serializer;assembly=Diz.Core");
		nsMgr.AddNamespace("ns1", "clr-namespace:Diz.Core.model;assembly=Diz.Core");
		nsMgr.AddNamespace("sys", "https://extendedxmlserializer.github.io/system");

		// Get project info using local-name() to handle namespaces
		var projectNode = doc.SelectSingleNode("//*[local-name()='Project']");
		var gameName = projectNode?.Attributes?["InternalRomGameName"]?.Value?.Trim() ?? "Unknown";

		// Get Data node for RomMapMode
		var dataNode = doc.SelectSingleNode("//*[local-name()='Data']");
		var mapMode = dataNode?.Attributes?["RomMapMode"]?.Value ?? "Unknown";

		// Estimate ROM size from RomBytes content
		var romBytesNode = doc.SelectSingleNode("//*[local-name()='RomBytes']");
		var romBytesText = romBytesNode?.InnerText ?? "";
		var romSize = CountRomBytes(romBytesText);

		var loader = new DizLoader(gameName, mapMode, romSize);

		// Parse labels - use local-name() to handle sys: namespace prefix
		var labelsNode = doc.SelectSingleNode("//*[local-name()='Labels']");
		if (labelsNode != null) {
			foreach (XmlNode item in labelsNode.ChildNodes) {
				if (item.LocalName != "Item") continue;

				var keyAttr = item.Attributes?["Key"];
				if (keyAttr == null || !int.TryParse(keyAttr.Value, out var address)) continue;

				// Find Value child node - it's directly under Item
				XmlNode? valueNode = null;
				foreach (XmlNode child in item.ChildNodes) {
					if (child.LocalName == "Value") {
						valueNode = child;
						break;
					}
				}
				if (valueNode == null) continue;

				var name = valueNode.Attributes?["Name"]?.Value ?? "";
				var comment = valueNode.Attributes?["Comment"]?.Value ?? "";

				if (!string.IsNullOrWhiteSpace(name)) {
					loader._labels[address] = new DizLabel(name, comment, DizDataType.Unreached);
				}
			}
		}

		// Parse RomBytes (compressed format)
		if (!string.IsNullOrEmpty(romBytesText)) {
			ParseCompressedRomBytes(loader, romBytesText);
		}

		return loader;
	}

	/// <summary>
	/// Counts the number of ROM bytes from the compressed format.
	/// </summary>
	private static int CountRomBytes(string romBytesText) {
		var lines = romBytesText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var count = 0;
		foreach (var line in lines) {
			var trimmed = line.Trim();
			if (trimmed.StartsWith("version:") || string.IsNullOrEmpty(trimmed)) continue;
			count++;
		}
		return count;
	}

	/// <summary>
	/// Parses the compressed RomBytes format from DiztinGUIsh.
	/// Format: Each line represents one byte with flags like:
	/// +C = Opcode with code flag
	/// .C = Operand with code flag
	/// +D = Opcode with data flag
	/// .D = Operand with data flag
	/// U = Unreached
	/// </summary>
	private static void ParseCompressedRomBytes(DizLoader loader, string romBytesText) {
		var lines = romBytesText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var offset = 0;

		foreach (var line in lines) {
			var trimmed = line.Trim();
			if (trimmed.StartsWith("version:") || string.IsNullOrEmpty(trimmed)) continue;

			// Parse the flag character
			// + = Opcode start, . = Operand/continuation
			// C = Code, D = Data, U = Unreached
			var dataType = DizDataType.Unreached;

			if (trimmed.StartsWith('+') || trimmed.StartsWith('.')) {
				var isOpcode = trimmed.StartsWith('+');
				var rest = trimmed[1..];

				if (rest.StartsWith('C')) {
					dataType = isOpcode ? DizDataType.Opcode : DizDataType.Operand;
				} else if (rest.StartsWith('D')) {
					dataType = isOpcode ? DizDataType.Data8 : DizDataType.Data8;
				}
			} else if (trimmed == "U") {
				dataType = DizDataType.Unreached;
			}

			if (dataType != DizDataType.Unreached) {
				loader._dataTypes[offset] = dataType;
			}
			offset++;
		}
	}

	/// <summary>
	/// Gets the label at an address, if any.
	/// </summary>
	/// <param name="address">The address to look up.</param>
	/// <returns>The label or null if not found.</returns>
	public DizLabel? GetLabel(int address) {
		return _labels.GetValueOrDefault(address);
	}

	/// <summary>
	/// Gets the data type at an offset.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>The data type, or Unreached if not set.</returns>
	public DizDataType GetDataType(int offset) {
		return _dataTypes.GetValueOrDefault(offset, DizDataType.Unreached);
	}

	/// <summary>
	/// Checks if an offset is code (opcode or operand).
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is code.</returns>
	public bool IsCode(int offset) {
		var dt = GetDataType(offset);
		return dt == DizDataType.Opcode || dt == DizDataType.Operand;
	}

	/// <summary>
	/// Checks if an offset is an opcode.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is an opcode.</returns>
	public bool IsOpcode(int offset) {
		return GetDataType(offset) == DizDataType.Opcode;
	}

	/// <summary>
	/// Checks if an offset is data.
	/// </summary>
	/// <param name="offset">The ROM file offset.</param>
	/// <returns>True if the offset is any data type.</returns>
	public bool IsData(int offset) {
		var dt = GetDataType(offset);
		return dt >= DizDataType.Data8 && dt <= DizDataType.Text;
	}

	/// <summary>
	/// Gets all opcode offsets (potential disassembly entry points).
	/// </summary>
	/// <returns>Set of ROM offsets marked as opcodes.</returns>
	public HashSet<int> GetOpcodeOffsets() {
		return _dataTypes
			.Where(kv => kv.Value == DizDataType.Opcode)
			.Select(kv => kv.Key)
			.ToHashSet();
	}

	/// <summary>
	/// Gets all data regions by type.
	/// </summary>
	/// <param name="type">The data type to find.</param>
	/// <returns>List of (start, end, type) tuples.</returns>
	public List<(int Start, int End, DizDataType Type)> GetDataRegions(DizDataType? type = null) {
		var regions = new List<(int Start, int End, DizDataType Type)>();
		var filtered = type.HasValue
			? _dataTypes.Where(kv => kv.Value == type.Value)
			: _dataTypes.Where(kv => kv.Value != DizDataType.Unreached &&
									 kv.Value != DizDataType.Opcode &&
									 kv.Value != DizDataType.Operand);

		var sorted = filtered.OrderBy(kv => kv.Key).ToList();
		if (sorted.Count == 0) return regions;

		int start = sorted[0].Key;
		int end = sorted[0].Key;
		var currentType = sorted[0].Value;

		for (int i = 1; i < sorted.Count; i++) {
			if (sorted[i].Key == end + 1 && sorted[i].Value == currentType) {
				end = sorted[i].Key;
			} else {
				regions.Add((start, end, currentType));
				start = sorted[i].Key;
				end = sorted[i].Key;
				currentType = sorted[i].Value;
			}
		}

		regions.Add((start, end, currentType));
		return regions;
	}

	/// <summary>
	/// Exports labels to a symbol loader compatible format.
	/// </summary>
	/// <param name="symbolLoader">The symbol loader to populate.</param>
	public void ExportToSymbolLoader(SymbolLoader symbolLoader) {
		foreach (var (address, label) in _labels) {
			if (!string.IsNullOrWhiteSpace(label.Name)) {
				symbolLoader.AddLabel((uint)address, label.Name);
			}
		}
	}

	/// <summary>
	/// Gets coverage statistics.
	/// </summary>
	/// <returns>Statistics about marked bytes.</returns>
	public (int Opcodes, int Operands, int DataBytes, int Unreached, double CoveragePercent) GetCoverageStats() {
		var opcodes = _dataTypes.Count(kv => kv.Value == DizDataType.Opcode);
		var operands = _dataTypes.Count(kv => kv.Value == DizDataType.Operand);
		var dataBytes = _dataTypes.Count(kv => kv.Value >= DizDataType.Data8 && kv.Value <= DizDataType.Text);
		var total = _romSize > 0 ? _romSize : _dataTypes.Count;
		var unreached = total - opcodes - operands - dataBytes;
		var coverage = total > 0 ? ((opcodes + operands + dataBytes) * 100.0) / total : 0;
		return (opcodes, operands, dataBytes, unreached, coverage);
	}
}
