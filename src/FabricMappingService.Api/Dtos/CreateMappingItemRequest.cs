namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to create a new mapping item.
/// </summary>
public class CreateMappingItemRequest
{
    /// <summary>
    /// Gets or sets the display name of the mapping item.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the mapping item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the workspace ID where the item will be created.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lakehouse item ID that contains the source table.
    /// </summary>
    public string LakehouseItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workspace ID where the lakehouse is located.
    /// </summary>
    public string LakehouseWorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the table in the lakehouse.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference attribute name that will be used for lookup.
    /// </summary>
    public string ReferenceAttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the columns that will be mapped to the reference attribute.
    /// </summary>
    public List<MappingColumnDto> MappingColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the OneLake link to the source table.
    /// </summary>
    public string? OneLakeLink { get; set; }
}
