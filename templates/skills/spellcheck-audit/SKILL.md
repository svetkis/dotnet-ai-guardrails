---
name: spellcheck-audit
description: >
  Project-wide проверка орфографии через CSpell. Ловит опечатки в
  публичных API-именах, DTO, комментариях, markdown-документации и
  конфигурации, прежде чем они закрепятся в контрактах.
---

# Spellcheck Audit — Skill

## Context Marker

Когда этот скилл активен, добавь `🔤` к своему STARTER_CHARACTER.
Пример: `🍀 🔤` = базовые правила + роль Spellcheck Audit активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.

## Роль

Ты — редактор/tech writer. Твоя задача — найти опечатки в текстах проекта:
именах публичных типов, properties, API endpoints, markdown-документации,
комментариях и конфигурационных файлах. Особое внимание — к публичным
символам, потому что опечатка в API-имени становится обратно
несовместимой после релиза.

## Адаптация под проект

- **Публичный API / OpenAPI** → проверяй имена типов, properties, enum values.
- **Только внутренний проект** → фокус на markdown/docs и комментариях.
- **Много технических терминов** → создай project dictionary (`cspell.json` +
  `.cspell/project-words.txt`).
- **Нет CSpell** → предложить внедрить как pre-commit / CI guardrail.

## Правила аудита

### 1. Конфигурация
- [ ] В корне проекта есть `cspell.json`.
- [ ] Есть project dictionary с техническими терминами.
- [ ] CSpell запускается в CI или pre-commit для изменённых файлов.

### 2. Что проверять
- [ ] Публичные имена типов, properties, enum values в `.cs`.
- [ ] Markdown документация (`docs/`, `README.md`).
- [ ] Комментарии к public API и `/// <summary>`.
- [ ] Конфигурационные файлы (`appsettings*.json`, `.yml`, `.yaml`).
- [ ] OpenAPI/JSON контракты.

### 3. Baseline ratchet
- [ ] Зафиксировано текущее количество опечаток.
- [ ] `SpellcheckGuardTest` падает, если появились новые нарушения.
- [ ] Новые технические термины добавляются в dictionary, а не suppress.

### 4. Приоритеты
- [ ] Публичные API-имена — BLOCKER (нельзя исправить без breaking change).
- [ ] Документация — CRITICAL/MAJOR.
- [ ] Комментарии — MINOR.

## Формат отчёта

```markdown
## Spellcheck Audit — {дата}

### Сводка
| Категория | Нарушений | Новых | Исправлено |
|-----------|-----------|-------|------------|
| Public API names | {N} | {N} | {N} |
| Markdown/docs | {N} | {N} | {N} |
| Comments | {N} | {N} | {N} |

### Public API опечатки (BLOCKER)
- [ ] [CERTAIN] `{File}:{Line}` — `{Symbol}`: "{misspelled}" → "{correct}"

### Документация и комментарии
- [ ] [CERTAIN|REVIEW] `{File}:{Line}` — "{misspelled}" → "{correct}"

### Новые слова в словарь
- [ ] `{term}` — добавить в `cspell.json` / project dictionary
```

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **Точный файл и строку:** `src/Application/DTOs/OrderResponse.cs:14`
2. **Цитату кода:** `public string RecieveNotificationEmail { get; set; }`
3. **Опечатку и исправление:** `Recieve` → `Receive`
4. **Категорию:** public API / docs / comment

**НИКОГДА не репорть:**
- «Опечатка где-то в документации» без места
- Имена собственные / торговые марки без проверки
- Слова, которые могут быть валидными терминами в домене (отмечай `[REVIEW]`)

## Severity Levels

- **BLOCKER** — опечатка в публичном API-имени (type/property/endpoint).
- **CRITICAL** — опечатка в пользовательской документации или UI-строках.
- **MAJOR** — опечатка в комментариях к критичному коду.
- **MINOR** — опечатка в internal комментариях.

## Confidence Level

- **CERTAIN** — слово явно не существует в английском языке и не в dictionary.
- **REVIEW** — технический термин, аббревиатура или доменное слово. Требует
  проверки human'ом перед добавлением в dictionary.

## Интеграция

**Input from:** Code Review Agent, API Design Audit, i18n Audit.
**Output to:** Backlog Hygiene Agent, Programmer Agent (исправление имён),
Doc Hygiene Agent (обновление dictionary).
