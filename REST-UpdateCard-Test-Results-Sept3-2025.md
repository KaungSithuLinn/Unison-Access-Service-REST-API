# REST UpdateCard Test Results - September 3, 2025

## Executive Summary

**Status:** FAILED - Service is SOAP-only, not REST  
**Date:** September 3, 2025  
**Service URL:** http://192.168.10.206:9003/Unison.AccessService  
**Token:** 7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3 (Working)

## Critical Finding

**The Unison Access Service is a SOAP service, not a REST service.** All REST endpoint attempts fail because the service only supports SOAP protocol with WSDL-defined operations.

## Test Matrix Results

### Test 1: current-updatecard-post-001 (Standard POST JSON)

- **URL:** `http://192.168.10.206:9003/Unison.AccessService/UpdateCard`
- **Method:** POST
- **Headers:**
  - `Unison-Token: 7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3`
  - `Content-Type: application/json`
  - `Accept: application/json`
- **Body:**

```json
{
  "userId": "TEST_USER_001",
  "profileName": "",
  "cardNumber": "12345678",
  "systemNumber": "001",
  "versionNumber": "1",
  "miscNumber": "000",
  "cardStatus": 1
}
```

- **Result:** ❌ HTTP 400 Bad Request
- **Response:** HTML error page stating "The server encountered an error processing the request"

### Test 2: current-updatecard-put-002 (Standard PUT JSON)

- **URL:** `http://192.168.10.206:9003/Unison.AccessService/UpdateCard`
- **Method:** PUT
- **Headers:** Same as Test 1
- **Body:** Same as Test 1
- **Result:** ❌ HTTP 400 Bad Request
- **Response:** Same HTML error page

### Test 3: current-updatecard-alt-003 (Alternative API Path)

- **URL:** `http://192.168.10.206:9003/api/UpdateCard`
- **Method:** POST
- **Headers:** Same as Test 1
- **Body:** Same as Test 1
- **Result:** ❌ HTTP 404 Not Found
- **Response:** "HTTP Error 404. The requested resource is not found."

### Test 4: current-updatecard-wrapped-004 (WCF Wrapped Format)

- **URL:** `http://192.168.10.206:9003/Unison.AccessService/UpdateCard`
- **Method:** POST
- **Headers:** Same as Test 1
- **Body:**

```json
{
  "UpdateCard": {
    "userId": "TEST_USER_001",
    "profileName": "",
    "cardNumber": "12345678",
    "systemNumber": "001",
    "versionNumber": "1",
    "miscNumber": "000",
    "cardStatus": 1
  }
}
```

- **Result:** ❌ HTTP 400 Bad Request
- **Response:** Same HTML error page

## WSDL Analysis

**WSDL URL:** `http://192.168.10.206:9003/Unison.AccessService?wsdl`  
**Status:** ✅ Successfully retrieved

### Key Findings:

1. **Service Type:** SOAP Web Service (Microsoft WCF)
2. **UpdateCard Operation:** ✅ Confirmed present in WSDL
3. **Expected Format:** SOAP envelope with XML payload
4. **Action:** `http://tempuri.org/IAccessService/UpdateCard`
5. **Input Message:** `tns:IAccessService_UpdateCard_InputMessage`
6. **Output Message:** `tns:IAccessService_UpdateCard_OutputMessage`

### SOAP Operation Definition:

```xml
<wsdl:operation name="UpdateCard">
  <wsdl:input wsaw:Action="http://tempuri.org/IAccessService/UpdateCard"
              message="tns:IAccessService_UpdateCard_InputMessage"/>
  <wsdl:output wsaw:Action="http://tempuri.org/IAccessService/UpdateCardResponse"
               message="tns:IAccessService_UpdateCard_OutputMessage"/>
  <wsdl:fault wsaw:Action="http://tempuri.org/IAccessService/UpdateCardServiceErrorFault"
              name="ServiceErrorFault"
              message="tns:IAccessService_UpdateCard_ServiceErrorFault_FaultMessage"/>
  <wsdl:fault wsaw:Action="http://tempuri.org/IAccessService/UpdateCardArgumentErrorFault"
              name="ArgumentErrorFault"
              message="tns:IAccessService_UpdateCard_ArgumentErrorFault_FaultMessage"/>
</wsdl:operation>
```

## Service Help Page Attempts

- **URL:** `http://192.168.10.206:9003/Unison.AccessService/help`
- **Result:** ❌ HTTP 400 Bad Request (both with and without token)
- **Issue:** Help page not accessible via standard HTTP requests

## Error Patterns

### HTTP 400 Responses

- **Pattern:** All REST attempts to valid SOAP operations
- **Root Cause:** JSON payload sent to SOAP-only endpoint
- **Server:** Microsoft-HTTPAPI/2.0
- **Expected Format:** SOAP XML envelope

### HTTP 404 Responses

- **Pattern:** Alternative REST paths (/api/UpdateCard)
- **Root Cause:** No REST endpoints exposed by this service

## Recommendations

### Immediate Actions Required:

1. **Switch to SOAP Client:** Use SOAP protocol instead of REST
2. **Generate SOAP Request:** Create proper SOAP envelope for UpdateCard
3. **Use WSDL:** Generate client code from WSDL for proper integration
4. **Validate SOAP Format:** Test with proper SOAP Action headers

### Next Steps:

1. **SOAP Testing:** Implement proper SOAP client testing
2. **Schema Analysis:** Extract UpdateCard parameter schema from WSDL
3. **Integration Update:** Modify calling applications to use SOAP instead of REST

## Technical Specifications

### Service Details:

- **Protocol:** SOAP 1.1/1.2
- **Transport:** HTTP
- **Port:** 9003
- **Authentication:** Custom Unison-Token header
- **Server Technology:** Microsoft WCF / .NET Framework

### Required Headers for SOAP:

```http
Content-Type: text/xml; charset=utf-8
SOAPAction: "http://tempuri.org/IAccessService/UpdateCard"
Unison-Token: 7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3
```

### Expected SOAP Envelope Structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <!-- Custom headers if needed -->
  </soap:Header>
  <soap:Body>
    <UpdateCard xmlns="http://tempuri.org/">
      <!-- Parameters from WSDL schema -->
    </UpdateCard>
  </soap:Body>
</soap:Envelope>
```

## Conclusion

The REST UpdateCard endpoint testing conclusively demonstrates that **the Unison Access Service does not support REST protocol**. All attempts to use JSON payloads with HTTP verbs (POST, PUT) result in HTTP 400 errors because the service exclusively operates as a SOAP web service.

**Mission Critical:** Any integration requiring UpdateCard functionality must implement SOAP client capabilities, not REST client capabilities.
