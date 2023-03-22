using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.Entities;

public sealed class TournamentInscription : Entity<int>
{
    private TournamentInscription(
        TournamentId tournamentId,
        FisherId fisherId,
        CategoryId categoryId)
        : base()
    {
        TournamentId = tournamentId;
        FisherId = fisherId;
        CategoryId = categoryId;
    }

    public TournamentId TournamentId { get; private set; }
    public FisherId FisherId { get; private set; }

    public CategoryId CategoryId { get; private set; }

    public static TournamentInscription Create(
        TournamentId tournamentId,
        FisherId fisherId,
        CategoryId categoryId)
    {
        return new TournamentInscription(tournamentId, fisherId, categoryId);
    }

#pragma warning disable CS8618
    private TournamentInscription()
    {
    }
#pragma warning restore CS8618
}