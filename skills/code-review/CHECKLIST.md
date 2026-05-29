# Code Review — Чеклист

## Перед началом
- [ ] Получен diff изменений (`git diff`)
- [ ] Известен контекст задачи (backlog item / spec)

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
