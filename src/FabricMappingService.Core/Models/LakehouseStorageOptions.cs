namespace FabricMappingService.Core.Models;

/// <summary>
/// Configuration options for lakehouse storage.
/// Provides flexible configuration for storing reference table configurations and data in lakehouse.
/// </summary>
public class LakehouseStorageOptions
{
    /// <summary>
    /// Gets or sets the base path in the lakehouse where reference tables are stored.
    /// This can be a local path for development or an Azure Data Lake Storage Gen2 path for production.
    /// Default: "./data/lakehouse"
    /// </summary>
    public string BasePath { get; set; } = "./data/lakehouse";

    /// <summary>
    /// Gets or sets whether to use in-memory storage (for development/testing).
    /// When true, InMemoryReferenceMappingStorage is used instead of lakehouse storage.
    /// Default: false (lakehouse storage is the default)
    /// </summary>
    public bool UseInMemoryStorage { get; set; } = false;

    /// <summary>
    /// Gets or sets the workspace ID for the lakehouse (optional, for Fabric integration).
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the lakehouse item ID (optional, for Fabric integration).
    /// </summary>
    public string? LakehouseItemId { get; set; }

    /// <summary>
    /// Gets or sets the subdirectory name for storing reference table configurations.
    /// Default: "ReferenceTableConfigurations"
    /// </summary>
    public string ConfigurationDirectory { get; set; } = "ReferenceTableConfigurations";

    /// <summary>
    /// Gets or sets the subdirectory name for storing reference table data.
    /// Default: "ReferenceTableData"
    /// </summary>
    public string DataDirectory { get; set; } = "ReferenceTableData";
}
