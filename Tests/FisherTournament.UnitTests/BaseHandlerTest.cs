using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MockQueryable.Moq;

namespace FisherTournament.UnitTests;

public abstract class BaseHandlerTest
{
    protected readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    protected readonly Mock<ITournamentFisherDbContext> _contextMock = new();

    /// <summary>
    /// Mocks a Category, because its id is supposed to be generated on add.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected Mock<Category> MockCategory(int id)
    {
        CategoryId categoryId = CategoryId.Create(id).Value;

        var categoryMock = new Mock<Category>();
        categoryMock.SetupGet(p => p.Id).Returns(categoryId);

        return categoryMock;
    }
}

public static partial class Extension
{
    public static Mock<ITournamentFisherDbContext> SetupTournaments(
        this Mock<ITournamentFisherDbContext> ctx,
        List<Tournament> tournaments)
    {
        ctx
            .Setup(c => c.Set<Tournament>())
            .Returns(tournaments.AsQueryable().BuildMockDbSet().Object);

        ctx
            .SetupGet(p => p.Tournaments)
            .Returns(tournaments.AsQueryable().BuildMockDbSet().Object);

        // By not setting up FindAsync, FirstOrDefaultAsync, etc. we are also testing that the
        // handlers are using the requested ids.

        return ctx;
    }

    public static Mock<ITournamentFisherDbContext> SetupTournament(
        this Mock<ITournamentFisherDbContext> ctx,
        Tournament tournament)
    {
        ctx
            .Setup(c => c.Set<Tournament>())
            .Returns(new List<Tournament>() { tournament }.AsQueryable().BuildMockDbSet().Object);

        ctx
            .SetupGet(p => p.Tournaments)
            .Returns(new List<Tournament>() { tournament }.AsQueryable().BuildMockDbSet().Object);

        return ctx;
    }

    public static Mock<ITournamentFisherDbContext> SetupFisher(
        this Mock<ITournamentFisherDbContext> ctx,
        Fisher fisher)
    {
        ctx
            .Setup(c => c.Set<Fisher>())
            .Returns(new List<Fisher>() { fisher }.AsQueryable().BuildMockDbSet().Object);

        ctx
            .Setup(c => c.Fishers)
            .Returns(new List<Fisher>() { fisher }.AsQueryable().BuildMockDbSet().Object);

        return ctx;
    }

    public static Mock<ITournamentFisherDbContext> SetupFishers(
        this Mock<ITournamentFisherDbContext> ctx,
        List<Fisher> fishers)
    {
        ctx
            .Setup(c => c.Set<Fisher>())
            .Returns(fishers.AsQueryable().BuildMockDbSet().Object);

        ctx
            .Setup(c => c.Fishers)
            .Returns(fishers.AsQueryable().BuildMockDbSet().Object);

        return ctx;
    }

    public static Mock<ITournamentFisherDbContext> SetupCompetitions(
        this Mock<ITournamentFisherDbContext> ctx,
        List<Competition> competitions)
    {
        ctx
            .Setup(c => c.Set<Competition>())
            .Returns(competitions.AsQueryable().BuildMockDbSet().Object);

        ctx
            .Setup(c => c.Competitions)
            .Returns(competitions.AsQueryable().BuildMockDbSet().Object);

        return ctx;
    }

    public static Mock<ITournamentFisherDbContext> SetupCompetition(
        this Mock<ITournamentFisherDbContext> ctx,
        Competition competition)
    {
        ctx
            .Setup(c => c.Set<Competition>())
            .Returns(new List<Competition>() { competition }.AsQueryable().BuildMockDbSet().Object);

        ctx
            .Setup(c => c.Competitions)
            .Returns(new List<Competition>() { competition }.AsQueryable().BuildMockDbSet().Object);

        return ctx;
    }
}