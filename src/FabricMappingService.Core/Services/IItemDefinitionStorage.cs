using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for storing and retrieving mapping item definitions.
/// Follows the Fabric Extensibility Toolkit pattern for item definition storage.
/// </summary>
public interface IItemDefinitionStorage
{
    /// <summary>
    /// Creates a new mapping item definition.
    /// </summary>
    /// <param name="definition">The mapping item definition to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateItemDefinitionAsync(MappingItemDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mapping item definition by its ID.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapping item definition, or null if not found.</returns>
    Task<MappingItemDefinition?> GetItemDefinitionAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mapping item definition.
    /// </summary>
    /// <param name="definition">The updated mapping item definition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateItemDefinitionAsync(MappingItemDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mapping item definition.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the item was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteItemDefinitionAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all mapping item definitions in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of mapping item definitions.</returns>
    Task<IEnumerable<MappingItemDefinition>> ListItemDefinitionsAsync(string workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an item definition exists.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the item exists, false otherwise.</returns>
    Task<bool> ItemExistsAsync(string itemId, CancellationToken cancellationToken = default);
}
