# Skeptical AI Engineering Control Pyramid

> **Skeptical AI Engineering (SAE)** — methodology from the talk "AI is confident. I'm not" (Dotnext 2026).
> Feedback speed determines how far the agent gets before you stop it.
> Compiler catches in seconds. E2E in minutes. Anything slower is not the pyramid — it's the outer loop.

---

<a name="layer-0"></a>
## Layer 0. Agent Instructions (AGENTS.md + Numbered Decisions)

This is not a "feedback layer" — it's the **ground rules**. Everything else is just enforcement.

- `rules/AGENTS_TEMPLATE.md` — hierarchical instructions: root + per-module
- `PERF-###` / `DB-###` / `AUD-###` — numbered decisions in code the agent must not "clean up"
- Architecture test checks uniqueness of decision IDs

---

<a name="layer-1-compiler"></a>
## Layer 1. Compiler + Types (~seconds)

**Trap:** Agent returns `string` instead of `DateTime`, changes a DTO, frontend never finds out.

**Fix:** Static typing — fastest and cheapest feedback loop.

### Backend
- `dotnet build` — Nullable reference types, record-based DTO immutability
- Agent cannot return `null` unchecked

### Frontend
- `tsc --noEmit` (strict mode) + `noUnusedLocals`
- Auto-generated types from OpenAPI snapshot — backend changes DTO, frontend fails to compile

**Pattern:** `tests/patterns/SnapshotTest.cs`

---

<a name="layer-2-architecture"></a>
## Layer 2. Architecture + Ratchet (~10 seconds)

**Trap:** Agent adds `using Infrastructure` in Application, deletes types/tests, or uses `.FindAsync()` in read-path.

**Fix:** 25+ automated checks running in seconds.

| Category | Checks |
|----------|--------|
| Layers (Clean Architecture) | 10 |
| Naming | 3 |
| Structure and anti-patterns | 11 |
| Performance | 1 |
| Test inventory | 1 |

### Whitelist with staleness check

Whitelist for exceptions (write-path) self-validates: if a file from the whitelist no longer contains the pattern — the test fails.

**Pattern:** `tests/patterns/ArchitectureRules.cs`, `tests/patterns/RatchetTest.cs`

---

<a name="layer-3-tests"></a>
## Layer 3. Tests (TUnit + `dotnet run`) (~30 seconds)

**Trap:** CI is green but nothing is actually being tested. Code merges unchecked for weeks.

**Fix:**
- **TUnit** + `dotnet run --project` (no `dotnet test`)
- **BUG-regression convention:** every bug-fix = file `BUG###_DescriptiveName.cs`
- **OpenAPI snapshot test:** backend changes DTO → snapshot fails → frontend is not forgotten
- **Characterization tests:** pinning behavior of critical algorithms ("this is how it works now")

**Artifacts:** `tests/conventions/TUnit_Guide.md`, `tests/conventions/BUG_TEMPLATE.cs`

---

<a name="layer-4-code-review"></a>
## Layer 4. Code review by agent (~2 minutes)

**Trap:** Agent wrote XSS in `returnUrl`, forgot `await`, leaked internal ClientId.

**Fix:** Before commit, a **separate agent** (not the one that wrote the code) reviews the diff.

From practice: 8 review commits with findings:
- XSS in query params
- Forgotten `await` on notification send (silent failure)
- Leaked internal ClientId in API response
- Constant-time hash not used (timing attack)

**Checklist:** when reviewing a commit with `fix:` — check for `BUG*Tests.cs`.

**Artifact:** `skills/code-review/`

---

<a name="layer-5-e2e"></a>
## Layer 5. E2E MCP (~5-15 minutes)

**Trap:** Agent does not see that shift mode shows all days as "Day Off" due to stale cache.

**Fix:** 20+ MCP tools: Telegram, VK, browser, API. Agent pokes the app itself.

- Stale cache (22 days in prod!)
- Dashboard date reset
- Self-booking bypass

