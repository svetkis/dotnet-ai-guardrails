# .NET Skeptical AI Engineering

AI-accelerated quality control methodology for .NET teams. Extending DORA practices: if AI can assist review, it can scale audits, load testing, and architecture guardrails too. Based on the talk "AI is confident. I am not" (Dotnext 2026).

[🇷🇺 Русская версия](README.md)

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![License MIT](https://img.shields.io/badge/License-MIT-green.svg)
![CI](https://github.com/svetkis/dotnet-skeptical-ai/workflows/Examples%20CI/badge.svg)

> Although examples and tests are implemented in .NET, the methodology itself — Decision Guards, three-layer verification loops, and prompt hygiene — applies to any stack.

This repository contains ready-made artifacts for .NET projects: rules, skills, test patterns, and CI workflows.

## Problem

AI agents (Cursor, Claude, Copilot) speed up code writing, but generate hidden tech debt, violate architectural boundaries, and break security. Manual review of such code becomes a bottleneck.

**Skeptical AI** is a Zero-Trust methodology for generated code. Control moves from probabilistic prompts into deterministic pipelines.

## How it works

Three verification loops: inner (on every change), outer (on schedule or before release), and artifact grooming (per sprint).

### Layer 1. Development cycle (fast feedback)

| Sub-layer | Speed | Tool |
|-----------|-------|------|
| 0. Instructions | — | `rules/AGENTS_TEMPLATE.md` + Decision Guards (ADR) |
| 1.1 Compiler | ~sec | `dotnet build`, `tsc --noEmit` |
| 1.2 Architecture | ~10 sec | NetArchTest |
| 1.3 Tests | ~30 sec | TUnit + `dotnet run` |
| 1.4 Pre-commit code review | ~2 min | Separate agent (staged diff) |
| 1.5 Smoke | ~5 min | 10 critical scenarios |

### Layer 2. Acceptance cycle

| Sub-layer | Frequency | Tool |
|-----------|-----------|------|
| 2.1 E2E MCP | Before release | Telegram, browser, API tools |
| 2.2 Audits | Per sprint / on PR in risk area | Security, DBA, UX, perf, i18n |
| 2.3 Load | Before release | NBomber |

### Outer loop

| Level | Frequency | Tool |
|-------|-----------|------|
| Human | After release | Business and product decisions |

`templates/skills/` — ready-made prompts for audits. Run on schedule or when code changes in their area.

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
# Open .agents/skills/skeptical-ai-bootstrap/SKILL.md and run the checklist —
# figure out what you already have and what to implement first.

# 4. Adapt skills to your stack
# See templates/skills/ADAPTATION.md — cross out irrelevant checks.

# 5. Copy needed artifacts
cp rules/AGENTS_TEMPLATE.md /your/project/
cp -r templates/skills/code-review /your/project/.kimi/skills/
# For React/TypeScript frontend:
# cp -r templates/skills/frontend-code-review /your/project/.kimi/skills/
cp tests/patterns/*.cs /your/project/tests/
```

## Structure

```
.
├── AGENTS.md                     # Instructions for AI agents
├── PYRAMID.md                    # Detailed breakdown of layers
├── rules/
│   ├── AGENTS_TEMPLATE.md        # Base constitution for agents (universal)
│   ├── AGENTS_TEMPLATE.efcore.md # Add-on: EF Core-specific rules
│   ├── AGENTS_TEMPLATE.dapper.md # Add-on: Dapper / Raw SQL-specific rules
│   └── CONVENTIONS.md            # Commits, workflow, tests
├── templates/skills/                        # Agent roles
│   ├── memory-hygiene/            # Grooming: Auto Memory
│   ├── doc-hygiene/               # Grooming: documentation
│   ├── backlog-hygiene/           # Grooming: backlog
│   ├── skeptical-ai-bootstrap/    # Maturity assessment + guardrails backlog
│   ├── code-review/               # Inner loop: pre-commit / PR review (.NET)
│   ├── task-compliance/           # Inner loop: scope check
│   ├── security-audit/            # Outer loop: trigger-based
│   ├── dba-audit/                 # Outer loop: trigger-based (EF Core)
│   ├── dba-audit-dapper/          # Outer loop: trigger-based (Dapper / Raw SQL)
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
    ├── DemoProject/               # Working .NET 10 example (Clean Architecture)
    ├── DemoProject.MinimalApi/    # Single-project MVP (Minimal API, no layers)
    └── DemoProject.Traps/         # Intentionally broken code — guardrails demo
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

## DemoProject.Traps

`examples/DemoProject.Traps/` — intentionally broken code demonstrating guardrails in action. Every test here fails, showing what an architectural test catches when an agent violates the rules.

```bash
cd examples/DemoProject.Traps
dotnet run --project tests/DemoProject.Traps.Tests
```

**What breaks:**
- `MutableState` — mutable state in Domain
- `DomainLeakingToInfra` — Domain depends on `System.Net.Http`
- `PaymentService` — direct dependency between Features (Orders → Payments)
- `Modules/` — cyclic dependencies between modules (ArchUnitNET)
- `RawGuidEntity` — raw `Guid` instead of strongly typed ID

See also [`examples/DemoProject.Traps/README.md`](examples/DemoProject.Traps/README.md).

## DemoProject.MinimalApi

`examples/DemoProject.MinimalApi/` — a variant for **Minimal API without Clean Architecture**. Shows how to adapt guardrails when there are no Domain / Application / Infrastructure layers.

```bash
cd examples/DemoProject.MinimalApi
dotnet build
dotnet run --project tests/DemoProject.MinimalApi.Tests
```

**What's inside:**
- Naming conventions, banned APIs (`DateTime.Now`)
- `CancellationToken` guard
- Ratchet tests for public types
- Duplication guard for business logic

See also [`examples/DemoProject.MinimalApi/README.md`](examples/DemoProject.MinimalApi/README.md).

## Navigation

Lost? Start with [docs/README.md](docs/README.md).

| What you need | Where to go |
|---------------|-------------|
| Agent rules (base) | `rules/AGENTS_TEMPLATE.md` |
| EF Core add-on | `rules/AGENTS_TEMPLATE.efcore.md` |
| Dapper add-on | `rules/AGENTS_TEMPLATE.dapper.md` |
| Security audit | `templates/skills/security-audit/` |
| DBA audit | `templates/skills/dba-audit/` |
| DBA audit (Dapper) | `templates/skills/dba-audit-dapper/` |
| Performance audit | `templates/skills/performance-audit/` |
| API design audit | `templates/skills/api-design-audit/` |
| Bot audit | `templates/skills/bot-audit/` |
| i18n audit | `templates/skills/i18n-audit/` |
| Pre-commit code review agent | `templates/skills/code-review/` |
| Frontend pre-commit code review agent | `templates/skills/frontend-code-review/` |
| Scope check | `templates/skills/task-compliance/` |
| Test pattern | `tests/patterns/` |
| CI security | `ci/github-actions/safe-ci.yml` |
| Agent traps | `docs/traps/` |
| Auto Memory grooming | `templates/skills/memory-hygiene/` |
| Doc grooming | `templates/skills/doc-hygiene/` |
| Backlog grooming | `templates/skills/backlog-hygiene/` |
| Architecture tests | `docs/solutions/architecture-tests.md` |
| AI patterns | `docs/solutions/ai-patterns.md` |
| Project onboarding | `templates/skills/skeptical-ai-bootstrap/` |
| Working example (Clean Architecture) | `examples/DemoProject/` |
| Working example (Single-project MVP) | `examples/DemoProject.MinimalApi/` |
| Failing demo (guardrails) | `examples/DemoProject.Traps/` |
| Kimi integration | `docs/agents/KIMI.md` |
| Claude Code integration | `docs/agents/CLAUDE-CODE.md` |
| Cursor integration | `docs/agents/CURSOR.md` |
| Codex integration | `docs/agents/CODEX.md` |
| OpenCode integration | `docs/agents/OPENCODE.md` |
| Bootstrap Protocol | `docs/agents/BOOTSTRAP-PROTOCOL.md` |
| Agent comparison | `docs/agents/README.md` |

## Author

**Svetlana Meleshkina** — creator of the Skeptical AI Engineering methodology, speaker at Dotnext 2026.

- 💬 Telegram channel: [@kot_review](https://t.me/kot_review)
- ✉️ Telegram: [@svetkis](https://t.me/svetkis)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). We accept new skills, test patterns, traps, and agent integrations.

## License

[MIT](LICENSE)
