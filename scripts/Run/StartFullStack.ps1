#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts both backend API and frontend development servers
.DESCRIPTION
    This script starts the ASP.NET Core API and React frontend simultaneously for full-stack development.
.PARAMETER ApiPort
    Port to run the API on (default: 5001)
.PARAMETER FrontendPort
    Port to run the frontend on (default: 3000)
.PARAMETER NoHttps
    Disable HTTPS for API
.EXAMPLE
    .\StartFullStack.ps1
    .\StartFullStack.ps1 -ApiPort 5500 -FrontendPort 3001
    .\StartFullStack.ps1 -NoHttps
#>

param(
    [int]$ApiPort = 5001,
    [int]$FrontendPort = 3000,
    [switch]$NoHttps
)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting Fabric Mapping Service Full Stack ===" -ForegroundColor Cyan
Write-Host ""

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir

$apiPath = Join-Path $solutionRoot "src" "FabricMappingService.Api"
$frontendPath = Join-Path $solutionRoot "src" "FabricMappingService.Frontend"

# Validate paths
if (-not (Test-Path $apiPath)) {
    Write-Host "✗ API project not found at: $apiPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $frontendPath)) {
    Write-Host "✗ Frontend project not found at: $frontendPath" -ForegroundColor Red
    exit 1
}

# Install frontend dependencies if needed
if (-not (Test-Path (Join-Path $frontendPath "node_modules"))) {
    Write-Host "Installing frontend npm packages..." -ForegroundColor Yellow
    Push-Location $frontendPath
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ npm install failed" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Host "✓ npm packages installed" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Starting services:" -ForegroundColor Yellow
if ($NoHttps) {
    Write-Host "  API:      http://localhost:$ApiPort" -ForegroundColor Green
}
else {
    Write-Host "  API:      https://localhost:$ApiPort" -ForegroundColor Green
}
Write-Host "  Frontend: http://localhost:$FrontendPort" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop both servers" -ForegroundColor Cyan
Write-Host ""

# Start API in background
$apiJob = Start-Job -ScriptBlock {
    param($apiPath, $port, $noHttps)
    Set-Location $apiPath
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    if ($noHttps) {
        $env:ASPNETCORE_URLS = "http://localhost:$port"
    }
    else {
        $httpPort = $port - 1
        $env:ASPNETCORE_URLS = "https://localhost:$port;http://localhost:$httpPort"
    }
    dotnet watch run 2>&1
} -ArgumentList $apiPath, $ApiPort, $NoHttps

Write-Host "[API] Started in background (Job ID: $($apiJob.Id))" -ForegroundColor Yellow

# Start Frontend in foreground
Push-Location $frontendPath
try {
    npm run start -- --port $FrontendPort
}
finally {
    # Cleanup: Stop API job when frontend is stopped
    Write-Host ""
    Write-Host "Stopping API server..." -ForegroundColor Yellow
    Stop-Job -Job $apiJob -ErrorAction SilentlyContinue
    Remove-Job -Job $apiJob -Force -ErrorAction SilentlyContinue
    Write-Host "✓ All servers stopped" -ForegroundColor Green
    Pop-Location
}
