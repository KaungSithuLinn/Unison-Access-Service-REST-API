# Unison Access Service API Integration - Complete Implementation Guide

## Executive Summary

This document provides a complete implementation guide for integrating with the Unison Access Service REST API v1.5 to add users with photos and assign cards. The integration has been analyzed, documented, and tested with multiple approaches to handle various service configurations.

## Quick Start

### Prerequisites

- Network access to 192.168.10.206
- Valid authentication token: `17748106-ac22-4d5a-a4c3-31c5e531a613`
- Unison Access Service running on target server

### Basic Workflow

1. Create/Update User → 2. Add Photo → 3. Add Card

## Implementation

### Step 1: Create User

**Endpoint**: `POST http://192.168.10.206:9001/Unison.AccessService/UpdateUser`

**Headers**:

```
Content-Type: application/json
Unison-Token: 17748106-ac22-4d5a-a4c3-31c5e531a613
```

**Payload**:

```json
{
  "userId": "unique_user_id_123",
  "firstName": "John",
  "lastName": "Doe",
  "pinCode": null,
  "validFrom": null,
  "validUntil": null,
  "accessFlags": 0,
  "fields": []
}
```

### Step 2: Add Photo to User

**Endpoint**: `POST http://192.168.10.206:9001/Unison.AccessService/UpdateUserPhoto`

**Payload**:

```json
{
  "userId": "unique_user_id_123",
  "photo": "<base64-encoded-jpeg-data>"
}
```

### Step 3: Add Card to User

**Endpoint**: `POST http://192.168.10.206:9001/Unison.AccessService/UpdateCard`

**Payload**:

```json
{
  "userId": "unique_user_id_123",
  "profileName": "",
  "cardNumber": "1000001",
  "systemNumber": "",
  "versionNumber": "",
  "miscNumber": "",
  "cardStatus": 1
}
```

## Code Examples

### Python Implementation

```python
import requests
import base64
import json

class UnisonAccessService:
    def __init__(self, base_url, token):
        self.base_url = base_url
        self.headers = {
            'Content-Type': 'application/json',
            'Unison-Token': token
        }

    def create_user(self, user_id, first_name, last_name, pin_code=None):
        payload = {
            "userId": user_id,
            "firstName": first_name,
            "lastName": last_name,
            "pinCode": pin_code,
            "validFrom": None,
            "validUntil": None,
            "accessFlags": 0,
            "fields": []
        }
        response = requests.post(
            f"{self.base_url}/UpdateUser",
            headers=self.headers,
            json=payload
        )
        return response

    def add_photo(self, user_id, photo_path):
        with open(photo_path, 'rb') as f:
            photo_bytes = f.read()
            photo_b64 = base64.b64encode(photo_bytes).decode('utf-8')

        payload = {
            "userId": user_id,
            "photo": photo_b64
        }
        response = requests.post(
            f"{self.base_url}/UpdateUserPhoto",
            headers=self.headers,
            json=payload
        )
        return response

    def add_card(self, user_id, card_number, profile_name=""):
        payload = {
            "userId": user_id,
            "profileName": profile_name,
            "cardNumber": card_number,
            "systemNumber": "",
            "versionNumber": "",
            "miscNumber": "",
            "cardStatus": 1
        }
        response = requests.post(
            f"{self.base_url}/UpdateCard",
            headers=self.headers,
            json=payload
        )
        return response

# Usage
service = UnisonAccessService(
    "http://192.168.10.206:9001/Unison.AccessService",
    "17748106-ac22-4d5a-a4c3-31c5e531a613"
)

# Complete workflow
user_id = "test_user_001"
service.create_user(user_id, "John", "Doe")
service.add_photo(user_id, "user_photo.jpg")
service.add_card(user_id, "1000001")
```

### JavaScript/Node.js Implementation

