# Deployment scripts for Unison Access Service REST API infrastructure

## Prerequisites

Before running these scripts, ensure you have:

- Azure CLI installed and configured (`az login`)
- Terraform installed (version ~> 1.9)
- Appropriate Azure permissions for resource creation

## Quick Start

### 1. Development Environment

```bash
# Navigate to infrastructure directory
cd infrastructure

# Initialize Terraform
terraform init

# Plan deployment
terraform plan -var-file="environments/development.tfvars"

# Deploy infrastructure
terraform apply -var-file="environments/development.tfvars"
```

### 2. Staging Environment

```bash
# Initialize with staging backend (if using remote state)
terraform init -backend-config="key=unison-access-service/staging/terraform.tfstate"

# Plan deployment
terraform plan -var-file="environments/staging.tfvars"

# Deploy infrastructure
terraform apply -var-file="environments/staging.tfvars"
```

### 3. Production Environment

```bash
# Initialize with production backend (if using remote state)
terraform init -backend-config="key=unison-access-service/production/terraform.tfstate"

# Plan deployment
terraform plan -var-file="environments/production.tfvars"

# Deploy infrastructure
terraform apply -var-file="environments/production.tfvars"
```

## PowerShell Deployment Scripts

### Development Deployment

```powershell
# Set execution policy if needed
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Navigate to infrastructure directory
Set-Location -Path "infrastructure"

# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Plan deployment
terraform plan -var-file="environments/development.tfvars" -out="tfplan"

# Apply deployment
terraform apply "tfplan"

# Show outputs
terraform output
```

### Production Deployment

```powershell
# Navigate to infrastructure directory
Set-Location -Path "infrastructure"

# Initialize with production backend
terraform init -backend-config="key=unison-access-service/production/terraform.tfstate"

# Validate configuration
terraform validate

# Plan deployment
terraform plan -var-file="environments/production.tfvars" -out="tfplan-prod"

# Review plan carefully for production
Write-Host "Please review the plan carefully before proceeding with production deployment" -ForegroundColor Yellow
Read-Host "Press Enter to continue or Ctrl+C to cancel"

# Apply deployment
terraform apply "tfplan-prod"

# Show outputs
terraform output
```

## Environment Variables

Set these environment variables for authentication:

```powershell
# Option 1: Service Principal Authentication
$env:ARM_CLIENT_ID = "your-service-principal-id"
$env:ARM_CLIENT_SECRET = "your-service-principal-secret"
$env:ARM_SUBSCRIPTION_ID = "your-subscription-id"
$env:ARM_TENANT_ID = "your-tenant-id"

# Option 2: Use Azure CLI authentication (default)
az login
```

## Backend State Storage Setup

Create Azure Storage for remote state (run once per environment):

```powershell
# Variables
$resourceGroupName = "rg-terraform-state-dev"
$storageAccountName = "saterraformstatedev001"
$containerName = "tfstate"
$location = "East US"

# Create resource group
az group create --name $resourceGroupName --location $location

# Create storage account
az storage account create `
  --resource-group $resourceGroupName `
  --name $storageAccountName `
  --sku Standard_LRS `
  --encryption-services blob

# Create container
az storage container create `
  --name $containerName `
  --account-name $storageAccountName
```

## Post-Deployment Configuration

After successful deployment:

1. **Configure Application Settings in Key Vault:**

```powershell
# Get Key Vault name from Terraform output
$keyVaultName = terraform output -raw key_vault_name

# Add connection strings and secrets
az keyvault secret set --vault-name $keyVaultName --name "ConnectionStrings--DefaultConnection" --value "your-connection-string"
az keyvault secret set --vault-name $keyVaultName --name "ConnectionStrings--SoapServiceEndpoint" --value "your-soap-endpoint"
```

2. **Deploy Application Code:**

```powershell
# Get web app name from Terraform output
$webAppName = terraform output -raw web_app_name
$resourceGroupName = terraform output -raw resource_group_name

# Deploy using Azure CLI (replace with your deployment package)
az webapp deployment source config-zip `
  --resource-group $resourceGroupName `
  --name $webAppName `
  --src "path/to/your/deployment-package.zip"
```

3. **Verify Deployment:**

```powershell
# Get web app URL
$webAppUrl = terraform output -raw web_app_url

# Test health endpoint
Invoke-RestMethod -Uri "$webAppUrl/health" -Method Get
```

## Monitoring and Maintenance

### View Application Insights

```powershell
$appInsightsName = terraform output -raw application_insights_name
az monitor app-insights component show --app $appInsightsName --resource-group $resourceGroupName
```

### View Logs

```powershell
# Stream logs
az webapp log tail --name $webAppName --resource-group $resourceGroupName
```

### Scale App Service Plan

```powershell
$appServicePlanName = terraform output -raw app_service_plan_name

# Scale up
az appservice plan update --name $appServicePlanName --resource-group $resourceGroupName --sku P2v3

# Scale out
az appservice plan update --name $appServicePlanName --resource-group $resourceGroupName --number-of-workers 3
```

## Cleanup

To destroy infrastructure (use with caution):

```powershell
# Development
terraform destroy -var-file="environments/development.tfvars"

# Staging
terraform destroy -var-file="environments/staging.tfvars"

# Production (requires extra confirmation)
Write-Host "WARNING: This will destroy the PRODUCTION environment!" -ForegroundColor Red
$confirmation = Read-Host "Type 'DELETE' to confirm destruction of production resources"
if ($confirmation -eq "DELETE") {
    terraform destroy -var-file="environments/production.tfvars"
}
```

## Troubleshooting

### Common Issues

1. **Authentication Errors:**

   - Ensure `az login` is completed
   - Verify service principal credentials if using

2. **Resource Naming Conflicts:**

   - Some Azure resources require globally unique names
   - Modify the naming module or add random suffixes

3. **Permission Issues:**

   - Ensure the account has Contributor access to the subscription
   - For Key Vault, ensure proper access policies are configured

4. **State Lock Issues:**
   - If using remote state, ensure no other Terraform process is running
   - Use `terraform force-unlock <lock-id>` if needed

### Useful Commands

```powershell
# Check Terraform version
terraform version

# Format Terraform files
terraform fmt -recursive

# Validate configuration
terraform validate

# Show current state
terraform show

# List resources in state
terraform state list

# Import existing resource
terraform import azurerm_resource_group.example /subscriptions/subscription-id/resourceGroups/rg-name
```
