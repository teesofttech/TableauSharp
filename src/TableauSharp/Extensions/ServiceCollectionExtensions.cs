using Microsoft.Extensions.DependencyInjection;
using TableauSharp.Auth.Service;
using TableauSharp.Projects.Services;
using TableauSharp.Users.Services;

namespace TableauSharp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTableauSharp(this IServiceCollection services)
    {
        services.AddHttpClient<IAuthService, AuthService>();
        services.AddHttpClient<IUserService, UserService>();
        services.AddHttpClient<IGroupService, GroupService>();
        services.AddHttpClient<IProjectService, ProjectService>();  
        return services;
    }
}