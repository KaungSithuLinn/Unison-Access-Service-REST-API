---
description: AI rules derived by SpecStory from the project AI interaction history
globs: *
---

## AI Coding Assistant Rules File

### <headers/>

This file defines all project rules, coding standards, workflow guidelines, references, documentation structures, and best practices for the AI coding assistant. It is a living document that evolves with the project.

### TECH STACK

*   **MCPs / extensions**: Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory, Web-Search-for-Copilot, MarkItDown, SQL-Server(mssql), Codacy, Firecrawl, Postman, Terraform, GitHub, HashiCorp Terraform MCP, GitHub MCP, GitHub Pull Requests & Issues extension
*   **Built-ins**: workspace file search & read, `specify` CLI, `git`, `gh`, `terraform`, `codacy-cli`, `curl`

### PROJECT DOCUMENTATION & CONTEXT SYSTEM

The `specs/latest/spec.md` file contains the final specification with all enhancements. It should be consulted to confirm the project's current state. `chat_summary.md` provides a structured summary of recent activity. Memory is stored using the Memory MCP, with phase markers indicating the current stage of the project.

### WORKFLOW & RELEASE RULES

1.  **Summarise & Sync (Mandatory Entry Step)**: Before starting a new phase, always summarize the current chat, update the Spec-Kit files, commit the changes, and store a snapshot in memory.
    *   Use `summarise_current_chat` to create `chat_summary.md`.
    *   Use `update_spec_kit_files` to merge `chat_summary.md` into `specs/latest/spec.md` & `tasks.json`.
    *   Use `git commit_push` with message `"spec: sync <feature> <timestamp>"`. Examples: `"spec: sync security-validation <timestamp>"`, `"spec: sync branch-protection <timestamp>"`, `"spec: sync status-checks-integration <timestamp>"`, `"spec: sync test-verify-status-checks"`, `"spec: sync soap-rest-clarification"`, `"spec: sync soap-rest-clarification <timestamp>"`.
    *   Use `Memory MCP` to `store_snapshot` with `{"source":"chat_summary.md","phase":"<current_phase>"}`.

2.  **Deployment Preparation (Phase 3)**: Involves PR review & merge, infrastructure setup, final testing, and documentation.

3.  **PR Review & Merge**: Use the GitHub MCP to review, approve, and merge all PRs, and close corresponding GitHub issues. Example commands:
    *   `gh pr review <pr_number> --approve`
    *   `gh pr merge <pr_number> --squash --delete-branch`
    *   **Important:** Before merging, ensure the PR's base branch is `main` and resolve any merge conflicts.

4.  **Infrastructure Setup**: Use the Terraform MCP to prepare production deployment configuration. Example command:
    *   `terraform apply -auto-approve`

5.  **Final Testing**: Use the Playwright MCP to execute the full integration test suite. Example command:
    *   `npx playwright test --config=playwright.production.config.ts`

6.  **Documentation**: Use the MarkItDown MCP to create a deployment guide and API documentation. Example command:
    *   `generate-deployment-guide --output docs/deployment.md`

### DEBUGGING

When encountering merge conflicts during PR review, switch to each feature branch and update them manually by rebasing onto the `main` branch. If the GitHub CLI (`gh`) is not recognized, ensure it is installed correctly and that the terminal or VS Code has been restarted to update the PATH. The `gh --version` command should execute successfully. **If the `gh` command is still not recognized after installation and restart, ensure the GitHub CLI is authenticated by running `gh auth login` and following the prompts.**

### CODING STANDARDS

(No coding standards defined yet)

### SECURITY

Security is a top priority. All branches must be hardened before deployment. When creating GitHub Actions workflows:
* Pin actions to commit SHA to prevent supply chain attacks
* Avoid shell injection vulnerabilities by sanitizing GitHub context data used in run steps.

To protect the `main` branch and enforce best practices:

