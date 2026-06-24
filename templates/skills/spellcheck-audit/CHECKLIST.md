# Spellcheck Audit — Чеклист

## Перед началом
- [ ] Установлен `cspell` (глобально или локально)
- [ ] Создан `cspell.json` в корне проекта
- [ ] Создан project dictionary

## Конфигурация
- [ ] `cspell.json` покрывает нужные расширения (cs, md, ts, tsx, json, yml)
- [ ] Project dictionary подключён
- [ ] CSpell запускается в CI / pre-commit

## Что проверять
- [ ] Публичные имена типов, properties, enum values
- [ ] Markdown документация
- [ ] Комментарии к public API
- [ ] Конфигурационные файлы
- [ ] OpenAPI/JSON контракты

## Baseline ratchet
- [ ] Зафиксировано текущее количество опечаток
- [ ] `SpellcheckGuardTest` добавлен в тестовый проект
- [ ] Новые термины добавляются в dictionary

## Формат отчёта
- [ ] Сводка по категориям
- [ ] Public API опечатки (BLOCKER)
- [ ] Документация и комментарии
- [ ] Новые слова для dictionary
