# Паттерны AI-driven разработки

> Проверенные практики из проектов, где 100% кода пишет агент.

---

## 1. Namespace/Type inventory ratchet — архитектурный инвентарь

**Паттерн:** Рефлексией считаем публичные типы в Application-слое (или заданном неймспейсе). Baseline зафиксирован вручную.

- `PublicTypeCount_ShouldNotDecrease` — количество `public` типов в сборке >= baseline. Агент не может тихо удалить сервис или DTO.
- `TestCount_ShouldNotDecrease` — количество тестов >= baseline. Ловит "0 tests ran".

**Почему нестандартно:** Обычно инвентарь ведут в голове. Здесь это compile-time enforced контракт: нельзя удалить публичный тип без явного обновления baseline.

**Обобщение:** `PublicTypeCount` / `InterfaceCount` / `EndpointCount` + inventory test + coverage guard. Применимо к любому слою, где агент любит "почистить".

**Паттерн:** `tests/patterns/RatchetTest.cs`

---

## 2. Source-code scanning в архитектурных тестах — ловушки для AI

**Паттерн:** Тесты сканируют исходный код regex-ами:
- `FindAsync()` — запрещён в сервисах (whitelist из файлов с пояснениями)
- `.Include()` — запрещён в `*QueryService*.cs`
- `cache.Set()` без `SetSized()` — запрещён во всём `src/`

**Почему нестандартно:** Это не обычные архитектурные тесты через NetArchTest (проверка зависимостей). Это regex-сканирование исходников на уровне текста, потому что семантический анализ через reflection не может поймать паттерн `.Include()` vs `.Select()`. Whitelist с пояснениями — это документация решения прямо в коде теста.

**Обобщение:** Source-level anti-pattern scanning + explicit whitelist. Regex-тесты на запрещённые API вызовы в конкретных слоях.

**Паттерн:** `tests/patterns/ArchitectureRules.cs`

---

## 3. BUG-нумерация как regression test system

**Паттерн:** Каждый баг — отдельный файл с полным test fixture. Не просто один тест, а покрытие **всех code paths** где баг мог проявиться.

```
BUG055_NewMaster_ShouldGenerateSlots.cs
BUG074_RaceCondition_ConcurrentBooking.cs
BUG090_DateTimeSerialization_ShouldHaveUtcMarker.cs
BUG098_PhoneVsNameMerge.cs (8 тестов на все пути!)
```

**Правило AGENTS.md:** "каждый баг-фикс ОБЯЗАТЕЛЬНО с тестом". Это AI-directed regression: агент фиксит баг → сразу пишет failing test → фиксит код → тест зеленеет.

**Обобщение:** `BUG###_DescriptiveName` convention + full fixture per bug + multi-path coverage.

**Паттерн:** `tests/conventions/BUG_TEMPLATE.cs`

---

## 4. Numbered optimization decisions в комментариях

**Паттерн:** Осознанное отклонение от стандарта задокументировано с номером тикета прямо в коде:

```csharp
// PERF-022: QueryFilter removed — it added JOIN to users via Workspace.Owner.DeletedAt
// in every EXISTS subquery, causing slow queries under load.
builder.HasQueryFilter(s => !s.IsDeleted); // REMOVED — see PERF-022
```

Агент, видя что другие конфигурации имеют QueryFilter, попытается "исправить" это. Комментарий с `PERF-022` останавливает.

**Обобщение:** Numbered optimization decisions in code comments (`PERF-###`, `DB-###`). Предотвращает "undo" AI-агентом.

**Паттерн:** `tests/patterns/ArchitectureRules.cs` (тест на уникальность ID)

---

## 5. Serialization contract tests at API boundary

**Паттерн:** Интеграционный тест проверяет что API endpoint возвращает `DateTime` строки с суффиксом `Z`.

```csharp
// BUG-090: тест делает HTTP-запрос и парсит JSON как строку,
// ищет формат "2026-...T...Z"
```

**Почему нестандартно:** Это cross-layer contract test: .NET сериализация -> JSON -> JavaScript parsing. Симптом: "клиент видит 15:30, мастер видит 12:30" (разница = UTC offset).

**Обобщение:** Тесты на формат JSON, не на бизнес-логику. Ловят timezone-ошибки, которые unit-тесты не видят.

**Паттерн:** `tests/patterns/SnapshotTest.cs`

---

## 6. Shared cache contract tests

**Паттерн:** Два уровня защиты кеша:
1. Арх-тест проверяет что каждый `cache.Set()` использует `SetSized()`
2. BUG-тест проверяет что два сервиса, использующие один shared cache, не кладут разные типы под один ключ

**Почему нестандартно:** BUG-тест воспроизводит баг двумя вызовами в разном порядке: "ServiceA -> ServiceB" и "ServiceB -> ServiceA". Это тест на ordering-dependent failure в shared state.

**Обобщение:** Shared cache contract tests + mandatory wrapper methods enforced by source scanning.

**Паттерн:** `tests/patterns/ArchitectureRules.cs`

---

## 7. Hierarchical agent instructions

**Паттерн:** Каждый слой проекта имеет свой `AGENTS.md` с контекстно-специфичными правилами:
- Корневой — workflow, deployment, коммиты
- Domain — "нулевые зависимости, миграция обязательна"
- Infrastructure — "Select() обязателен в read-path"
- Api — "static endpoint classes"
- Tests — "dotnet run, не dotnet test"

