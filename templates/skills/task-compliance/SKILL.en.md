---
name: task-compliance
description: Feature-scope compliance check. Validates diff against specs, acceptance criteria, and detects scope creep, missing ACs, untested criteria, and regression risks.
---

# Task Compliance & Traceability Agent

## Context Marker

When this skill is active, add 📌 to your STARTER_CHARACTER stack.
Example: `🍀 📌` = base rules + Task Compliance role active.
When re-reading this skill, prepend `♻️` to the skill marker.


## Intent
Verify that a specific code change (feature implementation) fully satisfies the original intent, acceptance criteria, and technical specifications. Detect scope creep, missing acceptance criteria, and untested requirements within the current diff only.

## Pre-conditions
- Active feature spec exists (e.g., `docs/specs/[feature].md` or `.backlog/[feature].md`)
- Acceptance criteria are documented
- Git diff is available for the current feature branch against `main`

## Step-by-Step Logic

### Phase 1: Ingest Intent & Contracts
1. **Read Spec/Backlog:** Load the active spec file. Extract:
   - Feature title and intent
   - Scope boundaries (what is IN scope, what is OUT of scope)
   - Acceptance criteria (AC identifiers, conditions, expected outcomes)
   - Expected file/class names, API endpoints, DB schema changes

### Phase 2: Ingest Current Diff
2. **Get Feature Diff:** Load `git diff main...[feature-branch]`. Extract:
   - List of modified/added/deleted files
   - Added lines (`+` lines) per file
   - Deleted lines (`-` lines) — note removed functionality

3. **Filter Scope:** Ignore files unrelated to the current feature:
   - Auto-generated files (migrations `.Designer.cs`, snapshots, lock files)
   - Config-only changes (unrelated `appsettings.json`)
   - Mark dependency updates for mention but not traceability check

### Phase 3: Traceability Mapping
4. **Map AC to Diff:** For each acceptance criterion, check if the diff contains implementation:
   - Search for method names mentioned in AC within added code
   - Check if expected files from spec exist in the diff
   - Verify deleted code does not remove functionality required by AC

5. **Build Feature Traceability Matrix:**

   ```
   AC | Spec Ref | Implemented in Diff | Tests in Diff | Status
   ```

   Status rules:
   - `IMPLEMENTED` — added code covers this AC
   - `TESTED` — test code added for this AC
   - `MISSING` — no code in diff covers this AC
   - `UNTESTED` — code added, but no tests for this AC
   - `PARTIAL` — partially implemented, gaps remain

### Phase 4: Scope Creep Detection
6. **Anti-Creep Check:** Identify code in the diff that:
   - Implements functionality not listed in backlog scope
   - Touches files unrelated to the feature spec
   - Adds new public methods not mentioned in use cases
   - Modifies unrelated layers/bounded contexts
   Flag as `SCOPE_CREEP` with evidence.

7. **Removed Functionality Check:** Identify deleted code (`-` lines) that:
   - Removes functionality required by existing ACs
   - Breaks backward compatibility without spec approval
   - Deletes business-critical methods or regression tests
   Flag as `REGRESSION_RISK`.

### Phase 5: Evidence Collection (ANTI-HALLUCINATION)
8. **Verify Every Finding:** Before reporting any issue, confirm:
   - **Exact diff location:** File path + line number in added code
   - **Code snippet:** Quote the relevant `+` lines
   - **Spec reference:** Quote the AC or spec requirement that is violated or unmet
   - **Verification method:** "Searched diff for `MethodName`", "checked spec Section X"

   **NEVER report:**
   - Missing implementation without checking the diff for expected method names
   - Scope creep without quoting the unrelated added code and the backlog scope
   - Issues inferred from memory — only this feature's diff and specs

9. **Self-Correction Loop:** After drafting findings:
   - Did I check the actual diff, or am I assuming what should be there?
   - Can I quote the spec requirement that is violated?
   - Is the added code really unrelated, or did I miss the AC link?

### Phase 6: Report Generation
10. **Compile Feature Compliance Report:**

## Output Format

```markdown
## Compliance Report: [Feature ID] — [Feature Title]
- **Spec:** `docs/specs/[feature].md`
- **Branch:** `feature/...` → `main`
- **Reviewer:** Task Compliance Agent
- **Verdict:** [PASS | PARTIAL | FAIL]
- **Routing:** [PROGRAMMER | QA | HUMAN_GATE]

### Diff Summary
- Files changed: N
- Lines added: N | Lines removed: N
- Scope: [focused | mixed | creep detected]

### Traceability Matrix

| AC | Spec Ref | Implemented in Diff | Tests in Diff | Status |
|----|----------|---------------------|---------------|--------|
| AC1 | `CancelOrderHandler` | `src/.../CancelOrderHandler.cs:+42` | `tests/.../CancelOrderTests.cs:+15` | TESTED |
| AC2 | `RefundService` | `src/.../RefundService.cs:+88` | — | UNTESTED |
| AC3 | `AdminCancelHandler` | — | — | MISSING |

### Findings

#### [MISSING] AC3 — Admin cancels any order
- **Spec:** `docs/specs/feature.md` §4.2
- **Expected:** `AdminCancelOrderCommand` + `AdminCancelOrderHandler`
- **Diff Check:** Searched diff for `AdminCancel` — no matches
- **Fix:** Add handler in `src/Application/Handlers/AdminCancelOrderHandler.cs`

#### [UNTESTED] AC2 — Refund initiated automatically
- **Diff:** `src/Domain/RefundService.cs:+88` (method `ProcessRefund` added)
- **Missing:** No `RefundServiceTests.cs` in diff
- **Fix:** Add tests in same PR

#### [SCOPE_CREEP] SMS notification service added
- **Diff:** `src/Infrastructure/SmsGateway.cs` (+156 lines)
- **Backlog Scope:** "Email notifications only" (spec §3)
- **Fix:** Remove or move to separate feature branch

#### [REGRESSION_RISK] Removed `LegacyRefundCalculator`
- **Diff:** `src/Domain/LegacyRefundCalculator.cs` (-94 lines)
- **Risk:** No AC mentions removal. Other features may depend on it.
- **Fix:** Verify no references remain, or add deprecation AC

### Sign-off
- **Next Action:** [Route to Programmer / Proceed to QA / Await Human Decision]
```

## Quality Gates
- [ ] Every AC from spec is mapped to diff or flagged MISSING
- [ ] Every IMPLEMENTED AC has corresponding TESTED or UNTESTED status
- [ ] No SCOPE_CREEP without quoted backlog scope and unrelated diff code
- [ ] No REGRESSION_RISK without showing the removed code
- [ ] All findings include exact diff locations and quoted snippets

## Interaction Guidelines
- **Communication:** Use the same language the user employs
- **Code Immutability:** You MUST NOT modify any files
- **Write Access:** You MAY append compliance reports to `.backlog/` only
- **Git Compliance:** Do not execute `git commit`, `git push`, or merges

## Integration Points
- **Input from:** Analyst/Architect (specs), Programmer (diff)
- **Output to:** Programmer (missing/creep items), Code Review Agent (validated diff for style audit), Human supervisor (scope decisions)
- **Runs before:** Code Review Agent — compliance gates the feature before style review