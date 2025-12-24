namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents the payload for creating or updating a mapping item.
/// This is used when communicating with the Fabric API.
/// </summary>
public class MappingItemPayload
{
    /// <summary>
    /// Gets or sets the display name of the item.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the definition content as a JSON string.
    /// This contains the serialized MappingItemConfiguration.
    /// </summary>
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workspace ID where the item will be created.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;
}
