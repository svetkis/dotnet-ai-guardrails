# План развития методологии Skeptical AI Engineering

> Рабочий документ: целевая модель, решения по неймингу и приоритизированный backlog.
> Это план изменения методологии, а не описание её текущего состояния.

## Цель

Превратить набор guardrails, test patterns и audit-skills в последовательную
инженерную методологию:

- с одной моделью уровней контроля;
- с общеупотребимой инженерной терминологией;
- с проверяемыми и ограниченными по области действия утверждениями;
- с единым контрактом skills и findings;
- с воспроизводимыми примерами и зелёным CI;
- с нейтральным английским без рекламной категоричности и prompt-ритуалов.

## Целевая модель: Engineering Assurance Levels

| Уровень | English name | Когда срабатывает | Что входит | Главный вопрос |
|---------|--------------|-------------------|------------|---------------|
| Основание | **Control Foundation** | До изменения кода | `AGENTS.md`, architecture boundaries, decision records, policies | Какие ограничения и решения уже приняты? |
| 1 | **Change Checks** | IDE, build, pre-commit | Compiler, nullable, analyzers, formatting, banned APIs, dependency checks | Может ли изменение технически существовать? |
| 2 | **Behavior Checks** | Локальный или CI test run | Unit, regression, contract, characterization, architecture tests, ratchets | Сохранились ли ожидаемые свойства и поведение? |
| 3 | **System Checks** | PR, CI, release pipeline | Integration, E2E, smoke, Testcontainers, load, deployment verification | Работает ли система целиком? |
| 4 | **Periodic Assurance** | По расписанию или risk-trigger | Security, database, performance, UX, API, i18n, tech-debt audits | Какие системные риски не видны автоматическим проверкам? |

Отдельные процессы, не являющиеся уровнями:

- **Engineering Governance** — принятие остаточного риска, release decision,
  бизнес- и продуктовые решения;
- **Control Maintenance** — актуализация инструкций, agent memory, backlog,
  baselines, suppressions и самих guardrails.

`Pyramid` можно сохранить как визуальную метафору доклада, но не использовать как
основной классификатор артефактов.

## Правила терминологии

1. Классифицировать control по области проверки, а не по месту запуска. Unit test
   не становится `System Check` только потому, что запущен в CI.
2. Использовать `gate` для обязательной точки прохождения: `commit gate`,
   `PR gate`, `release gate`.
3. Использовать `check` для автоматической детерминированной проверки.
4. Использовать `audit` или `assurance review` для исследовательской проверки,
   требующей evidence и инженерного суждения.
5. Помечать термины в glossary как `industry-standard`, `borrowed`,
   `SAE-specific` или `informal`.

| Сейчас | Целевое название или трактовка |
|--------|-------------------------------|
| Inner / Outer Loop | Убрать из основной модели; использовать названия уровней |
| Acceptance Cycle | **System Checks** или **Release Validation** по контексту |
| Human Outer Loop | **Engineering Governance** |
| Grooming Loop | **Control Maintenance** |
| Backlog grooming | **Backlog refinement** |
| Decision Guard = ADR | **Decision Guard** — SAE-specific ссылка на decision record/ADR, не синоним ADR |
| Context Markers | Optional interaction convention, не guardrail |
| Anti-Hallucination Protocol | **Evidence Requirements** |
| Paranoid mode | **High-assurance mode** |
| memory-hygiene | Рассмотреть `agent-memory-maintenance` |
| bot-audit | Рассмотреть `telegram-bot-audit`, пока skill Telegram-specific |
| dba-audit | Рассмотреть `database-design-and-query-audit` |
| type-safety | Привести к общей схеме: `type-safety-audit` |

Переименование директорий требует отдельного migration plan. До этого документация
должна точно определять текущие имена.

## Исправление принципов

### Обоснование guardrail

Правило «guardrail создаётся только после реального бага» заменить на:

> Guardrail должен быть обоснован реальным инцидентом, достоверной моделью угроз,
> нормативным требованием либо документированным сценарием отказа с высоким impact.

Нулевое количество срабатываний не является достаточным основанием для удаления.
Учитывать severity риска, вероятность, стоимость поддержки, false-positive rate и
наличие компенсирующих controls.

### Regression control

«Нет теста — нет bug fix» ограничить автоматизируемыми и воспроизводимыми
дефектами. Для configuration, documentation, operational и process defects
допускать другой regression control с явным объяснением.

### Независимость review

`Separate agent` не считать автоматически независимым reviewer. Признаки
разделения контроля: отдельный контекст, evidence-only режим, отсутствие
write-доступа, другой checklist и при необходимости другая модель или человек.

## Единый контракт skill

Каждый устанавливаемый skill должен иметь `SKILL.md`, `CHECKLIST.md` и разделы:

1. YAML frontmatter: `name`, `description`.
2. Purpose and non-goals.
3. Applicability and exclusions.
4. Required inputs.
5. Procedure.
6. Evidence requirements.
7. Finding schema.
8. Severity and confidence definitions.
9. Outputs and downstream consumer.
10. Trigger or schedule.
11. Limitations and expected false positives.

