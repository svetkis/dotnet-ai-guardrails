# Installing the Skeptical AI Bootstrap Skill

## Why

This skill is installed into **your** .NET project so that Kimi Code CLI can scan it and produce a guardrail implementation backlog.

## Quick Install

### 1. Copy the skill into your project

From the `dotnet-skeptical-ai` repository, copy the folder into your project:

```bash
# From the root of YOUR .NET project
cp -r /path/to/dotnet-skeptical-ai/skills/skeptical-ai-bootstrap ./.kimi/skills/
```

Or manually:
- Create `.kimi/skills/skeptical-ai-bootstrap/` in your project
- Copy `SKILL.md`, `CHECKLIST.md`, `EXAMPLE-REPORT.md`

**Important:** pick a language. The target project should have only one language:
- **Russian** → copy `SKILL.md` (RU), don't copy `SKILL.en.md`
- **English** → copy `SKILL.en.md` and rename to `SKILL.md`

### 2. Make sure Kimi Code CLI sees the skill

```bash
kimi skills list
```

`skeptical-ai-bootstrap` should appear in the list.

### 3. Run onboarding

```bash
kimi run skeptical-ai-bootstrap
```

Or in chat with Kimi:
```
@skeptical-ai-bootstrap scan this project in standard mode
```

### 4. Get the report

The agent will generate a `.backlog/onboarding-{date}.md` file in your project + output a summary in chat.

## Alternative: onboarding without installing the skill

If you don't want to install the skill, simply open your project in Kimi Code CLI and ask:

```
Scan this .NET project using the 5-layer pyramid methodology from dotnet-skeptical-ai.
Produce a guardrail implementation backlog.
```

The agent will find `.csproj`, assess layers, and propose a plan.

## After Onboarding

The report contains links to artifacts from `dotnet-skeptical-ai`:

| Artifact | Where to get |
|----------|--------------|
| `rules/AGENTS_TEMPLATE.md` | `dotnet-skeptical-ai/rules/AGENTS_TEMPLATE.md` |
| `rules/CONVENTIONS.md` | `dotnet-skeptical-ai/rules/CONVENTIONS.md` |
| Architecture tests | `dotnet-skeptical-ai/tests/patterns/ArchitectureRules.cs` |
| Ratchet tests | `dotnet-skeptical-ai/tests/patterns/RatchetTest.cs` |
| CI workflow | `dotnet-skeptical-ai/ci/github-actions/safe-ci.yml` |
| Code review skill | `dotnet-skeptical-ai/skills/code-review/` (RU or EN — one language) |
| Audits | `dotnet-skeptical-ai/skills/*-audit/` (RU or EN — one language) |
| Grooming | `dotnet-skeptical-ai/skills/memory-hygiene/`, `doc-hygiene/`, `backlog-hygiene/` (RU or EN — one language) |

**Recommendation:** fork `dotnet-skeptical-ai` and reference artifacts from your fork — this way you control versions.

## Agent Selection

The skill automatically determines which AI agent is used in the project:
- **Kimi Code CLI** → `.kimi/skills/`
- **Claude Code** → `.claude/CLAUDE.md` + commands
- **Codex** → `.codex/instructions.md`
- **OpenCode** → `.opencode/`
- **Multiple** → universal `AGENTS.md` + specific configs

See `docs/agents/` for details on each agent:
- `docs/agents/KIMI.md`
- `docs/agents/CLAUDE-CODE.md`
- `docs/agents/CODEX.md`
- `docs/agents/OPENCODE.md`

## For Agents

**If you are an AI agent executing this skill:**

1. **Do NOT create demo projects.** Do not create `examples/`, `DemoProject/`, or new `.csproj`/`.sln`.
2. **Do NOT copy the folder structure** of this repository (`rules/`, `skills/`, `tests/patterns/`) into the target project.
3. **Your output is markdown only:** reports, checklists, `.backlog/*.md`, `AGENTS.md`, `CONVENTIONS.md`.
4. **Your task:** read target project → assess → plan. Do not write code "as an example" or "to demonstrate".

## Modes

- `fast` — only critical (1-2 days)
- `standard` — all pyramid layers (1-2 weeks)
- `paranoid` — everything + outer loop (1 month)
