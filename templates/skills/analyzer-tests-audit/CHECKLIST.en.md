# Analyzer Tests Audit — Checklist

## Before Starting
- [ ] List of the project's custom Roslyn analyzers
- [ ] List of diagnostic IDs
- [ ] Version of `Microsoft.CodeAnalysis.*` packages

## Analyzer Coverage
- [ ] Every diagnostic ID has a positive test
- [ ] Every diagnostic ID has a negative test
- [ ] Whitelists / exceptions have dedicated tests
- [ ] Configurable parameters are covered by tests

## Test Quality
- [ ] Tests verify exact diagnostic span
- [ ] Correct `ReferenceAssemblies` are used
- [ ] Code fix providers are covered by tests

## Regression Guard
- [ ] Analyzer tests run in CI
- [ ] Roslyn package updates are accompanied by test runs

## Inventory
- [ ] Analyzer list and diagnostic IDs are documented
- [ ] For each ID it is documented: what it catches, why, and which test covers it

## Report Format
- [ ] Summary by diagnostic IDs
- [ ] Uncovered analyzers
- [ ] Weak tests
- [ ] Recommendations
