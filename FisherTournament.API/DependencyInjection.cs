using System.Reflection;
using FisherTournament.API.Common.ControllerRouteParameterConverters;
using FisherTournament.API.Common.Mapping;
using FisherTournament.Infrastracture.Settings;
using Mapster;
using MapsterMapper;

namespace FisherTournament.API;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddMappings();

        services.AddControllerRouteParameterConverters();

        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        DataBaseConectionSettings dataBaseConectionSettings = new();

        configurationManager.Bind(
            nameof(DataBaseConectionSettings),
            dataBaseConectionSettings);

        services.AddSingleton(dataBaseConectionSettings);

        return services;
    }
}