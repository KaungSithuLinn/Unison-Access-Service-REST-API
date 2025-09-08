# Unison AccessService SOAP Fault Test Results

**Test Date:** September 5, 2025  
**Test Type:** SOAP Fault Behavior Validation  
**Objective:** Verify AccessService returns SOAP faults instead of HTML error pages

---

## Test Configuration

**Endpoint:** `http://192.168.10.206:9003/Unison.AccessService`  
**Test Method:** POST with invalid SOAP request to trigger error  
**Expected Behavior:** Return XML SOAP fault with exception details

### Test Request

```http
POST /Unison.AccessService HTTP/1.1
Host: 192.168.10.206:9003
Content-Type: text/xml; charset=utf-8
SOAPAction: http://tempuri.org/IAccessService/UpdateCard

<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:tem="http://tempuri.org/">
    <soapenv:Body>
        <tem:UpdateCard>
            <tem:request>invalid_request_data</tem:request>
        </tem:UpdateCard>
    </soapenv:Body>
</soapenv:Envelope>
```

---

## Test Results: BEFORE FIX (Current State)

### Response Headers

```http
HTTP/1.1 400 Bad Request
Content-Length: 1786
Content-Type: text/html
Server: Microsoft-HTTPAPI/2.0
Date: Fri, 05 Sep 2025 06:10:42 GMT
```

### Response Body

```html
<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
  <head>
    <title>Request Error</title>
    <style>
      /* CSS styling for error page */
    </style>
  </head>
  <body>
    <div id="content">
      <p class="heading1">Request Error</p>
      <p xmlns="">
        The server encountered an error processing the request. Please see the
        <a
          rel="help-page"
          href="http://192.168.10.206:9003/Unison.AccessService/help"
        >
          service help page</a
        >
        for constructing valid requests to the service.
      </p>
    </div>
  </body>
</html>
```

### Analysis: ❌ FAILED

- **Content-Type:** `text/html` (Should be `text/xml`)
- **Response Format:** HTML error page
- **Information Level:** Generic error message only
- **SOAP Client Impact:** Cannot parse as SOAP fault
- **Debugging Value:** LOW - No technical details provided

---

## Expected Results: AFTER FIX (Target State)

### Expected Response Headers

```http
HTTP/1.1 500 Internal Server Error
Content-Type: text/xml; charset=utf-8
Server: Microsoft-HTTPAPI/2.0
```

### Expected Response Body Format

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <soap:Code>
        <soap:Value>soap:Receiver</soap:Value>
      </soap:Code>
      <soap:Reason>
        <soap:Text xml:lang="en">Exception details...</soap:Text>
      </soap:Reason>
      <soap:Detail>
        <ExceptionDetail xmlns="http://schemas.datacontract.org/2004/07/System.ServiceModel">
          <HelpLink></HelpLink>
          <InnerException></InnerException>
          <Message>Detailed error message</Message>
          <StackTrace>Stack trace information</StackTrace>
          <Type>System.Exception</Type>
        </ExceptionDetail>
      </soap:Detail>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

### Expected Analysis: ✅ SUCCESS

- **Content-Type:** `text/xml; charset=utf-8`
- **Response Format:** Valid SOAP fault
- **Information Level:** Detailed exception information
- **SOAP Client Impact:** Can parse and handle as proper SOAP fault
- **Debugging Value:** HIGH - Stack traces, exception details available

---

## WSDL Accessibility Test

### Test Command

```bash
curl "http://192.168.10.206:9003/Unison.AccessService?wsdl"
```

### Result: ✅ SUCCESS

- **Status:** WSDL fully accessible
- **Service Operations:** All expected operations available
  - Ping, GetVersion, UpdateUser, UpdateCard, etc.
- **Metadata Exchange:** Working properly
- **Service Status:** RUNNING and FUNCTIONAL

---

## Configuration Validation

### Current WCF Config (Pacom.Unison.Server.exe.config)

```xml
<system.serviceModel>
  <behaviors>
    <serviceBehaviors>
      <behavior name="NIM.DPM.DPMControlBehavior">
        <serviceMetadata httpGetEnabled="false" />
        <!-- ✅ CRITICAL: This setting enables SOAP faults -->
        <serviceDebug includeExceptionDetailInFaults="true" />
      </behavior>
    </serviceBehaviors>
  </behaviors>
</system.serviceModel>
```

### Status: ✅ CONFIGURATION CORRECT

- WCF serviceDebug setting is properly configured
- includeExceptionDetailInFaults="true" is present
- Configuration matches Microsoft documentation requirements

---

## Root Cause Analysis

### Issue: Configuration Not Applied to Running Service

1. **Configuration File:** ✅ Correct settings present
2. **Service State:** ❌ Running with old/cached configuration
3. **Required Action:** Service restart to pick up configuration changes

### Supporting Evidence

1. **WSDL Access:** Works (service is running)
2. **Error Response:** HTML format (old behavior)
3. **Config File:** Contains correct WCF settings
4. **Conclusion:** Service needs restart to apply config changes

---

## Remediation Steps

### 1. Reserve HTTP.SYS URL ACL (Admin Required)

```bash
netsh http add urlacl url=http://+:9003/ user="NT AUTHORITY\NETWORK SERVICE"
```

### 2. Restart Service

```powershell
# Find service name
Get-Service | Where-Object {$_.DisplayName -like "*Unison*" -or $_.DisplayName -like "*Pacom*"}

# Restart service
Restart-Service -Name "Pacom Unison Driver Service"
```

### 3. Validation Test

Run the same SOAP fault test and verify:

- Response Content-Type is `text/xml`
- Response body contains SOAP fault elements
- No HTML error page returned

---

## Success Criteria

### Primary Indicators

- [ ] Response Content-Type: `text/xml; charset=utf-8`
- [ ] Response body contains `<soap:Fault>` elements
- [ ] Exception details visible in SOAP fault
- [ ] No HTML error page returned

### Secondary Indicators

- [ ] WSDL still accessible after restart
- [ ] Service responds to valid requests
- [ ] Debug logging captures detailed fault information

---

## Risk Assessment

### Risk Level: LOW

- **Change Type:** Configuration application only
- **Reversibility:** HIGH - Can revert by changing config and restarting
- **Downtime:** Minimal - Service restart only
- **Impact:** Positive - Improved debugging capability

### Mitigation

- Backup current working configuration
- Test in development environment first
- Monitor service logs during restart
- Validate both error and success scenarios

---

**Test Executed By:** Unison Access Service Technical Team  
**Environment:** Development/Testing Environment  
**Test Status:** ISSUE CONFIRMED - READY FOR REMEDIATION
