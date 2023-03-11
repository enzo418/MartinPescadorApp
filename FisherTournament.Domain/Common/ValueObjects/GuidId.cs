namespace FisherTournament.Domain;

public class GuidId : ValueObject
{
    public Guid Value { get; }

    public GuidId(Guid value)
    {
        Value = value;
    }
    public static implicit operator GuidId(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}