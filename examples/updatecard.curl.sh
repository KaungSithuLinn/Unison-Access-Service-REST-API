#!/bin/bash
# UpdateCard API Example using curl
# This script demonstrates how to call the UpdateCard REST endpoint

# Configuration
BASE_URL="http://192.168.10.206:5001"
UNISON_TOKEN="595d799a-example-token-placeholder"

# Health check
echo "=== Health Check ==="
curl -X GET \
  "${BASE_URL}/api/health/ping" \
  -H "Content-Type: application/json" \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=== Update Card ==="

# Update card request
curl -X PUT \
  "${BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -d '{
    "cardId": "TEST123",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "department": "Engineering",
    "title": "Software Engineer",
    "isActive": true,
    "expirationDate": "2025-12-31T23:59:59Z",
    "customFields": {
      "buildingAccess": "A,B,C",
      "emergencyContact": "555-1234"
    }
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=== Test Unauthorized (without token) ==="

# Test unauthorized request
curl -X PUT \
  "${BASE_URL}/api/cards/update" \
  -H "Content-Type: application/json" \
  -d '{
    "cardId": "TEST123",
    "userName": "john.doe"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=== Get Card ==="

# Get card request
curl -X GET \
  "${BASE_URL}/api/cards/TEST123" \
  -H "Unison-Token: ${UNISON_TOKEN}" \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=== Done ==="
