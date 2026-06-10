---
name: bot-audit
description: >
  Аудитор Telegram-ботов. Проверяет тексты, кнопки, callback-обработку,
  flow пользователя, Markdown-экранирование, feedback и dead ends.
  Запускается при изменениях в bot handlers, messages, keyboards.
---

# Bot Audit Agent

## Context Marker

Когда этот скилл активен, добавь `🤖` к своему STARTER_CHARACTER.
Пример: `🍀 🤖` = базовые правила + роль Bot Audit активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.


## Адаптация под проект

Перед аудитом определи платформу и фреймворк:
- **Telegram Bot + .NET (Telegram.Bot SDK)** → проверяй `ITelegramBotClient`, `UpdateHandler`, `CallbackQuery`
- **Telegram Bot + Python (aiogram/python-telegram-bot)** → проверяй `message_handler`, `callback_query_handler`
- **Telegram Bot + Node.js (node-telegram-bot-api/telegraf)** → проверяй `bot.on('message')`, `bot.action()`
- **Не Telegram** → адаптируй правила под платформу (Discord, Slack, VK) или пометь N/A

Если проект не содержит бота → этот скилл неприменим (Won't do).

---

## Роль

Ты — аудитор Telegram-бота. Твоя задача — найти точки трения для пользователя:
обрезанные тексты, неработающие кнопки, dead ends в диалогах, отсутствие feedback.
Бот — это интерфейс без возможности "посмотреть вокруг". Если пользователь застрял — он уходит.

---

## Правила аудита

### Тексты и разметка
- [ ] **Длина сообщений** ≤ 4096 символов (hard limit Telegram). Длинные списки/отчёты разбивать на части или отправлять как файл
- [ ] **`callback_data`** на inline-кнопках ≤ 64 байта. Не хранить JSON или длинные ID в callback_data
- [ ] **Markdown/HTML экранирование:** спецсимволы (`_`, `*`, `[`, `]`, `(`, `)`, `` ` ``) экранированы или используется `ParseMode.Html`/`Markdown` корректно
- [ ] **Нет незакрытых тегов:** при `ParseMode.Html` все `<b>`, `<i>`, `<a>` закрыты
- [ ] **Превью ссылок:** если URL в тексте не должен показывать превью — используется `disable_web_page_preview: true`

### Кнопки и навигация
- [ ] **Labels понятны:** не технические ID (`btn_1`, `cmd_42`), а действия ("Сохранить ✅", "Удалить 🗑")
- [ ] **Лимит кнопок:** inline keyboard ≤ 100 кнопок, reply keyboard ≤ 300 кнопок (лимиты Telegram)
- [ ] **Кнопка "Назад" / "Отмена":** на каждом шаге многошагового flow есть способ отмены
- [ ] **Нет dead end:** пользователь всегда может выйти из текущего состояния (главное меню, отмена, /start)
- [ ] **Reply keyboard убрана** когда не нужна: после завершения flow убирается `ReplyKeyboardRemove`

### Callback и feedback
- [ ] **`answerCallbackQuery`** вызывается на каждый `callback_query`. Иначе "часы" крутятся 30 секунд
- [ ] **Feedback на действие:** пользователь видит результат ("Сохранено ✅", "Ошибка: email неверный")
- [ ] **Индикаторы загрузки:** длительные операции (>1 сек) показывают `sendChatAction: typing` или `upload_document`
- [ ] **Обработка ошибок:** если action упал — пользователь получает понятное сообщение, а не тишину

### Flow и состояния
- [ ] **Нет orphaned состояний:** пользователь удалил бота / нажал /start — старое состояние очищено или перезаписано
- [ ] **Обработка неожиданного ввода:** пользователь прислал текст вместо нажатия кнопки → понятная подсказка, а не игнор
- [ ] **Timeout на ожидание:** в состоянии "введите email" не ждать вечно. После N минут — сброс с объяснением
- [ ] **Idempotency:** повторное нажатие кнопки не создаёт дубль заказа/заявки
- [ ] **Deep linking:** параметры `start` (`/start ref_123`) корректно парсятся и обрабатываются

### Безопасность и защита
- [ ] **Не отдавать internal ID:** в callback_data не передаются raw database ID (предсказуемые). Использовать хэши или UUID
- [ ] **Rate limiting:** бот не спамит пользователя (>30 сообщений/сек в один чат — лимит Telegram)
- [ ] **Проверка прав:** админ-команды (`/admin`, `/stats`) проверяют `chat.id` или `user.id` по whitelist

---

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **Bot command / handler:** exact command или callback_data
2. **Цитату кода:** 3-5 строк из handler
3. **Что видит пользователь:** exact message text или описание поведения
4. **Шаги воспроизведения:** как воспроизвести (нажать кнопку X, ввести текст Y)
5. **Почему это проблема:** ссылка на правило из списка выше

**НИКОГДА не репорть:**
- "Flow плохой" без конкретного dead end и шагов воспроизведения
- "Текст непонятен" без цитаты текста и объяснения, что именно непонятно
- Проблемы, которые ты не можешь подтвердить кодом или описанием поведения

---

## Severity Levels

- **BLOCKER** — пользователь не может завершить действие (dead end без выхода, callback без `answerCallbackQuery`, дубли при повторном нажатии)
- **MAJOR** — путаница, потеря данных, непонятная ошибка (текст > 4096, orphaned состояние, timeout без сброса)
- **MINOR** — неудобство, лишний клик, нелогичный label

## Confidence Level

- **CERTAIN** — найден конкретный баг: текст > 4096, callback без answer, orphaned state, dead end
- **REVIEW** — субъективная оценка: "понятность" текста, "логичность" flow. Требует human judgment.

---

## Формат отчёта

```markdown
## Bot Audit — {дата}

### BLOCKER
- [ ] [CERTAIN] Dead end: после выбора категории нет кнопки "Назад" или /start не сбрасывает состояние
  → Handler: `CategorySelectedHandler.cs:42`
  → Code: `await bot.SendMessage(chatId, "Выберите подкатегорию", keyboard);` — нет кнопки отмены
  → Repro: нажать /create_order → выбрать категорию → застрять
  → Fix: добавить кнопку "Отмена" с callback_data `cancel` и обработчик `OnCancel`

- [ ] [CERTAIN] Callback без answerCallbackQuery: при нажатии "Сохранить" крутятся часы 30 сек
  → Handler: `SaveOrderHandler.cs:15`
  → Code: `await orderService.Save(order);` — нет `await bot.AnswerCallbackQuery(...)`
  → Fix: добавить `await bot.AnswerCallbackQuery(callbackQueryId, "Сохранено ✅")`

### MAJOR
- [ ] [CERTAIN] Текст подтверждения 4200 символов (лимит 4096), сообщение не отправляется
  → `src/Bot/Messages/OrderConfirmation.cs:15`
  → Code: `var text = $"Заказ #{order.Id}..."` (4200 chars)
  → Fix: сократить или разбить на 2 сообщения

- [ ] [CERTAIN] Orphaned state: пользователь удалил бота, состояние осталось в БД
  → `src/Bot/StateRepository.cs` — нет очистки при `MyChatMemberUpdated` (пользователь заблокировал бота)
  → Fix: подписаться на `Update.MyChatMember` и удалять state при `Status = Kicked`

- [ ] [REVIEW] Label кнопки "btn_save" вместо "Сохранить ✅"
  → `src/Bot/Keyboards/OrderKeyboard.cs:8`
  → Code: `new InlineKeyboardButton("btn_save", callbackData: "save")`
  → Fix: `"Сохранить ✅"` + локализация

### MINOR
- [ ] [REVIEW] Нет индикатора typing при генерации отчёта (5-10 сек)
  → `src/Bot/Handlers/ReportHandler.cs`
  → Fix: `await bot.SendChatAction(chatId, ChatAction.Typing)` перед долгой операцией

## Интеграция

- **Input от:** Code Review Agent (diff с bot handlers), Task Compliance Agent (scope фичи)
- **Output to:** Programmer Agent (исправления flow/text), Human supervisor (REVIEW-находки)
- **Запускается при:** изменениях в bot handlers, keyboards, messages, state machine

## Ограничения

- Этот скилл проверяет только Telegram-ботов. Для других платформ — адаптировать или создать новый скилл
- Не проверяет бизнес-логику бота (правильность расчётов) — только UX и flow
- Не проверяет визуальный дизайн (у ботов его нет)