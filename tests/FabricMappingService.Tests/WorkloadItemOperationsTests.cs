using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using FabricMappingService.Core.Workload;
using Xunit;

namespace FabricMappingService.Tests;

public class WorkloadItemOperationsTests
{
    private readonly MappingWorkload _workload;
    private readonly IItemDefinitionStorage _itemStorage;
    private readonly IOneLakeStorage _oneLakeStorage;

    public WorkloadItemOperationsTests()
    {
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);
        var configuration = new MappingConfiguration();
        var mappingService = new AttributeMappingService(configuration);
        _itemStorage = new ItemDefinitionStorage();
        _oneLakeStorage = new OneLakeStorage();

        _workload = new MappingWorkload(
            mappingIO,
            mappingService,
            configuration,
            _itemStorage,
            _oneLakeStorage);
    }

    [Fact]
    public async Task ExecuteAsync_CreateMappingItem_CreatesItemSuccessfully()
    {
        // Arrange
        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.CreateMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["displayName"] = "Test Mapping Item",
                ["workspaceId"] = "workspace-1",
                ["lakehouseItemId"] = "lakehouse-1",
                ["tableName"] = "TestTable",
                ["referenceAttributeName"] = "Id",
                ["description"] = "Test description",
                ["mappingColumns"] = "[]"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateMappingItem_UpdatesItemSuccessfully()
    {
        // Arrange
        // First create an item
        var itemId = Guid.NewGuid().ToString();
        var definition = new MappingItemDefinition
        {
            ItemId = itemId,
            DisplayName = "Original Name",
            WorkspaceId = "workspace-1",
            Configuration = new MappingItemConfiguration
            {
                LakehouseItemId = "lakehouse-1",
                TableName = "TestTable",
                ReferenceAttributeName = "Id"
            }
        };
        await _itemStorage.CreateItemDefinitionAsync(definition);

        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.UpdateMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = itemId,
                ["displayName"] = "Updated Name",
                ["description"] = "Updated description"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        var updated = await _itemStorage.GetItemDefinitionAsync(itemId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteMappingItem_DeletesItemSuccessfully()
    {
        // Arrange
        // First create an item
        var itemId = Guid.NewGuid().ToString();
        var definition = new MappingItemDefinition
        {
            ItemId = itemId,
            DisplayName = "Test Item",
            WorkspaceId = "workspace-1",
            Configuration = new MappingItemConfiguration
            {
                LakehouseItemId = "lakehouse-1",
                TableName = "TestTable",
                ReferenceAttributeName = "Id"
            }
        };
        await _itemStorage.CreateItemDefinitionAsync(definition);

        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.DeleteMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = itemId
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        var deleted = await _itemStorage.GetItemDefinitionAsync(itemId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ExecuteAsync_StoreToOneLake_StoresDataSuccessfully()
    {
        // Arrange
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Name"] = "Value1" }
        };
        var dataJson = System.Text.Json.JsonSerializer.Serialize(data);

        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.StoreToOneLake,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = "item-1",
                ["workspaceId"] = "workspace-1",
                ["tableName"] = "TestTable",
                ["data"] = dataJson
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_ReadFromOneLake_ReadsDataSuccessfully()
    {
        // Arrange
        // First store some data
        var data = new Dictionary<string, Dictionary<string, object?>>
        {
            ["key1"] = new Dictionary<string, object?> { ["Name"] = "Value1" }
        };
        await _oneLakeStorage.StoreMappingTableAsync("item-1", "workspace-1", "TestTable", data);

        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.ReadFromOneLake,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = "item-1",
                ["workspaceId"] = "workspace-1",
                ["tableName"] = "TestTable"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_CreateMappingItem_MissingParameters_ReturnsError()
    {
        // Arrange
        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.CreateMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                // Missing required parameters
                ["displayName"] = "Test"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateMappingItem_NonExistingItem_ReturnsError()
    {
        // Arrange
        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.UpdateMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = "non-existing-id",
                ["displayName"] = "Updated Name"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteMappingItem_NonExistingItem_ReturnsSuccessWithMessage()
    {
        // Arrange
        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.DeleteMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["itemId"] = "non-existing-id"
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_CreateMappingItem_WithMappingColumns_CreatesWithColumns()
    {
        // Arrange
        var mappingColumns = new[]
        {
            new { ColumnName = "Column1", DataType = "string", IsRequired = true },
            new { ColumnName = "Column2", DataType = "int", IsRequired = false }
        };
        var mappingColumnsJson = System.Text.Json.JsonSerializer.Serialize(mappingColumns);

        var config = new WorkloadConfiguration
        {
            OperationType = WorkloadOperationType.CreateMappingItem,
            Parameters = new Dictionary<string, object?>
            {
                ["displayName"] = "Test with Columns",
                ["workspaceId"] = "workspace-1",
                ["lakehouseItemId"] = "lakehouse-1",
                ["tableName"] = "TestTable",
                ["referenceAttributeName"] = "Id",
                ["mappingColumns"] = mappingColumnsJson
            }
        };

        // Act
        var result = await _workload.ExecuteAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }
}
