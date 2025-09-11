# Apply security fixes to all feature branches
param(
    [string]$SourceBranch = "feature/issue-003-performance-optimization"
)

$branches = @(
    "feature/issue-001-error-handling-enhancement",
    "feature/issue-002-structured-logging", 
    "feature/issue-004-security-hardening",
    "feature/issue-005-endpoint-expansion",
    "feature/issue-006-monitoring-health-checks",
    "feature/issue-007-integration-testing-playwright"
)

$securityFiles = @(
    ".github/workflows/cd.yml",
    ".github/workflows/ci.yml", 
    ".github/copilot-instructions.md",
    ".codacy/codacy.yaml",
    "UnisonHybridClient/Program.cs",
    "UnisonHybridClient/UnisonHybridClient.csproj",
    "UnisonRestAdapter/Properties/launchSettings.json",
    "UnisonRestAdapter/test_api.py",
    "comprehensive_updatecard_test.py",
    "step4_performance_monitoring.py"
)

Write-Host "Applying security fixes from $SourceBranch to all branches..." -ForegroundColor Green

foreach ($branch in $branches) {
    Write-Host "Processing branch: $branch" -ForegroundColor Yellow
    
    # Switch to the branch
    git checkout $branch
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to checkout $branch, skipping..." -ForegroundColor Red
        continue
    }
    
    # Apply security fixes for each file
    foreach ($file in $securityFiles) {
        Write-Host "  Copying security fix for: $file" -ForegroundColor Cyan
        git checkout $SourceBranch -- $file 2>$null
    }
    
    # Commit the changes
    git add . 
    $commitMessage = "Apply security fixes: CVE-2023-29331, GitHub Actions hardening, credential security

- Updated UnisonHybridClient to .NET 8.0 to resolve System.Security.Cryptography.Pkcs vulnerability CVE-2023-29331
- Hardened GitHub Actions workflows against shell injection vulnerabilities  
- Pinned GitHub Actions to specific versions for security
- Removed hardcoded password from UnisonHybridClient/Program.cs
- Fixed exception handling and bare except clauses
- Removed Unicode BOM from launchSettings.json
- Added Codacy quality analysis configuration"
    
    git commit -m $commitMessage
    
    # Push to origin
    git push origin $branch
    
    Write-Host "  ✓ Applied security fixes to $branch" -ForegroundColor Green
}

Write-Host "Security fixes applied to all branches!" -ForegroundColor Green
