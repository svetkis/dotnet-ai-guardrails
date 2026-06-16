# Frontend Code Review — Чеклист

## Перед началом
- [ ] Получен staged diff (`git diff --cached`)
- [ ] Известен контекст задачи (backlog item / spec)
- [ ] Подтверждена версия React (>= 18) и TypeScript (>= 4.8); легаси-проверки помечены N/A
- [ ] Скилл активирован автоматически перед `git commit` или явно через `/skill:frontend-code-review`

## Pre-commit / Триггер
- [ ] В staged-изменениях есть frontend-файлы (*.tsx, *.ts, *.jsx, *.js, *.css, *.scss, *.json)
- [ ] Backend-only изменения пропущены (используется `code-review`)
- [ ] При пустом staged diff агент не пишет находок и не блокирует коммит
- [ ] Агент НЕ вызывает `git commit` самостоятельно

## React / Hooks
- [ ] Хуки вызываются только на верхнем уровне, не в циклах/условиях
- [ ] Кастомные хуки именуются с `use`
- [ ] `useEffect` имеет исчерпывающий deps-массив или обоснованный комментарий
- [ ] `useEffect` не используется для derived state
- [ ] `useEffect` cleanup для подписок, таймеров, слушателей, `AbortController`
- [ ] `useState`: нет прямой мутации, используется функциональный updater где нужно
- [ ] `useMemo` / `useCallback` обоснованы, а не "на всякий случай"
- [ ] Context разбит по concern'ам, default value соответствует форме

## Рендеринг / JSX
- [ ] `key` — стабильные уникальные ID, не индекс (если список изменяемый)
- [ ] Условный рендеринг защищён от `0` / `""` (`!!condition` или тернарник)
- [ ] `dangerouslySetInnerHTML` только с санитизацией
- [ ] Нет inline объектов/массивов/функций в пропсах, ломающих memo
- [ ] Нет прямого DOM-манипулирования вне refs/effects
- [ ] Обработчики событий именованные, где это важно для perf/readability

## TypeScript
- [ ] Нет неявного `any` (`strict: true`)
- [ ] `!` non-null assertion обоснован
- [ ] `as` casts обоснованы и прокомментированы
- [ ] Экспортируемые компоненты/хуки имеют типы возвращаемых значений или сильную инференцию
- [ ] Discriminated unions вместо `| undefined`

## Производительность
- [ ] `React.memo` обоснован
- [ ] Тяжёлые компоненты / роуты лениво загружаются
- [ ] Context не передаёт новые объекты/массивы без мемоизации
- [ ] Изображения с lazy loading и явными размерами

## Доступность
- [ ] Кликабельные элементы — `<button>`, не `<div onClick>`
- [ ] Изображения имеют `alt` или `role="presentation"`
- [ ] Инпуты имеют `<label>` или `aria-label`/`aria-labelledby`
- [ ] Модалки/дропдауны ловят и восстанавливают фокус
- [ ] Нет положительного `tabIndex`
- [ ] Статусы/ошибки не только цветом

## Безопасность
- [ ] Нет `innerHTML`, `eval`, `new Function` с пользовательским вводом
- [ ] `href`/`src` не содержат сырой пользовательский ввод, нет `javascript:`
- [ ] Секреты не захардкожены в коде
- [ ] Новые npm-зависимости не подозрительны

## State Management
- [ ] Prop drilling не глубже 2 промежуточных компонентов без использования
- [ ] Глобальный state обоснован, локальный UI state не в Redux/Zustand без причины
- [ ] Серверный state через RTK Query / TanStack Query / SWR, а не ручной кэш
- [ ] Immutability в reducer/store

## Формы
- [ ] Контролируемые инпуты (`value` + `onChange`)
- [ ] Валидация доступна и срабатывает на submit/blur
- [ ] Обработчик submit предотвращает дефолт, управляет loading/error, блокирует повторную отправку

## Стили
- [ ] Единый подход в проекте (CSS Modules / Tailwind / styled-components)
- [ ] Нет inline стилей без причины
- [ ] Responsive/mobile-first

## Тесты
- [ ] React Testing Library + Vitest/Jest, нет Enzyme
- [ ] Предпочтительны `getByRole`/`getByLabelText` над `getByTestId`
- [ ] Используется `@testing-library/user-event`
- [ ] Асинхронные тесты корректно ждут состояние (`waitFor`, `findBy*`)

## Формат отчёта

```markdown
## Frontend Code Review — {дата}

### BLOCKER
- [ ] {описание} → {файл:строка}

### CRITICAL
- [ ] {описание} → {файл:строка}

### MAJOR
- [ ] {описание} → {файл:строка}

### Verdict
- [ ] APPROVED
- [ ] APPROVED_WITH_NITS
- [ ] CHANGES_REQUESTED
```
