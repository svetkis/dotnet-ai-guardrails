# Codex (OpenAI) — Guardrails Integration

> Codex CLI от OpenAI использует `.codex/instructions.md` для project instructions.
> Минималистичный подход: один файл инструкций + CLI-запросы.

## Структура интеграции

```
.codex/
└── instructions.md                # Единый файл инструкций

# Опционально:
codex.md                           # Альтернативное имя (в корне)
```

## Конфигурация проекта

### Создать `.codex/instructions.md`

```markdown
# Project Guardrails — {ProjectName}

## Ты — .NET-разработчик
Ты работаешь в проекте {ProjectName}. Следуй правилам ниже.

## Правила
- Не добавляй зависимости без explicit запроса
- Каждый баг-фикс — с regression тестом `BUG###_DescriptiveName`
- Все публичные async методы принимают `CancellationToken ct = default`
- Не используй `async void`
- Уважай nullable reference types

## Архитектура
- {Clean Architecture / Vertical Slice / etc.}
- Domain не зависит от Infrastructure
- API возвращает DTO, не Entity

## Stack
- .NET {version}
- {EF Core / Dapper / etc.}
- {TUnit / xUnit / NUnit}
- PostgreSQL / SQL Server

## Conventions
- Interfaces: `I{Name}`
- DTOs: record (immutable)
- Jobs: `{Name}Job`
- Tests: `[Test]` + `await Assert.That(value).IsEqualTo(expected)`

## Code Review Checklist
При любом изменении проверь:
- [ ] Нет SQL injection (параметризованные запросы)
- [ ] Нет утечки данных в логах/ответах
- [ ] Есть тесты на новый функционал
- [ ] CancellationToken прокидывается
```

## Запуск онбординга

```bash
# Установить Codex CLI (если ещё не установлен)
npm install -g @openai/codex

# Запустить в проекте
codex

# Внутри сессии:
> Просканируй этот .NET-проект. Оцени guardrails по 5 слоям.
> Выдай бэклог внедрения. Учитывай, что мы используем {стек}.
```

## Специфика Codex

### Что отличается от Kimi / Claude

| Аспект | Kimi | Claude Code | Codex |
|--------|------|-------------|-------|
| Формат правил | `.kimi/skills/*.md` | `.claude/CLAUDE.md` + commands | `.codex/instructions.md` |
| Количество файлов | Много (по файлу на скилл) | 1 + N commands | 1 файл |
| Запуск | `kimi run {name}` | `/{command}` | Прямой prompt |
| Интеграция | Ручная | Tools (bash, edit) | CLI-only |

### Нюансы Codex

1. **Один файл.** Вся конституция проекта — в одном `instructions.md`. Это значит:
   - Нельзя разбить на отдельные "аудиты" как скиллы
   - Нужно встраивать checklist'ы прямо в instructions
   - Можно использовать markdown-ссылки на внешние документы

2. **Нет custom commands.** Codex не поддерживает slash commands как Claude. Все инструкции — через prompt или `instructions.md`.

3. **Git-интеграция.** Codex умеет работать с git (видит diff, может коммитить). Это упрощает code review — агент сам может `git diff`.

4. **Прямой доступ к терминалу.** Codex может выполнять команды через `!bash` или прямо в CLI.

### Рекомендуемая структура instructions.md

```markdown
# {ProjectName} — Guardrails

## Роль
Ты — senior .NET-разработчик. Ты пишешь production-ready код.

## Правила (НЕ нарушать)
1. Не добавляй NuGet-пакеты без спроса
2. Не меняй структуру папок
3. Не удаляй тесты
4. Всегда прокидывай CancellationToken
5. Уважай nullable (string? vs string)

## Code Review Protocol
Перед тем как сказать "готово", проверь:
- [ ] Все новые публичные методы async с CancellationToken
- [ ] Нет SQL injection
- [ ] Нет утечки PII в логах/ответах
- [ ] Есть тесты
- [ ] Не нарушены архитектурные границы

## Stack Context
- .NET 10
- TUnit
- EF Core + PostgreSQL
- Minimal API
- Clean Architecture

## Conventions
- ...
```

## Онбординг для Codex

Поскольку Codex не имеет "скиллов", онбординг — это **генерация `instructions.md`** + **чеклистов**.

### Что делает агент при онбординге:

1. Сканирует проект (стек, архитектура, тесты)
2. Генерирует `.codex/instructions.md` с:
   - Ролью агента
   - Правилами (основанными на стеке)
   - Code Review Protocol
   - Stack Context
3. Генерирует `docs/guardrails-checklist.md` для human-разработчика
4. Генерирует `.github/workflows/codex-guard.yml` (CI pipeline)

### Пример CI для Codex-проекта

```yaml
# .github/workflows/codex-guard.yml
name: Codex Guardrails

on: [pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true

  tests:
    runs-on: ubuntu-latest
    needs: build
    steps:
      - uses: actions/checkout@v4
      - run: dotnet run --project tests/UnitTests/
      - run: ./ci/scripts/verify-tests.sh
```

## Ограничения

- Нет системы скиллов — только один файл инструкций
- Нет встроенных tools (read_file, edit_file) как у Claude Code
- Работает в режиме "prompt → response", агент не всегда видит всю кодбазу
- Нет marketplace или расширяемости
- Ограничен контекстом (зависит от модели, ~128k-200k tokens)
