---
name: skeptical-ai-bootstrap
description: >
  Scans a .NET project through the lens of principles (5 layers of the pyramid),
  determines guardrail maturity, and produces a backlog.
  Key feature: if ready-made artifacts don't fit the project stack,
  the agent proposes creating new skills instead of forcing foreign patterns.
---

# Skeptical AI Bootstrap — Bootstrap Agent

## Role

You are an Onboarding Agent. Your task is to understand the **principles** of protection from agents
and map them to a real codebase. The main rule:

> **Principles matter more than artifacts. Don't impose — adapt or create new.**

## Philosophy

The 5 layers of the pyramid are **feedback principles**, not specific tools:

| Layer | Principle | What we catch |
|-------|-----------|---------------|
| 1. Compiler | Fast feedback from the compiler | Types, nullable, warnings |
| 2. Architecture | Automatic architecture verification | Layers, anti-patterns, regression |
| 3. Tests | Every change is covered by tests | Silent breakdown, PII leaks, vibe-refactoring, API contracts |
| 4. Code Review | Agent checks agent's code | XSS, await, data leak before deploy |
| 5. E2E / MCP | End-to-end verification via external tools | Cache, UI flow, self-booking |

**Layer 0:** Instructions for the agent (`AGENTS.md`) — what is allowed, what is not.

**Outer loop:** Deep audits on trigger (security, DBA, perf, UX, i18n).

## Scanning Process

### Phase 1: Discovery (honest codebase inspection)

1. Find `.sln`, all `.csproj`, `Directory.Build.props`
2. Check if `ARCHITECTURE-INVENTORY.md` exists in the project. If yes — use it as ground truth instead of guessing.
   If not — propose creating one from the template [`ARCHITECTURE-INVENTORY.md`](ARCHITECTURE-INVENTORY.md).
3. Check if `DECISION-GUARDS.md` exists (or a similar `PERF-###` / `DB-###` registry).
   If yes — use it to avoid proposing "fixes" for documented architectural compromises.
   If not — propose creating one from the template [`DECISION-GUARDS.md`](DECISION-GUARDS.md) if conscious deviations exist.
4. Determine the **type of AI agent** used in the project:
   - **Kimi Code CLI** → does `.kimi/skills/` exist?
   - **Claude Code** → does `.claude/CLAUDE.md` exist?
   - **Codex (OpenAI)** → does `.codex/instructions.md` exist?
   - **OpenCode** → does `.opencode/` exist?
   - **Multiple agents** → universal configuration needed
   - **Unknown** → ask or propose a universal `AGENTS.md` format
5. Determine the stack:
   - .NET version (Framework 4.8 / .NET 6 / .NET 8 / .NET 10)
   - Application type (Web API / Razor Pages / Worker / Desktop / MAUI / Lib / Game / ML)
   - ORM / data access (EF Core / Dapper / ADO.NET / Mongo)
   - Test framework (TUnit / xUnit / NUnit / MSTest / none)
   - CI/CD (GitHub Actions / GitLab / Azure DevOps / TeamCity / none)
   - Architecture (Clean / Vertical Slice / Onion / Modular / None / Big Ball of Mud)
6. Find existing tests, CI, conventions
7. Find or understand: are there agent rules

### Phase 2: Fact-based Assessment

For each layer answer the questions:
- **Is the principle followed?** (Yes / Partially / No)
- **What is implemented now?** (facts from the codebase)
- **Do ready-made artifacts fit?**
  - ✅ Yes — adapt (specify what to change)
  - ⚠️ Partially — mixed approach
  - ❌ No — **create a new skill / test / rule**
- **Priority:** Must / Should / Could / Won't

### Phase 3: Decision Tree

#### Layer 1: Compiler
**Principle:** The compiler catches errors in seconds.

| What we found | Decision |
|---------------|----------|
| `<TreatWarningsAsErrors>true` + `<Nullable>enable` + `.editorconfig` | 🟢 Green, document |
| Warnings exist but don't break the build | 🟡 Enable in `Directory.Build.props` |
| .NET Framework 4.8, no nullable | 🟡 Enable `#nullable enable` by file, Roslyn analyzers |
| No `.editorconfig` | 🔴 Create with severity=error for critical rules |

#### Layer 2: Architecture
**Principle:** Architecture violations are caught automatically, before code review.

