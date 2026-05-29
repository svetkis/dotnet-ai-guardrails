# Performance Audit — Чеклист

## Горячие пути
- [ ] Самые частые запросы идентифицированы
- [ ] Количество SQL-запросов per request подсчитано
- [ ] Индексы на WHERE-условиях проверены
- [ ] Проекции `.Select()` используются где уместно (не обязательны везде; проекции не требуют `.AsNoTracking()`)

## EF Core (если используется)
- [ ] Read-path без проекции → `.AsNoTracking()`. Проекции `.Select()` — исключение.
- [ ] Write-path → нет `.AsNoTracking()`. Исключение: raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, `ExecuteUpdateAsync`).

## N+1
- [ ] Проверены циклы с DB-запросами внутри
- [ ] Проверены Background jobs

## API
- [ ] Все endpoints с пагинацией
- [ ] Summary DTO вместо полных entities

## Кэш
- [ ] Ключи централизованы (CacheKeys.cs)
- [ ] Размер указан у каждой записи
- [ ] Нет конфликтов типов под одним ключом
- [ ] Инвалидация покрывает все write-paths

## Jobs
- [ ] Интервалы оптимальны
- [ ] Используются проекции + ExecuteUpdateAsync

## Метрики
- [ ] P50 / P99 / Max latency измерены
- [ ] Read + Write mix протестирован
- [ ] Concurrent load проверен
