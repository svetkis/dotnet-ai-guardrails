# Frontend Code Review — Checklist

## Before Start
- [ ] Staged diff obtained (`git diff --cached`)
- [ ] Task context known (backlog item / spec)
- [ ] React version (>= 18) and TypeScript version (>= 4.8) confirmed; legacy checks marked N/A
- [ ] Skill activated automatically before `git commit` or explicitly via `/skill:frontend-code-review`

## Pre-commit / Trigger
- [ ] Staged changes include frontend files (*.tsx, *.ts, *.jsx, *.js, *.css, *.scss, *.json)
- [ ] Backend-only changes are skipped (use `code-review`)
- [ ] When staged diff is empty, agent reports nothing and does not block commit
- [ ] Agent does NOT run `git commit` itself

## React / Hooks
- [ ] Hooks called only at top level, not in loops/conditions
- [ ] Custom hooks named with `use` prefix
- [ ] `useEffect` has exhaustive deps array or justified comment
- [ ] `useEffect` not used for derived state
- [ ] `useEffect` cleanup for subscriptions, timers, listeners, `AbortController`
- [ ] `useState`: no direct mutation, functional updater used where needed
- [ ] `useMemo` / `useCallback` justified, not "just in case"
- [ ] Context split by concern, default value matches shape

## Rendering / JSX
- [ ] `key` uses stable unique IDs, not index (unless list is static)
- [ ] Conditional rendering guarded against `0` / `""` (`!!condition` or ternary)
- [ ] `dangerouslySetInnerHTML` only with sanitization
- [ ] No inline objects/arrays/functions in props breaking memo
- [ ] No direct DOM manipulation outside refs/effects
- [ ] Event handlers named where important for perf/readability

## TypeScript
- [ ] No implicit `any` (`strict: true`)
- [ ] `!` non-null assertion justified
- [ ] `as` casts justified and commented
- [ ] Exported components/hooks have return types or strong inference
- [ ] Discriminated unions preferred over `| undefined`

## Performance
- [ ] `React.memo` justified
- [ ] Heavy components / routes lazy loaded
- [ ] Context does not pass new objects/arrays without memoization
- [ ] Images lazy loaded with explicit dimensions

## Accessibility
- [ ] Clickable elements are `<button>`, not `<div onClick>`
- [ ] Images have `alt` or `role="presentation"`
- [ ] Inputs have `<label>` or `aria-label`/`aria-labelledby`
- [ ] Modals/dropdowns trap and restore focus
- [ ] No positive `tabIndex`
- [ ] Status/errors not conveyed by color alone

## Security
- [ ] No `innerHTML`, `eval`, `new Function` with user input
- [ ] `href`/`src` do not contain raw user input, no `javascript:` URLs
- [ ] Secrets not hardcoded
- [ ] New npm dependencies not suspicious

## State Management
- [ ] Prop drilling no deeper than 2 intermediate components without usage
- [ ] Global state justified, local UI state not in Redux/Zustand without reason
- [ ] Server state via RTK Query / TanStack Query / SWR, not manual cache
- [ ] Immutability in reducer/store

## Forms
- [ ] Controlled inputs (`value` + `onChange`)
- [ ] Validation accessible and triggered on submit/blur
- [ ] Submit handler prevents default, manages loading/error, disables re-submission

## Styling
- [ ] Single approach per project (CSS Modules / Tailwind / styled-components)
- [ ] No inline styles without reason
- [ ] Responsive/mobile-first

## Testing
- [ ] React Testing Library + Vitest/Jest, no Enzyme
- [ ] Prefer `getByRole`/`getByLabelText` over `getByTestId`
- [ ] Uses `@testing-library/user-event`
- [ ] Async tests correctly wait for state (`waitFor`, `findBy*`)
- [ ] No tautological assertions (`expect(true)`, `expect(1).toBe(1)`, etc.)
- [ ] No body-only checks: every response-body parse / DOM locator is followed by an explicit assertion on an observable postcondition
- [ ] No fixed sleeps (`waitForTimeout`, `cy.wait(ms)`); explicit condition waits preferred
- [ ] E2E assertions verify observable user-facing postconditions, not just code execution

## Report Format

```markdown
## Frontend Code Review — {date}

### BLOCKER
- [ ] {description} → {file:line}

### CRITICAL
- [ ] {description} → {file:line}

### MAJOR
- [ ] {description} → {file:line}

### Verdict
- [ ] APPROVED
- [ ] APPROVED_WITH_NITS
- [ ] CHANGES_REQUESTED
```
