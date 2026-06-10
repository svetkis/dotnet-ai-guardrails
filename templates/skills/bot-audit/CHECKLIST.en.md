# Bot Audit — Checklist

## Adaptation
- [ ] Platform defined (Telegram / Discord / Slack / VK)
- [ ] Framework defined (Telegram.Bot / aiogram / telegraf / node-telegram-bot-api)
- [ ] Inapplicable sections marked N/A

## Texts and Markup
- [ ] Message length ≤ 4096 characters
- [ ] `callback_data` ≤ 64 bytes, does not contain JSON or long IDs
- [ ] Markdown/HTML special characters are escaped
- [ ] No unclosed HTML tags
- [ ] `disable_web_page_preview` used where needed

## Buttons and Navigation
- [ ] Labels are clear (not technical IDs)
- [ ] Inline keyboard ≤ 100 buttons
- [ ] "Back" / "Cancel" button exists on multi-step flows
- [ ] No dead end
- [ ] Reply keyboard removed after flow completion (`ReplyKeyboardRemove`)

## Callback and Feedback
- [ ] Every `callback_query` gets `answerCallbackQuery`
- [ ] User sees feedback on action
- [ ] Long operations show `sendChatAction`
- [ ] Errors are handled: user gets a message, not silence

## Flow and States
- [ ] No orphaned states (cleanup on bot deletion / block)
- [ ] Unexpected input handling (text instead of button)
- [ ] Timeout on input wait with explanation
- [ ] Idempotency: repeated press doesn't create a duplicate
- [ ] Deep linking (`/start ref_123`) is handled correctly

## Security
- [ ] No raw database IDs in `callback_data`
- [ ] Rate limiting: no more than 30 messages/sec in a chat
- [ ] Admin commands check whitelist by `chat.id` / `user.id`

## Quality Gates
- [ ] Every finding includes: command/handler, code quote, what the user sees, reproduction steps
- [ ] No BLOCKER without a specific reproduction example
- [ ] REVIEW findings marked as requiring human judgment