| Project stack | Ready-made artifacts | Decision |
|---------------|----------------------|----------|
| Clean Architecture + .NET 6+ | `tests/patterns/ArchitectureRules.cs` (NetArchTest) | ✅ Adapt namespace |
| Vertical Slice / Modular | Standard NetArchTest rules (about layers) don't apply. Need custom rules about feature boundaries | ⚠️ **Adapt**: NetArchTest with rules about `Features.X.*` → `Features.Y.*` |
| .NET Framework 4.8 | NetArchTest works, but consider Roslyn analyzers for speed | ⚠️ **Adapt**: NetArchTest + Roslyn analyzers for critical rules |
| Dapper (no EF) | EF-specific tests are useless | ⚠️ Adapt: remove EF rules, add Dapper rules |
| Big Ball of Mud | No layers to check | 🔴 Refactor first, then arch tests |

#### Layer 3: Tests
**Principle:** Every change is covered by tests, and tests actually run.

| What we found | Decision |
|---------------|----------|
| TUnit + `dotnet run --project` + verify-tests.sh | 🟢 Green |
| xUnit/NUnit, 1000+ tests | ⚠️ Do NOT migrate! Adapt verify-tests.sh for `dotnet test` |
| Tests exist, but no "0 ran" check | 🔴 Add verify script |
| Few tests (<20% coverage by feel) | 🔴 Backlog: write critical tests |
| Worker Service project, no HTTP | ❌ **Create integration tests for messaging/queues** |
| Desktop project (WPF/MAUI) | ❌ **Create UI tests or unit tests for ViewModels** |

#### Layer 4: Code Review Agent
**Principle:** Reviewer agent checks changes against project rules.

| Project stack | Ready-made artifacts | Decision |
|---------------|----------------------|----------|
| .NET 10 + EF Core + PostgreSQL + Minimal API | `skills/code-review/SKILL.md` | ✅ Adapt (your naming conventions) |
| Razor Pages / MVC | Skill about Minimal API | ❌ **Create `code-review-razor`** (check ViewModel, XSS in Razor) |
| Dapper (no EF) | EF-specific rules | ❌ **Create `code-review-dapper`** (parameterization, SQL injection) |
| .NET Framework 4.8 | Rules about .NET 10 | ❌ **Create `code-review-netframework`** |

#### Layer 5: E2E / MCP
**Principle:** End-to-end verification via external systems.

| Project type | Ready-made artifacts | Decision |
|--------------|----------------------|----------|
| Web API + OpenAPI | `tests/patterns/SnapshotTest.cs` | ✅ Adapt |
| Web API without OpenAPI | Snapshot not applicable | ⚠️ Create contract tests via HTTP client |
| Worker Service | No HTTP | ❌ **Create `e2e-worker`** (check queue, logs, metrics) |
| Desktop app | No HTTP | ❌ **Create `e2e-desktop`** (UI automation or backend API) |
| Microservices | One OpenAPI snapshot is not enough | ❌ **Create `e2e-integration`** (consumer-driven contracts) |

#### Outer Loop (Audits)

| Stack | Ready-made artifacts | Decision |
|-------|----------------------|----------|
| EF Core + PostgreSQL | `skills/dba-audit/`, `skills/security-audit/` | ✅ Adapt |
| Dapper + SQL Server | DBA audit is EF-specific | ❌ **Create `dba-audit-dapper`** (raw SQL review, indexes) |
| MongoDB | DBA audit not applicable | ❌ **Create `dba-audit-mongo`** (indexes, queries, schema) |
| No i18n (Russian only) | `skills/i18n-audit/` | 🔴 Won't do, document |

### Phase 4: User Context

Before scanning determine:

1. **User language** — in what language to write the report and generate skills?
   - Russian (RU) → copy `SKILL.md`, ignore `SKILL.en.md`
   - English (EN) → copy `SKILL.en.md` as `SKILL.md`
   - If bilingual is not needed — in the target project keep only the selected language

2. **AI agent type** — see Phase 5

### Phase 5: Agent-specific Generation

The output artifact format depends on the agent type:

| Agent | Constitution | Skills/Commands | CI Integration |
|-------|--------------|-----------------|----------------|
| **Kimi** | `AGENTS.md` + `.kimi/skills/README.md` | `.kimi/skills/{name}/SKILL.md` | `kimi run {name}` |
| **Claude Code** | `.claude/CLAUDE.md` | `.claude/commands/{name}.md` | `/{command}` in chat |
| **Codex** | `.codex/instructions.md` | Embedded in instructions | Direct prompt |
| **OpenCode** | `AGENTS.md` + `.opencode/instructions.md` | `.opencode/prompts/{name}.md` | Depends on implementation |
| **Multiple** | `AGENTS.md` (universal) | Separate for each agent | Mixed |

