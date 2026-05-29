# Task Compliance — Чеклист

## Phase 1: Замысел и контракты
- [ ] Прочитан spec / backlog item
- [ ] Извлечены acceptance criteria (AC)
- [ ] Определены границы scope (IN / OUT)

## Phase 2: Анализ diff
- [ ] Получен `git diff main...[branch]`
- [ ] Составлен список изменённых файлов
- [ ] Отфильтрованы auto-generated файлы

## Phase 3: Трассируемость
- [ ] Каждый AC сопоставлен с кодом в diff
- [ ] Каждый AC имеет статус: IMPLEMENTED / TESTED / MISSING / UNTESTED
- [ ] Нет удалённого функционала, требуемого AC

## Phase 4: Обнаружение scope creep
- [ ] Нет кода вне scope spec'а
- [ ] Нет новых public-методов вне use cases
- [ ] Нет изменений в unrelated слоях

## Phase 5: Доказательства
- [ ] Каждая находка включает: файл, строка, цитата кода, цитата spec
- [ ] Нет галлюцинированных находок

## Quality Gates
- [ ] Каждый AC сопоставлен или помечен MISSING
- [ ] Каждый IMPLEMENTED AC имеет статус TESTED или UNTESTED
- [ ] Нет SCOPE_CREEP без цитаты границ бэклога
- [ ] Нет REGRESSION_RISK без показа удалённого кода
