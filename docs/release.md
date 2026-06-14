# Release Process

TableauSharp publishes prerelease NuGet packages from GitHub Actions.

## Prerequisites

- `NUGET_API_KEY` repository secret with permission to push `TableauSharp`.
- Passing CI on `master`.
- A prerelease version, for example `0.1.0-alpha.1`.

## Validate Locally

```bash
dotnet restore TableauSharp.sln
dotnet build TableauSharp.sln -c Release --no-restore
dotnet test TableauSharp.sln -c Release --no-build
dotnet pack src/TableauSharp/TableauSharp.csproj -c Release --no-build -p:PackageVersion=0.1.0-alpha.1 -o ./artifacts
```

## Dry Run In GitHub Actions

Run **Publish to NuGet** manually from GitHub Actions:

- `version`: prerelease version, for example `0.1.0-alpha.1`
- `dry_run`: `true`

The workflow restores, builds, tests, packs, and uploads package artifacts without pushing to NuGet.

## Publish A Prerelease

Run **Publish to NuGet** manually with:

- `version`: prerelease version, for example `0.1.0-alpha.1`
- `dry_run`: `false`

The workflow fails before publishing if `NUGET_API_KEY` is missing.
The publish command uploads the `.nupkg` package and the matching `.snupkg` symbols package from the artifacts directory.

## Publish From A Tag

Maintainers can also publish by pushing a version tag:

```bash
git tag v0.1.0-alpha.1
git push origin v0.1.0-alpha.1
```

Tags matching `v*.*.*` trigger the publish workflow. The package version is the tag name without the leading `v`.

## Versioning

Use prerelease versions until the SDK reaches a stable surface:

- `0.1.0-alpha.1`
- `0.1.0-alpha.2`
- `0.1.0-beta.1`

Do not reuse a NuGet package version once it has been published.
