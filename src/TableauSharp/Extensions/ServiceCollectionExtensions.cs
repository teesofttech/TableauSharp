using Microsoft.Extensions.DependencyInjection;
using TableauSharp.Auth.Service;
using TableauSharp.Common.Helper;
using TableauSharp.Projects.Services;
using TableauSharp.Users.Services;
using TableauSharp.Workbooks.Services;

namespace TableauSharp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTableauSharp(this IServiceCollection services)
    {
        // Typed client for all Tableau REST interactions
        services.AddHttpClient("TableauClient", client =>
        {
            // BaseAddress will be set dynamically from options in each service
            // Token will also be added dynamically when making calls
        });
        services.AddSingleton<ITableauTokenProvider, TableauTokenProvider>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IGroupService, GroupService>();
        services.AddTransient<IProjectService, ProjectService>();
        services.AddTransient<IWorkbookService, WorkbookService>();
        services.AddTransient<IViewService, ViewService>();

        return services;
    }
}