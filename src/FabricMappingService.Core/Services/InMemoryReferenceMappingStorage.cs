using System.Collections.Concurrent;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// In-memory implementation of reference mapping storage.
/// Uses a thread-safe concurrent dictionary for storing reference tables.
/// </summary>
public class InMemoryReferenceMappingStorage : IReferenceMappingStorage
{
    private readonly ConcurrentDictionary<string, ReferenceTable> _tables = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public ReferenceTable? GetReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        
        _tables.TryGetValue(tableName, out var table);
        return table;
    }

    /// <inheritdoc/>
    public void SaveReferenceTable(ReferenceTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(table.Name);

        table.UpdatedAt = DateTime.UtcNow;
        _tables.AddOrUpdate(table.Name, table, (_, _) => table);
    }

    /// <inheritdoc/>
    public bool DeleteReferenceTable(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        
        return _tables.TryRemove(tableName, out _);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAllTableNames()
    {
        return _tables.Keys.ToList();
    }

    /// <inheritdoc/>
    public bool TableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        
        return _tables.ContainsKey(tableName);
    }
}
