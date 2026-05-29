<!-- From: d:\Repos\dotnet-agentic-engineering\AGENTS.md -->
# AGENTS.md — Skeptical AI Engineering

> **Skeptical AI Engineering (SAE)** — a methodology for protecting .NET projects from AI agent errors.
> Talk "AI is confident. I am not" (Dotnext 2026).
> This file controls AI agent behavior in this repository.

## Mission

This repository contains **defensive artifacts** for .NET projects working with AI agents.
Do not write domain code here — only guardrails, skills, patterns, and examples.

## Repository Rules

### Never
- ❌ Do not add dependencies without explicit request
- ❌ Do not change folder structure (`rules/`, `skills/`, `tests/`, `ci/`, `docs/`)
- ❌ Do not remove code examples from `tests/patterns/` — they are template-based
- ❌ Do not use `dotnet test` in examples — only `dotnet run --project`

### Always
- ✅ Update `PYRAMID.md` when adding a new layer (currently 6 layers 0–5 + outer loop)
- ✅ Update `docs/agents/` when adding support for a new AI agent
- ✅ Update `docs/README.md` (knowledge map) when adding a new artifact
- ✅ Every new skill in `skills/` must contain `SKILL.md` + `CHECKLIST.md`
- ✅ Every new test pattern — with comments `// TRAP: ...` and `// GUARDRAIL: ...`
- ✅ Code examples compile (minimal `examples/DemoProject/` if verification needed)

## Repository Stack

- Documentation: Markdown
- Code examples: .NET 10, TUnit, NBomber, NetArchTest
- CI: GitHub Actions

## How to Apply to Your Project

This repository is a **collection of defensive artifacts**, not a NuGet package. To apply it to your own .NET project:

| Step | What to do | Where to go |
|------|-----------|-------------|
| 1. Assess maturity | Run the onboarding skill in your project | [`skills/skeptical-ai-bootstrap/SKILL.md`](skills/skeptical-ai-bootstrap/SKILL.md) |
| 2. Adapt | Cross out inapplicable checks for your stack | [`skills/ADAPTATION.md`](skills/ADAPTATION.md) |
| 3. Constitution | Copy `rules/AGENTS.md` to your project root and adapt to your stack | [`rules/AGENTS.md`](rules/AGENTS.md) |
| 4. Pyramid | Implement layers 1→5 from the onboarding backlog | [`PYRAMID.md`](PYRAMID.md) |
| 5. Agent | Configure your AI agent (Kimi / Claude / Cursor / Codex) | [`docs/agents/`](docs/agents/) |

> **Principle:** don't copy everything blindly — adapt or create new. See anti-patterns in `skills/skeptical-ai-bootstrap/SKILL.md` §"Onboarding Anti-Patterns".

---

## Navigation

**Lost?** Start with [docs/README.md](docs/README.md) — the unified map of all artifacts.
**Unfamiliar terms?** See [GLOSSARY.md](GLOSSARY.md).

| What you need | Where to go |
|-----------|-----------|
| Rules for the agent | `rules/AGENTS.md` |
| Security audit | `skills/security-audit/` |
| DBA audit | `skills/dba-audit/` |
| Performance audit | `skills/performance-audit/` |
| API design audit | `skills/api-design-audit/` |
| Bot audit | `skills/bot-audit/` |
| Localization audit | `skills/i18n-audit/` |
| Code review agent | `skills/code-review/` |
| Scope compliance check | `skills/task-compliance/` |
| Test pattern | `tests/patterns/` |
| Working example | `examples/DemoProject/` |
| CI security | `ci/github-actions/safe-ci.yml` |
| Trap description | `docs/traps/` |
| Architecture tests | `docs/solutions/architecture-tests.md` |
| AI development patterns | `docs/solutions/ai-patterns.md` |
| Project onboarding | `skills/skeptical-ai-bootstrap/` |
| Kimi integration | `docs/agents/KIMI.md` |
| Claude Code integration | `docs/agents/CLAUDE-CODE.md` |
| Codex integration | `docs/agents/CODEX.md` |
| OpenCode integration | `docs/agents/OPENCODE.md` |
| Agent comparison | `docs/agents/README.md` |
| Contributing | `CONTRIBUTING.md` |
| License | `LICENSE` |
