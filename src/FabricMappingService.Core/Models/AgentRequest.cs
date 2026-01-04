namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a request submitted to the multi-agent workflow system.
/// </summary>
public class AgentRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the request.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the title of the request.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of what needs to be accomplished.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority level of the request.
    /// </summary>
    public AgentRequestPriority Priority { get; set; } = AgentRequestPriority.Medium;

    /// <summary>
    /// Gets or sets the current status of the request.
    /// </summary>
    public AgentRequestStatus Status { get; set; } = AgentRequestStatus.Pending;

    /// <summary>
    /// Gets or sets when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the request was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the requirements analysis result (populated by architect agent).
    /// </summary>
    public RequirementsAnalysis? Analysis { get; set; }

    /// <summary>
    /// Gets or sets the jobs created from this request.
    /// </summary>
    public List<AgentJob> Jobs { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata for the request.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

/// <summary>
/// Defines the priority levels for agent requests.
/// </summary>
public enum AgentRequestPriority
{
    /// <summary>
    /// Low priority request.
    /// </summary>
    Low,

    /// <summary>
    /// Medium priority request (default).
    /// </summary>
    Medium,

    /// <summary>
    /// High priority request.
    /// </summary>
    High,

    /// <summary>
    /// Critical priority request - needs immediate attention.
    /// </summary>
    Critical
}

/// <summary>
/// Defines the status of an agent request.
/// </summary>
public enum AgentRequestStatus
{
    /// <summary>
    /// Request has been submitted but not yet analyzed.
    /// </summary>
    Pending,

    /// <summary>
    /// Architect agent is analyzing requirements.
    /// </summary>
    AnalyzingRequirements,

    /// <summary>
    /// Requirements analyzed, jobs are being created.
    /// </summary>
    CreatingJobs,

    /// <summary>
    /// Jobs have been distributed to worker agents.
    /// </summary>
    InProgress,

    /// <summary>
    /// All jobs have been completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Request failed or was cancelled.
    /// </summary>
    Failed,

    /// <summary>
    /// Request was cancelled by user.
    /// </summary>
    Cancelled
}
