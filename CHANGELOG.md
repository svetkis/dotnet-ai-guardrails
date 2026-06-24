# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `docs/agents/CURSOR.md` — интеграция с Cursor IDE (`.cursorrules`, `.cursor/rules/`, Composer mode).
- `docs/obstacles/context-rot.md` — препятствие "Деградация контекста" и компенсация через stateless guardrails.
- `docs/traps/stale-stack.md` — ловушка "Устаревший стек": агент использует preview SDK или устаревшие NuGet-пакеты из-за training cutoff.
- `docs/traps/log-leak.md` — ловушка "Утечка данных в логи": агент логирует email, phone, password.
- `docs/relationships.mmd` — граф связей guardrails, traps и obstacles (Mermaid).
- `templates/skills/version-audit/` — новый скилл аудита актуальности стека (SDK, NuGet, frontend, CI actions).
- `tests/patterns/VersionAuditTest.cs` — тест-паттерн: regex-сканирование `global.json`, `*.csproj`, `package.json` на preview-флаги и рассогласование версий.
- `tests/patterns/PiiGuardTest.cs` — тест-паттерн: `[SensitiveData]` attribute + ratchet + regex-сканирование Log* вызовов на PII.
- `tests/patterns/ComplexityRatchetTest.cs` — тест-паттерн: ratchet на рост нарушений `S3776`/`S1541`.
- `tests/patterns/AllocationBudgetTest.cs` — тест-паттерн: аллокации `[HotPath]` методов не превышают baseline + 10%.
- `tests/patterns/SpellcheckGuardTest.cs` — тест-паттерн: CSpell + baseline для публичных символов и документации.
- `tests/patterns/ReleaseReadinessTest.cs` — тест-паттерн: проверка обязательных артефактов перед релизом.
- `tests/patterns/MutationGuardTest.cs` — тест-паттерн: mutation score не падает (Stryker.NET).
- `tests/patterns/AnalyzerTests.cs` — тест-паттерн: positive/negative тесты для кастомных Roslyn-анализаторов.
- `templates/skills/complexity-audit/` — скилл аудита когнитивной / цикломатической сложности.
- `templates/skills/allocation-budget-audit/` — скилл аудита аллокаций hot path.
- `templates/skills/spellcheck-audit/` — скилл аудита орфографии публичных символов и документации.
- `templates/skills/release-readiness-audit/` — скилл аудита готовности к релизу.
- `templates/skills/mutation-audit/` — скилл аудита mutation testing.
- `templates/skills/analyzer-tests-audit/` — скилл аудита тестов кастомных анализаторов.
- `examples/DemoProject/src/DemoProject.Analyzers/HotPathAnalyzer.cs` — Roslyn-анализатор SAE003/004/005 для `[HotPath]` методов.
- `docs/solutions/ai-patterns.md` — паттерн #9: Attribute-driven PII redaction (compile-time + runtime).
- `rules/AGENTS_TEMPLATE.md` — перевод на английский, добавлены Semantic Anchors, Permission to Push Back, Context Markers.
- `docs/TRANSLATION_PLAN.md` — план перевода документации на два языка.
- `LICENSE` — MIT license.
- `.gitignore` — standard .NET + JetBrains Rider + Serena ignore rules.
- `global.json` — pins .NET 10 SDK with `latestFeature` roll-forward.
- `CONTRIBUTING.md` — bilingual (RU/EN) contribution guide with pre-PR checklist.
- `examples/DemoProject/` — working .NET 10 solution demonstrating all patterns:
  - Clean Architecture (Domain → Application → Infrastructure)
  - NetArchTest layer dependency checks
  - Ratchet tests for test inventory (count must not decrease)
  - Snapshot tests for JSON contracts
  - NBomber load tests (read + write mix)
  - TUnit 1.x with `dotnet run --project`
- `README.en.md` — full English translation of README.
- `.github/workflows/demo-project-ci.yml` — CI that builds DemoProject and runs all 15 tests.
- `SECURITY.md` — security policy and responsible disclosure process.
- `CODE_OF_CONDUCT.md` — Contributor Covenant Code of Conduct.
- `.github/ISSUE_TEMPLATE/` — issue templates for bug reports, feature requests, and proposals.
- `.github/pull_request_template.md` — pull request template with pre-PR checklist.

### Changed
- `README.md` — restructured with language badges, DemoProject section, and links to CONTRIBUTING/LICENSE.
- `AGENTS.md` — updated navigation table with `examples/DemoProject/`, `CONTRIBUTING.md`, and `LICENSE`.
- `tests/conventions/TUnit_Guide.md` — added note about TUnit 1.x auto-generated entry point (no `Program.cs` required).
- `README.md` and `README.en.md` — added badges (.NET 10, License, CI), author section, and community contacts.

## [0.1.0] - 2026-05-29

### Added
- Initial release of defensive artifacts for .NET agentic engineering.
- 5-layer inner-loop pyramid documented in `PYRAMID.md`.
- `rules/AGENTS_TEMPLATE.md` — EF Core, PostgreSQL, API/DTO, caching, and commit conventions.
- `rules/CONVENTIONS.md` — naming, workflow, and CI guardrails.
- `templates/skills/` — 8 agent roles: code-review, task-compliance, security-audit, dba-audit, ux-audit, performance-audit, i18n-audit, skeptical-ai-bootstrap.
- `tests/patterns/` — template tests: ArchitectureRules, RatchetTest, SnapshotTest, LoadTest.
- `tests/conventions/` — BUG_TEMPLATE.cs, TUnit_Guide.md.
- `docs/traps/` — 6 documented agent traps: agent-circles, context-blindness, false-safety, p50-vs-max, silent-breakdown, vibe-refactoring.
- `docs/solutions/` — architecture-tests.md, ai-patterns.md.
- `docs/agents/` — integration guides for Kimi, Claude Code, Codex, OpenCode.
- `ci/github-actions/safe-ci.yml` — template CI workflow for consumer projects.
- `ci/scripts/verify-tests.sh` — verifies that `dotnet run` actually executed tests.
