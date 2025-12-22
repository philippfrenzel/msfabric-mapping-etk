#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Registers the Fabric Mapping Workload in Microsoft Fabric
.DESCRIPTION
    This script registers the workload with Microsoft Fabric using the workload manifest.
    It requires the Microsoft Fabric PowerShell module or REST API access.
.PARAMETER TenantId
    Azure Active Directory Tenant ID
.PARAMETER WorkloadManifestPath
    Path to the workload manifest JSON file (optional, defaults to fabric-manifest/workload-manifest.json)
.PARAMETER UseRestApi
    Use REST API instead of PowerShell cmdlets for registration
.PARAMETER BackendUrl
    Backend API URL (overrides the URL in the manifest)
.PARAMETER AadAppId
    Azure AD Application ID (overrides the ID in the manifest)
.EXAMPLE
    .\RegisterWorkload.ps1 -TenantId "your-tenant-id"
.EXAMPLE
    .\RegisterWorkload.ps1 -TenantId "your-tenant-id" -BackendUrl "https://your-api.azurewebsites.net" -AadAppId "your-app-id"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    
    [string]$WorkloadManifestPath = "",
    
    [switch]$UseRestApi,
    
    [string]$BackendUrl = "",
    
    [string]$AadAppId = ""
)

$ErrorActionPreference = "Stop"

Write-Host "=== Microsoft Fabric Workload Registration ===" -ForegroundColor Cyan
Write-Host ""

# Determine manifest path
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir

if ([string]::IsNullOrEmpty($WorkloadManifestPath)) {
    $WorkloadManifestPath = Join-Path $solutionRoot "fabric-manifest" | Join-Path -ChildPath "workload-manifest.json"
}

if (-not (Test-Path $WorkloadManifestPath)) {
    Write-Host "✗ Workload manifest not found at: $WorkloadManifestPath" -ForegroundColor Red
    exit 1
}

Write-Host "Workload Manifest: $WorkloadManifestPath" -ForegroundColor Cyan
Write-Host ""

# Load and optionally update manifest
Write-Host "Loading workload manifest..." -ForegroundColor Yellow
$manifestContent = Get-Content $WorkloadManifestPath -Raw | ConvertFrom-Json

if (-not [string]::IsNullOrEmpty($BackendUrl)) {
    Write-Host "Updating backend URL to: $BackendUrl" -ForegroundColor Yellow
    $manifestContent.workloadManifest.backend.backendUrl = $BackendUrl
}

if (-not [string]::IsNullOrEmpty($AadAppId)) {
    Write-Host "Updating AAD App ID to: $AadAppId" -ForegroundColor Yellow
    $manifestContent.workloadManifest.authentication.aadAppId = $AadAppId
}

Write-Host "✓ Manifest loaded" -ForegroundColor Green
Write-Host ""
Write-Host "Workload Details:" -ForegroundColor Cyan
Write-Host "  ID: $($manifestContent.workloadManifest.workloadDetails.workloadId)" -ForegroundColor White
Write-Host "  Name: $($manifestContent.workloadManifest.workloadDetails.displayName)" -ForegroundColor White
Write-Host "  Version: $($manifestContent.workloadManifest.workloadDetails.version)" -ForegroundColor White
Write-Host "  Backend: $($manifestContent.workloadManifest.backend.backendUrl)" -ForegroundColor White
Write-Host "  AAD App: $($manifestContent.workloadManifest.authentication.aadAppId)" -ForegroundColor White
Write-Host ""

# Validate critical settings
if ($manifestContent.workloadManifest.backend.backendUrl -eq "YOUR_BACKEND_URL_HERE") {
    Write-Host "✗ Backend URL not configured in manifest. Please update it or use -BackendUrl parameter." -ForegroundColor Red
    exit 1
}

if ($manifestContent.workloadManifest.authentication.aadAppId -eq "YOUR_AAD_APP_ID_HERE") {
    Write-Host "✗ AAD App ID not configured in manifest. Please update it or use -AadAppId parameter." -ForegroundColor Red
    exit 1
}

# Register workload
if ($UseRestApi) {
    Write-Host "Using REST API for registration..." -ForegroundColor Yellow
    Write-Host ""
    
    # Get access token
    Write-Host "Acquiring access token..." -ForegroundColor Yellow
    
    # Check if Azure CLI is available
    if (Get-Command "az" -ErrorAction SilentlyContinue) {
        $accountInfo = az account show 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Please login to Azure..." -ForegroundColor Yellow
            az login --tenant $TenantId
        }
        
        az account set --subscription (az account show --query id -o tsv)
        $accessToken = az account get-access-token --resource "https://analysis.windows.net/powerbi/api" --query accessToken -o tsv
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Failed to acquire access token" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "✓ Access token acquired" -ForegroundColor Green
        Write-Host ""
        
        # Call Fabric API to register workload
        Write-Host "Registering workload via REST API..." -ForegroundColor Yellow
        
        $headers = @{
            "Authorization" = "Bearer $accessToken"
            "Content-Type" = "application/json"
        }
        
        $manifestJson = $manifestContent | ConvertTo-Json -Depth 10
        
        try {
            $response = Invoke-RestMethod -Uri "https://api.fabric.microsoft.com/v1/workloads" `
                -Method Post `
                -Headers $headers `
                -Body $manifestJson `
                -ErrorAction Stop
            
            Write-Host "✓ Workload registered successfully" -ForegroundColor Green
            Write-Host ""
            Write-Host "Response:" -ForegroundColor Cyan
            $response | ConvertTo-Json -Depth 5 | Write-Host
        }
        catch {
            Write-Host "✗ Failed to register workload" -ForegroundColor Red
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
            
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "Response: $responseBody" -ForegroundColor Red
            }
            
            exit 1
        }
    }
    else {
        Write-Host "✗ Azure CLI not found. Please install it or use PowerShell cmdlets." -ForegroundColor Red
        Write-Host "Download: https://learn.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
        exit 1
    }
}
else {
    Write-Host "Using PowerShell cmdlets for registration..." -ForegroundColor Yellow
    Write-Host ""
    
    # Check if Fabric module is installed
    if (-not (Get-Module -ListAvailable -Name "MicrosoftFabric")) {
        Write-Host "Microsoft Fabric PowerShell module not found." -ForegroundColor Yellow
        Write-Host "Please install it using: Install-Module -Name MicrosoftFabric" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Alternatively, use -UseRestApi flag to register via REST API" -ForegroundColor Yellow
        exit 1
    }
    
    # Import module
    Import-Module MicrosoftFabric -ErrorAction Stop
    
    # Connect to Fabric
    Write-Host "Connecting to Microsoft Fabric..." -ForegroundColor Yellow
    try {
        Connect-Fabric -TenantId $TenantId -ErrorAction Stop
        Write-Host "✓ Connected to Fabric" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host "✗ Failed to connect to Fabric" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
    
    # Register workload
    Write-Host "Registering workload..." -ForegroundColor Yellow
    try {
        $result = Register-FabricWorkload -ManifestPath $WorkloadManifestPath -ErrorAction Stop
        Write-Host "✓ Workload registered successfully" -ForegroundColor Green
        Write-Host ""
        $result | Format-List
    }
    catch {
        Write-Host "✗ Failed to register workload" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "=== Registration Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Verify the workload appears in your Fabric workspace"
Write-Host "2. Test the workload endpoints: $($manifestContent.workloadManifest.backend.backendUrl)/api/workload/health"
Write-Host "3. Create reference tables and mapping configurations"
Write-Host "4. Review the documentation in docs/WORKLOAD_GUIDE_DE.md"
Write-Host ""
