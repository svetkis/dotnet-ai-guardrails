#!/usr/bin/env bash
# check-skills.sh — schema-lint for installable skills in templates/skills/.
# Contract: templates/skills/SKILL-CONTRACT.md
set -u

SKILLS_DIR="${1:-templates/skills}"
FAIL=0

# Canonical section headings (case-insensitive substring match on heading lines).
REQUIRED_SECTIONS=(
  "purpose"
  "applicability"
  "required inputs"
  "procedure"
  "evidence requirements"
  "finding schema"
  "severity"
  "outputs"
  "trigger"
  "limitations"
)

shopt -s nullglob
for skill_dir in "$SKILLS_DIR"/*/; do
  name="$(basename "$skill_dir")"

  # skeptical-ai-bootstrap is a support-template bundle, not an installable skill.
  if [ "$name" = "skeptical-ai-bootstrap" ]; then
    continue
  fi

  skill="$skill_dir/SKILL.md"
  checklist="$skill_dir/CHECKLIST.md"

  if [ ! -f "$skill" ]; then
    echo "FAIL $name: SKILL.md missing"
    FAIL=1
    continue
  fi

  if [ ! -f "$checklist" ]; then
    echo "FAIL $name: CHECKLIST.md missing"
    FAIL=1
  fi

  # 1. YAML frontmatter with name and description.
  if ! head -1 "$skill" | grep -q '^---$'; then
    echo "FAIL $name: YAML frontmatter missing"
    FAIL=1
  else
    fm="$(awk 'NR==1 && /^---$/{f=1;next} f && /^---$/{exit} f' "$skill")"
    echo "$fm" | grep -q '^name:' || { echo "FAIL $name: frontmatter 'name' missing"; FAIL=1; }
    echo "$fm" | grep -q '^description:' || { echo "FAIL $name: frontmatter 'description' missing"; FAIL=1; }
  fi

  # 2. Forbidden legacy heading.
  if grep -qi 'anti-hallucination' "$skill"; then
    echo "FAIL $name: contains 'Anti-Hallucination' — rename to 'Evidence Requirements'"
    FAIL=1
  fi

  # 3. Required sections (headings only).
  headings="$(grep -i '^#\{1,3\} ' "$skill" || true)"
  for section in "${REQUIRED_SECTIONS[@]}"; do
    if ! echo "$headings" | grep -qi "$section"; then
      echo "FAIL $name: required section missing ('$section')"
      FAIL=1
    fi
  done

  # 4. Canonical confidence labels (forbid legacy CERTAIN/REVIEW markers).
  if grep -qE '\bCERTAIN\b|\[REVIEW\]' "$skill"; then
    echo "FAIL $name: legacy confidence labels (CERTAIN/REVIEW) — use CONFIRMED/NEEDS_REVIEW"
    FAIL=1
  fi
done

if [ "$FAIL" -eq 0 ]; then
  echo "OK: all skills pass the schema-lint."
else
  echo "Schema-lint FAILED."
fi
exit "$FAIL"
