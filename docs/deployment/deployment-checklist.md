# Deployment Checklist - Unison REST Adapter

## Pre-Deployment Verification

### Infrastructure Requirements ✅

- [ ] **Operating System**: Windows Server 2019/2022 or Linux with systemd
- [ ] **Runtime**: .NET 9.0 ASP.NET Core Runtime installed
- [ ] **Resources**: Minimum 2 GB RAM, 1 CPU core, 10 GB disk space available
- [ ] **Network**: Inbound ports 80/443 open, outbound access to 192.168.10.206:9003
- [ ] **SSL Certificate**: Valid certificate for HTTPS (if required)
- [ ] **Service Account**: Dedicated service account created with minimal privileges

### Application Configuration ✅

- [ ] **appsettings.Production.json**: Configured with production values
- [ ] **SOAP Endpoint**: Verified connectivity to <http://192.168.10.206:9003/Unison.AccessService>
- [ ] **Authentication**: Production tokens configured in secure storage
- [ ] **Logging**: Production logging level set (Information/Warning)
- [ ] **Health Checks**: Endpoints enabled (/health, /health/detailed)
- [ ] **HTTPS**: Enforced if SSL certificate is available
- [ ] **CORS**: Configured if cross-origin access required

### Security Validation ✅

- [ ] **Sensitive Data**: All sensitive data removed from configuration files
- [ ] **Token Security**: Authentication tokens stored securely
- [ ] **File Permissions**: Application files have appropriate permissions
- [ ] **Firewall Rules**: Configured and tested
- [ ] **Service Account**: Running with minimal required permissions
- [ ] **SSL/TLS**: Certificate properly installed and configured

### Dependencies Check ✅

- [ ] **SOAP Service**: Connectivity to Unison.AccessService verified
- [ ] **DNS Resolution**: All hostnames resolve correctly
- [ ] **Network Connectivity**: All required ports accessible
- [ ] **Load Balancer**: Configuration tested (if applicable)
- [ ] **Monitoring**: Health monitoring configured
- [ ] **Backup Strategy**: Rollback procedures documented and tested

## Deployment Execution

### Step 1: Backup Current Version ✅

- [ ] **Service Stopped**: Current service gracefully stopped
- [ ] **Files Backed Up**: Current deployment copied to backup location
- [ ] **Configuration Saved**: All configuration files backed up
- [ ] **Database State**: Current state documented (if applicable)
- [ ] **Rollback Verified**: Rollback procedure tested and documented

### Step 2: Deploy New Version ✅

- [ ] **Files Deployed**: New application files copied to deployment directory
- [ ] **Permissions Set**: File and directory permissions configured correctly
- [ ] **Configuration Applied**: Production configuration files in place
- [ ] **Dependencies Verified**: All required dependencies available
- [ ] **Service Installed**: Application installed as service/daemon

### Step 3: Service Configuration ✅

- [ ] **Service Settings**: Auto-start and failure recovery configured
- [ ] **Environment Variables**: All required environment variables set
- [ ] **Logging Directory**: Log directory created with proper permissions
- [ ] **Process Monitoring**: Service monitoring configured
- [ ] **Resource Limits**: Memory and CPU limits configured (if applicable)

## Post-Deployment Validation

### Service Health Verification ✅

```powershell
# Basic health check
Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET
```

- [ ] **Service Status**: Service running and responding
- [ ] **Health Endpoint**: /health returns 200 OK
- [ ] **Detailed Health**: /health/detailed accessible with authentication
- [ ] **Response Time**: Health checks respond within acceptable time
- [ ] **Memory Usage**: Service memory usage within expected range

### API Functionality Testing ✅

```powershell
# Authentication test
$headers = @{ "Unison-Token" = "production-token" }
Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body '{"cardId":"TEST"}' -ContentType "application/json"
```

- [ ] **Authentication**: Token validation working correctly
- [ ] **Authorization**: Unauthorized requests properly rejected
- [ ] **API Endpoints**: All documented endpoints responding
- [ ] **Error Handling**: Error responses properly formatted with correlation IDs
- [ ] **SOAP Integration**: Backend SOAP service connectivity verified

### Security Testing ✅

```powershell
# Test authentication required
try {
    Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT
} catch {
    Write-Output "✅ Authentication required: $($_.Exception.Response.StatusCode)"
}
```

- [ ] **Authentication Required**: Unauthenticated requests rejected (401)
- [ ] **Invalid Token**: Invalid tokens rejected (401)
- [ ] **HTTPS Redirect**: HTTP requests redirected to HTTPS (if configured)
- [ ] **Security Headers**: Appropriate security headers present
- [ ] **Sensitive Data**: No sensitive information in error responses

