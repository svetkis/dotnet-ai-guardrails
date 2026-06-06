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
- ✅ Update `PYRAMID.md` when adding a new layer (currently 3 layers 0–2 + outer loop)
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

**Full guide:** [`docs/ONBOARDING.md`](docs/ONBOARDING.md) — step-by-step implementation plan with checkpoints and anti-patterns.

| Step | What to do | Where to go |
|------|-----------|-------------|
| 0. Record architecture | Fill in assembly inventory, critical paths, and conscious deviations | [`skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md`](skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) + [`DECISION-GUARDS.md`](skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) |
| 1. Assess maturity | Run the onboarding skill in your project | [`skills/skeptical-ai-bootstrap/SKILL.md`](skills/skeptical-ai-bootstrap/SKILL.md) |
| 2. Adapt | Cross out inapplicable checks for your stack | [`skills/ADAPTATION.md`](skills/ADAPTATION.md) |
| 3. Constitution | Copy `rules/AGENTS_TEMPLATE.md` to your project root and adapt to your stack | [`rules/AGENTS_TEMPLATE.md`](rules/AGENTS_TEMPLATE.md) |
| 4. Pyramid | Implement layers 1→2 from the onboarding backlog | [`PYRAMID.md`](PYRAMID.md) |
| 5. Agent | Configure your AI agent (Kimi / Claude / Cursor / Codex) | [`docs/agents/`](docs/agents/) |
| 6. Acceptance | Implement audits + load (manual or AI) | [`docs/solutions/human-audit-bridge.md`](docs/solutions/human-audit-bridge.md) |

> **Principle:** don't copy everything blindly — adapt or create new. See anti-patterns in [`docs/ONBOARDING.md`](docs/ONBOARDING.md) §"Implementation Anti-Patterns".

---

## Context for External Agents

If you are an AI agent reading this file in the context of a **different** repository:
- This repository (`dotnet-skeptical-ai`) is a **methodology archive**, not a project template.
- Do NOT copy its folder structure (`rules/`, `skills/`, `examples/`, `tests/patterns/`) into the target project.
- Do NOT create `DemoProject`, `examples/`, or `DemoProject.Traps/` in the target project.
- Extract only the **principles** and **markdown guidelines** applicable to the target stack.
- Your output should be assessment reports and plans, not implementation code.

## Navigation

**Lost?** Start with [docs/README.md](docs/README.md) — the unified map of all artifacts.
**Unfamiliar terms?** See [GLOSSARY.md](GLOSSARY.md).

| What you need | Where to go |
|-----------|-----------|
| Rules for the agent | `rules/AGENTS_TEMPLATE.md` |
| Bootstrap protocol (don't create DemoProject) | `docs/agents/BOOTSTRAP-PROTOCOL.md` |
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
| Failing demo (guardrails) | `examples/DemoProject.Traps/` |
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
