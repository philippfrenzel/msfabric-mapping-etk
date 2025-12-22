# Project Setup Guide

This guide walks you through setting up the Fabric Reference Table & Data Mapping Service development environment.

## Prerequisites

### Required Software

- **.NET 10.0 SDK or later**: [Download](https://dotnet.microsoft.com/download)
  - For Azure deployment, you can retarget to .NET 8.0 or 9.0 if needed
- **Git**: [Download](https://git-scm.com/downloads)
- **Code Editor**: Visual Studio 2022, VS Code, or JetBrains Rider

### For Fabric Integration (Optional)

- **PowerShell 7**: [Download](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- **Azure CLI**: [Download](https://learn.microsoft.com/cli/azure/install-azure-cli)
- **Microsoft Fabric Tenant**: [Sign up](https://app.fabric.microsoft.com/)
- **Fabric Workspace**: With assigned capacity
- **Microsoft Entra ID App**: For authentication

### Recommended Tools

- **Postman or similar**: For API testing
- **Git client**: GitHub Desktop, SourceTree, or command line

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/philippfrenzel/msfabric-mapping-etk.git
cd msfabric-mapping-etk
```

### 2. Verify Prerequisites

Check that .NET is installed correctly:

```bash
dotnet --version
# Should show 10.0.x or later
```

### 3. Build the Solution

```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Run Tests

```bash
dotnet test
```

All tests should pass.

### 5. Run the API Locally

```bash
cd src/FabricMappingService.Api
dotnet run
```

The API will start at `https://localhost:5001` (or the port shown in the console).

### 6. Test the API

Open your browser or API client to:
```
https://localhost:5001/api/mapping/health
```

You should receive a health check response.

## Detailed Setup

### Development Environment Setup

#### Option 1: Visual Studio 2022

1. Open `FabricMappingService.sln`
2. Set `FabricMappingService.Api` as the startup project
3. Press F5 to run with debugging

#### Option 2: VS Code

1. Open the project folder in VS Code
2. Install recommended extensions:
   - C# Dev Kit
   - .NET Extension Pack
3. Use the Run and Debug panel to start the API

#### Option 3: Command Line

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run API
cd src/FabricMappingService.Api
dotnet run

# Run with watch mode (auto-reload on changes)
dotnet watch run
```

### Configuration

#### API Configuration

Edit `src/FabricMappingService.Api/appsettings.json` or `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Environment Variables

For local development, you can set environment variables:

```bash
# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"
```

## Microsoft Fabric Integration Setup

To integrate with Microsoft Fabric, follow these steps:

### 1. Create Microsoft Entra ID Application

You need an Entra ID (Azure AD) application for authentication:

```powershell
# Using Azure CLI
az login
az ad app create --display-name "FabricMappingService" --sign-in-audience AzureADMyOrg
```

Or create manually in [Azure Portal](https://portal.azure.com):
1. Navigate to Microsoft Entra ID → App registrations
2. Click "New registration"
3. Name: "FabricMappingService"
4. Supported account types: "Accounts in this organizational directory only"
5. Register

### 2. Configure Application

After creating the app:

1. **API Permissions**:
   - Add `https://analysis.windows.net/powerbi/api/.default`
   - Grant admin consent

2. **Authentication**:
   - Add platform: Web
   - Redirect URIs: Your backend URL
   - Enable ID tokens

3. **Certificates & Secrets**:
   - Create a client secret
   - Save it securely (you'll need it for deployment)

### 3. Update Workload Manifest

Edit `fabric-manifest/workload-manifest.json`:

```json
{
  "authentication": {
    "authenticationMethod": "AAD",
    "aadAppId": "YOUR_APP_ID_HERE",
    ...
  },
  "backend": {
    "backendUrl": "YOUR_BACKEND_URL_HERE",
    ...
  }
}
```

### 4. Deploy Backend API

Deploy the API to Azure App Service or your hosting platform:

```bash
# Publish for deployment
dotnet publish -c Release -o ./publish

# Deploy to Azure (example)
az webapp deployment source config-zip \
  --resource-group YourResourceGroup \
  --name YourAppServiceName \
  --src ./publish.zip
```

### 5. Register Workload in Fabric

1. Navigate to Fabric Admin Portal
2. Enable Developer Mode in settings
3. Upload your workload manifest package
4. Configure workspace access

For detailed Fabric integration instructions, see [FABRIC-INTEGRATION.md](FABRIC-INTEGRATION.md).

## Testing the Service

### Unit Tests

Run all tests:
```bash
dotnet test
```

Run specific test:
```bash
dotnet test --filter "FullyQualifiedName~MappingIOTests"
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing

#### Using curl

Test health endpoint:
```bash
curl https://localhost:5001/api/mapping/health
```

Create a reference table:
```bash
curl -X POST https://localhost:5001/api/reference-tables \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "test-table",
    "columns": [
      {
        "name": "Category",
        "dataType": "string",
        "order": 1
      }
    ],
    "isVisible": true
  }'
```

#### Using Postman

1. Import the API collection from `src/FabricMappingService.Api/FabricMappingService.Api.http`
2. Set base URL to `https://localhost:5001`
3. Run requests

## Common Issues and Solutions

### Issue: Certificate errors

**Solution**: Trust the development certificate
```bash
dotnet dev-certs https --trust
```

### Issue: Port already in use

**Solution**: Change the port in `launchSettings.json` or kill the process using the port

### Issue: .NET SDK not found

**Solution**: Ensure .NET 10.0 SDK is installed and in PATH
```bash
dotnet --list-sdks
```

### Issue: Build fails with missing packages

**Solution**: Restore packages
```bash
dotnet restore
```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/my-new-feature
```

### 2. Make Changes

- Add code in `src/`
- Add tests in `tests/`
- Update documentation as needed

### 3. Test Changes

```bash
# Build
dotnet build

# Run tests
dotnet test

# Test API locally
cd src/FabricMappingService.Api
dotnet run
```

### 4. Commit and Push

```bash
git add .
git commit -m "Add new feature"
git push origin feature/my-new-feature
```

### 5. Create Pull Request

Open a PR on GitHub for review.

## Project Scripts

### Build Scripts

```bash
# Clean build artifacts
dotnet clean

# Build release version
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish
```

### Test Scripts

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Development Scripts

```bash
# Run with hot reload
cd src/FabricMappingService.Api
dotnet watch run

# Run specific project
dotnet run --project src/FabricMappingService.Api
```

## Next Steps

1. **Explore the Code**: Start with `src/FabricMappingService.Core/Services/`
2. **Read Documentation**: Review [API.md](API.md) for endpoint details
3. **Try Examples**: Check `src/FabricMappingService.Core/Examples/`
4. **Integrate with Fabric**: Follow [FABRIC-INTEGRATION.md](FABRIC-INTEGRATION.md)

## Additional Resources

- [README.md](../README.md) - Project overview and usage examples
- [API.md](API.md) - API endpoint documentation
- [FABRIC-INTEGRATION.md](FABRIC-INTEGRATION.md) - Fabric integration guide
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Repository organization

## Getting Help

- Review [SUPPORT.md](../SUPPORT.md) for support options
- Open an issue on [GitHub](https://github.com/philippfrenzel/msfabric-mapping-etk/issues)
- Check [Microsoft Fabric Documentation](https://learn.microsoft.com/fabric/)

Happy coding! 🚀
