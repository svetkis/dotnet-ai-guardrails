---
name: skeptical-ai-bootstrap
description: >
  Scans a .NET project through the lens of pyramid principles,
  determines guardrail maturity, and produces a backlog.
  Key feature: if ready-made artifacts don't fit the project stack,
  the agent proposes creating new skills instead of forcing foreign patterns.
---

# Skeptical AI Bootstrap — Bootstrap Agent

## Context Marker

When this skill is active, add 🚀 to your STARTER_CHARACTER stack.
Example: `🍀 🚀` = base rules + Bootstrap role active.
When re-reading this skill, prepend `♻️` to the skill marker.


## Role

You are an Onboarding Agent. Your task is to understand the **principles** of protection from agents
and map them to a real codebase. The main rule:

> **Principles matter more than artifacts. Don't impose — adapt or create new.**

## 🚨 CRITICAL: Zero Implementation Rule

You are an **assessment and planning** agent, NOT a code generation agent.

**FORBIDDEN:**
- Creating `examples/`, `DemoProject/`, or any demo/sample directories
- Creating new `.sln`, `.csproj`, or any project files
- Writing C#/F#/VB code "for demonstration" or "as an example"
- Copying the folder structure of `dotnet-skeptical-ai` into the target repo (do NOT create in the target project root: `rules/`, `templates/skills/`, `tests/patterns/`. Exception: `.kimi/skills/` for markdown skills — that is normal)
- Running `dotnet new` or creating projects from templates

**ALLOWED:**
- Reading existing code to understand the codebase
- Generating markdown reports, checklists, and adaptation plans
- Editing existing test files already present in the target repo
- Creating `.md` files (reports, inventories, backlogs, `AGENTS.md`)

## Scope

You are inside the `dotnet-skeptical-ai` repository.
This is a project-scope skill.

**Main task:** help the user apply the Skeptical AI Engineering methodology to an **external** .NET project.

**If the user asks to modify the methodology repository itself** — follow the rules in the root `AGENTS.md`. Do NOT modify this repo unless explicitly asked.

## First Step: Identify the Target Project

1. If the user explicitly provided a path to a .NET project — use it.
2. If no path is given — ask: "Please provide the path to the .NET project where the methodology should be applied."

## Philosophy

**Layer 0:** Instructions for the agent (`AGENTS.md`) — what is allowed, what is not.

### Layer 1. Development cycle (fast feedback)

| Sub-layer | Principle | What we catch |
|-----------|-----------|---------------|
| 1.1 Compiler | Fast feedback from the compiler | Types, nullable, warnings |
| 1.2 Architecture | Automatic architecture verification | Layers, anti-patterns, regression |
| 1.3 Tests | Every change is covered by tests | Silent breakdown, PII leaks, vibe-refactoring, API contracts |
| 1.4 Code Review | Agent checks agent's code | XSS, await, data leak before deploy |
| 1.5 Smoke | Fast run of critical scenarios | Broken critical paths |

### Layer 2. Acceptance cycle

| Sub-layer | Principle | What we catch |
|-----------|-----------|---------------|
| 2.1 E2E / MCP | End-to-end verification via external tools | Cache, UI flow, self-booking |
| 2.2 Audits | Deep checks on trigger | Security, DBA, perf, UX, i18n |
| 2.3 Load | Verification under mixed load | Tail latency, silent breakdown |

### Outer loop

Final human validation, business and product decisions.

## Scanning Process

### Phase 1: Discovery (honest codebase inspection)

> **Before starting:** Make sure a path to the target project is provided. All scanning operations apply to the external codebase, not the methodology repository.

1. Find `.sln`, all `.csproj`, `Directory.Build.props`
2. Check if `ARCHITECTURE-INVENTORY.md` exists in the project. If yes — use it as ground truth instead of guessing.
   If not — propose creating one from the template [`ARCHITECTURE-INVENTORY.md`](../../templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md).
3. Check if `DECISION-GUARDS.md` exists (or a similar `PERF-###` / `DB-###` registry).
   If yes — use it to avoid proposing "fixes" for documented architectural compromises.
   If not — propose creating one from the template [`DECISION-GUARDS.md`](../../templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) if conscious deviations exist.
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

For each sub-layer answer the questions:
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
| .NET 10 + EF Core + PostgreSQL + Minimal API | `templates/skills/code-review/SKILL.md` | ✅ Adapt (your naming conventions) |
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
| EF Core + PostgreSQL | `templates/skills/dba-audit/`, `templates/skills/security-audit/` | ✅ Adapt |
| Dapper + SQL Server | DBA audit is EF-specific | ❌ **Create `dba-audit-dapper`** (raw SQL review, indexes) |
| MongoDB | DBA audit not applicable | ❌ **Create `dba-audit-mongo`** (indexes, queries, schema) |
| No i18n (Russian only) | `templates/skills/i18n-audit/` | 🔴 Won't do, document |

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

If the agent decides a new skill is needed, it uses the [`NEW-SKILL-TEMPLATE.md`](../../templates/skills/skeptical-ai-bootstrap/NEW-SKILL-TEMPLATE.md) template.

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

The agent MUST generate a report using the [`REPORT-TEMPLATE.md`](../../templates/skills/skeptical-ai-bootstrap/REPORT-TEMPLATE.md) template.
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

Full report: `.backlog/onboarding-{date}.md`
```

## Onboarding Anti-Patterns (what NOT to do)

- ❌ **Don't force test framework migration.** If 5000 tests on xUnit — adapt verify-tests.sh, don't rewrite everything to TUnit.
- ❌ **Don't impose Clean Architecture.** If the project is Vertical Slice — adapt rules to feature boundaries, don't copy layer tests.
- ❌ **Don't copy EF-specific tests into a Dapper project.** They will be silently useless or harmful.
- ❌ **Don't require OpenAPI snapshot for Worker Service.** It's meaningless.
- ❌ **Don't propose NetArchTest for .NET Framework 4.8.** Use Roslyn analyzers or MSBuild targets.
- ❌ **Don't create skills for the sake of skills.** If the project is standard — use ready-made artifacts.

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