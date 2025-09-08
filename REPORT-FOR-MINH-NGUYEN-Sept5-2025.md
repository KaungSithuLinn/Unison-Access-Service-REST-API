# Unison Access Service REST API – Technical Findings & Next Steps Report

**Date:** September 5, 2025  
**Prepared for:** Minh Nguyen  
**Prepared by:** Unison Access Service Technical Team

---

## Part A – Next Steps Outline

### Step 1: Confirm and Apply WCF Configuration Fix

- **MCP/Extension/Capability:** Built-in workspace file system, Microsoft Docs MCP
- **Capability Invoked:** read_file, search_in_workspace, fetch docs
- **Input:** Review `Pacom.Unison.Server.exe.config` and Microsoft Docs for `<serviceDebug includeExceptionDetailInFaults="true" />`
- **Expected Output:** Config file with correct WCF settings, validated against Microsoft Docs
- **Feeds Into:** Enables SOAP faults for proper error handling

### Step 2: Reserve HTTP.SYS URL ACL (If Needed)

- **MCP/Extension/Capability:** Microsoft Docs MCP, built-in file system
- **Capability Invoked:** fetch docs, read_file
- **Input:** Validate `netsh http add urlacl` command and current URL reservations
- **Expected Output:** Confirmed URL ACL reservation for service endpoint
- **Feeds Into:** Ensures service can bind to HTTP endpoint

### Step 3: Restart Unison Access Service

- **MCP/Extension/Capability:** Built-in file system, PowerShell/terminal
- **Capability Invoked:** run_in_terminal
- **Input:** `Restart-Service -Name "Pacom Unison Driver Service"`
- **Expected Output:** Service restarted with new configuration
- **Feeds Into:** Applies configuration changes for validation

### Step 4: Validate SOAP Fault Behavior

- **MCP/Extension/Capability:** Built-in file system, Postman MCP, Playwright MCP (if web UI)
- **Capability Invoked:** run test scripts, run Postman collection, automate browser validation
- **Input:** Run `test_soap_fault_fix.py`, `final_api_validation.py`, and Postman collection
- **Expected Output:** SOAP faults (not HTML error pages) returned on error
- **Feeds Into:** Confirms fix is effective

### Step 5: Document Test Results and Update Artifacts

- **MCP/Extension/Capability:** MarkItDown MCP, built-in file system
- **Capability Invoked:** convert_to_markdown, create_file
- **Input:** Test outputs, before/after results, updated documentation
- **Expected Output:** Markdown report summarizing validation and results
- **Feeds Into:** Provides evidence for stakeholders and future troubleshooting

### Step 6: Analyze Code Quality and Security

- **MCP/Extension/Capability:** Codacy MCP
- **Capability Invoked:** codacy_cli_analyze
- **Input:** Analyze all edited/tested files and dependencies
- **Expected Output:** Code quality and security report (e.g., CVE-2023-29331 found)
- **Feeds Into:** Ensures codebase is secure and compliant

### Step 7: Persist and Index All Artifacts

- **MCP/Extension/Capability:** Memory MCP, Context7 MCP, MarkItDown MCP
- **Capability Invoked:** add_observations, index documentation, ensure markdown format
- **Input:** All key findings, test results, and documentation
- **Expected Output:** All artifacts indexed and retrievable for future troubleshooting/onboarding
- **Feeds Into:** Completes knowledge engineering cycle

---

## Part B – Engineered Prompt for the Next Agent

---

**Engineered Prompt:**

You are the next AI agent in the Unison Access Service REST API technical workflow. Your objectives are to validate, document, and persist all technical findings, fixes, and lessons learned, using the following MCPs/extensions and built-in workspace file system access:

**Available MCPs/Extensions:**

- Microsoft Docs MCP
- Context7 MCP
- Sequential Thinking MCP
- Playwright MCP server
- Memory MCP servers
- Web Search for Copilot extension
- MarkItDown MCP Server
- SQL Server (mssql) Extension
- Codacy MCP
- Firecrawl MCP
- Postman MCP
- Terraform MCP
- GitHub MCP
- Built-in workspace file system (search, read, list, edit files)

**Context:**

- WCF AccessService was returning HTML error pages instead of SOAP faults due to missing WCF config and HTTP.SYS URL ACL
- Fix: Add correct WCF config, reserve URL with netsh, restart service
- Validation: WSDL and SOAP faults should work on localhost
- All findings and references must be documented, validated, and persisted for future troubleshooting and onboarding

**Action Plan:**

1. **Confirm and apply WCF configuration fix:**
   - Use file system and Microsoft Docs MCP to review and validate config
   - Template: `read_file`, `fetch docs for <serviceDebug includeExceptionDetailInFaults>`
2. **Reserve HTTP.SYS URL ACL (if needed):**
   - Use Microsoft Docs MCP and file system to validate and apply `netsh http add urlacl`
   - Template: `fetch docs for netsh http`, `run_in_terminal`
3. **Restart Unison Access Service:**
   - Use terminal to restart service
   - Template: `run_in_terminal: Restart-Service -Name "Pacom Unison Driver Service"`
4. **Validate SOAP fault behavior:**
   - Use test scripts, Postman MCP, Playwright MCP as needed
   - Template: `run test_soap_fault_fix.py`, `run Postman collection`, `automate browser validation`
5. **Document test results and update artifacts:**
   - Use MarkItDown MCP and file system to convert results to markdown
   - Template: `convert_to_markdown`, `create_file`
6. **Analyze code quality and security:**
   - Use Codacy MCP to analyze all relevant files
   - Template: `codacy_cli_analyze`
7. **Persist and index all artifacts:**
   - Use Memory MCP, Context7 MCP, MarkItDown MCP to persist and index findings
   - Template: `add_observations`, `index documentation`, `ensure markdown format`

**Constraints:**

- Do not re-ask for prior conversation history; all context is provided above
- Use the provided MCPs/extensions and built-in file system capabilities as specified
- For each step, use the template commands/snippets to invoke the correct MCP/extension or file system operation
- Ensure all outputs are logically chained and no step is skipped

**Your task:**
Execute the above action plan, producing all required artifacts and documentation, and ensuring all findings are persisted and indexed for future troubleshooting and onboarding.
