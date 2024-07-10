using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate.DomainEvents;

public record struct ParticipationRemovedDomainEvent(
    FisherId FisherId,
    CompetitionId CompetitionId) : IDomainEvent
{
    public DispatchOrder DispatchOrder { get; init; } = DispatchOrder.AfterSave;
}
