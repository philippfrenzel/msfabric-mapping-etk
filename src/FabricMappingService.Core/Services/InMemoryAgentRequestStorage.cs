using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// In-memory implementation of agent request storage for development and testing.
/// </summary>
public class InMemoryAgentRequestStorage : IAgentRequestStorage
{
    private readonly Dictionary<string, AgentRequest> _requests = new();
    private readonly Dictionary<string, AgentJob> _jobs = new();
    private readonly object _lock = new();

    /// <inheritdoc/>
    public Task SaveRequestAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            _requests[request.RequestId] = request;
            
            // Also index all jobs
            foreach (var job in request.Jobs)
            {
                _jobs[job.JobId] = job;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<AgentRequest?> GetRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        lock (_lock)
        {
            return Task.FromResult(_requests.GetValueOrDefault(requestId));
        }
    }

    /// <inheritdoc/>
    public Task<List<AgentRequest>> ListRequestsAsync(AgentRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var requests = _requests.Values.AsEnumerable();

            if (status.HasValue)
            {
                requests = requests.Where(r => r.Status == status.Value);
            }

            return Task.FromResult(requests.OrderByDescending(r => r.CreatedAt).ToList());
        }
    }

    /// <inheritdoc/>
    public Task UpdateRequestAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            if (!_requests.ContainsKey(request.RequestId))
            {
                throw new InvalidOperationException($"Request {request.RequestId} not found");
            }

            request.UpdatedAt = DateTime.UtcNow;
            _requests[request.RequestId] = request;

            // Update job index
            foreach (var job in request.Jobs)
            {
                _jobs[job.JobId] = job;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        lock (_lock)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                // Remove all jobs
                foreach (var job in request.Jobs)
                {
                    _jobs.Remove(job.JobId);
                }

                _requests.Remove(requestId);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<List<AgentJob>> GetJobsForAgentAsync(AgentType agentType, AgentJobStatus? status = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobs.Values
                .Where(j => j.AgentType == agentType);

            if (status.HasValue)
            {
                jobs = jobs.Where(j => j.Status == status.Value);
            }

            return Task.FromResult(jobs.OrderBy(j => j.Priority).ThenBy(j => j.CreatedAt).ToList());
        }
    }

    /// <inheritdoc/>
    public Task<AgentJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);

        lock (_lock)
        {
            return Task.FromResult(_jobs.GetValueOrDefault(jobId));
        }
    }

    /// <inheritdoc/>
    public Task UpdateJobAsync(AgentJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        lock (_lock)
        {
            if (!_jobs.ContainsKey(job.JobId))
            {
                throw new InvalidOperationException($"Job {job.JobId} not found");
            }

            _jobs[job.JobId] = job;

            // Also update in the parent request
            if (_requests.TryGetValue(job.RequestId, out var request))
            {
                var existingJob = request.Jobs.FirstOrDefault(j => j.JobId == job.JobId);
                if (existingJob != null)
                {
                    var index = request.Jobs.IndexOf(existingJob);
                    request.Jobs[index] = job;
                    request.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return Task.CompletedTask;
    }
}
