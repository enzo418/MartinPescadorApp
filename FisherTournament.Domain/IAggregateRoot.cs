namespace FisherTournament.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents(DispatchOrder dispatchOrderToClear);
}