### Performance Validation ✅

```powershell
# Response time test
Measure-Command { Invoke-RestMethod -Uri "http://localhost:80/health" -Method GET }
```

- [ ] **Response Time**: Health checks < 1 second
- [ ] **API Response**: Card operations < 5 seconds
- [ ] **Memory Usage**: Service memory usage stable
- [ ] **CPU Usage**: Service CPU usage within acceptable range
- [ ] **Concurrent Requests**: Service handles expected concurrent load

### Integration Testing ✅

```powershell
# End-to-end test
$testData = @{
    cardId = "DEPLOY-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
    userName = "test.deployment"
    firstName = "Deploy"
    lastName = "Test"
    isActive = $true
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:80/api/cards/update" -Method PUT -Headers $headers -Body $testData -ContentType "application/json"
```

- [ ] **SOAP Connectivity**: Backend service reachable and responding
- [ ] **End-to-End Flow**: Complete request/response cycle working
- [ ] **Data Validation**: Input validation working correctly
- [ ] **Error Propagation**: SOAP errors properly handled and returned
- [ ] **Logging**: All operations properly logged with correlation IDs

## Rollback Procedures

### Emergency Rollback Checklist ✅

- [ ] **Issue Identified**: Critical issue documented
- [ ] **Rollback Decision**: Stakeholders notified of rollback
- [ ] **Service Stopped**: Current service gracefully stopped
- [ ] **Previous Version**: Previous version files restored
- [ ] **Configuration Restored**: Previous configuration applied
- [ ] **Service Started**: Previous version service started
- [ ] **Functionality Verified**: Previous version working correctly
- [ ] **Monitoring Restored**: All monitoring pointing to previous version

### Rollback Validation ✅

- [ ] **Health Check**: /health endpoint responding (< 30 seconds after start)
- [ ] **Authentication**: Token validation working
- [ ] **API Functionality**: Core API operations working
- [ ] **SOAP Integration**: Backend connectivity restored
- [ ] **Performance**: Response times within expected range
- [ ] **Logging**: Application logging working correctly

## Documentation Updates

### Deployment Record ✅

- [ ] **Version Information**: New version number documented
- [ ] **Deployment Date**: Date and time recorded
- [ ] **Deployed By**: Person responsible documented
- [ ] **Configuration Changes**: All configuration changes documented
- [ ] **Known Issues**: Any known issues or limitations documented
- [ ] **Rollback Tested**: Rollback procedure verified to work

### Operational Handover ✅

- [ ] **Operations Team**: Notified of successful deployment
- [ ] **Monitoring Setup**: All monitoring alerts configured
- [ ] **Support Documentation**: Updated with any changes
- [ ] **Troubleshooting Guide**: Updated if new issues possible
- [ ] **Emergency Contacts**: Contact information verified current

## Sign-off

### Technical Validation ✅

- [ ] **Deployment Engineer**: ************\_************ Date: ****\_****
- [ ] **QA Validation**: ************\_************ Date: ****\_****
- [ ] **Security Review**: ************\_************ Date: ****\_****

### Business Approval ✅

- [ ] **Product Owner**: ************\_************ Date: ****\_****
- [ ] **Operations Manager**: ************\_************ Date: ****\_****
- [ ] **Release Manager**: ************\_************ Date: ****\_****

## Post-Deployment Monitoring

### First Hour ✅

- [ ] **Service Stability**: Service running without restarts
- [ ] **Error Logs**: No critical errors in logs
- [ ] **Performance**: Response times stable
- [ ] **Resource Usage**: Memory and CPU usage normal

### First 24 Hours ✅

- [ ] **Error Rate**: Error rate within acceptable thresholds
- [ ] **Performance Trending**: No performance degradation
- [ ] **User Feedback**: No critical user-reported issues
- [ ] **System Stability**: No unexpected behavior

### First Week ✅

- [ ] **Performance Baseline**: New performance baseline established
- [ ] **Error Analysis**: All errors reviewed and categorized
- [ ] **User Adoption**: API usage patterns normal
- [ ] **Documentation Updates**: Any lessons learned documented

---

**Deployment Checklist Version**: 1.0  
**Last Updated**: September 9, 2025  
**Template Created By**: Development Team

**Notes**:

- All checkboxes must be completed before proceeding to next section
- Any failed checks must be resolved before continuing deployment
- Emergency rollback can be initiated at any point if critical issues arise
- Post-deployment monitoring is mandatory for first 24 hours
