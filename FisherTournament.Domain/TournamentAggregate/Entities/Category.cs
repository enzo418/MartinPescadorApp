using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate.Entities;

public class Category : Entity<CategoryId>
{
    public string Name { get; private set; }

    private Category(string name) : base()
    {
        Name = name;
    }

    public void ChangeName(string name)
    {
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(name);
    }

#pragma warning disable CS8618
    public Category()
    {

    }
#pragma warning restore CS8618
}