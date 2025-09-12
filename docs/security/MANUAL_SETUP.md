# Manual Branch Protection Setup Guide

Since GitHub CLI is not available and a GitHub token needs to be configured, follow these manual steps to implement comprehensive branch protection for the main branch.

## Step 1: Access Repository Settings

1. Navigate to the repository: https://github.com/KaungSithuLinn/Unison-Access-Service-REST-API
2. Click on the **Settings** tab (requires admin access)
3. In the left sidebar, click **Branches**

## Step 2: Create Branch Protection Rule

1. Click **Add rule** button
2. In **Branch name pattern**, enter: `main`

## Step 3: Configure Protection Settings

### Required Status Checks

☑️ **Require status checks to pass before merging**

- ☑️ Check "Require branches to be up to date before merging"
- In the search box, add these status checks (they will appear after the first CI run):
  - `codacy/pr-check`
  - `ci/application`
  - `ci/infrastructure`

### Pull Request Reviews

☑️ **Require pull request reviews before merging**

- **Required number of approving reviews**: `1`
- ☑️ Check "Dismiss stale pull request approvals when new commits are pushed"
- ☑️ Check "Require approval of the most recent reviewable push"
- ☐ Leave "Require review from code owners" unchecked (optional, requires CODEOWNERS file)

### Additional Protection Rules

☑️ **Require linear history**
☑️ **Include administrators** (applies rules to repository admins)

### Restrictions

☐ **Restrict pushes that create files** (leave unchecked)
☐ **Restrict pushes that update files** (leave unchecked)  
☐ **Do not allow bypassing the above settings** (leave unchecked for now)

## Step 4: Advanced Settings

☑️ **Allow force pushes** - **UNCHECK THIS** (force pushes should be blocked)
☑️ **Allow deletions** - **UNCHECK THIS** (branch deletion should be blocked)

## Step 5: Save Configuration

1. Click **Create** to save the branch protection rule
2. You should see the new rule listed with a green checkmark

## Step 6: Verify Protection is Active

### Test 1: Direct Push (Should be Blocked)

```bash
# This should fail with protection error
git push origin main
```

Expected result: `remote: error: GH006: Protected branch update failed`

### Test 2: Force Push (Should be Blocked)

```bash
# This should fail
git push --force origin main
```

Expected result: `remote: error: GH006: Protected branch update failed`

### Test 3: Branch Deletion (Should be Blocked)

```bash
# This should fail
git push origin --delete main
```

Expected result: `remote: error: GH006: Protected branch update failed`

## Step 7: Configure Codacy Status Checks

To ensure Codacy status checks are properly integrated:

1. **Enable Codacy Status Checks**:

   - Codacy is already configured for this repository (Grade A)
   - Status checks should automatically appear in PRs
   - If not visible, check Codacy project settings

2. **Verify Integration**:
   - Create a test branch and pull request
   - Verify `codacy/pr-check` appears in the status checks
   - Verify PR cannot be merged until checks pass

## Step 8: Set Up GitHub Actions (If Not Already Present)

The protection requires these status checks to exist:

- `ci/application`
- `ci/infrastructure`

If these workflows don't exist yet:

1. Create `.github/workflows/application.yml`:

   ```yaml
   name: Application CI
   on: [push, pull_request]
   jobs:
     test:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - name: Application Tests
           run: echo "Application CI placeholder"
   ```

2. Create `.github/workflows/infrastructure.yml`:
   ```yaml
   name: Infrastructure CI
   on: [push, pull_request]
   jobs:
     validate:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - name: Infrastructure Validation
           run: echo "Infrastructure CI placeholder"
   ```

## Troubleshooting

### Issue: Status Checks Not Available

**Problem**: Required status checks don't appear in the dropdown.

**Solution**:

1. The status checks must exist first (CI workflows must run)
2. Create and run the workflows above
3. Return to branch protection settings and add the status checks

### Issue: Cannot Create Protection Rule

**Problem**: "You need admin access to create protection rules"

**Solution**:

1. Verify you have admin access to the repository
2. Contact the repository owner to grant admin permissions
3. Alternatively, ask an admin to implement these settings

### Issue: Codacy Status Check Missing

**Problem**: `codacy/pr-check` doesn't appear in pull requests.

**Solution**:

1. Verify Codacy is properly configured for the repository
2. Check that `.codacy/codacy.yaml` exists and is valid
3. Ensure Codacy has access to the repository
4. Create a test PR to trigger Codacy analysis

## Verification Checklist

After completing the setup, verify these items:

- [ ] Main branch shows "Protected" badge in GitHub
- [ ] Direct pushes to main are blocked
- [ ] Force pushes to main are blocked
- [ ] Branch deletion is blocked
- [ ] Pull requests show required status checks
- [ ] Pull requests require at least 1 review
- [ ] Stale reviews are dismissed on new pushes
- [ ] Linear history is enforced (no merge commits)
- [ ] Protection rules apply to administrators

## Security Impact

✅ **Protection Enabled**: The main branch is now secured against:

- Unauthorized direct changes
- Unreviewed code merges
- Quality gate bypasses
- History tampering
- Accidental deletion

⚠️ **Important Notes**:

- Emergency bypasses may require temporarily disabling protection
- All changes to protection rules should be logged and reviewed
- Regular audits of protection effectiveness are recommended

## Next Steps

1. **Test Protection**: Create a test PR to verify all checks work correctly
2. **Document Exceptions**: Create procedures for emergency rule bypasses
3. **Monitor Compliance**: Set up notifications for protection rule changes
4. **Team Training**: Ensure all team members understand the new workflow

---

**Last Updated**: 2025-09-12  
**Configuration Status**: ⚠️ Pending Manual Implementation  
**Priority**: 🔴 CRITICAL - Implement Immediately
