# Production Deployment Guide - Unison REST Adapter

## Overview

This guide provides comprehensive procedures for deploying the Unison REST Adapter to production environments, including validation steps and rollback procedures.

## Prerequisites

### Infrastructure Requirements

- **Windows Server 2019/2022** or **Linux** with .NET 9.0 Runtime
- **Minimum Resources**: 2 GB RAM, 1 CPU core, 10 GB disk space
- **Network Access**:
  - Inbound: Port 80/443 for REST API
  - Outbound: Access to Unison SOAP service (<http://192.168.10.206:9003>)
- **SSL Certificate**: For HTTPS production deployment

### Software Dependencies

- **.NET 9.0 Runtime**: ASP.NET Core runtime installed
- **IIS** (Windows) or **Nginx/Apache** (Linux): For reverse proxy
- **Windows Service** capabilities or **systemd** (Linux)

### Security Requirements

- **Service Account**: Dedicated service account with minimal privileges
- **Network Security**: Firewall rules configured
- **SSL/TLS**: Valid certificate for HTTPS
- **Authentication**: Unison-Token validation configured

## Pre-Deployment Checklist

### 1. Environment Validation ✅

```powershell
# Verify .NET Runtime
dotnet --list-runtimes

# Check available ports
netstat -an | findstr :80
netstat -an | findstr :443

# Verify service account
whoami /groups

# Test SOAP connectivity
Test-NetConnection -ComputerName 192.168.10.206 -Port 9003
```

### 2. Application Configuration ✅

- [ ] **appsettings.Production.json** configured with production values
- [ ] **Connection strings** updated for production SOAP endpoint
- [ ] **Logging configuration** set to appropriate level
- [ ] **Security settings** configured (HTTPS enforcement, token validation)
- [ ] **Health check endpoints** enabled
- [ ] **CORS settings** configured if required

### 3. Security Configuration ✅

- [ ] **SSL certificate** installed and configured
- [ ] **Service account** created with minimal required permissions
- [ ] **Firewall rules** configured
- [ ] **Authentication tokens** configured in secure storage
- [ ] **Sensitive data** removed from configuration files

### 4. Database/Dependencies ✅

- [ ] **SOAP service** connectivity verified
- [ ] **Network connectivity** tested
- [ ] **DNS resolution** working correctly
- [ ] **Load balancer** configuration (if applicable)

## Deployment Procedures

### Method 1: Windows Service Deployment

#### Step 1: Prepare Application Package

```powershell
# Build and publish application
cd "C:\Source\Unison Access Service REST API\UnisonRestAdapter"
dotnet publish -c Release -r win-x64 --self-contained false -o "C:\Deploy\UnisonRestAdapter"

# Verify published files
dir "C:\Deploy\UnisonRestAdapter"
```

#### Step 2: Install as Windows Service

```powershell
# Stop existing service (if upgrading)
Stop-Service -Name "UnisonRestAdapter" -ErrorAction SilentlyContinue

# Install new service
sc.exe create UnisonRestAdapter binPath="C:\Deploy\UnisonRestAdapter\UnisonRestAdapter.exe" start=auto
sc.exe description UnisonRestAdapter "Unison REST to SOAP Adapter Service"

# Set service account (optional)
sc.exe config UnisonRestAdapter obj=".\UnisonService" password="SecurePassword123"

# Start service
Start-Service -Name "UnisonRestAdapter"

# Verify service status
Get-Service -Name "UnisonRestAdapter"
```

#### Step 3: Configure IIS Reverse Proxy (Optional)

```xml
<!-- web.config for IIS reverse proxy -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="UnisonAPI" stopProcessing="true">
          <match url=".*" />
          <action type="Rewrite" url="http://localhost:5203/{R:0}" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### Method 2: Docker Container Deployment

#### Step 1: Build Docker Image

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish/ ./
EXPOSE 80
ENTRYPOINT ["dotnet", "UnisonRestAdapter.dll"]
```

```powershell
# Build container
docker build -t unison-rest-adapter:latest .

# Run container
docker run -d --name unison-api -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Unison__SoapEndpoint="http://192.168.10.206:9003/Unison.AccessService" \
  --restart unless-stopped \
  unison-rest-adapter:latest
```

### Method 3: Linux Systemd Service

#### Step 1: Deploy Application

```bash
# Create application directory
sudo mkdir -p /opt/unison-rest-adapter
sudo chown unison:unison /opt/unison-rest-adapter

# Copy published files
sudo cp -r publish/* /opt/unison-rest-adapter/
sudo chmod +x /opt/unison-rest-adapter/UnisonRestAdapter
```

#### Step 2: Create Systemd Service

```ini
# /etc/systemd/system/unison-rest-adapter.service
[Unit]
Description=Unison REST Adapter
After=network.target

[Service]
Type=notify
User=unison
Group=unison
WorkingDirectory=/opt/unison-rest-adapter
ExecStart=/opt/unison-rest-adapter/UnisonRestAdapter
Restart=always
RestartSec=10
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable unison-rest-adapter
sudo systemctl start unison-rest-adapter
sudo systemctl status unison-rest-adapter
```

## Post-Deployment Validation

### 1. Service Health Checks ✅

```powershell
# Basic health check
Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET

# Detailed health check
$token = "your-production-token"
$headers = @{ "Unison-Token" = $token }
Invoke-RestMethod -Uri "http://localhost:80/health/detailed" -Method GET -Headers $headers

# API functionality test
$body = @{
    cardId = "TEST001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body $body -ContentType "application/json"
```

### 2. Performance Validation ✅

```powershell
# Response time test
Measure-Command { Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET }

# Load test (basic)
1..10 | ForEach-Object -Parallel {
    Invoke-RestMethod -Uri "http://using:localhost:80/health" -Method GET
} -ThrottleLimit 5
```

### 3. Security Validation ✅

```powershell
# Test without authentication (should fail)
try {
    Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT
} catch {
    Write-Output "✅ Authentication required: $($_.Exception.Response.StatusCode)"
}

# Test with invalid token (should fail)
try {
    $badHeaders = @{ "Unison-Token" = "invalid-token" }
    Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $badHeaders
} catch {
    Write-Output "✅ Token validation working: $($_.Exception.Response.StatusCode)"
}

# Test HTTPS redirect (if configured)
Invoke-WebRequest -Uri "http://localhost:80/health" -MaximumRedirection 0
```

### 4. Integration Testing ✅

```powershell
# SOAP connectivity test
$detailedHealth = Invoke-RestMethod -Uri "http://localhost:80/health/detailed" -Headers $headers
Write-Output "SOAP Connectivity: $($detailedHealth.soapService.status)"

# End-to-end workflow test
$testCard = @{
    cardId = "DEPLOY-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
    userName = "test.user"
    firstName = "Test"
    lastName = "User"
    isActive = $true
}

$updateResult = Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body ($testCard | ConvertTo-Json) -ContentType "application/json"
Write-Output "End-to-End Test: $($updateResult.success)"
```

## Configuration Templates

### Production appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "UnisonRestAdapter": "Information"
    }
  },
  "AllowedHosts": "*",
  "Unison": {
    "SoapEndpoint": "http://192.168.10.206:9003/Unison.AccessService",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3
  },
  "Security": {
    "RequireHttps": true,
    "ValidTokens": ["${UNISON_API_TOKEN}"],
    "TokenRotationEnabled": true,
    "MaxTokenAge": "24:00:00"
  },
  "HealthChecks": {
    "Enabled": true,
    "DetailedEndpoint": "/health/detailed",
    "BasicEndpoint": "/health"
  }
}
```

### Environment Variables Template

```bash
# Production Environment Variables
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:80;https://0.0.0.0:443"
export UNISON_API_TOKEN="your-secure-production-token"
export Unison__SoapEndpoint="http://192.168.10.206:9003/Unison.AccessService"
export Security__RequireHttps=true
```

## Monitoring and Alerting

### 1. Health Monitoring Setup

```powershell
# Windows Task Scheduler for health monitoring
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Scripts\health-check.ps1"
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5)
Register-ScheduledTask -TaskName "UnisonAPIHealthCheck" -Action $action -Trigger $trigger
```

### 2. Log Monitoring

```powershell
# Windows Event Log monitoring
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='UnisonRestAdapter'} -MaxEvents 10

