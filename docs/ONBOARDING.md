# Онбординг Skeptical AI Engineering

> Пошаговый гайд по внедрению guardrails в существующий .NET-проект.  
> **Аудитория:** Tech Lead, CTO, Lead Developer.  
> **Формат:** делай сам или делегируй агенту по этому документу.

---

## Сколько это займёт

| Режим | Время | Что внедряем | Когда выбирать |
|-------|-------|--------------|----------------|
| **Fast** | 1–2 дня | Слой 0 (AGENTS.md) + Слой 1 (компилятор) + Слой 2 (базовые арх-тесты) | Пилот. Хотим быстро проверить, работает ли методология. |
| **Standard** | 1–2 недели | Слои 0→5 + 2–3 аудита внешнего цикла | Основной сценарий. Большинство проектов начинают здесь. |
| **Paranoid** | 3–4 недели | Все слои + все аудиты + E2E MCP + груминг артефактов | Проект с высокими рисками (fintech, health, high-load). |

> **Не пытайся внедрить всё за один день.** Guardrails работают только если команда их понимает и поддерживает.

---

## Предварительные требования

- [ ] Доступ к `.sln` и всем `.csproj`
- [ ] Понимание текущей архитектуры (Clean / VSlice / Modular / Big Ball of Mud)
- [ ] Права на изменение CI/CD и добавление файлов в корень репозитория
- [ ] Решение, какой AI-агент используется (Kimi / Claude / Cursor / Codex / несколько)
- [ ] 30 минут на заполнение [`ARCHITECTURE-INVENTORY.md`](../skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md)

---

## Пошаговый план

### Шаг 0. Зафиксировать текущую архитектуру

**Цель:** Агент (и ты сам) должен понимать, с чем работает, а не угадывать.

1. Заполни [`ARCHITECTURE-INVENTORY.md`](../skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md):
   - Нарисуй C4 Container diagram (4–6 блоков)
   - Заполни таблицу Assembly Boundaries
   - Выдели 3–5 Critical Paths
   - Заполни Technology Inventory
2. Сохрани файл в `docs/ARCHITECTURE-INVENTORY.md` своего проекта.
3. Если есть осознанные отклонения от стандартов — зафиксируй их как `PERF-###` / `DB-###` / `ARCH-###` по шаблону [`NUMBERED-DECISIONS.md`](../skills/skeptical-ai-bootstrap/NUMBERED-DECISIONS.md).

**Выход:** ground truth для всех последующих guardrails.

---

### Шаг 1. Оценить зрелость

**Цель:** Понять, что уже есть, а что нужно создать с нуля.

**Вариант А — через агента (рекомендуется):**
1. Установи скилл `skeptical-ai-bootstrap` в свой проект ([`INSTALL.md`](../skills/skeptical-ai-bootstrap/INSTALL.md))
2. Запусти: `kimi run skeptical-ai-bootstrap` (или аналог для своего агента)
3. Получи отчёт `.backlog/onboarding-{дата}.md`

**Вариант Б — ручная оценка:**
1. Открой [`PYRAMID.md`](../PYRAMID.md)
2. Для каждого слоя (1→5) ответь:
   - Принцип соблюдён? (Да / Частично / Нет)
   - Что реализовано сейчас?
   - Что нужно добавить?
3. Запиши в `.backlog/onboarding-manual.md`

**Выход:** бэклог задач с приоритетами Must / Should / Could.

---

### Шаг 2. Адаптировать артефакты под стек

**Цель:** Вычеркнуть неприменимое ДО первого запуска.

1. Открой [`skills/ADAPTATION.md`](../skills/ADAPTATION.md)
2. Найди свой стек в таблице «Если в проекте… → пропусти…»
3. Для каждого скилла, который планируешь использовать:
   - Открой `CHECKLIST.md`
   - Пометь пункты `[-]` (N/A) или `[ ]` (проверим)
4. Если >50% скилла неприменимо — не адаптируй, создай новый ([`NEW-SKILL-TEMPLATE.md`](../skills/skeptical-ai-bootstrap/NEW-SKILL-TEMPLATE.md))

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

### Шаг 4. Внедрить Слой 1. Компилятор

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

### Шаг 5. Внедрить Слой 2. Архитектура

**Цель:** Автоматическая проверка слоёв и антипаттернов.

1. Установи `NetArchTest` в тестовый проект
2. Скопируй [`tests/patterns/ArchitectureRules.cs`](../tests/patterns/ArchitectureRules.cs)
3. Адаптируй namespace и имена сборок под свой проект (используй таблицу из Шага 0)
4. Добавь `RatchetTest.cs` — baseline публичных типов и тестов
5. Если нужно — добавь regex-сканирование исходников (см. [`architecture-tests.md`](solutions/architecture-tests.md))
6. Запусти: `dotnet run --project tests/YourProject.Tests` — тесты проходят?

**Критерий готовности:** Новый `using Infrastructure` в Application = красный CI.

---

### Шаг 6. Внедрить Слой 3. Тесты

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

### Шаг 7. Внедрить Слой 4. Code review агентом

**Цель:** Второй агент проверяет код первого.

1. Скопируй [`skills/code-review/`](../skills/code-review/) в `.kimi/skills/code-review/` (или формат своего агента)
2. Адаптируй `SKILL.md` под свой стек (см. Шаг 2)
3. Настрой запуск на каждый PR / перед коммитом:
   - Kimi: `kimi run code-review --git-diff HEAD~1`
   - Claude: `/{command}` в чате
4. Проверь на 3–5 последних PR — агент находит реальные проблемы?

**Критерий готовности:** Code review агент ловит минимум 1 проблему из 5 PR.

