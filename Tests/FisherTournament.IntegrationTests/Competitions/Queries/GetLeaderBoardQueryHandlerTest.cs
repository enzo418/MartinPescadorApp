using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task Handler_Should_ReturnLeaderBoard()
        {
            // Arrange
            var user1 = await _fixture.AddAsync(User.Create("First1", "Last1"));
            var fisher1 = await _fixture.AddAsync(Fisher.Create(user1.Id));

            var user2 = await _fixture.AddAsync(User.Create("First2", "Last2"));
            var fisher2 = await _fixture.AddAsync(Fisher.Create(user2.Id));

            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)),
                beforeSave: t =>
                {
                    t.AddInscription(fisher1.Id);
                    t.AddInscription(fisher2.Id);
                });

            var competition = await _fixture.AddAsync(Competition.Create(
                _fixture.DateTimeProvider.Now.AddDays(1),
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place")),
                beforeSave: comp =>
                {
                    comp.AddScore(fisher1.Id, 1, _fixture.DateTimeProvider);
                    comp.AddScore(fisher1.Id, 10, _fixture.DateTimeProvider);

                    comp.AddScore(fisher2.Id, 200, _fixture.DateTimeProvider);
                });

            // Act
            var result = await _fixture.SendAsync(new GetLeaderBoardQuery(competition.Id.ToString()));

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            result.Value.First().FisherId.Should().Be(fisher2.Id);
            result.Value.First().TotalScore.Should().Be(200);

            result.Value.Last().FisherId.Should().Be(fisher1.Id);
            result.Value.Last().TotalScore.Should().Be(11);
        }
    }
}