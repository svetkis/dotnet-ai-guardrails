# AGENTS.md — Skeptical AI Engineering

> **Skeptical AI Engineering (SAE)** — методология защиты .NET-проектов от ошибок AI-агентов.  
> Доклад «ИИ уверен. Я нет» (Dotnext 2026).  
> Этот файл управляет поведением AI-агентов в этом репозитории.

## Миссия

Репозиторий содержит **защитные артефакты** для .NET-проектов, работающих с AI-агентами.
Не пиши здесь доменный код — только guardrails, скиллы, паттерны и примеры.

## Правила работы с репозиторием

### Никогда
- ❌ Не добавляй зависимости без явного запроса
- ❌ Не меняй структуру папок (rules/, skills/, tests/, ci/, docs/)
- ❌ Не убирай примеры кода из `tests/patterns/` — они шаблонные
- ❌ Не используй `dotnet test` в примерах — только `dotnet run --project`

### Всегда
- ✅ Обновляй `PYRAMID.md` при добавлении нового слоя (сейчас 6 слоёв 0–5 + внешний цикл)
- ✅ Обновляй `docs/agents/` при добавлении поддержки нового AI-агента
- ✅ Обновляй `docs/README.md` (карта знаний) при добавлении нового артефакта
- ✅ Каждый новый скилл в `skills/` должен содержать `SKILL.md` + `CHECKLIST.md`
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
| 0. Зафиксировать архитектуру | Заполнить инвентарь сборок, критичных путей и осознанных отклонений | [`skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md`](skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) + [`NUMBERED-DECISIONS.md`](skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md) |
| 1. Оценить зрелость | Запустить onboarding-скилл или ручную оценку | [`skills/skeptical-ai-bootstrap/SKILL.md`](skills/skeptical-ai-bootstrap/SKILL.md) |
| 2. Адаптировать | Вычеркнуть неприменимые проверки под свой стек | [`skills/ADAPTATION.md`](skills/ADAPTATION.md) |
| 3. Конституция | Скопировать `rules/AGENTS_TEMPLATE.md` в корень проекта и отредактировать под свой стек | [`rules/AGENTS_TEMPLATE.md`](rules/AGENTS_TEMPLATE.md) |
| 4. Пирамида | Внедрить слои 1→5 по бэклогу из onboarding | [`PYRAMID.md`](PYRAMID.md) |
| 5. Агент | Настроить свой AI-агент (Kimi / Claude / Cursor / Codex) | [`docs/agents/`](docs/agents/) |
| 6. Аудиты | Внедрить внешний цикл (ручной или AI) | [`docs/solutions/human-audit-bridge.md`](docs/solutions/human-audit-bridge.md) |

> **Принцип:** не копируй всё подряд — адаптируй или создавай новое. См. антипаттерны в [`docs/ONBOARDING.md`](docs/ONBOARDING.md) §«Антипаттерны внедрения».

---

## Навигация

**Потерялись?** Начните с [docs/README.md](docs/README.md) — единая карта всех артефактов.  
**Незнакомые термины?** См. [GLOSSARY.md](GLOSSARY.md).

| Что нужно | Куда идти |
|-----------|-----------|
| Правила для агента | `rules/AGENTS_TEMPLATE.md` |
| Аудит безопасности | `skills/security-audit/` |
| Аудит БД | `skills/dba-audit/` |
| Аудит производительности | `skills/performance-audit/` |
| Аудит дизайна API | `skills/api-design-audit/` |
| Аудит бота | `skills/bot-audit/` |
| Аудит локализации | `skills/i18n-audit/` |
| Code review агент | `skills/code-review/` |
| Проверка scope | `skills/task-compliance/` |
| Паттерн теста | `tests/patterns/` |
| Рабочий пример | `examples/DemoProject/` |
| Failing demo (guardrails) | `examples/DemoProject.Traps/` |
| CI безопасность | `ci/github-actions/safe-ci.yml` |
| Описание ловушки | `docs/traps/` |
| Архитектурные тесты | `docs/solutions/architecture-tests.md` |
| Паттерны AI-разработки | `docs/solutions/ai-patterns.md` |
| Осознанные отклонения (Numbered Decisions) | `skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md` |
| Онбординг проекта | `skills/skeptical-ai-bootstrap/` |
| Интеграция с Kimi | `docs/agents/KIMI.md` |
| Интеграция с Claude Code | `docs/agents/CLAUDE-CODE.md` |
| Интеграция с Codex | `docs/agents/CODEX.md` |
| Интеграция с OpenCode | `docs/agents/OPENCODE.md` |
| Сравнение агентов | `docs/agents/README.md` |
| Контрибуция | `CONTRIBUTING.md` |
| Лицензия | `LICENSE` |
