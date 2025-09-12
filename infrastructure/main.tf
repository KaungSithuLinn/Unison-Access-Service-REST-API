# Terraform configuration for Unison Access Service REST API deployment
terraform {
  required_version = "~> 1.9"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.5.0, < 4.0.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Random region selection for high availability
module "regions" {
  source  = "Azure/regions/azurerm"
  version = "0.8.0"
}

resource "random_integer" "region_index" {
  max = length(local.azure_regions) - 1
  min = 0
}

# CAF compliant naming convention
module "naming" {
  source  = "Azure/naming/azurerm"
  version = "0.4.2"
}

# Local variables for configuration
locals {
  azure_regions = [
    "East US",
    "East US 2", 
    "Central US",
    "West US 2",
    "West Europe",
    "North Europe"
  ]
  
  environment = var.environment
  project_name = "unison-access-service"
  
  common_tags = {
    Project     = "Unison Access Service REST API"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Repository  = "KaungSithuLinn/Unison-Access-Service-REST-API"
    Version     = "1.0.0"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  location = local.azure_regions[random_integer.region_index.result]
  name     = "${module.naming.resource_group.name_unique}-${local.environment}"
  
  tags = local.common_tags
}

# Application Service Plan (Premium P1v2 for production workloads)
resource "azurerm_service_plan" "main" {
  location            = azurerm_resource_group.main.location
  name                = "${module.naming.app_service_plan.name_unique}-${local.environment}"
  os_type             = "Linux"
  resource_group_name = azurerm_resource_group.main.name
  sku_name            = var.app_service_plan_sku
  
  tags = merge(local.common_tags, {
    Component = "App Service Plan"
  })
}

# Storage Account for Function App runtime storage
resource "azurerm_storage_account" "main" {
  account_replication_type = "ZRS"  # Zone-redundant storage for high availability
  account_tier             = "Standard"
  location                 = azurerm_resource_group.main.location
  name                     = "${module.naming.storage_account.name_unique}${local.environment}"
  resource_group_name      = azurerm_resource_group.main.name
  
  # Security configurations
  allow_nested_items_to_be_public = false
  enable_https_traffic_only       = true
  min_tls_version                 = "TLS1_2"
  
  # Network access rules
  network_rules {
    default_action = "Allow"
    bypass         = ["AzureServices"]
  }
  
  tags = merge(local.common_tags, {
    Component = "Storage Account"
  })
}

# Log Analytics Workspace for monitoring
resource "azurerm_log_analytics_workspace" "main" {
  location            = azurerm_resource_group.main.location
  name                = "${module.naming.log_analytics_workspace.name_unique}-${local.environment}"
  resource_group_name = azurerm_resource_group.main.name
  retention_in_days   = var.log_retention_days
  sku                 = "PerGB2018"
  
  tags = merge(local.common_tags, {
    Component = "Log Analytics"
  })
}

# Key Vault for secure configuration storage
resource "azurerm_key_vault" "main" {
  location                    = azurerm_resource_group.main.location
  name                        = "${module.naming.key_vault.name_unique}-${local.environment}"
  resource_group_name         = azurerm_resource_group.main.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  
  # Security configurations
  enabled_for_disk_encryption     = true
  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  purge_protection_enabled        = var.environment == "production" ? true : false
  
  # Network access configuration
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }
  
  tags = merge(local.common_tags, {
    Component = "Key Vault"
  })
}

# Current client configuration for tenant ID
data "azurerm_client_config" "current" {}

# Key Vault access policy for the current user/service principal
resource "azurerm_key_vault_access_policy" "main" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id
  
  key_permissions = [
    "Get", "List", "Create", "Delete", "Update"
  ]
  
  secret_permissions = [
    "Get", "List", "Set", "Delete", "Recover", "Backup", "Restore"
  ]
  
  certificate_permissions = [
    "Get", "List", "Create", "Delete", "Update", "Import"
  ]
}

# Web App using Azure Verified Module
module "web_app" {
  source = "Azure/avm-res-web-site/azurerm"
  version = "0.19.1"
  
  # Basic configuration
  kind                     = "webapp"
  location                 = azurerm_resource_group.main.location
  name                     = "${module.naming.app_service.name_unique}-${local.environment}"
  os_type                  = azurerm_service_plan.main.os_type
  resource_group_name      = azurerm_resource_group.main.name
  service_plan_resource_id = azurerm_service_plan.main.id
  
  # Application Insights for monitoring
  application_insights = {
    name                  = "${module.naming.application_insights.name_unique}-${local.environment}"
    workspace_resource_id = azurerm_log_analytics_workspace.main.id
    application_type      = "web"
  }
  
  # Security configuration
  https_only = true
  
