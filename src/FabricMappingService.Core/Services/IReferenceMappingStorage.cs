using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for storing and retrieving reference mapping tables.
/// </summary>
public interface IReferenceMappingStorage
{
    /// <summary>
    /// Gets a reference table by name.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>The reference table, or null if not found.</returns>
    ReferenceTable? GetReferenceTable(string tableName);

    /// <summary>
    /// Saves or updates a reference table.
    /// </summary>
    /// <param name="table">The reference table to save.</param>
    void SaveReferenceTable(ReferenceTable table);

    /// <summary>
    /// Deletes a reference table by name.
    /// </summary>
    /// <param name="tableName">The name of the reference table to delete.</param>
    /// <returns>True if the table was deleted, false if it didn't exist.</returns>
    bool DeleteReferenceTable(string tableName);

    /// <summary>
    /// Gets all reference table names.
    /// </summary>
    /// <returns>A collection of all reference table names.</returns>
    IEnumerable<string> GetAllTableNames();

    /// <summary>
    /// Checks if a reference table exists.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>True if the table exists, false otherwise.</returns>
    bool TableExists(string tableName);
}
