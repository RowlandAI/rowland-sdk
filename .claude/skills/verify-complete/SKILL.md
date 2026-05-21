---
name: verify-complete
description: >
  Run the full auto-fix + lint + type-check + test pipeline before
  marking any autonomous work as complete in the rowland-sdk repo.
  TRIGGER automatically whenever Claude is about to declare a task,
  phase, or change "done", "complete", "ready to commit", "ready
  for PR", "verified", or "shipped". Also trigger when the user
  says "make sure it works", "run the checks", "verify it",
  "lint and test", or "ship it". MUST run (and pass) before
  reporting success on any code change — especially during
  autonomous execution modes such as /gsd-execute-phase,
  /gsd-autonomous, or long-running loop-based work. The pipeline
  mirrors the `.github/workflows/ci.yaml` jobs (`ruff`, `mypy`,
  `pytest`) so passing locally guarantees CI will pass.
allowed-tools: Bash(bash .claude/skills/verify-complete/scripts/verify.sh:*), Bash(mise run:*), Bash(uv run:*), Bash(ruff:*), Bash(mypy:*), Bash(pytest:*), Bash(git status:*), Bash(git diff:*), Bash(git add:*), Read
---

# Pre-completion verification

Before marking any code change complete, run the full verification
pipeline. This catches formatting, lint, type, and test regressions
that silent autonomous loops otherwise miss. It mirrors the exact
steps the `Continuous Integration` workflow (`.github/workflows/ci.yaml`)
runs on every push and PR — passing locally guarantees CI passes.

## The pipeline

Three steps, stop-on-failure. All commands run from `python/`:

1. **Auto-fix** — `ruff check --fix .` + `ruff format .` (writes)
2. **Verify** — `ruff format --check .` + `ruff check .` + `mypy src`
3. **Test** — `pytest`

If any step fails, stop. Do not mark work complete. Report the
failure output to the user and fix the root cause.

## Preferred command

Run the bundled script from the repo root:

```bash
bash .claude/skills/verify-complete/scripts/verify.sh
```

The script:

- Early-exits cleanly when the working tree has no `*.py`,
  `pyproject.toml`, or `uv.lock` changes — keeps the `PostToolUse`
  hook cheap on edits that touch only docs, `.claude/`, etc.
- Runs all three steps with clear section banners.
- Uses `uv run` directly (the `mise` tasks in this repo are thin
  aliases over the same commands — no value in routing through them).
- Exits non-zero on any failure (`set -euo pipefail`).
- At the end, reports any files modified by the auto-fix step so
  they get committed before the task is considered done.

## Manual invocation (fallback)

From `python/`:

```bash
uv run ruff check --fix . && uv run ruff format . \
  && uv run ruff format --check . && uv run ruff check . && uv run mypy src \
  && uv run pytest
```

Mise aliases (also from `python/` or the repo root):

```bash
mise run quality && mise run test
```

Note: `mise run quality` calls `ruff format` and `ruff check --fix`,
both of which **write**. It does not include the strict CI-style
`ruff format --check` / `ruff check` (no-fix) gates — `verify.sh`
adds those because that's what CI enforces.

## Tests and `ROWLAND_API_KEY`

The pytest suite is entirely integration tests against the live
Rowland API. `python/tests/integration/conftest.py` calls
`pytest.skip("ROWLAND_API_KEY environment variable not set")` when
the key is absent — so locally without a key, `pytest` exits 0
with every test skipped. This is intentional and matches CI's
behavior when the secret isn't injected (forks, dependabot PRs).

A green local run with no key set means "no regressions in code
that doesn't touch the API path" — not "the API integration
works." If your change touches `client.py` or `models.py`,
**export `ROWLAND_API_KEY` and re-run before declaring done.**

The `slow` mark gates the long-running
`test_document_processing_completion` test (polls until the
backend finishes processing the fixture PDF, up to 15 minutes).
It's included by default — skip it with `-m "not slow"` if you're
iterating quickly, but run the full suite before PR.

## Rules

1. **Never mark work complete without running this pipeline**
   during autonomous execution. "Tests pass" is not self-evident —
   prove it.
2. **Never skip a step** because "it was fine last time". State
   drift is why this exists.
3. If auto-fix modifies files, **commit those changes** before
   declaring done. The script flags this explicitly at the end.
4. **Do not disable ruff rules or add `# type: ignore`** to make
   the pipeline pass. The mypy config is strict on purpose
   (`disallow_untyped_defs`, `warn_return_any`, etc. — see
   `[tool.mypy]` in `pyproject.toml`). Fix the underlying issue.
5. If a step fails intermittently (flaky network on integration
   test), investigate — do not retry the pipeline hoping for green.
6. **Build verification** (`hatch build` + import smoke) lives in
   CI and is not part of the local pipeline. Run it manually if
   you're changing `pyproject.toml` packaging metadata.

## When to skip

Only skip when the change is genuinely non-code:

- Pure docs edits (`*.md`) with no Python changes
- `.gitignore` only
- `.claude/skills/` edits
- `assets/` only

In every other case — run the pipeline.
