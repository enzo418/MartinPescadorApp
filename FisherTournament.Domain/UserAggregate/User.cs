using FisherTournament.Domain;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.Domain.UserAggregate;

public class User : AggregateRoot<UserId>
{
    private User(UserId id, string firstName, string lastName) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public static User Create(string firstName, string lastName)
    {
        return new User(Guid.NewGuid(), firstName, lastName);
    }

#pragma warning disable CS8618
    private User()
    {
    }
#pragma warning restore CS8618
}