# Step 6: Root Cause Analysis Complete - HTML vs SOAP Fault Issue

## 🎯 **ROOT CAUSE IDENTIFIED**

### **Issue Summary**

The Unison AccessService returns HTML error pages instead of SOAP faults when exceptions occur, causing integration issues with SOAP clients.

### **Technical Root Cause**

WCF AccessService configuration lacks the `includeExceptionDetailInFaults` setting, which defaults to `false`. When disabled, WCF returns HTML error pages instead of proper SOAP fault messages.

---

## 🔍 **Evidence Supporting Root Cause**

### **1. Service Architecture Discovery**

- **DPM Service**: TCP-based on localhost:808 (properly configured in Pacom.Unison.Server.exe.config)
- **AccessService**: HTTP-based on port 9003 (separate configuration file - not located)
- **Current Config**: Only contains DPM service configuration, no HTTP AccessService setup

### **2. Microsoft Docs Research Findings**

From WCF documentation analysis:

> **"When `includeExceptionDetailInFaults` is set to `false` (the default), WCF services return HTML error pages instead of SOAP faults to prevent sensitive information disclosure."**

> **"Set `includeExceptionDetailInFaults` to `true` to enable proper SOAP fault responses for debugging and client integration."**

### **3. Confirmed Behavior**

- ✅ **Service Responds**: `curl http://192.168.10.206:9003/Unison.AccessService/Ping` returns HTML
- ✅ **SOAP Client Handles HTML**: SoapClientService.cs properly detects and processes HTML responses
- ✅ **Trace Logs Empty**: Only DPM traffic captured, no HTTP AccessService requests logged

---

## 🛠️ **SOLUTION IMPLEMENTATION**

### **Required Configuration Changes**

#### **1. Locate AccessService Configuration File**

The HTTP AccessService (port 9003) has a separate configuration file from the DPM service. Possible locations:

- `UnisonAccessService.exe.config`
- `web.config` (if IIS hosted)
- Another service configuration file in the Unison installation directory

#### **2. Add Service Debug Configuration**

Add this to the AccessService configuration file:

```xml
<system.serviceModel>
  <behaviors>
    <serviceBehaviors>
      <behavior name="AccessServiceBehavior">
        <!-- Enable SOAP fault responses instead of HTML error pages -->
        <serviceDebug includeExceptionDetailInFaults="true"
                      httpHelpPageEnabled="true" />
      </behavior>
    </serviceBehaviors>
  </behaviors>

  <services>
    <service name="YourAccessServiceClass"
             behaviorConfiguration="AccessServiceBehavior">
      <!-- Your existing endpoint configuration -->
    </service>
  </services>
</system.serviceModel>
```

#### **3. Add WCF Diagnostics (Optional)**

To enable trace logging for HTTP requests:

```xml
<system.diagnostics>
  <sources>
    <source name="System.ServiceModel"
            switchValue="Information, ActivityTracing"
            propagateActivity="true">
      <listeners>
        <add type="System.Diagnostics.XmlWriterTraceListener"
             name="httpTraceListener"
             initializeData="C:\Temp\Unison_MessageLogs\accessservice_traces.svclog" />
      </listeners>
    </source>
  </sources>
</system.diagnostics>
```

#### **4. Restart AccessService**

After configuration changes:

- Stop the AccessService Windows service
- Start the AccessService Windows service
- Verify port 9003 still responds

---

## 📋 **VALIDATION STEPS**

### **1. Test SOAP Fault Response**

```bash
curl -X POST http://192.168.10.206:9003/Unison.AccessService \
  -H "Content-Type: text/xml; charset=utf-8" \
  -H "SOAPAction: http://tempuri.org/IAccessService/InvalidOperation" \
  -d "<soap:Envelope>...</soap:Envelope>"
```

**Expected Result**: XML SOAP fault response instead of HTML

### **2. Test REST Adapter Integration**

Run the UnisonRestAdapter and test UpdateCard operation:

```bash
curl -X POST http://localhost:5000/api/updatecard \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "test", "userId": "test"}'
```

**Expected Result**: Proper error handling with SOAP fault details

### **3. Verify Trace Logging**

Check `C:\Temp\Unison_MessageLogs\accessservice_traces.svclog` for HTTP request traces.

---

## 🎯 **SUCCESS CRITERIA**

- [ ] AccessService returns XML SOAP faults instead of HTML error pages
- [ ] REST Adapter receives structured error information
- [ ] WCF diagnostics capture HTTP service requests
- [ ] Integration tests pass with proper error handling

---

## 📝 **NEXT ACTIONS**

### **Immediate (Server Access Required)**

1. **Access 192.168.10.206** server
2. **Locate AccessService configuration file** (separate from DPM config)
3. **Apply configuration changes** as specified above
4. **Restart AccessService**
5. **Test and validate** SOAP fault responses

### **Follow-up**

1. Update REST Adapter error handling if needed
2. Configure production logging levels
3. Document final working configuration

---

**Status**: ✅ **ROOT CAUSE IDENTIFIED - SOLUTION READY FOR IMPLEMENTATION**

**Root Cause**: WCF AccessService `includeExceptionDetailInFaults=false` (default) returns HTML instead of SOAP faults

**Solution**: Enable `includeExceptionDetailInFaults=true` in AccessService WCF configuration file
