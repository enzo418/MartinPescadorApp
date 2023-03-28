using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.DomainEvents;

public record struct InscriptionAddedDomainEvent(FisherId FisherId, CategoryId CategoryId, TournamentId TournamentId) : IDomainEvent
{
    public DispatchOrder DispatchOrder { get; init; } = DispatchOrder.AfterSave;
}