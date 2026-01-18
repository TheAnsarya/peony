namespace Peony.Core;

using System.Text.Json;

/// <summary>
/// Interface for extracting structured data tables from ROMs
/// </summary>
public interface IDataTableExtractor {
	/// <summary>Platform name</summary>
	string Platform { get; }

	/// <summary>Extract all detected data tables</summary>
	DataExtractionResult ExtractAll(ReadOnlySpan<byte> rom, DataExtractionOptions options);

	/// <summary>Extract a specific data table by definition</summary>
	DataTable ExtractTable(ReadOnlySpan<byte> rom, DataTableDefinition definition);
}

/// <summary>
/// Options for data extraction
/// </summary>
public record DataExtractionOptions {
	/// <summary>Output directory for extracted files</summary>
	public string OutputDirectory { get; init; } = "output/data";

	/// <summary>Output format (json, csv)</summary>
	public string Format { get; init; } = "json";

	/// <summary>Generate schema files</summary>
	public bool GenerateSchema { get; init; } = true;

	/// <summary>Table definitions to extract (null = auto-detect)</summary>
	public List<DataTableDefinition>? TableDefinitions { get; init; }

	/// <summary>Include comments in output</summary>
	public bool IncludeComments { get; init; } = true;
}

/// <summary>
/// Result of data extraction
/// </summary>
public record DataExtractionResult {
	/// <summary>Extracted tables</summary>
	public required List<DataTable> Tables { get; init; }

	/// <summary>Output files generated</summary>
	public required List<string> OutputFiles { get; init; }

	/// <summary>Total records across all tables</summary>
	public int TotalRecords => Tables.Sum(t => t.Records.Count);
}

/// <summary>
/// Definition of a data table structure
/// </summary>
public record DataTableDefinition {
	/// <summary>Table name/identifier</summary>
	public required string Name { get; init; }

	/// <summary>ROM offset where table starts</summary>
	public int RomOffset { get; init; }

	/// <summary>Number of records (-1 to detect)</summary>
	public int RecordCount { get; init; } = -1;

	/// <summary>Size of each record in bytes</summary>
	public int RecordSize { get; init; }

	/// <summary>Field definitions</summary>
	public required List<FieldDefinition> Fields { get; init; }

	/// <summary>Description of the table</summary>
	public string? Description { get; init; }
}

/// <summary>
/// Definition of a field within a record
/// </summary>
public record FieldDefinition {
	/// <summary>Field name</summary>
	public required string Name { get; init; }

	/// <summary>Offset within record</summary>
	public int Offset { get; init; }

	/// <summary>Field data type</summary>
	public DataFieldType Type { get; init; }

	/// <summary>Size in bytes (for string/array types)</summary>
	public int Size { get; init; } = 1;

	/// <summary>Description/comment</summary>
	public string? Description { get; init; }

	/// <summary>Value mapping (e.g., enum values)</summary>
	public Dictionary<int, string>? ValueMap { get; init; }
}

/// <summary>
/// Data field types
/// </summary>
public enum DataFieldType {
	Byte,       // 8-bit unsigned
	SByte,      // 8-bit signed
	Word,       // 16-bit unsigned (little-endian)
	SWord,      // 16-bit signed
	WordBE,     // 16-bit big-endian
	Pointer,    // 16-bit address
	Flags,      // Bit flags
	ByteArray,  // Fixed-length byte array
	String      // Text string (requires table)
}

/// <summary>
/// Extracted data table
/// </summary>
public record DataTable {
	/// <summary>Table name</summary>
	public required string Name { get; init; }

	/// <summary>ROM offset</summary>
	public int RomOffset { get; init; }

	/// <summary>Total size in bytes</summary>
	public int SizeBytes { get; init; }

	/// <summary>Records in the table</summary>
	public required List<DataRecord> Records { get; init; }

	/// <summary>Field definitions used</summary>
	public required List<FieldDefinition> Fields { get; init; }

	/// <summary>Output file path (if saved)</summary>
	public string? OutputPath { get; init; }
}

/// <summary>
/// Single record from a data table
/// </summary>
public record DataRecord {
	/// <summary>Record index</summary>
	public int Index { get; init; }

	/// <summary>ROM offset of this record</summary>
	public int RomOffset { get; init; }

	/// <summary>Field values</summary>
	public required Dictionary<string, object> Values { get; init; }
}

/// <summary>
/// Utility methods for data extraction
/// </summary>
public static class DataExtraction {
	/// <summary>
	/// Read a field value from ROM data
	/// </summary>
	public static object ReadField(ReadOnlySpan<byte> data, int offset, FieldDefinition field) {
		if (offset + GetFieldSize(field) > data.Length) {
			return 0;
		}

