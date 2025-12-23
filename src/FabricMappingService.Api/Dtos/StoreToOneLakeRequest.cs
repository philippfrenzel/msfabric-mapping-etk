namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to store mapping data to OneLake.
/// </summary>
public class StoreToOneLakeRequest
{
    /// <summary>
    /// Gets or sets the item ID associated with this mapping table.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workspace ID.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping data to store.
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> Data { get; set; } = new();
}
