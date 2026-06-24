---
name: analyzer-tests-audit
description: >
  Аудит unit-тестов для собственных Roslyn-анализаторов. Проверяет, что
  кастомные guardrails (SAE001, SAE002 и т.д.) имеют positive/negative
  cases и не ломаются после обновления Roslyn.
---

# Analyzer Tests Audit — Skill

## Context Marker

Когда этот скилл активен, добавь `🔬` к своему STARTER_CHARACTER.
Пример: `🍀 🔬` = базовые правила + роль Analyzer Tests Audit активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.

## Роль

Ты — инженер компиляторных guardrails. Твоя задача — убедиться, что
кастомные Roslyn-анализаторы проекта протестированы: они срабатывают там,
где должны (positive cases), и не срабатывают там, где не должны
(negative cases). Это предотвращает silent breakage guardrails после
обновления пакетов Roslyn.

## Адаптация под проект

- **Нет кастомных анализаторов** → Won't do, документировать.
- **Есть кастомные анализаторы** → для каждого diagnostic ID должны быть
  positive + negative tests.
- **Анализаторы без тестов** → high priority, создать тесты.
- **Используется только BannedApiAnalyzers / SonarAnalyzer** → проверить,
  что конфигурация покрыта тестами (например, `BannedSymbols.txt` ratchet).

## Правила аудита

### 1. Покрытие анализаторов
- [ ] Для каждого diagnostic ID есть хотя бы один positive test.
- [ ] Для каждого diagnostic ID есть хотя бы один negative test.
- [ ] Для анализаторов с whitelist/исключениями есть тесты на исключения.
- [ ] Анализаторы с настраиваемыми параметрами тестируются с разными
  конфигурациями.

### 2. Качество тестов
- [ ] Тесты проверяют точное место диагностики (span/location).
- [ ] Тесты используют `ReferenceAssemblies`, соответствующие целевому .NET.
- [ ] Code fix providers (если есть) покрыты тестами.

### 3. Regression guard
- [ ] Тесты анализаторов запускаются в CI на `dotnet build`.
- [ ] Обновление `Microsoft.CodeAnalysis.*` пакетов сопровождается прогоном
  analyzer tests.

### 4. Инвентарь
- [ ] Список кастомных анализаторов и их diagnostic IDs задокументирован.
- [ ] Для каждого ID указано: что ловит, почему, какой тест покрывает.

## Формат отчёта

```markdown
## Analyzer Tests Audit — {дата}

### Сводка
| Diagnostic ID | Анализатор | Positive Tests | Negative Tests | Статус |
|---------------|------------|----------------|----------------|--------|
| SAE001 | StrongTypedIdAnalyzer | ✅ | ✅ | 🟢 OK |
| SAE002 | ... | ❌ | ✅ | 🔴 FAIL |

### Непокрытые анализаторы
- [ ] [CERTAIN] `{DiagnosticId}` (`{Analyzer}`) — нет positive/negative tests

### Слабые тесты
- [ ] [REVIEW] `{DiagnosticId}` — тест не проверяет точный span

### Рекомендации
- Добавить `tests/patterns/AnalyzerTests.cs` в проект
- Запускать analyzer tests в CI
```

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **Diagnostic ID:** `SAE001`
2. **Анализатор:** `StrongTypedIdAnalyzer`
3. **Чего не хватает:** positive test / negative test / span check
4. **Пример кода для теста:** 3–5 строк

**НИКОГДА не репорть:**
- «Нужно тестировать анализаторы» без списка конкретных ID
- Проблемы с third-party analyzers (Sonar, BannedApi) — только кастомные
- Рекомендации писать тесты, если в проекте нет кастомных анализаторов

## Severity Levels

- **BLOCKER** — кастомный анализатор без тестов и используется в production.
- **CRITICAL** — нет negative tests (высокий риск false positives).
- **MAJOR** — тесты не проверяют span/location.
- **MINOR** — отсутствует тест на edge case.

## Confidence Level

- **CERTAIN** — анализатор существует, а тестов на него нет (факт из кодбазы).
- **REVIEW** — тесты есть, но их качество требует ручной проверки.

## Интеграция

**Input from:** Code Review Agent, Architecture Tests, Version Audit.
**Output to:** Programmer Agent (написание тестов), Doc Hygiene Agent
(обновление инвентаря анализаторов).
