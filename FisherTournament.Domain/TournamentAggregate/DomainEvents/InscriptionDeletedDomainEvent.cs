using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.DomainEvents;

public record struct InscriptionDeletedDomainEvent(
    TournamentId TournamentId,
    FisherId FisherId,
    CategoryId CategoryId) : IDomainEvent
{
    public DispatchOrder DispatchOrder { get; init; } = DispatchOrder.AfterSave;
}