namespace FisherTournament.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // TODO: Protected
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents(DispatchOrder dispatchOrderToClear)
    {
        _domainEvents.RemoveAll(x => x.SaveState == dispatchOrderToClear);
    }

#pragma warning disable CS8618
    protected AggregateRoot()
    {
    }
#pragma warning restore CS8618
}