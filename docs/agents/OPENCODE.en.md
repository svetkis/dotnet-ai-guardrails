# OpenCode — Guardrails Integration

> OpenCode — open-source agent for development (open-source alternative to proprietary tools).
> Can work as a VS Code extension or standalone.
> Configuration format depends on the specific implementation.

## Integration Structure

OpenCode typically uses one of the formats:

### Option A: Markdown instructions (similar to Codex)

```
.opencode/
└── instructions.md                # Project instructions
```

### Option B: JSON/YAML config

```
.opencode/
├── config.json                    # Settings
├── prompts/
│   ├── code-review.md
│   ├── architecture-audit.md
│   └── {custom}.md
└── rules/
    └── guardrails.json
```

### Option C: VS Code workspace settings

```
.vscode/
└── settings.json                  # OpenCode extension settings
```

## Project Configuration

### Universal Approach (recommended)

Create `.opencode/instructions.md` + a set of prompt files:

```markdown
# Project Guardrails — {ProjectName}

## Role
You are a .NET developer in the {ProjectName} project.

## Rules
- Do not add dependencies without explicit request
- Follow Clean Architecture / Vertical Slice / {arch}
- Every bug fix comes with a regression test
- ...

## Code Review Protocol
For any change:
1. Check diff for SQL injection
2. Check nullable reference types
3. Make sure CancellationToken is passed through
4. Check that there is no data leak

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}
- ...
```

### If OpenCode supports prompt files:

```
.opencode/
├── instructions.md                # Base rules
└── prompts/
    ├── onboarding.md              # Project scanning
    ├── code-review.md             # PR review
    ├── security-audit.md          # Security audit
    ├── complexity-audit.md        # Complexity audit
    ├── allocation-budget-audit.md # Hot path allocation audit
    └── architecture-audit.md      # Architecture audit
```

## Running Onboarding

Depends on the OpenCode implementation:

```bash
# If CLI
opencode --prompt .opencode/prompts/onboarding.md

# If VS Code extension
# Open Command Palette → OpenCode: Run Prompt → onboarding
```

## OpenCode Specifics

### What Differs from Proprietary Agents

| Aspect | Kimi | Claude Code | Codex | OpenCode |
|--------|------|-------------|-------|----------|
| Format | `.kimi/skills/*.md` | `.claude/CLAUDE.md` | `.codex/instructions.md` | Depends on implementation |
| Open Source | No | No | No | Yes |
| Self-hosted | No | No | No | Possible |
| Model choice | Fixed | Claude | GPT-4o | Any (Ollama, etc.) |
| Integration | CLI | CLI + IDE | CLI | CLI + IDE |

### OpenCode Nuances

1. **Non-standard format.** Since OpenCode is open-source, different forks may have different configuration formats. It is recommended to:
   - Use simple Markdown (universal)
   - Keep instructions in one place
   - Document the format in the project's `README.md`

2. **Self-hosted models.** If OpenCode works with local models (Ollama, LM Studio):
   - Context may be smaller (4k-32k tokens)
   - Instructions should be shorter and more specific
   - More examples are needed (few-shot learning)

3. **Plugin architecture.** OpenCode may support plugins:
   - You can write a plugin to run `dotnet test`
   - You can write a plugin for NetArchTest
   - You can write a plugin for verify-tests

## Recommended Format for OpenCode

Since OpenCode has no single standard, a **hybrid approach** is recommended:

```
{project-root}/
├── AGENTS.md                      # Universal constitution (read by all agents)
├── CONVENTIONS.md                 # Naming, workflow
├── .opencode/
│   ├── instructions.md            # Brief instructions for OpenCode
│   └── prompts/                   # Prompt files for tasks
│       ├── onboarding.md
│       ├── code-review.md
│       └── security-audit.md
├── .kimi/skills/                  # If Kimi is used
├── .claude/                       # If Claude Code is used
└── .codex/instructions.md         # If Codex is used
```

### Universal Constitution `AGENTS.md`

```markdown
# AGENTS.md — {ProjectName}

> This file is read by ANY AI agent working in the project.
> Format: Markdown, tool-independent.

## Rules (universal)
1. Do not add dependencies without explicit request
2. Do not change folder structure
3. Do not delete tests
4. Every bug fix comes with a regression test `BUG###_`

## Stack
- .NET {version}
- {EF Core / Dapper}
- {TUnit / xUnit}

## Architecture
- {Clean / Vertical Slice / etc.}
- Layer boundaries: ...

## Conventions
- ...
```

## Onboarding for OpenCode

The agent during onboarding:

1. Determines which format OpenCode uses in the project (asks or checks `.opencode/`)
2. Generates configuration in the correct format
3. Creates `AGENTS.md` (universal) + `.opencode/instructions.md` (specific)
4. Generates prompt files for frequent tasks

## Limitations

- No single configuration standard
- May require adaptation for a specific fork/version
- Self-hosted models may poorly follow long instructions
- Fewer built-in tools compared to Claude Code
