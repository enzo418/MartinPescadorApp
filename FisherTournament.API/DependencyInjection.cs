using System.Diagnostics;
using System.Reflection;
using FisherTournament.API.Common.Errors;
using FisherTournament.Infrastracture.Common.Mapping;
using FisherTournament.Application.Common.Instrumentation;
using FisherTournament.Application.Common.Metrics;
using FisherTournament.Infrastracture.Persistence.Common.Diagnostics;
using FisherTournament.Infrastracture.Persistence.Common.Interceptors;
using FisherTournament.Infrastracture.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MinimalApi.Endpoint.Extensions;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FisherTournament.API;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services,
                                            IConfiguration configuration,
                                            IHostEnvironment environment,
                                            ILoggingBuilder loggingBuilder)
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

        AddMetrics(services, configuration, environment, loggingBuilder);

        return services;
    }

    private static void AddMetrics(IServiceCollection services,
                                   IConfiguration configuration,
                                   IHostEnvironment environment,
                                   ILoggingBuilder logging)
    {
        OpenTelemetrySettings openTelemetrySettings = new();
        configuration.Bind(nameof(OpenTelemetrySettings), openTelemetrySettings);

        string collectorOtlpEndpoint = openTelemetrySettings.Exporter.Otlp.Endpoint;


        var DefaultConfiguration = (ResourceBuilder resourceBuilder) =>
        {
            resourceBuilder.AddService(
                environment.ApplicationName, // service name
                environment.EnvironmentName, // namespace
                openTelemetrySettings.ApplicationVersion, // version

                // service Id
                false,
                Environment.MachineName
            );
        };

        DiagnosticListener.AllListeners.Subscribe(new EFCoreDiagnosticsListener());

        services.AddOpenTelemetry()
                    .WithTracing(options =>
                    {
                        options.AddSource(ApplicationInstrumentation.ActivitySourceName)
                                .AddSource(TraceDbConnectionInterceptor.ActivitySourceName)
                                .ConfigureResource(rBuilder => DefaultConfiguration(rBuilder))
                                .AddAspNetCoreInstrumentation(opt => opt.RecordException = true)
                                .AddEntityFrameworkCoreInstrumentation(opt =>
                                {
                                    if (environment.IsDevelopment())
                                    {
                                        opt.SetDbStatementForText = true;
                                    }
                                })
                                .AddHttpClientInstrumentation(opt => opt.RecordException = true)
                                // .AddSqlClientInstrumentation(opt =>
                                // {
                                //     opt.RecordException = true;
                                //     if (environment.IsDevelopment())
                                //     {
                                //         opt.SetDbStatementForStoredProcedure = false;
                                //         opt.SetDbStatementForText = true;
                                //     }
                                // })
                                .AddOtlpExporter(opt =>
                                {
                                    opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                                    opt.Endpoint = new Uri(collectorOtlpEndpoint);
                                });

                        if (environment.IsDevelopment() && openTelemetrySettings.Exporter.Console.EnableOnTrace)
                        {
                            options.AddConsoleExporter();
                        }
                    }).WithMetrics(metricsBuilder =>
                    {
                        metricsBuilder
                                .ConfigureResource(rBuilder => DefaultConfiguration(rBuilder))
                                .AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddEventCountersInstrumentation(cfg =>
                                {
                                    cfg.RefreshIntervalSecs = 2;
                                    cfg.AddEventSources("Microsoft.AspNetCore.Hosting");
                                })
                                .AddRuntimeInstrumentation()
                                .AddOtlpExporter(opt =>
                                {
                                    opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                                    opt.Endpoint = new Uri(collectorOtlpEndpoint);
                                });

                        if (environment.IsDevelopment() && openTelemetrySettings.Exporter.Console.EnableOnMetric)
                        {
                            metricsBuilder.AddConsoleExporter();
                        }

                        // Register all the meters (IProjectMetrics implementations)
                        var metricsNames = AppDomain.CurrentDomain.GetAssemblies()
                                            .SelectMany(a => a.GetTypes())
                                            .Where(t => t.IsClass
                                                        && !t.IsAbstract
                                                        && t.IsAssignableTo(typeof(IProjectMetrics)))
                                            .Select(t => t.GetField("MetricsName")!.GetValue(null) as string);

                        foreach (var metricName in metricsNames)
                        {
                            Console.WriteLine("Registered " + metricName);
                            metricsBuilder.AddMeter(metricName!);
                        }
                    });

        // Clear default logging providers used by WebApplication host.
        logging.ClearProviders();

        logging.AddOpenTelemetry(options =>
        {
            var resourceBuilder = ResourceBuilder.CreateDefault();

            DefaultConfiguration(resourceBuilder);

            options.SetResourceBuilder(resourceBuilder);

            options.AddOtlpExporter(opt =>
            {
                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                opt.Endpoint = new Uri(collectorOtlpEndpoint);
            });

            if (environment.IsDevelopment() && openTelemetrySettings.Exporter.Console.EnableOnLog)
            {
                options.AddConsoleExporter();
            }
        });
    }
}