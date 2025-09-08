# Unison REST-to-SOAP Adapter Test Script
# PowerShell script to test the adapter endpoints

param(
    [string]$BaseUrl = "http://localhost:5000/api",
    [string]$Token = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
)

Write-Host "🚀 Testing Unison REST-to-SOAP Adapter" -ForegroundColor Green
Write-Host "=" * 50

# Function to make HTTP requests with proper error handling
function Invoke-TestRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Method     = $Method
            Uri        = $Uri
            Headers    = $Headers
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
            $params.ContentType = "application/json"
        }
        
        $response = Invoke-RestMethod @params
        return @{
            Success    = $true
            StatusCode = 200
            Data       = $response
        }
    }
    catch {
        return @{
            Success    = $false
            StatusCode = $_.Exception.Response.StatusCode.value__
            Error      = $_.Exception.Message
            Response   = $_.Exception.Response
        }
    }
}

# Test 1: Health Check
Write-Host "`n1. Testing Health Check..." -ForegroundColor Yellow

$healthHeaders = @{
    "Unison-Token" = $Token
}

$healthResult = Invoke-TestRequest -Method "GET" -Uri "$BaseUrl/health" -Headers $healthHeaders

if ($healthResult.Success) {
    $isHealthy = $healthResult.Data.isHealthy
    $healthStatus = if ($isHealthy) { "✅ Healthy" } else { "❌ Unhealthy" }
    Write-Host "   Status: 200" -ForegroundColor Green
    Write-Host "   Health: $healthStatus"
    Write-Host "   Message: $($healthResult.Data.message)"
}
else {
    Write-Host "   Status: $($healthResult.StatusCode)" -ForegroundColor Red
    Write-Host "   Error: $($healthResult.Error)"
}

# Test 2: UpdateCard
Write-Host "`n2. Testing UpdateCard..." -ForegroundColor Yellow

$updateHeaders = @{
    "Unison-Token" = $Token
}

$updateBody = @{
    cardId         = "TEST_CARD_12345"
    userName       = "TEST_USER_001"
    firstName      = "John"
    lastName       = "Doe"
    email          = "john.doe@test.com"
    department     = "IT"
    title          = "Software Engineer"
    isActive       = $true
    expirationDate = "2025-12-31T23:59:59Z"
    customFields   = @{
        location     = "Building A"
        access_level = "Standard"
    }
}

$updateResult = Invoke-TestRequest -Method "PUT" -Uri "$BaseUrl/cards/update" -Headers $updateHeaders -Body $updateBody

if ($updateResult.Success) {
    $success = $updateResult.Data.success
    $successStatus = if ($success) { "✅" } else { "❌" }
    Write-Host "   Status: 200" -ForegroundColor Green
    Write-Host "   Success: $successStatus"
    Write-Host "   Message: $($updateResult.Data.message)"
    Write-Host "   Card ID: $($updateResult.Data.cardId)"
}
else {
    Write-Host "   Status: $($updateResult.StatusCode)" -ForegroundColor Red
    Write-Host "   Error: $($updateResult.Error)"
}

# Test 3: GetCard
Write-Host "`n3. Testing GetCard..." -ForegroundColor Yellow

$getHeaders = @{
    "Unison-Token" = $Token
}

$getResult = Invoke-TestRequest -Method "GET" -Uri "$BaseUrl/cards/TEST_CARD_12345" -Headers $getHeaders

if ($getResult.Success) {
    $success = $getResult.Data.success
    $successStatus = if ($success) { "✅" } else { "❌" }
    Write-Host "   Status: 200" -ForegroundColor Green
    Write-Host "   Success: $successStatus"
    Write-Host "   User: $($getResult.Data.userName)"
}
else {
    Write-Host "   Status: $($getResult.StatusCode)" -ForegroundColor Red
    Write-Host "   Error: $($getResult.Error)"
}

# Test 4: Authentication
Write-Host "`n🔐 Testing Authentication..." -ForegroundColor Yellow

# Test without token
$noTokenResult = Invoke-TestRequest -Method "GET" -Uri "$BaseUrl/health"
$authStatus = if ($noTokenResult.StatusCode -eq 401) { "✅ Unauthorized" } else { "❌ Should be unauthorized" }
Write-Host "   No token: $($noTokenResult.StatusCode) - $authStatus"

# Test with invalid token
$invalidHeaders = @{
    "Unison-Token" = "invalid-token"
}
$invalidResult = Invoke-TestRequest -Method "GET" -Uri "$BaseUrl/health" -Headers $invalidHeaders
Write-Host "   Invalid token: $($invalidResult.StatusCode)"

Write-Host "`n" + ("=" * 50)
Write-Host "🎯 Test completed!" -ForegroundColor Green

# Display summary
Write-Host "`n📊 Test Summary:" -ForegroundColor Cyan
Write-Host "   Base URL: $BaseUrl"
Write-Host "   Token: $Token"
Write-Host "   Health Check: $(if ($healthResult.Success) { '✅ Passed' } else { '❌ Failed' })"
Write-Host "   UpdateCard: $(if ($updateResult.Success) { '✅ Passed' } else { '❌ Failed' })"
Write-Host "   GetCard: $(if ($getResult.Success) { '✅ Passed' } else { '❌ Failed' })"
