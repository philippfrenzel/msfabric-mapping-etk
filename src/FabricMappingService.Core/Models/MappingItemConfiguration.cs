namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents the configuration for a mapping item.
/// Defines which lakehouse, table, and attribute will be used for lookup operations.
/// </summary>
public class MappingItemConfiguration
{
    /// <summary>
    /// Gets or sets the lakehouse item ID that contains the table with the attribute for lookup.
    /// </summary>
    public string LakehouseItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workspace ID where the lakehouse is located.
    /// </summary>
    public string LakehouseWorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the table in the lakehouse that will be used for lookup.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attribute/column name that will be used as the reference for lookup.
    /// </summary>
    public string ReferenceAttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of columns that will be mapped to the reference attribute.
    /// This represents a one-to-many mapping where multiple columns can map to the same reference.
    /// </summary>
    public List<MappingColumn> MappingColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the OneLake link to the source table.
    /// </summary>
    public string? OneLakeLink { get; set; }
}
