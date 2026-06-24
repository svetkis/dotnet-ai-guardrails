# Spellcheck Audit — Checklist

## Before Starting
- [ ] `cspell` is installed (global or local)
- [ ] `cspell.json` is created in the project root
- [ ] Project dictionary is created

## Configuration
- [ ] `cspell.json` covers required extensions (cs, md, ts, tsx, json, yml)
- [ ] Project dictionary is connected
- [ ] CSpell runs in CI / pre-commit

## What to Check
- [ ] Public type names, properties, enum values
- [ ] Markdown documentation
- [ ] Comments for public API
- [ ] Configuration files
- [ ] OpenAPI / JSON contracts

## Baseline Ratchet
- [ ] Current typo count is recorded
- [ ] `SpellcheckGuardTest` is added to the test project
- [ ] New terms are added to the dictionary

## Report Format
- [ ] Summary by category
- [ ] Public API typos (BLOCKER)
- [ ] Documentation and comments
- [ ] New words for the dictionary
