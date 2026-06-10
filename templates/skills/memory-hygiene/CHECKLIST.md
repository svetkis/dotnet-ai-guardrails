# Memory Hygiene Checklist

## Semantic Duplicates
- [ ] Найдены intent-duplicates (не только verbatim)
- [ ] Дубли AGENTS.md в Auto Memory помечены на удаление
- [ ] Переформулировки одного правила сгруппированы

## Hierarchical Drift
- [ ] Каждая архитектурная заметка сверена с ближайшим AGENTS.md
- [ ] Практические мелочи (команды, конвенции) оставлены в памяти
- [ ] Конфликты Auto Memory vs AGENTS.md задокументированы

## Workaround Audit
- [ ] Каждая негативная рекомендация («избегать», «не использовать») имеет source
- [ ] Workaround'ы без BUG### / PR / test > 30 дней помечены `stale-workaround`
- [ ] Нет костылей, зафиксированных как permanent rules

## Project Boundary
- [ ] Стек в памяти (.NET version, ORM) совпадает с `global.json` / `.csproj`
- [ ] Команды сборки/тестирования актуальны для текущего репо
- [ ] Нет упоминаний технологий, отсутствующих в проекте

## Stale References
- [ ] Нет ссылок на удалённые файлы
- [ ] Нет упоминаний устаревших технологий (не соответствующих global.json)
- [ ] Команды сборки/тестирования актуальны

## Todo Graveyard
- [ ] Найдены все «Consider», «Need to», «Should», «TODO», «Eventually"
- [ ] Items без тикета/PR и старше 30 дней помечены на архивирование
- [ ] Items с тикетом помечены `tracked`

## Observation Confidence
- [ ] «Preferences» / «conventions» имеют explicit source (PR, commit, human)
- [ ] Preferences без source и старше 60 дней помечены `unverified`
- [ ] Нет one-shot решений, обобщённых как team rules

## Canonical Boundaries
- [ ] Архитектура — только в AGENTS.md
- [ ] Практика — только в Auto Memory
- [ ] Нет смешивания слоёв
