using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Infrastracture.Persistence;
using FisherTournament.Infrastracture.Provider;
using FisherTournament.Infrastracture.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FisherTournament.Infrastracture;

public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastracture(this IServiceCollection services)
    {
        services.AddDbContext<ITournamentFisherDbContext, TournamentFisherDbContext>((provider, options) =>
        {
            var dataBaseConectionSettings = provider.GetRequiredService<DataBaseConectionSettings>();
            // options.UseSqlServer(dataBaseConectionSettings.ConnectionString);
            options.UseSqlite(dataBaseConectionSettings.ConnectionString);
            // options.LogTo(System.Console.WriteLine);
            // options.EnableSensitiveDataLogging();
        });

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        DataBaseConectionSettings dataBaseConectionSettings = new();

        configuration.Bind(
            nameof(DataBaseConectionSettings),
            dataBaseConectionSettings);

        services.AddSingleton(dataBaseConectionSettings);

        return services;
    }
}