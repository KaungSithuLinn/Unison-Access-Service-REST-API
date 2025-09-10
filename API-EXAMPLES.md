# Unison REST Adapter API Examples

## Overview

The Unison REST Adapter provides RESTful HTTP endpoints that proxy requests to the underlying SOAP-based Unison Access Service.

**Base URL**: `http://192.168.10.206:5203`
**Authentication**: Required via `Unison-Token` header
**Content-Type**: `application/json`

---

## Authentication

All API requests require a valid Unison token in the request header:

```http
Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7
```

---

## Endpoints

### 1. Update Card - Primary Endpoint

**Endpoint**: `POST /api/cards/update`

Updates card information in the Unison system.

#### Request Example (cURL)

```bash
curl -X POST "http://192.168.10.206:5203/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{
    "cardId": "12345",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "department": "Information Technology",
    "title": "Software Developer",
    "isActive": true
  }'
```

#### Request Example (PowerShell)

```powershell
$headers = @{
    "Content-Type" = "application/json"
    "Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
}

$body = @{
    cardId = "12345"
    userName = "john.doe"
    firstName = "John"
    lastName = "Doe"
    email = "john.doe@company.com"
    department = "Information Technology"
    title = "Software Developer"
    isActive = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://192.168.10.206:5203/api/cards/update" -Method POST -Headers $headers -Body $body
```

#### Request Example (Python)

```python
import requests
import json

url = "http://192.168.10.206:5203/api/cards/update"
headers = {
    "Content-Type": "application/json",
    "Unison-Token": "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
}

payload = {
    "cardId": "12345",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "department": "Information Technology",
    "title": "Software Developer",
    "isActive": True
}

response = requests.post(url, headers=headers, json=payload)
print(f"Status: {response.status_code}")
print(f"Response: {response.json()}")
```

#### Request Schema

```json
{
  "cardId": "string (required)",
  "userName": "string (optional)",
  "firstName": "string (optional)",
  "lastName": "string (optional)",
  "email": "string (optional)",
  "department": "string (optional)",
  "title": "string (optional)",
  "isActive": "boolean (optional)"
}
```

#### Success Response (200)

```json
{
  "success": true,
  "message": "Card updated successfully",
  "cardId": "12345",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

#### Error Response (400)

```json
{
  "success": false,
  "message": "Invalid card ID format",
  "errors": ["CardId is required"],
  "timestamp": "2025-09-09T10:30:00Z"
}
```

---

### 2. Update Card - Alternative Endpoint

**Endpoint**: `POST /updatecard`

Alternative route for backward compatibility.

#### Request Example (cURL)

```bash
curl -X POST "http://192.168.10.206:5203/updatecard" \
  -H "Content-Type: application/json" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7" \
  -d '{
    "cardId": "12345",
    "userName": "jane.smith",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": false
  }'
```

---

### 3. Get Card Information

**Endpoint**: `GET /api/cards/{cardId}`

Retrieves card information by card ID.

#### Request Example (cURL)

```bash
curl -X GET "http://192.168.10.206:5203/api/cards/12345" \
  -H "Unison-Token: 595d799a-9553-4ddf-8fd9-c27b1f233ce7"
```

#### Success Response (200)

```json
{
  "cardId": "12345",
  "userName": "john.doe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "department": "Information Technology",
  "title": "Software Developer",
  "isActive": true,
  "lastModified": "2025-09-09T10:30:00Z"
}
```

---

### 4. Health Check

**Endpoint**: `GET /health`

Service health status endpoint (no authentication required).

#### Request Example (cURL)

```bash
curl -X GET "http://192.168.10.206:5203/health"
```

#### Response (200)

```json
{
  "status": "Healthy",
  "timestamp": "2025-09-09T10:30:00Z",
  "version": "1.0.0"
}
```

---

## Error Codes

| Code | Description           | Common Causes                            |
| ---- | --------------------- | ---------------------------------------- |
| 400  | Bad Request           | Invalid JSON, missing required fields    |
| 401  | Unauthorized          | Missing or invalid Unison-Token          |
| 403  | Forbidden             | IP address not whitelisted               |
| 404  | Not Found             | Card ID not found in system              |
| 429  | Too Many Requests     | Rate limit exceeded                      |
| 500  | Internal Server Error | SOAP service unavailable, network issues |

---

## Rate Limits

- **Default**: 1000 requests per hour per token
- **Burst**: Up to 100 requests per minute
- **Headers**: Rate limit info included in response headers

---

## Testing Tools

### Swagger UI

Access the interactive API documentation at:
`http://192.168.10.206:5203/swagger`

### Postman Collection

Import the provided Postman collection for easy testing:

- Download: [Unison REST Adapter.postman_collection.json](./postman/Unison%20REST%20Adapter.postman_collection.json)
- Environment: [Unison Environment.postman_environment.json](./postman/Unison%20Environment.postman_environment.json)

### Health Monitoring

Monitor service health via:

```bash
# Windows (PowerShell)
while ($true) {
  $response = Invoke-RestMethod -Uri "http://192.168.10.206:5203/health" -Method GET
  Write-Host "$(Get-Date): $($response.status)"
  Start-Sleep -Seconds 30
}

# Linux/Mac (bash)
while true; do
  curl -s http://192.168.10.206:5203/health | jq '.status'
  sleep 30
done
```

---

## Troubleshooting

### Common Issues

1. **Connection Refused**

   - Verify service is running: `Get-Service -Name "UnisonRestAdapter"`
   - Check port availability: `netstat -an | findstr :5203`

2. **SOAP Backend Unavailable**

   - Test SOAP endpoint: `http://192.168.10.206:9003/Unison.AccessService?wsdl`
   - Check network connectivity between services

3. **Authentication Issues**

   - Verify token format and validity
   - Check token hasn't expired (if rotation enabled)

4. **Performance Issues**
   - Monitor rate limits
   - Check SOAP service response times
   - Review connection pooling configuration

### Support

For issues or questions, contact the development team or check the logs:

- **Service Logs**: Windows Event Viewer > Application
- **Debug Logs**: `C:\Services\UnisonRestAdapter\Logs\` (if file logging enabled)
