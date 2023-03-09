using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.FisherAggregate;

public sealed class Fisher : AggregateRoot<FisherId>
{
    private Fisher(FisherId id, string firstName, string lastName)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
}