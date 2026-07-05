# Test Audit — Checklist

## Before Start
- [ ] Stack known (TUnit / xUnit / NUnit)
- [ ] List of services, endpoints, jobs obtained

## Services
- [ ] Every service has a test file
- [ ] Public methods covered (not only happy path)
- [ ] Errors, null, empty collections covered
- [ ] Race condition tests (if applicable)

## Endpoints
- [ ] Every endpoint has an integration test
- [ ] Statuses tested: 200, 400, 401, 403, 404, 409, 500
- [ ] Input DTO validation tested

## Background Jobs
- [ ] Every Job has a test
- [ ] Empty/partial data and errors covered

## Regression Tests (BUG###_)
- [ ] Every `fix:` commit has `BUG*Tests.cs`
- [ ] BUG tests reproduce the bug (break code → test fails)
- [ ] No dead BUG tests (always pass)

## Characterization Tests
- [ ] Exist for critical algorithms
- [ ] Are current (not stale)

## Architectural Tests
- [ ] Every AGENTS.md rule has a guardrail test
- [ ] Ratchet tests did not fail
