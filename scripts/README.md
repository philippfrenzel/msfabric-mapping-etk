# Scripts Directory

This directory contains automation scripts for development, building, and deploying the Fabric Mapping Service.

## Directory Structure

```
scripts/
├── Setup/          # Environment setup and configuration
├── Run/            # Scripts to run development servers
├── Build/          # Build and packaging scripts
└── Deploy/         # Deployment scripts
```

## Prerequisites

All scripts require:
- **PowerShell 7** or later ([Download](https://learn.microsoft.com/powershell/scripting/install/installing-powershell))
- **.NET 10.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))

Some scripts may require additional tools:
- **Azure CLI** for deployment scripts ([Download](https://learn.microsoft.com/cli/azure/install-azure-cli))
- **Git** for version control ([Download](https://git-scm.com/downloads))

## Usage

### Setup Scripts

Located in `Setup/`

#### Setup.ps1
Sets up the development environment, restores packages, builds the solution, and runs tests.

```powershell
# Full setup
.\scripts\Setup\Setup.ps1

# Skip tests
.\scripts\Setup\Setup.ps1 -SkipTests

# Skip build
.\scripts\Setup\Setup.ps1 -SkipBuild
```

### Run Scripts

Located in `Run/`

#### StartDevServer.ps1
Starts the API development server with hot reload enabled.

```powershell
# Start with default settings (HTTPS on port 5001)
.\scripts\Run\StartDevServer.ps1

# Start on a different port
.\scripts\Run\StartDevServer.ps1 -Port 5500

# Start with HTTP only
.\scripts\Run\StartDevServer.ps1 -NoHttps
```

### Build Scripts

Located in `Build/`

#### Build.ps1
Builds the solution in Debug or Release configuration.

```powershell
# Build in Release mode (default)
.\scripts\Build\Build.ps1

# Build in Debug mode
.\scripts\Build\Build.ps1 -Configuration Debug

# Clean before building
.\scripts\Build\Build.ps1 -Clean
```

#### Publish.ps1
Publishes the API for deployment.

```powershell
# Publish to default location (./publish)
.\scripts\Build\Publish.ps1

# Publish to custom location
.\scripts\Build\Publish.ps1 -OutputPath ./deploy/output

# Publish Debug build
.\scripts\Build\Publish.ps1 -Configuration Debug
```

### Deploy Scripts

Located in `Deploy/`

#### DeployToAzure.ps1
Deploys the API to Azure App Service.

```powershell
# Deploy to Azure
.\scripts\Deploy\DeployToAzure.ps1 `
  -ResourceGroup "myResourceGroup" `
  -AppServiceName "fabric-mapping-api"

# Deploy to specific subscription
.\scripts\Deploy\DeployToAzure.ps1 `
  -ResourceGroup "myResourceGroup" `
  -AppServiceName "fabric-mapping-api" `
  -SubscriptionId "your-subscription-id"
```

**Prerequisites for Azure deployment:**
- Azure CLI installed and logged in (`az login`)
- Existing Azure App Service
- Appropriate permissions to deploy

## Platform-Specific Notes

### Windows
Run scripts directly in PowerShell 7:
```powershell
.\scripts\Setup\Setup.ps1
```

### macOS/Linux
Use `pwsh` to run PowerShell scripts:
```bash
pwsh ./scripts/Setup/Setup.ps1
```

## Script Development Guidelines

When creating new scripts:

1. **Use PowerShell 7**: Cross-platform compatibility
2. **Include documentation**: Add comment-based help at the top
3. **Error handling**: Set `$ErrorActionPreference = "Stop"`
4. **Validate parameters**: Use `[Parameter(Mandatory=$true)]` where appropriate
5. **Provide feedback**: Use `Write-Host` with colors for user feedback
6. **Return proper exit codes**: Use `exit 1` for failures

### Example Script Template

```powershell
#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Brief description of what the script does
.DESCRIPTION
    Detailed description of the script's functionality
.PARAMETER ParameterName
    Description of the parameter
.EXAMPLE
    .\ScriptName.ps1 -ParameterName "value"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ParameterName
)

$ErrorActionPreference = "Stop"

Write-Host "=== Script Name ===" -ForegroundColor Cyan
Write-Host ""

# Script logic here

Write-Host "✓ Operation completed successfully" -ForegroundColor Green
```

## Troubleshooting

### Script execution is disabled
If you get an error about execution policy:

```powershell
# Windows (run as Administrator)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or run with bypass
pwsh -ExecutionPolicy Bypass -File .\scripts\Setup\Setup.ps1
```

### Permission denied on macOS/Linux
Make scripts executable:
```bash
chmod +x scripts/**/*.ps1
```

### .NET SDK not found
Ensure .NET SDK is installed and in PATH:
```bash
dotnet --version
```

### Azure CLI not found
Install Azure CLI:
- Windows: `winget install Microsoft.AzureCLI`
- macOS: `brew install azure-cli`
- Linux: [Follow official instructions](https://learn.microsoft.com/cli/azure/install-azure-cli-linux)

## Additional Resources

- [PowerShell Documentation](https://learn.microsoft.com/powershell/)
- [.NET CLI Documentation](https://learn.microsoft.com/dotnet/core/tools/)
- [Azure CLI Documentation](https://learn.microsoft.com/cli/azure/)
- [Project Setup Guide](../docs/PROJECT_SETUP.md)

## Contributing

When adding new scripts:
1. Place them in the appropriate subdirectory
2. Follow the existing naming conventions
3. Include proper documentation
4. Test on multiple platforms (Windows, macOS, Linux)
5. Update this README with usage examples
