using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RydrSafe.Application.Common.Behaviors;

namespace RydrSafe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Registers the validators into the container. On its own this does nothing at runtime —
        // MediatR will not call them unless a pipeline behavior does, which is what the line
        // below is for. Removing it silently disables every validation rule in the codebase.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
