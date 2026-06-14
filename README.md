# TableauSharp

**TableauSharp** is a .NET SDK that provides a clean, developer-friendly abstraction over Tableau’s REST API. It simplifies authentication, user/group management, workbooks, permissions, and embedding for .NET developers.

---

## Features

- **Authentication** (Personal Access Token or Username/Password)
- **User & Group Management** (CRUD operations, role assignments)
- **Project Management** (create, update, organize)
- **Workbooks & Views** (fetch, export, publish)
- **Data Sources** (list, refresh, permissions)
- **Permissions API** (assign/revoke capabilities, sync with external systems)
- **Embedding Support** (trusted tickets, URL helpers)
- **Common Models & Utilities** (strongly typed POCOs, pagination, error handling)

---

## Installation

Install prerelease packages from NuGet as modules reach preview quality:

```powershell
dotnet add package TableauSharp --prerelease
```

For now, clone the repository:

```bash
git clone https://github.com/teesofttech/TableauSharp.git
```

Maintainers can publish prerelease packages from GitHub Actions after the [Module Completion Gate](docs/module-completion.md) is met. See [Release Process](docs/release.md).

---

## Configuration (appsettings.json)

Add your Tableau settings in `appsettings.json`:

```json
{
  "TableauOptions": {
    "Server": "https://your-tableau-server",
    "Version": "3.23",
    "Site": "yoursite"
  },
  "TableauAuthOptions": {
    "SiteContentUrl": "yoursite",
    "PersonalAccessTokenName": "your-token-name",
    "PersonalAccessTokenSecret": "your-token-secret",
    "UsePAT": true
  }
}
```

---

## Dependency Injection Setup

Register TableauSharp in `Program.cs` (or `Startup.cs`):

```csharp
using TableauSharp.Common.Models;
using TableauSharp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Tableau services
builder.Services.AddTableauSharp(builder.Configuration);

var app = builder.Build();
```

---

## Authentication Lifecycle

Call one of the sign-in methods before using site-scoped services:

```csharp
var authToken = await authService.SignInWithPATAsync();
var workbooks = await workbookService.GetAllAsync();
```

Successful sign-in stores the Tableau auth token, site LUID, site content URL, user LUID, and expiration in the scoped Tableau session. Site-scoped REST requests use the signed-in site LUID returned by Tableau, not the friendly site content URL.

The built-in session is scoped for a single logical caller. Server applications that serve multiple Tableau users should create an appropriate DI scope per caller/request or manage user-specific sessions explicitly.

---

## Quick Start

### Fetch Workbooks

```csharp
public class WorkbooksController : ControllerBase
{
    private readonly IWorkbookService _workbookService;

    public WorkbooksController(IWorkbookService workbookService)
    {
        _workbookService = workbookService;
    }

    [HttpGet("workbooks")]
    public async Task<IActionResult> GetWorkbooks()
    {
        var workbooks = await _workbookService.GetAllAsync();
        return Ok(workbooks);
    }
}
```

---

## Project Structure

```
TableauSharp/
├── Auth/           # Authentication services
├── Users/          # User & Group management
├── Workbooks/      # Workbooks and Views
├── DataSources/    # Data sources
├── Projects/       # Project management
├── Permissions/    # Permissions management
├── Embedding/      # Trusted tickets and embedding
└── Common/         # Shared models, enums, utilities
```

---

## Roadmap

### v0.1.0 (Preview)
- Authentication
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
