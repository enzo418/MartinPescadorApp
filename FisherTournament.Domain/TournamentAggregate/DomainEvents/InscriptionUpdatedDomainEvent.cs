using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.DomainEvents;

public record struct InscriptionUpdatedDomainEvent(
    TournamentId TournamentId,
    FisherId FisherId,
    CategoryId PreviousCategoryId,
    CategoryId NewCategoryId) : IDomainEvent
{
    public DispatchOrder DispatchOrder { get; init; } = DispatchOrder.AfterSave;
}