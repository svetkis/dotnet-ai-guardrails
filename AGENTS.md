# AGENTS.md — Skeptical AI Engineering

> **Skeptical AI Engineering (SAE)** — методология масштабирования контроля качества через AI-агентов.  
> Доклад «ИИ уверен. Я нет» (Dotnext 2026).  
> Этот файл управляет поведением AI-агентов в этом репозитории.

## Миссия

Репозиторий содержит **артефакты контроля качества** для .NET-проектов. Работают в классических процессах и с AI-агентами.
Не пиши здесь доменный код — только guardrails, скиллы, паттерны и примеры.

## Правила работы с репозиторием

### Никогда
- ❌ Не добавляй зависимости без явного запроса
- ❌ Не меняй структуру папок (rules/, templates/skills/, tests/, ci/, docs/)
- ❌ Не убирай примеры кода из `tests/patterns/` — они шаблонные
- ❌ Не используй `dotnet test` в примерах — только `dotnet run --project`

### Всегда
- ✅ Обновляй `PYRAMID.md` при добавлении нового слоя (сейчас 3 слоя 0–2 + внешний цикл)
- ✅ Обновляй `docs/agents/` при добавлении поддержки нового AI-агента
- ✅ Обновляй `docs/README.md` (карта знаний) при добавлении нового артефакта
- ✅ Каждый новый скилл в `templates/skills/` должен содержать `SKILL.md` + `CHECKLIST.md`
- ✅ Каждый новый паттерн теста — с комментарием `// TRAP: ...` и `// GUARDRAIL: ...`
- ✅ Примеры кода компилируются (минимальный `examples/DemoProject/` если нужна проверка)

## Стек репозитория

- Документация: Markdown
- Примеры кода: .NET 10, TUnit, NBomber, NetArchTest
- CI: GitHub Actions

## Как применить к своему проекту

Этот репозиторий — **набор защитных артефактов**, а не NuGet-пакет. Чтобы натянуть его на свой .NET-проект:

**Полный гайд:** [`docs/ONBOARDING.md`](docs/ONBOARDING.md) — пошаговый план внедрения с контрольными точками и антипаттернами.

Краткая сводка:

| Шаг | Что делать | Куда идти |
|-----|-----------|-----------|
| 0. Зафиксировать архитектуру | Заполнить инвентарь сборок, критичных путей и осознанных отклонений | [`templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md`](templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) + [`DECISION-GUARDS.md`](templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) |
| 1. Оценить зрелость | Запустить onboarding-скилл или ручную оценку | [`.agents/skills/skeptical-ai-bootstrap/SKILL.md`](.agents/skills/skeptical-ai-bootstrap/SKILL.md) |
| 2. Адаптировать | Вычеркнуть неприменимые проверки под свой стек | [`templates/skills/ADAPTATION.md`](templates/skills/ADAPTATION.md) |
| 3. Конституция | Скопировать `rules/AGENTS_TEMPLATE.md` в корень проекта и отредактировать под свой стек | [`rules/AGENTS_TEMPLATE.md`](rules/AGENTS_TEMPLATE.md) |
| 4. Пирамида | Внедрить слои 1→2 по бэклогу из onboarding | [`PYRAMID.md`](PYRAMID.md) |
| 5. Агент | Настроить свой AI-агент (Kimi / Claude / Cursor / Codex) | [`docs/agents/`](docs/agents/) |
| 6. Приёмочный цикл | Внедрить аудиты + нагрузку (ручной или AI) | [`docs/solutions/human-audit-bridge.md`](docs/solutions/human-audit-bridge.md) |

> **Принцип:** не копируй всё подряд — адаптируй или создавай новое. См. антипаттерны в [`docs/ONBOARDING.md`](docs/ONBOARDING.md) §«Антипаттерны внедрения».

---

## Контекст для внешних агентов

Если ты — AI-агент, читающий этот файл в контексте **другого** репозитория:
- Этот репозиторий (`dotnet-skeptical-ai`) — **архив методологии**, а не шаблон проекта.
- НЕ копируй его структуру папок (`rules/`, `templates/skills/`, `examples/`, `tests/patterns/`) в целевой проект.
- НЕ создавай `DemoProject`, `examples/` или `DemoProject.Traps/` в целевом проекте.
- Забирай только **принципы** и **markdown-гайдлайны**, применимые к стеку целевого проекта.
- Твой выход — отчёты оценки и планы, а не имплементационный код.

## Навигация

**Потерялись?** Начните с [docs/README.md](docs/README.md) — единая карта всех артефактов.  
**Незнакомые термины?** См. [GLOSSARY.md](GLOSSARY.md).

| Что нужно | Куда идти |
|-----------|-----------|
| Правила для агента | `rules/AGENTS_TEMPLATE.md` |
| Add-on: EF Core | `rules/AGENTS_TEMPLATE.efcore.md` |
| Add-on: Dapper / Raw SQL | `rules/AGENTS_TEMPLATE.dapper.md` |
| Протокол онбординга (не создавать DemoProject) | `docs/agents/BOOTSTRAP-PROTOCOL.md` |
| Аудит безопасности | `templates/skills/security-audit/` |
| Аудит БД | `templates/skills/dba-audit/` |
| Аудит производительности | `templates/skills/performance-audit/` |
| Аудит дизайна API | `templates/skills/api-design-audit/` |
| Аудит бота | `templates/skills/bot-audit/` |
| Аудит локализации | `templates/skills/i18n-audit/` |
| Pre-commit code review агент | `templates/skills/code-review/` |
| Frontend pre-commit code review агент | `templates/skills/frontend-code-review/` |
| Проверка scope | `templates/skills/task-compliance/` |
| Паттерн теста | `tests/patterns/` |
| Рабочий пример | `examples/DemoProject/` |
| Рабочий пример (Single-project MVP) | `examples/DemoProject.MinimalApi/` |
| Failing demo (guardrails) | `examples/DemoProject.Traps/` |
| CI безопасность | `ci/github-actions/safe-ci.yml` |
| Описание ловушки | `docs/traps/` |
| Архитектурные тесты | `docs/solutions/architecture-tests.md` |
| Паттерны AI-разработки | `docs/solutions/ai-patterns.md` |
| Осознанные отклонения (Decision Guards / ADR) | `templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md` |
| Онбординг проекта | `templates/skills/skeptical-ai-bootstrap/` |
| Интеграция с Kimi | `docs/agents/KIMI.md` |
| Интеграция с Claude Code | `docs/agents/CLAUDE-CODE.md` |
| Интеграция с Cursor | `docs/agents/CURSOR.md` |
| Интеграция с Codex | `docs/agents/CODEX.md` |
| Интеграция с OpenCode | `docs/agents/OPENCODE.md` |
| Bootstrap Protocol | `docs/agents/BOOTSTRAP-PROTOCOL.md` |
| Сравнение агентов | `docs/agents/README.md` |
| Контрибуция | `CONTRIBUTING.md` |
| Лицензия | `LICENSE` |
