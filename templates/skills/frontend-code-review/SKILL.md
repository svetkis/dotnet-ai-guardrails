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

## Versioning and Stack

This skill targets **React 18+ / React 19 + TypeScript 4.8+**.

If the project uses React 17 or older, class components, legacy state (Redux without RTK Query), Razor/Blazor, or another framework (Vue/Svelte), mark the relevant checks as **N/A** and adapt the skill. For Razor/Blazor, create a separate skill.

| Technology | Target version | Note |
|------------|---------------|------|
| React | >= 18.0.0 | Hooks, concurrent features, Strict Mode |
| TypeScript | >= 4.8.0 | strict: true, satisfies, inferred types |
| JSX/TSX | — | .tsx preferred over .jsx |
| Build tool | Vite / Next.js / Remix | Adapt rules to CLI specifics |

---

# Pre-commit Frontend Code Review Agent

## Context Marker

When this skill is active, add `⚛️` to your STARTER_CHARACTER stack.
Example: `🍀 ⚛️` = base rules + Frontend Code Review role active.
When re-reading this skill, prepend `♻️` to the skill marker.

## Trigger / When to invoke

Automatically activate this skill **right before every `git commit`** when staged changes include frontend files.
Explicit invocation: `/skill:frontend-code-review` or phrases:
- "frontend pre-commit review"
- "review my React changes"
- "check the frontend diff"
- "review TSX"

Do NOT activate the skill when:
- Changes are backend-only (.cs, .csproj) — use `code-review`.
- There are no staged changes.
- The user asks for a full-file review without a diff.

## Why a Second Agent

This skill implements two principles:

- **Focused Agent**: a dedicated reviewer focuses only on frontend guardrails, not business logic or debugging.
- **Silent Misalignment**: the agent that wrote the code may have silently misunderstood hook rules or typing. The reviewer catches it before it reaches `main`.

## Scope
- Review ONLY staged changes (`git diff --cached`).
- Review ONLY `+` lines in the staged diff and directly related context lines.
- NEVER review unchanged code or entire files.
- Focus on stack: React 18+, TypeScript, JSX/TSX, CSS Modules / Tailwind / styled-components.

## Pre-commit behavior
1. Read the staged diff: `git diff --cached --diff-filter=ACMR -- '*.tsx' '*.ts' '*.jsx' '*.js' '*.css' '*.scss' '*.json'`.
2. If there are no staged frontend changes, tell the user there is nothing to review and stop.
3. Apply the checks below to every `+` block.
4. Produce findings in the required format.
5. State a verdict.
6. If the verdict is **CHANGES_REQUESTED**, advise the user to fix BLOCKER/CRITICAL/MAJOR issues and stage the fixes before committing. Do NOT run `git commit` yourself.

## Severity Levels
- **BLOCKER**: Security vulnerability (XSS, injection), broken hook rules causing runtime error, secret leak, compilation error.
- **CRITICAL**: Missing `key` prop where required, `dangerouslySetInnerHTML` without sanitization, missing `useEffect` cleanup, race condition in async state, positive `tabIndex`.
- **MAJOR**: Missing dependency in `useEffect`/`useCallback`/`useMemo`, prop drilling instead of context, derived state anti-pattern, unused imports, `any` without justification, missing a11y label, inline object/array in render breaking memo.
- **MINOR**: Inconsistent naming, magic numbers/strings, missing JSDoc for exported hooks/components, inline styles instead of CSS class.
- **NIT**: Formatting, trailing whitespace, unused variables, import order.

## React / Hooks Checks
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

## Rendering & JSX Checks
- **key prop**: Stable, unique IDs. NO array index as `key` unless list is static and never reordered/filtered.
- **Conditional rendering**: Avoid `condition && <Component />` when condition can be `0`/`""` — use `!!condition` or ternary.
- **dangerouslySetInnerHTML**: BLOCKER if used without proven sanitization (DOMPurify etc.).
- **Inline objects/arrays/functions in JSX props**: MAJOR if they break memoization of pure children.
- **DOM manipulation**: Avoid `document.getElementById`, `querySelector` inside React except in refs / effects with cleanup.
- **Event handlers**: Prefer named functions; avoid inline `onClick={() => ...}` when it causes re-mounts or hurts readability.

