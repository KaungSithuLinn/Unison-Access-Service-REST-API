# SSH Server Deployment and Testing Script
Write-Host "=== Deploying and Starting REST Adapter via SSH ===" -ForegroundColor Green

# Step 1: Connect to server and start the service
Write-Host "Step 1: Starting REST adapter service on server..." -ForegroundColor Yellow

$startCommand = @'
# Navigate to the service directory
cd /mnt/c/Services/UnisonRestAdapter

# Kill any existing processes
pkill -f UnisonRestAdapter.exe

# Start the service on port 5001
nohup ./UnisonRestAdapter.exe --urls "http://0.0.0.0:5001" > service.log 2>&1 &

# Wait a moment for startup
sleep 5

# Check if process is running
if pgrep -f UnisonRestAdapter.exe > /dev/null; then
    echo "✅ Service started successfully"
    ps aux | grep UnisonRestAdapter | grep -v grep
else
    echo "❌ Service failed to start"
    echo "Log contents:"
    tail -n 20 service.log
fi
'@

Write-Host "Executing SSH command to start service..." -ForegroundColor Cyan
try {
    ssh Arrowcrest@192.168.10.206 $startCommand
    Write-Host "✅ SSH command executed" -ForegroundColor Green
}
catch {
    Write-Host "❌ SSH command failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 2: Test the service after a brief delay
Write-Host "`nStep 2: Testing service after startup..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Test health endpoint
Write-Host "Testing health endpoint..." -ForegroundColor Cyan
try {
    $healthResponse = Invoke-RestMethod -Uri "http://192.168.10.206:5001/api/health" -Method GET -Headers @{"Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7" } -TimeoutSec 15
    Write-Host "✅ Health check: SUCCESS" -ForegroundColor Green
    Write-Host "Response: $healthResponse" -ForegroundColor Gray
}
catch {
    Write-Host "❌ Health check: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test UpdateCard endpoint
Write-Host "Testing UpdateCard endpoint..." -ForegroundColor Cyan
$payload = @{
    ID            = "TEST_USER_001"
    Name          = "Server Test User"
    CardNumber    = "87654321"
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
}
catch {
    Write-Host "❌ UpdateCard test: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Deployment and Testing Complete ===" -ForegroundColor Green
