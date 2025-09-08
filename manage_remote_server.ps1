# Remote Server Management Script
Write-Host "=== Remote Server Management for REST Adapter ===" -ForegroundColor Green

# Function to execute PowerShell commands on remote server via SSH
function Invoke-SSHPowerShell {
    param([string]$Command)
    
    $escapedCommand = $Command -replace '"', '`"'
    $sshCommand = "ssh Arrowcrest@192.168.10.206 `"powershell -Command `"`"$escapedCommand`"`"`""
    
    Write-Host "Executing: $Command" -ForegroundColor Gray
    try {
        Invoke-Expression $sshCommand
        return $true
    }
    catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Step 1: Check deployment directory and files
Write-Host "Step 1: Checking deployment directory..." -ForegroundColor Yellow

$checkCommand = @"
if (Test-Path 'C:\Services\UnisonRestAdapter') {
    Write-Host 'Deployment directory exists'
    Get-ChildItem -Path 'C:\Services\UnisonRestAdapter' | Select-Object Name, Length, LastWriteTime
} else {
    Write-Host 'Deployment directory not found'
}
"@

Invoke-SSHPowerShell -Command $checkCommand

# Step 2: Check for any running instances
Write-Host "`nStep 2: Checking for running instances..." -ForegroundColor Yellow

$processCommand = @"
`$processes = Get-Process -Name 'UnisonRestAdapter' -ErrorAction SilentlyContinue
if (`$processes) {
    Write-Host 'Found running instances:'
    `$processes | Format-Table Id, ProcessName, CPU, WorkingSet
    Write-Host 'Stopping existing processes...'
    `$processes | Stop-Process -Force
} else {
    Write-Host 'No running instances found'
}
"@

Invoke-SSHPowerShell -Command $processCommand

# Step 3: Check port availability
Write-Host "`nStep 3: Checking port availability..." -ForegroundColor Yellow

$portCommand = @"
`$port = 5001
`$listener = Get-NetTCPConnection -LocalPort `$port -ErrorAction SilentlyContinue
if (`$listener) {
    Write-Host 'Port `$port is in use:'
    `$listener | Format-Table LocalAddress, LocalPort, State, OwningProcess
} else {
    Write-Host 'Port `$port is available'
}
"@

Invoke-SSHPowerShell -Command $portCommand

# Step 4: Start the REST adapter service
Write-Host "`nStep 4: Starting REST adapter service..." -ForegroundColor Yellow

$startCommand = @"
Set-Location 'C:\Services\UnisonRestAdapter'
`$process = Start-Process -FilePath '.\UnisonRestAdapter.exe' -ArgumentList '--urls', 'http://0.0.0.0:5001' -PassThru -WindowStyle Hidden
if (`$process) {
    Write-Host 'Service started with PID: ' `$process.Id
    Start-Sleep -Seconds 5
    if (-not `$process.HasExited) {
        Write-Host 'Service is running successfully'
    } else {
        Write-Host 'Service exited unexpectedly'
    }
} else {
    Write-Host 'Failed to start service'
}
"@

Invoke-SSHPowerShell -Command $startCommand

# Step 5: Wait and test the service
Write-Host "`nStep 5: Testing REST endpoints..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Test health endpoint
Write-Host "Testing health endpoint..." -ForegroundColor Cyan
try {
    $healthResponse = Invoke-RestMethod -Uri "http://192.168.10.206:5001/api/health" -Method GET -Headers @{"Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7" } -TimeoutSec 10
    Write-Host "✅ Health check: SUCCESS" -ForegroundColor Green
    Write-Host "Response: $healthResponse" -ForegroundColor Gray
    $healthWorking = $true
}
catch {
    Write-Host "❌ Health check: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    $healthWorking = $false
}

# Test UpdateCard endpoint if health check passed
if ($healthWorking) {
    Write-Host "Testing UpdateCard endpoint..." -ForegroundColor Cyan
    $payload = @{
        ID            = "REMOTE_TEST_001"
        Name          = "Remote Test User"
        CardNumber    = "11223344"
        SystemNumber  = "001"
        VersionNumber = "1"
        MiscNumber    = "000"
        CardStatus    = "Active"
    } | ConvertTo-Json

    $headers = @{
        "Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
        "Content-Type" = "application/json"
    }

    try {
        $updateResponse = Invoke-RestMethod -Uri "http://192.168.10.206:5001/api/cards/update" -Method POST -Body $payload -Headers $headers -TimeoutSec 15
        Write-Host "✅ UpdateCard test: SUCCESS" -ForegroundColor Green
        Write-Host "Response: $updateResponse" -ForegroundColor Gray
        $updateWorking = $true
    }
    catch {
        Write-Host "❌ UpdateCard test: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        $updateWorking = $false
    }
}

# Final status report
Write-Host "`n=== FINAL STATUS REPORT ===" -ForegroundColor White -BackgroundColor DarkBlue

if ($healthWorking -and $updateWorking) {
    Write-Host "🎉 REST-TO-SOAP ADAPTER FULLY OPERATIONAL! 🎉" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ Service deployed and running on server" -ForegroundColor Green
    Write-Host "✅ Health endpoint responding correctly" -ForegroundColor Green
    Write-Host "✅ UpdateCard endpoint functional" -ForegroundColor Green
    Write-Host "✅ REST-to-SOAP bridge working end-to-end" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 DEPLOYMENT STATUS: COMPLETE AND VERIFIED ✅" -ForegroundColor Green
    Write-Host "📍 Service URL: http://192.168.10.206:5001" -ForegroundColor Cyan
    Write-Host "🔐 Authentication: Unison-Token header required" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your statements to Minh are now 100% ACCURATE! ✅" -ForegroundColor Green
}
else {
    Write-Host "⚠️ DEPLOYMENT NEEDS ATTENTION" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Service Status: $(if ($healthWorking) { '✅ RUNNING' } else { '❌ NOT RESPONDING' })" -ForegroundColor (if ($healthWorking) { 'Green' } else { 'Red' })
    Write-Host "Health Check: $(if ($healthWorking) { '✅ PASSED' } else { '❌ FAILED' })" -ForegroundColor (if ($healthWorking) { 'Green' } else { 'Red' })
    Write-Host "UpdateCard: $(if ($updateWorking) { '✅ PASSED' } else { '❌ FAILED' })" -ForegroundColor (if ($updateWorking) { 'Green' } else { 'Red' })
}

Write-Host "`nTest completed at $(Get-Date)" -ForegroundColor Gray
