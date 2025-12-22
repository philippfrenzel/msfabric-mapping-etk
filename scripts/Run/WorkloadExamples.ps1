#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Example script demonstrating MappingWorkload usage
.DESCRIPTION
    This script shows how to interact with the MappingWorkload API endpoints
    and perform various reference table operations.
.PARAMETER BaseUrl
    Base URL of the API (default: https://localhost:5001)
.EXAMPLE
    .\WorkloadExamples.ps1
.EXAMPLE
    .\WorkloadExamples.ps1 -BaseUrl "https://fabric-mapping-service.azurewebsites.net"
#>

param(
    [string]$BaseUrl = "https://localhost:5001"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Microsoft Fabric Mapping Workload Examples ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor White
Write-Host ""

# Function to make API calls
function Invoke-WorkloadApi {
    param(
        [string]$Endpoint,
        [string]$Method = "GET",
        [object]$Body = $null
    )
    
    $uri = "$BaseUrl$Endpoint"
    $params = @{
        Uri = $uri
        Method = $Method
        ContentType = "application/json"
    }
    
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
    }
    
    try {
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Error calling $uri" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        throw
    }
}

# Example 1: Get Workload Info
Write-Host "Example 1: Get Workload Information" -ForegroundColor Yellow
Write-Host "-----------------------------------" -ForegroundColor DarkGray
$workloadInfo = Invoke-WorkloadApi -Endpoint "/api/workload/info"
$workloadInfo | ConvertTo-Json | Write-Host
Write-Host ""

# Example 2: Health Check
Write-Host "Example 2: Health Check" -ForegroundColor Yellow
Write-Host "-----------------------" -ForegroundColor DarkGray
$health = Invoke-WorkloadApi -Endpoint "/api/workload/health"
Write-Host "Status: $($health.status)" -ForegroundColor $(if($health.isHealthy) {"Green"} else {"Red"})
Write-Host "Version: $($health.version)" -ForegroundColor White
Write-Host ""

# Example 3: Create Reference Table
Write-Host "Example 3: Create Reference Table" -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor DarkGray
$createTableConfig = @{
    operationType = "CreateReferenceTable"
    timeoutSeconds = 60
    parameters = @{
        tableName = "beispiel_produkttyp"
        columns = '[{"name":"ProductType","dataType":"string","order":1},{"name":"Category","dataType":"string","order":2}]'
        isVisible = $true
        notifyOnNewMapping = $false
    }
}

try {
    $createResult = Invoke-WorkloadApi -Endpoint "/api/workload/execute" -Method "POST" -Body $createTableConfig
    if ($createResult.success) {
        Write-Host "✓ Reference table created successfully" -ForegroundColor Green
        Write-Host "Execution time: $($createResult.executionTimeMs)ms" -ForegroundColor Gray
    }
}
catch {
    Write-Host "Note: Table may already exist" -ForegroundColor Yellow
}
Write-Host ""

# Example 4: Sync Reference Table
Write-Host "Example 4: Sync Reference Table with Data" -ForegroundColor Yellow
Write-Host "------------------------------------------" -ForegroundColor DarkGray
$syncConfig = @{
    operationType = "SyncReferenceTable"
    timeoutSeconds = 60
    parameters = @{
        tableName = "beispiel_produkttyp"
        keyAttributeName = "ProduktId"
        data = '[{"ProduktId":"VTP001","Name":"Product A"},{"ProduktId":"VTP002","Name":"Product B"}]'
    }
}

$syncResult = Invoke-WorkloadApi -Endpoint "/api/workload/execute" -Method "POST" -Body $syncConfig
if ($syncResult.success) {
    Write-Host "✓ Reference table synchronized" -ForegroundColor Green
    Write-Host "New keys added: $($syncResult.data.newKeysAdded)" -ForegroundColor White
    Write-Host "Execution time: $($syncResult.executionTimeMs)ms" -ForegroundColor Gray
}
Write-Host ""

# Example 5: Read Reference Table
Write-Host "Example 5: Read Reference Table Data" -ForegroundColor Yellow
Write-Host "-------------------------------------" -ForegroundColor DarkGray
$readConfig = @{
    operationType = "ReadReferenceTable"
    timeoutSeconds = 30
    parameters = @{
        tableName = "beispiel_produkttyp"
    }
}

$readResult = Invoke-WorkloadApi -Endpoint "/api/workload/execute" -Method "POST" -Body $readConfig
if ($readResult.success) {
    Write-Host "✓ Reference table data retrieved" -ForegroundColor Green
    Write-Host "Data:" -ForegroundColor White
    $readResult.data.data | ConvertTo-Json -Depth 5 | Write-Host
    Write-Host "Execution time: $($readResult.executionTimeMs)ms" -ForegroundColor Gray
}
Write-Host ""

# Example 6: Update Reference Table Row
Write-Host "Example 6: Update Reference Table Row" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor DarkGray
$updateConfig = @{
    operationType = "UpdateReferenceTableRow"
    timeoutSeconds = 30
    parameters = @{
        tableName = "beispiel_produkttyp"
        key = "VTP001"
        attributes = '{"ProductType":"Basic","Category":"Insurance"}'
    }
}

$updateResult = Invoke-WorkloadApi -Endpoint "/api/workload/execute" -Method "POST" -Body $updateConfig
if ($updateResult.success) {
    Write-Host "✓ Reference table row updated" -ForegroundColor Green
    Write-Host "Key: $($updateResult.data.key)" -ForegroundColor White
    Write-Host "Execution time: $($updateResult.executionTimeMs)ms" -ForegroundColor Gray
}
Write-Host ""

# Example 7: Validate Configuration
Write-Host "Example 7: Validate Workload Configuration" -ForegroundColor Yellow
Write-Host "-------------------------------------------" -ForegroundColor DarkGray
$validateConfig = @{
    operationType = "CreateReferenceTable"
    timeoutSeconds = 60
    parameters = @{
        tableName = "test_validation"
        columns = '[]'
    }
}

$validationResult = Invoke-WorkloadApi -Endpoint "/api/workload/validate" -Method "POST" -Body $validateConfig
Write-Host "Valid: $($validationResult.isValid)" -ForegroundColor $(if($validationResult.isValid) {"Green"} else {"Yellow"})
if ($validationResult.errors.Count -gt 0) {
    Write-Host "Errors:" -ForegroundColor Red
    $validationResult.errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
}
if ($validationResult.warnings.Count -gt 0) {
    Write-Host "Warnings:" -ForegroundColor Yellow
    $validationResult.warnings | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
}
Write-Host ""

# Example 8: Alternative - Use Reference Table API Directly
Write-Host "Example 8: Direct Reference Table API" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor DarkGray
Write-Host "You can also use the direct reference table API:" -ForegroundColor White
Write-Host "  GET    $BaseUrl/api/reference-tables" -ForegroundColor Cyan
Write-Host "  GET    $BaseUrl/api/reference-tables/beispiel_produkttyp" -ForegroundColor Cyan
Write-Host "  POST   $BaseUrl/api/reference-tables" -ForegroundColor Cyan
Write-Host "  POST   $BaseUrl/api/reference-tables/sync" -ForegroundColor Cyan
Write-Host "  PUT    $BaseUrl/api/reference-tables/beispiel_produkttyp/rows" -ForegroundColor Cyan
Write-Host "  DELETE $BaseUrl/api/reference-tables/beispiel_produkttyp" -ForegroundColor Cyan
Write-Host ""

# Summary
Write-Host "=== Summary ===" -ForegroundColor Green
Write-Host ""
Write-Host "This script demonstrated:" -ForegroundColor White
Write-Host "  ✓ Getting workload information" -ForegroundColor Green
Write-Host "  ✓ Health check" -ForegroundColor Green
Write-Host "  ✓ Creating reference tables" -ForegroundColor Green
Write-Host "  ✓ Synchronizing reference tables with data" -ForegroundColor Green
Write-Host "  ✓ Reading reference table data" -ForegroundColor Green
Write-Host "  ✓ Updating reference table rows" -ForegroundColor Green
Write-Host "  ✓ Validating configurations" -ForegroundColor Green
Write-Host ""
Write-Host "For more information, see:" -ForegroundColor Yellow
Write-Host "  - docs/WORKLOAD_GUIDE_DE.md (German workload guide)" -ForegroundColor Cyan
Write-Host "  - docs/FABRIC-INTEGRATION.md (Integration guide)" -ForegroundColor Cyan
Write-Host "  - README.md (Project overview)" -ForegroundColor Cyan
Write-Host ""
