using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Withdrawal.Application.Common.Behaviors;

namespace Withdrawal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        return services;
    }
}