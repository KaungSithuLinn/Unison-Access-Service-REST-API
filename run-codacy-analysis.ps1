# PowerShell script to run Codacy CLI v2 analysis using Docker
# Usage: .\run-codacy-analysis.ps1

$ErrorActionPreference = "Stop"

# Create logs directory if it doesn't exist
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" -Force | Out-Null
}

# Ensure Docker is running
try {
    Write-Host "Checking Docker status..." -ForegroundColor Yellow
    docker info | Out-Null
    Write-Host "Docker is running." -ForegroundColor Green
}
catch {
    Write-Host "Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    exit 1
}

# Check if codacy.yml exists
if (!(Test-Path "codacy.yml")) {
    Write-Host "Warning: codacy.yml not found. Analysis will use default settings." -ForegroundColor Yellow
}

# Get current path for Docker mount
$currentPath = (Get-Location).ProviderPath
Write-Host "Current project path: $currentPath" -ForegroundColor Cyan

# Dump Codacy-related environment variables to a timed log to help diagnose missing credentials
$envLog = "logs\env-codacy-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Write-Host "Writing Codacy-related environment variables to: $envLog" -ForegroundColor Gray
[Environment]::GetEnvironmentVariables().GetEnumerator() |
Where-Object { $_.Key -like 'CODACY*' -or $_.Key -like 'CODACY_*' } |
ForEach-Object { "{0}={1}" -f $_.Key, $_.Value } | Out-File -FilePath $envLog -Encoding utf8

# Run Codacy CLI analysis with verbose output and logging
Write-Host "Running Codacy CLI analysis with Docker..." -ForegroundColor Cyan
Write-Host "Command: docker run --rm -e CODACY_PROJECT_TOKEN=$env:CODACY_PROJECT_TOKEN -v `"${currentPath}:/src`" codacy/codacy-analysis-cli analyze" -ForegroundColor Gray

# Capture output to both console and log file
$logFile = "logs\codacy-analyze-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Write-Host "Logging output to: $logFile" -ForegroundColor Gray

docker run --rm -e CODACY_PROJECT_TOKEN=$env:CODACY_PROJECT_TOKEN -v "${currentPath}:/src" codacy/codacy-analysis-cli analyze 2>&1 | Tee-Object -FilePath $logFile

$exitCode = $LASTEXITCODE
Write-Host "Docker command completed with exit code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })

if ($exitCode -eq 0) {
    Write-Host "Codacy analysis completed successfully." -ForegroundColor Green
    Write-Host "Log saved to: $logFile" -ForegroundColor Cyan
}
else {
    Write-Host "Codacy analysis failed. Check the output above and log file for details." -ForegroundColor Red
    Write-Host "Log file: $logFile" -ForegroundColor Cyan
    exit $exitCode
}
