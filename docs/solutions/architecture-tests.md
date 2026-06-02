# Архитектурные тесты в .NET — ловля нарушений до review

> 25+ тестов в 4 файлах — ловят нарушения ДО code review.

## Инструменты

| Подход | Что проверяет | Библиотека |
|--------|--------------|------------|
| Рефлексия сборок | Зависимости, именование, наследование | NetArchTest |
| Сканирование исходников | Антипаттерны в коде (regex по .cs файлам) | Свой хелпер |
| Подсчёт типов | Количество публичных типов и тестов не уменьшается | Рефлексия |

---

## 1. Слоёная архитектура (Clean Architecture)

**Файл:** `ArchitectureTests.cs`

| Тест | Правило |
|------|---------|
| `Domain_ShouldNotDependOn_Application` | Domain не знает об Application |
| `Domain_ShouldNotDependOn_Infrastructure` | Domain не знает об Infrastructure |
| `Application_ShouldNotDependOn_Infrastructure` | Application не зависит от Infrastructure |
| `Api_ShouldNotDependOn_Infrastructure` | Api зависит только через DI |

**Зачем:** Один `using MyApp.Infrastructure` в Application — и зависимость протекает. Тест ломается мгновенно.

---

## 2. Конвенции именования

**Файл:** `ArchitectureTests.Naming.cs`

| Тест | Правило |
|------|---------|
| `Interfaces_ShouldStartWith_I` | Интерфейсы в Domain и Application начинаются с `I` |
| `Jobs_ShouldEndWith_Job` | Фоновые задачи заканчиваются на `Job` |
| `Dtos_ShouldBeRecords` | DTO — только record (иммутабельность) |

**Зачем:** Единообразие = поиск по кодбазе. `grep *Job` находит все джобы.

---

## 3. Структурные правила + Regex-сканирование

**Файл:** `ArchitectureTests.Structure.cs`

### EF-антипаттерны (сканирование исходников)

Не только рефлексия сборок, но и **regex по .cs файлам**:

```csharp
// Ищем FindAsync() в сервисах (запрещено в read-path)
var violations = ScanServicesForPattern(@"\.FindAsync\(", "*.cs", whitelist);
```

| Тест | Правило |
|------|---------|
| `Services_ShouldNotUse_FindAsync` | Запрет `FindAsync()` — грузит полную Entity. Whitelist для write-path |
| `QueryServices_ShouldNotUse_Include` | Запрет `.Include()` в QueryService — только `.Select()` проекции |
| `FindAsync_Whitelist_ShouldNotBeStale` | Whitelist не протухает — если файл больше не использует FindAsync, тест падает |

### Безопасность кэша

| Тест | Правило |
|------|---------|
| `CacheSet_ShouldAlwaysSpecifySize` | Каждый `cache.Set()` должен указывать размер (SizeLimit на MemoryCache) |

### Отслеживание решений

> Registry template: [`NUMBERED-DECISIONS.md`](../../skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md)

| Тест | Правило |
|------|---------|
| `PerfAndDbDecisions_ShouldHaveUniqueIds` | ID оптимизаций (`PERF-###`, `DB-###`) уникальны по всей кодбазе |

---

## 4. Контроль архитектурного инвентаря

**Файл:** `ArchitectureTests.Ratchet.cs`

| Тест | Правило |
|------|---------|
| `PublicTypeCount_ShouldNotDecrease` | Количество публичных типов в Application >= baseline. Нельзя тихо удалить сервис или DTO |
| `TestCount_ShouldNotDecrease` | Количество тестов >= baseline. Ловит "0 tests ran" |

**Зачем:** Агент любит "почистить код" и удалить "неиспользуемые" сервисы. Рефлексия по неймспейсу ловит это мгновенно.

---

## 5. Строгая типизация идентификаторов

**Файл:** `StronglyTypedIds.cs`

| Тест | Правило |
|------|---------|
| `DomainEntities_ShouldNotUseRawPrimitivesForIds` | Свойства `*Id` в Domain-сущностях не могут иметь тип `Guid`, `string`, `int`, `long`. Допустимы только типы, оканчивающиеся на `Id` (например, `BookingId`) |
| `StronglyTypedIdUsage_ShouldNotDecrease` | Ratchet: количество strongly typed ID в Domain >= baseline |

**Зачем:** Агент по привычке использует `Guid` для всех идентификаторов. Это открывает дверь для подстановки `ClientId` в метод, ожидающий `AgentId`. Архитектурный тест заставляет создавать отдельный тип для каждой сущности — компилятор делает остальное.

**Юзкейс для доклада:**
- **Слой 1 (Компилятор):** показываем "магию" — IDE подчёркивает красным `GetAgent(clientId)`, потому что тип `ClientId` не приводится к `AgentId`.
- **Слой 2 (Архитектурные тесты):** показываем "полицию" — упавший пайплайн с ошибкой `DomainEntities_ShouldNotUseRawPrimitivesForIds` заставляет разработчика (и агента) создать `BookingId` вместо `Guid`.

**Шаблон:** [tests/patterns/StronglyTypedIds.cs](../../tests/patterns/StronglyTypedIds.cs)  
**Рабочий пример:** `examples/DemoProject/tests/DemoProject.Tests/StronglyTypedIds.cs`

---

## Whitelist со staleness check

Whitelist для исключений (write-path) сам проверяется: если файл из whitelist больше не содержит паттерн — тест падает. Агент не может "почистить" код и оставить мёртвую запись.

```csharp
var whitelist = new[]
{
    "BookingCommandService.cs:2 calls: write-path uses FindAsync for update",
    "MasterProfileService.cs:1 call: write-path entity load"
};
```

---

## Как добавить новый тест (шаблон)

```csharp
// Рефлексия: проверка типов
[Test]
public async Task MyRule_ShouldBeEnforced()
{
    var result = Types.InAssembly(DomainAssembly)
        .That().ResideInNamespace("MyApp.Domain")
        .Should().BeSealed()
        .GetResult();

    await Assert.That(result.IsSuccessful).IsTrue();
}

// Сканирование: поиск антипаттерна в коде
[Test]
public async Task Services_ShouldNotUse_BadPattern()
{
    var violations = ScanServicesForPattern(
        @"\.BadMethod\(",
        "*.cs",
        whitelist: new[] { "LegitimateUse.cs:2" });

    await Assert.That(violations)
        .IsEmpty()
        .Because("BadMethod загружает слишком много данных");
}
```

---

## Паттерн

См. `tests/patterns/ArchitectureRules.cs`
