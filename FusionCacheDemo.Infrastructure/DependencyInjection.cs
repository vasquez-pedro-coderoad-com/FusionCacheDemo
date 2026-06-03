using FusionCacheDemo.Application.Interfaces;
using FusionCacheDemo.Application.Services;
using FusionCacheDemo.Domain.Interfaces;
using FusionCacheDemo.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FusionCacheDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IDriverService, DriverService>();

        return services;
    }
}
