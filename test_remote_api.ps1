# Simple Remote REST API Test Script
param(
    [string]$ServerIP = "192.168.10.206",
    [string]$Port = "5001",
    [string]$Token = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
)

Write-Host "=== Testing REST API on Remote Server ===" -ForegroundColor Green
Write-Host "Server: ${ServerIP}:${Port}" -ForegroundColor Cyan
Write-Host "Token: $Token" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Check
Write-Host "1. Testing Health Endpoint..." -ForegroundColor Yellow
$healthUrl = "http://${ServerIP}:${Port}/api/health"
$headers = @{"Unison-Token" = $Token }

try {
    $healthResult = Invoke-RestMethod -Uri $healthUrl -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ HEALTH CHECK: SUCCESS" -ForegroundColor Green
    Write-Host "   Response: $healthResult" -ForegroundColor Gray
    $healthPassed = $true
}
catch {
    Write-Host "❌ HEALTH CHECK: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    $healthPassed = $false
}

Write-Host ""

# Test 2: UpdateCard Endpoint
Write-Host "2. Testing UpdateCard Endpoint..." -ForegroundColor Yellow
$updateUrl = "http://${ServerIP}:${Port}/api/cards/update"
$payload = @{
    ID            = "TEST123"
    Name          = "Test User"
    CardNumber    = "12345678"
    SystemNumber  = "001"
    VersionNumber = "1"
    MiscNumber    = "000"
    CardStatus    = "Active"
} | ConvertTo-Json

$updateHeaders = @{
    "Unison-Token" = $Token
    "Content-Type" = "application/json"
}

try {
    $updateResult = Invoke-RestMethod -Uri $updateUrl -Method POST -Body $payload -Headers $updateHeaders -TimeoutSec 15
    Write-Host "✅ UPDATECARD TEST: SUCCESS" -ForegroundColor Green
    Write-Host "   Response: $updateResult" -ForegroundColor Gray
    $updatePassed = $true
}
catch {
    Write-Host "❌ UPDATECARD TEST: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    $updatePassed = $false
}

Write-Host ""

# Test 3: Service Availability Check
Write-Host "3. Testing Service Availability..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName $ServerIP -Port $Port -InformationLevel Quiet
    if ($connection) {
        Write-Host "✅ PORT CONNECTIVITY: SUCCESS" -ForegroundColor Green
        Write-Host "   Port $Port is accessible on $ServerIP" -ForegroundColor Gray
        $portPassed = $true
    }
    else {
        Write-Host "❌ PORT CONNECTIVITY: FAILED" -ForegroundColor Red
        Write-Host "   Port $Port is not accessible on $ServerIP" -ForegroundColor Red
        $portPassed = $false
    }
}
catch {
    Write-Host "❌ PORT CONNECTIVITY: ERROR" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    $portPassed = $false
}

Write-Host ""

# Summary Report
Write-Host "=== TEST RESULTS SUMMARY ===" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host ""

if ($healthPassed -and $updatePassed -and $portPassed) {
    Write-Host "🎉 ALL TESTS PASSED! 🎉" -ForegroundColor Green
    Write-Host "   The REST-to-SOAP adapter is fully operational on the server!" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ Health endpoint working" -ForegroundColor Green
    Write-Host "✅ UpdateCard endpoint working" -ForegroundColor Green  
    Write-Host "✅ Network connectivity confirmed" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 DEPLOYMENT STATUS: COMPLETE AND FUNCTIONAL" -ForegroundColor Green
}
else {
    Write-Host "⚠️ SOME TESTS FAILED" -ForegroundColor Orange
    Write-Host ""
    Write-Host "Health Check: $(if ($healthPassed) { '✅ PASSED' } else { '❌ FAILED' })" -ForegroundColor (if ($healthPassed) { 'Green' } else { 'Red' })
    Write-Host "UpdateCard Test: $(if ($updatePassed) { '✅ PASSED' } else { '❌ FAILED' })" -ForegroundColor (if ($updatePassed) { 'Green' } else { 'Red' })
    Write-Host "Port Connectivity: $(if ($portPassed) { '✅ PASSED' } else { '❌ FAILED' })" -ForegroundColor (if ($portPassed) { 'Green' } else { 'Red' })
    Write-Host ""
    Write-Host "🔧 NEXT STEPS:" -ForegroundColor Yellow
    if (-not $portPassed) {
        Write-Host "   - Check if the service is running on the server" -ForegroundColor Orange
        Write-Host "   - Verify firewall settings for port $Port" -ForegroundColor Orange
    }
    if ($portPassed -and (-not $healthPassed -or -not $updatePassed)) {
        Write-Host "   - Check service logs for application errors" -ForegroundColor Orange
        Write-Host "   - Verify SOAP backend connectivity" -ForegroundColor Orange
        Write-Host "   - Check authentication token configuration" -ForegroundColor Orange
    }
}

Write-Host ""
Write-Host "Test completed at $(Get-Date)" -ForegroundColor Gray
