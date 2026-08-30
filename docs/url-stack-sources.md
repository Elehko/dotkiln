# URL Stack Sources And Caching

Dotkiln can load stack definitions from local file paths or HTTP/HTTPS URLs.

## Local File

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Dotkiln reads the file from disk and parses it as a stack.

## URL

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate https://example.com/stacks/aspnet-webapi-standard.dotkiln.yaml
```

Dotkiln fetches the URL, reads the response body as YAML, and validates it.

The same source format works for commands that accept a stack:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status https://example.com/stacks/aspnet-webapi-standard.dotkiln.yaml MyApp.csproj
```

## Caching

URL stack caching is not implemented yet.

Current behavior:

- every command fetches the URL directly
- no stack copy is written to a local cache
- no cache expiration policy exists
- no offline fallback exists for URL stacks
- no integrity pinning or checksum verification exists yet

If reproducibility matters today, store the stack file in your repository and reference it by local path.

## Snippets With URL Stacks

For local stack files, Dotkiln can print a snippet file referenced by `snippet` if the file exists relative to the stack file.

For URL stack sources, snippet loading is skipped in the current implementation. Dotkiln does not fetch secondary snippet URLs.

## Recommended Usage

Use local stack files for CI:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/company-webapi.dotkiln.yaml src/MyApi/MyApi.csproj
```

Use URL stack files for evaluation or shared internal experiments where live fetch behavior is acceptable.

## Future Cache Design

A future cache could include:

- `dotkiln stack fetch <url>`
- local cache directory under the user's profile
- cache expiration
- checksum pinning
- offline mode
- lockfile support

These features are not implemented today.