# Application log monitoring
Get-Content "C:\Logs\UnisonRestAdapter\app.log" -Tail 10 -Wait
```

### 3. Performance Monitoring

```powershell
# Performance counters
Get-Counter "\Process(UnisonRestAdapter)\Private Bytes"
Get-Counter "\Process(UnisonRestAdapter)\% Processor Time"
```

## Rollback Procedures

### Emergency Rollback (Windows Service)

```powershell
# Step 1: Stop current service
Stop-Service -Name "UnisonRestAdapter" -Force

# Step 2: Restore previous version
Remove-Item "C:\Deploy\UnisonRestAdapter" -Recurse -Force
Rename-Item "C:\Deploy\UnisonRestAdapter.backup" "C:\Deploy\UnisonRestAdapter"

# Step 3: Start service
Start-Service -Name "UnisonRestAdapter"

# Step 4: Verify rollback
Start-Sleep -Seconds 10
Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET
```

### Rollback with Docker

```bash
# Step 1: Stop current container
docker stop unison-api
docker rm unison-api

# Step 2: Start previous version
docker run -d --name unison-api -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --restart unless-stopped \
  unison-rest-adapter:previous

# Step 3: Verify rollback
curl http://localhost:80/health
```

### Configuration Rollback

```powershell
# Restore configuration files
Copy-Item "C:\Deploy\Config.backup\appsettings.Production.json" "C:\Deploy\UnisonRestAdapter\appsettings.Production.json"

