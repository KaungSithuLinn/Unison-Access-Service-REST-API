# Unison REST Adapter Service Uninstallation Script
# Run as Administrator

param(
    [string]$ServicePath = "C:\Services\UnisonRestAdapter",
    [switch]$RemoveFiles = $false
)

$serviceName = "UnisonRestAdapter"

Write-Host "Uninstalling Unison REST Adapter Windows Service..." -ForegroundColor Yellow

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

# Stop and remove the service if it exists
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    try {
        Stop-Service -Name $serviceName -Force -ErrorAction Stop
        Write-Host "Service stopped successfully" -ForegroundColor Green
    }
    catch {
        Write-Warning "Failed to stop service: $($_.Exception.Message)"
    }

    Write-Host "Removing service..." -ForegroundColor Yellow
    sc.exe delete $serviceName
    Start-Sleep -Seconds 3

    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Warning "Service still exists. Reboot may be required."
    }
    else {
        Write-Host "Service removed successfully" -ForegroundColor Green
    }
}
else {
    Write-Host "Service not found - nothing to remove" -ForegroundColor Cyan
}

# Remove event log source
Write-Host "Removing event log source..." -ForegroundColor Yellow
try {
    Remove-EventLog -Source "UnisonRestAdapter" -ErrorAction SilentlyContinue
    Write-Host "Event log source removed" -ForegroundColor Green
}
catch {
    Write-Warning "Could not remove event log source: $($_.Exception.Message)"
}

# Optionally remove service files
if ($RemoveFiles -and (Test-Path $ServicePath)) {
    $confirm = Read-Host "Remove service files from $ServicePath? (y/n)"
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        try {
            Remove-Item -Path $ServicePath -Recurse -Force
            Write-Host "Service files removed" -ForegroundColor Green
        }
        catch {
            Write-Warning "Failed to remove service files: $($_.Exception.Message)"
        }
    }
}

Write-Host "Uninstallation completed!" -ForegroundColor Green
