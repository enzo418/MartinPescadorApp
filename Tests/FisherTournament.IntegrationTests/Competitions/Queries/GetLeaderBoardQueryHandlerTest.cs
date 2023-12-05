using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Competitions.Queries
{
    // Fluent Assertion for the leaderboard tests, this makes it easier to write/read but difficult to pinpoint the exact error
    // just form the console without debugging.
    public static class LeaderBoardAssert
    {
        public static LeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<LeaderBoardCategory> leaderBoardCategories, string Name, CategoryId Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static LeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<LeaderBoardCategory> leaderBoardCategories, string Name, string Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static IEnumerable<LeaderBoardItem> ShouldHaveNPositions(this LeaderBoardCategory leaderBoardCategory, int n)
        {
            leaderBoardCategory.LeaderBoard.Should().HaveCount(n);
            return leaderBoardCategory.LeaderBoard;
        }

        public static IEnumerable<LeaderBoardItem> ShouldHavePosition(this IEnumerable<LeaderBoardItem> leaderBoard, int position, FisherId fisherId, int totalScore)
        {
            var leaderBoardPosition = leaderBoard.ElementAt(position - 1);
            leaderBoardPosition.FisherId.Should().Be(fisherId.ToString());
            leaderBoardPosition.TotalScore.Should().Be(totalScore);
            return leaderBoard;
        }
    }


    public class GetLeaderBoardQueryHandlerTest : BaseUseCaseTest
    {
        public GetLeaderBoardQueryHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_ReturnEmptyLeaderBoard()
        {
            // Arrange
            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(Guid.NewGuid().ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().BeEmpty();
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoard()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
            var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Secondary")
                .WithInscription(fisher4.Id, 4, "Secondary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");
            var categorySecondary = tournament.Categories.First(c => c.Name == "Secondary");

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 1)
                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 200)
                .WithScore(fisher3.Id, 100)
                .WithScore(fisher4.Id, 5)

                .WithN(1)

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher1.Id, 11);

            result.Value.ShouldHaveCategoryLeaderBoard("Secondary", categorySecondary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher3.Id, 100)
                        .ShouldHavePosition(2, fisher4.Id, 5);

            result.Value.ShouldHaveCategoryLeaderBoard("General", "General")
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher3.Id, 100)
                        .ShouldHavePosition(3, fisher1.Id, 11)
                        .ShouldHavePosition(4, fisher4.Id, 5);

        }

        // loads two competitions, some scores on them and check that the scores are not mixed between competitions
        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardNotMixed_TwoCompetitions()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
            var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Secondary")
                .WithInscription(fisher4.Id, 4, "Secondary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");
            var categorySecondary = tournament.Categories.First(c => c.Name == "Secondary");

            var competition1 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 1)
                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 200)
                .WithScore(fisher3.Id, 100)
                .WithScore(fisher4.Id, 5)

                .WithN(1)

                .Build());

            var competition2 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 100)
                .WithScore(fisher2.Id, 200)
                .WithScore(fisher3.Id, 300)
                .WithScore(fisher4.Id, 400)

                .WithN(2)

                .Build());

            await context.SaveChangesAsync(CancellationToken.None);

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition1.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher1.Id, 11);

            result.Value.ShouldHaveCategoryLeaderBoard("Secondary", categorySecondary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher3.Id, 100)
                        .ShouldHavePosition(2, fisher4.Id, 5);

            result.Value.ShouldHaveCategoryLeaderBoard("General", "General")
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher3.Id, 100)
                        .ShouldHavePosition(3, fisher1.Id, 11)
                        .ShouldHavePosition(4, fisher4.Id, 5);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByLargerPiece()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)
                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 10)
                .WithScore(fisher2.Id, 20)

                // fisher3 and fisher2 are tied, but fisher3 has a larger piece
                // (25 vs 20) so it's 2° and fisher2 is 3°
                .WithScore(fisher3.Id, 25)
                .WithScore(fisher3.Id, 5)

                .WithN(1)

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(3)
                        .ShouldHavePosition(1, fisher1.Id, 60)
                        .ShouldHavePosition(2, fisher3.Id, 30)
                        .ShouldHavePosition(3, fisher2.Id, 30);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByLargerPiece_WithMultipleCompetitions()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")

                .Build(CancellationToken.None);

            var competition1 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)
                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 10)
                .WithScore(fisher2.Id, 20)

                // fisher3 and fisher2 are tied, but fisher3 has a larger piece
                // (25 vs 20) so it's 2° and fisher2 is 3°
                .WithScore(fisher3.Id, 25)
                .WithScore(fisher3.Id, 5)

                .WithN(1)

                .Build());


            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            // competition to verify that the competition leader board is not affected by other competitions
            var competition2 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 500)

                .WithScore(fisher3.Id, 50)

                .WithN(2)

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition1.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(3)
                        .ShouldHavePosition(1, fisher1.Id, 60)
                        .ShouldHavePosition(2, fisher3.Id, 30)
                        .ShouldHavePosition(3, fisher2.Id, 30);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByFirstLargerPiece()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)
                .WithScore(fisher1.Id, 10)

                .WithScore(fisher2.Id, 6)
                .WithScore(fisher2.Id, 15)
                .WithScore(fisher2.Id, 14)

                // fisher3 and fisher2 are tied and have the same larger piece (15),
                // but fisher2 has a larger piece (6 vs 5) so it's 2° and fisher3 is 3°
                .WithScore(fisher3.Id, 5)
                .WithScore(fisher3.Id, 15)
                .WithScore(fisher3.Id, 15)

                .WithN(1)

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(3)
                        .ShouldHavePosition(1, fisher1.Id, 60)
                        .ShouldHavePosition(2, fisher2.Id, 35)
                        .ShouldHavePosition(3, fisher3.Id, 35);
        }
    }
}