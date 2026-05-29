# Contributing / Как контрибьютить

> 🌐 **English version** → [scroll down or click here](#english)  
> 🌐 **Русская версия** → [ниже или кликни сюда](#russian)

---

<a id="russian"></a>
# 🇷🇺 Как контрибьютить

Спасибо, что хотите помочь! Этот репозиторий — защитные артефакты для .NET-проектов, работающих с AI-агентами. Мы принимаем улучшения, новые скиллы, паттерны тестов и документацию.

## Философия

- **Принципы важнее артефактов.** Не навязывай — адаптируй или создавай новое.
- **Минимальные изменения.** Каждый PR должен решать одну задачу.
- **Документация = код.** Изменил правило — обнови документацию в том же коммите.

## Что можно добавлять

| Тип | Куда | Требования |
|-----|------|------------|
| Новый скилл | `skills/{name}/` | `SKILL.md` + `CHECKLIST.md` |
| Новый паттерн теста | `tests/patterns/` | Комментарии `// TRAP: ...` и `// GUARDRAIL: ...` |
| Новая ловушка | `docs/traps/` | Сценарий + последствия + решение + ссылка на паттерн |
| Новое решение | `docs/solutions/` | Подробный разбор с примерами |
| Адаптация агента | `docs/agents/` | Инструкция по интеграции |
| CI-улучшение | `ci/` | Проверено на GitHub Actions |

## Чего НЕ делать

- ❌ Не добавляй зависимости без явного запроса
- ❌ Не меняй структуру папок (`rules/`, `skills/`, `tests/`, `ci/`, `docs/`)
- ❌ Не убирай примеры кода из `tests/patterns/` — они шаблонные
- ❌ Не используй `dotnet test` в примерах — только `dotnet run --project`

## Процесс

1. **Форкни** репозиторий
2. **Создай ветку** по Conventional Commits: `feat/skill-name`, `fix/trap-description`
3. **Внеси изменения** согласно чеклисту ниже
4. **Проверь** `dotnet build` в `examples/DemoProject/` (если изменял код)
5. **Открой PR** с описанием: что, зачем, как проверено

## Чеклист перед PR

- [ ] Я прочитал `AGENTS.md` и `rules/AGENTS.md`
- [ ] Если добавлял скилл — есть `SKILL.md` + `CHECKLIST.md`
- [ ] Если добавлял паттерн — есть комментарии `// TRAP:` и `// GUARDRAIL:`
- [ ] Если менял пирамиду — обновил `PYRAMID.md`
- [ ] Если добавлял агента — обновил `docs/agents/`
- [ ] `dotnet build` проходит без warning'ов (если применимо)
- [ ] Коммиты следуют Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`)

## Стиль коммитов

```
feat: добавлен скилл ux-audit для WPF-приложений
test: ratchet-тест на количество Job-классов
docs: обновлена таблица сравнения агентов
fix: исправлен путь в SnapshotTest.cs
```

## Обновление PYRAMID.md

Каждый новый слой или существенное изменение архитектуры обратной связи требует обновления `PYRAMID.md`:
- Добавить слой в таблицу
- Обновить диаграмму синтеза
- Добавить ROI-оценку
- Обновить метрики эффективности (если есть данные)

## Вопросы?

Открывай Issue с префиксом `[question]` или `[proposal]`.

---

<a id="english"></a>
# 🇬🇧 How to Contribute

Thank you for your interest! This repository contains defensive artifacts for .NET projects working with AI agents. We welcome improvements, new skills, test patterns, and documentation.

## Philosophy

- **Principles over artifacts.** Don't force-fit — adapt or create anew.
- **Minimal changes.** Each PR should solve one thing.
- **Documentation is code.** Changed a rule — update docs in the same commit.

## What you can add

| Type | Location | Requirements |
|------|----------|--------------|
| New skill | `skills/{name}/` | `SKILL.md` + `CHECKLIST.md` |
| New test pattern | `tests/patterns/` | Comments `// TRAP: ...` and `// GUARDRAIL: ...` |
| New trap | `docs/traps/` | Scenario + consequences + solution + pattern link |
| New solution | `docs/solutions/` | Detailed guide with examples |
| Agent adaptation | `docs/agents/` | Integration instructions |
| CI improvement | `ci/` | Verified on GitHub Actions |

## What NOT to do

- ❌ Don't add dependencies without explicit request
- ❌ Don't change folder structure (`rules/`, `skills/`, `tests/`, `ci/`, `docs/`)
- ❌ Don't remove code examples from `tests/patterns/` — they are templates
- ❌ Don't use `dotnet test` in examples — only `dotnet run --project`

## Process

1. **Fork** the repository
2. **Create a branch** following Conventional Commits: `feat/skill-name`, `fix/trap-description`
3. **Make changes** according to the checklist below
4. **Verify** `dotnet build` in `examples/DemoProject/` (if you changed code)
5. **Open a PR** with description: what, why, how tested

## Pre-PR Checklist

- [ ] I have read `AGENTS.md` and `rules/AGENTS.md`
- [ ] If adding a skill — `SKILL.md` + `CHECKLIST.md` are present
- [ ] If adding a pattern — comments `// TRAP:` and `// GUARDRAIL:` are present
- [ ] If changing the pyramid — `PYRAMID.md` is updated
- [ ] If adding an agent — `docs/agents/` is updated
- [ ] `dotnet build` passes without warnings (if applicable)
- [ ] Commits follow Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`)

## Commit style

```
feat: add ux-audit skill for WPF applications
test: ratchet test for Job class count
docs: update agent comparison table
fix: correct path in SnapshotTest.cs
```

## Updating PYRAMID.md

Every new layer or significant change to the feedback architecture requires updating `PYRAMID.md`:
- Add the layer to the table
- Update the synthesis diagram
- Add ROI assessment
- Update effectiveness metrics (if you have data)

## Questions?

Open an Issue with prefix `[question]` or `[proposal]`.
