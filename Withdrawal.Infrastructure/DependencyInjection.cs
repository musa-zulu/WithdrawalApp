using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Withdrawal.Application.Interfaces;
using Withdrawal.Infrastructure.Services;
using Withdrawal.Infrastructure.Services.Messaging;
using System.Data;
using Npgsql;

namespace Withdrawal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
                .AddHttpContextAccessor()
                .AddServices()
                .AddBackgroundServices(configuration)
                .AddAuthentication(configuration)
                .AddPersistence(configuration);
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IOutboxService, OutboxService>();

        return services;
    }


    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(configuration.GetConnectionString("Default")));
        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: configure background services here e.g email notification services
        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add authentication services

        return services;
    }
}