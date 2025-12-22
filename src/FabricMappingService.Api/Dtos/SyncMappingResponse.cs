namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response from synchronizing a reference table.
/// </summary>
public class SyncMappingResponse
{
    /// <summary>
    /// Gets or sets whether the sync was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the name of the reference table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of new keys added.
    /// </summary>
    public int NewKeysAdded { get; set; }

    /// <summary>
    /// Gets or sets the total number of keys in the table.
    /// </summary>
    public int TotalKeys { get; set; }

    /// <summary>
    /// Gets or sets any error messages.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
