using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for storing data to OneLake.
/// Follows the Fabric Extensibility Toolkit pattern for OneLake data storage.
/// </summary>
public interface IOneLakeStorage
{
    /// <summary>
    /// Stores a mapping/lookup table to OneLake.
    /// </summary>
    /// <param name="itemId">The item ID associated with this mapping table.</param>
    /// <param name="workspaceId">The workspace ID where the data will be stored.</param>
    /// <param name="tableName">The name of the table to store.</param>
    /// <param name="data">The mapping data to store (key-value pairs).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OneLake path where the data was stored.</returns>
    Task<string> StoreMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        Dictionary<string, Dictionary<string, object?>> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a mapping/lookup table from OneLake.
    /// </summary>
    /// <param name="itemId">The item ID associated with this mapping table.</param>
    /// <param name="workspaceId">The workspace ID where the data is stored.</param>
    /// <param name="tableName">The name of the table to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapping data (key-value pairs).</returns>
    Task<Dictionary<string, Dictionary<string, object?>>> ReadMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mapping/lookup table from OneLake.
    /// </summary>
    /// <param name="itemId">The item ID associated with this mapping table.</param>
    /// <param name="workspaceId">The workspace ID where the data is stored.</param>
    /// <param name="tableName">The name of the table to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the table was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the OneLake path for a mapping table.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="tableName">The table name.</param>
    /// <returns>The OneLake path.</returns>
    string GetOneLakePath(string workspaceId, string itemId, string tableName);
}
