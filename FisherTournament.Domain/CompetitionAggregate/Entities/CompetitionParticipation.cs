using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate.Entities;

public sealed class CompetitionParticipation : Entity<int>
{
    private List<FishCaught> _fishCaught = new();

    private CompetitionParticipation(CompetitionId competitionId, FisherId fisherId, int totalScore)
    {
        CompetitionId = competitionId;
        FisherId = fisherId;
        TotalScore = totalScore;
    }

    public CompetitionId CompetitionId { get; private set; }

    public FisherId FisherId { get; private set; }
    public int TotalScore { get; private set; }
    public IReadOnlyCollection<FishCaught> FishCaught => _fishCaught.AsReadOnly();

    public static CompetitionParticipation Create(CompetitionId competitionId, FisherId fisherId)
    {
        return new CompetitionParticipation(competitionId, fisherId, 0);
    }

    public void AddFishCaught(FishCaught fishCaught)
    {
        _fishCaught.Add(fishCaught);
        TotalScore += fishCaught.Score;
    }
}