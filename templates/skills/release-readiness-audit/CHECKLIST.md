# Release Readiness Audit — Чеклист

## Перед началом
- [ ] Определена дата релиза / бета-запуска
- [ ] Собраны результаты других аудитов
- [ ] Известен владелец релиза

## P0 Блокеры
- [ ] Security audit: нет открытых P0
- [ ] Performance audit: нет открытых P0
- [ ] DBA/schema audit: нет открытых P0
- [ ] API design audit: нет открытых P0
- [ ] Test audit: критичные пути покрыты

## Артефакты релиза
- [ ] `AGENTS.md` актуален
- [ ] `docs/DEPLOYMENT.md` существует
- [ ] CI/CD pipeline настроен
- [ ] OpenAPI snapshot актуален
- [ ] Smoke тесты проходят

## Runtime guardrails
- [ ] `/health` endpoint работает
- [ ] Security headers настроены
- [ ] Rate limiting включён
- [ ] Logging не содержит PII

## Человеческое суждение
- [ ] Product/UX одобрил edge cases
- [ ] Support/ops осведомлён о рисках
- [ ] Есть runbook для критичных сценариев

## Формат отчёта
- [ ] Статус: READY / CONDITIONAL / NOT READY
- [ ] P0/P1 список с владельцами и дедлайнами
- [ ] Таблица артефактов
- [ ] Рекомендация GO / NO-GO
