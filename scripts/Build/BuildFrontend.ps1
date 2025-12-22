#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the Frontend for Fabric Mapping Service
.DESCRIPTION
    This script builds the React frontend with webpack in production or development mode.
.PARAMETER Mode
    Build mode (production or development, default: production)
.PARAMETER Clean
    Clean dist folder before building
.PARAMETER Install
    Force reinstall npm packages
.EXAMPLE
    .\BuildFrontend.ps1
    .\BuildFrontend.ps1 -Mode development
    .\BuildFrontend.ps1 -Clean
    .\BuildFrontend.ps1 -Install
#>

param(
    [ValidateSet("development", "production")]
    [string]$Mode = "production",
    [switch]$Clean,
    [switch]$Install
)

$ErrorActionPreference = "Stop"

Write-Host "=== Building Fabric Mapping Service Frontend ===" -ForegroundColor Cyan
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

Write-Host "Mode: $Mode" -ForegroundColor Yellow
Write-Host ""

# Clean if requested
if ($Clean) {
    $distPath = Join-Path $frontendPath "dist"
    if (Test-Path $distPath) {
        Write-Host "Cleaning dist folder..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force $distPath
        Write-Host "✓ Dist folder cleaned" -ForegroundColor Green
        Write-Host ""
    }
}

# Install or reinstall packages
$nodeModulesPath = Join-Path $frontendPath "node_modules"
if ($Install -or -not (Test-Path $nodeModulesPath)) {
    if ($Install -and (Test-Path $nodeModulesPath)) {
        Write-Host "Removing existing node_modules..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force $nodeModulesPath
    }
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ npm install failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ npm packages installed" -ForegroundColor Green
    Write-Host ""
}

# Build
Write-Host "Building frontend ($Mode)..." -ForegroundColor Yellow

if ($Mode -eq "production") {
    npm run build
}
else {
    npm run build:dev
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Frontend build failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Frontend build completed successfully" -ForegroundColor Green
Write-Host ""

# Show output location
$distPath = Join-Path $frontendPath "dist"
if (Test-Path $distPath) {
    $files = Get-ChildItem $distPath -Recurse -File
    $totalSize = ($files | Measure-Object -Property Length -Sum).Sum / 1KB
    Write-Host "Output: $distPath" -ForegroundColor Cyan
    Write-Host "Files: $($files.Count)" -ForegroundColor Cyan
    Write-Host "Total size: $([math]::Round($totalSize, 2)) KB" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=== Frontend Build Complete ===" -ForegroundColor Green
