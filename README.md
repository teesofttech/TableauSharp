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

Coming soon to NuGet:

```powershell
dotnet add package TableauSharp
```

For now, clone the repository:

```bash
git clone https://github.com/teesofttech/TableauSharp.git
```

---

## Configuration (appsettings.json)

Add your Tableau settings in `appsettings.json`:

```json
{
  "Tableau": {
    "ServerUrl": "https://your-tableau-server",
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

// Bind Tableau settings
builder.Services.Configure<TableauAuthOptions>(
    builder.Configuration.GetSection("Tableau"));

// Register Tableau services
builder.Services.AddTableauSharp();

var app = builder.Build();
```

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
