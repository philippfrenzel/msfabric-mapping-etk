using System.Text;
using System.Text.Json;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Implementation of OneLake storage service.
/// In production, this should use the actual OneLake APIs.
/// For now, this is a simulated implementation using in-memory storage.
/// </summary>
public class OneLakeStorage : IOneLakeStorage
{
    private readonly Dictionary<string, string> _storage = new();
    private const string OneLakeBasePath = "https://onelake.dfs.fabric.microsoft.com";

    /// <inheritdoc/>
    public Task<string> StoreMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        Dictionary<string, Dictionary<string, object?>> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(data);

        var key = GetStorageKey(workspaceId, itemId, tableName);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _storage[key] = json;

        var oneLakePath = GetOneLakePath(workspaceId, itemId, tableName);
        return Task.FromResult(oneLakePath);
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, Dictionary<string, object?>>> ReadMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var key = GetStorageKey(workspaceId, itemId, tableName);

        if (!_storage.TryGetValue(key, out var json))
        {
            throw new InvalidOperationException($"Mapping table '{tableName}' not found in OneLake for item '{itemId}'");
        }

        var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(json);
        if (data == null)
        {
            throw new InvalidOperationException($"Failed to deserialize mapping table '{tableName}' from OneLake");
        }

        return Task.FromResult(data);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteMappingTableAsync(
        string itemId,
        string workspaceId,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var key = GetStorageKey(workspaceId, itemId, tableName);
        var removed = _storage.Remove(key);

        return Task.FromResult(removed);
    }

    /// <inheritdoc/>
    public string GetOneLakePath(string workspaceId, string itemId, string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return $"{OneLakeBasePath}/{workspaceId}/{itemId}/Tables/{tableName}";
    }

    private static string GetStorageKey(string workspaceId, string itemId, string tableName)
    {
        return $"{workspaceId}/{itemId}/{tableName}";
    }
}
