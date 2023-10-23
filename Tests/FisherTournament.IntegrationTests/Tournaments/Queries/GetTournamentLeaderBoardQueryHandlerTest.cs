using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var leaderBoardPosition = leaderBoard.ElementAt(position - 1);
            leaderBoardPosition.FisherId.Should().Be(fisherId);
            leaderBoardPosition.Position.Should().Be(position);
            leaderBoardPosition.CompetitionPositions.Should().BeEquivalentTo(competitionPositions);
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

            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");
            var fisher4 = context.PrepareFisher("First4", "Last4");

            var fisher5 = context.PrepareFisher("First5", "Last5");
            var fisher6 = context.PrepareFisher("First6", "Last6");

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

            var competition1 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                // 1°
                .WithScore(fisher1.Id, 20)
                .WithScore(fisher1.Id, 10)

                // 2°
                .WithScore(fisher3.Id, 20)
                .WithScore(fisher3.Id, 5)

                // 3°
                .WithScore(fisher2.Id, 5)

                // 4°
                .WithScore(fisher4.Id, 1)


                // 1°
                .WithScore(fisher5.Id, 5)

                // 2°
                .WithScore(fisher6.Id, 2)

                .Build());

            var competition2 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City 2", "Test State 2", "Test Country 2", "Test Place 2"))

                // 1°
                .WithScore(fisher3.Id, 20)
                .WithScore(fisher3.Id, 5)

                // 2°
                .WithScore(fisher4.Id, 10)

                // 3°
                .WithScore(fisher2.Id, 6)

                // 4°
                .WithScore(fisher1.Id, 2)


                // 1°
                .WithScore(fisher5.Id, 5)

                // 2°
                .WithScore(fisher6.Id, 2)

                .Build());

            var competition3 = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                    .WithTournament(tournament.Id)
                    .WithLocation(Location.Create("Test City 3", "Test State 3", "Test Country 3", "Test Place 3"))

                    // 1°
                    .WithScore(fisher1.Id, 20)
                    .WithScore(fisher1.Id, 10)

                    // 2°
                    .WithScore(fisher3.Id, 20)
                    .WithScore(fisher3.Id, 5)

                    // 3°
                    .WithScore(fisher2.Id, 5)

                    // 4°
                    .WithScore(fisher4.Id, 1)


                    // 1°
                    .WithScore(fisher5.Id, 5)

                    // 2°
                    .WithScore(fisher6.Id, 2)

                    .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetTournamentLeaderBoardQuery(tournament.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                .ShouldHaveNPositions(4)
                .ShouldHavePosition(1, fisher3.Id, new List<int> { 2, 1, 2 }) // 5
                .ShouldHavePosition(2, fisher1.Id, new List<int> { 1, 4, 1 }) // 6
                .ShouldHavePosition(3, fisher2.Id, new List<int> { 3, 3, 3 }) // 9
                .ShouldHavePosition(4, fisher4.Id, new List<int> { 4, 2, 4 }); // 10

            result.Value.ShouldHaveCategoryLeaderBoard("Secondary", categorySecondary.Id)
                .ShouldHaveNPositions(2)
                .ShouldHavePosition(1, fisher5.Id, new List<int> { 1, 1, 1 })
                .ShouldHavePosition(2, fisher6.Id, new List<int> { 2, 2, 2 });
        }

        [Fact]
        public async Task Handler_Should_ReturnValidLeaderboard_BreakTieBy_HigherTotalScore()
        {
            using var context = _fixture.TournamentContext;

            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");
            var fisher4 = context.PrepareFisher("First4", "Last4");

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

                .Build());

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetTournamentLeaderBoardQuery(tournament.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                .ShouldHaveNPositions(4)
                .ShouldHavePosition(1, fisher3.Id, new List<int> { 1, 2, 2 }) // 4
                .ShouldHavePosition(2, fisher1.Id, new List<int> { 2, 1, 4 }) // 7
                .ShouldHavePosition(3, fisher2.Id, new List<int> { 3, 3, 1 }) // 7
                .ShouldHavePosition(4, fisher4.Id, new List<int> { 4, 2, 3 }); // 10
        }
    }
}