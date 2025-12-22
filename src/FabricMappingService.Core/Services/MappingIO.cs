using System.Reflection;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Implementation of MappingIO service for managing reference tables.
/// Provides functionality to create, sync, and read reference mapping tables.
/// </summary>
public class MappingIO : IMappingIO
{
    private readonly IReferenceMappingStorage _storage;

    /// <summary>
    /// Initializes a new instance of the MappingIO class.
    /// </summary>
    /// <param name="storage">The storage implementation for reference tables.</param>
    public MappingIO(IReferenceMappingStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc/>
    public int SyncMapping<T>(IEnumerable<T> data, string keyAttributeName, string mappingTableName)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyAttributeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(mappingTableName);

        var dataList = data.ToList();
        if (dataList.Count == 0)
        {
            return 0;
        }

        // Get the property that contains the key value
        var type = typeof(T);
        var keyProperty = type.GetProperty(keyAttributeName, BindingFlags.Public | BindingFlags.Instance);
        if (keyProperty == null)
        {
            throw new ArgumentException($"Property '{keyAttributeName}' not found on type '{type.Name}'", nameof(keyAttributeName));
        }

        // Get or create the reference table
        var table = _storage.GetReferenceTable(mappingTableName);
        if (table == null)
        {
            table = new ReferenceTable
            {
                Name = mappingTableName,
                KeyColumnName = "key",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVisible = true,
                NotifyOnNewMapping = true
            };
        }

        // Extract unique keys from the data
        var existingKeys = new HashSet<string>(table.Rows.Select(r => r.Key), StringComparer.OrdinalIgnoreCase);
        var newKeysAdded = 0;

        foreach (var item in dataList)
        {
            var keyValue = keyProperty.GetValue(item);
            if (keyValue == null)
            {
                continue;
            }

            var keyString = keyValue.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keyString))
            {
                continue;
            }

            // Only add new keys, don't update existing ones
            if (!existingKeys.Contains(keyString))
            {
                var newRow = new ReferenceTableRow
                {
                    Key = keyString,
                    Attributes = [],
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsNew = true
                };

                table.Rows.Add(newRow);
                existingKeys.Add(keyString);
                newKeysAdded++;
            }
        }

        // Save the updated table
        if (newKeysAdded > 0 || table.Rows.Count == 0)
        {
            table.UpdatedAt = DateTime.UtcNow;
            _storage.SaveReferenceTable(table);
        }

        return newKeysAdded;
    }

    /// <inheritdoc/>
    public Dictionary<string, Dictionary<string, object?>> ReadMapping(string mappingTableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mappingTableName);

        var table = _storage.GetReferenceTable(mappingTableName);
        if (table == null)
        {
            throw new InvalidOperationException($"Reference table '{mappingTableName}' not found");
        }

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

    /// <inheritdoc/>
    public void CreateReferenceTable(string tableName, List<ReferenceTableColumn> columns, bool isVisible = true, bool notifyOnNewMapping = false,
        string? sourceLakehouseItemId = null, string? sourceWorkspaceId = null, string? sourceTableName = null, string? sourceOneLakeLink = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(columns);

        if (_storage.TableExists(tableName))
        {
            throw new InvalidOperationException($"Reference table '{tableName}' already exists");
        }

        var table = new ReferenceTable
        {
            Name = tableName,
            KeyColumnName = "key",
            Columns = columns,
            Rows = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsVisible = isVisible,
            NotifyOnNewMapping = notifyOnNewMapping,
            SourceLakehouseItemId = sourceLakehouseItemId,
            SourceWorkspaceId = sourceWorkspaceId,
            SourceTableName = sourceTableName,
            SourceOneLakeLink = sourceOneLakeLink
        };

        _storage.SaveReferenceTable(table);
    }

    /// <inheritdoc/>
    public ReferenceTable? GetReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        return _storage.GetReferenceTable(tableName);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAllTableNames()
    {
        return _storage.GetAllTableNames();
    }

    /// <inheritdoc/>
    public bool DeleteReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        return _storage.DeleteReferenceTable(tableName);
    }

    /// <inheritdoc/>
    public void AddOrUpdateRow(string tableName, string key, Dictionary<string, object?> attributes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(attributes);

        var table = _storage.GetReferenceTable(tableName);
        if (table == null)
        {
            throw new InvalidOperationException($"Reference table '{tableName}' not found");
        }

        var existingRow = table.Rows.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase));
        
        if (existingRow != null)
        {
            // Update existing row
            existingRow.Attributes = attributes;
            existingRow.UpdatedAt = DateTime.UtcNow;
            existingRow.IsNew = false;
        }
        else
        {
            // Add new row
            var newRow = new ReferenceTableRow
            {
                Key = key,
                Attributes = attributes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsNew = true
            };
            table.Rows.Add(newRow);
        }

        table.UpdatedAt = DateTime.UtcNow;
        _storage.SaveReferenceTable(table);
    }
}
