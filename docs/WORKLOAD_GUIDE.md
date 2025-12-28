# Microsoft Fabric Workload Guide - MappingWorkload

## Overview

The **MappingWorkload** is a complete implementation of a Microsoft Fabric Extensibility Toolkit Workload for reference tables (KeyMapping) and data mapping operations. This guide describes how to build, deploy, and execute the workload in Microsoft Fabric.

## Architecture

The MappingWorkload implements the `IWorkload` interface and orchestrates all mapping and reference table operations:

```
┌─────────────────────────────────────────────────┐
│           Microsoft Fabric Portal               │
│  ┌────────────────────────────────────────────┐ │
│  │         Workspace                          │ │
│  │  - Reference Tables (KeyMapping)          │ │
│  │  - Mapping Configurations                 │ │
│  │  - Mapping Jobs                           │ │
│  └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
                     ↓ HTTPS
┌─────────────────────────────────────────────────┐
│      REST API Backend (Azure App Service)       │
│  ┌────────────────────────────────────────────┐ │
│  │         WorkloadController                │ │
│  │  - /api/workload/execute                  │ │
│  │  - /api/workload/health                   │ │
│  │  - /api/workload/validate                 │ │
│  └────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────┐ │
│  │         MappingWorkload                   │ │
│  │  - ExecuteAsync()                         │ │
│  │  - ValidateConfigurationAsync()           │ │
│  │  - GetHealthStatusAsync()                 │ │
│  └────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────┐ │
│  │     Core Services                         │ │
│  │  - MappingIO (Reference Tables)           │ │
│  │  - AttributeMappingService                │ │
│  └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         OneLake Storage                         │
│  - KeyMapping Outports                          │
│  - Mapping Configurations                       │
└─────────────────────────────────────────────────┘
```

## Components

### 1. IWorkload Interface

The `IWorkload` interface defines the contract for Fabric Workloads:

```csharp
public interface IWorkload
{
    string WorkloadId { get; }
    string DisplayName { get; }
    string Version { get; }
    
    Task<WorkloadExecutionResult> ExecuteAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken);
    
    Task<WorkloadValidationResult> ValidateConfigurationAsync(
        WorkloadConfiguration configuration,
        CancellationToken cancellationToken);
    
    Task<WorkloadHealthStatus> GetHealthStatusAsync(
        CancellationToken cancellationToken);
}
```

### 2. MappingWorkload Class

The central implementation of the workload with the following main methods:

- **ExecuteAsync**: Executes workload operations (create, sync, read reference tables, etc.)
- **ValidateConfigurationAsync**: Validates workload configuration before execution
- **GetHealthStatusAsync**: Returns the health status of the workload

### 3. Supported Operations

The workload supports the following operation types:

- `CreateReferenceTable`: Create a new reference table
- `SyncReferenceTable`: Synchronize reference table with source data
- `ReadReferenceTable`: Read reference table data
- `UpdateReferenceTableRow`: Update a row in a reference table
- `DeleteReferenceTable`: Delete a reference table
- `ExecuteMapping`: Execute data mapping
- `ValidateMapping`: Validate mapping configuration
- `HealthCheck`: Perform health check
- `CreateMappingItem`: Create a new mapping item in a Fabric workspace
- `UpdateMappingItem`: Update an existing mapping item
- `DeleteMappingItem`: Delete a mapping item
- `StoreToOneLake`: Store mapping data to OneLake
- `ReadFromOneLake`: Read mapping data from OneLake

## Build Instructions

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022, VS Code, or GitHub Codespaces
- PowerShell 7 (for automation scripts)

### Building the Project

```bash
# Build the solution
cd /path/to/msfabric-mapping-etk
dotnet build

# Or use the build script
pwsh ./scripts/Build/Build.ps1
```

### Running Tests

```bash
# Run all tests
dotnet test

# With code coverage
dotnet-coverage collect -f cobertura -o coverage.xml dotnet test
```

### Publishing for Production

```bash
# Using dotnet CLI
dotnet publish -c Release -o ./publish

# Or use the publish script
pwsh ./scripts/Build/Publish.ps1 -Configuration Release -OutputPath ./publish
```

## Deployment Instructions

### Option 1: Azure App Service Deployment

#### Step 1: Create Azure Resources

