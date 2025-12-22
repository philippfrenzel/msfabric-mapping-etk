#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Setup script for Fabric Mapping Service development environment
.DESCRIPTION
    This script automates the setup of the development environment for the Fabric Mapping Service.
    It checks prerequisites, builds the solution, and runs tests.
.PARAMETER SkipTests
    Skip running tests after build
.PARAMETER SkipBuild
    Skip building the solution
.EXAMPLE
    .\Setup.ps1
    .\Setup.ps1 -SkipTests
#>

param(
    [switch]$SkipTests,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Write-Host "=== Fabric Mapping Service Setup ===" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-CommandExists {
    param($command)
    $null -ne (Get-Command $command -ErrorAction SilentlyContinue)
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
if (Test-CommandExists "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
    
    # Check if version is 8.0 or later
    $versionMajor = [int]($dotnetVersion.Split('.')[0])
    if ($versionMajor -lt 8) {
        Write-Host "✗ .NET 8.0 or later is required. Please upgrade." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ .NET SDK not found. Please install from: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check Git
if (Test-CommandExists "git") {
    $gitVersion = git --version
    Write-Host "✓ Git found: $gitVersion" -ForegroundColor Green
} else {
    Write-Host "⚠ Git not found. Some features may not work." -ForegroundColor Yellow
}

Write-Host ""

# Navigate to solution directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir
Set-Location $solutionRoot

Write-Host "Solution directory: $solutionRoot" -ForegroundColor Cyan
Write-Host ""

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Packages restored successfully" -ForegroundColor Green
Write-Host ""

# Build solution
if (-not $SkipBuild) {
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Build completed successfully" -ForegroundColor Green
    Write-Host ""
}

# Run tests
if (-not $SkipTests -and -not $SkipBuild) {
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Tests failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ All tests passed" -ForegroundColor Green
    Write-Host ""
}

# Trust development certificate
Write-Host "Checking HTTPS development certificate..." -ForegroundColor Yellow
$trustCert = Read-Host "Do you want to trust the HTTPS development certificate? (Y/n)"
if ($trustCert -eq "" -or $trustCert -eq "Y" -or $trustCert -eq "y") {
    dotnet dev-certs https --trust
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Certificate trusted" -ForegroundColor Green
    } else {
        Write-Host "⚠ Certificate trust failed. You may need to do this manually." -ForegroundColor Yellow
    }
}
Write-Host ""

# Setup complete
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run the API locally:"
Write-Host "   cd src/FabricMappingService.Api"
Write-Host "   dotnet run"
Write-Host ""
Write-Host "2. Or use the run script:"
Write-Host "   .\scripts\Run\StartDevServer.ps1"
Write-Host ""
Write-Host "3. For Fabric integration, see docs/FABRIC-INTEGRATION.md"
Write-Host ""
Write-Host "For more information, see docs/PROJECT_SETUP.md" -ForegroundColor Yellow
