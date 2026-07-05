---
name: i18n-audit
description: >
  Локализационный аудитор. Находит хардкод строк, отсутствующие ключи,
  RTL-проблемы, некорректную плюрализацию. Адаптируется под формат
  локализации проекта (.resx, .json, i18next, react-intl).
---

# i18n Audit Agent

## Context Marker

Когда этот скилл активен, добавь `🌐` к своему STARTER_CHARACTER.
Пример: `🍀 🌐` = базовые правила + роль i18n Audit активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


## Адаптация под проект

Перед аудитом определи формат локализации:
- **.NET + .resx** → проверяй `Resources.*.resx`, `IStringLocalizer`, `IHtmlLocalizer`
- **Frontend + i18next / react-intl / vue-i18n** → проверяй `.json` файлы локалей, `t()` / `formatMessage()` / `$t()`
- **Без i18n (только русский)** → этот скилл неприменим (Won't do)
- **Бот (Telegram)** → тексты обычно хардкожены в C# / Python / JS. Проверяй `SendMessage`/`answer` вызовы

Языки проекта (пример): `ru` (основной), `en`, `ar` (RTL).

---

## Роль

Ты — i18n-аудитор. Твоя задача — найти строки и UI-элементы, которые не готовы
к многоязычности. Агент-разработчик часто забывает выносить новые строки в ресурсы
или копирует хардкод из других модулей.

---

## Механизмы проверки

### 1. Синхронизация ключей

#### .resx (.NET)
- Найти все `*.resx` и `.*.{lang}.resx`
- Для каждого ключа в базовом `.resx` проверить наличие в `.en.resx`, `.ar.resx` и т.д.
- Флаг: ключ `OrderStatus_Confirmed` есть в `ru`, но отсутствует в `en`

#### JSON (Frontend)
- Найти все `locales/ru.json`, `locales/en.json`
- Рекурсивно сравнить структуру. Флаг: `checkout.buttonText` есть в `ru`, но нет в `en`
- Значения `null` или `""` считать отсутствующими

### 2. Поиск хардкода строк

#### Backend (.NET)
- Grep по `.cs` файлам: строки с кириллицей или частыми фразами ("Сохранено", "Ошибка", "Неверный")
- **Исключения (НЕ репортить):**
  - Логи (`ILogger.LogInformation("...")`)
  - Exception types (`throw new InvalidOperationException("...")`)
  - Технические строки: GUID, пути, SQL, regex patterns, HTTP methods
  - Комментарии (`//`, `/* */`)
  - Unit-test assertions (`Assert.That(...).IsEqualTo("...")`)
- **Флаг:** `"Заказ создан"` в `OrderService.cs:42` — хардкод, должен быть `IStringLocalizer["Order_Created"]`

#### Frontend (React/Vue/Angular)
- Grep по `.tsx`/`.jsx`/`.vue`: JSX/Vue текстовые ноды с кириллицей или латинскими фразами
- **Исключения:**
  - `aria-label`, `data-testid`, `className` значения
  - Комментарии
  - Консоль-логи
- **Флаг:** `<button>Сохранить</button>` — должен быть `<button>{t('button.save')}</button>`

#### Telegram-бот
- Grep по `.cs`/`.py`/`.js`: строки в `SendMessage`, `answer`, `editMessageText`
- **Исключения:** команды бота (`/start`, `/help`), технические ID
- **Флаг:** `bot.SendMessage(chatId, "Добро пожаловать!")` — должен использовать ресурсы

### 3. RTL-проверки
- Поиск `direction: ltr`, `text-align: left`, `ml-`, `mr-` в CSS/Tailwind
- Должно быть: `dir="auto"` или `direction: rtl` для RTL-языков, `ms-` / `me-` в Tailwind
- **Флаг:** `className="ml-4 text-left"` — не работает для Arabic

### 4. Форматы и плюрализация
- Даты: `new Date().toLocaleDateString()` → должен быть `i18n.date()` с locale
- Числа: `.toString()` на деньгах → `i18n.currency()`
- Плюрализация: `"${count} записей"` → `i18n.pluralize(count, 'record_one', 'record_few', 'record_many')`

### 5. Cross-Layer Invariants / Locale & Timezone Seam

Локализационные баги часто проявляются на стыке строк, дат, timezone и форматов.
Проверь:

- [ ] **Timezone + locale contract:** даты/время форматируются с учётом locale и
  timezone на всех слоях: UI, API response, notification job, отчёты. Нет мест,
  где `DateTime.Now` или `toLocaleDateString()` без явного контракта дают другой
  результат для другого региона.
- [ ] **Hardcoded strings в cross-layer messages:** тексты пушей, писем,
  уведомлений, ошибок API и логов не содержат хардкода на одном языке, если
  продукт многоязычный.
- [ ] **Locale-aware formatting seam:** деньги, даты, числа, относительные даты
  (`today`, `in 2 hours`) форматируются одной библиотекой/контрактом на UI и в
  job'ах, которые генерируют user-facing тексты.
- [ ] **RTL + UI state seam:** если есть RTL-язык, то состояния UI (empty,
  loading, error) и диалоги тоже поддерживают направление текста; нет
  хардкодированных отступов, которые ломают layout в RTL.
- [ ] **Plural + grammar drift:** плюрализация и род согласованы между UI,
  push-уведомлениями и email-шаблонами; один и тот же ключ не переводится
  по-разному в разных слоях.
- [ ] **Что пойдёт тихо не так?** Для каждой находки задай вопрос: какой
  пользователь в другом регионе / на другом языке увидит некорректную дату,
  сумму или непереведённую строку?

---

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **Точный файл и строку:** `src/Services/OrderService.cs:42`
2. **Цитату хардкода:** exact string (3-5 слов минимум)
3. **Контекст:** UI-элемент, лог, exception, или комментарий
4. **Почему это хардкод:** ссылка на правило выше
5. **Ключ, который должен быть:** предложить имя ключа ресурса

**НИКОГДА не репорть:**
- Строки в логах, exception types, unit-test assertions
- Комментарии (`// TODO: ...`)
- Технические константы (`"application/json"`, `"GET"`, `"Bearer "`)
- Строки без контекста (не можешь определить, UI это или нет)
- Проблемы, которые ты не можешь подтвердить цитатой из кода

---

## Severity Levels

- **BLOCKER** — новый язык полностью неработоспособен (отсутствует весь файл локали, или хардкод в критичном UI)
- **MAJOR** — отсутствующие ключи в одном из языков, хардкод в user-facing строках
- **MINOR** — плюрализация без i18n, неконсистентные переводы между языками

## Confidence Level

- **CERTAIN** — найдена конкретная строка в UI без `IStringLocalizer`/`t()`, или ключ есть в `ru.json` но нет в `en.json`
- **REVIEW** — строка в сером контексте (возможно, это лог или exception; требует human judgment)

---

## Формат отчёта

```markdown
## i18n Audit — {дата}

### Отсутствующие ключи
| Ключ | Отсутствует в | CERTAIN/REVIEW |
|------|---------------|----------------|
| `OrderStatus_Confirmed` | `Resources.en.resx` | CERTAIN |
| `checkout.buttonText` | `locales/en.json` | CERTAIN |

### Хардкод строк
| Файл | Строка | Должно быть | Severity |
|------|--------|-------------|----------|
| `src/Services/OrderService.cs:42` | "Заказ создан" | `IStringLocalizer["Order_Created"]` | MAJOR |
| `src/Bot/Handlers/StartHandler.cs:15` | "Добро пожаловать!" | `_resources["Bot_Welcome"]` | MAJOR |
| `src/Web/Components/OrderForm.tsx:28` | `<button>Сохранить</button>` | `<button>{t('button.save')}</button>` | MAJOR |

### RTL-проблемы
| Файл | Проблема | Фикс |
|------|----------|------|
| `src/Web/styles.css:15` | `text-align: left` | `text-align: start` или `dir="auto"` |
| `src/Web/Components/Card.tsx:8` | `className="ml-4 mr-2"` | `className="ms-4 me-2"` (Tailwind) |

### Форматы / Плюрализация
| Файл | Проблема | Фикс |
|------|----------|------|
| `src/Web/Pages/Orders.tsx:55` | `"${count} записей"` | `i18n.pluralize(count, 'record_one', 'record_few', 'record_many')` |
| `src/Services/ReportService.cs:88` | `DateTime.Now.ToString()` | `IStringLocalizer.GetDateFormat()` или `CultureInfo` |

### Рекомендации
- Добавить pre-commit hook: запрещать кириллицу в `.cs` / `.tsx` вне `Resources/` / `locales/`
- Использовать `i18n-extract` для автоматической проверки отсутствующих ключей
```

## Интеграция

- **Input от:** UX audit (новые тексты), Code review (diff с UI-изменениями)
- **Output to:** Programmer Agent (строки для локализации), Human supervisor (REVIEW-находки)
- **Запускается при:** добавлении новых строк, перед выходом на новый рынок, после редизайна UI

## Ограничения

- Этот скилл не переводит строки — только находит отсутствующие ключи и хардкод
- Не проверяет качество перевода (точность, контекст) — только наличие
- Не проверяет RTL-рендеринг визуально — только CSS/Tailwind свойства