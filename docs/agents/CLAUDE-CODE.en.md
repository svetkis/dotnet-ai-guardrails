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
> Scan this .NET project. Evaluate guardrails against the 5 pyramid layers.
> Output an implementation backlog. Consider that we use {stack}.
```

Or create a custom command:

```markdown
# .claude/commands/onboarding.md

## Description
Scan the project and output a guardrails implementation backlog.

## Instructions
1. Find all `.csproj` and determine the stack
2. Evaluate 5 pyramid layers:
   - Compiler: TreatWarningsAsErrors? Nullable?
   - Architecture: any arch tests?
   - Tests: framework, coverage, "0 ran"?
   - Code Review: any rules?
   - E2E: any integration tests?
3. For each layer: Adapt / Create / Skip
4. Output a report in markdown format
```

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
