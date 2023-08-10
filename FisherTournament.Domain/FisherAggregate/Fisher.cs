using ErrorOr;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.Domain.FisherAggregate;

public sealed class Fisher : AggregateRoot<FisherId>
{
    public string Name { get; private set; }

    private Fisher(FisherId id, string name) : base(id)
    {
        Name = name;
    }

    public static Fisher Create(string firstName, string secondName)
    {
        return new Fisher(Guid.NewGuid(), $"{secondName} {firstName}");
    }

    public void ChangeName(string firstName, string secondName)
    {
        Name = $"{secondName} {firstName}";
    }

#pragma warning disable CS8618
    private Fisher()
    {
    }
#pragma warning restore CS8618
}