```bash
# Use Azure CLI
az login

# Create Resource Group
az group create --name fabric-mapping-rg --location westeurope

# Create App Service Plan
az appservice plan create \
  --name fabric-mapping-plan \
  --resource-group fabric-mapping-rg \
  --sku B1 \
  --is-linux

# Create App Service
az webapp create \
  --name fabric-mapping-service \
  --resource-group fabric-mapping-rg \
  --plan fabric-mapping-plan \
  --runtime "DOTNETCORE:8.0"
```

**Note**: For Azure deployment, it may be necessary to switch to .NET 8.0 or 9.0 until .NET 10 is supported in Azure App Service.

#### Step 2: Deploy Application

```powershell
# Using the deploy script
.\scripts\Deploy\DeployToAzure.ps1 `
  -ResourceGroup "fabric-mapping-rg" `
  -AppServiceName "fabric-mapping-service"
```

#### Step 3: Configure App Settings

```bash
# Set environment variables (if needed)
az webapp config appsettings set \
  --resource-group fabric-mapping-rg \
  --name fabric-mapping-service \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### Option 2: Azure Container Apps

```bash
# Build container
docker build -t fabric-mapping-service:latest .

# Push to Azure Container Registry
az acr login --name youracr
docker tag fabric-mapping-service youracr.azurecr.io/fabric-mapping-service
docker push youracr.azurecr.io/fabric-mapping-service

# Create Container App
az containerapp create \
  --name fabric-mapping-service \
  --resource-group fabric-mapping-rg \
  --environment yourenv \
  --image youracr.azurecr.io/fabric-mapping-service \
  --target-port 8080 \
  --ingress external
```

## Registering Workload in Fabric

### Prerequisites for Registration

1. **Azure AD App Registration**
   - Navigate to Azure Portal → Microsoft Entra ID → App registrations
   - Create a new registration: "Fabric Mapping Service"
   - Note: Application (client) ID and Directory (tenant) ID
   - Create a Client Secret and store it securely

2. **Configure API Permissions**
   - Add the following permissions:
     - Microsoft Graph: `User.Read`
     - Power BI Service: `Workspace.Read.All`, `Workspace.ReadWrite.All`
   - Grant admin consent

3. **Update Workload Manifest**
   
   Edit `fabric-manifest/workload-manifest.json`:

   ```json
   {
     "authentication": {
       "aadAppId": "YOUR_APPLICATION_CLIENT_ID"
     },
     "backend": {
       "backendUrl": "https://your-api.azurewebsites.net"
     }
   }
   ```

### Register Workload

#### Option A: Using PowerShell Script

```powershell
# Register workload
.\scripts\Deploy\RegisterWorkload.ps1 `
  -TenantId "your-tenant-id" `
  -BackendUrl "https://fabric-mapping-service.azurewebsites.net" `
  -AadAppId "your-app-id"

# Or with REST API
.\scripts\Deploy\RegisterWorkload.ps1 `
  -TenantId "your-tenant-id" `
  -UseRestApi
```

#### Option B: Manually with Azure CLI

```bash
# Get access token
az account get-access-token --resource "https://analysis.windows.net/powerbi/api"

# Register workload (with curl)
curl -X POST https://api.fabric.microsoft.com/v1/workloads \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d @fabric-manifest/workload-manifest.json
```

## Executing Workload

### 1. Perform Health Check

```bash
# Check API health
curl https://your-api.azurewebsites.net/api/workload/health

# Response:
{
  "isHealthy": true,
  "status": "Healthy",
  "version": "1.0.0",
  "details": {
    "workloadId": "fabric-mapping-service",
    "displayName": "Reference Table & Data Mapping Service"
  }
}
```

### 2. Get Workload Information

```bash
curl https://your-api.azurewebsites.net/api/workload/info
```

### 3. Execute Workload Operations

#### Create Reference Table

```bash
curl -X POST https://your-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "CreateReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "producttype",
      "columns": "[{\"name\":\"ProductType\",\"dataType\":\"string\",\"order\":1}]",
      "isVisible": true,
      "notifyOnNewMapping": false
    }
  }'
```

#### Sync Reference Table

```bash
curl -X POST https://your-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "SyncReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "producttype",
      "keyAttributeName": "Product",
      "data": "[{\"Product\":\"VTP001\",\"Name\":\"Product A\"}]"
    }
  }'
```

#### Read Reference Table

```bash
curl -X POST https://your-api.azurewebsites.net/api/workload/execute \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "ReadReferenceTable",
    "timeoutSeconds": 30,
    "parameters": {
      "tableName": "producttype"
    }
  }'
```

### 4. Validate Configuration

