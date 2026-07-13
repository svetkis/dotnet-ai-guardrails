#!/usr/bin/env bash
# check-guardrail-lifecycle.sh — stale suppressions and expired decision guards.
#
# 1. NuGetAuditSuppress entries must have an adjacent XML comment with
#    Owner: and Review: YYYY-MM-DD (risk acceptance with expiry).
# 2. Decision Guard records (**Review date:** YYYY-MM-DD) past due are reported
#    as warnings (they do not invalidate the decision — see DECISION-GUARDS.md).
set -u

FAIL=0
TODAY="$(date +%Y-%m-%d)"

# --- 1. Suppressions ---
while IFS= read -r line; do
    file="${line%%:*}"
    lineno="${line#*:}"; lineno="${lineno%%:*}"
    # Look at up to 5 lines above the suppression for an XML comment with Owner/Review
    context="$(sed -n "$((lineno > 5 ? lineno - 5 : 1)),${lineno}p" "$file")"
    if ! echo "$context" | grep -qi "owner:"; then
        echo "STALE SUPPRESSION (no owner): $file:$lineno"
        FAIL=1
    fi
    if ! echo "$context" | grep -qiE "review:[[:space:]]*[0-9]{4}-[0-9]{2}-[0-9]{2}"; then
        echo "STALE SUPPRESSION (no review date): $file:$lineno"
        FAIL=1
    else
        review="$(echo "$context" | grep -oiE "review:[[:space:]]*[0-9]{4}-[0-9]{2}-[0-9]{2}" | tail -1 | grep -oE "[0-9]{4}-[0-9]{2}-[0-9]{2}")"
        if [[ "$review" < "$TODAY" ]]; then
            echo "EXPIRED SUPPRESSION (review $review < $TODAY): $file:$lineno"
            FAIL=1
        fi
    fi
done < <(grep -rn "NuGetAuditSuppress" --include="*.csproj" --include="*.props" . || true)

# --- 2. Decision Guard review dates (warnings only) ---
while IFS= read -r line; do
    file="${line%%:*}"
    date_str="$(echo "$line" | grep -oE "[0-9]{4}-[0-9]{2}-[0-9]{2}" | head -1)"
    if [ -n "$date_str" ] && [[ "$date_str" < "$TODAY" ]]; then
        echo "WARNING: expired Decision Guard review date ($date_str): $file"
    fi
done < <(grep -rn "Review date:" --include="DECISION-GUARDS.md" . || true)

if [ "$FAIL" -eq 0 ]; then
    echo "OK: no stale suppressions."
else
    echo "Guardrail lifecycle check FAILED."
fi
exit "$FAIL"