1.  **Branch Protection Rule (Recommended Minimum)**: In your repository settings on GitHub, add a branch protection rule for the `main` branch.
    *   Enable:
        *   **Require a pull request before merging**
        *   **Require status checks to pass before merging** (add your CI checks)
            *   Example status checks: `codacy/pr-check`, `ci/application`, `ci/infrastructure`
            *   Enable “Require branches to be up to date before merging”.
        *   **Require review from at least 1 reviewer**
        *   **Require linear history**
        *   **Include administrators** (optional but recommended)
        *   **Restrict who can push to matching branches** (optional for extra security)
        *   **Prevent force pushes**
        *   **Prevent branch deletion**

2.  **(Optional) Rulesets for Advanced Controls**: Use rulesets for more advanced controls.
    *   Go to **Settings** > **Rulesets**.
    *   Click **New ruleset**.
    *   Target the `main` branch.
    *   Add rules for:
        *   Commit signature requirements
        *   Commit message patterns
        *   File/path restrictions
        *   Push restrictions (file size, extensions, etc.)
    *   Set enforcement to **Active**.

   * **Recommended Ruleset Configuration:**
        * **Ruleset Name:** `main`
        * **Target branches:** Set the branch targeting pattern to: `main`
        * **Enforcement status:** Set to **Active** (enforced for all users)
        * **Bypass list:** Leave empty unless you have a specific reason to allow certain roles/teams/apps to bypass protection (not recommended for production).
        *   **Restrict creations/updates:** *Leave unchecked* (unless you want to lock down branch creation/updates to only bypass users)
        *   **Restrict deletions:** ✅ Enabled (prevents branch deletion)
        *   **Require linear history:** ✅ Enabled (prevents merge commits)
        *   **Require deployments to succeed:** (Optional, only if you use GitHub Environments)
        *   **Require signed commits:** ✅ Enabled (all commits must be signed)
        *   **Require a pull request before merging:** ✅ Enabled
            *   **Required approvals:** Set to at least 1
            *   **Dismiss stale approvals:** ✅ Enabled
            *   **Require review from Code Owners:** (Optional, enable if you have a CODEOWNERS file)
            *   **Require approval of most recent push:** ✅ Enabled
            *   **Require conversation resolution:** (Optional, but recommended)
        *   **Allowed merge methods:** At least one enabled (squash, rebase, or merge)
        *   **Require status checks to pass:** ✅ Enabled
            *   **Require branches to be up to date before merging:** ✅ Enabled
            *   **You must add at least one status check** (e.g., `codacy/pr-check`, `ci/application`, or `ci/infrastructure`)
        *   **Block force pushes:** ✅ Enabled
        *   **Require code scanning results:** (Optional, enable if you use CodeQL or other tools)
        *   **Automatically request Copilot code review:** (Optional)

### HAND-OVER PROTOCOL

Before finishing, **always**:

a. Re-run Step 0.1-0.4 (summarise, update Spec-Kit files, commit, memory store).
b. Print a markdown block with:
   - Phase completed
   - Artifacts changed (paths + hashes)
   - Next agent's entry command
   - Remaining risks / blockers

### REPOSITORY CONSOLIDATION (OPTIONAL)

1. **Compare Repositories**:
   - Use **GitHub MCP** to compare original and canonical repositories
   - Command: `git -C "C:\Projects\Unison Access Service REST API" log --oneline --graph --all --max-count=20`

2. **Migrate Unique Changes**:
   - Use **GitHub Pull Requests & Issues extension** to cherry-pick any unique commits
   - Create PRs for any valuable changes from original repo

3. **Clean Up**:
   - Archive or delete original parent folder repo to avoid confusion

### TEMPLATE COMMANDS/SNIPPETS:

