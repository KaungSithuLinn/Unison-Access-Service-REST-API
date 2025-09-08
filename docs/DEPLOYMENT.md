# Deployment Guide

This guide provides comprehensive instructions for deploying the Unison REST Adapter API to various environments.

## Prerequisites

- .NET 9.0 Runtime
- Valid Unison SOAP backend access
- Authentication tokens for target environment

## Environment Configuration

### Development Environment

1. Configure local settings in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "UnisonService": {
    "SoapEndpoint": "http://dev-unison-service/endpoint",
    "Timeout": "00:00:30"
  }
}
```

2. Set environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5000;https://localhost:5001"
```

### Production Environment

1. Configure production settings in `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "UnisonService": {
    "SoapEndpoint": "https://prod-unison-service/endpoint",
    "Timeout": "00:01:00"
  },
  "AllowedHosts": "*"
}
```

2. Set production environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="https://+:443;http://+:80"
```

## Deployment Methods

### 1. Self-Contained Deployment

Build and publish the application:

```bash
# Build and publish for specific runtime
dotnet publish UnisonRestAdapter/UnisonRestAdapter.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish

# Run the published application
cd publish
./UnisonRestAdapter.exe
```

### 2. Framework-Dependent Deployment

```bash
# Publish framework-dependent
dotnet publish UnisonRestAdapter/UnisonRestAdapter.csproj \
  --configuration Release \
  --output ./publish

# Run with dotnet runtime
cd publish
dotnet UnisonRestAdapter.dll
```

### 3. Docker Deployment

Create a `Dockerfile`:

```dockerfile
# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UnisonRestAdapter/UnisonRestAdapter.csproj", "UnisonRestAdapter/"]
RUN dotnet restore "UnisonRestAdapter/UnisonRestAdapter.csproj"
COPY . .
WORKDIR "/src/UnisonRestAdapter"
RUN dotnet build "UnisonRestAdapter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UnisonRestAdapter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UnisonRestAdapter.dll"]
```

Build and run the Docker container:

```bash
# Build the image
docker build -t unison-rest-adapter .

# Run the container
docker run -p 8080:8080 -p 8081:8081 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e UnisonService__SoapEndpoint=https://your-soap-service/endpoint \
  unison-rest-adapter
```

### 4. IIS Deployment

1. **Install .NET 9.0 Hosting Bundle** on the IIS server

2. **Publish the application**:

```bash
dotnet publish UnisonRestAdapter/UnisonRestAdapter.csproj \
  --configuration Release \
  --output ./iis-publish
```

3. **Configure IIS**:

   - Create a new website in IIS Manager
   - Set physical path to the published folder
   - Configure application pool for "No Managed Code"
   - Set identity to appropriate service account

4. **Configure web.config** (auto-generated during publish):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\UnisonRestAdapter.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### 5. Linux Service Deployment

1. **Create service file** `/etc/systemd/system/unison-rest-adapter.service`:

```ini
[Unit]
Description=Unison REST Adapter API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/unison-rest-adapter
ExecStart=/usr/bin/dotnet /var/www/unison-rest-adapter/UnisonRestAdapter.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=unison-rest-adapter
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

2. **Enable and start the service**:

```bash
sudo systemctl enable unison-rest-adapter.service
sudo systemctl start unison-rest-adapter.service
sudo systemctl status unison-rest-adapter.service
```

## Load Balancer Configuration

### Nginx Reverse Proxy

Configure Nginx as a reverse proxy:

```nginx
upstream unison_api {
    server localhost:5000;
    server localhost:5001 backup;
}

server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://unison_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### HAProxy Configuration

```haproxy
global
    daemon

defaults
    mode http
    timeout connect 5000ms
    timeout client 50000ms
    timeout server 50000ms

frontend unison_api_frontend
    bind *:80
    default_backend unison_api_backend

backend unison_api_backend
    balance roundrobin
    server web1 127.0.0.1:5000 check
    server web2 127.0.0.1:5001 check backup
```

## SSL/TLS Configuration

### Let's Encrypt with Certbot

```bash
# Install certbot
sudo apt-get install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d api.yourdomain.com

# Auto-renewal
sudo crontab -e
# Add: 0 12 * * * /usr/bin/certbot renew --quiet
```

### Custom SSL Certificate

Configure in `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "certificate-password"
        }
      }
    }
  }
}
```

## Monitoring and Health Checks

### Application Insights (Azure)

Add to `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here"
  }
}
```

### Custom Health Check Endpoints

The API includes health check endpoints:

- `GET /api/health/ping` - Basic health status
- Future: `GET /health/ready` - Readiness probe
- Future: `GET /health/live` - Liveness probe

### Logging Configuration

Configure structured logging in `appsettings.Production.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/unison-rest-adapter/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Security Considerations

### Network Security

1. **Firewall Configuration**:

   - Allow incoming connections only on necessary ports (80, 443)
   - Restrict access to management ports
   - Use VPN for administrative access

2. **API Gateway Integration**:
   - Implement rate limiting
   - Add request/response validation
   - Enable request logging

### Authentication and Authorization

1. **Token Validation**:

   - Implement token expiration checks
   - Add token revocation support
   - Log authentication failures

2. **HTTPS Enforcement**:
   - Redirect HTTP to HTTPS
   - Use HSTS headers
   - Implement certificate pinning

### Data Protection

1. **Sensitive Data Handling**:
   - Never log authentication tokens
   - Encrypt configuration files
   - Use secure key management

## Performance Optimization

### Application Settings

Configure in `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 10485760
    }
  }
}
```

### Connection Pooling

For SOAP service connections:

```json
{
  "UnisonService": {
    "SoapEndpoint": "https://soap-service/endpoint",
    "Timeout": "00:01:00",
    "MaxConnections": 10,
    "ConnectionLifetime": "00:05:00"
  }
}
```

## Troubleshooting

### Common Issues

1. **Service Won't Start**:

   - Check .NET runtime installation
   - Verify port availability
   - Review application logs

2. **SOAP Backend Connectivity**:

   - Test network connectivity
   - Verify endpoint URL
   - Check authentication tokens

3. **Performance Issues**:
   - Monitor memory usage
   - Check connection pool settings
   - Review query performance

### Log Analysis

Key log patterns to monitor:

```bash
# Error patterns
grep "ERROR" /var/log/unison-rest-adapter/log-*.txt

# Authentication failures
grep "401\|Unauthorized" /var/log/unison-rest-adapter/log-*.txt

# Performance issues
grep "timeout\|slow" /var/log/unison-rest-adapter/log-*.txt
```

## Backup and Recovery

### Configuration Backup

```bash
# Backup configuration files
tar -czf unison-config-backup-$(date +%Y%m%d).tar.gz \
  /var/www/unison-rest-adapter/appsettings*.json \
  /etc/systemd/system/unison-rest-adapter.service
```

### Database Backup

If using persistent storage:

```bash
# Backup application data
cp -r /var/lib/unison-rest-adapter /backup/unison-data-$(date +%Y%m%d)
```

## Deployment Checklist

- [ ] .NET 9.0 runtime installed
- [ ] Application published and copied to target
- [ ] Configuration files updated for environment
- [ ] SSL certificates configured
- [ ] Firewall rules configured
- [ ] Health checks passing
- [ ] Monitoring and logging configured
- [ ] Backup procedures in place
- [ ] Load balancer configured (if applicable)
- [ ] Security scanning completed
- [ ] Performance testing completed

---

**Note**: Always test deployments in a staging environment that mirrors production before deploying to production systems.
