using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.Entities;

public sealed class TournamentInscription : Entity<int>
{
    private TournamentInscription(int id, TournamentId tournamentId, FisherId fisherId)
        : base(id)
    {
        TournamentId = tournamentId;
        FisherId = fisherId;
    }

    public TournamentId TournamentId { get; private set; }
    public FisherId FisherId { get; private set; }

    public static TournamentInscription Create(TournamentId tournamentId, FisherId fisherId)
    {
        return new TournamentInscription(new TournamentInscriptionId(Guid.NewGuid()), tournamentId, fisherId);
    }
}