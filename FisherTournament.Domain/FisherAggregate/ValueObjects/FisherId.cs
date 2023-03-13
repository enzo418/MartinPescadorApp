namespace FisherTournament.Domain.FisherAggregate.ValueObjects;

public class FisherId : GuidId<FisherId>
{
    public static implicit operator FisherId(Guid value) => new(value);
    public static implicit operator Guid(FisherId value) => value.Value;

    public FisherId(Guid value) : base(value)
    {
    }
}