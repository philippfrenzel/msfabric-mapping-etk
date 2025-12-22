#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the Fabric Mapping Service solution
.DESCRIPTION
    This script builds the solution in Release or Debug configuration.
.PARAMETER Configuration
    Build configuration (Release or Debug, default: Release)
.PARAMETER Clean
    Clean before building
.EXAMPLE
    .\Build.ps1
    .\Build.ps1 -Configuration Debug
    .\Build.ps1 -Clean
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "=== Building Fabric Mapping Service ===" -ForegroundColor Cyan
Write-Host ""

# Navigate to solution directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $solutionRoot

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    dotnet clean -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Clean failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Clean completed" -ForegroundColor Green
    Write-Host ""
}

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Restore completed" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build completed successfully" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test -c $Configuration --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Tests failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ All tests passed" -ForegroundColor Green
Write-Host ""

Write-Host "=== Build Complete ===" -ForegroundColor Green
