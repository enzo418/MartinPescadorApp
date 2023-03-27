using ErrorOr;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
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

    public ErrorOr<Success> AddScore(FisherId fisherId, int score, IDateTimeProvider dateTimeProvider)
    {
        if (EndDateTime.HasValue)
            return Errors.Competitions.HasEnded;

        if (StartDateTime > dateTimeProvider.Now)
            return Errors.Competitions.HasNotStarted;

        var participation = _participations.Where(x => x.FisherId == fisherId)
                                .SingleOrDefault();
        if (participation == null)
        {
            participation = CompetitionParticipation.Create(Id, fisherId);
            _participations.Add(participation);
        }

        participation.AddFishCaught(FishCaught.Create(this.Id, fisherId, score, dateTimeProvider));

        this.AddDomainEvent(new ScoreAddedDomainEvent(this.Id, fisherId, score));

        return Result.Success;
    }

    public void EndCompetition(IDateTimeProvider dateTimeProvider)
    {
        EndDateTime = dateTimeProvider.Now;
    }

    public void AddParticipation(FisherId fisherId)
    {
        if (_participations.Any(x => x.FisherId == fisherId))
            return;

        var participation = CompetitionParticipation.Create(Id, fisherId);

        _participations.Add(participation);

        AddDomainEvent(new ParticipationAddedDomainEvent(fisherId, this.Id));
    }

#pragma warning disable CS8618
    private Competition()
    {
    }
# pragma warning restore CS8618
}