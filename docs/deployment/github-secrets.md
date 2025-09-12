# GitHub Secrets Configuration

This document provides instructions for setting up the required GitHub secrets for the Unison Access Service deployment workflows.

## Required Secrets

### Azure Service Principal Credentials

Create separate service principals for each environment to maintain security isolation.

#### For Development Environment

```bash
# Create service principal for development
az ad sp create-for-rbac --name "unison-access-service-dev-sp" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/unison-access-service-dev-rg \
  --sdk-auth
```

Set the following GitHub secret:

- **Name**: `AZURE_CREDENTIALS_DEV`
- **Value**: JSON output from the above command

#### For Staging Environment

```bash
# Create service principal for staging
az ad sp create-for-rbac --name "unison-access-service-staging-sp" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/unison-access-service-staging-rg \
  --sdk-auth
```

Set the following GitHub secret:

- **Name**: `AZURE_CREDENTIALS_STAGING`
- **Value**: JSON output from the above command

#### For Production Environment

```bash
# Create service principal for production
az ad sp create-for-rbac --name "unison-access-service-prod-sp" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/unison-access-service-prod-rg \
  --sdk-auth
```

Set the following GitHub secret:

- **Name**: `AZURE_CREDENTIALS_PROD`
- **Value**: JSON output from the above command

### Terraform State Storage Secrets

#### Development Environment

- **Name**: `TF_STATE_RG_DEV`
- **Value**: `unison-terraform-state-dev-rg`

- **Name**: `TF_STATE_SA_DEV`
- **Value**: `unisonterraformstatedev`

#### Staging Environment

- **Name**: `TF_STATE_RG_STAGING`
- **Value**: `unison-terraform-state-staging-rg`

- **Name**: `TF_STATE_SA_STAGING`
- **Value**: `unisonterraformstatestaging`

#### Production Environment

- **Name**: `TF_STATE_RG_PROD`
- **Value**: `unison-terraform-state-prod-rg`

- **Name**: `TF_STATE_SA_PROD`
- **Value**: `unisonterraformstateprod`

### Additional Secrets

#### Azure Subscription ID

- **Name**: `AZURE_SUBSCRIPTION_ID`
- **Value**: Your Azure subscription ID

#### Alert Action Group ID

- **Name**: `ALERT_ACTION_GROUP_ID`
- **Value**: Resource ID of your Azure Monitor Action Group

## Setting Up GitHub Secrets

### Via GitHub Web Interface

1. Navigate to your repository on GitHub
2. Go to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name and value
5. Click **Add secret**

### Via GitHub CLI

```bash
# Set Azure credentials for development
gh secret set AZURE_CREDENTIALS_DEV --body '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'

# Set Azure credentials for staging
gh secret set AZURE_CREDENTIALS_STAGING --body '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'

# Set Azure credentials for production
gh secret set AZURE_CREDENTIALS_PROD --body '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'

# Set Terraform state secrets
gh secret set TF_STATE_RG_DEV --body "unison-terraform-state-dev-rg"
gh secret set TF_STATE_SA_DEV --body "unisonterraformstatedev"
gh secret set TF_STATE_RG_STAGING --body "unison-terraform-state-staging-rg"
gh secret set TF_STATE_SA_STAGING --body "unisonterraformstatestaging"
gh secret set TF_STATE_RG_PROD --body "unison-terraform-state-prod-rg"
gh secret set TF_STATE_SA_PROD --body "unisonterraformstateprod"

# Set additional secrets
gh secret set AZURE_SUBSCRIPTION_ID --body "your-subscription-id"
gh secret set ALERT_ACTION_GROUP_ID --body "/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/microsoft.insights/actionGroups/{action-group-name}"
```

## Environment Configuration

### GitHub Environment Protection Rules

Set up environment protection rules for sensitive environments:

#### Staging Environment

1. Go to **Settings** → **Environments**
2. Click **New environment** and name it `staging`
3. Add protection rules:
   - **Required reviewers**: Select team members who can approve staging deployments
   - **Wait timer**: Optional delay before deployment
   - **Deployment branches**: Restrict to `main` branch only

#### Production Environment

1. Go to **Settings** → **Environments**
2. Click **New environment** and name it `production`
3. Add protection rules:
   - **Required reviewers**: Select senior team members who can approve production deployments
   - **Wait timer**: 5-10 minutes to allow for review
   - **Deployment branches**: Restrict to `main` branch only

## Service Principal Permissions

Ensure your service principals have the following minimum permissions:

### Required Azure RBAC Roles

- **Contributor** on the resource group
- **Storage Blob Data Contributor** on the Terraform state storage account
- **Azure Container Registry Push** on the container registry

### Additional Permissions for Production

- **Monitoring Contributor** for creating alerts
- **Application Insights Component Contributor** for telemetry

## Security Best Practices

1. **Rotate secrets regularly** - Update service principal credentials quarterly
2. **Use least privilege** - Grant minimum required permissions
3. **Separate environments** - Use different service principals for each environment
4. **Monitor access** - Enable logging for service principal activities
5. **Review permissions** - Audit permissions monthly

## Troubleshooting

### Common Issues

#### Authentication Failed

- Verify the service principal credentials are correct
- Check that the service principal has the required permissions
- Ensure the subscription ID is correct

#### Terraform State Access Denied

- Verify the storage account name and resource group are correct
- Check that the service principal has Storage Blob Data Contributor role
- Ensure the storage account allows access from GitHub Actions

#### Container Registry Access Denied

- Verify the registry name in the workflow
- Check that the service principal has push permissions
- Ensure the registry allows access from GitHub Actions

### Verification Commands

```bash
# Test Azure login
az login --service-principal --username {client-id} --password {client-secret} --tenant {tenant-id}

# Test storage access
az storage blob list --account-name {storage-account} --container-name tfstate

# Test container registry access
az acr login --name {registry-name}
```

## Maintenance

### Monthly Tasks

- Review and audit all secrets
- Check service principal expiration dates
- Verify environment protection rules
- Test backup and recovery procedures

### Quarterly Tasks

- Rotate service principal credentials
- Update secrets in GitHub
- Review and update permissions
- Conduct security assessment