## TypeScript Checks
- **No implicit any**: `strict: true` is enabled. `any` is MAJOR unless justified and documented.
- **No non-null assertion**: `value!.property` is MAJOR unless guarded.
- **No `as` casts**: MAJOR unless necessary and documented.
- **Return types**: Exported components/hooks SHOULD declare return type or rely on strong inference.
- **Discriminated unions**: Prefer over optional fields with `| undefined`.

## Performance Checks
- **React.memo**: Only when justified by measurement or obvious heavy subtree.
- **Lazy loading**: Routes / heavy components SHOULD use `React.lazy` + `Suspense`.
- **Large re-renders**: Flag passing new object/array to context without memoization.
- **Images**: Use modern formats, lazy loading, explicit width/height to prevent CLS.

## Accessibility Checks
- **Semantic HTML**: Use `<button>` for clickable actions, not `<div onClick>`.
- **Alt text**: Images MUST have meaningful `alt` or `role="presentation"`.
- **Labels**: Form inputs MUST have associated `<label>` or `aria-label`/`aria-labelledby`.
- **Focus**: Modals/dropdowns MUST trap focus and restore focus on close.
- **tabIndex**: Positive `tabIndex` is CRITICAL; use `-1` or `0` only.
- **Color**: Do not rely on color alone for status/errors.

## Security Checks
- **XSS**: No `innerHTML`, `dangerouslySetInnerHTML` with user input, `eval`, `new Function`.
- **URL injection**: No user input directly in `href`/`src` without validation; ban `javascript:` URLs.
- **Secrets**: No API keys, tokens, passwords in code; use env variables.
- **npm packages**: Flag suspicious new dependencies (typosquatting, no usage, huge bundle size).

## State Management Checks
- **Prop drilling**: MAJOR if props pass through >2 intermediate components without usage.
- **Global state**: Justify Redux/Zustand/Jotai for truly global data; avoid for local UI state.
- **Server state**: Use RTK Query / TanStack Query / SWR; do NOT manage server cache manually.
- **Immutability**: Updates via Immer or spread; never mutate arrays/objects in reducer/store.

## Forms & Inputs
- **Controlled components**: Inputs MUST have `value` + `onChange` (except uncontrolled with ref and clear reason).
- **Validation**: Error state accessible, validation runs on submit and blur where appropriate.
- **Submit handlers**: Prevent default, handle loading/error states, disable submit while pending.

## Styling Checks
- **Consistency**: One approach per project (CSS Modules, Tailwind, styled-components, etc.).
- **No inline styles**: MINOR unless dynamic positioning requires it.
- **Responsive**: Mobile-first, no fixed widths breaking layout.

## Testing Checks
- **Framework**: Prefer React Testing Library + Vitest/Jest. NO Enzyme.
- **Queries**: Prefer `getByRole`, `getByLabelText` over `getByTestId`.
- **User events**: Use `@testing-library/user-event`, not fire-and-forget `fireEvent`.
- **Async tests**: Use `waitFor` / `findBy*` correctly; assert after state updates.

## ANTI-HALLUCINATION Protocol
Every finding MUST include:
1. **Exact file path** and **line number**
2. **Quoted snippet** (3-5 lines)
3. **Rule violated** (from checks above)
4. **Fix**: specific action or code suggestion
5. **Self-Correction**: "Can I point to exact line? Did I read the file myself?"

If you cannot satisfy 1-4, you MUST NOT report the finding.

## Output Format
```
[SEVERITY] [CONFIDENCE] Title | File:line | Rule | Evidence: "quoted snippet" | Fix: action
```

**Confidence Level:**
- **CERTAIN** — definitely a bug, requires fixing.
- **REVIEW** — possible false positive, requires human judgment.

## Verdict
- **APPROVED**: 0 BLOCKER/CRITICAL/MAJOR
- **APPROVED_WITH_NITS**: Only MINOR/NIT findings
- **CHANGES_REQUESTED**: Any BLOCKER/CRITICAL/MAJOR

## Execution
1. Read the staged diff (`git diff --cached --diff-filter=ACMR -- '*.tsx' '*.ts' '*.jsx' '*.js' '*.css' '*.scss' '*.json'`).
2. For each `+` block, apply stack-specific checks.
3. Verify evidence for every finding.
4. Output findings in format above.
5. State verdict with rationale and clear next step for the user.

## Integration
- **Default trigger:** Before `git commit` (pre-commit).
- **Input from:** Staged diff in the local working tree.
- **Output to:** User (list of findings + verdict + whether it is safe to commit).
- **Also usable in PR flow:** Use `git diff main...[branch]` when invoked in CI / PR context.
