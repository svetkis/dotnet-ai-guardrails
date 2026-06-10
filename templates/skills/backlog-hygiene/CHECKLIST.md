# Backlog Hygiene Checklist

## Stale
- [ ] Нет задач старше 90 дней без обновления
- [ ] Замерженые спеки закрыты в бэклоге
- [ ] CHANGELOG items связаны с закрытыми задачами

## Orphaned
- [ ] Каждая спека имеет реализацию, помечена `deferred` или удалена
- [ ] Каждый `BUG###_` тест имеет задачу или CHANGELOG entry
- [ ] Нет спек на удалённые модули

## Duplicates
- [ ] Нет duplicate задач в `.backlog/`
- [ ] Нет duplicate AC в разных спеках

## Priority
- [ ] `Must` реально блокирует ближайший релиз
- [ ] `Won't` не превращается в скрытый tech debt
- [ ] `Could` не реализовано молча в production

## Traceability
- [ ] Каждая открытая задача имеет спеку или AC
- [ ] Каждая задача ссылается на код/тест/PR

## Actionability
- [ ] Заголовок содержит глагол + объект (не существительное одно)
- [ ] Есть Definition of Done (1-3 пункта) или AC
- [ ] Нет задач с заголовком < 5 слов и без AC

## Source Tagging
- [ ] Каждая задача помечена `[human]` или `[agent]`
- [ ] `[agent]` без human approval > 14 дней помечены на архив
- [ ] `[agent]` с human approval имеют explicit source

## Test Debt
- [ ] Каждый новый `[HotPath]` имеет задачу на perf-тест
- [ ] Каждый новый публичный endpoint имеет задачу на snapshot-тест
- [ ] Каждое новое `[SensitiveData]` имеет задачу на PiiGuardTest
