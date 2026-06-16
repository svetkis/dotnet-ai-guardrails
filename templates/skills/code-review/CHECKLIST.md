# Code Review — Чеклист

## Перед началом
- [ ] Получен staged diff (`git diff --cached`)
- [ ] Известен контекст задачи (backlog item / spec)
- [ ] Скилл активирован автоматически перед `git commit` или явно через `/skill:code-review`

## Pre-commit / Триггер
- [ ] В staged-изменениях есть .NET backend файлы (*.cs, *.csproj, *.sln, *.props, *.targets)
- [ ] Frontend-only изменения пропущены (отдельный frontend-code-review скилл)
- [ ] При пустом staged diff агент не пишет находок и не блокирует коммит
- [ ] Агент НЕ вызывает `git commit` самостоятельно

## Security
- [ ] XSS: query params, returnUrl стрипаются
- [ ] Утечка ID: внутренние ClientId/OwnerId не возвращаются в API
- [ ] JWT не логируется
- [ ] Rate limiting на публичных endpoints
- [ ] Constant-time comparison для хешей

## EF Core
- [ ] Read-path без проекции: `.AsNoTracking()` присутствует. Проекции `.Select()` в DTO — не требуют.
- [ ] Write-path: нет `.AsNoTracking()`. Исключение: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).
- [ ] `.Include()` в QueryService обоснован: нет избыточных цепочек без проекции
- [ ] `.FindAsync()` в read-path разумно: допустим для чтения по PK, флаг если для списков/фильтров

## Архитектура
- [ ] Если проект использует Clean Architecture (есть проекты Domain / Infrastructure) — Domain не зависит от Infrastructure
- [ ] API возвращает DTO/records, не Entities
- [ ] CancellationToken принимается всеми async методами

## Тесты
- [ ] Каждый `fix:` коммит имеет `BUG*Tests.cs` или изменение существующего
- [ ] Новый функционал покрыт тестами
- [ ] Нет `Assert.That(true)` или пустых placeholder-тестов

## Качество кода
- [ ] Нет `async void`
- [ ] Нет пустых `catch { }`
- [ ] DateTime в UTC (`DateTime.UtcNow`, `DateTimeKind.Utc`)
- [ ] PostgreSQL колонки: `snake_case`

## Дублирование бизнес-логики (Semantic)
> Автотест ловит только буквальное копирование. Этот блок — для человека.
- [ ] Новая валидация/расчёт статуса — нет ли похожей логики в других сервисах?
- [ ] Правило можно вынести в Domain Service / Value Object / `BR-###`?
- [ ] Нет разъезда: в одном месте `>= 100`, в другом `> 100`

## Формат отчёта

```markdown
## Code Review — {дата}

### BLOCKER
- [ ] {описание} → {файл:строка}

### CRITICAL
- [ ] {описание} → {файл:строка}

### MAJOR
- [ ] {описание} → {файл:строка}

### Verdict
- [ ] APPROVED
- [ ] APPROVED_WITH_NITS
- [ ] CHANGES_REQUESTED
```
