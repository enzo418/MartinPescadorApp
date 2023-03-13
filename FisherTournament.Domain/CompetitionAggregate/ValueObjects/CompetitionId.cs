namespace FisherTournament.Domain.CompetitionAggregate.ValueObjects;

public class CompetitionId : GuidId<CompetitionId>
{
    public static implicit operator CompetitionId(Guid value) => new(value);
    public static implicit operator Guid(CompetitionId value) => value.Value;
    public CompetitionId(Guid value) : base(value)
    {
    }
}