---
name: public-api-surface
description: >
  Guardrail for changes to the public SDK surface that consumers
  of `rowland-sdk` import. The package is published to PyPI under
  semantic versioning — additive changes are safe, removals and
  rename-or-retype changes are breaking and require a major bump.
  TRIGGER on any edit to `python/src/rowland/__init__.py`,
  `python/src/rowland/client.py`, `python/src/rowland/models.py`,
  `python/src/rowland/exceptions.py`, or to the `__all__` list /
  any public class signature. Also trigger when the user asks to
  "rename this method", "remove this kwarg", "change the return
  type", "make this field required", "deprecate this", or "split
  out a module", or when reviewing a PR diff that touches those
  files. Anything imported by `from rowland import X` is a public
  promise — keep deprecation paths in mind, don't break consumers
  to land a refactor.
allowed-tools: Read, Grep, Glob, Edit, Bash(grep:*), Bash(git diff:*)
---

# Public API surface

`rowland-sdk` is a published library. The public surface is small
and easy to keep stable, but easy to break by accident — a rename
in `client.py`, a removed field on a model, or a tightened type
hint ships to every consumer the next time someone runs
`pip install -U rowland-sdk`. This skill is the guardrail.

## What counts as "public"

Anything reachable from `import rowland` without underscores:

| Surface | Source | Notes |
|---|---|---|
| Top-level exports | `src/rowland/__init__.py` `__all__` | The contract. If it's not in `__all__`, it's not public — even if it's importable. |
| Client class | `DocumentsApiClient` (`client.py`) | Constructor params, method names, parameter names, defaults, return types. |
| Models | `Document`, `DocumentExtractionResponse`, `PaginatedResponse[T]` (`models.py`) | Pydantic models — every field name, type, optionality. |
| Enums | `ProcessingStatus`, `DocumentType` (`models.py`) | String values are part of the contract (StrEnum) — they appear in user code as `doc.status == "success"`. |
| Exceptions | `RowlandError`, `RowlandHTTPError`, `RowlandAuthenticationError` (`exceptions.py`) | Class names and constructor signatures. Consumers do `except RowlandHTTPError`. |
| `__version__` | `__init__.py` (derived from installed package metadata) | Yes, consumers read it. Auto-tracks `pyproject.toml` via `importlib.metadata` — no manual update needed. See `[[release-process]]`. |

Anything not in `__all__` and not transitively reachable via a
public type signature is internal. The leading-underscore methods
on `DocumentsApiClient` (`_get_mime_type`, `_handle_response`,
`_client`, `_api_key`, `_base_url`) are internal — refactor freely.

## Change classifier

When editing the public surface, classify the change before merging:

### Safe — `patch`

- Bug fix that preserves signatures + return shapes.
- Internal refactor under leading-underscore names.
- Docstring updates.
- Adding a new internal module / helper not exported.
- Tightening internal mypy types (e.g. removing `Any` from a
  private helper).
- Loosening a parameter type to a superset (`str` → `str | bytes`)
  in a way that strictly accepts more inputs.

### Additive — `minor`

- **New method** on `DocumentsApiClient`.
- **New optional keyword argument** on an existing method, with a
  default that preserves prior behavior.
- **New optional field** on a model, with a default (`None` or
  similar). Pydantic must still parse responses that lack the
  field.
- **New enum member**. Existing consumers comparing against known
  values still work; consumers doing exhaustive `match` statements
  may need updating but that's expected for "minor."
- **New export** added to `__all__` (new model, new exception,
  new helper class). Consumers who don't import it are unaffected.

### Breaking — `major`

