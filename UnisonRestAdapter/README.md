# Unison Access Service REST Adapter - Implementation Documentation

## 🎯 Project Overview

The Unison Access Service REST Adapter is a complete ASP.NET Core WebAPI solution that provides a modern REST interface to the existing Unison Access Service SOAP endpoint. This adapter enables seamless integration between modern REST-based applications and the legacy SOAP service.

## 📋 Features

### Core Functionality

- **REST-to-SOAP Translation**: Converts REST API calls to SOAP requests
- **Authentication Passthrough**: Forwards Unison-Token authentication
- **Error Handling**: Comprehensive error handling with proper HTTP status codes
- **Health Monitoring**: Built-in health checks for service availability
- **Logging**: Structured logging for monitoring and debugging
- **Auto-Documentation**: Swagger/OpenAPI documentation

### Supported Operations

1. **UpdateCard** - Update card information via REST PUT endpoint
2. **GetCard** - Retrieve card information via REST GET endpoint
3. **Health Check** - Service availability monitoring

## 🏗️ Architecture

### Project Structure

```
UnisonRestAdapter/
├── Controllers/
│   ├── CardsController.cs      # REST API endpoints for card operations
│   ├── HealthController.cs     # Health check endpoints
│   └── UsersController.cs      # User management endpoints
├── Services/
│   ├── IUnisonService.cs       # Business logic interface
│   ├── UnisonService.cs        # Business logic implementation
│   ├── ISoapClientService.cs   # SOAP client interface
│   └── SoapClientService.cs    # SOAP client implementation
├── Models/
│   ├── Request/
│   │   └── UpdateCardRequest.cs    # REST request models
│   └── Response/
│       └── ResponseModels.cs       # REST response models
├── Configuration/
│   ├── ServiceConfiguration.cs    # Dependency injection setup
│   ├── UnisonSettings.cs          # Configuration models
│   └── UnisonHealthCheck.cs       # Health check implementation
├── Program.cs                  # Application startup
├── appsettings.json           # Development configuration
└── appsettings.Production.json # Production configuration
```

### Technology Stack

- **Framework**: ASP.NET Core 9.0
- **SOAP Client**: HttpClient with manual SOAP envelope creation
- **Documentation**: Swashbuckle.AspNetCore (Swagger/OpenAPI)
- **Mapping**: AutoMapper for object transformations
- **Logging**: Microsoft.Extensions.Logging
- **Health Checks**: Microsoft.Extensions.Diagnostics.HealthChecks

## 🔧 Configuration

### Application Settings

#### Development (appsettings.json)

```json
{
  "Unison": {
    "ServiceUrl": "http://192.168.10.206:9003/Unison.AccessService",
    "WsdlUrl": "http://192.168.10.206:9003/Unison.AccessService?wsdl",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3
  }
}
```

#### Production (appsettings.Production.json)

```json
{
  "Unison": {
    "ServiceUrl": "http://192.168.10.206:9003/Unison.AccessService",
    "WsdlUrl": "http://192.168.10.206:9003/Unison.AccessService?wsdl",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3,
    "DefaultToken": "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:5000"
      }
    }
  }
}
```

## 📚 API Documentation

### Base URL

- **Development**: `http://localhost:5000/api`
- **Production**: `http://192.168.10.206:5000/api`

### Authentication

All endpoints require a `Unison-Token` header:

```http
Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7
```

### Endpoints

#### 1. Update Card

**Endpoint**: `PUT /api/cards/update`

**Description**: Updates card information in the Unison Access Service

**Request Headers**:

```http
Content-Type: application/json
Unison-Token: {your-token}
```

**Request Body**:

```json
{
  "cardId": "TEST_CARD_12345",
  "userName": "TEST_USER_001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@test.com",
  "department": "IT",
  "title": "Software Engineer",
  "isActive": true,
  "expirationDate": "2025-12-31T23:59:59Z",
  "customFields": {
    "location": "Building A",
    "access_level": "Standard"
  }
}
```

**Response**:

```json
{
  "success": true,
  "message": "Card updated successfully",
  "cardId": "TEST_CARD_12345",
  "timestamp": "2025-01-27T10:00:00Z",
  "transactionId": "abc123"
}
```

#### 2. Get Card

**Endpoint**: `GET /api/cards/{cardId}`

**Description**: Retrieves card information from the Unison Access Service

**Request Headers**:

```http
Unison-Token: {your-token}
```

**Response**:

```json
{
  "success": true,
  "userName": "TEST_USER_001",
  "fullName": "John Doe",
  "email": "john.doe@test.com",
  "department": "IT",
  "isActive": true
}
```

#### 3. Health Check

**Endpoint**: `GET /api/health`

**Description**: Checks the health of the adapter and SOAP service connectivity

**Request Headers**:

```http
Unison-Token: {your-token}
```

**Response**:

```json
{
  "isHealthy": true,
  "message": "Service is healthy",
  "timestamp": "2025-01-27T10:00:00Z",
  "version": "1.0.0",
  "dependencies": {
    "soapService": "healthy"
  }
}
```

