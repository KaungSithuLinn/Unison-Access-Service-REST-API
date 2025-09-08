# UpdateCard API Example using PowerShell
# This script demonstrates how to call the UpdateCard REST endpoint using PowerShell

# Configuration
$BaseUrl = "http://192.168.10.206:5001"
$UnisonToken = "595d799a-example-token-placeholder"

# Common headers
$Headers = @{
    "Content-Type" = "application/json"
    "Unison-Token" = $UnisonToken
}

Write-Host "=== Unison REST Adapter API Examples ===" -ForegroundColor Green
Write-Host ""

# Health check
Write-Host "=== Health Check ===" -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/api/health/ping" -Method GET -ContentType "application/json"
    Write-Host "Response:" -ForegroundColor Cyan
    $healthResponse | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Update Card ===" -ForegroundColor Yellow

# Update card request
$updateCardRequest = @{
    cardId         = "TEST123"
    userName       = "john.doe"
    firstName      = "John"
    lastName       = "Doe"
    email          = "john.doe@example.com"
    department     = "Engineering"
    title          = "Software Engineer"
    isActive       = $true
    expirationDate = "2025-12-31T23:59:59Z"
    customFields   = @{
        buildingAccess   = "A,B,C"
        emergencyContact = "555-1234"
    }
} | ConvertTo-Json -Depth 3

try {
    $updateResponse = Invoke-RestMethod -Uri "$BaseUrl/api/cards/update" -Method PUT -Headers $Headers -Body $updateCardRequest
    Write-Host "Response:" -ForegroundColor Cyan
    $updateResponse | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Test Unauthorized (without token) ===" -ForegroundColor Yellow

# Test unauthorized request
$noTokenHeaders = @{
    "Content-Type" = "application/json"
}

$simpleRequest = @{
    cardId   = "TEST123"
    userName = "john.doe"
} | ConvertTo-Json

try {
    $unauthorizedResponse = Invoke-RestMethod -Uri "$BaseUrl/api/cards/update" -Method PUT -Headers $noTokenHeaders -Body $simpleRequest
    Write-Host "Unexpected success - should have been unauthorized" -ForegroundColor Red
}
catch {
    Write-Host "Expected unauthorized error:" -ForegroundColor Cyan
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Cyan
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "=== Get Card ===" -ForegroundColor Yellow

try {
    $getCardResponse = Invoke-RestMethod -Uri "$BaseUrl/api/cards/TEST123" -Method GET -Headers @{ "Unison-Token" = $UnisonToken }
    Write-Host "Response:" -ForegroundColor Cyan
    $getCardResponse | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Legacy Update Card Endpoint ===" -ForegroundColor Yellow

try {
    $legacyResponse = Invoke-RestMethod -Uri "$BaseUrl/updatecard" -Method POST -Headers $Headers -Body $updateCardRequest
    Write-Host "Response:" -ForegroundColor Cyan
    $legacyResponse | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green
Write-Host ""
Write-Host "Examples completed. Update the BaseUrl and UnisonToken variables above with your actual values." -ForegroundColor Magenta
