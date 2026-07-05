# Business Risk Audit — Checklist

## Pre-flight

- [ ] Получены findings от 5–7 доменных аудитов (security, dba, perf, ux, i18n, code-review и др.)
- [ ] Получен diff изменений за последние 1–2 недели или scope большого рефакторинга
- [ ] Известны 2–3 ключевых пользовательских сценария, затронутых изменениями

## End-to-end flow reconstruction

- [ ] Для каждого сценария восстановлена цепочка: UI/cache → API → domain → events → outbox → jobs → db/migrations
- [ ] Каждый слой цепочки отображён на конкретные файлы / сервисы
- [ ] Найдены места, где контекст передаётся между слоями (DTO, events, cache keys, migration model)

## Seam analysis — 5 вопросов на сценарий

- [ ] **Не тот субъект?** Identity / ownership / session context разрешается одинаково на всех слоях
- [ ] **Не тот момент времени?** Даты и timezone парсятся, хранятся и отображаются по одному контракту
- [ ] **Не тот источник истины?** Кэш, sessionStorage, read model и БД не противоречат после write
- [ ] **Не та проекция / миграционная реальность?** Runtime model соответствует БД и миграциям
- [ ] **Что пойдёт тихо не так для реального пользователя?** Каждая находка заканчивается этим вопросом

## Cross-layer invariants

- [ ] **State resurrection:** прерванный flow, back button, sessionStorage, retry не воскрешают устаревшее состояние
- [ ] **Ownership resolution:** UserId / OwnerId / ActorId / ContextId интерпретируются одинаково в API, domain и jobs
- [ ] **Timezone contract:** relative date parsing, DateTimeKind, БД, UI — всё по одному договору
- [ ] **Cache vs source-of-truth:** write инвалидирует кэш; чтение после write не возвращает stale данные
- [ ] **Runtime vs migration drift:** модель в коде, миграции и данные согласованы
- [ ] **Eventual consistency:** UI и downstream consumers корректно обрабатывают окно между событием и job

## Synthesis

- [ ] Findings доменных аудитов сгруппированы по сценариям, а не по скиллам
- [ ] Найдены комбинации findings из разных доменов, которые вместе дают системный риск
- [ ] Каждый системный риск имеет trigger и business impact
- [ ] Для каждого риска предложен fix на уровне контракта/пайплайна, а не одного файла

## Quality gates

- [ ] Каждая системная находка содержит: название риска, сценарий, seam, evidence, trigger, business impact, fix
- [ ] BLOCKER/CRITICAL findings имеют concrete шаги воспроизведения
- [ ] REVIEW findings помечены как требующие human judgment или E2E-проверки
- [ ] Нет findings вроде «система сложная» без конкретного сломанного инварианта
