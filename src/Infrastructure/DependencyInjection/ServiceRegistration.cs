using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PruebaNetCoreProject.Application.Ports.Driven;
using PruebaNetCoreProject.Domain.Common;
using PruebaNetCoreProject.Infrastructure.Persistence.Contexts;
using PruebaNetCoreProject.Infrastructure.Persistence.MongoDB.Contexts;
using PruebaNetCoreProject.Infrastructure.Persistence.Redis.Services;
using StackExchange.Redis;

namespace PruebaNetCoreProject.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddApplication()
            .AddMySqlWriteStore(configuration)
            .AddMongoDbReadStore(configuration)
            .AddRedisCache(configuration);

        return services;
    }

    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ICacheService).Assembly;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    private static IServiceCollection AddMySqlWriteStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("ConnectionStrings:MySQL no está configurado.");

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, serverVersion,
                mySql => mySql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddMongoDbReadStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("ConnectionStrings:MongoDB no está configurado.");

        var databaseName = configuration["MongoDB:DatabaseName"] ?? "fintech_db";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddSingleton<MongoDbContext>(sp =>
            new MongoDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));

        return services;
    }

    private static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis no está configurado.");

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(connectionString));

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
