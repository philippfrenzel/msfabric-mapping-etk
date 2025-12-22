using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

namespace FabricMappingService.Core.Examples;

/// <summary>
/// Example demonstrating reference table usage for product type mapping.
/// This example shows both manual and automated reference table scenarios.
/// </summary>
public static class ReferenceTableExample
{
    // Example product data model
    public class ProductData
    {
        public string Produkt { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Demonstrates Scenario 1: Manual reference table creation.
    /// </summary>
    public static void ManualReferenceTableExample()
    {
        // Setup
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);

        // Step 1: Create an empty reference table with custom columns
        var columns = new List<ReferenceTableColumn>
        {
            new() { Name = "Category", DataType = "string", Order = 1, Description = "Product category" },
            new() { Name = "Group", DataType = "string", Order = 2, Description = "Product group" },
            new() { Name = "SubGroup", DataType = "string", Order = 3, Description = "Product sub-group" }
        };

        mappingIO.CreateReferenceTable(
            tableName: "vertragsprodukte",
            columns: columns,
            isVisible: true,
            notifyOnNewMapping: false);

        Console.WriteLine("Created reference table 'vertragsprodukte'");

        // Step 2: Add rows manually for custom classifications
        mappingIO.AddOrUpdateRow(
            tableName: "vertragsprodukte",
            key: "VTP001",
            attributes: new Dictionary<string, object?>
            {
                ["Category"] = "Insurance",
                ["Group"] = "Health",
                ["SubGroup"] = "Basic Coverage"
            });

        mappingIO.AddOrUpdateRow(
            tableName: "vertragsprodukte",
            key: "VTP002",
            attributes: new Dictionary<string, object?>
            {
                ["Category"] = "Insurance",
                ["Group"] = "Health",
                ["SubGroup"] = "Premium Coverage"
            });

        Console.WriteLine("Added manual classification rows");

        // Step 3: Read and use the mapping
        var mapping = mappingIO.ReadMapping("vertragsprodukte");
        
        Console.WriteLine("\nReference table data:");
        foreach (var entry in mapping)
        {
            Console.WriteLine($"  Key: {entry.Key}");
            foreach (var attr in entry.Value)
            {
                Console.WriteLine($"    {attr.Key}: {attr.Value}");
            }
        }
    }

    /// <summary>
    /// Demonstrates Scenario 2: Automated reference table from source data.
    /// </summary>
    public static void AutomatedReferenceTableExample()
    {
        // Setup
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);

        // Step 1: Define source data
        var products = new List<ProductData>
        {
            new() { Produkt = "VTP001", Name = "Basic Health Plan", Price = 100m },
            new() { Produkt = "VTP002", Name = "Premium Health Plan", Price = 200m },
            new() { Produkt = "VTP003", Name = "Family Health Plan", Price = 300m }
        };

        Console.WriteLine("Source data loaded:");
        foreach (var product in products)
        {
            Console.WriteLine($"  {product.Produkt}: {product.Name} - ${product.Price}");
        }

        // Step 2: Sync reference table (creates table and adds keys)
        var newKeysAdded = mappingIO.SyncMapping(
            data: products,
            keyAttributeName: "Produkt",
            mappingTableName: "produkttyp");

        Console.WriteLine($"\nSynced reference table: {newKeysAdded} new keys added");

        // Step 3: Add more data (only new keys are added)
        var moreProducts = new List<ProductData>
        {
            new() { Produkt = "VTP002", Name = "Premium Health Plan Updated", Price = 250m }, // Duplicate
            new() { Produkt = "VTP004", Name = "Senior Health Plan", Price = 350m } // New
        };

        newKeysAdded = mappingIO.SyncMapping(
            data: moreProducts,
            keyAttributeName: "Produkt",
            mappingTableName: "produkttyp");

        Console.WriteLine($"Second sync: {newKeysAdded} new keys added (VTP002 ignored as duplicate)");

        // Step 4: Now add classification attributes to the keys
        mappingIO.AddOrUpdateRow(
            tableName: "produkttyp",
            key: "VTP001",
            attributes: new Dictionary<string, object?>
            {
                ["ProductType"] = "Basic",
                ["TargetGroup"] = "Individual"
            });

        mappingIO.AddOrUpdateRow(
            tableName: "produkttyp",
            key: "VTP002",
            attributes: new Dictionary<string, object?>
            {
                ["ProductType"] = "Premium",
                ["TargetGroup"] = "Individual"
            });

        mappingIO.AddOrUpdateRow(
            tableName: "produkttyp",
            key: "VTP003",
            attributes: new Dictionary<string, object?>
            {
                ["ProductType"] = "Basic",
                ["TargetGroup"] = "Family"
            });

        Console.WriteLine("\nAdded classification attributes");

        // Step 5: Read and use the mapping
        var mapping = mappingIO.ReadMapping("produkttyp");
        
        Console.WriteLine("\nFinal reference table:");
        foreach (var entry in mapping)
        {
            Console.WriteLine($"  {entry.Value["key"]}:");
            foreach (var attr in entry.Value.Where(a => a.Key != "key"))
            {
                Console.WriteLine($"    {attr.Key}: {attr.Value ?? "(not classified)"}");
            }
        }
    }

    /// <summary>
    /// Demonstrates managing multiple reference tables.
    /// </summary>
    public static void MultipleTablesExample()
    {
        var storage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(storage);

        // Create multiple reference tables
        mappingIO.CreateReferenceTable("cost_types", new List<ReferenceTableColumn>
        {
            new() { Name = "MainCategory", DataType = "string", Order = 1 },
            new() { Name = "SubCategory", DataType = "string", Order = 2 }
        });

        mappingIO.CreateReferenceTable("diagnosis_codes", new List<ReferenceTableColumn>
        {
            new() { Name = "Classification", DataType = "string", Order = 1 },
            new() { Name = "Severity", DataType = "string", Order = 2 }
        });

        // Add some sample data
        mappingIO.AddOrUpdateRow("cost_types", "COST001", new Dictionary<string, object?>
        {
            ["MainCategory"] = "Medical",
            ["SubCategory"] = "Consultation"
        });

        mappingIO.AddOrUpdateRow("diagnosis_codes", "DIAG001", new Dictionary<string, object?>
        {
            ["Classification"] = "Cardiovascular",
            ["Severity"] = "Moderate"
        });

        // List all tables
        var tableNames = mappingIO.GetAllTableNames();
        Console.WriteLine($"Total reference tables: {tableNames.Count()}");
        foreach (var name in tableNames)
        {
            var table = mappingIO.GetReferenceTable(name);
            Console.WriteLine($"  - {name}: {table?.Rows.Count ?? 0} rows");
        }
    }
}
