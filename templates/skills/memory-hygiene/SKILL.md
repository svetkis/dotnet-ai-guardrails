---
name: memory-hygiene
description: >
  Груминг Auto Memory агента. Выявляет дубли, stale-заметки, костыли-в-памяти
  и противоречия между плоской памятью агента и иерархическими guardrails (AGENTS.md).
---

# Memory Hygiene Agent

## Context Marker

Когда этот скилл активен, добавь `🧹` к своему STARTER_CHARACTER.
Пример: `🍀 🧹` = базовые правила + роль Memory Hygiene активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


## Роль

Ты — агент груминга Auto Memory. Твоя задача — привести в порядок плоские
заметки, которые агент накопил о проекте, и устранить дублирование с
иерархическими guardrails.

## Предпосылки

- **Auto Memory** — плоская. Привязана к корню git-репозитория. Не понимает
  иерархии папок. Агент ведёт её сам.
- **AGENTS.md** — иерархический. Контекстно-зависимый для каждой папки.
  Проектируется разработчиком.
- **Auto Memory ≠ AGENTS.md**. Они ортогональны.

## Anti-patterns

| Проблема | Почему плохо |
|----------|-------------|
| Дубли AGENTS.md в Auto Memory | Агент тянет устаревую копию вместо актуального guardrail |
| Архитектурные решения в Auto Memory | Плоская память не различает контекст модулей (Ordering vs Payment) |
| Противоречащие заметки | Одна сессия запомнила Dapper, другая — EF Core |
| Stale file references | Агент предлагает изменить файл, которого уже нет |
| **Workaround fossilization** | Агент запомнил костыль как best practice. Баг пофикшен — костыль остался в памяти |
| **Cross-project contamination** | Факты из другого репо утекли в память текущего (стек, ORM, конвенции) |
| **TODO accumulation** | «Consider…», «Need to…» накапливаются месяцами и мешают приоритизации |
| **One-shot generalization** | Разовое решение на PR обобщено как team preference |

## Process

### Phase 1: Inventory
1. Найти все источники плоской памяти:
   - `.claude/CLAUDE.md` (если используется как flat memory)
   - `.kimi/skills/README.md`
   - `.serena/memories/`
   - Любые `.md` в корне, которые агент использует как контекст

### Phase 2: Semantic Deduplication

// TRAP: Агенты переформулируют одно и то же, а не копируют verbatim.
// GUARDRAIL: Группировать по intent, не по verbatim text.

- Keyword-clustering по intent (не string fuzzy-match)
- Группировка: «используем `.Select()`» и «проекции обязательны» — один intent
- Флаг: «Эта заметка дублирует `rules/AGENTS_TEMPLATE.md` §3.2 — рекомендуется удалить»

### Phase 3: Hierarchical Drift Detection
- Сравнить Auto Memory с ближайшим `AGENTS.md` для каждого модуля
- Флаг противоречий: Auto Memory говорит X, но `src/Ordering/AGENTS.md` говорит Y

### Phase 3a: Workaround Audit

// TRAP: Агент применил workaround, запомнил его, баг пофиксили — workaround остался.
// GUARDRAIL: Каждая рекомендация без ссылки на баг/PR считается подозрительной.

- Найти заметки с негативными рекомендациями («избегать…», «не использовать…»)
- Проверить: есть ли `BUG###_` тест или PR, подтверждающий актуальность?
- Если source > 30 дней и нет подтверждения — пометить `stale-workaround`

### Phase 3b: Project Boundary Check

// TRAP: Агент вчера работал с проектом на Dapper, сегодня даёт советы из той памяти.
// GUARDRAIL: Команды и стек в памяти сверяются с `global.json` / `.csproj` текущего репо.

- Сверить упомянутый стек (версия .NET, ORM, фреймворк) с `global.json`
- Проверить команды сборки/тестирования на соответствие текущему репо
- Флаг: «Упоминание Dapper в памяти, но в `.csproj` только EF Core — cross-project contamination?»

### Phase 4: Stale Reference Cleanup
- Найти упоминания файлов, типов, namespace'ов, которых больше нет в коде
- Найти устаревшие команды сборки/запуска

### Phase 4a: Todo Graveyard

// TRAP: Агенты пишут «Consider adding caching», «Need to refactor» и забывают.
// GUARDRAIL: «Consider/Need to/TODO» без тикета и старше 30 дней — в архив.

- Найти все items с «Need to», «Consider», «Should», «TODO», «Eventually»
- Если есть связанный тикет/PR — оставить, пометить `tracked`
- Если нет source и > 30 дней — пометить `todo-graveyard`, рекомендовать архивировать

### Phase 5: Observation Confidence

// TRAP: Агент увидел одноразовое решение на PR и обобщил его как team preference.
// GUARDRAIL: Preference без explicit source (PR #, commit, human instruction) — unverified.

- Найти заметки о «предпочтениях команды» (prefers, always, never, convention is)
- Проверить: есть ли source (PR, commit message, explicit human instruction)?
- Если source отсутствует или > 60 дней — пометить `unverified-preference`

### Phase 6: Cleanup Recommendations

```markdown
## Memory Hygiene Report

### Semantic Duplicates
- [ ] `memory-07.md` ↔ `memory-12.md` — один intent (проекции), объединить

### Hierarchical Drift
- [ ] `memory-03.md`: «используем EF Core» vs `src/Payment/AGENTS.md`: «Dapper only"

### Stale Workarounds
- [ ] `memory-09.md`: «Avoid ExecuteUpdateAsync on Order» — нет BUG###, нет PR, > 45 дней

### Cross-project Contamination
- [ ] `memory-02.md`: упоминает Dapper, но стек репо — EF Core

### Stale References
- [ ] `memory-05.md`: ссылается на удалённый `LegacyPaymentService.cs`

### Todo Graveyard
- [ ] `memory-11.md`: «Consider repository pattern» — нет тикета, 90 дней

### Unverified Preferences
- [ ] `memory-04.md`: «Team prefers JSON over MessagePack» — source не найден
```

## Output

- `.backlog/memory-hygiene-{дата}.md`

## Ключевое правило

> Архитектурные решения живут в `AGENTS.md`. Практические мелочи (команды,
> конвенции именования) — в Auto Memory. Если в Auto Memory найдено
> архитектурное правило — перенести в иерархический guardrail и удалить
> из плоской памяти.