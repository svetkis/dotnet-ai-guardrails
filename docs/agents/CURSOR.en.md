# Cursor — Guardrails Integration

> Cursor IDE uses `.cursorrules` (project rules) and `.cursor/rules/` for
> context-dependent instructions. This is a VS Code-based editor with AI chat
> and Composer mode.

## Integration Structure

```
.cursor/
├── .cursorrules              # Main constitution (analog of AGENTS.md)
└── rules/                    # Context-dependent rules (new format)
    ├── 001-general.md        # General rules for the entire project
    ├── 002-domain.md         # Rules for Domain layer
    ├── 003-infrastructure.md # Rules for Infrastructure
    ├── 004-api.md            # Rules for API layer
    ├── 005-tests.md          # Rules for tests
    └── 006-audits.md         # Prompt templates for audits
```

## Project Configuration

### 1. Create `.cursorrules`

This is the analog of the root `AGENTS.md` — a single file with the project constitution:

```markdown
# Project Guardrails — {ProjectName}

## Mission
{project description}

## Rules for AI
- Do not add dependencies without explicit request
- Do not change folder structure without agreement
- Every bug fix comes with a regression test (BUG###_DescriptiveName.cs)
- All public async methods accept CancellationToken
- Do not use preview versions of SDK and NuGet packages
- ...

## Architecture
- {Clean / Vertical Slice / etc.}
- Dependencies between layers: Domain → Application → Infrastructure
- Domain does not depend on Infrastructure

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...

## Conventions
- Naming: ...
- Commits: ...
- Tests: `dotnet run --project`, not `dotnet test`
```

### 2. Create `.cursor/rules/` (recommended for projects 50k+ LOC)

Split `.cursorrules` into context-dependent files:

```markdown
---
description: Domain layer rules
glob: src/**/Domain/**/*.cs
alwaysApply: false
---

# Domain Layer Rules

- Zero external dependencies (except standard library)
- All entities are immutable records or have private setters
- Migration is mandatory when changing the model
- No direct DB, HTTP, File IO calls
```

```markdown
---
description: Infrastructure layer rules
glob: src/**/Infrastructure/**/*.cs
alwaysApply: false
---

# Infrastructure Layer Rules

- Select() is mandatory in read-path (.Include() forbidden)
- FindAsync() only in write-path (Command handlers)
- Every cache.Set() — with size specified
- All services implement an interface from Application
```

```markdown
---
description: Test patterns and conventions
glob: tests/**/*.cs
alwaysApply: false
---

# Test Conventions

- TUnit + `dotnet run --project`
- Every bug is a file BUG###_DescriptiveName.cs
- Architecture tests: NetArchTest
- Do not use `dotnet test` ("0 tests ran" trap)
```

```markdown
---
description: Audit prompts
glob: ""
alwaysApply: false
---

# Audit Prompts

## Code Review
Conduct code review of current changes:
1. Get diff: git diff origin/main
2. Check only + lines
3. Look for: SQL injection, missing await, XSS, data leak
4. For each finding specify file:line + code quote
5. Issue verdict: APPROVED / CHANGES_REQUESTED

## Security Audit
Check files for data leaks:
- Logs: no PII, tokens, connection strings
- API responses: no extra fields
- Exception messages: no SQL, no FS paths
- All new endpoints are covered by authorization
```

## Running Onboarding

### Option A: Chat mode (Ctrl+L)

```
Scan this .NET project. Evaluate guardrails against the pyramid layers.
Output an implementation backlog. Consider that we use {stack}.
```

### Option B: Composer mode (Ctrl+I)

```
Scan this .NET project as a multi-step onboarding:
1. Find all `.csproj` and determine the stack
2. Assess guardrails for layers 1.1→2.3
3. Output an implementation backlog with Adapt / Create / Skip decisions
```

## What Onboarding Creates for Review

**Goal:** after the initial assessment, get a project-specific review artifact for Cursor instead of an abstract universal loop.

### 1. What onboarding decides

Onboarding should determine:

- whether general review rules in `.cursorrules` are enough
- whether review checks should be moved into `.cursor/rules/` by layer or context
- whether a dedicated project-specific review prompt / notepad is needed for PR review

### 2. What should appear in the project

- review rules in `.cursorrules` and/or `.cursor/rules/`
- a dedicated review prompt / notepad when the stack needs one
- explicit documentation of which checks were struck out as N/A and which were added as project-specific

### 3. When the scenario is successful

- Cursor receives review context from project files instead of a random chat prompt
- the team knows where the canonical review prompt for this project lives
- if the base rules do not fit, that is reflected in project-specific rules instead of being reinvented by each developer

### 4. Important boundary

Onboarding first fixes review rules and prompts in the project. Only after that does the team use them for PR review and refactoring tasks.

## Cursor Specifics

### What Differs from Kimi / Claude Code

| Aspect | Kimi | Claude Code | Cursor |
|--------|------|-------------|--------|
| Rules format | `.kimi/skills/{name}/SKILL.md` | `.claude/CLAUDE.md` | `.cursorrules` + `.cursor/rules/*.md` |
| Rule context | Manual skill selection | Project-wide + commands | Automatic by glob mask |
| Launch | `kimi run {name}` | `/{command}` in chat | Chat / Composer / Tab |
| IDE | CLI | CLI | VS Code-based GUI |
| Tools | Limited | Bash, edit, read | Inline edits, chat, composer |
| Context | ~200k tokens | ~200k tokens | ~200k tokens |

### Cursor Nuances

1. **Context-aware rules.** Cursor automatically connects rules from `.cursor/rules/` based on the `glob` of the file you are working on. This is the closest to hierarchical `AGENTS.md` — Domain layer rules connect only when editing Domain.

2. **`.cursorrules` vs `.cursor/rules/`.**
   - `.cursorrules` — legacy format, one file per project
   - `.cursor/rules/` — new format, supports multiple files with YAML frontmatter
   - It is recommended to use `.cursor/rules/` for large projects

3. **Inline edits.** Cursor can edit code directly in the editor (Tab completion, Cmd+K). This changes the anti-hallucination protocol format: the agent sees the context of the open file.

4. **Composer mode.** Multi-step tasks (creating a feature end-to-end) are better done in Composer, not Chat. Composer remembers context between steps.

5. **Notepads.** Cursor supports Notepads — markdown files with context that can be attached to chat. This is an analog of Kimi skills.

## Limitations

- No built-in "skill" system like Kimi — only project rules + notepads
- No skill marketplace
- No CLI interface for automatic launch (unlike `kimi run` or `claude`)
- Rules do not auto-launch — you need to explicitly work with the file for the rule to connect
- Context is limited by the model window (~200k tokens)
- Cannot execute bash commands unlike Claude Code
