namespace FisherTournament.Domain.CompetitionAggregate.ValueObjects;

public class CompetitionId : GuidId
{
    public CompetitionId(Guid value) : base(value)
    {
    }
}