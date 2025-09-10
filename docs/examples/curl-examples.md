# cURL API Examples - Unison REST Adapter

This document provides comprehensive cURL examples for all Unison REST Adapter API endpoints.

## Authentication

All authenticated endpoints require the `Unison-Token` header:

```bash
export UNISON_TOKEN="your-token-here"
```

## Health Check Endpoints

### Basic Health Check

**Endpoint**: `GET /health`  
**Authentication**: Not required  
**Description**: Basic health status check

```bash
# Basic health check
curl -X GET "http://localhost:5203/health" \
  -H "Accept: application/json"
```

**Response**:

```json
{
  "status": "Healthy"
}
```

### Detailed Health Check

**Endpoint**: `GET /health/detailed`  
**Authentication**: Required  
**Description**: Detailed system health including SOAP connectivity

```bash
# Detailed health check with authentication
curl -X GET "http://localhost:5203/health/detailed" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN"
```

**Response**:

```json
{
  "status": "Healthy",
  "timestamp": "2025-09-09T10:30:00Z",
  "soapService": {
    "status": "Connected",
    "endpoint": "http://192.168.10.206:9003/Unison.AccessService",
    "responseTime": "150ms"
  },
  "version": "1.0.0"
}
```

## Card Management Endpoints

### Create Card

**Endpoint**: `POST /api/cards`  
**Authentication**: Required  
**Description**: Create a new card in the system

```bash
# Create a new card
curl -X POST "http://localhost:5203/api/cards" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN" \
  -d '{
    "cardId": "CARD001",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true
  }'
```

**Response (Success)**:

```json
{
  "success": true,
  "message": "Card created successfully",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "cardId": "CARD001",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

**Response (Error)**:

```json
{
  "success": false,
  "error": "Card ID already exists",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

### Update Card

**Endpoint**: `PUT /api/cards/update`  
**Authentication**: Required  
**Description**: Update an existing card

```bash
# Update existing card
curl -X PUT "http://localhost:5203/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN" \
  -d '{
    "cardId": "CARD001",
    "userName": "john.doe.updated",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": false
  }'
```

**Response**:

```json
{
  "success": true,
  "message": "Card updated successfully",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "cardId": "CARD001",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

### Get Card Details

**Endpoint**: `GET /api/cards/{cardId}`  
**Authentication**: Required  
**Description**: Retrieve details for a specific card

```bash
# Get card details
curl -X GET "http://localhost:5203/api/cards/CARD001" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN"
```

**Response**:

```json
{
  "success": true,
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "card": {
    "cardId": "CARD001",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "lastModified": "2025-09-09T10:30:00Z"
  },
  "timestamp": "2025-09-09T10:30:00Z"
}
```

### Delete Card

**Endpoint**: `DELETE /api/cards/{cardId}`  
**Authentication**: Required  
**Description**: Delete a specific card from the system

```bash
# Delete card
curl -X DELETE "http://localhost:5203/api/cards/CARD001" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN"
```

**Response**:

```json
{
  "success": true,
  "message": "Card deleted successfully",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "cardId": "CARD001",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

## User Management Endpoints

### Create User

**Endpoint**: `POST /api/users`  
**Authentication**: Required  
**Description**: Create a new user in the system

```bash
# Create new user
curl -X POST "http://localhost:5203/api/users" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN" \
  -d '{
    "userName": "jane.smith",
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@example.com",
    "isActive": true,
    "department": "Engineering"
  }'
```

**Response**:

```json
{
  "success": true,
  "message": "User created successfully",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "userId": "USER123",
  "userName": "jane.smith",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

### Update User

**Endpoint**: `PUT /api/users/{userId}`  
**Authentication**: Required  
**Description**: Update an existing user

```bash
# Update user
curl -X PUT "http://localhost:5203/api/users/USER123" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith-Johnson",
    "email": "jane.smith-johnson@example.com",
    "isActive": true,
    "department": "Senior Engineering"
  }'
```

**Response**:

```json
{
  "success": true,
  "message": "User updated successfully",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "userId": "USER123",
  "timestamp": "2025-09-09T10:30:00Z"
}
```

## Advanced Examples

### Batch Operations with Shell Script

```bash
#!/bin/bash
# batch-operations.sh - Example batch operations script

# Configuration
API_BASE="http://localhost:5203"
UNISON_TOKEN="your-token-here"

# Function to create card
create_card() {
    local card_id=$1
    local username=$2
    local firstname=$3
    local lastname=$4

    echo "Creating card: $card_id"

    response=$(curl -s -X POST "$API_BASE/api/cards" \
        -H "Content-Type: application/json" \
        -H "Accept: application/json" \
        -H "Unison-Token: $UNISON_TOKEN" \
        -d "{
            \"cardId\": \"$card_id\",
            \"userName\": \"$username\",
            \"firstName\": \"$firstname\",
            \"lastName\": \"$lastname\",
            \"isActive\": true
        }")

    echo "Response: $response"
    echo "---"
}

# Create multiple cards
create_card "BATCH001" "user1" "User" "One"
create_card "BATCH002" "user2" "User" "Two"
create_card "BATCH003" "user3" "User" "Three"

echo "Batch operations completed!"
```

### Error Handling Example

```bash
#!/bin/bash
# error-handling-example.sh

API_BASE="http://localhost:5203"
UNISON_TOKEN="your-token-here"

# Function to handle API call with error checking
call_api() {
    local method=$1
    local endpoint=$2
    local data=$3

    echo "Making $method request to $endpoint"

    response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X $method "$API_BASE$endpoint" \
        -H "Content-Type: application/json" \
        -H "Accept: application/json" \
        -H "Unison-Token: $UNISON_TOKEN" \
        -d "$data")

    # Extract HTTP code and body
    http_code=$(echo "$response" | tail -n1 | sed 's/.*HTTP_CODE://')
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo "✅ Success ($http_code): $body"
        return 0
    else
        echo "❌ Error ($http_code): $body"
        return 1
    fi
}

