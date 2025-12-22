namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response containing reference table data.
/// </summary>
public class ReferenceTableResponse
{
    /// <summary>
    /// Gets or sets whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the name of the reference table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference table data as a dictionary of rows.
    /// Each row is keyed by the key value and contains a dictionary of attribute values.
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>>? Data { get; set; }

    /// <summary>
    /// Gets or sets any error messages.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
