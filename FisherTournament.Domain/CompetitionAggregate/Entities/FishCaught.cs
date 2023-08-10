using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate.Entities;

// TODO: Rename to CaughtFish :)

public class FishCaught : Entity<int>
{
    private FishCaught(
        CompetitionId competitionId,
        FisherId fisherId,
        int score,
        DateTime dateTime)
    {
        CompetitionId = competitionId;
        FisherId = fisherId;
        Score = score;
        DateTime = dateTime;
    }

    public CompetitionId CompetitionId { get; private set; }
    public FisherId FisherId { get; private set; }
    public int Score { get; private set; }
    public DateTime DateTime { get; private set; }

    public static FishCaught Create(CompetitionId competitionId, FisherId fisherId, int score, IDateTimeProvider dateTimeProvider)
    {
        return new FishCaught(competitionId, fisherId, score, dateTimeProvider.Now);
    }

#pragma warning disable CS8618
    public FishCaught()
    {
    }
#pragma warning restore CS8618
}