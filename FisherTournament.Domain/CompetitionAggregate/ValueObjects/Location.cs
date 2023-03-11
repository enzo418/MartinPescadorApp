namespace FisherTournament.Domain.CompetitionAggregate.Entities;

public sealed class Location : ValueObject
{
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string Place { get; private set; } = null!;

# pragma warning disable CS8618
    private Location()
    {
    }
#pragma warning restore CS8618

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return City;
        yield return State;
        yield return Country;
        yield return Place;
    }
}