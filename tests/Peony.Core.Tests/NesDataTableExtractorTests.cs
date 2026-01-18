namespace Peony.Core.Tests;

using Peony.Platform.NES;
using Xunit;

/// <summary>
/// Tests for NES data table extraction
/// </summary>
public class NesDataTableExtractorTests {
	private readonly NesDataTableExtractor _extractor = new();

	[Fact]
	public void Platform_ReturnsNES() {
		Assert.Equal("NES", _extractor.Platform);
	}

	[Fact]
	public void ReadField_ReadsByte() {
		var data = new byte[] { 0x42, 0x00 };
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.Byte };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.Equal((byte)0x42, result);
	}

	[Fact]
	public void ReadField_ReadsSignedByte() {
		var data = new byte[] { 0xff }; // -1 as signed
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.SByte };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.Equal((sbyte)-1, result);
	}

	[Fact]
	public void ReadField_ReadsWordLittleEndian() {
		var data = new byte[] { 0x34, 0x12 }; // 0x1234 in little-endian
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.Word };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.Equal(0x1234, result);
	}

	[Fact]
	public void ReadField_ReadsWordBigEndian() {
		var data = new byte[] { 0x12, 0x34 }; // 0x1234 in big-endian
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.WordBE };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.Equal(0x1234, result);
	}

	[Fact]
	public void ReadField_ReadsPointer() {
		var data = new byte[] { 0x00, 0x80 }; // $8000
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.Pointer };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.Equal(0x8000, result);
	}

	[Fact]
	public void ReadField_ReadsByteArray() {
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
		var field = new FieldDefinition { Name = "test", Offset = 0, Type = DataFieldType.ByteArray, Size = 3 };

		var result = DataExtraction.ReadField(data, 0, field);

		Assert.IsType<byte[]>(result);
		Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, (byte[])result);
	}

	[Fact]
	public void GetFieldSize_ReturnsCorrectSizes() {
		Assert.Equal(1, DataExtraction.GetFieldSize(new FieldDefinition { Name = "t", Type = DataFieldType.Byte }));
		Assert.Equal(2, DataExtraction.GetFieldSize(new FieldDefinition { Name = "t", Type = DataFieldType.Word }));
		Assert.Equal(2, DataExtraction.GetFieldSize(new FieldDefinition { Name = "t", Type = DataFieldType.Pointer }));
		Assert.Equal(5, DataExtraction.GetFieldSize(new FieldDefinition { Name = "t", Type = DataFieldType.ByteArray, Size = 5 }));
	}

	[Fact]
	public void ExtractTable_ExtractsRecords() {
		// Create test data: 2 records of 4 bytes each
		var rom = new byte[] {
			0x10, 0x00, 0x05, 0x03, // Record 0: HP=16, ATK=5, DEF=3
			0x20, 0x00, 0x08, 0x04  // Record 1: HP=32, ATK=8, DEF=4
		};

		var definition = new DataTableDefinition {
			Name = "TestMonsters",
			RomOffset = 0,
			RecordCount = 2,
			RecordSize = 4,
			Fields = [
				new FieldDefinition { Name = "hp", Offset = 0, Type = DataFieldType.Word },
				new FieldDefinition { Name = "attack", Offset = 2, Type = DataFieldType.Byte },
				new FieldDefinition { Name = "defense", Offset = 3, Type = DataFieldType.Byte }
			]
		};

		var table = _extractor.ExtractTable(rom, definition);

		Assert.Equal("TestMonsters", table.Name);
		Assert.Equal(2, table.Records.Count);
		Assert.Equal(16, table.Records[0].Values["hp"]);
		Assert.Equal((byte)5, table.Records[0].Values["attack"]);
		Assert.Equal(32, table.Records[1].Values["hp"]);
		Assert.Equal((byte)8, table.Records[1].Values["attack"]);
	}

	[Fact]
	public void ExtractTable_HandlesAutoRecordCount() {
		var rom = new byte[16]; // Space for 4 records of 4 bytes
		var definition = new DataTableDefinition {
			Name = "Test",
			RomOffset = 0,
			RecordCount = -1, // Auto-detect
			RecordSize = 4,
			Fields = [
				new FieldDefinition { Name = "value", Offset = 0, Type = DataFieldType.Byte }
			]
		};

		var table = DataExtraction.ExtractTable(rom, definition);

		Assert.Equal(4, table.Records.Count);
	}

	[Fact]
	public void CreateMonsterTableDefinition_HasCorrectFields() {
		var definition = NesDataTableExtractor.CreateMonsterTableDefinition(0x8000, 50);

		Assert.Equal("Monsters", definition.Name);
		Assert.Equal(0x8000, definition.RomOffset);
		Assert.Equal(50, definition.RecordCount);
		Assert.Contains(definition.Fields, f => f.Name == "hp");
		Assert.Contains(definition.Fields, f => f.Name == "attack");
		Assert.Contains(definition.Fields, f => f.Name == "defense");
		Assert.Contains(definition.Fields, f => f.Name == "exp");
		Assert.Contains(definition.Fields, f => f.Name == "gold");
	}

	[Fact]
	public void CreateItemTableDefinition_HasCorrectFields() {
		var definition = NesDataTableExtractor.CreateItemTableDefinition(0x9000, 100);

		Assert.Equal("Items", definition.Name);
		Assert.Equal(100, definition.RecordCount);
		Assert.Contains(definition.Fields, f => f.Name == "type");
		Assert.Contains(definition.Fields, f => f.Name == "price");
	}

	[Fact]
	public void CreateSpellTableDefinition_HasCorrectFields() {
		var definition = NesDataTableExtractor.CreateSpellTableDefinition(0xa000, 30);

		Assert.Equal("Spells", definition.Name);
		Assert.Contains(definition.Fields, f => f.Name == "mp_cost");
		Assert.Contains(definition.Fields, f => f.Name == "power");
	}

	[Fact]
	public void CreateExpTableDefinition_HasCorrectStructure() {
		var definition = NesDataTableExtractor.CreateExpTableDefinition(0xb000, 30);

		Assert.Equal("ExperienceTable", definition.Name);
		Assert.Equal(30, definition.RecordCount);
		Assert.Equal(2, definition.RecordSize);
		Assert.Single(definition.Fields);
		Assert.Equal("exp_required", definition.Fields[0].Name);
	}

	[Fact]
	public void CreateShopTableDefinition_HasItemSlots() {
		var definition = NesDataTableExtractor.CreateShopTableDefinition(0xc000, 10, 8);

		Assert.Equal("Shops", definition.Name);
		Assert.Equal(10, definition.RecordCount);
		Assert.Equal(8, definition.RecordSize);
		Assert.Equal(8, definition.Fields.Count);
		Assert.Equal("item_0", definition.Fields[0].Name);
		Assert.Equal("item_7", definition.Fields[7].Name);
	}

	[Fact]
	public void ExtractAll_SavesJsonFiles() {
		var rom = new byte[32];
		// Fill with test data
		rom[0] = 0x10; rom[1] = 0x00; rom[2] = 0x05; rom[3] = 0x03;

		var outputDir = Path.Combine(Path.GetTempPath(), $"peony_test_data_{Guid.NewGuid():N}");

		try {
			var result = _extractor.ExtractAll(rom, new DataExtractionOptions {
				OutputDirectory = outputDir,
				Format = "json",
				GenerateSchema = true,
				TableDefinitions = [
					new DataTableDefinition {
						Name = "Test",
						RomOffset = 0,
						RecordCount = 1,
						RecordSize = 4,
						Fields = [
							new FieldDefinition { Name = "value", Offset = 0, Type = DataFieldType.Word }
						]
					}
				]
			});

			Assert.Single(result.Tables);
			Assert.True(result.OutputFiles.Count >= 1);
			Assert.True(File.Exists(Path.Combine(outputDir, "test.json")));
		} finally {
			if (Directory.Exists(outputDir)) {
				Directory.Delete(outputDir, true);
			}
		}
	}
}
