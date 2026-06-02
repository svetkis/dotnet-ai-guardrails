# Карта знаний / Knowledge Map

> Единое оглавление всех артефактов репозитория.  
> Если вы здесь впервые — начните с [GLOSSARY.md](../GLOSSARY.md), затем вернитесь сюда.

---

## Быстрый старт по ролям

| Я — ... | С чего начать |
|---------|---------------|
| **Новичок** в агентной разработке | [GLOSSARY.md](../GLOSSARY.md) → [PYRAMID.md](../PYRAMID.md) → `examples/DemoProject/` |
| **Tech Lead**, внедряю guardrails | [ONBOARDING.md](ONBOARDING.md) → [skills/skeptical-ai-bootstrap/SKILL.md](../skills/skeptical-ai-bootstrap/SKILL.md) → [ADAPTATION.md](../skills/ADAPTATION.md) → раздел «Внешний цикл» ниже |
| **Разработчик**, ищу паттерн теста | [tests/patterns/](#тестовые-паттерны) → копируй в проект |
| **Внедряю SAE с нуля** | [ONBOARDING.md](ONBOARDING.md) → пошаговый план с контрольными точками |
| **Аудитор**, готовлюсь к аудиту | [skills/](#скиллы-аудиты) → бери CHECKLIST.md → [human-audit-bridge.md](solutions/human-audit-bridge.md) для ручного прохода |
| **Контрибьютор** | [CONTRIBUTING.md](../CONTRIBUTING.md) → раздел «Что можно добавлять» |

---

## Пирамида: 6 слоёв (0–5) + внешний цикл

| Слой | Что это | Главный документ | Артефакты |
|------|---------|------------------|-----------|
| **0. Инструкции** | Правила для агента перед кодом | [PYRAMID.md §Слой 0](../PYRAMID.md#layer-0) | `rules/AGENTS_TEMPLATE.md` + Numbered Decisions |
| 1. Компилятор | Быстрая обратная связь от типов | [PYRAMID.md §Слой 1](../PYRAMID.md#layer-1-compiler) | `.editorconfig`, `Directory.Build.props`, `DemoProject.Analyzers` (кастомный Roslyn-анализатор) |
| 2. Архитектура | Авто-проверка слоёв и антипаттернов | [PYRAMID.md §Слой 2](../PYRAMID.md#layer-2-architecture) | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs), [RatchetTest.cs](../tests/patterns/RatchetTest.cs) |
| 3. Тесты | Silent breakdown, PII leaks, vibe-refactoring, контракты API | [PYRAMID.md §Слой 3](../PYRAMID.md#layer-3-tests) | [tests/patterns/](#тестовые-паттерны) |
| 4. Code Review | Агент проверяет агента | [PYRAMID.md §Слой 4](../PYRAMID.md#layer-4-code-review) | [skills/code-review/SKILL.md](../skills/code-review/SKILL.md) |
| 5. E2E / MCP | End-to-end через внешние системы | [PYRAMID.md §Слой 5](../PYRAMID.md#layer-5-e2e) | [tests/patterns/SnapshotTest.cs](../tests/patterns/SnapshotTest.cs), [LoadTest.cs](../tests/patterns/LoadTest.cs) |
| **Внешний цикл** | Аудиты, нагрузка, ручное | [PYRAMID.md §Внешний цикл](../PYRAMID.md#outer-loop) | [skills/](#скиллы-аудиты) |

---

## Тестовые паттерны

Все шаблоны — `copy-paste friendly`. Каждый содержит комментарии `// TRAP:` и `// GUARDRAIL:`.

| Паттерн | Зачем | Где лежит | Рабочий пример в DemoProject |
|---------|-------|-----------|------------------------------|
| **ArchitectureRules** | Проверка зависимостей между слоями (NetArchTest) | [tests/patterns/ArchitectureRules.cs](../tests/patterns/ArchitectureRules.cs) | `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs` |
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

---

## Скиллы (аудиты)

Каждый скилл = роль агента. Содержит `SKILL.md` (инструкция) + `CHECKLIST.md` (проверка).

| Скилл | Когда запускать |
|-------|-----------------|
| [code-review](../skills/code-review/SKILL.md) | На каждый PR |
| [task-compliance](../skills/task-compliance/SKILL.md) | На каждый PR |
| [security-audit](../skills/security-audit/SKILL.md) | Раз в спринт / на PR с Api/Infra |
| [dba-audit](../skills/dba-audit/SKILL.md) | Раз в спринт / при миграциях |
| [performance-audit](../skills/performance-audit/SKILL.md) | Перед релизом / при подозрении |
| [api-design-audit](../skills/api-design-audit/SKILL.md) | Раз в спринт |
| [bot-audit](../skills/bot-audit/SKILL.md) | Раз в спринт |
| [i18n-audit](../skills/i18n-audit/SKILL.md) | Раз в спринт |
| [version-audit](../skills/version-audit/SKILL.md) | Раз в спринт |
| [tech-debt-audit](../skills/tech-debt-audit/SKILL.md) | Раз в спринт / перед квартальным планированием |
| [test-audit](../skills/test-audit/SKILL.md) | После 3-5 фич / перед релизом |
| [simplicity-audit](../skills/simplicity-audit/SKILL.md) | Раз в спринт / когда код трудно объяснить |
| [ux-audit](../skills/ux-audit/SKILL.md) | При переработке UI / перед бетой |
| [type-safety](../skills/type-safety/SKILL.md) | На PR с Domain/DTO / при рефакторинге идентификаторов |
| [skeptical-ai-bootstrap](../skills/skeptical-ai-bootstrap/SKILL.md) | Однократно при старте |
| [adaptation-guide](../skills/ADAPTATION.md) | Перед первым запуском скиллов |

### Груминг артефактов

| Скилл | Когда запускать |
|-------|-----------------|
| [memory-hygiene](../skills/memory-hygiene/SKILL.md) | Раз в спринт или при смене агента |
| [doc-hygiene](../skills/doc-hygiene/SKILL.md) | Раз в спринт или после рефакторинга |
| [backlog-hygiene](../skills/backlog-hygiene/SKILL.md) | Раз в спринт |

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
| [agent-circles](traps/agent-circles.md) | Агенты зацикливаются на одной проблеме | [task-compliance](../skills/task-compliance/SKILL.md) |
| [stale-stack](traps/stale-stack.md) | Агент использует устаревший стек из-за training cutoff | [VersionAuditTest.cs](../tests/patterns/VersionAuditTest.cs) |
| [log-leak](traps/log-leak.md) | PII утекает в логи | [PiiGuardTest.cs](../tests/patterns/PiiGuardTest.cs) |
| [code-duplication](traps/code-duplication.md) | Агент дублирует бизнес-логику вместо реюза | [DuplicationGuardTest.cs](../tests/patterns/DuplicationGuardTest.cs) |
| [dependency-drift](traps/dependency-drift.md) | +1 using/#include замыкает цикл в графе зависимостей | [DependencyDriftTest.cs](../tests/patterns/DependencyDriftTest.cs) |
| [over-engineering](traps/over-engineering.md) | Агент строит архитектурный собор вместо простого решения | [simplicity-audit](../skills/simplicity-audit/SKILL.md) |

---

## Решения и паттерны (docs/solutions/)

| Документ | Что внутри |
|----------|------------|
| [architecture-tests.md](solutions/architecture-tests.md) | Подробный гайд по NetArchTest.eNhancedEdition + regex-сканированию |
| [ai-patterns.md](solutions/ai-patterns.md) | 9 проверенных паттернов AI-driven разработки |
| [human-audit-bridge.md](solutions/human-audit-bridge.md) | Как использовать AI-чеклисты для ручного аудита человеком |
| [ARCHITECTURE-INVENTORY.md](../skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) | Шаблон фиксации текущей архитектуры перед внедрением guardrails |
| [NUMBERED-DECISIONS.md](../skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md) | Шаблон реестра осознанных отклонений (`PERF-###`, `DB-###`, `AUD-###`) |

---

## Интеграции с агентами (docs/agents/)

| Агент | Файл | Формат конфигурации |
|-------|------|---------------------|
| Kimi Code CLI | [KIMI.md](agents/KIMI.md) | `.kimi/skills/{name}/SKILL.md` |
| Claude Code | [CLAUDE-CODE.md](agents/CLAUDE-CODE.md) | `.claude/CLAUDE.md` + commands |
| Cursor | [CURSOR.md](agents/CURSOR.md) | `.cursorrules` + `.cursor/rules/` |
| Codex (OpenAI) | [CODEX.md](agents/CODEX.md) | `.codex/instructions.md` |
| OpenCode | [OPENCODE.md](agents/OPENCODE.md) | `.opencode/instructions.md` |
| Сравнение | [README.md](agents/README.md) | Таблица сравнения всех агентов |

---

## CI / CD

| Артефакт | Назначение |
|----------|------------|
| [ci/github-actions/safe-ci.yml](../ci/github-actions/safe-ci.yml) | Шаблон воркфлоу: build + test + verify-tests |
| [ci/scripts/verify-tests.sh](../ci/scripts/verify-tests.sh) | Проверяет, что `dotnet run` реально выполнил тесты (не 0 ran) |
| [.github/workflows/demo-project-ci.yml](../.github/workflows/demo-project-ci.yml) | CI этого репозитория — собирает DemoProject |

---

## Правила проекта

| Файл | Что внутри |
|------|------------|
| [rules/AGENTS_TEMPLATE.md](../rules/AGENTS_TEMPLATE.md) | Конституция для AI-агентов: EF, тесты, даты, кэш, коммиты |
| [rules/CONVENTIONS.md](../rules/CONVENTIONS.md) | Именование тестов, workflow, CI guardrails |
| [BannedSymbols.txt](../examples/DemoProject/BannedSymbols.txt) | Compile-time guard: запрещённые API (BannedApiAnalyzers RS0030) |

---

## Как обновлять эту карту

При добавлении нового артефакта:
1. Добавить строку в соответствующую таблицу
2. Указать ссылку на паттерн/решение
3. Если новый слой пирамиды — обновить [PYRAMID.md](../PYRAMID.md)
