namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to submit a new agent request.
/// </summary>
public class SubmitAgentRequestDto
{
    /// <summary>
    /// Gets or sets the title of the request.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of what needs to be accomplished.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority level (Low, Medium, High, Critical).
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Gets or sets additional metadata for the request.
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; set; }
}

/// <summary>
/// Response for a submitted agent request.
/// </summary>
public class AgentRequestResponseDto
{
    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the request was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the request has been analyzed.
    /// </summary>
    public bool HasAnalysis { get; set; }

    /// <summary>
    /// Gets or sets the total number of jobs.
    /// </summary>
    public int JobsCount { get; set; }

    /// <summary>
    /// Gets or sets the number of completed jobs.
    /// </summary>
    public int CompletedJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of pending jobs.
    /// </summary>
    public int PendingJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of failed jobs.
    /// </summary>
    public int FailedJobs { get; set; }

    /// <summary>
    /// Gets or sets the requirements analysis (if available).
    /// </summary>
    public RequirementsAnalysisDto? Analysis { get; set; }

    /// <summary>
    /// Gets or sets the jobs (if available).
    /// </summary>
    public List<AgentJobDto>? Jobs { get; set; }
}

/// <summary>
/// Requirements analysis DTO.
/// </summary>
public class RequirementsAnalysisDto
{
    /// <summary>
    /// Gets or sets the analysis ID.
    /// </summary>
    public string AnalysisId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the complexity level.
    /// </summary>
    public string Complexity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated time in minutes.
    /// </summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the recommended agent types.
    /// </summary>
    public List<string> RecommendedAgents { get; set; } = new();

    /// <summary>
    /// Gets or sets the identified risks.
    /// </summary>
    public List<string> Risks { get; set; } = new();

    /// <summary>
    /// Gets or sets the requirements.
    /// </summary>
    public List<RequirementDto> Requirements { get; set; } = new();
}

/// <summary>
/// Requirement DTO.
/// </summary>
public class RequirementDto
{
    /// <summary>
    /// Gets or sets the requirement ID.
    /// </summary>
    public string RequirementId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requirement type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended agent type.
    /// </summary>
    public string RecommendedAgent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the acceptance criteria.
    /// </summary>
    public List<string> AcceptanceCriteria { get; set; } = new();
}

/// <summary>
/// Agent job DTO.
/// </summary>
public class AgentJobDto
{
    /// <summary>
    /// Gets or sets the job ID.
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent type.
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the job was completed (if applicable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the agent ID executing the job (if assigned).
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the execution result (if completed).
    /// </summary>
    public AgentJobResultDto? Result { get; set; }
}

/// <summary>
/// Agent job result DTO.
/// </summary>
public class AgentJobResultDto
{
    /// <summary>
    /// Gets or sets whether the job was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the output data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Request to execute an agent job.
/// </summary>
public class ExecuteJobRequestDto
{
    /// <summary>
    /// Gets or sets the job ID to execute.
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent ID executing the job (optional, will be auto-generated if not provided).
    /// </summary>
    public string? AgentId { get; set; }
}
