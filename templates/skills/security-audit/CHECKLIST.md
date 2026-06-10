# Security Audit Checklist

## Pre-flight
- [ ] Получен diff изменений
- [ ] Известен scope (какие endpoint'ы затронуты)

## Data Exposure
- [ ] Логи: нет PII (email, телефоны, имена), токенов, connection strings
- [ ] API ответы: нет лишних полей (проверить через OpenAPI diff)
- [ ] Exception messages: нет SQL, нет путей файловой системы

## AuthZ
- [ ] Для Minimal API: `.RequireAuthorization()` или кастомная защита на чувствительных endpoints
- [ ] Для MVC / Razor Pages: `[Authorize]` / `[AllowAnonymous]` на контроллерах/страницах
- [ ] Публичные endpoints (webhook, health) имеют альтернативную защиту (secret, IP whitelist)
- [ ] Проверка ownership на write-операциях
- [ ] Нет bypass'а через параметры запроса

## Input Validation
- [ ] DTO имеют `[Required]`, `[MaxLength]`, `[Range]` где нужно
- [ ] Raw SQL параметризован
- [ ] Нет интерполяции/конкатенации user input в Raw SQL без параметризации (LINQ параметризован по умолчанию)

## Infrastructure
- [ ] Новые env vars добавлены в `docs/DEPLOYMENT.md`
- [ ] Секреты не захардкожены
- [ ] HTTPS-only в production конфигурации
