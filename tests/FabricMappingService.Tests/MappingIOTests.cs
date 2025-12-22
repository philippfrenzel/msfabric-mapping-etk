using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class MappingIOTests
{
    private class TestDataModel
    {
        public string Produkt { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Fact]
    public void SyncMapping_WithNewData_ShouldCreateTableAndAddKeys()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data = new List<TestDataModel>
        {
            new() { Produkt = "VTP001", Name = "Product 1", Price = 100m },
            new() { Produkt = "VTP002", Name = "Product 2", Price = 200m },
            new() { Produkt = "VTP003", Name = "Product 3", Price = 300m }
        };

        // Act
        var newKeysAdded = mappingIO.SyncMapping(data, "Produkt", "produkttyp");

        // Assert
        Assert.Equal(3, newKeysAdded);
        var table = mappingIO.GetReferenceTable("produkttyp");
        Assert.NotNull(table);
        Assert.Equal("produkttyp", table.Name);
        Assert.Equal(3, table.Rows.Count);
        Assert.Contains(table.Rows, r => r.Key == "VTP001");
        Assert.Contains(table.Rows, r => r.Key == "VTP002");
        Assert.Contains(table.Rows, r => r.Key == "VTP003");
    }

    [Fact]
    public void SyncMapping_WithDuplicateKeys_ShouldOnlyAddUnique()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data1 = new List<TestDataModel>
        {
            new() { Produkt = "VTP001", Name = "Product 1", Price = 100m },
            new() { Produkt = "VTP002", Name = "Product 2", Price = 200m }
        };
        var data2 = new List<TestDataModel>
        {
            new() { Produkt = "VTP002", Name = "Product 2 Updated", Price = 250m },
            new() { Produkt = "VTP003", Name = "Product 3", Price = 300m }
        };

        // Act
        var firstSync = mappingIO.SyncMapping(data1, "Produkt", "produkttyp");
        var secondSync = mappingIO.SyncMapping(data2, "Produkt", "produkttyp");

        // Assert
        Assert.Equal(2, firstSync);
        Assert.Equal(1, secondSync); // Only VTP003 is new
        var table = mappingIO.GetReferenceTable("produkttyp");
        Assert.NotNull(table);
        Assert.Equal(3, table.Rows.Count);
    }

    [Fact]
    public void SyncMapping_WithNullData_ShouldThrowArgumentNullException()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        List<TestDataModel>? data = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mappingIO.SyncMapping(data!, "Produkt", "produkttyp"));
    }

    [Fact]
    public void SyncMapping_WithInvalidKeyAttribute_ShouldThrowArgumentException()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data = new List<TestDataModel>
        {
            new() { Produkt = "VTP001", Name = "Product 1", Price = 100m }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            mappingIO.SyncMapping(data, "NonExistentProperty", "produkttyp"));
        Assert.Contains("Property 'NonExistentProperty' not found", exception.Message);
    }

    [Fact]
    public void ReadMapping_WithExistingTable_ShouldReturnData()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data = new List<TestDataModel>
        {
            new() { Produkt = "VTP001", Name = "Product 1", Price = 100m },
            new() { Produkt = "VTP002", Name = "Product 2", Price = 200m }
        };
        mappingIO.SyncMapping(data, "Produkt", "produkttyp");

        // Act
        var result = mappingIO.ReadMapping("produkttyp");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("VTP001"));
        Assert.True(result.ContainsKey("VTP002"));
        Assert.Equal("VTP001", result["VTP001"]["key"]);
        Assert.Equal("VTP002", result["VTP002"]["key"]);
    }

    [Fact]
    public void ReadMapping_WithNonExistentTable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            mappingIO.ReadMapping("nonexistent"));
        Assert.Contains("Reference table 'nonexistent' not found", exception.Message);
    }

    [Fact]
    public void CreateReferenceTable_WithValidData_ShouldCreateTable()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 },
            new() { Name = "Group", DataType = "string", Order = 2 }
        };

        // Act
        mappingIO.CreateReferenceTable("vertragsprodukte", columns, isVisible: true, notifyOnNewMapping: false);

        // Assert
        var table = mappingIO.GetReferenceTable("vertragsprodukte");
        Assert.NotNull(table);
        Assert.Equal("vertragsprodukte", table.Name);
        Assert.Equal(2, table.Columns.Count);
        Assert.True(table.IsVisible);
        Assert.False(table.NotifyOnNewMapping);
    }

    [Fact]
    public void CreateReferenceTable_WithExistingTable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>();
        mappingIO.CreateReferenceTable("test", columns);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            mappingIO.CreateReferenceTable("test", columns));
    }

    [Fact]
    public void GetAllTableNames_WithMultipleTables_ShouldReturnAllNames()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>();
        mappingIO.CreateReferenceTable("table1", columns);
        mappingIO.CreateReferenceTable("table2", columns);
        mappingIO.CreateReferenceTable("table3", columns);

        // Act
        var tableNames = mappingIO.GetAllTableNames().ToList();

        // Assert
        Assert.Equal(3, tableNames.Count);
        Assert.Contains("table1", tableNames);
        Assert.Contains("table2", tableNames);
        Assert.Contains("table3", tableNames);
    }

    [Fact]
    public void DeleteReferenceTable_WithExistingTable_ShouldReturnTrue()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>();
        mappingIO.CreateReferenceTable("test", columns);

        // Act
        var result = mappingIO.DeleteReferenceTable("test");

        // Assert
        Assert.True(result);
        Assert.Null(mappingIO.GetReferenceTable("test"));
    }

    [Fact]
    public void DeleteReferenceTable_WithNonExistentTable_ShouldReturnFalse()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);

        // Act
        var result = mappingIO.DeleteReferenceTable("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AddOrUpdateRow_WithNewKey_ShouldAddRow()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 }
        };
        mappingIO.CreateReferenceTable("test", columns);
        var attributes = new Dictionary<string, object?> { ["Category"] = "Electronics" };

        // Act
        mappingIO.AddOrUpdateRow("test", "KEY001", attributes);

        // Assert
        var table = mappingIO.GetReferenceTable("test");
        Assert.NotNull(table);
        Assert.Single(table.Rows);
        Assert.Equal("KEY001", table.Rows[0].Key);
        Assert.Equal("Electronics", table.Rows[0].Attributes["Category"]);
    }

    [Fact]
    public void AddOrUpdateRow_WithExistingKey_ShouldUpdateRow()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 }
        };
        mappingIO.CreateReferenceTable("test", columns);
        var attributes1 = new Dictionary<string, object?> { ["Category"] = "Electronics" };
        var attributes2 = new Dictionary<string, object?> { ["Category"] = "Computers" };
        mappingIO.AddOrUpdateRow("test", "KEY001", attributes1);

        // Act
        mappingIO.AddOrUpdateRow("test", "KEY001", attributes2);

        // Assert
        var table = mappingIO.GetReferenceTable("test");
        Assert.NotNull(table);
        Assert.Single(table.Rows);
        Assert.Equal("KEY001", table.Rows[0].Key);
        Assert.Equal("Computers", table.Rows[0].Attributes["Category"]);
        Assert.False(table.Rows[0].IsNew); // Should be marked as not new after update
    }

    [Fact]
    public void AddOrUpdateRow_WithNonExistentTable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var attributes = new Dictionary<string, object?> { ["Category"] = "Electronics" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            mappingIO.AddOrUpdateRow("nonexistent", "KEY001", attributes));
    }

    [Fact]
    public void SyncMapping_WithEmptyData_ShouldReturnZero()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data = new List<TestDataModel>();

        // Act
        var newKeysAdded = mappingIO.SyncMapping(data, "Produkt", "produkttyp");

        // Assert
        Assert.Equal(0, newKeysAdded);
    }

    [Fact]
    public void SyncMapping_CaseInsensitiveKeys_ShouldTreatAsSame()
    {
        // Arrange
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var data1 = new List<TestDataModel>
        {
            new() { Produkt = "VTP001", Name = "Product 1", Price = 100m }
        };
        var data2 = new List<TestDataModel>
        {
            new() { Produkt = "vtp001", Name = "Product 1 Lower", Price = 150m }
        };

        // Act
        var firstSync = mappingIO.SyncMapping(data1, "Produkt", "produkttyp");
        var secondSync = mappingIO.SyncMapping(data2, "Produkt", "produkttyp");

        // Assert
        Assert.Equal(1, firstSync);
        Assert.Equal(0, secondSync); // Should not add duplicate with different case
        var table = mappingIO.GetReferenceTable("produkttyp");
        Assert.NotNull(table);
        Assert.Single(table.Rows);
    }
}
