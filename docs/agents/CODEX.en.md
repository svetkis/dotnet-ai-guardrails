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
> Scan this .NET project. Evaluate guardrails against the 5 layers.
> Output an implementation backlog. Consider that we use {stack}.
```

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

### Recommended instructions.md Structure

```markdown
# {ProjectName} — Guardrails

## Role
You are a senior .NET developer. You write production-ready code.

## Rules (DO NOT break)
1. Do not add NuGet packages without asking
2. Do not change folder structure
3. Do not delete tests
4. Always pass CancellationToken
5. Respect nullable (string? vs string)

## Code Review Protocol
Before saying "done", check:
- [ ] All new public methods are async with CancellationToken
- [ ] No SQL injection
- [ ] No PII leak in logs/responses
- [ ] Tests exist
- [ ] Architecture boundaries are not violated

## Stack Context
- .NET 10
- TUnit
- EF Core + PostgreSQL
- Minimal API
- Clean Architecture

## Conventions
- ...
```

## Onboarding for Codex

Since Codex has no "skills", onboarding is **generating `instructions.md`** + **checklists**.

### What the agent does during onboarding:

1. Scans the project (stack, architecture, tests)
2. Generates `.codex/instructions.md` with:
   - Agent role
   - Rules (based on stack)
   - Code Review Protocol
   - Stack Context
3. Generates `docs/guardrails-checklist.md` for the human developer
4. Generates `.github/workflows/codex-guard.yml` (CI pipeline)

### Example CI for a Codex Project

```yaml
# .github/workflows/codex-guard.yml
name: Codex Guardrails

on: [pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true

  tests:
    runs-on: ubuntu-latest
    needs: build
    steps:
      - uses: actions/checkout@v4
      - run: dotnet run --project tests/UnitTests/
      - run: ./ci/scripts/verify-tests.sh
```

## Limitations

- No skill system — only one instructions file
- No built-in tools (read_file, edit_file) like Claude Code
- Works in "prompt → response" mode, agent does not always see the entire codebase
- No marketplace or extensibility
- Limited by context (depends on model, ~128k-200k tokens)
