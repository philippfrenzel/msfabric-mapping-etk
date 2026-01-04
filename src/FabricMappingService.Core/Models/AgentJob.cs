namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a job assigned to a worker agent.
/// </summary>
public class AgentJob
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the ID of the request that spawned this job.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the job.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the job.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of agent required for this job.
    /// </summary>
    public AgentType AgentType { get; set; }

    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public AgentJobStatus Status { get; set; } = AgentJobStatus.Pending;

    /// <summary>
    /// Gets or sets the priority inherited from the request.
    /// </summary>
    public AgentRequestPriority Priority { get; set; } = AgentRequestPriority.Medium;

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the job was assigned to an agent.
    /// </summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>
    /// Gets or sets when the job was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the agent executing this job.
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the result of the job execution.
    /// </summary>
    public AgentJobResult? Result { get; set; }

    /// <summary>
    /// Gets or sets additional parameters for the job.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

/// <summary>
/// Defines the types of agents available in the system.
/// </summary>
public enum AgentType
{
    /// <summary>
    /// Architect agent - analyzes requirements and creates execution plans.
    /// </summary>
    Architect,

    /// <summary>
    /// Data mapping agent - handles data transformation and mapping tasks.
    /// </summary>
    DataMapper,

    /// <summary>
    /// Reference table agent - manages reference table operations.
    /// </summary>
    ReferenceTableManager,

    /// <summary>
    /// Validation agent - validates data and configurations.
    /// </summary>
    Validator,

    /// <summary>
    /// Integration agent - handles integration with external systems.
    /// </summary>
    Integrator,

    /// <summary>
    /// Analytics agent - performs data analysis and reporting.
    /// </summary>
    Analyst
}

/// <summary>
/// Defines the status of an agent job.
/// </summary>
public enum AgentJobStatus
{
    /// <summary>
    /// Job has been created but not yet assigned.
    /// </summary>
    Pending,

    /// <summary>
    /// Job has been assigned to an agent.
    /// </summary>
    Assigned,

    /// <summary>
    /// Job is currently being executed.
    /// </summary>
    InProgress,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Job failed during execution.
    /// </summary>
    Failed,

    /// <summary>
    /// Job was cancelled.
    /// </summary>
    Cancelled
}

/// <summary>
/// Represents the result of a job execution.
/// </summary>
public class AgentJobResult
{
    /// <summary>
    /// Gets or sets whether the job was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the output data from the job.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the execution.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();
}
