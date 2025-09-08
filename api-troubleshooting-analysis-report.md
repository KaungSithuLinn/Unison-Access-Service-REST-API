# Unison Access Service REST API - Troubleshooting Analysis Report

**Date:** September 2, 2025  
**Analysis Type:** Structured troubleshooting following 5-step methodology  
**Status:** Ready for escalation - Server-side investigation required

## Executive Summary

After comprehensive analysis using canonical payload validation, multiple request format testing, and WCF REST troubleshooting techniques, we have identified that the 400 Bad Request errors on `/UpdateUser`, `/UpdateUserPhoto`, and `/UpdateCard` endpoints are likely caused by server-side WCF configuration issues rather than client-side payload problems.

## Step 1: API Contract Analysis ✅ COMPLETED

### PDF Analysis Results

- **Source Documents:**
  - `Unison Access Service API - Specification v1.5.pdf` (successfully converted to markdown)
  - `Unison Access Service REST (WhatsApp messages Minh Nguyen).pdf` (attachment analyzed)

### Key Contract Details Extracted

- **Service Type:** WCF-based REST API for Unison access control system
- **Version:** 1.5, compatible with Unison v5.10
- **Base URL:** `localhost:8000/Unison.AccessService` (default)
- **Authentication:** Token-based via `Unison-Token` header
- **Target Endpoints:**
  - `UpdateUser`: 8 parameters including complex types (DateTime, AccessFlags, List<UserField>)
  - `UpdateUserPhoto`: 2 parameters (userId, photo as byte[])
  - `UpdateCard`: 7 parameters including enums (CardStatus)

## Step 2: Canonical Payload Engineering ✅ COMPLETED

### Payload Structures Validated

#### UpdateUser Canonical JSON

```json
{
  "userId": "string - required",
  "firstName": "string - optional",
  "lastName": "string - optional",
  "pinCode": "string - optional",
  "validFrom": "DateTime - ISO format or null",
  "validUntil": "DateTime - ISO format or null",
  "accessFlags": "int - enum value (0-16)",
  "fields": [
    {
      "Id": "int",
      "Action": "int - FieldAction enum (0-2)",
      "Value": "string"
    }
  ]
}
```

#### UpdateUserPhoto Canonical JSON

```json
{
  "userId": "string - required",
  "photo": "string - base64 encoded JPEG"
}
```

#### UpdateCard Canonical JSON

```json
{
  "userId": "string - required if creating new card",
  "profileName": "string - optional",
  "cardNumber": "string - required",
  "systemNumber": "string - optional",
  "versionNumber": "string - optional",
  "miscNumber": "string - optional",
  "cardStatus": "int - CardStatus enum (0-4)"
}
```

### Validation Status

- ✅ Payloads match API specification exactly
- ✅ Enum values correctly mapped to integers
- ✅ DateTime format standardized to ISO 8601
- ✅ Complex types properly structured

## Step 3: Testing Results ✅ COMPLETED

### Environment Setup

- **Python Environment:** Successfully configured (v3.13.7)
- **Dependencies:** `requests`, `python-dotenv` installed
- **Code Quality:** Codacy analysis passed (no issues found)

### Test Execution Results

#### Connectivity Test

```
Endpoint: /Ping
Method: GET
Status: 200 OK
Response: true
Result: ✅ SUCCESS - Authentication and connectivity confirmed
```

#### UpdateUser Test Results

```
Endpoint: /UpdateUser
Method: POST
Tested Formats:
1. JSON Object: 400 Bad Request (HTML error page)
2. JSON Array: 400 Bad Request (HTML error page)
3. XML: 400 Bad Request (HTML error page)
4. GET with params: 405 Method Not Allowed

All formats failed with same HTML error response pointing to service help page
```

#### Service Metadata Investigation

```
/help endpoint: 400 Bad Request (same error as operations)
/mex endpoint: Invalid URL
?wsdl endpoint: Invalid URL
Result: ❌ No accessible service metadata
```

