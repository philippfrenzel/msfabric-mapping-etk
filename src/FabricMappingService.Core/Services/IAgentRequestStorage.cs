using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Interface for storing and retrieving agent requests and jobs.
/// </summary>
public interface IAgentRequestStorage
{
    /// <summary>
    /// Stores a new agent request.
    /// </summary>
    /// <param name="request">The request to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveRequestAsync(AgentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an agent request by ID.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The agent request, or null if not found.</returns>
    Task<AgentRequest?> GetRequestAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all agent requests with optional filtering.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of agent requests.</returns>
    Task<List<AgentRequest>> ListRequestsAsync(AgentRequestStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing agent request.
    /// </summary>
    /// <param name="request">The request to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateRequestAsync(AgentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an agent request.
    /// </summary>
    /// <param name="requestId">The request ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteRequestAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs for a specific agent type.
    /// </summary>
    /// <param name="agentType">The agent type.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of agent jobs.</returns>
    Task<List<AgentJob>> GetJobsForAgentAsync(AgentType agentType, AgentJobStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific job by ID.
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The agent job, or null if not found.</returns>
    Task<AgentJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <param name="job">The job to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateJobAsync(AgentJob job, CancellationToken cancellationToken = default);
}