**Почему нестандартно:** Это фактически system prompt распределённый по файловой системе. Когда агент работает в `src/Infrastructure/`, он автоматически получает правила этого слоя.

**Обобщение:** Hierarchical agent instructions co-located with code layers. Один `AGENTS.md` быстро раздувается и теряет релевантность.

---

## 8. Concurrency integration test with real database

**Паттерн:** Создаёт два `IServiceScope`, получает два экземпляра сервиса, и запускает `Task.WhenAll` на одну и ту же операцию. Проверяет что оба вызова вернули один и тот же результат (не создали дубли).

**Почему нестандартно:** Race condition тесты редко пишут для CRUD-операций. Здесь это реальный интеграционный тест с PostgreSQL через Testcontainers. Два scope = два DbContext = реальная конкуренция.

**Обобщение:** Concurrency integration test with real database + scoped DI.

---

## 9. Attribute-driven PII redaction — compile-time + runtime

**Паттерн:** Custom `[SensitiveData]` attribute на свойствах + три уровня enforcement:

1. **Compile-time:** Roslyn analyzer запрещает `Log*($"...{email}...")` — interpolated strings в логировании
2. **Inventory test:** Все свойства с `*Email*`, `*Phone*`, `*Password*` обязаны иметь `[SensitiveData]`
3. **Runtime:** Serilog destructuring policy автоматически redact'ит помеченные поля

```csharp
// Агент добавил DTO:
public record UserDto(
    [SensitiveData] string Email,  // ✅ Ratchet: требует атрибут
    [SensitiveData] string Phone,
    string Name                    // Не sensitive — без атрибута
);

// Агент хочет залогировать:
_logger.LogInformation("User {Email} logged in", user.Email);
// ❌ PiiGuardTest падает: параметр типа с [SensitiveData] передан в Log*

// Правильно — через structured logging с redaction:
_logger.LogInformation("User {UserId} logged in", user.Id);
```

**Почему нестандартно:** Обычно PII защищают review'ом и политиками. Здесь это compile-time + inventory + runtime: нельзя добавить Email в DTO без `[SensitiveData]`, и нельзя передать такой тип в `Log*`.

**Обобщение:** `[SensitiveData]` + compile-time analyzer + inventory ratchet + runtime destructuring. Применимо к любым sensitive данным: credentials, tokens, health data.

**Паттерн:** `tests/patterns/PiiGuardTest.cs`, `docs/traps/log-leak.md`

---

## 10. Context Markers — визуальные маркеры активного контекста

**Паттерн:** Использовать эмодзи в начале каждого ответа агента для сигнализации активного контекста:

- 🍀 = базовые правила загружены
- 🔴/🌱/🌀 = текущая фаза TDD (red/green/refactor)
- ✅ = активна роль committer
- 🔍 = активна роль code reviewer
- ❗️ = агент флагает ошибку
- ♻️ = правила только что перечитаны
- ✨📂 = создание нового репозитория

**Stacking:** Маркеры складываются: `🍀 ✅` = базовые правила + роль committer. Это позволяет одним взглядом понять, какой контекст «в голове» у агента.

**Impromptu markers:** Когда даёшь критически важную инструкцию посреди разговора — попроси отвечать с дополнительным эмодзи. Например, после инструкции «всё DateTime — UTC» попросить добавить 🕒. Это делает невидимое состояние видимым.

**Почему нестандартно:** Обычно контекст агента невидим. Невозможно понять, прочитал ли агент ground rules, помнит ли он про TDD-цикл, или потерял контекст после длинной сессии. Маркеры превращают невидимое состояние в явный сигнал.

**Обобщение:** Любой проект с несколькими ролями агента (committer, reviewer, TDD) или длинными сессиями. Применимо к любому AI-агенту, поддерживающему system prompt.

**Паттерн:** `rules/AGENTS_TEMPLATE.md` §Context Markers

---

## Сводная таблица паттернов

| Паттерн | Суть | Где смотреть |
|---------|------|-------------|
| **Namespace/Type inventory ratchet** | Считаем публичные типы в слое + тесты | `tests/patterns/RatchetTest.cs` |
| **Source-level anti-pattern gates** | Regex-сканирование кода в CI, не reflection | `tests/patterns/ArchitectureRules.cs` |
| **Bug-as-fixture** | Один файл = один баг = все code paths | `tests/conventions/BUG_TEMPLATE.cs` |
| **Numbered optimization decisions** | `PERF-022`, `DB-013` в комментариях кода | [`skills/skeptical-ai-bootstrap/DECISION-GUARDS.md`](../../skills/skeptical-ai-bootstrap/DECISION-GUARDS.md) |
| **Serialization contract tests** | Тесты на формат JSON, не на бизнес-логику | `tests/patterns/SnapshotTest.cs` |
| **Shared state contract tests** | Тест на ordering-dependent failures в shared cache | `tests/patterns/ArchitectureRules.cs` |
| **Hierarchical agent instructions** | AGENTS.md per directory, не один на проект | `rules/AGENTS_TEMPLATE.md` |
| **Concurrency with real DB** | Race condition тесты на Testcontainers | `tests/patterns/` |
| **PII redaction guard** | `[SensitiveData]` + compile-time + runtime redaction | `tests/patterns/PiiGuardTest.cs` |
| **Context Markers** | Визуальные маркеры активного контекста в ответах агента | `rules/AGENTS_TEMPLATE.md` |
