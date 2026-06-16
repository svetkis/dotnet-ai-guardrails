# Конвенции проекта

> ⚠️ **ШАБЛОН** — Адаптируйте под свой стек. Замените `[ADAPT]` на ваши конвенции.

## Именование тестов

### Регрессионные тесты (обязательно)
```csharp
// Формат: BUG{номер}_{КраткоеОписание}
// [ADAPT] — замените [Test] на атрибут вашего фреймворка
[Test]
public async Task BUG055_NewMaster_ShouldGenerateSlots()
{
    // Arrange: воспроизводим баг
    // Act
    // Assert: баг не воспроизводится
}
```

### Ratchet-тесты
```csharp
// Формат: {Metric}_ShouldNotDecrease
[Test]
public void PublicTypeCount_ShouldNotDecrease()
{
    // Проверяем, что агент не удалил публичные типы при рефакторинге
}
```

### Архитектурные тесты
```csharp
// Формат: {RuleName}_Should{Satisfy}
[Test]
public void FindAsync_ShouldNotBeUsedInReadPath()
{
    // NetArchTest rule
}
```

> Справочные шаблоны в этом репозитории:
> - [`tests/conventions/BUG_TEMPLATE.cs`](../tests/conventions/BUG_TEMPLATE.cs) — формат regression-теста
> - [`tests/conventions/TUnit_Guide.md`](../tests/conventions/TUnit_Guide.md) — соглашения по TUnit

## Workflow после изменений

```
код → dotnet build → тесты ([ADAPT]: для TUnit используй dotnet run --project; для другого фреймворка зафиксируй команду явно) → docs → коммит
```

## Code Review агентом

Перед коммитом **обязательно** запустить отдельного агента для ревью:
- Проверка соответствия diff спекам
- Scope creep detection
- Regression risk assessment

## CI Guardrails

- `dotnet build` — должен падать при warning как error
- `[ADAPT]` — тесты должны реально бежать (не 0 ran)
- `[ADAPT]` — контрактные проверки (OpenAPI snapshot / DTO snapshot / etc.)
- `[ADAPT]` — нагрузочные метрики не должны деградировать (если применимо)
