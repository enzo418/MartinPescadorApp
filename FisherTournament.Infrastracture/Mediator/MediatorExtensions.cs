using FisherTournament.Domain;
using FisherTournament.Infrastracture.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FisherTournament.Infrastracture.Mediator;

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
                        && x.Entity.DomainEvents.Any(e => e.SaveState == DispatchOrder.BeforeSave));

        var domainEvents = aggregates
            .SelectMany(x => x.Entity.DomainEvents)
            .Where(x => x.SaveState == DispatchOrder.BeforeSave)
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
                        && x.Entity.DomainEvents.Any(e => e.SaveState == DispatchOrder.AfterSave));

        var domainEvents = aggregates
            .SelectMany(x => x.Entity.DomainEvents)
            .Where(x => x.SaveState == DispatchOrder.AfterSave)
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