Единая finding schema:

```text
ID
Severity: BLOCKER | CRITICAL | MAJOR | MINOR
Confidence: CONFIRMED | NEEDS_REVIEW
Category / Control
Evidence: file:line, command output, trace or reproduction
Impact
Recommended action
Owner / disposition
```

Severity описывает impact и срочность. `Caching`, `Performance`, `Authorization`
являются категориями, а не severity.

Checklist items без достаточного контекста являются investigation signals, а не
готовыми findings. Например, `SELECT *`, interface с одной реализацией, CQRS,
`Task.WhenAll` на малой коллекции и отсутствие индекса на отдельном `WHERE` не
доказывают defect автоматически.

## Backlog

### P0 — восстановить доверие к репозиторию

#### METH-001 — Вернуть зелёную сборку DemoProject ✅

> **Сделано:** NBomber обновлён 5.8.0 → 6.5.0 (в 6.x нет транзитивной зависимости
> MessagePack — suppression удалён, а не оформлен как risk acceptance).
> Clean restore + build (`TreatWarningsAsErrors=true`) + test run (33 passed) проходят.

- Разобраться с транзитивным `MessagePack 2.4.59` через NBomber.
- Обновить/заменить зависимость либо оформить временное risk acceptance.
- Для suppression добавить owner, rationale и expiry/review date.
- Проверить clean restore, build с warnings-as-errors и test run.

**Готово, когда:** команды из README проходят, а CI не скрывает известную
high-severity vulnerability бессрочным suppression.

#### METH-002 — Исправить Decision Guards ✅

> **Сделано:** шаблон переписан — пример `HasQueryFilter` больше не содержит вызова,
> объявленного удалённым (комментарий заменяет удалённый код); Decision Guard явно
> описан как SAE-specific ссылка на decision record/ADR, а не синоним ADR; в шаблон
> добавлен lifecycle (status/owner/review date/superseded-by). Glossary и подписи
> «Decision Guards (ADR)» в README/PYRAMID/AGENTS обновлены.

- Устранить пример, где `HasQueryFilter` объявлен удалённым, но вызывается.
- Перестать называть inline ID полной реализацией ADR.
- Добавить lifecycle: status, owner, review date, superseded-by.

**Готово, когда:** record и код не противоречат друг другу, связь Decision Guard ↔
ADR описана явно.

#### METH-003 — Убрать опасные абсолютные правила ✅

> **Сделано:** «born from pain» / «0 triggers in 3 sprints» заменены на risk-based
> правило (инцидент, threat model, нормативное требование или high-impact сценарий;
> ноль срабатываний — не основание для удаления). «No test — no fix» ограничено
> воспроизводимыми автоматизируемыми дефектами. Изменения синхронно внесены в
> PYRAMID.md, PYRAMID.en.md, rules/AGENTS_TEMPLATE.md.

- Переписать `born from pain`, `0 triggers in 3 sprints`, `no test — no fix`.
- Синхронно обновить README, PYRAMID, AGENTS template, onboarding и skills.
- Добавить risk-based критерии создания и удаления controls.

**Готово, когда:** правила допускают proactive security/compliance controls и
редкие high-impact risks.

#### METH-004 — Актуализировать Codex integration guide ✅

> **Сделано:** гайд переписан под актуальный Codex CLI — `AGENTS.md` (root + nested +
> `AGENTS.override.md`) вместо legacy `.codex/instructions.md`; убраны неподтверждённые
> утверждения об отсутствии extensibility (есть skills, custom agents, MCP, hooks) и о
> фиксированном context window; добавлены `last_verified: 2026-07-14` и ссылки на
> primary docs. Устаревшие упоминания `.codex/instructions.md` синхронно исправлены в
> docs/agents/README.md, docs/README(.en).md, OPENCODE.md, bootstrap skill, INSTALL.md,
> EXAMPLE-REPORT.md.

- Проверить configuration format и capabilities по official documentation.
- Удалить неподтверждённые утверждения про `.codex/instructions.md`, отсутствие
  extensibility и фиксированный context window.
- Добавить `last_verified` и ссылку на primary documentation.

**Готово, когда:** guide воспроизводим на актуальном Codex и учитывает `AGENTS.md`
и skills.

### P1 — сделать методологию последовательной

#### METH-010 — Внедрить Engineering Assurance Levels ✅

> **Сделано:** модель (Control Foundation + Change/Behavior/System Checks + Periodic
> Assurance + процессы Engineering Governance и Control Maintenance) введена в
> README.md/README.en.md как канонический классификатор с картой артефактов.
> PYRAMID.md/PYRAMID.en.md помечены как legacy/визуальная метафора с таблицей
> маппинга слоёв на уровни. GLOSSARY обновлён (уровни + процессы, legacy-термины
> помечены). ONBOARDING получил примечание о маппинге; `Paranoid` → `High-assurance`
> во всех артефактах. Правило AGENTS.md обновлено.

