# Карта знаний / Knowledge Map

> Единое оглавление всех артефактов репозитория.  
> Если вы здесь впервые — начните с [GLOSSARY.md](../GLOSSARY.md), затем вернитесь сюда.

---

## Быстрый старт по ролям

| Я — ... | С чего начать |
|---------|---------------|
| **Новичок** в агентной разработке | [GLOSSARY.md](../GLOSSARY.md) → [PYRAMID.md](../PYRAMID.md) → `examples/DemoProject/` |
| **Tech Lead**, внедряю guardrails | [ONBOARDING.md](ONBOARDING.md) → [.agents/skills/skeptical-ai-bootstrap/SKILL.md](../.agents/skills/skeptical-ai-bootstrap/SKILL.md) → [ADAPTATION.md](../templates/skills/ADAPTATION.md) → раздел «Внешний цикл» ниже |
| **Разработчик**, ищу паттерн теста | [tests/patterns/](#тестовые-паттерны) → копируй в проект |
| **Внедряю SAE с нуля** | [ONBOARDING.md](ONBOARDING.md) → пошаговый план с контрольными точками |
| **Аудитор**, готовлюсь к аудиту | [templates/skills/](#скиллы-аудиты) → бери CHECKLIST.md → [human-audit-bridge.md](solutions/human-audit-bridge.md) для ручного прохода |
| **Контрибьютор** | [CONTRIBUTING.md](../CONTRIBUTING.md) → раздел «Что можно добавлять» |

---

## Пирамида: 3 слоя (0–2) + внешний цикл

| Слой | Подслой | Что это | Главный документ | Артефакты |
|------|---------|---------|------------------|-----------|
| **0** | — | Инструкции для агента перед кодом | [PYRAMID.md §Слой 0](../PYRAMID.md#layer-0) | `rules/AGENTS_TEMPLATE.md` + Decision Guards (ADR) |
| **1** | 1.1 Компилятор | Быстрая обратная связь от типов | [PYRAMID.md §1.1](../PYRAMID.md#layer-1-compiler) | `.editorconfig`, `Directory.Build.props`, `DemoProject.Analyzers` (кастомный Roslyn-анализатор) |
| **1** | 1.2 Архитектура | Авто-проверка слоёв и антипаттернов | [PYRAMID.md §1.2](../PYRAMID.md#layer-1-architecture) | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs), [RatchetTest.cs](../tests/patterns/RatchetTest.cs), [ArchUnitNetSliceTest.cs](../tests/patterns/ArchUnitNetSliceTest.cs) |
| **1** | 1.3 Тесты | Регрессии, snapshot, vibe-refactoring, контракты API | [PYRAMID.md §1.3](../PYRAMID.md#layer-1-tests) | [tests/patterns/](#тестовые-паттерны) |
| **1** | 1.4 Code Review | Агент проверяет агента (pre-commit / PR) | [PYRAMID.md §1.4](../PYRAMID.md#layer-1-code-review) | [templates/skills/code-review/SKILL.md](../templates/skills/code-review/SKILL.md), [templates/skills/frontend-code-review/SKILL.md](../templates/skills/frontend-code-review/SKILL.md) |
| **1** | 1.5 Smoke | Быстрый прогон критичных сценариев | [PYRAMID.md §1.5](../PYRAMID.md#layer-1-smoke) | — |
| **2** | 2.1 E2E / MCP | Полные сценарии через внешние системы | [PYRAMID.md §2.1](../PYRAMID.md#layer-2-e2e) | [tests/patterns/SnapshotTest.cs](../tests/patterns/SnapshotTest.cs) |
| **2** | 2.2 Аудиты | Глубинные проверки по расписанию | [PYRAMID.md §2.2](../PYRAMID.md#layer-2-audits) | [templates/skills/](#скиллы-аудиты) |
| **2** | 2.3 Нагрузка | Silent breakdown под нагрузкой (NBomber) | [PYRAMID.md §2.3](../PYRAMID.md#layer-2-load) | [tests/patterns/LoadTest.cs](../tests/patterns/LoadTest.cs) |
| **Внешний** | — | Окончательная проверка человеком, бизнес и продуктовые решения | [PYRAMID.md §Внешний цикл](../PYRAMID.md#outer-loop) | — |

---

## Тестовые паттерны

Все шаблоны — `copy-paste friendly`. Каждый содержит комментарии `// TRAP:` и `// GUARDRAIL:`.

| Паттерн | Зачем | Где лежит | Рабочий пример в DemoProject |
|---------|-------|-----------|------------------------------|
| **ArchitectureRules** | Универсальная проверка зависимостей между слоями (NetArchTest) | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs) | `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs` |
| **EfCoreGuardRules** | EF Core-специфичные guardrails: `FindAsync`, `Include`, `AsNoTracking` | [tests/patterns/EfCoreGuardRules.cs](../tests/patterns/EfCoreGuardRules.cs) | `examples/DemoProject/tests/DemoProject.Tests/EfCoreGuardRules.cs` |
| **DapperGuardRules** | Dapper / Raw SQL guardrails: параметризация, инъекции, таймауты | [tests/patterns/DapperGuardRules.cs](../tests/patterns/DapperGuardRules.cs) | — |
| **ArchUnitNetSliceTest** | Циклические зависимости между слайсами (ArchUnitNET) | [tests/patterns/ArchUnitNetSliceTest.cs](../tests/patterns/ArchUnitNetSliceTest.cs) | `examples/DemoProject.Traps/tests/DemoProject.Traps.Tests/ArchUnitNetSliceTest.cs` |
| **RatchetTest** | Публичные типы и тесты не уменьшились | [tests/patterns/RatchetTest.cs](../tests/patterns/RatchetTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/RatchetTests.cs` |
| **SnapshotTest** | Контракт JSON-сериализации, OpenAPI | [tests/patterns/SnapshotTest.cs](../tests/patterns/SnapshotTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/SnapshotTests.cs` |
| **LoadTest** | Silent breakdown под нагрузкой: read-оптимизации, которые ломают write | [tests/patterns/LoadTest.cs](../tests/patterns/LoadTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/LoadTests.cs` |
| **PiiGuardTest** | `[SensitiveData]` + redaction guard | [tests/patterns/PiiGuardTest.cs](../tests/patterns/PiiGuardTest.cs) | — |
| **VersionAuditTest** | Аудит версий SDK/NuGet и frontend-зависимостей | [tests/patterns/VersionAuditTest.cs](../tests/patterns/VersionAuditTest.cs) | — |
| **DuplicationGuardTest** | Бизнес-логика не дублируется между сервисами | [tests/patterns/DuplicationGuardTest.cs](../tests/patterns/DuplicationGuardTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/DuplicationGuardTest.cs` |
| **DependencyDriftTest** | Циклические зависимости между проектами и дрейф слоёв | [tests/patterns/DependencyDriftTest.cs](../tests/patterns/DependencyDriftTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/DependencyDriftTest.cs` |
| **EntityLeakTest** | Application-интерфейсы не возвращают Domain Entity (ratchet) | [tests/patterns/EntityLeakTest.cs](../tests/patterns/EntityLeakTest.cs) | `examples/DemoProject/tests/DemoProject.Tests/EntityLeakTest.cs` |
| **StronglyTypedIds** | Domain-сущности используют strongly typed IDs, а не голые Guid/string/int | [tests/patterns/StronglyTypedIds.cs](../tests/patterns/StronglyTypedIds.cs) | `examples/DemoProject/tests/DemoProject.Tests/StronglyTypedIds.cs` |
| **BUG_TEMPLATE** | Формат regression-теста | [tests/conventions/BUG_TEMPLATE.cs](../tests/conventions/BUG_TEMPLATE.cs) | — |
| **TUnit_Guide** | Соглашения по тестам | [tests/conventions/TUnit_Guide.md](../tests/conventions/TUnit_Guide.md) | — |
| **Traps Demo** | Специально сломанный код для демонстрации guardrails (5 failing tests) | — | `examples/DemoProject.Traps/` |
| **MinimalApi Demo** | Single-project MVP без Clean Architecture — naming, banned APIs, ratchet | — | `examples/DemoProject.MinimalApi/` |

---

## Скиллы (аудиты)

Каждый standalone-скилл = роль агента. Обычно содержит `SKILL.md` (инструкция) + `CHECKLIST.md` (проверка).
Исключение: `templates/skills/skeptical-ai-bootstrap/` — это набор supporting templates; сам исполняемый bootstrap-скилл лежит в `.agents/skills/skeptical-ai-bootstrap/`.

| Скилл | Когда запускать |
|-------|-----------------|
| [code-review](../templates/skills/code-review/SKILL.md) | На каждый коммит (pre-commit) / PR |
| [frontend-code-review](../templates/skills/frontend-code-review/SKILL.md) | На каждый коммит (pre-commit) / PR с React/TS |
| [task-compliance](../templates/skills/task-compliance/SKILL.md) | На каждый PR |
| [security-audit](../templates/skills/security-audit/SKILL.md) | Раз в спринт / на PR с Api/Infra |
| [dba-audit](../templates/skills/dba-audit/SKILL.md) | Раз в спринт / при миграциях (EF Core) |
| [dba-audit-dapper](../templates/skills/dba-audit-dapper/SKILL.md) | Раз в спринт / при изменениях репозиториев (Dapper / Raw SQL) |
| [performance-audit](../templates/skills/performance-audit/SKILL.md) | Перед релизом / при подозрении |
| [api-design-audit](../templates/skills/api-design-audit/SKILL.md) | Раз в спринт |
| [bot-audit](../templates/skills/bot-audit/SKILL.md) | Раз в спринт |
| [i18n-audit](../templates/skills/i18n-audit/SKILL.md) | Раз в спринт |
| [version-audit](../templates/skills/version-audit/SKILL.md) | Раз в спринт |
| [tech-debt-audit](../templates/skills/tech-debt-audit/SKILL.md) | Раз в спринт / перед квартальным планированием |
| [test-audit](../templates/skills/test-audit/SKILL.md) | После 3-5 фич / перед релизом |
| [simplicity-audit](../templates/skills/simplicity-audit/SKILL.md) | Раз в спринт / когда код трудно объяснить |
| [ux-audit](../templates/skills/ux-audit/SKILL.md) | При переработке UI / перед бетой |
| [type-safety](../templates/skills/type-safety/SKILL.md) | На PR с Domain/DTO / при рефакторинге идентификаторов |
| [skeptical-ai-bootstrap](../.agents/skills/skeptical-ai-bootstrap/SKILL.md) | Однократно при старте |
| [adaptation-guide](../templates/skills/ADAPTATION.md) | Перед первым запуском скиллов |

### Груминг артефактов

| Скилл | Когда запускать |
|-------|-----------------|
| [memory-hygiene](../templates/skills/memory-hygiene/SKILL.md) | Раз в спринт или при смене агента |
| [doc-hygiene](../templates/skills/doc-hygiene/SKILL.md) | Раз в спринт или после рефакторинга |
| [backlog-hygiene](../templates/skills/backlog-hygiene/SKILL.md) | Раз в спринт |

---

## Ловушки агента (docs/traps/)

Читать перед внедрением — каждая ловушка объясняет, **почему** guardrail существует.

| Ловушка | Суть | Паттерн-решение |
|---------|------|-----------------|
| [silent-breakdown](traps/silent-breakdown.md) | `AsNoTracking` в write-path → молчаливая поломка | [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| [vibe-refactoring](traps/vibe-refactoring.md) | Агент удаляет "лишнее" — ломает hot paths | [RatchetTest.cs](../tests/patterns/RatchetTest.cs) |
| [context-blindness](traps/context-blindness.md) | Агент не видит бизнес-контекста | [AGENTS.md](../rules/AGENTS_TEMPLATE.md) |
| [false-safety](traps/false-safety.md) | Зелёный CI ≠ рабочий код | [verify-tests.sh](../ci/scripts/verify-tests.sh) |
| [p50-vs-max](traps/p50-vs-max.md) | Средняя latency хороша, tail — ужасен | [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| [agent-circles](traps/agent-circles.md) | Агенты зацикливаются на одной проблеме | [task-compliance](../templates/skills/task-compliance/SKILL.md) |
| [stale-stack](traps/stale-stack.md) | Агент использует устаревший стек из-за training cutoff | [VersionAuditTest.cs](../tests/patterns/VersionAuditTest.cs) |
| [log-leak](traps/log-leak.md) | PII утекает в логи | [PiiGuardTest.cs](../tests/patterns/PiiGuardTest.cs) |
| [code-duplication](traps/code-duplication.md) | Агент дублирует бизнес-логику вместо реюза | [DuplicationGuardTest.cs](../tests/patterns/DuplicationGuardTest.cs) |
| [dependency-drift](traps/dependency-drift.md) | +1 using/#include замыкает цикл в графе зависимостей | [DependencyDriftTest.cs](../tests/patterns/DependencyDriftTest.cs) |
| [over-engineering](traps/over-engineering.md) | Агент строит архитектурный собор вместо простого решения | [simplicity-audit](../templates/skills/simplicity-audit/SKILL.md) |

---

## Решения и паттерны (docs/solutions/)

| Документ | Что внутри |
|----------|------------|
| [architecture-tests.md](solutions/architecture-tests.md) | Подробный гайд по NetArchTest.eNhancedEdition, ArchUnitNET и границам архитектуры |
| [roslyn-analyzers.md](solutions/roslyn-analyzers.md) | Roslyn-first guardrails для C#: diagnostics в IDE / `dotnet build` вместо regex по `.cs` |
| [ai-patterns.md](solutions/ai-patterns.md) | 10 проверенных паттернов AI-driven разработки |
| [human-audit-bridge.md](solutions/human-audit-bridge.md) | Как использовать AI-чеклисты для ручного аудита человеком |
| [ARCHITECTURE-INVENTORY.md](../templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) | Шаблон фиксации текущей архитектуры перед внедрением guardrails |
| [DECISION-GUARDS.md](../templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) | Шаблон реестра осознанных отклонений (`PERF-###`, `DB-###`, `AUD-###`) |

---

## Интеграции с агентами (docs/agents/)

> **⚠️ Агентам:** Прочитайте [BOOTSTRAP-PROTOCOL.md](agents/BOOTSTRAP-PROTOCOL.md) перед началом работы.  
> Он определяет границу между "репо методологии" и "целевой проект".

| Агент | Файл | Формат конфигурации |
|-------|------|---------------------|
| Kimi Code CLI | [KIMI.md](agents/KIMI.md) | `.kimi/skills/{name}/SKILL.md` |
| Claude Code | [CLAUDE-CODE.md](agents/CLAUDE-CODE.md) | `.claude/CLAUDE.md` + commands |
| Cursor | [CURSOR.md](agents/CURSOR.md) | `.cursorrules` + `.cursor/rules/` |
| Codex (OpenAI) | [CODEX.md](agents/CODEX.md) | `.codex/instructions.md` |
| OpenCode | [OPENCODE.md](agents/OPENCODE.md) | `.opencode/instructions.md` |
| Bootstrap Protocol | [BOOTSTRAP-PROTOCOL.md](agents/BOOTSTRAP-PROTOCOL.md) | Правила поведения агента при онбординге |
| Сравнение | [README.md](agents/README.md) | Таблица сравнения всех агентов |

---

## CI / CD

| Артефакт | Назначение |
|----------|------------|
| [ci/github-actions/safe-ci.yml](../ci/github-actions/safe-ci.yml) | Шаблон воркфлоу: build + test + verify-tests |
| [ci/scripts/run-tests.sh](../ci/scripts/run-tests.sh) | Автоматически находит и запускает все тестовые проекты через `dotnet run --project` |
| [ci/scripts/verify-tests.sh](../ci/scripts/verify-tests.sh) | Проверяет, что `dotnet run` реально выполнил тесты (не 0 ran) |
| [.github/workflows/demo-project-ci.yml](../.github/workflows/demo-project-ci.yml) | CI этого репозитория — собирает DemoProject и DemoProject.MinimalApi |
| `traps-guardrails` job в `demo-project-ci.yml` | Проверяет, что intentionally broken тесты в DemoProject.Traps реально падают (guardrails работают) |

---

## Правила проекта

| Файл | Что внутри |
|------|------------|
| [rules/AGENTS_TEMPLATE.md](../rules/AGENTS_TEMPLATE.md) | Базовая конституция для AI-агентов: тесты, даты, кэш, коммиты (универсальная) |
| [rules/AGENTS_TEMPLATE.efcore.md](../rules/AGENTS_TEMPLATE.efcore.md) | Add-on: EF Core-специфичные правила (read/write path, `AsNoTracking`) |
| [rules/AGENTS_TEMPLATE.dapper.md](../rules/AGENTS_TEMPLATE.dapper.md) | Add-on: Dapper / Raw SQL-специфичные правила (параметризация, таймауты) |
| [rules/CONVENTIONS.md](../rules/CONVENTIONS.md) | Именование тестов, workflow, CI guardrails |
| [BannedSymbols.txt](../examples/DemoProject/BannedSymbols.txt) | Compile-time guard: запрещённые API (BannedApiAnalyzers RS0030) |

---

## Как обновлять эту карту

При добавлении нового артефакта:
1. Добавить строку в соответствующую таблицу
2. Указать ссылку на паттерн/решение
3. Если новый слой пирамиды — обновить [PYRAMID.md](../PYRAMID.md)
