# Rollback Procedures - Unison REST Adapter

## Overview

This document provides comprehensive procedures for rolling back the Unison REST Adapter to a previous stable version in case of deployment issues or critical problems.

## Emergency Rollback Decision Matrix

| Issue Severity                         | Rollback Required | Decision Timeline |
| -------------------------------------- | ----------------- | ----------------- |
| **Critical** - Service completely down | ✅ Immediate      | < 5 minutes       |
| **Major** - Core functionality broken  | ✅ Immediate      | < 15 minutes      |
| **Minor** - Performance degradation    | ⚠️ Consider       | < 30 minutes      |
| **Cosmetic** - UI/documentation issues | ❌ Fix forward    | N/A               |

## Pre-Rollback Preparation

### Emergency Contact List

- **Incident Commander**: [Contact Info]
- **Development Lead**: [Contact Info]
- **Operations Manager**: [Contact Info]
- **Infrastructure Team**: [Contact Info]

### Quick Assessment Checklist

- [ ] **Issue Severity**: Determined and documented
- [ ] **Impact Scope**: Affected users/systems identified
- [ ] **Rollback Decision**: Approved by incident commander
- [ ] **Communication**: Key stakeholders notified
- [ ] **Backup Verification**: Previous version backup confirmed available

## Windows Service Rollback

### Emergency Rollback (< 5 minutes)

```powershell
# Emergency rollback script - keep this ready for production use
# emergency-rollback.ps1

Write-Host "EMERGENCY ROLLBACK INITIATED" -ForegroundColor Red
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Yellow

# Step 1: Stop current service immediately
Write-Host "Stopping current service..." -ForegroundColor Yellow
Stop-Service -Name "UnisonRestAdapter" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 5

# Step 2: Backup current problematic version (for post-incident analysis)
Write-Host "Backing up current version for analysis..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
if (Test-Path "C:\Deploy\UnisonRestAdapter") {
    Rename-Item "C:\Deploy\UnisonRestAdapter" "C:\Deploy\UnisonRestAdapter.failed.$timestamp" -ErrorAction SilentlyContinue
}

# Step 3: Restore previous stable version
Write-Host "Restoring previous version..." -ForegroundColor Yellow
if (Test-Path "C:\Deploy\UnisonRestAdapter.backup") {
    Copy-Item "C:\Deploy\UnisonRestAdapter.backup" "C:\Deploy\UnisonRestAdapter" -Recurse -Force
} else {
    Write-Error "CRITICAL: Backup not found at C:\Deploy\UnisonRestAdapter.backup"
    exit 1
}

# Step 4: Restore previous configuration
Write-Host "Restoring configuration..." -ForegroundColor Yellow
if (Test-Path "C:\Deploy\Config.backup") {
    Copy-Item "C:\Deploy\Config.backup\*" "C:\Deploy\UnisonRestAdapter\" -Force
}

# Step 5: Start previous version
Write-Host "Starting previous version..." -ForegroundColor Yellow
Start-Service -Name "UnisonRestAdapter"
Start-Sleep -Seconds 10

# Step 6: Verify rollback success
Write-Host "Verifying rollback..." -ForegroundColor Yellow
$maxAttempts = 6
for ($i = 1; $i -le $maxAttempts; $i++) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET -TimeoutSec 5
        Write-Host "✅ ROLLBACK SUCCESSFUL - Service responding" -ForegroundColor Green
        Write-Host "Health Status: $($response.status)" -ForegroundColor Green
        break
    } catch {
        if ($i -eq $maxAttempts) {
            Write-Error "❌ ROLLBACK FAILED - Service not responding after $maxAttempts attempts"
            exit 1
        }
        Write-Host "Attempt $i failed, retrying..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    }
}

Write-Host "EMERGENCY ROLLBACK COMPLETED SUCCESSFULLY" -ForegroundColor Green
Write-Host "Next steps: Review logs and investigate issue" -ForegroundColor Yellow
```

### Detailed Windows Service Rollback

#### Step 1: Stop Current Service

