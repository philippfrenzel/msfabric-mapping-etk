using Microsoft.AspNetCore.Mvc;
using FabricMappingService.Api.Dtos;
using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

namespace FabricMappingService.Api.Controllers;

/// <summary>
/// Controller for multi-agent workflow operations.
/// </summary>
[ApiController]
[Route("api/agent-workflow")]
public class AgentWorkflowController : ControllerBase
{
    private readonly AgentWorkflowOrchestrator _orchestrator;
    private readonly ILogger<AgentWorkflowController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentWorkflowController"/> class.
    /// </summary>
    public AgentWorkflowController(
        AgentWorkflowOrchestrator orchestrator,
        ILogger<AgentWorkflowController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Submits a new agent request to the multi-agent workflow.
    /// </summary>
    /// <param name="request">The agent request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The submitted request with assigned ID.</returns>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(AgentRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitRequest(
        [FromBody] SubmitAgentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Title is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Description is required" });
            }

            if (!Enum.TryParse<AgentRequestPriority>(request.Priority, true, out var priority))
            {
                return BadRequest(new { error = $"Invalid priority: {request.Priority}. Must be Low, Medium, High, or Critical" });
            }

            var agentRequest = new AgentRequest
            {
                Title = request.Title,
                Description = request.Description,
                Priority = priority,
                Metadata = request.Metadata ?? new Dictionary<string, object?>()
            };

            var submittedRequest = await _orchestrator.SubmitRequestAsync(agentRequest, cancellationToken);

            _logger.LogInformation(
                "Agent request submitted: {RequestId} - {Title}",
                submittedRequest.RequestId,
                submittedRequest.Title);

