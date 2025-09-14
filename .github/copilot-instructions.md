---
description: AI rules derived by SpecStory from the project AI interaction history
globs: *
---

## AI Coding Assistant Rules File

### <headers/>

This file defines all project rules, coding standards, workflow guidelines, references, documentation structures, and best practices for the AI coding assistant. It is a living document that evolves with the project.

### TECH STACK

*   **MCPs / extensions**: Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory, Web-Search-for-Copilot, MarkItDown, SQL-Server(mssql), Codacy, Firecrawl, Postman, Terraform, GitHub, HashiCorp Terraform MCP, GitHub MCP, GitHub Pull Requests & Issues extension, GitHub Copilot Chat.
*   **Built-ins**: workspace file search & read, `specify` CLI, `git`, `gh`, `terraform`, `codacy-cli`, `curl`, `lighthouse-ci`, `axe-cli`, `npm/pnpm`, `docker`.

### PROJECT DOCUMENTATION & CONTEXT SYSTEM

The `specs/latest/spec.md` file contains the final specification with all enhancements. It should be consulted to confirm the project's current state. `chat_summary.md` provides a structured summary of recent activity. Memory is stored using the Memory MCP, with phase markers indicating the current stage of the project. `architecture_decisions.md` stores architecture decisions in ADR format. `AI_ENHANCED_NEXT_STEPS_OUTLINE.md` contains the complete 5-phase AI-enhanced workflow documentation. `ZERO_CONTEXT_AGENT_PROMPT.md` contains the agent handover instructions and startup checklist. `PHASE_1_HANDOVER_REPORT.md` contains the comprehensive handover documentation for Phase 1.

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
    * Committing and pushing with the message: `"spec: sync soap-rest-clarification"` or `"spec: sync soap-rest-clarification <timestamp>"`.
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

### WORKFLOW & RELEASE RULES - AI-ENHANCED SPECIFICATION FRAMEWORK

The following describes the AI-enhanced specification framework, including a mandatory entry step and phased execution with AI observability.

#### Global Tool-Set

*   See TECH STACK for list of tools.

#### Mandatory Entry Step – Summarise & Sync

| # | Tool / Built-in | Capability | Input | Output | Next |
|---|-----------------|------------|-------|--------|------|
| 0.1 | built-in + GitHub Copilot Chat | `ai_enhanced_summarise_current_chat` | Full transcript + prior chat_summary.md → AI-extracted insights (decisions, debt, bottlenecks) | `chat_summary.md` with structured sections (e.g., AI_Insights, Tech_Debt) | 0.2 |
| 0.2 | GitHub Copilot Chat + specify CLI | `ai_review_and_update_spec` | Current `specs/latest/spec.md` → AI-suggested enhancements (edge cases, accessibility) | Updated `spec.md`, `tasks.json`, `architecture_decisions.md` (ADR format) | 0.3 |
| 0.3 | built-in | `merge_artifacts` | AI outputs into Spec-Kit files | Synced Spec-Kit artifacts with versioned ADRs | 0.4 |
| 0.4 | GitHub Pull Requests & Issues extension | `commit & push` | Message = `"feat(spec): ai-enhanced framework sync - $(date '+%Y-%m-%d-%H-%M')"` | Remote branch updated via PR | 0.5 |
| 0.5 | Memory MCP | `store_enhanced_snapshot` | `{"source":"spec.md","phase":"Hand-off","ai_insights":["adapter scalability","perf budgets"],"tech_debt":["WSDL caching"]}` | Memory updated with AI context | Phase 1 |

#### Phase 1 – AI-Enhanced Specification (Gate ①: Requirements Lock)

1. **AI User Story Generation**:
   - Use GitHub Copilot Chat to expand WSDL ops (e.g., Ping, SyncUsers) into user stories with acceptance criteria, including accessibility (axe-cli checks) and performance (Lighthouse budgets).
   - Input: `docs/architecture.md` + WSDL from temp_wsdl.xml.
   - Output: Updated `specs/latest/user-stories.md`; validate with Playwright-generated scenarios (>80% coverage).

