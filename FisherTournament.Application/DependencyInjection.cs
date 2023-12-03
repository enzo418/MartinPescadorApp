using ErrorOr;
using FisherTournament.Application.Common.Behavior;
using FisherTournament.Application.Common.Instrumentation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TraceHandlerBehavior<,>));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorOrBasedValidationBehavior<,>));


        services.AddSingleton<ApplicationInstrumentation>();

        StopIfUsingInvalidTypes();

        return services;
    }

    private static void StopIfUsingInvalidTypes()
    {
        // This is cheaper than checking it in the pipeline
        var assembly = Assembly.GetAssembly(typeof(DependencyInjection))!;
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.IsInterface)
            {
                continue;
            }

            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IRequest<>))
                {
                    var genericArgument = @interface.GetGenericArguments()[0];
                    if (genericArgument.IsGenericType && genericArgument.GetGenericTypeDefinition() == typeof(ErrorOr<>))
                    {
                        continue;
                    }

                    throw new Exception($"{type.Name} implements IRequest<{type.Name}> but it should be ErrorOr<{type.Name}>");
                }
            }
        }
    }
}