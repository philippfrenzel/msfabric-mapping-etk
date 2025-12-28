using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class LakehouseReferenceMappingStorageTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LakehouseStorage _lakehouseStorage;
    private readonly LakehouseReferenceMappingStorage _storage;

    public LakehouseReferenceMappingStorageTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "FabricMappingServiceTests", Guid.NewGuid().ToString());
        _lakehouseStorage = new LakehouseStorage();
        _storage = new LakehouseReferenceMappingStorage(_lakehouseStorage, _testBasePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public void SaveReferenceTable_ValidTable_SavesSuccessfully()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_table",
            KeyColumnName = "key",
            Columns = [
                new ReferenceTableColumn { Name = "Category", DataType = "string", Order = 1 }
            ],
            Rows = [
                new ReferenceTableRow
                {
                    Key = "KEY001",
                    Attributes = new Dictionary<string, object?> { ["Category"] = "Electronics" }
                }
            ]
        };

        // Act
        _storage.SaveReferenceTable(table);

        // Assert
        var loaded = _storage.GetReferenceTable("test_table");
        Assert.NotNull(loaded);
        Assert.Equal("test_table", loaded.Name);
        Assert.Single(loaded.Rows);
        Assert.Equal("KEY001", loaded.Rows[0].Key);
    }

    [Fact]
    public void GetReferenceTable_NonExistingTable_ReturnsNull()
    {
        // Act
        var result = _storage.GetReferenceTable("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteReferenceTable_ExistingTable_ReturnsTrue()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_delete",
            KeyColumnName = "key",
            Columns = [],
            Rows = []
        };
        _storage.SaveReferenceTable(table);

        // Act
        var result = _storage.DeleteReferenceTable("test_delete");

        // Assert
        Assert.True(result);
        var loaded = _storage.GetReferenceTable("test_delete");
        Assert.Null(loaded);
    }

    [Fact]
    public void DeleteReferenceTable_NonExistingTable_ReturnsFalse()
    {
        // Act
        var result = _storage.DeleteReferenceTable("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAllTableNames_MultipleTables_ReturnsAllNames()
    {
        // Arrange
        var tables = new[]
        {
            new ReferenceTable { Name = "table1", KeyColumnName = "key", Columns = [], Rows = [] },
            new ReferenceTable { Name = "table2", KeyColumnName = "key", Columns = [], Rows = [] },
            new ReferenceTable { Name = "table3", KeyColumnName = "key", Columns = [], Rows = [] }
        };

        foreach (var table in tables)
        {
            _storage.SaveReferenceTable(table);
        }

        // Act
        var result = _storage.GetAllTableNames().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("table1", result);
        Assert.Contains("table2", result);
        Assert.Contains("table3", result);
    }

    [Fact]
    public void GetAllTableNames_NoTables_ReturnsEmpty()
    {
        // Act
        var result = _storage.GetAllTableNames();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void TableExists_ExistingTable_ReturnsTrue()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_exists",
            KeyColumnName = "key",
            Columns = [],
            Rows = []
        };
        _storage.SaveReferenceTable(table);

        // Act
        var result = _storage.TableExists("test_exists");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TableExists_NonExistingTable_ReturnsFalse()
    {
        // Act
        var result = _storage.TableExists("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SaveReferenceTable_UpdatesTimestamp()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_timestamp",
            KeyColumnName = "key",
            Columns = [],
            Rows = [],
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var oldTimestamp = table.UpdatedAt;

        // Act
        _storage.SaveReferenceTable(table);

        // Assert
        var loaded = _storage.GetReferenceTable("test_timestamp");
        Assert.NotNull(loaded);
        Assert.True(loaded.UpdatedAt > oldTimestamp);
    }

    [Fact]
    public void SaveReferenceTable_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _storage.SaveReferenceTable(null!));
    }

    [Fact]
    public void GetReferenceTable_EmptyTableName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _storage.GetReferenceTable(string.Empty));
    }

    [Fact]
    public void DeleteReferenceTable_EmptyTableName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _storage.DeleteReferenceTable(string.Empty));
    }

    [Fact]
    public void TableExists_EmptyTableName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _storage.TableExists(string.Empty));
    }
}
