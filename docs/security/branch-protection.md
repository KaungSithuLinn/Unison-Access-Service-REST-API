# Branch Protection Configuration Guide

## Overview

This document provides comprehensive guidance for implementing branch protection rules for the Unison Access Service REST API repository. The main branch is currently **UNPROTECTED** and requires immediate security configuration to prevent unauthorized changes and enforce quality standards.

## Current Security Risk

❌ **CRITICAL SECURITY GAP**: The main branch currently allows:

- Direct pushes without pull requests
- Force pushes that can rewrite history
- Deletion of the main branch
- Commits without review or validation

## Required Protection Rules

### 1. Basic Branch Protection Rules

The following basic protection rules must be implemented immediately:

#### Required Pull Request Reviews

- **Require pull request reviews before merging**: ✅ Enabled
- **Required number of reviewers**: 1 minimum
- **Dismiss stale pull request approvals when new commits are pushed**: ✅ Enabled
- **Require review from code owners**: ⚠️ Optional (requires CODEOWNERS file)

#### Status Check Requirements

- **Require status checks to pass before merging**: ✅ Enabled
- **Require branches to be up to date before merging**: ✅ Enabled
- **Required status checks**:
  - `codacy/pr-check` (Code quality validation)
  - `ci/application` (Application CI pipeline)
  - `ci/infrastructure` (Infrastructure validation)

#### Additional Protections

- **Require linear history**: ✅ Enabled (prevents merge commits)
- **Include administrators**: ✅ Enabled (applies rules to admins too)
- **Allow force pushes**: ❌ Disabled
- **Allow deletions**: ❌ Disabled

### 2. Advanced Repository Rulesets

For enhanced security, implement repository rulesets with the following features:

#### Commit Requirements

- **Require signed commits**: ✅ Enabled
- **Commit message patterns**: Optional
- **File path restrictions**: Optional

#### Pull Request Requirements

- **Required approving review count**: 1
- **Dismiss stale reviews**: ✅ Enabled
- **Require last push approval**: ✅ Enabled
- **Block creations**: ❌ Disabled (allow branch creation)

## Implementation Methods

### Method 1: GitHub Web Interface (Recommended for Initial Setup)

1. **Navigate to Repository Settings**:

   - Go to https://github.com/KaungSithuLinn/Unison-Access-Service-REST-API
   - Click "Settings" tab
   - Select "Branches" from the left sidebar

2. **Add Branch Protection Rule**:

   - Click "Add rule"
   - Branch name pattern: `main`
   - Configure all required settings listed above

3. **Verify Protection**:
   - Check that all rules are applied correctly
   - Test with a test pull request

### Method 2: GitHub CLI (When Available)

```bash
# Install GitHub CLI first
# Configure branch protection
gh api repos/KaungSithuLinn/Unison-Access-Service-REST-API/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["codacy/pr-check","ci/application","ci/infrastructure"]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true}' \
  --field restrictions=null \
  --field required_linear_history=true \
  --field allow_force_pushes=false \
  --field allow_deletions=false
```

### Method 3: REST API with PowerShell

Create and run the PowerShell script provided in this directory:

```powershell
# See scripts/Configure-BranchProtection.ps1
.\scripts\Configure-BranchProtection.ps1 -Owner "KaungSithuLinn" -Repo "Unison-Access-Service-REST-API" -Token $env:GITHUB_TOKEN
```

### Method 4: Repository Rulesets (Advanced)

```bash
# Create repository ruleset for enhanced protection
gh api repos/KaungSithuLinn/Unison-Access-Service-REST-API/rulesets \
  --method POST \
  --field name="Production Protection" \
  --field target="branch" \
  --field enforcement="active" \
  --field conditions='{"ref_name":{"include":["main"]}}' \
  --field rules='[
    {"type":"required_signatures","parameters":{}},
    {"type":"pull_request","parameters":{"required_approving_review_count":1,"dismiss_stale_reviews":true}},
    {"type":"required_status_checks","parameters":{"required_status_checks":[{"context":"codacy/pr-check"},{"context":"ci/application"}]}}
  ]'
```

