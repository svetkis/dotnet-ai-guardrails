# API Design Audit — Чеклист

## Адаптация
- [ ] Определён тип API (REST / gRPC / GraphQL / Webhook)
- [ ] Неприменимые разделы помечены N/A

## HTTP статусы и ошибки
- [ ] Статусы корректны: 400/401/403/404/409/422/429/500
- [ ] 500 — никогда со stack trace или SQL
- [ ] Error response — структурированное тело (ProblemDetails / RFC 7807)
- [ ] `message` понятен пользователю, не содержит internal details
- [ ] `code` / `type` присутствует для programmatic handling
- [ ] Валидационные ошибки привязаны к полям (`fieldErrors`)
- [ ] Нет утечки PII в сообщениях об ошибках

## Пагинация и сортировка
- [ ] Default `page`/`limit` или `cursor` заданы
- [ ] Нет дублирования или пропуска записей при перелистывании
- [ ] `limit` имеет max cap
- [ ] Offset-based: есть `total` + `page` + `limit`
- [ ] Cursor-based: есть `nextCursor` / `hasMore`
- [ ] Сортировка по умолчанию стабильна (не UUID без второго ключа)
- [ ] `sort` / `orderBy` поддерживает whitelist
- [ ] Направление сортировки явное: `asc` / `desc`
- [ ] Составная сортировка документирована

## API контракты и DTO
- [ ] Новые endpoint'ы имеют описание в OpenAPI (summary, description)
- [ ] Параметры имеют `example` и `schema`
- [ ] Response schemas описаны для всех статусов
- [ ] Breaking changes обнаружены (удаление поля, смена типа)
- [ ] API возвращает DTO/records, не Entities
- [ ] Naming консистентно (`OrderResponse`, `CreateOrderRequest`)
- [ ] Поле `id` консистентно по типу
- [ ] Даты в ISO 8601 с timezone
- [ ] Enum'ы в JSON — строки, не числа
- [ ] Breaking changes версионированы
- [ ] Deprecated endpoint'ы помечены в OpenAPI с альтернативой

## Rate limiting и защита
- [ ] Rate limiting на публичных endpoints
- [ ] Заголовки `X-RateLimit-*` / `Retry-After` в 429
- [ ] Bulk endpoints имеют отдельные лимиты
- [ ] CORS policy не `*` в production для sensitive endpoints

## Empty и error states
- [ ] Пустой список → 200 + `[]`, не 404
- [ ] Пустой поиск → 200 + пустой массив
- [ ] При 500 — ProblemDetails с `traceId`, без stack trace

## Quality Gates
- [ ] Каждая находка включает: endpoint, цитату кода, actual response, expected response
- [ ] Нет BLOCKER без конкретного примера запроса/ответа
- [ ] REVIEW-находки помечены как требующие human judgment
