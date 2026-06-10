# Skill Adaptation Guide

> Copying skills is easy. Adapting them is important.
> This guide will help you strike out inapplicable checks before the first run.

---

## Quick Checklist (3 steps)

1. **Define the stack**
   - .NET version, app type, ORM, architecture, test framework
   - Check `.csproj`, `global.json`, folder structure

2. **Strike out what doesn't apply**
   - For each skill, check the table below
   - Mark items N/A in CHECKLIST.md before running

3. **Run and filter**
   - After the first run, review findings marked `[REVIEW]`
   - If a finding is a false positive, add a condition to the project's SKILL.md

---

## Table: if the project has… → skip these checks

| If the project has… | Skip in skill… | Why |
|---|---|---|
| No Clean Architecture (single-project MVP) | `code-review` → layer checks (NetArchTest) | No Domain / Infrastructure projects — rule not applicable |
| Minimal API (not MVC) | `security-audit` → `[Authorize]` / `[AllowAnonymous]` | Minimal API uses `.RequireAuthorization()` or middleware |
| Minimal API | `code-review` → `[Authorize]` check | See above |
| Dapper / ADO.NET (no EF Core) | `performance-audit` → `.AsNoTracking()`, `.Include()`, `FindAsync()` | EF Core rules don't apply to raw SQL |
| Dapper / ADO.NET | `dba-audit` → migrations, `Include()`, `FindAsync()` | DBA audit for Dapper is a different skill |
| `.Select()` projections to DTO everywhere | `performance-audit`, `code-review` → missing `.AsNoTracking()` | EF Core doesn't track projections, AsNoTracking not needed |
| Raw SQL (`FromSqlRaw`, `ExecuteUpdateAsync`) | `code-review`, `performance-audit` → `.AsNoTracking()` on write-path | Change Tracker doesn't track raw SQL |
| Worker Service (no HTTP) | `security-audit` → rate limiting, XSS, `[Authorize]` | Worker has no endpoints in the classical sense |
| Worker Service | `code-review` → OpenAPI snapshot, API DTO | Worker doesn't return HTTP responses |
| .NET Framework 4.8 | All skills → NetArchTest, TUnit, Minimal API | Stack differs radically; use `skeptical-ai-bootstrap` |
| Razor Pages | `code-review` → Minimal API check | Razor Pages use PageModel, not endpoint routing |
| Vertical Slice Architecture | `code-review` → standard layer rules | Boundaries by feature, not by layer; use custom NetArchTest |

---

## Confidence Level: how to interpret

All audit skills since version 2026-06 mark findings with a confidence level:

| Marker | Meaning | Action |
|---|---|---|
| `[CERTAIN]` | Definitely a bug / vulnerability | Fix or create a task immediately |
| `[REVIEW]` | False positive possible | Check with a human before acting. Common reasons: projection without AsNoTracking, endpoint without `[Authorize]` but with middleware, single-project without Clean Architecture |

**Rule:** if the agent is not sure — it marks `[REVIEW]`. This is not weakness, this is honesty.

---

## Adaptation example

Project: **BetweenTheLines** (.NET 10, EF Core, PostgreSQL, Minimal API, single-project MVP)

Adaptations:
1. `code-review` — struck out: layer checks (no Clean Architecture). Added: `.RequireAuthorization()` instead of `[Authorize]`.
2. `performance-audit` — struck out: AsNoTracking on `.Select()` projections. Added: exception for `FromSqlRaw("UPDATE...")`.
3. `security-audit` — struck out: `[Authorize]` / `[AllowAnonymous]`. Added: check `.RequireAuthorization()` and webhook protection via secret token.

---

## If nothing fits

If ready-made skills require more than 50% adaptation — don't adapt, create a new one:

1. Run `skeptical-ai-bootstrap` — it provides a framework for creating a new skill
2. Use `SKILL-ARCHITECTURE.md` to design a Guardrail from scratch
3. Use `NEW-SKILL-TEMPLATE.md` to generate files for the new skill

See `templates/skills/skeptical-ai-bootstrap/SKILL-ARCHITECTURE.md`
