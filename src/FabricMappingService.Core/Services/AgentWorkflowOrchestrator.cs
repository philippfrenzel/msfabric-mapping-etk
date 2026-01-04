using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Orchestrates the multi-agent workflow system.
/// Coordinates architect agents for requirements analysis and worker agents for job execution.
/// </summary>
public class AgentWorkflowOrchestrator
{
    private readonly IAgentRequestStorage _storage;
    private readonly ArchitectAgentService _architectAgent;
    private readonly WorkerAgentService _workerAgent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentWorkflowOrchestrator"/> class.
    /// </summary>
    /// <param name="storage">The agent request storage.</param>
    /// <param name="architectAgent">The architect agent service.</param>
    /// <param name="workerAgent">The worker agent service.</param>
    public AgentWorkflowOrchestrator(
        IAgentRequestStorage storage,
        ArchitectAgentService architectAgent,
        WorkerAgentService workerAgent)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(architectAgent);
        ArgumentNullException.ThrowIfNull(workerAgent);

        _storage = storage;
        _architectAgent = architectAgent;
        _workerAgent = workerAgent;
    }

    /// <summary>
    /// Submits a new agent request to the workflow.
    /// </summary>
    /// <param name="request">The agent request to submit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The submitted request with assigned ID.</returns>
    public async Task<AgentRequest> SubmitRequestAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Status = AgentRequestStatus.Pending;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _storage.SaveRequestAsync(request, cancellationToken);

        return request;
    }

    /// <summary>
    /// Analyzes requirements for a pending request using the architect agent.
    /// </summary>
    /// <param name="requestId">The request ID to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requirements analysis result.</returns>
    public async Task<RequirementsAnalysis> AnalyzeRequirementsAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        var request = await _storage.GetRequestAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} not found");

        // Update status
        request.Status = AgentRequestStatus.AnalyzingRequirements;
        await _storage.UpdateRequestAsync(request, cancellationToken);

        // Perform analysis
        var analysis = await _architectAgent.AnalyzeRequirementsAsync(request, cancellationToken);

        // Update request with analysis
        request.Analysis = analysis;
        request.Status = AgentRequestStatus.CreatingJobs;
        await _storage.UpdateRequestAsync(request, cancellationToken);

        return analysis;
    }

    /// <summary>
    /// Creates jobs from a requirements analysis.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created jobs.</returns>
    public async Task<List<AgentJob>> CreateJobsFromAnalysisAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        var request = await _storage.GetRequestAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} not found");

        if (request.Analysis == null)
        {
            throw new InvalidOperationException($"Request {requestId} has not been analyzed yet");
        }

        // Create jobs from analysis
        var jobs = await _architectAgent.CreateJobsFromAnalysisAsync(request.Analysis, request, cancellationToken);

        // Add jobs to request
        request.Jobs.AddRange(jobs);
        request.Status = AgentRequestStatus.InProgress;
        await _storage.UpdateRequestAsync(request, cancellationToken);

        return jobs;
    }

    /// <summary>
    /// Processes a complete request end-to-end: analyze requirements and create jobs.
    /// </summary>
    /// <param name="requestId">The request ID to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processed request with analysis and jobs.</returns>
    public async Task<AgentRequest> ProcessRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        // Analyze requirements
        await AnalyzeRequirementsAsync(requestId, cancellationToken);

        // Create jobs
        await CreateJobsFromAnalysisAsync(requestId, cancellationToken);

        // Return updated request
        var request = await _storage.GetRequestAsync(requestId, cancellationToken);
        return request!;
    }

    /// <summary>
    /// Gets jobs available for a specific agent type.
    /// </summary>
    /// <param name="agentType">The agent type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending jobs for the agent type.</returns>
    public async Task<List<AgentJob>> GetJobsForAgentAsync(AgentType agentType, CancellationToken cancellationToken = default)
    {
        return await _storage.GetJobsForAgentAsync(agentType, AgentJobStatus.Pending, cancellationToken);
    }

    /// <summary>
    /// Executes a specific job using the appropriate worker agent.
    /// </summary>
    /// <param name="jobId">The job ID to execute.</param>
    /// <param name="agentId">The ID of the agent executing the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job result.</returns>
    public async Task<AgentJobResult> ExecuteJobAsync(string jobId, string agentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);

        var job = await _storage.GetJobAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        // Update job status
        job.Status = AgentJobStatus.InProgress;
        job.AgentId = agentId;
        job.AssignedAt = DateTime.UtcNow;
        await _storage.UpdateJobAsync(job, cancellationToken);

        // Execute the job
        var result = await _workerAgent.ExecuteJobAsync(job, cancellationToken);

        // Update job with result
        job.Result = result;
        job.Status = result.Success ? AgentJobStatus.Completed : AgentJobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        await _storage.UpdateJobAsync(job, cancellationToken);

        // Check if all jobs in the request are complete
        await UpdateRequestStatusAsync(job.RequestId, cancellationToken);

        return result;
    }

    /// <summary>
    /// Gets the status of an agent request.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The agent request with current status.</returns>
    public async Task<AgentRequest> GetRequestStatusAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        var request = await _storage.GetRequestAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} not found");

        return request;
    }

    /// <summary>
    /// Cancels an agent request and all its pending jobs.
    /// </summary>
    /// <param name="requestId">The request ID to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CancelRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        var request = await _storage.GetRequestAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} not found");

        request.Status = AgentRequestStatus.Cancelled;

        // Cancel all pending or in-progress jobs
        var jobsToCancel = request.Jobs.Where(j => j.Status is AgentJobStatus.Pending or AgentJobStatus.InProgress or AgentJobStatus.Assigned).ToList();
        foreach (var job in jobsToCancel)
        {
            job.Status = AgentJobStatus.Cancelled;
            await _storage.UpdateJobAsync(job, cancellationToken);
        }

        await _storage.UpdateRequestAsync(request, cancellationToken);
    }

    private async Task UpdateRequestStatusAsync(string requestId, CancellationToken cancellationToken)
    {
        var request = await _storage.GetRequestAsync(requestId, cancellationToken);
        if (request == null)
        {
            return;
        }

        // Check if all jobs are complete
        if (request.Jobs.All(j => j.Status is AgentJobStatus.Completed or AgentJobStatus.Failed or AgentJobStatus.Cancelled))
        {
            // Determine overall status
            if (request.Jobs.Any(j => j.Status == AgentJobStatus.Failed))
            {
                request.Status = AgentRequestStatus.Failed;
            }
            else if (request.Jobs.All(j => j.Status == AgentJobStatus.Completed))
            {
                request.Status = AgentRequestStatus.Completed;
            }
            else
            {
                request.Status = AgentRequestStatus.Cancelled;
            }

            await _storage.UpdateRequestAsync(request, cancellationToken);
        }
    }
}