**Key point:** E2E found stale cache that slipped through:
- Compiler ✅ (code is syntactically correct)
- Unit tests ✅ (mock cache)
- Code review ✅ (caching diff looked correct)

**Pattern:** `docs/traps/silent-breakdown.md`

---

## Synthesis: inner loop

```
          ┌─────────────────────┐
          │   E2E MCP           │  ← Layer 5: Poke with real hands
          ├─────────────────────┤
          │   Code review       │  ← Layer 4: Second agent reads diff
          │   by agent          │
          ├─────────────────────┤
          │   TUnit             │  ← Layer 3: BUG-regression, snapshot
          │   + dotnet run      │
          ├─────────────────────┤
          │   NetArchTest       │  ← Layer 2: Layers, ratchet
          ├─────────────────────┤
          │   Compiler          │  ← Layer 1: dotnet build, tsc
          │   + Snapshot        │
          ├─────────────────────┤
          │   AGENTS.md         │  ← Layer 0: Instructions before code
          │  + Numbered Decisions│
          └─────────────────────┘
```

---

<a name="outer-loop"></a>
## Outer Loop — 3 levels

> Anything that runs on schedule, trigger, or before release — not part of the daily feedback loop, but deep validation.

### Audit skills

This is not a "feedback level" — it's a **repeatable persona**. Without skills, outer loop becomes chaotic manual browsing.

- `skills/security-audit/`, `skills/dba-audit/` etc. — narrow personas with `CHECKLIST.md`
- Run in batches — cross-pollination of findings between security and UX, performance and DB schema
- Each skill = an agent role you can run anytime with the same result

---

### Level 1. Audits (~1-2 hours)

Narrow skill-personas run in batches — cross-pollination of findings between security and UX, performance and DB schema.

| Audit | Artifact |
|-------|----------|
| Security | `skills/security-audit/` |
| DBA | `skills/dba-audit/` |
| Performance | `skills/performance-audit/` |
| i18n | `skills/i18n-audit/` |
| API Design | `skills/api-design-audit/` |
| Tech Debt | `skills/tech-debt-audit/` |
| Test Coverage | `skills/test-audit/` |
| UX Flow | `skills/ux-audit/` |
| Simplicity | `skills/simplicity-audit/` |
| Version | `skills/version-audit/` |

**Schedule:** once per sprint or ad-hoc when you smell danger.

### Level 2. Load (~minutes)

**Trap:** Agent shows P50 = 6ms, but Max = 4400ms. User hits tail latency.

**Fix:** NBomber — read + write mix, spike, concurrent booking. Runs before release or when degradation is suspected.

Scenarios: read + write mix, spike, concurrent booking.

**Pattern:** `tests/patterns/LoadTest.cs`

### Level 3. Manual testing (~hours-days)

- **Smoke check** (15 min) — 10 critical scenarios
- **Production usage** — real users, real data

Last line of defense. Anything that got here passed 5 inner loop layers + audits + load.

---

## Synthesis: outer loop

```
          ┌─────────────────────┐
          │   Manual            │  ← Level 3: Human, real data
          │   testing           │
          ├─────────────────────┤
          │   Load              │  ← Level 2: NBomber, read+write mix
          │   (NBomber)         │
          ├─────────────────────┤
          │   Audits            │  ← Level 1: Batch narrow personas
          │   (batch)           │
          ├─────────────────────┤
          │   skills/           │  ← Audit skills
          │   + CHECKLIST.md    │
          └─────────────────────┘
```

---

## Effectiveness metrics

### What each layer caught (from git history)

| Cycle | Layer | Bugs found | % of fixes |
|-------|-------|-----------|------------|
| **Inner** | Compiler + types | ~0 commits | — |
| **Inner** | Arch tests + Ratchet | ~0 commits | — |
| **Inner** | Unit/integration | 6 commits | 1.3% |
| **Inner** | Code review | 8 commits | 1.8% |
| **Inner** | E2E (MCP) | 9 commits | 2.0% |
| **Outer** | Load (NBomber) | ~0 commits | — |
| **Outer** | Audits | 19 commits | 4.2% |
| **Outer** | Manual (BUG-NNN) | ~78 commits | 17% |
| — | Gray zone | ~331 commits | 74% |

