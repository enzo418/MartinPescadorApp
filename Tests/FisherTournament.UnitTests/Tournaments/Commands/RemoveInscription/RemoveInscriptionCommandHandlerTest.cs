using FisherTournament.Application.Tournaments.Commands.RemoveInscription;
using FisherTournament.Domain.CompetitionAggregate.Entities;

namespace FisherTournament.UnitTests.Tournaments.Commands.RemoveInscription
{
    public class RemoveInscriptionCommandHandlerTest : BaseHandlerTest
    {
        private RemoveInscriptionCommandHandler Handler => new(_contextMock.Object, _dateTimeProviderMock.Object);

        public RemoveInscriptionCommandHandlerTest()
        {
            _dateTimeProviderMock.SetupGet(p => p.Now).Returns(DateTime.UtcNow);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveInscription_WhenTournamentDoesNotExist()
        {
            // Arrange
            var fisher = Fisher.Create("First Name", "Last Name");

            _contextMock.SetupTournaments(new List<Tournament>())
                             .SetupFisher(fisher);

            var command = new RemoveInscriptionCommand(Guid.Empty.ToString(), fisher.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveInscription_WhenFisherDoesNotExist()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2));

            _contextMock.SetupTournament(tournament)
                             .SetupFishers(new List<Fisher>());

            var command = new RemoveInscriptionCommand(tournament.Id.ToString(), Guid.Empty.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveInscription_WhenFisherIsNotEnrolled()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2));

            var fisher = Fisher.Create("First Name", "Last Name");

            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(1),
                                                 tournament.Id,
                                                 Location.Create("", "", "", ""),
                                                 1);

            tournament.AddCompetition(competition.Id);

            _contextMock.SetupTournament(tournament)
                            .SetupCompetition(competition)
                             .SetupFisher(fisher);

            var command = new RemoveInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.InscriptionNotFound);
        }

        [Fact]
        public async Task Handler_Should_RemoveInscription_WhenFisherIsEnrolled()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2));

            var categoryMock = MockCategory(1);
            var categoryResult = tournament.AddCategory(categoryMock.Object);

            var fisher = Fisher.Create("First Name", "Last Name");

            var inscriptionResult = tournament.AddInscription(fisher.Id, categoryMock.Object.Id, 1, _dateTimeProviderMock.Object);

            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(1),
                                                 tournament.Id,
                                                 Location.Create("", "", "", ""),
                                                 1);

            tournament.AddCompetition(competition.Id);

            _contextMock.SetupTournament(tournament)
                            .SetupCompetition(competition)
                             .SetupFisher(fisher);

            var command = new RemoveInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            categoryResult.IsError.Should().BeFalse();
            inscriptionResult.IsError.Should().BeFalse();
            result.IsError.Should().BeFalse();
        }
    }
}
