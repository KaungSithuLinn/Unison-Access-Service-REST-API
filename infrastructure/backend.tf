# Terraform State Backend Configuration
# This file configures remote state storage in Azure Storage

terraform {
  backend "azurerm" {
    # These values should be provided via backend configuration or environment variables
    # resource_group_name   = "rg-terraform-state"
    # storage_account_name  = "saterraformstate001"
    # container_name        = "tfstate"
    # key                   = "unison-access-service/terraform.tfstate"
  }
}

# Example backend configuration commands:
# 
# Development:
# terraform init -backend-config="resource_group_name=rg-terraform-state-dev" \
#                -backend-config="storage_account_name=saterraformstatedev001" \
#                -backend-config="container_name=tfstate" \
#                -backend-config="key=unison-access-service/dev/terraform.tfstate"
#
# Staging:
# terraform init -backend-config="resource_group_name=rg-terraform-state-staging" \
#                -backend-config="storage_account_name=saterraformstatestaging001" \
#                -backend-config="container_name=tfstate" \
#                -backend-config="key=unison-access-service/staging/terraform.tfstate"
#
# Production:
# terraform init -backend-config="resource_group_name=rg-terraform-state-prod" \
#                -backend-config="storage_account_name=saterraformstateprod001" \
#                -backend-config="container_name=tfstate" \
#                -backend-config="key=unison-access-service/prod/terraform.tfstate"
