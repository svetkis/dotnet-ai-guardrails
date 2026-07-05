# Release Readiness Audit — Checklist

## Before Starting
- [ ] Release / beta date is defined
- [ ] Results from other audits are collected
- [ ] Release owner is known

## P0 Blockers
- [ ] Security audit: no open P0s
- [ ] Performance audit: no open P0s
- [ ] DBA / schema audit: no open P0s
- [ ] API design audit: no open P0s
- [ ] Test audit: critical paths are covered

## Release Artifacts
- [ ] Project documentation (`AGENTS.md` or equivalent) is current
- [ ] Deployment document exists
- [ ] CI / CD pipeline is configured
- [ ] API schema snapshot / contract is current
- [ ] Smoke tests pass

## Runtime Guardrails
- [ ] Health / ready endpoint works (if applicable)
- [ ] Security headers are configured
- [ ] Rate limiting is enabled (if applicable)
- [ ] Logging does not contain PII / sensitive data

## Human Judgment
- [ ] Product / UX approved edge cases
- [ ] Support / ops is aware of the risks
- [ ] Runbook exists for critical scenarios

## Report Format
- [ ] Status: READY / CONDITIONAL / NOT READY
- [ ] P0 / P1 list with owners and deadlines
- [ ] Artifact table
- [ ] GO / NO-GO recommendation
