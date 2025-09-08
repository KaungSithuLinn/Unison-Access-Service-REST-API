# WCF Message Logging Configuration Guide

**Generated:** September 4, 2025  
**Target:** Unison AccessService Server Configuration

## Purpose

This guide provides step-by-step instructions for enabling WCF message logging on the Unison AccessService to capture detailed SOAP message exchanges and server-side exceptions.

## Required Configuration Changes

### Step 1: Enable ServiceDebug Behavior

Add or modify the `<system.serviceModel>` section in the primary configuration file.

#### File Location (Primary)

`C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.Server.exe.config`

#### Configuration Template

```xml
<configuration>
  <system.serviceModel>
    <behaviors>
      <serviceBehaviors>
        <behavior name="DebugServiceBehavior">
          <!-- CRITICAL: Enable detailed SOAP faults for debugging -->
          <serviceDebug includeExceptionDetailInFaults="true"
                        httpHelpPageEnabled="true" />

          <!-- Optional: Enable metadata for WSDL access -->
          <serviceMetadata httpGetEnabled="true" />
        </behavior>
      </serviceBehaviors>
    </behaviors>

    <services>
      <service name="Pacom.Unison.AccessService"
               behaviorConfiguration="DebugServiceBehavior">
        <!-- Existing endpoint configurations remain unchanged -->
      </service>
    </services>

    <!-- Enable comprehensive message logging -->
    <diagnostics>
      <messageLogging logEntireMessage="true"
                      logMalformedMessages="true"
                      logMessagesAtServiceLevel="true"
                      logMessagesAtTransportLevel="true"
                      maxMessagesToLog="3000"
                      maxSizeOfMessageToLog="2000000" />
    </diagnostics>
  </system.serviceModel>

  <!-- Configure trace listeners for message logs -->
  <system.diagnostics>
    <trace autoflush="true" />
    <sources>
      <source name="System.ServiceModel.MessageLogging"
              switchValue="Warning, ActivityTracing">
        <listeners>
          <add type="System.Diagnostics.XmlWriterTraceListener"
               name="messages"
               initializeData="C:\Temp\Unison_MessageLogs\messages.svclog" />
        </listeners>
      </source>

      <source name="System.ServiceModel"
              switchValue="Information, ActivityTracing"
              propagateActivity="true">
        <listeners>
          <add type="System.Diagnostics.XmlWriterTraceListener"
               name="traceListener"
               initializeData="C:\Temp\Unison_MessageLogs\traces.svclog" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

### Step 2: Create Log Directory

Create the directory for log files with appropriate permissions:

```powershell
New-Item -Path "C:\Temp\Unison_MessageLogs" -ItemType Directory -Force
```

### Step 3: Alternative Configuration Files

If the primary config doesn't work, also check these files:

#### Driver Configuration

`C:\Program Files\Pacom Systems\Unison\Server\Drivers\WCU\Pacom.Unison.Drivers.Wcu.Driver.exe.config`

#### Server Manager Configuration

`C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.ServerManager.exe.config`

### Step 4: Service Restart

After configuration changes:

```powershell
# Stop the service
Stop-Service -Name "Pacom Unison Driver Service" -Force

# Wait for service to stop
Start-Sleep -Seconds 5

# Start the service
Start-Service -Name "Pacom Unison Driver Service"

# Verify service is running
Get-Service -Name "Pacom Unison Driver Service"
```

## Message Logging Levels Explained

### Service Level Logging (`logMessagesAtServiceLevel="true"`)

- **Purpose:** Captures messages as they enter/leave service operations
- **Content:** Messages are logged after WCF processing but before business logic
- **Security:** Messages are decrypted if encryption was used

### Transport Level Logging (`logMessagesAtTransportLevel="true"`)

- **Purpose:** Captures raw messages as transmitted over HTTP
- **Content:** Messages exactly as sent/received on the wire
- **Security:** Messages may be encrypted if HTTPS is used

### Malformed Message Logging (`logMalformedMessages="true"`)

- **Purpose:** Captures messages that WCF cannot parse
- **Content:** Raw message content that caused parsing failures
- **Use Case:** Debugging client request format issues

## Configuration Attributes Explained

| Attribute                        | Recommended Value | Purpose                                          |
| -------------------------------- | ----------------- | ------------------------------------------------ |
| `logEntireMessage`               | `true`            | Log complete message (headers + body)            |
| `maxMessagesToLog`               | `3000`            | Limit total messages to prevent disk overflow    |
| `maxSizeOfMessageToLog`          | `2000000`         | Max message size in bytes (2MB)                  |
| `includeExceptionDetailInFaults` | `true`            | **CRITICAL**: Return stack traces in SOAP faults |
| `httpHelpPageEnabled`            | `true`            | Enable service help page                         |

## Expected Log Files

After configuration and service restart, these files will be created:

### Message Log File

- **Location:** `C:\Temp\Unison_MessageLogs\messages.svclog`
- **Content:** Complete SOAP envelopes (request/response)
- **Format:** XML trace format viewable with Service Trace Viewer

### Trace Log File

- **Location:** `C:\Temp\Unison_MessageLogs\traces.svclog`
- **Content:** WCF processing events and errors
- **Format:** XML trace format with activity correlation

## Analyzing Log Files

### Using Service Trace Viewer

1. **Download:** Windows SDK includes `SvcTraceViewer.exe`
2. **Open:** Load `.svclog` files directly
3. **Navigate:** View message flow and exception details
4. **Search:** Filter by operation name or timestamp

### Manual Analysis

```powershell
# Search for SOAP faults in message logs
Select-String -Path "C:\Temp\Unison_MessageLogs\messages.svclog" -Pattern "soap:Fault|s:Fault"

