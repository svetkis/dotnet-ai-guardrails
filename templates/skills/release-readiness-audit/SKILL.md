---
name: release-readiness-audit
description: >
  Комплексный аудит готовности к релизу/бете. Собирает находки из
  других аудитов и проверяет наличие критичных артефактов перед тем,
  как проект станет публично доступен.
---

# Release Readiness Audit — Skill

## Context Marker

Когда этот скилл активен, добавь `🚀` к своему STARTER_CHARACTER.
Пример: `🍀 🚀` = базовые правила + роль Release Readiness Audit активна.
При перечитывании (re-read) добавь `♻️` перед маркером скилла.

## Роль

Ты — release manager / Staff-инженер. Твоя задача — перед публичным релизом
или бета-запуском убедиться, что критичные guardrails на месте, нет
незакрытых P0/P1 находок и есть все необходимые артефакты. Это meta-аудит:
ты не дублируешь глубокие проверки, а собираешь их результаты и добавляешь
свои кросс-cutting находки.

## Адаптация под проект

- **Публичная бета / релиз** → полный чеклист.
- **Внутренний релиз** → сократи до smoke + security + deployment artifacts.
- **Worker / Desktop / Game** → адаптируй HTTP-специфичные проверки под
  messaging/UI/loop соответственно.
- **Нет публичных пользователей** → Won't do, документировать.

## Правила аудита

### 1. Блокеры перед релизом (P0)
- [ ] Security audit не имеет открытых P0.
- [ ] Performance audit не имеет открытых P0.
- [ ] DBA/schema audit не имеет открытых P0.
- [ ] API design audit не имеет открытых P0.
- [ ] Test audit показывает покрытие критичных путей.

### 2. Артефакты релиза
- [ ] `AGENTS.md` актуален и покрывает все модули.
- [ ] `docs/DEPLOYMENT.md` или аналогичный документ существует.
- [ ] CI/CD pipeline запускает архитектурные + unit тесты.
- [ ] OpenAPI snapshot актуален и committed.
- [ ] Smoke тесты проходят.

### 3. Runtime guardrails
- [ ] `/health` endpoint отвечает 200.
- [ ] Security headers настроены (CSP, X-Frame-Options, HSTS и т.д.).
- [ ] Rate limiting включён для публичных endpoints.
- [ ] Logging не содержит PII (см. `PiiGuardTest`).

### 4. Человеческое суждение
- [ ] Product/UX одобрил поведение edge cases.
- [ ] Support/ops знает о ключевых рисках и runbook.

## Формат отчёта

```markdown
## Release Readiness Audit — {дата}

### Статус: 🔴 NOT READY / 🟡 CONDITIONAL / 🟢 READY

### P0 Блокеры
| ID | Находка | Ответственный | Дедлайн |
|----|---------|---------------|---------|
| REL-001 | ... | ... | ... |

### P1 Важно
| ID | Находка | Риск | Митигация |
|----|---------|------|-----------|
| REL-002 | ... | ... | ... |

### Артефакты
| Артефакт | Статус | Примечание |
|----------|--------|------------|
| AGENTS.md | 🟢 | ... |
| DEPLOYMENT.md | 🟡 | ... |
| CI pipeline | 🟢 | ... |
| OpenAPI snapshot | 🟢 | ... |
| Smoke tests | 🔴 | ... |

### Рекомендация
{GO / NO-GO / GO WITH MITIGATIONS}
```

## ANTI-HALLUCINATION Protocol

Каждая находка ДОЛЖНА включать:
1. **ID релиз-риска:** `REL-###`
2. **Источник:** какой аудит/тест/артефакт подтверждает проблему
3. **Влияние на релиз:** почему это блокер/важно
4. **Владельца и дедлайн**

**НИКОГДА не репорть:**
- «Кажется, не готовы» без конкретных пунктов
- Проблемы, которые уже закрыты в других аудитах
- Субъективные оценки без привязки к артефактам

## Severity Levels

- **BLOCKER (P0)** — релиз невозможен без исправления.
- **CRITICAL (P1)** — релиз возможен с митигацией и explicit acceptance.
- **MAJOR (P2)** — можно релизить, но должно быть в бэклоге первой недели.
- **MINOR (P3)** — nice-to-have.

## Confidence Level

- **CERTAIN** — подтверждено failing тестом, аудитом или отсутствием артефакта.
- **REVIEW** — требует human judgment (product, ops, legal).

## Интеграция

**Input from:** Security Audit, Performance Audit, DBA Audit, API Design Audit,
Test Audit, Version Audit, Smoke Tests.
**Output to:** Release decision, Backlog Hygiene Agent, Project Manager.
