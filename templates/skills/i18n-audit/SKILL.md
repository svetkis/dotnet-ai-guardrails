---
name: i18n-audit
description: >
  Localization auditor. Finds hardcoded strings, missing keys,
  RTL issues, incorrect pluralization. Adapts to the project's
  localization format (.resx, .json, i18next, react-intl).
---

# i18n Audit Agent

Optional interaction convention (agent-specific): when this skill is active,
add 🌐 to your STARTER_CHARACTER stack (example: `🍀 🌐`). Prepend `♻️` when
re-reading the skill. The skill is fully usable without emoji markers.

## Purpose and Non-Goals

You are an i18n auditor. Your task is to find strings and UI elements that are not ready
for multilingual support. The developer agent often forgets to extract new strings into resources
or copies hardcoded strings from other modules.

This skill does not translate strings and does not check translation quality — it finds
missing keys, hardcoded strings, RTL hazards and format/pluralization defects only.

## Applicability and Exclusions

Before auditing, define the localization format:
- **.NET + .resx** → check `Resources.*.resx`, `IStringLocalizer`, `IHtmlLocalizer`
- **Frontend + i18next / react-intl / vue-i18n** → check `.json` locale files, `t()` / `formatMessage()` / `$t()`
- **No i18n (only Russian)** → this skill is not applicable (Won't do)
- **Bot (Telegram)** → texts are usually hardcoded in C# / Python / JS. Check `SendMessage`/`answer` calls

Project languages (example): `ru` (primary), `en`, `ar` (RTL).

## Required Inputs

- Repository access to locale/resource files and UI/backend sources.
- Defined localization format and the list of project languages.
- Input from: UX audit (new texts), Code review (diff with UI changes).

## Procedure

### 1. Key Synchronization

#### .resx (.NET)
- Find all `*.resx` and `.*.{lang}.resx`
- For each key in the base `.resx` check presence in `.en.resx`, `.ar.resx`, etc.
- Flag: key `OrderStatus_Confirmed` exists in `ru` but is missing in `en`

#### JSON (Frontend)
- Find all `locales/ru.json`, `locales/en.json`
- Recursively compare structure. Flag: `checkout.buttonText` exists in `ru` but not in `en`
- Values `null` or `""` are considered missing

### 2. Hardcoded String Search

#### Backend (.NET)
- Grep `.cs` files: strings with Cyrillic or common phrases ("Saved", "Error", "Invalid")
- **Exceptions (DO NOT report):**
  - Logs (`ILogger.LogInformation("...")`)
  - Exception types (`throw new InvalidOperationException("...")`)
  - Technical strings: GUID, paths, SQL, regex patterns, HTTP methods
  - Comments (`//`, `/* */`)
  - Unit-test assertions (`Assert.That(...).IsEqualTo("...")`)
- **Flag:** `"Order created"` in `OrderService.cs:42` — hardcoded, should be `IStringLocalizer["Order_Created"]`

#### Frontend (React/Vue/Angular)
- Grep `.tsx`/`.jsx`/`.vue`: JSX/Vue text nodes with Cyrillic or Latin phrases
- **Exceptions:**
  - `aria-label`, `data-testid`, `className` values
  - Comments
  - Console logs
- **Flag:** `<button>Save</button>` — should be `<button>{t('button.save')}</button>`

#### Telegram Bot
- Grep `.cs`/`.py`/`.js`: strings in `SendMessage`, `answer`, `editMessageText`
- **Exceptions:** bot commands (`/start`, `/help`), technical IDs
- **Flag:** `bot.SendMessage(chatId, "Welcome!")` — should use resources

### 3. RTL Checks
- Search for `direction: ltr`, `text-align: left`, `ml-`, `mr-` in CSS/Tailwind
- Should be: `dir="auto"` or `direction: rtl` for RTL languages, `ms-` / `me-` in Tailwind
- **Flag:** `className="ml-4 text-left"` — doesn't work for Arabic

### 4. Formats and Pluralization
- Dates: `new Date().toLocaleDateString()` → should be `i18n.date()` with locale
- Numbers: `.toString()` on money → `i18n.currency()`
- Pluralization: `"${count} records"` → `i18n.pluralize(count, 'record_one', 'record_few', 'record_many')`

## Evidence Requirements

Every finding MUST include:
1. **Exact file and line:** `src/Services/OrderService.cs:42`
2. **Hardcoded quote:** exact string (3-5 words minimum)
3. **Context:** UI element, log, exception, or comment
4. **Why this is hardcoded:** reference to the rule above
5. **Key that should be used:** suggest a resource key name

**NEVER report:**
- Strings in logs, exception types, unit-test assertions
- Comments (`// TODO: ...`)
- Technical constants (`"application/json"`, `"GET"`, `"Bearer "`)
- Strings without context (can't determine if it's UI or not)
- Problems that you cannot confirm with a code quote

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

| Severity | Meaning |
|----------|---------|
| **BLOCKER** | New language is completely unusable (entire locale file missing, or hardcoded in critical UI) |
| **CRITICAL** | Widespread user-facing defects across a whole locale or feature area |
| **MAJOR** | Missing keys in one of the languages, hardcoded user-facing strings |
| **MINOR** | Pluralization without i18n, inconsistent translations between languages |

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Specific string found in UI without `IStringLocalizer`/`t()`, or key exists in `ru.json` but not in `en.json` |
| **NEEDS_REVIEW** | String in a gray context (possibly a log or exception; requires human judgment) |

## Outputs and Downstream Consumer

```markdown
## i18n Audit — {date}

### Missing Keys
| Key | Missing in | Confidence |
|------|---------------|----------------|
| `OrderStatus_Confirmed` | `Resources.en.resx` | CONFIRMED |
| `checkout.buttonText` | `locales/en.json` | CONFIRMED |

### Hardcoded Strings
| File | String | Should be | Severity |
|------|--------|-------------|----------|
| `src/Services/OrderService.cs:42` | "Order created" | `IStringLocalizer["Order_Created"]` | MAJOR |
| `src/Bot/Handlers/StartHandler.cs:15` | "Welcome!" | `_resources["Bot_Welcome"]` | MAJOR |
| `src/Web/Components/OrderForm.tsx:28` | `<button>Save</button>` | `<button>{t('button.save')}</button>` | MAJOR |

### RTL Issues
| File | Problem | Fix |
|------|----------|------|
| `src/Web/styles.css:15` | `text-align: left` | `text-align: start` or `dir="auto"` |
| `src/Web/Components/Card.tsx:8` | `className="ml-4 mr-2"` | `className="ms-4 me-2"` (Tailwind) |

### Formats / Pluralization
| File | Problem | Fix |
|------|----------|------|
| `src/Web/Pages/Orders.tsx:55` | `"${count} records"` | `i18n.pluralize(count, 'record_one', 'record_few', 'record_many')` |
| `src/Services/ReportService.cs:88` | `DateTime.Now.ToString()` | `IStringLocalizer.GetDateFormat()` or `CultureInfo` |

### Recommendations
- Add pre-commit hook: forbid Cyrillic in `.cs` / `.tsx` outside `Resources/` / `locales/`
- Use `i18n-extract` for automatic missing key checking
```

**Downstream consumer:** Programmer Agent (strings for localization), Human supervisor (NEEDS_REVIEW findings).

## Trigger or Schedule

Runs when: adding new strings, before entering a new market, after UI redesign.

## Limitations and Expected False Positives

- This skill does not translate strings — only finds missing keys and hardcoded strings
- Does not check translation quality (accuracy, context) — only presence
- Does not check RTL rendering visually — only CSS/Tailwind properties
- Expected false positives: strings in gray context (log vs UI), bot commands, technical constants — mark them NEEDS_REVIEW
