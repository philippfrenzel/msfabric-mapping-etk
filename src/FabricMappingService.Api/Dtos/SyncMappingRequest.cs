namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to synchronize a reference table with data.
/// </summary>
public class SyncMappingRequest
{
    /// <summary>
    /// Gets or sets the name of the reference table.
    /// </summary>
    public string MappingTableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the property that contains the key values.
    /// </summary>
    public string KeyAttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data items containing the key values.
    /// </summary>
    public List<Dictionary<string, object?>> Data { get; set; } = [];
}