		return field.Type switch {
			DataFieldType.Byte => data[offset],
			DataFieldType.SByte => (sbyte)data[offset],
			DataFieldType.Word => data[offset] | (data[offset + 1] << 8),
			DataFieldType.SWord => (short)(data[offset] | (data[offset + 1] << 8)),
			DataFieldType.WordBE => (data[offset] << 8) | data[offset + 1],
			DataFieldType.Pointer => data[offset] | (data[offset + 1] << 8),
			DataFieldType.Flags => data[offset],
			DataFieldType.ByteArray => ReadByteArray(data, offset, field.Size),
			DataFieldType.String => ReadByteArray(data, offset, field.Size),
			_ => data[offset]
		};
	}

	/// <summary>
	/// Get the size of a field in bytes
	/// </summary>
	public static int GetFieldSize(FieldDefinition field) {
		return field.Type switch {
			DataFieldType.Byte => 1,
			DataFieldType.SByte => 1,
			DataFieldType.Word => 2,
			DataFieldType.SWord => 2,
			DataFieldType.WordBE => 2,
			DataFieldType.Pointer => 2,
			DataFieldType.Flags => 1,
			DataFieldType.ByteArray => field.Size,
			DataFieldType.String => field.Size,
			_ => 1
		};
	}

	private static byte[] ReadByteArray(ReadOnlySpan<byte> data, int offset, int size) {
		int actualSize = Math.Min(size, data.Length - offset);
		return data.Slice(offset, actualSize).ToArray();
	}

	/// <summary>
	/// Extract a complete table from ROM
	/// </summary>
	public static DataTable ExtractTable(ReadOnlySpan<byte> rom, DataTableDefinition definition) {
		var records = new List<DataRecord>();
		int recordSize = definition.RecordSize > 0
			? definition.RecordSize
			: definition.Fields.Sum(f => GetFieldSize(f));

		int recordCount = definition.RecordCount > 0
			? definition.RecordCount
			: (rom.Length - definition.RomOffset) / recordSize;

		for (int i = 0; i < recordCount; i++) {
			int recordOffset = definition.RomOffset + (i * recordSize);
			if (recordOffset + recordSize > rom.Length) break;

			var values = new Dictionary<string, object>();
			foreach (var field in definition.Fields) {
				var value = ReadField(rom, recordOffset + field.Offset, field);
				values[field.Name] = value;
			}

			records.Add(new DataRecord {
				Index = i,
				RomOffset = recordOffset,
				Values = values
			});
		}

		return new DataTable {
			Name = definition.Name,
			RomOffset = definition.RomOffset,
			SizeBytes = records.Count * recordSize,
			Records = records,
			Fields = definition.Fields
		};
	}

	/// <summary>
	/// Save table to JSON file
	/// </summary>
	public static void SaveTableAsJson(DataTable table, string path, bool includeComments = true) {
		var output = new {
			name = table.Name,
			rom_offset = $"0x{table.RomOffset:x}",
			record_count = table.Records.Count,
			size_bytes = table.SizeBytes,
			fields = table.Fields.Select(f => new {
				name = f.Name,
				type = f.Type.ToString().ToLowerInvariant(),
				offset = f.Offset,
				size = GetFieldSize(f),
				description = includeComments ? f.Description : null
			}).Where(f => includeComments || f.description == null),
			records = table.Records.Select(r => new {
				index = r.Index,
				offset = $"0x{r.RomOffset:x}",
				values = r.Values
			})
		};

		var json = JsonSerializer.Serialize(output, new JsonSerializerOptions {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
		});
		File.WriteAllText(path, json);
	}

	/// <summary>
	/// Generate JSON schema for a table definition
	/// </summary>
	public static void SaveTableSchema(DataTableDefinition definition, string path) {
		var properties = new Dictionary<string, object>();
		foreach (var field in definition.Fields) {
			var prop = new Dictionary<string, object> {
				["type"] = GetJsonSchemaType(field.Type)
			};
			if (field.Description != null) {
				prop["description"] = field.Description;
			}
			if (field.ValueMap != null) {
				prop["enum"] = field.ValueMap.Keys.ToArray();
				prop["enumNames"] = field.ValueMap.Values.ToArray();
			}
			properties[field.Name] = prop;
		}

		var schema = new {
			type = "object",
			title = definition.Name,
			description = definition.Description,
			properties
		};

		var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
		File.WriteAllText(path, json);
	}

	private static string GetJsonSchemaType(DataFieldType type) {
		return type switch {
			DataFieldType.Byte or DataFieldType.Word or DataFieldType.WordBE or
			DataFieldType.Pointer or DataFieldType.Flags => "integer",
			DataFieldType.SByte or DataFieldType.SWord => "integer",
			DataFieldType.ByteArray => "array",
			DataFieldType.String => "string",
			_ => "integer"
		};
	}
}
