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

    public static Location Create(string city, string state, string country, string place)
    {
        return new Location
        {
            City = city,
            State = state,
            Country = country,
            Place = place
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return City;
        yield return State;
        yield return Country;
        yield return Place;
    }
}