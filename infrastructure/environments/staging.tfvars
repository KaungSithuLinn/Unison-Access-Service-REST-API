# Environment-specific configuration for staging
environment = "staging"
app_service_plan_sku = "P1v2"
log_retention_days = 60
aspnetcore_environment = "Staging"
log_level = "Information"

# CORS configuration for staging
cors_allowed_origins = [
  "https://staging.yourdomain.com",
  "https://test.yourdomain.com"
]

# Monitoring configuration
monitoring_configuration = {
  enable_application_insights = true
  enable_availability_tests   = true
  alert_email_addresses      = ["qa@yourdomain.com", "dev@yourdomain.com"]
  response_time_threshold_ms = 5000
  error_rate_threshold       = 5.0
}

# Security configuration for staging
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

# Scaling configuration for staging
scaling_configuration = {
  minimum_instance_count = 1
  maximum_instance_count = 5
  default_instance_count = 2
  scale_out_cooldown     = "PT5M"
  scale_in_cooldown      = "PT10M"
}

# Database configuration
database_configuration = {
  enable_sql_database     = false
  sql_database_sku       = "S1"
  enable_backup          = true
  backup_retention_days  = 14
  enable_geo_redundancy  = false
}

# Container registry (enabled for staging)
enable_container_registry = true

# Backup configuration
backup_enabled = true

# Additional tags
tags = {
  Owner = "QA Team"
  CostCenter = "IT-Testing"
  Purpose = "Staging API Service"
  Environment = "Pre-Production"
}
