using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class AddCompetitionsHandlerTest : BaseUseCaseTest
    {
        public AddCompetitionsHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task Handler_Should_AddCompetitions()
        {
            // 
            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));
            var user = await _fixture.AddAsync(User.Create("First", "Last"));
            var fisher = await _fixture.AddAsync(Fisher.Create(user.Id));

            var command = new AddCompetitionsCommand(tournament.Id.ToString(), new List<AddCompetitionCommand>
            {
                new AddCompetitionCommand
                {
                    StartDateTime = _fixture.DateTimeProvider.Now.AddDays(1),
                    City = "Test City 1",
                    State = "Test State 1",
                    Country = "Test Country 1",
                    Place = "Test Place 1",
                },
                new AddCompetitionCommand
                {
                    StartDateTime = _fixture.DateTimeProvider.Now.AddDays(2),
                    City = "Test City 2",
                    State = "Test State 2",
                    Country = "Test Country 2",
                    Place = "Test Place 2",
                }
            });

            // 
            var result = await _fixture.SendAsync(command);
            var tournamentWithCompetitions = await _fixture.FindAsync<Tournament>(tournament.Id);
            var competitions = await _fixture.FindAllAsync<Competition, CompetitionId>(result.Value.Select(c => c.Id));

            // Assert correct response
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            tournamentWithCompetitions.Should().NotBeNull();
            tournamentWithCompetitions!.CompetitionsIds.Should()
                .HaveCount(2)
                .And
                .Contain(c => c == result.Value.First().Id)
                .And
                .Contain(c => c == result.Value.Last().Id);

            competitions.Should().HaveCount(2);

            // Assert correct competition data was stored
            competitions.FirstOrDefault(c => c.StartDateTime == command.Competitions.First().StartDateTime
                                             && c.Location.City == command.Competitions.First().City).Should().NotBeNull();

            competitions.FirstOrDefault(c => c.StartDateTime == command.Competitions.Last().StartDateTime
                                                && c.Location.City == command.Competitions.Last().City).Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_NotAddCompetitions_When_TournamentDoesntExist()
        {
            // 
            var command = new AddCompetitionsCommand(Guid.NewGuid().ToString(), new List<AddCompetitionCommand>
            {
                new AddCompetitionCommand
                {
                    StartDateTime = _fixture.DateTimeProvider.Now.AddDays(1),
                    City = "Test City",
                    State = "Test State",
                    Country = "Test Country",
                    Place = "Test Place",
                }
            });

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournament.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddCompetitions_When_CompetitionStartIsBeforeTournamentStart()
        {
            // 
            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(4),
                _fixture.DateTimeProvider.Now.AddDays(6)));

            var command = new AddCompetitionsCommand(tournament.Id.ToString(), new List<AddCompetitionCommand>
            {
                new AddCompetitionCommand
                {
                    StartDateTime = _fixture.DateTimeProvider.Now.AddDays(1),
                    City = "Test City",
                    State = "Test State",
                    Country = "Test Country",
                    Place = "Test Place",
                }
            });

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competition.StartDateBeforeTournament);
        }

    }
}