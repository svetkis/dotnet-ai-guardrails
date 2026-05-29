# .NET Agentic Engineering

> **Vibe coding мёртв. Агент — это быстрый, но опасный аутсорсер. Мы должны выстроить процесс приёмки.**

Этот репозиторий — выстраданный щит из 7 слоёв контроля за агентами в .NET-проектах.
Он содержит готовые к копированию артефакты: правила, скиллы, тестовые паттерны и CI-воркфлоу.

## 7 слоёв контроля (Пирамида)

| Слой | Инструмент | Что ловит |
|------|-----------|-----------|
| 1. Нагрузка | NBomber | Молчаливая поломка write-path при оптимизации read |
| 2. Правила (CLAUDE.md) | Конвенции | `AsNoTracking` в read без `.Select()`, `FindAsync` в read-path |
| 3. Компилятор | `dotnet build`, Snapshot | Изменение DTO без обновления контракта |
| 4. Тесты | TUnit + `dotnet run` | "0 tests ran", но CI зелёный |
| 5. Архитектура | NetArchTest | Нарушение слоёв, запрет `FindAsync` где попало |
| 6. Храповики | Ratchet-тесты | Агент тихо снёс `[HotPath]` или критичную логику |
| 7. E2E + Аудиты | MCP, Персоны | Слепота контекста — агент не видит системные дыры |

## Быстрый старт

1. Скопируй `rules/CLAUDE.md` в свой проект
2. Установи паттерны из `tests/patterns/`
3. Настрой аудиты из `skills/`
4. Внедри `ci/github-actions/safe-ci.yml`

## Структура

```
.
├── AGENTS.md                 # Инструкции для AI-агентов
├── PYRAMID.md                # Подробный разбор 7 слоёв
├── rules/
│   ├── CLAUDE.md             # EF-правила, naming, конвенции
│   └── CONVENTIONS.md        # Коммиты, воркфлоу, тесты
├── skills/
│   ├── security-audit/       # Персона Security-аудитора
│   ├── dba-audit/            # Персона DBA-аудитора
│   ├── ux-audit/             # Персона UX-аудитора
│   └── code-review/          # Персона Code Review
├── tests/
│   ├── patterns/             # Шаблоны тестов (Ratchet, NetArchTest, NBomber)
│   └── conventions/          # Шаблоны именования, TUnit гайд
├── ci/                       # CI/CD guardrails
└── docs/                     # Ловушки и решения
```

## Доклад

Разработано для доклада **DotNext**.

---
*Copy-paste friendly. Fork and survive the agent.*