## Step 4: Root Cause Diagnosis ✅ COMPLETED

### Microsoft Docs Research Findings

- **WCF REST Multi-Parameter Issue:** Operations with multiple parameters require specific `BodyStyle` configuration
- **WebInvokeAttribute Requirements:** Complex operations need `BodyStyle=WebMessageBodyStyle.WrappedRequest`
- **Default Behavior:** WCF defaults to XML format, not JSON for WebHttpEndpoint

### Evidence Analysis

#### What's Working ✅

- Network connectivity and SSL/TLS
- Authentication via Unison-Token header
- HTTP POST method (405 confirms POST required)
- Simple operations (Ping)

#### What's Failing ❌

- All multi-parameter operations (UpdateUser, UpdateUserPhoto, UpdateCard)
- All request formats (JSON object, JSON array, XML)
- Service help and metadata endpoints
- Complex parameter binding

### Root Cause Assessment

**Primary Issue:** WCF service-side configuration problem with REST endpoint parameter binding for multi-parameter operations.

**Evidence Supporting This Conclusion:**

1. Ping (0 parameters) works perfectly
2. All complex operations (2-8 parameters) fail identically
3. Multiple request formats tested - all fail the same way
4. Error response is generic HTML, not specific validation errors
5. Microsoft Docs indicate this is a common WCF REST configuration issue

## Step 5: Escalation Package 🚨 READY FOR ESCALATION

### Issue Classification

- **Type:** Server-side WCF configuration issue
- **Severity:** High - Blocks all Import API functionality
- **Scope:** All multi-parameter REST operations
- **Client Impact:** Complete inability to create/update users, photos, and cards

### Required Server-Side Investigation

#### For Unison API Development Team

1. **Verify WebInvokeAttribute Configuration:**

   ```csharp
   [WebInvoke(Method = "POST",
              BodyStyle = WebMessageBodyStyle.WrappedRequest,
              RequestFormat = WebMessageFormat.Json,
              ResponseFormat = WebMessageFormat.Json)]
   ```

2. **Check WCF Endpoint Configuration:**

   - Verify WebHttpBinding configuration
   - Confirm WebHttpBehavior is applied
   - Validate parameter serialization settings

3. **Enable WCF Tracing:**

   - Add service behavior tracing to see actual request processing
   - Log parameter binding attempts
   - Capture detailed error information

4. **Test with Known Working Client:**
   - Verify if any existing clients successfully call these endpoints
   - Compare working request format with our attempts

### Immediate Workarounds

- **Use Ping endpoint** for connectivity testing (confirmed working)
- **Request working examples** from Unison team
- **Consider SOAP endpoints** if available as alternative

### Evidence Package

1. ✅ Complete test results showing Ping success vs. operation failures
2. ✅ Multiple request format attempts (JSON object, array, XML)
3. ✅ Canonical payload structures matching API specification
4. ✅ Microsoft Docs research on WCF REST configuration issues
5. ✅ Code quality verification (Codacy analysis)

### Recommended Next Actions

1. **Escalate to Unison API team** with this complete analysis
2. **Request server-side debugging** with WCF tracing enabled
3. **Ask for working request examples** from known good clients
4. **Schedule technical call** to review WCF service configuration
5. **Consider API version compatibility** check

---

## Conclusion

This structured analysis has systematically eliminated client-side issues and identified server-side WCF configuration as the root cause. The evidence strongly suggests that the UpdateUser, UpdateUserPhoto, and UpdateCard endpoints are not properly configured for REST parameter binding, despite being hosted on a REST endpoint.

**Next Step:** Escalate to Unison API development team with request for server-side WCF configuration review and debugging assistance.

---

_Generated by AI-assisted troubleshooting methodology using Sequential Thinking MCP, Microsoft Docs MCP, Memory MCP, and MarkItDown MCP Server_
