#!/usr/bin/env bash
# .claude/skills/verify-complete/scripts/verify.sh
#
# Full verification pipeline — run before marking autonomous work
# complete. Three steps, stop-on-failure:
#
#   1. Auto-fix     — ruff check --fix + ruff format
#   2. Verify       — ruff format --check + ruff check + mypy src
#   3. Test         — pytest (integration tests skip without
#                     ROWLAND_API_KEY; see conftest.py)
#
# All steps run from python/ via `uv run`.
#
# Exit codes:
#   0  — all green, safe to mark complete (also early-exit when no
#        Python / build / lock files are dirty)
#   1  — a step failed; output above explains which
#
# Usage (from repo root):
#   bash .claude/skills/verify-complete/scripts/verify.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../../.." && pwd)"
PY_DIR="$REPO_ROOT/python"

if [ ! -f "$PY_DIR/pyproject.toml" ]; then
  echo "error: could not find python/pyproject.toml at $REPO_ROOT" >&2
  echo "hint: run this script from the rowland-sdk repo root." >&2
  exit 1
fi

# Early-exit when the working tree has no python/build/lock changes.
# Keeps the PostToolUse hook cheap on edits that touch only docs,
# .claude/, .github/, etc.
if git -C "$REPO_ROOT" rev-parse --git-dir >/dev/null 2>&1; then
  # git status --porcelain emits `XY <path>`, so we match on the path
  # tail rather than anchoring to start-of-line.
  if ! git -C "$REPO_ROOT" status --porcelain \
       | grep -qE ' python/.*\.py$| python/pyproject\.toml$| python/uv\.lock$'; then
    exit 0
  fi
fi

banner() {
  echo ""
  echo "━━━ $1 ━━━"
}

# Snapshot git state before auto-fix so we can report if files changed.
PRE_FIX_STATUS=""
if git -C "$REPO_ROOT" rev-parse --git-dir >/dev/null 2>&1; then
  PRE_FIX_STATUS="$(git -C "$REPO_ROOT" status --porcelain)"
fi

# Step 1 — auto-fix (writes in place)
banner "1/3 auto-fix (ruff check --fix + ruff format)"
(cd "$PY_DIR" && uv run ruff check --fix . && uv run ruff format .)

# Step 2 — verify (strict, mirrors CI)
banner "2/3 verify (ruff format --check + ruff check + mypy src)"
(cd "$PY_DIR" && uv run ruff format --check . && uv run ruff check . && uv run mypy src)

# Step 3 — tests
# Integration suite uses ROWLAND_API_KEY; conftest.py issues
# pytest.skip if unset, so this exits 0 locally without a key.
banner "3/3 tests (pytest)"
if [ -z "${ROWLAND_API_KEY:-}" ]; then
  echo "note: ROWLAND_API_KEY not set — integration tests will skip."
fi
(cd "$PY_DIR" && uv run pytest)

echo ""
echo "all checks passed — safe to mark complete."

# Flag newly modified files from the auto-fix step so they get committed.
if git -C "$REPO_ROOT" rev-parse --git-dir >/dev/null 2>&1; then
  POST_FIX_STATUS="$(git -C "$REPO_ROOT" status --porcelain 2>/dev/null || true)"
  if [ "$PRE_FIX_STATUS" != "$POST_FIX_STATUS" ]; then
    echo ""
    echo "note: auto-fix modified files in working tree. Review & commit before marking done:"
    git -C "$REPO_ROOT" status --short
  fi
fi
