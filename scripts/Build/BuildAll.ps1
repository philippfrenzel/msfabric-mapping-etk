#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds both Backend and Frontend for Fabric Mapping Service
.DESCRIPTION
    This script builds the .NET solution and React frontend in one step.
.PARAMETER Configuration
    .NET build configuration (Release or Debug, default: Release)
.PARAMETER FrontendMode
    Frontend build mode (production or development, default: production)
.PARAMETER Clean
    Clean before building
.PARAMETER SkipTests
    Skip running .NET tests
.PARAMETER SkipFrontend
    Skip building the frontend
.PARAMETER SkipBackend
    Skip building the backend
.EXAMPLE
    .\BuildAll.ps1
    .\BuildAll.ps1 -Configuration Debug -FrontendMode development
    .\BuildAll.ps1 -Clean
    .\BuildAll.ps1 -SkipTests
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [ValidateSet("development", "production")]
    [string]$FrontendMode = "production",
    [switch]$Clean,
    [switch]$SkipTests,
    [switch]$SkipFrontend,
    [switch]$SkipBackend
)

$ErrorActionPreference = "Stop"

Write-Host "=== Building Fabric Mapping Service (Full Stack) ===" -ForegroundColor Cyan
Write-Host ""

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptPath
$solutionRoot = Split-Path -Parent $scriptsDir

$backendSuccess = $true
$frontendSuccess = $true

# Build Backend
if (-not $SkipBackend) {
    Write-Host "--- Building Backend (.NET) ---" -ForegroundColor Magenta
    Write-Host ""
    
    Set-Location $solutionRoot
    
    if ($Clean) {
        Write-Host "Cleaning..." -ForegroundColor Yellow
        dotnet clean -c $Configuration
        Write-Host "✓ Clean completed" -ForegroundColor Green
        Write-Host ""
    }
    
    Write-Host "Restoring packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Restore failed" -ForegroundColor Red
        $backendSuccess = $false
    }
    else {
        Write-Host "✓ Restore completed" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "Building ($Configuration)..." -ForegroundColor Yellow
        dotnet build -c $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Build failed" -ForegroundColor Red
            $backendSuccess = $false
        }
        else {
            Write-Host "✓ Build completed" -ForegroundColor Green
            Write-Host ""
            
            if (-not $SkipTests) {
                Write-Host "Running tests..." -ForegroundColor Yellow
                dotnet test -c $Configuration --no-build --verbosity normal
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "✗ Tests failed" -ForegroundColor Red
                    $backendSuccess = $false
                }
                else {
                    Write-Host "✓ All tests passed" -ForegroundColor Green
                }
            }
            else {
                Write-Host "Skipping tests" -ForegroundColor Yellow
            }
        }
    }
    Write-Host ""
}

# Build Frontend
if (-not $SkipFrontend) {
    Write-Host "--- Building Frontend (React) ---" -ForegroundColor Magenta
    Write-Host ""
    
    $frontendPath = Join-Path $solutionRoot "src" "FabricMappingService.Frontend"
    
    if (-not (Test-Path $frontendPath)) {
        Write-Host "✗ Frontend project not found at: $frontendPath" -ForegroundColor Red
        $frontendSuccess = $false
    }
    else {
        Set-Location $frontendPath
        
        if ($Clean) {
            $distPath = Join-Path $frontendPath "dist"
            if (Test-Path $distPath) {
                Write-Host "Cleaning dist folder..." -ForegroundColor Yellow
                Remove-Item -Recurse -Force $distPath
                Write-Host "✓ Dist folder cleaned" -ForegroundColor Green
                Write-Host ""
            }
        }
        
        # Install packages if needed
        if (-not (Test-Path (Join-Path $frontendPath "node_modules"))) {
            Write-Host "Installing npm packages..." -ForegroundColor Yellow
            npm install
            if ($LASTEXITCODE -ne 0) {
                Write-Host "✗ npm install failed" -ForegroundColor Red
                $frontendSuccess = $false
            }
            else {
                Write-Host "✓ npm packages installed" -ForegroundColor Green
                Write-Host ""
            }
        }
        
        if ($frontendSuccess) {
            Write-Host "Building frontend ($FrontendMode)..." -ForegroundColor Yellow
            if ($FrontendMode -eq "production") {
                npm run build
            }
            else {
                npm run build:dev
            }
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "✗ Frontend build failed" -ForegroundColor Red
                $frontendSuccess = $false
            }
            else {
                Write-Host "✓ Frontend build completed" -ForegroundColor Green
            }
        }
    }
    Write-Host ""
}

# Summary
Write-Host "=== Build Summary ===" -ForegroundColor Cyan
if (-not $SkipBackend) {
    if ($backendSuccess) {
        Write-Host "  Backend:  ✓ Success" -ForegroundColor Green
    }
    else {
        Write-Host "  Backend:  ✗ Failed" -ForegroundColor Red
    }
}
if (-not $SkipFrontend) {
    if ($frontendSuccess) {
        Write-Host "  Frontend: ✓ Success" -ForegroundColor Green
    }
    else {
        Write-Host "  Frontend: ✗ Failed" -ForegroundColor Red
    }
}
Write-Host ""

if ($backendSuccess -and $frontendSuccess) {
    Write-Host "=== All Builds Complete ===" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "=== Build Failed ===" -ForegroundColor Red
    exit 1
}
