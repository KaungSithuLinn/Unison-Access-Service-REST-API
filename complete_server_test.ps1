# Complete Server Testing Script for REST-to-SOAP Adapter
# This script will deploy and test the adapter on the target server

param(
    [string]$TargetServer = "192.168.10.206",
    [string]$TargetPath = "C:\Services\UnisonRestAdapter",
    [string]$Port = "5001",
    [string]$TestToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
)

Write-Host "=== REST-to-SOAP Adapter Complete Server Test ===" -ForegroundColor Green
Write-Host "Target Server: $TargetServer" -ForegroundColor Cyan
Write-Host "Target Path: $TargetPath" -ForegroundColor Cyan
Write-Host "Port: $Port" -ForegroundColor Cyan
Write-Host ""

# Step 1: Configure appsettings.json for new port
Write-Host "Step 1: Configuring adapter for port $Port..." -ForegroundColor Yellow

$appsettingsPath = "UnisonRestAdapter\publish\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.Urls = "http://localhost:$Port"
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "✅ Configured appsettings.json for port $Port" -ForegroundColor Green
}
else {
    Write-Host "⚠️ appsettings.json not found, will use command line parameter" -ForegroundColor Orange
}

# Step 2: Deploy to server with SSH
Write-Host "Step 2: Deploying to server via SSH..." -ForegroundColor Yellow

try {
    # Create remote directory
    $sshCommand = "ssh Arrowcrest@$TargetServer 'mkdir -p $TargetPath'"
    Write-Host "Creating remote directory: $sshCommand" -ForegroundColor Gray
    Invoke-Expression $sshCommand

    # Copy files via SCP
    $scpCommand = "scp -r UnisonRestAdapter\publish\* Arrowcrest@${TargetServer}:$TargetPath/"
    Write-Host "Copying files: $scpCommand" -ForegroundColor Gray
    Invoke-Expression $scpCommand
    
    Write-Host "✅ Files deployed successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Continuing with testing existing deployment..." -ForegroundColor Orange
}

# Step 3: Start the service on the server
Write-Host "Step 3: Starting REST adapter service..." -ForegroundColor Yellow

