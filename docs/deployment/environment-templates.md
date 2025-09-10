# Environment Configuration Templates

## Production Environment Variables

### Windows Environment Variables

```cmd
# Core Application Settings
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://0.0.0.0:80;https://0.0.0.0:443
set DOTNET_ENVIRONMENT=Production

# Unison Service Configuration
set Unison__SoapEndpoint=http://192.168.10.206:9003/Unison.AccessService
set Unison__TimeoutSeconds=30
set Unison__RetryAttempts=3

# Security Configuration
set UNISON_API_TOKEN=your-secure-production-token-here
set Security__RequireHttps=true
set Security__TokenRotationEnabled=true
set Security__MaxTokenAge=24:00:00

# Logging Configuration
set Logging__LogLevel__Default=Information
set Logging__LogLevel__Microsoft.AspNetCore=Warning
set Logging__LogLevel__UnisonRestAdapter=Information

# Health Check Configuration
set HealthChecks__Enabled=true
set HealthChecks__DetailedEndpoint=/health/detailed
set HealthChecks__BasicEndpoint=/health
```

### PowerShell Environment Variables

```powershell
# Core Application Settings
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:80;https://0.0.0.0:443"
$env:DOTNET_ENVIRONMENT = "Production"

# Unison Service Configuration
$env:Unison__SoapEndpoint = "http://192.168.10.206:9003/Unison.AccessService"
$env:Unison__TimeoutSeconds = "30"
$env:Unison__RetryAttempts = "3"

# Security Configuration
$env:UNISON_API_TOKEN = "your-secure-production-token-here"
$env:Security__RequireHttps = "true"
$env:Security__TokenRotationEnabled = "true"
$env:Security__MaxTokenAge = "24:00:00"

# Logging Configuration
$env:Logging__LogLevel__Default = "Information"
$env:Logging__LogLevel__Microsoft.AspNetCore = "Warning"
$env:Logging__LogLevel__UnisonRestAdapter = "Information"

# Health Check Configuration
$env:HealthChecks__Enabled = "true"
$env:HealthChecks__DetailedEndpoint = "/health/detailed"
$env:HealthChecks__BasicEndpoint = "/health"
```

### Linux Environment Variables

```bash
#!/bin/bash
# Core Application Settings
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:80;https://0.0.0.0:443"
export DOTNET_ENVIRONMENT=Production

# Unison Service Configuration
export Unison__SoapEndpoint="http://192.168.10.206:9003/Unison.AccessService"
export Unison__TimeoutSeconds=30
export Unison__RetryAttempts=3

# Security Configuration
export UNISON_API_TOKEN="your-secure-production-token-here"
export Security__RequireHttps=true
export Security__TokenRotationEnabled=true
export Security__MaxTokenAge="24:00:00"

# Logging Configuration
export Logging__LogLevel__Default=Information
export Logging__LogLevel__Microsoft.AspNetCore=Warning
export Logging__LogLevel__UnisonRestAdapter=Information

# Health Check Configuration
export HealthChecks__Enabled=true
export HealthChecks__DetailedEndpoint="/health/detailed"
export HealthChecks__BasicEndpoint="/health"
```

## Production appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "UnisonRestAdapter": "Information"
    },
    "Console": {
      "IncludeScopes": false,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    },
    "File": {
      "Path": "logs/unison-rest-adapter-.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30,
      "FileSizeLimitBytes": 10485760,
      "RollOnFileSizeLimit": true
    }
  },
  "AllowedHosts": "*",
  "Unison": {
    "SoapEndpoint": "http://192.168.10.206:9003/Unison.AccessService",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 2,
    "MaxConcurrentRequests": 10,
    "EnableConnectionPooling": true
  },
  "Security": {
    "RequireHttps": true,
    "ValidTokens": ["${UNISON_API_TOKEN}"],
    "TokenRotationEnabled": true,
    "MaxTokenAge": "24:00:00",
    "AllowedOrigins": ["https://your-domain.com", "https://www.your-domain.com"]
  },
  "HealthChecks": {
    "Enabled": true,
    "DetailedEndpoint": "/health/detailed",
    "BasicEndpoint": "/health",
    "TimeoutSeconds": 10,
    "CheckIntervalSeconds": 30
  },
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 1048576,
      "KeepAliveTimeout": "00:02:00",
      "RequestHeadersTimeout": "00:00:30"
    },
    "EndPoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      },
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Subject": "CN=your-domain.com",
          "Store": "My",
          "Location": "LocalMachine",
          "AllowInvalid": false
        }
      }
    }
  },
  "Cors": {
    "AllowCredentials": false,
    "AllowedOrigins": [
      "https://your-domain.com",
      "https://www.your-domain.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": [
      "Content-Type",
      "Authorization",
      "Unison-Token",
      "X-Requested-With"
    ]
  }
}
```

## Staging Environment Configuration

### appsettings.Staging.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "UnisonRestAdapter": "Debug"
    }
  },
  "AllowedHosts": "*",
  "Unison": {
    "SoapEndpoint": "http://192.168.10.206:9003/Unison.AccessService",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3
  },
  "Security": {
    "RequireHttps": false,
    "ValidTokens": ["staging-test-token-2025", "integration-test-token"],
    "TokenRotationEnabled": false,
    "MaxTokenAge": "168:00:00"
  },
  "HealthChecks": {
    "Enabled": true,
    "DetailedEndpoint": "/health/detailed",
    "BasicEndpoint": "/health"
  }
}
```

