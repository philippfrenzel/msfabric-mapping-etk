namespace FabricMappingService.Core.Workload;

/// <summary>
/// Interface for Microsoft Fabric workload implementations.
/// Defines the contract for executing workload operations in the Fabric platform.
/// </summary>
public interface IWorkload
{
    /// <summary>
    /// Gets the unique identifier for this workload.
    /// </summary>
    string WorkloadId { get; }

    /// <summary>
    /// Gets the display name of the workload.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the version of the workload.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Executes the workload asynchronously with the provided configuration.
    /// </summary>
    /// <param name="configuration">The workload configuration parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the execution result.</returns>
    Task<WorkloadExecutionResult> ExecuteAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the workload configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A validation result indicating whether the configuration is valid.</returns>
    Task<WorkloadValidationResult> ValidateConfigurationAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the workload health status.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the health check operation.</returns>
    Task<WorkloadHealthStatus> GetHealthStatusAsync(
        CancellationToken cancellationToken = default);
}
