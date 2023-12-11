using ErrorOr;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.DomainEvents;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Domain.TournamentAggregate;

public class Tournament : AggregateRoot<TournamentId>
{
    public static readonly string GeneralCategoryName = "General";

    private List<CompetitionId> _competitionsIds = new();
    private List<TournamentInscription> _inscriptions = new();
    private List<Category> _categories = new();

    private Tournament(TournamentId id,
                       string name,
                       DateTime startDate,
                       DateTime? endDate,
                       List<Category> categories)
        : base(id)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        _categories = categories;

        AddCategory(Category.Create(GeneralCategoryName));
    }

    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public IReadOnlyCollection<CompetitionId> CompetitionsIds => _competitionsIds.AsReadOnly();
    public IReadOnlyCollection<TournamentInscription> Inscriptions => _inscriptions.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public void AddCompetition(CompetitionId competitionId)
    {
        _competitionsIds.Add(competitionId);

        AddDomainEvent(new CompetitionAddedDomainEvent(Id, competitionId));
    }

    public void RemoveCompetition(CompetitionId competitionId)
    {
        _competitionsIds.Remove(competitionId);
    }

    public ErrorOr<Success> AddInscription(FisherId fisherId,
                                           CategoryId categoryId,
                                           int number,
                                           IDateTimeProvider dateTimeProvider)
    {
        if (Inscriptions.Any(i => i.FisherId == fisherId))
        {
            return Errors.Tournaments.InscriptionAlreadyExists;
        }

        if (Inscriptions.Any(i => i.Number == number))
        {
            return Errors.Tournaments.InscriptionNumberAlreadyExists;
        }

        if (EndDate < dateTimeProvider.Now)
        {
            return Errors.Tournaments.IsOver;
        }

        if (!Categories.Any(c => c.Id == categoryId))
        {
            return Errors.Categories.NotFound;
        }

        if (Categories.Where(c => c.Name == GeneralCategoryName).Any(c => c.Id == categoryId))
        {
            return Errors.Tournaments.CannotAddInscriptionToGeneralCategory;
        }

        _inscriptions.Add(TournamentInscription.Create(Id, fisherId, categoryId, number));

        AddDomainEvent(new InscriptionAddedDomainEvent(fisherId, categoryId, Id));

        return Result.Success;
    }

    public ErrorOr<Updated> UpdateInscription(FisherId fisherId,
                                              CategoryId? categoryId,
                                              int? number,
                                              IDateTimeProvider dateTimeProvider)
    {
        if (EndDate < dateTimeProvider.Now)
        {
            return Errors.Tournaments.IsOver;
        }

        var inscription = _inscriptions.Find(i => i.FisherId == fisherId);

        if (inscription == null)
        {
            return Errors.Tournaments.InscriptionNotFound;
        }

        if (categoryId is not null)
        {
            if (!Categories.Any(c => c.Id == categoryId))
            {
                return Errors.Categories.NotFound;
            }

            if (Categories.Where(c => c.Name == GeneralCategoryName).Any(c => c.Id == categoryId))
            {
                return Errors.Tournaments.CannotAddInscriptionToGeneralCategory;
            }

            inscription.UpdateCategory(categoryId);
        }

        if (number.HasValue)
        {
            if (Inscriptions.Any(i => i.Number == number))
            {
                return Errors.Tournaments.InscriptionNumberAlreadyExists;
            }

            inscription.UpdateNumber(number.Value);
        }

        // TODO: Update leadeboard ???
        //AddDomainEvent(new InscriptionUpdatedDomainEvent(fisherId, categoryId, Id));

        return Result.Updated;
    }

    public bool IsFisherEnrolled(FisherId fisherId)
    {
        return _inscriptions.Find(x => x.FisherId == fisherId) != null;
    }

    public ErrorOr<Category> AddCategory(string categoryName)
    {
        if (_categories.Any(c => c.Name == categoryName))
        {
            return Errors.Categories.AlreadyExistsWithName;
        }

        var category = Category.Create(categoryName);

        _categories.Add(category);

        return category;
    }

    public ErrorOr<Category> AddCategory(Category category)
    {
        if (_categories.Any(c => c.Name == category.Name))
        {
            return Errors.Categories.AlreadyExistsWithName;
        }

        _categories.Add(category);

        return category;
    }

    public ErrorOr<Success> DeleteCategory(CategoryId categoryId)
    {
        var category = _categories.Find(c => c.Id == categoryId);

        if (category == null)
        {
            return Errors.Categories.NotFound;
        }

        if (category.Name == GeneralCategoryName)
        {
            return Error.Conflict();
        }

        _categories.Remove(category);

        return Result.Success;
    }

    public ErrorOr<Success> EditCategory(CategoryId id, string name)
    {
        if (_categories.Any(c => c.Id == id))
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);

            if (category is not null)
            {
                if (_categories.Any(c => c.Name == name))
                {
                    return Errors.Categories.AlreadyExistsWithName;
                }

                if (category.Name == GeneralCategoryName)
                {
                    return Error.Conflict();
                }

                category.ChangeName(name);
            }

            return Result.Success;
        } else
        {
            return Errors.Categories.NotFound;
        }
    }

    public static Tournament Create(string name,
                                    DateTime startDate,
                                    DateTime? endDate,
                                    List<Category>? categories = null)
    {
        return new Tournament(Guid.NewGuid(),
                              name,
                              startDate,
                              endDate,
                              categories ?? new List<Category>());
    }

    public ErrorOr<Success> SetName(string name)
    {
        Name = name;

        return Result.Success;
    }

    public ErrorOr<Success> SetStartDate(DateTime startDate)
    {
        if (startDate.Kind != DateTimeKind.Utc)
        {
            return Error.Validation(nameof(startDate), "Start date must be UTC");
        }

        StartDate = startDate;

        return Result.Success;
    }

    public ErrorOr<Success> EndTournament(IDateTimeProvider dateTimeProvider)
    {
        if (EndDate != null)
        {
            return Errors.Tournaments.AlreadyEnded;
        }

        EndDate = dateTimeProvider.Now;

        return Result.Success;
    }

    public ErrorOr<Success> UndoEndTournament()
    {
        EndDate = null;

        return Result.Success;
    }

#pragma warning disable CS8618
    private Tournament()
    {
    }
#pragma warning restore CS8618
}