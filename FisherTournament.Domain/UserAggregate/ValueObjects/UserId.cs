namespace FisherTournament.Domain.UserAggregate.ValueObjects;

public sealed class UserId : GuidId<UserId>
{
    public static implicit operator UserId(Guid value) => new(value);
    public static implicit operator Guid(UserId value) => value.Value;
    public UserId(Guid value) : base(value)
    {
    }
}