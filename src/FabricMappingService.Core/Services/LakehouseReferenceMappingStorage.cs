using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Lakehouse-based implementation of reference mapping storage.
/// Stores reference table configurations and data in a lakehouse.
/// </summary>
public class LakehouseReferenceMappingStorage : IReferenceMappingStorage
{
    private readonly ILakehouseStorage _lakehouseStorage;
    private readonly string _lakehousePath;

    /// <summary>
    /// Initializes a new instance of the LakehouseReferenceMappingStorage class.
    /// </summary>
    /// <param name="lakehouseStorage">The lakehouse storage implementation.</param>
    /// <param name="lakehousePath">The base path in the lakehouse for storing reference tables.</param>
    public LakehouseReferenceMappingStorage(ILakehouseStorage lakehouseStorage, string lakehousePath)
    {
        _lakehouseStorage = lakehouseStorage ?? throw new ArgumentNullException(nameof(lakehouseStorage));
        _lakehousePath = !string.IsNullOrWhiteSpace(lakehousePath) 
            ? lakehousePath 
            : throw new ArgumentException("Lakehouse path cannot be null or empty", nameof(lakehousePath));
    }

    /// <inheritdoc/>
    public ReferenceTable? GetReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        // Load configuration and data synchronously
        var table = _lakehouseStorage.LoadReferenceTableConfigurationAsync(tableName, _lakehousePath)
            .GetAwaiter()
            .GetResult();

        if (table == null)
        {
            return null;
        }

        // Load the data
        var data = _lakehouseStorage.LoadReferenceTableDataAsync(tableName, _lakehousePath)
            .GetAwaiter()
            .GetResult();

        // Rebuild rows from data
        table.Rows = data.Select(kvp => new ReferenceTableRow
        {
            Key = kvp.Key,
            Attributes = kvp.Value.Where(attr => !string.Equals(attr.Key, "key", StringComparison.OrdinalIgnoreCase))
                                  .ToDictionary(attr => attr.Key, attr => attr.Value),
            CreatedAt = DateTime.UtcNow, // These timestamps are not preserved in data
            UpdatedAt = DateTime.UtcNow,
            IsNew = false
        }).ToList();

        return table;
    }

    /// <inheritdoc/>
    public void SaveReferenceTable(ReferenceTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(table.Name);

        table.UpdatedAt = DateTime.UtcNow;

        // Save configuration
        _lakehouseStorage.SaveReferenceTableConfigurationAsync(table, _lakehousePath)
            .GetAwaiter()
            .GetResult();

        // Save data
        var data = ConvertRowsToData(table);
        _lakehouseStorage.SaveReferenceTableDataAsync(table.Name, data, _lakehousePath)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc/>
    public bool DeleteReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return _lakehouseStorage.DeleteReferenceTableAsync(tableName, _lakehousePath)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAllTableNames()
    {
        return _lakehouseStorage.ListReferenceTableNamesAsync(_lakehousePath)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc/>
    public bool TableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return _lakehouseStorage.TableExistsAsync(tableName, _lakehousePath)
            .GetAwaiter()
            .GetResult();
    }

    private static Dictionary<string, Dictionary<string, object?>> ConvertRowsToData(ReferenceTable table)
    {
        var result = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in table.Rows)
        {
            var rowData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                [table.KeyColumnName] = row.Key
            };

            foreach (var attr in row.Attributes)
            {
                rowData[attr.Key] = attr.Value;
            }

            result[row.Key] = rowData;
        }

        return result;
    }
}
