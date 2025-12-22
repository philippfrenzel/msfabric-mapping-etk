# Microsoft Fabric Integration Guide

## Overview

This guide explains how to integrate the Fabric Reference Table & Data Mapping Service with Microsoft Fabric using the Extensibility Toolkit. The primary focus is on **reference tables (KeyMapping outports)** for data classification and harmonization, with additional support for attribute-based data mapping.

## Prerequisites

- Microsoft Fabric workspace
- Azure subscription
- Microsoft Entra ID (Azure AD) tenant
- Basic understanding of Fabric workloads

## Architecture

The integration follows the Microsoft Fabric Extensibility Toolkit architecture with focus on KeyMapping outports:

```
┌─────────────────────────────────────────────────┐
│         Microsoft Fabric Portal                 │
│  ┌──────────────────────────────────────────┐  │
│  │     Workspace (with your workload)        │  │
│  │  ┌────────────────────────────────────┐  │  │
│  │  │   Reference Tables (KeyMapping)    │  │  │
│  │  │   - Master Data Classifications    │  │  │
│  │  │   - Lookup Tables                  │  │  │
│  │  │   - Code Mappings                  │  │  │
│  │  │                                     │  │  │
│  │  │   Mapping Configurations (Optional)│  │  │
│  │  │   Mapping Jobs (Optional)          │  │  │
│  │  └────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
                    │ HTTPS
                    ↓
┌─────────────────────────────────────────────────┐
│     Your Backend API (Azure App Service)        │
│  PRIMARY: Reference Table Endpoints             │
│  - /api/reference-tables (GET/POST/DELETE)      │
│  - /api/reference-tables/{name} (GET)           │
│  - /api/reference-tables/sync (POST)            │
│  - /api/reference-tables/{name}/rows (PUT)      │
│                                                  │
│  ADDITIONAL: Mapping Endpoints                  │
│  - /api/mapping/customer/legacy-to-modern       │
│  - /api/mapping/product/external-to-internal    │
│  - /api/mapping/health                          │
└─────────────────────────────────────────────────┘
                    │
                    ↓
┌─────────────────────────────────────────────────┐
│         OneLake Storage (KeyMapping)            │
│  - Reference tables as KeyMapping outports      │
│  - Classification data                          │
│  - Lookup table configurations                  │
│  - Mapping results (optional)                   │
│  - Audit logs                                   │
└─────────────────────────────────────────────────┘
```

## Step 1: Register Application in Microsoft Entra ID

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Microsoft Entra ID** → **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: Fabric Mapping Service
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Web - `https://your-api-url/signin-oidc`
5. Click **Register**
6. Note down:
   - **Application (client) ID**
   - **Directory (tenant) ID**

### Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Add these permissions:
   - `User.Read`
5. Select **APIs my organization uses** → **Power BI Service**
6. Add these permissions:
   - `Workspace.Read.All`
   - `Workspace.ReadWrite.All`
7. Click **Grant admin consent**

### Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add description and expiry
4. Click **Add**
5. **Copy the secret value immediately** (you won't see it again)

## Step 2: Deploy Backend API

### Option A: Azure App Service

> **Note**: This project is built with .NET 10. For Azure deployment, you may need to retarget to .NET 8 or 9 until .NET 10 is officially supported in Azure App Service. To retarget, update the `<TargetFramework>` in all `.csproj` files from `net10.0` to `net8.0` or `net9.0`.

1. **Create App Service**:
   ```bash
   az webapp create --resource-group YOUR_RG \
     --plan YOUR_PLAN \
     --name fabric-mapping-service \
     --runtime "DOTNET|8.0"
   ```

2. **Deploy Application**:
   ```bash
   cd src/FabricMappingService.Api
   dotnet publish -c Release
   az webapp deployment source config-zip \
     --resource-group YOUR_RG \
     --name fabric-mapping-service \
     --src publish.zip
   ```

3. **Configure App Settings**:
   ```bash
   az webapp config appsettings set \
     --resource-group YOUR_RG \
     --name fabric-mapping-service \
     --settings AzureAd__TenantId="YOUR_TENANT_ID" \
                AzureAd__ClientId="YOUR_CLIENT_ID" \
                AzureAd__ClientSecret="YOUR_CLIENT_SECRET"
   ```

### Option B: Azure Container Apps

1. **Build Container**:
   ```bash
   docker build -t fabric-mapping-service:latest .
   ```

2. **Push to Registry**:
   ```bash
   az acr login --name YOUR_REGISTRY
   docker tag fabric-mapping-service YOUR_REGISTRY.azurecr.io/fabric-mapping-service
   docker push YOUR_REGISTRY.azurecr.io/fabric-mapping-service
   ```

3. **Deploy Container App**:
   ```bash
   az containerapp create \
     --name fabric-mapping-service \
     --resource-group YOUR_RG \
     --environment YOUR_ENV \
     --image YOUR_REGISTRY.azurecr.io/fabric-mapping-service \
     --target-port 8080
   ```

## Step 3: Configure Workload Manifest

1. Open `fabric-manifest/workload-manifest.json`
2. Update these fields:

```json
{
  "authentication": {
    "aadAppId": "YOUR_APPLICATION_CLIENT_ID"
  },
  "backend": {
    "backendUrl": "https://your-api-url.azurewebsites.net"
  },
  "frontend": {
    "entryPointUrl": "https://your-frontend-url"
  }
}
```

## Step 4: Register Workload in Fabric

### Using PowerShell

```powershell
# Install Fabric PowerShell module
Install-Module -Name MicrosoftFabric

# Connect to Fabric
Connect-Fabric -TenantId "YOUR_TENANT_ID"

# Register workload
Register-FabricWorkload -ManifestPath "./fabric-manifest/workload-manifest.json"
```

### Using REST API

```bash
curl -X POST https://api.fabric.microsoft.com/v1/workloads \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d @fabric-manifest/workload-manifest.json
```

## Step 5: Enable Developer Mode (Optional)

For local development:

1. Enable Fabric developer mode in tenant settings
2. Use DevGateway for local testing
3. Configure manifest to point to localhost

```json
{
  "backend": {
    "backendUrl": "https://localhost:5001"
  }
}
```

## Step 6: Create Reference Tables (Primary Use Case)

### Create Reference Table Items (KeyMapping)

Reference tables are the primary feature and provide KeyMapping outports for data classification.

```csharp
// Example: Create a reference table for product classification
var referenceTable = new ReferenceTable
{
    Name = "produkttyp",
    DisplayName = "Product Type Classification",
    OutportType = "KeyMapping",
    Columns = new List<ReferenceTableColumn>
    {
        new() { Name = "ProductType", DataType = "string", Order = 1 },
        new() { Name = "TargetGroup", DataType = "string", Order = 2 }
    }
};

// Store in Fabric workspace as KeyMapping item
await fabricClient.CreateItemAsync(workspaceId, referenceTable);
```

### Sync Reference Table from Source Data

```csharp
// Example: Sync reference table from outport data
var products = await GetProductsFromOutport();

// Call API to sync the reference table
var syncRequest = new SyncMappingRequest
{
    MappingTableName = "produkttyp",
    KeyAttributeName = "Produkt",
    Data = products
};

var response = await httpClient.PostAsJsonAsync(
    "/api/reference-tables/sync", 
    syncRequest);
```

### Add Classification Attributes

```csharp
// Example: Add classification to reference table keys
var classifications = new Dictionary<string, Dictionary<string, object?>>
{
    ["VTP001"] = new() { ["ProductType"] = "Basic", ["TargetGroup"] = "Individual" },
    ["VTP002"] = new() { ["ProductType"] = "Premium", ["TargetGroup"] = "Individual" }
};

foreach (var (key, attributes) in classifications)
{
    await httpClient.PutAsJsonAsync(
        $"/api/reference-tables/produkttyp/rows",
        new { key, attributes });
}
```

### Consume Reference Table as KeyMapping Outport

```csharp
// Example: Read reference table and provide as KeyMapping outport
var referenceData = await httpClient.GetFromJsonAsync<ReferenceTableResponse>(
    "/api/reference-tables/produkttyp");

// The data is now available as KeyMapping outport
// Other data products can consume it for lookups
```

## Step 7: Create Mapping Configurations (Optional)

### Create Mapping Configuration

```csharp
// Example: Create a mapping configuration item
var config = new MappingConfiguration
{
    Name = "Customer Migration Config",
    SourceType = "LegacyCustomerModel",
    TargetType = "ModernCustomerModel",
    ProfileName = "CustomerMapping"
};

// Store in Fabric workspace
await fabricClient.CreateItemAsync(workspaceId, config);
```

### Execute Mapping Job

```csharp
// Example: Execute a mapping job
var job = new MappingJob
{
    ConfigurationId = configId,
    Status = "Pending",
    SourceData = legacyData
};

await fabricClient.CreateItemAsync(workspaceId, job);
```

## Step 8: Configure OneLake Integration

Store reference tables as KeyMapping outports and mapping results in OneLake:

### Store Reference Tables as KeyMapping Outports

```csharp
using Microsoft.Fabric.OneLake;

var oneLakeClient = new OneLakeClient(fabricConnection);

// Store reference table as KeyMapping outport
var referenceTable = await mappingIO.ReadMapping("produkttyp");

await oneLakeClient.WriteFileAsync(
    workspaceId,
    lakehouseId,
    "keymapping/produkttyp/mapping.json",
    JsonSerializer.Serialize(referenceTable),
    metadata: new Dictionary<string, string>
    {
        ["OutportType"] = "KeyMapping",
        ["TableName"] = "produkttyp"
    }
);
```

### Store Mapping Results (Optional)

```csharp
// Store mapping result
await oneLakeClient.WriteFileAsync(
    workspaceId,
    lakehouseId,
    "mappings/results/mapping-result.json",
    resultJson
);
```

## KeyMapping Outport Requirements

When working with reference tables as KeyMapping outports in Fabric:

1. **Outport Type**: Always use `"KeyMapping"` as the outport type for reference tables
2. **Key Column**: The key element is automatically designated as `"key"`
3. **Structure**: Reference tables follow this structure:
   ```json
   {
     "KEY001": {
       "key": "KEY001",
       "Attribute1": "Value1",
       "Attribute2": "Value2"
     }
   }
   ```
4. **Consumption**: Other data products can consume reference tables via KeyMapping outports
5. **Size Limits**: Keep reference tables under 4MB for optimal performance (Fabric Lookup activity limit)
6. **Updates**: Use the sync endpoint for incremental updates (only new keys are added)

## Common KeyMapping Patterns

### Pattern 1: Master Data Classification

```csharp
// Create master data reference table
var columns = new List<ReferenceTableColumn>
{
    new() { Name = "Category", DataType = "string" },
    new() { Name = "SubCategory", DataType = "string" },
    new() { Name = "Status", DataType = "string" }
};

mappingIO.CreateReferenceTable("master_data", columns);

// Add classifications
mappingIO.AddOrUpdateRow("master_data", "MD001", new Dictionary<string, object?>
{
    ["Category"] = "Product",
    ["SubCategory"] = "Electronics",
    ["Status"] = "Active"
});
```

### Pattern 2: Code Harmonization

```csharp
// Sync from multiple source systems
var systemACodes = GetCodesFromSystemA();
var systemBCodes = GetCodesFromSystemB();

// Create unified reference table
mappingIO.SyncMapping(systemACodes, "CodeA", "unified_codes");
mappingIO.SyncMapping(systemBCodes, "CodeB", "unified_codes");

// Add harmonized mappings
mappingIO.AddOrUpdateRow("unified_codes", "A001", new Dictionary<string, object?>
{
    ["UnifiedCode"] = "U001",
    ["Description"] = "Common Code 1",
    ["Source"] = "SystemA"
});
```

### Pattern 3: Hierarchical Classification

```csharp
// Create product hierarchy
mappingIO.AddOrUpdateRow("product_hierarchy", "P001", new Dictionary<string, object?>
{
    ["Level1"] = "Consumer Goods",
    ["Level2"] = "Electronics",
    ["Level3"] = "Mobile Devices",
    ["Level4"] = "Smartphones"
});
```

## Security Considerations

1. **Authentication**: Use Microsoft Entra ID for all API calls
2. **Authorization**: Implement workspace-level permissions
3. **Secrets**: Store secrets in Azure Key Vault
4. **Network**: Use private endpoints for production
5. **Audit**: Log all mapping operations

## Monitoring and Logging

### Application Insights

```csharp
// Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks

Fabric will monitor your `/api/mapping/health` endpoint.

## Testing Your Integration

### 1. Test Health Endpoint
```bash
curl https://your-api.azurewebsites.net/api/mapping/health
```

### 2. Test Authentication
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://your-api.azurewebsites.net/api/mapping/info
```

### 3. Test Reference Table Operations (Primary)

**Create a reference table:**
```bash
curl -X POST https://your-api.azurewebsites.net/api/reference-tables \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "test_table",
    "columns": [
      {"name": "Category", "dataType": "string", "order": 1}
    ]
  }'
```

**Sync reference table from data:**
```bash
curl -X POST https://your-api.azurewebsites.net/api/reference-tables/sync \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "mappingTableName": "test_table",
    "keyAttributeName": "Id",
    "data": [
      {"Id": "TEST001", "Name": "Test Item"}
    ]
  }'
```

**Read reference table (KeyMapping outport):**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://your-api.azurewebsites.net/api/reference-tables/test_table
```

**List all reference tables:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://your-api.azurewebsites.net/api/reference-tables
```

### 4. Test Mapping Operation (Optional)
```bash
curl -X POST https://your-api.azurewebsites.net/api/mapping/customer/legacy-to-modern \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d @test-data.json
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check AAD app registration
   - Verify token has required scopes
   - Confirm API permissions are granted

2. **404 Not Found**
   - Verify backend URL in manifest
   - Check API deployment
   - Confirm routes are correct

3. **500 Internal Server Error**
   - Check application logs
   - Verify configuration settings
   - Test API locally first

### Debugging

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Production Checklist

### Core Setup
- [ ] AAD app registered and configured
- [ ] Client secret stored securely in Key Vault
- [ ] API deployed to production environment
- [ ] SSL/TLS certificate configured
- [ ] Workload manifest updated with production URLs
- [ ] Workload registered in Fabric tenant

### Reference Tables (Primary Feature)
- [ ] Reference table endpoints tested and working
- [ ] KeyMapping outport type configured correctly
- [ ] OneLake storage configured for reference tables
- [ ] Reference table size limits validated (<4MB per table)
- [ ] Sync operations tested with sample data
- [ ] Classification attributes structure defined
- [ ] Reference table naming conventions established

### Monitoring & Operations
- [ ] Health monitoring configured
- [ ] Logging and diagnostics enabled
- [ ] Application Insights configured
- [ ] Audit logging for reference table changes
- [ ] Performance testing completed
- [ ] Reference table access patterns optimized

### Documentation & Training
- [ ] Security review completed
- [ ] Documentation updated
- [ ] User training completed for reference tables
- [ ] KeyMapping outport usage documented
- [ ] Data classification guidelines created

## Support

For issues with:
- **This project**: Open an issue on GitHub
- **Fabric integration**: See [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/)
- **Extensibility Toolkit**: See [Toolkit documentation](https://learn.microsoft.com/fabric/extensibility-toolkit/)

## References

- [Fabric Extensibility Toolkit Overview](https://learn.microsoft.com/fabric/extensibility-toolkit/overview-story)
- [Implement Fabric Backend](https://learn.microsoft.com/fabric/workload-development-kit/extensibility-back-end)
- [Fabric Authentication](https://learn.microsoft.com/fabric/extensibility-toolkit/authentication)
- [OneLake API](https://learn.microsoft.com/fabric/onelake/onelake-api-reference)
