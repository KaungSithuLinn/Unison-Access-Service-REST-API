# Unison API Testing Script
# Set environment variables for API testing

$env:UNISON_API_TOKEN = "7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3"
$env:UNISON_API_BASE_URL = "http://192.168.10.206:9003/Unison.AccessService"

Write-Host "Starting Unison API Validation Tests..." -ForegroundColor Green
Write-Host "API Token: $env:UNISON_API_TOKEN" -ForegroundColor Yellow
Write-Host "Base URL: $env:UNISON_API_BASE_URL" -ForegroundColor Yellow
Write-Host ""

# Run the test script
& "C:/Users/Kaung Sithu Linn/AppData/Local/Programs/Python/Python313/python.exe" test_api_fixes.py

Write-Host ""
Write-Host "API Validation Tests Completed!" -ForegroundColor Green
