using ErrorOr;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.DomainEvents;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.CompetitionAggregate;

public sealed class Competition : AggregateRoot<CompetitionId>
{
    private List<CompetitionParticipation> _participations = new();

    private Competition(CompetitionId id,
                     DateTime startDateTime,
                     DateTime? endDateTime,
                     TournamentId tournamentId,
                     Location location,
                     int n)
        : base(id)
    {
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        TournamentId = tournamentId;
        Location = location;
        N = n;
    }

    public DateTime StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public TournamentId TournamentId { get; private set; }
    public Location Location { get; private set; }
    public IReadOnlyCollection<CompetitionParticipation> Participations => _participations.AsReadOnly();

    /// <summary>
    /// Competition number.
    /// For example, the first competition of the tournament is number 1.
    /// </summary>
    public int N { get; private set; }

    public static Competition Create(DateTime startDateTime, TournamentId tournamentId, Location location, int competitionNumber)
    {
        if (competitionNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(competitionNumber));

        return new Competition(Guid.NewGuid(), startDateTime, null, tournamentId, location, competitionNumber);
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
            AddParticipation(fisherId);
            return AddScore(fisherId, score, dateTimeProvider);
        }

        participation.AddFishCaught(FishCaught.Create(this.Id, fisherId, score, dateTimeProvider));

        this.AddDomainEvent(new ScoreAddedDomainEvent(this.Id, fisherId, score));

        return Result.Success;
    }

    public ErrorOr<Success> AddScores(FisherId fisherId, IEnumerable<int> scores, IDateTimeProvider dateTimeProvider)
    {
        if (EndDateTime.HasValue)
            return Errors.Competitions.HasEnded;

        if (StartDateTime > dateTimeProvider.Now)
            return Errors.Competitions.HasNotStarted;

        var participation = _participations.Where(x => x.FisherId == fisherId)
                                .SingleOrDefault();
        if (participation == null)
        {
            AddParticipation(fisherId);
            return AddScores(fisherId, scores, dateTimeProvider);
        }

        foreach (var score in scores)
        {
            participation.AddFishCaught(FishCaught.Create(this.Id, fisherId, score, dateTimeProvider));
        }

        // No need to trigger an event. AddParticipation already does that (it was removed before this call)

        return Result.Success;
    }

    public void EndCompetition(IDateTimeProvider dateTimeProvider)
    {
        EndDateTime = dateTimeProvider.Now;
    }

    public void UndoEndCompetition()
    {
        EndDateTime = null;
    }

    public void AddParticipation(FisherId fisherId)
    {
        if (_participations.Any(x => x.FisherId == fisherId))
            return;

        var participation = CompetitionParticipation.Create(Id, fisherId);

        _participations.Add(participation);

        AddDomainEvent(new ParticipationAddedDomainEvent(fisherId, this.Id));
    }

    public void RemoveParticipation(FisherId fisherId)
    {
        var participation = _participations.FirstOrDefault(x => x.FisherId == fisherId);

        if (participation is null)
            return;

        _participations.Remove(participation);

        AddDomainEvent(new ParticipationRemovedDomainEvent(fisherId, this.Id));
    }

    public void EditStartDate(DateTime startDateTime)
    {
        if (startDateTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be UTC", nameof(startDateTime));

        StartDateTime = startDateTime;
    }

    public void EditLocation(Location location)
    {
        Location = location;
    }

#pragma warning disable CS8618
    private Competition()
    {
    }
#pragma warning restore CS8618
}