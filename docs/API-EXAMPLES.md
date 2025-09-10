# Unison REST Adapter API - cURL Examples

## Environment Variables

```bash
# Set these environment variables for easy testing
export API_BASE_URL="http://localhost:5203"
export UNISON_TOKEN="595d799a-9553-4ddf-8fd9-c27b1f233ce7"
```

## Health Check Endpoints

### Basic Health Check (No Authentication Required)

```bash
curl -X GET "${API_BASE_URL}/health" \
  -H "Accept: application/json" \
  | jq '.'
```

**Expected Response:**

```json
{
  "status": "Healthy",
  "timestamp": "2025-01-05T10:30:00Z",
  "service": "UnisonRestAdapter",
  "version": "1.0.0",
  "environment": "Development"
}
```

### Detailed Health Check (Optional Authentication)

```bash
curl -X GET "${API_BASE_URL}/health/detailed" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  | jq '.'
```

### Kubernetes Health Probes

```bash
# Readiness probe
curl -X GET "${API_BASE_URL}/health/ready" \
  -H "Accept: application/json"

# Liveness probe
curl -X GET "${API_BASE_URL}/health/live" \
  -H "Accept: application/json"
```

## Card Management Endpoints

### Update Card - Complete Example

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
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
      "floor": "3",
      "accessLevel": "Standard"
    }
  }' \
  | jq '.'
```

### Update Card - Minimal Example

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "CARD456",
    "firstName": "Jane",
    "isActive": false
  }' \
  | jq '.'
```

### Update Card - Deactivate Card

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "CARD789",
    "isActive": false,
    "customFields": {
      "deactivationReason": "Employee departure"
    }
  }' \
  | jq '.'
```

### Get Card Information

```bash
curl -X GET "${API_BASE_URL}/api/cards/CARD123" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  | jq '.'
```

**Expected Success Response:**

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

## User Management Endpoints

### Get User Information

```bash
curl -X GET "${API_BASE_URL}/api/users/USER123" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  | jq '.'
```

## Alternative/Deprecated Endpoints

### Legacy POST Update (Deprecated)

```bash
curl -X POST "${API_BASE_URL}/updatecard" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "CARD999",
    "firstName": "Legacy",
    "lastName": "User"
  }' \
  | jq '.'
```

## Error Testing Examples

### Missing Authentication Token

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "cardId": "CARD123",
    "firstName": "Test"
  }' \
  | jq '.'
```

**Expected 401 Response:**

```json
{
  "error": "Unison-Token header is required",
  "timestamp": "2025-01-05T10:30:00Z"
}
```

### Invalid Authentication Token

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: invalid-token-12345" \
  -d '{
    "cardId": "CARD123",
    "firstName": "Test"
  }' \
  | jq '.'
```

### Missing Required Field (CardId)

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "firstName": "Test",
    "lastName": "User"
  }' \
  | jq '.'
```

**Expected 400 Response:**

```json
{
  "success": false,
  "message": "CardId is required and must be 1-50 characters",
  "cardId": "",
  "timestamp": "2025-01-05T10:30:00Z",
  "transactionId": null
}
```

### Invalid CardId Format

```bash
curl -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "INVALID@CARD#123",
    "firstName": "Test"
  }' \
  | jq '.'
```

### Card Not Found

```bash
curl -X GET "${API_BASE_URL}/api/cards/NONEXISTENT999" \
  -H "Accept: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  | jq '.'
```

**Expected 404 Response:**

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

## Batch Testing Script

Create a script to test multiple endpoints:

```bash
#!/bin/bash
# test-api.sh

API_BASE_URL="http://localhost:5203"
UNISON_TOKEN="595d799a-9553-4ddf-8fd9-c27b1f233ce7"

echo "=== Testing Unison REST Adapter API ==="
echo

echo "1. Basic Health Check (No Auth)"
curl -s -X GET "${API_BASE_URL}/health" | jq '.status'

echo
echo "2. Detailed Health Check (With Auth)"
curl -s -X GET "${API_BASE_URL}/health/detailed" \
  -H "Unison-Token: ${UNISON_TOKEN}" | jq '.status'

echo
echo "3. Update Card Test"
CARD_RESPONSE=$(curl -s -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "TESTCARD001",
    "firstName": "Test",
    "lastName": "User",
    "email": "test.user@company.com",
    "isActive": true
  }')

echo $CARD_RESPONSE | jq '.success'
TRANSACTION_ID=$(echo $CARD_RESPONSE | jq -r '.transactionId')
echo "Transaction ID: $TRANSACTION_ID"

echo
echo "4. Get Card Test"
curl -s -X GET "${API_BASE_URL}/api/cards/TESTCARD001" \
  -H "Unison-Token: ${UNISON_TOKEN}" | jq '.success'

echo
echo "5. Authentication Error Test"
curl -s -X GET "${API_BASE_URL}/api/cards/TESTCARD001" | jq '.error // "No error field"'

echo
echo "=== API Testing Complete ==="
```

