# CI/CD Deployment Automation Script
# TASK-009: CI/CD Pipeline Setup
# Purpose: Automated deployment script for various environments

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("development", "staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("docker", "windows-service", "iis", "manual")]
    [string]$DeploymentType = "windows-service",
    
    [Parameter(Mandatory = $false)]
    [string]$BuildNumber = (Get-Date -Format "yyyyMMddHHmmss"),
    
    [Parameter(Mandatory = $false)]
    [string]$SourcePath = ".\publish",
    
    [Parameter(Mandatory = $false)]
    [string]$TargetServer = "localhost",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

# =====================================
# CONFIGURATION
# =====================================
$script:Config = @{
    ServiceName     = "UnisonRestAdapter"
    ApplicationName = "Unison REST Adapter"
    Version         = "1.0.0"
    BuildNumber     = $BuildNumber
    
    Environments    = @{
        development = @{
            DeployPath         = "C:\Deploy\UnisonRestAdapter\Development"
            ApiUrl             = "http://localhost:5203"
            BackupRetention    = 3
            RequiresApproval   = $false
            HealthCheckTimeout = 30
        }
        staging     = @{
            DeployPath         = "C:\Deploy\UnisonRestAdapter\Staging"
            ApiUrl             = "http://staging-server:5203"
            BackupRetention    = 5
            RequiresApproval   = $false
            HealthCheckTimeout = 60
        }
        production  = @{
            DeployPath         = "C:\Deploy\UnisonRestAdapter\Production"
            ApiUrl             = "http://production-server"
            BackupRetention    = 10
            RequiresApproval   = $true
            HealthCheckTimeout = 120
        }
    }
}

# =====================================
# LOGGING FUNCTIONS
# =====================================
function Write-Log {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Level = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "Info" { "White" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Success" { "Green" }
    }
    
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
    
    # Also log to file
    $logPath = ".\deployment-logs"
    if (!(Test-Path $logPath)) { New-Item -ItemType Directory -Path $logPath -Force | Out-Null }
    "$timestamp [$Level] $Message" | Out-File -FilePath "$logPath\deploy-$Environment-$(Get-Date -Format 'yyyyMMdd').log" -Append
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n🔄 $Message" -ForegroundColor Cyan
    Write-Log -Message $Message -Level "Info"
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
    Write-Log -Message $Message -Level "Success"
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
    Write-Log -Message $Message -Level "Warning"
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
    Write-Log -Message $Message -Level "Error"
}

# =====================================
# VALIDATION FUNCTIONS
# =====================================
function Test-Prerequisites {
    Write-Step "Validating deployment prerequisites..."
    
    $errors = @()
    
    # Check if source path exists
    if (!(Test-Path $SourcePath)) {
        $errors += "Source path does not exist: $SourcePath"
    }
    
    # Check if required files exist
    $requiredFiles = @(
        "$SourcePath\UnisonRestAdapter.exe",
        "$SourcePath\UnisonRestAdapter.dll",
        "$SourcePath\appsettings.json"
    )
    
    foreach ($file in $requiredFiles) {
        if (!(Test-Path $file)) {
            $errors += "Required file missing: $file"
        }
    }
    
    # Check .NET runtime
    try {
        $dotnetVersion = dotnet --version
        Write-Log "Found .NET version: $dotnetVersion"
    }
    catch {
        $errors += ".NET runtime not found or not accessible"
    }
    
    # Check PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 5) {
        $errors += "PowerShell 5.0 or higher is required"
    }
    
    if ($errors.Count -gt 0) {
        Write-Error "Prerequisites validation failed:"
        $errors | ForEach-Object { Write-Error "  - $_" }
        return $false
    }
    
    Write-Success "Prerequisites validation passed"
    return $true
}

function Test-EnvironmentConnectivity {
    param([string]$ServerName)
    
    Write-Step "Testing connectivity to target environment..."
    
    if ($ServerName -eq "localhost") {
        Write-Success "Local deployment - connectivity OK"
        return $true
    }
    
    try {
        $result = Test-Connection -ComputerName $ServerName -Count 2 -Quiet
        if ($result) {
            Write-Success "Connectivity to $ServerName: OK"
            return $true
        }
        else {
            Write-Error "Cannot reach target server: $ServerName"
            return $false
        }
    }
    catch {
        Write-Error "Connectivity test failed: $($_.Exception.Message)"
        return $false
    }
}

