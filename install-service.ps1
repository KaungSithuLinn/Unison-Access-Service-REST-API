# Unison REST Adapter Service Installation Script
# Run as Administrator

param(
    [string]$ServicePath = "C:\Services\UnisonRestAdapter",
    [string]$SourcePath = ".\UnisonRestAdapter\bin\Release\net9.0\publish",
    [switch]$Build = $true
)

$serviceName = "UnisonRestAdapter"
$serviceDisplayName = "Unison REST Adapter Service"
$serviceDescription = "REST-to-SOAP adapter for Unison Access Service"
$executablePath = Join-Path $ServicePath "UnisonRestAdapter.exe"

Write-Host "Installing Unison REST Adapter as Windows Service..." -ForegroundColor Green
Write-Host "Service Path: $ServicePath" -ForegroundColor Cyan

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

# Build the application if requested
if ($Build) {
    Write-Host "Building application..." -ForegroundColor Yellow
    dotnet publish .\UnisonRestAdapter\UnisonRestAdapter.csproj -c Release -o $SourcePath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
}

# Create service directory
if (!(Test-Path $ServicePath)) {
    Write-Host "Creating service directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $ServicePath -Force
}

# Copy files to service directory
Write-Host "Copying application files..." -ForegroundColor Yellow
Copy-Item -Path "$SourcePath\*" -Destination $ServicePath -Recurse -Force

# Stop and remove existing service if it exists
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping existing service..." -ForegroundColor Yellow
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Write-Host "Removing existing service..." -ForegroundColor Yellow
    sc.exe delete $serviceName
    Start-Sleep -Seconds 3
}

# Create the service
Write-Host "Creating new service..." -ForegroundColor Yellow
sc.exe create $serviceName binPath= $executablePath start= auto DisplayName= $serviceDisplayName
sc.exe description $serviceName $serviceDescription

# Configure service recovery options
Write-Host "Configuring service recovery options..." -ForegroundColor Yellow
sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/10000/restart/30000

# Set service to start automatically and configure delayed start
sc.exe config $serviceName start= delayed-auto

# Create event log source if it doesn't exist
Write-Host "Configuring event log source..." -ForegroundColor Yellow
try {
    New-EventLog -LogName Application -Source "UnisonRestAdapter" -ErrorAction SilentlyContinue
}
catch {
    # Source may already exist
}

Write-Host "Service installed successfully!" -ForegroundColor Green
Write-Host "Service Name: $serviceName" -ForegroundColor Cyan
Write-Host "Service Path: $executablePath" -ForegroundColor Cyan
Write-Host "Service will start automatically on system boot (delayed)" -ForegroundColor Cyan
Write-Host "Health check endpoint: http://localhost:5203/health" -ForegroundColor Cyan
Write-Host "API documentation: http://localhost:5203/api/docs" -ForegroundColor Cyan

# Optional: Start the service now
$startNow = Read-Host "Start the service now? (y/n)"
if ($startNow -eq 'y' -or $startNow -eq 'Y') {
    Write-Host "Starting service..." -ForegroundColor Yellow
    Start-Service -Name $serviceName
    Start-Sleep -Seconds 5
    $serviceStatus = Get-Service -Name $serviceName
    if ($serviceStatus.Status -eq 'Running') {
        Write-Host "Service started successfully!" -ForegroundColor Green
        Write-Host "Testing health endpoint..." -ForegroundColor Yellow
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5203/health" -UseBasicParsing -TimeoutSec 10
            Write-Host "Health check: $($response.StatusCode) - Service is healthy!" -ForegroundColor Green
        }
        catch {
            Write-Warning "Health check failed: $($_.Exception.Message)"
        }
    }
    else {
        Write-Warning "Service failed to start. Status: $($serviceStatus.Status)"
    }
}
