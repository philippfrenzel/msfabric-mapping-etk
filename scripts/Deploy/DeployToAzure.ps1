#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploys the Fabric Mapping Service to Azure App Service
.DESCRIPTION
    This script publishes and deploys the API to an Azure App Service.
.PARAMETER ResourceGroup
    Azure resource group name
.PARAMETER AppServiceName
    Azure App Service name
.PARAMETER SubscriptionId
    Azure subscription ID (optional, uses default if not provided)
.EXAMPLE
    .\DeployToAzure.ps1 -ResourceGroup "myResourceGroup" -AppServiceName "fabric-mapping-api"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [string]$SubscriptionId
)

$ErrorActionPreference = "Stop"

Write-Host "=== Deploying Fabric Mapping Service to Azure ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Azure CLI not found. Please install from: https://learn.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Login check
Write-Host "Checking Azure login..." -ForegroundColor Yellow
$accountInfo = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not logged in to Azure. Please login..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Azure login failed" -ForegroundColor Red
        exit 1
    }
}

# Set subscription if provided
if ($SubscriptionId) {
    Write-Host "Setting subscription to: $SubscriptionId" -ForegroundColor Yellow
    az account set --subscription $SubscriptionId
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to set subscription" -ForegroundColor Red
        exit 1
    }
}

$currentSubscription = az account show --query name -o tsv
Write-Host "✓ Using subscription: $currentSubscription" -ForegroundColor Green
Write-Host ""

# Navigate to solution directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir
Set-Location $solutionRoot

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Yellow
$publishPath = "./publish"
$publishScriptPath = Join-Path $solutionRoot "scripts" | Join-Path -ChildPath "Build" | Join-Path -ChildPath "Publish.ps1"
& $publishScriptPath -OutputPath $publishPath -Configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Publish failed" -ForegroundColor Red
    exit 1
}

# Create deployment package
Write-Host "Creating deployment package..." -ForegroundColor Yellow
$zipPath = Join-Path $solutionRoot "deploy.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

$fullPublishPath = Join-Path $solutionRoot $publishPath
Compress-Archive -Path "$fullPublishPath\*" -DestinationPath $zipPath -Force
Write-Host "✓ Deployment package created: $zipPath" -ForegroundColor Green
Write-Host ""

# Check if App Service exists
Write-Host "Checking if App Service exists..." -ForegroundColor Yellow
$appExists = az webapp show --name $AppServiceName --resource-group $ResourceGroup 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ App Service '$AppServiceName' not found in resource group '$ResourceGroup'" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please create the App Service first using:" -ForegroundColor Yellow
    Write-Host "az webapp create --name $AppServiceName --resource-group $ResourceGroup --plan <AppServicePlan>" -ForegroundColor Cyan
    exit 1
}
Write-Host "✓ App Service found" -ForegroundColor Green
Write-Host ""

# Deploy to Azure
Write-Host "Deploying to Azure App Service..." -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "App Service: $AppServiceName" -ForegroundColor Cyan
Write-Host ""

az webapp deployment source config-zip `
    --resource-group $ResourceGroup `
    --name $AppServiceName `
    --src $zipPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Deployment failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Deployment completed successfully" -ForegroundColor Green
Write-Host ""

# Get the app URL
$appUrl = az webapp show --name $AppServiceName --resource-group $ResourceGroup --query defaultHostName -o tsv
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Your API is now available at: https://$appUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test your API: https://$appUrl/api/mapping/health"
Write-Host "2. Update fabric-manifest/workload-manifest.json with backend URL: https://$appUrl"
Write-Host "3. Configure authentication settings in Azure Portal if needed"
Write-Host "4. Register your workload in Microsoft Fabric"
Write-Host ""

# Cleanup
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
