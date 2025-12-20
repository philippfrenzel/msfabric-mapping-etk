# Microsoft Fabric Integration Guide

## Overview

This guide explains how to integrate the Fabric Data Attribute Mapping Service with Microsoft Fabric using the Extensibility Toolkit.

## Prerequisites

- Microsoft Fabric workspace
- Azure subscription
- Microsoft Entra ID (Azure AD) tenant
- Basic understanding of Fabric workloads

## Architecture

The integration follows the Microsoft Fabric Extensibility Toolkit architecture:

```
┌─────────────────────────────────────────────────┐
│         Microsoft Fabric Portal                 │
│  ┌──────────────────────────────────────────┐  │
│  │     Workspace (with your workload)        │  │
│  │  ┌────────────────────────────────────┐  │  │
│  │  │   Mapping Configuration Items      │  │  │
│  │  │   Mapping Job Items                │  │  │
│  │  └────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
                    │ HTTPS
                    ↓
┌─────────────────────────────────────────────────┐
│     Your Backend API (Azure App Service)        │
│  - /api/mapping/customer/legacy-to-modern       │
│  - /api/mapping/product/external-to-internal    │
│  - /api/mapping/health                          │
└─────────────────────────────────────────────────┘
                    │
                    ↓
┌─────────────────────────────────────────────────┐
│         OneLake Storage (Optional)              │
│  - Mapping configurations                       │
│  - Mapping results                              │
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

## Step 6: Create Workload Items

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

## Step 7: Configure OneLake Integration

Store mapping results in OneLake:

```csharp
using Microsoft.Fabric.OneLake;

var oneLakeClient = new OneLakeClient(fabricConnection);

// Store mapping result
await oneLakeClient.WriteFileAsync(
    workspaceId,
    lakehouseId,
    "mappings/results/mapping-result.json",
    resultJson
);
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

1. **Test Health Endpoint**:
   ```bash
   curl https://your-api.azurewebsites.net/api/mapping/health
   ```

2. **Test Authentication**:
   ```bash
   curl -H "Authorization: Bearer YOUR_TOKEN" \
     https://your-api.azurewebsites.net/api/mapping/info
   ```

3. **Test Mapping Operation**:
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

- [ ] AAD app registered and configured
- [ ] Client secret stored securely in Key Vault
- [ ] API deployed to production environment
- [ ] SSL/TLS certificate configured
- [ ] Workload manifest updated with production URLs
- [ ] Workload registered in Fabric tenant
- [ ] Health monitoring configured
- [ ] Logging and diagnostics enabled
- [ ] Security review completed
- [ ] Performance testing completed
- [ ] Documentation updated
- [ ] User training completed

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
