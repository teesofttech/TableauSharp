using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TableauSharp.Examples.Examples;
using TableauSharp.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(b => b.AddConsole());
        services.AddTableauSharp(ctx.Configuration);

        services.AddTransient<AuthExamples>();
        services.AddTransient<UserGroupExamples>();
        services.AddTransient<ProjectExamples>();
        services.AddTransient<WorkbookExamples>();
        services.AddTransient<DataSourceExamples>();
        services.AddTransient<PermissionExamples>();
        services.AddTransient<EmbeddingExamples>();
    })
    .Build();

// Pick which example to run via command-line arg, or run all sequentially.
// Usage: dotnet run -- auth
//        dotnet run -- users
//        dotnet run -- workbooks
//        dotnet run -- (no arg = menu)

var example = args.Length > 0 ? args[0].ToLowerInvariant() : "menu";

Console.WriteLine("=== TableauSharp SDK Examples ===");
Console.WriteLine();

switch (example)
{
    case "auth":
        await host.Services.GetRequiredService<AuthExamples>().RunAsync();
        break;
    case "users":
        await host.Services.GetRequiredService<UserGroupExamples>().RunAsync();
        break;
    case "projects":
        await host.Services.GetRequiredService<ProjectExamples>().RunAsync();
        break;
    case "workbooks":
        await host.Services.GetRequiredService<WorkbookExamples>().RunAsync();
        break;
    case "datasources":
        await host.Services.GetRequiredService<DataSourceExamples>().RunAsync();
        break;
    case "permissions":
        await host.Services.GetRequiredService<PermissionExamples>().RunAsync();
        break;
    case "embedding":
        await host.Services.GetRequiredService<EmbeddingExamples>().RunAsync();
        break;
    default:
        Console.WriteLine("Available examples:");
        Console.WriteLine("  dotnet run -- auth         Sign in with PAT, JWT, credentials");
        Console.WriteLine("  dotnet run -- users        User & group CRUD");
        Console.WriteLine("  dotnet run -- projects     Project CRUD");
        Console.WriteLine("  dotnet run -- workbooks    List, publish, export workbooks");
        Console.WriteLine("  dotnet run -- datasources  List, publish, refresh data sources");
        Console.WriteLine("  dotnet run -- permissions  Get, grant, revoke permissions");
        Console.WriteLine("  dotnet run -- embedding    Trusted ticket & embed URLs");
        break;
}