- **Removing** anything from `__all__`, or renaming an export.
- **Removing or renaming** a method on `DocumentsApiClient`.
- **Renaming a parameter** on a public method (keyword callers
  break — even if positional callers don't).
- **Removing a parameter**, or changing a default to a behaviorally
  different value.
- **Making an optional parameter required** (no default).
- **Removing a field** from a model, or changing its type to a
  non-superset (`str | None` → `str` is breaking, `str` → `str |
  None` is additive).
- **Renaming a field** on a model — Pydantic parses by name, so
  prior server responses break consumers.
- **Removing or renaming an enum member**, or changing its `value`
  (the string side of `StrEnum`).
- **Changing a return type** to something that's not a superset.
- **Renaming or removing an exception class**, or changing its
  base class in a way that breaks `except`.

## Patterns for safe evolution

### Adding a new parameter — keyword-only with default

```python
def upload_document(
    self,
    file: BinaryIO | bytes,
    filename: str,
    *,
    user_id: str | None = None,
    organization_id: str | None = None,
    # NEW (minor bump):
    metadata: dict[str, Any] | None = None,
) -> Document:
    ...
```

`*,` makes the new arg keyword-only — protects against positional
callers binding to the wrong slot if the order ever changes. The
existing `DocumentsApiClient.upload_document` already follows this
shape; preserve it on new methods.

### Adding a new field on a model — optional with default

```python
class Document(BaseModel):
    id: str
    name: str
    # NEW (minor bump):
    tags: list[str] | None = None
```

Default must be present (Pydantic will reject responses lacking
the field otherwise). For a list, `None` is preferred over `[]` to
distinguish "server didn't send it" from "empty list."

### Deprecating something

When you need to remove a method or field, deprecate first, remove
in the next major release:

1. **Minor release** — add the replacement, keep the old name
   working. In the old name's docstring, mark it deprecated and
   point at the new one. Issue `warnings.warn("... use X instead",
   DeprecationWarning, stacklevel=2)` from the old code path.
2. **Major release** — remove the old name. Document the removal
   in the release notes.

Skipping the deprecation step (just removing in a major bump) is
technically allowed by SemVer, but is unfriendly to consumers when
a deprecation path is cheap.

### Splitting a module

`models.py` or `client.py` getting large is fine — splitting them
is a refactor, but the **public-facing names must still import
from `rowland`**. Re-export from `__init__.py`:

```python
# new file: src/rowland/_documents.py
class DocumentsApiClient: ...

# src/rowland/__init__.py
from ._documents import DocumentsApiClient

__all__ = ["DocumentsApiClient", ...]
```

Consumers doing `from rowland.client import DocumentsApiClient`
(direct submodule import) **will break** if `client.py` no longer
exists. That submodule path is part of the public surface in
practice. Either keep `client.py` as a re-export shim
(`from ._documents import *`), or treat the move as a `major`
breaking change.

## Pre-PR checklist

Before merging a PR that touches `client.py`, `models.py`,
`exceptions.py`, or `__init__.py`:

1. **`grep '__all__' src/rowland/__init__.py`** — make sure new
   names you added are exported, and removed names are dropped.
2. **Diff public signatures**:
   ```bash
   git diff main -- python/src/rowland/client.py python/src/rowland/models.py python/src/rowland/exceptions.py python/src/rowland/__init__.py
   ```
   Read every `def`, every model class, every `__all__` line.
   Classify the diff using the table above.
3. **Match the version bump to the classifier.** If it's a `major`
   change, the next `mise run release` invocation must be
   `mise run release major`. See `[[release-process]]`.
4. **Examples still work.** `python/examples/basic_usage.py` is
   shipped with the package. If you renamed something, update it.
5. **Integration tests still pass.** `tests/integration/` exercises
   the surface from a consumer's perspective. Type errors in the
   test file usually mean a real consumer-visible break.

## When in doubt

If you can't decide whether a change is "minor" or "major," ask:
**will any existing consumer's code stop working when they `pip
install -U rowland-sdk`?** If yes — even one usage pattern, even
keyword-call-only — it's major. SemVer's promise is that minor
upgrades are safe to apply blind; preserving that promise is
cheaper than fielding the bug reports later.

## Anti-patterns to refuse

- Removing a method "because nobody uses it." You can't see all
  the consumers — they're on PyPI, not GitHub. Deprecate, don't
  remove.
- Renaming a field on a Pydantic model in a `minor`. Always major.
- "I'll just change the type — it's a subtype." Subtype on the
  return is safe; subtype on a parameter is breaking (you're
  rejecting inputs that previously worked).
- Adding `**kwargs` to "future-proof" a method. Makes mistyped
  kwargs silently ignored — degrades the IntelliSense experience
  that's listed as a selling point in the README.
- Hardcoding a version string in `__init__.py`. `__version__` is
  derived from installed package metadata via `importlib.metadata`
  — don't reintroduce drift. See `[[release-process]]`.
