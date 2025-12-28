namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response containing a mapping item definition.
/// </summary>
public class MappingItemResponse
{
    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the workspace ID.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lakehouse item ID.
    /// </summary>
    public string LakehouseItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lakehouse workspace ID.
    /// </summary>
    public string LakehouseWorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference attribute name.
    /// </summary>
    public string ReferenceAttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping columns.
    /// </summary>
    public List<MappingColumnDto> MappingColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the OneLake link.
    /// </summary>
    public string? OneLakeLink { get; set; }

    /// <summary>
    /// Gets or sets when the item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the item was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
