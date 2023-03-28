using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Repositories;
using FisherTournament.Infrastracture.Persistence.Tournaments;
using FisherTournament.Infrastracture.Provider;
using FisherTournament.Infrastracture.Settings;
using FisherTournament.ReadModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FisherTournament.Infrastracture;

public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastracture(this IServiceCollection services)
    {
        services.AddDbContext<ITournamentFisherDbContext, TournamentFisherDbContext>((provider, options) =>
        {
            var dataBaseConnectionSettings = provider.GetRequiredService<DataBaseConnectionSettings>();
            // options.UseSqlServer(dataBaseConnectionSettings.ConnectionString);
            options.UseSqlite(dataBaseConnectionSettings.TournamentDbConnectionString);
            // options.LogTo(System.Console.WriteLine);
            // options.EnableSensitiveDataLogging();
        });

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        AddReadModels(services);

        return services;
    }

    private static void AddReadModels(IServiceCollection services)
    {
        services.AddDbContext<ReadModelsDbContext>((provider, builder) =>
        {
            var dataBaseConnectionSettings = provider.GetRequiredService<DataBaseConnectionSettings>();
            builder.UseSqlite(dataBaseConnectionSettings.ReadModelsDbConnectionString);
        });

        services.AddScoped<IReadModelsUnitOfWork, ReadModelsUnitOfWork>();
        services.AddScoped<ILeaderBoardRepository, LeaderBoardRepository>();
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        DataBaseConnectionSettings dataBaseConnectionSettings = new();

        configuration.Bind(
            nameof(DataBaseConnectionSettings),
            dataBaseConnectionSettings);

        services.AddSingleton(dataBaseConnectionSettings);

        return services;
    }
}