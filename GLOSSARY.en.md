# Glossary

> Key terms of the repository. If you encounter an unfamiliar word in `AGENTS.en.md` or `PYRAMID.en.md` — it is most likely here.
>
> [🇷🇺 Русская версия](GLOSSARY.md)

---

## Feedback Architecture

| Term | Definition | Used in |
|------|------------|---------|
| **Layer 1 (development cycle)** | Fast feedback: Compiler → Architecture → Tests → Code Review → Smoke. Runs on every change, from seconds to minutes. | [PYRAMID.en.md §Layer 1](PYRAMID.en.md#layer-1) |
| **Outer loop** | Final human verification, business and product decisions. Not part of the daily feedback loop. | [PYRAMID.en.md §Outer loop](PYRAMID.en.md#outer-loop) |
| **Layer 0** | Instructions for the agent: `AGENTS.md` + numbered decisions. The agent reads before code. | [PYRAMID.en.md §Layer 0](PYRAMID.en.md#layer-0) |
| **AGENTS.md** | File with rules for AI agents. Read by the agent before every task. Can be hierarchical (root + per-module). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Test Patterns

| Term | Definition | Example |
|------|------------|---------|
| **Ratchet** | Test inventory: a metric (e.g., number of public types or tests) must **not decrease**. If an agent deletes types or tests — the test fails. | [tests/patterns/RatchetTest.cs](tests/patterns/RatchetTest.cs) |
| **BUG###** | Regression test naming convention: one bug = one file `BUG###_DescriptiveName.cs`. Covers all code paths where the bug could manifest. | [tests/conventions/BUG_TEMPLATE.cs](tests/conventions/BUG_TEMPLATE.cs) |
| **Snapshot test** | A test that captures and compares output (JSON, OpenAPI) with a reference file. If the DTO changes — the snapshot breaks. | [tests/patterns/SnapshotTest.cs](tests/patterns/SnapshotTest.cs) |
| **Characterization test** | A test that captures current system behavior without judging correctness. Needed so refactoring does not change behavior. | [PYRAMID.en.md §Layer 1.3](PYRAMID.en.md#layer-1-tests) |
| **"0 tests ran"** | Problem when the test runner found no tests, but exit code = 0. CI looks green though nothing was checked. | [PYRAMID.en.md §Layer 1.3](PYRAMID.en.md#layer-1-tests) |

## Code Patterns

| Term | Definition | Used in |
|------|------------|---------|
| **Read-path** | Data read path: read-only queries. `.Select()` + `.AsNoTracking()` are **mandatory**. `.Include()`, `.FindAsync()` are forbidden. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Write-path** | Data write path: commands that change state. Change tracking is required, `.AsNoTracking()` is forbidden. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Decision Guard** | SAE-specific: intentional deviation from standard, documented by an ID in a comment (`PERF-###`, `DB-###`, `AUD-###`) plus a short registry entry. **Not a synonym for ADR** — a lightweight reference to a decision; when full ADRs exist, the entry links to them. Checked by an architecture test for uniqueness. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md), [templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md](templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) |
| **Semantic Anchors** | Established terms instead of descriptions. Each term activates a specific methodology (e.g., "read-path" = `.Select()` + `.AsNoTracking()`). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Agents and Tools

| Term | Definition | Used in |
|------|------------|---------|
| **MCP (Model Context Protocol)** | Protocol for connecting external tools to an AI agent. Allows the agent to "touch" Telegram, browser, API. | [PYRAMID.en.md §Layer 2.1](PYRAMID.en.md#layer-2-e2e) |
| **Code Review Agent** | A separate AI agent instance that reviews the diff **before** commit. Not the one that wrote the code. | [templates/skills/code-review/SKILL.md](templates/skills/code-review/SKILL.md) |
| **Skill** | Agent role: instruction + checklist for a specific task (audit, review, onboarding). Installed in `.kimi/skills/` or equivalent. | `templates/skills/` |
| **Context Marker** | Emoji marker at the beginning of an agent's reply showing active context: 🍀 (ground rules), 🔍 (review), ✅ (commit). | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |
| **Focused Agent** | Principle: one agent — one task. A review agent does not write code; a code agent does not review. | [templates/skills/code-review/SKILL.md](templates/skills/code-review/SKILL.md) |

## Processes and Metrics

| Term | Definition | Used in |
|------|------------|---------|
| **Audit** | Deep check of one narrow area (security, perf, DB). Runs once per sprint or on trigger, not on every PR. | `templates/skills/` |
| **Cross-pollination** | Exchange of findings between audits. For example, a security audit finds a log leak, while a UX audit finds the same endpoint as a dead-end. | [PYRAMID.en.md §Outer loop](PYRAMID.en.md#outer-loop) |
| **P50 / P95 / Max** | Latency percentiles: median, 95th percentile, maximum. Agents often optimize P50 while forgetting tail latency (Max). | [docs/traps/p50-vs-max.md](docs/traps/p50-vs-max.md) |
| **Scope creep** | Task expansion: an agent adds changes to a PR that go beyond the original request. | [templates/skills/task-compliance/SKILL.md](templates/skills/task-compliance/SKILL.md) |
| **Silent misalignment** | Silent error: the agent did not ask clarifying questions even though instructions were unclear or contradictory. | [rules/AGENTS_TEMPLATE.md](rules/AGENTS_TEMPLATE.md) |

## Technologies

| Term | Definition | Link |
|------|------------|------|
| **TUnit** | Modern test framework for .NET. Used in this repository instead of xUnit/NUnit. Run via `dotnet run --project`. | [tests/conventions/TUnit_Guide.md](tests/conventions/TUnit_Guide.md) |
| **NetArchTest** | Library for architecture tests based on reflection. Checks dependencies between layers, naming, interfaces. | [tests/patterns/ArchitectureRules.cs](tests/patterns/ArchitectureRules.cs) |
| **NBomber** | Load testing framework. Catches silent breakdown and weak points under mixed read+write load, not just "degradation at high load". | [tests/patterns/LoadTest.cs](tests/patterns/LoadTest.cs) |
| **Testcontainers** | Infrastructure for running real databases (PostgreSQL, Redis) in Docker containers during tests. Alternative to EF Core InMemory provider. | [docs/traps/silent-breakdown.md](docs/traps/silent-breakdown.md) |
