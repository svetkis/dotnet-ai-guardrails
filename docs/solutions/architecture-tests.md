# Архитектурные тесты в .NET — ловля нарушений до review

> 25+ тестов в 4 файлах — ловят нарушения ДО code review.  
> **Failing demo:** [`examples/DemoProject.Traps/`](../../examples/DemoProject.Traps/) — специально сломанный код с 5 падающими тестами, демонстрирующий guardrails в действии.

## Инструменты

| Подход | Что проверяет | Библиотека |
|--------|--------------|------------|
| Roslyn-анализаторы | C# semantic rules: API-вызовы, типы, символы, атрибуты | Microsoft.CodeAnalysis |
| Рефлексия сборок | Зависимости, именование, наследование | NetArchTest |
| Сканирование артефактов | Markdown/config/manifests, где не нужен C# semantic model | Regex / structured parser |
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

## 3. Структурные правила: Roslyn-first

**Файл:** `ArchitectureTests.Structure.cs`

### EF-антипаттерны

Для C#-кода предпочтительный путь — Roslyn analyzer. Он проверяет syntax tree и semantic model, поэтому отличает реальный вызов от комментария, extension method от похожей строки, read-path от write-path по namespace/attribute/symbol.

```csharp
// Идея правила SAE006:
// InvocationExpression -> symbol.Name == "FindAsync"
// + containing namespace/type/attribute определяют read-path vs write-path.
```

| Тест | Правило |
|------|---------|
| Правило | Guardrail |
|---------|-----------|
| Запрет `FindAsync()` в read-path | Roslyn analyzer: реальный вызов метода, контекст read/write через namespace/attribute |
| Запрет `.Include()` в QueryService | Roslyn analyzer: invocation chain + тип query service |
| Исключения для write-path | Явный атрибут/namespace/decision ID вместо текстового whitelist |

### Безопасность кэша

| Тест | Правило |
|------|---------|
| `CacheSet_ShouldAlwaysSpecifySize` | Каждый `cache.Set()` должен указывать размер (SizeLimit на MemoryCache) |

### Отслеживание решений

> Registry template: [`DECISION-GUARDS.md`](../../templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md)

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

## 6. Иммутабельность доменных типов (eNhancedEdition)

**Файл:** `ArchitectureRules.cs`

| Тест | Правило |
|------|---------|
| `DomainTypes_ShouldBeImmutableExternally` | Типы в Domain с public access modifier не должны иметь mutable state (public fields/setters). Enum'ы исключаются. |

**Зачем:** Агент добавляет `public string Status { get; set; }` в value object, "потому что так удобнее обновлять". Это ломает инварианты Domain.

**Ограничение:** В eNhancedEdition 1.4.5 `BeImmutableExternally` ловит **public fields**, но auto-properties (`{ get; set; }`) могут не детектироваться. Для точной проверки mutable properties — используйте Roslyn analyzers.

**Рабочий пример:** `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs` (имя файла)  
**NOTE:** `HaveSourceFilePathMatchingNamespace` в eNhancedEdition 1.4.5 может работать нестабильно в зависимости от структуры проекта. При необходимости используйте `HaveSourceFileNameMatchingName`.

---

## 7. Конвенции файлов и namespace (eNhancedEdition)

**Файл:** `ArchitectureRules.cs`

| Тест | Правило |
|------|---------|
| `Types_ShouldHaveSourceFileNameMatchingName` | Имя файла `.cs` должно совпадать с именем типа (кроме nested types) |
| `Types_ShouldResideInMatchingFilePath` | Путь к файлу должен соответствовать namespace |

**Зачем:** Агент при рефакторинге переименовывает класс, но забывает переименовать файл. Поиск по имени файла ломается, namespace рассогласовывается с папкой.

**Рабочий пример:** `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs`

---

## 8. Slices — межмодульные зависимости (eNhancedEdition)

**Файл:** `ArchitectureTests.Slices.cs`

| Тест | Правило |
|------|---------|
| `Features_ShouldNotDependOn_EachOther` | Модули (slices) не должны напрямую зависеть друг от друга |

```csharp
var result = Types.InAssembly(typeof(Program).Assembly)
    .Slice()
    .ByNamespacePrefix("MyApp.Features")
    .Should()
    .NotHaveDependenciesBetweenSlices()
    .GetResult();
```

**Зачем:** В модульном монолите агент добавляет `using Features.Orders` в `Features.Payments` "ради одного DTO". Slice-тест ловит это мгновенно.

**Рабочий пример:** см. [`docs/ONBOARDING.md`](../../docs/ONBOARDING.md) §"Шаг 5. Slices"

---

## 9. Циклические зависимости между слайсами (ArchUnitNET)