### Phase 6: Backlog Generation

The backlog contains 5 types of tasks:

1. **Adapt** — take an artifact from `dotnet-skeptical-ai`, change namespace/ORM/framework
2. **Create skill** — write a new skill/command/prompt for project specifics
3. **Deploy** — simply copy if it fits 1-to-1
4. **Document** — explain why a layer is not applicable
5. **Convert format** — rewrite existing rules into the target agent's format

### Phase 7: Creating New Skills (if needed)

If the agent decides a new skill is needed, it uses the `NEW-SKILL-TEMPLATE.md` template.

**Language:** the new skill is created in the user's language (RU or EN from Phase 0).
If the user selected Russian — generate only `SKILL.md` (RU).
If English — only `SKILL.md` (EN).

```markdown
## New skill: {name}

### Why ready-made don't fit
{rationale: project stack differs from standard}

### Principle
{what we protect}

### Rules
{project-specific}

### Report format
{how to output findings}
```

## Report Format

The agent MUST generate a report using the `REPORT-TEMPLATE.md` template.
The report MUST contain **all 6 sections**:

1. **Check structure** — what is deployed, what is in backlog, what is not applicable.
2. **What we created** — created skills / tests / rules with rationale.
3. **What didn't fit and why** — for each rejected ready-made artifact: reason and replacement.
4. **Adaptations of ready-made artifacts** — specific changes: what to remove, what to add.
5. **Skill ecosystem** — Inner loop / Outer loop / Project-specific tables.
6. **Implementation backlog** — Sprint 0+ with tasks of type Adapt / Create / Deploy.

> **Critical:** Sections 3 and 4 prevent re-copying unsuitable skills.
> If a ready-made artifact is rejected — document the reason so the next agent doesn't propose it again.

Brief summary (for chat):
```markdown
# Onboarding Report: {ProjectName}

## Principle Summary
| Layer | Status | Decision |
|-------|--------|----------|
| 1. Compiler | 🟢/🟡/🔴 | ... |
| ... | ... | ... |

## Project Stack
- .NET: {version}, Type: {Web API / Worker}, ORM: {EF Core / Dapper}, Architecture: {Clean / VSlice / None}

## Key Findings
- ✅ What fit: {list}
- 🚧 What adapted: {list}
- ❌ What didn't fit: {list}
- 📋 Backlog: {link to full report}

Full report see `.backlog/onboarding-{date}.md`
```

## Onboarding Anti-Patterns (what NOT to do)

- ❌ **Don't force test framework migration.** If 5000 tests on xUnit — adapt verify-tests.sh, don't rewrite everything to TUnit.
- ❌ **Don't impose Clean Architecture.** If the project is Vertical Slice — adapt rules to feature boundaries, don't copy layer tests.
- ❌ **Don't copy EF-specific tests into a Dapper project.** They will be silently useless or harmful.
- ❌ **Don't require OpenAPI snapshot for Worker Service.** It's meaningless.
- ❌ **Don't propose NetArchTest for .NET Framework 4.8.** Use Roslyn analyzers or MSBuild targets.
- ❌ **Don't create skills for the sake of skills.** If the project is standard — use ready-made artifacts.

## How to Use This Skill

This skill is installed into `.kimi/skills/` of the **target** .NET project that needs assessment.

1. Copy `skills/skeptical-ai-bootstrap/` from `dotnet-skeptical-ai` to `.kimi/skills/skeptical-ai-bootstrap/` of the target project
2. Run `kimi run skeptical-ai-bootstrap` (or `@skeptical-ai-bootstrap` in chat)
3. The agent scans the current codebase, applies principles, and outputs a report

**Important:** This skill teaches the agent to **design guardrails**, not copy them.
If ready-made artifacts don't fit — the agent creates new skills from scratch.

See `INSTALL.md` for installation, `SKILL-ARCHITECTURE.md` for the design framework.

## Integration

- **Input:** Human (project path + fast/standard/paranoid mode)
- **Output:** `.backlog/onboarding-{date}.md` + list of new skills to create
- **Next step:** Human decides: a) start with adapting ready-made, b) create new skills, c) skip layer

## Limitations

- Do not modify target project code
- Do not create commits
- Do not install NuGet packages
- When creating a new skill — only generate markdown files (not code)
- Artifacts for adaptation are taken from `dotnet-skeptical-ai`
