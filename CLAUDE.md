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

## Conventions

### Seams for readings and conversions

**A reading or conversion of a type gets a seam of its own, not a private helper.** A conversion
between two types, or a reading of one type that anything else could want, is a contract someone
can get wrong, and as a private helper it can only be reached through the class that calls it. Give
it a home a unit test can call directly:

- **A type you own, in the same layer: a member of that type.** A record or model gets the method
  itself — `quotaContext.RemainingRatio(charged)`, `retryOptions.RetryDelay(attempt, random)`. No
  extension class for a type you can simply change.
- **A type you cannot change: an extension.** Enums, BCL types and vendor types —
  `TimeBucket.BucketStart`, `TimeSpan.AgoText`, a broker's message.
- **A type you own whose reading belongs to another layer: an extension in that layer.** Display text
  for a core model lives in the UI project; a conversion to another domain's result lives with that
  domain, so the source type never learns about it.
- **Entities stay plain data.** Their conversions go in the domain's `Mappers`.

An extension is `<Type>Extensions`, in an `Extensions` or `Mappers` folder, holding extensions of
that one type, written as a C# 14 `extension(T receiver)` block:

```csharp
internal static class TimeBucketExtensions
{
    extension(TimeBucket bucket)
    {
        public string PeriodLabel(DateTime bucketStart) => ...;
    }
}
```

Either way, name it for what comes back (`RemainingRatio`, `ToSettleQuotaResult`,
`PlaceholderConnectionString`), keep it `internal` with `InternalsVisibleTo` in the `.csproj`, and
test it directly.

**Don't**:
- leave it as a private helper on the class that happens to need it, including the instance form
  that closes over a field instead of taking a parameter;
- inline the conversion where it is used, with no seam at all;
- write an extension for a type you own and could give the member to;
- write a helper class not attached to a type — `MessageHelper`, `MappingUtils`, a grab-bag
  `Extensions.cs` or `...Messages` class holding readings of several unrelated types;
- name it for the receiver or the mechanics — `RemainingRatioOf`, `GetRatioFromContext`;
- use `this T` parameters, or make anything
  `public` purely so a test can reach it. Public is for real API only.

This covers conversions and derived readings, not every private method: a helper that only
structures the code it sits in stays where it is, and so does a private step inside an extension
class.

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
