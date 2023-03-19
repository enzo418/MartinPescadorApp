namespace FisherTournament.IntegrationTests;

using FisherTournament.Application;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Infrastracture;
using FisherTournament.Infrastracture.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class UseCaseTestsFixture : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    public IDateTimeProvider DateTimeProvider => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDateTimeProvider>();

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
            services.AddInfrastracture();
            services.AddSettings(configuration);
        });

        var host = builder.Build();

        _scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

        TournamentFisherDbContext context = (host.Services.GetRequiredService<ITournamentFisherDbContext>() as TournamentFisherDbContext)!;

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
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

    public async Task<TEntity?> FindAsync<TEntity>(params object[] id)
     where TEntity : class
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ITournamentFisherDbContext>();
            return await context.Set<TEntity>().FindAsync(id);
        }
    }

    public async Task<IEnumerable<TEntity>> FindAllAsync<TEntity, TId>(IEnumerable<TId> Ids)
        where TId : notnull
        where TEntity : AggregateRoot<TId>
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ITournamentFisherDbContext>();
            return await context.Set<TEntity>().Where(c => Ids.Contains(c.Id)).ToListAsync();
        }
    }

    public async Task<TEntity> AddAsync<TEntity>(TEntity entity, Action<TEntity>? beforeSave = null) where TEntity : class
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ITournamentFisherDbContext>();
            await context.Set<TEntity>().AddAsync(entity);

            if (beforeSave is not null)
            {
                beforeSave(entity);
            }

            await context.SaveChangesAsync(default);
            return entity;
        }
    }

    public async Task<IEnumerable<TEntity>> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ITournamentFisherDbContext>();
            await context.Set<TEntity>().AddRangeAsync(entities);
            await context.SaveChangesAsync(default);
            return entities;
        }
    }

    public async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ITournamentFisherDbContext>();
            return await context.Set<TEntity>().CountAsync();
        }
    }
}

[CollectionDefinition(nameof(UseCaseTestsCollection))]
public class UseCaseTestsCollection : ICollectionFixture<UseCaseTestsFixture>
{
    // This class is never created.
}