# План перевода документации на два языка

## Цель

Вся документация в `docs/` доступна на русском (primary) и английском. Промпты для агентов (`rules/AGENTS.md`, `skills/*/SKILL.md`, `skills/*/CHECKLIST.md`) — на русском, переводить не нужно.

## Конвенция именования

Используем тот же подход, что и для `README.md` / `README.en.md`:

- `File.md` — русская версия (primary, canonical)
- `File.en.md` — английская версия

Пример:
```
docs/traps/silent-breakdown.md      # русский
docs/traps/silent-breakdown.en.md   # английский
```

## Scope

### Приоритет 1 (core docs)
- [x] `docs/obstacles/context-rot.md` → `docs/obstacles/context-rot.en.md`
- [x] `docs/relationships.mmd` → `docs/relationships.en.mmd` (Mermaid graph + Node Inventory)
- [x] `docs/solutions/ai-patterns.md` → `docs/solutions/ai-patterns.en.md`
- [x] `docs/solutions/architecture-tests.md` → `docs/solutions/architecture-tests.en.md`

### Приоритет 2 (traps — most referenced)
- [x] `docs/traps/silent-breakdown.md` → `docs/traps/silent-breakdown.en.md`
- [x] `docs/traps/context-blindness.md` → `docs/traps/context-blindness.en.md`
- [x] `docs/traps/vibe-refactoring.md` → `docs/traps/vibe-refactoring.en.md`
- [x] `docs/traps/log-leak.md` → `docs/traps/log-leak.en.md`

### Приоритет 3 (remaining traps)
- [x] `docs/traps/false-safety.md` → `docs/traps/false-safety.en.md`
- [x] `docs/traps/stale-stack.md` → `docs/traps/stale-stack.en.md`
- [x] `docs/traps/agent-circles.md` → `docs/traps/agent-circles.en.md`
- [x] `docs/traps/p50-vs-max.md` → `docs/traps/p50-vs-max.en.md`

### Приоритет 4 (agent integrations)
- [x] `docs/agents/README.md` → `docs/agents/README.en.md`
- [x] `docs/agents/KIMI.md` → `docs/agents/KIMI.en.md`
- [x] `docs/agents/CLAUDE-CODE.md` → `docs/agents/CLAUDE-CODE.en.md`
- [x] `docs/agents/CODEX.md` → `docs/agents/CODEX.en.md`
- [x] `docs/agents/OPENCODE.md` → `docs/agents/OPENCODE.en.md`
- [x] `docs/agents/CURSOR.md` → `docs/agents/CURSOR.en.md`

## Правила перевода

### Стилистика
1. **Сохранить структуру** — заголовки, таблицы, списки, code blocks идентичны оригиналу
2. **Сохранить ссылки** — все внутренние ссылки (`docs/traps/...`, `skills/...`, `tests/...`) остаются как есть
3. **Сохранить code blocks** — C#, JSON, bash не переводятся
4. **Сохранить термины** — `Read-path`, `Write-path`, `BUG###`, `Ratchet`, `Numbered Decision` не переводятся
5. **Адаптировать ссылки на augmented-coding-patterns** — оставить как есть (они ведут на английский репозиторий)

### Языковые конвенции
| Русский | English |
|---------|---------|
| Ловушка | Trap |
| Препятствие | Obstacle |
| Guardrail | Guardrail (не переводить) |
| Агент | Agent |
| Ревью | Review |
| Скилл | Skill |
| Аудит | Audit |

### Проверка качества
После перевода каждого файла:
1. Сверить количество заголовков (должно совпадать)
2. Сверить количество code blocks (должно совпадать)
3. Сверить все внутренние ссылки (не должно быть битых)
4. Убедиться, что английская версия не содержит кириллицы за исключением цитат

## Как запускать агенту

```
Переведи {файл} на английский по правилам из docs/TRANSLATION_PLAN.md.
Сохрани результат как {файл}.en.md.
После перевода проверь: совпадает ли структура с оригиналом?
```

## Примечание

- Файлы `rules/AGENTS.md` — на английском, переводить не нужно.
- Скиллы в `skills/*/` — на русском (primary), английские версии не требуются.
