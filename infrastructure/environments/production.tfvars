# Environment-specific configuration for production
environment = "production"
app_service_plan_sku = "P2v3"
log_retention_days = 90
aspnetcore_environment = "Production"
log_level = "Warning"

# CORS configuration for production (restrict to specific domains)
cors_allowed_origins = [
  "https://yourdomain.com",
  "https://www.yourdomain.com",
  "https://api.yourdomain.com"
]

# Monitoring configuration
monitoring_configuration = {
  enable_application_insights = true
  enable_availability_tests   = true
  alert_email_addresses      = ["admin@yourdomain.com", "ops@yourdomain.com"]
  response_time_threshold_ms = 3000
  error_rate_threshold       = 2.0
}

# Security configuration for production
security_configuration = {
  enable_waf                    = true
  enable_ddos_protection       = true
  enable_advanced_threat_protection = true
  minimum_tls_version          = "1.3"
  enable_https_only            = true
}

# Network security configuration
network_security = {
  restrict_to_vnet      = false
  allowed_ip_ranges     = []
  enable_private_endpoint = false
}

# Scaling configuration for production
scaling_configuration = {
  minimum_instance_count = 2
  maximum_instance_count = 20
  default_instance_count = 3
  scale_out_cooldown     = "PT3M"
  scale_in_cooldown      = "PT15M"
}

# Database configuration
database_configuration = {
  enable_sql_database     = false  # Set to true when database is needed
  sql_database_sku       = "S2"
  enable_backup          = true
  backup_retention_days  = 35
  enable_geo_redundancy  = true
}

# Container registry (enabled for production)
enable_container_registry = true

# Staging slot configuration
enable_staging_slot = true

# Backup configuration
backup_enabled = true

# Custom domain configuration (uncomment and configure when ready)
# custom_domain = {
#   hostname           = "api.yourdomain.com"
#   certificate_source = "AppService"
# }

# Additional tags
tags = {
  Owner = "Operations Team"
  CostCenter = "IT-Production"
  Purpose = "Production API Service"
  BusinessUnit = "Technology"
  Compliance = "Required"
  BackupSchedule = "Daily"
}
