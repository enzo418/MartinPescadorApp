using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.Domain.FisherAggregate;

public sealed class Fisher : AggregateRoot<FisherId>
{
    public UserId UserId { get; private set; }

    private Fisher(FisherId id, UserId userId) : base(id)
    {
        UserId = userId;
    }

    public static Fisher Create(FisherId id, UserId userId)
    {
        return new Fisher(id, userId);
    }

#pragma warning disable CS8618
    private Fisher()
    {
    }
#pragma warning restore CS8618
}