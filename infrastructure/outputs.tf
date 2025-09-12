# Output values for Unison Access Service REST API infrastructure

# Resource Group Information
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Location of the resource group"
  value       = azurerm_resource_group.main.location
}

output "resource_group_id" {
  description = "ID of the resource group"
  value       = azurerm_resource_group.main.id
}

# Web App Information
output "web_app_name" {
  description = "Name of the web app"
  value       = module.web_app.resource.name
}

output "web_app_url" {
  description = "Default URL of the web app"
  value       = "https://${module.web_app.resource.default_hostname}"
}

output "web_app_id" {
  description = "ID of the web app"
  value       = module.web_app.resource.id
}

output "web_app_principal_id" {
  description = "Principal ID of the web app's managed identity"
  value       = module.web_app.resource.identity[0].principal_id
}

output "web_app_tenant_id" {
  description = "Tenant ID of the web app's managed identity"
  value       = module.web_app.resource.identity[0].tenant_id
}

output "web_app_outbound_ip_addresses" {
  description = "Outbound IP addresses of the web app"
  value       = module.web_app.resource.outbound_ip_addresses
}

output "web_app_possible_outbound_ip_addresses" {
  description = "Possible outbound IP addresses of the web app"
  value       = module.web_app.resource.possible_outbound_ip_addresses
}

# App Service Plan Information
output "app_service_plan_name" {
  description = "Name of the app service plan"
  value       = azurerm_service_plan.main.name
}

output "app_service_plan_id" {
  description = "ID of the app service plan"
  value       = azurerm_service_plan.main.id
}

output "app_service_plan_sku" {
  description = "SKU of the app service plan"
  value       = azurerm_service_plan.main.sku_name
}

# Storage Account Information
output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.main.name
}

output "storage_account_id" {
  description = "ID of the storage account"
  value       = azurerm_storage_account.main.id
}

output "storage_account_primary_endpoint" {
  description = "Primary endpoint of the storage account"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

# Key Vault Information
output "key_vault_name" {
  description = "Name of the key vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_id" {
  description = "ID of the key vault"
  value       = azurerm_key_vault.main.id
}

output "key_vault_uri" {
  description = "URI of the key vault"
  value       = azurerm_key_vault.main.vault_uri
}

# Log Analytics Workspace Information
output "log_analytics_workspace_name" {
  description = "Name of the log analytics workspace"
  value       = azurerm_log_analytics_workspace.main.name
}

output "log_analytics_workspace_id" {
  description = "ID of the log analytics workspace"
  value       = azurerm_log_analytics_workspace.main.id
}

output "log_analytics_workspace_workspace_id" {
  description = "Workspace ID of the log analytics workspace"
  value       = azurerm_log_analytics_workspace.main.workspace_id
}

# Application Insights Information
output "application_insights_name" {
  description = "Name of the application insights instance"
  value       = module.web_app.resource_application_insights != null ? module.web_app.resource_application_insights.name : null
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key of the application insights instance"
  value       = module.web_app.resource_application_insights != null ? module.web_app.resource_application_insights.instrumentation_key : null
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string of the application insights instance"
  value       = module.web_app.resource_application_insights != null ? module.web_app.resource_application_insights.connection_string : null
  sensitive   = true
}

# Container Registry Information (if enabled)
output "container_registry_name" {
  description = "Name of the container registry"
  value       = var.enable_container_registry ? azurerm_container_registry.main[0].name : null
}

output "container_registry_id" {
  description = "ID of the container registry"
  value       = var.enable_container_registry ? azurerm_container_registry.main[0].id : null
}

output "container_registry_login_server" {
  description = "Login server of the container registry"
  value       = var.enable_container_registry ? azurerm_container_registry.main[0].login_server : null
}

# Deployment Information
output "deployment_environment" {
  description = "Deployment environment"
  value       = var.environment
}

output "deployment_region" {
  description = "Azure region where resources are deployed"
  value       = azurerm_resource_group.main.location
}

# API Endpoints
output "api_health_check_url" {
  description = "Health check endpoint URL"
  value       = "https://${module.web_app.resource.default_hostname}/health"
}

output "api_swagger_url" {
  description = "Swagger documentation URL"
  value       = "https://${module.web_app.resource.default_hostname}/swagger"
}

output "api_base_url" {
  description = "Base API URL"
  value       = "https://${module.web_app.resource.default_hostname}/api"
}

# Staging Slot Information (if created)
output "staging_slot_name" {
  description = "Name of the staging deployment slot"
  value       = var.environment == "production" && var.enable_staging_slot ? "staging" : null
}

output "staging_slot_url" {
  description = "URL of the staging deployment slot"
  value       = var.environment == "production" && var.enable_staging_slot ? "https://${module.web_app.resource.name}-staging.azurewebsites.net" : null
}

# Security Information
output "managed_identity_principal_id" {
  description = "Principal ID of the web app's system-assigned managed identity"
  value       = module.web_app.resource.identity[0].principal_id
}

output "managed_identity_tenant_id" {
  description = "Tenant ID of the web app's system-assigned managed identity"
  value       = module.web_app.resource.identity[0].tenant_id
}

# Configuration for CI/CD
output "publish_profile_name" {
  description = "Name for downloading publish profile"
  value       = module.web_app.resource.name
}

output "scm_site_url" {
  description = "SCM site URL for deployments"
  value       = "https://${module.web_app.resource.name}.scm.azurewebsites.net"
}

# Network Information
output "vnet_integration_subnet_id" {
  description = "Subnet ID for VNet integration (if configured)"
  value       = null  # Will be populated when VNet integration is implemented
}

output "private_endpoint_ip" {
  description = "Private endpoint IP address (if configured)"
  value       = null  # Will be populated when private endpoints are implemented
}
