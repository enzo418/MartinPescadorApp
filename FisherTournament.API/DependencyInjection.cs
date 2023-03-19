using System.Reflection;
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
        this IServiceCollection services)
    {
        // services.AddControllers();

        services.AddEndpoints();

        services.AddMappings();

        services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();

        return services;
    }
}