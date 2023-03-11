using System.Collections.Generic;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate;

public class Tournament : AggregateRoot<TournamentId>
{
    private List<CompetitionId> _competitions = new();
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

    public IReadOnlyCollection<TournamentInscription> Inscriptions => _inscriptions.AsReadOnly();

    public void AddCompetition(CompetitionId competitionId)
    {
        _competitions.Add(competitionId);
    }

    public void AddInscription(FisherId fisherId)
    {
        _inscriptions.Add(TournamentInscription.Create(Id, fisherId));
    }

#pragma warning disable CS8618
    private Tournament()
    {
    }
#pragma warning restore CS8618
}