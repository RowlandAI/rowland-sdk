---
name: release-process
description: >
  Reference for cutting a release of the rowland-sdk PyPI package.
  Covers the `mise run release` flow (bump → commit → tag → push →
  open Release draft), the version-of-truth rule (`python/pyproject.toml`
  is the only place; `rowland.__version__` reads it at runtime via
  `importlib.metadata`), and the GitHub Release → PyPI publish
  trigger. TRIGGER whenever the
  user asks to "cut a release", "publish to PyPI", "tag a version",
  "bump the version", "release a patch / minor / major", "push a
  new version", or is reviewing changes to `python/pyproject.toml`,
  `python/uv.lock`, `python/src/rowland/__init__.py`, the `release`
  task in `mise.toml`, or `.github/workflows/publish.yaml`. Also
  trigger when the user reports a publish failure or a version
  mismatch between PyPI / pyproject / `__version__`.
allowed-tools: Read, Grep, Edit, Bash(git log:*), Bash(git status:*), Bash(git diff:*), Bash(git tag:*), Bash(gh release list:*), Bash(gh release view:*), Bash(gh run list:*), Bash(uv version:*), Bash(uv pip:*), Bash(mise tasks:*), Bash(mise run release:*)
---

# Cutting a release

The rowland-sdk publishes to PyPI as `rowland-sdk`. Releases are
driven from `main` and triggered by publishing a GitHub Release —
**not** by pushing tags directly. This skill is the operator's
runbook.

## Pipeline at a glance

```
edit on main → mise run release <bump>
                  ├─ uv version --bump (writes python/pyproject.toml + uv.lock)
                  ├─ git commit -am "chore: release vX.Y.Z"
                  ├─ git tag vX.Y.Z
                  └─ git push origin main vX.Y.Z   ← CI runs here
                        ↓
                  GitHub Release draft opens in browser
                        ↓
                  human clicks "Publish release"  ← gates PyPI publish
                        ↓
                  .github/workflows/publish.yaml runs on release.published
                        ├─ hatch build
                        └─ pypa/gh-action-pypi-publish → PyPI (Trusted Publisher / OIDC)
```

The `mise run release` task lives in `mise.toml`. It enforces:
clean working tree, on `main`, fast-forward pull, then bump/commit/
tag/push. After it finishes, the GitHub Release **draft** is opened
in the browser — the actual PyPI publish doesn't happen until a
human clicks "Publish release."

## Version source of truth

**`python/pyproject.toml` `project.version` is the only source of
truth.** That's what `uv version --bump` writes, what the
`mise.toml` release task reads (`uv version --short`), what the
git tag is derived from, and what `hatch build` ships to PyPI.

**`rowland.__version__` derives from the installed package
metadata at import time.** `__init__.py` calls
`importlib.metadata.version("rowland-sdk")`, which reads the
dist-info written by `pip install` / `uv pip install -e .`. No
manual lockstep update is needed — bumping `pyproject.toml` and
running an install (which the CI build job does, and which
contributors do via `uv pip install -e ".[dev,test]"`) is enough.
If `rowland.__version__` reports the wrong value, the fix is to
reinstall the package, not to edit `__init__.py`.

In the rare case the package isn't installed (running from a raw
source checkout without `pip install`), `__version__` falls back
to `"0.0.0+unknown"`. That's intentional — better than crashing on
import, and a clear signal something is wrong with the install.

## Running the release

From a clean `main`:

```bash
mise run release patch   # 1.1.0 → 1.1.1
mise run release minor   # 1.1.0 → 1.2.0
mise run release major   # 1.1.0 → 2.0.0
```

Pre-flight (the task enforces these, fail-fast):

1. Current branch is `main`.
2. Working tree is clean (`git status --porcelain` empty).
3. `git pull --ff-only origin main` succeeds.

Post-flight (the task does these):

1. `cd python && uv version --bump <bump>` — writes
   `python/pyproject.toml` + `python/uv.lock`.
2. Reads the new version: `NEW_VERSION=$(uv version --short)`.
3. `git commit -am "chore: release v$NEW_VERSION"`.
4. `git tag v$NEW_VERSION`.
5. `git push origin main v$NEW_VERSION`.
6. Prints + opens the Release draft URL:
   `https://github.com/RowlandAI/rowland-sdk/releases/new?tag=v$NEW_VERSION&title=v$NEW_VERSION`.

