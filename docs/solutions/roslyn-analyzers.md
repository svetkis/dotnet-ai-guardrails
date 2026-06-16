# Roslyn-анализаторы как compile-time guardrails

> Regex ищет строки. Roslyn понимает C#.
> Для C# guardrails по исходникам default choice — Roslyn analyzer.

## Когда выбирать Roslyn

Используй анализатор, если правило зависит от смысла C#:

| Правило | Почему Roslyn |
|---------|---------------|
| Запрет raw `Guid Id` в Domain | Нужно понимать тип свойства и namespace |
| Запрет `FindAsync()` в read-path | Нужно отличать реальный invocation от комментария и write-path |
| Запрет allocations в `[HotPath]` | Нужно видеть attribute, `new`, array creation, boxing, `async` |
| DTO наружу не содержит internal entity | Нужно анализировать symbols и return types |
| Метод обязан принимать `CancellationToken` | Нужно смотреть signature и async usage |

## Когда НЕ Roslyn

| Проверка | Инструмент |
|----------|------------|
| Зависимости между сборками / слоями | NetArchTest |
| Циклы между slices/modules | ArchUnitNET |
| Уникальность `PERF-###`, `DB-###`, `AUD-###` в markdown/config/code comments | Regex или parser |
| `.csproj`, `.yml`, `package.json`, lock-файлы | Structured parser или regex |

## Рабочий пример

`examples/DemoProject/src/DemoProject.Analyzers/`:

- `StronglyTypedIdAnalyzer.cs`
  - `SAE001`: primitive `*Id` property в Domain
  - `SAE002`: raw `Guid *Id` parameter в Domain
- `HotPathAnalyzer.cs`
  - `SAE003`: allocation в `[HotPath]`
  - `SAE004`: `async` state machine в `[HotPath]`
  - `SAE005`: boxing в `[HotPath]`

Подключение к проекту:

```xml
<ProjectReference Include="..\DemoProject.Analyzers\DemoProject.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## Минимальный процесс для агента

1. Сформулируй bug class: какой конкретный агентский баг ловим.
2. Напиши 2-3 маленьких примера кода: должен сработать / не должен сработать.
3. Создай `DiagnosticDescriptor` с ID `SAE###`.
4. Зарегистрируй syntax или operation action.
5. Используй `SemanticModel`, когда правило зависит от типа или symbol.
6. Подключи analyzer как `OutputItemType="Analyzer"`.
7. Зафиксируй severity: error для safety/correctness, warning для performance guidance.

## Правило репозитория

Regex по `.cs` — только временный spike или fallback. Если C#-правило стабилизировалось и должно защищать команду постоянно, перенеси его в Roslyn analyzer.
