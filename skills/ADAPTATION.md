# Адаптация скиллов под проект / Skill Adaptation Guide

> Копировать скиллы — просто. Адаптировать — важно.
> Этот гайд поможет вычеркнуть неприменимые проверки до первого запуска.

---

## Быстрый чеклист (3 шага)

1. **Определи стек**
   - .NET версия, тип приложения, ORM, архитектура, тестовый фреймворк
   - Посмотри `.csproj`, `global.json`, структуру папок

2. **Вычеркни неприменимое**
   - Для каждого скилла проверь таблицу ниже
   - Пометь пункты N/A в CHECKLIST.md перед запуском

3. **Запусти и отфильтруй**
   - После первого прогона посмотри находки с меткой `[REVIEW]`
   - Если находка — false positive, добавь условие в SKILL.md проекта

---

## Таблица: если в проекте… → пропусти эти проверки

| Если в проекте… | Пропусти в скилле… | Почему |
|---|---|---|
| Нет Clean Architecture (single-project MVP) | `code-review` → проверку слоёв (NetArchTest) | Нет проектов Domain / Infrastructure — правило неприменимо |
| Minimal API (не MVC) | `security-audit` → `[Authorize]` / `[AllowAnonymous]` | Minimal API использует `.RequireAuthorization()` или middleware |
| Minimal API | `code-review` → проверку `[Authorize]` | См. выше |
| Dapper / ADO.NET (нет EF Core) | `performance-audit` → `.AsNoTracking()`, `.Include()`, `FindAsync()` | Правила EF Core неприменимы к raw SQL |
| Dapper / ADO.NET | `dba-audit` → миграции, `Include()`, `FindAsync()` | DBA-аудит для Dapper — другой скилл |
| Проекции `.Select()` в DTO повсюду | `performance-audit`, `code-review` → отсутствие `.AsNoTracking()` | EF Core не отслеживает проекции, AsNoTracking не нужен |
| Raw SQL (`FromSqlRaw`, `ExecuteUpdateAsync`) | `code-review`, `performance-audit` → `.AsNoTracking()` на write-path | Change Tracker не отслеживает raw SQL |
| Worker Service (нет HTTP) | `security-audit` → rate limiting, XSS, `[Authorize]` | Worker не имеет endpoints в классическом смысле |
| Worker Service | `code-review` → OpenAPI snapshot, DTO API | Worker не возвращает HTTP-ответы |
| .NET Framework 4.8 | Все скиллы → NetArchTest, TUnit, Minimal API | Стек отличается кардинально; используй `skeptical-ai-bootstrap` |
| Razor Pages | `code-review` → проверку Minimal API | Razor Pages используют PageModel, не endpoint-роутинг |
| Vertical Slice Architecture | `code-review` → стандартные слоёвые правила | Границы по фичам, а не по слоям; используй custom NetArchTest |

---

## Confidence Level: как интерпретировать

Все аудит-скиллы с версии 2025-06 помечают находки уровнем уверенности:

| Маркер | Значение | Действие |
|---|---|---|
| `[CERTAIN]` | Точно баг / уязвимость | Исправляй или создавай задачу сразу |
| `[REVIEW]` | Возможен false positive | Проверь human'ом перед действием. Частые причины: проекция без AsNoTracking, endpoint без `[Authorize]` но с middleware, single-project без Clean Architecture |

**Правило:** если агент не уверен — он ставит `[REVIEW]`. Это не слабость, это честность.

---

## Пример адаптации

Проект: **BetweenTheLines** (.NET 10, EF Core, PostgreSQL, Minimal API, single-project MVP)

Адаптации:
1. `code-review` — вычеркнуто: проверка слоёв (нет Clean Architecture). Добавлено: `.RequireAuthorization()` вместо `[Authorize]`.
2. `performance-audit` — вычеркнуто: AsNoTracking на `.Select()`-проекциях. Добавлено: исключение для `FromSqlRaw("UPDATE...")`.
3. `security-audit` — вычеркнуто: `[Authorize]` / `[AllowAnonymous]`. Добавлено: проверка `.RequireAuthorization()` и защита webhook'ов через secret token.

---

## Если ничего не подходит

Если готовые скиллы требуют более 50% адаптации — не адаптируй, а создай новый:

1. Запусти `skeptical-ai-bootstrap` — он даст фреймворк для создания нового скилла
2. Используй `SKILL-ARCHITECTURE.md` для проектирования guardrail'а с нуля
3. Используй `NEW-SKILL-TEMPLATE.md` для генерации файлов нового скилла

См. `skills/skeptical-ai-bootstrap/SKILL-ARCHITECTURE.md`
