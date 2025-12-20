namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request DTO for batch mapping operations.
/// </summary>
public class BatchMappingRequest
{
    /// <summary>
    /// Collection of source data items to map.
    /// </summary>
    public List<object> SourceDataItems { get; set; } = new();

    /// <summary>
    /// The fully qualified name of the source type.
    /// </summary>
    public string SourceTypeName { get; set; } = string.Empty;

    /// <summary>
    /// The fully qualified name of the target type.
    /// </summary>
    public string TargetTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Optional mapping profile name to use.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Whether to stop processing on first error.
    /// </summary>
    public bool StopOnError { get; set; } = false;
}
