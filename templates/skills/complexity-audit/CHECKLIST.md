# Complexity Audit — Чеклист

## Перед началом
- [ ] Известен стек проекта (.NET version, SonarAnalyzer version)
- [ ] Определён тип проекта: новый / legacy
- [ ] Известны пороги: cognitive ___ / cyclomatic ___
- [ ] Зафиксирован baseline (для legacy)

## Compile-time guardrails (новые проекты)
- [ ] `SonarAnalyzer.CSharp` подключён
- [ ] `S3776` (cognitive) severity = error, threshold = ___
- [ ] `S1541` (cyclomatic) severity = error, threshold = ___
- [ ] `TreatWarningsAsErrors=true`
- [ ] Для API/endpoint слоя отдельные строгие пороги

## Legacy ratchet
- [ ] Baseline-аудит выполнен
- [ ] `complexity-baseline.txt` создан и закоммичен
- [ ] `ComplexityRatchetTest` добавлен в тестовый проект
- [ ] Топ-10 hotspots задокументированы
- [ ] План рефакторинга hotspots сроками (Q/N)

## Hotspot analysis
- [ ] Методы cognitive > 25 разобраны
- [ ] Методы cyclomatic > 15 разобраны
- [ ] Проверено на дублирование логики (cross-check `DuplicationGuardTest`)
- [ ] Для каждого hotspot есть причина сложности

## Decision Guards
- [ ] Осознанные отклонения оформлены как `COMPLEXITY-###`
- [ ] `COMPLEXITY-###` добавлены в `DECISION-GUARDS.md`

## Формат отчёта
- [ ] Сводка по S3776/S1541 с динамикой
- [ ] Список BLOCKER/CRITICAL с файлами/строками
- [ ] Бэклог рефакторинга с ID и сроками
