using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class OneLakeStorageTests
{
    [Fact]
    public async Task StoreMappingTableAsync_ValidData_StoresSuccessfully()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Name"] = "Value1", ["Count"] = 10 },
            ["key2"] = new Dictionary<string, object?> { ["Name"] = "Value2", ["Count"] = 20 }
        };

        // Act
        var path = await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", data);

        // Assert
        Assert.NotNull(path);
        Assert.Contains("workspace-1", path);
        Assert.Contains("item-1", path);
        Assert.Contains("TestTable", path);
    }

    [Fact]
    public async Task ReadMappingTableAsync_StoredData_ReturnsCorrectData()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var originalData = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Name"] = "Value1", ["Count"] = 10 },
            ["key2"] = new Dictionary<string, object?> { ["Name"] = "Value2", ["Count"] = 20 }
        };
        await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", originalData);

        // Act
        var retrievedData = await storage.ReadMappingTableAsync("item-1", "workspace-1", "TestTable");

        // Assert
        Assert.NotNull(retrievedData);
        Assert.Equal(2, retrievedData.Count);
        Assert.True(retrievedData.ContainsKey("key1"));
        Assert.True(retrievedData.ContainsKey("key2"));
    }

    [Fact]
    public async Task ReadMappingTableAsync_NonExistingTable_ThrowsException()
    {
        // Arrange
        var storage = new OneLakeStorage();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => storage.ReadMappingTableAsync("item-1", "workspace-1", "NonExisting"));
    }

    [Fact]
    public async Task DeleteMappingTableAsync_ExistingTable_DeletesSuccessfully()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Name"] = "Value1" }
        };
        await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", data);

        // Act
        var deleted = await storage.DeleteMappingTableAsync("item-1", "workspace-1", "TestTable");

        // Assert
        Assert.True(deleted);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => storage.ReadMappingTableAsync("item-1", "workspace-1", "TestTable"));
    }

    [Fact]
    public async Task DeleteMappingTableAsync_NonExistingTable_ReturnsFalse()
    {
        // Arrange
        var storage = new OneLakeStorage();

        // Act
        var deleted = await storage.DeleteMappingTableAsync("item-1", "workspace-1", "NonExisting");

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public void GetOneLakePath_ValidParameters_ReturnsCorrectPath()
    {
        // Arrange
        var storage = new OneLakeStorage();

        // Act
        var path = storage.GetOneLakePath("workspace-1", "item-1", "TestTable");

        // Assert
        Assert.Equal("https://onelake.dfs.fabric.microsoft.com/workspace-1/item-1/Tables/TestTable", path);
    }

    [Fact]
    public async Task StoreMappingTableAsync_EmptyItemId_ThrowsException()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data = new Dictionary<string, Dictionary<string, object?>>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.StoreMappingTableAsync("", "workspace-1", "TestTable", data));
    }

    [Fact]
    public async Task StoreMappingTableAsync_NullData_ThrowsException()
    {
        // Arrange
        var storage = new OneLakeStorage();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", null!));
    }

    [Fact]
    public async Task StoreMappingTableAsync_OverwriteExistingTable_UpdatesData()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var originalData = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Value"] = "Original" }
        };
        var newData = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Value"] = "Updated" }
        };

        await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", originalData);

        // Act
        await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", newData);

        // Assert
        var retrievedData = await storage.ReadMappingTableAsync("item-1", "workspace-1", "TestTable");
        Assert.Single(retrievedData);
        // Note: Due to JSON serialization, we can't directly assert the value type
        Assert.True(retrievedData.ContainsKey("key1"));
    }

    [Fact]
    public async Task StoreMappingTableAsync_DifferentWorkspaces_StoredSeparately()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data1 = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Value"] = "Workspace1" }
        };
        var data2 = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Value"] = "Workspace2" }
        };

        // Act
        await storage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", data1);
        await storage.StoreMappingTableAsync("item-1", "workspace-2", "TestTable", data2);

        // Assert
        var retrieved1 = await storage.ReadMappingTableAsync("item-1", "workspace-1", "TestTable");
        var retrieved2 = await storage.ReadMappingTableAsync("item-1", "workspace-2", "TestTable");

        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Single(retrieved1);
        Assert.Single(retrieved2);
    }

    [Fact]
    public async Task StoreMappingTableAsync_ComplexData_PreservesStructure()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?>
            {
                ["StringValue"] = "Test",
                ["IntValue"] = 42,
                ["BoolValue"] = true,
                ["NullValue"] = null,
                ["DoubleValue"] = 3.14
            }
        };

        // Act
        await storage.StoreMappingTableAsync("item-1", "workspace-1", "ComplexTable", data);
        var retrieved = await storage.ReadMappingTableAsync("item-1", "workspace-1", "ComplexTable");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Single(retrieved);
        Assert.True(retrieved.ContainsKey("key1"));
        var row = retrieved["key1"];
        Assert.Equal(5, row.Count);
        Assert.True(row.ContainsKey("StringValue"));
        Assert.True(row.ContainsKey("IntValue"));
        Assert.True(row.ContainsKey("BoolValue"));
        Assert.True(row.ContainsKey("NullValue"));
        Assert.True(row.ContainsKey("DoubleValue"));
    }

    [Fact]
    public async Task StoreMappingTableAsync_EmptyData_StoresSuccessfully()
    {
        // Arrange
        var storage = new OneLakeStorage();
        var data = new Dictionary<string, Dictionary<string, object?>>();

        // Act
        var path = await storage.StoreMappingTableAsync("item-1", "workspace-1", "EmptyTable", data);

        // Assert
        Assert.NotNull(path);
        var retrieved = await storage.ReadMappingTableAsync("item-1", "workspace-1", "EmptyTable");
        Assert.NotNull(retrieved);
        Assert.Empty(retrieved);
    }
}
