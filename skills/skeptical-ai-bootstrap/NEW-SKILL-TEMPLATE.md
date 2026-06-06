# Шаблон нового скилла

> Используй этот шаблон, когда готовые артефакты из `dotnet-skeptical-ai`
> не подходят под стек или архитектуру проекта.

---

```markdown
---
name: {skill-name}
description: >
  {Краткое описание: что проверяет, для какого стека/архитектуры}
---

# {Название} — Skill

## Почему создан

Готовый скилл `{оригинальный скилл}` не подходит, потому что:
- {стек проекта отличается}
- {архитектура требует других проверок}
- {специфичные риски проекта}

## Context Marker

Когда этот скилл активен, добавь `{marker}` к своему STARTER_CHARACTER.
Пример: `🍀 {marker}` = базовые правила + роль {роль} активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.

## Роль

Ты — {роль} в .NET-проекте. Твоя задача — {что делать}.

## Контекст проекта

- .NET: {version}
- Тип приложения: {Web API / Worker / Desktop / etc.}
- ORM/Данные: {EF Core / Dapper / Mongo / etc.}
- Архитектура: {Clean / Vertical Slice / Modular / etc.}
- Особенности: {что важно знать агенту}

## Принцип

{Один абзац: что защищаем и почему это важно}

## Правила / Чеклист

### {Категория 1}
- [ ] {правило 1}
- [ ] {правило 2}

### {Категория 2}
- [ ] {правило 3}
- [ ] {правило 4}

## Anti-Hallucination Protocol

Каждая находка ДОЛЖНА включать:
1. **Точный путь к файлу** и **номер строки**
2. **Цитату кода** (3-5 строк)
3. **Нарушенное правило** (из списка выше)
4. **Исправление**: конкретное действие или код

Если не можешь указать 1-4 — НЕ репортишь находку.

## Формат отчёта

```markdown
## {Название аудита} — {дата}

### Критично
- [ ] {описание} → {файл:строка}

### Средне
- [ ] {описание} → {файл:строка}

### Рекомендации
- {описание}
```

## Инструкция по запуску

- **Когда запускать:** {на каждый PR / раз в спринт / по триггеру}
- **На что смотреть:** {какие файлы/изменения триггерят запуск}
- **Кто потребитель:** {программист / QA / human gate}

## Интеграция

- **Input от:** {откуда берём контекст}
- **Output to:** {кому передаём результаты}
- **Runs before/after:** {связь с другими скиллами}
```

---

## Примеры создания скиллов по мотивам проектов

### Пример 1: Vertical Slice Architecture

```markdown
---
name: architecture-audit-vslice

> Примечание: NetArchTest ВСЁ-ТАКИ работает с Vertical Slice — нужны только
> custom rules про границы фич, а не про слои. Regex — не единственный путь.
> См. примеры в `SKILL-ARCHITECTURE.md`.

---

# Architecture Audit — Vertical Slice

## Почему создан
NetArchTest проверяет зависимости между слоями (Domain/Application/Infrastructure),
но в Vertical Slice границы — по фичам (Features/Orders/, Features/Payments/),
а не по слоям. Нужен сканер, который проверяет:
- Slice A не импортирует внутренности Slice B
- Каждый Slice имеет чёткий API (Handler/Endpoint/Validator)
- Нет shared database без явного контракта
```

### Пример 2: Dapper + SQL Server

```markdown
name: dba-audit-dapper
---
# DBA Audit — Dapper

## Почему создан
Готовый `dba-audit` заточен под EF Core (миграции, Include, AsNoTracking).
В проекте Dapper — проверяем:
- Raw SQL параметризован (нет string interpolation в SQL)
- Нет SELECT * (explicit column list)
- Используются async-методы (QueryAsync, ExecuteAsync)
- Нет N+1 без явного комментария // DECISION:
```

### Пример 3: .NET Framework 4.8 + WPF

```markdown
name: code-review-wpf
---
# Code Review — WPF Desktop

## Почему создан
Готовый `code-review` про Minimal API и EF Core.
В WPF проверяем:
- ViewModel не обращается к БД напрямую (через Service)
- INotifyPropertyChanged реализован корректно
- Нет blocking call в UI thread (async/await)
- Commands используют AsyncCommand, не void
```
```