**Файл:** `ArchUnitNetSliceTest.cs`

| Тест | Правило |
|------|---------|
| `Modules_ShouldBeFreeOfCycles` | Слайсы (модули/фичи) не должны иметь циклических зависимостей |

```csharp
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

private static readonly Architecture Architecture = new ArchLoader()
    .LoadAssemblies(typeof(Program).Assembly)
    .Build();

IArchRule rule = SliceRuleDefinition.Slices()
    .Matching("MyApp.Modules.(*)..")
    .Should()
    .BeFreeOfCycles();

rule.Check(Architecture);
```

**Зачем:** В модульном монолите агент добавляет интеграционное событие или вызов через Mediator, создавая неявный цикл `Orders → Payments → Shipping → Orders`. NetArchTest `NotHaveDependenciesBetweenSlices` запрещает **любые** зависимости между слайсами (zero-tolerance). ArchUnitNET позволяет иметь направленный ациклический граф (DAG), но ловит только циклы.

| Инструмент | Подход | Когда использовать |
|------------|--------|-------------------|
| NetArchTest.eNhancedEdition | `NotHaveDependenciesBetweenSlices` — zero-tolerance | Модули должны быть полностью изолированы; любой `using` в соседнюю фичу — ошибка |
| ArchUnitNET | `BeFreeOfCycles` — DAG validation | Модули могут зависеть друг от друга по иерархии, но не должно быть замкнутых циклов |

**Рабочий пример:** `examples/DemoProject.Traps/tests/DemoProject.Traps.Tests/ArchUnitNetSliceTest.cs`

---

## 10. IType.Explanation — диагностика падений (eNhancedEdition)

В отличие от оригинального NetArchTest 1.3.2, eNhancedEdition даёт **причину** падения для каждого типа:

```csharp
private static string FormatFailingTypes(NetArchTest.Rules.TestResult result)
{
    if (result.IsSuccessful)
        return string.Empty;

    var lines = result.FailingTypes
        .Select(t => $"- {t.FullName}: {t.Explanation}")
        .ToList();

    return "Failing types:\n" + string.Join("\n", lines);
}
```

Используйте в Assert:
```csharp
await Assert.That(result.IsSuccessful).IsTrue()
    .Because(FormatFailingTypes(result));
```

**Зачем:** Когда тест падает с "IsSuccessful = False", непонятно, кто виноват. С `IType.Explanation` видно: `MyService depends on Infrastructure`.

**Рабочий пример:** `examples/DemoProject/tests/DemoProject.Tests/ArchitectureRules.cs`

---

## 11. Roslyn-анализаторы как default для C#

**Файл:** `DemoProject.Analyzers/StronglyTypedIdAnalyzer.cs`

Для C# guardrails по исходникам default choice — Roslyn analyzer. Regex ищет строки, Roslyn понимает C#: syntax tree, semantic model, типы, symbols, attributes и реальные invocation'ы. Разница во времени обратной связи:

| Подход | Время обратной связи | Когда срабатывает |
|--------|---------------------|-------------------|
| Regex-сканирование `.cs` | ~10 секунд | `dotnet run --project` (Layer 1.2), только как временный fallback |
| Roslyn-анализатор | ~0.5 секунды | Ввод кода в IDE / `dotnet build` (Слой 1) |

**Пример:** `SAE001` ловит `public Guid Id { get; init; }` в Domain-сущностях ещё до компиляции — IDE показывает красное подчёркивание. `SAE002` ловит `void DoSomething(Guid orderId)` — сырой Guid в параметре.

**Когда использовать Roslyn:** если правило смотрит на C#-код и зависит от смысла языка: вызовы методов, типы, атрибуты, namespace, inheritance, generic constraints, nullable, allocation patterns.

**Когда НЕ Roslyn:** если проверяешь граф зависимостей между сборками — используй NetArchTest / ArchUnitNET. Если проверяешь markdown, `.csproj`, `.yml`, lock-файлы или уникальность Decision Guard ID — используй regex или structured parser.

**Рабочий пример:** `examples/DemoProject/src/DemoProject.Analyzers/`

**Пример 2 — Performance:** `SAE003` ловит `new` в `[HotPath]` методах, `SAE004` — `async` state machine, `SAE005` — boxing (explicit cast struct → interface/reference). Атрибут `[HotPath]` ставит разработчик осознанно; анализатор не даёт забыть про аллокации.

**Рабочий пример:** `examples/DemoProject/src/DemoProject.Analyzers/HotPathAnalyzer.cs`


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

// Для C# anti-pattern по исходникам сначала пишите Roslyn analyzer.
// Regex оставляйте для markdown/config/manifests или временного spike.
```

---

## Паттерн

См. `tests/patterns/ArchitectureRules.cs`