2. **Schema & Contract Refinement**:
   - Run `specify CLI` with AI prompts for type-safe REST schemas mirroring SOAP envelopes.
   - Test via Postman collections for adapter endpoints.
   - Document edge cases (e.g., fault handling) in `architecture_decisions.md`.

3. **Metrics Baseline**:
   - Define Core Web Vitals targets (LCP <2.5s) and security reqs (OWASP top 10 coverage).
   - Run initial axe-cli on mockups.

#### Phase 2 – Intelligent Planning (Gate ②: Architecture Freeze)

1. **IaC Generation**:
   - Use Terraform MCP + Copilot Chat for optimized modules (e.g., adapter deployment with auto-scaling).
   - AI prompt: "Optimize for cost/security in REST-SOAP proxy."
   - Validate: `terraform validate` + security scan.

2. **Component Design**:
   - AI-suggest patterns (e.g., circuit breaker for SOAP calls) via Sequential-Thinking.
   - Build dependency graph; flag circular deps.

3. **Threat Modeling**:
   - AI-assisted (Copilot Chat) for adapter risks (e.g., XML injection); document in ADR.

#### Phase 3 – Smart Task Generation (Gate ③: Task Backlog Ready)

1. **Issue Creation**:
   - GitHub Extension + AI: Generate issues for adapter impl (e.g., "Implement Ping endpoint") with DoD, estimates.
   - Prioritize via tech debt from Memory MCP.

2. **Dependency Mapping**:
   - Create graph in `tasks.json`; optimize order for parallelism.

3. **CI/CD Mapping**:
   - Link to pipelines with feature flags for SOAP integration.

#### Phase 4 – AI-Assisted Implementation (Gate ④: Code Complete)

1. **Task Fetch & Code Gen**:
   - Pull top issue; use Copilot Chat for TypeScript impl (e.g., SOAP client lib).
   - Standards: ESLint, Prettier; include unit tests.

2. **Testing Suite**:
   - Playwright E2E + Postman for adapter; aim 90% coverage.
   - Run lighthouse-ci, axe-cli.

3. **PR Creation**:
   - Auto-generate desc; link issues, request reviews.

#### Phase 5 – Intelligent Review & Deploy (Gate ⑤: Production Ready)

1. **Quality Gates**:
   - Codacy + AI review; fix suggestions.

2. **Merge & Deploy**:
   - Squash commit; Terraform apply with monitoring (e.g., Azure App Insights).

3. **Observability**:
   - Setup alerts for adapter latency.

#### Handover Protocol (AI-Enhanced Framework)

Before finishing, **always**:

a. Re-run Mandatory Entry Step 0.1-0.5 (AI summarize, spec update, commit & PR via GitHub Extension, enhanced memory store).
b. Print a markdown block with:
   - Phase completed
   - Artifacts changed (paths + SHAs/metrics)
   - Next agent’s entry command
   - Remaining risks / blockers (e.g., tech debt items)
   - Quality Metrics (coverage %, perf score, etc.)

### START-UP CHECKLIST (AI-Enhanced Framework)

1. Pull latest: `git pull origin main --rebase`
2. Read `chat_summary.md` (root) → confirms AI framework adoption and clarification phase completion
3. Read `specs/latest/spec.md` → verify AI-enhanced user stories and schemas
4. Read `architecture_decisions.md` → load ADRs for REST-SOAP patterns
5. Check `memory/current_phase.json` (should be current phase post-sync)

### WORKFLOW & RELEASE RULES - MERGE CONFLICTS

If the AI encounters merge conflicts and is unable to resolve them automatically, it must stop and provide detailed instructions to the user on how to resolve the conflicts manually. It should not proceed with the merge until the user confirms that the conflicts have been resolved. Untracked files that would be overwritten by the merge must be handled with care. The AI should offer to move these files to a backup location before proceeding with the merge.

### WORKFLOW & RELEASE RULES - BRANCH MANAGEMENT

When the AI is instructed to merge or copy files from feature branches into the main branch, the following steps must be followed:

