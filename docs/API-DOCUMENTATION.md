# Unison REST Adapter API Documentation

## Overview

The Unison REST Adapter API provides JSON HTTP endpoints that proxy to an existing SOAP WCF service. This REST-to-SOAP adapter simplifies integration with the Unison Access Service by providing modern REST endpoints while maintaining compatibility with the existing SOAP backend.

## Base URL

- **Development**: `http://localhost:5203`
- **Production**: `https://api.company.com` _(when deployed)_

## Authentication

All API endpoints (except health checks) require authentication via the `Unison-Token` header.

### Header Format

```http
Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7
```

### Authentication Examples

**✅ Valid Request:**

```bash
curl -X PUT http://localhost:5203/api/cards/update \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{"cardId": "CARD123", "firstName": "John"}'
```

**❌ Missing Token:**

```bash
curl -X PUT http://localhost:5203/api/cards/update \
  -H "Content-Type: application/json" \
  -d '{"cardId": "CARD123", "firstName": "John"}'

# Response: 401 Unauthorized
{
  "error": "Unison-Token header is required",
  "timestamp": "2025-01-05T10:30:00Z"
}
```

## API Endpoints

### Health Endpoints

#### 🟢 GET /health

Basic health check for load balancers and monitoring tools.

**Features:**

- No authentication required
- Fast response time
- Basic service status

**Example Request:**

```bash
curl http://localhost:5203/health
```

**Example Response:**

```json
{
  "status": "Healthy",
  "timestamp": "2025-01-05T10:30:00Z",
  "service": "UnisonRestAdapter",
  "version": "1.0.0",
  "environment": "Development"
}
```

#### 🔍 GET /health/detailed

Comprehensive health check with dependency validation.

**Features:**

- Optional authentication (enhanced checks with token)
- System metrics and dependency status
- SOAP service connectivity test

**Example Request:**

```bash
curl -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  http://localhost:5203/health/detailed
```

**Example Response:**

```json
{
  "status": "Healthy",
  "timestamp": "2025-01-05T10:30:00Z",
  "service": "UnisonRestAdapter",
  "version": "1.0.0",
  "environment": "Development",
  "responseTime": 25,
  "checks": {
    "Application": {
      "status": "Healthy",
      "uptime": "2 days, 4 hours",
      "memory": "45 MB",
      "processId": 12345
    },
    "SOAPService": {
      "status": "Healthy",
      "details": "SOAP service connectivity verified",
      "responseTime": 150
    },
    "Configuration": {
      "status": "Healthy",
      "details": "All required configuration values present"
    }
  }
}
```

### Card Management Endpoints

#### 📝 PUT /api/cards/update

Updates card information in the Unison system.

**Features:**

- Primary card update endpoint
- Comprehensive validation
- Transaction ID tracking
- SOAP backend integration

**Request Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `cardId` | string | ✅ | Card identifier (1-50 chars, alphanumeric) |
| `userName` | string | ❌ | User's login name |
| `firstName` | string | ❌ | User's first name |
| `lastName` | string | ❌ | User's last name |
| `email` | string | ❌ | User's email address |
| `department` | string | ❌ | User's department |
| `title` | string | ❌ | User's job title |
| `isActive` | boolean | ❌ | Whether the card is active |
| `expirationDate` | datetime | ❌ | Card expiration date (ISO 8601) |
| `customFields` | object | ❌ | Additional custom properties |

**Example Request - Full Update:**

```bash
curl -X PUT http://localhost:5203/api/cards/update \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{
    "cardId": "CARD123",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "department": "Engineering",
    "title": "Software Developer",
    "isActive": true,
    "expirationDate": "2025-12-31T00:00:00Z",
    "customFields": {
      "building": "Building A",
      "floor": "3"
    }
  }'
```

**Example Request - Minimal Update:**

```bash
curl -X PUT http://localhost:5203/api/cards/update \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{
    "cardId": "CARD456",
    "firstName": "Jane",
    "isActive": false
  }'
```

**Success Response (200):**

```json
{
  "success": true,
  "message": "Card updated successfully",
  "cardId": "CARD123",
  "timestamp": "2025-01-05T10:30:00Z",
  "transactionId": "TXN-20250105-103000-CARD123"
}
```

**Validation Error Response (400):**

```json
{
  "success": false,
  "message": "CardId is required and must be 1-50 characters",
  "cardId": "",
  "timestamp": "2025-01-05T10:30:00Z",
  "transactionId": null
}
```

**SOAP Service Error Response (400):**

```json
{
  "success": false,
  "message": "Card not found in system",
  "cardId": "INVALID123",
  "timestamp": "2025-01-05T10:30:00Z",
  "transactionId": "TXN-20250105-103000-INVALID123"
}
```

