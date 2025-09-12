# Environment-specific configuration for development
environment = "development"
app_service_plan_sku = "B1"
log_retention_days = 30
aspnetcore_environment = "Development"
log_level = "Information"

# CORS configuration for development
cors_allowed_origins = ["*"]

# Monitoring configuration
monitoring_configuration = {
  enable_application_insights = true
  enable_availability_tests   = false
  alert_email_addresses      = []
  response_time_threshold_ms = 10000
  error_rate_threshold       = 10.0
}

# Security configuration for development
security_configuration = {
  enable_waf                    = false
  enable_ddos_protection       = false
  enable_advanced_threat_protection = false
  minimum_tls_version          = "1.2"
  enable_https_only            = true
}

# Network security configuration
network_security = {
  restrict_to_vnet      = false
  allowed_ip_ranges     = []
  enable_private_endpoint = false
}

# Scaling configuration for development
scaling_configuration = {
  minimum_instance_count = 1
  maximum_instance_count = 3
  default_instance_count = 1
  scale_out_cooldown     = "PT5M"
  scale_in_cooldown      = "PT10M"
}

# Database configuration (disabled for development)
database_configuration = {
  enable_sql_database     = false
  sql_database_sku       = "Basic"
  enable_backup          = false
  backup_retention_days  = 7
  enable_geo_redundancy  = false
}

# Container registry (disabled for development)
enable_container_registry = false

# Backup configuration
backup_enabled = false

# Additional tags
tags = {
  Owner = "Development Team"
  CostCenter = "IT-Development"
  Purpose = "API Development"
}
