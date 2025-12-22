#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the Frontend development environment for Fabric Mapping Service
.DESCRIPTION
    This script installs npm packages and verifies the frontend setup.
.PARAMETER Force
    Force reinstall all npm packages
.PARAMETER Audit
    Run npm audit to check for vulnerabilities
.EXAMPLE
    .\SetupFrontend.ps1
    .\SetupFrontend.ps1 -Force
    .\SetupFrontend.ps1 -Audit
#>

param(
    [switch]$Force,
    [switch]$Audit
)

$ErrorActionPreference = "Stop"

Write-Host "=== Setting up Fabric Mapping Service Frontend ===" -ForegroundColor Cyan
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

# Check Node.js
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "✓ Node.js: $nodeVersion" -ForegroundColor Green
}
catch {
    Write-Host "✗ Node.js not found. Please install Node.js from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

try {
    $npmVersion = npm --version
    Write-Host "✓ npm: v$npmVersion" -ForegroundColor Green
}
catch {
    Write-Host "✗ npm not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Install packages
$nodeModulesPath = Join-Path $frontendPath "node_modules"
if ($Force -and (Test-Path $nodeModulesPath)) {
    Write-Host "Removing existing node_modules..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $nodeModulesPath
    Write-Host "✓ node_modules removed" -ForegroundColor Green
    Write-Host ""
}

if (-not (Test-Path $nodeModulesPath)) {
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ npm install failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ npm packages installed" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "✓ npm packages already installed" -ForegroundColor Green
    Write-Host ""
}

# Run audit if requested
if ($Audit) {
    Write-Host "Running npm audit..." -ForegroundColor Yellow
    npm audit
    Write-Host ""
}

# Verify TypeScript
Write-Host "Verifying TypeScript configuration..." -ForegroundColor Yellow
$tsconfigPath = Join-Path $frontendPath "tsconfig.json"
if (Test-Path $tsconfigPath) {
    Write-Host "✓ tsconfig.json found" -ForegroundColor Green
}
else {
    Write-Host "✗ tsconfig.json not found" -ForegroundColor Red
    exit 1
}

# Verify Webpack
$webpackConfigPath = Join-Path $frontendPath "webpack.config.js"
if (Test-Path $webpackConfigPath) {
    Write-Host "✓ webpack.config.js found" -ForegroundColor Green
}
else {
    Write-Host "✗ webpack.config.js not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Summary
Write-Host "=== Frontend Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Available commands:" -ForegroundColor Cyan
Write-Host "  npm run start     - Start development server" -ForegroundColor White
Write-Host "  npm run build     - Build for production" -ForegroundColor White
Write-Host "  npm run build:dev - Build for development" -ForegroundColor White
Write-Host ""
Write-Host "Or use PowerShell scripts:" -ForegroundColor Cyan
Write-Host "  .\scripts\Run\StartFrontendDevServer.ps1  - Start dev server" -ForegroundColor White
Write-Host "  .\scripts\Build\BuildFrontend.ps1         - Build frontend" -ForegroundColor White
Write-Host "  .\scripts\Run\StartFullStack.ps1          - Start API + Frontend" -ForegroundColor White
