# Analyzer Tests Audit — Checklist

## Before Starting
- [ ] List of the project's custom analyzers / linters
- [ ] List of diagnostic / rule IDs
- [ ] Version of analyzer dependencies

## Analyzer Coverage
- [ ] Every diagnostic / rule ID has a positive test
- [ ] Every diagnostic / rule ID has a negative test
- [ ] Whitelists / exceptions have dedicated tests
- [ ] Configurable parameters are covered by tests

## Test Quality
- [ ] Tests verify exact diagnostic span / location
- [ ] Correct reference fixtures / assemblies are used
- [ ] Code fix providers / autofixes are covered by tests

## Regression Guard
- [ ] Analyzer tests run in CI
- [ ] Analyzer package updates are accompanied by test runs

## Inventory
- [ ] Analyzer list and diagnostic / rule IDs are documented
- [ ] For each ID it is documented: what it catches, why, and which test covers it

## Report Format
- [ ] Summary by diagnostic / rule IDs
- [ ] Uncovered analyzers
- [ ] Weak tests
- [ ] Recommendations
