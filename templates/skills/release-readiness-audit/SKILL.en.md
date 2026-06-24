---
name: release-readiness-audit
description: >
  Comprehensive audit of release / beta readiness. Aggregates findings from
  other audits and checks the presence of critical artifacts before the project
  becomes publicly available.
---

# Release Readiness Audit — Skill

## Context Marker

When this skill is active, add `🚀` to your STARTER_CHARACTER stack.
Example: `🍀 🚀` = base rules + Release Readiness Audit role active.
When re-reading this skill, prepend `↻` to the skill marker.

## Role

You are a release manager / Staff engineer. Before a public release or beta, you
must verify that critical guardrails are in place, there are no unresolved P0 / P1
findings, and all required artifacts exist. This is a meta-audit: you do not
repeat deep checks, you aggregate their results and add your own cross-cutting findings.

## Adaptation for Project

- **Public beta / release** → use the full checklist.
- **Internal release** → reduce to smoke + security + deployment artifacts.
- **Worker / Desktop / Game** → adapt HTTP-specific checks to messaging / UI / game loop.
- **No public users** → Won't do, document it.

## Audit Rules

### 1. Release blockers (P0)
- [ ] Security audit has no open P0s.
- [ ] Performance audit has no open P0s.
- [ ] DBA / schema audit has no open P0s.
- [ ] API design audit has no open P0s.
- [ ] Test audit confirms critical path coverage.

### 2. Release artifacts
- [ ] `AGENTS.md` is up to date and covers all modules.
- [ ] `docs/DEPLOYMENT.md` or an equivalent document exists.
- [ ] CI / CD pipeline runs architecture + unit tests.
- [ ] OpenAPI snapshot is current and committed.
- [ ] Smoke tests pass.

### 3. Runtime guardrails
- [ ] `/health` endpoint returns 200.
- [ ] Security headers are configured (CSP, X-Frame-Options, HSTS, etc.).
- [ ] Rate limiting is enabled for public endpoints.
- [ ] Logging does not contain PII (see `PiiGuardTest`).

### 4. Human judgment
- [ ] Product / UX approved edge-case behavior.
- [ ] Support / ops knows key risks and the runbook.

## Report Format

```markdown
## Release Readiness Audit — {date}

### Status: 🔴 NOT READY / 🟡 CONDITIONAL / 🟢 READY

### P0 Blockers
| ID | Finding | Owner | Deadline |
|----|---------|-------|----------|
| REL-001 | ... | ... | ... |

### P1 Important
| ID | Finding | Risk | Mitigation |
|----|---------|------|------------|
| REL-002 | ... | ... | ... |

### Artifacts
| Artifact | Status | Note |
|----------|--------|------|
| AGENTS.md | 🟢 | ... |
| DEPLOYMENT.md | 🟡 | ... |
| CI pipeline | 🟢 | ... |
| OpenAPI snapshot | 🟢 | ... |
| Smoke tests | 🔴 | ... |

### Recommendation
{GO / NO-GO / GO WITH MITIGATIONS}
```

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Release risk ID:** `REL-###`
2. **Source:** which audit / test / artifact confirms the problem
3. **Release impact:** why this is a blocker / important
4. **Owner and deadline**

**NEVER report:**
- “It seems not ready” without concrete checklist items
- Problems already closed in other audits
- Subjective judgments without artifact evidence

## Severity Levels

- **BLOCKER (P0)** — release is impossible without a fix.
- **CRITICAL (P1)** — release is possible only with mitigation and explicit acceptance.
- **MAJOR (P2)** — release is possible, but it must enter the first-week backlog.
- **MINOR (P3)** — nice-to-have.

## Confidence Level

- **CERTAIN** — confirmed by a failing test, audit, or missing artifact.
- **REVIEW** — requires human judgment (product, ops, legal).

## Integration

**Input from:** Security Audit, Performance Audit, DBA Audit, API Design Audit,
Test Audit, Version Audit, Smoke Tests.
**Output to:** Release decision, Backlog Hygiene Agent, Project Manager.
