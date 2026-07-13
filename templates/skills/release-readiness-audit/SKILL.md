---
name: release-readiness-audit
description: >
  Comprehensive audit of release / beta readiness. Aggregates findings from
  other audits and checks the presence of critical artifacts before the project
  becomes publicly available.
---

# Release Readiness Audit — Skill

Optional interaction convention (agent-specific): when this skill is active,
add `📦` to your STARTER_CHARACTER stack (example: `🍀 📦`). Prepend `♻️` when
re-reading the skill. The skill is fully usable without emoji markers.

## Purpose and Non-Goals

You are a release manager / Staff engineer. Before a public release or beta, you
must verify that critical guardrails are in place, there are no unresolved
BLOCKER / CRITICAL findings, and all required artifacts exist.

This is a meta-audit: you do not repeat deep checks, you aggregate their results
and add your own cross-cutting findings. It does not replace security,
performance, DBA or API design audits — it consumes them.

## Portable core

- Before a public release or beta, critical guardrails must be in place, P0 findings closed, and release artifacts must exist.
- This is a meta-audit: it does not repeat deep checks, it aggregates results from other audits and adds cross-cutting findings.
- Verdict: `READY` / `CONDITIONAL` / `NOT READY` with explicit P0/P1, owners, and deadlines.

## Applicability and Exclusions

**Requires adaptation:**
- Application type: Web API, Worker, Desktop, Mobile, Game — adapt HTTP-specific checks to messaging / UI / game loop.
- Release type: public beta, internal release, pilot — shorten the checklist accordingly.
- Set of mandatory audits and artifacts in the project.
- **Public beta / release** → use the full checklist.
- **Internal release** → reduce to smoke + security + deployment artifacts.
- **Worker / Desktop / Game** → adapt HTTP-specific checks to messaging / UI / game loop.

**Not applicable when:**
- The project is not preparing for a release / beta launch.
- There are no public users / production deployment → Won't do, document it.

## Required Inputs

- Reports from: Security Audit, Performance Audit, DBA Audit, API Design Audit, Test Audit, Version Audit, Smoke Tests.
- Access to release artifacts: `AGENTS.md`, `docs/DEPLOYMENT.md`, CI/CD pipeline, API schema snapshot.
- Named owners for product / ops sign-off.

## Procedure

### 1. Release blockers (P0)
- [ ] Security audit has no open P0s.
- [ ] Performance audit has no open P0s.
- [ ] DBA / schema audit has no open P0s.
- [ ] API design audit has no open P0s.
- [ ] Test audit confirms critical path coverage.

### 2. Release artifacts
- [ ] Project documentation (`AGENTS.md` or equivalent) is up to date and covers all modules.
- [ ] Deployment document (`docs/DEPLOYMENT.md` or equivalent) exists.
- [ ] CI / CD pipeline runs architecture + unit tests.
- [ ] API schema snapshot / contract is current and committed.
- [ ] Smoke tests pass.

### 3. Runtime guardrails
- [ ] Health / ready endpoint returns 200 (if applicable).
- [ ] Security headers are configured (CSP, X-Frame-Options, HSTS, etc.).
- [ ] Rate limiting is enabled for public endpoints (if applicable).
- [ ] Logging does not contain PII / sensitive data.

### 4. Human judgment
- [ ] Product / UX approved edge-case behavior.
- [ ] Support / ops knows key risks and the runbook.

## Project-specific examples

> The examples below illustrate application in a .NET Web API. Replace with your stack.

### Example: .NET Web API

- **Health endpoint:** `/health` returns 200.
- **PII guardrail:** an equivalent `PiiGuardTest` checks that logs do not contain sensitive data.
- **OpenAPI snapshot:** current and committed.
- **Security headers:** CSP, X-Frame-Options, HSTS configured in middleware.

## Evidence Requirements

Every finding MUST include:
1. **Release risk ID:** `REL-###`
2. **Source:** which audit / test / artifact confirms the problem
3. **Release impact:** why this is a blocker / important
4. **Owner and deadline**

**NEVER report:**
- “It seems not ready” without concrete checklist items
- Problems already closed in other audits
- Subjective judgments without artifact evidence

## Finding Schema

```text
ID
Severity: BLOCKER | CRITICAL | MAJOR | MINOR
Confidence: CONFIRMED | NEEDS_REVIEW
Category / Control
Evidence: file:line, command output, trace or reproduction
Impact
Recommended action
Owner / disposition
```

## Severity and Confidence

| Severity | Meaning |
|----------|---------|
| **BLOCKER** (P0) | Release is impossible without a fix |
| **CRITICAL** (P1) | Release is possible only with mitigation and explicit acceptance |
| **MAJOR** (P2) | Release is possible, but it must enter the first-week backlog |
| **MINOR** (P3) | Nice-to-have |

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Confirmed by a failing test, audit, or missing artifact |
| **NEEDS_REVIEW** | Requires human judgment (product, ops, legal) |

## Outputs and Downstream Consumer

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
| API schema snapshot | 🟢 | ... |
| Smoke tests | 🔴 | ... |

### Recommendation
{GO / NO-GO / GO WITH MITIGATIONS}
```

**Downstream consumer:** Release decision, Backlog Hygiene Agent, Project Manager.

## Trigger or Schedule

Runs before every public release / beta launch, and as a checkpoint when the
project enters release preparation. Not part of the regular PR cycle.

## Limitations and Expected False Positives

- Aggregation quality is bounded by the input audits — a missing upstream audit yields a blind spot, not a green light.
- HTTP-specific checks (health endpoint, security headers, rate limiting) are false positives on Worker / Desktop / Game projects — adapt or mark N/A.
- P1/P2 severity calls often need product or legal judgment — mark them NEEDS_REVIEW rather than blocking unilaterally.
- The verdict is advisory; the final GO / NO-GO stays with the human release owner.