## 🚀 Deployment

### Prerequisites

- Windows Server 2019 or later
- .NET 9.0 Runtime
- Network access to Unison SOAP service (192.168.10.206:9003)
- Administrative privileges for service installation

### Deployment Steps

1. **Build and Publish**:

   ```powershell
   dotnet publish -c Release -o publish --self-contained false
   ```

2. **Deploy to Server**:

   ```powershell
   .\deploy.ps1 -TargetServer "192.168.10.206" -TargetPath "C:\Services\UnisonRestAdapter"
   ```

3. **Install as Windows Service**:
   ```powershell
   .\install_service.ps1 -ServiceName "UnisonRestAdapter" -BinaryPath "C:\Services\UnisonRestAdapter\UnisonRestAdapter.exe"
   ```

### Manual Deployment

If automated deployment fails, follow these manual steps:

1. Copy published files to `C:\Services\UnisonRestAdapter\`
2. Install Windows service:
   ```cmd
   sc create UnisonRestAdapter binpath= "C:\Services\UnisonRestAdapter\UnisonRestAdapter.exe" displayname= "Unison REST Adapter" start= auto
   ```
3. Start the service:
   ```cmd
   sc start UnisonRestAdapter
   ```

## 🧪 Testing

### Automated Tests

#### PowerShell Test Script

```powershell
.\test_api.ps1 -BaseUrl "http://192.168.10.206:5000/api" -Token "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
```

#### Python Test Script

```bash
python test_api.py
```

### Manual Testing

#### Using curl

```bash
# Health Check
curl -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
     http://192.168.10.206:5000/api/health

# Update Card
curl -X PUT \
     -H "Content-Type: application/json" \
     -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
     -d '{"cardId":"TEST123","userName":"testuser","isActive":true}' \
     http://192.168.10.206:5000/api/cards/update
```

#### Using PowerShell

```powershell
$headers = @{ "Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7" }
Invoke-RestMethod -Uri "http://192.168.10.206:5000/api/health" -Headers $headers
```

## 🔍 Monitoring and Troubleshooting

### Logging

Logs are written to the Windows Event Log and console. Key log categories:

- `UnisonRestAdapter.Controllers`: API request/response logging
- `UnisonRestAdapter.Services`: Business logic and SOAP communication
- `Microsoft.AspNetCore`: Framework-level logging

### Health Monitoring

- Health check endpoint: `/api/health`
- Windows Service status via Services.msc
- Application logs in Event Viewer

### Common Issues

#### 1. SOAP Service Connectivity

**Symptoms**: Health check fails, UpdateCard returns errors
**Solutions**:

- Verify network connectivity to 192.168.10.206:9003
- Check Unison service status
- Validate firewall rules

#### 2. Authentication Errors

**Symptoms**: 401 Unauthorized responses
**Solutions**:

- Verify Unison-Token header is present
- Check token validity with Unison service
- Review authentication logs

#### 3. Service Won't Start

**Symptoms**: Windows service fails to start
**Solutions**:

- Check .NET 9.0 runtime installation
- Verify file permissions on service directory
- Review Windows Event Log for startup errors

## 🔐 Security Considerations

### Authentication

- Token-based authentication using Unison-Token header
- No token storage or caching (passthrough authentication)
- All tokens validated against Unison SOAP service

### Network Security

- HTTP communication (no TLS by default)
- Firewall rules should restrict access to port 5000
- Consider reverse proxy for TLS termination

### Data Protection

- No sensitive data stored locally
- All requests proxied to backend SOAP service
- Logging configured to avoid token exposure

## 📈 Performance

### Expected Performance

- **Latency**: ~100-500ms (depends on SOAP service response time)
- **Throughput**: Limited by SOAP service capacity
- **Memory**: ~50-100MB for basic operation
- **CPU**: Minimal overhead for REST-to-SOAP translation

### Optimization Recommendations

- Implement connection pooling for SOAP client
- Add response caching for read operations
- Monitor SOAP service performance
- Consider load balancing for high availability

## 🛠️ Maintenance

### Regular Tasks

- Monitor service health and logs
- Update authentication tokens as needed
- Review performance metrics
- Apply security updates

### Backup Considerations

- Configuration files (appsettings.json)
- Service installation scripts
- Application binaries

### Update Process

1. Stop Windows service
2. Backup current installation
3. Deploy new version
4. Update configuration if needed
5. Start service and verify functionality

## 📞 Support Information

### Service Details

- **Service Name**: UnisonRestAdapter
- **Display Name**: Unison Access Service REST Adapter
- **Installation Path**: C:\Services\UnisonRestAdapter
- **Port**: 5000
- **Protocol**: HTTP

### Key Contacts

- **Development Team**: [Contact information]
- **Infrastructure Team**: [Contact information]
- **Unison Service Owner**: [Contact information]

---

_Documentation last updated: January 27, 2025_
_Version: 1.0.0_