### The invisible layer paradox

Compiler and arch tests are the **most effective** layers, but in git they show 0 commits. They prevent bugs **before** code leaves the workstation.

### ROI by layer

| Layer | Setup cost | Maintenance cost | Break-even |
|-------|-----------|------------------|------------|
| Compiler + types | 0 (built-in) | 0 | Instant |
| Arch tests + Ratchet | ~2 days | ~1 hour/month | First week |
| Unit tests | ~2 weeks | ~30 min/feature | First month |
| Code review | ~0 (AGENTS.md rule) | ~2 min/commit | First XSS |
| E2E MCP | ~3 days | ~1 hour/platform | After stale cache (22 days in prod) |
| Load (NBomber) | ~1 day | ~30 min/scenario | First silent breakdown under load |
| Audits | ~0 (prompts) | ~2 hours/audit | First batch |
| Manual (smoke) | — | ~15 min/session | Impossible to measure |

## Grooming loop — Artifact maintenance

> Inner loop catches bugs in code. Outer loop catches cross-cutting issues.
> But agent artifacts rot too: AGENTS.md drifts from code, Auto Memory
> accumulates duplicates, backlog turns into a graveyard. This is the third loop.

```
          ┌─────────────────────┐
          │   backlog-hygiene   │  ← Clean dead specs and priority drift
          ├─────────────────────┤
          │   doc-hygiene       │  ← AGENTS.md consistency and cross-agent docs
          ├─────────────────────┤
          │   memory-hygiene    │  ← Auto Memory: duplicates, hierarchical drift
          └─────────────────────┘
```

### Why separate

Grooming is not a pyramid layer or an audit. It does not give feedback on code
during development. It prevents **meta-information decay** that the whole
pyramid depends on.

| Artifact |
|----------|
| **Auto Memory** |
| **AGENTS.md** |
| **Backlog** |

### Grooming skills

| Skill |
|-------|
| `skills/memory-hygiene/` |
| `skills/doc-hygiene/` |
| `skills/backlog-hygiene/` |

### The invisible decay paradox

Rules and checks get stale. You won't see it in git, CI won't flag it, tests won't catch it.
It hits suddenly: the agent "forgets" a rule because AGENTS.md
is out of sync, or makes an architectural decision based on a stale note from
Auto Memory. Grooming is the only defense against this.

### ROI

| Skill | Cost | Break-even |
|-------|------|------------|
| memory-hygiene | ~30 min/sprint | First architectural bug from stale memory |
| doc-hygiene | ~1 hour/sprint | First time agent broke code due to outdated AGENTS.md |
| backlog-hygiene | ~30 min/sprint | First false positive `task-compliance` on a dead spec |

---

## Evolution: how the system grows

```
January:   compiler + types → unit tests
  ↓ architecture bugs
February:  + arch tests → + audits (in batches) → + code review
  ↓ UI bugs
March:     + E2E MCP → + characterization tests
  ↓ write-path degradation
April:     + NBomber (read+write mix) → + ratchet tests
```

Every new layer is a reaction to a bug class that previous layers missed.

### Principle: Guardrails are born from pain

> Do not add a guardrail for a problem that has not yet occurred.

Every test, regex, architecture check, or linter rule must answer: **"What specific bug does this catch?"**

A dead guardrail (0 triggers in 3 sprints) is not protection — it is tech debt. It creates a false sense of security, wastes CI time, and dilutes team attention. Delete it without regret.

## 4 rules for Monday

1. **Every bug-fix = `BUG###_` test.** No test — no fix.
2. **Every PR = `dotnet run --project` tests + code review by agent.**
3. **Every sprint = batch audit + NBomber before release.** An agent does not see cross-cutting issues — a persona does.
4. **Every sprint = groom artifacts.** Memory-hygiene, doc-hygiene, backlog-hygiene — agent artifacts rot too.
