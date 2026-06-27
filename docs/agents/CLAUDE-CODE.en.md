# Claude Code — Guardrails Integration

> Claude Code (Anthropic) uses `CLAUDE.md` for project instructions
> and `.claude/commands/` for custom slash commands.
> It has a different mental model: not "skills", but "project instructions + commands".

## Integration Structure

```
.claude/
├── CLAUDE.md                      # Main constitution (analog of AGENTS.md)
├── settings.json                  # Project settings
└── commands/                      # Custom slash commands
    ├── code-review.md             # /code-review
    ├── task-compliance.md         # /task-compliance
    ├── security-audit.md          # /security-audit
    ├── complexity-audit.md        # /complexity-audit
    ├── allocation-budget-audit.md # /allocation-budget-audit
    └── {project-specific}.md      # Custom commands
```

## Project Configuration

### 1. Create `CLAUDE.md`

This is the analog of `AGENTS.md` + `CONVENTIONS.md` combined:

```markdown
# Project Guardrails — {ProjectName}

## Mission
{project description}

## Rules for Claude
- Do not add dependencies without explicit request
- Do not change folder structure without agreement
- Every bug fix comes with a regression test
- All public async methods accept CancellationToken
- ...

## Architecture
- {Clean / Vertical Slice / etc.}
- Dependencies between layers: ...

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...

## Conventions
- Naming: ...
- Commits: ...
- Tests: ...
```

### 2. Create `.claude/settings.json`

```json
{
  "project": {
    "instructions": "CLAUDE.md"
  },
  "permissions": {
    "allow_bash": true,
    "allow_edit": true,
    "allow_read": true
  }
}
```

### 3. Create custom commands

Each command is a Markdown file in `.claude/commands/`:

```markdown
# code-review

## Description
Conduct code review of current changes.

## Instructions
1. Get diff: `git diff origin/main`
2. Check only `+` lines
3. Look for: SQL injection, missing await, XSS, data leak
4. For each finding specify file:line + code quote
5. Issue verdict: APPROVED / CHANGES_REQUESTED

## Severity
- BLOCKER: Security, data loss, compilation error
- CRITICAL: async void, missing CancellationToken, race condition
- MAJOR: Missing test, exception swallowing
- MINOR: Naming, magic number
```

Launch: `/code-review` inside Claude Code.

## Running Onboarding

```bash
# Launch Claude Code in the project
claude

# Inside the session:
> Scan this .NET project. Evaluate guardrails against the pyramid layers.
> Output an implementation backlog. Consider that we use {stack}.
```

Or create a custom command:

```markdown
# .claude/commands/onboarding.md

## Description
Scan the project and output a guardrails implementation backlog.

## Instructions
1. Find all `.csproj` and determine the stack
2. Evaluate layers 1.1→2.3:
   - 1.1 Compiler: TreatWarningsAsErrors? Nullable?
   - 1.2 Architecture: any arch tests?
   - 1.3 Tests: framework, coverage, "0 ran"?
   - 1.4 Code Review: any rules?
   - 1.5 Smoke: any critical scenario runs?
   - 2.1 E2E / MCP: any integration tests?
   - 2.2 Audits: have any been run?
   - 2.3 Load: any load tests?
3. For each layer: Adapt / Create / Skip
4. Output a report in markdown format
```

## What Onboarding Creates for Review

**Goal:** after project assessment, define a concrete Claude review command for your stack.

### 1. What onboarding decides

Onboarding should determine:

- whether the standard `code-review` command is enough
- whether the template must be adapted for the project stack
- whether a separate review command is needed, such as `code-review-dapper` or `code-review-razor`

### 2. What should appear in the project

- `.claude/commands/code-review.md` or `.claude/commands/code-review-{context}.md`
- an updated `CLAUDE.md` if review rules also need to be captured in the shared constitution
- a report or backlog item explaining what was adapted and what did not fit from the ready-made artifacts

### 3. When the scenario is successful

- the team has an exact slash command for review in this project
- the team knows why it is this command and not some other variant
- if the standard template does not fit, that is reflected in an explicit project-specific command rather than informal adaptation

### 4. Important boundary

Onboarding first creates or adapts the review command for the project. Only after that does the command become part of the PR flow.

## Claude Code Specifics

### What Differs from Kimi

| Aspect | Kimi | Claude Code |
|--------|------|-------------|
| Rules format | `.kimi/skills/{name}/SKILL.md` | `.claude/CLAUDE.md` + `.claude/commands/*.md` |
| Skill launch | `kimi run {name}` | `/{command-name}` in chat |
| Context | Limited by window | 200k tokens + tools (read, edit, bash) |
| CI integration | Manual launch | Can run bash scripts |
| Auto-launch | No | No (but has tools) |

### Claude Code Nuances

1. **CLAUDE.md — single file.** You cannot split into 10 separate skills like in `.kimi/skills/`. But you can do:
   - Main `CLAUDE.md` with constitution
   - `.claude/commands/` with specific tasks (audit, review)

2. **Tools.** Claude Code has built-in tools:
   - `read_file` — reads files
   - `edit_file` — edits files
   - `bash` — executes commands
   - This changes the anti-hallucination protocol format: the agent actually sees files

3. **Context management.** Claude can `/add` files to context and `/compact` history. This is implicit in skills.

4. **No skill installation required.** Just create files in `.claude/` — and they work.

## Limitations

- No built-in "skill" system like Kimi — only project instructions + commands
- No skill marketplace
- Every new project requires manual creation of `.claude/`
- Does not automatically read `.claude/commands/` on startup (needs explicit `/command` call)