# Example with error handling
if call_api "POST" "/api/cards" '{
    "cardId": "ERROR_TEST",
    "userName": "test.user",
    "firstName": "Test",
    "lastName": "User",
    "isActive": true
}'; then
    echo "Card created successfully, now trying to create duplicate..."

    # This should fail with duplicate error
    call_api "POST" "/api/cards" '{
        "cardId": "ERROR_TEST",
        "userName": "test.user2",
        "firstName": "Test2",
        "lastName": "User2",
        "isActive": true
    }'
else
    echo "Initial card creation failed"
fi
```

### Health Monitoring Script

```bash
#!/bin/bash
# health-monitor.sh - Continuous health monitoring

API_BASE="http://localhost:5203"
UNISON_TOKEN="your-token-here"
CHECK_INTERVAL=30  # seconds

echo "Starting health monitor for Unison REST Adapter..."
echo "Checking every ${CHECK_INTERVAL} seconds"
echo "Press Ctrl+C to stop"

while true; do
    timestamp=$(date '+%Y-%m-%d %H:%M:%S')

    # Basic health check
    basic_health=$(curl -s -w "%{http_code}" -o /dev/null "$API_BASE/health")

    if [ "$basic_health" = "200" ]; then
        echo "[$timestamp] ✅ Basic health: OK"

        # Detailed health check
        detailed_response=$(curl -s "$API_BASE/health/detailed" \
            -H "Accept: application/json" \
            -H "Unison-Token: $UNISON_TOKEN")

        if [ $? -eq 0 ]; then
            soap_status=$(echo "$detailed_response" | jq -r '.soapService.status // "Unknown"')
            echo "[$timestamp] ✅ Detailed health: OK, SOAP: $soap_status"
        else
            echo "[$timestamp] ⚠️  Detailed health check failed"
        fi
    else
        echo "[$timestamp] ❌ Basic health check failed (HTTP: $basic_health)"
    fi

    sleep $CHECK_INTERVAL
done
```

## Testing and Debugging

### Verbose Output Example

```bash
# Enable verbose output for debugging
curl -v -X GET "http://localhost:5203/health" \
  -H "Accept: application/json"

# Save response headers and body to files
curl -X GET "http://localhost:5203/health/detailed" \
  -H "Accept: application/json" \
  -H "Unison-Token: $UNISON_TOKEN" \
  -D response-headers.txt \
  -o response-body.json

# Check response headers
cat response-headers.txt

# Check response body
cat response-body.json
```

### Performance Testing

```bash
# Simple load test with curl
echo "Running performance test..."

for i in {1..10}; do
    echo -n "Request $i: "

    start_time=$(date +%s.%N)

    curl -s -o /dev/null -w "%{http_code}" "http://localhost:5203/health"

    end_time=$(date +%s.%N)
    duration=$(echo "$end_time - $start_time" | bc)

    echo " - ${duration}s"
done
```

## Common HTTP Status Codes

| Code | Meaning               | Description                                       |
| ---- | --------------------- | ------------------------------------------------- |
| 200  | OK                    | Request successful                                |
| 400  | Bad Request           | Invalid request format or missing required fields |
| 401  | Unauthorized          | Missing or invalid Unison-Token                   |
| 404  | Not Found             | Endpoint or resource not found                    |
| 409  | Conflict              | Resource already exists (e.g., duplicate card ID) |
| 500  | Internal Server Error | Server error, check logs                          |
| 502  | Bad Gateway           | SOAP service connectivity issue                   |
| 503  | Service Unavailable   | Service temporarily unavailable                   |

## Environment Variables Reference

```bash
# Required environment variables
export UNISON_TOKEN="your-production-token"
export API_BASE_URL="http://localhost:5203"  # or production URL

# Optional debugging
export CURL_DEBUG=1  # Enable verbose output
export LOG_REQUESTS=1  # Log all requests (custom implementation)
```

---

**Documentation Version**: 1.0  
**Last Updated**: September 9, 2025  
**API Version**: Compatible with Unison REST Adapter v1.0

**Quick Start**:

1. Set your authentication token: `export UNISON_TOKEN="your-token"`
2. Test connectivity: `curl http://localhost:5203/health`
3. Try authenticated endpoint: `curl -H "Unison-Token: $UNISON_TOKEN" http://localhost:5203/health/detailed`
