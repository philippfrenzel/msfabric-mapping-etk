using Xunit;
using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

namespace FabricMappingService.Tests;

/// <summary>
/// Tests for the ArchitectAgentService class.
/// </summary>
public class ArchitectAgentServiceTests
{
    private readonly ArchitectAgentService _service;

    public ArchitectAgentServiceTests()
    {
        _service = new ArchitectAgentService();
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncReturnsAnalysisForRequest()
    {
        // Arrange
        var request = new AgentRequest
        {
            RequestId = "test-request-1",
            Title = "Test Request",
            Description = "We need to map customer data and validate it",
            Priority = AgentRequestPriority.High
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.NotNull(analysis);
        Assert.Equal(request.RequestId, analysis.RequestId);
        Assert.NotEmpty(analysis.Summary);
        Assert.NotEmpty(analysis.Requirements);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesMappingRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Data Mapping Task",
            Description = "Transform and map data from legacy system to new format"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Contains(analysis.Requirements, r => r.RecommendedAgent == AgentType.DataMapper);
        Assert.Contains(analysis.RecommendedAgents, a => a == AgentType.DataMapper);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesReferenceTableRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Reference Table Setup",
            Description = "Create reference tables for product lookup and classification"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Contains(analysis.Requirements, r => r.RecommendedAgent == AgentType.ReferenceTableManager);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesValidationRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Data Validation",
            Description = "Validate customer data against business rules and constraints"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Contains(analysis.Requirements, r => r.RecommendedAgent == AgentType.Validator);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesIntegrationRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "System Integration",
            Description = "Integrate with external API and sync data"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Contains(analysis.Requirements, r => r.RecommendedAgent == AgentType.Integrator);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesAnalyticsRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Data Analysis",
            Description = "Analyze customer behavior patterns and generate insights"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Contains(analysis.Requirements, r => r.RecommendedAgent == AgentType.Analyst);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncCreatesGeneralRequirementForUnknownRequest()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Unknown Task",
            Description = "Do something with the data"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.Single(analysis.Requirements);
        Assert.Equal(AgentType.DataMapper, analysis.Requirements[0].RecommendedAgent);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncAssessesComplexityCorrectly()
    {
        // Arrange - Simple request
        var simpleRequest = new AgentRequest
        {
            Title = "Simple Task",
            Description = "Map customer names"
        };

        // Act
        var simpleAnalysis = await _service.AnalyzeRequirementsAsync(simpleRequest);

        // Assert
        Assert.Equal(ComplexityLevel.Low, simpleAnalysis.Complexity);

        // Arrange - Complex request
        var complexRequest = new AgentRequest
        {
            Title = "Complex Task",
            Description = "Map customer data, validate it, integrate with external system, and analyze patterns"
        };

        // Act
        var complexAnalysis = await _service.AnalyzeRequirementsAsync(complexRequest);

        // Assert
        Assert.True(complexAnalysis.Complexity >= ComplexityLevel.High);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncEstimatesTimeBasedOnComplexity()
    {
        // Arrange - Low complexity
        var simpleRequest = new AgentRequest
        {
            Title = "Simple",
            Description = "Simple task"
        };

        // Act
        var simpleAnalysis = await _service.AnalyzeRequirementsAsync(simpleRequest);

        // Assert
        Assert.True(simpleAnalysis.EstimatedMinutes > 0);
        Assert.True(simpleAnalysis.EstimatedMinutes < 60);

        // Arrange - High complexity
        var complexRequest = new AgentRequest
        {
            Title = "Complex",
            Description = "Map, validate, integrate, and analyze"
        };

        // Act
        var complexAnalysis = await _service.AnalyzeRequirementsAsync(complexRequest);

        // Assert
        Assert.True(complexAnalysis.EstimatedMinutes > simpleAnalysis.EstimatedMinutes);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncIdentifiesRisks()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Risky Integration",
            Description = "Integrate with external API, validate data, and ensure security"
        };

        // Act
        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Assert
        Assert.NotEmpty(analysis.Risks);
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncCreatesJobsForEachRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            RequestId = "req-123",
            Title = "Multi-step Task",
            Description = "Map data and validate it"
        };

        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Act
        var jobs = await _service.CreateJobsFromAnalysisAsync(analysis, request);

        // Assert
        Assert.Equal(analysis.Requirements.Count, jobs.Count);
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncCreatesJobsWithCorrectProperties()
    {
        // Arrange
        var request = new AgentRequest
        {
            RequestId = "req-456",
            Title = "Data Mapping",
            Description = "Map customer data from legacy system",
            Priority = AgentRequestPriority.High
        };

        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Act
        var jobs = await _service.CreateJobsFromAnalysisAsync(analysis, request);

        // Assert
        foreach (var job in jobs)
        {
            Assert.Equal(request.RequestId, job.RequestId);
            Assert.NotEmpty(job.JobId);
            Assert.NotEmpty(job.Title);
            Assert.NotEmpty(job.Description);
            Assert.Equal(AgentJobStatus.Pending, job.Status);
            Assert.NotNull(job.Parameters);
        }
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncInheritsPriorityFromRequirement()
    {
        // Arrange
        var request = new AgentRequest
        {
            RequestId = "req-789",
            Title = "Critical Task",
            Description = "Urgent data validation",
            Priority = AgentRequestPriority.Critical
        };

        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Act
        var jobs = await _service.CreateJobsFromAnalysisAsync(analysis, request);

        // Assert
        Assert.All(jobs, job => Assert.True(job.Priority >= AgentRequestPriority.Medium));
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncIncludesRequirementMetadata()
    {
        // Arrange
        var request = new AgentRequest
        {
            RequestId = "req-meta",
            Title = "Test",
            Description = "Test task"
        };

        var analysis = await _service.AnalyzeRequirementsAsync(request);

        // Act
        var jobs = await _service.CreateJobsFromAnalysisAsync(analysis, request);

        // Assert
        foreach (var job in jobs)
        {
            Assert.True(job.Parameters.ContainsKey("RequirementId"));
            Assert.True(job.Parameters.ContainsKey("RequirementType"));
            Assert.True(job.Parameters.ContainsKey("AcceptanceCriteria"));
        }
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncThrowsForNullRequest()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.AnalyzeRequirementsAsync(null!));
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncThrowsForNullAnalysis()
    {
        // Arrange
        var request = new AgentRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.CreateJobsFromAnalysisAsync(null!, request));
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncThrowsForNullRequest()
    {
        // Arrange
        var analysis = new RequirementsAnalysis();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.CreateJobsFromAnalysisAsync(analysis, null!));
    }
}
