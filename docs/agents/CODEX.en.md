# Codex (OpenAI) — Guardrails Integration

> Codex CLI from OpenAI uses `.codex/instructions.md` for project instructions.
> Minimalist approach: one instructions file + CLI prompts.

## Integration Structure

```
.codex/
└── instructions.md                # Single instructions file

# Optional:
codex.md                           # Alternative name (in root)
```

## Project Configuration

### Create `.codex/instructions.md`

```markdown
# Project Guardrails — {ProjectName}

## You are a .NET Developer
You work in the {ProjectName} project. Follow the rules below.

## Rules
- Do not add dependencies without explicit request
- Every bug fix comes with a regression test `BUG###_DescriptiveName`
- All public async methods accept `CancellationToken ct = default`
- Do not use `async void`
- Respect nullable reference types

## Architecture
- {Clean Architecture / Vertical Slice / etc.}
- Domain does not depend on Infrastructure
- API returns DTO, not Entity

## Stack
- .NET {version}
- {EF Core / Dapper / etc.}
- {TUnit / xUnit / NUnit}
- PostgreSQL / SQL Server

## Conventions
- Interfaces: `I{Name}`
- DTOs: record (immutable)
- Jobs: `{Name}Job`
- Tests: `[Test]` + `await Assert.That(value).IsEqualTo(expected)`

## Code Review Checklist
For any change check:
- [ ] No SQL injection (parameterized queries)
- [ ] No data leak in logs/responses
- [ ] Tests for new functionality
- [ ] CancellationToken is passed through
```

## Running Onboarding

```bash
# Install Codex CLI (if not already installed)
npm install -g @openai/codex

# Launch in the project
codex

# Inside the session:
> Scan this .NET project. Evaluate guardrails against the pyramid layers.
> Output an implementation backlog. Consider that we use {stack}.
```

## What Onboarding Creates for Review

**Goal:** after the first scan, get a project-specific review protocol for Codex instead of an abstract workflow.

### 1. What onboarding decides

Onboarding should determine:

- whether the review protocol inside `.codex/instructions.md` is sufficient
- whether a separate markdown review-prompt template is needed in the repo
- which checks must be removed or added for the real project stack

### 2. What should appear in the project

- an updated `.codex/instructions.md` with review rules for this specific project
- optionally `docs/guardrails-checklist.md` or a similar file containing the canonical review prompt
- a report explaining what was adapted from ready-made guardrails and what had to become project-specific

### 3. When the scenario is successful

- the team has one canonical review prompt for this project
- review rules already reflect the stack and architecture boundaries instead of staying generic
- if standard checks do not fit, that is explicitly documented in the project instructions

### 4. Important boundary

Onboarding first assembles the project-specific review protocol. Only after that does the team use the protocol in PR checks.

## Codex Specifics

### What Differs from Kimi / Claude

| Aspect | Kimi | Claude Code | Codex |
|--------|------|-------------|-------|
| Rules format | `.kimi/skills/*.md` | `.claude/CLAUDE.md` + commands | `.codex/instructions.md` |
| Number of files | Many (one per skill) | 1 + N commands | 1 file |
| Launch | `kimi run {name}` | `/{command}` | Direct prompt |
| Integration | Manual | Tools (bash, edit) | CLI-only |

### Codex Nuances

1. **One file.** The entire project constitution is in one `instructions.md`. This means:
   - Cannot split into separate "audits" like skills
   - Need to embed checklists directly into instructions
   - Can use markdown links to external documents

2. **No custom commands.** Codex does not support slash commands like Claude. All instructions are via prompt or `instructions.md`.

3. **Git integration.** Codex can work with git (sees diff, can commit). This simplifies code review — the agent can `git diff` itself.

4. **Direct terminal access.** Codex can execute commands via `!bash` or directly in CLI.

### instructions.md Format

Codex uses a single `.codex/instructions.md`. It should contain an adapted version of `rules/AGENTS_TEMPLATE.md`: role, forbidden actions, stack, architecture boundaries, and a review protocol. Do not copy the template verbatim — keep only the checks relevant to the project.

## Limitations

- No skill system — only one instructions file
- No built-in tools (read_file, edit_file) like Claude Code
- Works in "prompt → response" mode, agent does not always see the entire codebase
- No marketplace or extensibility
- Limited by context (depends on model, ~128k-200k tokens)
