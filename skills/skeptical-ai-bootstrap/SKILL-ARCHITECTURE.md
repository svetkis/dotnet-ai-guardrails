# Архитектура экосистемы скиллов

## Context Marker

Когда этот скилл активен, добавь `🏗️` к своему STARTER_CHARACTER.
Пример: `🍀 🏗️` = базовые правила + роль Architecture Bootstrap активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


> Этот документ учит агента **проектировать guardrails**, а не копировать их.
> Когда готовые скиллы не подходят — агент сам решает: что проверять, как, и куда это класть.

---

## Принцип: Скилл = роль + контекст + правила + интеграция

Готовые скиллы из `dotnet-skeptical-ai` — это **примеры**. Они показывают:
- Как формулировать роль
- Как структурировать правила
- Как интегрировать с пайплайном

Но конкретный проект может требовать **уникальных ролей**. И агент должен уметь их придумывать.

---

## Дерево проектирования нового скилла

### Шаг 1: Что ловим? (Threat Model)

Не "какой скилл взять?", а "что агент может сломать в этом проекте?"

| Тип проекта | Что ломает агент? | Пример находки |
|-------------|-------------------|----------------|
| Worker + RabbitMQ | Потеря сообщений, повторная обработка, нет idempotency | Агент убрал `IDeliveryContext.Complete()` |
| Desktop (WPF) | Blocking UI thread, утечка подписок | Агент сделал `await _service.Get()` в UI thread |
| gRPC сервис | Несовместимость proto, breaking change | Агент переименовал поле в `.proto` без `reserved` |
| ML inference | Утечка памяти GPU, неправильный batch size | Агент убрал `using` для Tensor |
| Game server | Race condition в state, desync | Агент поменял lock на ConcurrentDictionary |

**Правило:** сначала threat model, потом скилл.

### Шаг 2: Кто ловит? (Роль)

Каждый скилл — это роль. Роль зависит от того, КОГДА проверяем:

| Когда проверяем | Роль | Примеры скиллов |
|-----------------|------|-----------------|
| На каждый PR | Code Review Agent | `code-review-dapper`, `code-review-grpc` |
| На каждый PR | Compliance Agent | `task-compliance`, `api-contract-compliance` |
| При сборке | Build-time Guard | `ArchitectureRules.cs` (custom), `proto-compatibility-check` |
| По расписанию | Auditor | `security-audit`, `dba-audit-mongo` |
| Перед релизом | Release Gate | `load-test-grpc`, `gpu-memory-audit` |
| По инциденту | Forensics Agent | `post-mortem-analyzer`, `bug-regression-guard` |

**Правило:** одна роль = один скилл. Не смешивай code review и аудит в одном файле.

### Шаг 3: Как ловим? (Механизм)

Механизм зависит от стека и скорости обратной связи:

| Механизм | Скорость | Когда использовать | Пример |
|----------|----------|-------------------|--------|
| Roslyn Analyzer | Мгновенно (в IDE) | Строгие правила, частые нарушения | `nullable`, `async void` |
| MSBuild Target | Сборка | Проверка артефактов, regex | `proto diff check`, `localization completeness` |
| Unit Test | Сборка + тесты | Логика, зависимости, контракты | `architecture rules`, `ratchet tests` |
| Script (bash/ps) | CI | Внешние тулы, парсинг | `verify-tests.sh`, `openapi diff` |
| AI Agent (скилл) | PR / ручной | Смысловой анализ, контекст | `code review`, `security audit` |
| E2E Test | CI / ночь | Интеграция, flow | `worker messaging flow`, `desktop UI flow` |

**Правило:** чем раньше ловим — тем лучше. Roslyn > Tests > AI Agent.

### Шаг 4: Куда класть? (Структура)

Все скиллы проекта живут в `.kimi/skills/`:

```
.kimi/
└── skills/
    ├── skeptical-ai-bootstrap/          # Этот скилл (установлен раз)
    │   └── SKILL.md
    │
    ├── code-review/                 # Внутренний цикл: каждый PR
    │   ├── SKILL.md
    │   └── CHECKLIST.md
    │
    ├── architecture-audit/          # Внутренний цикл: сборка
    │   └── SKILL.md
    │
    ├── security-audit/              # Внешний цикл: раз в спринт
    │   ├── SKILL.md
    │   └── CHECKLIST.md
    │
    └── {project-specific}/          # Уникальные для проекта
        ├── grpc-contract-guard/     # Проверка proto на breaking changes
        ├── gpu-memory-audit/        # ML-specific
        ├── game-state-sync-check/   # Game-specific
        └── worker-idempotency-guard/# Worker-specific
```

**Правило именования:** `{что-проверяем}-{контекст}` — `code-review-dapper`, `dba-audit-mongo`.

