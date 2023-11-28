using ErrorOr;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.Domain.UserAggregate;

using Common.Errors;

public class User : AggregateRoot<UserId>
{
    private User(UserId id, string firstName, string lastName, string dni, FisherId? fisherId) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        FisherId = fisherId;
        DNI = dni;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DNI { get; private set; }

    public FisherId? FisherId { get; private set; } = null;

    public static User Create(string firstName, string lastName, string dni, FisherId? fisherId)
    {
        return new User(Guid.NewGuid(), firstName, lastName, dni, fisherId);
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

    /// <summary>
    /// Change the user's name.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public void ChangeName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Change the user's DNI.
    /// </summary>
    /// <param name="dni"></param>
    /// <returns></returns>
    public ErrorOr<Success> ChangeDNI(string dni)
    {
        DNI = dni;
        return Result.Success;
    }


    /// <summary>
    /// Unassigns the fisher from the user.
    /// </summary>
    /// <returns></returns>
    public void WithoutFisher()
    {
        FisherId = null;
    }

#pragma warning disable CS8618
    private User()
    {
    }
#pragma warning restore CS8618
}