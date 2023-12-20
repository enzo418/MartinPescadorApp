using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.IntegrationTests.Competitions.Commands
{
    public class AddScoreHandlerTest : BaseUseCaseTest
    {
        public AddScoreHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_AddScore()
        {
            // 
            using var context = _fixture.TournamentContext;

            var fisher = context.PrepareFisher("First", "Last", out var _);

            var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("tournamentname")
                .WithCategory("foo")
                .WithInscription(fisher.Id, 1, "foo")
                .Build(default);

            var competition = await
                CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
                .WithLocation(Location.Create("Test City", "Test State", "Test Country", "Test Place"))
                .WithN(1)
                .WithTournament(tournament.Id)
                .WithStartDate(_fixture.DateTimeProvider.Now)
                .Build(default);

            var addScoreCommand = new AddScoreCommand(fisher.Id.ToString(), competition.Id.ToString(), 10);

            // 
            var result = await _fixture.SendAsync(addScoreCommand);
            var competitionWithScore = await context.Competitions.Where(c => c.Id == competition.Id).FirstOrDefaultAsync();

            // 
            result.IsError.Should().BeFalse();
            competitionWithScore.Should().NotBeNull();
            competitionWithScore!.Participations.Should().HaveCount(1);
            competitionWithScore.Participations.First().TotalScore.Should().Be(10);
        }


        [Fact]
        public async Task Handler_Should_NotAddScore_When_FisherDoesntExist()
        {
            // 
            using var context = _fixture.TournamentContext;
            var tournament = await context.WithAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(5)));

            var competition = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now.AddDays(1),
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place"), 1));

            var fisher = Fisher.Create("First", "Last");

            var command = new AddScoreCommand(fisher.Id.ToString(), competition.Id.ToString(), 10);

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddScore_When_CompetitionDoesntExist()
        {
            // 
            using var context = _fixture.TournamentContext;
            var fisher = await context.WithFisherAsync("First", "Last");
            var competition = Competition.Create(
                _fixture.DateTimeProvider.Now.AddDays(1),
                Guid.NewGuid(),
                Location.Create("Test City", "Test State", "Test Country", "Test Place"), 1);

            var command = new AddScoreCommand(fisher.Id.ToString(), competition.Id.ToString(), 10);

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competitions.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddScore_When_FisherIsNotEnrolled()
        {
            // 
            using var context = _fixture.TournamentContext;
            var tournament = await context.WithAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(5)));

            var competition = await context.WithAsync(Competition.Create(
                _fixture.DateTimeProvider.Now.AddDays(1),
                tournament.Id,
                Location.Create("Test City", "Test State", "Test Country", "Test Place"), 3));

            var fisher = await context.WithFisherAsync("First", "Last");

            var command = new AddScoreCommand(fisher.Id.ToString(), competition.Id.ToString(), 10);

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotEnrolled);
        }
    }
}