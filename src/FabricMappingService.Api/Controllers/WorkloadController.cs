using Microsoft.AspNetCore.Mvc;
using FabricMappingService.Core.Workload;

namespace FabricMappingService.Api.Controllers;

/// <summary>
/// Controller for Microsoft Fabric Workload operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkloadController : ControllerBase
{
    private readonly IWorkload _workload;
    private readonly ILogger<WorkloadController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkloadController"/> class.
    /// </summary>
    public WorkloadController(IWorkload workload, ILogger<WorkloadController> logger)
    {
        _workload = workload;
        _logger = logger;
    }

    /// <summary>
    /// Gets workload information.
    /// </summary>
    /// <returns>Workload metadata.</returns>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            workloadId = _workload.WorkloadId,
            displayName = _workload.DisplayName,
            version = _workload.Version,
            description = "Reference Table & Data Mapping Service for Microsoft Fabric"
        });
    }

    /// <summary>
    /// Gets workload health status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health status information.</returns>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        try
        {
            var health = await _workload.GetHealthStatusAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking workload health");
            return StatusCode(500, new { error = "Failed to check health status" });
        }
    }

    /// <summary>
    /// Executes a workload operation.
    /// </summary>
    /// <param name="configuration">The workload configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution result.</returns>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute(
        [FromBody] WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            if (configuration == null)
            {
                return BadRequest(new { error = "Configuration is required" });
            }

            _logger.LogInformation(
                "Executing workload operation: {OperationType}",
                configuration.OperationType);

            var result = await _workload.ExecuteAsync(configuration, cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Workload operation was cancelled");
            return StatusCode(499, new { error = "Operation cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workload operation");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Validates workload configuration.
    /// </summary>
    /// <param name="configuration">The workload configuration to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateConfiguration(
        [FromBody] WorkloadConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            if (configuration == null)
            {
                return BadRequest(new { error = "Configuration is required" });
            }

            var validationResult = await _workload.ValidateConfigurationAsync(
                configuration,
                cancellationToken);

            return Ok(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workload configuration");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
