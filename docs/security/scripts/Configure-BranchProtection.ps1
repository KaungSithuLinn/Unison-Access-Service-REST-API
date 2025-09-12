# Configure-BranchProtection.ps1
# PowerShell script to configure comprehensive branch protection for the main branch
# Requires GitHub Personal Access Token with repo permissions

param(
    [Parameter(Mandatory = $true)]
    [string]$Owner,
    
    [Parameter(Mandatory = $true)]
    [string]$Repo,
    
    [Parameter(Mandatory = $false)]
    [string]$Token = $env:GITHUB_TOKEN,
    
    [Parameter(Mandatory = $false)]
    [string]$Branch = "main",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

# Validate parameters
if (-not $Token) {
    throw "GitHub token is required. Set GITHUB_TOKEN environment variable or pass -Token parameter."
}

# GitHub API base URL
$apiBase = "https://api.github.com"
$headers = @{
    "Authorization"        = "Bearer $Token"
    "Accept"               = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

# Branch protection configuration
$protectionConfig = @{
    required_status_checks        = @{
        strict   = $true
        contexts = @(
            "codacy/pr-check",
            "ci/application", 
            "ci/infrastructure"
        )
    }
    enforce_admins                = $true
    required_pull_request_reviews = @{
        required_approving_review_count = 1
        dismiss_stale_reviews           = $true
        require_code_owner_reviews      = $false
        require_last_push_approval      = $true
    }
    restrictions                  = $null
    required_linear_history       = $true
    allow_force_pushes            = $false
    allow_deletions               = $false
    block_creations               = $false
}

function Write-Status {
    param([string]$Message, [string]$Level = "Info")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        default { "White" }
    }
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Test-GitHubConnection {
    try {
        $response = Invoke-RestMethod -Uri "$apiBase/user" -Headers $headers
        Write-Status "Connected to GitHub as: $($response.login)" -Level "Success"
        return $true
    }
    catch {
        Write-Status "Failed to connect to GitHub: $($_.Exception.Message)" -Level "Error"
        return $false
    }
}

function Get-CurrentProtection {
    try {
        $uri = "$apiBase/repos/$Owner/$Repo/branches/$Branch/protection"
        $response = Invoke-RestMethod -Uri $uri -Headers $headers
        Write-Status "Current protection rules retrieved" -Level "Success"
        return $response
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Status "No existing protection rules found" -Level "Warning"
            return $null
        }
        throw
    }
}

function Set-BranchProtection {
    try {
        $uri = "$apiBase/repos/$Owner/$Repo/branches/$Branch/protection"
        $json = $protectionConfig | ConvertTo-Json -Depth 10
        
        if ($WhatIf) {
            Write-Status "WHAT-IF: Would apply the following protection configuration:" -Level "Warning"
            Write-Host $json -ForegroundColor Cyan
            return $true
        }
        
        $response = Invoke-RestMethod -Uri $uri -Method PUT -Headers $headers -Body $json -ContentType "application/json"
        Write-Status "Branch protection rules applied successfully" -Level "Success"
        return $response
    }
    catch {
        Write-Status "Failed to apply protection rules: $($_.Exception.Message)" -Level "Error"
        throw
    }
}

function Test-BranchProtection {
    try {
        $protection = Get-CurrentProtection
        if (-not $protection) {
            Write-Status "No protection rules found - configuration failed" -Level "Error"
            return $false
        }
        
        Write-Status "Validating protection configuration..." -Level "Info"
        
        # Check required status checks
        if ($protection.required_status_checks.strict -and 
            $protection.required_status_checks.contexts -contains "codacy/pr-check") {
            Write-Status "✅ Required status checks configured correctly" -Level "Success"
        }
        else {
            Write-Status "❌ Required status checks not configured properly" -Level "Error"
        }
        
        # Check PR reviews
        if ($protection.required_pull_request_reviews.required_approving_review_count -ge 1) {
            Write-Status "✅ Pull request reviews configured correctly" -Level "Success"
        }
        else {
            Write-Status "❌ Pull request reviews not configured properly" -Level "Error"
        }
        
        # Check admin enforcement
        if ($protection.enforce_admins.enabled) {
            Write-Status "✅ Admin enforcement enabled" -Level "Success"
        }
        else {
            Write-Status "❌ Admin enforcement not enabled" -Level "Error"
        }
        
        # Check linear history
        if ($protection.required_linear_history.enabled) {
            Write-Status "✅ Linear history required" -Level "Success"
        }
        else {
            Write-Status "❌ Linear history not required" -Level "Error"
        }
        
        # Check force push protection
        if (-not $protection.allow_force_pushes.enabled) {
            Write-Status "✅ Force pushes blocked" -Level "Success"
        }
        else {
            Write-Status "❌ Force pushes not blocked" -Level "Error"
        }
        
        # Check deletion protection
        if (-not $protection.allow_deletions.enabled) {
            Write-Status "✅ Branch deletion blocked" -Level "Success"
        }
        else {
            Write-Status "❌ Branch deletion not blocked" -Level "Error"
        }
        
        return $true
    }
    catch {
        Write-Status "Failed to validate protection: $($_.Exception.Message)" -Level "Error"
        return $false
    }
}

function Main {
    Write-Status "Starting branch protection configuration for $Owner/$Repo:$Branch" -Level "Info"
    
    # Test GitHub connection
    if (-not (Test-GitHubConnection)) {
        exit 1
    }
    
    # Show current protection status
    Write-Status "Checking current protection status..." -Level "Info"
    $currentProtection = Get-CurrentProtection
    
    if ($currentProtection) {
        Write-Status "Current protection rules found - will update existing configuration" -Level "Warning"
    }
    else {
        Write-Status "No existing protection rules - will create new configuration" -Level "Info"
    }
    
    # Apply protection rules
    Write-Status "Applying branch protection rules..." -Level "Info"
    try {
        Set-BranchProtection
        
        if (-not $WhatIf) {
            # Wait a moment for changes to propagate
            Start-Sleep -Seconds 2
            
            # Validate the configuration
            Write-Status "Validating applied configuration..." -Level "Info"
            Test-BranchProtection
            
            Write-Status "Branch protection configuration completed successfully!" -Level "Success"
            Write-Status "Repository: https://github.com/$Owner/$Repo/settings/branches" -Level "Info"
        }
    }
    catch {
        Write-Status "Configuration failed: $($_.Exception.Message)" -Level "Error"
        exit 1
    }
}

# Run the main function
Main