---

### Шаг 8. Внедрить Слой 5. E2E / MCP

**Цель:** Агент "протыкивает" приложение реальными руками.

1. Определи, какие MCP-тулы доступны (browser, Telegram, API, DB)
2. Напиши 3–5 E2E-сценария для критичных путей (см. Critical Paths из Шага 0)
3. Если есть frontend — добавь визуальные проверки (скриншоты)
4. Запускай в CI ночью или перед релизом

**Критерий готовности:** E2E находит проблему, которую не ловят юнит-тесты (например, stale cache).

---

### Шаг 9. Настроить Внешний цикл (аудиты)

**Цель:** Глубинные проверки по расписанию.

| Аудит | Частота | Когда начинать |
|-------|---------|----------------|
| Security audit | Раз в спринт | После внедрения Слоя 3 |
| DBA audit | Раз в спринт / при миграциях | Если используешь EF Core / Dapper |
| Performance audit | Перед релизом | После стабилизации архитектуры |
| Tech debt audit | Раз в спринт | Сразу — ловит дублирование и мёртвый код |
| Test audit | После 3–5 фич | Когда появляются новые фичи без тестов |

**Как внедрять:**
1. Скопируй `skills/{audit}/` в `.kimi/skills/{audit}/`
2. Адаптируй `CHECKLIST.md` (Шаг 2)
3. Запланируй в календаре команды (recurring meeting или CI-trigger)
4. Для ручного аудита — используй [`human-audit-bridge.md`](solutions/human-audit-bridge.md)

**Критерий готовности:** Каждый аудит проведён хотя бы 1 раз, результаты зафиксированы.

---

### Шаг 10. Настроить AI-агента

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
| **Big Bang** — внедрить все 5 слоёв за один спринт | Команда не усваивает, guardrails ломаются и отключаются | По одному слою за спринт, начиная с 1 |
| **Копипаста без адаптации** — скопировать все скиллы 1-к-1 | False positives заглушают команду, чеклисты игнорируются | Вычеркни N/A перед первым запуском |
| **Только агент, без human review** | Агенты галлюцинируют, пропускают контекст | Human-audit раз в спринт ([`human-audit-bridge.md`](solutions/human-audit-bridge.md)) |
| **AGENTS.md из другого проекта** | Правила про чужой стек вводят команду в заблуждение | Напиши свой, используя [`rules/AGENTS_TEMPLATE.md`](../rules/AGENTS_TEMPLATE.md) как шаблон |
| **Архитектурные тесты без инвентаря** | NetArchTest настроен на несуществующие сборки | Сначала Шаг 0 — зафиксируй границы |
| **"Мы ещё не готовы к guardrails"** | Guardrails нужны именно когда код пишет агент | Начни с Fast-режима (1–2 дня) |

---

## Проверочный чеклист: готов ли проект?

Пройди этот список после внедрения. Если всё отмечено — guardrails работают.

### Слой 0–2 (Must have)
- [ ] `AGENTS.md` в корне проекта, команда знает о его существовании
- [ ] `dotnet build` падает на warning'ах
- [ ] Архитектурные тесты проходят (NetArchTest или аналог)
- [ ] `verify-tests.sh` проверяет, что тесты реально запускались

### Слой 3–5 (Should have)
- [ ] Есть хотя бы 3 `BUG###_` regression-теста
- [ ] OpenAPI snapshot test (если есть API) или аналогичный контрактный тест
- [ ] Code review агентом проводился на последних 5 PR
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
A: Не слои, а конвенции: именование, запрещённые API-вызовы, ratchet на публичные типы. См. [`ADAPTATION.md`](../skills/ADAPTATION.md).

**Q: Команда сопротивляется — "это замедляет разработку".**  
A: Начни с Fast-режима (1–2 дня). Покажи, как `AGENTS.md` предотвращает переписывание кода агентом. Guardrails экономят время, а не тратят.

**Q: Можно ли внедрять без AI-агента (просто для команды)?**  
A: Можно, но 50% ценности — в защите ОТ агентов. Без агента это просто хорошие engineering practices.

**Q: Сколько стоит поддержка?**  
A: Слои 0–2 — "поставил и забыл" (минимальная поддержка). Аудиты — 1–2 часа раз в спринт. E2E — настройка 1 день, далее self-running.

---

## Навигация по онбордингу

| Застрял на шаге | Куда идти |
|-----------------|-----------|
| Не понимаю, какая у нас архитектура | [`ARCHITECTURE-INVENTORY.md`](../skills/skeptical-ai-bootstrap/ARCHITECTURE-INVENTORY.md) |
| Не знаю, какие скиллы выбрать | [`ADAPTATION.md`](../skills/ADAPTATION.md) |
| Не знаю, как настроить агента | [`docs/agents/`](agents/) → выбери своего |
| Не понимаю, как работает слой | [`PYRAMID.md`](../PYRAMID.md) |
| Хочу провести аудит руками | [`human-audit-bridge.md`](solutions/human-audit-bridge.md) |
| Агент не находит скиллы | [`INSTALL.md`](../skills/skeptical-ai-bootstrap/INSTALL.md) |
| Готовые артефакты не подходят | [`NEW-SKILL-TEMPLATE.md`](../skills/skeptical-ai-bootstrap/NEW-SKILL-TEMPLATE.md) + [`SKILL-ARCHITECTURE.md`](../skills/skeptical-ai-bootstrap/SKILL-ARCHITECTURE.md) |

---

> **Принцип:** SAE — не про идеальность. Это про то, чтобы агент не ломал код быстрее, чем команда успевает чинить. Начни с малого, добавляй слои по мере роста проекта.
