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

### Complexity ratchet
```csharp
// Формат: {Metric}_ShouldNotIncrease
[Test]
public void SonarComplexityViolations_ShouldNotIncrease()
{
    // Бaseline + ratchet для S3776/S1541
}
```

### Allocation budget
```csharp
// Формат: {HotPathMethod}_AllocationBudget
[Test]
public void GetAvailableSlots_AllocationBudget()
{
    // GC.GetAllocatedBytesForCurrentThread vs baseline + 10%
}
```

### Spellcheck / Release readiness / Mutation / Analyzer tests
```csharp
// Формат: {Guard}_Should{Condition}
[Test]
public void CSpell_ShouldNotFindNewMisspellings() { }

[Test]
public void HealthEndpoint_ShouldBeHealthy() { }

[Test]
public void StrykerMutationScore_ShouldMeetBaseline() { }

[Test]
public void StrongTypedIdAnalyzer_FlagsPrimitiveIdInDomainEntity() { }
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
- `[ADAPT]` — complexity violations не должны расти (baseline + ratchet)
- `[ADAPT]` — allocation budget tests для `[HotPath]` методов
- `[ADAPT]` — spellcheck для markdown/public API (если применимо)
- `[ADAPT]` — mutation testing перед релизом (если применимо)
- `[ADAPT]` — analyzer tests для кастомных Roslyn-анализаторов (если применимо)

## Decision Guard IDs

- `PERF-###` — performance decisions
- `DB-###` — database/schema decisions
- `AUD-###` — audit decisions
- `COMPLEXITY-###` — complexity threshold deviations
- `SPELL-###` — intentional misspellings / non-dictionary terms
- `MUTATION-###` — mutation testing exceptions