## Status Check Integration

### Codacy Integration

To enable Codacy status checks as required status checks:

1. **Enable Codacy Status Checks**:

   ```bash
   # Using Codacy CLI
   codacy-cli enable-github-status-checks --provider gh --organization KaungSithuLinn --repository Unison-Access-Service-REST-API
   ```

2. **Configure Codacy Analysis**:
   - Ensure `.codacy/codacy.yaml` is properly configured
   - Verify analysis runs on all pull requests
   - Add `codacy/pr-check` to required status checks

### GitHub Actions Integration

Ensure the following workflows are configured to run as required status checks:

1. **Application CI** (`.github/workflows/application.yml`):

   - Build and test application
   - Security scanning
   - Code quality validation

2. **Infrastructure CI** (`.github/workflows/infrastructure.yml`):
   - Terraform validation
   - Infrastructure security scanning
   - Deployment readiness checks

## Verification Steps

After implementing branch protection, verify the following:

### 1. Direct Push Prevention

```bash
# This should be rejected
git push origin main
```

### 2. Force Push Prevention

```bash
# This should be rejected
git push --force origin main
```

### 3. Pull Request Requirement

- Create a test branch
- Make changes and push
- Verify that merge requires pull request
- Verify that status checks are required

### 4. Status Check Enforcement

- Create a pull request
- Verify that status checks run
- Verify that merge is blocked until all checks pass

---

### ✅ Status Check Enforcement Verified (2025-09-12)

A test pull request ([#15](https://github.com/KaungSithuLinn/Unison-Access-Service-REST-API/pull/15)) was created from `test-status-verify` to `main` to verify branch protection enforcement. The following was observed:

- Both required status checks (`ci/application` and `ci/infrastructure`) appeared on the PR.
- The PR could not be merged until both checks passed.
- Branch protection is confirmed to be working as intended for production security.

The test PR and branch were deleted after verification. This section serves as an audit record of the enforcement test.

## Monitoring and Maintenance

### Regular Reviews

- **Monthly**: Review protection rules for effectiveness
- **Quarterly**: Audit bypass permissions and rule exceptions
- **After incidents**: Assess if additional protections are needed

### Alerting

- Configure notifications for:
  - Branch protection rule changes
  - Failed status checks
  - Force push attempts (should be blocked)
  - Rule bypass usage

### Documentation Updates

- Keep this document updated with any rule changes
- Document any approved exceptions or bypasses
- Maintain change log of protection rule modifications

## Emergency Procedures

### Temporary Rule Bypass

In case of emergency requiring rule bypass:

1. **Document the emergency**: Create issue describing the situation
2. **Temporary disable**: Disable specific rules temporarily
3. **Implement change**: Make necessary emergency changes
4. **Re-enable protection**: Immediately re-enable all rules
5. **Post-incident review**: Analyze what happened and improve processes

### Recovery from Unauthorized Changes

If the main branch is compromised:

1. **Identify the issue**: Determine what unauthorized changes were made
2. **Create recovery branch**: Create a clean branch from the last known good state
3. **Review changes**: Carefully review all commits since the last known good state
4. **Selective merge**: Cherry-pick only the authorized changes
5. **Force push recovery**: Use force push to restore the main branch (requires temporary rule disable)
6. **Incident report**: Document the incident and improve protection rules

## Related Documentation

- [GitHub Actions Security Guide](../ci-cd/github-actions-security.md)
- [Codacy Configuration Guide](../quality/codacy-configuration.md)
- [Emergency Response Procedures](../operations/emergency-response.md)

## Change Log

| Date       | Version | Changes          | Author       |
| ---------- | ------- | ---------------- | ------------ |
| 2025-09-12 | 1.0     | Initial creation | AI Assistant |

---

**⚠️ IMPORTANT**: This configuration is critical for production security. Any changes to branch protection rules must be reviewed and approved through the established change management process.
