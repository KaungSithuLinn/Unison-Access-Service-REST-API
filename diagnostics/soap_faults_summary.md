# SOAP Faults and Diagnostics Summary

**Generated:** September 4, 2025  
**Status:** HTML Error Pages Detected - WCF Configuration Required

## Current Issue Analysis

### Problem Statement

The Unison AccessService SOAP endpoint at `http://192.168.10.206:9003/Unison.AccessService` is returning HTML error pages instead of proper SOAP faults, making debugging impossible.

### Evidence Collected

#### Test Request (2025-09-04 07:41:58 GMT)

- **Endpoint:** `http://192.168.10.206:9003/Unison.AccessService`
- **Operation:** UpdateCard
- **SOAPAction:** `http://tempuri.org/IAccessService/UpdateCard`
- **Request File:** `corrected_updatecard_soap_request_v3.xml`
- **Response Status:** HTTP 400 Bad Request
- **Content-Type:** text/html (Expected: text/xml with SOAP fault)
- **Response File:** `soap_test_response_20250904.txt`

#### Response Analysis

```html
<p class="heading1">Request Error</p>
<p>
  The server encountered an error processing the request. Please see the
  <a href="http://192.168.10.206:9003/Unison.AccessService/help"
    >service help page</a
  >
  for constructing valid requests to the service.
</p>
```

**Key Finding:** This generic HTML error indicates that WCF's `includeExceptionDetailInFaults` is disabled, preventing detailed error information from being returned.

## Required Server Configuration Changes

### Primary Issue: WCF ServiceDebug Configuration

The server needs `includeExceptionDetailInFaults="true"` enabled in the WCF service configuration.

### Required Configuration Files

Based on Microsoft documentation and typical Unison installations:

1. **Primary Config:** `C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.Server.exe.config`
2. **Manager Config:** `C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.ServerManager.exe.config`
3. **Driver Config:** `C:\Program Files\Pacom Systems\Unison\Server\Drivers\WCU\Pacom.Unison.Drivers.Wcu.Driver.exe.config`

### Configuration Template

Add to `<system.serviceModel>` section:

```xml
<system.serviceModel>
  <behaviors>
    <serviceBehaviors>
      <behavior name="DebugServiceBehavior">
        <!-- WARNING: For debugging only - remove in production -->
        <serviceDebug includeExceptionDetailInFaults="true"
                      httpHelpPageEnabled="true" />
        <serviceMetadata httpGetEnabled="true" />
      </behavior>
    </serviceBehaviors>
  </behaviors>

  <services>
    <service name="Pacom.Unison.AccessService"
             behaviorConfiguration="DebugServiceBehavior">
      <!-- Existing endpoint configurations -->
    </service>
  </services>
</system.serviceModel>
```

## Next Steps Required

### For Server Administrator

1. ✅ **Backup existing config files**
2. ✅ **Apply WCF configuration changes**
3. ✅ **Restart Unison services**
4. ✅ **Test with provided SOAP requests**
5. ✅ **Enable WCF message logging (optional)**

### For Development Team

1. ✅ **Create reproducible test collection**
2. ✅ **Document expected SOAP fault format**
3. ✅ **Add integration tests for error handling**
4. ✅ **Plan production rollback strategy**

## Test Results Expected After Configuration

### Before (Current)

```
HTTP/1.1 400 Bad Request
Content-Type: text/html

<html>Request Error...</html>
```

### After (Expected)

```
HTTP/1.1 500 Internal Server Error
Content-Type: text/xml

<soap:Envelope>
  <soap:Body>
    <soap:Fault>
      <faultcode>Server</faultcode>
      <faultstring>...</faultstring>
      <detail>
        <ExceptionDetail>
          <!-- Stack trace and exception details -->
        </ExceptionDetail>
      </detail>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

## Security Considerations

⚠️ **WARNING:** `includeExceptionDetailInFaults="true"` exposes internal server details and should only be used in:

- Development environments
- Controlled debugging scenarios
- Staging environments during troubleshooting

**Production Recommendation:** Use environment-specific configuration or feature flags to control this setting.

## Files Generated

- `diagnostics/soap_faults_summary.md` (this file)
- `soap_test_response_20250904.txt` (raw HTML error response)
- `postman/Unison-UpdateCard-tests.postman_collection.json` (test collection)

## References

- [Microsoft Docs: ServiceDebugBehavior](https://learn.microsoft.com/en-us/dotnet/api/system.servicemodel.description.servicedebugbehavior)
- [WCF Service Debug Configuration](https://learn.microsoft.com/en-us/dotnet/framework/wcf/samples/service-debug-behavior)
- [WCF Fault Handling](https://learn.microsoft.com/en-us/dotnet/framework/wcf/specifying-and-handling-faults-in-contracts-and-services)
