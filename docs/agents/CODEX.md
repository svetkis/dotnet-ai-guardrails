# Codex (OpenAI) — Guardrails Integration

> **last_verified:** 2026-07-14 (Codex CLI ≥ 0.117)
> **Primary documentation:**
> [developers.openai.com/codex](https://developers.openai.com/codex) ·
> [AGENTS.md guide](https://developers.openai.com/codex/guides/agents-md) ·
> [Config reference](https://developers.openai.com/codex/config-reference) ·
> [CLI reference](https://developers.openai.com/codex/cli/reference) ·
> [openai/codex on GitHub](https://github.com/openai/codex)
>
> Codex reads project instructions from `AGENTS.md` (open cross-tool standard),
> layered from the repo root down to nested directories. Configuration lives in
> `~/.codex/config.toml` (user) and `.codex/config.toml` (project, trusted repos only).
> Skills and custom agent roles provide extensibility — see below.

## Integration Structure

```
AGENTS.md                          # Project instructions (repo root, primary)
{subdir}/AGENTS.md                 # Nested instructions (closer file wins)
{subdir}/AGENTS.override.md        # Temporary local override

.codex/
├── config.toml                    # Project config (loaded only for trusted repos)
└── agents/
    └── reviewer.toml              # Optional custom agent role

~/.codex/
├── AGENTS.md                      # Global user instructions (all repos)
├── config.toml                    # User config: model, sandbox, approvals, MCP
└── skills/                        # User-level skills
```

> **Legacy:** older guides referenced `.codex/instructions.md` and custom prompts in
> `~/.codex/prompts/`. Custom prompts were removed in Codex CLI 0.117.0 — convert them
> to skills. Use `AGENTS.md` for project instructions.

## Project Configuration

### Create `AGENTS.md` in the repo root

Start from [`rules/AGENTS_TEMPLATE.md`](../../rules/AGENTS_TEMPLATE.md) and keep only
the checks relevant to the project — do not copy the template verbatim.

```markdown
# Project Guardrails — {ProjectName}

## You are a .NET Developer
You work in the {ProjectName} project. Follow the rules below.

## Rules
- Do not add dependencies without explicit request
- Every reproducible bug fix comes with a regression test `BUG###_DescriptiveName`
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

**Goal:** after the first scan, define a Codex review protocol tied to the project.

### 1. What onboarding decides

- whether the review protocol inside `AGENTS.md` is sufficient, or whether it belongs
  in a skill (`.agents/skills/`) so it can be invoked on demand instead of being always loaded
- which sections of the review protocol need adaptation for the project stack
- which checks must be removed or added for the real project stack

### 2. What should appear in the project

- an updated `AGENTS.md` with review rules for this specific project
- optionally a review skill under `.agents/skills/` (registered via `[[skills.config]]`
  in `.codex/config.toml`) for pre-commit review on demand
- a report explaining what was adapted from ready-made guardrails and what had to
  become project-specific

### 3. When the scenario is successful

- the review protocol is recorded in `AGENTS.md` or a registered skill
- review rules already reflect the stack and architecture boundaries instead of staying generic
- if standard checks do not fit, that is explicitly documented in `AGENTS.md`

### 4. Important boundary

Onboarding first assembles the project-specific review protocol. Only after that does
the team use the protocol in PR checks.

## Codex Specifics

### What Differs from Kimi / Claude

| Aspect | Kimi | Claude Code | Codex |
|--------|------|-------------|-------|
| Rules format | `.kimi/skills/*.md` | `.claude/CLAUDE.md` + commands | `AGENTS.md` (root + nested) |
| User config | — | `.claude/settings.json` | `~/.codex/config.toml` |
| Project config | — | `.mcp.json`, settings | `.codex/config.toml` (trusted repos) |
| Extensibility | skills | skills, subagents, hooks | skills (`.agents/skills/`), custom agents (`.codex/agents/*.toml`), MCP, hooks |
| Launch | `kimi run {name}` | `/{command}` | Direct prompt, `codex exec` (non-interactive), slash commands in TUI |

### Codex Nuances

1. **Layered instructions.** `AGENTS.md` files chain from the repo root down to the
   working directory — the closer file wins. Put global constitution in the root
   `AGENTS.md`, module-specific rules in nested `AGENTS.md`, and temporary deviations
   in `AGENTS.override.md`.
2. **Skills, not one file.** Audits and review protocols do not have to live inside
   `AGENTS.md`: register them as skills (`.agents/skills/<name>/SKILL.md`) via
   `[[skills.config]]` in `.codex/config.toml`. This is the Codex counterpart of the
   skill layout in `templates/skills/`.
3. **Custom agent roles.** An independent reviewer can be defined as a custom agent in
   `.codex/agents/*.toml` — a separate context with its own instructions, which is
   closer to "separation of control" than asking the same session to review itself.
4. **Git and terminal access.** Codex can read diffs, commit, and execute commands
   under the configured sandbox/approval policy — the agent can run `git diff` and
   the test command itself.
5. **Non-interactive mode.** `codex exec` runs a prompt headlessly — usable in CI
   review jobs.

### AGENTS.md Format

Codex uses `AGENTS.md` as the single project-instruction standard. It should contain
an adapted version of `rules/AGENTS_TEMPLATE.md`: role, forbidden actions, stack,
architecture boundaries, and a review protocol. Long audit checklists belong in skills,
referenced from `AGENTS.md`, not pasted into it.

## Limitations

- Context window and available models depend on the configured model and plan —
  check the current values in the [config reference](https://developers.openai.com/codex/config-reference)
  rather than relying on fixed numbers.
- Project `.codex/config.toml` is loaded only for repositories marked as trusted;
  the VS Code extension may not apply all repo-local config keys (known gaps are
  tracked in `openai/codex` issues).
- Skills and custom agents are evolving mechanisms — verify the exact config keys
  (`[[skills.config]]`, `agents.<name>.*`) against the current official docs before
  committing them to a team setup.
