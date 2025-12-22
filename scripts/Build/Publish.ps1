#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes the Fabric Mapping Service for deployment
.DESCRIPTION
    This script publishes the API project for deployment to Azure or other hosting platforms.
.PARAMETER OutputPath
    Output path for published files (default: ./publish)
.PARAMETER Configuration
    Build configuration (default: Release)
.EXAMPLE
    .\Publish.ps1
    .\Publish.ps1 -OutputPath ./deploy/output
#>

param(
    [string]$OutputPath = "./publish",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Publishing Fabric Mapping Service ===" -ForegroundColor Cyan
Write-Host ""

# Navigate to solution directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir
$apiProject = Join-Path $solutionRoot "src" "FabricMappingService.Api" "FabricMappingService.Api.csproj"

if (-not (Test-Path $apiProject)) {
    Write-Host "✗ API project not found at: $apiProject" -ForegroundColor Red
    exit 1
}

# Create output directory
$fullOutputPath = Join-Path $solutionRoot $OutputPath
if (Test-Path $fullOutputPath) {
    Write-Host "Cleaning output directory..." -ForegroundColor Yellow
    Remove-Item -Path $fullOutputPath -Recurse -Force
}
New-Item -Path $fullOutputPath -ItemType Directory -Force | Out-Null

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output path: $fullOutputPath" -ForegroundColor Yellow
Write-Host ""

# Publish
Write-Host "Publishing API..." -ForegroundColor Yellow
dotnet publish $apiProject -c $Configuration -o $fullOutputPath --no-self-contained
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Publish failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Publish completed successfully" -ForegroundColor Green
Write-Host ""
Write-Host "Published files are in: $fullOutputPath" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Test the published output locally"
Write-Host "2. Deploy to your hosting platform (Azure, AWS, etc.)"
Write-Host "3. Update fabric-manifest/workload-manifest.json with your backend URL"
Write-Host ""
Write-Host "For Azure deployment, see scripts/Deploy/DeployToAzure.ps1" -ForegroundColor Yellow
