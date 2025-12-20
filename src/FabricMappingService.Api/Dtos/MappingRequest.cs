namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request DTO for mapping operations.
/// </summary>
public class MappingRequest
{
    /// <summary>
    /// The source data to map from (JSON object).
    /// </summary>
    public object SourceData { get; set; } = new();

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
    /// Whether to include detailed error information in the response.
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
}
