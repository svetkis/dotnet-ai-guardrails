---
name: bot-audit
description: >
  Telegram bot auditor. Checks texts, buttons, callback handling,
  user flow, Markdown escaping, feedback and dead ends.
  Runs when bot handlers, messages, keyboards change.
---

# Bot Audit Agent

> Optional interaction convention (agent-specific): some agents mark an active
> skill with an emoji in their status stack (e.g., `🤖` for this skill, prefixed
> with `♻️` on re-read). The skill is fully usable without it.

## Purpose and Non-Goals

You are a Telegram bot auditor. Your task is to find friction points for the user:
truncated texts, broken buttons, dead ends in dialogs, missing feedback.
A bot is an interface without the ability to "look around". If the user gets stuck — they leave.

Non-goals: bot business logic (correctness of calculations) — only UX and flow;
visual design (bots don't have it); non-Telegram platforms (adapt or create a new skill).

## Applicability and Exclusions

Before auditing, define the platform and framework:
- **Telegram Bot + .NET (Telegram.Bot SDK)** → check `ITelegramBotClient`, `UpdateHandler`, `CallbackQuery`
- **Telegram Bot + Python (aiogram/python-telegram-bot)** → check `message_handler`, `callback_query_handler`
- **Telegram Bot + Node.js (node-telegram-bot-api/telegraf)** → check `bot.on('message')`, `bot.action()`
- **Not Telegram** → adapt rules for the platform (Discord, Slack, VK) or mark N/A

If the project does not contain a bot → this skill is not applicable (Won't do).

## Required Inputs

- Repo access: bot handlers, keyboards, message texts, state machine code, and the diff under review.
- The platform/framework in use (see adaptation above).
- A way to reproduce flows: bot token/test chat, handler logs, or step-by-step code tracing.

## Procedure

### Texts and Markup
- [ ] **Message length** ≤ 4096 characters (Telegram hard limit). Long lists/reports should be split into parts or sent as a file
- [ ] **`callback_data`** on inline buttons ≤ 64 bytes. Don't store JSON or long IDs in callback_data
- [ ] **Markdown/HTML escaping:** special characters (`_`, `*`, `[`, `]`, `(`, `)`, `` ` ``) are escaped or `ParseMode.Html`/`Markdown` is used correctly
- [ ] **No unclosed tags:** with `ParseMode.Html` all `<b>`, `<i>`, `<a>` are closed
- [ ] **Link preview:** if URL in text should not show preview — use `disable_web_page_preview: true`

### Buttons and Navigation
- [ ] **Labels are clear:** not technical IDs (`btn_1`, `cmd_42`), but actions ("Save ✅", "Delete 🗑")
- [ ] **Button limit:** inline keyboard ≤ 100 buttons, reply keyboard ≤ 300 buttons (Telegram limits)
- [ ] **"Back" / "Cancel" button:** at every step of a multi-step flow there is a way to cancel
- [ ] **No dead end:** user can always exit the current state (main menu, cancel, /start)
- [ ] **Reply keyboard removed** when not needed: after flow completion remove with `ReplyKeyboardRemove`

### Callback and Feedback
- [ ] **`answerCallbackQuery`** is called for every `callback_query`. Otherwise the "clock" spins for 30 seconds
- [ ] **Feedback on action:** user sees the result ("Saved ✅", "Error: invalid email")
- [ ] **Loading indicators:** long operations (>1 sec) show `sendChatAction: typing` or `upload_document`
- [ ] **Error handling:** if action fails — user gets a clear message, not silence

### Flow and States
- [ ] **No orphaned states:** user deleted the bot / pressed /start — old state is cleared or overwritten
- [ ] **Unexpected input handling:** user sent text instead of pressing a button → clear hint, not ignore
- [ ] **Timeout on wait:** in state "enter email" don't wait forever. After N minutes — reset with explanation
- [ ] **Idempotency:** repeated button press doesn't create a duplicate order/request
- [ ] **Deep linking:** `start` parameters (`/start ref_123`) are parsed and handled correctly

### Security and Protection
- [ ] **Don't expose internal ID:** callback_data doesn't pass raw database IDs (predictable). Use hashes or UUID
- [ ] **Rate limiting:** bot doesn't spam the user (>30 messages/sec in one chat — Telegram limit)
- [ ] **Permission check:** admin commands (`/admin`, `/stats`) check `chat.id` or `user.id` against a whitelist

## Evidence Requirements

Every finding MUST include:
1. **Bot command / handler:** exact command or callback_data
2. **Code quote:** 3-5 lines from handler
3. **What the user sees:** exact message text or behavior description
4. **Reproduction steps:** how to reproduce (press button X, enter text Y)
5. **Why this is a problem:** reference to a rule from the list above

**NEVER report:**
- "Flow is bad" without a specific dead end and reproduction steps
- "Text is unclear" without a text quote and explanation of what exactly is unclear
- Problems that you cannot confirm with code or behavior description

## Finding Schema

```text
ID
Severity: BLOCKER | CRITICAL | MAJOR | MINOR
Confidence: CONFIRMED | NEEDS_REVIEW
Category / Control
Evidence: file:line, command output, trace or reproduction
Impact
Recommended action
Owner / disposition
```

## Severity and Confidence

Severity describes impact and urgency:

| Severity | Meaning |
|----------|---------|
| **BLOCKER** | Change/release must not proceed; immediate action required |
| **CRITICAL** | High impact; fix in the current iteration |
| **MAJOR** | Degradation or defect; schedule the fix |
| **MINOR** | Improvement; backlog |

Skill-specific calibration:
- **BLOCKER** — user cannot complete an action (dead end with no exit, callback without `answerCallbackQuery`, duplicates on repeated press)
- **CRITICAL** — core flow technically completes but loses data or traps state (orphaned state on bot block, timeout without reset on a payment flow)
- **MAJOR** — confusion, data loss, unclear error (text > 4096, orphaned state, timeout without reset)
- **MINOR** — inconvenience, extra click, illogical label

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Proven by evidence: file:line, reproduction, command output |
| **NEEDS_REVIEW** | Investigation signal; requires human judgment before action |

Skill-specific calibration:
- **CONFIRMED** — specific bug found: text > 4096, callback without answer, orphaned state, dead end
- **NEEDS_REVIEW** — subjective assessment: "clarity" of text, "logic" of flow. Requires human judgment.

## Outputs and Downstream Consumer

Report format:

```markdown
## Bot Audit — {date}

### BLOCKER
- [ ] [CONFIRMED] Dead end: after selecting a category there is no "Back" button or /start doesn't reset state
  → Handler: `CategorySelectedHandler.cs:42`
  → Code: `await bot.SendMessage(chatId, "Choose subcategory", keyboard);` — no cancel button
  → Repro: press /create_order → select category → get stuck
  → Fix: add "Cancel" button with callback_data `cancel` and handler `OnCancel`

- [ ] [CONFIRMED] Callback without answerCallbackQuery: when pressing "Save" the clock spins for 30 seconds
  → Handler: `SaveOrderHandler.cs:15`
  → Code: `await orderService.Save(order);` — no `await bot.AnswerCallbackQuery(...)`
  → Fix: add `await bot.AnswerCallbackQuery(callbackQueryId, "Saved ✅")`

### MAJOR
- [ ] [CONFIRMED] Confirmation text is 4200 characters (limit 4096), message is not sent
  → `src/Bot/Messages/OrderConfirmation.cs:15`
  → Code: `var text = $"Order #{order.Id}..."` (4200 chars)
  → Fix: shorten or split into 2 messages

- [ ] [CONFIRMED] Orphaned state: user deleted the bot, state remains in DB
  → `src/Bot/StateRepository.cs` — no cleanup on `MyChatMemberUpdated` (user blocked the bot)
  → Fix: subscribe to `Update.MyChatMember` and delete state when `Status = Kicked`

- [ ] [NEEDS_REVIEW] Button label "btn_save" instead of "Save ✅"
  → `src/Bot/Keyboards/OrderKeyboard.cs:8`
  → Code: `new InlineKeyboardButton("btn_save", callbackData: "save")`
  → Fix: `"Save ✅"` + localization

### MINOR
- [ ] [NEEDS_REVIEW] No typing indicator when generating a report (5-10 sec)
  → `src/Bot/Handlers/ReportHandler.cs`
  → Fix: `await bot.SendChatAction(chatId, ChatAction.Typing)` before long operation
```

Consumers:
- **Input from:** Code Review Agent (diff with bot handlers), Task Compliance Agent (feature scope)
- **Output to:** Programmer Agent (flow/text fixes), Human supervisor (NEEDS_REVIEW findings)

## Trigger or Schedule

Runs when changes touch bot handlers, keyboards, messages, or the state machine.

## Limitations and Expected False Positives

- This skill only checks Telegram bots. For other platforms — adapt or create a new skill.
- Does not check bot business logic (correctness of calculations) — only UX and flow.
- Does not check visual design (bots don't have it).
- Label "clarity" and flow "logic" findings are subjective and expected as
  NEEDS_REVIEW signals, not defects.