- Переписать модель в README, PYRAMID, GLOSSARY и ONBOARDING.
- Убрать противоречия между Layer 2, audits и Outer Loop.
- Отделить Governance и Control Maintenance от product checks.
- Решить судьбу имени `PYRAMID.md` как legacy/visual alias.

**Готово, когда:** каждый артефакт относится к одному уровню или supporting process.

#### METH-011 — Создать нормативный glossary

- Определить `check`, `test`, `gate`, `audit`, `assurance`, `validation`,
  `governance`, `maintenance`, `ratchet`, `baseline`.
- Отделить SAE-specific names от ADR, Zero Trust, DORA и других известных терминов.
- Удалить или обосновать claims про DORA и Zero Trust.

**Готово, когда:** читатель понимает происхождение и точный смысл каждого термина.

#### METH-012 — Нормализовать skill contracts

- Добавить отсутствующий frontmatter.
- Унифицировать разделы, finding schema, severity и confidence.
- Переименовать `ANTI-HALLUCINATION` в `Evidence Requirements`.
- Убрать обязательные emoji/context markers из переносимого ядра.
- Явно оформить bootstrap support templates как неустанавливаемый skill.

**Готово, когда:** автоматический schema-lint проверяет любой skill.

#### METH-013 — Пересмотреть эвристические audits

Приоритет: `simplicity-audit`, `performance-audit`, `security-audit`,
`dba-audit`, `tech-debt-audit`.

- Разделить evidence-backed findings и investigation signals.
- Удалить универсальные thresholds либо требовать project baseline.
- Убрать ложные правила вроде «индекс для каждого WHERE».
- Для security опираться на threat model и data classification.
- Связывать severity с impact, а не с риторикой.

**Готово, когда:** `CONFIRMED` finding доказывается evidence, а эвристика не
получает severity автоматически.

#### METH-014 — Свести onboarding к одному безопасному пути

- Убрать массовое `cp tests/patterns/*.cs` из quick start.
- Использовать путь inventory → risk profile → selected controls → validation.
- Согласовать сроки внедрения с запретом Big Bang.
- Заменить `3 BUG tests` и `last 5 PRs` на capability-based criteria.
- Переименовать `Paranoid` в `High-assurance`.

**Готово, когда:** onboarding не предлагает копировать неприменимые зависимости и
не противоречит adaptation rules.

### P2 — доказательность и поддерживаемость

#### METH-020 — Добавить evidence model

- Для процентов и ROI указывать dataset, period, denominator и classification.
- Разделять measured result, observed case, estimate и hypothesis.
- Неподтверждённые точные числа удалить или маркировать illustrative.

#### METH-021 — Отредактировать английский

- Сохранить яркий язык в talk/manifesto материалах.
- В normative docs использовать neutral scoped statements.
- Заменить `repeatable persona`, `pokes the app`, `pattern fetishism`,
  `data matryoshkas`, `anathema`, `delete without regret`.
- Унифицировать capitalization, hyphenation и названия ролей.

#### METH-022 — Устранить drift integration guides

- Хранить `last_verified`, primary docs и tested version.
- Удалить ненужные context-window comparisons.
- Проверять устаревшие configuration formats периодическим audit.

#### METH-023 — Автоматизировать качество репозитория

- Markdown link check.
- Skill schema/frontmatter check.
- Knowledge-map completeness check.
- Проверка stale suppressions и expired decision guards.
- Один источник test-running logic вместо дублирования scripts и workflow.
- Не запускать один test project дважды в CI без причины.

#### METH-024 — Добавить case studies

- Минимум один небольшой проект и один production-like сценарий.
- Показать risk profile, selected/rejected controls, стоимость, findings,
  false positives и maintenance cost.
- Показать удалённые и неприменимые guardrails, а не только успехи.

## Рекомендуемая последовательность

1. Закрыть `METH-001`–`METH-004`.
2. Принять terminology decision по `METH-010` и `METH-011`.
3. На новом contract переработать 2–3 skills и затем мигрировать остальные.
4. Переписать onboarding после стабилизации levels и skill schema.
5. Провести language pass после структурных изменений.
6. Добавить evidence и CI checks до публикации claims о результативности.

До нормализации существующих артефактов не добавлять новые audit-skills и не
начинать массовое переименование директорий.

## Definition of Done ревизии

- README объясняет модель за одну страницу и ведёт по одному onboarding path.
- В core docs нет конкурирующих классификаций `layers/loops/cycles`.
- Все устанавливаемые skills проходят schema-проверку.
- Обязательные controls имеют applicability, evidence и lifecycle.
- Примеры собираются и выполняются командами из README.
- Количественные claims имеют источник или помечены как estimates.
- Integration guides содержат дату и источник проверки.
- Английский нейтрален, конкретен и не опирается на persona/emoji ceremony.
