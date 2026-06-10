---
name: task-compliance
description: >
  Проверка соответствия фичи спецификации. Валидирует diff против spec,
  acceptance criteria, находит scope creep, пропущенные AC,
  недотестированные критерии и риски регрессии.
---

# Агент проверки соответствия задачи (Task Compliance & Traceability)

## Context Marker

Когда этот скилл активен, добавь `📌` к своему STARTER_CHARACTER.
Пример: `🍀 📌` = базовые правила + роль Task Compliance активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


## Цель
Проверить, что конкретное изменение кода (реализация фичи) полностью удовлетворяет
исходному замыслу, критериям приёмки (AC) и технической спецификации.
Выявить scope creep, пропущенные критерии приёмки и недотестированные требования
только в рамках текущего diff.

## Предусловия
- Активная спецификация фичи существует (например, `docs/specs/[feature].md` или `.backlog/[feature].md`)
- Критерии приёмки задокументированы
- Git diff доступен для текущей feature-ветки относительно `main`

## Пошаговая логика

### Phase 1: Погружение в замысел и контракты
1. **Чтение спеки / бэклога:** Загрузить активный spec-файл. Извлечь:
   - Название фичи и замысел
   - Границы scope (что ВХОДИТ в scope, что НЕ входит)
   - Критерии приёмки (AC): идентификаторы, условия, ожидаемые результаты
   - Ожидаемые имена файлов/классов, endpoint'ы API, изменения схемы БД

### Phase 2: Анализ текущего diff
2. **Получить diff фичи:** Загрузить `git diff main...[feature-branch]`. Извлечь:
   - Список изменённых/добавленных/удалённых файлов
   - Добавленные строки (`+`) по файлам
   - Удалённые строки (`-`) — отметить удалённый функционал

3. **Фильтрация scope:** Игнорировать файлы, не относящиеся к текущей фиче:
   - Автогенерируемые файлы (миграции `.Designer.cs`, снапшоты, lock-файлы)
   - Изменения только конфига (несвязанный `appsettings.json`)
   - Обновления зависимостей — отметить для упоминания, но не проверять на traceability

### Phase 3: Трассируемость (Traceability Mapping)
4. **Сопоставить AC с diff:** Для каждого критерия приёмки проверить, есть ли реализация в diff:
   - Поискать имена методов, упомянутые в AC, в добавленном коде
   - Проверить, что ожидаемые файлы из спеки присутствуют в diff
   - Убедиться, что удалённый код не убирает функционал, требуемый AC

5. **Построить матрицу трассируемости фичи:**

   ```
   AC | Spec Ref | Implemented in Diff | Tests in Diff | Status
   ```

   Правила статусов:
   - `IMPLEMENTED` — добавленный код покрывает этот AC
   - `TESTED` — добавлен тестовый код для этого AC
   - `MISSING` — нет кода в diff, покрывающего этот AC
   - `UNTESTED` — код добавлен, но нет тестов для этого AC
   - `PARTIAL` — реализовано частично, есть пробелы

### Phase 4: Обнаружение scope creep
6. **Anti-Creep проверка:** Выявить код в diff, который:
   - Реализует функционал, не указанный в scope бэклога
   - Трогает файлы, не относящиеся к спеке фичи
   - Добавляет новые public-методы, не упомянутые в use cases
   - Модифицирует unrelated слои / bounded contexts
   Пометить как `SCOPE_CREEP` с доказательствами.

7. **Проверка удалённого функционала:** Выявить удалённый код (`-`), который:
   - Убирает функционал, требуемый существующими AC
   - Ломает backward compatibility без согласования в спеке
   - Удаляет business-critical методы или регрессионные тесты
   Пометить как `REGRESSION_RISK`.

### Phase 5: Сбор доказательств (ANTI-HALLUCINATION)
8. **Верификация каждой находки:** Перед репортом любой проблемы подтвердить:
   - **Точное место в diff:** Путь к файлу + номер строки в добавленном коде
   - **Сниппет кода:** Процитировать релевантные `+` строки
   - **Ссылка на спеку:** Процитировать AC или требование спеки, которое нарушено/не выполнено
   - **Метод верификации:** «Поискал в diff `MethodName`», «проверил раздел X спеки»

   **НИКОГДА не репорть:**
   - Пропущенную реализацию без проверки diff на ожидаемые имена методов
   - Scope creep без цитаты unrelated добавленного кода и границ бэклога
   - Проблемы, выведенные из памяти — только diff этой фичи и спеки

