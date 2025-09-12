# Branch Protection Implementation

## Quick Start

To implement comprehensive branch protection for the main branch, follow these steps:

### Prerequisites

1. **GitHub Personal Access Token**: Required with `repo` permissions
2. **PowerShell**: Windows PowerShell or PowerShell Core
3. **Repository Access**: Admin access to KaungSithuLinn/Unison-Access-Service-REST-API

### Set Environment Variable

```powershell
# Set your GitHub token (replace with your actual token)
$env:GITHUB_TOKEN = "your_github_token_here"
```

### Run the Configuration Script

```powershell
# Navigate to the security scripts directory
cd "docs\security\scripts"

# Test the configuration (dry run)
.\Configure-BranchProtection.ps1 -Owner "KaungSithuLinn" -Repo "Unison-Access-Service-REST-API" -WhatIf

# Apply the configuration
.\Configure-BranchProtection.ps1 -Owner "KaungSithuLinn" -Repo "Unison-Access-Service-REST-API"
```

## Configuration Details

The script will apply the following protection rules to the `main` branch:

### ✅ Required Status Checks

- `codacy/pr-check` (Code quality validation)
- `ci/application` (Application CI pipeline)
- `ci/infrastructure` (Infrastructure validation)
- Branches must be up to date before merging

### ✅ Pull Request Reviews

- Minimum 1 approving review required
- Dismiss stale reviews when new commits are pushed
- Require approval of the most recent reviewable push

### ✅ Admin Enforcement

- Rules apply to administrators
- No bypass permissions for admins

### ✅ History Protection

- Require linear history (no merge commits)
- Block force pushes
- Block branch deletion

## Verification

After running the script, verify the protection rules are active:

1. **GitHub Web Interface**:

   - Go to: https://github.com/KaungSithuLinn/Unison-Access-Service-REST-API/settings/branches
   - Verify all rules are listed under "Branch protection rules"

2. **Test Direct Push** (should be blocked):

   ```bash
   git push origin main
   # Should return: "Required status check is expected but missing: codacy/pr-check"
   ```

3. **Test Force Push** (should be blocked):
   ```bash
   git push --force origin main
   # Should return: "denied: force push not allowed on protected branch"
   ```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**:

   - Check that your GitHub token has `repo` permissions
   - Verify the token is set correctly: `$env:GITHUB_TOKEN`

2. **403 Forbidden**:

   - Ensure you have admin access to the repository
   - Verify the repository name and owner are correct

3. **422 Validation Failed**:
   - Status checks may not exist yet (CI workflows need to run first)
   - Consider starting with basic protection and adding status checks later

### Manual Fallback

If the script fails, you can configure branch protection manually:

1. Go to: https://github.com/KaungSithuLinn/Unison-Access-Service-REST-API/settings/branches
2. Click "Add rule"
3. Branch name pattern: `main`
4. Configure settings as described above

## Security Notes

⚠️ **IMPORTANT**: This configuration is critical for production security. Any changes to branch protection rules must be reviewed and approved through the established change management process.

### What This Protects Against

- **Unauthorized Direct Pushes**: Forces all changes through pull requests
- **Unreviewed Changes**: Requires at least one approval before merge
- **Low-Quality Code**: Blocks merges until Codacy analysis passes
- **CI Failures**: Prevents merging broken builds
- **History Tampering**: Blocks force pushes and maintains linear history
- **Accidental Deletion**: Prevents deletion of the main branch

### What This Does NOT Protect Against

- **Malicious Reviewers**: A compromised admin can still approve malicious changes
- **Token Compromise**: A stolen admin token can modify protection rules
- **Supply Chain Attacks**: Malicious dependencies in CI/CD pipelines

## Next Steps

After implementing branch protection:

1. **Test the Protection**: Create a test PR to verify all checks work
2. **Document Exceptions**: Create procedures for emergency bypasses
3. **Monitor Compliance**: Set up alerts for protection rule changes
4. **Regular Reviews**: Quarterly review of protection effectiveness

See [branch-protection.md](../branch-protection.md) for complete documentation.
