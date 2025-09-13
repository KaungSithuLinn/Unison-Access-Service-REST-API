# Zero-Context Engineered Prompt for Next Agent

## 🚀 AGENT HANDOVER PROTOCOL

You are continuing an **AI-First Spec-Kit project** with advanced automation and quality gates.

---

## ⚡ CRITICAL STARTUP SEQUENCE

### Step 1: Environment Preparation

```powershell
# Pull latest changes and sync workspace
git pull origin main --rebase
npm ci  # or yarn install if package.json uses yarn
```

### Step 2: Context Loading (MANDATORY)

1. **Read `ai_enhanced_chat_summary.md`** → Latest decisions, blockers, and AI insights
2. **Read `specs/latest/spec.md`** → Canonical requirements and architecture
3. **Read `memory/current_phase.json`** → Your exact starting gate and phase context
4. **Read `docs/AI_ENHANCED_NEXT_STEPS_OUTLINE.md`** → Detailed phase workflows

### Step 3: Tool Verification

Verify these 14+ tools are available:

- Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory MCPs
- Web-Search-for-Copilot, MarkItDown, SQL-Server, Codacy, Firecrawl MCPs
- Postman, HashiCorp Terraform, GitHub, GitHub Pull Requests & Issues MCPs
- Built-ins: `specify` CLI, git, gh, terraform, codacy-cli, lighthouse-ci, axe-cli

---

## 🎯 PHASE-SPECIFIC QUICK COMMANDS

### **Phase 1 - Specify** (AI-Enhanced Specification)

```bash
# Generate user stories from WSDL
specify init user-stories --ai --source=temp_wsdl.xml

# Create type-safe schemas
specify generate schemas --format=openapi3 --typescript

# Setup E2E test stubs
playwright codegen --target=typescript
```

### **Phase 2 - Plan** (Intelligent Planning) ← **LIKELY CURRENT PHASE**

```bash
# Infrastructure optimization
terraform plan -var-file=environments/production.tfvars

# AI-assisted architecture review
# Use GitHub Copilot Chat: "Review infrastructure plan for cost, security, scalability"

# Component pattern validation
# Use Sequential-Thinking MCP for complex architecture decisions
```

### **Phase 3 - Tasks** (Smart Task Generation)

```bash
# Create prioritized GitHub issues
gh issue create --title="[TASK]" --body-file=task-template.md

# Setup CI/CD integration
gh workflow run ci.yml --ref=main
```

### **Phase 4 - Implement** (AI-Assisted Coding)

```bash
# Fetch top priority issue
gh issue list --state=open --sort=priority --limit=1

# Generate code with AI assistance
# Use GitHub Copilot Chat for implementation

# Run full test suite
npm test && npm run test:integration && npx playwright test

# Performance & accessibility validation
npx lighthouse-ci autorun
npx axe-cli http://localhost:3000
```

### **Phase 5 - Review & Deploy** (Intelligent Review)

```bash
# Quality scan
codacy-cli scan --provider=gh --organization=KaungSithuLinn --repository=Unison-Access-Service-REST-API

# Production deployment
terraform apply -auto-approve

# Smoke tests
npx playwright test --config=playwright.production.config.ts
```

---

## 🔄 MANDATORY WORKFLOW PATTERNS

### Before Starting ANY Work

**ALWAYS execute Steps 0.1-0.4 (Mandatory Entry Step)**:

1. **Summarize current chat** → `ai_enhanced_chat_summary.md`
2. **Update Spec-Kit files** → Merge into `spec.md` & `tasks.json`
3. **Commit & push** → `"spec: sync <feature> $(Get-Date -Format 'yyyy-MM-dd-HH-mm')"`
4. **Store memory snapshot** → Memory MCP with AI insights and tech debt

### During Work (Quality Gates)

