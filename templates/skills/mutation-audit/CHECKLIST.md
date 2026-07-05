# Mutation Audit — Checklist

## Before Starting
- [ ] `dotnet-stryker` is installed (global or local)
- [ ] 1–3 critical assemblies are identified
- [ ] Runtime budget is known

## Configuration
- [ ] `stryker-config.json` exists for every assembly
- [ ] `dotnet-tools.json` contains `dotnet-stryker`
- [ ] Configuration is compatible with the project's test framework

## Baseline and Ratchet
- [ ] Mutation score baseline is recorded
- [ ] `MutationGuardTest` is added to the test project
- [ ] Minimum threshold is defined (for example, 70%)

## Mutant Analysis
- [ ] Top surviving mutants are reviewed
- [ ] Test is added / strengthened for each one
- [ ] False-positive (equivalent) mutants are documented

## Integration
- [ ] Stryker runs before release / once per sprint
- [ ] Report is stored as a CI artifact
- [ ] Regular PRs are not blocked by runtime

## Report Format
- [ ] Summary by assembly
- [ ] Top surviving mutants
- [ ] False positives