```powershell
# Graceful shutdown attempt
Write-Host "Attempting graceful shutdown..." -ForegroundColor Yellow
Stop-Service -Name "UnisonRestAdapter" -ErrorAction SilentlyContinue

# Wait for graceful shutdown
Start-Sleep -Seconds 10

# Force stop if still running
$service = Get-Service -Name "UnisonRestAdapter" -ErrorAction SilentlyContinue
if ($service -and $service.Status -eq "Running") {
    Write-Host "Forcing service stop..." -ForegroundColor Yellow
    Stop-Process -Name "UnisonRestAdapter" -Force -ErrorAction SilentlyContinue
}

# Verify service stopped
$service = Get-Service -Name "UnisonRestAdapter" -ErrorAction SilentlyContinue
if ($service -and $service.Status -eq "Stopped") {
    Write-Host "✅ Service stopped successfully" -ForegroundColor Green
} else {
    Write-Error "❌ Failed to stop service"
    exit 1
}
```

#### Step 2: Backup Current Version

```powershell
# Create timestamped backup of failed version
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$currentPath = "C:\Deploy\UnisonRestAdapter"
$backupPath = "C:\Deploy\UnisonRestAdapter.failed.$timestamp"

if (Test-Path $currentPath) {
    Write-Host "Creating backup of failed version..." -ForegroundColor Yellow
    Move-Item $currentPath $backupPath -Force
    Write-Host "✅ Failed version backed up to: $backupPath" -ForegroundColor Green
}
```

#### Step 3: Restore Previous Version

```powershell
# Restore from backup
$backupPath = "C:\Deploy\UnisonRestAdapter.backup"
$deployPath = "C:\Deploy\UnisonRestAdapter"

if (-not (Test-Path $backupPath)) {
    Write-Error "❌ Backup not found at: $backupPath"
    Write-Error "Cannot proceed with rollback"
    exit 1
}

Write-Host "Restoring previous version from backup..." -ForegroundColor Yellow
Copy-Item $backupPath $deployPath -Recurse -Force

# Verify restore
if (Test-Path "$deployPath\UnisonRestAdapter.exe") {
    Write-Host "✅ Previous version restored successfully" -ForegroundColor Green
} else {
    Write-Error "❌ Failed to restore previous version"
    exit 1
}
```

#### Step 4: Restore Configuration

```powershell
# Restore configuration files
$configBackupPath = "C:\Deploy\Config.backup"
$configPath = "C:\Deploy\UnisonRestAdapter"

if (Test-Path $configBackupPath) {
    Write-Host "Restoring configuration files..." -ForegroundColor Yellow
    Copy-Item "$configBackupPath\appsettings.Production.json" "$configPath\" -Force -ErrorAction SilentlyContinue
    Copy-Item "$configBackupPath\web.config" "$configPath\" -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Configuration files restored" -ForegroundColor Green
} else {
    Write-Warning "⚠️ Configuration backup not found, using current configuration"
}
```

#### Step 5: Start Previous Version

```powershell
# Start the service
Write-Host "Starting previous version..." -ForegroundColor Yellow
Start-Service -Name "UnisonRestAdapter"

# Wait for startup
Start-Sleep -Seconds 15

# Verify service started
$service = Get-Service -Name "UnisonRestAdapter"
if ($service.Status -eq "Running") {
    Write-Host "✅ Service started successfully" -ForegroundColor Green
} else {
    Write-Error "❌ Service failed to start"

    # Check event logs for startup errors
    Get-WinEvent -FilterHashtable @{LogName='Application'; ID=1000,1001} -MaxEvents 5 | Where-Object {$_.Message -like "*UnisonRestAdapter*"}
    exit 1
}
```

## Docker Container Rollback

### Emergency Docker Rollback

```bash
#!/bin/bash
# emergency-docker-rollback.sh

echo "🚨 EMERGENCY DOCKER ROLLBACK INITIATED"
echo "Timestamp: $(date)"

# Step 1: Stop and remove current container
echo "Stopping current container..."
docker stop unison-api --time 10 2>/dev/null
docker rm unison-api 2>/dev/null

# Step 2: Start previous version
echo "Starting previous version..."
docker run -d --name unison-api -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e UNISON_API_TOKEN="$UNISON_PRODUCTION_TOKEN" \
  --restart unless-stopped \
  unison-rest-adapter:previous

# Step 3: Verify rollback
echo "Verifying rollback..."
sleep 15

for i in {1..6}; do
  if curl -f http://localhost:80/health >/dev/null 2>&1; then
    echo "✅ ROLLBACK SUCCESSFUL - Container responding"
    break
  elif [ $i -eq 6 ]; then
    echo "❌ ROLLBACK FAILED - Container not responding"
    docker logs unison-api --tail 20
    exit 1
  else
    echo "Attempt $i failed, retrying..."
    sleep 10
  fi
done

echo "✅ EMERGENCY DOCKER ROLLBACK COMPLETED"
```

