# Ловушка: Молчаливая поломка (Silent Breakdown)

## Сценарий

Агент оптимизирует read-запросы ради перформанса. Добавляет `.AsNoTracking()` во все запросы подряд, не разбираясь в разнице между read-path и write-path.

```csharp
// Агент оптимизировал "список тикетов"
var tickets = await dbContext.Tickets
    .AsNoTracking()  // ✅ Тут ок — чистый read
    .ToListAsync();

// Но затем скопировал тот же паттерн в команду
var ticket = await dbContext.Tickets
    .AsNoTracking()  // ❌ АД! Change tracking отключён
    .FirstAsync(t => t.Id == id);

ticket.Resolve();     // Меняем статус
await dbContext.SaveChangesAsync();  // Молча не сохраняет! 0 rows affected
```

## Почему InMemory тесты проглатывают

InMemory провайдер EF Core **не эмулирует change tracking**. `SaveChanges()` всегда "успешен", даже с `AsNoTracking`.

## Последствия

- CI зелёный
- Юнит-тесты проходят
- На проде write ложится на 21 час
- Баг без эксепшена — самый дорогой

## Решение

1. **AGENTS.md** — явное правило: `AsNoTracking` только в read-path с `.Select()`
2. **NBomber** — гоняем read + write mix под нагрузкой. $Max$ write latency вырастает или появляются failed-запросы
3. **Интеграционные тесты** — только на реальной БД (TestContainers), никаких InMemory для логики

## Паттерн

См. `tests/patterns/LoadTest.cs`
