using System.Reflection;
using App.Metrics;
using App.Metrics.Formatters.Prometheus;
using FisherTournament.API.Common.Errors;
using FisherTournament.API.Common.Mapping;
using FisherTournament.Infrastracture.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MinimalApi.Endpoint.Extensions;

namespace FisherTournament.API;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // services.AddControllers();

        services.AddEndpoints();

        services.AddMappings();

        services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();

        AddMetrics(services, configuration);

        return services;
    }

    private static void AddMetrics(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMetrics();
        services.AddMetricsEndpoints(configuration, setup =>
        {
            setup.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
            setup.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
        });
        services.AddMetricsTrackingMiddleware(configuration);
    }
}