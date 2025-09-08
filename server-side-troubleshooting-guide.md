# Unison Access Service REST API - Server-Side Troubleshooting Guide

## Overview

This guide provides comprehensive server-side troubleshooting steps for the Unison Access Service REST API on the 206 server, based on analysis of the Unison 5.13.0 User Guide and Microsoft WCF best practices.

## Target Environment

- **Server**: 206 (192.168.10.206)
- **Application**: Pacom Unison Client
- **REST API Endpoint**: http://192.168.10.206:9001/Unison.AccessService
- **Admin Credentials**: admin/Arrow1234@

## Structured Troubleshooting Phases

### Phase 1: Initial Access and Authentication

**Objective**: Establish admin access to Unison system

#### Steps:

1. **Server Access**

   - [ ] Access 206 server via RDP or physical console
   - [ ] Verify Windows login successful
   - [ ] Confirm network connectivity

2. **Unison Client Login**
   - [ ] Launch Pacom Unison Client application
   - [ ] Login with credentials: admin/Arrow1234@
   - [ ] Verify successful authentication
   - [ ] Confirm admin privileges and access level

#### Expected Outcomes:

- Successful admin login to Unison Client
- Full administrative privileges available
- No authentication error messages

#### Troubleshooting:

- **If login fails**: Check operator groups, roles, and authentication modes
- **If access limited**: Verify admin operator permissions in System Management

---

### Phase 2: Service Status Assessment

**Objective**: Verify WCF and Access Service configurations

#### Steps:

1. **Navigate to Function Drivers**

   - [ ] Go to System ribbon > System Configuration
   - [ ] Select "Unison Function Drivers" from explorer
   - [ ] Locate WCF Service entry
   - [ ] Locate Access Service entry

2. **WCF Service Analysis**

   - [ ] Check WCF Service status (running/stopped/error)
   - [ ] Review configuration settings
   - [ ] Note any error messages or alerts
   - [ ] Verify HTTP endpoint configuration

3. **Access Service Analysis**

   - [ ] Check Access Service status
   - [ ] Review HTTP/HTTPS settings
   - [ ] Verify port 9001 configuration
   - [ ] Check network connection settings

4. **System Service Manager**
   - [ ] Open Unison Service Manager utility
   - [ ] Check Unison Windows service status
   - [ ] Review device driver status
   - [ ] Check for any service failures

#### Expected Outcomes:

- WCF Service showing "running" status
- Access Service showing "running" status
- Port 9001 properly configured for REST API
- No service errors or failures

#### Troubleshooting:

- **Services not running**: Restart through Service Manager
- **Configuration errors**: Review HTTP endpoint settings
- **Port conflicts**: Check Windows netstat for port 9001 usage

---

### Phase 3: Debug Setup and Monitoring

**Objective**: Enable comprehensive error tracking

#### Steps:

1. **Enable Debug Logging**

   - [ ] In WCF Service properties, enable "Debug Protocol" option
   - [ ] In Access Service properties, enable debug logging
   - [ ] Save configuration changes

2. **Launch Debug Monitor**

   - [ ] Open Unison Debug Monitor application
   - [ ] Configure to monitor WCF and Access services
   - [ ] Watch for red text errors indicating issues

3. **Protocol Error Logging**
   - [ ] Enable protocol error logging for communications troubleshooting
   - [ ] Configure log file retention settings

#### Expected Outcomes:

- Debug logging active for both services
- Debug Monitor showing real-time service activity
- No red error messages in debug output

#### Troubleshooting:

- **Debug Monitor not starting**: Check Windows service dependencies
- **No debug output**: Verify logging settings are saved and active

---

### Phase 4: Configuration Validation

**Objective**: Verify all system components are properly configured

#### Steps:

1. **Database Connectivity**

   - [ ] Navigate to System > Database settings
   - [ ] Check main database connection status
   - [ ] Verify log database connection
   - [ ] Test archive database connection
   - [ ] Review SQL Server connection strings

2. **Network Configuration**

   - [ ] Verify REST API endpoint shows http://192.168.10.206:9001/Unison.AccessService
   - [ ] Check network interface bindings
   - [ ] Test internal connectivity to port 9001
   - [ ] Verify firewall exceptions

3. **License Validation**

   - [ ] Check license information through About dialog
   - [ ] Verify license covers REST API functionality
   - [ ] Check for any grace period warnings
   - [ ] Validate license expiration dates

4. **User and Access Control**
   - [ ] Review user management configuration
   - [ ] Check access control settings that might affect API
   - [ ] Verify access schedules and permissions

#### Expected Outcomes:

