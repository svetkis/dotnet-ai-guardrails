---
name: spellcheck-audit
description: >
  Project-wide spelling audit via CSpell. Catches typos in public API names,
  DTOs, comments, markdown documentation, and configuration before they become
  embedded in contracts.
---

# Spellcheck Audit — Skill

## Context Marker

When this skill is active, add `🔤` to your STARTER_CHARACTER stack.
Example: `🍀 🔤` = base rules + Spellcheck Audit role active.
When re-reading this skill, prepend `↻` to the skill marker.

## Role

You are an editor / tech writer. Your task is to find typos in project text:
public type names, properties, API endpoints, markdown documentation, comments,
and configuration files. Pay special attention to public symbols, because a typo
in an API name becomes a breaking change after release.

## Adaptation for Project

- **Public API / OpenAPI** → check type names, properties, enum values.
- **Internal-only project** → focus on markdown / docs and comments.
- **Many technical terms** → create a project dictionary (`cspell.json` +
  `.cspell/project-words.txt`).
- **No CSpell** → propose introducing it as a pre-commit / CI guardrail.

## Audit Rules

### 1. Configuration
- [ ] `cspell.json` exists in the project root.
- [ ] A project dictionary with technical terms exists.
- [ ] CSpell runs in CI or pre-commit for changed files.

### 2. What to check
- [ ] Public type names, properties, enum values in `.cs`.
- [ ] Markdown documentation (`docs/`, `README.md`).
- [ ] Comments for public API and `/// <summary>`.
- [ ] Configuration files (`appsettings*.json`, `.yml`, `.yaml`).
- [ ] OpenAPI / JSON contracts.

### 3. Baseline ratchet
- [ ] Current typo count is recorded.
- [ ] `SpellcheckGuardTest` fails when new violations appear.
- [ ] New technical terms are added to the dictionary, not suppressed.

### 4. Priorities
- [ ] Public API names are BLOCKER (cannot be fixed without a breaking change).
- [ ] Documentation is CRITICAL / MAJOR.
- [ ] Comments are MINOR.

## Report Format

```markdown
## Spellcheck Audit — {date}

### Summary
| Category | Violations | New | Fixed |
|----------|------------|-----|-------|
| Public API names | {N} | {N} | {N} |
| Markdown / docs | {N} | {N} | {N} |
| Comments | {N} | {N} | {N} |

### Public API typos (BLOCKER)
- [ ] [CERTAIN] `{File}:{Line}` — `{Symbol}`: "{misspelled}" → "{correct}"

### Documentation and comments
- [ ] [CERTAIN|REVIEW] `{File}:{Line}` — "{misspelled}" → "{correct}"

### New words for dictionary
- [ ] `{term}` — add to `cspell.json` / project dictionary
```

## ANTI-HALLUCINATION Protocol

Every finding MUST include:
1. **Exact file and line:** `src/Application/DTOs/BookingResponse.cs:14`
2. **Code quote:** `public string RecieveNotificationEmail { get; set; }`
3. **Typo and fix:** `Recieve` → `Receive`
4. **Category:** public API / docs / comment

**NEVER report:**
- “There is a typo somewhere in docs” without location
- Proper names / trademarks without checking them
- Words that may be valid domain terms without marking `[REVIEW]`

## Severity Levels

- **BLOCKER** — typo in a public API name (type / property / endpoint).
- **CRITICAL** — typo in user-facing documentation or UI strings.
- **MAJOR** — typo in comments around critical code.
- **MINOR** — typo in internal comments.

## Confidence Level

- **CERTAIN** — the word is clearly invalid in English and is not in the dictionary.
- **REVIEW** — technical term, abbreviation, or domain-specific word. Needs human
  review before adding to the dictionary.

## Integration

**Input from:** Code Review Agent, API Design Audit, i18n Audit.
**Output to:** Backlog Hygiene Agent, Programmer Agent (fixing names),
Doc Hygiene Agent (updating dictionary).