            return Ok(MapToDto(submittedRequest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting agent request");
            return StatusCode(500, new { error = "Failed to submit agent request", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of an agent request.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The request status.</returns>
    [HttpGet("requests/{requestId}")]
    [ProducesResponseType(typeof(AgentRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequestStatus(
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await _orchestrator.GetRequestStatusAsync(requestId, cancellationToken);
            return Ok(MapToDto(request, includeJobs: true));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Request {requestId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request status for {RequestId}", requestId);
            return StatusCode(500, new { error = "Failed to get request status", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes requirements for a pending request using the architect agent.
    /// </summary>
    /// <param name="requestId">The request ID to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requirements analysis result.</returns>
    [HttpPost("requests/{requestId}/analyze")]
    [ProducesResponseType(typeof(RequirementsAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeRequirements(
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Analyzing requirements for request {RequestId}", requestId);

            var analysis = await _orchestrator.AnalyzeRequirementsAsync(requestId, cancellationToken);

            return Ok(MapToDto(analysis));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Request {requestId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing requirements for {RequestId}", requestId);
            return StatusCode(500, new { error = "Failed to analyze requirements", details = ex.Message });
        }
    }

    /// <summary>
    /// Creates jobs from a requirements analysis.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created jobs.</returns>
    [HttpPost("requests/{requestId}/create-jobs")]
    [ProducesResponseType(typeof(List<AgentJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJobs(
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating jobs for request {RequestId}", requestId);

            var jobs = await _orchestrator.CreateJobsFromAnalysisAsync(requestId, cancellationToken);

            return Ok(jobs.Select(MapToDto).ToList());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Request {requestId} not found" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not been analyzed"))
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating jobs for {RequestId}", requestId);
            return StatusCode(500, new { error = "Failed to create jobs", details = ex.Message });
        }
    }

    /// <summary>
    /// Processes a request end-to-end: analyze requirements and create jobs.
    /// </summary>
    /// <param name="requestId">The request ID to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processed request with analysis and jobs.</returns>
    [HttpPost("requests/{requestId}/process")]
    [ProducesResponseType(typeof(AgentRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessRequest(
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing request {RequestId}", requestId);

            var request = await _orchestrator.ProcessRequestAsync(requestId, cancellationToken);

            return Ok(MapToDto(request, includeJobs: true));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Request {requestId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {RequestId}", requestId);
            return StatusCode(500, new { error = "Failed to process request", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets pending jobs for a specific agent type.
    /// </summary>
    /// <param name="agentType">The agent type (DataMapper, ReferenceTableManager, Validator, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending jobs for the agent type.</returns>
    [HttpGet("jobs/{agentType}")]
    [ProducesResponseType(typeof(List<AgentJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetJobsForAgent(
        string agentType,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<AgentType>(agentType, true, out var agentTypeEnum))
            {
                return BadRequest(new { error = $"Invalid agent type: {agentType}" });
            }

            _logger.LogInformation("Getting jobs for agent type {AgentType}", agentType);

            var jobs = await _orchestrator.GetJobsForAgentAsync(agentTypeEnum, cancellationToken);

            return Ok(jobs.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs for agent type {AgentType}", agentType);
            return StatusCode(500, new { error = "Failed to get jobs", details = ex.Message });
        }
    }

    /// <summary>
    /// Executes a specific job.
    /// </summary>
    /// <param name="request">The execute job request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job execution result.</returns>
    [HttpPost("jobs/execute")]
    [ProducesResponseType(typeof(AgentJobResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteJob(
        [FromBody] ExecuteJobRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.JobId))
            {
                return BadRequest(new { error = "JobId is required" });
            }

            var agentId = string.IsNullOrWhiteSpace(request.AgentId)
                ? $"agent-{Guid.NewGuid():N}"
                : request.AgentId;

            _logger.LogInformation(
                "Executing job {JobId} with agent {AgentId}",
                request.JobId,
                agentId);

            var result = await _orchestrator.ExecuteJobAsync(request.JobId, agentId, cancellationToken);

            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing job {JobId}", request.JobId);
            return StatusCode(500, new { error = "Failed to execute job", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancels an agent request and all its pending jobs.
    /// </summary>
    /// <param name="requestId">The request ID to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response.</returns>
    [HttpPost("requests/{requestId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelRequest(
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cancelling request {RequestId}", requestId);

            await _orchestrator.CancelRequestAsync(requestId, cancellationToken);

            return Ok(new { success = true, message = $"Request {requestId} cancelled successfully" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Request {requestId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling request {RequestId}", requestId);
            return StatusCode(500, new { error = "Failed to cancel request", details = ex.Message });
        }
    }

    #region Mapping Helpers

    private static AgentRequestResponseDto MapToDto(AgentRequest request, bool includeJobs = false)
    {
        return new AgentRequestResponseDto
        {
            RequestId = request.RequestId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status.ToString(),
            Priority = request.Priority.ToString(),
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            HasAnalysis = request.Analysis != null,
            JobsCount = request.Jobs.Count,
            CompletedJobs = request.Jobs.Count(j => j.Status == AgentJobStatus.Completed),
            PendingJobs = request.Jobs.Count(j => j.Status == AgentJobStatus.Pending),
            FailedJobs = request.Jobs.Count(j => j.Status == AgentJobStatus.Failed),
            Analysis = request.Analysis != null ? MapToDto(request.Analysis) : null,
            Jobs = includeJobs ? request.Jobs.Select(MapToDto).ToList() : null
        };
    }

    private static RequirementsAnalysisDto MapToDto(RequirementsAnalysis analysis)
    {
        return new RequirementsAnalysisDto
        {
            AnalysisId = analysis.AnalysisId,
            Summary = analysis.Summary,
            Complexity = analysis.Complexity.ToString(),
            EstimatedMinutes = analysis.EstimatedMinutes,
            RecommendedAgents = analysis.RecommendedAgents.Select(a => a.ToString()).ToList(),
            Risks = analysis.Risks,
            Requirements = analysis.Requirements.Select(MapToDto).ToList()
        };
    }

    private static RequirementDto MapToDto(Requirement requirement)
    {
        return new RequirementDto
        {
            RequirementId = requirement.RequirementId,
            Title = requirement.Title,
            Description = requirement.Description,
            Type = requirement.Type.ToString(),
            RecommendedAgent = requirement.RecommendedAgent.ToString(),
            AcceptanceCriteria = requirement.AcceptanceCriteria
        };
    }

    private static AgentJobDto MapToDto(AgentJob job)
    {
        return new AgentJobDto
        {
            JobId = job.JobId,
            RequestId = job.RequestId,
            Title = job.Title,
            Description = job.Description,
            AgentType = job.AgentType.ToString(),
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            AgentId = job.AgentId,
            Result = job.Result != null ? MapToDto(job.Result) : null
        };
    }

    private static AgentJobResultDto MapToDto(AgentJobResult result)
    {
        return new AgentJobResultDto
        {
            Success = result.Success,
            Data = result.Data,
            ErrorMessage = result.ErrorMessage,
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }

    #endregion
}
