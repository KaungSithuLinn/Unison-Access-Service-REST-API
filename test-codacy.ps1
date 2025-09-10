# Test Codacy Docker run with simplified approach
$ErrorActionPreference = "Continue"

Write-Host "Starting Codacy test run..." -ForegroundColor Cyan
Write-Host "Current directory: $(Get-Location)" -ForegroundColor Gray

# Test Docker with simple command first
Write-Host "`nTesting Docker hello-world..." -ForegroundColor Yellow
docker run --rm hello-world

Write-Host "`nTesting Codacy version..." -ForegroundColor Yellow
docker run --rm codacy/codacy-analysis-cli --version

Write-Host "`nTesting Codacy analyze with current directory..." -ForegroundColor Yellow
$currentDir = (Get-Location).Path
Write-Host "Mounting: $currentDir -> /src" -ForegroundColor Gray

docker run --rm -v "${currentDir}:/src" codacy/codacy-analysis-cli analyze --verbose

Write-Host "`nTest completed." -ForegroundColor Cyan