## Development Environment Configuration

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "UnisonRestAdapter": "Trace"
    }
  },
  "AllowedHosts": "*",
  "Unison": {
    "SoapEndpoint": "http://192.168.10.206:9003/Unison.AccessService",
    "TimeoutSeconds": 60,
    "RetryAttempts": 1
  },
  "Security": {
    "RequireHttps": false,
    "ValidTokens": ["dev-token-123", "test-token-456"],
    "TokenRotationEnabled": false,
    "MaxTokenAge": "168:00:00"
  },
  "HealthChecks": {
    "Enabled": true,
    "DetailedEndpoint": "/health/detailed",
    "BasicEndpoint": "/health"
  },
  "Swagger": {
    "Enabled": true,
    "Title": "Unison REST Adapter API - Development",
    "Version": "v1",
    "Description": "Development environment for testing API endpoints"
  }
}
```

## Docker Environment Configuration

### docker-compose.yml for Production

```yaml
version: "3.8"

services:
  unison-rest-adapter:
    image: unison-rest-adapter:latest
    container_name: unison-api-prod
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - Unison__SoapEndpoint=http://192.168.10.206:9003/Unison.AccessService
      - Security__RequireHttps=true
      - UNISON_API_TOKEN=${UNISON_PRODUCTION_TOKEN}
    volumes:
      - ./logs:/app/logs
      - ./certs:/app/certs:ro
    networks:
      - unison-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    deploy:
      resources:
        limits:
          cpus: "2.0"
          memory: 2G
        reservations:
          cpus: "0.5"
          memory: 512M

networks:
  unison-network:
    driver: bridge
```

### .env file for Docker

```bash
# Production Environment Variables for Docker
UNISON_PRODUCTION_TOKEN=your-production-token-here
COMPOSE_PROJECT_NAME=unison-rest-adapter
```

## Kubernetes Configuration

### deployment.yaml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: unison-rest-adapter
  labels:
    app: unison-rest-adapter
spec:
  replicas: 3
  selector:
    matchLabels:
      app: unison-rest-adapter
  template:
    metadata:
      labels:
        app: unison-rest-adapter
    spec:
      containers:
        - name: unison-rest-adapter
          image: unison-rest-adapter:latest
          ports:
            - containerPort: 80
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ASPNETCORE_URLS
              value: "http://+:80"
            - name: Unison__SoapEndpoint
              value: "http://192.168.10.206:9003/Unison.AccessService"
            - name: UNISON_API_TOKEN
              valueFrom:
                secretKeyRef:
                  name: unison-secrets
                  key: api-token
          resources:
            limits:
              cpu: "1"
              memory: "1Gi"
            requests:
              cpu: "200m"
              memory: "512Mi"
          livenessProbe:
            httpGet:
              path: /health
              port: 80
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /health
              port: 80
            initialDelaySeconds: 5
            periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: unison-rest-adapter-service
spec:
  selector:
    app: unison-rest-adapter
  ports:
    - protocol: TCP
      port: 80
      targetPort: 80
  type: LoadBalancer
---
apiVersion: v1
kind: Secret
metadata:
  name: unison-secrets
type: Opaque
data:
  api-token: <base64-encoded-token>
```

## Windows Service Configuration

### Service Installation Script