# =====================================
# DEPLOYMENT FUNCTIONS
# =====================================
function Backup-CurrentDeployment {
    param(
        [string]$TargetPath,
        [int]$RetentionCount
    )
    
    Write-Step "Creating backup of current deployment..."
    
    if (!(Test-Path $TargetPath)) {
        Write-Log "Target path does not exist, skipping backup"
        return $true
    }
    
    try {
        $backupPath = "$TargetPath\..\Backups"
        if (!(Test-Path $backupPath)) {
            New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
        }
        
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupName = "backup_$Environment`_$timestamp"
        $fullBackupPath = Join-Path $backupPath $backupName
        
        if ($WhatIf) {
            Write-Log "WHATIF: Would create backup at $fullBackupPath"
        }
        else {
            Copy-Item -Path $TargetPath -Destination $fullBackupPath -Recurse -Force
            Write-Success "Backup created: $fullBackupPath"
        }
        
        # Cleanup old backups
        $backups = Get-ChildItem $backupPath | Where-Object { $_.Name -like "backup_$Environment`_*" } | Sort-Object CreationTime -Descending
        if ($backups.Count -gt $RetentionCount) {
            $toDelete = $backups | Select-Object -Skip $RetentionCount
            foreach ($backup in $toDelete) {
                if ($WhatIf) {
                    Write-Log "WHATIF: Would delete old backup $($backup.Name)"
                }
                else {
                    Remove-Item $backup.FullName -Recurse -Force
                    Write-Log "Removed old backup: $($backup.Name)"
                }
            }
        }
        
        return $true
        
    }
    catch {
        Write-Error "Backup failed: $($_.Exception.Message)"
        return $false
    }
}

function Deploy-WindowsService {
    param(
        [string]$SourcePath,
        [string]$TargetPath
    )
    
    Write-Step "Deploying as Windows Service..."
    
    $serviceName = $script:Config.ServiceName
    $envConfig = $script:Config.Environments[$Environment]
    
    try {
        # Stop service if running
        $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        if ($service -and $service.Status -eq "Running") {
            if ($WhatIf) {
                Write-Log "WHATIF: Would stop service $serviceName"
            }
            else {
                Write-Log "Stopping service: $serviceName"
                Stop-Service -Name $serviceName -Force
                Write-Success "Service stopped"
            }
        }
        
        # Deploy files
        if ($WhatIf) {
            Write-Log "WHATIF: Would copy files from $SourcePath to $TargetPath"
        }
        else {
            if (!(Test-Path $TargetPath)) {
                New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
            }
            
            Copy-Item -Path "$SourcePath\*" -Destination $TargetPath -Recurse -Force
            Write-Success "Application files deployed"
        }
        
        # Update configuration for environment
        $configPath = Join-Path $TargetPath "appsettings.json"
        if (Test-Path $configPath -and !$WhatIf) {
            Update-Configuration -ConfigPath $configPath -Environment $Environment
        }
        
        # Install/update service
        if ($WhatIf) {
            Write-Log "WHATIF: Would install/update Windows service"
        }
        else {
            $binaryPath = Join-Path $TargetPath "UnisonRestAdapter.exe"
            
            if (!$service) {
                # Install new service
                New-Service -Name $serviceName -BinaryPathName $binaryPath -DisplayName $script:Config.ApplicationName -StartupType Automatic
                Write-Success "Service installed: $serviceName"
            }
            
            # Start service
            Start-Service -Name $serviceName
            Write-Success "Service started: $serviceName"
        }
        
        return $true
        
    }
    catch {
        Write-Error "Windows service deployment failed: $($_.Exception.Message)"
        return $false
    }
}

