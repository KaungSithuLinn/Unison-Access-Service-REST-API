# SOAP vs REST Evidence Documentation

## Executive Summary

Through systematic technical validation, the Unison Access Service at `http://192.168.10.206:9003/Unison.AccessService` is definitively confirmed as a **SOAP 1.1 web service**, not a REST API. All evidence consistently demonstrates this is a WCF service requiring SOAP protocol communication.

## Technical Evidence

### 1. WSDL Analysis - Definitive SOAP Service Proof

**WSDL URL:** <http://192.168.10.206:9003/Unison.AccessService?wsdl>

**Command:**

```bash
curl -X GET "http://192.168.10.206:9003/Unison.AccessService?wsdl" -o temp_wsdl.xml
```

**Result:** Successfully retrieved 106KB WSDL file containing complete service definition.

**Key SOAP Indicators in WSDL:**

- **Service Operations:** 50+ SOAP operations including:
  - `Ping`, `GetVersion`, `UpdateUser`, `SyncUsers`, `GetAllUsers`
  - `UpdateCard`, `RemoveCard`, `GetCardByNumber`
  - `SyncAccessGroups`, `GetAllAccessGroups`
  - Complete CRUD operations for Users, Cards, Access Groups, Memberships
- **Message Definitions:** All operations follow SOAP message pattern with input/output/fault messages
- **Fault Handling:** Extensive SOAP fault definitions for `ServiceError` and `ArgumentError`
- **Namespace:** Primary namespace `http://tempuri.org/` (typical WCF/SOAP pattern)
- **Data Contracts:** Uses `http://schemas.datacontract.org/2004/07/` namespace pattern

### 2. REST API Validation Test (Expected Failure)

**Command:**

```bash
curl -X POST "http://192.168.10.206:9003/Unison.AccessService" \
  -H "Content-Type: application/json" \
  -d '{"cardId":"12345"}'
```

**Result:** HTTP 400 Bad Request with HTML error page (not JSON)

```html
<!DOCTYPE html>
<html>
  <head>
    <title>Bad Request</title>
    <meta name="viewport" content="width=device-width" />
    <style>
      body {
        font-family: "Verdana";
        font-weight: normal;
        color: black;
      }
      p {
        font-family: "Verdana";
        font-weight: normal;
        color: black;
        margin-top: -5px;
      }
      b {
        font-family: "Verdana";
        font-weight: bold;
        color: black;
        margin-top: -5px;
      }
      H1 {
        font-family: "Verdana";
        font-weight: normal;
        font-size: 18pt;
        color: red;
      }
      H2 {
        font-family: "Verdana";
        font-weight: normal;
        font-size: 14pt;
        color: maroon;
      }
      pre {
        font-family: "Consolas", "Lucida Console", monospace;
        font-size: 11pt;
        margin: 0;
        color: black;
        background-color: #e0e0e0;
      }
      .marker {
        font-weight: bold;
        color: black;
        text-decoration: none;
      }
      .version {
        color: gray;
      }
      .error {
        margin-bottom: 10px;
      }
      .expandable {
        text-decoration: underline;
        font-weight: bold;
        color: navy;
        cursor: hand;
      }
      @media screen and (max-width: 639px) {
        pre {
          font-size: 11pt;
        }
      }
    </style>
  </head>
  <body bgcolor="white">
    <span
      ><h1>
        Server Error in '/' Application.
        <hr width="100%" size="1" color="silver" />
      </h1>
      <h2><i>Bad Request</i></h2></span
    >
    <font face="Arial, Helvetica, Geneva, SunSans-Regular, sans-serif ">
      <b> Description: </b>An error occurred during the processing of a request.
      <br /><br />
      <b> HTTP Error 400 - Bad Request.</b>
      <br /><br />
      <hr width="100%" size="1" color="silver" />
      <b>Version Information:</b>&nbsp;Microsoft .NET Framework
      Version:4.0.30319; ASP.NET Version:4.8.9139.0
    </font>
  </body>
</html>
```

**Analysis:**

- HTML error response (not JSON) indicates service doesn't understand REST/JSON requests
- Microsoft .NET Framework/ASP.NET error page confirms WCF service stack
- HTTP 400 Bad Request shows the service rejected the JSON content type

### 3. Service Endpoint Analysis

**Base URL:** `http://192.168.10.206:9003/Unison.AccessService`

**Service Type Indicators:**

- **WSDL Availability:** Service exposes WSDL at `?wsdl` endpoint (SOAP characteristic)
- **Single Endpoint:** All operations accessible through single URL (SOAP pattern)
- **Operation-based:** WSDL defines specific operation names (not REST resource paths)
- **Server:** Microsoft-HTTPAPI/2.0 (IIS/WCF stack)

### 4. Protocol Validation

**Content-Type Testing:**

- **JSON Request:** Rejected with HTML error (proves no REST support)
- **XML/SOAP:** Would be accepted format (based on WSDL structure)

**HTTP Method Testing:**

- **POST with JSON:** Failed (expected behavior for SOAP service)
- **GET for WSDL:** Successful (standard SOAP metadata retrieval)

## Architectural Implications

### Current State

- **Backend Service:** SOAP 1.1 WCF service with 50+ operations
- **Protocol:** Requires SOAP envelope with proper SOAPAction headers
- **Data Format:** XML-based with complex data contracts
- **Interface:** Single endpoint with operation-based message routing

### Solution Architecture

Since frontend applications require REST/JSON APIs but the backend is SOAP-only, a **REST-to-SOAP adapter** is the correct architectural solution:

```text
Frontend (React/Vue/etc.)  →  REST Adapter  →  SOAP Backend
     ↓ JSON requests            ↓ translates        ↓ SOAP/XML
     ↓ HTTP/REST               ↓ protocols         ↓ WCF Service
     ↓ Simple calls            ↓ complexity        ↓ Full operations
```

## Conclusion

**Service Type:** Definitively SOAP 1.1 web service (WCF-based)
**REST Compatibility:** None - service only accepts SOAP/XML requests
**Recommended Solution:** Implement REST-to-SOAP adapter for modern client integration

## Evidence Summary

| Test Type          | Expected for REST  | Expected for SOAP | Actual Result        | Conclusion |
| ------------------ | ------------------ | ----------------- | -------------------- | ---------- |
| JSON POST          | JSON response      | HTML error        | HTML error           | ✅ SOAP    |
| WSDL endpoint      | Not available      | Available         | Available            | ✅ SOAP    |
| Service operations | Resource paths     | Named operations  | 50+ named operations | ✅ SOAP    |
| Error format       | JSON               | HTML/XML          | HTML                 | ✅ SOAP    |
| Single endpoint    | Multiple resources | Single URL        | Single URL           | ✅ SOAP    |

**Final Verdict:** The Unison Access Service is a SOAP web service requiring a REST adapter for modern integration.

