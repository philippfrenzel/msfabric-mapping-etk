# Multi-Agent Workflow System

## Overview

The Multi-Agent Workflow System provides a sophisticated way to handle complex data processing requests by breaking them down into specialized tasks and distributing them across different agent types. The system uses an **Architect Agent** to analyze requirements and create execution plans, then distributes jobs to specialized **Worker Agents** for execution.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Agent Workflow System                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │  Submit Agent Request   │
              │  (Title + Description)  │
              └─────────────────────────┘
                            │
                            ▼
         ┌──────────────────────────────────┐
         │   Architect Agent                │
         │   - Analyzes requirements        │
         │   - Identifies agent types       │
         │   - Estimates complexity         │
         │   - Creates execution plan       │
         └──────────────────────────────────┘
                            │
                            ▼
         ┌──────────────────────────────────┐
         │   Job Creation                   │
         │   - Break down into tasks        │
         │   - Assign to agent types        │
         │   - Set priorities               │
         │   - Define dependencies          │
         └──────────────────────────────────┘
                            │
                            ▼
    ┌──────────────────────────────────────────┐
    │        Worker Agents (Parallel)          │
    ├──────────────────────────────────────────┤
    │  • DataMapper                            │
    │  • ReferenceTableManager                 │
    │  • Validator                             │
    │  • Integrator                            │
    │  • Analyst                               │
    └──────────────────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │  Request Complete       │
              │  All jobs finished      │
              └─────────────────────────┘
```

## Agent Types

### Architect Agent

The **Architect Agent** (`ArchitectAgentService`) is responsible for:

- **Requirements Analysis**: Analyzes incoming requests to identify what needs to be done
- **Agent Selection**: Determines which agent types are needed
- **Complexity Assessment**: Evaluates the complexity level (Low, Medium, High, Critical)
- **Time Estimation**: Estimates how long the work will take
- **Risk Identification**: Identifies potential risks and challenges
- **Job Planning**: Creates a detailed execution plan with dependencies

### Worker Agents

Worker agents execute specific types of tasks:

1. **DataMapper**: Handles data transformation and mapping operations
2. **ReferenceTableManager**: Creates and manages reference tables for data classification
3. **Validator**: Validates data against business rules and constraints
4. **Integrator**: Integrates with external systems and APIs
5. **Analyst**: Performs data analysis and generates insights

## API Endpoints

### Submit a New Request

**POST** `/api/agent-workflow/requests`

Submit a new agent request to the workflow system.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/requests \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Customer Data Integration",
    "description": "Transform customer data from legacy system, validate it, and integrate with the new platform",
    "priority": "High",
    "metadata": {
      "source": "LegacyDB",
      "target": "NewPlatform"
    }
  }'
```

**Response:**
```json
{
  "requestId": "req-12345",
  "title": "Customer Data Integration",
  "status": "Pending",
  "priority": "High",
  "createdAt": "2024-01-15T10:00:00Z",
  "hasAnalysis": false,
  "jobsCount": 0
}
```

### Get Request Status

**GET** `/api/agent-workflow/requests/{requestId}`

Get the current status and details of a request.

```bash
curl https://localhost:5001/api/agent-workflow/requests/req-12345
```

**Response:**
```json
{
  "requestId": "req-12345",
  "title": "Customer Data Integration",
  "status": "InProgress",
  "priority": "High",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-01-15T10:05:00Z",
  "hasAnalysis": true,
  "jobsCount": 3,
  "completedJobs": 1,
  "pendingJobs": 2,
  "failedJobs": 0,
  "analysis": {
    "summary": "Complex integration requiring multiple agents",
    "complexity": "High",
    "estimatedMinutes": 90,
    "recommendedAgents": ["DataMapper", "Validator", "Integrator"]
  },
  "jobs": [...]
}
```

### Analyze Requirements

**POST** `/api/agent-workflow/requests/{requestId}/analyze`

