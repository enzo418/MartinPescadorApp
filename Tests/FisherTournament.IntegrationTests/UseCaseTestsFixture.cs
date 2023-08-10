using FisherTournament.Infrastracture.Persistence.Tournaments;

namespace FisherTournament.IntegrationTests;

using System.Diagnostics;
using FisherTournament.Application;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.LeaderBoard;
using FisherTournament.Domain;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Infrastracture;
using FisherTournament.Infrastracture.Persistence;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class UseCaseTestsFixture : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    public IDateTimeProvider DateTimeProvider => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDateTimeProvider>();

    public TournamentFisherDbContext TournamentContext => (_scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITournamentFisherDbContext>() as TournamentFisherDbContext)!;

    public ReadModelsDbContext ReadModelsContext => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReadModelsDbContext>()!;

    public UseCaseTestsFixture()
    {
        var builder = Host.CreateDefaultBuilder();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .Build();

        builder.ConfigureAppConfiguration(builder =>
        {
            builder.Sources.Clear();
            builder.AddConfiguration(configuration);
        });

        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddApplication();
            services.AddInfrastructure();
            services.AddSettings(configuration);

            // add a ActivitySource, just so it can be resolved
            services.AddSingleton(new ActivitySource("Test"));
        });


        var host = builder.Build();

        _scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

        TournamentFisherDbContext tournamentContext = (host.Services.GetRequiredService<ITournamentFisherDbContext>() as TournamentFisherDbContext)!;
        ReadModelsDbContext readModelsDbContext = host.Services.GetRequiredService<ReadModelsDbContext>()!;

        tournamentContext.Database.EnsureDeleted();
        tournamentContext.Database.EnsureCreated();

        readModelsDbContext.Database.EnsureDeleted();
        readModelsDbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {

    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.Send(request);
        }
    }

    /// <summary>
    /// Executes all pending leader board jobs.
    /// </summary>
    /// <returns>The number of executed jobs.</returns>
    public async Task<int> ExecutePendingLeaderBoardJobs()
    {
        using var scope = _scopeFactory.CreateScope();

        var scheduler = scope.ServiceProvider.GetRequiredService<ILeaderBoardUpdateScheduler>();
        var updater = scope.ServiceProvider.GetRequiredService<ILeaderBoardUpdater>();

        int executed = 0;
        while (scheduler.GetNextJob() is var job && job != null)
        {
            await updater.UpdateLeaderBoard(job.TournamentId, job.CategoryId, job.CompetitionsToUpdate);
            executed++;
        }

        return executed;
    }
}

[CollectionDefinition(nameof(UseCaseTestsCollection))]
public class UseCaseTestsCollection : ICollectionFixture<UseCaseTestsFixture>
{
    // This class is never created.
}