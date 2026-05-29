# Security Audit Checklist

## Pre-flight
- [ ] Diff obtained
- [ ] Scope known (which endpoints are affected)

## Data Exposure
- [ ] Logs: no PII, tokens, connection strings
- [ ] API responses: no extra fields (check via OpenAPI diff)
- [ ] Exception messages: no SQL, no file system paths

## AuthZ
- [ ] For Minimal API: `.RequireAuthorization()` or custom protection on sensitive endpoints
- [ ] For MVC / Razor Pages: `[Authorize]` / `[AllowAnonymous]` on controllers/pages
- [ ] Public endpoints (webhook, health) have alternative protection (secret, IP whitelist)
- [ ] Ownership check on write operations
- [ ] No bypass via request parameters

## Input Validation
- [ ] DTOs have `[Required]`, `[MaxLength]`, `[Range]` where needed
- [ ] Raw SQL is parameterized
- [ ] No direct use of `user input` in LINQ without validation

## Infrastructure
- [ ] New env vars added to `docs/DEPLOYMENT.md`
- [ ] Secrets not hardcoded
- [ ] HTTPS-only in production configuration
