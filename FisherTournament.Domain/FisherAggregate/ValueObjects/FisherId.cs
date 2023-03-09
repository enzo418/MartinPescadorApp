namespace FisherTournament.Domain.FisherAggregate.ValueObjects;

public class FisherId : GuidId
{
    public FisherId(Guid value) : base(value)
    {
    }
}