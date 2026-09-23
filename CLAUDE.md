# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Part of the Corely suite. See `README.md` for what this library does and `Docs/index.md` for usage.

## Build and test

```powershell
.\RebuildAndTest.ps1
```

Formats with CSharpier, rebuilds, and runs the full suite. Run it before committing.

## Releasing

Bump `<Version>` in the library csproj, then tag: `git tag vX.Y.Z && git push origin vX.Y.Z`. The tag triggers `release.yml`, which builds, tests, packs and pushes to NuGet via OIDC. `ci.yml` runs build and test on every push and pull request.

## Documentation

`Docs/` describes **how the current version works**. Nothing else.

- **No version numbers of this library.** No "since 2.1", "fixed in 3.0.2", "1.x did X". A reader on
  an older version is served by that version's docs. Migration guides are the sole exception and
  live at the repository root, not in `Docs/`.
- **No references to `Plans/`.** Plans are working material. Never link to one from documentation and
  never cite one as the reason something is the way it is.
- **Match the house style.** Terse and code-forward: a short orienting paragraph, then examples.
  Not an essay with nested headings. Read the neighbouring files in `Docs/` before adding one.
- **Legacy identifiers may be named, versions may not.** "The legacy name `X` stays registered as an
  alias" is fine; "the 1.x name `X`" is not.

The full guide is `DOCUMENTATION-STYLE.md` in the Corely.IAM repository.
