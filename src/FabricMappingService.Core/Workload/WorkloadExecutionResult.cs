namespace FabricMappingService.Core.Workload;

/// <summary>
/// Result of a workload execution.
/// </summary>
public class WorkloadExecutionResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the result data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the execution duration in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the execution.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of warnings generated during execution.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Result of workload configuration validation.
/// </summary>
public class WorkloadValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the configuration is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Workload health status information.
/// </summary>
public class WorkloadHealthStatus
{
    /// <summary>
    /// Gets or sets a value indicating whether the workload is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the health status message.
    /// </summary>
    public string Status { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the workload version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets additional health check details.
    /// </summary>
    public Dictionary<string, object?> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the health check.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