Trigger the architect agent to analyze requirements.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/requests/req-12345/analyze
```

**Response:**
```json
{
  "analysisId": "analysis-67890",
  "summary": "Analysis of request 'Customer Data Integration'...",
  "complexity": "High",
  "estimatedMinutes": 90,
  "recommendedAgents": ["DataMapper", "Validator", "Integrator"],
  "risks": [
    "External system integration may have availability issues",
    "Multiple requirements require coordination"
  ],
  "requirements": [
    {
      "requirementId": "req-1",
      "title": "Data Mapping Requirement",
      "type": "Functional",
      "recommendedAgent": "DataMapper"
    },
    ...
  ]
}
```

### Create Jobs from Analysis

**POST** `/api/agent-workflow/requests/{requestId}/create-jobs`

Create executable jobs from the requirements analysis.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/requests/req-12345/create-jobs
```

**Response:**
```json
[
  {
    "jobId": "job-001",
    "requestId": "req-12345",
    "title": "Data Mapping Requirement",
    "agentType": "DataMapper",
    "status": "Pending",
    "priority": "High",
    "createdAt": "2024-01-15T10:05:00Z"
  },
  ...
]
```

### Process Request (End-to-End)

**POST** `/api/agent-workflow/requests/{requestId}/process`

Analyze requirements and create jobs in a single operation.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/requests/req-12345/process
```

This is equivalent to calling `/analyze` followed by `/create-jobs`.

### Get Jobs for Agent Type

**GET** `/api/agent-workflow/jobs/{agentType}`

Get all pending jobs for a specific agent type.

```bash
curl https://localhost:5001/api/agent-workflow/jobs/DataMapper
```

**Response:**
```json
[
  {
    "jobId": "job-001",
    "requestId": "req-12345",
    "title": "Data Mapping Requirement",
    "description": "Perform data mapping and transformation operations",
    "agentType": "DataMapper",
    "status": "Pending",
    "priority": "High",
    "createdAt": "2024-01-15T10:05:00Z"
  },
  ...
]
```

### Execute a Job

**POST** `/api/agent-workflow/jobs/execute`

Execute a specific job using a worker agent.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/jobs/execute \
  -H "Content-Type: application/json" \
  -d '{
    "jobId": "job-001",
    "agentId": "mapper-agent-01"
  }'
```

**Response:**
```json
{
  "success": true,
  "jobId": "job-001",
  "agentId": "mapper-agent-01",
  "executionTimeMs": 1234,
  "data": {
    "jobType": "DataMapping",
    "status": "Completed",
    "message": "Data mapping operation completed successfully"
  }
}
```

### Cancel a Request

**POST** `/api/agent-workflow/requests/{requestId}/cancel`

Cancel a request and all its pending jobs.

```bash
curl -X POST https://localhost:5001/api/agent-workflow/requests/req-12345/cancel
```

## Usage Examples

### Example 1: Simple Data Mapping Request

```bash
# 1. Submit request
REQUEST_ID=$(curl -X POST https://localhost:5001/api/agent-workflow/requests \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Map Customer Names",
    "description": "Transform customer names from legacy format",
    "priority": "Medium"
  }' | jq -r '.requestId')

# 2. Process request (analyze + create jobs)
curl -X POST https://localhost:5001/api/agent-workflow/requests/$REQUEST_ID/process

# 3. Get jobs for DataMapper agent
curl https://localhost:5001/api/agent-workflow/jobs/DataMapper

# 4. Execute the job
curl -X POST https://localhost:5001/api/agent-workflow/jobs/execute \
  -H "Content-Type: application/json" \
  -d '{
    "jobId": "job-xxx"
  }'

# 5. Check request status
curl https://localhost:5001/api/agent-workflow/requests/$REQUEST_ID
```

### Example 2: Complex Multi-Agent Request

```bash
# Submit a complex request requiring multiple agents
REQUEST_ID=$(curl -X POST https://localhost:5001/api/agent-workflow/requests \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Customer Data Processing Pipeline",
    "description": "Create reference tables for product types, map customer data from legacy system, validate business rules, integrate with external CRM, and analyze customer segments",
    "priority": "High"
  }' | jq -r '.requestId')

# Process the request
curl -X POST https://localhost:5001/api/agent-workflow/requests/$REQUEST_ID/process

# The architect agent will identify multiple requirements:
# - ReferenceTableManager: Create product reference tables
# - DataMapper: Map legacy customer data
# - Validator: Validate business rules
# - Integrator: Integrate with CRM
# - Analyst: Analyze customer segments

# Workers can now pick up jobs based on their type
curl https://localhost:5001/api/agent-workflow/jobs/ReferenceTableManager
curl https://localhost:5001/api/agent-workflow/jobs/DataMapper
curl https://localhost:5001/api/agent-workflow/jobs/Validator
# ... and so on
```

