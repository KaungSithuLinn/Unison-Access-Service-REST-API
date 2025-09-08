# Server-Side Action Execution Checklist

## Pre-Execution Preparation

- [ ] Verify RDP/console access to 206 server available
- [ ] Confirm admin credentials: admin/Arrow1234@
- [ ] Backup current Unison configuration (if possible)
- [ ] Prepare documentation template for findings

## Phase 1: Access and Authentication

### Server Access

- [ ] **1.1** Access 206 server via RDP/console
- [ ] **1.2** Verify Windows authentication successful
- [ ] **1.3** Check network connectivity to server

### Unison Client Authentication

- [ ] **1.4** Launch Pacom Unison Client application
- [ ] **1.5** Enter credentials: admin/Arrow1234@
- [ ] **1.6** Verify successful login
- [ ] **1.7** Confirm admin privileges available

**Status**: ☐ Complete ☐ Issues Found ☐ Blocked

**Notes/Issues**:

---

## Phase 2: Service Status Assessment

### Navigate to Function Drivers

- [ ] **2.1** Go to System ribbon
- [ ] **2.2** Select System Configuration
- [ ] **2.3** Navigate to Unison Function Drivers
- [ ] **2.4** Locate WCF Service entry

### WCF Service Check

- [ ] **2.5** Check WCF Service status (Running/Stopped/Error)
- [ ] **2.6** Review configuration properties
- [ ] **2.7** Note any error messages
- [ ] **2.8** Verify HTTP endpoint settings

### Access Service Check

- [ ] **2.9** Locate Access Service entry
- [ ] **2.10** Check service status
- [ ] **2.11** Review HTTP/HTTPS configuration
- [ ] **2.12** Verify port 9001 settings

### Service Manager Check

- [ ] **2.13** Open Unison Service Manager utility
- [ ] **2.14** Check Unison Windows service status
- [ ] **2.15** Review device driver status
- [ ] **2.16** Check for service failures/errors

**Status**: ☐ Complete ☐ Issues Found ☐ Blocked

**Current Service Status**:

- WCF Service: ******\_\_\_\_******
- Access Service: ******\_\_\_\_******
- Windows Service: ******\_\_\_\_******

**Issues Found**:

---

## Phase 3: Debug Setup and Monitoring

### Enable Debug Logging

- [ ] **3.1** Access WCF Service properties
- [ ] **3.2** Enable "Debug Protocol" option
- [ ] **3.3** Save WCF Service configuration
- [ ] **3.4** Access Access Service properties
- [ ] **3.5** Enable debug logging
- [ ] **3.6** Save Access Service configuration

### Launch Debug Monitor

- [ ] **3.7** Open Unison Debug Monitor application
- [ ] **3.8** Configure monitoring for WCF/Access services
- [ ] **3.9** Monitor for red text errors
- [ ] **3.10** Enable protocol error logging

**Status**: ☐ Complete ☐ Issues Found ☐ Blocked

**Debug Output Summary**:

---

## Phase 4: Configuration Validation

### Database Connectivity

- [ ] **4.1** Navigate to System > Database settings
- [ ] **4.2** Check main database connection
- [ ] **4.3** Verify log database connection
- [ ] **4.4** Test archive database connection
- [ ] **4.5** Review SQL connection strings

### Network Configuration

- [ ] **4.6** Verify REST API endpoint configuration
- [ ] **4.7** Check port 9001 binding
- [ ] **4.8** Test internal connectivity to port 9001
- [ ] **4.9** Verify firewall exceptions

### License Validation

- [ ] **4.10** Access About dialog for license info
- [ ] **4.11** Check license covers REST API
- [ ] **4.12** Verify no grace period warnings
- [ ] **4.13** Check license expiration

### Access Control Check

- [ ] **4.14** Review user management settings
- [ ] **4.15** Check access control affecting API
- [ ] **4.16** Verify access schedules/permissions

**Status**: ☐ Complete ☐ Issues Found ☐ Blocked

**Configuration Summary**:

- Database Status: ******\_\_\_\_******
- REST Endpoint: ******\_\_\_\_******
- License Status: ******\_\_\_\_******
- Access Control: ******\_\_\_\_******

## Phase 5: Testing and Validation

### Internal API Testing

- [ ] **5.1** Test localhost REST API access
- [ ] **5.2** Use curl/PowerShell for HTTP test
- [ ] **5.3** Check WSDL accessibility (?wsdl)
- [ ] **5.4** Test basic API operations

### Service Response Testing

- [ ] **5.5** Monitor debug output during calls
- [ ] **5.6** Check HTTP response codes
- [ ] **5.7** Verify service responding properly
- [ ] **5.8** Look for timeout/connection issues

### Log Analysis

- [ ] **5.9** Review transaction logs for API entries
- [ ] **5.10** Check system logs for errors
- [ ] **5.11** Analyze debug patterns
- [ ] **5.12** Document error codes/messages

**Status**: ☐ Complete ☐ Issues Found ☐ Blocked

**API Test Results**:

- HTTP Response: ******\_\_\_\_******
- WSDL Access: ******\_\_\_\_******
- Service Response: ******\_\_\_\_******

## Summary and Next Steps

### Issues Discovered

**Priority 1 (Critical)**:

---

**Priority 2 (Important)**:

---

**Priority 3 (Low)**:

---

### Actions Taken

---

### Recommended Next Steps

---

### Escalation Required

☐ Pacom Support (License/Config issues)
☐ IT/Network Team (Infrastructure issues)  
☐ Database Administrator (SQL Server issues)
☐ No escalation needed

### Final Status

☐ **RESOLVED** - REST API fully functional
☐ **PARTIAL** - Some issues resolved, testing continues  
☐ **ESCALATED** - Requires vendor/specialist support
☐ **BLOCKED** - Cannot proceed without external help

**Completion Date/Time**: ******\_\_\_\_******
**Executed By**: ******\_\_\_\_******
**Next Review Date**: ******\_\_\_\_******
