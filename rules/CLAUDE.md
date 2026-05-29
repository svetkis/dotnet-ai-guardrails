# CLAUDE.md — Правила для .NET Agentic Engineering

> Этот файл — первый слой защиты. Агент читает его перед каждой задачей.

## Entity Framework

### Read-path (запрещено без исключений)
- ❌ EF-запросы без `.Select()` — **ЗАПРЕЩЕНЫ**
- ❌ `.Include()` в read-path — **ЗАПРЕЩЕНО**
- ❌ `.FindAsync()` в read-path — **ЗАПРЕЩЕНО**
- ✅ `.Select()` + `.AsNoTracking()` — **ОБЯЗАТЕЛЬНО**
- ✅ Вложенные коллекции в `.Select()` — выносим в отдельный batch-запрос

### Write-path
- ✅ Полная загрузка Entity допустима (change tracking нужен)
- ✅ `.FindAsync()` допустим только в write/командных сценариях
- ❌ `.AsNoTracking()` в write-path — **ЗАПРЕЩЕНО**

## Тесты

- **TUnit** — единственный фреймворк. НЕ xUnit, НЕ NUnit.
- Запуск: `dotnet run --project tests/ProjectName/ProjectName.csproj`
- ❌ `dotnet test` — **ЗАПРЕЩЕНО** (ломается на .NET 10 с TUnit/MTP)
- Каждый баг-фикс сопровождать тестом: `BUG###_DescriptiveName`
- Сначала failing test → потом фикс → тест зелёный

## PostgreSQL

- Колонки: `snake_case` (Fluent API `.HasColumnName`)
- Таблицы: `snake_case`, множественное число
- Индексы: `IX_table_column`

## API / DTO

- ❌ Изменение DTO без обновления `webapp/src/types/` — **ЗАПРЕЩЕНО**
- ❌ Изменение Response DTO без перегенерации OpenAPI snapshot — **ЗАПРЕЩЕНО**

## Даты

- Все даты в БД: **UTC**
- Backend: `DateTime.UtcNow`, `DateTime.SpecifyKind(..., DateTimeKind.Utc)`
- JSON: `"2025-02-27T10:00:00Z"` (с Z!)
- ❌ Конвертация UTC→Local на backend — **ЗАПРЕЩЕНА**

## Коммиты

- Conventional Commits: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `db`
- Обновление документации — в том же коммите, что и код

## Жёсткие запреты

- ❌ Коммит без `dotnet build` + тесты
- ❌ Новый env var без обновления `docs/DEPLOYMENT.md`
- ❌ Хардкод строк в UI — только i18n
- ❌ Raw SQL без комментария с объяснением
