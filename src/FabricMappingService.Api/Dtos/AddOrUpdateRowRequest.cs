namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to add or update a row in a reference table.
/// </summary>
public class AddOrUpdateRowRequest
{
    /// <summary>
    /// Gets or sets the key value for the row.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attribute values for the row.
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = [];
}
