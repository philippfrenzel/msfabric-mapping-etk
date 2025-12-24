namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents the definition of a mapping item in a Fabric workspace.
/// This follows the Fabric Extensibility Toolkit pattern for storing item definitions.
/// </summary>
public class MappingItemDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier of the mapping item.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the mapping item.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the mapping item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the workspace ID where the item is located.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the item was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the configuration for this mapping item.
    /// </summary>
    public MappingItemConfiguration Configuration { get; set; } = new();
}
