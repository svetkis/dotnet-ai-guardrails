# API Design Audit — Checklist

## Adaptation
- [ ] API type defined (REST / gRPC / GraphQL / Webhook)
- [ ] Inapplicable sections marked N/A

## HTTP Statuses and Errors
- [ ] Statuses are correct: 400/401/403/404/409/422/429/500
- [ ] 500 — never with stack trace or SQL
- [ ] Error response — structured body (ProblemDetails / RFC 7807)
- [ ] `message` is user-friendly, contains no internal details
- [ ] `code` / `type` present for programmatic handling
- [ ] Validation errors bound to fields (`fieldErrors`)
- [ ] No PII leak in error messages

## Pagination and Sorting
- [ ] Default `page`/`limit` or `cursor` is set
- [ ] No duplication or skipped records when paging
- [ ] `limit` has a max cap
- [ ] Offset-based: has `total` + `page` + `limit`
- [ ] Cursor-based: has `nextCursor` / `hasMore`
- [ ] Default sort is stable (not UUID without secondary key)
- [ ] `sort` / `orderBy` supports whitelist
- [ ] Sort direction is explicit: `asc` / `desc`
- [ ] Composite sort is documented

## API Contracts and DTOs
- [ ] New endpoints have descriptions in OpenAPI (summary, description)
- [ ] Parameters have `example` and `schema`
- [ ] Response schemas described for all statuses
- [ ] Breaking changes detected (field removal, type change)
- [ ] API returns DTOs/records, not Entities
- [ ] Naming is consistent (`OrderResponse`, `CreateOrderRequest`)
- [ ] `id` field is consistent by type
- [ ] Dates in ISO 8601 with timezone
- [ ] Enums in JSON are strings, not numbers
- [ ] Breaking changes are versioned
- [ ] Deprecated endpoints marked in OpenAPI with an alternative

## Rate Limiting and Protection
- [ ] Rate limiting on public endpoints
- [ ] Headers `X-RateLimit-*` / `Retry-After` in 429
- [ ] Bulk endpoints have separate limits
- [ ] CORS policy is not `*` in production for sensitive endpoints

## Empty and Error States
- [ ] Empty list → 200 + `[]`, not 404
- [ ] Empty search → 200 + empty array
- [ ] On 500 — ProblemDetails with `traceId`, without stack trace

## Quality Gates
- [ ] Every finding includes: endpoint, code quote, actual response, expected response
- [ ] No BLOCKER without a specific request/response example
- [ ] REVIEW findings marked as requiring human judgment
