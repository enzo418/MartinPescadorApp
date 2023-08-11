using ErrorOr;
using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.Entities;

namespace FisherTournament.IntegrationTests.Competitions.Queries
{
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
            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");
            var fisher4 = context.PrepareFisher("First4", "Last4");

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now,
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var categoryPrimary = tournament.AddCategory("Primary").Value;
            var categorySecondary = tournament.AddCategory("Secondary").Value;

            await context.SaveChangesAsync(CancellationToken.None);

            var inscriptionResults = new List<ErrorOr<Success>>()
            {
                tournament.AddInscription(fisher1.Id, categoryPrimary.Id, 1, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher2.Id, categoryPrimary.Id, 2, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher3.Id, categorySecondary.Id, 3, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher4.Id, categorySecondary.Id, 4, _fixture.DateTimeProvider)
            };

            inscriptionResults.Should().NotContain(r => r.IsError);

            await context.SaveChangesAsync(CancellationToken.None);

            var competition = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        comp.AddScore(fisher1.Id, 1, _fixture.DateTimeProvider),
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 200, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 100, _fixture.DateTimeProvider),
                        comp.AddScore(fisher4.Id, 5, _fixture.DateTimeProvider)
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            var primaryCategory = result.Value.FirstOrDefault(c => c.Id == categoryPrimary.Id.Value.ToString() && c.Name == categoryPrimary.Name);
            primaryCategory.Should().NotBeNull();

            primaryCategory!.LeaderBoard.Should().HaveCount(2);

            primaryCategory.LeaderBoard.First().FisherId.Should().Be(fisher2.Id);
            primaryCategory.LeaderBoard.First().TotalScore.Should().Be(200);

            primaryCategory.LeaderBoard.Last().FisherId.Should().Be(fisher1.Id);
            primaryCategory.LeaderBoard.Last().TotalScore.Should().Be(11);

            var secondaryCategory = result.Value.FirstOrDefault(c => c.Id == categorySecondary.Id.Value.ToString() && c.Name == categorySecondary.Name);
            secondaryCategory.Should().NotBeNull();
            secondaryCategory.LeaderBoard.Should().HaveCount(2);
            secondaryCategory!.LeaderBoard.First().FisherId.Should().Be(fisher3.Id);
            secondaryCategory.LeaderBoard.First().TotalScore.Should().Be(100);
        }

        // loads two competitions, some scores on them and check that the scores are not mixed between competitions
        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardNotMixed_TwoCompetitions()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");
            var fisher4 = context.PrepareFisher("First4", "Last4");

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now,
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var categoryPrimary = tournament.AddCategory("Primary").Value;
            var categorySecondary = tournament.AddCategory("Secondary").Value;

            await context.SaveChangesAsync(CancellationToken.None);

            var inscriptionResults = new List<ErrorOr<Success>>()
            {
                tournament.AddInscription(fisher1.Id, categoryPrimary.Id, 1, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher2.Id, categoryPrimary.Id, 2, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher3.Id, categorySecondary.Id, 3, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher4.Id, categorySecondary.Id, 4, _fixture.DateTimeProvider)
            };

            inscriptionResults.Should().NotContain(r => r.IsError);

            await context.SaveChangesAsync(CancellationToken.None);

            var competition1 = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        comp.AddScore(fisher1.Id, 1, _fixture.DateTimeProvider),
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 200, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 100, _fixture.DateTimeProvider),
                        comp.AddScore(fisher4.Id, 5, _fixture.DateTimeProvider)
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            var competition2 = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        comp.AddScore(fisher1.Id, 100, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 200, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 300, _fixture.DateTimeProvider),
                        comp.AddScore(fisher4.Id, 400, _fixture.DateTimeProvider)
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            await context.SaveChangesAsync(CancellationToken.None);

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition1.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            var primaryCategory = result.Value.FirstOrDefault(c => c.Id == categoryPrimary.Id.Value.ToString() && c.Name == categoryPrimary.Name);
            primaryCategory.Should().NotBeNull();

            primaryCategory!.LeaderBoard.Should().HaveCount(2);

            primaryCategory.LeaderBoard.First().FisherId.Should().Be(fisher2.Id);
            primaryCategory.LeaderBoard.First().TotalScore.Should().Be(200);

            primaryCategory.LeaderBoard.Last().FisherId.Should().Be(fisher1.Id);
            primaryCategory.LeaderBoard.Last().TotalScore.Should().Be(11);

            var secondaryCategory = result.Value.FirstOrDefault(c => c.Id == categorySecondary.Id.Value.ToString() && c.Name == categorySecondary.Name);
            secondaryCategory.Should().NotBeNull();

            secondaryCategory!.LeaderBoard.Should().HaveCount(2);

            secondaryCategory.LeaderBoard.First().FisherId.Should().Be(fisher3.Id);
            secondaryCategory.LeaderBoard.First().TotalScore.Should().Be(100);

            secondaryCategory.LeaderBoard.Last().FisherId.Should().Be(fisher4.Id);
            secondaryCategory.LeaderBoard.Last().TotalScore.Should().Be(5);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByLargerPiece()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now,
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var categoryPrimary = tournament.AddCategory("Primary").Value;
            var categorySecondary = tournament.AddCategory("Secondary").Value;

            await context.SaveChangesAsync(CancellationToken.None);

            var inscriptionResults = new List<ErrorOr<Success>>()
            {
                tournament.AddInscription(fisher1.Id, categoryPrimary.Id, 1, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher2.Id, categoryPrimary.Id, 2, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher3.Id, categoryPrimary.Id, 3, _fixture.DateTimeProvider)
            };

            inscriptionResults.Should().NotContain(r => r.IsError);

            await context.SaveChangesAsync(CancellationToken.None);

            var competition = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        // fisher 1 is 1°
                        comp.AddScore(fisher1.Id, 50, _fixture.DateTimeProvider),
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),

                        comp.AddScore(fisher2.Id, 10, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 20, _fixture.DateTimeProvider),

                        // fisher3 and fisher2 are tied, but fisher3 has a larger piece
                        // (25 vs 20) so it's 2° and fisher2 is 3°
                        comp.AddScore(fisher3.Id, 25, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 5, _fixture.DateTimeProvider),
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(1);

            var primaryCategory = result.Value.FirstOrDefault(c => c.Id == categoryPrimary.Id.Value.ToString() && c.Name == categoryPrimary.Name);
            primaryCategory.Should().NotBeNull();

            primaryCategory!.LeaderBoard.Should().HaveCount(3);

            primaryCategory.LeaderBoard.ElementAt(0).FisherId.Should().Be(fisher1.Id);
            primaryCategory.LeaderBoard.ElementAt(0).TotalScore.Should().Be(60);

            primaryCategory.LeaderBoard.ElementAt(1).FisherId.Should().Be(fisher3.Id);
            primaryCategory.LeaderBoard.ElementAt(1).TotalScore.Should().Be(30);

            primaryCategory.LeaderBoard.ElementAt(2).FisherId.Should().Be(fisher2.Id);
            primaryCategory.LeaderBoard.ElementAt(2).TotalScore.Should().Be(30);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByLargerPiece_WithMultipleCompetitions()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now,
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var categoryPrimary = tournament.AddCategory("Primary").Value;
            var categorySecondary = tournament.AddCategory("Secondary").Value;

            await context.SaveChangesAsync(CancellationToken.None);

            var inscriptionResults = new List<ErrorOr<Success>>()
            {
                tournament.AddInscription(fisher1.Id, categoryPrimary.Id, 1, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher2.Id, categoryPrimary.Id, 2, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher3.Id, categoryPrimary.Id, 3, _fixture.DateTimeProvider)
            };

            inscriptionResults.Should().NotContain(r => r.IsError);

            await context.SaveChangesAsync(CancellationToken.None);

            var competition1 = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        // fisher 1 is 1°
                        comp.AddScore(fisher1.Id, 50, _fixture.DateTimeProvider),
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),

                        comp.AddScore(fisher2.Id, 10, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 20, _fixture.DateTimeProvider),

                        // fisher3 and fisher2 are tied, but fisher3 has a larger piece
                        // (25 vs 20) so it's 2° and fisher2 is 3°
                        comp.AddScore(fisher3.Id, 25, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 5, _fixture.DateTimeProvider),
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            // competition to verify that the competition leader board is not affected by other competitions
            var competition2 = await context.WithAsync(Competition.Create(
            _fixture.DateTimeProvider.Now,
            tournament.Id,
            Location.Create("Test City", "Test State", "Test Country", "Test Place")),
            beforeSave: comp =>
            {
                var results = new List<ErrorOr<Success>>()
                {
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),

                        comp.AddScore(fisher2.Id, 500, _fixture.DateTimeProvider),

                        comp.AddScore(fisher3.Id, 50, _fixture.DateTimeProvider),
                };

                results.Should().NotContain(r => r.IsError);
            });

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition1.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(1);

            var primaryCategory = result.Value.FirstOrDefault(c => c.Id == categoryPrimary.Id.Value.ToString() && c.Name == categoryPrimary.Name);
            primaryCategory.Should().NotBeNull();

            primaryCategory!.LeaderBoard.Should().HaveCount(3);

            primaryCategory.LeaderBoard.ElementAt(0).FisherId.Should().Be(fisher1.Id);
            primaryCategory.LeaderBoard.ElementAt(0).TotalScore.Should().Be(60);

            primaryCategory.LeaderBoard.ElementAt(1).FisherId.Should().Be(fisher3.Id);
            primaryCategory.LeaderBoard.ElementAt(1).TotalScore.Should().Be(30);

            primaryCategory.LeaderBoard.ElementAt(2).FisherId.Should().Be(fisher2.Id);
            primaryCategory.LeaderBoard.ElementAt(2).TotalScore.Should().Be(30);
        }

        [Fact]
        public async Task Handler_Should_ReturnLeaderBoardWithTieBreakingByFirstLargerPiece()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var fisher1 = context.PrepareFisher("First1", "Last1");
            var fisher2 = context.PrepareFisher("First2", "Last2");
            var fisher3 = context.PrepareFisher("First3", "Last3");

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now,
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var categoryPrimary = tournament.AddCategory("Primary").Value;

            await context.SaveChangesAsync(CancellationToken.None);

            var inscriptionResults = new List<ErrorOr<Success>>()
            {
                tournament.AddInscription(fisher1.Id, categoryPrimary.Id, 1, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher2.Id, categoryPrimary.Id, 2, _fixture.DateTimeProvider),
                tournament.AddInscription(fisher3.Id, categoryPrimary.Id, 3, _fixture.DateTimeProvider)
            };

            inscriptionResults.Should().NotContain(r => r.IsError);

            await context.SaveChangesAsync(CancellationToken.None);

            var competition = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now,
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    var results = new List<ErrorOr<Success>>()
                    {
                        // fisher 1 is 1°
                        comp.AddScore(fisher1.Id, 50, _fixture.DateTimeProvider),
                        comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider),

                        comp.AddScore(fisher2.Id, 6, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 15, _fixture.DateTimeProvider),
                        comp.AddScore(fisher2.Id, 14, _fixture.DateTimeProvider),

                        // fisher3 and fisher2 are tied and have the same larger piece (15),
                        // but fisher2 has a larger piece (6 vs 5) so it's 2° and fisher3 is 3°
                        comp.AddScore(fisher3.Id, 5, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 15, _fixture.DateTimeProvider),
                        comp.AddScore(fisher3.Id, 15, _fixture.DateTimeProvider),
                    };

                    results.Should().NotContain(r => r.IsError);
                });

            int jobsExecuted = await _fixture.ExecutePendingLeaderBoardJobs();

            // Act
            var result = await _fixture.SendAsync(new GetCompetitionLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            jobsExecuted.Should().BeGreaterThan(0);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(1);

            var primaryCategory = result.Value.FirstOrDefault(c => c.Id == categoryPrimary.Id.Value.ToString() && c.Name == categoryPrimary.Name);
            primaryCategory.Should().NotBeNull();

            primaryCategory!.LeaderBoard.Should().HaveCount(3);

            primaryCategory.LeaderBoard.ElementAt(0).FisherId.Should().Be(fisher1.Id);
            primaryCategory.LeaderBoard.ElementAt(0).TotalScore.Should().Be(60);

            primaryCategory.LeaderBoard.ElementAt(1).FisherId.Should().Be(fisher2.Id);
            primaryCategory.LeaderBoard.ElementAt(1).TotalScore.Should().Be(35);

            primaryCategory.LeaderBoard.ElementAt(2).FisherId.Should().Be(fisher3.Id);
            primaryCategory.LeaderBoard.ElementAt(2).TotalScore.Should().Be(35);
        }
    }
}