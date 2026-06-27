# Установка скилла Skeptical AI Bootstrap

## Зачем

Этот скилл устанавливается в **твой** .NET-проект, чтобы Kimi Code CLI мог просканировать его и выдать бэклог внедрения guardrails.

## Быстрая установка

### 1. Скопируй скилл в свой проект

Из репозитория `dotnet-ai-guardrails` скопируй исполняемый скилл и supporting templates в свой проект:

```bash
# Находясь в корне ТВОЕГО .NET-проекта
mkdir -p ./.kimi/skills/skeptical-ai-bootstrap
cp /path/to/dotnet-ai-guardrails/.agents/skills/skeptical-ai-bootstrap/SKILL.md ./.kimi/skills/skeptical-ai-bootstrap/
cp -r /path/to/dotnet-ai-guardrails/templates/skills/skeptical-ai-bootstrap/* ./.kimi/skills/skeptical-ai-bootstrap/
```

Или вручную:
- Создать `.kimi/skills/skeptical-ai-bootstrap/` в своём проекте
- Скопировать `.agents/skills/skeptical-ai-bootstrap/SKILL.md`
- Скопировать нужные supporting templates из `templates/skills/skeptical-ai-bootstrap/`

**Важно:** выбери язык. В целевом проекте должен быть только один язык:
- **Русский** → скопируй `SKILL.md` (RU), не копируй `SKILL.en.md`
- **Английский** → используй английскую версию скилла, если она добавлена; supporting templates бери из `*.en.md` и переименовывай в базовые имена

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
Просканируй этот .NET-проект по методологии пирамиды из dotnet-ai-guardrails.
Выдай бэклог внедрения guardrails.
```

Агент сам найдёт `.csproj`, оценит слои и предложит план.

## После онбординга

Отчёт содержит ссылки на артефакты из `dotnet-ai-guardrails`:

| Артефакт | Откуда взять |
|----------|--------------|
| `rules/AGENTS_TEMPLATE.md` | `dotnet-ai-guardrails/rules/AGENTS_TEMPLATE.md` |
| `rules/CONVENTIONS.md` | `dotnet-ai-guardrails/rules/CONVENTIONS.md` |
| Архитектурные тесты | `dotnet-ai-guardrails/tests/patterns/ArchitectureRules.cs` |
| Ratchet тесты | `dotnet-ai-guardrails/tests/patterns/RatchetTest.cs` |
| CI воркфлоу | `dotnet-ai-guardrails/ci/github-actions/safe-ci.yml` |
| Code review скилл | `dotnet-ai-guardrails/templates/skills/code-review/` (RU или EN — один язык) |
| Аудиты | `dotnet-ai-guardrails/templates/skills/*-audit/` (RU или EN — один язык) |
| Груминг | `dotnet-ai-guardrails/templates/skills/memory-hygiene/`, `doc-hygiene/`, `backlog-hygiene/` (RU или EN — один язык) |

**Рекомендация:** форкни `dotnet-ai-guardrails` и указывай артефакты из своего форка — так контролируешь версии.

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

## Для агентов / For Agents

**Если ты AI-агент, выполняющий этот скилл:**

1. **НЕ создавай демо-проекты.** Не создавай `examples/`, `DemoProject/`, или новые `.csproj`/`.sln`.
2. **НЕ копируй структуру папок** этого репозитория (`rules/`, `templates/skills/`, `tests/patterns/`) в целевой проект.
3. **Твой выход — только markdown-файлы:** отчёты, чеклисты, `.backlog/*.md`, `AGENTS.md`, `CONVENTIONS.md`.
4. **Твоя задача:** читать целевой проект → оценивать → планировать. Не пиши код "для примера" или "чтобы показать".

## Режимы

- `fast` — только критичное (1-2 дня)
- `standard` — все слои пирамиды (1-2 недели)
- `paranoid` — всё + внешний цикл (1 месяц)
