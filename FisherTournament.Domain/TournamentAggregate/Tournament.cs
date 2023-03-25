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
    private List<Category> _categories = new();

    private Tournament(TournamentId id,
                       string name,
                       DateTime startDate,
                       DateTime endDate,
                       List<Category> categories)
        : base(id)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        _categories = categories;
    }

    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public IReadOnlyCollection<CompetitionId> CompetitionsIds => _competitionsIds.AsReadOnly();
    public IReadOnlyCollection<TournamentInscription> Inscriptions => _inscriptions.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public void AddCompetition(CompetitionId competitionId)
    {
        _competitionsIds.Add(competitionId);
    }

    public ErrorOr<Success> AddInscription(FisherId fisherId,
                                           CategoryId categoryId,
                                           IDateTimeProvider dateTimeProvider)
    {
        if (Inscriptions.Any(i => i.FisherId == fisherId))
        {
            return Errors.Tournaments.InscriptionAlreadyExists;
        }

        if (EndDate < dateTimeProvider.Now)
        {
            return Errors.Tournaments.IsOver;
        }

        if (!Categories.Any(c => c.Id == categoryId))
        {
            return Errors.Categories.NotFound;
        }

        _inscriptions.Add(TournamentInscription.Create(Id, fisherId, categoryId));

        return Result.Success;
    }

    public bool IsFisherEnrolled(FisherId fisherId)
    {
        return _inscriptions.Find(x => x.FisherId == fisherId) != null;
    }

    public ErrorOr<Category> AddCategory(string categoryName)
    {
        if (_categories.Any(c => c.Name == categoryName))
        {
            return Errors.Categories.AlreadyExists;
        }

        var category = Category.Create(categoryName);

        _categories.Add(category);

        return category;
    }

    public ErrorOr<Category> AddCategory(Category category)
    {
        if (_categories.Any(c => c.Name == category.Name))
        {
            return Errors.Categories.AlreadyExists;
        }

        _categories.Add(category);

        return category;
    }

    public static Tournament Create(string name,
                                    DateTime startDate,
                                    DateTime endDate,
                                    List<Category>? categories = null)
    {
        return new Tournament(Guid.NewGuid(),
                              name,
                              startDate,
                              endDate,
                              categories ?? new List<Category>());
    }

#pragma warning disable CS8618
    private Tournament()
    {
    }
#pragma warning restore CS8618
}