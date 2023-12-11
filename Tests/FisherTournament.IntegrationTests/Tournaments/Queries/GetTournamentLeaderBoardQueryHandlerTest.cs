using FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Tournaments.Queries
{
    // Fluent Assertion for the leaderboard tests, this makes it easier to write/read but difficult to pinpoint the exact error
    // just form the console without debugging.
    public static class LeaderBoardAssert
    {
        public static TournamentLeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<TournamentLeaderBoardCategory> leaderBoardCategories,
                                                                                  string Name,
                                                                                  CategoryId Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static TournamentLeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<TournamentLeaderBoardCategory> leaderBoardCategories,
                                                                                 string Name,
                                                                                 string Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static IEnumerable<TournamentLeaderBoardItem> ShouldHaveNPositions(this TournamentLeaderBoardCategory leaderBoardCategory,
                                                                                  int n)
        {
            leaderBoardCategory.LeaderBoard.Should().HaveCount(n);
            return leaderBoardCategory.LeaderBoard;
        }

        public static IEnumerable<TournamentLeaderBoardItem> ShouldHavePosition(this IEnumerable<TournamentLeaderBoardItem> leaderBoard,
                                                                                int position,
                                                                                FisherId fisherId,
                                                                                List<int> competitionPositions)
        {
            var leaderBoardPosition = leaderBoard.Where(p => p.Position == position).ToList();

            leaderBoardPosition.Count.Should().Be(1);

            leaderBoardPosition.First().FisherId.Should().Be(fisherId.ToString());

            leaderBoardPosition.First().Position.Should().Be(position);

            leaderBoardPosition.First().CompetitionPositions.Should().BeEquivalentTo(competitionPositions);

            return leaderBoard;
        }
    }

    public class GetTournamentLeaderBoardQueryHandlerTest : BaseUseCaseTest
    {
        public GetTournamentLeaderBoardQueryHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_ReturnEmptyLeaderBoard()
        {
            // Arrange
            // Act
            var result = await _fixture.SendAsync(new GetTournamentLeaderBoardQuery(Guid.NewGuid().ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().BeEmpty();
        }

        [Fact]
        public async Task Handler_Should_ReturnValidLeaderboard()
        {
            using var context = _fixture.TournamentContext;

            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
            var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

            var fisher5 = context.PrepareFisher("First5", "Last5", out var _);
            var fisher6 = context.PrepareFisher("First6", "Last6", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

                .WithCategory("Primary")
                .WithCategory("Secondary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")
                .WithInscription(fisher4.Id, 4, "Primary")

                .WithInscription(fisher5.Id, 5, "Secondary")
                .WithInscription(fisher6.Id, 6, "Secondary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");
            var categorySecondary = tournament.Categories.First(c => c.Name == "Secondary");
            var categoryGeneral = tournament.Categories.First(c => c.Name == Tournament.GeneralCategoryName);

            var competition1 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                // category | general
                // 1°       |   1°
                .WithScore(fisher1.Id, 20)
                .WithScore(fisher1.Id, 10)

                // 2°       |   2°
                .WithScore(fisher3.Id, 20)
                .WithScore(fisher3.Id, 5)

                // 3°       |   3°
                .WithScore(fisher2.Id, 4)
                .WithScore(fisher2.Id, 1)

                // 4°       |   6°
                .WithScore(fisher4.Id, 1)


                // 1°       |   4°
                .WithScore(fisher5.Id, 3)
                .WithScore(fisher5.Id, 2)

                // 2°       |   5°
                .WithScore(fisher6.Id, 2)

                .WithN(1)

                .Build());

            var competition2 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City 2", "Test State 2", "Test Country 2", "Test Place 2"))

                // category | general
                // 1°       |   1°
                .WithScore(fisher3.Id, 20)
                .WithScore(fisher3.Id, 5)

                // 2°       |   2°
                .WithScore(fisher4.Id, 10)

                // 3°       |   3°
                .WithScore(fisher2.Id, 6)

                // 4°       |   6°
                .WithScore(fisher1.Id, 1)
                .WithScore(fisher1.Id, 1)


                // 1°       |   4°
                .WithScore(fisher5.Id, 5)

                // 2°       |   5°
                .WithScore(fisher6.Id, 2)

                .WithN(2)

                .Build());

            var competition3 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                    .WithTournament(tournament.Id)
                    .WithLocation(Location.Create("Test City 3", "Test State 3", "Test Country 3", "Test Place 3"))

                    // category | general
                    // 4°       |   7°
                    // fisher1 absent

                    // 1°       |   1°
                    .WithScore(fisher3.Id, 20)
                    .WithScore(fisher3.Id, 5)

                    // 2°       |   3°
                    .WithScore(fisher2.Id, 1)
                    .WithScore(fisher2.Id, 1)
                    .WithScore(fisher2.Id, 3)

                    // 3°       |   5°
                    .WithScore(fisher4.Id, 1)


                    // 1°       |   2°
                    .WithScore(fisher5.Id, 20)

                    // 2°       |   4°
                    .WithScore(fisher6.Id, 2)

                    .WithN(3)

                    .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetTournamentLeaderBoardQuery(tournament.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                .ShouldHaveNPositions(4)
                .ShouldHavePosition(1, fisher3.Id, new List<int> { 2, 1, 1 }) // 4
                .ShouldHavePosition(2, fisher2.Id, new List<int> { 3, 3, 2 }) // 8
                .ShouldHavePosition(3, fisher1.Id, new List<int> { 1, 4, 4 }) // 9 total score = 32
                .ShouldHavePosition(4, fisher4.Id, new List<int> { 4, 2, 3 }); // 9 total score = 12

            result.Value.ShouldHaveCategoryLeaderBoard("Secondary", categorySecondary.Id)
                .ShouldHaveNPositions(2)
                .ShouldHavePosition(1, fisher5.Id, new List<int> { 1, 1, 1 }) // 3
                .ShouldHavePosition(2, fisher6.Id, new List<int> { 2, 2, 2 }); // 6

            result.Value.ShouldHaveCategoryLeaderBoard(categoryGeneral.Name, categoryGeneral.Id)
                .ShouldHaveNPositions(6)
                .ShouldHavePosition(1, fisher3.Id, new List<int> { 2, 1, 1 }) // 4
                .ShouldHavePosition(2, fisher2.Id, new List<int> { 3, 3, 3 }) // 9
                .ShouldHavePosition(3, fisher5.Id, new List<int> { 4, 4, 2 }) // 10
                .ShouldHavePosition(4, fisher4.Id, new List<int> { 6, 2, 5 }); // 13
            /*.ShouldHavePosition(?, fisher1.Id, new List<int> { 1, 6, 7 }) // 14
            .ShouldHavePosition(?, fisher6.Id, new List<int> { 5, 5, 4 }) // 14*/

            var generalLeaderboard = result.Value.First(l => l.Id == categoryGeneral.Id);

            var fisher1Item = generalLeaderboard.LeaderBoard.FirstOrDefault(p => p.FisherId == fisher1.Id.ToString());
            var fisher6Item = generalLeaderboard.LeaderBoard.FirstOrDefault(p => p.FisherId == fisher6.Id.ToString());

            fisher1Item.Should().NotBeNull();
            fisher6Item.Should().NotBeNull();

            fisher1Item!.CompetitionPositions.Should().BeEquivalentTo(new List<int> { 1, 6, 7 });
            fisher6Item!.CompetitionPositions.Should().BeEquivalentTo(new List<int> { 5, 5, 4 });

            fisher1Item.Position.Should().NotBe(fisher6Item.Position); // One should win, ordered by some criteria.
        }

        /*
        [Fact]
        public async Task Handler_Should_ReturnValidLeaderboard_BreakTieBy_HigherTotalScore()
        {
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
                .WithInscription(fisher3.Id, 3, "Primary")
                .WithInscription(fisher4.Id, 4, "Primary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            var competition1 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                // 1°
                .WithScore(fisher1.Id, 20)

                // 2°
                .WithScore(fisher3.Id, 10)

                // 3°
                .WithScore(fisher2.Id, 5)

                // 4°
                .WithScore(fisher4.Id, 1)

                .WithN(1)

                .Build());

            var competition2 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City 2", "Test State 2", "Test Country 2", "Test Place 2"))

                // 1°
                .WithScore(fisher3.Id, 20)

                // 2°
                .WithScore(fisher4.Id, 10)

                // 3°
                .WithScore(fisher2.Id, 6)

                // 4°
                .WithScore(fisher1.Id, 2)

                .WithN(2)

                .Build());

            var competition3 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City 3", "Test State 3", "Test Country 3", "Test Place 3"))

                // 1°
                .WithScore(fisher1.Id, 20)

                // 2°
                .WithScore(fisher3.Id, 20)

                // 3°
                .WithScore(fisher2.Id, 5)

                // 4°
                .WithScore(fisher4.Id, 1)

                .WithN(3)

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetTournamentLeaderBoardQuery(tournament.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                .ShouldHaveNPositions(4)
                .ShouldHavePosition(1, fisher3.Id, new List<int> { 1, 2, 2 }) // 4
                .ShouldHavePosition(2, fisher1.Id, new List<int> { 2, 1, 4 }) // 7
                .ShouldHavePosition(3, fisher2.Id, new List<int> { 3, 3, 1 }) // 7
                .ShouldHavePosition(4, fisher4.Id, new List<int> { 4, 2, 3 }); // 10
        }
        */
    }
}