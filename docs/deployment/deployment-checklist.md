# Deployment Checklist - Unison Access Service REST API

## Pre-Deployment Checklist

### Code Quality and Security ✅

- [x] All pull requests reviewed and merged
- [x] Security validation completed
- [x] Code quality analysis passed
- [x] Vulnerability scanning completed
- [x] Dependencies security scan passed

### Infrastructure Setup ✅

- [x] Terraform configuration created
- [x] Azure-verified modules selected
- [x] Multi-environment configuration ready
- [x] Backend state storage configured
- [x] Network security configured

### CI/CD Pipeline Setup ✅

- [x] GitHub Actions workflows created
- [x] Infrastructure deployment pipeline ready
- [x] Application deployment pipeline ready
- [x] Security scanning integrated
- [x] Environment protection rules configured

### Configuration Management 📋

#### Required GitHub Secrets

**Azure Service Principal Credentials**

- [ ] `AZURE_CREDENTIALS_DEV` - Development environment service principal
- [ ] `AZURE_CREDENTIALS_STAGING` - Staging environment service principal
- [ ] `AZURE_CREDENTIALS_PROD` - Production environment service principal

**Terraform State Storage**

- [ ] `TF_STATE_RG_DEV` - Development state resource group
- [ ] `TF_STATE_SA_DEV` - Development state storage account
- [ ] `TF_STATE_RG_STAGING` - Staging state resource group
- [ ] `TF_STATE_SA_STAGING` - Staging state storage account
- [ ] `TF_STATE_RG_PROD` - Production state resource group
- [ ] `TF_STATE_SA_PROD` - Production state storage account

**Additional Configuration**

- [ ] `AZURE_SUBSCRIPTION_ID` - Azure subscription identifier
- [ ] `ALERT_ACTION_GROUP_ID` - Monitoring alert action group

#### Azure Prerequisites

**Resource Groups**

- [ ] Create `unison-terraform-state-dev-rg`
- [ ] Create `unison-terraform-state-staging-rg`
- [ ] Create `unison-terraform-state-prod-rg`

**Storage Accounts for Terraform State**

- [ ] Create `unisonterraformstatedev` in dev resource group
- [ ] Create `unisonterraformstatestaging` in staging resource group
- [ ] Create `unisonterraformstateprod` in production resource group

**Container for State Files**

- [ ] Create `tfstate` container in each storage account
- [ ] Configure appropriate access permissions

## Deployment Process

### Phase 1: Infrastructure Deployment (Development) 🚀

1. **Validate Terraform Configuration**

   ```bash
   cd infrastructure
   terraform fmt -check
   terraform validate
   ```

2. **Initialize Development Environment**

   ```bash
   terraform init \
     -backend-config="resource_group_name=unison-terraform-state-dev-rg" \
     -backend-config="storage_account_name=unisonterraformstatedev" \
     -backend-config="container_name=tfstate" \
     -backend-config="key=unison-access-service/dev/terraform.tfstate"
   ```

3. **Plan Development Deployment**

   ```bash
   terraform plan -var-file="environments/development.tfvars"
   ```

4. **Deploy Development Infrastructure**

   ```bash
   terraform apply -var-file="environments/development.tfvars"
   ```

5. **Verify Development Deployment**
   - [ ] Resource group created successfully
   - [ ] App Service deployed and running
   - [ ] Key Vault accessible
   - [ ] Application Insights configured
   - [ ] Container Registry available

### Phase 2: Application Deployment (Development) 📦

1. **Build and Push Container Image**

   - GitHub Actions will automatically build and push on main branch push

2. **Deploy Application**

   - GitHub Actions will automatically deploy to development environment

3. **Verify Application Deployment**
   - [ ] Health endpoint responding (200 OK)
   - [ ] Swagger UI accessible
   - [ ] API endpoints functional
   - [ ] Telemetry data flowing to Application Insights

### Phase 3: Staging Environment 🔄

