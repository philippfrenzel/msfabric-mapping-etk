namespace FabricMappingService.Core.Workload;

/// <summary>
/// Configuration for workload execution.
/// </summary>
public class WorkloadConfiguration
{
    /// <summary>
    /// Gets or sets the operation type to execute.
    /// </summary>
    public WorkloadOperationType OperationType { get; set; }

    /// <summary>
    /// Gets or sets the workspace ID.
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// Gets or sets additional parameters for the operation.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout for the operation in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Defines the types of operations that can be executed by the workload.
/// </summary>
public enum WorkloadOperationType
{
    /// <summary>
    /// Create a new reference table.
    /// </summary>
    CreateReferenceTable,

    /// <summary>
    /// Sync reference table with source data.
    /// </summary>
    SyncReferenceTable,

    /// <summary>
    /// Read reference table data.
    /// </summary>
    ReadReferenceTable,

    /// <summary>
    /// Update reference table row.
    /// </summary>
    UpdateReferenceTableRow,

    /// <summary>
    /// Delete reference table.
    /// </summary>
    DeleteReferenceTable,

    /// <summary>
    /// Execute data mapping operation.
    /// </summary>
    ExecuteMapping,

    /// <summary>
    /// Validate mapping configuration.
    /// </summary>
    ValidateMapping,

    /// <summary>
    /// Health check operation.
    /// </summary>
    HealthCheck,

    /// <summary>
    /// Create a new mapping item in a Fabric workspace.
    /// </summary>
    CreateMappingItem,

    /// <summary>
    /// Update an existing mapping item.
    /// </summary>
    UpdateMappingItem,

    /// <summary>
    /// Delete a mapping item.
    /// </summary>
    DeleteMappingItem,

    /// <summary>
    /// Store mapping data to OneLake.
    /// </summary>
    StoreToOneLake,

    /// <summary>
    /// Read mapping data from OneLake.
    /// </summary>
    ReadFromOneLake,

    /// <summary>
    /// Submit a new agent request to the multi-agent workflow.
    /// </summary>
    SubmitAgentRequest,

    /// <summary>
    /// Analyze requirements for an agent request (architect agent).
    /// </summary>
    AnalyzeRequirements,

    /// <summary>
    /// Create jobs from a requirements analysis.
    /// </summary>
    CreateJobsFromAnalysis,

    /// <summary>
    /// Get the status of an agent request.
    /// </summary>
    GetAgentRequestStatus,

    /// <summary>
    /// Get jobs for a specific agent type.
    /// </summary>
    GetJobsForAgent,

    /// <summary>
    /// Execute a specific agent job.
    /// </summary>
    ExecuteAgentJob,

    /// <summary>
    /// Cancel an agent request and its jobs.
    /// </summary>
    CancelAgentRequest
}
