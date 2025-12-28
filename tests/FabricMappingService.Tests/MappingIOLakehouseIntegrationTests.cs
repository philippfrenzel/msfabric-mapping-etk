using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

/// <summary>
/// Integration tests for MappingIO with lakehouse storage.
/// </summary>
public class MappingIOLakehouseIntegrationTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LakehouseStorage _lakehouseStorage;
    private readonly LakehouseReferenceMappingStorage _storage;
    private readonly MappingIO _mappingIO;

    private class TestProduct
    {
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public MappingIOLakehouseIntegrationTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "FabricMappingServiceTests", Guid.NewGuid().ToString());
        _lakehouseStorage = new LakehouseStorage();
        _storage = new LakehouseReferenceMappingStorage(_lakehouseStorage, _testBasePath);
        _mappingIO = new MappingIO(_storage);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public void CreateReferenceTable_WithLakehouseStorage_SavesConfigurationAndData()
    {
        // Arrange
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1, Description = "Product category" },
            new() { Name = "Group", DataType = "string", Order = 2, Description = "Product group" }
        };

        // Act
        _mappingIO.CreateReferenceTable(
            tableName: "products",
            columns: columns,
            isVisible: true,
            notifyOnNewMapping: true);

        // Assert
        var table = _mappingIO.GetReferenceTable("products");
        Assert.NotNull(table);
        Assert.Equal("products", table.Name);
        Assert.Equal(2, table.Columns.Count);
        Assert.True(table.IsVisible);
        Assert.True(table.NotifyOnNewMapping);
        
        // Verify it persisted to lakehouse
        var reloaded = _storage.GetReferenceTable("products");
        Assert.NotNull(reloaded);
        Assert.Equal("products", reloaded.Name);
    }

    [Fact]
    public void SyncMapping_WithLakehouseStorage_PersistsData()
    {
        // Arrange
        var products = new List<TestProduct>
        {
            new() { ProductCode = "PROD001", Name = "Laptop", Price = 999.99m },
            new() { ProductCode = "PROD002", Name = "Mouse", Price = 29.99m },
            new() { ProductCode = "PROD003", Name = "Keyboard", Price = 79.99m }
        };

        // Act - First sync
        var newKeysAdded = _mappingIO.SyncMapping(
            data: products,
            keyAttributeName: "ProductCode",
            mappingTableName: "product_mapping");

        // Assert first sync
        Assert.Equal(3, newKeysAdded);
        
        var mapping = _mappingIO.ReadMapping("product_mapping");
        Assert.Equal(3, mapping.Count);
        Assert.True(mapping.ContainsKey("PROD001"));
        Assert.True(mapping.ContainsKey("PROD002"));
        Assert.True(mapping.ContainsKey("PROD003"));

        // Act - Second sync with one new product
        var moreProducts = new List<TestProduct>
        {
            new() { ProductCode = "PROD002", Name = "Mouse Updated", Price = 24.99m }, // Existing
            new() { ProductCode = "PROD004", Name = "Monitor", Price = 299.99m } // New
        };

        var secondSync = _mappingIO.SyncMapping(
            data: moreProducts,
            keyAttributeName: "ProductCode",
            mappingTableName: "product_mapping");

        // Assert second sync
        Assert.Equal(1, secondSync); // Only PROD004 should be new
        
        mapping = _mappingIO.ReadMapping("product_mapping");
        Assert.Equal(4, mapping.Count);
        Assert.True(mapping.ContainsKey("PROD004"));
    }

    [Fact]
    public void AddOrUpdateRow_WithLakehouseStorage_PersistsChanges()
    {
        // Arrange
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 },
            new() { Name = "SubCategory", DataType = "string", Order = 2 }
        };

        _mappingIO.CreateReferenceTable("categories", columns, true, false);

        // Act - Add a new row
        _mappingIO.AddOrUpdateRow(
            tableName: "categories",
            key: "CAT001",
            attributes: new Dictionary<string, object?>
            {
                ["Category"] = "Electronics",
                ["SubCategory"] = "Computers"
            });

        // Assert - Verify the row was added
        var mapping = _mappingIO.ReadMapping("categories");
        Assert.Single(mapping);
        Assert.True(mapping.ContainsKey("CAT001"));
        Assert.Equal("Electronics", mapping["CAT001"]["Category"]?.ToString());

        // Act - Update the same row
        _mappingIO.AddOrUpdateRow(
            tableName: "categories",
            key: "CAT001",
            attributes: new Dictionary<string, object?>
            {
                ["Category"] = "Technology",
                ["SubCategory"] = "Laptops"
            });

        // Assert - Verify the row was updated
        mapping = _mappingIO.ReadMapping("categories");
        Assert.Single(mapping);
        Assert.Equal("Technology", mapping["CAT001"]["Category"]?.ToString());
        Assert.Equal("Laptops", mapping["CAT001"]["SubCategory"]?.ToString());
    }

    [Fact]
    public void DeleteReferenceTable_WithLakehouseStorage_RemovesPersistedData()
    {
        // Arrange
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Description", DataType = "string", Order = 1 }
        };

        _mappingIO.CreateReferenceTable("temp_table", columns, true, false);
        
        // Verify it exists
        var tableNames = _mappingIO.GetAllTableNames().ToList();
        Assert.Contains("temp_table", tableNames);

        // Act
        var deleted = _mappingIO.DeleteReferenceTable("temp_table");

        // Assert
        Assert.True(deleted);
        tableNames = _mappingIO.GetAllTableNames().ToList();
        Assert.DoesNotContain("temp_table", tableNames);
        
        var table = _mappingIO.GetReferenceTable("temp_table");
        Assert.Null(table);
    }

    [Fact]
    public void GetAllTableNames_WithLakehouseStorage_ReturnsPersistedTables()
    {
        // Arrange
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Value", DataType = "string", Order = 1 }
        };

        _mappingIO.CreateReferenceTable("table1", columns, true, false);
        _mappingIO.CreateReferenceTable("table2", columns, true, false);
        _mappingIO.CreateReferenceTable("table3", columns, true, false);

        // Act
        var tableNames = _mappingIO.GetAllTableNames().ToList();

        // Assert
        Assert.Equal(3, tableNames.Count);
        Assert.Contains("table1", tableNames);
        Assert.Contains("table2", tableNames);
        Assert.Contains("table3", tableNames);
    }

    [Fact]
    public void CreateReferenceTable_WithSourceLakehouseInfo_PersistsMetadata()
    {
        // Arrange
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 }
        };

        // Act
        _mappingIO.CreateReferenceTable(
            tableName: "sourced_table",
            columns: columns,
            isVisible: true,
            notifyOnNewMapping: false,
            sourceLakehouseItemId: "lakehouse-123",
            sourceWorkspaceId: "workspace-456",
            sourceTableName: "SourceTable",
            sourceOneLakeLink: "https://onelake.dfs.fabric.microsoft.com/workspace/lakehouse/Tables/SourceTable");

        // Assert
        var table = _mappingIO.GetReferenceTable("sourced_table");
        Assert.NotNull(table);
        Assert.Equal("lakehouse-123", table.SourceLakehouseItemId);
        Assert.Equal("workspace-456", table.SourceWorkspaceId);
        Assert.Equal("SourceTable", table.SourceTableName);
        Assert.Equal("https://onelake.dfs.fabric.microsoft.com/workspace/lakehouse/Tables/SourceTable", table.SourceOneLakeLink);
    }

    [Fact]
    public void CompleteWorkflow_CreateSyncUpdateDelete_WorksEndToEnd()
    {
        // Step 1: Create reference table
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1 },
            new() { Name = "Priority", DataType = "int", Order = 2 }
        };

        _mappingIO.CreateReferenceTable("workflow_test", columns, true, true);

        // Step 2: Sync data
        var testData = new List<TestProduct>
        {
            new() { ProductCode = "WF001", Name = "Item 1", Price = 10m },
            new() { ProductCode = "WF002", Name = "Item 2", Price = 20m }
        };

        var synced = _mappingIO.SyncMapping(testData, "ProductCode", "workflow_test");
        Assert.Equal(2, synced);

        // Step 3: Update a row
        _mappingIO.AddOrUpdateRow(
            "workflow_test",
            "WF001",
            new Dictionary<string, object?>
            {
                ["Category"] = "High Value",
                ["Priority"] = 1
            });

        // Step 4: Verify updates
        var mapping = _mappingIO.ReadMapping("workflow_test");
        Assert.Equal(2, mapping.Count);
        Assert.Equal("High Value", mapping["WF001"]["Category"]?.ToString());
        // JSON deserializes numbers as JsonElement, convert to string then parse
        var priorityValue = mapping["WF001"]["Priority"]?.ToString();
        Assert.NotNull(priorityValue);
        Assert.Equal("1", priorityValue);

        // Step 5: Delete table
        var deleted = _mappingIO.DeleteReferenceTable("workflow_test");
        Assert.True(deleted);

        // Step 6: Verify deletion
        var table = _mappingIO.GetReferenceTable("workflow_test");
        Assert.Null(table);
    }
}
