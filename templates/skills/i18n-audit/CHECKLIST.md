# i18n Audit — Checklist

## Adaptation
- [ ] Localization format defined (.resx / .json / i18next / bot)
- [ ] Project languages defined (ru, en, ar, etc.)
- [ ] Inapplicable sections marked N/A

## Key Synchronization
- [ ] All keys from the base language are present in other languages
- [ ] JSON locale structures match recursively
- [ ] No empty values `""` or `null` in translations

## Hardcoded Strings
- [ ] Backend (.cs): no Cyrillic / common phrases outside `Resources/` and `IStringLocalizer`
- [ ] Frontend (.tsx/.jsx/.vue): no text nodes outside `t()` / `formatMessage()` / `$t()`
- [ ] Bot: no hardcoded strings in `SendMessage` / `answer` / `editMessageText`
- [ ] Exceptions verified: logs, exception types, comments, unit-tests, technical constants

## RTL
- [ ] No `direction: ltr`, `text-align: left`
- [ ] Tailwind uses `ms-` / `me-`, not `ml-` / `mr-`
- [ ] HTML elements with text have `dir="auto"` or correct `dir`

## Formats and Pluralization
- [ ] Dates formatted via i18n / `CultureInfo`
- [ ] Numbers and currencies formatted via i18n
- [ ] Pluralization handled (1 record / 2 records / 5 records)
- [ ] No string concatenation for pluralization (`"${count} records"`)

## Edge cases
- [ ] Long strings don't break layout
- [ ] Short strings don't look odd
- [ ] Special characters are escaped correctly

## Quality Gates
- [ ] Every finding includes: file, line, hardcoded quote, context (UI/log/exception)
- [ ] No MAJOR without an exact string quote from code
- [ ] NEEDS_REVIEW findings marked as requiring human judgment
- [ ] Technical strings (logs, exceptions) excluded from report
