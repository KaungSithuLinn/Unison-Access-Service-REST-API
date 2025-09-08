# REST UpdateCard Endpoint Troubleshooting Report - September 3, 2025

**Mission Phase:** Step 1-2 of Sequential MCP Pipeline  
**Objective:** Systematic REST UpdateCard troubleshooting using multiple HTTP methods, headers, and payload formats  
**Status:** 🔄 IN PROGRESS

## Executive Summary

This report documents systematic REST API troubleshooting for the Unison Access Service UpdateCard endpoint, addressing previous HTTP 400/405 errors through comprehensive testing variations.

## Background Context

### Previous Analysis Summary

- **Issue:** UpdateCard operations failing with HTTP 400 Bad Request and HTTP 405 Method Not Allowed
- **Previous Testing:** Conducted on port 9001 (inaccessible service)
- **Current Status:** Service confirmed operational on port 9003
- **Authentication Token:** Updated to working token: `7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3`

### Key Troubleshooting Insights from Research

#### Web Search Findings

- **HTTP 405 Method Not Allowed:** Occurs when the HTTP method is not supported by the server endpoint
- **HTTP 400 Bad Request:** Indicates malformed syntax or invalid request structure
- **Common Causes:** Content-Type header mismatches, incorrect payload format, unsupported HTTP verbs

#### Microsoft Docs Research

- **WCF REST Configuration:** Multi-parameter operations require specific `BodyStyle` configuration
- **WebInvokeAttribute Requirements:** Complex operations need `BodyStyle=WebMessageBodyStyle.WrappedRequest`
- **Parameter Binding:** WCF defaults to XML format, not JSON for WebHttpEndpoint

#### CoreWCF Documentation Analysis

- **REST vs SOAP:** CoreWCF primarily designed for SOAP services with limited REST support
- **Service Configuration:** REST endpoints may require specific binding and behavior configuration
- **Authentication:** Token-based authentication confirmed working for simple operations

## Testing Strategy

### REST API Troubleshooting Matrix

| Test Variant | HTTP Method | URL Path                           | Payload Format | Content-Type     | Purpose                |
| ------------ | ----------- | ---------------------------------- | -------------- | ---------------- | ---------------------- |
| **Test 1**   | POST        | `/Unison.AccessService/UpdateCard` | Standard JSON  | application/json | Standard REST approach |
| **Test 2**   | PUT         | `/Unison.AccessService/UpdateCard` | Standard JSON  | application/json | REST update semantics  |
| **Test 3**   | POST        | `/api/UpdateCard`                  | Standard JSON  | application/json | Alternative API path   |
| **Test 4**   | POST        | `/Unison.AccessService/UpdateCard` | WCF Wrapped    | application/json | WCF-specific format    |

### Payload Variations Tested

#### Standard JSON Format

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

#### WCF Wrapped Request Format

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

## Test Execution Results

### Environment Configuration

- **Service Endpoint:** http://192.168.10.206:9003/Unison.AccessService
- **Authentication Token:** 7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3
- **Testing Tool:** Postman MCP (Collection ID: 7c3025de-0b0b-42fb-9eaf-86354426a5df)
- **Test Date:** September 3, 2025

### Connectivity Verification

**Test:** Basic service connectivity on working port 9003  
**Method:** GET /Unison.AccessService  
**Status:** ⏳ PENDING EXECUTION

### REST UpdateCard Test Results

#### Test 1: POST Standard JSON Format

**Endpoint:** `POST http://192.168.10.206:9003/Unison.AccessService/UpdateCard`  
**Headers:**

- Unison-Token: 7d8ef85c-0e8c-4b8c-afcf-76fe28c480a3
- Content-Type: application/json
- Accept: application/json

**Status:** ⏳ PENDING EXECUTION

#### Test 2: PUT Standard JSON Format

**Endpoint:** `PUT http://192.168.10.206:9003/Unison.AccessService/UpdateCard`  
**Headers:** Same as Test 1  
**Status:** ⏳ PENDING EXECUTION

#### Test 3: POST Alternative Path

**Endpoint:** `POST http://192.168.10.206:9003/api/UpdateCard`  
**Headers:** Same as Test 1  
**Status:** ⏳ PENDING EXECUTION

#### Test 4: POST WCF Wrapped Format

**Endpoint:** `POST http://192.168.10.206:9003/Unison.AccessService/UpdateCard`  
**Headers:** Same as Test 1  
**Payload:** WCF wrapped request format  
**Status:** ⏳ PENDING EXECUTION

## Troubleshooting Methodology Applied

### 1. Research-Based Analysis ✅ COMPLETED

- Web search for HTTP 400/405 troubleshooting guidance
- Microsoft Docs research on REST API error codes
- CoreWCF documentation analysis for REST/SOAP hybrid services

### 2. Systematic Testing Approach ⏳ IN PROGRESS

- Updated service configuration (port 9003, current token)
- Multiple HTTP method testing (POST, PUT)
- Alternative URL path exploration
- WCF-specific request format testing

### 3. Documentation Standards

- Comprehensive test result logging
- Error response analysis and categorization
- Recommended next steps based on findings

## Key Hypotheses Under Investigation

### Hypothesis 1: HTTP Method Compatibility

**Theory:** UpdateCard endpoint may only support specific HTTP methods  
**Testing:** POST vs PUT method comparison  
**Evidence Needed:** Method-specific error responses

### Hypothesis 2: URL Path Configuration

**Theory:** UpdateCard may be available at alternative paths  
**Testing:** `/Unison.AccessService/UpdateCard` vs `/api/UpdateCard`  
**Evidence Needed:** Path-specific 404 vs other errors

### Hypothesis 3: WCF Request Format Requirements

**Theory:** WCF services may require wrapped request format  
**Testing:** Standard JSON vs WCF wrapped format  
**Evidence Needed:** Format-specific validation errors

### Hypothesis 4: Service Configuration Issues

**Theory:** UpdateCard endpoint may not be properly configured for REST  
**Testing:** Comparison with known working endpoints  
**Evidence Needed:** Consistent 405/400 patterns vs working operations

## Next Steps

### Immediate Actions

1. **Execute Test Matrix:** Run all 4 test variations systematically
2. **Document Results:** Capture exact error responses and status codes
3. **Analyze Patterns:** Identify consistent failure modes vs partial successes
4. **Generate Recommendations:** Based on test outcome patterns

### Escalation Criteria

- **All tests fail with 405:** HTTP method not supported - server configuration issue
- **All tests fail with 400:** Request format issue - payload or header problem
- **Mixed results:** Endpoint partially configured - specific format requirements
- **All tests succeed:** Issue resolved - document working configuration

## Memory Integration

This testing session builds upon previous troubleshooting analysis and integrates findings into the project memory system for future reference and team handover.

---

_Generated using Sequential Thinking MCP, Postman MCP, Web Search for Copilot extension, Microsoft Docs MCP, and Context7 MCP_

**Report Status:** 🔄 LIVE DOCUMENT - Updated during testing execution  
**Next Update:** Upon completion of test execution matrix