**Правило размещения:**
- Универсальные скиллы (security, perf) — корень `.kimi/skills/`
- Проектно-специфичные — подпапка `.kimi/skills/{domain}/`
- Генерируемые агентом — тот же уровень, что и ручные

### Шаг 5: Как интегрируем? (Пайплайн)

Скилл не работает изолированно. Он встраивается в пайплайн:

```
PR создан
    → Task Compliance Agent (scope check)
        → Code Review Agent (style + bugs)
            → Build (compilation + analyzers + unit tests)
                → Integration Tests (E2E for this service)
                    → Human Approval
                        → Merge
                            → Nightly: Security Audit
                            → Nightly: Performance Audit
                            → Before Release: Load Tests
```

Каждый скилл должен знать:
- **Input:** от кого получает контекст (diff, spec, metrics)
- **Output:** куда передаёт результаты (Programmer, QA, Human Gate)
- **Trigger:** что запускает (PR, schedule, manual, incident)
- **Gate:** блокирует ли merge (BLOCKER / WARNING / INFO)

---

## Паттерны генерации скиллов

### Паттерн A: "Спецификация стека"

Когда стек уникален, но принцип стандартный.

**Пример:** Проект на gRPC + .NET 8.
- Принцип: "Не ломать контракты без explicit versioning"
- Готовый скилл: нет (все про Web API / OpenAPI)
- Новый скилл: `grpc-contract-guard`
- Механизм: MSBuild target + unit test
- Что проверяет:
  - `reserved` в `.proto` при удалении полей
  - Нумерация полей не меняется тип
  - Новые поля — только `optional` или с `default`

#### NetArchTest для Vertical Slice (важно!)

NetArchTest **работает** с Vertical Slice — нужны только **custom rules про фичи**, а не про слои.

```csharp
// Проверка: фича Orders не зависит от Payments (кроме Shared)
// Повторить для каждой пары фич или параметризовать
[Test]
public void OrdersSlice_ShouldNotDependOn_PaymentsSlice()
{
    var result = Types.InCurrentDomain()
        .That().ResideInNamespace("Features.Orders")
        .Should().NotHaveDependencyOnAny("Features.Payments")
        .GetResult();

    Assert.That(result.IsSuccessful).IsTrue();
}

// Проверка: любой тип из одной фичи не зависит от другой фичи
[Test]
public void NoSlice_ShouldDependOn_AnotherSlice()
{
    var slices = new[] { "Features.Orders", "Features.Payments", "Features.Deliveries" };

    foreach (var slice in slices)
    {
        var otherSlices = slices.Where(s => s != slice);

        foreach (var other in otherSlices)
        {
            var result = Types.InCurrentDomain()
                .That().ResideInNamespace(slice)
                .Should().NotHaveDependencyOnAny(other)
                .GetResult();

            Assert.That(result.IsSuccessful).IsTrue(
                $"Slice {slice} depends on {other}");
        }
    }
}

// Проверка: внутренние типы фичи — internal, не public
[Test]
public void SliceImplementation_ShouldBeInternal()
{
    var result = Types.InCurrentDomain()
        .That().ResideInNamespaceMatching("^Features\\.([^.]+)\\.Internal")
        .Should().BeInternal()
        .GetResult();

    Assert.That(result.IsSuccessful).IsTrue();
}

// Проверка: каждая фича имеет Handler
[Test]
public void EachSlice_ShouldHaveHandler()
{
    var slices = new[] { "Features.Orders", "Features.Payments", "Features.Deliveries" };
    
    foreach (var slice in slices)
    {
        var hasHandler = Types.InCurrentDomain()
            .That().ResideInNamespace(slice)
            .And().HaveNameEndingWith("Handler")
            .GetTypes()
            .Any();

        Assert.That(hasHandler).IsTrue($"Slice {slice} missing Handler");
    }
}
```

**Ключевой инсайт:** NetArchTest проверяет **namespace'ы и типы**, не только сборки.
Regex НЕ НУЖЕН — используй `ResideInNamespace()`, `HaveNameEndingWith()`, `BeInternal()`.

### Паттерн B: "Доменная специфика"

Когда предметная область требует уникальных проверок.

**Пример:** Финтех-проект, расчёт комиссий.
- Принцип: "Комиссия не может измениться без explicit approval"
- Новый скилл: `fee-calculation-guard`
- Механизм: ratchet test + code review
- Что проверяет:
  - Diff в `FeeCalculator.cs` триггерит дополнительный review
  - Тест `FeeRatchet` проверяет, что baseline комиссий не уменьшился
  - Новые fee rules — только через `// DECISION: FEE###`

### Паттерн C: "Инфраструктурная специфика"

Когда инфраструктура проекта уникальна.