function Deploy-Docker {
    Write-Step "Deploying with Docker..."
    
    try {
        # Check if Docker is available
        $dockerVersion = docker --version
        Write-Log "Found Docker: $dockerVersion"
        
        $containerName = "unison-api-$Environment"
        $imageName = "unison-rest-adapter:$($script:Config.BuildNumber)"
        
        # Stop and remove existing container
        if ($WhatIf) {
            Write-Log "WHATIF: Would stop and remove container $containerName"
        }
        else {
            docker stop $containerName 2>$null
            docker rm $containerName 2>$null
        }
        
        # Build new image
        if ($WhatIf) {
            Write-Log "WHATIF: Would build Docker image $imageName"
        }
        else {
            Write-Log "Building Docker image..."
            docker build -t $imageName .
            Write-Success "Docker image built: $imageName"
        }
        
        # Run container
        if ($WhatIf) {
            Write-Log "WHATIF: Would run container $containerName"
        }
        else {
            $envConfig = $script:Config.Environments[$Environment]
            $port = if ($Environment -eq "production") { "80:8080" } elseif ($Environment -eq "staging") { "6000:8080" } else { "5000:8080" }
            
            docker run -d --name $containerName -p $port -e ASPNETCORE_ENVIRONMENT=$Environment --restart unless-stopped $imageName
            Write-Success "Container started: $containerName"
        }
        
        return $true
        
    }
    catch {
        Write-Error "Docker deployment failed: $($_.Exception.Message)"
        return $false
    }
}

function Update-Configuration {
    param(
        [string]$ConfigPath,
        [string]$Environment
    )
    
    Write-Step "Updating configuration for $Environment environment..."
    
    try {
        if (!(Test-Path $ConfigPath)) {
            Write-Warning "Configuration file not found: $ConfigPath"
            return
        }
        
        $config = Get-Content $ConfigPath | ConvertFrom-Json
        
        # Environment-specific updates
        switch ($Environment) {
            "development" {
                $config.Logging.LogLevel.Default = "Debug"
                $config.Logging.LogLevel.UnisonRestAdapter = "Trace"
                if ($config.Security) {
                    $config.Security.RequireHttps = $false
                    $config.Security.ValidTokens = @("dev-token-123", "test-token-456")
                }
            }
            "staging" {
                $config.Logging.LogLevel.Default = "Information"
                $config.Logging.LogLevel.UnisonRestAdapter = "Debug"
                if ($config.Security) {
                    $config.Security.RequireHttps = $false
                    $config.Security.ValidTokens = @("staging-test-token-2025", "integration-test-token")
                }
            }
            "production" {
                $config.Logging.LogLevel.Default = "Information"
                $config.Logging.LogLevel."Microsoft.AspNetCore" = "Warning"
                $config.Logging.LogLevel.UnisonRestAdapter = "Information"
                if ($config.Security) {
                    $config.Security.RequireHttps = $true
                    $config.Security.TokenRotationEnabled = $true
                    $config.Security.MaxTokenAge = "24:00:00"
                }
            }
        }
        
        # Save updated configuration
        $config | ConvertTo-Json -Depth 10 | Set-Content $ConfigPath -Encoding UTF8
        Write-Success "Configuration updated for $Environment"
        
    }
    catch {
        Write-Error "Configuration update failed: $($_.Exception.Message)"
    }
}

# =====================================
# HEALTH CHECK FUNCTIONS
# =====================================
function Test-DeploymentHealth {
    param([string]$ApiUrl, [int]$TimeoutSeconds)
    
    Write-Step "Running deployment health checks..."
    
    $maxAttempts = [Math]::Ceiling($TimeoutSeconds / 5)
    $attempt = 1
    
    do {
        try {
            Write-Log "Health check attempt $attempt of $maxAttempts..."
            
            # Basic health check
            $response = Invoke-WebRequest -Uri "$ApiUrl/health" -Method GET -TimeoutSec 30 -UseBasicParsing
            
            if ($response.StatusCode -eq 200) {
                $healthData = $response.Content | ConvertFrom-Json
                Write-Success "Health check passed - Status: $($healthData.status)"
                
                # Test detailed health endpoint if available
                try {
                    $detailedResponse = Invoke-WebRequest -Uri "$ApiUrl/health/detailed" -Method GET -TimeoutSec 30 -UseBasicParsing
                    if ($detailedResponse.StatusCode -eq 200) {
                        Write-Success "Detailed health check passed"
                    }
                }
                catch {
                    Write-Warning "Detailed health check not available"
                }
                
                return $true
            }
            
        }
        catch {
            Write-Log "Health check attempt $attempt failed: $($_.Exception.Message)"
        }
        
        if ($attempt -lt $maxAttempts) {
            Write-Log "Waiting 5 seconds before retry..."
            Start-Sleep -Seconds 5
        }
        
        $attempt++
        
    } while ($attempt -le $maxAttempts)
    
    Write-Error "Health checks failed after $maxAttempts attempts"
    return $false
}

