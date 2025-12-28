using System.Text;
using System.Text.Json;
using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Implementation of lakehouse storage for reference tables.
/// Stores reference table configurations and data as JSON files in a lakehouse structure.
/// For production, this implementation can be extended to use Azure Data Lake Storage Gen2 APIs
/// or Microsoft Fabric REST APIs for native OneLake integration.
/// Currently uses file system storage which works for both local development and mounted lakehouse volumes.
/// </summary>
public class LakehouseStorage : ILakehouseStorage
{
    private readonly LakehouseStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the LakehouseStorage class with default options.
    /// Note: When using dependency injection, prefer injecting LakehouseStorageOptions
    /// to ensure consistent configuration across the application.
    /// This constructor is primarily for testing and standalone usage.
    /// </summary>
    public LakehouseStorage()
        : this(new LakehouseStorageOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the LakehouseStorage class with specified options.
    /// This is the preferred constructor when using dependency injection.
    /// </summary>
    /// <param name="options">The lakehouse storage options.</param>
    public LakehouseStorage(LakehouseStorageOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async Task SaveReferenceTableConfigurationAsync(
        ReferenceTable table,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(table.Name);

        var configPath = GetConfigurationPath(lakehousePath, table.Name);
        EnsureDirectoryExists(Path.GetDirectoryName(configPath)!);

        var json = JsonSerializer.Serialize(table, _jsonOptions);
        await File.WriteAllTextAsync(configPath, json, Encoding.UTF8, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ReferenceTable?> LoadReferenceTableConfigurationAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var configPath = GetConfigurationPath(lakehousePath, tableName);
        
        if (!File.Exists(configPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(configPath, cancellationToken)
            .ConfigureAwait(false);
        
        return JsonSerializer.Deserialize<ReferenceTable>(json, _jsonOptions);
    }

    /// <inheritdoc/>
    public async Task SaveReferenceTableDataAsync(
        string tableName,
        Dictionary<string, Dictionary<string, object?>> data,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var dataPath = GetDataPath(lakehousePath, tableName);
        EnsureDirectoryExists(Path.GetDirectoryName(dataPath)!);

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(dataPath, json, Encoding.UTF8, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Dictionary<string, object?>>> LoadReferenceTableDataAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var dataPath = GetDataPath(lakehousePath, tableName);

        if (!File.Exists(dataPath))
        {
            return new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
        }

        var json = await File.ReadAllTextAsync(dataPath, cancellationToken)
            .ConfigureAwait(false);

        var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(json, _jsonOptions);
        return data ?? new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteReferenceTableAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var configPath = GetConfigurationPath(lakehousePath, tableName);
        var dataPath = GetDataPath(lakehousePath, tableName);

        var configDeleted = DeleteFileIfExists(configPath);
        var dataDeleted = DeleteFileIfExists(dataPath);

        return Task.FromResult(configDeleted || dataDeleted);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> ListReferenceTableNamesAsync(
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var configDir = GetConfigurationDirectory(lakehousePath);

        if (!Directory.Exists(configDir))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var tableNames = Directory.GetFiles(configDir, "*_config.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => name?.Replace("_config", "") ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        return Task.FromResult<IEnumerable<string>>(tableNames);
    }

    /// <inheritdoc/>
    public Task<bool> TableExistsAsync(
        string tableName,
        string lakehousePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lakehousePath);

        var configPath = GetConfigurationPath(lakehousePath, tableName);
        return Task.FromResult(File.Exists(configPath));
    }

    private string GetConfigurationDirectory(string lakehousePath)
    {
        return Path.Combine(lakehousePath, _options.ConfigurationDirectory);
    }

    private string GetDataDirectory(string lakehousePath)
    {
        return Path.Combine(lakehousePath, _options.DataDirectory);
    }

    private string GetConfigurationPath(string lakehousePath, string tableName)
    {
        var configDir = GetConfigurationDirectory(lakehousePath);
        return Path.Combine(configDir, $"{tableName}_config.json");
    }

    private string GetDataPath(string lakehousePath, string tableName)
    {
        var dataDir = GetDataDirectory(lakehousePath);
        return Path.Combine(dataDir, $"{tableName}_data.json");
    }

    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static bool DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }
}
