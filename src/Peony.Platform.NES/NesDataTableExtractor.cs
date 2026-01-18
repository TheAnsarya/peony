namespace Peony.Platform.NES;

using Peony.Core;

/// <summary>
/// NES data table extractor with support for common NES RPG data formats
/// </summary>
public class NesDataTableExtractor : IDataTableExtractor {
	public string Platform => "NES";

	/// <summary>
	/// Extract all data tables using provided definitions or auto-detection
	/// </summary>
	public DataExtractionResult ExtractAll(ReadOnlySpan<byte> rom, DataExtractionOptions options) {
		var tables = new List<DataTable>();
		var outputFiles = new List<string>();

		Directory.CreateDirectory(options.OutputDirectory);

		var definitions = options.TableDefinitions ?? [];

		foreach (var definition in definitions) {
			var table = ExtractTable(rom, definition);

			// Save to file
			var fileName = $"{SanitizeFileName(table.Name)}.{options.Format}";
			var outputPath = Path.Combine(options.OutputDirectory, fileName);

			if (options.Format.Equals("json", StringComparison.OrdinalIgnoreCase)) {
				DataExtraction.SaveTableAsJson(table, outputPath, options.IncludeComments);
				table = table with { OutputPath = outputPath };
			}

			tables.Add(table);
			if (table.OutputPath != null) {
				outputFiles.Add(table.OutputPath);
			}

			// Generate schema
			if (options.GenerateSchema) {
				var schemaPath = Path.Combine(options.OutputDirectory, $"{SanitizeFileName(table.Name)}.schema.json");
				DataExtraction.SaveTableSchema(definition, schemaPath);
				outputFiles.Add(schemaPath);
			}
		}

		return new DataExtractionResult {
			Tables = tables,
			OutputFiles = outputFiles
		};
	}

	/// <summary>
	/// Extract a specific table by definition
	/// </summary>
	public DataTable ExtractTable(ReadOnlySpan<byte> rom, DataTableDefinition definition) {
		return DataExtraction.ExtractTable(rom, definition);
	}

	/// <summary>
	/// Create a standard NES monster table definition
	/// Common in Dragon Quest/Warrior style games
	/// </summary>
	public static DataTableDefinition CreateMonsterTableDefinition(int romOffset, int recordCount, int recordSize = 16) {
		return new DataTableDefinition {
			Name = "Monsters",
			RomOffset = romOffset,
			RecordCount = recordCount,
			RecordSize = recordSize,
			Description = "Monster/Enemy statistics table",
			Fields = [
				new FieldDefinition { Name = "hp", Offset = 0, Type = DataFieldType.Word, Description = "Hit Points" },
				new FieldDefinition { Name = "attack", Offset = 2, Type = DataFieldType.Byte, Description = "Attack Power" },
				new FieldDefinition { Name = "defense", Offset = 3, Type = DataFieldType.Byte, Description = "Defense" },
				new FieldDefinition { Name = "agility", Offset = 4, Type = DataFieldType.Byte, Description = "Agility/Speed" },
				new FieldDefinition { Name = "exp", Offset = 5, Type = DataFieldType.Word, Description = "Experience Points" },
				new FieldDefinition { Name = "gold", Offset = 7, Type = DataFieldType.Word, Description = "Gold Reward" },
				new FieldDefinition { Name = "drop_item", Offset = 9, Type = DataFieldType.Byte, Description = "Drop Item ID" },
				new FieldDefinition { Name = "drop_rate", Offset = 10, Type = DataFieldType.Byte, Description = "Drop Rate (1/N)" },
				new FieldDefinition { Name = "spell_flags", Offset = 11, Type = DataFieldType.Byte, Description = "Spell/Ability Flags" },
				new FieldDefinition { Name = "resist_flags", Offset = 12, Type = DataFieldType.Byte, Description = "Resistance Flags" }
			]
		};
	}

	/// <summary>
	/// Create a standard NES item table definition
	/// </summary>
	public static DataTableDefinition CreateItemTableDefinition(int romOffset, int recordCount, int recordSize = 8) {
		return new DataTableDefinition {
			Name = "Items",
			RomOffset = romOffset,
			RecordCount = recordCount,
			RecordSize = recordSize,
			Description = "Item definitions table",
			Fields = [
				new FieldDefinition { Name = "type", Offset = 0, Type = DataFieldType.Byte, Description = "Item Type" },
				new FieldDefinition { Name = "effect", Offset = 1, Type = DataFieldType.Byte, Description = "Effect/Power" },
				new FieldDefinition { Name = "price", Offset = 2, Type = DataFieldType.Word, Description = "Buy Price" },
				new FieldDefinition { Name = "flags", Offset = 4, Type = DataFieldType.Byte, Description = "Item Flags" },
				new FieldDefinition { Name = "equip_class", Offset = 5, Type = DataFieldType.Byte, Description = "Equip Class Mask" }
			]
		};
	}

	/// <summary>
	/// Create a standard NES spell table definition
	/// </summary>
	public static DataTableDefinition CreateSpellTableDefinition(int romOffset, int recordCount, int recordSize = 6) {
		return new DataTableDefinition {
			Name = "Spells",
			RomOffset = romOffset,
			RecordCount = recordCount,
			RecordSize = recordSize,
			Description = "Spell/Magic definitions table",
			Fields = [
				new FieldDefinition { Name = "mp_cost", Offset = 0, Type = DataFieldType.Byte, Description = "MP Cost" },
				new FieldDefinition { Name = "effect_type", Offset = 1, Type = DataFieldType.Byte, Description = "Effect Type" },
				new FieldDefinition { Name = "power", Offset = 2, Type = DataFieldType.Byte, Description = "Base Power" },
				new FieldDefinition { Name = "target_type", Offset = 3, Type = DataFieldType.Byte, Description = "Target Type" },
				new FieldDefinition { Name = "flags", Offset = 4, Type = DataFieldType.Byte, Description = "Spell Flags" }
			]
		};
	}

	/// <summary>
	/// Create an experience table definition
	/// </summary>
	public static DataTableDefinition CreateExpTableDefinition(int romOffset, int levelCount) {
		return new DataTableDefinition {
			Name = "ExperienceTable",
			RomOffset = romOffset,
			RecordCount = levelCount,
			RecordSize = 2,
			Description = "Experience points required per level",
			Fields = [
				new FieldDefinition { Name = "exp_required", Offset = 0, Type = DataFieldType.Word, Description = "EXP for this level" }
			]
		};
	}

	/// <summary>
	/// Create a shop inventory table definition
	/// </summary>
	public static DataTableDefinition CreateShopTableDefinition(int romOffset, int shopCount, int itemsPerShop = 8) {
		var fields = new List<FieldDefinition>();
		for (int i = 0; i < itemsPerShop; i++) {
			fields.Add(new FieldDefinition {
				Name = $"item_{i}",
				Offset = i,
				Type = DataFieldType.Byte,
				Description = $"Item slot {i} (0 = empty)"
			});
		}

		return new DataTableDefinition {
			Name = "Shops",
			RomOffset = romOffset,
			RecordCount = shopCount,
			RecordSize = itemsPerShop,
			Description = "Shop inventory definitions",
			Fields = fields
		};
	}

	private static string SanitizeFileName(string name) {
		var invalid = Path.GetInvalidFileNameChars();
		return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries))
			.ToLowerInvariant()
			.Replace(" ", "_");
	}
}