- **After ANY file edit**: Run `codacy_cli_analyze` on changed files
- **After dependency changes**: Run security scan with Codacy + trivy
- **Before PR creation**: Validate performance (lighthouse) + accessibility (axe)
- **Every commit**: Ensure tests pass and coverage ≥90%

### Before Finishing (Exit Protocol)

1. **Re-run Steps 0.1-0.4** (sync latest state)
2. **Print Handover Report** (see template below)

---

## 📋 HANDOVER REPORT TEMPLATE

````markdown
## 🎯 HANDOVER REPORT

### Phase Completed

- **Phase**: [X] - [Name]
- **Status**: ✅ Complete / 🚧 Partial / ❌ Blocked
- **Duration**: [X] hours

### Artifacts Changed

- `path/to/file.ext` (SHA: abc123...)
- `another/file.ts` (SHA: def456...)
- **Quality Metrics**: Coverage X%, Performance Y/100, Security Z issues

### Next Agent Entry Command

```bash
# Next agent should start with:
git pull origin main --rebase
# Read context files (step 2 above)
# Execute Phase [Y] starting with step [Y.1]
```
````

### Remaining Risks / Blockers

- **Risk**: [Description] → **Mitigation**: [Action needed]
- **Blocker**: [Issue] → **Resolution**: [Required action]

### Technical Debt Items

- **Item**: [Description] → **Priority**: High/Medium/Low
- **Estimated Effort**: [X] hours/days

### Quality Status

- **Test Coverage**: X% (target: ≥90%)
- **Security Score**: X/10 (target: ≥9)
- **Performance**: Core Web Vitals Y/100 (target: ≥90)
- **Accessibility**: WCAG 2.1 AA X% (target: 100%)

```markdown
---

## 🛡️ CRITICAL SUCCESS FACTORS

### Non-Negotiable Quality Gates
- **0 critical security vulnerabilities** (Codacy scan required)
- **≥90% test coverage** (unit + integration + E2E)
- **Core Web Vitals compliance** (LCP <2.5s, FID <100ms, CLS <0.1)
- **WCAG 2.1 AA accessibility** (axe-cli validation required)

### AI Integration Requirements
- **Use GitHub Copilot Chat** for code generation and architecture decisions
- **Use Sequential-Thinking MCP** for complex problem decomposition
- **Use Memory MCP** for context persistence and AI insights tracking
- **Use specialist MCPs** (Terraform, Playwright, etc.) for domain expertise

### Repository Standards
- **Provider**: gh
- **Organization**: KaungSithuLinn  
- **Repository**: Unison-Access-Service-REST-API
- **Branch Strategy**: Feature branches → Main (with PR reviews)
- **Commit Messages**: Semantic format (`feat:`, `fix:`, `docs:`, `spec:`)

---

## 🧠 PROJECT CONTEXT QUICK REFERENCE

**Architecture**: REST-to-SOAP adapter (validated and production-ready)  
**Backend Service**: SOAP 1.1 web service (confirmed via testing)  
**Infrastructure**: Azure-based with Terraform IaC  
**Security**: JWT authentication, OWASP Top 10 compliance  
**Framework**: AI-enhanced Spec-Kit with 14+ MCP tool integration

**Current Technical Debt**:

1. Circuit breaker patterns for adapter scalability
2. Dynamic WSDL caching mechanism
3. Real-time Web Vitals monitoring integration
4. Automated accessibility validation pipeline

---

## 🎯 SUCCESS METRICS

**Deployment Ready When**:

- ✅ All phases complete with quality gates passed
- ✅ Production infrastructure deployed and monitored
- ✅ Security audit complete with 0 critical issues
- ✅ Performance targets achieved and validated
- ✅ Accessibility compliance verified
- ✅ Rollback procedures tested and documented

---

_This prompt is engineered for zero-context agent onboarding with maximum automation and quality assurance._

**Version**: 1.0  
**Last Updated**: September 13, 2025  
**Framework**: AI-Enhanced Spec-Kit  
**Phase Ready**: Phase 2 (Intelligent Planning)
```
