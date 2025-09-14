# Zero-Context Agent Prompt

You are continuing an AI-First Spec-Kit project with complete tool integration and framework support.

## Tool-Set Available

**14 MCPs/Extensions**: Microsoft-Docs, Context7, Sequential-Thinking, Playwright, Memory, Web-Search-for-Copilot, MarkItDown, SQL-Server(mssql), Codacy, Firecrawl, Postman, HashiCorp Terraform, GitHub, GitHub Pull Requests & Issues

**Built-ins**: workspace file search & read, `specify` CLI, `git`, `gh`, `terraform`, `codacy-cli`, npm/pnpm, docker, lighthouse-ci, axe-cli

## START-UP CHECKLIST

**Execute these steps in order before beginning work:**

1. **Environment Sync**: `git pull origin main --rebase && npm ci`
2. **Context Loading**: Read `chat_summary.md` → latest decisions & blockers
3. **Requirements Review**: Read `specs/latest/spec.md` → canonical requirements
4. **Phase Confirmation**: Read `memory/current_phase.json` → your starting gate

## PHASE-SPECIFIC COMMANDS

**Quick Reference for Each Phase:**

- **Specify**: `specify init <feature> --ai`
- **Plan**: `/plan` (Terraform extension auto-plans)
- **Tasks**: `/tasks` (GitHub issues created)
- **Implement**: pick top issue, Copilot-generate code, run full test suite (unit+int+e2e), lighthouse & axe, PR
- **Review**: Codacy scan, Copilot checklist, merge, Terraform apply, enable observability

## MANDATORY ENTRY STEP (0.1-0.4)

**ALWAYS execute before starting phase work:**

1. **Summarize**: Create/update `chat_summary.md` with AI insights
2. **Sync Specs**: Merge chat summary into `specs/latest/spec.md` & `tasks.json`
3. **Commit**: `git commit -m "spec: sync <feature> <timestamp>"`
4. **Memory**: Store snapshot with Memory MCP

## WORKFLOW STEPS BY PHASE

### Phase 1 - Specify (AI-Enhanced) ✅ COMPLETE

**Status**: All deliverables complete

- User stories with acceptance criteria in `specs/latest/user-stories.md`
- Type-safe schemas in `specs/latest/api-schema.yaml`
- Performance baselines in `specs/latest/metrics-baseline.md`
- E2E test stubs generated with Playwright

### Phase 2 - Plan (Intelligent Planning) 🚀 READY

**Your immediate actions:**

1. **Infrastructure Planning**: Use HashiCorp Terraform MCP for Azure optimization
2. **Cost/Security Review**: Use GitHub Copilot Chat for plan analysis
3. **Schema Validation**: Use SQL-Server(mssql) MCP for DDL generation
4. **Quality Gates**: Use Codacy MCP for planning artifact validation

**Success Criteria:**

- Terraform plan validated and cost-optimized
- Security review completed (OWASP compliance)
- Database schema performance-optimized
- All planning artifacts pass quality thresholds

### Phase 3 - Tasks (Smart Generation)

**Actions:**

1. **Issue Creation**: GitHub Pull Requests & Issues MCP with AI prioritization
2. **Implementation Hints**: GitHub Copilot Chat for code guidance
3. **Dependency Mapping**: Task graph generation for optimized execution

### Phase 4 - Implement (AI-Assisted Coding)

**Actions:**

1. **Top Issue**: Fetch highest priority with GitHub MCP
2. **Code Generation**: GitHub Copilot Chat with TypeScript/ESLint standards
3. **Testing**: Playwright + lighthouse-ci + axe-cli (90% coverage target)
4. **PR Creation**: Automated with quality reports

### Phase 5 - Review & Deploy (Intelligent Review)

**Actions:**

1. **Quality Scan**: Codacy MCP for threshold validation
2. **Review Checklist**: AI-assisted security/performance/accessibility check
3. **Deployment**: Terraform apply with monitoring activation

## QUALITY GATES & TARGETS

**Performance**: Core Web Vitals (LCP <2.5s, FID <100ms, CLS <0.1)
**Accessibility**: WCAG 2.1 AA compliance via axe-cli
**Security**: OWASP Top 10 coverage, Codacy rating >8.0
**Testing**: 90% coverage (unit + integration + E2E)

## TECHNICAL DEBT AWARENESS

**High Priority:**

1. WSDL caching strategy for latency reduction
2. SOAP fault translation standardization
3. API documentation automation

**Monitor & Address**: Check `chat_summary.md` technical debt section for latest items

## BEFORE YOU LEAVE

**ALWAYS execute handover protocol:**

a. Re-run Mandatory Entry Step 0.1-0.4 (summarise, update Spec-Kit files, commit, memory snapshot)

b. Print **Hand-over Report** markdown block:

```markdown
## Hand-over Report

**Phase Completed**: [Phase name and gate]
**Artifacts Changed**:

- [file path] (SHA: [commit hash])
- [performance metrics if applicable]

**Next Agent Entry Command**: [specific next action]
**Remaining Risks/Blockers**: [any issues that need attention]
**Quality Metrics**: [coverage %, performance scores, etc.]
```

## PROJECT CONTEXT

**Architecture**: REST-to-SOAP adapter for Unison Access Service
**Technology Stack**: .NET 9.0, Azure deployment, Docker containerization
**Current Status**: Phase 1 complete, Phase 2 ready for intelligent planning
**Branch Protection**: Active (requires PRs, signed commits, status checks)

## CRITICAL SUCCESS FACTORS

1. **Zero Context Gaps**: All information available in documented artifacts
2. **Quality Gates**: Never compromise on established thresholds
3. **AI Integration**: Leverage all 14 MCPs/extensions for optimal results
4. **Handover Excellence**: Ensure next agent has complete context

You have everything needed to succeed. Focus on the current phase, execute with quality, and maintain the handover chain.

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
