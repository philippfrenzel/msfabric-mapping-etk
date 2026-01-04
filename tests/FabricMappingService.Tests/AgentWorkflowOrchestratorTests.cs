using Xunit;
using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

namespace FabricMappingService.Tests;

/// <summary>
/// Tests for the AgentWorkflowOrchestrator class.
/// </summary>
public class AgentWorkflowOrchestratorTests
{
    private readonly AgentWorkflowOrchestrator _orchestrator;
    private readonly IAgentRequestStorage _storage;

    public AgentWorkflowOrchestratorTests()
    {
        _storage = new InMemoryAgentRequestStorage();
        var architectAgent = new ArchitectAgentService();
        var referenceMappingStorage = new InMemoryReferenceMappingStorage();
        var mappingIO = new MappingIO(referenceMappingStorage);
        var mappingConfig = new MappingConfiguration();
        var attributeMappingService = new AttributeMappingService(mappingConfig);
        var workerAgent = new WorkerAgentService(mappingIO, attributeMappingService);
        
        _orchestrator = new AgentWorkflowOrchestrator(_storage, architectAgent, workerAgent);
    }

    [Fact]
    public async Task SubmitRequestAsyncCreatesRequestWithId()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Test Request",
            Description = "Test Description",
            Priority = AgentRequestPriority.Medium
        };

        // Act
        var submittedRequest = await _orchestrator.SubmitRequestAsync(request);

        // Assert
        Assert.NotNull(submittedRequest);
        Assert.NotEmpty(submittedRequest.RequestId);
        Assert.Equal(AgentRequestStatus.Pending, submittedRequest.Status);
        Assert.Equal(request.Title, submittedRequest.Title);
    }

    [Fact]
    public async Task SubmitRequestAsyncStoresRequest()
    {
        // Arrange
        var request = new AgentRequest
        {
            Title = "Test",
            Description = "Test"
        };

        // Act
        var submitted = await _orchestrator.SubmitRequestAsync(request);
        var retrieved = await _storage.GetRequestAsync(submitted.RequestId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(submitted.RequestId, retrieved.RequestId);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncUpdatesRequestStatus()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Map data and validate it"
        });

        // Act
        await _orchestrator.AnalyzeRequirementsAsync(request.RequestId);
        var updated = await _storage.GetRequestAsync(request.RequestId);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(AgentRequestStatus.CreatingJobs, updated.Status);
        Assert.NotNull(updated.Analysis);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncPerformsAnalysis()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Data Processing",
            Description = "Transform customer data from legacy format"
        });

        // Act
        var analysis = await _orchestrator.AnalyzeRequirementsAsync(request.RequestId);

        // Assert
        Assert.NotNull(analysis);
        Assert.Equal(request.RequestId, analysis.RequestId);
        Assert.NotEmpty(analysis.Requirements);
        Assert.NotEmpty(analysis.RecommendedAgents);
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncThrowsForNonexistentRequest()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orchestrator.AnalyzeRequirementsAsync("nonexistent-id"));
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncCreatesJobs()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Map and validate data"
        });
        await _orchestrator.AnalyzeRequirementsAsync(request.RequestId);

        // Act
        var jobs = await _orchestrator.CreateJobsFromAnalysisAsync(request.RequestId);

        // Assert
        Assert.NotEmpty(jobs);
        Assert.All(jobs, job => Assert.Equal(request.RequestId, job.RequestId));
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncUpdatesRequestStatus()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Test task"
        });
        await _orchestrator.AnalyzeRequirementsAsync(request.RequestId);

        // Act
        await _orchestrator.CreateJobsFromAnalysisAsync(request.RequestId);
        var updated = await _storage.GetRequestAsync(request.RequestId);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(AgentRequestStatus.InProgress, updated.Status);
        Assert.NotEmpty(updated.Jobs);
    }

    [Fact]
    public async Task CreateJobsFromAnalysisAsyncThrowsWhenNotAnalyzed()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Test"
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orchestrator.CreateJobsFromAnalysisAsync(request.RequestId));
    }

    [Fact]
    public async Task ProcessRequestAsyncPerformsEndToEndProcessing()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Complete Process",
            Description = "Map customer data and validate it"
        });

        // Act
        var processedRequest = await _orchestrator.ProcessRequestAsync(request.RequestId);

        // Assert
        Assert.NotNull(processedRequest);
        Assert.NotNull(processedRequest.Analysis);
        Assert.NotEmpty(processedRequest.Jobs);
        Assert.Equal(AgentRequestStatus.InProgress, processedRequest.Status);
    }

    [Fact]
    public async Task GetJobsForAgentAsyncReturnsFilteredJobs()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Map data"
        });
        await _orchestrator.ProcessRequestAsync(request.RequestId);

        // Act
        var mapperJobs = await _orchestrator.GetJobsForAgentAsync(AgentType.DataMapper);

        // Assert
        Assert.NotEmpty(mapperJobs);
        Assert.All(mapperJobs, job => Assert.Equal(AgentType.DataMapper, job.AgentType));
    }

    [Fact]
    public async Task ExecuteJobAsyncUpdatesJobStatus()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Simple task"
        });
        await _orchestrator.ProcessRequestAsync(request.RequestId);
        var jobs = await _orchestrator.GetJobsForAgentAsync(AgentType.DataMapper);
        var jobToExecute = jobs.First();

        // Act
        var result = await _orchestrator.ExecuteJobAsync(jobToExecute.JobId, "agent-001");

        // Assert
        Assert.NotNull(result);
        
        var updatedJob = await _storage.GetJobAsync(jobToExecute.JobId);
        Assert.NotNull(updatedJob);
        Assert.Equal(AgentJobStatus.Completed, updatedJob.Status);
        Assert.NotNull(updatedJob.CompletedAt);
    }

    [Fact]
    public async Task ExecuteJobAsyncReturnsResult()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Test"
        });
        var processedRequest = await _orchestrator.ProcessRequestAsync(request.RequestId);
        var job = processedRequest.Jobs.First();

        // Act
        var result = await _orchestrator.ExecuteJobAsync(job.JobId, "agent-test");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ExecutionTimeMs >= 0);
    }

    [Fact]
    public async Task ExecuteJobAsyncUpdatesRequestWhenAllJobsComplete()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Simple Task",
            Description = "Do one thing"
        });
        await _orchestrator.ProcessRequestAsync(request.RequestId);
        
        // Execute all jobs
        var allJobs = request.Jobs.ToList();
        foreach (var job in allJobs)
        {
            await _orchestrator.ExecuteJobAsync(job.JobId, "agent-test");
        }

        // Act
        var finalRequest = await _storage.GetRequestAsync(request.RequestId);

        // Assert
        Assert.NotNull(finalRequest);
        Assert.Equal(AgentRequestStatus.Completed, finalRequest.Status);
    }

    [Fact]
    public async Task GetRequestStatusAsyncReturnsCurrentStatus()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Test"
        });

        // Act
        var status = await _orchestrator.GetRequestStatusAsync(request.RequestId);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(request.RequestId, status.RequestId);
        Assert.Equal(AgentRequestStatus.Pending, status.Status);
    }

    [Fact]
    public async Task CancelRequestAsyncCancelsRequestAndJobs()
    {
        // Arrange
        var request = await _orchestrator.SubmitRequestAsync(new AgentRequest
        {
            Title = "Test",
            Description = "Test"
        });
        await _orchestrator.ProcessRequestAsync(request.RequestId);

        // Act
        await _orchestrator.CancelRequestAsync(request.RequestId);

        // Assert
        var cancelled = await _storage.GetRequestAsync(request.RequestId);
        Assert.NotNull(cancelled);
        Assert.Equal(AgentRequestStatus.Cancelled, cancelled.Status);
        Assert.All(cancelled.Jobs, job => 
            Assert.True(job.Status is AgentJobStatus.Cancelled or AgentJobStatus.Completed));
    }

    [Fact]
    public async Task CancelRequestAsyncThrowsForNonexistentRequest()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orchestrator.CancelRequestAsync("nonexistent"));
    }

    [Fact]
    public async Task SubmitRequestAsyncThrowsForNullRequest()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _orchestrator.SubmitRequestAsync(null!));
    }

    [Fact]
    public async Task AnalyzeRequirementsAsyncThrowsForNullOrEmptyId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _orchestrator.AnalyzeRequirementsAsync(string.Empty));
    }

    [Fact]
    public async Task ExecuteJobAsyncThrowsForNullOrEmptyJobId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _orchestrator.ExecuteJobAsync(string.Empty, "agent"));
    }

    [Fact]
    public async Task ExecuteJobAsyncThrowsForNullOrEmptyAgentId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _orchestrator.ExecuteJobAsync("job-1", string.Empty));
    }
}
