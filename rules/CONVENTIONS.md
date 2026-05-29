# Конвенции проекта

## Именование тестов

### Регрессионные тесты (обязательно)
```csharp
// Формат: BUG{номер}_{КраткоеОписание}
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
public void HotPathCount_ShouldNotDecrease()
{
    // Проверяем, что агент не снёс критичную логику
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

## Workflow после изменений

```
код → dotnet build → тесты (dotnet run --project) → docs → коммит
```

## Code Review агентом

Перед коммитом **обязательно** запустить отдельного агента для ревью:
- Проверка соответствия diff спекам
- Scope creep detection
- Regression risk assessment

## CI Guardrails

- `dotnet build` — должен падать при warning как error
- `dotnet run --project` — тесты должны реально бежать (не 0 ran)
- OpenAPI snapshot — должен совпадать
- NBomber — $Max$ не должен деградировать
