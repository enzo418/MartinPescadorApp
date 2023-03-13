using FisherTournament.Infrastracture.Settings;

namespace FisherTournament.API;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services.AddControllers();

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