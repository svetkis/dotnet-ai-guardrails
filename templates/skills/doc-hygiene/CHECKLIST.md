# Doc Hygiene Checklist

## Hierarchy
- [ ] Все `AGENTS.md` иерархически согласованы
- [ ] Deep overrides явно помечены и обоснованы
- [ ] Нет циклических противоречий (корень vs модуль vs подмодуль)

## Internal Contradictions
- [ ] Нет пар MUST/FORBIDDEN, противоречащих друг другу в одном файле
- [ ] Конфликты разрешены или помечены на обсуждение

## Code Drift
- [ ] Каждое правило AGENTS.md имеет guardrail в коде или CI
- [ ] Упомянутые скиллы/тесты существуют в `templates/skills/`, `tests/`
- [ ] Decision Guards из AGENTS.md найдены в коде

## Dead Rules
- [ ] Каждое MUST/FORBIDDEN имеет enforcement (тест, компилятор, linter, CI)
- [ ] Правила без enforcement > 90 дней помечены `dead-rule`
- [ ] Для каждого dead rule — решение: добавить guardrail или удалить

## Cross-Agent
- [ ] Все `docs/agents/*.md` консистентны с корневым `AGENTS.md`
- [ ] Описания pipeline идентичны для всех агентов (формат отличается, суть — нет)
- [ ] Нет ссылок на удалённые скиллы/модули

## Fact Check (Documentation vs Code)
- [ ] Все числа в отчётах/документации верифицированы через `git log` / `wc` / `grep`
- [ ] Все даты коммитов соответствуют `git log`
- [ ] Все имена файлов и номера строк из примеров существуют
- [ ] Все `PERF-###` / `DB-###` / `BR-###` ведут на существующий код
- [ ] Все хеши коммитов в case studies корректны (`git show --stat`)

## README & Changelog
- [ ] Команды сборки актуальны
- [ ] CHANGELOG покрывает последний релиз
- [ ] Нет stale ссылок

## Size Budget
- [ ] Корневой AGENTS.md ≤ 200 строк (warning > 150)
- [ ] Module-level AGENTS.md ≤ 80 строк
- [ ] Если превышен — есть план разбиения или рефакторинга
