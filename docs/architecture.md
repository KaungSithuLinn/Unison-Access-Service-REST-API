# Architecture Documentation

## Service Type Classification

The Unison Access Service has been **definitively confirmed as a SOAP 1.1 web service**, not a REST API. This classification is based on comprehensive technical validation including WSDL analysis, protocol testing, and service behavior assessment.

## Current Backend Architecture

### Unison Access Service - SOAP Backend

- **Service Type:** SOAP 1.1 WCF Service
- **Endpoint:** `http://192.168.10.206:9003/Unison.AccessService`
- **WSDL:** Available at `http://192.168.10.206:9003/Unison.AccessService?wsdl`
- **Operations:** 50+ SOAP operations for comprehensive access control management
- **Technology Stack:** Microsoft .NET Framework 4.0, ASP.NET, WCF
- **Protocol:** HTTP POST with SOAP/XML envelope
- **Server:** Microsoft-HTTPAPI/2.0 (IIS/WCF)

### Core Operations

The service provides extensive access control functionality including:

- **User Management:** `UpdateUser`, `SyncUsers`, `GetAllUsers`, `GetUserByKey`, `RemoveUser`
- **Card Management:** `UpdateCard`, `GetCardByNumber`, `SyncCards`, `RemoveCard`
- **Access Groups:** `SyncAccessGroups`, `GetAllAccessGroups`, `UpdateAccessGroup`
- **Memberships:** `SyncMemberships`, `GetMembershipsByUserKey`, `UpdateMembership`
- **Access Nodes:** `SyncAccessNodes`, `GetAllAccessNodes`, `GetFilteredAccessNodes`
- **System Operations:** `Ping`, `GetVersion`, `SyncReset`, `SyncBegin`, `SyncEnd`

## Solution Architecture - REST-to-SOAP Adapter

Since modern frontend applications require REST/JSON APIs but the backend is SOAP-only, the **REST-to-SOAP adapter pattern** is the appropriate architectural solution.

### Architecture Overview

```text
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Frontend      │    │   REST Adapter   │    │  SOAP Backend   │
│  Applications   │    │                  │    │                 │
│                 │    │                  │    │   Unison        │
│ • React         │◄──►│ • REST endpoints │◄──►│ Access Service  │
│ • Vue           │    │ • JSON handling  │    │                 │
│ • Angular       │    │ • SOAP translation│    │ • 50+ operations│
│ • Mobile apps   │    │ • Error mapping  │    │ • WCF service   │
│                 │    │                  │    │ • XML/SOAP      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Adapter Responsibilities

#### 1. Protocol Translation

- **Inbound:** Accept REST requests with JSON payloads
- **Outbound:** Generate SOAP envelopes with proper SOAPAction headers
- **Response:** Convert SOAP/XML responses to JSON format

#### 2. API Design

- **REST Endpoints:** Resource-based URLs (e.g., `/users/{id}`, `/cards/{cardNumber}`)
- **HTTP Methods:** Proper use of GET, POST, PUT, DELETE
- **Status Codes:** Standard HTTP status codes for different scenarios
- **Error Handling:** Consistent JSON error responses

#### 3. Data Transformation

- **Request Mapping:** JSON to SOAP parameter mapping
- **Response Mapping:** SOAP result to JSON conversion
- **Error Translation:** SOAP faults to HTTP status codes and JSON errors
- **Data Validation:** Input validation before SOAP calls

### Example Translation

#### REST Request (Frontend → Adapter)

```http
GET /api/users/12345
Authorization: Bearer {token}
```

#### SOAP Request (Adapter → Backend)

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetUserByKey xmlns="http://tempuri.org/">
      <userKey>12345</userKey>
    </GetUserByKey>
  </soap:Body>
</soap:Envelope>
```

#### SOAP Response (Backend → Adapter)

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetUserByKeyResponse xmlns="http://tempuri.org/">
      <GetUserByKeyResult>
        <UserId>john.doe</UserId>
        <FirstName>John</FirstName>
        <LastName>Doe</LastName>
        <Email>john.doe@company.com</Email>
      </GetUserByKeyResult>
    </GetUserByKeyResponse>
  </soap:Body>
</soap:Envelope>
```

#### REST Response (Adapter → Frontend)

```json
{
  "id": "12345",
  "userId": "john.doe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com"
}
```

## Technical Benefits

### For Frontend Developers

- **Modern API:** Standard REST/JSON interface
- **Simplified Integration:** No SOAP complexity
- **Better DX:** Standard HTTP tools and libraries work
- **Documentation:** OpenAPI/Swagger compatibility

### For Backend Integration

- **No Backend Changes:** Unison service remains unchanged
- **Preserved Functionality:** All SOAP operations accessible
- **Error Handling:** Proper SOAP fault handling maintained
- **Security:** Backend security model preserved

### For System Architecture

- **Clean Separation:** Clear boundaries between layers
- **Maintainability:** Changes isolated to adapter layer
- **Scalability:** Adapter can be scaled independently
- **Flexibility:** Multiple frontend technologies supported

## Implementation Considerations

### Adapter Technology Stack

Recommended technologies for the REST-to-SOAP adapter:

- **Node.js + Express:** For REST API framework
- **Soap.js:** For SOAP client functionality
- **TypeScript:** For type safety and better development experience
- **OpenAPI:** For API documentation and client generation

### Security Integration

- **Authentication:** JWT tokens in REST, translate to backend auth
- **Authorization:** Role-based access control mapping
- **Input Validation:** Comprehensive validation before SOAP calls
- **Error Sanitization:** Prevent SOAP fault leakage to clients

### Error Handling Strategy

| SOAP Fault Type | HTTP Status | JSON Error Response |
|------------------|-------------|-------------------|
| ArgumentError | 400 Bad Request | `{"error": "Invalid parameters", "details": "..."}` |
| ServiceError | 500 Internal Server Error | `{"error": "Service unavailable", "message": "..."}` |
| Authentication | 401 Unauthorized | `{"error": "Authentication required"}` |
| Authorization | 403 Forbidden | `{"error": "Access denied"}` |

## Conclusion

The REST-to-SOAP adapter architecture provides the optimal solution for integrating modern frontend applications with the existing SOAP-based Unison Access Service. This approach:

- **Preserves** all existing backend functionality
- **Modernizes** the API interface for frontend consumption
- **Maintains** system stability and security
- **Enables** future frontend technology adoption
- **Provides** clear separation of concerns

This architecture ensures that the project can deliver the required REST API functionality while respecting the constraints and capabilities of the existing SOAP backend infrastructure.
