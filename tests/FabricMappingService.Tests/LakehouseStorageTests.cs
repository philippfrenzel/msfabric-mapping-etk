using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class LakehouseStorageTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LakehouseStorage _storage;

    public LakehouseStorageTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "FabricMappingServiceTests", Guid.NewGuid().ToString());
        _storage = new LakehouseStorage();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public async Task SaveReferenceTableConfigurationAsync_ValidTable_SavesSuccessfully()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_table",
            KeyColumnName = "key",
            Columns = [
                new ReferenceTableColumn { Name = "Category", DataType = "string", Order = 1 }
            ],
            IsVisible = true,
            NotifyOnNewMapping = false
        };

        // Act
        await _storage.SaveReferenceTableConfigurationAsync(table, _testBasePath);

        // Assert
        var loaded = await _storage.LoadReferenceTableConfigurationAsync("test_table", _testBasePath);
        Assert.NotNull(loaded);
        Assert.Equal("test_table", loaded.Name);
        Assert.Equal("key", loaded.KeyColumnName);
        Assert.Single(loaded.Columns);
        Assert.Equal("Category", loaded.Columns[0].Name);
    }

    [Fact]
    public async Task LoadReferenceTableConfigurationAsync_NonExistingTable_ReturnsNull()
    {
        // Act
        var result = await _storage.LoadReferenceTableConfigurationAsync("nonexistent", _testBasePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveReferenceTableDataAsync_ValidData_SavesSuccessfully()
    {
        // Arrange
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["KEY001"] = new Dictionary<string, object?>
            {
                ["key"] = "KEY001",
                ["Category"] = "Electronics",
                ["Price"] = 100
            },
            ["KEY002"] = new Dictionary<string, object?>
            {
                ["key"] = "KEY002",
                ["Category"] = "Books",
                ["Price"] = 20
            }
        };

        // Act
        await _storage.SaveReferenceTableDataAsync("test_data", data, _testBasePath);

        // Assert
        var loaded = await _storage.LoadReferenceTableDataAsync("test_data", _testBasePath);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Count);
        Assert.True(loaded.ContainsKey("KEY001"));
        Assert.Equal("Electronics", loaded["KEY001"]["Category"]?.ToString());
    }

    [Fact]
    public async Task LoadReferenceTableDataAsync_NonExistingTable_ReturnsEmptyDictionary()
    {
        // Act
        var result = await _storage.LoadReferenceTableDataAsync("nonexistent", _testBasePath);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteReferenceTableAsync_ExistingTable_DeletesSuccessfully()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_delete",
            KeyColumnName = "key",
            Columns = []
        };
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["KEY001"] = new Dictionary<string, object?> { ["key"] = "KEY001" }
        };

        await _storage.SaveReferenceTableConfigurationAsync(table, _testBasePath);
        await _storage.SaveReferenceTableDataAsync("test_delete", data, _testBasePath);

        // Act
        var result = await _storage.DeleteReferenceTableAsync("test_delete", _testBasePath);

        // Assert
        Assert.True(result);
        var config = await _storage.LoadReferenceTableConfigurationAsync("test_delete", _testBasePath);
        Assert.Null(config);
    }

    [Fact]
    public async Task DeleteReferenceTableAsync_NonExistingTable_ReturnsFalse()
    {
        // Act
        var result = await _storage.DeleteReferenceTableAsync("nonexistent", _testBasePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ListReferenceTableNamesAsync_MultipleTables_ReturnsAllNames()
    {
        // Arrange
        var tables = new[]
        {
            new ReferenceTable { Name = "table1", KeyColumnName = "key", Columns = [] },
            new ReferenceTable { Name = "table2", KeyColumnName = "key", Columns = [] },
            new ReferenceTable { Name = "table3", KeyColumnName = "key", Columns = [] }
        };

        foreach (var table in tables)
        {
            await _storage.SaveReferenceTableConfigurationAsync(table, _testBasePath);
        }

        // Act
        var result = await _storage.ListReferenceTableNamesAsync(_testBasePath);

        // Assert
        var names = result.ToList();
        Assert.Equal(3, names.Count);
        Assert.Contains("table1", names);
        Assert.Contains("table2", names);
        Assert.Contains("table3", names);
    }

    [Fact]
    public async Task ListReferenceTableNamesAsync_EmptyDirectory_ReturnsEmpty()
    {
        // Act
        var result = await _storage.ListReferenceTableNamesAsync(_testBasePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task TableExistsAsync_ExistingTable_ReturnsTrue()
    {
        // Arrange
        var table = new ReferenceTable
        {
            Name = "test_exists",
            KeyColumnName = "key",
            Columns = []
        };
        await _storage.SaveReferenceTableConfigurationAsync(table, _testBasePath);

        // Act
        var result = await _storage.TableExistsAsync("test_exists", _testBasePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task TableExistsAsync_NonExistingTable_ReturnsFalse()
    {
        // Act
        var result = await _storage.TableExistsAsync("nonexistent", _testBasePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SaveReferenceTableConfigurationAsync_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _storage.SaveReferenceTableConfigurationAsync(null!, _testBasePath));
    }

    [Fact]
    public async Task SaveReferenceTableDataAsync_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _storage.SaveReferenceTableDataAsync("test", null!, _testBasePath));
    }
}