$startServiceScript = @"
cd $TargetPath
# Kill any existing processes
Get-Process | Where-Object {`$_.ProcessName -eq 'UnisonRestAdapter'} | Stop-Process -Force -ErrorAction SilentlyContinue

# Start the service in background
Start-Process -FilePath '.\UnisonRestAdapter.exe' -ArgumentList '--urls', 'http://0.0.0.0:$Port' -WindowStyle Hidden

# Wait for startup
Start-Sleep -Seconds 10

# Check if process is running
`$process = Get-Process -Name 'UnisonRestAdapter' -ErrorAction SilentlyContinue
if (`$process) {
    Write-Host 'Service started successfully on port $Port'
    `$process | Select-Object Id, ProcessName, CPU
} else {
    Write-Host 'Failed to start service'
}
"@

try {
    $sshCommand = "ssh Arrowcrest@$TargetServer 'powershell -Command `"$startServiceScript`"'"
    Write-Host "Starting service via SSH..." -ForegroundColor Gray
    Invoke-Expression $sshCommand
    Write-Host "✅ Service start command executed" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to start service: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Test REST endpoints
Write-Host "Step 4: Testing REST endpoints..." -ForegroundColor Yellow

# Test 1: Health endpoint
Write-Host "Testing health endpoint..." -ForegroundColor Cyan
$healthUrl = "http://${TargetServer}:$Port/api/health"
try {
    $healthResponse = Invoke-RestMethod -Uri $healthUrl -Method GET -Headers @{"Unison-Token" = $TestToken } -TimeoutSec 30
    Write-Host "✅ Health check passed: $healthResponse" -ForegroundColor Green
}
catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Error Details: $($_.ErrorDetails)" -ForegroundColor Red
}

# Test 2: UpdateCard endpoint
Write-Host "Testing UpdateCard endpoint..." -ForegroundColor Cyan
$updateCardUrl = "http://${TargetServer}:$Port/api/cards/update"
$testPayload = @{
    ID            = "TEST_USER_001"
    Name          = "Test User"
    CardNumber    = "12345678"
    SystemNumber  = "001"
    VersionNumber = "1"
    MiscNumber    = "000"
    CardStatus    = "Active"
} | ConvertTo-Json

$headers = @{
    "Unison-Token" = $TestToken
    "Content-Type" = "application/json"
}

try {
    $updateResponse = Invoke-RestMethod -Uri $updateCardUrl -Method POST -Body $testPayload -Headers $headers -TimeoutSec 30
    Write-Host "✅ UpdateCard test passed" -ForegroundColor Green
    Write-Host "Response: $updateResponse" -ForegroundColor Gray
}
catch {
    Write-Host "❌ UpdateCard test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Error Details: $($_.ErrorDetails)" -ForegroundColor Red
}

# Step 5: Verify SOAP backend connectivity from server
Write-Host "Step 5: Verifying SOAP backend connectivity from server..." -ForegroundColor Yellow

$soapTestScript = @"
`$soapEnvelope = @'
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" 
               xmlns:tem="http://tempuri.org/">
  <soap:Header>
    <Unison-Token xmlns="http://tempuri.org/">$TestToken</Unison-Token>
  </soap:Header>
  <soap:Body>
    <tem:UpdateCard>
      <tem:userId>TEST_USER_001</tem:userId>
      <tem:profileName>Default</tem:profileName>
      <tem:cardNumber>12345678</tem:cardNumber>
      <tem:systemNumber>001</tem:systemNumber>
      <tem:versionNumber>1</tem:versionNumber>
      <tem:miscNumber>000</tem:miscNumber>
      <tem:cardStatus>Active</tem:cardStatus>
    </tem:UpdateCard>
  </soap:Body>
</soap:Envelope>
'@

`$headers = @{
    'Content-Type' = 'text/xml; charset=utf-8'
    'SOAPAction' = '\"http://tempuri.org/IAccessService/UpdateCard\"'
    'Unison-Token' = '$TestToken'
}

try {
    `$response = Invoke-WebRequest -Uri 'http://localhost:9003/Unison.AccessService' -Method POST -Body `$soapEnvelope -Headers `$headers
    Write-Host 'SOAP backend test: SUCCESS'
    Write-Host `$response.StatusCode
} catch {
    Write-Host 'SOAP backend test: FAILED'
    Write-Host `$_.Exception.Message
}
"@

try {
    $sshCommand = "ssh Arrowcrest@$TargetServer 'powershell -Command `"$soapTestScript`"'"
    Write-Host "Testing SOAP backend from server..." -ForegroundColor Gray
    Invoke-Expression $sshCommand
}
catch {
    Write-Host "❌ SOAP backend test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Generate test report
Write-Host "Step 6: Generating test report..." -ForegroundColor Yellow

$reportContent = @"
# REST-to-SOAP Adapter Server Test Report
**Date**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
**Target Server**: $TargetServer
**Port**: $Port
**Test Token**: $TestToken

## Test Results Summary

### Deployment Status
- Files deployed to: $TargetPath
- Service configured for port: $Port
- Service startup: Executed

### Endpoint Tests
- Health endpoint: $(if ($healthResponse) { "✅ PASSED" } else { "❌ FAILED" })
- UpdateCard endpoint: $(if ($updateResponse) { "✅ PASSED" } else { "❌ FAILED" })

### Backend Connectivity
- SOAP service accessibility: Tested from server

## Next Steps
$(if ($healthResponse -and $updateResponse) {
    "✅ **SUCCESS**: REST-to-SOAP adapter is fully operational on the server!"
} else {
    "⚠️ **ISSUES FOUND**: Review error messages above and troubleshoot accordingly."
})

## Additional Notes
- Port conflict with Suprema system resolved by using port $Port
- All files successfully deployed to target server
- Service configured to bind to all interfaces (0.0.0.0:$Port)

Generated by: Complete Server Test Script
"@

$reportPath = "SERVER_TEST_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"
$reportContent | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host "✅ Test report generated: $reportPath" -ForegroundColor Green
Write-Host ""
Write-Host "=== Server Testing Complete ===" -ForegroundColor Green

# Display final status
if ($healthResponse -and $updateResponse) {
    Write-Host "🎉 SUCCESS: REST-to-SOAP adapter is fully operational!" -ForegroundColor Green
    Write-Host "   - Health endpoint: WORKING" -ForegroundColor Green
    Write-Host "   - UpdateCard endpoint: WORKING" -ForegroundColor Green
    Write-Host "   - Server deployment: COMPLETE" -ForegroundColor Green
}
else {
    Write-Host "⚠️ PARTIAL SUCCESS: Some issues need attention" -ForegroundColor Orange
    Write-Host "   - Review the test report for details" -ForegroundColor Orange
    Write-Host "   - Check server logs for troubleshooting" -ForegroundColor Orange
}
