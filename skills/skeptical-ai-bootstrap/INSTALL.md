# Установка скилла Skeptical AI Bootstrap

## Зачем

Этот скилл устанавливается в **твой** .NET-проект, чтобы Kimi Code CLI мог просканировать его и выдать бэклог внедрения guardrails.

## Быстрая установка

### 1. Скопируй скилл в свой проект

Из репозитория `dotnet-skeptical-ai` скопируй папку в свой проект:

```bash
# Находясь в корне ТВОЕГО .NET-проекта
cp -r /path/to/dotnet-skeptical-ai/skills/skeptical-ai-bootstrap ./.kimi/skills/
```

Или вручную:
- Создать `.kimi/skills/skeptical-ai-bootstrap/` в своём проекте
- Скопировать `SKILL.md`, `CHECKLIST.md`, `EXAMPLE-REPORT.md`

**Важно:** выбери язык. В целевом проекте должен быть только один язык:
- **Русский** → скопируй `SKILL.md` (RU), не копируй `SKILL.en.md`
- **Английский** → скопируй `SKILL.en.md` и переименуй в `SKILL.md`

### 2. Убедись, что Kimi Code CLI видит скилл

```bash
kimi skills list
```

Должен появиться `skeptical-ai-bootstrap` в списке.

### 3. Запусти онбординг

```bash
kimi run skeptical-ai-bootstrap
```

Или в чате с Kimi:
```
@skeptical-ai-bootstrap просканируй этот проект в режиме standard
```

### 4. Получи отчёт

Агент сгенерирует файл `.backlog/onboarding-{дата}.md` в твоём проекте + выведет сводку в чат.

## Альтернатива: онбординг без установки скилла

Если не хочешь ставить скилл, просто открой свой проект в Kimi Code CLI и попроси:

```
Просканируй этот .NET-проект по методологии 5 слоёв пирамиды из dotnet-skeptical-ai.
Выдай бэклог внедрения guardrails.
```

Агент сам найдёт `.csproj`, оценит слои и предложит план.

## После онбординга

Отчёт содержит ссылки на артефакты из `dotnet-skeptical-ai`:

| Артефакт | Откуда взять |
|----------|--------------|
| `rules/AGENTS_TEMPLATE.md` | `dotnet-skeptical-ai/rules/AGENTS_TEMPLATE.md` |
| `rules/CONVENTIONS.md` | `dotnet-skeptical-ai/rules/CONVENTIONS.md` |
| Архитектурные тесты | `dotnet-skeptical-ai/tests/patterns/ArchitectureRules.cs` |
| Ratchet тесты | `dotnet-skeptical-ai/tests/patterns/RatchetTest.cs` |
| CI воркфлоу | `dotnet-skeptical-ai/ci/github-actions/safe-ci.yml` |
| Code review скилл | `dotnet-skeptical-ai/skills/code-review/` (RU или EN — один язык) |
| Аудиты | `dotnet-skeptical-ai/skills/*-audit/` (RU или EN — один язык) |
| Груминг | `dotnet-skeptical-ai/skills/memory-hygiene/`, `doc-hygiene/`, `backlog-hygiene/` (RU или EN — один язык) |

**Рекомендация:** форкни `dotnet-skeptical-ai` и указывай артефакты из своего форка — так контролируешь версии.

## Выбор агента

Скилл автоматически определяет, какой AI-агент используется в проекте:
- **Kimi Code CLI** → `.kimi/skills/`
- **Claude Code** → `.claude/CLAUDE.md` + commands
- **Codex** → `.codex/instructions.md`
- **OpenCode** → `.opencode/`
- **Несколько** → универсальный `AGENTS.md` + специфичные конфиги

См. `docs/agents/` для подробностей по каждому агенту:
- `docs/agents/KIMI.md`
- `docs/agents/CLAUDE-CODE.md`
- `docs/agents/CODEX.md`
- `docs/agents/OPENCODE.md`

## Режимы

- `fast` — только критичное (1-2 дня)
- `standard` — все 5 слоёв пирамиды (1-2 недели)
- `paranoid` — всё + аудиты (1 месяц)
