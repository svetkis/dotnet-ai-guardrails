# Security Audit Checklist

## Pre-flight
- [ ] Получен diff изменений
- [ ] Известен scope (какие endpoint'ы затронуты)

## Data Exposure
- [ ] Логи: нет PII, токенов, connection strings
- [ ] API ответы: нет лишних полей (проверить через OpenAPI diff)
- [ ] Exception messages: нет SQL, нет путей файловой системы

## AuthZ
- [ ] Все новые endpoints покрыты авторизацией
- [ ] Проверка ownership на write-операциях
- [ ] Нет bypass'а через параметры запроса

## Input Validation
- [ ] DTO имеют `[Required]`, `[MaxLength]`, `[Range]` где нужно
- [ ] Raw SQL параметризован
- [ ] Нет прямого использования `user input` в LINQ без валидации

## Infrastructure
- [ ] Новые env vars добавлены в `docs/DEPLOYMENT.md`
- [ ] Секреты не захардкожены
- [ ] HTTPS-only в production конфигурации
