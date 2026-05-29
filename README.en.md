# .NET Skeptical AI Engineering

Quality control methodology when working with AI agents. Based on the talk "AI is confident. I am not" (Dotnext 2026).

[🇷🇺 Русская версия](README.md)

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![License MIT](https://img.shields.io/badge/License-MIT-green.svg)
![CI](https://github.com/svetkis/dotnet-skeptical-ai/workflows/DemoProject%20CI/badge.svg)

This repository contains ready-made artifacts for .NET projects: rules, skills, test patterns, and CI workflows.

## How it works

Three verification loops: inner (on every change), outer (on schedule or before release), and artifact grooming (per sprint).

### Inner loop — 5 layers

| Layer | Speed | Tool |
|-------|-------|------|
| 0. Instructions | — | `rules/AGENTS.md` + Numbered Decisions |
| 1. Compiler | ~sec | `dotnet build`, `tsc --noEmit` |
| 2. Architecture | ~10 sec | NetArchTest |
| 3. Tests | ~30 sec | TUnit + `dotnet run` |
| 4. Code review | ~2 min | Separate agent |
| 5. E2E MCP | ~15 min | Telegram, browser, API tools |

### Outer loop — 3 levels

| Level | Frequency | Tool |
|-------|-----------|------|
| 1. Audits | Per sprint / on PR in risk area | Security, DBA, UX, perf, i18n |
| 2. Load | Before release | NBomber |
| 3. Manual | After release | Human |

`skills/` — ready-made prompts for audits. Run on schedule or when code changes in their area.

### Grooming loop

Agent artifacts degrade: AGENTS.md drifts from code, Auto Memory accumulates duplicates, backlog becomes a graveyard.

| Skill | What it cleans | Frequency |
|-------|---------------|-----------|
| memory-hygiene | Auto Memory: duplicates, drift, stale refs | Per sprint |
| doc-hygiene | AGENTS.md: hierarchy consistency, code drift | Per sprint |
| backlog-hygiene | Backlog: stale, orphaned, duplicates | Per sprint |

## Quick start

```bash
# 1. Clone
git clone https://github.com/svetkis/dotnet-skeptical-ai.git

# 2. Run DemoProject
cd examples/DemoProject
dotnet build
dotnet run --project tests/DemoProject.Tests

# 3. Assess your project
# Open skills/skeptical-ai-bootstrap/SKILL.md and run the checklist —
# figure out what you already have and what to implement first.

# 4. Adapt skills to your stack
# See skills/ADAPTATION.md — cross out irrelevant checks.

# 5. Copy needed artifacts
cp rules/AGENTS.md /your/project/
cp -r skills/code-review /your/project/.kimi/skills/
cp tests/patterns/*.cs /your/project/tests/
```

## Structure

```
.
├── AGENTS.md                     # Instructions for AI agents
├── PYRAMID.md                    # Detailed breakdown of layers
├── rules/
│   ├── AGENTS.md                 # EF rules, naming, conventions
│   └── CONVENTIONS.md            # Commits, workflow, tests
├── skills/                        # Agent roles
│   ├── memory-hygiene/            # Grooming: Auto Memory
│   ├── doc-hygiene/               # Grooming: documentation
│   ├── backlog-hygiene/           # Grooming: backlog
│   ├── skeptical-ai-bootstrap/    # Maturity assessment + guardrails backlog
│   ├── code-review/               # Inner loop: review every PR
│   ├── task-compliance/           # Inner loop: scope check
│   ├── security-audit/            # Outer loop: trigger-based
│   ├── dba-audit/                 # Outer loop: trigger-based
│   ├── api-design-audit/          # Outer loop: trigger-based
│   ├── bot-audit/                 # Outer loop: trigger-based
│   ├── performance-audit/         # Outer loop: trigger-based
│   └── i18n-audit/                # Outer loop: trigger-based
├── docs/
│   ├── traps/                     # Agent traps
│   └── solutions/
│       ├── architecture-tests.md  # Guide to arch tests
│       └── ai-patterns.md         # 10 AI-driven development patterns
├── tests/
│   ├── patterns/                  # Test templates (Ratchet, NetArchTest, NBomber)
│   └── conventions/               # Naming, TUnit guide
├── ci/                            # CI/CD guardrails
└── examples/
    └── DemoProject/               # Working .NET 10 example
```

## DemoProject

`examples/DemoProject/` is a working .NET 10 example with all patterns:

- Clean Architecture (Domain → Application → Infrastructure)
- NetArchTest: layer dependency checks
- Ratchet tests: public type and test count control
- Snapshot tests: JSON serialization contracts
- NBomber: load tests (read + write mix)
- TUnit: run via `dotnet run --project`

```bash
cd examples/DemoProject
dotnet build
dotnet run --project tests/DemoProject.Tests
```

## Navigation

Lost? Start with [docs/README.md](docs/README.md).

| What you need | Where to go |
|---------------|-------------|
| Agent rules | `rules/AGENTS.md` |
| Security audit | `skills/security-audit/` |
| DBA audit | `skills/dba-audit/` |
| Performance audit | `skills/performance-audit/` |
| i18n audit | `skills/i18n-audit/` |
| Code review agent | `skills/code-review/` |
| Scope check | `skills/task-compliance/` |
| Test pattern | `tests/patterns/` |
| CI security | `ci/github-actions/safe-ci.yml` |
| Agent traps | `docs/traps/` |
| Auto Memory grooming | `skills/memory-hygiene/` |
| Doc grooming | `skills/doc-hygiene/` |
| Backlog grooming | `skills/backlog-hygiene/` |
| Architecture tests | `docs/solutions/architecture-tests.md` |
| AI patterns | `docs/solutions/ai-patterns.md` |
| Project onboarding | `skills/skeptical-ai-bootstrap/` |
| Kimi integration | `docs/agents/KIMI.md` |
| Claude Code integration | `docs/agents/CLAUDE-CODE.md` |
| Codex integration | `docs/agents/CODEX.md` |
| OpenCode integration | `docs/agents/OPENCODE.md` |
| Agent comparison | `docs/agents/README.md` |

## Author

**Svetlana Meleshkina** — creator of the Skeptical AI Engineering methodology, speaker at Dotnext 2026.

- 💬 Telegram channel: [@kot_review](https://t.me/kot_review)
- ✉️ Telegram: [@svetkis](https://t.me/svetkis)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). We accept new skills, test patterns, traps, and agent integrations.

## License

[MIT](LICENSE)
