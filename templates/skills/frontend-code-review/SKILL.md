---
name: frontend-code-review
description: |
  Pre-commit diff-based code review for React + TypeScript frontend projects.
  Trigger this skill right before `git commit`, when the user asks to review frontend changes,
  or when the staged diff contains React/TypeScript files (*.tsx, *.ts, *.jsx, *.js, *.css, *.scss).
  Reviews staged changes against React Hooks rules, TypeScript strictness, performance, accessibility,
  and security hygiene.
whenToUse:
  - Before committing React/TypeScript frontend changes (pre-commit).
  - User says "frontend pre-commit review", "review my React changes", "check the frontend diff", "code review".
  - Staged diff includes *.tsx, *.ts, *.jsx, *.js, *.css, *.scss, package.json, tsconfig.json.
triggers:
  - pre-commit
  - frontend code review
  - review React changes
  - check frontend diff before commit
invocation:
  manual: true
  auto: true
version: 1.0.0
---

# Pre-commit Frontend Code Review Agent

## Purpose and Non-Goals

- Review **only staged frontend changes** (`git diff --cached`, `+` lines with immediate context) against React Hooks rules, TypeScript strictness, performance, accessibility, and security hygiene.
- Act as a second, focused reviewer: a dedicated reviewer focuses only on frontend guardrails (Focused Agent), and catches silent misalignment in hook rules or typing before it reaches `main`.
- **Non-goals:** reviewing unchanged code or entire files without a diff; UX/design sign-off; running `git commit` on the user's behalf.

## Applicability and Exclusions

This skill targets **React 18+ / React 19 + TypeScript 4.8+**.

| Technology | Target version | Note |
|------------|---------------|------|
| React | >= 18.0.0 | Hooks, concurrent features, Strict Mode |
| TypeScript | >= 4.8.0 | strict: true, satisfies, inferred types |
| JSX/TSX | — | .tsx preferred over .jsx |
| Build tool | Vite / Next.js / Remix | Adapt rules to CLI specifics |

**Not applicable when:**
- The project uses React 17 or older, class components, legacy state (Redux without RTK Query) — mark the relevant checks as **N/A** and adapt.
- Razor/Blazor or another framework (Vue/Svelte) — create a separate skill.
- Changes are backend-only (.cs, .csproj) — use `code-review`.
- There are no staged changes, or the user asks for a full-file review without a diff.

## Required Inputs

- Read access to the repository and the staged diff (`git diff --cached --diff-filter=ACMR -- '*.tsx' '*.ts' '*.jsx' '*.js' '*.css' '*.scss' '*.json'`; in PR flow `git diff main...[branch]`).
- Project stack facts: React/TypeScript versions, styling approach, state management, test framework.

## Procedure

1. Read the staged diff: frontend files only. Review ONLY `+` lines and directly related context lines — NEVER unchanged code or entire files.
2. If there are no staged frontend changes, tell the user there is nothing to review and stop.
3. Apply the checks below to every `+` block. If some checks are not applicable to the stack — mark them N/A, do not report as findings.
4. Produce findings in the required format and verify evidence for each (see Evidence Requirements).
5. State a verdict. If **CHANGES_REQUESTED**, advise the user to fix BLOCKER/CRITICAL/MAJOR issues and stage the fixes before committing. Do NOT run `git commit` yourself.

### React / Hooks Checks
- **Rules of Hooks**: Hooks called only at top level of function components / custom hooks. NO hooks inside loops, conditions, or nested functions.
- **Hook naming**: Custom hooks MUST start with `use`.
- **useEffect**:
  - Dependency array MUST be exhaustive or explicitly disabled with comment explaining why.
  - NO direct `async` in `useEffect` body (create inner async function).
  - Cleanup function for subscriptions, timers, event listeners, `AbortController`.
  - Do NOT use `useEffect` for derived state that can be computed during render.
- **useState**:
  - NO direct mutation of state objects/arrays.
  - Prefer functional updater when next state depends on previous.
  - Avoid initializing state from props unless truly needed (derived state anti-pattern).
- **useMemo / useCallback**:
  - Must have meaningful deps.
  - Do NOT wrap every value "just in case" — flag premature optimization.
