# Self-Validating Tests — план усиления Behavior Checks

> Дочерний workstream плана [`METHODOLOGY-REVISION-PLAN.md`](METHODOLOGY-REVISION-PLAN.md).
> **Статус:** частично выполнен (SV-001…SV-003 done; SV-004, SV-005 — pending).
> **Источник практики:** незавершённые изменения в `D:\Repos\Slotik` на 2026-07-14.
> Исходный код оттуда **не копировать** до стабилизации и фиксации source revision;
> в этом репозитории живут только принципы, правила и шаблоны.

## Зачем это нужно

Наличие теста и зелёный test run не доказывают, что тест способен обнаружить
регрессию. В `Slotik` найдены реальные классы ложной безопасности:

- test method без assertion;
- `IsNotNull()` как единственная проверка поведения;
- assertion внутри `if`, который может не выполниться;
- tautological assertion (`Assert.That(x).IsEqualTo(x)`);
- negative-only fixture без positive control;
- Playwright `expect(true)`, body-only checks и `waitForTimeout`;
- имя теста обещает postcondition, которого нет в assertions.

Этот workstream определяет **self-validating test** как тест, который не только
запускается и зеленеет, но и **гарантированно падает**, когда поведение,
обещанное его именем/сценарием, сломано.

## Цель

Сделать self-validation проверяемым свойством Behavior Checks:
правило в конституции агента, trap-документ, пункты в audit-чеклистах,
pattern-шаблон и воспроизводимая демонстрация (green/trap).

## Backlog

### SV-001 — Правило в конституции (Control Foundation) ✅ done

- Добавить в `rules/AGENTS_TEMPLATE.md` секцию Tests:
  - каждый тест self-validating: падает при поломке поведения из имени теста;
  - запрещены zero-assert, `IsNotNull()`-only, conditionally skipped,
    tautological и negative-only тесты без positive control — unless weaker
    check **is** the contract и причина задокументирована;
  - assertions проверяют observable postconditions, не факт выполнения;
  - для критичного поведения — fault sensitivity (mutation testing или
    deliberate fault injection).
- **Готово, когда:** правило есть в шаблоне + ссылка на trap-документ.

### SV-002 — Trap-документ ✅ done

- `docs/traps/non-validating-tests.md`: каталог форм невалидирующих тестов,
  почему они остаются зелёными, как ловить.
- **Готово, когда:** документ в knowledge map, cross-link из `false-safety.md`.

### SV-003 — Аудит-чеклисты (Periodic Assurance) ✅ done

- `templates/skills/test-audit/`: пункты на zero-assert / IsNotNull-only /
  conditional assert / tautology / negative-only.
- `templates/skills/mutation-audit/`: связать mutation score с fault sensitivity
  критичных тестов.
- **Готово, когда:** пункты есть, schema-lint проходит.

### SV-004 — Pattern + демонстрация (Behavior Checks) ⏳ pending

- `tests/patterns/` — шаблон fault-injection проверки (тест, который ломает
  прод-код локально и доказывает чувствительность набора).
- Green: `examples/DemoProject/`; Red: `examples/DemoProject.Traps/`
  (невалидирующий тест, который guardrail ловит).
- **Готово, когда:** CI зелёный на DemoProject, Traps падает по design.

### SV-005 — Frontend (опционально) ⏳ pending

- `templates/skills/frontend-code-review/`: пункты про `expect(true)`,
  body-only checks, `waitForTimeout` как замену condition-wait.
- **Готово, когда:** пункты есть, schema-lint проходит.

## Non-goals

- Не переписывать существующие тесты DemoProject под mutation coverage.
- Не копировать код, фикстуры или конфиги из `Slotik` — только принципы.
- Не вводить обязательный mutation testing на каждый PR (cost — release-only
  или risk-trigger).

## Связанные артефакты

- [`docs/traps/false-safety.md`](traps/false-safety.md) — зелёный CI ≠ рабочий код
- [`templates/skills/test-audit/`](../templates/skills/test-audit/)
- [`templates/skills/mutation-audit/`](../templates/skills/mutation-audit/)
- [`tests/conventions/`](../tests/conventions/)
