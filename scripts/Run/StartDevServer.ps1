#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the development server for Fabric Mapping Service
.DESCRIPTION
    This script starts the ASP.NET Core API in development mode with hot reload enabled.
.PARAMETER Port
    Port to run the API on (default: 5001 for HTTPS, 5000 for HTTP)
.PARAMETER NoHttps
    Disable HTTPS and run HTTP only
.EXAMPLE
    .\StartDevServer.ps1
    .\StartDevServer.ps1 -Port 5500
    .\StartDevServer.ps1 -NoHttps
#>

param(
    [int]$Port = 5001,
    [switch]$NoHttps
)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting Fabric Mapping Service Development Server ===" -ForegroundColor Cyan
Write-Host ""

# Navigate to API project
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
$apiPath = Join-Path $solutionRoot "src\FabricMappingService.Api"

if (-not (Test-Path $apiPath)) {
    Write-Host "✗ API project not found at: $apiPath" -ForegroundColor Red
    exit 1
}

Set-Location $apiPath

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Development"

if ($NoHttps) {
    $env:ASPNETCORE_URLS = "http://localhost:$Port"
    Write-Host "Starting server at: http://localhost:$Port" -ForegroundColor Green
} else {
    $httpPort = $Port - 1
    $env:ASPNETCORE_URLS = "https://localhost:$Port;http://localhost:$httpPort"
    Write-Host "Starting server at: https://localhost:$Port" -ForegroundColor Green
}

Write-Host "Environment: Development" -ForegroundColor Yellow
Write-Host "Hot reload: Enabled" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan
Write-Host ""

# Start the server with watch mode for hot reload
dotnet watch run