  # Site configuration for .NET 9.0 API
  site_config = {
    always_on                = true
    minimum_tls_version      = "1.3"
    ftps_state               = "FtpsOnly"
    http2_enabled            = true
    websockets_enabled       = false
    use_32_bit_worker        = false
    health_check_path        = "/health"
    
    # .NET 9.0 application stack
    application_stack = {
      dotnet = {
        dotnet_version              = "9.0"
        use_custom_runtime          = false
        use_dotnet_isolated_runtime = true
      }
    }
    
    # CORS configuration for API
    cors = {
      allowed_origins = var.cors_allowed_origins
      support_credentials = false
    }
  }
  
  # Application settings
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"               = var.aspnetcore_environment
    "WEBSITE_RUN_FROM_PACKAGE"             = "1"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"      = "true"
    "WEBSITE_TIME_ZONE"                    = "UTC"
    
    # Connection strings and configuration (will be stored in Key Vault)
    "KeyVault__VaultUri"                   = azurerm_key_vault.main.vault_uri
    "Logging__LogLevel__Default"           = var.log_level
    "Logging__LogLevel__Microsoft"         = "Warning"
    "Logging__LogLevel__Microsoft.Hosting.Lifetime" = "Information"
    
    # API-specific settings
    "Api__Title"                           = "Unison Access Service REST API"
    "Api__Version"                         = "v1"
    "Api__Description"                     = "REST API with SOAP adapter for Unison Access Service"
    
    # Performance settings
    "WEBSITE_HTTPLOGGING_RETENTION_DAYS"   = "3"
    "WEBSITE_HTTPSCALEV2_ENABLED"          = "1"
  }
  
  # Connection strings for database and external services
  connection_strings = {
    DefaultConnection = {
      name  = "DefaultConnection"
      type  = "SQLServer"
      value = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/ConnectionStrings--DefaultConnection/)"
    }
    SoapServiceEndpoint = {
      name  = "SoapServiceEndpoint" 
      type  = "Custom"
      value = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/ConnectionStrings--SoapServiceEndpoint/)"
    }
  }
  
  # Auto-heal configuration for production resilience
  auto_heal_setting = {
    setting_1 = {
      action = {
        action_type                    = "Recycle"
        minimum_process_execution_time = "00:01:00"
      }
      trigger = {
        requests = {
          request = {
            count    = 1000
            interval = "00:05:00"
          }
        }
        status_code = {
          status_500 = {
            count             = 10
            interval          = "00:05:00"
            path              = "/health"
            status_code_range = 500
            sub_status        = 0
          }
        }
        private_memory_kb = 2097152  # 2GB memory threshold
      }
    }
  }
  
  # Managed Identity for secure access to Azure resources
  managed_identities = {
    system_assigned = true
  }
  
  # Backup configuration for production
  backup = var.environment == "production" ? {
    backup = {
      name                = "${local.project_name}-backup"
      storage_account_url = "${azurerm_storage_account.main.primary_blob_endpoint}backups"
      enabled             = true
      schedule = {
        frequency_interval       = 1
        frequency_unit           = "Day" 
        keep_at_least_one_backup = true
        retention_period_days    = 30
        start_time               = "02:00"
      }
    }
  } : {}
  
  # Deployment slots for staging (production only)
  deployment_slots = var.environment == "production" ? {
    staging = {
      name = "staging"
      site_config = {
        application_stack = {
          dotnet = {
            dotnet_version              = "9.0"
            use_custom_runtime          = false
            use_dotnet_isolated_runtime = true
          }
        }
        health_check_path = "/health"
      }
      app_settings = {
        "ASPNETCORE_ENVIRONMENT" = "Staging"
        "WEBSITE_RUN_FROM_PACKAGE" = "1"
      }
    }
  } : {}
  
  # Diagnostic settings
  diagnostic_settings = {
    default = {
      name                  = "default-diagnostics"
      workspace_resource_id = azurerm_log_analytics_workspace.main.id
      
      enabled_log = [
        {
          category_group = "allLogs"
        }
      ]
      
      metric = [
        {
          category = "AllMetrics"
        }
      ]
    }
  }
  
  # Enable telemetry
  enable_telemetry = true
  
  tags = merge(local.common_tags, {
    Component = "Web App"
  })
}

# Grant Key Vault access to the Web App's managed identity
resource "azurerm_key_vault_access_policy" "web_app" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = module.web_app.resource.identity[0].tenant_id
  object_id    = module.web_app.resource.identity[0].principal_id
  
  secret_permissions = [
    "Get", "List"
  ]
  
  depends_on = [module.web_app]
}

# Container Registry for Docker images (optional for future containerization)
resource "azurerm_container_registry" "main" {
  count = var.enable_container_registry ? 1 : 0
  
  location            = azurerm_resource_group.main.location
  name                = "${module.naming.container_registry.name_unique}${local.environment}"
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"
  admin_enabled       = false
  
  # Network access rules
  network_rule_set = {
    default_action = "Allow"
  }
  
  tags = merge(local.common_tags, {
    Component = "Container Registry"
  })
}