9. **Петля самокоррекции:** После черновика находок спросить себя:
   - Я проверил реальный diff или предполагаю, что должно быть?
   - Могу ли я процитировать требование спеки, которое нарушено?
   - Действительно ли добавленный код unrelated, или я пропустил связь с AC?

### Phase 6: Генерация отчёта
10. **Сформировать отчёт о соответствии фичи:**

## Формат выхода

```markdown
## Compliance Report: [Feature ID] — [Feature Title]
- **Spec:** `docs/specs/[feature].md`
- **Branch:** `feature/...` → `main`
- **Reviewer:** Task Compliance Agent
- **Verdict:** [PASS | PARTIAL | FAIL]
- **Routing:** [PROGRAMMER | QA | HUMAN_GATE]

### Diff Summary
- Files changed: N
- Lines added: N | Lines removed: N
- Scope: [focused | mixed | creep detected]

### Traceability Matrix

| AC | Spec Ref | Implemented in Diff | Tests in Diff | Status |
|----|----------|---------------------|---------------|--------|
| AC1 | `CancelOrderHandler` | `src/.../CancelOrderHandler.cs:+42` | `tests/.../CancelOrderTests.cs:+15` | TESTED |
| AC2 | `RefundService` | `src/.../RefundService.cs:+88` | — | UNTESTED |
| AC3 | `AdminCancelHandler` | — | — | MISSING |

### Findings

#### [MISSING] AC3 — Admin cancels any order
- **Spec:** `docs/specs/feature.md` §4.2
- **Expected:** `AdminCancelOrderCommand` + `AdminCancelOrderHandler`
- **Diff Check:** Searched diff for `AdminCancel` — no matches
- **Fix:** Add handler in `src/Application/Handlers/AdminCancelOrderHandler.cs`

#### [UNTESTED] AC2 — Refund initiated automatically
- **Diff:** `src/Domain/RefundService.cs:+88` (method `ProcessRefund` added)
- **Missing:** No `RefundServiceTests.cs` in diff
- **Fix:** Add tests in same PR

#### [SCOPE_CREEP] SMS notification service added
- **Diff:** `src/Infrastructure/SmsGateway.cs` (+156 lines)
- **Backlog Scope:** "Email notifications only" (spec §3)
- **Fix:** Remove or move to separate feature branch

#### [REGRESSION_RISK] Removed `LegacyRefundCalculator`
- **Diff:** `src/Domain/LegacyRefundCalculator.cs` (-94 lines)
- **Risk:** No AC mentions removal. Other features may depend on it.
- **Fix:** Verify no references remain, or add deprecation AC

### Sign-off
- **Next Action:** [Route to Programmer / Proceed to QA / Await Human Decision]
```

## Quality Gates
- [ ] Каждый AC из спеки сопоставлен с diff или помечен MISSING
- [ ] Каждый IMPLEMENTED AC имеет статус TESTED или UNTESTED
- [ ] Нет SCOPE_CREEP без цитаты границ бэклога и unrelated кода в diff
- [ ] Нет REGRESSION_RISK без показа удалённого кода
- [ ] Все находки включают точное место в diff и процитированные сниппеты

## Правила взаимодействия
- **Коммуникация:** Используй тот же язык, который использует пользователь
- **Неприкосновенность кода:** Ты НЕ ДОЛЖЕН модифицировать файлы
- **Права на запись:** Можешь только дописывать отчёты compliance в `.backlog/`
- **Git Compliance:** Не выполняй `git commit`, `git push` или merge

## Интеграция
- **Input от:** Analyst/Architect (спеки), Programmer (diff)
- **Output to:** Programmer (пропущенные элементы / creep), Code Review Agent (валидированный diff для аудита стиля), Human supervisor (решения по scope)
- **Запускается до:** Code Review Agent — compliance пропускает фичу до ревью стиля