# =====================================
# MAIN DEPLOYMENT LOGIC
# =====================================
function Start-Deployment {
    Write-Host "`n🚀 Starting deployment to $Environment environment" -ForegroundColor Magenta
    Write-Host "Deployment Type: $DeploymentType" -ForegroundColor Magenta
    Write-Host "Build Number: $BuildNumber" -ForegroundColor Magenta
    Write-Host "What-If Mode: $WhatIf" -ForegroundColor Magenta
    Write-Host "`n" + ("=" * 60) + "`n" -ForegroundColor Gray
    
    $envConfig = $script:Config.Environments[$Environment]
    
    # Step 1: Prerequisites
    if (!(Test-Prerequisites)) {
        Write-Error "Prerequisites check failed. Deployment aborted."
        return $false
    }
    
    # Step 2: Environment connectivity
    if (!(Test-EnvironmentConnectivity -ServerName $TargetServer)) {
        Write-Error "Environment connectivity check failed. Deployment aborted."
        return $false
    }
    
    # Step 3: Approval for production
    if ($envConfig.RequiresApproval -and !$WhatIf) {
        Write-Warning "Production deployment requires manual approval."
        $approval = Read-Host "Type 'DEPLOY' to proceed with production deployment"
        if ($approval -ne "DEPLOY") {
            Write-Warning "Deployment cancelled by user."
            return $false
        }
    }
    
    # Step 4: Backup current deployment
    if (!(Backup-CurrentDeployment -TargetPath $envConfig.DeployPath -RetentionCount $envConfig.BackupRetention)) {
        Write-Error "Backup failed. Deployment aborted."
        return $false
    }
    
    # Step 5: Deploy based on type
    $deploymentSuccess = switch ($DeploymentType) {
        "windows-service" { Deploy-WindowsService -SourcePath $SourcePath -TargetPath $envConfig.DeployPath }
        "docker" { Deploy-Docker }
        default { 
            Write-Error "Unsupported deployment type: $DeploymentType"
            $false
        }
    }
    
    if (!$deploymentSuccess) {
        Write-Error "Deployment failed."
        return $false
    }
    
    # Step 6: Health checks
    if (!$SkipTests -and !$WhatIf) {
        Start-Sleep -Seconds 10  # Allow time for service to start
        
        if (!(Test-DeploymentHealth -ApiUrl $envConfig.ApiUrl -TimeoutSeconds $envConfig.HealthCheckTimeout)) {
            Write-Error "Health checks failed. Consider rollback."
            return $false
        }
    }
    elseif ($SkipTests) {
        Write-Warning "Health checks skipped as requested"
    }
    
    # Step 7: Success
    Write-Host "`n" + ("=" * 60) -ForegroundColor Gray
    Write-Success "🎉 Deployment to $Environment completed successfully!"
    Write-Success "Build Number: $BuildNumber"
    Write-Success "Deployment Type: $DeploymentType"
    Write-Success "API URL: $($envConfig.ApiUrl)"
    
    if ($WhatIf) {
        Write-Host "`nNote: This was a What-If run. No actual changes were made." -ForegroundColor Yellow
    }
    
    return $true
}

# =====================================
# SCRIPT EXECUTION
# =====================================
try {
    $success = Start-Deployment
    if ($success) {
        exit 0
    }
    else {
        exit 1
    }
}
catch {
    Write-Error "Deployment script failed with unexpected error: $($_.Exception.Message)"
    Write-Error $_.ScriptStackTrace
    exit 1
}