The draft is empty by default. In the GitHub UI, click
**Generate release notes** (optional but recommended) and then
**Publish release**. Publishing fires `release.published`, which
triggers `publish.yaml` → PyPI.

### Why "draft → publish" rather than tag-driven publish

Two reasons baked into `publish.yaml`:

1. `on: release: types: [published]` — only **published** releases
   trigger the workflow. A pushed tag alone does nothing.
2. The `publish` job uses `environment: pypi` which requires the
   Trusted Publisher relationship configured on PyPI side. Tags
   pushed by mistake therefore can't accidentally ship.

If the workflow doesn't run after publishing the Release, check
that the Release was created against the expected tag and that the
`pypi` environment exists in the repo's GitHub settings.

## When NOT to use the release task

- **Hotfix off a tag** — if `main` has unshippable work and you
  need to patch the last release, branch off the tag, cherry-pick
  the fix, and cut a release from there manually (`uv version
  --bump patch`, commit, tag, push the tag only — don't push the
  branch to `main`). Then forward-port the fix to `main`. The
  `mise run release` task refuses to run off-`main`.
- **Re-publishing the same version** — PyPI rejects duplicate
  uploads. If a publish failed for transient reasons, debug the
  workflow; don't rev the version just to retry the upload.
- **Pre-releases** (`1.2.0rc1`, alphas, etc.) — the task only
  supports `major`/`minor`/`patch`. For a pre-release, edit
  `python/pyproject.toml` directly and tag manually. The current
  workflow has no pre-release gating, so it will publish to PyPI
  the same way — only do this if you actually want it on PyPI.

## Hand-bumping (when the task can't run)

If `mise` isn't available, replicate the steps directly:

```bash
cd python
uv version --bump patch
NEW_VERSION=$(uv version --short)
cd ..
git commit -am "chore: release v$NEW_VERSION"
git tag "v$NEW_VERSION"
git push origin main "v$NEW_VERSION"
```

## Linking commits to releases

```bash
git tag --list 'v*'                          # see published versions
gh release list                              # see release status (draft vs published)
gh release view v1.1.0                       # inspect a release
gh run list --workflow=publish.yaml          # see PyPI publish workflow runs
git log v1.0.0..v1.1.0 --oneline             # diff between releases
```

## Pre-release checklist

Before running the task:

1. **Tests pass on `main`** — `gh run list --workflow=ci.yaml
   --branch main --limit 1` should show a green run for the
   current HEAD.
2. **No `__init__.py` edit needed.** `rowland.__version__` is
   derived from installed package metadata via
   `importlib.metadata`. The release task bumps `pyproject.toml` +
   `uv.lock`; the next install picks up the new version
   automatically.
3. **No unreleased breaking changes that don't match the bump.**
   See `public-api-surface` for what counts as breaking. Removed
   methods, renamed exports, or changed return shapes → `major`.
   New optional kwargs, new methods, new exports → `minor`. Bug
   fixes that don't change the public surface → `patch`.
4. **README / examples are accurate** for the new surface. The
   `python/examples/` directory ships as-is; broken examples
   embarrass on PyPI.

## After publish

1. **Verify on PyPI**: https://pypi.org/project/rowland-sdk/ should
   show the new version within a couple minutes.
2. **Smoke-install**: `pip install -U rowland-sdk` in a clean
   venv, then `python -c "import rowland; print(rowland.__version__)"`
   — confirms the package installs and reports the new version
   (read from the installed dist-info).
3. **Announce** in whichever channel the team uses. Not automated.

## Gotchas

- `mise.toml`'s `release` task uses `-am` for the commit. That
  catches modifications to **already-tracked** files only — if
  you've added a new file as part of the release prep, stage it
  manually before running the task.
- `uv version --bump` doesn't know about pre-release suffixes.
  Don't run it against `1.2.0rc1`; it'll produce something
  surprising.
- The publish workflow uses `pypa/gh-action-pypi-publish@release/v1`
  with `id-token: write` permission — it relies on PyPI Trusted
  Publishers (OIDC), no API token. Don't add a token-based
  fallback without coordinating with the PyPI project maintainers.
- Tags are immutable in practice. If `mise run release` succeeds
  in tagging but the push fails partway through, **don't re-run
  the task** — it'll try to bump again. Investigate the push state
  with `git tag --list` and `git log origin/main..HEAD` first.
