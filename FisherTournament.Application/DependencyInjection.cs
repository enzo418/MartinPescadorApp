using System.Reflection;
using FisherTournament.Application.Common.Behavior;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FisherTournament.Application;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(DependencyInjection)));

        // if c# would have negative constraints on type parameters i would uncomment this line
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionBasedValidationBehavior<,>));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorOrBasedValidationBehavior<,>));

        return services;
    }
}