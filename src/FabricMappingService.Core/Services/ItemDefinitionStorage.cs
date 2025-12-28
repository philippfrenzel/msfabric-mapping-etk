using System.Collections.Concurrent;
using System.Text.Json;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// In-memory implementation of item definition storage.
/// In production, this should be replaced with persistent storage (e.g., Azure Storage, SQL Database).
/// </summary>
public class ItemDefinitionStorage : IItemDefinitionStorage
{
    private readonly ConcurrentDictionary<string, MappingItemDefinition> _storage = new();

    /// <inheritdoc/>
    public Task CreateItemDefinitionAsync(MappingItemDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.ItemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.WorkspaceId);

        if (_storage.ContainsKey(definition.ItemId))
        {
            throw new InvalidOperationException($"Item definition with ID '{definition.ItemId}' already exists");
        }

        definition.CreatedAt = DateTime.UtcNow;
        definition.UpdatedAt = DateTime.UtcNow;

        if (!_storage.TryAdd(definition.ItemId, definition))
        {
            throw new InvalidOperationException($"Failed to create item definition with ID '{definition.ItemId}'");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<MappingItemDefinition?> GetItemDefinitionAsync(string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        _storage.TryGetValue(itemId, out var definition);
        return Task.FromResult(definition);
    }

    /// <inheritdoc/>
    public Task UpdateItemDefinitionAsync(MappingItemDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.ItemId);

        if (!_storage.ContainsKey(definition.ItemId))
        {
            throw new InvalidOperationException($"Item definition with ID '{definition.ItemId}' not found");
        }

        definition.UpdatedAt = DateTime.UtcNow;
        _storage[definition.ItemId] = definition;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteItemDefinitionAsync(string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        var removed = _storage.TryRemove(itemId, out _);
        return Task.FromResult(removed);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<MappingItemDefinition>> ListItemDefinitionsAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);

        var items = _storage.Values
            .Where(d => d.WorkspaceId == workspaceId)
            .OrderBy(d => d.DisplayName)
            .ToList();

        return Task.FromResult<IEnumerable<MappingItemDefinition>>(items);
    }

    /// <inheritdoc/>
    public Task<bool> ItemExistsAsync(string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        var exists = _storage.ContainsKey(itemId);
        return Task.FromResult(exists);
    }
}