```powershell
# install-service.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ServicePath,

    [Parameter(Mandatory=$true)]
    [string]$ServiceAccount,

    [Parameter(Mandatory=$true)]
    [string]$ServicePassword
)

# Stop existing service
Stop-Service -Name "UnisonRestAdapter" -Force -ErrorAction SilentlyContinue

# Remove existing service
sc.exe delete "UnisonRestAdapter"

# Create new service
sc.exe create UnisonRestAdapter binPath="$ServicePath\UnisonRestAdapter.exe" start=auto
sc.exe description UnisonRestAdapter "Unison REST to SOAP Adapter Service"

# Set service account
sc.exe config UnisonRestAdapter obj="$ServiceAccount" password="$ServicePassword"

# Set recovery options
sc.exe failure UnisonRestAdapter reset=86400 actions=restart/5000/restart/10000/restart/30000

# Set environment variables for service
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UnisonRestAdapter" /v Environment /t REG_MULTI_SZ /d "ASPNETCORE_ENVIRONMENT=Production`0UNISON_API_TOKEN=$env:UNISON_API_TOKEN"

# Start service
Start-Service -Name "UnisonRestAdapter"

# Verify service status
Get-Service -Name "UnisonRestAdapter"
```

## IIS Configuration

### web.config for IIS Reverse Proxy

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="UnisonAPI" stopProcessing="true">
          <match url=".*" />
          <conditions>
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="http://localhost:5203/{R:0}" />
          <serverVariables>
            <set name="HTTP_X_ORIGINAL_HOST" value="{HTTP_HOST}" />
            <set name="HTTP_X_FORWARDED_PROTO" value="{HTTPS}" />
          </serverVariables>
        </rule>
      </rules>
    </rewrite>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="1048576" />
      </requestFiltering>
    </security>
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

## Environment-Specific Validation Scripts

### Production Validation Script

```powershell
# validate-production-config.ps1
Write-Host "Validating Production Configuration..." -ForegroundColor Green

# Check environment variables
$requiredVars = @(
    "ASPNETCORE_ENVIRONMENT",
    "UNISON_API_TOKEN",
    "Unison__SoapEndpoint"
)

foreach ($var in $requiredVars) {
    $value = [Environment]::GetEnvironmentVariable($var)
    if (-not $value) {
        Write-Error "Missing required environment variable: $var"
        exit 1
    }
    Write-Host "✅ $var is set" -ForegroundColor Green
}

# Validate HTTPS requirement
if ($env:Security__RequireHttps -ne "true") {
    Write-Warning "HTTPS is not enforced in production"
}

# Test SOAP connectivity
try {
    $response = Invoke-WebRequest -Uri "$env:Unison__SoapEndpoint?wsdl" -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ SOAP endpoint accessible" -ForegroundColor Green
    }
} catch {
    Write-Error "Cannot reach SOAP endpoint: $env:Unison__SoapEndpoint"
    exit 1
}

Write-Host "Production configuration validation completed successfully!" -ForegroundColor Green
```

### Configuration Security Check

```powershell
# security-check.ps1
Write-Host "Performing Security Configuration Check..." -ForegroundColor Yellow

# Check for hardcoded secrets in config files
$configFiles = Get-ChildItem -Path "." -Filter "appsettings*.json" -Recurse

foreach ($file in $configFiles) {
    $content = Get-Content $file.FullName -Raw

    # Check for common secrets patterns
    $patterns = @(
        "password.*[\"":]\s*[\""].*[\""]",
        "secret.*[\"":]\s*[\""].*[\""]",
        "token.*[\"":]\s*[\""](?!.*\$\{).*[\""]"
    )

    foreach ($pattern in $patterns) {
        if ($content -match $pattern) {
            Write-Warning "Potential hardcoded secret found in $($file.Name)"
        }
    }
}

# Check file permissions
$appFiles = Get-ChildItem -Path "." -Filter "*.dll" -Recurse
foreach ($file in $appFiles) {
    $acl = Get-Acl $file.FullName
    $everyone = $acl.Access | Where-Object { $_.IdentityReference -match "Everyone" }
    if ($everyone -and $everyone.FileSystemRights -match "Write") {
        Write-Warning "File $($file.Name) is writable by Everyone"
    }
}

Write-Host "Security check completed!" -ForegroundColor Green
```

---

**Template Version**: 1.0  
**Last Updated**: September 9, 2025  
**Created By**: Development Team

**Usage Instructions**:

1. Copy appropriate template for your deployment environment
2. Replace placeholder values with actual production values
3. Secure sensitive values using environment variables or secret management
4. Validate configuration using provided validation scripts
5. Test configuration in staging environment before production deployment