### Detailed Docker Rollback

#### Step 1: Stop Current Container

```bash
# Graceful stop with timeout
docker stop unison-api --time 30

# Verify container stopped
if [ "$(docker ps -q -f name=unison-api)" ]; then
    echo "❌ Container still running, forcing stop"
    docker kill unison-api
fi

# Remove stopped container
docker rm unison-api
```

#### Step 2: Backup Current Image

```bash
# Tag current image for investigation
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
docker tag unison-rest-adapter:latest unison-rest-adapter:failed-$TIMESTAMP
echo "✅ Current image tagged as: unison-rest-adapter:failed-$TIMESTAMP"
```

#### Step 3: Start Previous Version

```bash
# Start previous stable version
docker run -d --name unison-api -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:80 \
  -e Unison__SoapEndpoint=http://192.168.10.206:9003/Unison.AccessService \
  -e UNISON_API_TOKEN="$UNISON_PRODUCTION_TOKEN" \
  -v $(pwd)/logs:/app/logs \
  --restart unless-stopped \
  unison-rest-adapter:previous

# Wait for container to start
sleep 20
```

#### Step 4: Verify Rollback

```bash
# Check container status
if [ "$(docker ps -q -f name=unison-api -f status=running)" ]; then
    echo "✅ Container running"
else
    echo "❌ Container not running"
    docker logs unison-api --tail 50
    exit 1
fi

# Test health endpoint
if curl -f http://localhost:80/health >/dev/null 2>&1; then
    echo "✅ Health endpoint responding"
else
    echo "❌ Health endpoint not responding"
    docker logs unison-api --tail 20
    exit 1
fi
```

## Linux Systemd Service Rollback

### Emergency Systemd Rollback

```bash
#!/bin/bash
# emergency-systemd-rollback.sh

echo "🚨 EMERGENCY SYSTEMD ROLLBACK INITIATED"
echo "Timestamp: $(date)"

# Step 1: Stop current service
echo "Stopping current service..."
sudo systemctl stop unison-rest-adapter
sudo systemctl disable unison-rest-adapter

# Step 2: Backup current version
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
if [ -d "/opt/unison-rest-adapter" ]; then
    echo "Backing up failed version..."
    sudo mv /opt/unison-rest-adapter /opt/unison-rest-adapter.failed.$TIMESTAMP
fi

# Step 3: Restore previous version
if [ -d "/opt/unison-rest-adapter.backup" ]; then
    echo "Restoring previous version..."
    sudo cp -r /opt/unison-rest-adapter.backup /opt/unison-rest-adapter
    sudo chown -R unison:unison /opt/unison-rest-adapter
    sudo chmod +x /opt/unison-rest-adapter/UnisonRestAdapter
else
    echo "❌ CRITICAL: Backup not found"
    exit 1
fi

# Step 4: Start previous version
echo "Starting previous version..."
sudo systemctl enable unison-rest-adapter
sudo systemctl start unison-rest-adapter

# Step 5: Verify rollback
echo "Verifying rollback..."
sleep 15

for i in {1..6}; do
    if curl -f http://localhost:80/health >/dev/null 2>&1; then
        echo "✅ ROLLBACK SUCCESSFUL"
        break
    elif [ $i -eq 6 ]; then
        echo "❌ ROLLBACK FAILED"
        sudo journalctl -u unison-rest-adapter --lines=20
        exit 1
    else
        echo "Attempt $i failed, retrying..."
        sleep 10
    fi
done

echo "✅ EMERGENCY SYSTEMD ROLLBACK COMPLETED"
```

## Configuration-Only Rollback

### Rollback Configuration Without Code Changes