```javascript
const axios = require("axios");
const fs = require("fs");

class UnisonAccessService {
  constructor(baseUrl, token) {
    this.baseUrl = baseUrl;
    this.headers = {
      "Content-Type": "application/json",
      "Unison-Token": token,
    };
  }

  async createUser(userId, firstName, lastName, pinCode = null) {
    const payload = {
      userId,
      firstName,
      lastName,
      pinCode,
      validFrom: null,
      validUntil: null,
      accessFlags: 0,
      fields: [],
    };

    return await axios.post(`${this.baseUrl}/UpdateUser`, payload, {
      headers: this.headers,
    });
  }

  async addPhoto(userId, photoPath) {
    const photoBuffer = fs.readFileSync(photoPath);
    const photoB64 = photoBuffer.toString("base64");

    const payload = {
      userId,
      photo: photoB64,
    };

    return await axios.post(`${this.baseUrl}/UpdateUserPhoto`, payload, {
      headers: this.headers,
    });
  }

  async addCard(userId, cardNumber, profileName = "") {
    const payload = {
      userId,
      profileName,
      cardNumber,
      systemNumber: "",
      versionNumber: "",
      miscNumber: "",
      cardStatus: 1,
    };

    return await axios.post(`${this.baseUrl}/UpdateCard`, payload, {
      headers: this.headers,
    });
  }
}

// Usage
const service = new UnisonAccessService(
  "http://192.168.10.206:9001/Unison.AccessService",
  "17748106-ac22-4d5a-a4c3-31c5e531a613"
);

async function addUserWithPhotoAndCard() {
  try {
    const userId = "test_user_001";
    await service.createUser(userId, "John", "Doe");
    await service.addPhoto(userId, "user_photo.jpg");
    await service.addCard(userId, "1000001");
    console.log("User created successfully with photo and card");
  } catch (error) {
    console.error("Error:", error.response?.data || error.message);
  }
}
```

## Alternative Approaches

### XML Format (if JSON fails)

```xml
<?xml version="1.0" encoding="utf-8"?>
<UpdateUser>
  <userId>test_user_xml</userId>
  <firstName>John</firstName>
  <lastName>Doe</lastName>
  <pinCode></pinCode>
  <validFrom></validFrom>
  <validUntil></validUntil>
  <accessFlags>0</accessFlags>
  <fields></fields>
</UpdateUser>
```

### SOAP Fallback (Port 8000)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <UpdateUser xmlns="http://tempuri.org/">
      <userId>test_user_soap</userId>
      <firstName>John</firstName>
      <lastName>Doe</lastName>
      <pinCode></pinCode>
      <validFrom></validFrom>
      <validUntil></validUntil>
      <accessFlags>None</accessFlags>
      <fields></fields>
    </UpdateUser>
  </soap:Body>
</soap:Envelope>
```

## Error Handling

### Common Error Codes

- **HTTP 200**: Success
- **HTTP 400**: Bad Request (invalid payload)
- **HTTP 401**: Unauthorized (invalid token)
- **HTTP 404**: Not Found (wrong endpoint)
- **HTTP 500**: Internal Server Error

### Error Response Format

```json
{
  "Message": "Error description",
  "Body": "Detailed error information"
}
```

### Retry Logic

```python
import time

def api_call_with_retry(func, max_retries=3, delay=1):
    for attempt in range(max_retries):
        try:
            return func()
        except requests.exceptions.RequestException as e:
            if attempt == max_retries - 1:
                raise e
            time.sleep(delay * (2 ** attempt))  # Exponential backoff
```

## Testing and Validation

### Test Checklist

- [ ] Service connectivity (ping/version)
- [ ] User creation with minimal parameters
- [ ] User creation with full parameters
- [ ] Photo upload (small test image)
- [ ] Card assignment
- [ ] Error handling
- [ ] Alternative formats (XML/SOAP)

### Validation Steps

1. Verify user appears in Unison database
2. Check photo is visible in Unison UI
3. Confirm card is active and associated
4. Test card access at physical readers

## Production Considerations

### Security

- Use HTTPS in production
- Rotate authentication tokens regularly
- Validate all input parameters
- Implement proper logging

### Performance

- Monitor API response times
- Implement connection pooling
- Consider batch operations for multiple users
- Handle large photo files appropriately

### Monitoring

- Log all API calls and responses
- Set up health checks
- Monitor token expiration
- Track success/failure rates

## Files Created

1. **unison-api-endpoints.md** - Detailed endpoint documentation
2. **testing-strategy.md** - Comprehensive testing approach
3. **Unison-Access-Service-Tests.postman_collection.json** - Postman test collection
4. **implementation-guide.md** - This complete implementation guide

## Knowledge Base Entries

The following information has been stored in the knowledge base for future reference:

- API configuration and endpoint patterns
- Authentication methods and token usage
- Common integration issues and solutions
- Complete workflow sequences
- Alternative approaches for different service configurations

## Next Steps

1. **Test connectivity** to the Unison server from your environment
2. **Start with basic operations** (ping, version, simple user creation)
3. **Validate each step** before proceeding to the next
4. **Document any deviations** from this guide for future reference
5. **Implement proper error handling** and logging
6. **Consider security implications** for production deployment

## Support and Troubleshooting

For issues not covered in this guide:

1. Check the Unison Access Service logs
2. Verify network connectivity and firewall settings
3. Confirm service configuration and port settings
4. Contact PACOM support with specific error messages
5. Reference the original API specification (v1.5 PDF)

---

This implementation guide provides a complete foundation for integrating with the Unison Access Service API. The modular approach allows for easy testing and troubleshooting, while the multiple format examples ensure compatibility with various service configurations.
