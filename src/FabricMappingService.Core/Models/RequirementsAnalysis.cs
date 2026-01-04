namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents the requirements analysis performed by the architect agent.
/// </summary>
public class RequirementsAnalysis
{
    /// <summary>
    /// Gets or sets the unique identifier for this analysis.
    /// </summary>
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the request ID this analysis is for.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the analysis was performed.
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the summary of the analysis.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of identified requirements.
    /// </summary>
    public List<Requirement> Requirements { get; set; } = new();

    /// <summary>
    /// Gets or sets the recommended agent types for fulfilling this request.
    /// </summary>
    public List<AgentType> RecommendedAgents { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated complexity of the request.
    /// </summary>
    public ComplexityLevel Complexity { get; set; } = ComplexityLevel.Medium;

    /// <summary>
    /// Gets or sets the estimated time to complete in minutes.
    /// </summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Gets or sets any risks or concerns identified.
    /// </summary>
    public List<string> Risks { get; set; } = new();

    /// <summary>
    /// Gets or sets recommended dependencies between jobs.
    /// </summary>
    public List<JobDependency> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata from the analysis.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a single requirement identified in the analysis.
/// </summary>
public class Requirement
{
    /// <summary>
    /// Gets or sets the unique identifier for this requirement.
    /// </summary>
    public string RequirementId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the title of the requirement.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of requirement.
    /// </summary>
    public RequirementType Type { get; set; }

    /// <summary>
    /// Gets or sets the priority of this requirement.
    /// </summary>
    public AgentRequestPriority Priority { get; set; } = AgentRequestPriority.Medium;

    /// <summary>
    /// Gets or sets the recommended agent type to fulfill this requirement.
    /// </summary>
    public AgentType RecommendedAgent { get; set; }

    /// <summary>
    /// Gets or sets acceptance criteria for this requirement.
    /// </summary>
    public List<string> AcceptanceCriteria { get; set; } = new();
}

/// <summary>
/// Defines types of requirements.
/// </summary>
public enum RequirementType
{
    /// <summary>
    /// Functional requirement - describes what the system should do.
    /// </summary>
    Functional,

    /// <summary>
    /// Data requirement - describes data needs.
    /// </summary>
    Data,

    /// <summary>
    /// Integration requirement - describes integration needs.
    /// </summary>
    Integration,

    /// <summary>
    /// Validation requirement - describes validation needs.
    /// </summary>
    Validation,

    /// <summary>
    /// Performance requirement - describes performance needs.
    /// </summary>
    Performance,

    /// <summary>
    /// Security requirement - describes security needs.
    /// </summary>
    Security
}

/// <summary>
/// Defines complexity levels for requirements analysis.
/// </summary>
public enum ComplexityLevel
{
    /// <summary>
    /// Simple requirement that can be completed quickly.
    /// </summary>
    Low,

    /// <summary>
    /// Moderate complexity requiring some coordination.
    /// </summary>
    Medium,

    /// <summary>
    /// Complex requirement requiring multiple agents and coordination.
    /// </summary>
    High,

    /// <summary>
    /// Very complex requirement requiring extensive planning and coordination.
    /// </summary>
    Critical
}

/// <summary>
/// Represents a dependency between two jobs.
/// </summary>
public class JobDependency
{
    /// <summary>
    /// Gets or sets the ID of the job that must complete first.
    /// </summary>
    public string PredecessorJobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the job that depends on the predecessor.
    /// </summary>
    public string SuccessorJobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of dependency.
    /// </summary>
    public DependencyType Type { get; set; } = DependencyType.FinishToStart;
}

/// <summary>
/// Defines types of dependencies between jobs.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// Successor can't start until predecessor finishes.
    /// </summary>
    FinishToStart,

    /// <summary>
    /// Successor can't finish until predecessor finishes.
    /// </summary>
    FinishToFinish,

    /// <summary>
    /// Successor can't start until predecessor starts.
    /// </summary>
    StartToStart
}