```powershell
# config-only-rollback.ps1

Write-Host "Performing configuration-only rollback..." -ForegroundColor Yellow

# Step 1: Stop service
Stop-Service -Name "UnisonRestAdapter"

# Step 2: Backup current configuration
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
Copy-Item "C:\Deploy\UnisonRestAdapter\appsettings.Production.json" "C:\Deploy\UnisonRestAdapter\appsettings.Production.json.failed.$timestamp"

# Step 3: Restore previous configuration
if (Test-Path "C:\Deploy\Config.backup\appsettings.Production.json") {
    Copy-Item "C:\Deploy\Config.backup\appsettings.Production.json" "C:\Deploy\UnisonRestAdapter\"
    Write-Host "✅ Configuration restored" -ForegroundColor Green
} else {
    Write-Error "❌ Configuration backup not found"
    exit 1
}

# Step 4: Restart service
Start-Service -Name "UnisonRestAdapter"

# Step 5: Verify
Start-Sleep -Seconds 15
try {
    $response = Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET
    Write-Host "✅ Configuration rollback successful" -ForegroundColor Green
} catch {
    Write-Error "❌ Configuration rollback failed"
    exit 1
}
```

## Database/State Rollback

### SOAP Service State Considerations

```powershell
# Since this is a stateless REST adapter, no database rollback is needed
# However, we may need to consider SOAP service state

Write-Host "Checking SOAP service compatibility..." -ForegroundColor Yellow

# Test SOAP connectivity with previous version
try {
    $headers = @{ "Unison-Token" = $env:UNISON_API_TOKEN }
    $testData = @{
        cardId = "ROLLBACK-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body $testData -ContentType "application/json"

    if ($response.success) {
        Write-Host "✅ SOAP integration working after rollback" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ SOAP integration issue detected"
    }
} catch {
    Write-Error "❌ SOAP integration failed: $($_.Exception.Message)"
}
```

## Rollback Validation Procedures

### Comprehensive Validation Script

```powershell
# validate-rollback.ps1

Write-Host "=== ROLLBACK VALIDATION STARTED ===" -ForegroundColor Green
$validationResults = @()

# Test 1: Service Health
Write-Host "Testing service health..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET -TimeoutSec 10
    if ($healthResponse.status -eq "Healthy") {
        $validationResults += "✅ Health Check: PASS"
    } else {
        $validationResults += "❌ Health Check: FAIL - Status: $($healthResponse.status)"
    }
} catch {
    $validationResults += "❌ Health Check: FAIL - Not responding"
}

# Test 2: Authentication
Write-Host "Testing authentication..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT
    $validationResults += "❌ Authentication: FAIL - No auth required"
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        $validationResults += "✅ Authentication: PASS - 401 returned"
    } else {
        $validationResults += "⚠️ Authentication: UNKNOWN - Status: $($_.Exception.Response.StatusCode)"
    }
}

# Test 3: API Functionality
Write-Host "Testing API functionality..." -ForegroundColor Yellow
try {
    $headers = @{ "Unison-Token" = $env:UNISON_API_TOKEN }
    $testData = @{
        cardId = "ROLLBACK-VALIDATION-$(Get-Date -Format 'yyyyMMddHHmmss')"
        userName = "test.rollback"
        firstName = "Rollback"
        lastName = "Test"
        isActive = $true
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body $testData -ContentType "application/json"

    if ($response.success) {
        $validationResults += "✅ API Functionality: PASS"
    } else {
        $validationResults += "❌ API Functionality: FAIL - $($response.error)"
    }
} catch {
    $validationResults += "❌ API Functionality: FAIL - $($_.Exception.Message)"
}

# Test 4: Performance
Write-Host "Testing performance..." -ForegroundColor Yellow
$responseTime = Measure-Command { Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET }
if ($responseTime.TotalSeconds -lt 2) {
    $validationResults += "✅ Performance: PASS - Response time: $($responseTime.TotalSeconds)s"
} else {
    $validationResults += "⚠️ Performance: SLOW - Response time: $($responseTime.TotalSeconds)s"
}

# Display Results
Write-Host "`n=== VALIDATION RESULTS ===" -ForegroundColor Green
foreach ($result in $validationResults) {
    Write-Host $result
}

# Determine overall status
$failures = $validationResults | Where-Object { $_ -like "*❌*" }
if ($failures.Count -eq 0) {
    Write-Host "`n✅ ROLLBACK VALIDATION: SUCCESSFUL" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ ROLLBACK VALIDATION: FAILED ($($failures.Count) failures)" -ForegroundColor Red
    exit 1
}
```

## Post-Rollback Procedures

### Incident Documentation

```powershell
# generate-rollback-report.ps1

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$reportPath = "C:\Deploy\Logs\rollback-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"

$report = @"
# Rollback Report

