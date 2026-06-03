using FusionCacheDemo.Application.Cache;
using FusionCacheDemo.Application.Interfaces;
using FusionCacheDemo.Application.Services;
using FusionCacheDemo.Domain.Interfaces;
using FusionCacheDemo.Infrastructure.Data;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace FusionCacheDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string redisConnectionString)
    {
        // SQL Connection Factory
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

        // Repositories (Dapper - sin cache)
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();

        // Base Services (sin cache)
        services.AddScoped<AccountService>();
        services.AddScoped<DriverService>();

        // FusionCache Configuration
        services.AddFusionCache()
            .WithDefaultEntryOptions(options => options
                .SetDuration(TimeSpan.FromMinutes(5))
                .SetFailSafe(true, TimeSpan.FromMinutes(15))
                .SetFactoryTimeouts(
                    softTimeout: TimeSpan.FromMilliseconds(100),
                    hardTimeout: TimeSpan.FromMilliseconds(500)))
            .WithSerializer(new FusionCacheNewtonsoftJsonSerializer())
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions
                {
                    Configuration = redisConnectionString
                }))
            .WithBackplane(
                new RedisBackplane(new RedisBackplaneOptions
                {
                    Configuration = redisConnectionString
                }));

        // Cached Services (Decorator Pattern)
        services.AddScoped<IAccountService>(sp =>
            new CachedAccountService(
                sp.GetRequiredService<AccountService>(),
                sp.GetRequiredService<IFusionCache>()));

        services.AddScoped<IDriverService>(sp =>
            new CachedDriverService(
                sp.GetRequiredService<DriverService>(),
                sp.GetRequiredService<IFusionCache>()));

        return services;
    }
}
