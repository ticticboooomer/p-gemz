using Gemz.Api.Auth.Data.Factory;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Data.Repository.AuthState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;

namespace Gemz.Api.Auth.Data;

public static class DependencyExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection s, IConfiguration config)
    {
        var pack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("My Solution Conventions", pack, t => true);

        s.Configure<DbConfig>(config.GetSection("DatabaseConfig"));
        s.AddSingleton<DbFactory>();
        s.AddTransient<IAuthStateRepository, AuthStateRepository>();
        s.AddTransient<IAccountRepository, AccountRepository>();
        return s;
    }
}