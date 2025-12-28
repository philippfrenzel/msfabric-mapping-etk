using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for lakehouse storage operations.
/// Provides methods to store and retrieve reference table configurations and data from a lakehouse.
/// </summary>
public interface ILakehouseStorage
{
    /// <summary>
    /// Saves a reference table configuration to the lakehouse as JSON.
    /// </summary>
    /// <param name="table">The reference table to save.</param>
    /// <param name="lakehousePath">The path in the lakehouse where configurations are stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveReferenceTableConfigurationAsync(
        ReferenceTable table,
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a reference table configuration from the lakehouse.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="lakehousePath">The path in the lakehouse where configurations are stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reference table configuration, or null if not found.</returns>
    Task<ReferenceTable?> LoadReferenceTableConfigurationAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves reference table data (rows) to the lakehouse.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="data">The mapping data to save.</param>
    /// <param name="lakehousePath">The path in the lakehouse where data is stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveReferenceTableDataAsync(
        string tableName,
        Dictionary<string, Dictionary<string, object?>> data,
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads reference table data (rows) from the lakehouse.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="lakehousePath">The path in the lakehouse where data is stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reference table data.</returns>
    Task<Dictionary<string, Dictionary<string, object?>>> LoadReferenceTableDataAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a reference table configuration and its data from the lakehouse.
    /// </summary>
    /// <param name="tableName">The name of the reference table to delete.</param>
    /// <param name="lakehousePath">The path in the lakehouse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the table was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteReferenceTableAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all reference table names in the lakehouse.
    /// </summary>
    /// <param name="lakehousePath">The path in the lakehouse where configurations are stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all reference table names.</returns>
    Task<IEnumerable<string>> ListReferenceTableNamesAsync(
        string lakehousePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a reference table exists in the lakehouse.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <param name="lakehousePath">The path in the lakehouse where configurations are stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the table exists, false otherwise.</returns>
    Task<bool> TableExistsAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default);
}
