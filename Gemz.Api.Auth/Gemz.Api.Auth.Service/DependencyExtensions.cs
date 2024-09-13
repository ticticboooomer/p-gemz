using System.Text.Json;
using Gemz.Api.Auth.Service.Accounts;
using Gemz.Api.Auth.Service.Auth;
using Gemz.Api.Auth.Service.Auth.HttpServices;
using Gemz.Api.Auth.Service.Auth.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gemz.Api.Auth.Service;

public static class DependencyExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IGetTwitchAccessToken, GetTwitchAccessToken>();
        services.AddSingleton<IGetUserInfoFromTwitch, GetUserInfoFromTwitch>();
        services.AddSingleton<IValidateTwitchAccessToken, ValidateTwitchAccessToken>();
        services.AddSingleton<IRefreshTwitchToken, RefreshTwitchToken>();
        services.AddHttpClient<TwitchClient>("TwitchClient");
        services.AddSingleton<TwitchClientFactory>();
        services.AddTransient<ITwitchAuthService, TwitchAuthService>();
        services.AddTransient<IAccountService, AccountService>();
        services.Configure<AuthConfig>(config.GetSection("Auth"));
        return services;
    }
}