**Timestamp**: $timestamp
**Incident**: [Describe the issue that triggered rollback]
**Severity**: [Critical/Major/Minor]
**Duration**: [Time from issue detection to rollback completion]

## Rollback Details

- **Previous Version**: [Version rolled back from]
- **Restored Version**: [Version rolled back to]
- **Rollback Method**: [Windows Service/Docker/Systemd]
- **Executed By**: $env:USERNAME

## Validation Results

- [ ] Service Health: [PASS/FAIL]
- [ ] Authentication: [PASS/FAIL]
- [ ] API Functionality: [PASS/FAIL]
- [ ] Performance: [PASS/FAIL]

## Next Steps

- [ ] Root cause analysis
- [ ] Fix identification
- [ ] Testing plan for re-deployment
- [ ] Stakeholder communication

## Files Affected

- Application: C:\Deploy\UnisonRestAdapter
- Configuration: [List configuration files]
- Logs: [List relevant log files]

## Lessons Learned

[Document what went wrong and how to prevent it in the future]
"@

$report | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "Rollback report generated: $reportPath" -ForegroundColor Green
```

### Monitoring and Alerting Reset

```powershell
# reset-monitoring.ps1

Write-Host "Resetting monitoring and alerting..." -ForegroundColor Yellow

# Clear any existing alerts
# This would depend on your monitoring system (e.g., SCOM, Nagios, etc.)

# Verify monitoring is working with rolled-back version
$healthCheck = Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET
Write-Host "Current health status: $($healthCheck.status)" -ForegroundColor Green

# Update monitoring configuration if needed
# This would be specific to your monitoring setup

Write-Host "✅ Monitoring reset complete" -ForegroundColor Green
```

## Prevention and Best Practices

### Pre-Deployment Backup Strategy

```powershell
# pre-deployment-backup.ps1
# Run this BEFORE every deployment

Write-Host "Creating pre-deployment backup..." -ForegroundColor Green

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupBase = "C:\Deploy\Backups\$timestamp"

# Create backup directory
New-Item -ItemType Directory -Path $backupBase -Force

# Backup application files
Copy-Item "C:\Deploy\UnisonRestAdapter" "$backupBase\Application" -Recurse -Force

# Backup configuration
New-Item -ItemType Directory -Path "$backupBase\Configuration" -Force
Copy-Item "C:\Deploy\UnisonRestAdapter\appsettings.Production.json" "$backupBase\Configuration\" -Force

# Create restore script
$restoreScript = @"
# Auto-generated restore script
# Run this to restore version from $timestamp

Stop-Service -Name "UnisonRestAdapter" -Force
Remove-Item "C:\Deploy\UnisonRestAdapter" -Recurse -Force
Copy-Item "$backupBase\Application" "C:\Deploy\UnisonRestAdapter" -Recurse -Force
Start-Service -Name "UnisonRestAdapter"

Write-Host "Restored version from $timestamp"
"@

$restoreScript | Out-File -FilePath "$backupBase\restore.ps1" -Encoding UTF8

# Update "current backup" symlink
if (Test-Path "C:\Deploy\UnisonRestAdapter.backup") {
    Remove-Item "C:\Deploy\UnisonRestAdapter.backup" -Recurse -Force
}
Copy-Item "$backupBase\Application" "C:\Deploy\UnisonRestAdapter.backup" -Recurse -Force

Write-Host "✅ Pre-deployment backup created: $backupBase" -ForegroundColor Green
```

### Automated Rollback Triggers

```powershell
# Setup automated rollback monitoring
# This would monitor health and automatically rollback if issues detected

# health-monitor.ps1
while ($true) {
    try {
        $health = Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET -TimeoutSec 10

        if ($health.status -ne "Healthy") {
            Write-Warning "Health check failed: $($health.status)"
            # Could trigger automated rollback here based on criteria
        }
    } catch {
        Write-Warning "Health endpoint not responding"
        # Count failures and trigger rollback if threshold reached
    }

    Start-Sleep -Seconds 30
}
```

---

**Document Version**: 1.0  
**Last Updated**: September 9, 2025  
**Emergency Contact**: [Your On-Call Number]

**CRITICAL REMINDERS**:

1. **Keep emergency rollback scripts tested and ready**
2. **Always backup before deploying**
3. **Test rollback procedures in staging first**
4. **Document every rollback for continuous improvement**
5. **Communicate with stakeholders throughout the process**
