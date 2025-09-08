# Unison Access Service REST API Endpoints

## Base Configuration

- **Base URL**: `http://192.168.10.206:9001/Unison.AccessService`
- **Authentication**: Header `Unison-Token: 17748106-ac22-4d5a-a4c3-31c5e531a613`
- **Content-Type**: `application/json`

## Endpoint Analysis

### 1. Add User with Photo Workflow

#### Step 1: Create/Update User

- **Method**: POST
- **URL**: `http://192.168.10.206:9001/Unison.AccessService/UpdateUser`
- **Headers**:
  ```
  Content-Type: application/json
  Unison-Token: 17748106-ac22-4d5a-a4c3-31c5e531a613
  ```
- **Payload**:
  ```json
  {
    "userId": "testuser_20250826_001",
    "firstName": "John",
    "lastName": "Doe",
    "pinCode": null,
    "validFrom": null,
    "validUntil": null,
    "accessFlags": 0,
    "fields": []
  }
  ```

#### Step 2: Add Photo to User

- **Method**: POST
- **URL**: `http://192.168.10.206:9001/Unison.AccessService/UpdateUserPhoto`
- **Headers**:
  ```
  Content-Type: application/json
  Unison-Token: 17748106-ac22-4d5a-a4c3-31c5e531a613
  ```
- **Payload**:
  ```json
  {
    "userId": "testuser_20250826_001",
    "photo": "<base64-encoded-jpeg-data>"
  }
  ```

### 2. Add Card to User

#### Step 3: Add Card

- **Method**: POST
- **URL**: `http://192.168.10.206:9001/Unison.AccessService/UpdateCard`
- **Headers**:
  ```
  Content-Type: application/json
  Unison-Token: 17748106-ac22-4d5a-a4c3-31c5e531a613
  ```
- **Payload**:
  ```json
  {
    "userId": "testuser_20250826_001",
    "profileName": "",
    "cardNumber": "1000001",
    "systemNumber": "",
    "versionNumber": "",
    "miscNumber": "",
    "cardStatus": 1
  }
  ```

## Data Types Reference

### AccessFlags Enum

- 0: None
- 1: AllowArm
- 2: AllowDisarm
- 4: ExtendedTimes
- 8: CommandControl
- 16: NoChange

### CardStatus Enum

- 0: NoChange
- 1: Active
- 2: Blocked
- 3: Lost
- 4: Canceled

## Alternative Endpoint Patterns

If the above REST pattern doesn't work, try these alternatives:

### WCF REST with XML

- **Content-Type**: `application/xml` or `text/xml`
- **Payload Format**: XML instead of JSON

### WCF SOAP Endpoints (Port 8000)

- **Base URL**: `http://192.168.10.206:8000/Unison.AccessService`
- **Content-Type**: `text/xml; charset=utf-8`
- **SOAPAction Header**: Required for SOAP calls

### Query Parameter Style

- **URL**: `http://192.168.10.206:9001/Unison.AccessService/UpdateUser?userId=test&firstName=John&lastName=Doe`

## Error Handling

Expected error responses:

- Authentication errors: Check token validity
- Argument errors: Validate required parameters
- Service errors: Check service availability

## Testing Notes

1. Start with simple UpdateUser call
2. Verify user creation before adding photo
3. Test with minimal required parameters first
4. Add photo as base64 encoded JPEG
5. Finally add card to created user
