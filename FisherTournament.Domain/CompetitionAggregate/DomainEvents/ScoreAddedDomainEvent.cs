
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate.DomainEvents;

public record struct ScoreAddedDomainEvent(
    CompetitionId CompetitionId,
    FisherId FisherId,
    int Score
) : IDomainEvent
{
    public DispatchOrder DispatchOrder { get; init; } = DispatchOrder.AfterSave;
}
