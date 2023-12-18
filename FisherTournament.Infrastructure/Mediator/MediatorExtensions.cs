using FisherTournament.Domain;
using FisherTournament.Infrastructure.Persistence.Tournaments;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FisherTournament.Infrastructure.Mediator;

public static partial class MediatorExtensions
{
    // DispatchDomainEventsAsync
    public static async Task DispatchDomainEventsBeforeSaveAsync(
        this IMediator mediator,
        TournamentFisherDbContext ctx,
        CancellationToken cancellationToken = default)
    {
        var aggregates = ctx.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents != null
                        && x.Entity.DomainEvents.Any(e => e.DispatchOrder == DispatchOrder.BeforeSave));

        var domainEvents = aggregates
            .SelectMany(x => x.Entity.DomainEvents)
            .Where(x => x.DispatchOrder == DispatchOrder.BeforeSave)
            .ToList();

        await mediator.Dispatch(domainEvents, cancellationToken);

        ClearDomainEvents(aggregates, DispatchOrder.BeforeSave);
    }

    public static async Task DispatchDomainEventsAfterSaveAsync(
        this IMediator mediator,
        TournamentFisherDbContext ctx,
        CancellationToken cancellationToken = default)
    {
        var aggregates = ctx.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents != null
                        && x.Entity.DomainEvents.Any(e => e.DispatchOrder == DispatchOrder.AfterSave));

        var domainEvents = aggregates
            .SelectMany(x => x.Entity.DomainEvents)
            .Where(x => x.DispatchOrder == DispatchOrder.AfterSave)
            .ToList();

        await mediator.Dispatch(domainEvents, cancellationToken);

        ClearDomainEvents(aggregates, DispatchOrder.AfterSave);
    }

    public static async Task Dispatch(this IMediator mediator,
                                      List<IDomainEvent> events,
                                      CancellationToken cancellationToken = default)
    {
        var tasks = events
            .Select(async (domainEvent) =>
            {
                await mediator.Publish(domainEvent, cancellationToken);
            });

        await Task.WhenAll(tasks);
    }

    private static void ClearDomainEvents(IEnumerable<EntityEntry<IAggregateRoot>> aggregates,
                                          DispatchOrder dispatchOrder)
    {
        aggregates.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents(dispatchOrder));
    }
}