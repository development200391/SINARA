using ERP.Application.Services;
using ERP.Domain.Interfaces;
using ERP.Infrastructure.Cache;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly("ERP.Infrastructure"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectRetry = 5;
        redisOptions.ConnectTimeout = 10000;
        redisOptions.SyncTimeout = 10000;

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
            options.InstanceName = configuration["Redis:InstanceName"];
        });

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
