#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the frontend development server for Fabric Mapping Service
.DESCRIPTION
    This script starts the React frontend with webpack-dev-server in development mode with hot reload enabled.
.PARAMETER Port
    Port to run the frontend on (default: 3000)
.PARAMETER Open
    Automatically open browser after starting
.EXAMPLE
    .\StartFrontendDevServer.ps1
    .\StartFrontendDevServer.ps1 -Port 3001
    .\StartFrontendDevServer.ps1 -Open
#>

param(
    [int]$Port = 3000,
    [switch]$Open
)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting Fabric Mapping Service Frontend Development Server ===" -ForegroundColor Cyan
Write-Host ""

# Navigate to Frontend project
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir
$frontendPath = Join-Path $solutionRoot "src" "FabricMappingService.Frontend"

if (-not (Test-Path $frontendPath)) {
    Write-Host "✗ Frontend project not found at: $frontendPath" -ForegroundColor Red
    exit 1
}

Set-Location $frontendPath

# Check if node_modules exists
if (-not (Test-Path (Join-Path $frontendPath "node_modules"))) {
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ npm install failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ npm packages installed" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Starting frontend server at: http://localhost:$Port" -ForegroundColor Green
Write-Host "Hot reload: Enabled" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan
Write-Host ""

# Build the command arguments
$npmArgs = @("run", "start", "--", "--port", $Port)

if ($Open) {
    $npmArgs += "--open"
}

# Start the development server
npm @npmArgs