- All database connections active and healthy
- REST API endpoint properly configured on port 9001
- Valid license covering all required functionality
- No access control restrictions blocking API operations

#### Troubleshooting:

- **Database errors**: Check SQL Server status and connection strings
- **Network issues**: Use netstat, telnet, or ping to test connectivity
- **License issues**: Contact Pacom support for license renewal
- **Access control problems**: Review operator groups and permissions

---

### Phase 5: Testing and Validation

**Objective**: Confirm REST API functionality

#### Steps:

1. **Internal API Testing**

   - [ ] Test REST API endpoint from 206 server localhost
   - [ ] Use curl or PowerShell to test HTTP connectivity
   - [ ] Verify WSDL accessibility at endpoint?wsdl
   - [ ] Test basic API operations if possible

2. **Service Response Testing**

   - [ ] Monitor debug output during API calls
   - [ ] Check for proper HTTP response codes
   - [ ] Verify service is responding to requests
   - [ ] Look for any timeout or connection issues

3. **Log Analysis**
   - [ ] Review transaction logs for API-related entries
   - [ ] Check system logs for errors or warnings
   - [ ] Analyze debug monitor output for patterns
   - [ ] Document any error codes or messages

#### Expected Outcomes:

- REST API endpoint responds to HTTP requests
- WSDL accessible and properly formatted
- Debug monitor shows successful request processing
- No error patterns in logs

#### Troubleshooting:

- **API not responding**: Check service status and restart if needed
- **HTTP errors**: Review WCF configuration and error messages
- **Timeout issues**: Check database performance and connectivity

---

## Common Issues and Solutions

### Issue: HTTP Error 404.3 - Not Found

**Cause**: Extension configuration problems
**Solution**:

- Check IIS configuration if applicable
- Verify WCF service registration
- Use WFServicesReg.exe tool with /c switch

### Issue: Port 9001 Not Accessible

**Cause**: Port conflicts or firewall blocking
**Solution**:

- Check Windows netstat for port usage
- Add firewall exception for port 9001
- Verify network interface bindings

### Issue: Service Authentication Failures

**Cause**: Operator permissions or AD integration issues
**Solution**:

- Review operator groups and roles
- Check Active Directory integration settings
- Verify authentication mode configuration

### Issue: Database Connection Errors

**Cause**: SQL Server issues or connection string problems
**Solution**:

- Verify SQL Server service is running
- Test connection strings manually
- Check database permissions and access

### Issue: License Validation Errors

**Cause**: Expired or insufficient license
**Solution**:

- Contact Pacom support for license renewal
- Check grace period status
- Verify license covers REST API functionality

---

## Documentation Template

### Service Status Summary

```
Date/Time: _______________
Operator: ________________

WCF Service Status: [Running/Stopped/Error] _______________
Access Service Status: [Running/Stopped/Error] _______________
Unison Windows Service: [Running/Stopped/Error] _______________
Database Connectivity: [OK/Error] _______________
REST API Endpoint: [Accessible/Not Accessible] _______________
```

### Error Log Template

```
Service: _______________
Error Code: _______________
Error Message: _______________
Timestamp: _______________
Debug Output: _______________
Resolution Attempted: _______________
Outcome: _______________
```

### Configuration Checklist

```
[ ] WCF Service configuration verified
[ ] Access Service configuration verified
[ ] Database connections tested
[ ] Network connectivity confirmed
[ ] License validation completed
[ ] Debug logging enabled
[ ] Firewall exceptions verified
[ ] User permissions checked
```

---

## Escalation Criteria

Escalate to Pacom Support if:

- [ ] License issues cannot be resolved locally
- [ ] WCF Service configuration errors persist after troubleshooting
- [ ] Database connectivity issues require vendor assistance
- [ ] Multiple service failures indicate system-level problems

Escalate to IT/Network Team if:

- [ ] Network connectivity issues beyond local server
- [ ] Firewall configuration requires enterprise changes
- [ ] DNS or routing problems affecting REST API access
- [ ] Security policy conflicts with service requirements

---

## Next Steps Template

Based on troubleshooting results:

1. **If All Services Working**:

   - Document successful configuration
   - Test REST API functionality thoroughly
   - Implement monitoring for ongoing issues

2. **If Partial Issues Found**:

   - Address specific service problems
   - Retest after each fix
   - Document changes made

3. **If Major Issues Found**:
   - Escalate according to criteria above
   - Prepare detailed issue summary
   - Include all error logs and configuration details

---

## References

- Unison 5.13.0 User Guide - System Management section
- Microsoft WCF Troubleshooting Documentation
- Pacom Support Portal: support.pacom.com
