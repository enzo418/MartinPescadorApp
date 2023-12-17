using FisherTournament.Application.Common.Resources;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Domain.TournamentAggregate.Entities;

namespace FisherTournament.UnitTests.Tournaments.Commands.AddCompetitions
{
    public class AddCompetitionsCommandHandlerTests : BaseHandlerTest
    {
        private AddCompetitionsCommandHandler Handler => new(_contextMock.Object);

        [Fact]
        public async Task Handler_ShouldNot_AddCompetitions_WhenTournamentDoesNotExist()
        {
            // Arrange
            _contextMock.SetupTournaments(new List<Tournament>());

            var command = new AddCompetitionsCommand(Guid.Empty.ToString(),
                                                    new List<AddCompetitionCommand>()
                                                    {
                                                        new AddCompetitionCommand(
                                                            DateTime.UtcNow.AddDays(1),
                                                            new CompetitionLocationResource("City",
                                                            "State",
                                                            "Country",
                                                            "Place"))
                                                    });

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        // StartDateBeforeTournament
        [Fact]
        public async Task Handler_ShouldNot_AddCompetitions_WhenCompetitionStartDateIsBeforeTournamentStartDate()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               DateTime.UtcNow.AddDays(1),
                                               DateTime.UtcNow.AddDays(2),
                                               new List<Category>());

            _contextMock.SetupTournament(tournament);

            var command = new AddCompetitionsCommand(tournament.Id.ToString(),
                                                    new List<AddCompetitionCommand>()
                                                    {
                                                        new AddCompetitionCommand(
                                                            DateTime.UtcNow.AddDays(-1).AddHours(-1),
                                                            new CompetitionLocationResource("City",
                                                            "State",
                                                            "Country",
                                                            "Place"))
                                                    });

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competitions.StartDateBeforeTournament);
        }
    }
}