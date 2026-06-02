# Knowledge Map

> Unified table of contents for all repository artifacts.  
> If you are here for the first time — start with [GLOSSARY.md](../GLOSSARY.md), then return here.
>
> [🇷🇺 Русская версия](README.md)

---

## Quick Start by Role

| I am a ... | Where to start |
|------------|----------------|
| **Newcomer** to agentic development | [GLOSSARY.md](../GLOSSARY.md) → [PYRAMID.md](../PYRAMID.md) → `examples/DemoProject/` |
| **Tech Lead** implementing guardrails | [ONBOARDING.md](ONBOARDING.md) → [skills/skeptical-ai-bootstrap/SKILL.md](../skills/skeptical-ai-bootstrap/SKILL.md) → [ADAPTATION.md](../skills/ADAPTATION.md) → "Outer Loop" section below |
| **Developer** looking for a test pattern | [tests/patterns/](#test-patterns) → copy into your project |
| **Implementing SAE from scratch** | [ONBOARDING.md](ONBOARDING.md) → step-by-step guide with checkpoints |
| **Auditor** preparing for an audit | [skills/](#skills-audits) → take CHECKLIST.md |
| **Contributor** | [CONTRIBUTING.md](../CONTRIBUTING.md) → "What can be added" section |

---

## Pyramid: 6 Layers (0–5) + Outer Loop

| Layer | What it is | Main document | Artifacts |
|-------|------------|---------------|-----------|
| **0. Instructions** | Rules the agent reads before code | [PYRAMID.md §Layer 0](../PYRAMID.md#layer-0) | `rules/AGENTS_TEMPLATE.md` + Numbered Decisions |
| 1. Compiler | Fast feedback from types | [PYRAMID.md §Layer 1](../PYRAMID.md#layer-1-compiler) | `.editorconfig`, `Directory.Build.props`, `DemoProject.Analyzers` (custom Roslyn analyzer) |
| 2. Architecture | Auto-check of layers and anti-patterns | [PYRAMID.md §Layer 2](../PYRAMID.md#layer-2-architecture) | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs), [RatchetTest.cs](../tests/patterns/RatchetTest.cs) |
| 3. Tests | Silent breakdown, PII leaks, vibe-refactoring, API contracts | [PYRAMID.md §Layer 3](../PYRAMID.md#layer-3-tests) | [tests/patterns/](#test-patterns) |
| 4. Code Review | Agent checks agent | [PYRAMID.md §Layer 4](../PYRAMID.md#layer-4-code-review) | [skills/code-review/SKILL.md](../skills/code-review/SKILL.md) |
| 5. E2E / MCP | End-to-end through external systems | [PYRAMID.md §Layer 5](../PYRAMID.md#layer-5-e2e) | [tests/patterns/SnapshotTest.cs](../tests/patterns/SnapshotTest.cs), [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| **Outer Loop** | Audits, load, manual | [PYRAMID.md §Outer Loop](../PYRAMID.md#outer-loop) | [skills/](#skills-audits) |

---

## Test Patterns

All templates are `copy-paste friendly`. Each contains comments `// TRAP:` and `// GUARDRAIL:`.

| Pattern | Purpose | Location | Working example in DemoProject |
|---------|---------|----------|-------------------------------|
| **ArchitectureRules** | NetArchTest + regex source scanning | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs) | `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs` |
| **RatchetTest** | Public types and tests did not decrease | [tests/patterns/RatchetTest.cs](../tests/patterns/RatchetTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/RatchetTests.cs` |
| **SnapshotTest** | JSON serialization contract, OpenAPI | [tests/patterns/SnapshotTest.cs](../tests/patterns/SnapshotTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/SnapshotTests.cs` |
| **LoadTest** | Silent breakdown under load: read optimizations that break write path | [tests/patterns/LoadTest.cs](../tests/patterns/LoadTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/LoadTests.cs` |
| **PiiGuardTest** | `[SensitiveData]` + redaction guard | [tests/patterns/PiiGuardTest.cs](../tests/patterns/PiiGuardTest.cs) | — |
| **VersionAuditTest** | Regex scanning of SDK/NuGet versions | [tests/patterns/VersionAuditTest.cs](../tests/patterns/VersionAuditTest.cs) | — |
| **StronglyTypedIds** | Domain entities must use strongly typed IDs, not raw Guid/string/int | [tests/patterns/StronglyTypedIds.cs](../tests/patterns/StronglyTypedIds.cs) | `examples/DemoProject/tests/DemoProject.Tests/StronglyTypedIds.cs` |
| **BUG_TEMPLATE** | Regression test format | [tests/conventions/BUG_TEMPLATE.cs](../tests/conventions/BUG_TEMPLATE.cs) | — |
| **TUnit_Guide** | Test conventions | [tests/conventions/TUnit_Guide.md](../tests/conventions/TUnit_Guide.md) | — |

---

## Skills (Audits)

Each skill = an agent role. Contains `SKILL.md` (instructions) + `CHECKLIST.md` (checklist).

| Skill | When to run |
|-------|-------------|
| [code-review](../skills/code-review/SKILL.md) | On every PR |
| [task-compliance](../skills/task-compliance/SKILL.md) | On every PR |
| [security-audit](../skills/security-audit/SKILL.md) | Once per sprint / on PR with Api/Infra |
| [dba-audit](../skills/dba-audit/SKILL.md) | Once per sprint / on migrations |
| [performance-audit](../skills/performance-audit/SKILL.md) | Before release / on suspicion |
| [api-design-audit](../skills/api-design-audit/SKILL.md) | Once per sprint |
| [bot-audit](../skills/bot-audit/SKILL.md) | Once per sprint |
| [i18n-audit](../skills/i18n-audit/SKILL.md) | Once per sprint |
| [version-audit](../skills/version-audit/SKILL.md) | Once per sprint |
| [simplicity-audit](../skills/simplicity-audit/SKILL.md) | Once per sprint / when code is hard to explain |
| [type-safety](../skills/type-safety/SKILL.md) | On PR with Domain/DTO / when refactoring identifiers |
| [skeptical-ai-bootstrap](../skills/skeptical-ai-bootstrap/SKILL.md) | Once at project start |
| [adaptation-guide](../skills/ADAPTATION.md) | Before first skill run |

### Artifact Grooming

| Skill | When to run |
|-------|-------------|
| [memory-hygiene](../skills/memory-hygiene/SKILL.md) | Once per sprint or on agent change |
| [doc-hygiene](../skills/doc-hygiene/SKILL.md) | Once per sprint or after refactoring |
| [backlog-hygiene](../skills/backlog-hygiene/SKILL.md) | Once per sprint |

---

## Agent Traps (docs/traps/)

Read before implementation — each trap explains **why** a guardrail exists.

| Trap | Essence | Pattern solution |
|------|---------|------------------|
| [silent-breakdown](traps/silent-breakdown.md) | `AsNoTracking` in write-path → silent breakdown | [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| [vibe-refactoring](traps/vibe-refactoring.md) | Agent removes "unnecessary" — breaks hot paths | [RatchetTest.cs](../tests/patterns/RatchetTest.cs) |
| [context-blindness](traps/context-blindness.md) | Agent does not see business context | [AGENTS.md](../rules/AGENTS_TEMPLATE.md) |
| [false-safety](traps/false-safety.md) | Green CI ≠ working code | [verify-tests.sh](../ci/scripts/verify-tests.sh) |
| [p50-vs-max](traps/p50-vs-max.md) | Average latency is good, tail is terrible | [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| [agent-circles](traps/agent-circles.md) | Agents loop on one problem | [task-compliance](../skills/task-compliance/SKILL.md) |
| [stale-stack](traps/stale-stack.md) | Agent uses outdated stack due to training cutoff | [VersionAuditTest.cs](../tests/patterns/VersionAuditTest.cs) |
| [log-leak](traps/log-leak.md) | PII leaks into logs | [PiiGuardTest.cs](../tests/patterns/PiiGuardTest.cs) |
| [over-engineering](traps/over-engineering.md) | Agent builds an architectural cathedral instead of a simple solution | [simplicity-audit](../skills/simplicity-audit/SKILL.md) |

---

## Solutions and Patterns (docs/solutions/)

| Document | What's inside |
|----------|---------------|
| [architecture-tests.md](solutions/architecture-tests.md) | Detailed guide to NetArchTest.eNhancedEdition + regex scanning |
| [ai-patterns.md](solutions/ai-patterns.md) | 9 proven AI-driven development patterns |
| [NUMBERED-DECISIONS.md](../skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md) | Template for intentional deviation registry (`PERF-###`, `DB-###`, `AUD-###`) |

---

## Agent Integrations (docs/agents/)

| Agent | File | Configuration format |
|-------|------|----------------------|
| Kimi Code CLI | [KIMI.md](agents/KIMI.md) | `.kimi/skills/{name}/SKILL.md` |
| Claude Code | [CLAUDE-CODE.md](agents/CLAUDE-CODE.md) | `.claude/CLAUDE.md` + commands |
| Cursor | [CURSOR.md](agents/CURSOR.md) | `.cursorrules` + `.cursor/rules/` |
| Codex (OpenAI) | [CODEX.md](agents/CODEX.md) | `.codex/instructions.md` |
| OpenCode | [OPENCODE.md](agents/OPENCODE.md) | `.opencode/instructions.md` |
| Comparison | [README.md](agents/README.md) | Comparison table of all agents |

---

## CI / CD

| Artifact | Purpose |
|----------|---------|
| [ci/github-actions/safe-ci.yml](../ci/github-actions/safe-ci.yml) | Workflow template: build + test + verify-tests |
| [ci/scripts/verify-tests.sh](../ci/scripts/verify-tests.sh) | Checks that `dotnet run` actually executed tests (not 0 ran) |
| [.github/workflows/demo-project-ci.yml](../.github/workflows/demo-project-ci.yml) | CI of this repository — builds DemoProject |

---

## Project Rules

| File | What's inside |
|------|---------------|
| [rules/AGENTS_TEMPLATE.md](../rules/AGENTS_TEMPLATE.md) | Constitution for AI agents: EF, tests, dates, cache, commits |
| [rules/CONVENTIONS.md](../rules/CONVENTIONS.md) | Test naming, workflow, CI guardrails |

---

## How to Update This Map

When adding a new artifact:
1. Add a row to the corresponding table
2. Provide a link to the pattern/solution
3. If it's a new pyramid layer — update [PYRAMID.md](../PYRAMID.md)
