# Unison REST Adapter - Windows Service Installation Script
# Run as Administrator

param(
    [string]$ServiceName = "UnisonRestAdapter",
    [string]$DisplayName = "Unison Access Service REST Adapter",
    [string]$Description = "REST-to-SOAP adapter for Unison Access Service",
    [string]$BinaryPath = "C:\Services\UnisonRestAdapter\UnisonRestAdapter.exe",
    [string]$StartupType = "Automatic"
)

Write-Host "🔧 Installing Unison REST Adapter as Windows Service" -ForegroundColor Green
Write-Host "=" * 60

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator!"
    exit 1
}

# Create service directory if it doesn't exist
$serviceDir = Split-Path $BinaryPath -Parent
if (!(Test-Path $serviceDir)) {
    Write-Host "Creating service directory: $serviceDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $serviceDir -Force
}

# Stop and remove existing service if it exists
try {
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Host "Stopping existing service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        
        Write-Host "Removing existing service..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    }
}
catch {
    Write-Host "No existing service found" -ForegroundColor Gray
}

# Create the Windows service
Write-Host "Creating Windows service..." -ForegroundColor Yellow
$createResult = sc.exe create $ServiceName binpath= $BinaryPath displayname= $DisplayName start= auto

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Service created successfully" -ForegroundColor Green
    
    # Set service description
    sc.exe description $ServiceName $Description
    
    # Configure service recovery options
    Write-Host "Configuring service recovery options..." -ForegroundColor Yellow
    sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/5000/restart/5000
    
    # Start the service
    Write-Host "Starting service..." -ForegroundColor Yellow
    $startResult = Start-Service -Name $ServiceName -PassThru
    
    if ($startResult.Status -eq "Running") {
        Write-Host "✅ Service started successfully" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Failed to start service" -ForegroundColor Red
    }
    
    # Display service information
    Write-Host "`n📊 Service Information:" -ForegroundColor Cyan
    Get-Service -Name $ServiceName | Format-Table Name, Status, StartType -AutoSize
    
}
else {
    Write-Host "❌ Failed to create service" -ForegroundColor Red
    Write-Host "Error: $createResult"
}

Write-Host "`n🎯 Installation completed!" -ForegroundColor Green
Write-Host "Service can be managed through:"
Write-Host "  - Services.msc (Windows Services Manager)"
Write-Host "  - PowerShell: Get-Service '$ServiceName'"
Write-Host "  - Command Line: sc query $ServiceName"
