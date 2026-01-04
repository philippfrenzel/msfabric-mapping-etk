using FabricMappingService.Core.Models;

namespace FabricMappingService.Core.Services;

/// <summary>
/// Architect agent service that analyzes requirements and creates execution plans.
/// </summary>
public class ArchitectAgentService
{
    /// <summary>
    /// Analyzes an agent request and produces a requirements analysis.
    /// </summary>
    /// <param name="request">The agent request to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requirements analysis result.</returns>
    public Task<RequirementsAnalysis> AnalyzeRequirementsAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var analysis = new RequirementsAnalysis
        {
            RequestId = request.RequestId,
            AnalyzedAt = DateTime.UtcNow,
            Summary = GenerateSummary(request)
        };

        // Analyze the request description to identify requirements
        var requirements = IdentifyRequirements(request);
        analysis.Requirements.AddRange(requirements);

        // Determine recommended agents based on requirements
        analysis.RecommendedAgents.AddRange(
            requirements.Select(r => r.RecommendedAgent).Distinct()
        );

        // Assess complexity
        analysis.Complexity = AssessComplexity(requirements);

        // Estimate time
        analysis.EstimatedMinutes = EstimateTime(requirements, analysis.Complexity);

        // Identify risks
        analysis.Risks.AddRange(IdentifyRisks(requirements));

        // Determine dependencies
        analysis.Dependencies.AddRange(IdentifyDependencies(requirements));

