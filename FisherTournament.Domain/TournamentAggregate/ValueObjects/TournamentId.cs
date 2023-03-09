namespace FisherTournament.Domain.TournamentAggregate.ValueObjects;

public class TournamentId : GuidId
{
    public TournamentId(Guid value) : base(value)
    {
    }
}