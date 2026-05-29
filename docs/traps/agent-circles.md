# Ловушка: Агент ходит кругами (Agent Circles)

## Сценарий

Агент входит в цикл fix → fix → fix (или fix → revert), когда оптимизация или рефакторинг затрагивает слишком много подсистем. Агент видит локальную проблему, чинит её, но создаёт новую — и так по кругу. Иногда единственный выход — откат всего решения.

### Примеры из практики

**1. NoTracking default — 21 час в проде**
```
perf: QueryTrackingBehavior.NoTracking globally (p99: 2727→209ms, 13x!)
  → fix: AsTracking() в 5 write-методов (заплатка)
  → db: убрать NoTracking default, ручной AsNoTracking (правильный фикс)
```

**2. Tailwind 3→4 — 5 фиксов за 20 часов, потом полный revert**
```
upgrade Tailwind CSS 3→4
  → fix: outline-none, bg-opacity, ring-offset breaking
  → fix: CSS reset cascade layers
  → fix: Docker rolldown musl binding
  → REVERT: CSS reset fundamentally incompatible
```

**3. .Include() → .Select() — 11 файлов рефакторинга, 46 файлов починки**
```
refactor: replace Include/FirstOrDefaultAsync with Select projections (11 файлов)
  → fix: "Починили все прокидывания entity" (46 файлов!)
  → 4 доп. perf-коммита с доделками
```

## Почему агент входит в цикл

1. **Не видит blast radius** — оптимизирует сервис A, не зная что сервис B зависит от побочного эффекта A
2. **Тесты дают ложную уверенность** — InMemory DB, изолированные моки, нет cross-service тестов
3. **Чинит симптом, не причину** — `AsTracking()` в 5 методов вместо отката глобального NoTracking
4. **Визуальные баги невидимы** — `tsc --noEmit` проходит, но layout сломан
5. **Каждый фикс создаёт иллюзию прогресса** — "ещё один коммит и готово"

## Признаки входа в цикл

- Второй fix-коммит на ту же проблему
- Fix затрагивает файлы, которых не было в оригинальном изменении
- Commit message содержит "still", "another", "properly", "actually"

## Решение

### Когда прерывать

| Сигнал | Действие |
|--------|----------|
| 2-й fix-коммит на ту же проблему | Ревью blast radius |
| 3-й fix-коммит | Рассмотреть revert |
| Fix затрагивает 4x больше файлов чем оригинал | **Точно revert** |

### Профилактика

1. **E2E-тестирование после perf-коммитов** — ловит 80% циклов (stale cache, layout breaks)
2. **Интеграционные тесты вместо моков** — для cross-service взаимодействий
3. **Правило: после perf-коммита агента — ручной аудит write-paths**
4. **Ratchet-тесты** — не дают удалить критичные атрибуты при рефакторинге

## Паттерн

См. `tests/patterns/RatchetTest.cs` и `tests/patterns/ArchitectureRules.cs`