        return Task.FromResult(analysis);
    }

    /// <summary>
    /// Creates agent jobs from a requirements analysis.
    /// </summary>
    /// <param name="analysis">The requirements analysis.</param>
    /// <param name="request">The original request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created jobs.</returns>
    public Task<List<AgentJob>> CreateJobsFromAnalysisAsync(RequirementsAnalysis analysis, AgentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(request);

        var jobs = new List<AgentJob>();

        foreach (var requirement in analysis.Requirements)
        {
            var job = new AgentJob
            {
                RequestId = request.RequestId,
                Title = requirement.Title,
                Description = requirement.Description,
                AgentType = requirement.RecommendedAgent,
                Priority = requirement.Priority,
                Status = AgentJobStatus.Pending,
                Parameters = new Dictionary<string, object?>
                {
                    ["RequirementId"] = requirement.RequirementId,
                    ["RequirementType"] = requirement.Type.ToString(),
                    ["AcceptanceCriteria"] = requirement.AcceptanceCriteria
                }
            };

            jobs.Add(job);
        }

        return Task.FromResult(jobs);
    }

    private string GenerateSummary(AgentRequest request)
    {
        return $"Analysis of request '{request.Title}': {request.Description.Length} characters, Priority: {request.Priority}";
    }

    private List<Requirement> IdentifyRequirements(AgentRequest request)
    {
        var requirements = new List<Requirement>();
        var description = request.Description.ToLowerInvariant();

        // Data mapping requirements
        if (description.Contains("map") || description.Contains("transform") || description.Contains("convert"))
        {
            requirements.Add(new Requirement
            {
                Title = "Data Mapping Requirement",
                Description = "Perform data mapping and transformation operations",
                Type = RequirementType.Functional,
                Priority = request.Priority,
                RecommendedAgent = AgentType.DataMapper,
                AcceptanceCriteria = new List<string>
                {
                    "Data is correctly mapped from source to target",
                    "All transformations are applied correctly",
                    "Mapping is validated"
                }
            });
        }

        // Reference table requirements
        if (description.Contains("reference") || description.Contains("lookup") || description.Contains("table"))
        {
            requirements.Add(new Requirement
            {
                Title = "Reference Table Management",
                Description = "Create, update, or manage reference tables",
                Type = RequirementType.Data,
                Priority = request.Priority,
                RecommendedAgent = AgentType.ReferenceTableManager,
                AcceptanceCriteria = new List<string>
                {
                    "Reference table is created or updated",
                    "Data integrity is maintained",
                    "Table is accessible for lookups"
                }
            });
        }

        // Validation requirements
        if (description.Contains("validat") || description.Contains("check") || description.Contains("verify"))
        {
            requirements.Add(new Requirement
            {
                Title = "Data Validation",
                Description = "Validate data against defined rules and constraints",
                Type = RequirementType.Validation,
                Priority = request.Priority,
                RecommendedAgent = AgentType.Validator,
                AcceptanceCriteria = new List<string>
                {
                    "All validation rules are applied",
                    "Validation errors are reported",
                    "Valid data passes checks"
                }
            });
        }

        // Integration requirements
        if (description.Contains("integrat") || description.Contains("sync") || description.Contains("connect"))
        {
            requirements.Add(new Requirement
            {
                Title = "System Integration",
                Description = "Integrate with external systems or data sources",
                Type = RequirementType.Integration,
                Priority = request.Priority,
                RecommendedAgent = AgentType.Integrator,
                AcceptanceCriteria = new List<string>
                {
                    "Connection to external system is established",
                    "Data synchronization is successful",
                    "Integration errors are handled"
                }
            });
        }

        // Analytics requirements
        if (description.Contains("analy") || description.Contains("report") || description.Contains("insight"))
        {
            requirements.Add(new Requirement
            {
                Title = "Data Analysis",
                Description = "Perform analysis and generate insights",
                Type = RequirementType.Functional,
                Priority = request.Priority,
                RecommendedAgent = AgentType.Analyst,
                AcceptanceCriteria = new List<string>
                {
                    "Analysis is complete and accurate",
                    "Insights are meaningful and actionable",
                    "Reports are generated"
                }
            });
        }

        // If no specific requirements identified, create a general one
        if (requirements.Count == 0)
        {
            requirements.Add(new Requirement
            {
                Title = "General Data Processing",
                Description = request.Description,
                Type = RequirementType.Functional,
                Priority = request.Priority,
                RecommendedAgent = AgentType.DataMapper,
                AcceptanceCriteria = new List<string>
                {
                    "Request requirements are fulfilled",
                    "Data is processed correctly"
                }
            });
        }

        return requirements;
    }

    private ComplexityLevel AssessComplexity(List<Requirement> requirements)
    {
        // Base complexity on number of requirements and their types
        var count = requirements.Count;
        var hasIntegration = requirements.Any(r => r.Type == RequirementType.Integration);
        var hasSecurity = requirements.Any(r => r.Type == RequirementType.Security);

        if (count >= 4 || (hasIntegration && hasSecurity))
        {
            return ComplexityLevel.Critical;
        }

        if (count >= 3 || hasIntegration)
        {
            return ComplexityLevel.High;
        }

        if (count >= 2)
        {
            return ComplexityLevel.Medium;
        }

        return ComplexityLevel.Low;
    }

    private int EstimateTime(List<Requirement> requirements, ComplexityLevel complexity)
    {
        // Base estimate on complexity and number of requirements
        var baseMinutes = complexity switch
        {
            ComplexityLevel.Low => 15,
            ComplexityLevel.Medium => 30,
            ComplexityLevel.High => 60,
            ComplexityLevel.Critical => 120,
            _ => 30
        };

        // Add time per requirement
        return baseMinutes + (requirements.Count * 10);
    }

    private List<string> IdentifyRisks(List<Requirement> requirements)
    {
        var risks = new List<string>();

        if (requirements.Any(r => r.Type == RequirementType.Integration))
        {
            risks.Add("External system integration may have availability or latency issues");
        }

        if (requirements.Any(r => r.Type == RequirementType.Security))
        {
            risks.Add("Security requirements need careful validation to prevent vulnerabilities");
        }

        if (requirements.Count > 3)
        {
            risks.Add("Multiple requirements may require coordination and careful sequencing");
        }

        return risks;
    }

    private List<JobDependency> IdentifyDependencies(List<Requirement> requirements)
    {
        var dependencies = new List<JobDependency>();

        // Create dependencies based on requirement types
        // For example, validation should happen after data mapping
        var mappingReq = requirements.FirstOrDefault(r => r.RecommendedAgent == AgentType.DataMapper);
        var validationReq = requirements.FirstOrDefault(r => r.RecommendedAgent == AgentType.Validator);

        if (mappingReq != null && validationReq != null)
        {
            dependencies.Add(new JobDependency
            {
                PredecessorJobId = mappingReq.RequirementId,
                SuccessorJobId = validationReq.RequirementId,
                Type = DependencyType.FinishToStart
            });
        }

        // Reference tables should be created before mapping
        var refTableReq = requirements.FirstOrDefault(r => r.RecommendedAgent == AgentType.ReferenceTableManager);
        if (refTableReq != null && mappingReq != null)
        {
            dependencies.Add(new JobDependency
            {
                PredecessorJobId = refTableReq.RequirementId,
                SuccessorJobId = mappingReq.RequirementId,
                Type = DependencyType.FinishToStart
            });
        }

        return dependencies;
    }
}