- **Context**:
  - Split context by concern to avoid unnecessary re-renders.
  - Default value should match shape.

### Rendering & JSX Checks
- **key prop**: Stable, unique IDs. NO array index as `key` unless list is static and never reordered/filtered.
- **Conditional rendering**: Avoid `condition && <Component />` when condition can be `0`/`""` — use `!!condition` or ternary.
- **dangerouslySetInnerHTML**: BLOCKER if used without proven sanitization (DOMPurify etc.).
- **Inline objects/arrays/functions in JSX props**: MAJOR if they break memoization of pure children.
- **DOM manipulation**: Avoid `document.getElementById`, `querySelector` inside React except in refs / effects with cleanup.
- **Event handlers**: Prefer named functions; avoid inline `onClick={() => ...}` when it causes re-mounts or hurts readability.

### TypeScript Checks
- **No implicit any**: `strict: true` is enabled. `any` is MAJOR unless justified and documented.
- **No non-null assertion**: `value!.property` is MAJOR unless guarded.
- **No `as` casts**: MAJOR unless necessary and documented.
- **Return types**: Exported components/hooks SHOULD declare return type or rely on strong inference.
- **Discriminated unions**: Prefer over optional fields with `| undefined`.

### Performance Checks
- **React.memo**: Only when justified by measurement or obvious heavy subtree.
- **Lazy loading**: Routes / heavy components SHOULD use `React.lazy` + `Suspense`.
- **Large re-renders**: Flag passing new object/array to context without memoization.
- **Images**: Use modern formats, lazy loading, explicit width/height to prevent CLS.

### Accessibility Checks
- **Semantic HTML**: Use `<button>` for clickable actions, not `<div onClick>`.
- **Alt text**: Images MUST have meaningful `alt` or `role="presentation"`.
- **Labels**: Form inputs MUST have associated `<label>` or `aria-label`/`aria-labelledby`.
- **Focus**: Modals/dropdowns MUST trap focus and restore focus on close.
- **tabIndex**: Positive `tabIndex` is CRITICAL; use `-1` or `0` only.
- **Color**: Do not rely on color alone for status/errors.

### Security Checks
- **XSS**: No `innerHTML`, `dangerouslySetInnerHTML` with user input, `eval`, `new Function`.
- **URL injection**: No user input directly in `href`/`src` without validation; ban `javascript:` URLs.
- **Secrets**: No API keys, tokens, passwords in code; use env variables.
- **npm packages**: Flag suspicious new dependencies (typosquatting, no usage, huge bundle size).

### State Management Checks
- **Prop drilling**: MAJOR if props pass through >2 intermediate components without usage.
- **Global state**: Justify Redux/Zustand/Jotai for truly global data; avoid for local UI state.
- **Server state**: Use RTK Query / TanStack Query / SWR; do NOT manage server cache manually.
- **Immutability**: Updates via Immer or spread; never mutate arrays/objects in reducer/store.

### Forms & Inputs
- **Controlled components**: Inputs MUST have `value` + `onChange` (except uncontrolled with ref and clear reason).
- **Validation**: Error state accessible, validation runs on submit and blur where appropriate.
- **Submit handlers**: Prevent default, handle loading/error states, disable submit while pending.

### Styling Checks
- **Consistency**: One approach per project (CSS Modules, Tailwind, styled-components, etc.).
- **No inline styles**: MINOR unless dynamic positioning requires it.
- **Responsive**: Mobile-first, no fixed widths breaking layout.

