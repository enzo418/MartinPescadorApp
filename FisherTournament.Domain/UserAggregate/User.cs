using ErrorOr;
using FisherTournament.Domain;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.Domain.UserAggregate;

using Common.Errors;

public class User : AggregateRoot<UserId>
{
    private User(UserId id, string firstName, string lastName, FisherId? fisherId) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        FisherId = fisherId;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public FisherId? FisherId { get; private set; }

    public static User Create(string firstName, string lastName, FisherId? fisherId)
    {
        return new User(Guid.NewGuid(), firstName, lastName, fisherId);
    }

    /// <summary>
    /// Assigns a fisher to the user.
    /// </summary>
    /// <param name="fisherId"></param>
    /// <returns></returns>
    public ErrorOr<Success> WithFisher(FisherId fisherId)
    {
        if (FisherId != null)
        {
            return Errors.Users.AlreadyHasFisher;
        }

        FisherId = fisherId;

        return Result.Success;
    }

#pragma warning disable CS8618
    private User()
    {
    }
#pragma warning restore CS8618
}