1. **Identify Relevant Branches:** Identify all feature branches to be merged.
2. **Update Branches:** For each branch, check out the branch and pull the latest changes.
3. **Merge or Cherry-Pick:** Merge or cherry-pick changes into `main`, resolving conflicts if needed.
4. **Validate:** Validate that all project files/folders are present and correct in `main`.
5. **Delete Branches:** After a successful merge, delete the merged branches locally and remotely.
6. **Confirm Workspace:** Confirm the workspace is clean and up to date.
7. **Handle Untracked Files:** If untracked files would be overwritten by the merge, move these files to a backup location before proceeding with the merge.

### WORKFLOW & RELEASE RULES - NEXT-STEPS OUTLINE

The following outlines the next steps in the project, following a Spec-Kit-aligned, AI-enhanced workflow:

#### Global Tool-Set

*   **MCPs / extensions:** Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory, Web-Search-for-Copilot, MarkItDown, SQL-Server(mssql), Codacy, Firecrawl, Postman, HashiCorp Terraform, GitHub, GitHub Pull Requests & Issues
*   **Built-ins:** workspace file search & read, `specify` CLI, `git`, `gh`, `terraform`, `codacy-cli`, npm/pnpm, docker, lighthouse-ci, axe-cli

#### Mandatory Entry Step – Summarise & Sync

| #   | Tool / Built-in                | Specific Capability         | Input (concrete)                                 | Output (artifact)         | Feeds Into |
|-----|-------------------------------|----------------------------|--------------------------------------------------|--------------------------|------------|
| 0.1 | built-in + AI                 | `summarise_current_chat`     | full transcript → decisions, features, fixes     | `chat_summary.md`        | 0.2        |
| 0.2 | built-in                      | `update_spec_kit_files`      | merge chat_summary.md into spec.md & `tasks.json` | updated Spec-Kit artifacts | 0.3        |
| 0.3 | GitHub Pull Requests & Issues | commit & push              | message = `"spec: sync chat-summary <timestamp>"`| remote branch updated     | 0.4        |
| 0.4 | Memory MCP                    | store_snapshot             | `{"source":"chat_summary.md","phase":"Hand-off"}`| memory updated            | Phase 1    |

#### Phase 1 – Specify (Gate ①) – AI-Enhanced

| #   | Tool / Built-in         | Capability                | Input / Prompt                                  | Output / Artifact         | Feeds Into |
|-----|------------------------|---------------------------|-------------------------------------------------|--------------------------|------------|
| 1.1 | GitHub Copilot Chat    | `generate_user_stories`     | “Create stories & acceptance criteria for spec.md” | `user_stories.json`      | 1.2        |
| 1.2 | Built-in + specify CLI | `create_schemas`            | stories → type-safe schemas / API contracts     | `schemas.ts` + `api.yml` | 1.3        |
| 1.3 | Playwright MCP         | `generate_e2e_stub`         | stories → `e2e/<story>.spec.ts`                 | test stubs               | 1.4        |
| 1.4 | Built-in               | write_file                | merge above → spec.md updated    | spec finalized           | Phase 2    |

#### Phase 2 – Plan (Gate ②) – Intelligent Planning

| #   | Tool / Built-in         | Capability                | Input / Prompt                                  | Output / Artifact         | Feeds Into |
|-----|------------------------|----------------ニーカー|-------------------------------------------------|--------------------------|------------|
| 2.1 | HashiCorp Terraform MCP| plan_infrastructure       | `{"file":"infra/main.tf","cloud":"azure"}`      | `tfplan.json`            | 2.2        |
| 2.2 | GitHub Copilot Chat    | review_plan               | “Check plan for cost, security, scalability”    | improvement list          | 2.3        |
| 2.3 | SQL-Server(mssql) MCP  | validate_schema_draft     | `schemas.ts` → DDL                              | `.sql` file              | 2.4        |
| 2.4 | Codacy MCP             | scan_plan                 | `plan.md` + DDL + TF                            | quality report            | Phase 3    |

#### Phase 3 – Tasks (Gate ③) – Smart Task Generation

