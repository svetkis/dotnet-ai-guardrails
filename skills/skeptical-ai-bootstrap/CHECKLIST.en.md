# Skeptical AI Bootstrap Checklist

## Pre-flight
- [ ] Path to the .NET solution root obtained
- [ ] Mode determined: `fast` | `standard` | `paranoid`
- [ ] Stack determined (don't assume — check `.csproj`)

## Discovery — honest codebase inspection
- [ ] All `.csproj` and `.sln` found
- [ ] .NET version determined (`TargetFramework`)
- [ ] Application type determined (Web / Worker / Desktop / Lib / Game / ML)
- [ ] ORM/data access determined (EF / Dapper / Mongo / ADO.NET)
- [ ] Test framework determined (TUnit / xUnit / NUnit / MSTest / none)
- [ ] CI/CD configs found
- [ ] Architecture determined (Clean / VSlice / Modular / None / BBoM)
- [ ] Existing `AGENTS.md`, conventions found

## Layer 1 — Compiler (principle: fast feedback)
- [ ] `<TreatWarningsAsErrors>`?
- [ ] `<Nullable>` or `#nullable`?
- [ ] Is there `.editorconfig`?
- [ ] Does the build pass without warnings?
- [ ] **If .NET Framework:** are there Roslyn analyzers?

## Layer 2 — Architecture (principle: auto-check layers)
- [ ] Are there architectural tests of ANY kind?
- [ ] Does NetArchTest fit the stack?
  - [ ] .NET 6+? → NetArchTest is possible
  - [ ] .NET Framework? → different approach needed
  - [ ] Vertical Slice? → NetArchTest works, but needs custom rules about features (not layers)
- [ ] Is there source scanning? (Roslyn analyzers / MSBuild tasks / fallback regex)
- [ ] Are critical attributes checked (ratchet)?
- [ ] **Decision:** Adapt / Create new skill / Skip

## Layer 3 — Tests (principle: coverage + real DB, not InMemory)
- [ ] Are there unit tests?
- [ ] Are there integration tests?
- [ ] Is there a "0 tests ran" check?
- [ ] Do not propose migration if >1000 tests on another framework
- [ ] **If Worker/Desktop/Game/ML:** E2E tests will be different — propose creating
- [ ] **Decision:** Adapt / Create new skill / Skip

## Layer 4 — Code Review (principle: agent checks agent)
- [ ] Are there agent rules?
- [ ] Does the ready `code-review` skill fit the stack?
  - [ ] Razor Pages? → need `code-review-razor`
  - [ ] Dapper? → need `code-review-dapper`
  - [ ] .NET Framework? → need `code-review-netframework`
  - [ ] gRPC? → need `code-review-grpc`
  - [ ] Game? → need `code-review-game`
  - [ ] ML/AI? → need `code-review-ml`
  - [ ] Minimal API + EF Core? → ✅ adapt
- [ ] **Decision:** Adapt / Create new skill / Skip

## Layer 5 — E2E / MCP (principle: end-to-end via external systems)
- [ ] Are there E2E checks?
- [ ] Does the project type allow OpenAPI snapshot?
  - [ ] Web API → ✅ can snapshot
  - [ ] gRPC → ❌ need proto compatibility check
  - [ ] Worker / Desktop / Game → ❌ need different E2E
- [ ] Are there load tests?
- [ ] **Decision:** Adapt / Create new skill / Skip

## Layer 0 — Instructions
- [ ] Is there `AGENTS.md` or equivalent?
- [ ] Is there `CONVENTIONS.md`?
- [ ] Are there decision guards?

## Outer Loop — Audits
- [ ] Are there security artifacts?
- [ ] Is DBA audit applicable to project ORM?
  - [ ] EF Core → ✅ ready skill
  - [ ] Dapper → ❌ create `dba-audit-dapper`
  - [ ] Mongo → ❌ create `dba-audit-mongo`
- [ ] Are Perf / UX / i18n audits needed?
  - [ ] Russian only → i18n not needed, document
- [ ] **Decision:** Adapt / Create new skill / Skip

## New Skill Design (if required)
- [ ] **Threat Model** defined: what can the agent break?
- [ ] **Role** defined: when and who checks?
- [ ] **Mechanism** selected: Roslyn / Test / Script / AI Agent / MSBuild?
- [ ] **Name** invented: `{what-checks}-{context}`
- [ ] **Place** defined: `.kimi/skills/{name}/`
- [ ] **Integration** defined:
  - [ ] Input: where do we get context?
  - [ ] Output: where do we send results?
  - [ ] Trigger: what launches it (PR / schedule / manual)?
  - [ ] Gate: does it block merge?
- [ ] Files generated:
  - [ ] `SKILL.md`
  - [ ] `CHECKLIST.md` (if complex process)
  - [ ] Mechanism code (test / analyzer / script)

## Ecosystem Map (generated in report)
- [ ] Table of all project skills created
- [ ] Each skill has status: Active / WIP / Backlog
- [ ] Gaps between layers defined
- [ ] New skills added to the map

## Anti-Patterns Check (do not impose!)
- [ ] Did not propose xUnit→TUnit migration at >1000 tests
- [ ] Did not copy EF tests into Dapper project
- [ ] Did not require OpenAPI snapshot for Worker/Desktop
- [ ] Did not propose NetArchTest for .NET Framework
- [ ] Did not propose Clean Architecture for BBoM without refactoring
- [ ] Did not create a "God Skill" (everything in one file)
- [ ] Each new skill has a specific launch mechanism
- [ ] Did not create a skill "for the sake of skill" — there is a real threat

## Report Quality Gates
- [ ] Each layer has status + rationale
- [ ] For each layer specified: Adapt / Create / Deploy / Skip
- [ ] New skills described with rationale (why ready-made don't fit)
- [ ] Each new skill has: role, mechanism, place, integration
- [ ] Non-applicable layers documented (why)
- [ ] Skill ecosystem map generated
- [ ] No hallucinations — only facts from the codebase
