# AI Agents Integration

> This directory contains instructions for integrating guardrails
> with various AI agents and development tools.
>
> Each agent has its own ecosystem, configuration format, and nuances.
> Choose the file matching your tool.

## Bootstrap Protocol (read this first)

Before configuring any agent — read [BOOTSTRAP-PROTOCOL.md](BOOTSTRAP-PROTOCOL.md).  
It prevents situations where an agent tries to create a `DemoProject` in the target repo instead of assessing the existing codebase.

## Available Agents

| Agent | File | Configuration Format | Features |
|-------|------|---------------------|----------|
| **Kimi Code CLI** | [KIMI.md](KIMI.md) | `.kimi/skills/{name}/SKILL.md` | Skill system, marketplace |
| **Claude Code** | [CLAUDE-CODE.md](CLAUDE-CODE.md) | `.claude/CLAUDE.md` + commands | Tools (bash, edit, read) |
| **Cursor** | [CURSOR.md](CURSOR.md) | `.cursorrules` + `.cursor/rules/` | IDE-integrated, context-aware rules |
| **Codex (OpenAI)** | [CODEX.md](CODEX.md) | `AGENTS.md` + `~/.codex/config.toml` | AGENTS.md standard, skills, custom agents |
| **OpenCode** | [OPENCODE.md](OPENCODE.md) | `.opencode/instructions.md` | Open-source, self-hosted |

## How to Read These Guides

Each guide focuses on one practical question: **what onboarding should create for review**.
Run onboarding first, then read the "What Onboarding Creates for Review" section.

## Universal Approach

If multiple agents are used in the project or the agent is unknown —
use the universal `AGENTS.md` in the project root:

```markdown
# AGENTS.md — {ProjectName}

> This file is read by ANY AI agent working in the project.

## Rules (universal)
1. Do not add dependencies without explicit request
2. ...

## Stack
- .NET {version}
- ...
```

## How to Choose the Format

1. **Identify which agent is used** (or will be used)
2. **Read the corresponding file** from the table above
3. **Follow the instructions** for the configuration structure
4. **Use `skeptical-ai-bootstrap`** for automatic scanning:
   - It determines the agent type
   - Generates configuration in the correct format
   - Creates agent-specific artifacts

## Agent Comparison

| Characteristic | Kimi | Claude Code | Cursor | Codex | OpenCode |
|----------------|------|-------------|--------|-------|----------|
| **Rules format** | Multiple files (skills) | 1 file + commands | `.cursorrules` + rules/ | 1 file | Depends on implementation |
| **Launch** | `kimi run {name}` | `/{command}` | Chat / Composer | Direct prompt | CLI / IDE |
| **Tools** | Limited | Bash, edit, read | Inline edits | CLI-only | Depends on implementation |
| **Context** | Model-dependent (check current docs) | Model-dependent | Model-dependent | Model-dependent | Model-dependent |
| **Open Source** | No | No | No | No | Yes |
| **Self-hosted** | No | No | No | No | Possible |

## Recommendation

If you are just starting — use **Kimi Code CLI**, **Claude Code**, or **Cursor**.
They have the most mature ecosystems for guardrails.

- **Kimi / Claude Code** — for CLI-first workflow, scripts, CI integration
- **Cursor** — for IDE-first workflow, inline edits, context-dependent rules

If privacy or self-hosting is important — look at **OpenCode** with local models.

## Guide Freshness (drift control)

Agent configuration formats change fast. Every guide in this directory must carry a
`last_verified:` header with the date and a link to the vendor's primary documentation.

- A guide with `last_verified` older than **6 months** is stale: re-verify its
  configuration format against primary docs before relying on it, then bump the date.
- Claims about context windows, model names, and pricing do not belong here — they
  drift weekly; link to primary docs instead.
- Freshness of integration guides is checked by the `doc-hygiene` skill
  (Control Maintenance).