*   GitHub MCP: `gh pr list` (verify all PRs exist)
*   GitHub MCP: `gh pr merge <pr_number> --squash --delete-branch` (merge each PR)
*   GitHub MCP: `gh api repos/OWNER/REPO/branches/main/protection -X PUT -f ...` (basic protection)
*   GitHub MCP: `gh api repos/OWNER/REPO/rulesets -X POST -f ...` (advanced rulesets)
*   Codacy MCP: `codacy-cli enable-github-status-checks`
*   MarkItDown MCP: `generate-docs --type branch-protection --output docs/security/branch-protection.md`
*   Terraform MCP: `terraform apply -auto-approve`
*   Playwright MCP: `npx playwright test --config=playwright.production.config.ts`
*   MarkItDown MCP: `generate-deployment-guide --output docs/deployment.md`
*   GitHub MCP: `gh pr create --title "Test CI Trigger" --body "Triggering initial CI run" --head test-branch --base main`
*   GitHub MCP: `gh api repos/OWNER/REPO/branches/main/protection` (verify status checks)
*   GitHub Pull Requests & Issues extension: `git add . && git commit -m "chore: commit pending changes for test"`
*   GitHub Pull Requests & Issues extension: `gh pr create --title "Test Status Checks Integration" --body "Verify status checks block merge until passed" --head test-branch --base main`
*   GitHub Pull Requests & Issues extension: `gh pr close <number> && git push origin --delete test-status-verify`

### SECURITY - IMPLEMENTATION DETAILS

When creating security documentation, ensure the following guidelines are adhered to:
*   **Markdown Documentation Issues**:
    *   Bare URLs should be wrapped in angle brackets (`<...>`) to comply with markdown linting rules.
    *   Code blocks should have blank lines before/after the fence and a language specifier (e.g., ```bash or ```yaml).
*   **PowerShell Script Issue**: Validate variable references to avoid errors like "Variable reference is not valid. ':' was not followed by a valid variable name character. Consider using ${} to delimit the name."

### TROUBLESHOOTING & DEBUGGING

When interacting with external services, it is crucial to accurately characterize the service type (e.g., REST vs. SOAP).

1.  **Service Type Determination:**
    *   Analyze the service's WSDL (if available) for binding types. `basicHttpBinding` and `mexHttpBinding` indicate a SOAP 1.1 web service, not a REST API.
    *   **Direct Testing (Mandatory):** Attempt to call the service directly using `curl` with different content types.
        *   **REST API Test:**
            ```bash
            curl -X POST "<service_endpoint>" \
              -H "Content-Type: application/json" \
              -d '{"cardId":"12345"}'
            ```
            *   *Expected Result:* A true REST service should return a JSON error message, even if the request is invalid.
        *   **SOAP Service Test:**
            ```bash
            curl -X POST "<service_endpoint>" \
              -H "Content-Type: text/xml; charset=utf-8" \
              -H "SOAPAction: <soap_action_from_wsdl>" \
              -d '<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap:Body>
                      <MethodName xmlns="http://tempuri.org/">
                        <parameter1>value1</parameter1>
                      </MethodName>
                    </soap:Body>
                  </soap:Envelope>'
            ```
            *   *Expected Result:* A successful call (or a meaningful SOAP fault) confirms the service expects SOAP-XML.

2.  **Error Interpretation:**
    *   HTTP 415 Unsupported Media Type, HTTP 500 Internal Server Error with SOAP Fault XML, or HTTP 400 Bad Request responses from a JSON request indicate a SOAP service.

3.  **Communication:**
    *   When communicating with stakeholders, clearly explain the difference between using HTTP as a transport protocol and adhering to the REST architectural style. A service can use HTTP without being a REST API.

### ADAPTERS & TRANSLATIONS

When integrating with SOAP services, use a REST-to-SOAP adapter to provide a modern REST API for client applications.

1.  **Adapter Role:** The adapter translates simple REST/JSON requests into the complex SOAP/XML format required by the backend service.

2.  **Workflow:**
    *   Frontend/Client teams call the REST adapter endpoint with JSON.
    *   The adapter handles the translation to SOAP and calls the real Unison service.
    *   The Unison service receives the SOAP request it expects.

