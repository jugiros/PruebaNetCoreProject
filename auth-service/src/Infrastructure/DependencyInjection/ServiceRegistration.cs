using AuthService.Application.Ports;
using AuthService.Application.UseCases.Commands.RegisterUser;
using AuthService.Infrastructure.DependencyInjection.Adapters;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthService.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddAuthServiceDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext(configuration);
        services.AddApplicationServices();
        services.AddJwtAuthentication(configuration);

        return services;
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("ConnectionStrings:MySQL no configurado.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Scoped);
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly));

        services.AddValidatorsFromAssembly(typeof(RegisterUserCommandHandler).Assembly);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddHttpContextAccessor();
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret   = configuration["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret no configurado.");
        var issuer   = configuration["Jwt:Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAuthRead", policy =>
                policy.RequireClaim("scope", "auth:read"));
        });
    }
}