**Пример:** ML inference service, GPU memory.
- Принцип: "GPU memory не ликит между inference calls"
- Новый скилл: `gpu-memory-audit`
- Механизм: unit test + E2E benchmark
- Что проверяет:
  - Каждый inference method обёрнут в `using var tensor`
  - После 1000 calls GPU memory < baseline + 5%
  - Нет sync-over-async в GPU thread

### Паттерн D: "Организационная специфика"

Когда процессы компании требуют кастомных проверок.

**Пример:** Медицинский проект, HIPAA compliance.
- Принцип: "PHI (Protected Health Information) не попадает в логи"
- Новый скилл: `hipaa-phi-guard`
- Механизм: Roslyn analyzer + code review
- Что проверяет:
  - Любое поле с `[PHI]` не передаётся в `ILogger`
  - API ответы не содержат PHI без explicit `[Redact]`
  - Diff в `*Patient*.cs` триггерит дополнительный аудит

---

## Алгоритм для агента

Когда агент обнаруживает, что готовый скилл не подходит:

```
1. Определи ПРИНЦИП (что защищаем?)
2. Определи РОЛЬ (когда и кто проверяет?)
3. Выбери МЕХАНИЗМ (Roslyn / Test / Script / AI Agent)
4. Придумай ИМЯ ({что}-{контекст})
5. Определи МЕСТО (.kimi/skills/ или .kimi/skills/{domain}/)
6. Определи ИНТЕГРАЦИЮ (input / output / trigger / gate)
7. Сгенерируй файлы:
   - SKILL.md (роль + правила + формат отчёта)
   - CHECKLIST.md (если сложный процесс)
   - {механизм} (код теста / analyzer / script)
8. Добавь в onboarding report как "новый скилл"
```

---

## Примеры интеграции в CI

Скилл должен быть не просто markdown — он должен **работать**.

### Интеграция: Code Review скилл

```yaml
# .github/workflows/pr-guard.yml
jobs:
  code-review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run Code Review Agent
        run: |
          kimi run code-review --diff "$(git diff origin/main)"
        # Агент читает .kimi/skills/code-review/SKILL.md
        # и применяет правила к diff
```

### Интеграция: Architecture Guard (unit test)

```yaml
# .github/workflows/build.yml
jobs:
  architecture-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run Architecture Tests
        run: dotnet run --project tests/ArchitectureTests/
        # Тесты читают conventions из .kimi/skills/architecture-audit/
```

### Интеграция: Custom Audit (скрипт)

```yaml
# .github/workflows/nightly.yml
jobs:
  gpu-memory-audit:
    runs-on: [self-hosted, gpu]
    steps:
      - uses: actions/checkout@v4
      - name: Run GPU Memory Audit
        run: ./scripts/gpu-memory-check.sh
        # Скрипт реализует правила из .kimi/skills/gpu-memory-audit/SKILL.md
```

---

## Метаданные экосистемы

Агент должен поддерживать "карту скиллов" проекта:

```markdown
# .kimi/skills/README.md (генерируется онбордингом)

## Скиллы проекта {ProjectName}

### Внутренний цикл (Inner Loop)
| Скилл | Роль | Триггер | Gate | Статус |
|-------|------|---------|------|--------|
| code-review-dapper | Reviewer | PR | BLOCKER | ✅ Active |
| task-compliance | Scope Guard | PR | BLOCKER | ✅ Active |
| architecture-audit (VSlice) | Build Guard | NetArchTest custom | Build | BLOCKER | 🚧 WIP |

### Внешний цикл (Outer Loop)
| Скилл | Роль | Триггер | Gate | Статус |
|-------|------|---------|------|--------|
| security-audit | Security Auditor | Weekly | WARNING | ✅ Active |
| dba-audit-dapper | DBA Auditor | Sprint | BLOCKER | 🚧 WIP |
| gpu-memory-audit | Perf Auditor | Release | BLOCKER | 📋 Backlog |

### Легенда
- ✅ Active — работает в CI
- 🚧 WIP — скилл создан, но не в CI
- 📋 Backlog — только описание, нет кода
```

Эта карта помогает агенту понимать:
- Что уже есть
- Что в работе
- Что ещё не создано
- Какие gaps между слоями

---

## Антипаттерны проектирования скиллов

- ❌ **"God Skill"** — один скилл проверяет и код, и архитектуру, и безопасность. Разделяй.
- ❌ **"Read-only Skill"** — скилл только описывает, что проверять, но не интегрирован в CI. Каждый скилл должен иметь механизм запуска.
- ❌ **"Theory without Practice"** — скилл требует невозможного (например, NetArchTest для .NET Framework). Выбирай реалистичный механизм.
- ❌ **"Copy-paste without Context"** — скопировал `skills/security-audit/` но не адаптировал под ORM проекта. Скилл должен знать стек.
- ❌ **"Orphan Skill"** — скилл не знает, кто его запускает и куда передаёт результаты. Определи input/output/trigger.