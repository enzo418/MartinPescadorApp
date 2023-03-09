namespace FisherTournament.Domain;

public class GuidId : ValueObject
{
    public Guid Value { get; }

    public GuidId(Guid value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}