## Integration with Workload API

The agent workflow can also be invoked through the Fabric Workload API:

```bash
# Submit an agent request via workload API
curl -X POST https://localhost:5001/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "SubmitAgentRequest",
    "parameters": {
      "title": "Data Processing Task",
      "description": "Process customer data",
      "priority": "High"
    }
  }'

# Analyze requirements via workload API
curl -X POST https://localhost:5001/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "AnalyzeRequirements",
    "parameters": {
      "requestId": "req-12345"
    }
  }'

# Get jobs for agent via workload API
curl -X POST https://localhost:5001/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "GetJobsForAgent",
    "parameters": {
      "agentType": "DataMapper"
    }
  }'

# Execute a job via workload API
curl -X POST https://localhost:5001/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "ExecuteAgentJob",
    "parameters": {
      "jobId": "job-001",
      "agentId": "agent-001"
    }
  }'
```

## Request Lifecycle

1. **Pending** - Request submitted, awaiting analysis
2. **AnalyzingRequirements** - Architect agent is analyzing
3. **CreatingJobs** - Jobs are being created from analysis
4. **InProgress** - Jobs have been distributed to agents
5. **Completed** - All jobs completed successfully
6. **Failed** - One or more jobs failed
7. **Cancelled** - Request was cancelled by user

## Job Lifecycle

1. **Pending** - Job created, awaiting assignment
2. **Assigned** - Job assigned to a specific agent
3. **InProgress** - Job is being executed
4. **Completed** - Job completed successfully
5. **Failed** - Job execution failed
6. **Cancelled** - Job was cancelled

## Best Practices

### Writing Effective Request Descriptions

The architect agent uses the request description to identify requirements. For best results:

- **Be specific**: Include keywords like "map", "validate", "integrate", "analyze"
- **Mention systems**: Reference specific systems or data sources
- **Include objectives**: Clearly state what you want to achieve

**Good example:**
```
"Transform customer data from LegacyDB, validate email addresses and phone numbers, 
create reference tables for product categories, integrate with Salesforce CRM, 
and analyze purchase patterns"
```

**Poor example:**
```
"Do something with customer data"
```

### Monitoring Request Progress

Always check the request status after submitting:

```bash
# Monitor progress
watch -n 5 'curl -s https://localhost:5001/api/agent-workflow/requests/$REQUEST_ID | jq'
```

### Handling Failures

If jobs fail, check the error message in the job result:

```bash
# Get detailed job info
curl https://localhost:5001/api/agent-workflow/requests/$REQUEST_ID | \
  jq '.jobs[] | select(.status == "Failed")'
```

### Customizing Agent Behavior

Worker agents can be customized by passing parameters in the request metadata:

```json
{
  "title": "Custom Mapping",
  "description": "Map data with custom rules",
  "metadata": {
    "mappingRules": "...",
    "validationLevel": "strict",
    "batchSize": 1000
  }
}
```

## Error Handling

All API endpoints return standard HTTP status codes:

- **200 OK** - Request successful
- **400 Bad Request** - Invalid input
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

Error responses include detailed messages:

```json
{
  "error": "Request req-xxx not found",
  "details": "..."
}
```

## Performance Considerations

- **In-Memory Storage**: The default implementation uses in-memory storage. For production, implement persistent storage.
- **Parallel Execution**: Worker agents can execute jobs in parallel
- **Job Batching**: Consider batching similar jobs for efficiency
- **Timeout Management**: Set appropriate timeouts in workload configurations

## See Also

- [Workload Guide](WORKLOAD_GUIDE.md) - Complete workload deployment guide
- [API Documentation](API.md) - Detailed API reference
- [Fabric Integration](FABRIC-INTEGRATION.md) - Microsoft Fabric integration
