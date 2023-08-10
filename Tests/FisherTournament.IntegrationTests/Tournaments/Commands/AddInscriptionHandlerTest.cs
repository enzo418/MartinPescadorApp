using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Domain.TournamentAggregate.Entities;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class AddInscriptionHandlerTest : BaseUseCaseTest
    {
        public AddInscriptionHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task Handler_Should_AddInscription()
        {
            using var context = _fixture.TournamentContext;

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2))
            );

            var category = tournament.AddCategory("test").Value;
            var fisher = context.PrepareAdd(Fisher.Create("First", "Last"));

            // EF NOTE: If you don't clear the change tracker, find will return the same as `tournament` variable,
            // since it was modified in the inscription command (different DB context instance).
            await context.SaveChangesAndClear();

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    category.Id,
                                                    1);

            // Act
            var result = await _fixture.SendAsync(command);
            var tournamentWithInscription = await context.FindAsync<Tournament>(tournament.Id);

            // Assert
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            tournamentWithInscription.Should().NotBeNull();
            tournamentWithInscription!.Inscriptions.Should()
                .HaveCount(1)
                .And
                .Contain(i => i.FisherId == fisher.Id);
        }
    }
}