# TableauSharp

**TableauSharp** is a .NET SDK that provides a clean, developer-friendly abstraction over Tableau’s REST API. It simplifies authentication, user/group management, workbooks, permissions, and embedding for .NET developers.

---

## Features

- **Authentication** (Personal Access Token, Username/Password, or Connected App JWT)
- **User & Group Management** (CRUD operations, group membership)
- **Project Management** (create, update, delete)
- **Workbooks & Views** (list, publish, delete, export views as PDF/PNG/CSV)
- **Data Sources** (list, publish, refresh, delete)
- **Permissions API** (get, add, and delete capabilities on workbooks, projects, and data sources)
- **Embedding Support** (trusted tickets, embed URL helpers)
- **Common Models & Utilities** (strongly typed POCOs, pagination, error handling)

---

## Installation

Install prerelease packages from NuGet as modules reach preview quality:

```powershell
dotnet add package TableauSharp --prerelease
```

Or clone the repository to build locally:

```bash
git clone https://github.com/teesofttech/TableauSharp.git
cd TableauSharp
dotnet build TableauSharp.sln
```

Maintainers can publish prerelease packages from GitHub Actions after the [Module Completion Gate](docs/module-completion.md) is met. See [Release Process](docs/release.md).

---

## Configuration (appsettings.json)

Add your Tableau settings in `appsettings.json`. Include only the fields relevant to the authentication method you plan to use.

```json
{
  "TableauOptions": {
    "Server": "https://your-tableau-server",
    "Version": "3.23",
    "Site": "your-site-name"
  },
  "TableauAuthOptions": {
    "SiteContentUrl": "your-site-name",

    // Personal Access Token (recommended for server-side apps)
    "PersonalAccessTokenName": "your-pat-name",
    "PersonalAccessTokenSecret": "your-pat-secret",

    // Username & Password
    "Username": "your-username",
    "Password": "your-password",

    // Connected App / JWT
    "SecretId": "your-connected-app-secret-id",
    "SecretValue": "your-connected-app-secret-value",
    "Jwt_Expiry_Minutes": 10,
    "Jwt_Audience": "https://your-tableau-server",
    "Scopes": "tableau:views:read tableau:workbooks:read tableau:datasources:read",

    "UsePAT": true
  }
}
```

> **Note:** Comments (`//`) are not valid JSON. Remove them before use or switch to `appsettings.Development.json`.

---

## Dependency Injection Setup

Register TableauSharp in `Program.cs` (or `Startup.cs`):

```csharp
using TableauSharp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTableauSharp(builder.Configuration);

var app = builder.Build();
```

`AddTableauSharp` registers all services — `IAuthService`, `IUserService`, `IGroupService`, `IProjectService`, `IWorkbookService`, `IViewService`, `IDataSourceService`, `IPermissionService`, and `IEmbeddingService` — as scoped dependencies.

---

## Authentication Lifecycle

Call one of the sign-in methods before using site-scoped services. The token is stored automatically in the scoped `ITableauTokenProvider` and injected into all other services.

### Personal Access Token (recommended)

```csharp
var token = await authService.SignInWithPATAsync();
```

### Username & Password

```csharp
var token = await authService.SignInWithUserCredentialsAsync();
```

### Connected App / JWT

```csharp
var token = await authService.SignInWithJWTAsync("user@example.com");
```

Configure `SecretId`, `SecretValue`, `Jwt_Expiry_Minutes`, `Jwt_Audience`, and `Scopes` in `TableauAuthOptions`.

### Sign Out

```csharp
await authService.SignOutAsync(token.Token);
```

Successful sign-in stores the Tableau auth token, site LUID, site content URL, user LUID, and expiration. Site-scoped REST requests use the signed-in site LUID returned by Tableau, not the friendly site content URL.

The built-in session is scoped for a single logical caller. Server applications that serve multiple Tableau users should create an appropriate DI scope per caller/request or manage user-specific sessions explicitly.

---

## Quick Start

### Fetch Workbooks

```csharp
public class WorkbooksController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWorkbookService _workbookService;

    public WorkbooksController(IAuthService authService, IWorkbookService workbookService)
    {
        _authService = authService;
        _workbookService = workbookService;
    }

    [HttpGet("workbooks")]
    public async Task<IActionResult> GetWorkbooks()
    {
        await _authService.SignInWithPATAsync();
        var workbooks = await _workbookService.GetAllAsync();
        return Ok(workbooks);
    }
}
```

### Export a View

```csharp
var export = await viewService.ExportViewAsync(new ExportRequest
{
    ViewId = "view-id",
    Format = "PNG"   // PNG, PDF, or CSV
});

await File.WriteAllBytesAsync(export.FileName, export.FileContent);
```

---

## Running the Examples

The `samples/TableauSharp.Examples` project is a runnable console app that demonstrates every service.

```bash
cd samples/TableauSharp.Examples

# Show available examples
dotnet run

# Run a specific example
dotnet run -- auth         # Sign in with PAT, JWT, credentials
dotnet run -- users        # User & group CRUD
dotnet run -- projects     # Project CRUD
dotnet run -- workbooks    # List, publish, export workbooks
dotnet run -- datasources  # List, publish, refresh data sources
dotnet run -- permissions  # Get, grant, revoke permissions
dotnet run -- embedding    # Trusted ticket & embed URLs
```

Configure real Tableau credentials in `samples/TableauSharp.Examples/appsettings.json` before running.

---

## Project Structure

```
TableauSharp/
├── src/
│   └── TableauSharp/           # SDK source code
│       ├── Auth/               # Authentication services (PAT, credentials, JWT)
│       ├── Users/              # User & Group management
│       ├── Workbooks/          # Workbooks and Views (list, publish, export)
│       ├── DataSources/        # Data source management and refresh
│       ├── Projects/           # Project management
│       ├── Permissions/        # Permissions for workbooks, projects, data sources
│       ├── Embedding/          # Trusted tickets and embed URL helpers
│       ├── Extensions/         # IServiceCollection extension (AddTableauSharp)
│       ├── Settings/           # TableauOptions and TableauAuthOptions
│       └── Common/             # Shared models, enums, HTTP helpers, token provider
├── samples/
│   └── TableauSharp.Examples/  # Runnable console app with per-module examples
├── test/
│   └── TableauSharp.Tests/     # Unit tests
└── docs/                       # Module completion gate and release process docs
```

---

## Roadmap

### v0.1.0 (Preview)
- Authentication (PAT, Username/Password, Connected App JWT)
- Fetch workbooks & export views
- Basic permissions support

### v0.2.0 (Beta)
- Full CRUD for users, groups, projects
- Data source refresh
- Permissions sync API

### v1.0.0 (Stable)
- Embedding support
- Webhooks and Metadata API
- CLI tooling
- Comprehensive docs & samples

---

## Contributing

Contributions are welcome! Please fork the repo and submit a pull request.

---

## License

MIT License. See [LICENSE](LICENSE) for details.
