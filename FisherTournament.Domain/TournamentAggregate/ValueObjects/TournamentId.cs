namespace FisherTournament.Domain.TournamentAggregate.ValueObjects;

public class TournamentId : GuidId
{
    public static implicit operator TournamentId(Guid value) => new(value);
    public static implicit operator Guid(TournamentId value) => value.Value;

    public TournamentId(Guid value) : base(value)
    {
    }
}