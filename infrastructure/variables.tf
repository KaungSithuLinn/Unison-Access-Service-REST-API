# Variable definitions for Unison Access Service REST API infrastructure

variable "environment" {
  description = "Environment name (development, staging, production)"
  type        = string
  default     = "development"
  
  validation {
    condition     = contains(["development", "staging", "production"], var.environment)
    error_message = "Environment must be one of: development, staging, production."
  }
}

variable "app_service_plan_sku" {
  description = "SKU for the App Service Plan"
  type        = string
  default     = "P1v2"
  
  validation {
    condition = contains([
      "F1", "D1", "B1", "B2", "B3", 
      "S1", "S2", "S3", 
      "P1v2", "P2v2", "P3v2",
      "P1v3", "P2v3", "P3v3"
    ], var.app_service_plan_sku)
    error_message = "App Service Plan SKU must be a valid Azure App Service Plan SKU."
  }
}

variable "log_retention_days" {
  description = "Number of days to retain logs in Log Analytics workspace"
  type        = number
  default     = 30
  
  validation {
    condition     = var.log_retention_days >= 30 && var.log_retention_days <= 730
    error_message = "Log retention days must be between 30 and 730."
  }
}

variable "aspnetcore_environment" {
  description = "ASP.NET Core environment setting"
  type        = string
  default     = "Development"
  
  validation {
    condition     = contains(["Development", "Staging", "Production"], var.aspnetcore_environment)
    error_message = "ASPNETCORE_ENVIRONMENT must be one of: Development, Staging, Production."
  }
}

variable "log_level" {
  description = "Default logging level for the application"
  type        = string
  default     = "Information"
  
  validation {
    condition = contains([
      "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None"
    ], var.log_level)
    error_message = "Log level must be a valid .NET log level."
  }
}

variable "cors_allowed_origins" {
  description = "List of allowed CORS origins for the API"
  type        = list(string)
  default     = ["*"]
}

variable "enable_container_registry" {
  description = "Whether to create an Azure Container Registry"
  type        = bool
  default     = false
}

variable "enable_staging_slot" {
  description = "Whether to create a staging deployment slot (production environment only)"
  type        = bool
  default     = true
}

variable "backup_enabled" {
  description = "Whether to enable automatic backups for the web app"
  type        = bool
  default     = true
}

variable "custom_domain" {
  description = "Custom domain configuration for the web app"
  type = object({
    hostname              = string
    certificate_source    = string  # "AppService" or "KeyVault"
    key_vault_secret_id   = optional(string)
  })
  default = null
}

variable "network_security" {
  description = "Network security configuration"
  type = object({
    restrict_to_vnet      = bool
    allowed_ip_ranges     = list(string)
    enable_private_endpoint = bool
  })
  default = {
    restrict_to_vnet      = false
    allowed_ip_ranges     = []
    enable_private_endpoint = false
  }
}

variable "scaling_configuration" {
  description = "Auto-scaling configuration for the app service plan"
  type = object({
    minimum_instance_count = number
    maximum_instance_count = number
    default_instance_count = number
    scale_out_cooldown     = string
    scale_in_cooldown      = string
  })
  default = {
    minimum_instance_count = 1
    maximum_instance_count = 10
    default_instance_count = 2
    scale_out_cooldown     = "PT5M"
    scale_in_cooldown      = "PT10M"
  }
}

variable "monitoring_configuration" {
  description = "Monitoring and alerting configuration"
  type = object({
    enable_application_insights = bool
    enable_availability_tests   = bool
    alert_email_addresses      = list(string)
    response_time_threshold_ms = number
    error_rate_threshold       = number
  })
  default = {
    enable_application_insights = true
    enable_availability_tests   = false
    alert_email_addresses      = []
    response_time_threshold_ms = 5000
    error_rate_threshold       = 5.0
  }
}

variable "database_configuration" {
  description = "Database configuration settings"
  type = object({
    enable_sql_database     = bool
    sql_database_sku       = string
    enable_backup          = bool
    backup_retention_days  = number
    enable_geo_redundancy  = bool
  })
  default = {
    enable_sql_database     = false
    sql_database_sku       = "S0"
    enable_backup          = true
    backup_retention_days  = 7
    enable_geo_redundancy  = false
  }
}

variable "security_configuration" {
  description = "Security-related configuration"
  type = object({
    enable_waf                    = bool
    enable_ddos_protection       = bool
    enable_advanced_threat_protection = bool
    minimum_tls_version          = string
    enable_https_only            = bool
  })
  default = {
    enable_waf                    = false
    enable_ddos_protection       = false
    enable_advanced_threat_protection = false
    minimum_tls_version          = "1.3"
    enable_https_only            = true
  }
}

variable "tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default     = {}
}
