---
description: AI rules derived by SpecStory from the project AI interaction history
globs: *
---

## AI Coding Assistant Rules File

### <headers/>

This file defines all project rules, coding standards, workflow guidelines, references, documentation structures, and best practices for the AI coding assistant. It is a living document that evolves with the project.

### TECH STACK

*   **MCPs / extensions**: Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory, Web-Search-for-Copilot, MarkItDown, SQL-Server(mssql), Codacy, Firecrawl, Postman, Terraform, GitHub
*   **Built-ins**: workspace file search & read, `specify` CLI, `git`, `gh`, `terraform`, `codacy-cli`

### PROJECT DOCUMENTATION & CONTEXT SYSTEM

The `specs/latest/spec.md` file contains the final specification with all enhancements. It should be consulted to confirm the project's current state. `chat_summary.md` provides a structured summary of recent activity. Memory is stored using the Memory MCP, with phase markers indicating the current stage of the project.

### WORKFLOW & RELEASE RULES

1.  **Summarise & Sync (Mandatory Entry Step)**: Before starting a new phase, always summarize the current chat, update the Spec-Kit files, commit the changes, and store a snapshot in memory.
    *   Use `summarise_current_chat` to create `chat_summary.md`.
    *   Use `update_spec_kit_files` to merge `chat_summary.md` into `specs/latest/spec.md` & `tasks.json`.
    *   Use `git commit_push` with message `"spec: sync <feature> <timestamp>"`. Examples: `"spec: sync security-validation <timestamp>"`, `"spec: sync branch-protection <timestamp>"`
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

When encountering merge conflicts during PR review, switch to each feature branch and update them manually by rebasing onto the `main` branch.

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

### HAND-OVER PROTOCOL

Before finishing, always:

a. Re-run Step 0.1-0.4 (summarise, update Spec-Kit files, commit, memory store).
b. Print a markdown block with:
   - Phase completed
   - Artifacts changed (paths + hashes)
   - Next agent's entry command
   - Remaining risks / blockers

### TEMPLATE COMMANDS/SNIPPETS:

*   GitHub MCP: `gh pr list` (verify all PRs exist)
*   GitHub MCP: `gh pr merge <pr_number> --squash --delete-branch` (merge each PR)
*   GitHub MCP: `gh api repos/OWNER/REPO/branches/main/protection -X PUT -f ...` (basic protection)
*   GitHub MCP: `gh api repos/OWNER/REPO/rulesets -X POST -f ...` (advanced rulesets)
*   Terraform MCP: `terraform apply -auto-approve`
*   Playwright MCP: `npx playwright test --config=playwright.production.config.ts`
*   MarkItDown MCP: `generate-deployment-guide --output docs/deployment.md`
*   Codacy MCP: `codacy-cli enable-github-status-checks`
*   MarkItDown MCP: `generate-docs --type branch-protection --output docs/security/branch-protection.md`