# Restart service to apply configuration
Restart-Service -Name "UnisonRestAdapter"
```

## Troubleshooting Guide

### Common Issues

#### 1. Service Won't Start

```powershell
# Check Windows Event Log
Get-WinEvent -FilterHashtable @{LogName='System'; ID=7000,7009,7011,7023,7024,7026,7031,7032,7034} | Where-Object {$_.Message -like "*UnisonRestAdapter*"}

# Check application logs
Get-Content "C:\Deploy\UnisonRestAdapter\logs\app.log" -Tail 20

# Verify file permissions
icacls "C:\Deploy\UnisonRestAdapter" /verify
```

#### 2. SOAP Connectivity Issues

```powershell
# Test network connectivity
Test-NetConnection -ComputerName 192.168.10.206 -Port 9003

# Test SOAP endpoint
Invoke-WebRequest -Uri "http://192.168.10.206:9003/Unison.AccessService?wsdl" -Method GET
```

#### 3. Authentication Failures

```powershell
# Verify token configuration
$config = Get-Content "C:\Deploy\UnisonRestAdapter\appsettings.Production.json" | ConvertFrom-Json
Write-Output "Token Count: $($config.Security.ValidTokens.Count)"

# Check security logs
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='UnisonRestAdapter'} | Where-Object {$_.Message -like "*authentication*"}
```

### Recovery Procedures

#### Application Crash Recovery

1. **Check crash logs**
2. **Verify system resources**
3. **Restart service**
4. **Monitor for stability**
5. **Escalate if persistent**

#### Database Connectivity Recovery

1. **Test SOAP endpoint directly**
2. **Check network connectivity**
3. **Verify credentials**
4. **Restart adapter service**
5. **Contact infrastructure team**

## Maintenance Procedures

### Regular Maintenance Tasks

#### Daily

- [ ] Check service health status
- [ ] Review error logs
- [ ] Monitor response times

#### Weekly

- [ ] Review performance metrics
- [ ] Check disk space usage
- [ ] Verify backup procedures

#### Monthly

- [ ] Review security logs
- [ ] Update documentation
- [ ] Test rollback procedures
- [ ] Performance baseline review

### Update Procedures

#### Minor Updates (Bug Fixes)

1. **Create backup** of current deployment
2. **Deploy to staging** environment first
3. **Run validation tests**
4. **Deploy during maintenance window**
5. **Verify functionality**
6. **Update version documentation**

#### Major Updates (Feature Changes)

1. **Follow full deployment procedure**
2. **Extended testing period**
3. **Rollback plan rehearsal**
4. **Stakeholder notification**
5. **Documentation updates**

## Security Considerations

### Production Hardening

- **Remove development tools** and debugging symbols
- **Disable detailed error pages** in production
- **Configure secure headers** (HSTS, CSP, etc.)
- **Enable request logging** for security monitoring
- **Regular security updates** for underlying platform

### Access Control

- **Service account principle** of least privilege
- **Network segmentation** where possible
- **Regular token rotation** procedures
- **Audit logging** of administrative access

## Support and Escalation

### Contact Information

- **Primary Support**: Infrastructure Team
- **Secondary Support**: Development Team
- **Emergency Contact**: On-call rotation

### Escalation Matrix

1. **Level 1**: Service restart, basic troubleshooting
2. **Level 2**: Configuration changes, log analysis
3. **Level 3**: Code fixes, infrastructure changes
4. **Level 4**: Vendor support, major incident response

---

**Document Version**: 1.0  
**Last Updated**: September 9, 2025  
**Prepared By**: Development Team  
**Approved By**: Infrastructure Team