## PowerShell Examples

For Windows environments without bash:

```powershell
# PowerShell API testing script
$API_BASE_URL = "http://localhost:5203"
$UNISON_TOKEN = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"

# Basic Health Check
$healthResponse = Invoke-RestMethod -Uri "$API_BASE_URL/health"
Write-Output "Health Status: $($healthResponse.status)"

# Update Card
$updateBody = @{
    cardId = "PSCARD001"
    firstName = "PowerShell"
    lastName = "Test"
    email = "ps.test@company.com"
    isActive = $true
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
    "Unison-Token" = $UNISON_TOKEN
}

try {
    $updateResponse = Invoke-RestMethod -Uri "$API_BASE_URL/api/cards/update" `
        -Method PUT -Body $updateBody -Headers $headers
    Write-Output "Update Success: $($updateResponse.success)"
    Write-Output "Transaction ID: $($updateResponse.transactionId)"
}
catch {
    Write-Output "Error: $($_.Exception.Message)"
}

# Get Card
try {
    $cardResponse = Invoke-RestMethod -Uri "$API_BASE_URL/api/cards/PSCARD001" `
        -Headers @{ "Unison-Token" = $UNISON_TOKEN }
    Write-Output "Card Found: $($cardResponse.success)"
    Write-Output "User Name: $($cardResponse.userName)"
}
catch {
    Write-Output "Error retrieving card: $($_.Exception.Message)"
}
```

## Performance Testing with cURL

### Response Time Testing

```bash
# Test response times
echo "=== Response Time Testing ==="

# Health endpoint (should be fastest)
echo "Health endpoint:"
curl -w "@curl-format.txt" -o /dev/null -s "${API_BASE_URL}/health"

# Card update endpoint
echo "Card update endpoint:"
curl -w "@curl-format.txt" -o /dev/null -s -X PUT "${API_BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{"cardId": "PERF001", "firstName": "Performance"}'

# Card retrieval endpoint
echo "Card retrieval endpoint:"
curl -w "@curl-format.txt" -o /dev/null -s "${API_BASE_URL}/api/cards/PERF001" \
  -H "Unison-Token: ${UNISON_TOKEN}"
```

Create `curl-format.txt`:

```text
     time_namelookup:  %{time_namelookup}s\n
        time_connect:  %{time_connect}s\n
     time_appconnect:  %{time_appconnect}s\n
    time_pretransfer:  %{time_pretransfer}s\n
       time_redirect:  %{time_redirect}s\n
  time_starttransfer:  %{time_starttransfer}s\n
                     ----------\n
          time_total:  %{time_total}s\n
```

## Integration with Other Tools

### Postman Collection Export

The API can be tested with Postman using the OpenAPI specification:

1. Import the OpenAPI spec: `docs/openapi/unison-rest-adapter-openapi-v1.yaml`
2. Set environment variables:
   - `baseUrl`: `http://localhost:5203`
   - `unisonToken`: `595d799a-9553-4ddf-8fd9-c27b1f233ce7`

### HTTPie Examples

```bash
# Install HTTPie: pip install httpie

# Basic health check
http GET localhost:5203/health

# Update card with HTTPie
http PUT localhost:5203/api/cards/update \
  Unison-Token:595d799a-9553-4ddf-8fd9-c27b1f233ce7 \
  cardId=HTTPIE001 \
  firstName=HTTPie \
  lastName=Test \
  email=httpie@company.com \
  isActive:=true

# Get card information
http GET localhost:5203/api/cards/HTTPIE001 \
  Unison-Token:595d799a-9553-4ddf-8fd9-c27b1f233ce7
```

---

_Last Updated: January 5, 2025_
_API Version: 1.0.0_