1. **Deploy Infrastructure to Staging**

   - Use GitHub Actions workflow with staging environment selection

2. **Deploy Application to Staging**

   - Automatic deployment after successful development deployment

3. **Staging Validation**
   - [ ] Performance tests passing
   - [ ] Integration tests successful
   - [ ] User acceptance testing completed

### Phase 4: Production Deployment 🎯

1. **Final Pre-Production Checks**

   - [ ] All staging tests passed
   - [ ] Backup procedures verified
   - [ ] Rollback plan prepared
   - [ ] Monitoring alerts configured

2. **Deploy Production Infrastructure**

   - Use GitHub Actions workflow with production environment selection
   - Requires manual approval

3. **Deploy Production Application**

   - Use GitHub Actions workflow with production environment selection
   - Requires manual approval

4. **Post-Production Validation**
   - [ ] Health checks passing
   - [ ] Performance within acceptable limits
   - [ ] Monitoring alerts configured
   - [ ] Backup verification completed

## Post-Deployment Tasks

### Monitoring Setup 📊

- [ ] Configure Application Insights alerts
- [ ] Set up availability tests
- [ ] Configure log analytics queries
- [ ] Set up dashboard for monitoring

### Security Hardening 🔒

- [ ] Review and update Key Vault access policies
- [ ] Configure network security groups
- [ ] Enable diagnostic logging
- [ ] Set up security center recommendations

### Documentation 📚

- [ ] Update API documentation
- [ ] Document configuration settings
- [ ] Create operational runbooks
- [ ] Update disaster recovery procedures

### Team Communication 📢

- [ ] Notify stakeholders of successful deployment
- [ ] Share production URLs and documentation
- [ ] Schedule post-deployment review meeting
- [ ] Update project status

## Troubleshooting Guide

### Common Issues

#### Terraform Deployment Fails

1. Check Azure credentials and permissions
2. Verify subscription quotas
3. Check resource naming conflicts
4. Review error logs in GitHub Actions

#### Application Deployment Fails

1. Check container image build logs
2. Verify Azure Web App configuration
3. Check Key Vault access permissions
4. Review application logs

#### Health Check Fails

1. Check application startup logs
2. Verify environment variables
3. Check database connectivity
4. Verify SOAP service connectivity

### Emergency Procedures

#### Rollback Application

```bash
# Use GitHub Actions rollback workflow
# Or manual rollback via Azure CLI
az webapp deployment slot swap \
  --name unison-access-service-prod \
  --resource-group unison-access-service-prod-rg \
  --slot production \
  --target-slot staging
```

#### Emergency Stop

```bash
# Stop the web app
az webapp stop \
  --name unison-access-service-prod \
  --resource-group unison-access-service-prod-rg
```

## Success Criteria ✅

### Development Environment

- [ ] Infrastructure deployed successfully
- [ ] Application running and healthy
- [ ] All API endpoints functional
- [ ] Monitoring configured

### Staging Environment

- [ ] Infrastructure deployed successfully
- [ ] Application running and healthy
- [ ] Performance tests passing
- [ ] Integration tests successful

### Production Environment

- [ ] Infrastructure deployed successfully
- [ ] Application running and healthy
- [ ] All health checks passing
- [ ] Monitoring and alerting active
- [ ] Backup and recovery verified

## Sign-off

### Technical Team

- [ ] **DevOps Engineer**: ************\_\_\_************ Date: ****\_\_\_****
- [ ] **Backend Developer**: **********\_\_\_\_********** Date: ****\_\_\_****
- [ ] **Security Engineer**: **********\_\_\_\_********** Date: ****\_\_\_****

### Business Team

- [ ] **Product Owner**: ************\_\_\_\_************ Date: ****\_\_\_****
- [ ] **Project Manager**: ************\_\_************ Date: ****\_\_\_****

## Notes

- Document any deviations from this checklist
- Record deployment times and any issues encountered
- Update this checklist based on lessons learned
