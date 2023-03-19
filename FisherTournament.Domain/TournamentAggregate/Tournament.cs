using System.Collections.Generic;
using ErrorOr;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;

namespace FisherTournament.Domain.TournamentAggregate;

public class Tournament : AggregateRoot<TournamentId>
{
    private List<CompetitionId> _competitionsIds = new();
    private List<TournamentInscription> _inscriptions = new();

    private Tournament(TournamentId id, string name, DateTime startDate, DateTime endDate)
        : base(id)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public IReadOnlyCollection<CompetitionId> CompetitionsIds => _competitionsIds.AsReadOnly();
    public IReadOnlyCollection<TournamentInscription> Inscriptions => _inscriptions.AsReadOnly();

    public void AddCompetition(CompetitionId competitionId)
    {
        _competitionsIds.Add(competitionId);
    }

    public ErrorOr<Success> AddInscription(FisherId fisherId, IDateTimeProvider dateTimeProvider)
    {
        if (Inscriptions.Any(i => i.FisherId == fisherId))
        {
            return Errors.Tournament.InscriptionAlreadyExists;
        }

        if (EndDate < dateTimeProvider.Now)
        {
            return Errors.Tournament.IsOver;
        }

        _inscriptions.Add(TournamentInscription.Create(Id, fisherId));

        return Result.Success;
    }

    public bool IsFisherEnrolled(FisherId fisherId)
    {
        return _inscriptions.Find(x => x.FisherId == fisherId) != null;
    }

    public static Tournament Create(string name, DateTime startDate, DateTime endDate)
    {
        return new Tournament(Guid.NewGuid(), name, startDate, endDate);
    }

#pragma warning disable CS8618
    private Tournament()
    {
    }
#pragma warning restore CS8618
}