namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Response DTO for batch mapping operations.
/// </summary>
public class BatchMappingResponse
{
    /// <summary>
    /// Indicates whether all mappings were successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Collection of mapped results.
    /// </summary>
    public List<MappingResponse> Results { get; set; } = new();

    /// <summary>
    /// Total number of items processed.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of successfully mapped items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed mappings.
    /// </summary>
    public int FailureCount { get; set; }
}
