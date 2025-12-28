namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response after storing data to OneLake.
/// </summary>
public class StoreToOneLakeResponse
{
    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the OneLake path where the data was stored.
    /// </summary>
    public string OneLakePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of rows stored.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets any error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