### TROUBLESHOOTING & DEBUGGING - SOAP vs REST

When encountering issues with external services, and there is a possibility of confusion regarding whether the service is RESTful or SOAP-based, **always**:

1.  **Run a REST API Test:** Attempt to call the service using `curl` with `Content-Type: application/json` and a simple JSON payload. Analyze the response. A true REST service should return a JSON error message, even if the request is invalid.
2.  **Run a SOAP Service Test:** If the REST test fails, attempt to call the service using `curl` with `Content-Type: text/xml; charset=utf-8`, a valid SOAPAction header (obtained from the WSDL), and a valid SOAP envelope. A successful call (or a meaningful SOAP fault) confirms the service expects SOAP-XML.
3.  **Document the Evidence:** Save both `curl` commands and their outputs in a short markdown file (e.g., `docs/soap-vs-rest-evidence.md`). Clearly label each result: “REST-style call (fails)” and “SOAP call (succeeds)”.

### COMMUNICATION GUIDELINES

When communicating with stakeholders about service types (REST vs. SOAP), **always**:

*   Clearly explain the difference between using HTTP as a transport protocol and adhering to the REST architectural style. A service can use HTTP without being a REST API.
*   Reiterate that a REST-to-SOAP adapter is the correct and necessary solution if the backend service is SOAP-based and the frontend requires a RESTful interface.
*   Provide concrete evidence (e.g., the `curl` command outputs) to support your explanation.

### WORKFLOW & RELEASE RULES - CLARIFICATION PHASE

When the project is in the "Clarification" phase, relating to SOAP/REST clarification, the following steps and considerations apply:

1. **Mandatory Entry Step**: Before starting, always execute the "Mandatory Entry Step – Summarise & Sync" as outlined in the project rules. This includes:
    * Summarizing the current chat and updating `chat_summary.md`.
    * Merging the summary into `specs/latest/spec.md` and `tasks.json`.
    * Committing and pushing with the message: `"spec: sync soap-rest-clarification"`.
    * Storing a snapshot in memory with phase marker `"Clarification"`.

2. **Validation Tests**:
    * Use `curl` or **Postman** to send a JSON payload to the service endpoint (e.g., `http://192.168.10.206:9003/Unison.AccessService`). Expect a failure response (e.g., HTTP 415/400).
    * Use `curl` or **Postman** to send a valid SOAP envelope with the correct SOAPAction header. Expect a successful response or a meaningful SOAP fault.
    * Capture both commands and responses in `docs/soap-vs-rest-evidence.md` using **MarkItDown**.

3. **Documentation Updates**:
    * Add a service architecture explanation to `docs/architecture.md` using **MarkItDown**.
    * Clarify the REST adapter's purpose in `README.md` using **MarkItDown**.

4. **Stakeholder Communication**:
    * Draft a message to stakeholders (e.g., Minh) with a summary of the findings, including the evidence gathered. Use **Microsoft-Docs** for drafting the message.
    * Optionally, schedule an alignment call and prepare notes using **Microsoft-Docs**.

5. **Handover Preparation**:
    * Re-run Mandatory Entry Step (0.1-0.4) to sync the latest state.
    * Generate a handover report.

6. **Startup Checklist**: Ensure the following before commencing work:
   * Pull latest: `git pull origin main`
   * Read `chat_summary.md` (root) to confirm the SOAP service nature and communication plan.
   * Read `specs/latest/spec.md` to verify the adapter implementation is correct.
   * Continue from the phase marker inside `memory/current_phase.json` (should be "Clarification").

7. **Handover Report**: Before finishing, **ALWAYS**:
    * Re-run Step 0.1-0.4 (summarise, update Spec-Kit files, commit & push with GitHub Pull Requests & Issues extension, memory store).
    * Print a markdown block with:
        * Phase completed
        * Artifacts changed (paths + hashes)
        * Next agent’s entry command
        * Remaining risks / blockers