#### 🔍 GET /api/cards/{cardId}

Retrieves card information by card ID.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `cardId` | string | Card identifier to lookup |

**Example Request:**

```bash
curl -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  http://localhost:5203/api/cards/CARD123
```

**Success Response (200):**

```json
{
  "userId": "USER123",
  "userName": "john.doe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "department": "Engineering",
  "title": "Software Developer",
  "isActive": true,
  "success": true,
  "message": "User information retrieved successfully",
  "timestamp": "2025-01-05T10:30:00Z"
}
```

**Not Found Response (404):**

```json
{
  "userId": "",
  "userName": null,
  "firstName": null,
  "lastName": null,
  "email": null,
  "department": null,
  "title": null,
  "isActive": false,
  "success": false,
  "message": "Card not found",
  "timestamp": "2025-01-05T10:30:00Z"
}
```

#### 📝 POST /updatecard

Alternative POST endpoint for card updates (deprecated).

**⚠️ Deprecated:** Use `PUT /api/cards/update` instead.

This endpoint maintains backward compatibility for clients that cannot use PUT requests.

**Example Request:**

```bash
curl -X POST http://localhost:5203/updatecard \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{"cardId": "CARD123", "firstName": "John"}'
```

### User Management Endpoints

#### 👤 GET /api/users/{userId}

Retrieves user information by user ID.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `userId` | string | User identifier to lookup |

**Example Request:**

```bash
curl -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  http://localhost:5203/api/users/USER123
```

**Response:** Same format as card lookup response.

## Error Handling

### HTTP Status Codes

| Code | Description                                       |
| ---- | ------------------------------------------------- |
| 200  | Success                                           |
| 400  | Bad Request - Invalid input or SOAP service error |
| 401  | Unauthorized - Missing or invalid token           |
| 404  | Not Found - Card/user not found                   |
| 500  | Internal Server Error                             |

### Error Response Format

All error responses follow a consistent JSON structure:

```json
{
  "success": false,
  "message": "Human-readable error description",
  "timestamp": "2025-01-05T10:30:00Z",
  "transactionId": "TXN-ID-or-null"
}
```

## SOAP Backend Integration

### Backend Service Details

- **SOAP Endpoint**: `http://192.168.10.206:9003/Unison.AccessService`
- **WSDL**: `http://192.168.10.206:9003/Unison.AccessService?wsdl`
- **Binding**: basicHttpBinding
- **Available Operations**: 50+ SOAP operations

### Request Mapping

The REST API automatically maps JSON requests to SOAP XML format:

**REST Request (JSON):**

```json
{
  "cardId": "CARD123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**SOAP Request (XML):**

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <UpdateCard xmlns="http://tempuri.org/">
      <cardNumber>CARD123</cardNumber>
      <firstName>John</firstName>
      <lastName>Doe</lastName>
    </UpdateCard>
  </soap:Body>
</soap:Envelope>
```

## Rate Limiting

Currently, no rate limiting is implemented. This may be added in future versions for production deployments.

## Versioning

- **Current Version**: v1
- **API Version Header**: Not currently implemented
- **Backward Compatibility**: Maintained through alternative endpoints

## Development and Testing

### Local Development

1. **Start the service:**

   ```bash
   cd UnisonRestAdapter
   dotnet run
   ```

2. **Access Swagger UI:**

   - URL: http://localhost:5203/swagger
   - Interactive API documentation and testing

3. **Test endpoints:**

   ```bash
   # Health check
   curl http://localhost:5203/health

   # API test with token
   curl -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
     http://localhost:5203/api/cards/TEST123
   ```

### Production Considerations

- **Authentication**: Implement proper token rotation and management
- **Logging**: All requests are logged with correlation IDs
- **Monitoring**: Use health endpoints for container orchestration
- **Security**: HTTPS enforcement in production
- **Performance**: Connection pooling and caching configured

## Support and Troubleshooting

### Common Issues

1. **401 Unauthorized**

   - Verify `Unison-Token` header is present and correct
   - Check token has not expired

2. **400 Bad Request**

   - Validate JSON request format
   - Check required fields are present
   - Verify card ID format (alphanumeric, 1-50 chars)

3. **SOAP Service Connectivity**
   - Use `/health/detailed` to check backend connectivity
   - Verify network access to `192.168.10.206:9003`

### Contact Information

- **Support Email**: support@company.com
- **API Documentation**: Available at `/swagger` endpoint
- **Service Logs**: Windows Event Log under "UnisonRestAdapter"

---

_Generated: January 5, 2025_  
_API Version: 1.0.0_
