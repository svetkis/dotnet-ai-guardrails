# Онбординг Skeptical AI Engineering

> Пошаговый гайд по внедрению guardrails в существующий .NET-проект.  
> **Аудитория:** Tech Lead, CTO, Lead Developer.  
> **Формат:** делай сам или делегируй агенту по этому документу.
>
> **Модель контроля:** Engineering Assurance Levels — см. [README.md](../README.md#как-это-работает).
> Ниже используются legacy-имена слоёв (0, 1.1–2.3) из `PYRAMID.md` как ссылки на
> конкретные шаги; их соответствие уровням: Слой 0 → Control Foundation,
> 1.1/1.4 → Change Checks, 1.2/1.3 → Behavior Checks, 1.5/2.1/2.3 → System Checks,
> 2.2 → Periodic Assurance, внешний цикл → Engineering Governance,
> груминг артефактов → Control Maintenance.

---

## Сколько это займёт

| Режим | Время | Что внедряем | Когда выбирать |
|-------|-------|--------------|----------------|
| **Fast** | 1–2 дня | Слой 0 (AGENTS.md) + Слой 1.1 (компилятор) + Слой 1.2 (базовые арх-тесты) | Пилот. Хотим быстро проверить, работает ли методология. |
| **Standard** | 2–4 недели, инкрементально | Слой 0 → Change Checks → Behavior Checks; затем по одному подслою/аудиту за спринт | Основной сценарий. Большинство проектов начинают здесь. |
| **High-assurance** | 1–2 месяца, инкрементально | Все уровни + Engineering Governance + Control Maintenance | Проект с высокими рисками (fintech, health, high-load). |

> **Не пытайся внедрить всё за один день.** Guardrails работают только если команда их понимает и поддерживает. Сроки выше — календарные, внедрение всегда инкрементальное: один control за раз, с проверкой, что предыдущий работает.

---

## Предварительные требования

- [ ] Доступ к `.sln` и всем `.csproj`
- [ ] Понимание текущей архитектуры (Clean / VSlice / Modular / Big Ball of Mud)
- [ ] Права на изменение CI/CD и добавление файлов в корень репозитория
- [ ] Решение, какой AI-агент используется (Kimi / Claude / Cursor / Codex / несколько)
- [ ] 30 минут на заполнение [`ARCHITECTURE-INVENTORY.md`](../templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md)

---

## Пошаговый план

### Шаг 0. Зафиксировать текущую архитектуру

**Цель:** Агент (и ты сам) должен понимать, с чем работает, а не угадывать.

1. Заполни [`ARCHITECTURE-INVENTORY.md`](../templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md):
   - Нарисуй C4 Container diagram (4–6 блоков)
   - Заполни таблицу Assembly Boundaries
   - Выдели 3–5 Critical Paths
   - Заполни Technology Inventory
2. Сохрани файл в `docs/ARCHITECTURE-INVENTORY.md` своего проекта.
3. Если есть осознанные отклонения от стандартов — зафиксируй их как `PERF-###` / `DB-###` / `ARCH-###` по шаблону [`DECISION-GUARDS.md`](../templates/skills/skeptical-ai-bootstrap/DECISION-GUARDS.md).

**Выход:** ground truth для всех последующих guardrails.

---

### Шаг 1. Оценить зрелость

**Цель:** Понять, что уже есть, а что нужно создать с нуля.

**Вариант А — через агента (рекомендуется):**
1. Установи скилл `skeptical-ai-bootstrap` в свой проект ([`INSTALL.md`](../templates/skills/skeptical-ai-bootstrap/INSTALL.md))
2. Запусти: `kimi run skeptical-ai-bootstrap` (или аналог для своего агента)
3. Получи отчёт `.backlog/onboarding-{дата}.md`

**Вариант Б — ручная оценка:**
1. Открой [`PYRAMID.md`](../PYRAMID.md)
2. Для каждого подслоя (1.1→2.3) ответь:
   - Принцип соблюдён? (Да / Частично / Нет)
   - Что реализовано сейчас?
   - Что нужно добавить?
3. Запиши в `.backlog/onboarding-manual.md`

**Выход:** бэклог задач с приоритетами Must / Should / Could.

---

### Шаг 2. Адаптировать артефакты под стек

**Цель:** Вычеркнуть неприменимое ДО первого запуска.

1. Открой [`templates/skills/ADAPTATION.md`](../templates/skills/ADAPTATION.md)
2. Найди свой стек в таблице «Если в проекте… → пропусти…»
3. Для каждого скилла, который планируешь использовать:
   - Открой `CHECKLIST.md`
   - Пометь пункты `[-]` (N/A) или `[ ]` (проверим)
4. Если >50% скилла неприменимо — не адаптируй, создай новый ([`NEW-SKILL-TEMPLATE.md`](../templates/skills/skeptical-ai-bootstrap/NEW-SKILL-TEMPLATE.md))

**Выход:** адаптированные чеклисты, которые не генерируют false positives.

---

### Шаг 3. Написать Конституцию (Слой 0)

**Цель:** Агент читает правила ДО кода.

1. Скопируй [`rules/AGENTS_TEMPLATE.md`](../rules/AGENTS_TEMPLATE.md) в корень своего проекта
2. Отредактируй под свой стек:
   - Убери неприменимые правила (например, `[Authorize]` для Minimal API)
   - Добавь специфичные (например, «В нашем проекте `FindAsync()` разрешён только в `*CommandService.cs`»)
   - Зафиксируй naming conventions
3. Если проект большой — добавь `AGENTS.md` в подпапки (глубинные инструкции превалируют)
4. Добавь `rules/CONVENTIONS.md` — именование тестов, workflow, CI guardrails

**Выход:** `AGENTS.md` + `CONVENTIONS.md` в корне проекта, которые читает агент.

---

### Шаг 4. Внедрить Слой 1.1. Компилятор

**Цель:** Самый быстрый feedback loop — падает сборка.

1. В `Directory.Build.props` (или `.csproj` если single-project):
   ```xml
   <Nullable>enable</Nullable>
   <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
   ```
2. Добавь `.editorconfig` с severity=error для критичных правил
3. Для frontend (если есть): `tsc --noEmit` в strict mode + генерация типов из OpenAPI
4. Проверь: `dotnet build` падает на warning'ах?

**Критерий готовности:** `dotnet build` без warning'ов = зелёный CI.

---

### Шаг 5. Внедрить Слой 1.2. Архитектура

**Цель:** Автоматическая проверка слоёв и антипаттернов.

1. Установи `NetArchTest.eNhancedEdition` в тестовый проект (форк с исправленными багами и новыми фичами: Slices, Immutable-правила, проверка пути файла)
2. Скопируй [`tests/patterns/ArchitectureRules.cs`](../tests/patterns/ArchitectureRules.cs)
3. Адаптируй namespace и имена сборок под свой проект (используй таблицу из Шага 0)
4. Добавь `RatchetTest.cs` — baseline публичных типов и тестов
5. **Modular Monolith / Vertical Slice:** используй `Slice().ByNamespacePrefix(...).Should().NotHaveDependenciesBetweenSlices()` для проверки межмодульных зависимостей
6. Если правило смотрит на C#-исходники — предпочитай Roslyn analyzer (см. [`roslyn-analyzers.md`](solutions/roslyn-analyzers.md)); regex оставляй для config/markdown/manifests или временного spike
7. Запусти: `dotnet run --project tests/YourProject.Tests` — тесты проходят?
8. Посмотри живой failing demo: [`examples/DemoProject.Traps/`](../examples/DemoProject.Traps/) — 7 intentionally broken guardrails с `IType.Explanation` и ArchUnitNET

**Критерий готовности:** Новый `using Infrastructure` в Application = красный CI.

---

### Шаг 6. Внедрить Слой 1.3. Тесты

**Цель:** Каждое изменение покрыто тестами, и тесты реально запускаются.

1. **Проверка "0 tests ran":** скопируй [`ci/scripts/verify-tests.sh`](../ci/scripts/verify-tests.sh) в CI
2. **BUG-regression convention:** для каждого баг-фикса создавай `BUG###_DescriptiveName.cs` ([`BUG_TEMPLATE.cs`](../tests/conventions/BUG_TEMPLATE.cs))
3. **Snapshot test:** если есть API — добавь OpenAPI snapshot test ([`SnapshotTest.cs`](../tests/patterns/SnapshotTest.cs))
4. **Characterization tests:** зафиксируй поведение критичных алгоритмов (см. [`ai-patterns.md`](solutions/ai-patterns.md))
5. Если используешь TUnit — прочитай [`TUnit_Guide.md`](../tests/conventions/TUnit_Guide.md)

**Критерий готовности:**
- CI падает, если `0 tests ran`
- Каждый `fix:` коммит имеет `BUG*Tests.cs`
- Backend поменял DTO → snapshot test падает

---

### Шаг 7. Внедрить Слой 1.4. Code review агентом

**Цель:** Второй агент проверяет код первого.

1. Скопируй [`templates/skills/code-review/`](../templates/skills/code-review/) в `.kimi/skills/code-review/` (или формат своего агента)
2. Адаптируй `SKILL.md` под свой стек (см. Шаг 2)
3. Настрой запуск на каждый PR / перед коммитом:
   - Kimi: `kimi run code-review --git-diff HEAD~1`
   - Claude: `/{command}` в чате
4. Проверь на 3–5 последних PR — агент находит реальные проблемы?

**Критерий готовности:** Code review агент ловит минимум 1 проблему из 5 PR.

---

### Шаг 8. Внедрить Слой 1.5. Smoke тесты

**Цель:** Быстрая проверка, что критичные пути не сломаны.

1. Определи 10 критичных сценариев (см. Critical Paths из Шага 0)
2. Напиши автоматизированные smoke-тесты — минимум 1 на каждый путь
3. Запускай smoke перед каждым мержем или в CI на каждый PR

**Критерий готовности:** Smoke падает, если сломан критичный путь (авторизация, оплата, бронирование).

---

### Шаг 9. Внедрить Слой 2.1. E2E / MCP

**Цель:** Агент "протыкивает" приложение реальными руками.

1. Определи, какие MCP-тулы доступны (browser, Telegram, API, DB)
2. Напиши 3–5 E2E-сценария для критичных путей (см. Critical Paths из Шага 0)
3. Если есть frontend — добавь визуальные проверки (скриншоты)
4. Запускай в CI ночью или перед релизом

**Критерий готовности:** E2E находит проблему, которую не ловят юнит-тесты (например, stale cache).

---

### Шаг 10. Внедрить Слой 2.2. Аудиты

**Цель:** Глубинные проверки по расписанию.

| Аудит | Частота | Когда начинать |
|-------|---------|----------------|
| Security audit | Раз в спринт | После внедрения Слоя 1.3 |
| DBA audit | Раз в спринт / при миграциях | Если используешь EF Core / Dapper |
| Performance audit | Перед релизом | После стабилизации архитектуры |
| Complexity audit | Раз в спринт | Когда методы начинают разрастаться |
| Allocation budget audit | Перед релизом / при изменении hot paths | Если есть latency-sensitive пути |
| Spellcheck audit | Раз в спринт | Если есть публичные API / документация |
| Release readiness audit | Перед релизом / бета-запуском | Перед выходом в прод |
| Mutation audit | Раз в спринт | Если Stryker совместим с тестовым фреймворком |
| Analyzer tests audit | При создании / обновлении Roslyn-анализаторов | Если есть кастомные analyzers |
| Tech debt audit | Раз в спринт | Сразу — ловит дублирование и мёртвый код |
| Test audit | После 3–5 фич | Когда появляются новые фичи без тестов |

**Как внедрять:**
1. Скопируй `templates/skills/{audit}/` в `.kimi/skills/{audit}/`
2. Адаптируй `CHECKLIST.md` (Шаг 2)
3. Запланируй в календаре команды (recurring meeting или CI-trigger)
4. Для ручного аудита — используй [`human-audit-bridge.md`](solutions/human-audit-bridge.md)

**Критерий готовности:** Каждый аудит проведён хотя бы 1 раз, результаты зафиксированы.

---

### Шаг 11. Внедрить Слой 2.3. Нагрузка (NBomber)

**Цель:** Не дать агенту сломать production нагрузкой.

1. Установи `NBomber` в тестовый проект
2. Скопируй [`tests/patterns/LoadTest.cs`](../tests/patterns/LoadTest.cs)
3. Напиши сценарий: read + write mix для критичного пути
4. Запускай перед релизом или при подозрении на деградацию

**Критерий готовности:** NBomber показывает tail latency (Max, P95), а не только среднее.

---

### Шаг 12. Настроить AI-агента

**Цель:** Агент знает, как работать с твоим проектом.

1. Выбери своего агента:
   - **Kimi Code CLI** → [`docs/agents/KIMI.md`](agents/KIMI.md)
   - **Claude Code** → [`docs/agents/CLAUDE-CODE.md`](agents/CLAUDE-CODE.md)
   - **Cursor** → [`docs/agents/CURSOR.md`](agents/CURSOR.md)
   - **Codex** → [`docs/agents/CODEX.md`](agents/CODEX.md)
   - **OpenCode** → [`docs/agents/OPENCODE.md`](agents/OPENCODE.md)
2. Скопируй конфигурацию в свой проект
3. Убедись, что агент видит `AGENTS.md` и скиллы

**Критерий готовности:** Агент генерирует код, который проходит архитектурные тесты с первого раза.

---

## Антипаттерны внедрения (чего НЕ делать)

| Антипаттерн | Почему вредно | Что делать вместо |
|-------------|---------------|-------------------|
| **Big Bang** — внедрить все слои за один спринт | Команда не усваивает, guardrails ломаются и отключаются | По одному подслою за спринт, начиная с 1.1 |
| **Копипаста без адаптации** — скопировать все скиллы 1-к-1 | False positives заглушают команду, чеклисты игнорируются | Вычеркни N/A перед первым запуском |
| **Только агент, без human review** | Агенты галлюцинируют, пропускают контекст | Human-audit раз в спринт ([`human-audit-bridge.md`](solutions/human-audit-bridge.md)) |
| **AGENTS.md из другого проекта** | Правила про чужой стек вводят команду в заблуждение | Напиши свой, используя [`rules/AGENTS_TEMPLATE.md`](../rules/AGENTS_TEMPLATE.md) как шаблон |
| **Архитектурные тесты без инвентаря** | NetArchTest настроен на несуществующие сборки | Сначала Шаг 0 — зафиксируй границы |
| **"Мы ещё не готовы к guardrails"** | Guardrails нужны именно когда код пишет агент | Начни с Fast-режима (1–2 дня) |
| **Клонирование DemoProject** | Агент создаёт `examples/DemoProject/` в целевом репо, копируя структуру из `dotnet-ai-guardrails` | `examples/` — это демонстрация методологии, не шаблон для копирования. Не создавай демо-проекты в рабочем репо. |

---

## Проверочный чеклист: готов ли проект?

Пройди этот список после внедрения. Если всё отмечено — guardrails работают.

### Слой 0 + Слой 1.1–1.2 (Must have)
- [ ] `AGENTS.md` в корне проекта, команда знает о его существовании
- [ ] `dotnet build` падает на warning'ах
- [ ] Архитектурные тесты проходят (NetArchTest или аналог)
- [ ] `verify-tests.sh` проверяет, что тесты реально запускались

### Слой 1.3–1.5 + Слой 2.1 (Should have)
- [ ] Regression-тесты покрывают все воспроизводимые баг-фиксы (capability: ни один закрытый баг без `BUG###_` теста или обоснованной альтернативы)
- [ ] OpenAPI snapshot test (если есть API) или аналогичный контрактный тест
- [ ] Code review агентом — часть PR-процесса (встроен в workflow каждого PR, а не разовая акция)
- [ ] Smoke проходят на каждый PR
- [ ] E2E проходил хотя бы 1 раз и нашёл или подтвердил работу критичного пути

### Внешний цикл (Could have)
- [ ] Security audit проведён, находки зафиксированы в бэклоге
- [ ] DBA audit проведён, планы выполнения новых запросов проверены
- [ ] Tech debt audit проведён, семантическое дублирование зафиксировано

### Экосистема
- [ ] Агент настроен и видит `AGENTS.md`
- [ ] Скиллы лежат в `.kimi/skills/` (или аналогичной папке для другого агента)
- [ ] CI запускает архитектурные тесты + verify-tests на каждый PR

---

## FAQ

**Q: У нас .NET Framework 4.8. SAE применима?**  
A: Да, но адаптируй. Nullable включай файлом (`#nullable enable`), NetArchTest замени на Roslyn analyzers, E2E — на интеграционные тесты через `HttpClient`.

**Q: У нас Single-project MVP, нет Clean Architecture. Что проверять архитектурными тестами?**  
A: Не слои, а конвенции: именование, запрещённые API-вызовы, ratchet на публичные типы. См. [`ADAPTATION.md`](../templates/skills/ADAPTATION.md).

**Q: Команда сопротивляется — "это замедляет разработку".**  
A: Начни с Fast-режима (1–2 дня). Покажи, как `AGENTS.md` предотвращает переписывание кода агентом. Guardrails экономят время, а не тратят.

**Q: Можно ли внедрять без AI-агента (просто для команды)?**  
A: Можно, но 50% ценности — в защите ОТ агентов. Без агента это просто хорошие engineering practices.

**Q: Сколько стоит поддержка?**  
A: Слои 0 + 1.1–1.2 — "поставил и забыл" (минимальная поддержка). Аудиты — 1–2 часа раз в спринт. E2E — настройка 1 день, далее self-running.

---

## Навигация по онбордингу

| Застрял на шаге | Куда идти |
|-----------------|-----------|
| Не понимаю, какая у нас архитектура | [`ARCHITECTURE-INVENTORY.md`](../templates/skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) |
| Не знаю, какие скиллы выбрать | [`ADAPTATION.md`](../templates/skills/ADAPTATION.md) |
| Не знаю, как настроить агента | [`docs/agents/`](agents/) → выбери своего |
| Не понимаю, как работает слой | [`PYRAMID.md`](../PYRAMID.md) |
| Хочу провести аудит руками | [`human-audit-bridge.md`](solutions/human-audit-bridge.md) |
| Агент не находит скиллы | [`INSTALL.md`](../templates/skills/skeptical-ai-bootstrap/INSTALL.md) |
| Готовые артефакты не подходят | [`NEW-SKILL-TEMPLATE.md`](../templates/skills/skeptical-ai-bootstrap/NEW-SKILL-TEMPLATE.md) + [`SKILL-ARCHITECTURE.md`](../templates/skills/skeptical-ai-bootstrap/SKILL-ARCHITECTURE.md) |

---

> **Принцип:** SAE — не про идеальность. Это про то, чтобы агент не ломал код быстрее, чем команда успевает чинить. Начни с малого, добавляй слои по мере роста проекта.