# Search for UpdateCard operations
Select-String -Path "C:\Temp\Unison_MessageLogs\messages.svclog" -Pattern "UpdateCard"
```

## Security Considerations

⚠️ **WARNING: Production Security Risk**

### Risk Assessment

- **Data Exposure:** Log files contain complete message content including sensitive data
- **Stack Traces:** Exception details expose internal server implementation
- **Performance Impact:** Message logging increases I/O and processing overhead

### Mitigation Strategies

1. **Environment Gating:** Only enable in development/staging environments
2. **Log Rotation:** Implement automated log cleanup
3. **Access Control:** Restrict log directory permissions to authorized personnel
4. **Temporary Usage:** Disable after debugging is complete

### Recommended Production Configuration

```xml
<!-- PRODUCTION: Disable debug features -->
<serviceDebug includeExceptionDetailInFaults="false"
              httpHelpPageEnabled="false" />

<messageLogging logEntireMessage="false"
                logMalformedMessages="false"
                logMessagesAtServiceLevel="false"
                logMessagesAtTransportLevel="false" />
```

## Testing the Configuration

### Verification Steps

1. **Restart Service:** Ensure configuration is loaded
2. **Run SOAP Request:** Execute UpdateCard operation
3. **Check Response:** Should return SOAP fault with `<detail>` section
4. **Verify Logs:** Confirm `.svclog` files are created and populated

### Success Indicators

- ✅ HTTP 500 response instead of HTTP 400
- ✅ Content-Type: `text/xml` instead of `text/html`
- ✅ SOAP fault with `<ExceptionDetail>` element
- ✅ Log files contain complete message exchanges

### Failure Indicators

- ❌ Still receiving HTML error pages
- ❌ Log files not created or empty
- ❌ Service fails to start after configuration change

## Rollback Plan

### Emergency Rollback

1. **Backup:** Keep original config files as `.bak`
2. **Restore:** Copy backup over current config
3. **Restart:** Restart service to apply original settings

### Rollback Commands

```powershell
# Restore original configuration
Copy-Item -Path "C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.Server.exe.config.bak" -Destination "C:\Program Files\Pacom Systems\Unison\Server\Pacom.Unison.Server.exe.config"

# Restart service
Restart-Service -Name "Pacom Unison Driver Service"
```

## Troubleshooting

### Common Issues

#### Service Won't Start

- **Cause:** XML syntax error in configuration
- **Solution:** Validate XML syntax, check Event Viewer

#### No Log Files Created

- **Cause:** Directory permissions or path issues
- **Solution:** Verify directory exists and service account has write access

#### Still Getting HTML Errors

- **Cause:** Wrong configuration file or service not restarted
- **Solution:** Verify correct config file, restart service, check Windows services

## Next Steps After Configuration

1. **Execute Test Collection:** Run Postman collection to verify SOAP faults
2. **Collect Samples:** Gather 3-5 SOAP fault examples for analysis
3. **Document Patterns:** Identify common error types and causes
4. **Create Fix Strategy:** Plan code/configuration fixes based on fault analysis
5. **Plan Production Rollback:** Schedule disabling debug features for production

## References

- [Microsoft Docs: Configuring Message Logging](https://learn.microsoft.com/en-us/dotnet/framework/wcf/diagnostics/configuring-message-logging)
- [Microsoft Docs: Service Debug Behavior](https://learn.microsoft.com/en-us/dotnet/framework/wcf/samples/service-debug-behavior)
- [Microsoft Docs: Service Trace Viewer Tool](https://learn.microsoft.com/en-us/dotnet/framework/wcf/service-trace-viewer-tool-svctraceviewer-exe)
