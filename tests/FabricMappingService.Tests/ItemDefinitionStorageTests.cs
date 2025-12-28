using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class ItemDefinitionStorageTests
{
    [Fact]
    public async Task CreateItemDefinitionAsync_ValidItem_CreatesSuccessfully()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "test-item-1",
            DisplayName = "Test Item",
            WorkspaceId = "workspace-1",
            Configuration = new MappingItemConfiguration
            {
                LakehouseItemId = "lakehouse-1",
                TableName = "TestTable",
                ReferenceAttributeName = "Id"
            }
        };

        // Act
        await storage.CreateItemDefinitionAsync(definition);

        // Assert
        var retrieved = await storage.GetItemDefinitionAsync("test-item-1");
        Assert.NotNull(retrieved);
        Assert.Equal("Test Item", retrieved.DisplayName);
        Assert.Equal("workspace-1", retrieved.WorkspaceId);
    }

    [Fact]
    public async Task CreateItemDefinitionAsync_DuplicateId_ThrowsException()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition1 = new MappingItemDefinition
        {
            ItemId = "duplicate-id",
            DisplayName = "Item 1",
            WorkspaceId = "workspace-1"
        };
        var definition2 = new MappingItemDefinition
        {
            ItemId = "duplicate-id",
            DisplayName = "Item 2",
            WorkspaceId = "workspace-1"
        };

        await storage.CreateItemDefinitionAsync(definition1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => storage.CreateItemDefinitionAsync(definition2));
    }

    [Fact]
    public async Task CreateItemDefinitionAsync_NullDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => storage.CreateItemDefinitionAsync(null!));
    }

    [Fact]
    public async Task CreateItemDefinitionAsync_EmptyItemId_ThrowsArgumentException()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "",
            DisplayName = "Test",
            WorkspaceId = "workspace-1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.CreateItemDefinitionAsync(definition));
    }

    [Fact]
    public async Task GetItemDefinitionAsync_ExistingItem_ReturnsItem()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "item-1",
            DisplayName = "Test Item",
            WorkspaceId = "workspace-1"
        };
        await storage.CreateItemDefinitionAsync(definition);

        // Act
        var retrieved = await storage.GetItemDefinitionAsync("item-1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("item-1", retrieved.ItemId);
        Assert.Equal("Test Item", retrieved.DisplayName);
    }

    [Fact]
    public async Task GetItemDefinitionAsync_NonExistingItem_ReturnsNull()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();

        // Act
        var retrieved = await storage.GetItemDefinitionAsync("non-existing");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task UpdateItemDefinitionAsync_ExistingItem_UpdatesSuccessfully()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "item-1",
            DisplayName = "Original Name",
            WorkspaceId = "workspace-1"
        };
        await storage.CreateItemDefinitionAsync(definition);

        // Act
        definition.DisplayName = "Updated Name";
        await storage.UpdateItemDefinitionAsync(definition);

        // Assert
        var retrieved = await storage.GetItemDefinitionAsync("item-1");
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", retrieved.DisplayName);
    }

    [Fact]
    public async Task UpdateItemDefinitionAsync_NonExistingItem_ThrowsException()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "non-existing",
            DisplayName = "Test",
            WorkspaceId = "workspace-1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => storage.UpdateItemDefinitionAsync(definition));
    }

    [Fact]
    public async Task DeleteItemDefinitionAsync_ExistingItem_DeletesSuccessfully()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "item-to-delete",
            DisplayName = "Test",
            WorkspaceId = "workspace-1"
        };
        await storage.CreateItemDefinitionAsync(definition);

        // Act
        var deleted = await storage.DeleteItemDefinitionAsync("item-to-delete");

        // Assert
        Assert.True(deleted);
        var retrieved = await storage.GetItemDefinitionAsync("item-to-delete");
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteItemDefinitionAsync_NonExistingItem_ReturnsFalse()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();

        // Act
        var deleted = await storage.DeleteItemDefinitionAsync("non-existing");

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task ListItemDefinitionsAsync_MultipleItemsInWorkspace_ReturnsAllItems()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var workspace1Items = new[]
        {
            new MappingItemDefinition { ItemId = "item-1", DisplayName = "Item 1", WorkspaceId = "workspace-1" },
            new MappingItemDefinition { ItemId = "item-2", DisplayName = "Item 2", WorkspaceId = "workspace-1" }
        };
        var workspace2Item = new MappingItemDefinition { ItemId = "item-3", DisplayName = "Item 3", WorkspaceId = "workspace-2" };

        foreach (var item in workspace1Items)
        {
            await storage.CreateItemDefinitionAsync(item);
        }
        await storage.CreateItemDefinitionAsync(workspace2Item);

        // Act
        var items = await storage.ListItemDefinitionsAsync("workspace-1");

        // Assert
        var itemsList = items.ToList();
        Assert.Equal(2, itemsList.Count);
        Assert.All(itemsList, item => Assert.Equal("workspace-1", item.WorkspaceId));
    }

    [Fact]
    public async Task ListItemDefinitionsAsync_EmptyWorkspace_ReturnsEmpty()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();

        // Act
        var items = await storage.ListItemDefinitionsAsync("empty-workspace");

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public async Task ItemExistsAsync_ExistingItem_ReturnsTrue()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "existing-item",
            DisplayName = "Test",
            WorkspaceId = "workspace-1"
        };
        await storage.CreateItemDefinitionAsync(definition);

        // Act
        var exists = await storage.ItemExistsAsync("existing-item");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ItemExistsAsync_NonExistingItem_ReturnsFalse()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();

        // Act
        var exists = await storage.ItemExistsAsync("non-existing");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CreateItemDefinitionAsync_SetsCreatedAndUpdatedDates()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var beforeCreate = DateTime.UtcNow;
        var definition = new MappingItemDefinition
        {
            ItemId = "item-1",
            DisplayName = "Test",
            WorkspaceId = "workspace-1"
        };

        // Act
        await storage.CreateItemDefinitionAsync(definition);
        var afterCreate = DateTime.UtcNow;

        // Assert
        var retrieved = await storage.GetItemDefinitionAsync("item-1");
        Assert.NotNull(retrieved);
        Assert.True(retrieved.CreatedAt >= beforeCreate && retrieved.CreatedAt <= afterCreate);
        Assert.True(retrieved.UpdatedAt >= beforeCreate && retrieved.UpdatedAt <= afterCreate);
    }

    [Fact]
    public async Task UpdateItemDefinitionAsync_UpdatesUpdatedDate()
    {
        // Arrange
        var storage = new ItemDefinitionStorage();
        var definition = new MappingItemDefinition
        {
            ItemId = "item-1",
            DisplayName = "Original",
            WorkspaceId = "workspace-1"
        };
        await storage.CreateItemDefinitionAsync(definition);
        var originalUpdatedAt = definition.UpdatedAt;

        await Task.Delay(10); // Small delay to ensure different timestamp

        // Act
        definition.DisplayName = "Updated";
        await storage.UpdateItemDefinitionAsync(definition);

        // Assert
        var retrieved = await storage.GetItemDefinitionAsync("item-1");
        Assert.NotNull(retrieved);
        Assert.True(retrieved.UpdatedAt > originalUpdatedAt);
    }
}
