# Module Completion Gate

Use this gate before publishing a prerelease for a completed TableauSharp module and moving to the next module.

## Required Before Prerelease

- Public API reviewed for naming, cancellation tokens, async behavior, and Tableau REST parity.
- Request and response models identify required, optional, and nullable fields.
- Unit tests cover request construction, response parsing, auth headers, validation, and error paths.
- Integration tests cover at least one happy-path real Tableau call when credentials are configured.
- Examples project includes a runnable sample for the module.
- README or docs include minimal usage for the module.
- CI passes restore, build, test, and pack.
- Publish workflow dry run succeeds for the target prerelease version.

## Integration Test Policy

Integration tests are required for module completeness, but they must be opt-in. They should skip automatically unless real Tableau credentials are configured in the environment.

Do not block ordinary pull requests or community contributions on unavailable Tableau credentials. Before a module prerelease, maintainers should run the integration tests in an environment that has credentials and record the result in the PR or release notes.

## CI Gate

Every PR runs the standard CI gate:

```bash
dotnet restore TableauSharp.sln
dotnet build TableauSharp.sln --no-restore -c Release
dotnet test TableauSharp.sln --no-build -c Release
dotnet pack src/TableauSharp/TableauSharp.csproj --no-build -c Release -p:ContinuousIntegrationBuild=true
```

Maintainers can also run the CI workflow manually and provide the module name to make the validation run visible in GitHub Actions.

## Publishing Sequence

1. Complete the module issue checklist.
2. Merge the module PR after CI passes.
3. Run the **Publish to NuGet** workflow as a dry run for the prerelease version.
4. Publish from a version tag or run the workflow with `dry_run` set to `false`.
5. Move to the next module issue.
