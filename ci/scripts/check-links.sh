#!/usr/bin/env bash
# check-links.sh — markdown link check for relative links.
# Scans tracked .md files; verifies that relative link targets exist.
set -u

FAIL=0

while IFS= read -r file; do
    dir="$(dirname "$file")"
    # Extract link targets: ](target) — skip images' alt text concerns (target-only match)
    while IFS= read -r raw; do
        target="${raw#](}"
        target="${target%)}"
        # Strip optional title: path "title"
        target="${target%% \"*}"
        # Skip external, anchors-only, mailto
        case "$target" in
            http://*|https://*|mailto:*|\#*) continue ;;
        esac
        # Strip anchor
        path="${target%%#*}"
        [ -z "$path" ] && continue
        # Decode %20
        path="${path//%20/ }"
        if [ ! -e "$dir/$path" ]; then
            echo "BROKEN LINK: $file -> $target"
            FAIL=1
        fi
    done < <(grep -oE '\]\([^)]+\)' "$file" || true)
done < <(git ls-files '*.md')

if [ "$FAIL" -eq 0 ]; then
    echo "OK: all relative markdown links resolve."
else
    echo "Link check FAILED."
fi
exit "$FAIL"
