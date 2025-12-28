namespace FabricMappingService.Core.Storage;

/// <summary>
/// Configuration options for lakehouse-based storage.
/// Defines the base path and folder structure for storing reference tables and mappings.
/// </summary>
public class LakehouseStorageOptions
{
    /// <summary>
    /// Gets or sets the base path for the lakehouse storage.
    /// This can be a local file path for development or an OneLake path for production.
    /// </summary>
    /// <example>
    /// Local: "/data/lakehouse"
    /// OneLake: "abfss://workspace@onelake.dfs.fabric.microsoft.com/lakehouse.Lakehouse/Files"
    /// </example>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder name for storing reference table configurations.
    /// Default is "config".
    /// </summary>
    public string ConfigFolder { get; set; } = "config";

    /// <summary>
    /// Gets or sets the folder name for storing mapping data.
    /// Default is "mappings".
    /// </summary>
    public string MappingsFolder { get; set; } = "mappings";

    /// <summary>
    /// Gets or sets the workspace ID for OneLake integration.
    /// Optional for local file storage.
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the lakehouse item ID for OneLake integration.
    /// Optional for local file storage.
    /// </summary>
    public string? LakehouseItemId { get; set; }

    /// <summary>
    /// Gets or sets whether to use pretty-printed JSON for storage.
    /// Useful for development and debugging.
    /// </summary>
    public bool PrettyPrintJson { get; set; } = true;

    /// <summary>
    /// Gets the full path to the config folder.
    /// </summary>
    public string ConfigPath => Path.Combine(BasePath, ConfigFolder);

    /// <summary>
    /// Gets the full path to the mappings folder.
    /// </summary>
    public string MappingsPath => Path.Combine(BasePath, MappingsFolder);

    /// <summary>
    /// Gets the file path for a specific reference table's configuration.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>The full path to the table's configuration file.</returns>
    public string GetTableConfigPath(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        return Path.Combine(MappingsPath, $"{SanitizeFileName(tableName)}-config.json");
    }

    /// <summary>
    /// Gets the file path for a specific reference table's data.
    /// </summary>
    /// <param name="tableName">The name of the reference table.</param>
    /// <returns>The full path to the table's data file.</returns>
    public string GetTableDataPath(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        return Path.Combine(MappingsPath, $"{SanitizeFileName(tableName)}-data.json");
    }

    /// <summary>
    /// Gets the path to the reference tables index file.
    /// </summary>
    public string ReferenceTablesIndexPath => Path.Combine(ConfigPath, "reference-tables.json");

    /// <summary>
    /// Sanitizes a table name for use as a file name.
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        return sanitized.ToLowerInvariant();
    }
}
