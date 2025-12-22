using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for MappingIO service that manages reference tables.
/// Provides methods to synchronize and read reference mapping tables.
/// </summary>
public interface IMappingIO
{
    /// <summary>
    /// Synchronizes a reference table with data from a collection.
    /// Creates the table if it doesn't exist, or updates existing keys.
    /// New keys are added, but existing keys are not removed.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="data">The collection of objects containing the key values.</param>
    /// <param name="keyAttributeName">The name of the property that contains the key values.</param>
    /// <param name="mappingTableName">The name of the reference table to create or update.</param>
    /// <returns>The number of new keys added to the reference table.</returns>
    int SyncMapping<T>(IEnumerable<T> data, string keyAttributeName, string mappingTableName);

    /// <summary>
    /// Reads a reference table and returns its data as a dictionary.
    /// </summary>
    /// <param name="mappingTableName">The name of the reference table to read.</param>
    /// <returns>A dictionary where keys are the reference keys and values are dictionaries of attribute values.</returns>
    Dictionary<string, Dictionary<string, object?>> ReadMapping(string mappingTableName);

    /// <summary>
    /// Creates an empty reference table with specified columns.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="columns">The column definitions for the table.</param>
    /// <param name="isVisible">Whether the table is visible in the UI.</param>
    /// <param name="notifyOnNewMapping">Whether to send notifications when new mappings are added.</param>
    void CreateReferenceTable(string tableName, List<ReferenceTableColumn> columns, bool isVisible = true, bool notifyOnNewMapping = false);

    /// <summary>
    /// Gets a reference table by name.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>The reference table, or null if not found.</returns>
    ReferenceTable? GetReferenceTable(string tableName);

    /// <summary>
    /// Gets all reference table names.
    /// </summary>
    /// <returns>A collection of all reference table names.</returns>
    IEnumerable<string> GetAllTableNames();

    /// <summary>
    /// Deletes a reference table.
    /// </summary>
    /// <param name="tableName">The name of the reference table to delete.</param>
    /// <returns>True if the table was deleted, false if it didn't exist.</returns>
    bool DeleteReferenceTable(string tableName);

    /// <summary>
    /// Adds or updates a row in a reference table.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="key">The key value for the row.</param>
    /// <param name="attributes">The attribute values for the row.</param>
    void AddOrUpdateRow(string tableName, string key, Dictionary<string, object?> attributes);
}