```bash
curl -X POST https://your-api.azurewebsites.net/api/workload/validate \
  -H "Content-Type: application/json" \
  -d '{
    "operationType": "CreateReferenceTable",
    "timeoutSeconds": 60,
    "parameters": {
      "tableName": "test_table",
      "columns": "[]"
    }
  }'
```

## Integration in Fabric Workspace

### 1. Create or Open Workspace

1. Sign in to Microsoft Fabric
2. Navigate to your workspace or create a new one
3. The registered workload should now be available

### 2. Create Reference Table Items

In the workspace, you can now create Reference Table Items:

1. Click "New" → "Reference Table (KeyMapping)"
2. Enter the configuration
3. The reference table is provided as a KeyMapping outport

### 3. Create Mapping Configurations

1. Create Mapping Configuration Items for your data transformations
2. Configure source and target types
3. Execute mapping jobs

## Usage Examples

### Example 1: Product Type Classification

```csharp
// Configure workload
var config = new WorkloadConfiguration
{
    OperationType = WorkloadOperationType.CreateReferenceTable,
    TimeoutSeconds = 60,
    Parameters = new Dictionary<string, object?>
    {
        ["tableName"] = "producttype",
        ["columns"] = JsonSerializer.Serialize(new[]
        {
            new { name = "ProductType", dataType = "string", order = 1 },
            new { name = "TargetGroup", dataType = "string", order = 2 }
        }),
        ["isVisible"] = true
    }
};

// Execute workload
var result = await workload.ExecuteAsync(config);
```

### Example 2: Sync Data

```csharp
var syncConfig = new WorkloadConfiguration
{
    OperationType = WorkloadOperationType.SyncReferenceTable,
    TimeoutSeconds = 120,
    Parameters = new Dictionary<string, object?>
    {
        ["tableName"] = "producttype",
        ["keyAttributeName"] = "Product",
        ["data"] = JsonSerializer.Serialize(productData)
    }
};

var result = await workload.ExecuteAsync(syncConfig);
```

## Troubleshooting

### Problem: Workload not visible in Fabric

**Solution**:
1. Check workload registration
2. Ensure AAD App ID and Backend URL are correct
3. Check admin permissions for App Registration

### Problem: API calls fail

**Solution**:
1. Check Backend URL reachability
2. Check CORS configuration
3. Validate API permissions
4. Check Application Insights logs

### Problem: Health Check returns error

**Solution**:
1. Check deployment logs in Azure
2. Ensure all dependencies are correctly installed
3. Check App Service configuration

## Monitoring and Logging

### Enable Application Insights

```bash
# Enable Application Insights
az monitor app-insights component create \
  --app fabric-mapping-insights \
  --location westeurope \
  --resource-group fabric-mapping-rg

# Get Instrumentation Key
az monitor app-insights component show \
  --app fabric-mapping-insights \
  --resource-group fabric-mapping-rg \
  --query instrumentationKey

# Add to App Settings
az webapp config appsettings set \
  --resource-group fabric-mapping-rg \
  --name fabric-mapping-service \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=your-key"
```

### Log Queries

```kusto
// Show workload executions
traces
| where message contains "Executing workload operation"
| project timestamp, message, customDimensions

// Show errors
exceptions
| where timestamp > ago(1h)
| project timestamp, type, outerMessage, innermostMessage
```

## Best Practices

1. **Validation**: Always validate configuration before execution
2. **Timeouts**: Set appropriate timeouts for operations
3. **Error Handling**: Provide detailed error messages for debugging
4. **Logging**: Structured logging for all important operations
5. **Monitoring**: Monitor health checks and metrics
6. **Security**: Store secrets in Azure Key Vault
7. **Testing**: Write comprehensive unit and integration tests
8. **Documentation**: Maintain code comments and API documentation

## Additional Resources

- [Microsoft Fabric Documentation](https://learn.microsoft.com/fabric/)
- [Extensibility Toolkit Overview](https://learn.microsoft.com/fabric/extensibility-toolkit/overview)
- [Fabric Backend Implementation](https://learn.microsoft.com/fabric/workload-development-kit/extensibility-back-end)
- [OneLake API Reference](https://learn.microsoft.com/fabric/onelake/onelake-api-reference)
- [Project README](../README.md)
- [Fabric Integration Guide](./FABRIC-INTEGRATION.md)
- [API Documentation](./API.md)

## Support

For questions or issues:
- Open an issue on GitHub
- See [SUPPORT.md](../SUPPORT.md) for more contact options
- Read the [FAQ in README](../README.md)

---

**Created for the Microsoft Fabric Extensibility Toolkit Contest**
