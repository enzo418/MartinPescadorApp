using FisherTournament.Application.Tournaments.Commands.CreateTournament;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class CreateTournamentHandlerTest : BaseUseCaseTest
    {
        public CreateTournamentHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task Handler_Should_CreateTournament()
        {
            // 
            using var context = _fixture.Context;
            var command = new CreateTournamentCommand(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2));

            // 
            var result = await _fixture.SendAsync(command);
            var tournament = await context.FindAsync<Tournament>(result.Value.Id);

            // 
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            tournament.Should().NotBeNull();
            tournament!.Name.Should().Be("Test Tournament");
            tournament.StartDate.Should().Be(command.StartDate);
            tournament.EndDate.Should().Be(command.EndDate);
        }
    }
}