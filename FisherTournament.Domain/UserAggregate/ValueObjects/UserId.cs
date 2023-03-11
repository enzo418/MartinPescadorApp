namespace FisherTournament.Domain.UserAggregate.ValueObjects;

public sealed class UserId : GuidId
{
    public UserId(Guid value) : base(value)
    {
    }
}