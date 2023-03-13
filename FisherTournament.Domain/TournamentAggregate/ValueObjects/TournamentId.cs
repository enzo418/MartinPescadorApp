namespace FisherTournament.Domain.TournamentAggregate.ValueObjects;

public class TournamentId : GuidId<TournamentId>
{
    public static implicit operator TournamentId(Guid value) => new(value);
    public static implicit operator Guid(TournamentId value) => value.Value;

    public TournamentId(Guid value) : base(value)
    {
    }
}