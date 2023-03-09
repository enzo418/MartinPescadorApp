using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate;

public sealed class Competition : AggregateRoot<CompetitionId>
{
    private Competition(CompetitionId id, DateTime startDateTime, DateTime? endDateTime, TournamentId tournamentId, Location location)
        : base(id)
    {
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        TournamentId = tournamentId;
        Location = location;
    }

    public DateTime StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public TournamentId TournamentId { get; private set; }
    public Location Location { get; private set; }

# pragma warning disable CS8618
    private Competition()
    {
    }
# pragma warning restore CS8618
}