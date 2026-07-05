# Business Risk Audit — Checklist

## Pre-flight

- [ ] Findings from 5–7 domain audits are available (security, dba, perf, ux, i18n, code-review, etc.)
- [ ] Diff for the last 1–2 weeks or scope of a large refactor is available
- [ ] 2–3 key user scenarios affected by the changes are known

## End-to-end flow reconstruction

- [ ] For each scenario the chain is reconstructed: UI/cache → API → domain → events → outbox → jobs → db/migrations
- [ ] Each layer of the chain is mapped to concrete files / services
- [ ] Places where context is passed between layers are found (DTO, events, cache keys, migration model)

## Seam analysis — 5 questions per scenario

- [ ] **Wrong subject?** Identity / ownership / session context is resolved consistently across all layers
- [ ] **Wrong point in time?** Dates and timezones are parsed, stored, and displayed under one contract
- [ ] **Wrong source of truth?** Cache, sessionStorage, read model, and DB do not contradict each other after a write
- [ ] **Wrong projection / migration reality?** Runtime model matches the DB and migrations
- [ ] **What will silently go wrong for a real user?** Every finding ends with this question

## Cross-layer invariants

- [ ] **State resurrection:** interrupted flow, back button, sessionStorage, retry do not resurrect stale state
- [ ] **Ownership resolution:** UserId / OwnerId / ActorId / ContextId are interpreted consistently in API, domain, and jobs
- [ ] **Timezone contract:** relative date parsing, DateTimeKind, DB, UI — all under one contract
- [ ] **Cache vs source-of-truth:** write invalidates cache; reading after write does not return stale data
- [ ] **Runtime vs migration drift:** model in code, migrations, and data are consistent
- [ ] **Eventual consistency:** UI and downstream consumers correctly handle the window between event and job

## Synthesis

- [ ] Domain audit findings are grouped by scenario, not by skill
- [ ] Combinations of findings from different domains that together create a system risk are found
- [ ] Every system risk has a trigger and business impact
- [ ] For every risk a fix is proposed at the contract/pipeline level, not just a single file

## Quality gates

- [ ] Every system finding contains: risk name, scenario, seam, evidence, trigger, business impact, fix
- [ ] BLOCKER/CRITICAL findings have concrete reproduction steps
- [ ] REVIEW findings are marked as requiring human judgment or E2E verification
- [ ] No findings like "system is complex" without a concrete broken invariant
