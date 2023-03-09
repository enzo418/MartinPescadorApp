using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate.Entities;

public class FishCaught : Entity<int>
{
    private FishCaught(CompetitionId competitionId, FisherId fisherId, int score)
    {
        CompetitionId = competitionId;
        FisherId = fisherId;
        Score = score;
    }

    public CompetitionId CompetitionId { get; private set; }
    public FisherId FisherId { get; private set; }
    public int Score { get; private set; }

    public static FishCaught Create(CompetitionId competitionId, FisherId fisherId, int score)
    {
        return new FishCaught(competitionId, fisherId, score);
    }
}