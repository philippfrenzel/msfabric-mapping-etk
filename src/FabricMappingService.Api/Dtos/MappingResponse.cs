namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response DTO for mapping operations.
/// </summary>
public class MappingResponse
{
    /// <summary>
    /// Indicates whether the mapping was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The mapped result data (JSON object).
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Any errors that occurred during mapping.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Any warnings generated during mapping.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Number of properties successfully mapped.
    /// </summary>
    public int? MappedPropertiesCount { get; set; }

    /// <summary>
    /// Additional metadata about the mapping operation.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
