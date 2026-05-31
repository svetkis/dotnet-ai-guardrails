# UX Audit — Checklist

## Before Start
- [ ] Key scenarios known (onboarding, booking, payment, cancellation)
- [ ] Access to frontend / bot / API response code obtained

## Scenario: New user
- [ ] Interruption at each step — what happens?
- [ ] Validation — specific errors (not generic)
- [ ] Empty states — has CTA?

## Scenario: Core action
- [ ] Empty list / no data — explanation + CTA
- [ ] Stale data — protected (slot taken, price changed)
- [ ] Double confirmation — prevents accidental action

## Scenario: Cancel / modify
- [ ] Already passed / already cancelled — specific feedback
- [ ] Alternatives exist (reschedule instead of cancel)

## Scenario: Payment
- [ ] Failed payment — specific error + retry
- [ ] Double payment / webhook — protected
- [ ] Expired subscription / trial — user sees explanation

## States and feedback
- [ ] Empty states — CTA or explanation
- [ ] Loading states — indicator at >1 sec
- [ ] Error states — specific error + action (Retry)
- [ ] Success states — user sees result
- [ ] Dead ends — always an exit (cancel, back, menu)

## UI race conditions
- [ ] Double press — does not create duplicate
- [ ] Fast input + submit — does not send old data
- [ ] Polling / refresh — does not overwrite new result

## API → Frontend contract
- [ ] API returns data for all UI states
- [ ] Flags for special states (`BookingPaused`, `TrialExpired`)
- [ ] Errors have machine-readable codes, not just text