| #   | Tool / Built-in                | Capability                | Input / Prompt                                  | Output / Artifact         | Feeds Into |
|-----|-------------------------------|---------------------------|-------------------------------------------------|--------------------------|------------|
| 3.1 | GitHub Pull Requests & Issues | create_issues             | `tasks.json` (AI-prioritised)                   | issue list               | 3.2        |
| 3.2 | GitHub Copilot Chat           | suggest_impl              | per issue → code hints                          | hints stored in issue    | 3.3        |
| 3.3 | Built-in                      | dependency_graph          | tasks → ordered graph                           | `task_graph.json`        | Phase 4    |

#### Phase 4 – Implement (Gate ④) – AI-Assisted Coding

| #   | Tool / Built-in                | Capability                | Input / Prompt                                  | Output / Artifact         | Feeds Into |
|-----|-------------------------------|---------------------------|-------------------------------------------------|--------------------------|------------|
| 4.1 | GitHub Pull Requests & Issues | fetch_top_issue           | highest priority                                | issue context            | 4.2        |
| 4.2 | GitHub Copilot Chat           | generate_code             | “Implement issue #123 per hints”                | `.ts/.tsx` files         | 4.3        |
| 4.3 | Playwright + Built-in         | run_tests                 | unit + int + e2e                                | all green                | 4.4        |
| 4.4 | lighthouse-ci & axe-cli       | performance_accessibility | URL = `localhost:3000`                          | budgets met, WCAG AA     | 4.5        |
| 4.5 | GitHub Pull Requests & Issues | create_pr                 | template + hints + reports                      | PR #xxx                  | Phase 5    |

#### Phase 5 – Review & Deploy (Gate ⑤) – Intelligent Review

| #   | Tool / Built-in         | Capability                | Input / Prompt                                  | Output / Artifact         | Feeds Into |
|-----|------------------------|---------------------------|-------------------------------------------------|--------------------------|------------|
| 5.1 | Codacy MCP             | scan_pr                   | PR diff                                         | quality gate ≥ threshold | 5.2        |
| 5.2 | GitHub Copilot Chat    | review_checklist          | “Security, perf, a11y, i18n”                    | checklist ok             | 5.3        |
| 5.3 | GitHub Pull Requests & Issues | merge_pr           | squash + semantic msg                           | trunk updated            | 5.4        |
| 5.4 | HashiCorp Terraform MCP| apply                     | `tfplan.json`                                   | infra live               | 5.5        |
| 5.5 | Built-in               | feature_flags & observability | enable flags, dashboards                   | monitored release        | Hand-off   |

### HAND-OVER PROTOCOL - ENGINEERED PROMPT FOR NEXT AGENT

```
You are continuing an AI-First Spec-Kit project.
Tool-set: the 14 MCPs/extensions listed above + workspace file access + `specify` CLI.

START-UP CHECKLIST  
1. `git pull origin main --rebase && npm ci`  
2. Read `chat_summary.md` → latest decisions & blockers  
3. Read `specs/latest/spec.md` → canonical requirements  
4. Read `memory/current_phase.json` → your starting gate

PHASE-SPECIFIC COMMANDS  
- Specify: `specify init <feature> --ai`  
- Plan: `/plan` (Terraform extension auto-plans)  
- Tasks: `/tasks` (GitHub issues created)  
- Implement: pick top issue, Copilot-generate code, run full test suite (unit+int+e2e), lighthouse & axe, PR  
- Review: Codacy scan, Copilot checklist, merge, Terraform apply, enable observability

BEFORE YOU LEAVE  
Re-run Step 0.1-0.4 (summarise, update Spec-Kit files, commit, memory snapshot) and print the **Hand-over Report** markdown block:
- Phase completed
- Artifacts changed (paths + SHAs)
- Next agent’s entry command
- Remaining risks / blockers
```

**Note**: Before proceeding with any changes, always check the latest content of `PHASE_1_HANDOVER_REPORT.md`, `chat_summary.md`, and `current_phase.json` for manual edits made after the branch merges.