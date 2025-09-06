using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TableauSharp.Auth.Service;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;
using TableauSharp.Projects.Services;
using TableauSharp.Settings;
using TableauSharp.Users.Services;
using TableauSharp.Workbooks.Services;

namespace TableauSharp.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTableauSharp(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TableauOptions>(configuration.GetSection("Tableau"));
        services.Configure<TableauAuthOptions>(configuration.GetSection("TableauAuth"));

        services.AddHttpClient("TableauClient", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TableauOptions>>().Value;
            client.BaseAddress = new Uri($"{options.Url}/api/{options.Version}/");
        });

        services.AddSingleton<ITableauTokenProvider, TableauTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IWorkbookService, WorkbookService>();
        services.AddScoped<IViewService, ViewService>();

        return services;
    }
}