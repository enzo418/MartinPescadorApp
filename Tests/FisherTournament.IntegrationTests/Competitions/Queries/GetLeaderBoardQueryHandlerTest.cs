using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.IntegrationTests.Common;
using FisherTournament.IntegrationTests.Competitions.Extensions;

namespace FisherTournament.IntegrationTests.Competitions.Queries
{
    public partial class GetLeaderBoardQueryHandlerTest : BaseUseCaseTest
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
            var categoryGeneral = tournament.Categories.First(c => c.Name == Tournament.GeneralCategoryName);

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

            result.Value.ShouldHaveCategoryLeaderBoard(categoryGeneral.Name, categoryGeneral.Id)
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher3.Id, 100)
                        .ShouldHavePosition(3, fisher1.Id, 11)
                        .ShouldHavePosition(4, fisher4.Id, 5);

        }

        // loads two competitions, some fisherman scores on them, then check that the scores are not mixed between competitions
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
            var categoryGeneral = tournament.Categories.First(c => c.Name == Tournament.GeneralCategoryName);

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

            result.Value.ShouldHaveCategoryLeaderBoard(categoryGeneral.Name, categoryGeneral.Id)
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher2.Id, 200)
                        .ShouldHavePosition(2, fisher3.Id, 100)
                        .ShouldHavePosition(3, fisher1.Id, 11)
                        .ShouldHavePosition(4, fisher4.Id, 5);
        }

        [Fact]
        /// <summary>
        /// When a fisher didn't participate in a competition, but was inscribed in the tournament,
        /// the leaderboard should still show the fisher with 0 points.
        /// </summary>
        public async Task Handler_Should_ReturnValidLeaderboard_WhenParticipationIsNull()
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

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)
                .WithScore(fisher1.Id, 5)

                // Don't register a "CompetitionParticipation" ->> .WithScore(fisher2.Id, 50)

                .WithScore(fisher3.Id, 15)

                .WithN(1)

                .Build());


            Func<Task> updateCall = () => _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            await updateCall.Should().NotThrowAsync();

            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(3)
                        .ShouldHavePosition(1, fisher1.Id, 55)
                        .ShouldHavePosition(2, fisher3.Id, 15)
                        .ShouldHavePosition(3, fisher2.Id, 0);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderboardWithSamePositionForAbsentFishers()
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

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")
                .WithInscription(fisher4.Id, 4, "Primary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)

                .WithScore(fisher2.Id, 15)

                .WithN(1)

                .Build());

            await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher1.Id, 50)
                        .ShouldHavePosition(2, fisher2.Id, 15)
                        .ShouldHavePosition(3, fisher3.Id, 0)
                        .ShouldHavePosition(3, fisher4.Id, 0);
        }

        [Fact]
        public async Task Handler_Should_ReturnCorrectLeaderboardGeneralCategory()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
            var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
            var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
            var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

            var fisher5 = context.PrepareFisher("First5", "Last5", out var _);
            var fisher6 = context.PrepareFisher("First6", "Last6", out var _);

            var fisher7 = context.PrepareFisher("First7", "Last7", out var _);
            var fisher8 = context.PrepareFisher("First8", "Last8", out var _);

            int tournamentTotalFishers = 8;

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

                .WithCategory("Primary")
                .WithCategory("Secondary")
                .WithCategory("Tertiary")

                .WithInscription(fisher1.Id, 1, "Primary")
                .WithInscription(fisher2.Id, 2, "Primary")
                .WithInscription(fisher3.Id, 3, "Primary")
                .WithInscription(fisher4.Id, 4, "Primary")

                .WithInscription(fisher5.Id, 5, "Secondary")
                .WithInscription(fisher6.Id, 6, "Secondary")

                .WithInscription(fisher7.Id, 7, "Tertiary")
                .WithInscription(fisher8.Id, 8, "Tertiary")

                .Build(CancellationToken.None);

            var categoryPrimary = tournament.Categories.First(c => c.Name == "Primary");
            var categorySecondary = tournament.Categories.First(c => c.Name == "Secondary");
            var categoryTertiary = tournament.Categories.First(c => c.Name == "Tertiary");
            var categoryGeneral = tournament.Categories.First(c => c.Name == Tournament.GeneralCategoryName);

            var competition = await context.WithAsync(CompetitionBuilder.Create(_fixture.DateTimeProvider)
                .WithTournament(tournament.Id)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))

                .WithScore(fisher1.Id, 50)
                .WithScore(fisher2.Id, 15)
                // 3 and 4 are absent

                .WithScore(fisher5.Id, 100)
                .WithScore(fisher6.Id, 200)

                .WithScore(fisher7.Id, 300)

                .WithN(1)

                .Build());

            await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(4);

            result.Value.ShouldHaveCategoryLeaderBoard("Primary", categoryPrimary.Id)
                        .ShouldHaveNPositions(4)
                        .ShouldHavePosition(1, fisher1.Id, 50)
                        .ShouldHavePosition(2, fisher2.Id, 15)
                        .ShouldHavePosition(3, fisher3.Id, 0)
                        .ShouldHavePosition(3, fisher4.Id, 0);

            result.Value.ShouldHaveCategoryLeaderBoard("Secondary", categorySecondary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher6.Id, 200)
                        .ShouldHavePosition(2, fisher5.Id, 100);

            result.Value.ShouldHaveCategoryLeaderBoard("Tertiary", categoryTertiary.Id)
                        .ShouldHaveNPositions(2)
                        .ShouldHavePosition(1, fisher7.Id, 300)
                        .ShouldHavePosition(2, fisher8.Id, 0);

            result.Value.ShouldHaveCategoryLeaderBoard(categoryGeneral.Name, categoryGeneral.Id)
                        .ShouldHaveNPositions(8)
                        .ShouldHavePosition(1, fisher7.Id, 300)
                        .ShouldHavePosition(2, fisher6.Id, 200)
                        .ShouldHavePosition(3, fisher5.Id, 100)
                        .ShouldHavePosition(4, fisher1.Id, 50)
                        .ShouldHavePosition(5, fisher2.Id, 15)
                        .ShouldHavePosition(tournamentTotalFishers + 1, fisher3.Id, 0)
                        .ShouldHavePosition(tournamentTotalFishers + 1, fisher4.Id, 0)
                        .ShouldHavePosition(tournamentTotalFishers + 1, fisher8.Id, 0);
        }
    }
}