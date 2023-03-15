using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate;

public sealed class Competition : AggregateRoot<CompetitionId>
{
    private List<CompetitionParticipation> _participations = new();

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
    public IReadOnlyCollection<CompetitionParticipation> Participations => _participations.AsReadOnly();

    public static Competition Create(DateTime startDateTime, TournamentId tournamentId, Location location)
    {
        return new Competition(Guid.NewGuid(), startDateTime, null, tournamentId, location);
    }

    public void AddScore(FisherId fisherId, int score)
    {
        var participation = _participations.Where(x => x.FisherId == fisherId)
                                .SingleOrDefault();
        if (participation == null)
        {
            participation = CompetitionParticipation.Create(Id, fisherId);
            _participations.Add(participation);
        }

        participation.AddFishCaught(FishCaught.Create(this.Id, fisherId, score));
    }


#pragma warning disable CS8618
    private Competition()
    {
    }
# pragma warning restore CS8618
}