### Testing Checks
- **Framework**: Prefer React Testing Library + Vitest/Jest. NO Enzyme.
- **Queries**: Prefer `getByRole`, `getByLabelText` over `getByTestId`.
- **User events**: Use `@testing-library/user-event`, not fire-and-forget `fireEvent`.
- **Async tests**: Use `waitFor` / `findBy*` correctly; assert after state updates.
- **No tautological assertions**: `expect(true)`, `expect(true).toBe(true)`, `expect(1).toBe(1)` and other always-pass assertions are MAJOR — they do not verify the behavior named by the test.
- **No body-only checks**: Playwright / E2E tests that parse the response body or query a DOM node without an explicit assertion on the postcondition are MAJOR. Example: `const body = await response.text();` with no subsequent `expect`; `page.locator('[data-testid="x"]')` with no assertion. A locator alone is not a behavior check.
- **No `waitForTimeout` as a substitute for condition waits**: Fixed sleeps (`await page.waitForTimeout(1000)`, `cy.wait(1000)`) are MAJOR because they slow tests and still flake. Replace with explicit waits: `waitForSelector`, `waitForFunction`, `waitForResponse`, `findBy*`, or `waitFor(() => ...)` with a real assertion.
- **E2E assertions verify observable postconditions**: Each E2E test must assert something the user can observe (UI text, URL, network outcome, absence/presence of an element) rather than "the code ran".

## Evidence Requirements

Every finding MUST include:
1. **Exact file path** and **line number**
2. **Quoted snippet** (3-5 lines)
3. **Rule violated** (from checks above)
4. **Fix**: specific action or code suggestion
5. **Self-Correction**: "Can I point to exact line? Did I read the file myself?"

If you cannot satisfy 1-4, you MUST NOT report the finding.

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

Compact one-line form for pre-commit output:

```text
[SEVERITY] [CONFIDENCE] Title | File:line | Rule | Evidence: "quoted snippet" | Fix: action
```

## Severity and Confidence

| Severity | Meaning |
|----------|---------|
| **BLOCKER** | Change/release must not proceed; immediate action required |
| **CRITICAL** | High impact; fix in the current iteration |
| **MAJOR** | Degradation or defect; schedule the fix |
| **MINOR** | Improvement; backlog |

Project-specific mapping:
- **BLOCKER**: Security vulnerability (XSS, injection), broken hook rules causing runtime error, secret leak, compilation error.
- **CRITICAL**: Missing `key` prop where required, `dangerouslySetInnerHTML` without sanitization, missing `useEffect` cleanup, race condition in async state, positive `tabIndex`.
- **MAJOR**: Missing dependency in `useEffect`/`useCallback`/`useMemo`, prop drilling instead of context, derived state anti-pattern, unused imports, `any` without justification, missing a11y label, inline object/array in render breaking memo.
- **MINOR**: Inconsistent naming, magic numbers/strings, missing JSDoc for exported hooks/components, inline styles instead of CSS class, formatting, trailing whitespace, unused variables, import order.

| Confidence | Meaning |
|------------|---------|
| **CONFIRMED** | Proven by evidence: file:line, reproduction, command output |
| **NEEDS_REVIEW** | Investigation signal; requires human judgment before action |

## Outputs and Downstream Consumer

**Output format:** findings list in the format above, then a verdict:
- **APPROVED**: 0 BLOCKER/CRITICAL/MAJOR
- **APPROVED_WITH_NITS**: only MINOR findings (style/formatting)
- **CHANGES_REQUESTED**: any BLOCKER/CRITICAL/MAJOR

**Consumer:** the user, right before commit — findings + verdict + whether it is safe to commit. The user fixes and stages; this skill never commits. In PR flow the same report is posted on the branch diff.

## Trigger or Schedule

- **Default trigger:** automatically right before every `git commit` when staged changes include frontend files.
- **Explicit invocation:** `/skill:frontend-code-review` or phrases: "frontend pre-commit review", "review my React changes", "check the frontend diff", "review TSX".
- **PR flow:** use `git diff main...[branch]` when invoked in CI / PR context.

## Limitations and Expected False Positives

- Version-sensitive checks (concurrent features, `satisfies`, modern hook patterns) produce false positives on older React/TypeScript — mark N/A after adaptation.
- `useMemo`/`useCallback`/`React.memo` judgments without measurements are **NEEDS_REVIEW** signals — premature optimization goes both ways.
- Scope is limited to the staged diff and its immediate context; cross-component contract breaks may be missed.
- A finding without verifiable file:line evidence is an investigation signal, not a defect.

> Optional interaction convention (agent-specific): some agents add `⚛️` to their starter-character stack while this skill is active. Not required — the skill is fully usable without emoji.
